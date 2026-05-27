using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public static class WorkOrderStatusRules
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
            WorkOrderStatuses.Open => to is WorkOrderStatuses.InProgress or WorkOrderStatuses.Cancelled,
            WorkOrderStatuses.InProgress => to is WorkOrderStatuses.Completed or WorkOrderStatuses.Cancelled,
            _ => false,
        };
    }
}
