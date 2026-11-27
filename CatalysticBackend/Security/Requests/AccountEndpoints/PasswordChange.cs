namespace CatalysticBackend.Security;

public static partial class AccountEndpoints
{
    public record PasswordChange(string? CurrentPassword, string? NewPassword);
}
