using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public static class ProcurementExceptionEscalationRules
{
    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 50, 1, 500);

    public static int NormalizeCooldownHours(int? hours) =>
        Math.Clamp(hours ?? ProcurementExceptionEscalationDefaults.EscalationCooldownHours, 1, 168);

    public static int NormalizeMaxEscalations(int? maxEscalations) =>
        Math.Clamp(maxEscalations ?? ProcurementExceptionEscalationDefaults.MaxEscalationsPerException, 1, 50);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 10, 1, 100);

    public static int NormalizeEventListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static bool IsActiveStatus(string status) =>
        ProcurementExceptionStatuses.Active.Contains(status);

    public static bool IsSlaBreached(ProcurementException entity, DateTimeOffset asOfUtc) =>
        entity.SlaDueAt is not null
        && IsActiveStatus(entity.Status)
        && asOfUtc > entity.SlaDueAt.Value;

    public static bool IsDueForEscalation(
        ProcurementException entity,
        TenantProcurementExceptionEscalationSettingsSnapshot settings,
        DateTimeOffset asOfUtc)
    {
        if (!settings.IsEnabled || !IsSlaBreached(entity, asOfUtc))
        {
            return false;
        }

        if (entity.EscalationCount >= settings.MaxEscalationsPerException)
        {
            return false;
        }

        if (entity.LastEscalatedAt is null)
        {
            return true;
        }

        return asOfUtc >= entity.LastEscalatedAt.Value.AddHours(settings.EscalationCooldownHours);
    }

    public static double ComputeHoursOverdue(DateTimeOffset? slaDueAt, DateTimeOffset asOfUtc) =>
        slaDueAt is null ? 0 : Math.Max(0, (asOfUtc - slaDueAt.Value).TotalHours);

    public static double? ComputeHoursUntilNextEscalation(
        ProcurementException entity,
        TenantProcurementExceptionEscalationSettingsSnapshot settings,
        DateTimeOffset asOfUtc)
    {
        if (!settings.IsEnabled || !IsSlaBreached(entity, asOfUtc))
        {
            return null;
        }

        if (entity.EscalationCount >= settings.MaxEscalationsPerException)
        {
            return null;
        }

        var nextDueAt = entity.LastEscalatedAt is null
            ? entity.SlaDueAt
            : entity.LastEscalatedAt.Value.AddHours(settings.EscalationCooldownHours);

        if (nextDueAt is null)
        {
            return null;
        }

        var remaining = (nextDueAt.Value - asOfUtc).TotalHours;
        return remaining <= 0 ? 0 : remaining;
    }

    public static bool ShouldNotify(TenantProcurementExceptionEscalationSettingsSnapshot settings) =>
        settings.NotifyOnProcurementExceptionSlaEscalation;
}

public sealed record TenantProcurementExceptionEscalationSettingsSnapshot(
    bool IsEnabled,
    int EscalationCooldownHours,
    int MaxEscalationsPerException,
    bool NotifyOnProcurementExceptionSlaEscalation,
    bool AutoCloseCompletedExceptionsEnabled,
    int AutoCloseCompletedExceptionsAfterHours);
