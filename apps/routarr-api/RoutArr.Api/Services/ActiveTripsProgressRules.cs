using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class ActiveTripsProgressRules
{
    public static (int CompletedStopCount, int TotalStopCount, int StopProgressPercent) ComputeStopProgress(
        int completedStopCount,
        int totalStopCount)
    {
        if (totalStopCount <= 0)
        {
            return (completedStopCount, totalStopCount, 0);
        }

        var percent = (int)Math.Round(completedStopCount * 100.0 / totalStopCount, MidpointRounding.AwayFromZero);
        return (completedStopCount, totalStopCount, Math.Clamp(percent, 0, 100));
    }

    public static bool IsCompletedStop(string stopStatus) =>
        string.Equals(stopStatus, RouteStopStatuses.Completed, StringComparison.OrdinalIgnoreCase)
        || string.Equals(stopStatus, RouteStopStatuses.Skipped, StringComparison.OrdinalIgnoreCase);
}
