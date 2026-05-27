using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class EquipmentAvailabilityRulesTests
{
    [Fact]
    public void Overlaps_detects_partial_overlap()
    {
        var rangeStart = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero);
        var rangeEnd = new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);
        var tripStart = new DateTimeOffset(2026, 5, 27, 10, 0, 0, TimeSpan.Zero);
        var tripEnd = new DateTimeOffset(2026, 5, 27, 14, 0, 0, TimeSpan.Zero);

        Assert.True(EquipmentAvailabilityRules.Overlaps(rangeStart, rangeEnd, tripStart, tripEnd));
    }

    [Fact]
    public void Overlaps_returns_false_when_ranges_are_disjoint()
    {
        var rangeStart = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero);
        var rangeEnd = new DateTimeOffset(2026, 5, 27, 10, 0, 0, TimeSpan.Zero);
        var tripStart = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero);
        var tripEnd = new DateTimeOffset(2026, 5, 27, 13, 0, 0, TimeSpan.Zero);

        Assert.False(EquipmentAvailabilityRules.Overlaps(rangeStart, rangeEnd, tripStart, tripEnd));
    }

    [Fact]
    public void FindConflictingTrips_flags_unavailable_overlap_with_assigned_active_trip()
    {
        const string vehicleRefKey = "truck-42";
        var record = new EquipmentAvailability
        {
            VehicleRefKey = vehicleRefKey,
            AvailabilityStatus = EquipmentAvailabilityStatuses.Unavailable,
            StartsAt = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero),
            EndsAt = new DateTimeOffset(2026, 5, 27, 18, 0, 0, TimeSpan.Zero),
        };

        var trips = new[]
        {
            new Trip
            {
                VehicleRefKey = vehicleRefKey,
                DispatchStatus = TripDispatchStatuses.Assigned,
                ScheduledStartAt = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero),
                ScheduledEndAt = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero),
            },
            new Trip
            {
                VehicleRefKey = vehicleRefKey,
                DispatchStatus = TripDispatchStatuses.Completed,
                ScheduledStartAt = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero),
                ScheduledEndAt = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero),
            },
            new Trip
            {
                VehicleRefKey = "other-truck",
                DispatchStatus = TripDispatchStatuses.Assigned,
                ScheduledStartAt = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero),
                ScheduledEndAt = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero),
            },
        };

        var conflicts = EquipmentAvailabilityRules.FindConflictingTrips(record, trips).ToList();

        Assert.Single(conflicts);
        Assert.Equal(TripDispatchStatuses.Assigned, conflicts[0].DispatchStatus);
    }

    [Fact]
    public void FindConflictingTrips_ignores_available_status()
    {
        const string vehicleRefKey = "truck-42";
        var record = new EquipmentAvailability
        {
            VehicleRefKey = vehicleRefKey,
            AvailabilityStatus = EquipmentAvailabilityStatuses.Available,
            StartsAt = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero),
            EndsAt = new DateTimeOffset(2026, 5, 27, 18, 0, 0, TimeSpan.Zero),
        };

        var trips = new[]
        {
            new Trip
            {
                VehicleRefKey = vehicleRefKey,
                DispatchStatus = TripDispatchStatuses.Assigned,
                ScheduledStartAt = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero),
                ScheduledEndAt = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero),
            },
        };

        Assert.Empty(EquipmentAvailabilityRules.FindConflictingTrips(record, trips));
    }
}
