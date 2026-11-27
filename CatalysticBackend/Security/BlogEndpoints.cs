using CatalysticBackend.Service;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using CatalysticBackend.Data;
using CatalysticBackend.Models;
using System.Text.RegularExpressions;

namespace CatalysticBackend.Security;

public static partial class BlogEndpoints
{
    public static void MapBlogEndpoints(this WebApplication app)
    {
        var publicBlog = app.MapGroup("/api/blog").AllowAnonymous();
        publicBlog.MapGet("/", async (int? labelId, BlogService service, CancellationToken token) =>
            Results.Ok(await service.ListAsync(false, token, labelId)));
        publicBlog.MapGet("/labels", async (CatalysticDbContext db, TimeProvider clock, CancellationToken token) =>
        {
            var now = clock.GetUtcNow().UtcDateTime;
            return Results.Ok(await db.BlogLabels.AsNoTracking().Where(label => label.Posts.Any(post => post.IsPublished && post.ReleaseTime <= now))
                .OrderBy(label => label.Name).Select(label => new LabelResponse(label.Id, label.Name, label.Colour)).ToListAsync(token));
        });
        publicBlog.MapGet("/{id:int}", async (int id, BlogService service, CancellationToken token) =>
        {
            var post = await service.GetPublicAsync(id, token);
            return post is null ? Results.NotFound() : Results.Ok(post);
        });

        var admin = app.MapGroup("/api/admin/blog").RequireAuthorization("Admin");
        admin.MapGet("/labels", async (CatalysticDbContext db, HttpContext context, CancellationToken token) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            return Results.Ok(await db.BlogLabels.AsNoTracking().OrderBy(label => label.Name).Select(label => new LabelResponse(label.Id, label.Name, label.Colour)).ToListAsync(token));
        });
        admin.MapPost("/labels", async (LabelInput input, CatalysticDbContext db, IAntiforgery csrf, HttpContext context, CancellationToken token) =>
        {
            if (!await ValidCsrf(context, csrf) || !ValidLabel(input)) return Results.BadRequest();
            var label = new BlogLabel { Name = input.Name!.Trim(), Colour = input.Colour!.ToUpperInvariant() };
            db.BlogLabels.Add(label);
            await db.SaveChangesAsync(token);
            return Results.Ok(new LabelResponse(label.Id, label.Name, label.Colour));
        });
        admin.MapPut("/labels/{id:int}", async (int id, LabelInput input, CatalysticDbContext db, IAntiforgery csrf, HttpContext context, CancellationToken token) =>
        {
            if (!await ValidCsrf(context, csrf) || !ValidLabel(input)) return Results.BadRequest();
            var label = await db.BlogLabels.FindAsync([id], token);
            if (label is null) return Results.NotFound();
            label.Name = input.Name!.Trim();
            label.Colour = input.Colour!.ToUpperInvariant();
            await db.SaveChangesAsync(token);
            return Results.Ok(new LabelResponse(label.Id, label.Name, label.Colour));
        });
        admin.MapGet("/", async (BlogService service, HttpContext context, CancellationToken token) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            return Results.Ok(await service.ListAsync(true, token));
        });
        admin.MapPost("/", async (BlogInput input, BlogService service, HttpContext context, IAntiforgery csrf, CancellationToken token) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (!BlogService.IsValid(input)) return Results.BadRequest(new { error = "Check the title, content and UTC publication time." });
            try { return Results.Ok(await service.SaveAsync(null, input, token)); }
            catch (ArgumentException) { return Results.BadRequest(new { error = "One or more labels do not exist." }); }
        });
        admin.MapPut("/{id:int}", async (int id, BlogInput input, BlogService service, HttpContext context, IAntiforgery csrf, CancellationToken token) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (!BlogService.IsValid(input)) return Results.BadRequest(new { error = "Check the title, content and UTC publication time." });
            try
            {
                var post = await service.SaveAsync(id, input, token);
                return post is null ? Results.NotFound() : Results.Ok(post);
            }
            catch (ArgumentException) { return Results.BadRequest(new { error = "One or more labels do not exist." }); }
        });
        admin.MapDelete("/{id:int}", async (int id, BlogService service, HttpContext context, IAntiforgery csrf, CancellationToken token) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            return await service.DeleteAsync(id, token) ? Results.NoContent() : Results.NotFound();
        });
    }

    private static bool ValidLabel(LabelInput input) => input.Name?.Trim().Length is >= 1 and <= 64 &&
        input.Colour is not null && Regex.IsMatch(input.Colour, "^#[0-9a-fA-F]{6}$", RegexOptions.CultureInvariant);

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
