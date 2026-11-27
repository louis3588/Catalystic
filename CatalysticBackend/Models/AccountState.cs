namespace CatalysticBackend.Models;

// TODO(production): implement reviewed offboarding and a scheduled six-week retention workflow.
// Dormancy alone must not start or imply offboarding; retaining ClientData remains personal data.
public enum AccountState
{
    Unactivated = 0,
    Activated = 1,
    Dormant = 2,
}
