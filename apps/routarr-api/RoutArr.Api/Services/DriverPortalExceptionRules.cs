using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public static class DriverPortalExceptionRules
{
    public const string TrafficDelay = "traffic_delay";

    public const string EquipmentIssue = "equipment_issue";

    public const string CustomerAccess = "customer_access";

    public const string RouteIssue = "route_issue";

    public const string Other = "other";

    public static readonly IReadOnlySet<string> ExceptionTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        TrafficDelay,
        EquipmentIssue,
        CustomerAccess,
        RouteIssue,
        Other,
    };

    public static readonly IReadOnlySet<string> ReportableTripStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        TripDispatchStatuses.Assigned,
        TripDispatchStatuses.Dispatched,
        TripDispatchStatuses.InProgress,
    };

    public static void EnsureReportableTripStatus(string dispatchStatus)
    {
        if (!ReportableTripStatuses.Contains(dispatchStatus))
        {
            throw new StlApiException(
                "driver_portal.exception.trip_not_reportable",
                "Exceptions can only be reported on assigned, dispatched, or in-progress trips.",
                400);
        }
    }

    public static string MapExceptionTypeToCategory(string? exceptionType)
    {
        var normalized = string.IsNullOrWhiteSpace(exceptionType)
            ? Other
            : exceptionType.Trim().ToLowerInvariant();

        if (!ExceptionTypes.Contains(normalized))
        {
            throw new StlApiException(
                "driver_portal.exception.invalid_type",
                "Exception type is not valid.",
                400);
        }

        return normalized switch
        {
            TrafficDelay => DispatchExceptionCategories.Delay,
            EquipmentIssue => DispatchExceptionCategories.Vehicle,
            CustomerAccess => DispatchExceptionCategories.Stop,
            RouteIssue => DispatchExceptionCategories.Route,
            _ => DispatchExceptionCategories.Other,
        };
    }

    public static string BuildDriverReportedDescription(string description) =>
        string.IsNullOrWhiteSpace(description)
            ? "[Driver-reported]"
            : $"[Driver-reported] {description.Trim()}";
}
