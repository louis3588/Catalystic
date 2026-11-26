using System.Security.Claims;
using CatalysticBackend.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using CatalysticBackend.Models;

namespace CatalysticBackend.Service;

public sealed class SessionValidation(CatalysticDbContext dbContext) : CookieAuthenticationEvents
{
    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;
        var idValue = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var versionValue = principal?.FindFirstValue("session_version");

        if (!int.TryParse(idValue, out var id) || id <= 0 ||
            !Guid.TryParse(versionValue, out var version) || version == Guid.Empty)
        {
            await RejectAsync(context);
            return;
        }

        var account = await dbContext.Users.AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new { user.AccountState, user.Role, user.SessionVersion, user.MustChangePassword })
            .SingleOrDefaultAsync(context.HttpContext.RequestAborted);

        if (account is null || account.AccountState != AccountState.Activated || account.MustChangePassword ||
            account.SessionVersion != version ||
            (account.Role != "Admin" && account.Role != "Client") ||
            !principal!.IsInRole(account.Role))
        {
            await RejectAsync(context);
        }
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
