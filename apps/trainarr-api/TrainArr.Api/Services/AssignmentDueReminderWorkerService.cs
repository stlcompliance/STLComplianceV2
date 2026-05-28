using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class AssignmentDueReminderWorkerService(
    TrainArrDbContext db,
    AssignmentDueReminderSettingsService settingsService,
    TrainingNotificationEnqueueService notificationEnqueue,
    ITrainArrAuditService audit)
{
    public const string ProcessDueRemindersActionScope = "trainarr.assignments.due_reminders.dispatch";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f3");

    public async Task<PendingAssignmentDueRemindersResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = AssignmentDueReminderRules.NormalizeBatchSize(batchSize);
        var candidates = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        var items = candidates
            .Select(x => new PendingAssignmentDueReminderItem(
                x.AssignmentId,
                x.StaffarrPersonId,
                x.DueAt,
                x.DueReminderCount,
                x.LastDueReminderSentAt,
                AssignmentDueReminderRules.ComputeHoursUntilDue(x.DueAt, asOf),
                AssignmentDueReminderRules.ComputeHoursUntilNextReminder(
                    x.DueAt,
                    x.LastDueReminderSentAt,
                    x.DueSoonLeadDays,
                    x.ReminderCooldownHours,
                    x.DueReminderCount,
                    x.MaxRemindersPerAssignment,
                    asOf)))
            .ToList();

        return new PendingAssignmentDueRemindersResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessAssignmentDueRemindersResponse> ProcessBatchAsync(
        ProcessAssignmentDueRemindersRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = AssignmentDueReminderRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var sent = new List<AssignmentDueReminderResult>();
        var skipped = new List<AssignmentDueReminderSkip>();
        var runStats = new Dictionary<Guid, (int Candidates, int Sent, int Skipped)>();

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
                var result = await SendReminderAsync(candidate, asOf, cancellationToken);
                sent.Add(result);
                stats = runStats[candidate.TenantId];
                stats.Sent++;
                runStats[candidate.TenantId] = stats;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new AssignmentDueReminderSkip(candidate.AssignmentId, ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.AssignmentDueReminderRuns.Add(new AssignmentDueReminderRun
            {
                Id = Guid.NewGuid(),
                TenantId = tenantIdKey,
                AsOfUtc = asOf,
                CandidatesFound = stats.Candidates,
                RemindersSentCount = stats.Sent,
                SkippedCount = stats.Skipped,
                CreatedAt = asOf,
            });
        }

        if (runStats.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        if (request.TenantId is Guid tenantId && sent.Count > 0)
        {
            await audit.WriteAsync(
                "trainarr.assignment_due_reminder.batch",
                tenantId,
                WorkerActorUserId,
                "assignment_due_reminder_run",
                $"{sent.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessAssignmentDueRemindersResponse(
            asOf,
            batchSize,
            candidates.Count,
            sent.Count,
            skipped.Count,
            sent,
            skipped);
    }

    public async Task<AssignmentDueReminderRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = AssignmentDueReminderRules.NormalizeRunListLimit(limit);
        var runs = await db.AssignmentDueReminderRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new AssignmentDueReminderRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.RemindersSentCount,
                x.SkippedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AssignmentDueReminderRunsResponse(runs);
    }

    private async Task<AssignmentDueReminderResult> SendReminderAsync(
        PendingAssignmentDueReminderCandidate candidate,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        if (!AssignmentDueReminderRules.IsDueForReminder(
                candidate.DueAt,
                candidate.LastDueReminderSentAt,
                candidate.DueSoonLeadDays,
                candidate.ReminderCooldownHours,
                candidate.DueReminderCount,
                candidate.MaxRemindersPerAssignment,
                asOfUtc))
        {
            throw new InvalidOperationException("Assignment is not due for a reminder.");
        }

        var assignment = await db.TrainingAssignments
            .FirstOrDefaultAsync(x => x.Id == candidate.AssignmentId, cancellationToken)
            ?? throw new InvalidOperationException($"Assignment {candidate.AssignmentId} was not found.");

        assignment.DueReminderCount += 1;
        assignment.LastDueReminderSentAt = asOfUtc;
        assignment.UpdatedAt = asOfUtc;

        Guid? notificationDispatchId = await notificationEnqueue.TryEnqueueRepeatableAsync(
            assignment.TenantId,
            TrainingNotificationEventKinds.AssignmentDueReminder,
            assignment.StaffarrPersonId,
            "training_assignment",
            assignment.Id,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return new AssignmentDueReminderResult(
            assignment.Id,
            assignment.DueReminderCount,
            notificationDispatchId);
    }

    private async Task<List<PendingAssignmentDueReminderCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var settingsQuery = db.TenantAssignmentDueReminderSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled);

        if (tenantId is Guid scopedTenantId)
        {
            settingsQuery = settingsQuery.Where(x => x.TenantId == scopedTenantId);
        }

        var tenantSettings = await settingsQuery.ToListAsync(cancellationToken);
        var results = new List<PendingAssignmentDueReminderCandidate>();

        foreach (var settings in tenantSettings)
        {
            if (results.Count >= batchSize)
            {
                break;
            }

            var snapshot = AssignmentDueReminderSettingsService.ToSnapshot(settings);
            var windowStart = asOfUtc.AddDays(-snapshot.DueSoonLeadDays);

            var assignments = await db.TrainingAssignments
                .AsNoTracking()
                .Where(x => x.TenantId == settings.TenantId
                    && x.DueAt != null
                    && x.DueAt > asOfUtc
                    && x.DueAt <= asOfUtc.AddDays(snapshot.DueSoonLeadDays)
                    && (x.Status == "assigned" || x.Status == "in_progress"))
                .OrderBy(x => x.DueAt)
                .Take(batchSize - results.Count)
                .Select(x => new PendingAssignmentDueReminderCandidate(
                    x.TenantId,
                    x.Id,
                    x.StaffarrPersonId,
                    x.DueAt!.Value,
                    x.DueReminderCount,
                    x.LastDueReminderSentAt,
                    snapshot.DueSoonLeadDays,
                    snapshot.ReminderCooldownHours,
                    snapshot.MaxRemindersPerAssignment))
                .ToListAsync(cancellationToken);

            foreach (var assignment in assignments)
            {
                if (AssignmentDueReminderRules.IsDueForReminder(
                        assignment.DueAt,
                        assignment.LastDueReminderSentAt,
                        assignment.DueSoonLeadDays,
                        assignment.ReminderCooldownHours,
                        assignment.DueReminderCount,
                        assignment.MaxRemindersPerAssignment,
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

    private sealed record PendingAssignmentDueReminderCandidate(
        Guid TenantId,
        Guid AssignmentId,
        Guid StaffarrPersonId,
        DateTimeOffset DueAt,
        int DueReminderCount,
        DateTimeOffset? LastDueReminderSentAt,
        int DueSoonLeadDays,
        int ReminderCooldownHours,
        int MaxRemindersPerAssignment);
}
