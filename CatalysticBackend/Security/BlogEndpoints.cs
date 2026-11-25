using CatalysticBackend.Service;
using Microsoft.AspNetCore.Antiforgery;

namespace CatalysticBackend.Security;

public static class BlogEndpoints
{
    public static void MapBlogEndpoints(this WebApplication app)
    {
        var publicBlog = app.MapGroup("/api/blog").AllowAnonymous();
        publicBlog.MapGet("/", async (BlogService service, CancellationToken token) =>
            Results.Ok(await service.ListAsync(false, token)));
        publicBlog.MapGet("/{id:int}", async (int id, BlogService service, CancellationToken token) =>
        {
            var post = await service.GetPublicAsync(id, token);
            return post is null ? Results.NotFound() : Results.Ok(post);
        });

        var admin = app.MapGroup("/api/admin/blog").RequireAuthorization("Admin");
        admin.MapGet("/", async (BlogService service, HttpContext context, CancellationToken token) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            return Results.Ok(await service.ListAsync(true, token));
        });
        admin.MapPost("/", async (BlogInput input, BlogService service, HttpContext context, IAntiforgery csrf, CancellationToken token) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (!BlogService.IsValid(input)) return Results.BadRequest(new { error = "Check the title, content and UTC publication time." });
            return Results.Ok(await service.SaveAsync(null, input, token));
        });
        admin.MapPut("/{id:int}", async (int id, BlogInput input, BlogService service, HttpContext context, IAntiforgery csrf, CancellationToken token) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            if (!BlogService.IsValid(input)) return Results.BadRequest(new { error = "Check the title, content and UTC publication time." });
            var post = await service.SaveAsync(id, input, token);
            return post is null ? Results.NotFound() : Results.Ok(post);
        });
        admin.MapDelete("/{id:int}", async (int id, BlogService service, HttpContext context, IAntiforgery csrf, CancellationToken token) =>
        {
            if (!await ValidCsrf(context, csrf)) return Results.BadRequest();
            return await service.DeleteAsync(id, token) ? Results.NoContent() : Results.NotFound();
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
