using System;

namespace CatalyticBackend.Models;

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
}