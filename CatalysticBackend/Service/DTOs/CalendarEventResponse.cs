namespace CatalysticBackend.Service;

public record CalendarEventResponse(long Id, string Type, string Title, string Description, DateTime StartsAt, DateTime EndsAt);
