using TrainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class EvidenceRetentionRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("completed", true)]
    [InlineData("cancelled", true)]
    [InlineData("assigned", false)]
    [InlineData("in_progress", false)]
    public void IsClosedAssignmentStatus_matches_terminal_states(string status, bool expected)
    {
        Assert.Equal(expected, EvidenceRetentionRules.IsClosedAssignmentStatus(status));
    }

    [Fact]
    public void GetAssignmentClosedAt_uses_completed_at_for_completed_assignments()
    {
        var completedAt = AsOf.AddDays(-100);
        var updatedAt = AsOf.AddDays(-1);
        var result = EvidenceRetentionRules.GetAssignmentClosedAt("completed", completedAt, updatedAt);
        Assert.Equal(completedAt, result);
    }

    [Fact]
    public void GetAssignmentClosedAt_uses_updated_at_for_cancelled_assignments()
    {
        var updatedAt = AsOf.AddDays(-40);
        var result = EvidenceRetentionRules.GetAssignmentClosedAt("cancelled", null, updatedAt);
        Assert.Equal(updatedAt, result);
    }

    [Fact]
    public void IsExpired_respects_retention_boundary()
    {
        var closedAt = AsOf.AddDays(-31);
        Assert.True(EvidenceRetentionRules.IsExpired(closedAt, AsOf, 30));
        Assert.False(EvidenceRetentionRules.IsExpired(AsOf.AddDays(-29), AsOf, 30));
    }

    [Theory]
    [InlineData(null, 365)]
    [InlineData(10, 30)]
    [InlineData(5000, 3650)]
    public void NormalizeRetentionDays_clamps_to_supported_range(int? input, int expected)
    {
        Assert.Equal(expected, EvidenceRetentionRules.NormalizeRetentionDays(input));
    }
}
