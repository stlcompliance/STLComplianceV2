using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class TenantLifecycleRulesTests
{
    [Fact]
    public void Normalize_batch_size_clamps_to_safe_worker_bounds()
    {
        Assert.Equal(1, TenantLifecycleRules.NormalizeBatchSize(0));
        Assert.Equal(25, TenantLifecycleRules.NormalizeBatchSize(null));
        Assert.Equal(100, TenantLifecycleRules.NormalizeBatchSize(500));
    }

    [Fact]
    public void Normalize_suspend_grace_days_keeps_retired_setting_bounded_for_legacy_records()
    {
        Assert.Equal(0, TenantLifecycleRules.NormalizeSuspendGraceDays(-5));
        Assert.Equal(7, TenantLifecycleRules.NormalizeSuspendGraceDays(null));
        Assert.Equal(365, TenantLifecycleRules.NormalizeSuspendGraceDays(999));
    }

    [Fact]
    public void Normalize_run_list_limit_clamps_to_safe_history_bounds()
    {
        Assert.Equal(1, TenantLifecycleRules.NormalizeRunListLimit(0));
        Assert.Equal(20, TenantLifecycleRules.NormalizeRunListLimit(null));
        Assert.Equal(100, TenantLifecycleRules.NormalizeRunListLimit(500));
    }
}
