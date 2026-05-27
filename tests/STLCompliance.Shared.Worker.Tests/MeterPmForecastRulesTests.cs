using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class MeterPmForecastRulesTests
{
    [Theory]
    [InlineData("active", "scheduled", 1000, 900, true)]
    [InlineData("active", "scheduled", 899, 900, false)]
    [InlineData("paused", "scheduled", 1000, 900, false)]
    [InlineData("active", "due", 1000, 900, false)]
    public void ShouldMarkDueFromUsage_respects_threshold_and_status(
        string scheduleStatus,
        string dueStatus,
        decimal currentReading,
        decimal nextDueAtUsage,
        bool expected)
    {
        Assert.Equal(
            expected,
            MeterPmForecastRules.ShouldMarkDueFromUsage(
                scheduleStatus,
                dueStatus,
                currentReading,
                nextDueAtUsage));
    }

    [Fact]
    public void ComputeUsageUntilDue_returns_remaining_usage()
    {
        Assert.Equal(50m, MeterPmForecastRules.ComputeUsageUntilDue(850, 900));
        Assert.Equal(0m, MeterPmForecastRules.ComputeUsageUntilDue(950, 900));
        Assert.Null(MeterPmForecastRules.ComputeUsageUntilDue(100, null));
    }

    [Fact]
    public void ComputeInitialNextDueAtUsage_adds_interval_to_baseline()
    {
        Assert.Equal(1500m, MeterPmForecastRules.ComputeInitialNextDueAtUsage(1000, 500));
    }
}
