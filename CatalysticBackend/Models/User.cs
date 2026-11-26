using System;

namespace CatalysticBackend.Models;

//User model
public class User
{
    public int Id  { get; set; }
    
    public string FirstName { get; set; }= string.Empty; //max
    public string? LastName { get; set; }
    //Contact details are a must for notifications
    public string Email { get; set; }= string.Empty;
    public string? PasswordHash { get; set; }
    // Existing records remain inactive until explicitly provisioned.
    public AccountState AccountState { get; set; } = AccountState.Unactivated;
    public string? Role { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? BootstrapExpiresAt { get; set; }
    public Guid SessionVersion { get; set; }
    public string PhoneNumber { get; set; }= string.Empty;
    public string City { get; set; }= string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string ProfileImageUrl { get; set; }= string.Empty;
    //If email is not confirmed they are not full user and have no capabilities
    public bool IsEmailVerified { get; set; }= false;
    public bool IsPhoneNumberVerified {get; set;}
    public string? ConfirmationTokenHash { get; set; }= string.Empty;
    public DateTime? ConformationTokenExpiry { get; set; }
    public DateTime? LastLogin { get; set; }= DateTime.MinValue;
}
