using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ApprovalReminderWorkerService(
    SupplyArrDbContext db,
    ApprovalReminderSettingsService settingsService,
    ProcurementNotificationEnqueueService notificationEnqueue,
    ISupplyArrAuditService audit)
{
    public const string ProcessApprovalRemindersActionScope = "supplyarr.approval_reminders.dispatch";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fd");

    public async Task<PendingApprovalRemindersResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = ApprovalReminderRules.NormalizeBatchSize(batchSize);
        var candidates = await LoadPendingCandidatesAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        var items = candidates
            .Select(x => new PendingApprovalReminderItem(
                x.SubjectType,
                x.SubjectId,
                x.DocumentKey,
                x.Title,
                x.DocumentStatus,
                x.PendingSince,
                x.LastReminderSentAt,
                x.ReminderCount,
                x.HoursPending,
                x.HoursUntilNextReminder))
            .ToList();

        return new PendingApprovalRemindersResponse(asOf, normalizedBatchSize, items);
    }

    public async Task<ProcessApprovalRemindersResponse> ProcessBatchAsync(
        ProcessApprovalRemindersRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = ApprovalReminderRules.NormalizeBatchSize(request.BatchSize);
        var candidates = await LoadPendingCandidatesAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var sent = new List<ApprovalReminderResult>();
        var skipped = new List<ApprovalReminderSkip>();
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
                skipped.Add(new ApprovalReminderSkip(candidate.SubjectType, candidate.SubjectId, ex.Message));
                stats = runStats[candidate.TenantId];
                stats.Skipped++;
                runStats[candidate.TenantId] = stats;
            }
        }

        foreach (var (tenantIdKey, stats) in runStats)
        {
            db.ApprovalReminderRuns.Add(new ApprovalReminderRun
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
                "supplyarr.approval_reminder.batch",
                tenantId,
                WorkerActorUserId,
                "approval_reminder_run",
                $"{sent.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessApprovalRemindersResponse(
            asOf,
            batchSize,
            candidates.Count,
            sent.Count,
            skipped.Count,
            sent,
            skipped);
    }

    public async Task<ApprovalReminderRunsResponse> ListRecentRunsAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = ApprovalReminderRules.NormalizeRunListLimit(limit);
        var runs = await db.ApprovalReminderRuns
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedLimit)
            .Select(x => new ApprovalReminderRunItem(
                x.Id,
                x.AsOfUtc,
                x.CandidatesFound,
                x.RemindersSentCount,
                x.SkippedCount,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new ApprovalReminderRunsResponse(runs);
    }

    private async Task<ApprovalReminderResult> SendReminderAsync(
        PendingApprovalReminderCandidate candidate,
        DateTimeOffset asOfUtc,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.LoadSnapshotAsync(candidate.TenantId, cancellationToken)
            ?? throw new InvalidOperationException("Approval reminder settings are not configured for this tenant.");

        var thresholdHours = ApprovalReminderRules.GetThresholdHours(settings, candidate.SubjectType);
        if (!ApprovalReminderRules.IsDueForReminder(
                candidate.PendingSince,
                candidate.LastReminderSentAt,
                thresholdHours,
                settings.ReminderCooldownHours,
                candidate.ReminderCount,
                settings.MaxRemindersPerSubject,
                asOfUtc))
        {
            throw new InvalidOperationException("Subject is not due for an approval reminder.");
        }

        Guid? notificationDispatchId = null;
        if (ApprovalReminderRules.ShouldNotify(settings, candidate.SubjectType))
        {
            var eventKind = ApprovalReminderRules.GetReminderEventKind(candidate.SubjectType);
            notificationDispatchId = await notificationEnqueue.TryEnqueueRepeatableAsync(
                candidate.TenantId,
                eventKind,
                candidate.SupplierId,
                ApprovalReminderRules.GetRelatedEntityType(candidate.SubjectType),
                candidate.SubjectId,
                cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        var state = await db.ApprovalReminderStates
            .FirstOrDefaultAsync(
                x => x.TenantId == candidate.TenantId
                    && x.SubjectType == candidate.SubjectType
                    && x.SubjectId == candidate.SubjectId,
                cancellationToken);

        if (state is null)
        {
            state = new ApprovalReminderState
            {
                Id = Guid.NewGuid(),
                TenantId = candidate.TenantId,
                SubjectType = candidate.SubjectType,
                SubjectId = candidate.SubjectId,
                CreatedAt = now,
            };
            db.ApprovalReminderStates.Add(state);
        }

        state.DocumentKey = candidate.DocumentKey;
        state.Title = candidate.Title;
        state.DocumentStatus = candidate.DocumentStatus;
        state.SupplierId = candidate.SupplierId;
        state.PendingSince = candidate.PendingSince;
        state.LastReminderSentAt = asOfUtc;
        state.ReminderCount = candidate.ReminderCount + 1;
        state.LastReminderEventKind = ApprovalReminderRules.GetReminderEventKind(candidate.SubjectType);
        state.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return new ApprovalReminderResult(
            candidate.SubjectType,
            candidate.SubjectId,
            candidate.DocumentKey,
            state.ReminderCount,
            notificationDispatchId);
    }

    private async Task<IReadOnlyList<PendingApprovalReminderCandidate>> LoadPendingCandidatesAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var enabledTenantIds = await db.TenantApprovalReminderSettings
            .AsNoTracking()
            .Where(x => x.IsEnabled && (tenantId == null || x.TenantId == tenantId))
            .Select(x => x.TenantId)
            .ToListAsync(cancellationToken);

        if (enabledTenantIds.Count == 0)
        {
            return [];
        }

        var settingsByTenant = await db.TenantApprovalReminderSettings
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .ToDictionaryAsync(x => x.TenantId, cancellationToken);

        var stateLookup = await db.ApprovalReminderStates
            .AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId))
            .ToDictionaryAsync(
                x => (x.TenantId, x.SubjectType, x.SubjectId),
                x => x,
                cancellationToken);

        var candidates = new List<PendingApprovalReminderCandidate>();

        var submittedPrStatus = PurchaseRequestStatuses.Submitted;
        var purchaseRequests = await db.PurchaseRequests.AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId) && x.Status == submittedPrStatus)
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                x.RequestKey,
                x.Title,
                x.Status,
                x.SupplierId,
                PendingSince = x.SubmittedAt ?? x.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        foreach (var pr in purchaseRequests)
        {
            if (!settingsByTenant.TryGetValue(pr.TenantId, out var settingsEntity))
            {
                continue;
            }

            var settings = ApprovalReminderSettingsService.ToSnapshot(settingsEntity);
            stateLookup.TryGetValue(
                (pr.TenantId, ApprovalReminderSubjectTypes.PurchaseRequest, pr.Id),
                out var state);

            var reminderCount = state?.ReminderCount ?? 0;
            var lastReminderSentAt = state?.LastReminderSentAt;
            var thresholdHours = ApprovalReminderRules.GetThresholdHours(
                settings,
                ApprovalReminderSubjectTypes.PurchaseRequest);

            if (!ApprovalReminderRules.IsDueForReminder(
                    pr.PendingSince,
                    lastReminderSentAt,
                    thresholdHours,
                    settings.ReminderCooldownHours,
                    reminderCount,
                    settings.MaxRemindersPerSubject,
                    asOfUtc))
            {
                continue;
            }

            var hoursPending = ApprovalReminderRules.ComputeHoursPending(pr.PendingSince, asOfUtc);
            var hoursUntilNext = ApprovalReminderRules.ComputeHoursUntilNextReminder(
                pr.PendingSince,
                lastReminderSentAt,
                thresholdHours,
                settings.ReminderCooldownHours,
                reminderCount,
                settings.MaxRemindersPerSubject,
                asOfUtc) ?? 0;

            candidates.Add(new PendingApprovalReminderCandidate(
                pr.TenantId,
                ApprovalReminderSubjectTypes.PurchaseRequest,
                pr.Id,
                pr.RequestKey,
                pr.Title,
                pr.Status,
                pr.SupplierId,
                pr.PendingSince,
                lastReminderSentAt,
                reminderCount,
                hoursPending,
                hoursUntilNext));
        }

        var draftPoStatus = PurchaseOrderStatuses.Draft;
        var purchaseOrders = await db.PurchaseOrders.AsNoTracking()
            .Where(x => enabledTenantIds.Contains(x.TenantId) && x.Status == draftPoStatus)
            .Select(x => new
            {
                x.TenantId,
                x.Id,
                x.OrderKey,
                x.Title,
                x.Status,
                x.SupplierId,
                PendingSince = x.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        foreach (var po in purchaseOrders)
        {
            if (!settingsByTenant.TryGetValue(po.TenantId, out var settingsEntity))
            {
                continue;
            }

            var settings = ApprovalReminderSettingsService.ToSnapshot(settingsEntity);
            stateLookup.TryGetValue(
                (po.TenantId, ApprovalReminderSubjectTypes.PurchaseOrder, po.Id),
                out var state);

            var reminderCount = state?.ReminderCount ?? 0;
            var lastReminderSentAt = state?.LastReminderSentAt;
            var thresholdHours = ApprovalReminderRules.GetThresholdHours(
                settings,
                ApprovalReminderSubjectTypes.PurchaseOrder);

            if (!ApprovalReminderRules.IsDueForReminder(
                    po.PendingSince,
                    lastReminderSentAt,
                    thresholdHours,
                    settings.ReminderCooldownHours,
                    reminderCount,
                    settings.MaxRemindersPerSubject,
                    asOfUtc))
            {
                continue;
            }

            var hoursPending = ApprovalReminderRules.ComputeHoursPending(po.PendingSince, asOfUtc);
            var hoursUntilNext = ApprovalReminderRules.ComputeHoursUntilNextReminder(
                po.PendingSince,
                lastReminderSentAt,
                thresholdHours,
                settings.ReminderCooldownHours,
                reminderCount,
                settings.MaxRemindersPerSubject,
                asOfUtc) ?? 0;

            candidates.Add(new PendingApprovalReminderCandidate(
                po.TenantId,
                ApprovalReminderSubjectTypes.PurchaseOrder,
                po.Id,
                po.OrderKey,
                po.Title,
                po.Status,
                po.SupplierId,
                po.PendingSince,
                lastReminderSentAt,
                reminderCount,
                hoursPending,
                hoursUntilNext));
        }

        return candidates
            .OrderByDescending(x => x.HoursPending)
            .Take(batchSize)
            .ToList();
    }

    private sealed record PendingApprovalReminderCandidate(
        Guid TenantId,
        string SubjectType,
        Guid SubjectId,
        string DocumentKey,
        string Title,
        string DocumentStatus,
        Guid? SupplierId,
        DateTimeOffset PendingSince,
        DateTimeOffset? LastReminderSentAt,
        int ReminderCount,
        double HoursPending,
        double HoursUntilNextReminder);
}
