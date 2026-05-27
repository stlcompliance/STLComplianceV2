using RoutArr.Api.Entities;

namespace RoutArr.Api.Services;

public static class TripDispatchStatusRules
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
            TripDispatchStatuses.Planned => to is TripDispatchStatuses.Assigned or TripDispatchStatuses.Cancelled,
            TripDispatchStatuses.Assigned => to is TripDispatchStatuses.Dispatched or TripDispatchStatuses.Cancelled,
            TripDispatchStatuses.Dispatched => to is TripDispatchStatuses.InProgress or TripDispatchStatuses.Cancelled,
            TripDispatchStatuses.InProgress => to is TripDispatchStatuses.Completed or TripDispatchStatuses.Cancelled,
            _ => false,
        };
    }
}
