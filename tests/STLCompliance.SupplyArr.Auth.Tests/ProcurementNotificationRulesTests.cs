using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class ProcurementNotificationRulesTests
{
    [Fact]
    public void ShouldNotifyForEvent_requires_enabled_webhook_and_toggle()
    {
        var settings = new TenantProcurementNotificationSettingsSnapshot(
            true,
            "https://hooks.example.com/supplyarr",
            NotifyOnPurchaseRequestSubmitted: true,
            NotifyOnPurchaseRequestApproved: false,
            NotifyOnPurchaseOrderIssued: false,
            NotifyOnReceivingReceiptPosted: false);

        Assert.True(ProcurementNotificationRules.ShouldNotifyForEvent(
            settings,
            ProcurementNotificationEventKinds.PurchaseRequestSubmitted));
        Assert.False(ProcurementNotificationRules.ShouldNotifyForEvent(
            settings,
            ProcurementNotificationEventKinds.PurchaseOrderIssued));
    }

    [Fact]
    public void NormalizeFormat_rejects_invalid_webhook()
    {
        Assert.Throws<StlApiException>(
            () => ProcurementNotificationRules.NormalizeWebhookUrl("not-a-url", allowInsecureHttp: true));
    }
}
