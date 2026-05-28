using TrainArr.Api.Services;

namespace STLCompliance.Shared.Worker.Tests;

public class StaffarrPublicationRulesTests
{
    [Theory]
    [InlineData(null, 25)]
    [InlineData(0, 1)]
    [InlineData(500, 200)]
    public void NormalizeBatchSize_clamps(int? input, int expected) =>
        Assert.Equal(expected, StaffarrPublicationRules.NormalizeBatchSize(input));

    [Theory]
    [InlineData(null, 10)]
    [InlineData(0, 1)]
    [InlineData(100, 50)]
    public void NormalizeMaxAttempts_clamps(int? input, int expected) =>
        Assert.Equal(expected, StaffarrPublicationRules.NormalizeMaxAttempts(input));

    [Theory]
    [InlineData(null, 5)]
    [InlineData(0, 1)]
    [InlineData(5000, 1440)]
    public void NormalizeRetryIntervalMinutes_clamps(int? input, int expected) =>
        Assert.Equal(expected, StaffarrPublicationRules.NormalizeRetryIntervalMinutes(input));

    [Fact]
    public void ShouldRetryForTenant_defaults_to_enabled_when_settings_missing() =>
        Assert.True(StaffarrPublicationRules.ShouldRetryForTenant(null));

    [Fact]
    public void ShouldRetryForTenant_respects_disabled_tenant() =>
        Assert.False(StaffarrPublicationRules.ShouldRetryForTenant(new TenantStaffarrPublicationSettingsSnapshot(false, 10, 5)));
}
