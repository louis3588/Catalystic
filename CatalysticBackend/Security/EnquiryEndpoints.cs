using System.ComponentModel.DataAnnotations;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;

namespace CatalysticBackend.Security;

public static partial class EnquiryEndpoints
{

    private static readonly string[] Statuses =
        ["New", "Under review", "Consultation requested", "Accepted", "Declined", "Converted to client", "Closed"];

    public static void MapEnquiryEndpoints(this WebApplication app)
    {
        app.MapPost("/api/enquiries", async (SubmitRequest request, HttpContext context, IAntiforgery csrf, CatalysticDbContext db) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();

            var name = request.Name?.Trim() ?? "";
            var email = request.Email?.Trim() ?? "";
            var goals = request.Goals?.Trim() ?? "";
            var message = request.Message?.Trim() ?? "";

            if (name.Length is < 1 or > 120 || email.Length is < 1 or > 254 ||
                !new EmailAddressAttribute().IsValid(email) || request.ContactPreference != "Email" ||
                goals.Length > 1000 || message.Length is < 1 or > 2000 || !request.Consent ||
                !string.IsNullOrEmpty(request.Website))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["form"] = ["Please check the form and try again."] });
            }

            db.Enquiries.Add(new Enquiry
            {
                Name = name,
                Email = email,
                ContactPreference = "Email",
                Goals = goals,
                Message = message,
                CreatedAt = DateTime.UtcNow,
            });

            await db.SaveChangesAsync(context.RequestAborted);
            return Results.StatusCode(StatusCodes.Status201Created);
        }).AllowAnonymous().RequireRateLimiting("enquiries");

        var admin = app.MapGroup("/api/admin/enquiries").RequireAuthorization("Admin");

        admin.MapGet("/", async (HttpContext context, CatalysticDbContext db) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            var items = await db.Enquiries.AsNoTracking().OrderByDescending(e => e.CreatedAt)
                .Take(100).Select(e => new { e.Id, e.Name, e.Email, e.ContactPreference, e.Goals, e.Message, e.Status, e.CreatedAt })
                .ToListAsync(context.RequestAborted);
            return Results.Ok(items);
        });

        admin.MapPatch("/{id:long}", async (long id, StatusRequest request, HttpContext context, IAntiforgery csrf, CatalysticDbContext db) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (request.Status == null || !Statuses.Contains(request.Status)) return Results.BadRequest();
            var enquiry = await db.Enquiries.FindAsync([id], context.RequestAborted);
            if (enquiry == null) return Results.NotFound();
            enquiry.Status = request.Status;
            await db.SaveChangesAsync(context.RequestAborted);
            return Results.NoContent();
        });
    }

    private static async Task<bool> ValidCsrf(HttpContext context, IAntiforgery csrf)
    {
        try
        {
            await csrf.ValidateRequestAsync(context);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
    }
}
