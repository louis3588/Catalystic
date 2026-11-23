namespace CatalysticBackend.Models;

// Private and shared content are separate tables to prevent accidental disclosure.
public class PractitionerNote
{
    public long Id { get; set; }
    public int ClientId { get; set; }
    public User Client { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SharedClientNote
{
    public long Id { get; set; }
    public int ClientId { get; set; }
    public User Client { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
}

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
