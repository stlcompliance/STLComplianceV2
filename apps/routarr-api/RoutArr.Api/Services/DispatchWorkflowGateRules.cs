using RoutArr.Api.Contracts;

namespace RoutArr.Api.Services;

public static class DispatchWorkflowGateRules
{
    public static string MergeOutcome(IEnumerable<string> gateOutcomes)
    {
        var hasBlock = false;
        var hasWarn = false;

        foreach (var outcome in gateOutcomes)
        {
            if (string.Equals(outcome, DispatchWorkflowGateOutcomes.Block, StringComparison.OrdinalIgnoreCase))
            {
                hasBlock = true;
            }
            else if (string.Equals(outcome, DispatchWorkflowGateOutcomes.Warn, StringComparison.OrdinalIgnoreCase))
            {
                hasWarn = true;
            }
        }

        if (hasBlock)
        {
            return DispatchWorkflowGateOutcomes.Block;
        }

        if (hasWarn)
        {
            return DispatchWorkflowGateOutcomes.Warn;
        }

        return DispatchWorkflowGateOutcomes.Allow;
    }

    public static bool IsBlockingOutcome(string outcome) =>
        string.Equals(outcome, DispatchWorkflowGateOutcomes.Block, StringComparison.OrdinalIgnoreCase);

    public static (string ReasonCode, string Message) BuildMergedReason(
        string outcome,
        IReadOnlyList<DispatchWorkflowGateResultSummary> gates)
    {
        if (string.Equals(outcome, DispatchWorkflowGateOutcomes.Block, StringComparison.OrdinalIgnoreCase))
        {
            var blocking = gates.FirstOrDefault(g => IsBlockingOutcome(g.Outcome));
            if (blocking is not null)
            {
                return (blocking.ReasonCode, blocking.Message);
            }

            return ("workflow_gate_blocked", "Compliance workflow gate blocked dispatch assignment.");
        }

        if (string.Equals(outcome, DispatchWorkflowGateOutcomes.Warn, StringComparison.OrdinalIgnoreCase))
        {
            var warning = gates.FirstOrDefault(g =>
                string.Equals(g.Outcome, DispatchWorkflowGateOutcomes.Warn, StringComparison.OrdinalIgnoreCase));
            if (warning is not null)
            {
                return (warning.ReasonCode, warning.Message);
            }

            return ("workflow_gate_warn", "Compliance workflow gate returned warnings.");
        }

        return ("workflow_gate_clear", "Compliance workflow gates passed.");
    }

    public static DispatchAssignmentPreviewResponse ApplyWorkflowGates(
        DispatchAssignmentPreviewResponse preview,
        DispatchWorkflowGateCheckResponse? workflowGates)
    {
        if (workflowGates is null)
        {
            return preview;
        }

        var summary = new DispatchAssignmentWorkflowGateSummary(
            workflowGates.Outcome,
            workflowGates.ReasonCode,
            workflowGates.Message,
            workflowGates.IsBlocking,
            workflowGates.Gates);

        var hasBlocking = preview.HasBlockingConflicts || workflowGates.IsBlocking;

        return preview with
        {
            CanAssign = !hasBlocking,
            HasBlockingConflicts = hasBlocking,
            WorkflowGates = summary,
        };
    }
}
