using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class ProcurementExceptionRules
{
    public static string NormalizeExceptionKey(string exceptionKey)
    {
        var normalized = exceptionKey.Trim();
        if (normalized.Length is < 2 or > 64)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "procurement_exceptions.invalid_key",
                "Exception key must be between 2 and 64 characters.",
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
                "procurement_exceptions.invalid_title",
                "Title must be between 3 and 256 characters.",
                400);
        }

        return normalized;
    }

    public static string NormalizeDescription(string description) =>
        string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim()[..Math.Min(description.Trim().Length, 2048)];

    public static string NormalizeCategory(string category)
    {
        var normalized = category.Trim().ToLowerInvariant();
        if (!ProcurementExceptionCategories.All.Contains(normalized))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "procurement_exceptions.invalid_category",
                "Exception category is not supported.",
                400);
        }

        return normalized;
    }

    public static string NormalizeSubjectType(string subjectType)
    {
        var normalized = subjectType.Trim().ToLowerInvariant();
        if (!ProcurementExceptionSubjectTypes.All.Contains(normalized))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "procurement_exceptions.invalid_subject_type",
                "Subject type must be purchase_request, purchase_order, or rfq.",
                400);
        }

        return normalized;
    }

    public static string NormalizeWaiveJustification(string justification)
    {
        var normalized = justification.Trim();
        if (normalized.Length is < 10 or > 2048)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "procurement_exceptions.invalid_waive_justification",
                "Waive justification must be between 10 and 2048 characters.",
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
                "procurement_exceptions.invalid_resolution_notes",
                "Resolution notes must be between 3 and 2048 characters.",
                400);
        }

        return normalized;
    }

    public static bool CanTransition(string currentStatus, string targetStatus) =>
        (currentStatus, targetStatus) switch
        {
            (ProcurementExceptionStatuses.Open, ProcurementExceptionStatuses.Investigating) => true,
            (ProcurementExceptionStatuses.Open, ProcurementExceptionStatuses.Cancelled) => true,
            (ProcurementExceptionStatuses.Investigating, ProcurementExceptionStatuses.Resolved) => true,
            (ProcurementExceptionStatuses.Investigating, ProcurementExceptionStatuses.WaivePending) => true,
            (ProcurementExceptionStatuses.Investigating, ProcurementExceptionStatuses.Cancelled) => true,
            (ProcurementExceptionStatuses.WaivePending, ProcurementExceptionStatuses.Waived) => true,
            (ProcurementExceptionStatuses.WaivePending, ProcurementExceptionStatuses.Investigating) => true,
            (ProcurementExceptionStatuses.Resolved, ProcurementExceptionStatuses.Closed) => true,
            (ProcurementExceptionStatuses.Waived, ProcurementExceptionStatuses.Closed) => true,
            _ => false,
        };
}
