namespace CatalysticBackend.Security;

public static partial class InvitationEndpoints
{
    public record AcceptRequest(string? Token, string? Password);
}
