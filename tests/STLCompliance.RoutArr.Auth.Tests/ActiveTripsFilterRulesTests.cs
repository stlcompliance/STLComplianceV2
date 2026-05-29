using RoutArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class ActiveTripsFilterRulesTests
{
    [Theory]
    [InlineData("dispatched", "dispatched", true)]
    [InlineData("dispatched", "in_progress", false)]
    [InlineData("in_progress", "in_progress", true)]
    [InlineData("planned", "all", true)]
    public void MatchesStatusFilter_respects_filter(string dispatchStatus, string filter, bool expected)
    {
        var normalized = ActiveTripsFilterRules.NormalizeStatusFilter(filter);
        Assert.Equal(expected, ActiveTripsFilterRules.MatchesStatusFilter(dispatchStatus, normalized));
    }

    [Fact]
    public void MatchesAttentionFilter_only_late_or_at_risk_when_enabled()
    {
        Assert.True(ActiveTripsFilterRules.MatchesAttentionFilter(true, false, true));
        Assert.True(ActiveTripsFilterRules.MatchesAttentionFilter(false, true, true));
        Assert.False(ActiveTripsFilterRules.MatchesAttentionFilter(false, false, true));
        Assert.True(ActiveTripsFilterRules.MatchesAttentionFilter(false, false, false));
    }

    [Fact]
    public void NormalizeStatusFilter_rejects_unknown()
    {
        var ex = Assert.Throws<StlApiException>(() =>
            ActiveTripsFilterRules.NormalizeStatusFilter("invalid"));
        Assert.Equal("active_trips.invalid_status_filter", ex.Code);
    }
}
