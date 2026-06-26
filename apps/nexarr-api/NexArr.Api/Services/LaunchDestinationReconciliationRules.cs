namespace NexArr.Api.Services;

public static class LaunchDestinationReconciliationRules
{
    public const int DefaultBatchSize = 50;
    public const string NoDrift = "none";
    public const string SuspendedTenantDrift = "suspended_tenant";
    public const string InactiveProductDrift = "inactive_product";
    public const string StaleLaunchDestinationDrift = "stale_launch_destination";
    public const string MissingLaunchDestinationDrift = "missing_launch_destination";
    public const string LegacyStaleEntitlementDrift = "stale_entitlement";
    public const string LegacyMissingEntitlementDrift = "missing_entitlement";

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
        bool launchDestinationActive,
        bool licenseValid) =>
        !tenantActive && launchDestinationActive
            ? SuspendedTenantDrift
            : launchDestinationActive && !productActive
                ? InactiveProductDrift
                : launchDestinationActive && !licenseValid
                    ? StaleLaunchDestinationDrift
                    : !launchDestinationActive && licenseValid
                        ? MissingLaunchDestinationDrift
                        : NoDrift;

    public static string NormalizeDriftKind(string driftKind) =>
        driftKind switch
        {
            LegacyStaleEntitlementDrift => StaleLaunchDestinationDrift,
            LegacyMissingEntitlementDrift => MissingLaunchDestinationDrift,
            _ => driftKind,
        };
}

public static class CompatibilityLegacyEntitlementReconciliationRules
{
    public const int DefaultBatchSize = LaunchDestinationReconciliationRules.DefaultBatchSize;
    public const string NoDrift = LaunchDestinationReconciliationRules.NoDrift;
    public const string SuspendedTenantDrift = LaunchDestinationReconciliationRules.SuspendedTenantDrift;
    public const string InactiveProductDrift = LaunchDestinationReconciliationRules.InactiveProductDrift;
    public const string StaleLaunchDestinationDrift = LaunchDestinationReconciliationRules.StaleLaunchDestinationDrift;
    public const string MissingLaunchDestinationDrift = LaunchDestinationReconciliationRules.MissingLaunchDestinationDrift;
    public const string LegacyStaleEntitlementDrift = LaunchDestinationReconciliationRules.LegacyStaleEntitlementDrift;
    public const string LegacyMissingEntitlementDrift = LaunchDestinationReconciliationRules.LegacyMissingEntitlementDrift;

    public static int NormalizeBatchSize(int? batchSize) =>
        LaunchDestinationReconciliationRules.NormalizeBatchSize(batchSize);

    public static int NormalizeRunListLimit(int? limit) =>
        LaunchDestinationReconciliationRules.NormalizeRunListLimit(limit);

    public static bool IsLicenseCurrentlyValid(
        string licenseStatus,
        DateTimeOffset validFrom,
        DateTimeOffset? validTo,
        DateTimeOffset asOfUtc) =>
        LaunchDestinationReconciliationRules.IsLicenseCurrentlyValid(licenseStatus, validFrom, validTo, asOfUtc);

    public static string ResolveDriftKind(
        bool tenantActive,
        bool productActive,
        bool launchDestinationActive,
        bool licenseValid) =>
        LaunchDestinationReconciliationRules.ResolveDriftKind(
            tenantActive,
            productActive,
            launchDestinationActive,
            licenseValid);

    public static string NormalizeDriftKind(string driftKind) =>
        LaunchDestinationReconciliationRules.NormalizeDriftKind(driftKind);
}
