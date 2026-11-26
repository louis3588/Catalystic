using System.Security.Claims;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CatalysticBackend.Security;

public static class AccountEndpoints
{
    public record Credentials(string? Email, string? Password);
    public record PasswordChange(string? CurrentPassword, string? NewPassword);
    public record BootstrapChange(string? Email, string? Password, string? NewPassword);

    public static void MapAccountEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");
        group.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            return Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken });
        }).AllowAnonymous();

        group.MapPost("/login", async (Credentials request, HttpContext context, IAntiforgery csrf,
            CatalysticDbContext db, IPasswordHasher<User> hasher) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            var account = await FindAsync(db, request.Email);
            if (!Verify(account, request.Password, hasher)) return Results.Unauthorized();
            if (account!.MustChangePassword)
            {
                if (account.BootstrapExpiresAt <= DateTime.UtcNow || account.BootstrapExpiresAt == null)
                    return Results.Unauthorized();
                return Results.Ok(new { passwordChangeRequired = true });
            }
            if (account.SessionVersion == Guid.Empty || (account.Role != "Admin" && account.Role != "Client"))
                return Results.Unauthorized();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Role, account.Role), new Claim("session_version", account.SessionVersion.ToString()) };
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
            return Results.Ok(new { passwordChangeRequired = false });
        }).AllowAnonymous().RequireRateLimiting("authentication");

        group.MapPost("/bootstrap-password", async (BootstrapChange request, HttpContext context, IAntiforgery csrf,
            CatalysticDbContext db, IPasswordHasher<User> hasher) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            var account = await FindAsync(db, request.Email);
            if (!Verify(account, request.Password, hasher) || account!.Role != "Admin" || !account.MustChangePassword ||
                account.BootstrapExpiresAt == null || account.BootstrapExpiresAt <= DateTime.UtcNow)
                return Results.Unauthorized();
            if (!ValidPassword(request.NewPassword)) return Results.BadRequest(new { error = "Password must be 12 to 256 characters." });
            var oldHash = account.PasswordHash;
            var newHash = hasher.HashPassword(account, request.NewPassword!);
            var now = DateTime.UtcNow;
            var changed = await db.Users.Where(user => user.Id == account.Id && user.AccountState == AccountState.Activated && user.MustChangePassword &&
                    user.PasswordHash == oldHash && user.BootstrapExpiresAt > now)
                .ExecuteUpdateAsync(update => update.SetProperty(user => user.PasswordHash, newHash)
                    .SetProperty(user => user.MustChangePassword, false)
                    .SetProperty(user => user.BootstrapExpiresAt, (DateTime?)null)
                    .SetProperty(user => user.SessionVersion, Guid.NewGuid()));
            return changed == 1 ? Results.NoContent() : Results.Unauthorized();
        }).AllowAnonymous().RequireRateLimiting("authentication");

        group.MapPost("/password", async (PasswordChange request, HttpContext context, IAntiforgery csrf,
            CatalysticDbContext db, IPasswordHasher<User> hasher) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Results.Unauthorized();
            var account = await db.Users.SingleOrDefaultAsync(user => user.Id == id);
            if (!Verify(account, request.CurrentPassword, hasher)) return Results.Unauthorized();
            if (!ValidPassword(request.NewPassword)) return Results.BadRequest(new { error = "Password must be 12 to 256 characters." });
            var oldHash = account!.PasswordHash;
            var hash = hasher.HashPassword(account, request.NewPassword!);
            var changed = await db.Users.Where(user => user.Id == id && user.AccountState == AccountState.Activated && user.PasswordHash == oldHash)
                .ExecuteUpdateAsync(update => update.SetProperty(user => user.PasswordHash, hash)
                    .SetProperty(user => user.SessionVersion, Guid.NewGuid()));
            await context.SignOutAsync();
            return changed == 1 ? Results.NoContent() : Results.Unauthorized();
        }).RequireAuthorization().RequireRateLimiting("authentication");

        group.MapPost("/logout", async (HttpContext context, IAntiforgery csrf, CatalysticDbContext db) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Results.Unauthorized();
            await db.Users.Where(user => user.Id == id).ExecuteUpdateAsync(update =>
                update.SetProperty(user => user.SessionVersion, Guid.NewGuid()));
            await context.SignOutAsync();
            return Results.NoContent();
        }).RequireAuthorization();
    }

    private static bool ValidPassword(string? password) => password is { Length: >= 12 and <= 256 };

    private static bool Verify(User? user, string? password, IPasswordHasher<User> hasher)
    {
        if (user is not { AccountState: AccountState.Activated } || string.IsNullOrEmpty(user.PasswordHash) ||
            password is not { Length: > 0 and <= 256 }) return false;

        try
        {
            return hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            // Legacy or malformed hashes are never valid credentials.
            return false;
        }
    }

    private static async Task<User?> FindAsync(CatalysticDbContext db, string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 256) return null;
        var normalized = email.Trim().ToLowerInvariant();
        var matches = await db.Users.Where(user => user.Email.ToLower() == normalized).Take(2).ToListAsync();
        return matches.Count == 1 ? matches[0] : null;
    }

    private static async Task<bool> ValidCsrf(HttpContext context, IAntiforgery antiforgery)
    {
        try { await antiforgery.ValidateRequestAsync(context); return true; }
        catch (AntiforgeryValidationException) { return false; }
    }
}
