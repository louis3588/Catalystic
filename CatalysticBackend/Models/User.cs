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
    
    public string PasswordHash { get; set; }= string.Empty;
    public string PhoneNumber { get; set; }= string.Empty;
    public string City { get; set; }= string.Empty;
    public DateTime DateOfBirth { get; set; }= DateTime.MinValue;
    public string ProfileImageUrl { get; set; }= string.Empty;
    //If email is not confirmed they are not full user and have no capabilities
    // TODO(2): Implement email confirmation system
    public bool IsEmailVerified { get; set; }= false;
    public string? ComfirmationTokenHash { get; set; }= string.Empty;
    public DateTime? ConformationTokenExpiry { get; set; }
    public DateTime? LastLogin { get; set; }= DateTime.MinValue;
}