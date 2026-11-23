using System.Security.Claims;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using CatalysticBackend.Service;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalysticBackend.Tests;

public class ClientOwnershipTests
{
    [Fact]
    public async Task Profile_uses_authenticated_identity_and_rejects_other_roles()
    {
        var options = new DbContextOptionsBuilder<CatalysticDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new CatalysticDbContext(options);
        db.Users.AddRange(new User { Id = 1, FirstName = "First" }, new User { Id = 2, FirstName = "Second" });
        await db.SaveChangesAsync();
        var service = new ProfileService(db);
        var client = Principal("1", "Client");
        var result = await service.GetUserProfile(client);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.Id);
        Assert.False((await service.GetUserProfile(Principal("2", "Admin"))).IsSuccess);
        Assert.False((await service.GetUserProfile(new ClaimsPrincipal())).IsSuccess);
    }

    private static ClaimsPrincipal Principal(string id, string role) => new(new ClaimsIdentity(
        new[] { new Claim(ClaimTypes.NameIdentifier, id), new Claim(ClaimTypes.Role, role) }, "test"));
}
