using System.Security.Claims;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;

namespace CatalysticBackend.Security;

public static class ClientWorkEndpoints
{
    public record NoteRequest(string? Content);
    public record TaskRequest(string? Title, string? Description, string? Pillar, DateTime? DueAt);
    public record AccessRequest(bool Active);

    private static readonly string[] Pillars =
        ["Physical", "Dietary", "Social", "Emotional", "Intellectual", "Habitual", "Financial"];

    public static void MapClientWorkEndpoints(this WebApplication app)
    {
        var admin = app.MapGroup("/api/admin/clients").RequireAuthorization("Admin");

        admin.MapGet("/", async (CatalysticDbContext db, HttpContext context) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            var clients = await db.Users.AsNoTracking().Where(user => user.Role == "Client")
                .OrderBy(user => user.FirstName).ThenBy(user => user.Id).Take(100)
                .Select(user => new { user.Id, user.FirstName, user.LastName, user.Email, user.IsActive })
                .ToListAsync(context.RequestAborted);
            return Results.Ok(clients);
        });

        admin.MapGet("/{id:int}", async (int id, CatalysticDbContext db, HttpContext context) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            var client = await db.Users.AsNoTracking().Where(user => user.Id == id && user.Role == "Client")
                .Select(user => new { user.Id, user.FirstName, user.LastName, user.Email, user.IsActive })
                .SingleOrDefaultAsync(context.RequestAborted);
            if (client is null) return Results.NotFound();

            var privateNotes = await db.PractitionerNotes.AsNoTracking().Where(note => note.ClientId == id)
                .OrderByDescending(note => note.CreatedAt).Take(100)
                .Select(note => new { note.Id, note.Content, note.CreatedAt }).ToListAsync(context.RequestAborted);
            var sharedNotes = await db.SharedClientNotes.AsNoTracking().Where(note => note.ClientId == id)
                .OrderByDescending(note => note.SharedAt).Take(100)
                .Select(note => new { note.Id, note.Content, note.SharedAt }).ToListAsync(context.RequestAborted);
            var tasks = await db.ClientTasks.AsNoTracking().Where(task => task.ClientId == id)
                .OrderByDescending(task => task.CreatedAt).Take(100)
                .Select(task => new { task.Id, task.Title, task.Description, task.Pillar, task.DueAt, task.CompletedAt, task.CreatedAt })
                .ToListAsync(context.RequestAborted);
            return Results.Ok(new { client, privateNotes, sharedNotes, tasks });
        });

        admin.MapPatch("/{id:int}/access", async (int id, AccessRequest request, HttpContext context, IAntiforgery csrf, CatalysticDbContext db) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();

            var client = await db.Users.SingleOrDefaultAsync(user => user.Id == id && user.Role == "Client", context.RequestAborted);
            if (client is null) return Results.NotFound();
            if (request.Active)
            {
                var hasAcceptedInvitation = await db.ClientInvitations.AnyAsync(
                    invitation => invitation.ClientId == id && invitation.AcceptedAt != null,
                    context.RequestAborted);

                if ((!client.IsEmailVerified && !hasAcceptedInvitation) || string.IsNullOrWhiteSpace(client.PasswordHash) || client.MustChangePassword)
                    return Results.BadRequest(new { error = "This account has not completed provisioning." });
            }

            client.IsActive = request.Active;
            client.SessionVersion = Guid.NewGuid();
            await db.SaveChangesAsync(context.RequestAborted);
            return Results.NoContent();
        });

        admin.MapPost("/{id:int}/private-notes", async (int id, NoteRequest request, HttpContext context, IAntiforgery csrf, CatalysticDbContext db) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            var content = request.Content?.Trim() ?? "";
            if (content.Length is < 1 or > 4000) return Results.BadRequest();
            if (!await IsClient(db, id, context.RequestAborted)) return Results.NotFound();
            db.PractitionerNotes.Add(new PractitionerNote { ClientId = id, Content = content, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync(context.RequestAborted);
            return Results.StatusCode(StatusCodes.Status201Created);
        });

        admin.MapPost("/{id:int}/shared-notes", async (int id, NoteRequest request, HttpContext context, IAntiforgery csrf, CatalysticDbContext db) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            var content = request.Content?.Trim() ?? "";
            if (content.Length is < 1 or > 4000) return Results.BadRequest();
            if (!await IsClient(db, id, context.RequestAborted)) return Results.NotFound();
            db.SharedClientNotes.Add(new SharedClientNote { ClientId = id, Content = content, SharedAt = DateTime.UtcNow });
            await db.SaveChangesAsync(context.RequestAborted);
            return Results.StatusCode(StatusCodes.Status201Created);
        });

        admin.MapPost("/{id:int}/tasks", async (int id, TaskRequest request, HttpContext context, IAntiforgery csrf, CatalysticDbContext db) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            var title = request.Title?.Trim() ?? "";
            var description = request.Description?.Trim() ?? "";
            if (title.Length is < 1 or > 160 || description.Length > 4000 ||
                request.Pillar is null || !Pillars.Contains(request.Pillar) ||
                (request.DueAt.HasValue && request.DueAt.Value.Kind != DateTimeKind.Utc)) return Results.BadRequest();
            if (!await IsClient(db, id, context.RequestAborted)) return Results.NotFound();
            db.ClientTasks.Add(new ClientTask { ClientId = id, Title = title, Description = description,
                Pillar = request.Pillar, DueAt = request.DueAt, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync(context.RequestAborted);
            return Results.StatusCode(StatusCodes.Status201Created);
        });

        var clientArea = app.MapGroup("/api/client/work").RequireAuthorization("Client");
        clientArea.MapGet("/", async (HttpContext context, CatalysticDbContext db) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) || id <= 0)
                return Results.Unauthorized();
            var notes = await db.SharedClientNotes.AsNoTracking().Where(note => note.ClientId == id)
                .OrderByDescending(note => note.SharedAt).Take(100)
                .Select(note => new { note.Id, note.Content, note.SharedAt }).ToListAsync(context.RequestAborted);
            var tasks = await db.ClientTasks.AsNoTracking().Where(task => task.ClientId == id)
                .OrderByDescending(task => task.CreatedAt).Take(100)
                .Select(task => new { task.Id, task.Title, task.Description, task.Pillar, task.DueAt, task.CompletedAt, task.CreatedAt })
                .ToListAsync(context.RequestAborted);
            return Results.Ok(new { notes, tasks });
        });
    }

    private static Task<bool> IsClient(CatalysticDbContext db, int id, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(user => user.Id == id && user.Role == "Client", cancellationToken);

    private static async Task<bool> ValidCsrf(HttpContext context, IAntiforgery csrf)
    {
        try
        {
            await csrf.ValidateRequestAsync(context);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
    }
}
