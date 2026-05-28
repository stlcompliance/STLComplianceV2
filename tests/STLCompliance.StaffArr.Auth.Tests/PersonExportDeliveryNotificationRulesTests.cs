using StaffArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class PersonExportDeliveryNotificationRulesTests
{
    [Fact]
    public void NormalizeWebhookUrl_accepts_https()
    {
        var normalized = PersonExportDeliveryNotificationRules.NormalizeWebhookUrl(
            "https://hooks.example.test/path",
            allowInsecureHttp: false);
        Assert.Equal("https://hooks.example.test/path", normalized);
    }

    [Fact]
    public void NormalizeWebhookUrl_rejects_http_when_https_required()
    {
        var exception = Assert.Throws<StlApiException>(() =>
            PersonExportDeliveryNotificationRules.NormalizeWebhookUrl(
                "http://hooks.example.test/path",
                allowInsecureHttp: false));
        Assert.Equal("person.export_notification.webhook_https_required", exception.Code);
    }

    [Fact]
    public void NormalizeNotificationListLimit_clamps_to_max()
    {
        Assert.Equal(20, PersonExportDeliveryNotificationRules.NormalizeNotificationListLimit(null));
        Assert.Equal(100, PersonExportDeliveryNotificationRules.NormalizeNotificationListLimit(500));
    }
}
