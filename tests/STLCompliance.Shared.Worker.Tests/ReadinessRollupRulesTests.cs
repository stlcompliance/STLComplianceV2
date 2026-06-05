using StaffArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class ReadinessRollupRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(null, 1, true)]
    [InlineData(-2, 1, true)]
    [InlineData(-1, 1, false)]
    [InlineData(-3, 2, true)]
    public void IsStale_uses_computed_at_and_staleness_window(
        int? computedOffsetHours,
        int stalenessHours,
        bool expected)
    {
        DateTimeOffset? computedAt = computedOffsetHours is int offset
            ? AsOf.AddHours(offset)
            : null;

        Assert.Equal(expected, ReadinessRollupRules.IsStale(computedAt, AsOf, stalenessHours));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(4, 2, 50)]
    [InlineData(3, 3, 100)]
    public void ComputeReadyPercent_rounds_to_one_decimal(int total, int ready, decimal expected) =>
        Assert.Equal(expected, ReadinessRollupRules.ComputeReadyPercent(total, ready));

    [Fact]
    public void AggregateCounts_tracks_ready_not_ready_and_overrides()
    {
        var members = new[]
        {
            new PersonReadinessRollupSnapshot(Guid.NewGuid(), "ready", false, "high"),
            new PersonReadinessRollupSnapshot(Guid.NewGuid(), "not_ready", true, "medium"),
            new PersonReadinessRollupSnapshot(Guid.NewGuid(), "not_ready", false, "low")
        };

        var (ready, notReady, overrides) = ReadinessRollupRules.AggregateCounts(members);

        Assert.Equal(1, ready);
        Assert.Equal(2, notReady);
        Assert.Equal(1, overrides);
    }

    [Theory]
    [InlineData(null, 50)]
    [InlineData(600, 500)]
    [InlineData(0, 1)]
    public void NormalizeBatchSize_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, ReadinessRollupRules.NormalizeBatchSize(input));
}
