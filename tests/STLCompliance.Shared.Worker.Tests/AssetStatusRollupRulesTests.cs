using MaintainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class AssetStatusRollupRulesTests
{
    [Fact]
    public void IsStale_returns_true_when_never_computed()
    {
        var asOf = DateTimeOffset.UtcNow;
        Assert.True(AssetStatusRollupRules.IsStale(null, asOf, 1));
    }

    [Fact]
    public void IsStale_returns_false_when_within_window()
    {
        var asOf = DateTimeOffset.UtcNow;
        var computedAt = asOf.AddMinutes(-30);
        Assert.False(AssetStatusRollupRules.IsStale(computedAt, asOf, 1));
    }

    [Theory]
    [InlineData(null, 50)]
    [InlineData(0, 1)]
    [InlineData(1000, 500)]
    public void NormalizeBatchSize_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, AssetStatusRollupRules.NormalizeBatchSize(input));

    [Theory]
    [InlineData(null, 1)]
    [InlineData(0, 1)]
    [InlineData(200, 168)]
    public void NormalizeStalenessHours_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, AssetStatusRollupRules.NormalizeStalenessHours(input));

    [Fact]
    public void ComputeReadyPercent_rounds_to_one_decimal()
    {
        Assert.Equal(66.7m, AssetStatusRollupRules.ComputeReadyPercent(3, 2));
    }
}
