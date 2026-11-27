namespace CatalysticBackend.Models;

public class CallRequest
{
    public long Id { get; set; }
    public int ClientId { get; set; }
    public User Client { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public long? ScheduledCallId { get; set; }
    public ScheduledCall? ScheduledCall { get; set; }
}
