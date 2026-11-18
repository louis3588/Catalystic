namespace CatalyticBackend.Models;

//User model
public class User
{
    public int Id;
    
    public string FirstName = string.Empty; //max
    public string? LastName;
    //Contact details are a must for notifications
    public string Email = string.Empty;
    public string PhoneNumber = string.Empty;
    public string City = string.Empty;
    public DateTime DateOfBirth;
    public string ProfileImageUrl = string.Empty;
}