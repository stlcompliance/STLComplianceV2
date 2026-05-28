using SupplyArr.Api.Contracts;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class DemandProcessingRulesTests
{
    [Fact]
    public void IsDueForProcessing_returns_true_when_never_processed_and_min_hours_elapsed()
    {
        var receivedAt = DateTimeOffset.UtcNow.AddHours(-2);
        var asOf = DateTimeOffset.UtcNow;

        Assert.True(DemandProcessingRules.IsDueForProcessing(
            receivedAt,
            lastProcessedAt: null,
            minHoursBeforeProcessing: 0,
            stalenessHours: 4,
            asOf));
    }

    [Fact]
    public void IsDueForProcessing_returns_false_before_min_hours()
    {
        var receivedAt = DateTimeOffset.UtcNow.AddMinutes(-30);
        var asOf = DateTimeOffset.UtcNow;

        Assert.False(DemandProcessingRules.IsDueForProcessing(
            receivedAt,
            lastProcessedAt: null,
            minHoursBeforeProcessing: 2,
            stalenessHours: 4,
            asOf));
    }

    [Fact]
    public void IsDueForProcessing_returns_false_when_recently_processed()
    {
        var receivedAt = DateTimeOffset.UtcNow.AddDays(-1);
        var lastProcessedAt = DateTimeOffset.UtcNow.AddHours(-1);
        var asOf = DateTimeOffset.UtcNow;

        Assert.False(DemandProcessingRules.IsDueForProcessing(
            receivedAt,
            lastProcessedAt,
            minHoursBeforeProcessing: 0,
            stalenessHours: 4,
            asOf));
    }

    [Fact]
    public void ResolveOutcome_returns_stock_short_when_lines_are_short()
    {
        var (outcome, action) = DemandProcessingRules.ResolveOutcome(2, 2, 1);
        Assert.Equal(DemandProcessingOutcomes.StockShort, outcome);
        Assert.Equal(DemandProcessingRecommendedActions.CreatePurchaseRequest, action);
    }

    [Fact]
    public void ResolveOutcome_returns_stock_available_when_all_catalog_lines_covered()
    {
        var (outcome, action) = DemandProcessingRules.ResolveOutcome(2, 2, 0);
        Assert.Equal(DemandProcessingOutcomes.StockAvailable, outcome);
        Assert.Equal(DemandProcessingRecommendedActions.FulfillFromStock, action);
    }

    [Fact]
    public void IsSourceEnabled_respects_per_source_flags()
    {
        var settings = new TenantDemandProcessingSettingsSnapshot(
            IsEnabled: true,
            AutoCreatePrDraftWhenShort: false,
            MinHoursBeforeProcessing: 0,
            StalenessHours: 4,
            NotifyOnPrDraftCreated: false,
            ProcessMaintainarrDemandRefs: true,
            ProcessRoutarrDemandRefs: false,
            ProcessTrainarrDemandRefs: true,
            ProcessStaffarrDemandRefs: false);

        Assert.True(settings.IsSourceEnabled(DemandRefSources.MaintainArr));
        Assert.False(settings.IsSourceEnabled(DemandRefSources.RoutArr));
        Assert.True(settings.IsSourceEnabled(DemandRefSources.TrainArr));
        Assert.False(settings.IsSourceEnabled("unknown"));
    }

    [Fact]
    public void ResolveOutcome_returns_no_catalog_parts_when_none_linked()
    {
        var (outcome, action) = DemandProcessingRules.ResolveOutcome(2, 0, 0);
        Assert.Equal(DemandProcessingOutcomes.NoCatalogParts, outcome);
        Assert.Equal(DemandProcessingRecommendedActions.ReviewManually, action);
    }

    [Fact]
    public void ValidateSettings_rejects_enabled_worker_without_sources()
    {
        var ex = Assert.Throws<StlApiException>(() =>
            DemandProcessingRules.ValidateSettings(new UpsertDemandProcessingSettingsRequest(
                true,
                false,
                0,
                4,
                true,
                false,
                false,
                false,
                false)));

        Assert.Equal("demand_processing_settings.no_sources", ex.Code);
    }
}
