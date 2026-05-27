using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class BulkDispatchRulesTests
{
    [Fact]
    public void Preview_status_transition_requires_driver_for_assigned()
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            DispatchStatus = TripDispatchStatuses.Planned,
            AssignedDriverPersonId = null,
        };

        var (canTransition, errorCode, _) = BulkDispatchRules.PreviewStatusTransition(
            trip,
            TripDispatchStatuses.Assigned,
            canManageAny: true,
            effectiveDriverPersonId: null);

        Assert.False(canTransition);
        Assert.Equal("trip.driver_required", errorCode);
    }

    [Fact]
    public void Preview_status_transition_allows_assigned_when_driver_present()
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            DispatchStatus = TripDispatchStatuses.Planned,
            AssignedDriverPersonId = "driver-1",
        };

        var (canTransition, errorCode, _) = BulkDispatchRules.PreviewStatusTransition(
            trip,
            TripDispatchStatuses.Assigned,
            canManageAny: true,
            effectiveDriverPersonId: "driver-1");

        Assert.True(canTransition);
        Assert.Null(errorCode);
    }

    [Fact]
    public void Preview_status_transition_blocks_cancel_without_manage()
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            DispatchStatus = TripDispatchStatuses.Planned,
            AssignedDriverPersonId = "driver-1",
        };

        var (canTransition, errorCode, _) = BulkDispatchRules.PreviewStatusTransition(
            trip,
            TripDispatchStatuses.Cancelled,
            canManageAny: false,
            effectiveDriverPersonId: "driver-1");

        Assert.False(canTransition);
        Assert.Equal("auth.forbidden", errorCode);
    }

    [Fact]
    public void Apply_simulation_updates_driver_and_vehicle_for_next_preview()
    {
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            DispatchStatus = TripDispatchStatuses.Planned,
            AssignedDriverPersonId = null,
            VehicleRefKey = null,
        };

        BulkDispatchRules.ApplySimulation(
            new BulkDispatchActionItem(
                trip.Id,
                "driver-1",
                "truck-1",
                TripDispatchStatuses.Assigned),
            trip);

        Assert.Equal("driver-1", trip.AssignedDriverPersonId);
        Assert.Equal("truck-1", trip.VehicleRefKey);
        Assert.Equal(TripDispatchStatuses.Assigned, trip.DispatchStatus);
    }
}
