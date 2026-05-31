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
            NotifyOnTripAccepted: true,
            NotifyOnTripInProgress: false,
            NotifyOnTripCompleted: false,
            NotifyOnTripCancelled: false,
            NotifyOnDriverAssignmentChanged: true,
            NotifyOnRouteCancelled: true);

        Assert.True(DispatchNotificationRules.ShouldNotifyForEvent(
            settings,
            DispatchNotificationEventKinds.TripAssigned));
        Assert.False(DispatchNotificationRules.ShouldNotifyForEvent(
            settings,
            DispatchNotificationEventKinds.TripDispatched));
        Assert.True(DispatchNotificationRules.ShouldNotifyForEvent(
            settings,
            DispatchNotificationEventKinds.TripAccepted));
        Assert.True(DispatchNotificationRules.ShouldNotifyForEvent(
            settings,
            DispatchNotificationEventKinds.DriverAssignmentChanged));
        Assert.True(DispatchNotificationRules.ShouldNotifyForEvent(
            settings,
            DispatchNotificationEventKinds.RouteCancelled));
    }

    [Fact]
    public void ValidateUpsertRequest_requires_webhook_when_enabled()
    {
        var exception = Assert.Throws<StlApiException>(() =>
            DispatchNotificationRules.ValidateUpsertRequest(true, null));
        Assert.Equal("routarr.notification.webhook_required", exception.Code);

        Assert.Throws<StlApiException>(() =>
            DispatchNotificationRules.ValidateUpsertRequest(true, "   "));
    }

    [Fact]
    public void ValidateUpsertRequest_allows_empty_webhook_when_disabled()
    {
        DispatchNotificationRules.ValidateUpsertRequest(false, null);
        DispatchNotificationRules.ValidateUpsertRequest(false, "   ");
    }

    [Fact]
    public void NormalizeWebhookUrl_rejects_invalid_url()
    {
        var exception = Assert.Throws<StlApiException>(() =>
            DispatchNotificationRules.NormalizeWebhookUrl("not-a-url", allowInsecureHttp: true));
        Assert.Equal("routarr.notification.webhook_invalid", exception.Code);
    }
}
