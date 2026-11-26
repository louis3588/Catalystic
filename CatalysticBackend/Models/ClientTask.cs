namespace CatalysticBackend.Models;

public class ClientTask
{
    public long Id { get; set; }
    public int ClientId { get; set; }
    public User Client { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Pillar { get; set; } = string.Empty;
    public DateTime? DueAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
