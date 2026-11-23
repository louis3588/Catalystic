using System.ComponentModel.DataAnnotations;

namespace CatalysticBackend.Service.DTOs;

public class RegisterRequest
{
    [Required]
    [MaxLength(64)]
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MaxLength(16)]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    [MaxLength(256)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(256)]
    public string City { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
}