namespace CatalysticBackend.Models;

public class ClientInvitation
{
    public long Id { get; set; }
    public int ClientId { get; set; }
    public User Client { get; set; } = null!;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public int IssuedById { get; set; }
    public User IssuedBy { get; set; } = null!;
}
