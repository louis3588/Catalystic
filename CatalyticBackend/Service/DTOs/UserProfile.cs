using System;

namespace CatalyticBackend.Service;

public class UserProfile
{
    public int? Id { get; set; }
    public string? LastName { get; set; }
    //Contact details are a must for notifications
    public string Email { get; set; }= string.Empty;
    public string PhoneNumber { get; set; }= string.Empty;
    public string City { get; set; }= string.Empty;
    public DateTime DateOfBirth { get; set; }= DateTime.MinValue;
    public string ProfileImageUrl { get; set; }= string.Empty;
}