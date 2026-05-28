using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class AssignmentDueReminderRulesTests
{
    private static readonly DateTimeOffset DueAt = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsDueForReminder_true_inside_lead_window()
    {
        var asOf = DueAt.AddDays(-3);
        Assert.True(AssignmentDueReminderRules.IsDueForReminder(
            DueAt,
            lastReminderSentAt: null,
            dueSoonLeadDays: 7,
            cooldownHours: 24,
            reminderCount: 0,
            maxReminders: 5,
            asOf));
    }

    [Fact]
    public void IsDueForReminder_false_after_due_date()
    {
        var asOf = DueAt.AddHours(1);
        Assert.False(AssignmentDueReminderRules.IsDueForReminder(
            DueAt,
            null,
            7,
            24,
            0,
            5,
            asOf));
    }

    [Fact]
    public void IsDueForReminder_false_when_max_reminders_reached()
    {
        var asOf = DueAt.AddDays(-1);
        Assert.False(AssignmentDueReminderRules.IsDueForReminder(
            DueAt,
            DueAt.AddDays(-2),
            7,
            24,
            5,
            5,
            asOf));
    }
}
