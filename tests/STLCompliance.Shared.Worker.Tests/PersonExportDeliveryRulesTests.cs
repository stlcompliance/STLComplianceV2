using StaffArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class PersonExportDeliveryRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 27, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(null, 24, true)]
    [InlineData(-25, 24, true)]
    [InlineData(-23, 24, false)]
    [InlineData(-49, 48, true)]
    public void IsDue_uses_last_delivered_at_and_interval(
        int? deliveredOffsetHours,
        int intervalHours,
        bool expected)
    {
        DateTimeOffset? lastDeliveredAt = deliveredOffsetHours is int offset
            ? AsOf.AddHours(offset)
            : null;

        Assert.Equal(expected, PersonExportDeliveryRules.IsDue(lastDeliveredAt, AsOf, intervalHours));
    }

    [Theory]
    [InlineData(null, 10)]
    [InlineData(200, 100)]
    [InlineData(0, 1)]
    public void NormalizeBatchSize_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, PersonExportDeliveryRules.NormalizeBatchSize(input));

    [Theory]
    [InlineData(null, 24)]
    [InlineData(1000, 720)]
    [InlineData(0, 1)]
    public void NormalizeIntervalHours_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, PersonExportDeliveryRules.NormalizeIntervalHours(input));
}
