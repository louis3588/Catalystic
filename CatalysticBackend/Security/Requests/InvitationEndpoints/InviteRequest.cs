namespace CatalysticBackend.Security;

public static partial class InvitationEndpoints
{
    public record InviteRequest(string? FirstName, string? LastName, string? Email, bool ManuallyApproved);
}
