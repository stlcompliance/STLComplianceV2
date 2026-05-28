using RoutArr.Api.Entities;
using RoutArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DispatchNotificationRulesTests
{
    [Fact]
    public void MapDispatchStatusToEventKind_maps_lifecycle_statuses()
    {
        Assert.Equal(
            DispatchNotificationEventKinds.TripAssigned,
            DispatchNotificationRules.MapDispatchStatusToEventKind(TripDispatchStatuses.Assigned));
        Assert.Equal(
            DispatchNotificationEventKinds.TripDispatched,
            DispatchNotificationRules.MapDispatchStatusToEventKind(TripDispatchStatuses.Dispatched));
        Assert.Equal(
            DispatchNotificationEventKinds.TripInProgress,
            DispatchNotificationRules.MapDispatchStatusToEventKind(TripDispatchStatuses.InProgress));
        Assert.Equal(
            DispatchNotificationEventKinds.TripCompleted,
            DispatchNotificationRules.MapDispatchStatusToEventKind(TripDispatchStatuses.Completed));
        Assert.Equal(
            DispatchNotificationEventKinds.TripCancelled,
            DispatchNotificationRules.MapDispatchStatusToEventKind(TripDispatchStatuses.Cancelled));
        Assert.Null(DispatchNotificationRules.MapDispatchStatusToEventKind(TripDispatchStatuses.Planned));
    }

    [Fact]
    public void ShouldNotifyForEvent_requires_enabled_webhook_and_toggle()
    {
        var settings = new TenantDispatchNotificationSettingsSnapshot(
            true,
            "https://hooks.example.com/routarr",
            NotifyOnTripAssigned: true,
            NotifyOnTripDispatched: false,
            NotifyOnTripInProgress: false,
            NotifyOnTripCompleted: false,
            NotifyOnTripCancelled: false);

        Assert.True(DispatchNotificationRules.ShouldNotifyForEvent(
            settings,
            DispatchNotificationEventKinds.TripAssigned));
        Assert.False(DispatchNotificationRules.ShouldNotifyForEvent(
            settings,
            DispatchNotificationEventKinds.TripDispatched));
    }
}
