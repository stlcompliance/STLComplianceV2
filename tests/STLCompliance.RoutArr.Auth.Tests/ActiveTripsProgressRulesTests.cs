using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class ActiveTripsProgressRulesTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(2, 4, 50)]
    [InlineData(3, 3, 100)]
    public void ComputeStopProgress_calculates_percent(int completed, int total, int expectedPercent)
    {
        var result = ActiveTripsProgressRules.ComputeStopProgress(completed, total);
        Assert.Equal(completed, result.CompletedStopCount);
        Assert.Equal(total, result.TotalStopCount);
        Assert.Equal(expectedPercent, result.StopProgressPercent);
    }

    [Theory]
    [InlineData("completed", true)]
    [InlineData("skipped", true)]
    [InlineData("pending", false)]
    public void IsCompletedStop_recognizes_terminal_stops(string status, bool expected)
    {
        Assert.Equal(expected, ActiveTripsProgressRules.IsCompletedStop(status));
    }
}
