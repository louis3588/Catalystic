namespace CatalysticBackend.Service;

public record ScheduleEventInput(string? Type, string? Title, string? Description, DateTime? StartsAt, DateTime? EndsAt, long? CallRequestId);
