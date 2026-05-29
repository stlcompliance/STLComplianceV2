using ComplianceCore.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class FactSourceSyncRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(null, true)]
    [InlineData(-61, true)]
    [InlineData(-59, false)]
    [InlineData(0, false)]
    public void IsDue_uses_interval_minutes_from_last_attempt(int? minutesSinceLastAttempt, bool expected)
    {
        DateTimeOffset? lastAttempt = minutesSinceLastAttempt.HasValue
            ? AsOf.AddMinutes(minutesSinceLastAttempt.Value)
            : null;

        Assert.Equal(
            expected,
            FactSourceSyncRules.IsDue(lastAttempt, intervalMinutes: 60, AsOf));
    }

    [Theory]
    [InlineData(null, 60)]
    [InlineData(0, 5)]
    [InlineData(5, 5)]
    [InlineData(5000, 1440)]
    public void NormalizeIntervalMinutes_clamps_to_allowed_range(int? input, int expected) =>
        Assert.Equal(expected, FactSourceSyncRules.NormalizeIntervalMinutes(input));

    [Theory]
    [InlineData(null, 50)]
    [InlineData(0, 1)]
    [InlineData(500, 200)]
    public void NormalizeBatchSize_clamps_to_allowed_range(int? input, int expected) =>
        Assert.Equal(expected, FactSourceSyncRules.NormalizeBatchSize(input));

    [Theory]
    [InlineData(null, "tenant")]
    [InlineData("", "tenant")]
    [InlineData("  Site-A  ", "site-a")]
    public void NormalizeScopeKey_trims_and_lowercases(string? input, string expected) =>
        Assert.Equal(expected, FactSourceSyncRules.NormalizeScopeKey(input));
}
