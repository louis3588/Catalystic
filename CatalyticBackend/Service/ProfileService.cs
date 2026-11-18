
using CatalyticBackend.Data;
using CatalyticBackend.Models;
using CatalyticBackend.Service.DTOs;

namespace CatalyticBackend.Service;

public class ProfileService
{
    private readonly CatalysticDbContext _dbContext;
    
    public ProfileService(CatalysticDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ModelResult<UserProfile>> GetUserProfile(int userId)
    {
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