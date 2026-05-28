using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainingNotificationRulesTests
{
    [Fact]
    public void NormalizeWebhookUrl_accepts_https_in_testing()
    {
        var normalized = TrainingNotificationRules.NormalizeWebhookUrl(
            "https://hooks.example.test/trainarr",
            allowInsecureHttp: true);
        Assert.Equal("https://hooks.example.test/trainarr", normalized);
    }

    [Fact]
    public void NormalizeWebhookUrl_rejects_invalid_url()
    {
        Assert.Throws<STLCompliance.Shared.Contracts.StlApiException>(() =>
            TrainingNotificationRules.NormalizeWebhookUrl("not-a-url", allowInsecureHttp: true));
    }

    [Fact]
    public void NormalizeDispatchListLimit_clamps()
    {
        Assert.Equal(20, TrainingNotificationRules.NormalizeDispatchListLimit(null));
        Assert.Equal(100, TrainingNotificationRules.NormalizeDispatchListLimit(500));
    }
}
