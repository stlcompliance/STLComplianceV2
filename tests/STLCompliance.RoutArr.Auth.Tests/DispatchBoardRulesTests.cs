using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DispatchBoardRulesTests
{
    [Fact]
    public void Late_trip_when_scheduled_start_passed_and_not_started()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = new Trip
        {
            DispatchStatus = TripDispatchStatuses.Planned,
            ScheduledStartAt = now.AddHours(-1),
        };

        Assert.True(DispatchBoardRules.IsLateTrip(trip, now));
    }

    [Fact]
    public void At_risk_trip_when_end_within_two_hours()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = new Trip
        {
            DispatchStatus = TripDispatchStatuses.InProgress,
            ScheduledEndAt = now.AddMinutes(90),
        };

        Assert.True(DispatchBoardRules.IsAtRiskTrip(trip, now));
    }

    [Fact]
    public void Completed_trip_is_neither_late_nor_at_risk()
    {
        var now = DateTimeOffset.UtcNow;
        var trip = new Trip
        {
            DispatchStatus = TripDispatchStatuses.Completed,
            ScheduledStartAt = now.AddHours(-5),
            ScheduledEndAt = now.AddHours(-1),
        };

        Assert.False(DispatchBoardRules.IsLateTrip(trip, now));
        Assert.False(DispatchBoardRules.IsAtRiskTrip(trip, now));
    }
}
