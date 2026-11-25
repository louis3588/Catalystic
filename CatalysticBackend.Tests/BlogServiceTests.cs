using CatalysticBackend.Data;
using CatalysticBackend.Service;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalysticBackend.Tests;

public class BlogServiceTests
{
    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Current { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Current;
    }

    private static CatalysticDbContext Database() => new(new DbContextOptionsBuilder<CatalysticDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Theory]
    [InlineData("", "content")]
    [InlineData("title", " ")]
    public void Rejects_empty_title_or_content(string title, string content)
    {
        Assert.False(BlogService.IsValid(new BlogInput(title, content, true, null)));
    }

    [Fact]
    public void Rejects_non_utc_publication_times()
    {
        Assert.False(BlogService.IsValid(new BlogInput("Title", "Content", true, new DateTime(2030, 1, 1))));
    }

    [Fact]
    public async Task Public_can_only_see_published_posts_that_are_due()
    {
        await using var db = Database();
        var service = new BlogService(db, TimeProvider.System);
        var now = DateTime.UtcNow;
        var published = await service.SaveAsync(null, new BlogInput("Published", "Text", true, now.AddMinutes(-1)), default);
        var draft = await service.SaveAsync(null, new BlogInput("Draft", "Text", false, now.AddMinutes(-1)), default);
        var scheduled = await service.SaveAsync(null, new BlogInput("Scheduled", "Text", true, now.AddHours(1)), default);

        Assert.Single(await service.ListAsync(false, default));
        Assert.Equal(3, (await service.ListAsync(true, default)).Count);
        Assert.NotNull(await service.GetPublicAsync(published!.Id, default));
        Assert.Null(await service.GetPublicAsync(draft!.Id, default));
        Assert.Null(await service.GetPublicAsync(scheduled!.Id, default));

        await service.SaveAsync(scheduled.Id, new BlogInput("Updated", "Text", true, now.AddMinutes(-1)), default);
        Assert.Equal("Updated", (await service.GetPublicAsync(scheduled.Id, default))!.Title);
        Assert.True(await service.DeleteAsync(scheduled.Id, default));
        Assert.False(await service.DeleteAsync(scheduled.Id, default));
    }

    [Fact]
    public async Task Scheduled_post_becomes_visible_at_release_time_without_a_job()
    {
        await using var db = Database();
        var clock = new TestClock(new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var service = new BlogService(db, clock);
        var release = clock.Current.AddHours(1);
        var post = await service.SaveAsync(null, new BlogInput("Scheduled", "Content", true, release.UtcDateTime), default);
        Assert.Null(await service.GetPublicAsync(post!.Id, default));
        clock.Current = release;
        Assert.NotNull(await service.GetPublicAsync(post.Id, default));
    }
}
