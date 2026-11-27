namespace CatalysticBackend.Service;

public sealed record BlogResponse(int Id, string Title, string Content, bool IsPublished, DateTime ReleaseTime, DateTime CreatedAt, DateTime UpdatedAt, List<LabelResponse> Labels);
