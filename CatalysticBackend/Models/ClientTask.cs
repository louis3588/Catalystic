namespace CatalysticBackend.Models;

public class ClientTask : CalendarEvent
{
    public string Pillar { get; set; } = string.Empty;
    public DateTime? DueAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
