using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public static class DispatchExceptionRules
{
    public const int MaxBatchItems = 50;

    public static DateTimeOffset ComputeDefaultSlaDueAt(string category, DateTimeOffset createdAt)
    {
        var hours = category switch
        {
            DispatchExceptionCategories.Delay => 2,
            DispatchExceptionCategories.Driver => 4,
            DispatchExceptionCategories.Vehicle => 4,
            DispatchExceptionCategories.Route => 8,
            DispatchExceptionCategories.Stop => 4,
            DispatchExceptionCategories.Compliance => 24,
            _ => 8,
        };

        return createdAt.AddHours(hours);
    }

    public static DateTimeOffset? NormalizeSlaDueAt(
        DateTimeOffset? slaDueAt,
        string category,
        DateTimeOffset createdAt) =>
        slaDueAt ?? ComputeDefaultSlaDueAt(category, createdAt);

    public static string NormalizeResolutionTemplateKey(string? templateKey)
    {
        if (string.IsNullOrWhiteSpace(templateKey))
        {
            return string.Empty;
        }

        var normalized = templateKey.Trim().ToLowerInvariant();
        if (!DispatchExceptionResolutionTemplates.Keys.Contains(normalized))
        {
            throw new StlApiException(
                "dispatch_exception.invalid_resolution_template",
                "Resolution template key is not supported.",
                400);
        }

        return normalized;
    }

    public static string BuildResolutionNotes(string? templateKey, string? resolutionNotes)
    {
        var notes = resolutionNotes?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(templateKey))
        {
            return notes;
        }

        var template = DispatchExceptionResolutionTemplates.All
            .First(x => string.Equals(x.TemplateKey, templateKey, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(notes))
        {
            return $"{template.Label}: {template.DefaultResolutionNotes}";
        }

        return $"{template.Label}: {template.DefaultResolutionNotes} — {notes}";
    }

    public static bool IsSlaBreached(DispatchException entity, DateTimeOffset asOfUtc) =>
        entity.SlaDueAt is not null
        && DispatchExceptionStatuses.OpenQueue.Contains(entity.Status)
        && asOfUtc > entity.SlaDueAt.Value;

    public static void EnsureAssignee(Guid assignedToUserId)
    {
        if (assignedToUserId == Guid.Empty)
        {
            throw new StlApiException(
                "dispatch_exception.assignee_required",
                "Assignee user id is required.",
                400);
        }
    }

    public static IReadOnlyList<Guid> ValidateBulkExceptionIds(IReadOnlyList<Guid>? exceptionIds)
    {
        if (exceptionIds is null || exceptionIds.Count == 0)
        {
            throw new StlApiException(
                "dispatch_exception.bulk_ids_required",
                "At least one exception id is required.",
                400);
        }

        if (exceptionIds.Count > MaxBatchItems)
        {
            throw new StlApiException(
                "dispatch_exception.bulk_too_many",
                $"Bulk exception actions are limited to {MaxBatchItems} items per request.",
                400);
        }

        if (exceptionIds.Any(x => x == Guid.Empty))
        {
            throw new StlApiException(
                "dispatch_exception.bulk_invalid_id",
                "Exception ids must be valid GUIDs.",
                400);
        }

        return exceptionIds.Distinct().ToList();
    }
}
