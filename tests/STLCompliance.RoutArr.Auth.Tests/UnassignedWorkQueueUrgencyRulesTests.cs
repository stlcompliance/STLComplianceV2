using RoutArr.Api.Contracts;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class UnassignedWorkQueueUrgencyRulesTests
{
    [Fact]
    public void SortByUrgency_orders_late_before_at_risk_before_on_track()
    {
        var now = DateTimeOffset.UtcNow;
        var items = new[]
        {
            new UnassignedWorkQueueTripRow(
                Guid.NewGuid(), "C", "On track", "planned", now.AddHours(2), null, false, false, 0, 0, 120),
            new UnassignedWorkQueueTripRow(
                Guid.NewGuid(), "A", "Late", "planned", now.AddHours(-1), null, true, false, 0, 0, -60),
            new UnassignedWorkQueueTripRow(
                Guid.NewGuid(), "B", "At risk", "planned", now.AddMinutes(30), null, false, true, 0, 0, 30),
        };

        var sorted = UnassignedWorkQueueUrgencyRules.SortByUrgency(items);

        Assert.Equal("Late", sorted[0].Title);
        Assert.Equal("At risk", sorted[1].Title);
        Assert.Equal("On track", sorted[2].Title);
    }

    [Fact]
    public void MatchesAttentionFilter_only_urgent_when_enabled()
    {
        Assert.True(UnassignedWorkQueueUrgencyRules.MatchesAttentionFilter(true, false, true));
        Assert.True(UnassignedWorkQueueUrgencyRules.MatchesAttentionFilter(false, true, true));
        Assert.False(UnassignedWorkQueueUrgencyRules.MatchesAttentionFilter(false, false, true));
    }
}
