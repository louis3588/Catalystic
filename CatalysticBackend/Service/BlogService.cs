using CatalysticBackend.Data;
using CatalysticBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalysticBackend.Service;

public sealed record BlogInput(string? Title, string? Content, bool IsPublished, DateTime? ReleaseTime);
public sealed record BlogResponse(int Id, string Title, string Content, bool IsPublished, DateTime ReleaseTime, DateTime CreatedAt, DateTime UpdatedAt);

public sealed class BlogService(CatalysticDbContext db, TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<List<BlogResponse>> ListAsync(bool admin, CancellationToken cancellationToken)
    {
        var now = Now;
        var query = db.BlogPosts.AsNoTracking();
        if (!admin) query = query.Where(post => post.IsPublished && post.ReleaseTime <= now);
        return await query.OrderByDescending(post => post.ReleaseTime).ThenByDescending(post => post.Id)
            .Select(post => new BlogResponse(post.Id, post.Title, post.Content, post.IsPublished, post.ReleaseTime, post.CreatedAt, post.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<BlogResponse?> GetPublicAsync(int id, CancellationToken cancellationToken)
    {
        var now = Now;
        return await db.BlogPosts.AsNoTracking().Where(post => post.Id == id && post.IsPublished && post.ReleaseTime <= now)
            .Select(post => new BlogResponse(post.Id, post.Title, post.Content, post.IsPublished, post.ReleaseTime, post.CreatedAt, post.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public static bool IsValid(BlogInput input) =>
        input.Title is not null && input.Title.Trim().Length is >= 1 and <= 128 &&
        input.Content is not null && input.Content.Trim().Length is >= 1 and <= 8192 &&
        (!input.ReleaseTime.HasValue || input.ReleaseTime.Value.Kind == DateTimeKind.Utc);

    public async Task<BlogResponse?> SaveAsync(int? id, BlogInput input, CancellationToken cancellationToken)
    {
        var post = id.HasValue ? await db.BlogPosts.FindAsync([id.Value], cancellationToken) : new BlogPost { CreatedAt = Now };
        if (post is null) return null;
        var now = Now;
        post.Title = input.Title!.Trim();
        post.Content = input.Content!.Trim();
        post.IsPublished = input.IsPublished;
        post.ReleaseTime = input.ReleaseTime ?? now;
        post.UpdatedAt = now;
        if (!id.HasValue) db.BlogPosts.Add(post);
        await db.SaveChangesAsync(cancellationToken);
        return new BlogResponse(post.Id, post.Title, post.Content, post.IsPublished, post.ReleaseTime, post.CreatedAt, post.UpdatedAt);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var post = await db.BlogPosts.FindAsync([id], cancellationToken);
        if (post is null) return false;
        db.BlogPosts.Remove(post);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
