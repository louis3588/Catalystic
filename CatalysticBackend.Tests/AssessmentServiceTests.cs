using CatalysticBackend.Data;
using CatalysticBackend.Models;
using CatalysticBackend.Service;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CatalysticBackend.Tests;

public class AssessmentServiceTests
{
    private static AssessmentInput Input(DateOnly date) => new(date, 6, 5, 4, 7, 8, 3, 5, 6);

    [Fact]
    public void Requires_all_scores_and_valid_precision()
    {
        var input = Input(new DateOnly(2030, 1, 1));
        Assert.True(AssessmentService.IsValid(input));
        Assert.False(AssessmentService.IsValid(input with { PREOS = null }));
        Assert.False(AssessmentService.IsValid(input with { Physical = -1 }));
        Assert.False(AssessmentService.IsValid(input with { PREOS = 10.01m }));
        Assert.False(AssessmentService.IsValid(input with { Dietary = 2.123m }));
        Assert.False(AssessmentService.IsValid(input with { AssessmentDate = null }));
    }

    [Fact]
    public async Task Records_dated_scores_only_for_existing_clients_and_rejects_duplicates()
    {
        var options = new DbContextOptionsBuilder<CatalysticDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new CatalysticDbContext(options);
        db.Users.Add(new User { Id = 1, Role = "Client" });
        await db.SaveChangesAsync();
        var service = new AssessmentService(db, TimeProvider.System);
        var input = Input(new DateOnly(2030, 1, 1));
        Assert.Equal("NotFound", await service.CreateAsync(2, input, default));
        Assert.Null(await service.CreateAsync(1, input, default));
        Assert.Equal("Duplicate", await service.CreateAsync(1, input, default));
        Assert.Single(await service.ListAsync(1, default));
        Assert.Empty(await service.ListAsync(2, default));
        Assert.Equal(6m, (await db.WellnessAssessments.SingleAsync()).PREOS);
    }
}
