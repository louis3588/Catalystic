
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using CatalysticBackend.Service.DTOs;
using System.Security.Claims;

namespace CatalysticBackend.Service;

public class ProfileService
{
    private readonly CatalysticDbContext _dbContext;
    
    public ProfileService(CatalysticDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModelResult<UserProfile>> GetUserProfile(ClaimsPrincipal principal)
    {
        // The account identity comes from the authenticated session, never the request.
        if (principal.Identity?.IsAuthenticated != true ||
            !principal.IsInRole("Client") ||
            !int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ||
            userId <= 0)
        {
            return ModelResult<UserProfile>.Failure("Failed to get user");
        }

        var user = await _dbContext.Users
            .FindAsync(userId);

        if (user == null)
        {
            return ModelResult<UserProfile>.Failure("Failed to get user");
        }

        var result = ToProfile(user);
        return ModelResult<UserProfile>.Success(result);
    }
    
    

    public static UserProfile ToProfile(User user)
    {
        return new UserProfile
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            DateOfBirth = user.DateOfBirth,
            ProfileImageUrl = user.ProfileImageUrl
        };
    }
    

}
