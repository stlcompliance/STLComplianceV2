using MaintainArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintenancePlatformEventRulesTests
{
    [Fact]
    public void HasReadinessTransition_detects_readiness_change()
    {
        Assert.True(MaintenancePlatformEventRules.HasReadinessTransition(
            "ready",
            "not_ready",
            "active",
            "active"));
    }

    [Fact]
    public void HasReadinessTransition_detects_lifecycle_change()
    {
        Assert.True(MaintenancePlatformEventRules.HasReadinessTransition(
            "ready",
            "ready",
            "active",
            "out_of_service"));
    }

    [Fact]
    public void HasReadinessTransition_ignores_unchanged_state()
    {
        Assert.False(MaintenancePlatformEventRules.HasReadinessTransition(
            "ready",
            "ready",
            "active",
            "active"));
    }

    [Fact]
    public void IsOutOfServiceTransition_detects_oos_entry()
    {
        Assert.True(MaintenancePlatformEventRules.IsOutOfServiceTransition("active", "out_of_service"));
        Assert.False(MaintenancePlatformEventRules.IsOutOfServiceTransition("out_of_service", "out_of_service"));
    }

    [Fact]
    public void IsReturnedToServiceTransition_detects_oos_exit()
    {
        Assert.True(MaintenancePlatformEventRules.IsReturnedToServiceTransition("out_of_service", "active"));
        Assert.False(MaintenancePlatformEventRules.IsReturnedToServiceTransition("active", "active"));
    }

    [Fact]
    public void BuildReadinessChangedIdempotencyKey_is_stable()
    {
        var assetId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var key = MaintenancePlatformEventRules.BuildReadinessChangedIdempotencyKey(
            assetId,
            "ready",
            "not_ready",
            "active",
            "active");

        Assert.Contains(assetId.ToString("D"), key, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ready>not_ready", key, StringComparison.OrdinalIgnoreCase);
    }
}
