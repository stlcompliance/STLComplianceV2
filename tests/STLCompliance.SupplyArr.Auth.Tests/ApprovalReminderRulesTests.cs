using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class ApprovalReminderRulesTests
{
    [Fact]
    public void IsDueForReminder_returns_true_when_initial_threshold_passed()
    {
        var asOf = DateTimeOffset.UtcNow;
        var pendingSince = asOf.AddHours(-48);
        Assert.True(ApprovalReminderRules.IsDueForReminder(
            pendingSince,
            null,
            thresholdHours: 24,
            cooldownHours: 24,
            reminderCount: 0,
            maxReminders: 10,
            asOf));
    }

    [Fact]
    public void IsDueForReminder_returns_false_when_under_initial_threshold()
    {
        var asOf = DateTimeOffset.UtcNow;
        var pendingSince = asOf.AddHours(-2);
        Assert.False(ApprovalReminderRules.IsDueForReminder(
            pendingSince,
            null,
            thresholdHours: 24,
            cooldownHours: 24,
            reminderCount: 0,
            maxReminders: 10,
            asOf));
    }

    [Fact]
    public void IsDueForReminder_returns_false_when_max_reminders_reached()
    {
        var asOf = DateTimeOffset.UtcNow;
        var pendingSince = asOf.AddHours(-100);
        Assert.False(ApprovalReminderRules.IsDueForReminder(
            pendingSince,
            asOf.AddHours(-50),
            thresholdHours: 24,
            cooldownHours: 24,
            reminderCount: 10,
            maxReminders: 10,
            asOf));
    }

    [Fact]
    public void IsDueForReminder_respects_cooldown_after_previous_reminder()
    {
        var asOf = DateTimeOffset.UtcNow;
        var pendingSince = asOf.AddHours(-100);
        var lastReminder = asOf.AddHours(-2);
        Assert.False(ApprovalReminderRules.IsDueForReminder(
            pendingSince,
            lastReminder,
            thresholdHours: 24,
            cooldownHours: 24,
            reminderCount: 1,
            maxReminders: 10,
            asOf));
    }

    [Fact]
    public void IsOverdue_identifies_items_past_threshold()
    {
        var asOf = DateTimeOffset.UtcNow;
        var pendingSince = asOf.AddHours(-30);
        Assert.True(ApprovalReminderRules.IsOverdue(pendingSince, 24, asOf));
        Assert.False(ApprovalReminderRules.IsOverdue(asOf.AddHours(-2), 24, asOf));
    }

    [Fact]
    public void GetReminderEventKind_maps_subject_types()
    {
        Assert.Equal(
            ProcurementNotificationEventKinds.PurchaseRequestApprovalReminder,
            ApprovalReminderRules.GetReminderEventKind(ApprovalReminderSubjectTypes.PurchaseRequest));
        Assert.Equal(
            ProcurementNotificationEventKinds.PurchaseOrderApprovalReminder,
            ApprovalReminderRules.GetReminderEventKind(ApprovalReminderSubjectTypes.PurchaseOrder));
    }
}
