namespace CatalysticBackend.Models;

public class SharedClientNote
{
    public long Id { get; set; }
    public int ClientId { get; set; }
    public User Client { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
}
