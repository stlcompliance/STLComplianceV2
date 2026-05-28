using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class AssignmentEscalationWorkerService(
    TrainArrDbContext db,
    AssignmentEscalationSettingsService settingsService,
    TrainingNotificationEnqueueService notificationEnqueue,
    ITrainArrAuditService audit)
{
    public const string ProcessEscalationsActionScope = "trainarr.assignments.escalate";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f4");

    public async Task<PendingAssignmentEscalationsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = AssignmentEscalationRules.NormalizeBatchSize(batchSize);
        var candidates = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        var items = candidates
            .Select(x => new PendingAssignmentEscalationItem(
                x.AssignmentId,
                x.StaffarrPersonId,
                x.DueAt,
                x.EscalationCount,
                x.LastEscalatedAt,
                AssignmentEscalationRules.ComputeHoursOverdue(x.DueAt, asOf),
                AssignmentEscalationRules.ComputeHoursUntilNextEscalation(
                    x.DueAt,
                    x.LastEscalatedAt,
                    x.OverdueEscalationAfterHours,
                    x.EscalationCooldownHours,
                    x.EscalationCount,
                    x.MaxEscalationsPerAssignment,
                    asOf)))
            .ToList();

        return new PendingAssignmentEscalationsResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessAssignmentEscalationsResponse> ProcessBatchAsync(
        ProcessAssignmentEscalationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = AssignmentEscalationRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var escalated = new List<AssignmentEscalationResult>();
        var skipped = new List<AssignmentEscalationSkip>();
        var runStats = new Dictionary<Guid, (int Candidates, int Escalated, int Skipped)>();

        foreach (var candidate in candidates)
        {
            if (!runStats.ContainsKey(candidate.TenantId))
            {
                runStats[candidate.TenantId] = (0, 0, 0);
            }

            var stats = runStats[candidate.TenantId];
            stats.Candidates++;
            runStats[candidate.TenantId] = stats;

            try
            {
                var result = await EscalateAssignmentAsync(candidate, asOf, cancellationToken);
                escalated.Add(result);
                stats = runStats[candidate.TenantId];
                stats.Escalated++;
                runStats[candidate.TenantId] = stats;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new AssignmentEscalationSkip(candidate.AssignmentId, ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.AssignmentEscalationRuns.Add(new AssignmentEscalationRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdKey,
                AsOfUtc = asOf,
                CandidatesFound = stats.Candidates,
                EscalatedCount = stats.Escalated,
                SkippedCount = stats.Skipped,
                CreatedAt = asOf,
            });
        }

        if (runStats.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (request.TenantId is Guid tenantId && escalated.Count > 0)
        {
            await audit.WriteAsync(
                "trainarr.assignment_escalation.batch",
                tenantId,
                WorkerActorUserId,
                "assignment_escalation_run",
                $"{escalated.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessAssignmentEscalationsResponse(
            asOf,
            batchSize,
            candidates.Count,
            escalated.Count,
            skipped.Count,
            escalated,
            skipped);
    }

    public async Task<AssignmentEscalationRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = AssignmentEscalationRules.NormalizeRunListLimit(limit);
        var runs = await db.AssignmentEscalationRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new AssignmentEscalationRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.EscalatedCount,
                x.SkippedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AssignmentEscalationRunsResponse(runs);
    }

    public async Task<AssignmentEscalationEventsResponse> ListRecentEventsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = AssignmentEscalationRules.NormalizeEventListLimit(limit);
        var events = await db.AssignmentEscalationEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new AssignmentEscalationEventItem(
                x.Id,
                x.TrainingAssignmentId,
                x.StaffarrPersonId,
                x.DueAt,
                x.EscalationCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AssignmentEscalationEventsResponse(events);
    }

    private async Task<AssignmentEscalationResult> EscalateAssignmentAsync(
        PendingAssignmentEscalationCandidate candidate,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        if (!AssignmentEscalationRules.IsDueForEscalation(
                candidate.DueAt,
                candidate.LastEscalatedAt,
                candidate.OverdueEscalationAfterHours,
                candidate.EscalationCooldownHours,
                candidate.EscalationCount,
                candidate.MaxEscalationsPerAssignment,
                asOfUtc))
        {
            throw new InvalidOperationException("Assignment is not due for escalation.");
        }

        var assignment = await db.TrainingAssignments
            .FirstOrDefaultAsync(x => x.Id == candidate.AssignmentId, cancellationToken)
            ?? throw new InvalidOperationException($"Assignment {candidate.AssignmentId} was not found.");

        assignment.EscalationCount += 1;
        assignment.LastEscalatedAt = asOfUtc;
        assignment.UpdatedAt = asOfUtc;

        db.AssignmentEscalationEvents.Add(new AssignmentEscalationEvent
        {
            Id = Guid.NewGuid(),
            TenantId = assignment.TenantId,
            TrainingAssignmentId = assignment.Id,
            StaffarrPersonId = assignment.StaffarrPersonId,
            DueAt = assignment.DueAt,
            EscalationCount = assignment.EscalationCount,
            CreatedAt = asOfUtc,
        });

        Guid? notificationDispatchId = await notificationEnqueue.TryEnqueueRepeatableAsync(
            assignment.TenantId,
            TrainingNotificationEventKinds.AssignmentOverdueEscalation,
            assignment.StaffarrPersonId,
            "training_assignment",
            assignment.Id,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return new AssignmentEscalationResult(
            assignment.Id,
            assignment.EscalationCount,
            notificationDispatchId);
    }

    private async Task<List<PendingAssignmentEscalationCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var settingsQuery = db.TenantAssignmentEscalationSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled);

        if (tenantId is Guid scopedTenantId)
        {
            settingsQuery = settingsQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var tenantSettings = await settingsQuery.ToListAsync(cancellationToken);
        var results = new List<PendingAssignmentEscalationCandidate>();

        foreach (var settings in tenantSettings)
        {
            if (results.Count >= batchSize)
            {
                break;
            }

            var snapshot = AssignmentEscalationSettingsService.ToSnapshot(settings);

            var assignments = await db.TrainingAssignments
                .AsNoTracking()
                .Where(x => x.TenantId == settings.TenantId
                    && x.DueAt != null
                    && x.DueAt <= asOfUtc
                    && (x.Status == "assigned" || x.Status == "in_progress"))
                .OrderBy(x => x.DueAt)
                .Take(batchSize * 2)
                .Select(x => new PendingAssignmentEscalationCandidate(
                    x.TenantId,
                    x.Id,
                    x.StaffarrPersonId,
                    x.DueAt!.Value,
                    x.EscalationCount,
                    x.LastEscalatedAt,
                    snapshot.OverdueEscalationAfterHours,
                    snapshot.EscalationCooldownHours,
                    snapshot.MaxEscalationsPerAssignment))
                .ToListAsync(cancellationToken);

            foreach (var assignment in assignments)
            {
                if (AssignmentEscalationRules.IsDueForEscalation(
                        assignment.DueAt,
                        assignment.LastEscalatedAt,
                        assignment.OverdueEscalationAfterHours,
                        assignment.EscalationCooldownHours,
                        assignment.EscalationCount,
                        assignment.MaxEscalationsPerAssignment,
                        asOfUtc))
                {
                    results.Add(assignment);
                    if (results.Count >= batchSize)
                    {
                        break;
                    }
                }
            }
        }

        return results;
    }

    private sealed record PendingAssignmentEscalationCandidate(
        Guid TenantId,
        Guid AssignmentId,
        Guid StaffarrPersonId,
        DateTimeOffset DueAt,
        int EscalationCount,
        DateTimeOffset? LastEscalatedAt,
        int OverdueEscalationAfterHours,
        int EscalationCooldownHours,
        int MaxEscalationsPerAssignment);
}
