namespace CatalysticBackend.Security;

public static partial class AccountEndpoints
{
    public record BootstrapChange(string? Email, string? Password, string? NewPassword);
}
