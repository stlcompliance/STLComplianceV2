using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DispatchWorkflowGateService(
    RoutArrDbContext db,
    ComplianceCoreWorkflowGateClient complianceCoreClient,
    IOptions<DispatchWorkflowGateOptions> gateOptions,
    IRoutArrAuditService audit)
{
    public const string CheckAction = "dispatch_workflow_gate.check";

    public async Task<DispatchWorkflowGateCheckResponse> CheckForTripAsync(
        Guid tenantId,
        Guid? actorUserId,
        Trip trip,
        string assignmentKind,
        string? driverPersonId,
        string? vehicleRefKey,
        CancellationToken cancellationToken = default)
    {
        var options = gateOptions.Value;
        if (!options.CheckComplianceCoreWorkflowGates)
        {
            return BuildClearResponse(trip.Id, []);
        }

        var gateKeys = ResolveGateKeys(assignmentKind, options);
        if (gateKeys.Count == 0)
        {
            return BuildClearResponse(trip.Id, []);
        }

        var context = DispatchWorkflowGateContextBuilder.BuildTripContext(
            trip,
            assignmentKind,
            driverPersonId,
            vehicleRefKey);

        var batchItems = gateKeys
            .Select(gateKey => new ComplianceCoreInternalWorkflowGateBatchCheckItem(gateKey, context))
            .ToList();

        ComplianceCoreWorkflowGateBatchCheckResponse? batch;
        try
        {
            batch = await complianceCoreClient.CheckBatchAsync(
                tenantId,
                batchItems,
                context,
                cancellationToken);
        }
        catch (StlApiException ex) when (ex.StatusCode is 404 or 400)
        {
            return BuildUnavailableResponse(trip.Id, gateKeys, ex.Message);
        }

        if (batch is null)
        {
            return BuildUnavailableResponse(
                trip.Id,
                gateKeys,
                "Compliance Core workflow gate integration is not configured.");
        }

        var gateSummaries = batch.Results
            .Select(result => new DispatchWorkflowGateResultSummary(
                result.GateKey,
                result.Outcome,
                result.ReasonCode,
                result.Message,
                DispatchWorkflowGateRules.IsBlockingOutcome(result.Outcome),
                result.CheckResultId,
                result.GateLabel,
                result.RuleEvaluationRunId,
                result.Reasons
                    .Select(reason => new DispatchWorkflowGateReasonSummary(
                        reason.Code,
                        reason.Message,
                        reason.RuleKey,
                        reason.FactKey))
                    .ToList(),
                result.CheckedAt,
                result.AppliedWaiverId,
                result.AppliedWaiverKey))
            .ToList();

        var outcome = DispatchWorkflowGateRules.MergeOutcome(gateSummaries.Select(x => x.Outcome));
        var (reasonCode, message) = DispatchWorkflowGateRules.BuildMergedReason(outcome, gateSummaries);

        var checkedAt = gateSummaries
            .Where(x => x.CheckedAt.HasValue)
            .Select(x => x.CheckedAt!.Value)
            .DefaultIfEmpty(DateTimeOffset.UtcNow)
            .Max();

        DispatchWorkflowGateAuditSnapshotResponse? auditSnapshot = null;

        if (actorUserId.HasValue)
        {
            var auditResult = await audit.WriteAsync(
                CheckAction,
                tenantId,
                actorUserId.Value,
                "trip",
                trip.Id.ToString(),
                outcome,
                reasonCode,
                cancellationToken: cancellationToken);

            auditSnapshot = new DispatchWorkflowGateAuditSnapshotResponse(
                auditResult.AuditEventId,
                auditResult.OccurredAt,
                auditResult.Action,
                auditResult.Result,
                auditResult.ReasonCode);
        }

        return new DispatchWorkflowGateCheckResponse(
            trip.Id,
            outcome,
            reasonCode,
            message,
            DispatchWorkflowGateRules.IsBlockingOutcome(outcome),
            gateSummaries,
            batch.BatchId,
            checkedAt,
            context,
            auditSnapshot,
            new DispatchReleaseReadinessSnapshotResponse(
                "dispatch_release_readiness",
                checkedAt,
                context,
                gateSummaries));
    }

    public async Task<DispatchWorkflowGateCheckResponse> CheckAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid tripId,
        string? driverPersonId,
        string? vehicleRefKey,
        string? assignmentKind,
        CancellationToken cancellationToken = default)
    {
        var trip = await db.Trips
            .AsNoTracking()
            .Include(x => x.Loads)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

        var normalizedKind = NormalizeAssignmentKind(assignmentKind, driverPersonId, vehicleRefKey);

        return await CheckForTripAsync(
            tenantId,
            actorUserId,
            trip,
            normalizedKind,
            driverPersonId,
            vehicleRefKey,
            cancellationToken);
    }

    public async Task EnsureWorkflowGatesAllowedAsync(
        Guid tenantId,
        Trip trip,
        string assignmentKind,
        string? driverPersonId,
        string? vehicleRefKey,
        bool ignoreWorkflowGateBlocks,
        CancellationToken cancellationToken = default)
    {
        if (ignoreWorkflowGateBlocks)
        {
            return;
        }

        var result = await CheckForTripAsync(
            tenantId,
            actorUserId: null,
            trip,
            assignmentKind,
            driverPersonId,
            vehicleRefKey,
            cancellationToken);

        if (result.IsBlocking)
        {
            throw new StlApiException(
                "dispatch.workflow_gate_blocked",
                result.Message,
                409,
                result);
        }
    }

    private static IReadOnlyList<string> ResolveGateKeys(string assignmentKind, DispatchWorkflowGateOptions options)
    {
        if (string.Equals(assignmentKind, DispatchAssignmentService.AssignmentKinds.Driver, StringComparison.OrdinalIgnoreCase))
        {
            return NormalizeGateKeys(options.DriverAssignmentGateKeys);
        }

        if (string.Equals(assignmentKind, DispatchAssignmentService.AssignmentKinds.Vehicle, StringComparison.OrdinalIgnoreCase))
        {
            return NormalizeGateKeys(options.VehicleAssignmentGateKeys);
        }

        return [];
    }

    private static IReadOnlyList<string> NormalizeGateKeys(IEnumerable<string>? gateKeys) =>
        gateKeys?
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Select(key => key.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
        ?? [];

    private static string NormalizeAssignmentKind(
        string? assignmentKind,
        string? driverPersonId,
        string? vehicleRefKey)
    {
        if (!string.IsNullOrWhiteSpace(assignmentKind))
        {
            var normalized = assignmentKind.Trim().ToLowerInvariant();
            if (DispatchAssignmentService.AssignmentKinds.All.Contains(normalized))
            {
                return normalized;
            }
        }

        if (!string.IsNullOrWhiteSpace(driverPersonId))
        {
            return DispatchAssignmentService.AssignmentKinds.Driver;
        }

        if (!string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            return DispatchAssignmentService.AssignmentKinds.Vehicle;
        }

        return DispatchAssignmentService.AssignmentKinds.Driver;
    }

    private static DispatchWorkflowGateCheckResponse BuildClearResponse(
        Guid tripId,
        IReadOnlyList<DispatchWorkflowGateResultSummary> gates) =>
        new(
            tripId,
            DispatchWorkflowGateOutcomes.Allow,
            "workflow_gate_clear",
            "Compliance workflow gates passed.",
            false,
            gates);

    private static DispatchWorkflowGateCheckResponse BuildUnavailableResponse(
        Guid tripId,
        IReadOnlyList<string> gateKeys,
        string detail)
    {
        var gateSummaries = gateKeys
            .Select(gateKey => new DispatchWorkflowGateResultSummary(
                gateKey,
                DispatchWorkflowGateOutcomes.Warn,
                "workflow_gate_check_unavailable",
                detail,
                false))
            .ToList();

        return new DispatchWorkflowGateCheckResponse(
            tripId,
            DispatchWorkflowGateOutcomes.Warn,
            "workflow_gate_check_unavailable",
            detail,
            false,
            gateSummaries);
    }
}
