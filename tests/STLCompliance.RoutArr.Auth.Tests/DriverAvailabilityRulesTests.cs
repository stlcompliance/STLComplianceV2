using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DriverAvailabilityRulesTests
{
    [Fact]
    public void Overlaps_detects_partial_overlap()
    {
        var rangeStart = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero);
        var rangeEnd = new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);
        var tripStart = new DateTimeOffset(2026, 5, 27, 10, 0, 0, TimeSpan.Zero);
        var tripEnd = new DateTimeOffset(2026, 5, 27, 14, 0, 0, TimeSpan.Zero);

        Assert.True(DriverAvailabilityRules.Overlaps(rangeStart, rangeEnd, tripStart, tripEnd));
    }

    [Fact]
    public void Overlaps_returns_false_when_ranges_are_disjoint()
    {
        var rangeStart = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero);
        var rangeEnd = new DateTimeOffset(2026, 5, 27, 10, 0, 0, TimeSpan.Zero);
        var tripStart = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero);
        var tripEnd = new DateTimeOffset(2026, 5, 27, 13, 0, 0, TimeSpan.Zero);

        Assert.False(DriverAvailabilityRules.Overlaps(rangeStart, rangeEnd, tripStart, tripEnd));
    }

    [Fact]
    public void FindConflictingTrips_flags_unavailable_overlap_with_assigned_active_trip()
    {
        var personId = Guid.NewGuid().ToString();
        var record = new DriverAvailability
        {
            PersonId = personId,
            AvailabilityStatus = DriverAvailabilityStatuses.Unavailable,
            StartsAt = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero),
            EndsAt = new DateTimeOffset(2026, 5, 27, 18, 0, 0, TimeSpan.Zero),
        };

        var trips = new[]
        {
            new Trip
            {
                AssignedDriverPersonId = personId,
                DispatchStatus = TripDispatchStatuses.Assigned,
                ScheduledStartAt = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero),
                ScheduledEndAt = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero),
            },
            new Trip
            {
                AssignedDriverPersonId = personId,
                DispatchStatus = TripDispatchStatuses.Completed,
                ScheduledStartAt = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero),
                ScheduledEndAt = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero),
            },
        };

        var conflicts = DriverAvailabilityRules.FindConflictingTrips(record, trips).ToList();

        Assert.Single(conflicts);
        Assert.Equal(TripDispatchStatuses.Assigned, conflicts[0].DispatchStatus);
    }

    [Fact]
    public void FindConflictingTrips_ignores_available_status()
    {
        var record = new DriverAvailability
        {
            PersonId = Guid.NewGuid().ToString(),
            AvailabilityStatus = DriverAvailabilityStatuses.Available,
            StartsAt = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero),
            EndsAt = new DateTimeOffset(2026, 5, 27, 18, 0, 0, TimeSpan.Zero),
        };

        var trips = new[]
        {
            new Trip
            {
                AssignedDriverPersonId = record.PersonId,
                DispatchStatus = TripDispatchStatuses.Assigned,
                ScheduledStartAt = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero),
                ScheduledEndAt = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero),
            },
        };

        Assert.Empty(DriverAvailabilityRules.FindConflictingTrips(record, trips));
    }
}
