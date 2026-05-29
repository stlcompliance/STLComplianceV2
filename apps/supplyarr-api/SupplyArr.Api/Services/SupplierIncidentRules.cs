using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class SupplierIncidentRules
{
    public static string NormalizeIncidentKey(string incidentKey)
    {
        var normalized = incidentKey.Trim();
        if (normalized.Length is < 2 or > 64)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "supplier_incidents.invalid_key",
                "Incident key must be between 2 and 64 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeTitle(string title)
    {
        var normalized = title.Trim();
        if (normalized.Length is < 3 or > 256)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "supplier_incidents.invalid_title",
                "Title must be between 3 and 256 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeDescription(string description) =>
        string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim()[..Math.Min(description.Trim().Length, 2048)];

    public static string NormalizeIncidentType(string incidentType)
    {
        var normalized = incidentType.Trim().ToLowerInvariant();
        if (!SupplierIncidentTypes.All.Contains(normalized))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "supplier_incidents.invalid_type",
                "Incident type is not supported.",
                400);
        }

        return normalized;
    }

    public static string NormalizeSeverity(string severity)
    {
        var normalized = severity.Trim().ToLowerInvariant();
        if (!SupplierIncidentSeverities.All.Contains(normalized))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "supplier_incidents.invalid_severity",
                "Severity is not supported.",
                400);
        }

        return normalized;
    }

    public static string NormalizeReopenReason(string reason)
    {
        var normalized = reason.Trim();
        if (normalized.Length is < 10 or > 512)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "supplier_incidents.invalid_reopen_reason",
                "Reopen reason must be between 10 and 512 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeCancellationReason(string reason)
    {
        var normalized = reason.Trim();
        if (normalized.Length is < 3 or > 512)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "supplier_incidents.invalid_cancellation_reason",
                "Cancellation reason must be between 3 and 512 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeResolutionNotes(string notes)
    {
        var normalized = notes.Trim();
        if (normalized.Length is < 3 or > 2048)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "supplier_incidents.invalid_resolution_notes",
                "Resolution notes must be between 3 and 2048 characters.",
                400);
        }

        return normalized;
    }

    public static bool CanTransition(string currentStatus, string targetStatus) =>
        (currentStatus, targetStatus) switch
        {
            (SupplierIncidentStatuses.Open, SupplierIncidentStatuses.Investigating) => true,
            (SupplierIncidentStatuses.Open, SupplierIncidentStatuses.Cancelled) => true,
            (SupplierIncidentStatuses.Investigating, SupplierIncidentStatuses.Resolved) => true,
            (SupplierIncidentStatuses.Investigating, SupplierIncidentStatuses.Cancelled) => true,
            (SupplierIncidentStatuses.Resolved, SupplierIncidentStatuses.Closed) => true,
            (SupplierIncidentStatuses.Cancelled, SupplierIncidentStatuses.Investigating) => true,
            _ => false,
        };
}
