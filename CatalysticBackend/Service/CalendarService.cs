using CatalysticBackend.Data;
using CatalysticBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalysticBackend.Service;

public sealed class CalendarService(CatalysticDbContext db, TimeProvider clock)
{
    public async Task<List<CalendarEventResponse>> ListAsync(int clientId, CancellationToken token)
    {
        var events = await db.Set<TimedEvent>().AsNoTracking()
            .Where(item => item.ClientId == clientId)
            .OrderBy(item => item.StartsAt).ToListAsync(token);

        return events.Select(item => new CalendarEventResponse(item.Id,
            item is ScheduledCall ? "Call" : "Meeting", item.Title, item.Description,
            item.StartsAt, item.EndsAt)).ToList();
    }

    public async Task<string?> ScheduleAsync(int clientId, ScheduleEventInput input, CancellationToken token)
    {
        var title = input.Title?.Trim() ?? "";
        var description = input.Description?.Trim() ?? "";
        if (input.Type is not ("Call" or "Meeting") || title.Length is < 1 or > 160 || description.Length > 4000 ||
            input.StartsAt is null || input.EndsAt is null || input.StartsAt.Value.Kind != DateTimeKind.Utc ||
            input.EndsAt.Value.Kind != DateTimeKind.Utc || input.EndsAt <= input.StartsAt ||
            input.StartsAt <= clock.GetUtcNow().UtcDateTime || (input.CallRequestId != null && input.Type != "Call"))
            return "Invalid event details.";

        // One practitioner is supported initially. The transaction-scoped lock serializes all
        // booking checks across application instances, including concurrent requests.
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(token) : null;
        if (db.Database.IsNpgsql())
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(714938201)", token);

        if (!await db.Users.AnyAsync(user => user.Id == clientId && user.Role == "Client" && user.AccountState == AccountState.Activated, token))
            return "Client not found or inactive.";

        CallRequest? request = null;
        if (input.CallRequestId is not null)
        {
            request = await db.CallRequests.SingleOrDefaultAsync(item => item.Id == input.CallRequestId && item.ClientId == clientId && item.Status == "Pending", token);
            if (request is null) return "Call request not found or already resolved.";
        }

        var overlap = await db.Set<TimedEvent>().AnyAsync(item =>
            item.StartsAt < input.EndsAt && input.StartsAt < item.EndsAt, token);
        if (overlap) return "That time overlaps an existing appointment.";

        TimedEvent scheduled = input.Type == "Call" ? new ScheduledCall() : new ScheduledMeeting();
        scheduled.ClientId = clientId;
        scheduled.Title = title;
        scheduled.Description = description;
        scheduled.StartsAt = input.StartsAt.Value;
        scheduled.EndsAt = input.EndsAt.Value;
        scheduled.CreatedAt = clock.GetUtcNow().UtcDateTime;
        db.CalendarEvents.Add(scheduled);
        if (request is not null)
        {
            request.Status = "Scheduled";
            request.ResolvedAt = clock.GetUtcNow().UtcDateTime;
            request.ScheduledCall = (ScheduledCall)scheduled;
        }
        await db.SaveChangesAsync(token);
        if (transaction is not null) await transaction.CommitAsync(token);
        return null;
    }
}
