using TrainArr.Api.Entities;
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

    [Fact]
    public void TryMapDomainEventKind_maps_lifecycle_events()
    {
        Assert.Equal(
            TrainingNotificationEventKinds.AssignmentCompleted,
            TrainingNotificationRules.TryMapDomainEventKind(TrainingDomainEventKinds.AssignmentCompleted));
        Assert.Equal(
            TrainingNotificationEventKinds.QualificationIssued,
            TrainingNotificationRules.TryMapDomainEventKind(TrainingDomainEventKinds.QualificationIssued));
        Assert.Null(TrainingNotificationRules.TryMapDomainEventKind("unknown"));
    }

    [Fact]
    public void NormalizeMaxAttempts_clamps()
    {
        Assert.Equal(10, TrainingNotificationRules.NormalizeMaxAttempts(null));
        Assert.Equal(50, TrainingNotificationRules.NormalizeMaxAttempts(500));
    }

    [Fact]
    public void ShouldNotifyForEvent_includes_reminder_and_escalation_kinds()
    {
        var settings = new TenantTrainingNotificationSettingsSnapshot(
            true,
            "https://hooks.example.test",
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            true,
            30,
            10,
            5);

        Assert.True(TrainingNotificationRules.ShouldNotifyForEvent(
            settings,
            TrainingNotificationEventKinds.AssignmentDueReminder));
        Assert.True(TrainingNotificationRules.ShouldNotifyForEvent(
            settings,
            TrainingNotificationEventKinds.AssignmentOverdueEscalation));
    }
}
