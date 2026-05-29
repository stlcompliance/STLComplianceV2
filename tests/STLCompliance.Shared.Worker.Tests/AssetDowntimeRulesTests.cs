using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public sealed class AssetDowntimeRulesTests
{
    [Theory]
    [InlineData("out_of_service", "ready", true, true, true)]
    [InlineData("active", "not_ready", true, true, true)]
    [InlineData("active", "ready", true, true, false)]
    [InlineData("out_of_service", "not_ready", true, false, true)]
    [InlineData("active", "not_ready", true, false, false)]
    public void IsAutomaticDowntimeState_respects_tracking_flags(
        string lifecycleStatus,
        string readinessStatus,
        bool trackOos,
        bool trackNotReady,
        bool expected)
    {
        var actual = AssetDowntimeRules.IsAutomaticDowntimeState(
            lifecycleStatus,
            readinessStatus,
            trackOos,
            trackNotReady);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null, 50)]
    [InlineData(0, 1)]
    [InlineData(1000, 500)]
    public void NormalizeBatchSize_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, AssetDowntimeRules.NormalizeBatchSize(input));

    [Theory]
    [InlineData(null, 30)]
    [InlineData(0, 1)]
    [InlineData(500, 365)]
    public void NormalizeAvailabilityPeriodDays_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, AssetDowntimeRules.NormalizeAvailabilityPeriodDays(input));

    [Theory]
    [InlineData(null, 10)]
    [InlineData(0, 1)]
    [InlineData(200, 100)]
    public void NormalizeRunListLimit_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, AssetDowntimeRules.NormalizeRunListLimit(input));

    [Theory]
    [InlineData(AssetDowntimeReasons.InRepair, true)]
    [InlineData(AssetDowntimeReasons.OutOfService, false)]
    [InlineData(AssetDowntimeReasons.RestrictedUse, false)]
    public void IsManualReason_distinguishes_manual_from_automatic_reasons(string reason, bool expected) =>
        Assert.Equal(expected, AssetDowntimeRules.IsManualReason(reason));

    [Fact]
    public void ComputeAvailabilityPercent_returns_zero_when_fully_down()
    {
        var percent = AssetDowntimeRules.ComputeAvailabilityPercent(100m, 100m);
        Assert.Equal(0m, percent);
    }

    [Fact]
    public void ComputeAvailabilityPercent_returns_full_availability_when_no_downtime()
    {
        Assert.Equal(100m, AssetDowntimeRules.ComputeAvailabilityPercent(100m, 0m));
    }

    [Fact]
    public void ComputeDowntimeHoursForPeriod_clips_to_window()
    {
        var periodStart = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = periodStart.AddDays(1);
        var events = new[]
        {
            new DowntimeInterval(periodStart.AddHours(-2), periodStart.AddHours(4), false),
            new DowntimeInterval(periodStart.AddHours(20), null, false),
        };

        var hours = AssetDowntimeRules.ComputeDowntimeHoursForPeriod(events, periodStart, periodEnd);
        Assert.Equal(8m, hours);
    }

    [Fact]
    public void SplitPlannedDowntimeHours_separates_planned_and_unplanned_intervals()
    {
        var periodStart = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = periodStart.AddDays(1);
        var events = new[]
        {
            new DowntimeInterval(periodStart, periodStart.AddHours(4), true),
            new DowntimeInterval(periodStart.AddHours(8), periodStart.AddHours(10), false),
        };

        var (planned, unplanned) = AssetDowntimeRules.SplitPlannedDowntimeHours(events, periodStart, periodEnd);
        Assert.Equal(4m, planned);
        Assert.Equal(2m, unplanned);
    }

    [Fact]
    public void ResolveAutomaticReason_prefers_out_of_service()
    {
        var reason = AssetDowntimeRules.ResolveAutomaticReason("out_of_service", "not_ready");
        Assert.Equal(AssetDowntimeReasons.OutOfService, reason);
    }

    [Fact]
    public void ResolveAutomaticReason_uses_restricted_use_for_not_ready_assets()
    {
        var reason = AssetDowntimeRules.ResolveAutomaticReason("active", "not_ready");
        Assert.Equal(AssetDowntimeReasons.RestrictedUse, reason);
    }

    [Theory]
    [InlineData("out_of_service", "ready", "lifecycle:out_of_service")]
    [InlineData("active", "not_ready", "readiness:not_ready")]
    public void ResolveAutomaticStatusTrigger_reflects_driving_status(
        string lifecycleStatus,
        string readinessStatus,
        string expected)
    {
        var trigger = AssetDowntimeRules.ResolveAutomaticStatusTrigger(lifecycleStatus, readinessStatus);
        Assert.Equal(expected, trigger);
    }
}
