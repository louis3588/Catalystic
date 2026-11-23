using CatalysticBackend;
using Xunit;

namespace CatalysticBackend.Tests;

public class ModelResultTests
{
    [Fact]
    public void Success_returns_value_without_error()
    {
        var result = ModelResult<string>.Success("profile");

        Assert.True(result.IsSuccess);
        Assert.Equal("profile", result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Failure_returns_error_without_value()
    {
        var result = ModelResult<string>.Failure("Not found");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("Not found", result.ErrorMessage);
    }
}
