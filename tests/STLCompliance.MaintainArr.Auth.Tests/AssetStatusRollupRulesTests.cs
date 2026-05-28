using MaintainArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class AssetStatusRollupRulesTests
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

    [Fact]
    public void ComputeReadyPercent_rounds_to_one_decimal()
    {
        Assert.Equal(66.7m, AssetStatusRollupRules.ComputeReadyPercent(3, 2));
    }

    [Fact]
    public void AggregateAssetCounts_counts_ready_and_not_ready()
    {
        var snapshots = new List<AssetStatusRollupSnapshot>
        {
            new(Guid.NewGuid(), "ready"),
            new(Guid.NewGuid(), "ready"),
            new(Guid.NewGuid(), "not_ready"),
        };

        var (readyCount, notReadyCount) = AssetStatusRollupRules.AggregateAssetCounts(snapshots);
        Assert.Equal(2, readyCount);
        Assert.Equal(1, notReadyCount);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("  Site-A  ", "Site-A")]
    public void NormalizeSiteKey_trims_and_handles_empty(string? input, string expected)
    {
        Assert.Equal(expected, AssetStatusRollupRules.NormalizeSiteKey(input));
    }
}
