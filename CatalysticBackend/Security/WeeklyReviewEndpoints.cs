using System.Security.Claims;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CatalysticBackend.Security;

public static partial class WeeklyReviewEndpoints
{

    public static void MapWeeklyReviewEndpoints(this WebApplication app)
    {
        var client = app.MapGroup("/api/client/weekly-reviews").RequireAuthorization("Client");
        client.MapPost("/", async (Submit input, CatalysticDbContext db, IAntiforgery csrf, HttpContext context, TimeProvider clock, CancellationToken token) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Results.Unauthorized();
            var today = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
            decimal?[] values = [input.Physical, input.Dietary, input.Social, input.Emotional, input.Intellectual, input.Habitual, input.Financial];
            if (input.NextReviewDate is null || input.NextReviewDate <= today || input.NextReviewDate > today.AddDays(7) || values.Any(v => !v.HasValue || v < 0 || v > 10 || decimal.Round(v.Value, 2) != v.Value)) return Results.BadRequest();
            var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId && u.Role == "Client", token);
            if (user is null) return Results.Unauthorized();
            var data = await db.ClientData.SingleOrDefaultAsync(c => c.UserId == userId, token);
            if (data is null) { data = new ClientData { UserId = user.Id, FirstName = user.FirstName, LastName = user.LastName }; db.ClientData.Add(data); }
            if (await db.WeeklyReviews.AnyAsync(r => r.ClientData.UserId == userId && r.Status != "Completed", token)) return Results.Conflict();
            db.WeeklyReviews.Add(new WeeklyReview { ClientData = data, ReviewDate = today, NextReviewDate = input.NextReviewDate.Value,
                Physical = input.Physical!.Value, Dietary = input.Dietary!.Value, Social = input.Social!.Value, Emotional = input.Emotional!.Value,
                Intellectual = input.Intellectual!.Value, Habitual = input.Habitual!.Value, Financial = input.Financial!.Value, SubmittedAt = clock.GetUtcNow().UtcDateTime });
            try { await db.SaveChangesAsync(token); }
            catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { return Results.Conflict(); }
            return Results.StatusCode(201);
        });

        var admin = app.MapGroup("/api/admin/clients/{clientId:int}/weekly-reviews").RequireAuthorization("Admin");
        admin.MapGet("/", async (int clientId, CatalysticDbContext db, HttpContext context, CancellationToken token) => {
            context.Response.Headers.CacheControl = "no-store";
            return Results.Ok(await db.WeeklyReviews.AsNoTracking().Where(r => r.ClientData.UserId == clientId).OrderByDescending(r => r.SubmittedAt)
                .Select(r => new { r.Id, r.ReviewDate, r.NextReviewDate, r.Status, r.Physical, r.Dietary, r.Social, r.Emotional, r.Intellectual, r.Habitual, r.Financial }).ToListAsync(token));
        });
        admin.MapPost("/{reviewId:long}/contacted", async (int clientId, long reviewId, CatalysticDbContext db, IAntiforgery csrf, HttpContext context, TimeProvider clock, CancellationToken token) => {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            var changed = await db.WeeklyReviews.Where(r => r.Id == reviewId && r.ClientData.UserId == clientId && r.Status == "PendingContact")
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, "Contacted").SetProperty(r => r.ContactedAt, clock.GetUtcNow().UtcDateTime), token);
            return changed == 1 ? Results.NoContent() : Results.Conflict();
        });
        admin.MapPost("/{reviewId:long}/complete", async (int clientId, long reviewId, Finish input, CatalysticDbContext db, IAntiforgery csrf, HttpContext context, TimeProvider clock, CancellationToken token) => {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (input.Preos is null || input.Preos < 0 || input.Preos > 10 || decimal.Round(input.Preos.Value, 2) != input.Preos.Value) return Results.BadRequest();
            await using var tx = await db.Database.BeginTransactionAsync(token);
            var changed = await db.WeeklyReviews.Where(r => r.Id == reviewId && r.ClientData.UserId == clientId && r.Status == "Contacted")
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, "Completed").SetProperty(r => r.CompletedAt, clock.GetUtcNow().UtcDateTime), token);
            if (changed != 1) return Results.Conflict();
            var review = await db.WeeklyReviews.AsNoTracking().SingleAsync(r => r.Id == reviewId, token);
            if (await db.WellnessAssessments.AnyAsync(a => a.ClientDataId == review.ClientDataId && a.AssessmentDate == review.ReviewDate, token)) return Results.Conflict();
            db.WellnessAssessments.Add(new WellnessAssessment { ClientDataId = review.ClientDataId, AssessmentDate = review.ReviewDate, Physical = review.Physical, Dietary = review.Dietary, Social = review.Social, Emotional = review.Emotional, Intellectual = review.Intellectual, Habitual = review.Habitual, Financial = review.Financial, PREOS = input.Preos.Value, CreatedAt = clock.GetUtcNow().UtcDateTime });
            try { await db.SaveChangesAsync(token); }
            catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation }) { return Results.Conflict(); }
            await tx.CommitAsync(token);
            return Results.NoContent();
        });
    }

    private static async Task<bool> ValidCsrf(HttpContext context, IAntiforgery csrf)
    {
        try { await csrf.ValidateRequestAsync(context); return true; }
        catch (AntiforgeryValidationException) { return false; }
    }
}
