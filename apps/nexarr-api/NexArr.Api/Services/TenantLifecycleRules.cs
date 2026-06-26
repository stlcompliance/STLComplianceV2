using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public static class TenantLifecycleRules
{
    public const int DefaultBatchSize = 25;
    public const int DefaultSuspendGraceDays = 7;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? DefaultBatchSize, 1, 100);

    public static int NormalizeSuspendGraceDays(int? graceDays) =>
        Math.Clamp(graceDays ?? DefaultSuspendGraceDays, 0, 365);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static bool HasAnyValidLicense(
        IEnumerable<TenantProductLicense> licenses,
        DateTimeOffset asOfUtc) =>
        licenses.Any(l => LaunchDestinationReconciliationRules.IsLicenseCurrentlyValid(
            l.Status,
            l.ValidFrom,
            l.ValidTo,
            asOfUtc));

    public static DateTimeOffset ResolveCoverageBaseline(
        Tenant tenant,
        IReadOnlyList<TenantProductLicense> licenses,
        DateTimeOffset asOfUtc)
    {
        if (licenses.Count == 0)
        {
            return tenant.CreatedAt;
        }

        var coverageEnds = licenses
            .Select(l => ResolveLicenseCoverageEndedAt(l, asOfUtc))
            .ToList();

        return coverageEnds.Max();
    }

    public static DateTimeOffset ResolveLicenseCoverageEndedAt(
        TenantProductLicense license,
        DateTimeOffset asOfUtc)
    {
        if (LaunchDestinationReconciliationRules.IsLicenseCurrentlyValid(
                license.Status,
                license.ValidFrom,
                license.ValidTo,
                asOfUtc))
        {
            return license.ValidTo ?? asOfUtc;
        }

        return license.ValidTo ?? license.ValidFrom;
    }

    public static string ResolvePendingActionKind(
        string tenantStatus,
        bool hasValidLicense,
        DateTimeOffset coverageBaseline,
        DateTimeOffset asOfUtc,
        int suspendGraceDays,
        bool autoSuspendWhenNoValidLicense,
        bool autoReactivateWhenValidLicense)
    {
        if (string.Equals(tenantStatus, TenantStatuses.Active, StringComparison.Ordinal)
            && autoSuspendWhenNoValidLicense
            && !hasValidLicense)
        {
            var eligibleAt = coverageBaseline.AddDays(suspendGraceDays);
            return asOfUtc >= eligibleAt ? "suspend" : "none";
        }

        if (string.Equals(tenantStatus, TenantStatuses.Suspended, StringComparison.Ordinal)
            && autoReactivateWhenValidLicense
            && hasValidLicense)
        {
            return "reactivate";
        }

        return "none";
    }
}
