namespace CatalysticBackend.Service;

public sealed record BlogInput(string? Title, string? Content, bool IsPublished, DateTime? ReleaseTime, int[]? LabelIds = null);
