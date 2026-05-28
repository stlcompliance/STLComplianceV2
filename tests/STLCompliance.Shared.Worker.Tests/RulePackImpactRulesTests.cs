using TrainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class RulePackImpactRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsStale_treats_missing_computed_at_as_stale()
    {
        Assert.True(RulePackImpactRules.IsStale(null, AsOf, 24));
    }

    [Fact]
    public void IsStale_respects_staleness_boundary()
    {
        Assert.False(RulePackImpactRules.IsStale(AsOf.AddHours(-1), AsOf, 24));
        Assert.True(RulePackImpactRules.IsStale(AsOf.AddHours(-25), AsOf, 24));
    }

    [Theory]
    [InlineData(true, false, false, true)]
    [InlineData(true, true, false, false)]
    [InlineData(true, false, true, false)]
    [InlineData(false, false, false, false)]
    public void ShouldAutoUpdateBaselines_requires_enabled_clean_assessment(
        bool enabled,
        bool requiresAttention,
        bool packNotFound,
        bool expected)
    {
        Assert.Equal(
            expected,
            RulePackImpactRules.ShouldAutoUpdateBaselines(enabled, requiresAttention, packNotFound));
    }
}
