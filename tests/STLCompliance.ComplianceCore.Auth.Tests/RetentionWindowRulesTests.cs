using ComplianceCore.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class RetentionWindowRulesTests
{
    [Fact]
    public void TryParseRetentionDays_supports_common_duration_phrases()
    {
        Assert.Equal(365, RetentionWindowRules.TryParseRetentionDays("1 year"));
        Assert.Equal(25, RetentionWindowRules.TryParseRetentionDays("25 days"));
        Assert.Equal(14, RetentionWindowRules.TryParseRetentionDays("2 weeks"));
        Assert.Equal(90, RetentionWindowRules.TryParseRetentionDays("3 months"));
    }

    [Fact]
    public void EvaluateCurrent_returns_due_soon_information_for_near_expiry_values()
    {
        var now = new DateTimeOffset(2026, 6, 18, 0, 0, 0, TimeSpan.Zero);
        var result = RetentionWindowRules.EvaluateCurrent(
            assertedAt: now.AddDays(-340),
            effectiveAt: null,
            expiresAt: null,
            retentionPeriod: "1 year",
            value: string.Empty,
            now: now);

        Assert.True(result.Passed);
        Assert.True(result.IsDueSoon);
        Assert.Equal(25, result.DaysRemaining);
        Assert.Equal("current (due in 25 days)", result.EvaluatedValue);
    }

    [Fact]
    public void EvaluateCurrent_marks_expired_values_as_failed()
    {
        var now = new DateTimeOffset(2026, 6, 18, 0, 0, 0, TimeSpan.Zero);
        var result = RetentionWindowRules.EvaluateCurrent(
            assertedAt: now.AddDays(-366),
            effectiveAt: null,
            expiresAt: null,
            retentionPeriod: "1 year",
            value: string.Empty,
            now: now);

        Assert.False(result.Passed);
        Assert.False(result.IsDueSoon);
        Assert.Equal(-1, result.DaysRemaining);
        Assert.Equal("expired 1 day ago", result.EvaluatedValue);
    }
}
