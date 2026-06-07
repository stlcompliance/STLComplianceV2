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
            RouteStopStatuses.Pending or RouteStopStatuses.Planned => to is RouteStopStatuses.EnRoute or RouteStopStatuses.Arrived or RouteStopStatuses.Skipped or RouteStopStatuses.Failed or RouteStopStatuses.Canceled,
            RouteStopStatuses.EnRoute => to is RouteStopStatuses.Arrived or RouteStopStatuses.Skipped or RouteStopStatuses.Failed or RouteStopStatuses.Canceled,
            RouteStopStatuses.Arrived => to is RouteStopStatuses.InProgress or RouteStopStatuses.Completed or RouteStopStatuses.Skipped or RouteStopStatuses.Failed or RouteStopStatuses.Canceled,
            RouteStopStatuses.InProgress => to is RouteStopStatuses.Completed or RouteStopStatuses.Failed or RouteStopStatuses.Canceled,
            _ => false,
        };
    }
}
