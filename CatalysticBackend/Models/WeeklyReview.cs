namespace CatalysticBackend.Models;

public class WeeklyReview
{
    public long Id { get; set; }
    public int ClientDataId { get; set; }
    public ClientData ClientData { get; set; } = null!;
    public DateOnly ReviewDate { get; set; }
    public DateOnly NextReviewDate { get; set; }
    public decimal Physical { get; set; }
    public decimal Dietary { get; set; }
    public decimal Social { get; set; }
    public decimal Emotional { get; set; }
    public decimal Intellectual { get; set; }
    public decimal Habitual { get; set; }
    public decimal Financial { get; set; }
    public string Status { get; set; } = "PendingContact";
    public DateTime SubmittedAt { get; set; }
    public DateTime? ContactedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
