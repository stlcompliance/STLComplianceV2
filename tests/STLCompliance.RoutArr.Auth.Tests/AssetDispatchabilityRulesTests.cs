using RoutArr.Api.Contracts;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class AssetDispatchabilityRulesTests
{
    [Fact]
    public void Maintainarr_not_ready_merges_to_block()
    {
        var outcome = AssetDispatchabilityRules.MergeOutcome("not_ready", assetFound: true);
        Assert.Equal(AssetDispatchabilityOutcomes.Block, outcome);
    }

    [Fact]
    public void Asset_not_found_merges_to_warn()
    {
        var outcome = AssetDispatchabilityRules.MergeOutcome(null, assetFound: false);
        Assert.Equal(AssetDispatchabilityOutcomes.Warn, outcome);
    }

    [Fact]
    public void Ready_asset_merges_to_allow()
    {
        var outcome = AssetDispatchabilityRules.MergeOutcome("ready", assetFound: true);
        Assert.Equal(AssetDispatchabilityOutcomes.Allow, outcome);
    }

    [Fact]
    public void ApplyDispatchability_marks_preview_blocked()
    {
        var preview = new DispatchAssignmentPreviewResponse(
            Guid.NewGuid(),
            "vehicle",
            true,
            false,
            [],
            [],
            []);

        var dispatchability = new AssetDispatchabilityCheckResponse(
            "VEH-1",
            null,
            AssetDispatchabilityOutcomes.Block,
            "maintainarr_not_ready",
            "Asset is not ready for dispatch.",
            true,
            new AssetDispatchabilityMaintainArrSummary(
                Guid.NewGuid(),
                "VEH-1",
                "not_ready",
                "computed",
                1,
                "Open critical defect"));

        var merged = AssetDispatchabilityRules.ApplyDispatchability(preview, dispatchability);

        Assert.False(merged.CanAssign);
        Assert.True(merged.HasBlockingConflicts);
        Assert.NotNull(merged.AssetDispatchability);
        Assert.Equal(AssetDispatchabilityOutcomes.Block, merged.AssetDispatchability!.Outcome);
    }
}
