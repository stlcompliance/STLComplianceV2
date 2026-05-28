using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class QualificationRecalculationService(
    TrainArrDbContext db,
    QualificationRecalculationSettingsService settingsService,
    QualificationCheckService qualificationCheckService,
    QualificationIssueService qualificationIssueService,
    ITrainArrAuditService audit)
{
    public const string ProcessRecalculationsActionScope = "trainarr.qualifications.recalculate";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f4");

    public async Task<PendingQualificationRecalculationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        int? stalenessHours,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = QualificationRecalculationRules.NormalizeBatchSize(batchSize);
        var normalizedStalenessHours = QualificationRecalculationRules.NormalizeStalenessHours(stalenessHours);
        var items = await LoadPendingCandidatesAsync(
            tenantId,
            asOf,
            normalizedBatchSize,
            normalizedStalenessHours,
            cancellationToken);

        var responseItems = items
            .Select(x => new PendingQualificationRecalculationItem(
                x.QualificationIssueId,
                x.TenantId,
                x.StaffarrPersonId,
                x.QualificationKey,
                x.QualificationName,
                x.Status,
                x.LastComputedAt))
            .ToList();

        return new PendingQualificationRecalculationsResponse(
            asOf,
            normalizedStalenessHours,
            normalizedBatchSize,
            responseItems);
    }

    public async Task<ProcessQualificationRecalculationsResponse> ProcessBatchAsync(
        ProcessQualificationRecalculationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = QualificationRecalculationRules.NormalizeBatchSize(request.BatchSize);
        var stalenessHours = QualificationRecalculationRules.NormalizeStalenessHours(request.StalenessHours);
        var candidates = await LoadPendingCandidatesAsync(
            request.TenantId,
            asOf,
            batchSize,
            stalenessHours,
            cancellationToken);

        var recalculatedIssueIds = new List<Guid>();
        var suspendedIssueIds = new List<Guid>();
        var skipped = new List<QualificationRecalculationSkip>();

        foreach (var candidate in candidates)
        {
            try
            {
                var settings = await settingsService.LoadSnapshotAsync(candidate.TenantId, cancellationToken);
                var autoSuspendOnBlock = settings?.AutoSuspendOnBlock == true;

                var issue = await db.QualificationIssues.FirstAsync(
                    x => x.TenantId == candidate.TenantId && x.Id == candidate.QualificationIssueId,
                    cancellationToken);

                var check = await qualificationCheckService.EvaluateIssueAsync(
                    candidate.TenantId,
                    issue,
                    candidate.TrainingDefinitionId,
                    cancellationToken);

                var runOutcome = "recalculated";
                var now = DateTimeOffset.UtcNow;

                var existingState = await db.QualificationRecalculationStates
                    .FirstOrDefaultAsync(
                        x => x.TenantId == candidate.TenantId && x.QualificationIssueId == candidate.QualificationIssueId,
                        cancellationToken);

                if (existingState is null)
                {
                    existingState = new QualificationRecalculationState
                    {
                        Id = Guid.NewGuid(),
                        TenantId = candidate.TenantId,
                        QualificationIssueId = candidate.QualificationIssueId,
                        StaffarrPersonId = candidate.StaffarrPersonId,
                        QualificationKey = candidate.QualificationKey,
                        CreatedAt = now,
                    };
                    db.QualificationRecalculationStates.Add(existingState);
                }
                else
                {
                    existingState.PreviousOutcome = existingState.Outcome;
                }

                existingState.Outcome = check.Outcome;
                existingState.ReasonCode = check.ReasonCode;
                existingState.Message = Truncate(check.Message, 1024);
                existingState.RulePackKey = check.ComplianceCore?.RulePackKey;
                existingState.ComputedAt = now;
                existingState.UpdatedAt = now;

                if (QualificationRecalculationRules.ShouldAutoSuspend(
                        autoSuspendOnBlock,
                        issue.Status,
                        check.Outcome,
                        check.ComplianceCore?.Outcome))
                {
                    await qualificationIssueService.SuspendAsync(
                        candidate.TenantId,
                        WorkerActorUserId,
                        candidate.QualificationIssueId,
                        new QualificationLifecycleActionRequest(
                            "Qualification suspended after scheduled recalculation detected a compliance block."),
                        cancellationToken);
                    runOutcome = "suspended";
                    suspendedIssueIds.Add(candidate.QualificationIssueId);
                }

                db.QualificationRecalculationRuns.Add(new QualificationRecalculationRun
                {
                    Id = Guid.NewGuid(),
                    TenantId = candidate.TenantId,
                    QualificationIssueId = candidate.QualificationIssueId,
                    Outcome = runOutcome,
                    CheckOutcome = check.Outcome,
                    ProcessedAt = now,
                    CreatedAt = now,
                });

                await db.SaveChangesAsync(cancellationToken);
                recalculatedIssueIds.Add(candidate.QualificationIssueId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                db.QualificationRecalculationRuns.Add(new QualificationRecalculationRun
                {
                    Id = Guid.NewGuid(),
                    TenantId = candidate.TenantId,
                    QualificationIssueId = candidate.QualificationIssueId,
                    Outcome = "skipped",
                    SkipReason = Truncate(ex.Message, 512),
                    ProcessedAt = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                });
                await db.SaveChangesAsync(cancellationToken);
                skipped.Add(new QualificationRecalculationSkip(candidate.QualificationIssueId, ex.Message));
            }
        }

        if (recalculatedIssueIds.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "qualification_recalculation.batch",
                tenantId,
                WorkerActorUserId,
                "qualification_recalculation",
                $"{recalculatedIssueIds.Count}",
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        return new ProcessQualificationRecalculationsResponse(
            asOf,
            batchSize,
            candidates.Count,
            recalculatedIssueIds.Count,
            suspendedIssueIds.Count,
            skipped.Count,
            recalculatedIssueIds,
            suspendedIssueIds,
            skipped);
    }

    public async Task<QualificationRecalculationStatesResponse> ListRecentStatesAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = QualificationRecalculationRules.NormalizeStateListLimit(limit);
        var rows = await db.QualificationRecalculationStates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ComputedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new QualificationRecalculationStateItem(
                x.QualificationIssueId,
                x.StaffarrPersonId,
                x.QualificationKey,
                x.Outcome,
                x.ReasonCode,
                x.RulePackKey,
                x.PreviousOutcome,
                x.ComputedAt))
            .ToList();

        return new QualificationRecalculationStatesResponse(items);
    }

    public async Task<QualificationRecalculationRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = QualificationRecalculationRules.NormalizeRunListLimit(limit);
        var rows = await db.QualificationRecalculationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ProcessedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new QualificationRecalculationRunItem(
                x.Id,
                x.QualificationIssueId,
                x.Outcome,
                x.CheckOutcome,
                x.SkipReason,
                x.ProcessedAt))
            .ToList();

        return new QualificationRecalculationRunsResponse(items);
    }

    private async Task<IReadOnlyList<PendingRecalculationCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        int stalenessHours,
        CancellationToken cancellationToken)
    {
        var settingsQuery = db.TenantQualificationRecalculationSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled);

        if (tenantId is Guid scopedTenantId)
        {
            settingsQuery = settingsQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var tenantSettings = await settingsQuery.ToListAsync(cancellationToken);
        var results = new List<PendingRecalculationCandidate>();

        foreach (var settings in tenantSettings)
        {
            if (results.Count >= batchSize)
            {
                break;
            }

            var tenantStalenessHours = QualificationRecalculationRules.NormalizeStalenessHours(settings.StalenessHours);
            var effectiveStalenessHours = tenantId is null
                ? tenantStalenessHours
                : QualificationRecalculationRules.NormalizeStalenessHours(stalenessHours);

            var rawCandidates = await (
                from issue in db.QualificationIssues.AsNoTracking()
                join sourceAssignment in db.TrainingAssignments.AsNoTracking()
                    on issue.TrainingAssignmentId equals sourceAssignment.Id
                join state in db.QualificationRecalculationStates.AsNoTracking()
                    on new { issue.TenantId, issue.Id }
                    equals new { state.TenantId, Id = state.QualificationIssueId } into states
                from state in states.DefaultIfEmpty()
                where issue.TenantId == settings.TenantId
                    && QualificationRecalculationRules.RecalculableStatuses.Contains(issue.Status)
                select new
                {
                    QualificationIssueId = issue.Id,
                    issue.TenantId,
                    issue.StaffarrPersonId,
                    issue.QualificationKey,
                    issue.QualificationName,
                    issue.Status,
                    TrainingDefinitionId = sourceAssignment.TrainingDefinitionId,
                    LastComputedAt = state != null ? (DateTimeOffset?)state.ComputedAt : null,
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rawCandidates)
            {
                if (results.Count >= batchSize)
                {
                    break;
                }

                if (!QualificationRecalculationRules.IsStale(row.LastComputedAt, asOfUtc, effectiveStalenessHours))
                {
                    continue;
                }

                results.Add(new PendingRecalculationCandidate(
                    row.QualificationIssueId,
                    row.TenantId,
                    row.StaffarrPersonId,
                    row.TrainingDefinitionId,
                    row.QualificationKey,
                    row.QualificationName,
                    row.Status,
                    row.LastComputedAt));
            }
        }

        return results;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private sealed record PendingRecalculationCandidate(
        Guid QualificationIssueId,
        Guid TenantId,
        Guid StaffarrPersonId,
        Guid TrainingDefinitionId,
        string QualificationKey,
        string QualificationName,
        string Status,
        DateTimeOffset? LastComputedAt);
}
