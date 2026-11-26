using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CatalysticBackend.Security;

public static partial class InvitationEndpoints
{

    public static void MapInvitationEndpoints(this WebApplication app)
    {
        app.MapPost("/api/admin/invitations", async (InviteRequest request, HttpContext context,
            IAntiforgery csrf, CatalysticDbContext db) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminId) || adminId <= 0)
                return Results.Unauthorized();

            var firstName = request.FirstName?.Trim() ?? "";
            var lastName = request.LastName?.Trim();
            var email = request.Email?.Trim() ?? "";
            if (!request.ManuallyApproved || firstName.Length is < 1 or > 64 || lastName?.Length > 64 ||
                email.Length is < 1 or > 254 || !new EmailAddressAttribute().IsValid(email))
                return Results.BadRequest(new { error = "Please check the invitation details and approval." });

            var normalized = email.ToUpperInvariant();
            var secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var hash = Hash(secret);
            var now = DateTime.UtcNow;
            var expires = now.AddHours(48);

            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, context.RequestAborted);
            if (await db.Users.AnyAsync(user => user.Email.ToUpper() == normalized, context.RequestAborted) ||
                await db.ClientInvitations.AnyAsync(invitation => invitation.NormalizedEmail == normalized, context.RequestAborted))
                return Results.Conflict(new { error = "An account or invitation already exists for this email." });

            var client = new User { FirstName = firstName, LastName = lastName, Email = email, Role = "Client", AccountState = AccountState.Unactivated };
            db.Users.Add(client);
            db.ClientData.Add(new ClientData { User = client, FirstName = firstName, LastName = lastName });
            db.ClientInvitations.Add(new ClientInvitation
            {
                Client = client,
                IssuedById = adminId,
                NormalizedEmail = normalized,
                TokenHash = hash,
                CreatedAt = now,
                ExpiresAt = expires,
            });

            try
            {
                await db.SaveChangesAsync(context.RequestAborted);
                await transaction.CommitAsync(context.RequestAborted);
            }
            catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                return Results.Conflict(new { error = "An account or invitation already exists for this email." });
            }
            catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure })
            {
                return Results.Conflict(new { error = "The invitation could not be created. Please try again." });
            }
            catch (PostgresException error) when (error.SqlState == PostgresErrorCodes.SerializationFailure)
            {
                return Results.Conflict(new { error = "The invitation could not be created. Please try again." });
            }
            catch (InvalidOperationException error) when (IsSerializationFailure(error))
            {
                return Results.Conflict(new { error = "The invitation could not be created. Please try again." });
            }

            // The credential is returned once and is never stored in plaintext.
            return Results.Ok(new { token = secret, expiresAt = expires });
        }).RequireAuthorization("Admin");

        app.MapPost("/api/invitations/accept", async (AcceptRequest request, HttpContext context,
            IAntiforgery csrf, CatalysticDbContext db, IPasswordHasher<User> hasher) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (request.Token is not { Length: 64 } || !request.Token.All(Uri.IsHexDigit) ||
                request.Password is not { Length: >= 12 and <= 256 }) return Results.BadRequest();

            var hash = Hash(request.Token.ToUpperInvariant());
            var now = DateTime.UtcNow;
            await using var transaction = await db.Database.BeginTransactionAsync(context.RequestAborted);
            var invitation = await db.ClientInvitations.AsNoTracking()
                .SingleOrDefaultAsync(item => item.TokenHash == hash, context.RequestAborted);
            if (invitation is null) return Results.BadRequest(new { error = "The invitation is invalid or expired." });
            var client = await db.Users.SingleOrDefaultAsync(user => user.Id == invitation.ClientId && user.Role == "Client", context.RequestAborted);
            if (client is null || client.AccountState != AccountState.Unactivated || !string.IsNullOrEmpty(client.PasswordHash))
                return Results.BadRequest(new { error = "The invitation is invalid or expired." });

            var accepted = await db.ClientInvitations.Where(item => item.Id == invitation.Id && item.AcceptedAt == null && item.ExpiresAt > now)
                .ExecuteUpdateAsync(update => update.SetProperty(item => item.AcceptedAt, now), context.RequestAborted);
            if (accepted != 1) return Results.BadRequest(new { error = "The invitation is invalid or expired." });

            client.PasswordHash = hasher.HashPassword(client, request.Password);
            // A manually delivered link proves possession of the link, not the email inbox.
            client.IsEmailVerified = false;
            client.AccountState = AccountState.Activated;
            client.SessionVersion = Guid.NewGuid();
            await db.SaveChangesAsync(context.RequestAborted);
            await transaction.CommitAsync(context.RequestAborted);
            return Results.NoContent();
        }).AllowAnonymous().RequireRateLimiting("authentication");
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static bool IsSerializationFailure(Exception error)
    {
        for (Exception? current = error; current != null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure }) return true;
        }

        return false;
    }

    private static async Task<bool> ValidCsrf(HttpContext context, IAntiforgery antiforgery)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
    }
}
