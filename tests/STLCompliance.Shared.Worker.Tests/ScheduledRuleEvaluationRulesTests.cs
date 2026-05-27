using ComplianceCore.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class ScheduledRuleEvaluationRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("published", true)]
    [InlineData("review", false)]
    [InlineData("draft", false)]
    [InlineData("archived", false)]
    public void IsEligibleForScheduledEvaluation_requires_published_status_with_content(
        string status,
        bool expected)
    {
        Assert.Equal(
            expected,
            ScheduledRuleEvaluationRules.IsEligibleForScheduledEvaluation(
                status,
                """{"schemaVersion":1}"""));
    }

    [Fact]
    public void IsEligibleForScheduledEvaluation_requires_rule_content() =>
        Assert.False(
            ScheduledRuleEvaluationRules.IsEligibleForScheduledEvaluation("published", null));

    [Theory]
    [InlineData(null, true)]
    [InlineData(-25, true)]
    [InlineData(-23, false)]
    [InlineData(0, false)]
    public void IsDue_uses_interval_hours_from_last_run(int? hoursSinceLastRun, bool expected)
    {
        DateTimeOffset? lastRun = hoursSinceLastRun.HasValue
            ? AsOf.AddHours(hoursSinceLastRun.Value)
            : null;

        Assert.Equal(
            expected,
            ScheduledRuleEvaluationRules.IsDue(lastRun, AsOf, intervalHours: 24));
    }

    [Theory]
    [InlineData(null, 24)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(1000, 720)]
    public void NormalizeIntervalHours_clamps_to_allowed_range(int? input, int expected) =>
        Assert.Equal(expected, ScheduledRuleEvaluationRules.NormalizeIntervalHours(input));

    [Theory]
    [InlineData(0, 1)]
    [InlineData(100, 100)]
    [InlineData(1000, 500)]
    public void NormalizeBatchSize_clamps_to_allowed_range(int input, int expected) =>
        Assert.Equal(expected, ScheduledRuleEvaluationRules.NormalizeBatchSize(input));
}
