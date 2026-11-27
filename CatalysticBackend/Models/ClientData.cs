namespace CatalysticBackend.Models;

// Retained client information remains personal data, including after unlinking the account.
public class ClientData
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
}
