using RoutArr.Api.Contracts;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DriverEligibilityRulesTests
{
    [Fact]
    public void MergeOutcome_blocks_when_staffarr_not_ready()
    {
        var outcome = DriverEligibilityRules.MergeOutcome("allow", "not_ready");
        Assert.Equal(DriverEligibilityOutcomes.Block, outcome);
    }

    [Fact]
    public void MergeOutcome_blocks_when_trainarr_blocks()
    {
        var outcome = DriverEligibilityRules.MergeOutcome(DriverEligibilityOutcomes.Block, "ready");
        Assert.Equal(DriverEligibilityOutcomes.Block, outcome);
    }

    [Fact]
    public void MergeOutcome_warns_when_trainarr_warns_and_staffarr_ready()
    {
        var outcome = DriverEligibilityRules.MergeOutcome(DriverEligibilityOutcomes.Warn, "ready");
        Assert.Equal(DriverEligibilityOutcomes.Warn, outcome);
    }

    [Fact]
    public void ApplyEligibility_marks_preview_blocked_for_eligibility_block()
    {
        var preview = new DispatchAssignmentPreviewResponse(
            Guid.NewGuid(),
            "driver",
            CanAssign: true,
            HasBlockingConflicts: false,
            [],
            [],
            []);

        var eligibility = new DriverEligibilityCheckResponse(
            Guid.NewGuid().ToString(),
            DriverEligibilityOutcomes.Block,
            "staffarr_not_ready",
            "Driver is not ready.",
            true,
            null,
            new DriverEligibilityStaffArrSummary("not_ready", "training_blockers", 1, "Missing certification"));

        var merged = DriverEligibilityRules.ApplyEligibility(preview, eligibility);

        Assert.False(merged.CanAssign);
        Assert.True(merged.HasBlockingConflicts);
        Assert.NotNull(merged.DriverEligibility);
        Assert.Equal(DriverEligibilityOutcomes.Block, merged.DriverEligibility!.Outcome);
    }
}
