using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class RouteStopStatusRules
{
    public static bool CanTransition(string fromStatus, string toStatus)
    {
        var from = fromStatus.Trim().ToLowerInvariant();
        var to = toStatus.Trim().ToLowerInvariant();

        if (string.Equals(from, to, StringComparison.Ordinal))
        {
            return true;
        }

        return from switch
        {
            RouteStopStatuses.Pending => to is RouteStopStatuses.Arrived or RouteStopStatuses.Skipped,
            RouteStopStatuses.Arrived => to is RouteStopStatuses.Completed or RouteStopStatuses.Skipped,
            _ => false,
        };
    }
}
