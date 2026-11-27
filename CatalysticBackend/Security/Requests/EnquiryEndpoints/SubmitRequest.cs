namespace CatalysticBackend.Security;

public static partial class EnquiryEndpoints
{
    public record SubmitRequest(string? Name, string? Email, string? ContactPreference, string? Goals, string? Message, bool Consent, string? Website);
}
