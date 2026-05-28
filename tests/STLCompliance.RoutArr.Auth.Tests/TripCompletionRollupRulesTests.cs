using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class TripCompletionRollupRulesTests
{
    [Fact]
    public void IsStale_returns_true_when_never_computed()
    {
        var asOf = DateTimeOffset.UtcNow;
        Assert.True(TripCompletionRollupRules.IsStale(null, asOf, 1));
    }

    [Fact]
    public void IsPending_returns_true_when_trip_updated_after_source()
    {
        var asOf = DateTimeOffset.UtcNow;
        var tripUpdatedAt = asOf;
        var sourceUpdatedAt = asOf.AddHours(-2);
        var computedAt = asOf.AddMinutes(-30);

        Assert.True(TripCompletionRollupRules.IsPending(
            tripUpdatedAt,
            sourceUpdatedAt,
            computedAt,
            asOf,
            1));
    }

    [Fact]
    public void ComputeDurationMinutes_calculates_elapsed_minutes()
    {
        var startedAt = DateTimeOffset.Parse("2026-05-28T10:00:00Z");
        var completedAt = startedAt.AddMinutes(95);

        Assert.Equal(95, TripCompletionRollupRules.ComputeDurationMinutes(startedAt, completedAt));
    }

    [Fact]
    public void IsTerminalTrip_matches_completed_and_cancelled()
    {
        Assert.True(TripCompletionRollupRules.IsTerminalTrip(TripDispatchStatuses.Completed));
        Assert.True(TripCompletionRollupRules.IsTerminalTrip(TripDispatchStatuses.Cancelled));
        Assert.False(TripCompletionRollupRules.IsTerminalTrip(TripDispatchStatuses.InProgress));
    }
}
