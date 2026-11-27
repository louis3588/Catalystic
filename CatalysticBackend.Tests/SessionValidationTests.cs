using System.Security.Claims;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using CatalysticBackend.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CatalysticBackend.Tests;

public class SessionValidationTests
{
    [Theory]
    [InlineData(true, false, true, "Client", "Client", true)]
    [InlineData(false, false, true, "Client", "Client", false)]
    [InlineData(true, true, true, "Admin", "Admin", false)]
    [InlineData(true, false, false, "Client", "Client", false)]
    [InlineData(true, false, true, "Client", "Admin", false)]
    public async Task Session_is_valid_only_while_account_and_cookie_match(
        bool active, bool mustChange, bool sameVersion, string storedRole, string claimRole, bool expected)
    {
        var options = new DbContextOptionsBuilder<CatalysticDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new CatalysticDbContext(options);
        var version = Guid.NewGuid();
        db.Users.Add(new User { Id = 1, AccountState = active ? AccountState.Activated : AccountState.Dormant, MustChangePassword = mustChange,
            SessionVersion = version, Role = storedRole });
        await db.SaveChangesAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
        await using var provider = services.BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = provider };
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, claimRole),
            new Claim("session_version", (sameVersion ? version : Guid.NewGuid()).ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var scheme = new AuthenticationScheme("Cookies", null, typeof(CookieAuthenticationHandler));
        var ticket = new AuthenticationTicket(principal, scheme.Name);
        var context = new CookieValidatePrincipalContext(http, scheme, new CookieAuthenticationOptions(), ticket);
        await new SessionValidation(db).ValidatePrincipal(context);
        Assert.Equal(expected, context.Principal != null);
    }
}
