using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RouteCalendarRulesTests
{
    [Fact]
    public void Enumerate_days_returns_each_day_in_window()
    {
        var start = new DateTimeOffset(2026, 5, 27, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddDays(3);

        var days = RouteCalendarRules.EnumerateDays(start, end).ToList();

        Assert.Equal(3, days.Count);
        Assert.Equal(start, days[0]);
        Assert.Equal(start.AddDays(1), days[1]);
        Assert.Equal(start.AddDays(2), days[2]);
    }

    [Fact]
    public void Days_for_event_places_single_day_event_on_matching_day()
    {
        var windowStart = new DateTimeOffset(2026, 5, 27, 0, 0, 0, TimeSpan.Zero);
        var windowEnd = windowStart.AddDays(7);
        var scheduledAt = windowStart.AddHours(10);

        var days = RouteCalendarRules
            .DaysForEvent(scheduledAt, null, windowStart, windowEnd)
            .ToList();

        Assert.Single(days);
        Assert.Equal(windowStart, days[0]);
    }

    [Fact]
    public void Days_for_event_spans_multiple_days_when_trip_crosses_midnight()
    {
        var windowStart = new DateTimeOffset(2026, 5, 27, 0, 0, 0, TimeSpan.Zero);
        var windowEnd = windowStart.AddDays(7);
        var scheduledAt = windowStart.AddHours(22);
        var scheduledEnd = windowStart.AddDays(1).AddHours(6);

        var days = RouteCalendarRules
            .DaysForEvent(scheduledAt, scheduledEnd, windowStart, windowEnd)
            .ToList();

        Assert.Equal(2, days.Count);
        Assert.Equal(windowStart, days[0]);
        Assert.Equal(windowStart.AddDays(1), days[1]);
    }
}
