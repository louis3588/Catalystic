using CatalysticBackend.Domain;
using Xunit;

namespace CatalysticBackend.Tests;

public class WellnessScoresTests
{
    [Fact]
    public void Computes_total_and_mean_from_seven_pillars_and_independence()
    {
        var scores = new WellnessScores(6, 5, 4, 7, 8, 3, 5, 6);

        Assert.Equal(44m, scores.Total);
        Assert.Equal(5.5m, scores.Mean);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Rejects_out_of_range_scores(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new WellnessScores(value, 5, 5, 5, 5, 5, 5, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => new WellnessScores(5, 5, 5, 5, 5, 5, 5, value));
    }

    [Fact]
    public void Accepts_range_boundaries_and_fractional_scores()
    {
        var scores = new WellnessScores(0, 10, 5.5m, 5, 5, 5, 5, 5);

        Assert.Equal(40.5m, scores.Total);
    }
}
