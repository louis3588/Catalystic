namespace CatalysticBackend.Domain;

/// <summary>
/// Subjective coaching indicators, not clinical measurements.
/// </summary>
public sealed record WellnessScores
{
    public decimal Physical { get; }
    public decimal Dietary { get; }
    public decimal Social { get; }
    public decimal Emotional { get; }
    public decimal Intellectual { get; }
    public decimal Habitual { get; }
    public decimal Financial { get; }

    // PREOS: proactiveness, responsiveness, and effectiveness of services.
    // This is a subjective rating recorded by the practitioner for administrative use.
    public decimal PREOS { get; }

    public decimal Total => Physical + Dietary + Social + Emotional + Intellectual + Habitual + Financial + PREOS;
    public decimal Mean => Total / 8m;

    public WellnessScores(decimal physical, decimal dietary, decimal social, decimal emotional,
        decimal intellectual, decimal habitual, decimal financial, decimal preos)
    {
        decimal[] values = [physical, dietary, social, emotional, intellectual, habitual, financial, preos];

        if (values.Any(value => value < 0 || value > 10))
        {
            throw new ArgumentOutOfRangeException(nameof(values), "Each score must be between 0 and 10.");
        }

        Physical = physical;
        Dietary = dietary;
        Social = social;
        Emotional = emotional;
        Intellectual = intellectual;
        Habitual = habitual;
        Financial = financial;
        PREOS = preos;
    }
}
