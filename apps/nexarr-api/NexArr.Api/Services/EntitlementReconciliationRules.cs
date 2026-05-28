namespace NexArr.Api.Services;

public static class EntitlementReconciliationRules
{
    public const int DefaultBatchSize = 50;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? DefaultBatchSize, 1, 200);

    public static int NormalizeRunListLimit(int? limit) =>
        Math.Clamp(limit ?? 20, 1, 100);

    public static bool IsLicenseCurrentlyValid(
        string licenseStatus,
        DateTimeOffset validFrom,
        DateTimeOffset? validTo,
        DateTimeOffset asOfUtc) =>
        string.Equals(licenseStatus, Entities.LicenseStatuses.Active, StringComparison.Ordinal)
        && validFrom <= asOfUtc
        && (validTo is null || validTo.Value > asOfUtc);

    public static string ResolveDriftKind(
        bool tenantActive,
        bool productActive,
        bool entitlementActive,
        bool licenseValid) =>
        !tenantActive && entitlementActive
            ? "suspended_tenant"
            : entitlementActive && !productActive
                ? "inactive_product"
                : entitlementActive && !licenseValid
                    ? "stale_entitlement"
                    : !entitlementActive && licenseValid
                        ? "missing_entitlement"
                        : "none";
}
