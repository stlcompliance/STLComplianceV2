using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintenanceNotificationRulesTests
{
    [Fact]
    public void NormalizeWebhookUrl_allows_https_in_production_mode()
    {
        var url = MaintenanceNotificationRules.NormalizeWebhookUrl("https://hooks.example.com/maintainarr", false);
        Assert.Equal("https://hooks.example.com/maintainarr", url);
    }

    [Fact]
    public void NormalizeWebhookUrl_rejects_http_when_https_required()
    {
        Assert.Throws<StlApiException>(() =>
            MaintenanceNotificationRules.NormalizeWebhookUrl("http://hooks.example.com/maintainarr", false));
    }

    [Fact]
    public void ShouldNotifyForEvent_requires_enabled_webhook_and_toggle()
    {
        var settings = new TenantMaintenanceNotificationSettingsSnapshot(
            true,
            "https://hooks.example.com/maintainarr",
            NotifyOnWorkOrderCreated: true,
            NotifyOnPmScheduleDue: false,
            NotifyOnPmScheduleOverdue: false);

        Assert.True(MaintenanceNotificationRules.ShouldNotifyForEvent(
            settings,
            MaintenanceNotificationEventKinds.WorkOrderCreated));
        Assert.False(MaintenanceNotificationRules.ShouldNotifyForEvent(
            settings,
            MaintenanceNotificationEventKinds.PmScheduleDue));
    }

    [Fact]
    public void MapPmDueStatusToEventKind_maps_due_and_overdue()
    {
        Assert.Equal(
            MaintenanceNotificationEventKinds.PmScheduleDue,
            MaintenanceNotificationRules.MapPmDueStatusToEventKind(PmDueStatuses.Due));
        Assert.Equal(
            MaintenanceNotificationEventKinds.PmScheduleOverdue,
            MaintenanceNotificationRules.MapPmDueStatusToEventKind(PmDueStatuses.Overdue));
        Assert.Null(MaintenanceNotificationRules.MapPmDueStatusToEventKind("not_due"));
    }
}
