using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class ScheduledRuleEvaluationService(
    ComplianceCoreDbContext db,
    InternalRuleEvaluationService internalRuleEvaluationService,
    IComplianceCoreAuditService auditService)
{
    public const string ProcessScheduledActionScope = "compliancecore.rules.evaluate.scheduled";

    public static readonly string[] AcceptedActionScopes =
    [
        ProcessScheduledActionScope,
        InternalRuleEvaluationService.EvaluateActionScope,
    ];

    public async Task<PendingScheduledRuleEvaluationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? intervalHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = ScheduledRuleEvaluationRules.NormalizeBatchSize(batchSize ?? 100);
        var normalizedIntervalHours = ScheduledRuleEvaluationRules.NormalizeIntervalHours(intervalHours);
        var items = await LoadDuePacksAsync(tenantId, asOf, normalizedIntervalHours, normalizedBatchSize, cancellationToken);
        return new PendingScheduledRuleEvaluationsResponse(asOf, normalizedIntervalHours, normalizedBatchSize, items);
    }

    public async Task<ProcessScheduledRuleEvaluationsResponse> ProcessBatchAsync(
        ProcessScheduledRuleEvaluationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = ScheduledRuleEvaluationRules.NormalizeBatchSize(request.BatchSize ?? 100);
        var intervalHours = ScheduledRuleEvaluationRules.NormalizeIntervalHours(request.IntervalHours);
        var dueItems = await LoadDuePacksAsync(request.TenantId, asOf, intervalHours, batchSize, cancellationToken);

        var run = new ScheduledRuleEvaluationRun
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            StartedAt = asOf,
            Status = ScheduledRuleEvaluationRunStatuses.InProgress,
            IntervalHours = intervalHours,
            PacksDueCount = dueItems.Count,
        };

        db.ScheduledRuleEvaluationRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var evaluationRunIds = new List<Guid>();
        var skipped = new List<ScheduledRuleEvaluationSkip>();
        var allowCount = 0;
        var warnCount = 0;
        var blockCount = 0;
        var evaluatedCount = 0;

        foreach (var item in dueItems)
        {
            try
            {
                var result = await internalRuleEvaluationService.EvaluateAsync(
                    new InternalEvaluateRulePackRequest(
                        item.TenantId,
                        item.PackKey,
                        null,
                        request.EmitFindings),
                    sourceProductKey: "shared-worker",
                    cancellationToken);

                var pack = await db.RulePacks.FirstAsync(
                    x => x.TenantId == item.TenantId && x.Id == item.RulePackId,
                    cancellationToken);
                pack.LastScheduledEvaluationAt = asOf;
                pack.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(cancellationToken);

                evaluatedCount++;
                if (result.EvaluationRunId.HasValue)
                {
                    evaluationRunIds.Add(result.EvaluationRunId.Value);
                }

                switch (result.Outcome)
                {
                    case ComplianceEvaluationOutcomes.Allow:
                        allowCount++;
                        break;
                    case ComplianceEvaluationOutcomes.Warn:
                        warnCount++;
                        break;
                    default:
                        blockCount++;
                        break;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new ScheduledRuleEvaluationSkip(item.RulePackId, item.PackKey, ex.Message));
            }
        }

        run.CompletedAt = DateTimeOffset.UtcNow;
        run.Status = ScheduledRuleEvaluationRunStatuses.Completed;
        run.PacksProcessedCount = dueItems.Count;
        run.EvaluatedCount = evaluatedCount;
        run.SkippedCount = skipped.Count;
        run.AllowCount = allowCount;
        run.WarnCount = warnCount;
        run.BlockCount = blockCount;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "rules.scheduled_evaluate_batch",
            request.TenantId ?? Guid.Empty,
            actorUserId: null,
            "scheduled_rule_evaluation_run",
            run.Id.ToString(),
            $"{evaluatedCount} evaluated, {skipped.Count} skipped",
            reasonCode: blockCount > 0 ? "batch_has_blocks" : null,
            cancellationToken: cancellationToken);

        return new ProcessScheduledRuleEvaluationsResponse(
            run.Id,
            asOf,
            intervalHours,
            batchSize,
            dueItems.Count,
            dueItems.Count,
            evaluatedCount,
            skipped.Count,
            allowCount,
            warnCount,
            blockCount,
            evaluationRunIds,
            skipped);
    }

    public async Task<ProcessScheduledRuleEvaluationsResponse> ProcessTenantManualAsync(
        Guid tenantId,
        bool emitFindings = true,
        CancellationToken cancellationToken = default)
    {
        var asOf = DateTimeOffset.UtcNow;
        var dueItems = await LoadEligiblePacksForTenantAsync(tenantId, cancellationToken);
        var intervalHours = ScheduledRuleEvaluationRules.DefaultIntervalHours;
        var batchSize = ScheduledRuleEvaluationRules.NormalizeBatchSize(dueItems.Count);

        var run = new ScheduledRuleEvaluationRun
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StartedAt = asOf,
            Status = ScheduledRuleEvaluationRunStatuses.InProgress,
            IntervalHours = intervalHours,
            PacksDueCount = dueItems.Count,
        };

        db.ScheduledRuleEvaluationRuns.Add(run);
        await db.SaveChangesAsync(cancellationToken);

        var evaluationRunIds = new List<Guid>();
        var skipped = new List<ScheduledRuleEvaluationSkip>();
        var allowCount = 0;
        var warnCount = 0;
        var blockCount = 0;
        var evaluatedCount = 0;

        foreach (var item in dueItems)
        {
            try
            {
                var result = await internalRuleEvaluationService.EvaluateAsync(
                    new InternalEvaluateRulePackRequest(
                        item.TenantId,
                        item.PackKey,
                        null,
                        emitFindings),
                    sourceProductKey: "compliancecore-ui",
                    cancellationToken);

                var pack = await db.RulePacks.FirstAsync(
                    x => x.TenantId == item.TenantId && x.Id == item.RulePackId,
                    cancellationToken);
                pack.LastScheduledEvaluationAt = asOf;
                pack.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(cancellationToken);

                evaluatedCount++;
                if (result.EvaluationRunId.HasValue)
                {
                    evaluationRunIds.Add(result.EvaluationRunId.Value);
                }

                switch (result.Outcome)
                {
                    case ComplianceEvaluationOutcomes.Allow:
                        allowCount++;
                        break;
                    case ComplianceEvaluationOutcomes.Warn:
                        warnCount++;
                        break;
                    default:
                        blockCount++;
                        break;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new ScheduledRuleEvaluationSkip(item.RulePackId, item.PackKey, ex.Message));
            }
        }

        run.CompletedAt = DateTimeOffset.UtcNow;
        run.Status = ScheduledRuleEvaluationRunStatuses.Completed;
        run.PacksProcessedCount = dueItems.Count;
        run.EvaluatedCount = evaluatedCount;
        run.SkippedCount = skipped.Count;
        run.AllowCount = allowCount;
        run.WarnCount = warnCount;
        run.BlockCount = blockCount;
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "rules.scheduled_evaluate_manual",
            tenantId,
            actorUserId: null,
            "scheduled_rule_evaluation_run",
            run.Id.ToString(),
            $"{evaluatedCount} evaluated, {skipped.Count} skipped",
            reasonCode: blockCount > 0 ? "batch_has_blocks" : null,
            cancellationToken: cancellationToken);

        return new ProcessScheduledRuleEvaluationsResponse(
            run.Id,
            asOf,
            intervalHours,
            batchSize,
            dueItems.Count,
            dueItems.Count,
            evaluatedCount,
            skipped.Count,
            allowCount,
            warnCount,
            blockCount,
            evaluationRunIds,
            skipped);
    }

    private async Task<IReadOnlyList<PendingScheduledRuleEvaluationItem>> LoadEligiblePacksForTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var candidates = await db.RulePacks.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Where(x => x.IsActive)
            .Where(x => x.Status == RulePackStatuses.Published)
            .Where(x => x.RuleContentJson != null && x.RuleContentJson != "")
            .OrderBy(x => x.PackKey)
            .ThenByDescending(x => x.VersionNumber)
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                x.PackKey,
                x.Label,
                x.VersionNumber,
                x.Status,
                x.RuleContentJson,
                x.LastScheduledEvaluationAt,
            })
            .ToListAsync(cancellationToken);

        return candidates
            .GroupBy(x => x.PackKey)
            .Select(group => group.First())
            .Where(x => ScheduledRuleEvaluationRules.IsEligibleForScheduledEvaluation(x.Status, x.RuleContentJson))
            .OrderBy(x => x.PackKey)
            .Select(x => new PendingScheduledRuleEvaluationItem(
                x.TenantId,
                x.Id,
                x.PackKey,
                x.Label,
                x.VersionNumber,
                x.LastScheduledEvaluationAt))
            .ToList();
    }

    private async Task<IReadOnlyList<PendingScheduledRuleEvaluationItem>> LoadDuePacksAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int intervalHours,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.RulePacks.AsNoTracking()
            .Where(x => x.IsActive)
            .Where(x => x.Status == RulePackStatuses.Published)
            .Where(x => x.RuleContentJson != null && x.RuleContentJson != "");

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        var candidates = await query
            .OrderBy(x => x.TenantId)
            .ThenBy(x => x.PackKey)
            .ThenByDescending(x => x.VersionNumber)
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                x.PackKey,
                x.Label,
                x.VersionNumber,
                x.Status,
                x.RuleContentJson,
                x.LastScheduledEvaluationAt,
            })
            .ToListAsync(cancellationToken);

        var latestPublished = candidates
            .GroupBy(x => new { x.TenantId, x.PackKey })
            .Select(group => group.First())
            .Where(x => ScheduledRuleEvaluationRules.IsEligibleForScheduledEvaluation(x.Status, x.RuleContentJson))
            .Where(x => ScheduledRuleEvaluationRules.IsDue(x.LastScheduledEvaluationAt, asOfUtc, intervalHours))
            .OrderBy(x => x.LastScheduledEvaluationAt ?? DateTimeOffset.MinValue)
            .ThenBy(x => x.PackKey)
            .Take(batchSize)
            .Select(x => new PendingScheduledRuleEvaluationItem(
                x.TenantId,
                x.Id,
                x.PackKey,
                x.Label,
                x.VersionNumber,
                x.LastScheduledEvaluationAt))
            .ToList();

        return latestPublished;
    }
}
