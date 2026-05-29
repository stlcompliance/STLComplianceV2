using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public static class StaffArrWorkerAdminRules
{
    public static string NormalizeWorkerKey(string workerKey)
    {
        if (!StaffArrWorkerKeys.All.Contains(workerKey))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "staffarr_worker.unknown_key",
                $"Unknown StaffArr worker key '{workerKey}'.",
                400);
        }

        return StaffArrWorkerKeys.All.First(x => string.Equals(x, workerKey, StringComparison.OrdinalIgnoreCase));
    }

    public static int NormalizeScanIntervalMinutes(int? minutes) =>
        minutes is null or < 1 or > 24 * 60 ? StaffArrWorkerSettingsDefaults.ScanIntervalMinutes : minutes.Value;

    public static int NormalizeBatchSize(int? batchSize, string workerKey) =>
        batchSize is null or < 1 or > 500 ? DefaultBatchSize(workerKey) : batchSize.Value;

    public static int? NormalizeStalenessHours(int? stalenessHours, string workerKey)
    {
        if (!SupportsStaleness(workerKey))
        {
            return null;
        }

        return stalenessHours is null or < 1 or > 168
            ? StaffArrWorkerSettingsDefaults.StalenessHours
            : stalenessHours.Value;
    }

    public static bool SupportsStaleness(string workerKey) =>
        workerKey is StaffArrWorkerKeys.ReadinessRollup
            or StaffArrWorkerKeys.PermissionProjection
            or StaffArrWorkerKeys.PersonnelHistoryRollup;

    public static int DefaultScanIntervalMinutes(string workerKey) => workerKey switch
    {
        StaffArrWorkerKeys.CertificationExpiration => 15,
        StaffArrWorkerKeys.AuditPackageGeneration => 2,
        _ => StaffArrWorkerSettingsDefaults.ScanIntervalMinutes,
    };

    public static int DefaultBatchSize(string workerKey) => workerKey switch
    {
        StaffArrWorkerKeys.CertificationExpiration => 100,
        StaffArrWorkerKeys.PermissionProjection => 100,
        StaffArrWorkerKeys.PersonnelHistoryRollup => 100,
        StaffArrWorkerKeys.AuditPackageGeneration => 5,
        _ => StaffArrWorkerSettingsDefaults.BatchSize,
    };

    public static int DefaultStalenessHours(string workerKey) =>
        SupportsStaleness(workerKey) ? StaffArrWorkerSettingsDefaults.StalenessHours : 0;

    public static int NormalizeRunListLimit(int? limit) =>
        limit is null or < 1 or > 50 ? 5 : limit.Value;
}
