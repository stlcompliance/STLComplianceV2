using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class ActiveTripsTimelineTests
{
    [Fact]
    public void ComputeTimelinePosition_places_trip_within_window()
    {
        var windowStart = new DateTimeOffset(2026, 5, 28, 8, 0, 0, TimeSpan.Zero);
        var windowEnd = windowStart.AddHours(8);
        var spanMs = (windowEnd - windowStart).TotalMilliseconds;

        var (offset, width) = ActiveTripsTimelineRules.ComputeTimelinePosition(
            windowStart.AddHours(2),
            windowStart.AddHours(4),
            windowStart,
            windowEnd,
            spanMs);

        Assert.InRange(offset, 20, 30);
        Assert.InRange(width, 20, 30);
    }

    [Fact]
    public void ComputeTimelinePosition_defaults_when_start_missing()
    {
        var windowStart = DateTimeOffset.UtcNow;
        var windowEnd = windowStart.AddDays(1);

        var (offset, width) = ActiveTripsTimelineRules.ComputeTimelinePosition(
            null,
            null,
            windowStart,
            windowEnd,
            (windowEnd - windowStart).TotalMilliseconds);

        Assert.Equal(0, offset);
        Assert.Equal(6, width);
    }
}
