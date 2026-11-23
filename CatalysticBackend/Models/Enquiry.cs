namespace CatalysticBackend.Models;

public class Enquiry
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ContactPreference { get; set; } = string.Empty;
    public string Goals { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "New";
    public DateTime CreatedAt { get; set; }
}
