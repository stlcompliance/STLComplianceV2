using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DispatchAssignmentRulesTests
{
    [Fact]
    public void FindBlockingDriverAvailability_flags_unavailable_overlap()
    {
        var personId = Guid.NewGuid().ToString();
        var tripStart = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero);
        var tripEnd = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero);

        var records = new[]
        {
            new DriverAvailability
            {
                PersonId = personId,
                AvailabilityStatus = DriverAvailabilityStatuses.Unavailable,
                StartsAt = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero),
                EndsAt = new DateTimeOffset(2026, 5, 27, 18, 0, 0, TimeSpan.Zero),
            },
            new DriverAvailability
            {
                PersonId = personId,
                AvailabilityStatus = DriverAvailabilityStatuses.Available,
                StartsAt = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero),
                EndsAt = new DateTimeOffset(2026, 5, 27, 18, 0, 0, TimeSpan.Zero),
            },
        };

        var blocking = DispatchAssignmentRules
            .FindBlockingDriverAvailability(personId, tripStart, tripEnd, records)
            .ToList();

        Assert.Single(blocking);
        Assert.Equal(DriverAvailabilityStatuses.Unavailable, blocking[0].AvailabilityStatus);
    }

    [Fact]
    public void FindOverlappingDriverTrips_excludes_target_trip()
    {
        var personId = Guid.NewGuid().ToString();
        var targetTripId = Guid.NewGuid();
        var target = new Trip
        {
            Id = targetTripId,
            AssignedDriverPersonId = personId,
            DispatchStatus = TripDispatchStatuses.Planned,
            ScheduledStartAt = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero),
            ScheduledEndAt = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero),
        };

        var other = new Trip
        {
            Id = Guid.NewGuid(),
            AssignedDriverPersonId = personId,
            DispatchStatus = TripDispatchStatuses.Assigned,
            ScheduledStartAt = new DateTimeOffset(2026, 5, 27, 10, 0, 0, TimeSpan.Zero),
            ScheduledEndAt = new DateTimeOffset(2026, 5, 27, 12, 0, 0, TimeSpan.Zero),
        };

        var overlaps = DispatchAssignmentRules
            .FindOverlappingDriverTrips(target, personId, [target, other])
            .ToList();

        Assert.Single(overlaps);
        Assert.Equal(other.Id, overlaps[0].Id);
    }

    [Fact]
    public void FindBlockingEquipmentAvailability_flags_limited_overlap()
    {
        var vehicleKey = "vehicle-42";
        var tripStart = new DateTimeOffset(2026, 5, 27, 9, 0, 0, TimeSpan.Zero);
        var tripEnd = new DateTimeOffset(2026, 5, 27, 11, 0, 0, TimeSpan.Zero);

        var records = new[]
        {
            new EquipmentAvailability
            {
                VehicleRefKey = vehicleKey,
                AvailabilityStatus = EquipmentAvailabilityStatuses.Limited,
                StartsAt = new DateTimeOffset(2026, 5, 27, 8, 0, 0, TimeSpan.Zero),
                EndsAt = new DateTimeOffset(2026, 5, 27, 18, 0, 0, TimeSpan.Zero),
            },
        };

        var blocking = DispatchAssignmentRules
            .FindBlockingEquipmentAvailability(vehicleKey, tripStart, tripEnd, records)
            .ToList();

        Assert.Single(blocking);
    }
}
