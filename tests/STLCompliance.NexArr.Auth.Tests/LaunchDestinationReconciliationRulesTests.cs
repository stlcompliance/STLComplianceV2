using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class LaunchDestinationReconciliationRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsLicenseCurrentlyValid_requires_active_status_and_date_window()
    {
        Assert.True(LaunchDestinationReconciliationRules.IsLicenseCurrentlyValid(
            "Active",
            AsOf.AddDays(-1),
            AsOf.AddDays(1),
            AsOf));

        Assert.False(LaunchDestinationReconciliationRules.IsLicenseCurrentlyValid(
            "Revoked",
            AsOf.AddDays(-1),
            AsOf.AddDays(1),
            AsOf));

        Assert.False(LaunchDestinationReconciliationRules.IsLicenseCurrentlyValid(
            "Active",
            AsOf.AddDays(1),
            AsOf.AddDays(2),
            AsOf));

        Assert.False(LaunchDestinationReconciliationRules.IsLicenseCurrentlyValid(
            "Active",
            AsOf.AddDays(-10),
            AsOf.AddDays(-1),
            AsOf));
    }

    [Theory]
    [InlineData(true, true, true, true, "none")]
    [InlineData(true, true, true, false, "stale_launch_destination")]
    [InlineData(true, true, false, true, "missing_launch_destination")]
    [InlineData(false, true, true, true, "suspended_tenant")]
    [InlineData(true, false, true, true, "inactive_product")]
    public void ResolveDriftKind_maps_expected_values(
        bool tenantActive,
        bool productActive,
        bool launchDestinationActive,
        bool licenseValid,
        string expected)
    {
        var actual = LaunchDestinationReconciliationRules.ResolveDriftKind(
            tenantActive,
            productActive,
            launchDestinationActive,
            licenseValid);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("stale_entitlement", "stale_launch_destination")]
    [InlineData("missing_entitlement", "missing_launch_destination")]
    [InlineData("inactive_product", "inactive_product")]
    public void NormalizeDriftKind_preserves_canonical_and_maps_legacy_aliases(
        string driftKind,
        string expected)
    {
        Assert.Equal(expected, LaunchDestinationReconciliationRules.NormalizeDriftKind(driftKind));
    }
}
