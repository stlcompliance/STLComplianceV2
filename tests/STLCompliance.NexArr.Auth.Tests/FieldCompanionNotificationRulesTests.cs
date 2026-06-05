using NexArr.Api.Entities;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class FieldCompanionNotificationRulesTests
{
    [Fact]
    public void ShouldNotifyForEvent_respects_toggles()
    {
        var settings = new TenantFieldCompanionNotificationSettingsSnapshot(
            true,
            "https://hooks.example.com/fieldcompanion",
            NotifyOnHandoffRedeemed: true,
            NotifyOnFieldInboxRefreshed: false);

        Assert.True(FieldCompanionNotificationRules.ShouldNotifyForEvent(
            settings,
            FieldCompanionNotificationEventKinds.HandoffRedeemed));
        Assert.False(FieldCompanionNotificationRules.ShouldNotifyForEvent(
            settings,
            FieldCompanionNotificationEventKinds.FieldInboxRefreshed));
    }

    [Fact]
    public void NormalizeWebhookUrl_rejects_invalid()
    {
        Assert.Throws<STLCompliance.Shared.Contracts.StlApiException>(
            () => FieldCompanionNotificationRules.NormalizeWebhookUrl("not-a-url", allowInsecureHttp: true));
    }
}
