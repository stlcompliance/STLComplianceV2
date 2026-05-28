namespace RoutArr.Api.Services;

public static class ActiveTripsTimelineRules
{
    public static (double OffsetPercent, double WidthPercent) ComputeTimelinePosition(
        DateTimeOffset? scheduledStartAt,
        DateTimeOffset? scheduledEndAt,
        DateTimeOffset windowStart,
        DateTimeOffset windowEnd,
        double windowSpanMs)
    {
        const double minWidth = 6;
        const double maxWidth = 40;

        if (!scheduledStartAt.HasValue)
        {
            return (0, minWidth);
        }

        var startMs = (scheduledStartAt.Value - windowStart).TotalMilliseconds;
        var offset = Math.Clamp(startMs / windowSpanMs * 100, 0, 100 - minWidth);

        if (!scheduledEndAt.HasValue)
        {
            return (offset, minWidth);
        }

        var durationMs = (scheduledEndAt.Value - scheduledStartAt.Value).TotalMilliseconds;
        var width = Math.Clamp(durationMs / windowSpanMs * 100, minWidth, maxWidth);
        if (offset + width > 100)
        {
            width = 100 - offset;
        }

        return (Math.Round(offset, 2), Math.Round(width, 2));
    }
}
