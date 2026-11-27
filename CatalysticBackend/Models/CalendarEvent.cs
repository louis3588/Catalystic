namespace CatalysticBackend.Models;

public abstract class CalendarEvent
{
    public long Id { get; set; }
    public int ClientId { get; set; }
    public User Client { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
