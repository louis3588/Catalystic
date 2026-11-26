namespace CatalysticBackend.Security;

public static partial class ClientWorkEndpoints
{
    public record TaskRequest(string? Title, string? Description, string? Pillar, DateTime? DueAt);
}
