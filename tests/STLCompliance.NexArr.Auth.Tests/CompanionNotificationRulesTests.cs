using NexArr.Api.Entities;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class CompanionNotificationRulesTests
{
    [Fact]
    public void ShouldNotifyForEvent_respects_toggles()
    {
        var settings = new TenantCompanionNotificationSettingsSnapshot(
            true,
            "https://hooks.example.com/companion",
            NotifyOnHandoffRedeemed: true,
            NotifyOnFieldInboxRefreshed: false);

        Assert.True(CompanionNotificationRules.ShouldNotifyForEvent(
            settings,
            CompanionNotificationEventKinds.HandoffRedeemed));
        Assert.False(CompanionNotificationRules.ShouldNotifyForEvent(
            settings,
            CompanionNotificationEventKinds.FieldInboxRefreshed));
    }

    [Fact]
    public void NormalizeWebhookUrl_rejects_invalid()
    {
        Assert.Throws<STLCompliance.Shared.Contracts.StlApiException>(
            () => CompanionNotificationRules.NormalizeWebhookUrl("not-a-url", allowInsecureHttp: true));
    }
}
