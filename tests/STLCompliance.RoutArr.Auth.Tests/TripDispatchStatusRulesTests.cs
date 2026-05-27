using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class TripDispatchStatusRulesTests
{
    [Theory]
    [InlineData(TripDispatchStatuses.Planned, TripDispatchStatuses.Assigned, true)]
    [InlineData(TripDispatchStatuses.Planned, TripDispatchStatuses.Cancelled, true)]
    [InlineData(TripDispatchStatuses.Assigned, TripDispatchStatuses.Dispatched, true)]
    [InlineData(TripDispatchStatuses.Dispatched, TripDispatchStatuses.InProgress, true)]
    [InlineData(TripDispatchStatuses.InProgress, TripDispatchStatuses.Completed, true)]
    [InlineData(TripDispatchStatuses.Planned, TripDispatchStatuses.Completed, false)]
    [InlineData(TripDispatchStatuses.Completed, TripDispatchStatuses.InProgress, false)]
    public void CanTransition_respects_dispatch_lifecycle(string from, string to, bool expected)
    {
        Assert.Equal(expected, TripDispatchStatusRules.CanTransition(from, to));
    }
}
