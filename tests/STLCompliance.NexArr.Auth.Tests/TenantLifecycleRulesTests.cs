using NexArr.Api.Entities;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class TenantLifecycleRulesTests
{
    [Fact]
    public void HasAnyValidLicense_returns_true_when_one_license_is_active()
    {
        var asOf = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);
        var licenses = new[]
        {
            new TenantProductLicense
            {
                Status = LicenseStatuses.Active,
                ValidFrom = asOf.AddDays(-10),
                ValidTo = asOf.AddDays(30),
            },
            new TenantProductLicense
            {
                Status = LicenseStatuses.Expired,
                ValidFrom = asOf.AddYears(-2),
                ValidTo = asOf.AddDays(-1),
            },
        };

        Assert.True(TenantLifecycleRules.HasAnyValidLicense(licenses, asOf));
    }

    [Fact]
    public void ResolvePendingActionKind_returns_suspend_after_grace_when_no_valid_license()
    {
        var asOf = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);
        var baseline = asOf.AddDays(-10);

        var action = TenantLifecycleRules.ResolvePendingActionKind(
            TenantStatuses.Active,
            hasValidLicense: false,
            coverageBaseline: baseline,
            asOfUtc: asOf,
            suspendGraceDays: 7,
            autoSuspendWhenNoValidLicense: true,
            autoReactivateWhenValidLicense: true);

        Assert.Equal("suspend", action);
    }

    [Fact]
    public void ResolvePendingActionKind_returns_none_before_grace_elapses()
    {
        var asOf = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero);
        var baseline = asOf.AddDays(-3);

        var action = TenantLifecycleRules.ResolvePendingActionKind(
            TenantStatuses.Active,
            hasValidLicense: false,
            coverageBaseline: baseline,
            asOfUtc: asOf,
            suspendGraceDays: 7,
            autoSuspendWhenNoValidLicense: true,
            autoReactivateWhenValidLicense: true);

        Assert.Equal("none", action);
    }

    [Fact]
    public void ResolvePendingActionKind_returns_reactivate_for_suspended_tenant_with_license()
    {
        var asOf = DateTimeOffset.UtcNow;

        var action = TenantLifecycleRules.ResolvePendingActionKind(
            TenantStatuses.Suspended,
            hasValidLicense: true,
            coverageBaseline: asOf,
            asOfUtc: asOf,
            suspendGraceDays: 7,
            autoSuspendWhenNoValidLicense: true,
            autoReactivateWhenValidLicense: true);

        Assert.Equal("reactivate", action);
    }
}
