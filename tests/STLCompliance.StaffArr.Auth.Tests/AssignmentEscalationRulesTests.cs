using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class AssignmentEscalationRulesTests
{
    private static readonly DateTimeOffset DueAt = new(2026, 5, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsDueForEscalation_true_after_overdue_threshold()
    {
        var asOf = DueAt.AddHours(30);
        Assert.True(AssignmentEscalationRules.IsDueForEscalation(
            DueAt,
            lastEscalatedAt: null,
            overdueEscalationAfterHours: 24,
            cooldownHours: 48,
            escalationCount: 0,
            maxEscalations: 10,
            asOf));
    }

    [Fact]
    public void IsDueForEscalation_false_before_overdue_threshold()
    {
        var asOf = DueAt.AddHours(12);
        Assert.False(AssignmentEscalationRules.IsDueForEscalation(
            DueAt,
            null,
            24,
            48,
            0,
            10,
            asOf));
    }
}
