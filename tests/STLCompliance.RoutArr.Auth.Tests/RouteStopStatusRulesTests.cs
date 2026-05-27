using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RouteStopStatusRulesTests
{
    [Theory]
    [InlineData(RouteStopStatuses.Pending, RouteStopStatuses.Arrived, true)]
    [InlineData(RouteStopStatuses.Pending, RouteStopStatuses.Skipped, true)]
    [InlineData(RouteStopStatuses.Arrived, RouteStopStatuses.Completed, true)]
    [InlineData(RouteStopStatuses.Arrived, RouteStopStatuses.Skipped, true)]
    [InlineData(RouteStopStatuses.Pending, RouteStopStatuses.Completed, false)]
    [InlineData(RouteStopStatuses.Completed, RouteStopStatuses.Pending, false)]
    public void CanTransition_respects_stop_lifecycle(string from, string to, bool expected)
    {
        Assert.Equal(expected, RouteStopStatusRules.CanTransition(from, to));
    }
}
