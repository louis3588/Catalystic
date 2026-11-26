namespace CatalysticBackend.Service;

public record AssessmentInput(DateOnly? AssessmentDate, decimal? Physical, decimal? Dietary, decimal? Social,
    decimal? Emotional, decimal? Intellectual, decimal? Habitual, decimal? Financial, decimal? PREOS);
