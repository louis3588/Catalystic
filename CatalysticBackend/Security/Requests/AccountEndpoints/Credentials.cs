namespace CatalysticBackend.Security;

public static partial class AccountEndpoints
{
    public record Credentials(string? Email, string? Password);
}
