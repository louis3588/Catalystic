namespace CatalysticBackend.Models;

public abstract class TimedEvent : CalendarEvent
{
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
}
