using System.Text;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using CatalysticBackend.Service;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalysticBackend.Tests;

public class CalendarServiceTests
{
    private static CatalysticDbContext Database() => new(new DbContextOptionsBuilder<CatalysticDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Scheduling_resolves_the_owning_request_and_rejects_overlaps()
    {
        await using var db = Database();
        db.Users.AddRange(new User { Id = 1, Role = "Client", AccountState = AccountState.Activated },
            new User { Id = 2, Role = "Client", AccountState = AccountState.Activated });
        db.CallRequests.Add(new CallRequest { Id = 1, ClientId = 1, Message = "Please call" });
        await db.SaveChangesAsync();
        var service = new CalendarService(db, TimeProvider.System);
        var start = DateTime.UtcNow.AddDays(10);
        var input = new ScheduleEventInput("Call", "Review", "Details", start, start.AddHours(1), 1);

        Assert.NotNull(await service.ScheduleAsync(2, input, default));
        Assert.Null(await service.ScheduleAsync(1, input, default));
        Assert.Equal("Scheduled", (await db.CallRequests.SingleAsync()).Status);
        Assert.NotNull(await service.ScheduleAsync(2, new ScheduleEventInput("Meeting", "Overlap", "", start.AddMinutes(30), start.AddHours(2), null), default));
        Assert.Single(await service.ListAsync(1, default));
        Assert.Empty(await service.ListAsync(2, default));
    }

    [Fact]
    public async Task Scheduling_rejects_invalid_or_non_utc_times()
    {
        await using var db = Database();
        var service = new CalendarService(db, TimeProvider.System);
        Assert.NotNull(await service.ScheduleAsync(1, new ScheduleEventInput("Call", "Test", "", new DateTime(2099, 1, 1), new DateTime(2099, 1, 2), null), default));
        Assert.NotNull(await service.ScheduleAsync(1, new ScheduleEventInput("Call", "Test", "", DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(1), null), default));
    }

    [Fact]
    public void Calendar_file_escapes_text_and_exports_utc_times()
    {
        var call = new ScheduledCall { Id = 5, Title = "Test, call", Description = "Line one\nBEGIN:VEVENT", StartsAt = new DateTime(2099, 2, 1, 10, 0, 0, DateTimeKind.Utc), EndsAt = new DateTime(2099, 2, 1, 11, 0, 0, DateTimeKind.Utc) };
        var result = Encoding.UTF8.GetString(CalendarFile.Create(call, DateTime.UtcNow));
        Assert.Contains("SUMMARY:Test\\, call\r\n", result);
        Assert.Contains("DESCRIPTION:Line one\\nBEGIN:VEVENT\r\n", result);
        Assert.Contains("DTSTART:20990201T100000Z\r\n", result);
        Assert.Equal(2, result.Split("\r\nBEGIN:VEVENT\r\n").Length);
    }

    [Fact]
    public async Task Adjacent_appointments_are_allowed_but_inactive_clients_and_resolved_requests_are_rejected()
    {
        await using var db = Database();
        db.Users.AddRange(new User { Id = 1, Role = "Client", AccountState = AccountState.Activated },
            new User { Id = 2, Role = "Client", AccountState = AccountState.Dormant });
        db.CallRequests.Add(new CallRequest { Id = 4, ClientId = 1, Message = "Resolved", Status = "Declined" });
        await db.SaveChangesAsync();
        var service = new CalendarService(db, TimeProvider.System);
        var start = DateTime.UtcNow.AddDays(3);

        Assert.Null(await service.ScheduleAsync(1, new ScheduleEventInput("Meeting", "First", "", start, start.AddHours(1), null), default));
        Assert.Null(await service.ScheduleAsync(1, new ScheduleEventInput("Call", "Second", "", start.AddHours(1), start.AddHours(2), null), default));
        Assert.NotNull(await service.ScheduleAsync(2, new ScheduleEventInput("Call", "Inactive", "", start.AddHours(2), start.AddHours(3), null), default));
        Assert.NotNull(await service.ScheduleAsync(1, new ScheduleEventInput("Call", "Resolved", "", start.AddHours(2), start.AddHours(3), 4), default));
        Assert.Equal(2, await db.CalendarEvents.CountAsync());
    }

    [Fact]
    public void All_day_task_export_folds_utf8_lines_and_rejects_missing_due_dates()
    {
        var task = new ClientTask { Id = 6, Title = new string('é', 90), Description = "One; two, three\\four", DueAt = new DateTime(2030, 1, 31, 0, 0, 0, DateTimeKind.Utc) };
        var file = Encoding.UTF8.GetString(CalendarFile.Create(task, DateTime.UtcNow));
        Assert.Contains("DTSTART;VALUE=DATE:20300131\r\n", file);
        Assert.Contains("DTEND;VALUE=DATE:20300201\r\n", file);
        Assert.Contains("DESCRIPTION:One\\; two\\, three\\\\four", file);
        Assert.All(file.Split("\r\n"), line => Assert.True(Encoding.UTF8.GetByteCount(line) <= 75));
        Assert.Throws<ArgumentException>(() => CalendarFile.Create(new ClientTask(), DateTime.UtcNow));
    }
}
