using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class AttachmentRetentionRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(TripDispatchStatuses.Completed, true)]
    [InlineData(TripDispatchStatuses.Cancelled, true)]
    [InlineData(TripDispatchStatuses.Assigned, false)]
    [InlineData(TripDispatchStatuses.InProgress, false)]
    public void IsClosedTripStatus_matches_terminal_states(string status, bool expected)
    {
        Assert.Equal(expected, AttachmentRetentionRules.IsClosedTripStatus(status));
    }

    [Fact]
    public void GetTripClosedAt_prefers_closed_at_for_completed_trips()
    {
        var closedAt = AsOf.AddDays(-100);
        var completedAt = AsOf.AddDays(-90);
        var updatedAt = AsOf.AddDays(-1);
        var result = AttachmentRetentionRules.GetTripClosedAt(
            TripDispatchStatuses.Completed,
            closedAt,
            completedAt,
            null,
            updatedAt);
        Assert.Equal(closedAt, result);
    }

    [Fact]
    public void GetTripClosedAt_uses_cancelled_at_for_cancelled_trips()
    {
        var cancelledAt = AsOf.AddDays(-40);
        var updatedAt = AsOf.AddDays(-1);
        var result = AttachmentRetentionRules.GetTripClosedAt(
            TripDispatchStatuses.Cancelled,
            null,
            null,
            cancelledAt,
            updatedAt);
        Assert.Equal(cancelledAt, result);
    }

    [Fact]
    public void IsExpired_respects_retention_boundary()
    {
        var closedAt = AsOf.AddDays(-31);
        Assert.True(AttachmentRetentionRules.IsExpired(closedAt, AsOf, 30));
        Assert.False(AttachmentRetentionRules.IsExpired(AsOf.AddDays(-29), AsOf, 30));
    }

    [Theory]
    [InlineData(null, 365)]
    [InlineData(10, 30)]
    [InlineData(5000, 3650)]
    public void NormalizeRetentionDays_clamps_to_supported_range(int? input, int expected)
    {
        Assert.Equal(expected, AttachmentRetentionRules.NormalizeRetentionDays(input));
    }
}
