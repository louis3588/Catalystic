namespace CatalysticBackend.Models;

public class WellnessAssessment
{
    public long Id { get; set; }
    public int ClientDataId { get; set; }
    public ClientData ClientData { get; set; } = null!;
    public DateOnly AssessmentDate { get; set; }
    public decimal Physical { get; set; }
    public decimal Dietary { get; set; }
    public decimal Social { get; set; }
    public decimal Emotional { get; set; }
    public decimal Intellectual { get; set; }
    public decimal Habitual { get; set; }
    public decimal Financial { get; set; }
    public decimal PREOS { get; set; }
    public DateTime CreatedAt { get; set; }
}
