using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class EntitlementReconciliationRulesTests
{
    private static readonly DateTimeOffset AsOf = new(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsLicenseCurrentlyValid_requires_active_status_and_date_window()
    {
        Assert.True(EntitlementReconciliationRules.IsLicenseCurrentlyValid(
            "Active",
            AsOf.AddDays(-1),
            AsOf.AddDays(1),
            AsOf));

        Assert.False(EntitlementReconciliationRules.IsLicenseCurrentlyValid(
            "Revoked",
            AsOf.AddDays(-1),
            AsOf.AddDays(1),
            AsOf));

        Assert.False(EntitlementReconciliationRules.IsLicenseCurrentlyValid(
            "Active",
            AsOf.AddDays(1),
            AsOf.AddDays(2),
            AsOf));

        Assert.False(EntitlementReconciliationRules.IsLicenseCurrentlyValid(
            "Active",
            AsOf.AddDays(-10),
            AsOf.AddDays(-1),
            AsOf));
    }

    [Theory]
    [InlineData(true, true, true, true, "none")]
    [InlineData(true, true, true, false, "stale_entitlement")]
    [InlineData(true, true, false, true, "missing_entitlement")]
    [InlineData(false, true, true, true, "suspended_tenant")]
    [InlineData(true, false, true, true, "inactive_product")]
    public void ResolveDriftKind_maps_expected_values(
        bool tenantActive,
        bool productActive,
        bool entitlementActive,
        bool licenseValid,
        string expected)
    {
        var actual = EntitlementReconciliationRules.ResolveDriftKind(
            tenantActive,
            productActive,
            entitlementActive,
            licenseValid);

        Assert.Equal(expected, actual);
    }
}
