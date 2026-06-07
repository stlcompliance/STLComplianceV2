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
            WorkOrderStatuses.Draft => to is
                WorkOrderStatuses.Open or
                WorkOrderStatuses.Scheduled or
                WorkOrderStatuses.InProgress or
                WorkOrderStatuses.Cancelled or
                WorkOrderStatuses.Canceled,
            WorkOrderStatuses.Open or WorkOrderStatuses.Requested => to is
                WorkOrderStatuses.Triage or
                WorkOrderStatuses.Rejected or
                WorkOrderStatuses.Approved or
                WorkOrderStatuses.Planned or
                WorkOrderStatuses.Scheduled or
                WorkOrderStatuses.Assigned or
                WorkOrderStatuses.InProgress or
                WorkOrderStatuses.Cancelled or
                WorkOrderStatuses.Canceled,
            WorkOrderStatuses.Triage => to is
                WorkOrderStatuses.Rejected or
                WorkOrderStatuses.Approved or
                WorkOrderStatuses.Planned or
                WorkOrderStatuses.Scheduled or
                WorkOrderStatuses.Assigned or
                WorkOrderStatuses.InProgress or
                WorkOrderStatuses.Cancelled or
                WorkOrderStatuses.Canceled,
            WorkOrderStatuses.Approved => to is
                WorkOrderStatuses.Planned or
                WorkOrderStatuses.Scheduled or
                WorkOrderStatuses.Assigned or
                WorkOrderStatuses.InProgress or
                WorkOrderStatuses.WaitingParts or
                WorkOrderStatuses.WaitingLabor or
                WorkOrderStatuses.WaitingVendor or
                WorkOrderStatuses.WaitingApproval or
                WorkOrderStatuses.WaitingCompliance or
                WorkOrderStatuses.Cancelled or
                WorkOrderStatuses.Canceled,
            WorkOrderStatuses.Planned => to is
                WorkOrderStatuses.WaitingParts or
                WorkOrderStatuses.WaitingLabor or
                WorkOrderStatuses.WaitingVendor or
                WorkOrderStatuses.WaitingApproval or
                WorkOrderStatuses.WaitingCompliance or
                WorkOrderStatuses.Scheduled or
                WorkOrderStatuses.Assigned or
                WorkOrderStatuses.InProgress or
                WorkOrderStatuses.Cancelled or
                WorkOrderStatuses.Canceled,
            WorkOrderStatuses.WaitingParts or
            WorkOrderStatuses.WaitingLabor or
            WorkOrderStatuses.WaitingVendor or
            WorkOrderStatuses.WaitingApproval or
            WorkOrderStatuses.WaitingCompliance or
            WorkOrderStatuses.Scheduled or
            WorkOrderStatuses.Assigned => to is
                WorkOrderStatuses.Planned or
                WorkOrderStatuses.Assigned or
                WorkOrderStatuses.InProgress or
                WorkOrderStatuses.Paused or
                WorkOrderStatuses.Blocked or
                WorkOrderStatuses.CompletedPendingReview or
                WorkOrderStatuses.Cancelled or
                WorkOrderStatuses.Canceled,
            WorkOrderStatuses.InProgress => to is
                WorkOrderStatuses.Paused or
                WorkOrderStatuses.Blocked or
                WorkOrderStatuses.CompletedPendingReview or
                WorkOrderStatuses.Completed or
                WorkOrderStatuses.Closed or
                WorkOrderStatuses.Cancelled or
                WorkOrderStatuses.Canceled,
            WorkOrderStatuses.Paused => to is
                WorkOrderStatuses.InProgress or
                WorkOrderStatuses.Blocked or
                WorkOrderStatuses.Cancelled or
                WorkOrderStatuses.Canceled,
            WorkOrderStatuses.Blocked => to is
                WorkOrderStatuses.InProgress or
                WorkOrderStatuses.Paused or
                WorkOrderStatuses.Cancelled or
                WorkOrderStatuses.Canceled,
            WorkOrderStatuses.CompletedPendingReview => to is
                WorkOrderStatuses.Completed or
                WorkOrderStatuses.Closed,
            WorkOrderStatuses.Completed => to is
                WorkOrderStatuses.Closed,
            WorkOrderStatuses.Closed => false,
            _ => false,
        };
    }
}
