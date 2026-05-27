using RoutArr.Api.Contracts;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class DispatchWorkflowGateRulesTests
{
    [Fact]
    public void MergeOutcome_blocks_when_any_gate_blocks()
    {
        var outcome = DispatchWorkflowGateRules.MergeOutcome(["allow", "warn", "block"]);
        Assert.Equal(DispatchWorkflowGateOutcomes.Block, outcome);
    }

    [Fact]
    public void MergeOutcome_warns_when_any_gate_warns()
    {
        var outcome = DispatchWorkflowGateRules.MergeOutcome(["allow", "warn"]);
        Assert.Equal(DispatchWorkflowGateOutcomes.Warn, outcome);
    }

    [Fact]
    public void ApplyWorkflowGates_marks_preview_blocked_for_gate_block()
    {
        var preview = new DispatchAssignmentPreviewResponse(
            Guid.NewGuid(),
            "driver",
            CanAssign: true,
            HasBlockingConflicts: false,
            [],
            [],
            []);

        var workflowGates = new DispatchWorkflowGateCheckResponse(
            preview.TripId,
            DispatchWorkflowGateOutcomes.Block,
            "rule_failed",
            "Driver qualification gate blocked assignment.",
            true,
            [
                new DispatchWorkflowGateResultSummary(
                    "dispatch_driver_qualification",
                    DispatchWorkflowGateOutcomes.Block,
                    "rule_failed",
                    "Driver qualification gate blocked assignment.",
                    true),
            ]);

        var merged = DispatchWorkflowGateRules.ApplyWorkflowGates(preview, workflowGates);

        Assert.False(merged.CanAssign);
        Assert.True(merged.HasBlockingConflicts);
        Assert.NotNull(merged.WorkflowGates);
        Assert.Equal(DispatchWorkflowGateOutcomes.Block, merged.WorkflowGates!.Outcome);
    }
}
