using System;
using System.Threading.Tasks;
using CatalysticBackend.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using CatalysticBackend.Service;
using CatalysticBackend.Security;
using Microsoft.AspNetCore.Identity;
using CatalysticBackend.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.NoClobber().Load();
var connectionString =
    $"Host={Environment.GetEnvironmentVariable("PGHOST")};" +
    $"Database={Environment.GetEnvironmentVariable("PGDATABASE")};" +
    $"Username={Environment.GetEnvironmentVariable("PGUSER")};" +
    $"Password={Environment.GetEnvironmentVariable("PGPASSWORD")};" +
    $"SSL Mode={Environment.GetEnvironmentVariable("PGSSLMODE")};" +
    $"Channel Binding={Environment.GetEnvironmentVariable("PGCHANNELBINDING")}";

builder.Services.AddDbContext<CatalysticDbContext>(option =>
{
    option.UseNpgsql(connectionString);
});


builder.Services.AddScoped<SessionValidation>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<AssessmentService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-Catalystic.Antiforgery";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("authentication", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));
    options.AddPolicy("enquiries", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(15), QueueLimit = 0 }));
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "__Host-Catalystic.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
        options.EventsType = typeof(SessionValidation);
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("Client", policy => policy.RequireRole("Client"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Asistym API",
            Version = "v1"
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (args.Length == 1 && args[0] == "bootstrap-admin")
{
    await AdminBootstrap.RunAsync(app.Services);
    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(options =>
    {
        // Relative to /swagger/index.html, including when hosted under a path base.
        options.SwaggerEndpoint("../openapi/v1.json", "Asistym API v1");
    });
}
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapAccountEndpoints();
app.MapEnquiryEndpoints();
app.MapClientWorkEndpoints();
app.MapInvitationEndpoints();
app.MapBlogEndpoints();
app.MapAssessmentEndpoints();
app.MapWeeklyReviewEndpoints();
app.MapCalendarEndpoints();
app.Run();

