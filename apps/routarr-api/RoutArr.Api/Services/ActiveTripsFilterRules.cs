using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public static class ActiveTripsFilterRules
{
    public const string StatusAll = "all";

    public const string StatusDispatched = "dispatched";

    public const string StatusInProgress = "in_progress";

    public static string NormalizeStatusFilter(string? statusFilter)
    {
        if (string.IsNullOrWhiteSpace(statusFilter))
        {
            return StatusAll;
        }

        var normalized = statusFilter.Trim().ToLowerInvariant();
        return normalized switch
        {
            StatusAll => StatusAll,
            StatusDispatched => StatusDispatched,
            StatusInProgress => StatusInProgress,
            _ => throw new StlApiException(
                "active_trips.invalid_status_filter",
                "Active trips status filter must be all, dispatched, or in_progress.",
                400),
        };
    }

    public static bool MatchesStatusFilter(string dispatchStatus, string statusFilter) =>
        statusFilter switch
        {
            StatusDispatched => string.Equals(
                dispatchStatus,
                TripDispatchStatuses.Dispatched,
                StringComparison.OrdinalIgnoreCase),
            StatusInProgress => string.Equals(
                dispatchStatus,
                TripDispatchStatuses.InProgress,
                StringComparison.OrdinalIgnoreCase),
            _ => true,
        };

    public static bool MatchesAttentionFilter(bool isLate, bool isAtRisk, bool attentionOnly) =>
        !attentionOnly || isLate || isAtRisk;
}
