using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public static class RouteCalendarRules
{
    public const int MaxCustomRangeDays = 31;

    public static DateTimeOffset ParseUtcDate(string value, string errorCode = "route_calendar.invalid_date")
    {
        if (!DateTimeOffset.TryParse(value, out var parsed))
        {
            throw new StlApiException(
                errorCode,
                "Dates must be ISO-8601 values.",
                400);
        }

        return ToUtcDayStart(parsed);
    }

    public static DateTimeOffset ToUtcDayStart(DateTimeOffset value) =>
        new(value.UtcDateTime.Date, TimeSpan.Zero);

    public static IEnumerable<DateTimeOffset> EnumerateDays(DateTimeOffset windowStart, DateTimeOffset windowEnd)
    {
        var day = ToUtcDayStart(windowStart);
        var end = ToUtcDayStart(windowEnd);
        while (day < end)
        {
            yield return day;
            day = day.AddDays(1);
        }
    }

    public static IEnumerable<DateTimeOffset> DaysForEvent(
        DateTimeOffset scheduledAt,
        DateTimeOffset? scheduledEndAt,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd)
    {
        var eventStart = scheduledAt;
        var eventEnd = scheduledEndAt ?? scheduledAt;

        foreach (var day in EnumerateDays(windowStart, windowEnd))
        {
            var dayEnd = day.AddDays(1);
            if (eventStart < dayEnd && eventEnd >= day)
            {
                yield return day;
            }
        }
    }
}
