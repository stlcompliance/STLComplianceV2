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

    public static DateTimeOffset ComputeDefaultSlaDueAt(string category, DateTimeOffset createdAt)
    {
        var hours = category switch
        {
            ProcurementExceptionCategories.ApprovalDelay => 48,
            ProcurementExceptionCategories.VendorIssue => 72,
            ProcurementExceptionCategories.BudgetOverride => 24,
            ProcurementExceptionCategories.PolicyViolation => 24,
            ProcurementExceptionCategories.PricingVariance => 48,
            _ => 72,
        };

        return createdAt.AddHours(hours);
    }

    public static DateTimeOffset? NormalizeSlaDueAt(DateTimeOffset? slaDueAt, string category, DateTimeOffset createdAt) =>
        slaDueAt ?? ComputeDefaultSlaDueAt(category, createdAt);

    public static string NormalizeResolutionTemplateKey(string? templateKey)
    {
        if (string.IsNullOrWhiteSpace(templateKey))
        {
            return string.Empty;
        }

        var normalized = templateKey.Trim().ToLowerInvariant();
        if (!ProcurementExceptionResolutionTemplates.Keys.Contains(normalized))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "procurement_exceptions.invalid_resolution_template",
                "Resolution template key is not supported.",
                400);
        }

        return normalized;
    }

    public static string BuildResolutionNotes(string? templateKey, string resolutionNotes)
    {
        var normalizedNotes = NormalizeResolutionNotes(resolutionNotes);
        if (string.IsNullOrWhiteSpace(templateKey))
        {
            return normalizedNotes;
        }

        var template = ProcurementExceptionResolutionTemplates.All
            .First(x => string.Equals(x.TemplateKey, templateKey, StringComparison.OrdinalIgnoreCase));

        return $"{template.Label}: {template.DefaultResolutionNotes} — {normalizedNotes}";
    }

    public static bool IsSlaBreached(ProcurementException entity, DateTimeOffset asOfUtc) =>
        entity.SlaDueAt is not null
        && ProcurementExceptionStatuses.Active.Contains(entity.Status)
        && asOfUtc > entity.SlaDueAt.Value;

    public static void EnsureAssignee(Guid assignedToUserId)
    {
        if (assignedToUserId == Guid.Empty)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "procurement_exceptions.invalid_assignee",
                "Assigned resolver user id is required.",
                400);
        }
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
            (ProcurementExceptionStatuses.Cancelled, ProcurementExceptionStatuses.Investigating) => true,
            _ => false,
        };

    public static string NormalizeReopenReason(string reason)
    {
        var normalized = reason.Trim();
        if (normalized.Length is < 10 or > 512)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "procurement_exceptions.invalid_reopen_reason",
                "Reopen reason must be between 10 and 512 characters.",
                400);
        }

        return normalized;
    }
}
