using CatalysticBackend.Data;
using CatalysticBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalysticBackend.Service;





public sealed class BlogService(CatalysticDbContext db, TimeProvider clock)
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<List<BlogResponse>> ListAsync(bool admin, CancellationToken cancellationToken, int? labelId = null)
    {
        var now = Now;
        var query = db.BlogPosts.AsNoTracking();
        if (!admin) query = query.Where(post => post.IsPublished && post.ReleaseTime <= now);
        if (labelId.HasValue) query = query.Where(post => post.Labels.Any(label => label.Id == labelId));
        return await query.OrderByDescending(post => post.ReleaseTime).ThenByDescending(post => post.Id)
            .Select(post => new BlogResponse(post.Id, post.Title, post.Content, post.IsPublished, post.ReleaseTime, post.CreatedAt, post.UpdatedAt, post.Labels.OrderBy(label => label.Name).Select(label => new LabelResponse(label.Id, label.Name, label.Colour)).ToList()))
            .ToListAsync(cancellationToken);
    }

    public async Task<BlogResponse?> GetPublicAsync(int id, CancellationToken cancellationToken)
    {
        var now = Now;
        return await db.BlogPosts.AsNoTracking().Where(post => post.Id == id && post.IsPublished && post.ReleaseTime <= now)
            .Select(post => new BlogResponse(post.Id, post.Title, post.Content, post.IsPublished, post.ReleaseTime, post.CreatedAt, post.UpdatedAt, post.Labels.OrderBy(label => label.Name).Select(label => new LabelResponse(label.Id, label.Name, label.Colour)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public static bool IsValid(BlogInput input) =>
        input.Title is not null && input.Title.Trim().Length is >= 1 and <= 128 &&
        input.Content is not null && input.Content.Trim().Length is >= 1 and <= 8192 &&
        (!input.ReleaseTime.HasValue || input.ReleaseTime.Value.Kind == DateTimeKind.Utc);

    public async Task<BlogResponse?> SaveAsync(int? id, BlogInput input, CancellationToken cancellationToken)
    {
        var post = id.HasValue ? await db.BlogPosts.Include(item => item.Labels).SingleOrDefaultAsync(item => item.Id == id.Value, cancellationToken) : new BlogPost { CreatedAt = Now };
        if (post is null) return null;
        if (input.LabelIds is not null)
        {
            var ids = input.LabelIds.Distinct().ToArray();
            var labels = await db.BlogLabels.Where(label => ids.Contains(label.Id)).ToListAsync(cancellationToken);
            if (labels.Count != ids.Length) throw new ArgumentException("One or more labels do not exist.");
            post.Labels.Clear();
            foreach (var label in labels) post.Labels.Add(label);
        }
        var now = Now;
        post.Title = input.Title!.Trim();
        post.Content = input.Content!.Trim();
        post.IsPublished = input.IsPublished;
        post.ReleaseTime = input.ReleaseTime ?? now;
        post.UpdatedAt = now;
        if (!id.HasValue) db.BlogPosts.Add(post);
        await db.SaveChangesAsync(cancellationToken);
        return new BlogResponse(post.Id, post.Title, post.Content, post.IsPublished, post.ReleaseTime, post.CreatedAt, post.UpdatedAt, post.Labels.OrderBy(label => label.Name).Select(label => new LabelResponse(label.Id, label.Name, label.Colour)).ToList());
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
