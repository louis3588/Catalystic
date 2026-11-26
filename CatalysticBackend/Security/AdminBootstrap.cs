using System.Data;
using System.Security.Cryptography;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CatalysticBackend.Security;

public static class AdminBootstrap
{
    public static async Task RunAsync(IServiceProvider services)
    {
        Console.Write("Existing administrator email: ");
        var email = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(email)) throw new InvalidOperationException("Email is required.");
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalysticDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (await db.Users.AnyAsync(user => user.Role == "Admin"))
            throw new InvalidOperationException("An administrator already exists; bootstrap is disabled.");
        var matches = await db.Users.Where(user => user.Email.ToLower() == email.ToLower()).Take(2).ToListAsync();
        if (matches.Count != 1 || matches[0].AccountState != AccountState.Unactivated || !string.IsNullOrEmpty(matches[0].PasswordHash))
            throw new InvalidOperationException("Expected exactly one inactive account without a password.");
        var account = matches[0];
        var secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        account.PasswordHash = hasher.HashPassword(account, secret);
        account.Role = "Admin";
        account.AccountState = AccountState.Activated;
        account.IsEmailVerified = true;
        account.IsPhoneNumberVerified = true;
        account.MustChangePassword = true;
        account.BootstrapExpiresAt = DateTime.UtcNow.AddHours(1);
        account.SessionVersion = Guid.NewGuid();
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        Console.WriteLine("One-time password (expires in one hour; store it securely):");
        Console.WriteLine(secret);
    }
}
