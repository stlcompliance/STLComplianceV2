using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public static class DefectEscalationRules
{
    public const int MinThresholdHours = 1;

    public const int MaxThresholdHours = 720;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 25, 1, 200);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static int NormalizeEventListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static int NormalizeThresholdHours(int? hours) =>
        Math.Clamp(hours ?? DefectEscalationDefaults.MediumThresholdHours, MinThresholdHours, MaxThresholdHours);

    public static bool IsEscalatableStatus(string status) =>
        string.Equals(status, DefectStatuses.Open, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, DefectStatuses.Acknowledged, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, DefectStatuses.InRepair, StringComparison.OrdinalIgnoreCase);

    public static int GetThresholdHours(TenantDefectEscalationSettingsSnapshot settings, string severity) =>
        severity.Trim().ToLowerInvariant() switch
        {
            DefectSeverities.Low => settings.LowThresholdHours,
            DefectSeverities.Medium => settings.MediumThresholdHours,
            DefectSeverities.High => settings.HighThresholdHours,
            DefectSeverities.Critical => settings.CriticalThresholdHours,
            _ => settings.MediumThresholdHours,
        };

    public static DateTimeOffset GetStagnationAnchor(Defect defect) =>
        defect.LastEscalatedAt ?? defect.UpdatedAt;

    public static bool IsDueForEscalation(
        Defect defect,
        TenantDefectEscalationSettingsSnapshot settings,
        DateTimeOffset asOfUtc)
    {
        if (!settings.IsEnabled || !IsEscalatableStatus(defect.Status))
        {
            return false;
        }

        var thresholdHours = GetThresholdHours(settings, defect.Severity);
        var stagnation = asOfUtc - GetStagnationAnchor(defect);
        return stagnation >= TimeSpan.FromHours(thresholdHours);
    }

    public static string? BumpSeverity(string currentSeverity)
    {
        return currentSeverity.Trim().ToLowerInvariant() switch
        {
            DefectSeverities.Low => DefectSeverities.Medium,
            DefectSeverities.Medium => DefectSeverities.High,
            DefectSeverities.High => DefectSeverities.Critical,
            _ => null,
        };
    }

    public static bool ShouldAutoAcknowledge(
        TenantDefectEscalationSettingsSnapshot settings,
        Defect defect) =>
        settings.AutoAcknowledgeOnEscalation
        && string.Equals(defect.Status, DefectStatuses.Open, StringComparison.OrdinalIgnoreCase);

    public static bool ShouldBumpSeverity(
        TenantDefectEscalationSettingsSnapshot settings,
        Defect defect) =>
        settings.BumpSeverityOnRepeatEscalation
        && defect.EscalationCount > 0
        && BumpSeverity(defect.Severity) is not null;
}

public sealed record TenantDefectEscalationSettingsSnapshot(
    bool IsEnabled,
    int LowThresholdHours,
    int MediumThresholdHours,
    int HighThresholdHours,
    int CriticalThresholdHours,
    bool AutoAcknowledgeOnEscalation,
    bool AutoCreateWorkOrderOnEscalation,
    bool BumpSeverityOnRepeatEscalation,
    bool NotifyOnEscalation);
