using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class ProcurementExceptionAutomationRules
{
    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeAutoCloseCompletedExceptionsAfterHours(int? hours) =>
        Math.Clamp(
            hours ?? ProcurementExceptionEscalationDefaults.AutoCloseCompletedExceptionsAfterHours,
            1,
            720);

    public static bool IsCompletedStatus(string status) =>
        string.Equals(status, ProcurementExceptionStatuses.Resolved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, ProcurementExceptionStatuses.Waived, StringComparison.OrdinalIgnoreCase);

    public static DateTimeOffset? GetCompletedAt(ProcurementException entity) =>
        entity.ResolvedAt ?? entity.WaivedAt;

    public static bool IsDueForAutoClose(
        ProcurementException entity,
        TenantProcurementExceptionEscalationSettingsSnapshot settings,
        DateTimeOffset asOfUtc)
    {
        if (!settings.AutoCloseCompletedExceptionsEnabled || entity.ClosedAt is not null)
        {
            return false;
        }

        if (!IsCompletedStatus(entity.Status))
        {
            return false;
        }

        var completedAt = GetCompletedAt(entity);
        return completedAt is not null
            && asOfUtc >= completedAt.Value.AddHours(settings.AutoCloseCompletedExceptionsAfterHours);
    }

    public static double ComputeHoursCompleted(DateTimeOffset? completedAt, DateTimeOffset asOfUtc) =>
        completedAt is null ? 0 : Math.Max(0, (asOfUtc - completedAt.Value).TotalHours);

    public static double? ComputeHoursUntilAutoClose(
        ProcurementException entity,
        TenantProcurementExceptionEscalationSettingsSnapshot settings,
        DateTimeOffset asOfUtc)
    {
        if (!settings.AutoCloseCompletedExceptionsEnabled)
        {
            return null;
        }

        var completedAt = GetCompletedAt(entity);
        if (completedAt is null || entity.ClosedAt is not null || !IsCompletedStatus(entity.Status))
        {
            return null;
        }

        var remaining = completedAt.Value.AddHours(settings.AutoCloseCompletedExceptionsAfterHours) - asOfUtc;
        return remaining.TotalHours <= 0 ? 0 : remaining.TotalHours;
    }
}
