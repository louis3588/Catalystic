namespace CatalysticBackend.Security;

public static partial class WeeklyReviewEndpoints
{
    public record Submit(DateOnly? NextReviewDate, decimal? Physical, decimal? Dietary, decimal? Social, decimal? Emotional, decimal? Intellectual, decimal? Habitual, decimal? Financial);
}
