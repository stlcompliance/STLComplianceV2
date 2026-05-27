using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class PmDueScanRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("active", true)]
    [InlineData("paused", false)]
    public void IsScannableScheduleStatus_matches_active_schedules_only(string status, bool expected) =>
        Assert.Equal(expected, PmDueScanRules.IsScannableScheduleStatus(status));

    [Theory]
    [InlineData("active", PmDueStatuses.Scheduled, -1, true)]
    [InlineData("active", PmDueStatuses.Scheduled, 1, false)]
    [InlineData("paused", PmDueStatuses.Scheduled, -1, false)]
    public void ShouldMarkDue_uses_next_due_and_status(
        string scheduleStatus,
        string dueStatus,
        int dueOffsetDays,
        bool expected)
    {
        var nextDueAt = AsOf.AddDays(dueOffsetDays);
        Assert.Equal(
            expected,
            PmDueScanRules.ShouldMarkDue(scheduleStatus, dueStatus, nextDueAt, AsOf));
    }

    [Theory]
    [InlineData("active", PmDueStatuses.Due, -2, true)]
    [InlineData("active", PmDueStatuses.Due, 0, false)]
    [InlineData("active", PmDueStatuses.Scheduled, -2, true)]
    public void ShouldMarkOverdue_applies_grace_period(
        string scheduleStatus,
        string dueStatus,
        int dueOffsetDays,
        bool expected)
    {
        var nextDueAt = AsOf.AddDays(dueOffsetDays);
        Assert.Equal(
            expected,
            PmDueScanRules.ShouldMarkOverdue(scheduleStatus, dueStatus, nextDueAt, AsOf, overdueGraceDays: 1));
    }

    [Fact]
    public void ResolveTargetDueStatus_prefers_overdue_when_past_grace() =>
        Assert.Equal(
            PmDueStatuses.Overdue,
            PmDueScanRules.ResolveTargetDueStatus(
                "active",
                PmDueStatuses.Due,
                AsOf.AddDays(-3),
                AsOf,
                overdueGraceDays: 1));
}
