using RoutArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public sealed class IntegrationEventRulesTests
{
    [Theory]
    [InlineData(null, 50)]
    [InlineData(0, 1)]
    [InlineData(1000, 200)]
    public void NormalizeBatchSize_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, IntegrationEventRules.NormalizeBatchSize(input));

    [Theory]
    [InlineData(null, 5)]
    [InlineData(0, 1)]
    [InlineData(100, 20)]
    public void NormalizeMaxAttempts_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, IntegrationEventRules.NormalizeMaxAttempts(input));

    [Theory]
    [InlineData(null, 15)]
    [InlineData(0, 1)]
    [InlineData(5000, 1440)]
    public void NormalizeRetryIntervalMinutes_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, IntegrationEventRules.NormalizeRetryIntervalMinutes(input));

    [Theory]
    [InlineData(null, 50)]
    [InlineData(0, 1)]
    [InlineData(500, 200)]
    public void NormalizeEventListLimit_clamps_to_valid_range(int? input, int expected) =>
        Assert.Equal(expected, IntegrationEventRules.NormalizeEventListLimit(input));

    [Fact]
    public void ShouldProcessForTenant_defaults_to_enabled_when_settings_missing() =>
        Assert.True(IntegrationEventRules.ShouldProcessForTenant(null));

    [Fact]
    public void ShouldProcessForTenant_respects_tenant_disable() =>
        Assert.False(IntegrationEventRules.ShouldProcessForTenant(
            new TenantIntegrationEventSettingsSnapshot(
                IsEnabled: false,
                MaxAttempts: 5,
                RetryIntervalMinutes: 15)));

    [Fact]
    public void BuildOutboxIdempotencyKey_is_stable_and_lowercase()
    {
        var entityId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var key = IntegrationEventRules.BuildOutboxIdempotencyKey(
            "TripCompleted",
            "trip",
            entityId);

        Assert.Equal(
            "outbox:tripcompleted:trip:11111111-1111-1111-1111-111111111111",
            key);
    }

    [Fact]
    public void BuildOutboxIdempotencyKey_includes_suffix_when_provided()
    {
        var entityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var key = IntegrationEventRules.BuildOutboxIdempotencyKey(
            "DriverAssignmentChanged",
            "trip",
            entityId,
            "v2");

        Assert.Equal(
            "outbox:driverassignmentchanged:trip:22222222-2222-2222-2222-222222222222:v2",
            key);
    }

    [Fact]
    public void ComputeNextRetryAt_adds_normalized_retry_interval()
    {
        var now = new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero);
        var next = IntegrationEventRules.ComputeNextRetryAt(now, retryIntervalMinutes: 30);
        Assert.Equal(now.AddMinutes(30), next);
    }
}
