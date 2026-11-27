using System.Security.Claims;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using CatalysticBackend.Service;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;

namespace CatalysticBackend.Security;

public static class CalendarEndpoints
{
    public static void MapCalendarEndpoints(this WebApplication app)
    {
        var client = app.MapGroup("/api/client/calendar").RequireAuthorization("Client");
        client.MapGet("/", async (HttpContext context, CalendarService calendar) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            return CurrentClient(context) is int id
                ? Results.Ok(await calendar.ListAsync(id, context.RequestAborted)) : Results.Unauthorized();
        });
        client.MapGet("/requests", async (HttpContext context, CatalysticDbContext db) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            if (CurrentClient(context) is not int id) return Results.Unauthorized();
            return Results.Ok(await db.CallRequests.AsNoTracking().Where(item => item.ClientId == id)
                .OrderByDescending(item => item.CreatedAt).Take(50)
                .Select(item => new { item.Id, item.Message, item.Status, item.CreatedAt }).ToListAsync(context.RequestAborted));
        });
        client.MapPost("/requests", async (CallRequestInput input, HttpContext context, CatalysticDbContext db, IAntiforgery csrf, TimeProvider clock) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (CurrentClient(context) is not int id) return Results.Unauthorized();
            var message = input.Message?.Trim() ?? "";
            if (message.Length is < 1 or > 1000) return Results.BadRequest(new { error = "Please enter a message of up to 1,000 characters." });
            db.CallRequests.Add(new CallRequest { ClientId = id, Message = message, CreatedAt = clock.GetUtcNow().UtcDateTime });
            await db.SaveChangesAsync(context.RequestAborted);
            return Results.StatusCode(StatusCodes.Status201Created);
        });
        client.MapGet("/events/{eventId:long}/file", async (long eventId, HttpContext context, CatalysticDbContext db, TimeProvider clock) =>
        {
            if (CurrentClient(context) is not int id) return Results.Unauthorized();
            return await FileForEvent(db, context, eventId, id, clock);
        });

        var admin = app.MapGroup("/api/admin/clients/{clientId:int}/calendar").RequireAuthorization("Admin");
        admin.MapGet("/", async (int clientId, HttpContext context, CalendarService calendar, CatalysticDbContext db) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            var requests = await db.CallRequests.AsNoTracking().Where(item => item.ClientId == clientId)
                .OrderByDescending(item => item.CreatedAt).Take(50)
                .Select(item => new { item.Id, item.Message, item.Status, item.CreatedAt }).ToListAsync(context.RequestAborted);
            return Results.Ok(new { events = await calendar.ListAsync(clientId, context.RequestAborted), requests });
        });
        admin.MapPost("/events", async (int clientId, ScheduleEventInput input, HttpContext context, IAntiforgery csrf, CalendarService calendar) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            try
            {
                var error = await calendar.ScheduleAsync(clientId, input, context.RequestAborted);
                return error is null ? Results.StatusCode(StatusCodes.Status201Created) : Results.BadRequest(new { error });
            }
            catch (DbUpdateConcurrencyException) { return Results.Conflict(new { error = "The request was already updated. Refresh and try again." }); }
        });
        admin.MapPost("/requests/{requestId:long}/decline", async (int clientId, long requestId, HttpContext context, IAntiforgery csrf, CatalysticDbContext db, TimeProvider clock) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            var request = await db.CallRequests.SingleOrDefaultAsync(item => item.Id == requestId && item.ClientId == clientId && item.Status == "Pending", context.RequestAborted);
            if (request is null) return Results.NotFound();
            request.Status = "Declined";
            request.ResolvedAt = clock.GetUtcNow().UtcDateTime;
            try { await db.SaveChangesAsync(context.RequestAborted); }
            catch (DbUpdateConcurrencyException) { return Results.Conflict(); }
            return Results.NoContent();
        });
        admin.MapGet("/events/{eventId:long}/file", async (int clientId, long eventId, HttpContext context, CatalysticDbContext db, TimeProvider clock) =>
            await FileForEvent(db, context, eventId, clientId, clock));
    }

    private static async Task<IResult> FileForEvent(CatalysticDbContext db, HttpContext context, long eventId, int clientId, TimeProvider clock)
    {
        context.Response.Headers.CacheControl = "no-store";
        var item = await db.CalendarEvents.AsNoTracking().SingleOrDefaultAsync(item => item.Id == eventId && item.ClientId == clientId, context.RequestAborted);
        if (item is null || item is ClientTask { DueAt: null }) return Results.NotFound();
        return Results.File(CalendarFile.Create(item, clock.GetUtcNow().UtcDateTime), "text/calendar; charset=utf-8", "asistym-event.ics");
    }

    private static int? CurrentClient(HttpContext context) =>
        int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0 ? id : null;

    private static async Task<bool> ValidCsrf(HttpContext context, IAntiforgery csrf)
    {
        try { await csrf.ValidateRequestAsync(context); return true; }
        catch (AntiforgeryValidationException) { return false; }
    }
}
