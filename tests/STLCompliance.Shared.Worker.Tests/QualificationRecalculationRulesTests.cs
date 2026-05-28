using TrainArr.Api.Contracts;
using TrainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class QualificationRecalculationRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(24, 24)]
    [InlineData(500, 168)]
    [InlineData(null, 24)]
    public void NormalizeStalenessHours_clamps(int? input, int expected) =>
        Assert.Equal(expected, QualificationRecalculationRules.NormalizeStalenessHours(input));

    [Fact]
    public void IsStale_treats_missing_computed_at_as_stale()
    {
        Assert.True(QualificationRecalculationRules.IsStale(null, AsOf, 24));
    }

    [Fact]
    public void IsStale_respects_staleness_boundary()
    {
        Assert.False(QualificationRecalculationRules.IsStale(AsOf.AddHours(-1), AsOf, 24));
        Assert.True(QualificationRecalculationRules.IsStale(AsOf.AddHours(-25), AsOf, 24));
    }

    [Theory]
    [InlineData(true, "issued", QualificationCheckOutcomes.Block, QualificationCheckOutcomes.Block, true)]
    [InlineData(true, "issued", QualificationCheckOutcomes.Block, QualificationCheckOutcomes.Warn, false)]
    [InlineData(true, "suspended", QualificationCheckOutcomes.Block, QualificationCheckOutcomes.Block, false)]
    [InlineData(false, "issued", QualificationCheckOutcomes.Block, QualificationCheckOutcomes.Block, false)]
    public void ShouldAutoSuspend_requires_enabled_issued_compliance_block(
        bool autoSuspendOnBlock,
        string issueStatus,
        string checkOutcome,
        string complianceOutcome,
        bool expected) =>
        Assert.Equal(
            expected,
            QualificationRecalculationRules.ShouldAutoSuspend(
                autoSuspendOnBlock,
                issueStatus,
                checkOutcome,
                complianceOutcome));
}
