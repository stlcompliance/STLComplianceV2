using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;

using TrainArr.Api.Contracts;

using TrainArr.Api.Data;

using TrainArr.Api.Entities;



namespace TrainArr.Api.Services;



public sealed class TrainingNotificationDispatchService(

    TrainArrDbContext db,

    TrainingNotificationSettingsService settingsService,

    IHttpClientFactory httpClientFactory,

    ITrainArrAuditService audit)

{

    public const string ProcessNotificationsActionScope = "trainarr.notifications.dispatch";



    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f2");



    public const string WebhookHttpClientName = "TrainArrTrainingNotificationWebhook";



    public async Task<PendingTrainingNotificationsResponse> ListPendingAsync(

        Guid? tenantId,

        DateTimeOffset? asOfUtc,

        int? batchSize,

        CancellationToken cancellationToken = default)

    {

        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;

        var normalizedBatchSize = TrainingNotificationRules.NormalizeBatchSize(batchSize);

        var items = await LoadPendingAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);



        return new PendingTrainingNotificationsResponse(

            asOf,

            normalizedBatchSize,

            items.Select(x => new PendingTrainingNotificationItem(

                x.Id,

                x.TenantId,

                x.EventKind,

                x.StaffarrPersonId,

                x.AttemptCount,

                x.NextRetryAt,

                x.CreatedAt)).ToList());

    }



    public async Task<ProcessTrainingNotificationsResponse> ProcessBatchAsync(

        ProcessTrainingNotificationsRequest request,

        CancellationToken cancellationToken = default)

    {

        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;

        var batchSize = TrainingNotificationRules.NormalizeBatchSize(request.BatchSize);

        var enqueuedExpiring = await EnqueueExpiringQualificationsAsync(

            request.TenantId,

            asOf,

            batchSize,

            cancellationToken);



        var pending = await LoadPendingAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var dispatches = new List<TrainingNotificationDispatchResult>();

        var skipped = new List<TrainingNotificationDispatchSkip>();

        var dispatchedCount = 0;

        var retriedCount = 0;

        var abandonedCount = 0;



        foreach (var item in pending)

        {

            try

            {

                var beforeAttempts = item.AttemptCount;

                var result = await DispatchOneAsync(item, cancellationToken);

                dispatches.Add(result);



                if (string.Equals(result.DispatchStatus, TrainingNotificationDispatchStatuses.Sent, StringComparison.OrdinalIgnoreCase)

                    || string.Equals(result.DispatchStatus, TrainingNotificationDispatchStatuses.Skipped, StringComparison.OrdinalIgnoreCase))

                {

                    dispatchedCount++;

                }

                else if (string.Equals(result.DispatchStatus, TrainingNotificationDispatchStatuses.Abandoned, StringComparison.OrdinalIgnoreCase))

                {

                    abandonedCount++;

                }

                else if (item.AttemptCount > beforeAttempts)

                {

                    retriedCount++;

                }

            }

            catch (Exception ex) when (ex is not OperationCanceledException)

            {

                skipped.Add(new TrainingNotificationDispatchSkip(item.Id, ex.Message));

            }

        }



        if (dispatches.Count > 0 && request.TenantId is Guid tenantId)

        {

            await audit.WriteAsync(

                "trainarr.notification_dispatch.batch",

                tenantId,

                WorkerActorUserId,

                "training_notification_dispatch",

                $"{dispatches.Count}",

                "success",

                cancellationToken: cancellationToken);

        }



        return new ProcessTrainingNotificationsResponse(

            asOf,

            batchSize,

            enqueuedExpiring,

            pending.Count,

            dispatchedCount,

            retriedCount,

            abandonedCount,

            skipped.Count,

            dispatches,

            skipped);

    }



    public async Task<TrainingNotificationDispatchesResponse> ListRecentAsync(

        Guid tenantId,

        int? limit,

        CancellationToken cancellationToken = default)

    {

        var take = TrainingNotificationRules.NormalizeDispatchListLimit(limit);

        var rows = await db.TrainingNotificationDispatches

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId)

            .OrderByDescending(x => x.CreatedAt)

            .Take(take)

            .ToListAsync(cancellationToken);



        var items = rows

            .Select(x => new TrainingNotificationDispatchItem(

                x.Id,

                x.EventKind,

                x.DispatchStatus,

                x.StaffarrPersonId,

                x.RelatedEntityType,

                x.RelatedEntityId,

                x.AttemptCount,

                x.WebhookHost,

                x.HttpStatusCode,

                x.ErrorMessage,

                x.CreatedAt,

                x.NextRetryAt,

                x.DispatchedAt))

            .ToList();



        return new TrainingNotificationDispatchesResponse(items);

    }



    private async Task<int> EnqueueExpiringQualificationsAsync(

        Guid? tenantId,

        DateTimeOffset asOfUtc,

        int batchSize,

        CancellationToken cancellationToken)

    {

        var settingsQuery = db.TenantTrainingNotificationSettings

            .AsNoTracking()

            .Where(x => x.IsEnabled && x.NotifyOnQualificationExpiring);



        if (tenantId is Guid scopedTenantId)

        {

            settingsQuery = settingsQuery.Where(x => x.TenantId == scopedTenantId);

        }



        var tenantSettings = await settingsQuery.ToListAsync(cancellationToken);

        var enqueued = 0;



        foreach (var settings in tenantSettings)

        {

            if (enqueued >= batchSize)

            {

                break;

            }



            var windowEnd = asOfUtc.AddDays(settings.ExpiringLeadDays);

            var rawCandidates = await (

                from issue in db.QualificationIssues.AsNoTracking()

                join publication in db.CertificationPublications.AsNoTracking()

                    on issue.GrantPublicationId equals publication.Id into publications

                from publication in publications.DefaultIfEmpty()

                where issue.TenantId == settings.TenantId

                    && QualificationExpirationRules.ExpirableStatuses.Contains(issue.Status)

                select new

                {

                    issue.Id,

                    issue.TenantId,

                    issue.StaffarrPersonId,

                    EffectiveExpiresAt = issue.ExpiresAt ?? publication.ExpiresAt,

                })

                .ToListAsync(cancellationToken);



            var candidates = rawCandidates

                .Where(x => x.EffectiveExpiresAt is not null

                    && x.EffectiveExpiresAt > asOfUtc

                    && x.EffectiveExpiresAt <= windowEnd)

                .Take(batchSize - enqueued)

                .ToList();



            foreach (var candidate in candidates)

            {

                var snapshot = TrainingNotificationSettingsService.ToSnapshot(settings);

                if (!TrainingNotificationRules.ShouldNotifyForEvent(

                        snapshot,

                        TrainingNotificationEventKinds.QualificationExpiring))

                {

                    continue;

                }



                var duplicate = await db.TrainingNotificationDispatches.AnyAsync(

                    x => x.TenantId == candidate.TenantId

                        && x.EventKind == TrainingNotificationEventKinds.QualificationExpiring

                        && x.RelatedEntityType == "qualification_issue"

                        && x.RelatedEntityId == candidate.Id

                        && (x.DispatchStatus == TrainingNotificationDispatchStatuses.Pending

                            || x.DispatchStatus == TrainingNotificationDispatchStatuses.Sent),

                    cancellationToken);



                if (duplicate)

                {

                    continue;

                }



                var now = DateTimeOffset.UtcNow;

                db.TrainingNotificationDispatches.Add(new TrainingNotificationDispatch

                {

                    Id = Guid.NewGuid(),

                    TenantId = candidate.TenantId,

                    EventKind = TrainingNotificationEventKinds.QualificationExpiring,

                    StaffarrPersonId = candidate.StaffarrPersonId,

                    RelatedEntityType = "qualification_issue",

                    RelatedEntityId = candidate.Id,

                    DispatchStatus = TrainingNotificationDispatchStatuses.Pending,

                    AttemptCount = 0,

                    CreatedAt = now,

                    UpdatedAt = now,

                });

                enqueued++;

            }

        }



        if (enqueued > 0)

        {

            await db.SaveChangesAsync(cancellationToken);

        }



        return enqueued;

    }



    private async Task<List<TrainingNotificationDispatch>> LoadPendingAsync(

        Guid? tenantId,

        DateTimeOffset asOfUtc,

        int batchSize,

        CancellationToken cancellationToken)

    {

        var query = db.TrainingNotificationDispatches

            .Where(x => x.DispatchStatus == TrainingNotificationDispatchStatuses.Pending

                && (x.NextRetryAt == null || x.NextRetryAt <= asOfUtc));



        if (tenantId is Guid scopedTenantId)

        {

            query = query.Where(x => x.TenantId == scopedTenantId);

        }



        return await query

            .OrderBy(x => x.NextRetryAt ?? x.CreatedAt)

            .ThenBy(x => x.CreatedAt)

            .Take(batchSize)

            .ToListAsync(cancellationToken);

    }



    private async Task<TrainingNotificationDispatchResult> DispatchOneAsync(

        TrainingNotificationDispatch item,

        CancellationToken cancellationToken)

    {

        var settings = await settingsService.LoadSnapshotAsync(item.TenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;



        if (settings is null || !TrainingNotificationRules.ShouldNotifyForEvent(settings, item.EventKind))

        {

            return await MarkAsync(

                item,

                TrainingNotificationDispatchStatuses.Skipped,

                settings?.NotificationWebhookUrl,

                null,

                "notifications_disabled_or_webhook_missing",

                now,

                cancellationToken);

        }



        var maxAttempts = TrainingNotificationRules.NormalizeMaxAttempts(settings.MaxAttempts);

        var retryIntervalMinutes = TrainingNotificationRules.NormalizeRetryIntervalMinutes(settings.RetryIntervalMinutes);

        item.AttemptCount += 1;

        item.UpdatedAt = now;



        var payload = BuildPayload(item);

        var webhookUrl = settings.NotificationWebhookUrl!;



        try

        {

            var client = httpClientFactory.CreateClient(WebhookHttpClientName);

            using var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);

            var statusCode = (int)response.StatusCode;



            if (response.IsSuccessStatusCode)

            {

                return await MarkAsync(

                    item,

                    TrainingNotificationDispatchStatuses.Sent,

                    webhookUrl,

                    statusCode,

                    null,

                    now,

                    cancellationToken);

            }



            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            var truncated = body.Length > 200 ? body[..200] : body;

            return await ApplyDispatchFailureAsync(

                item,

                webhookUrl,

                statusCode,

                $"http_{statusCode}:{truncated}",

                maxAttempts,

                retryIntervalMinutes,

                now,

                cancellationToken);

        }

        catch (Exception ex) when (ex is not OperationCanceledException)

        {

            return await ApplyDispatchFailureAsync(

                item,

                webhookUrl,

                null,

                ex.Message,

                maxAttempts,

                retryIntervalMinutes,

                now,

                cancellationToken);

        }

    }



    private async Task<TrainingNotificationDispatchResult> ApplyDispatchFailureAsync(

        TrainingNotificationDispatch item,

        string webhookUrl,

        int? httpStatusCode,

        string errorMessage,

        int maxAttempts,

        int retryIntervalMinutes,

        DateTimeOffset now,

        CancellationToken cancellationToken)

    {

        item.WebhookHost = TrainingNotificationRules.TryGetWebhookHost(webhookUrl);

        item.HttpStatusCode = httpStatusCode;

        item.ErrorMessage = Truncate(errorMessage, 512);



        if (item.AttemptCount >= maxAttempts)

        {

            item.DispatchStatus = TrainingNotificationDispatchStatuses.Abandoned;

            item.NextRetryAt = null;

            item.DispatchedAt = now;

        }

        else

        {

            item.DispatchStatus = TrainingNotificationDispatchStatuses.Pending;

            item.NextRetryAt = TrainingNotificationRules.ComputeNextRetryAt(now, retryIntervalMinutes);

            item.DispatchedAt = null;

        }



        await db.SaveChangesAsync(cancellationToken);



        await audit.WriteAsync(

            "trainarr.notification_dispatch",

            item.TenantId,

            WorkerActorUserId,

            item.RelatedEntityType,

            item.RelatedEntityId.ToString(),

            item.DispatchStatus,

            reasonCode: item.EventKind,

            cancellationToken: cancellationToken);



        return new TrainingNotificationDispatchResult(item.Id, item.DispatchStatus);

    }



    private static object BuildPayload(TrainingNotificationDispatch item) =>

        item.EventKind switch

        {

            TrainingNotificationEventKinds.AssignmentCreated => new

            {

                @event = "trainarr.training.assignment_created",

                tenantId = item.TenantId,

                staffarrPersonId = item.StaffarrPersonId,

                trainingAssignmentId = item.RelatedEntityId,

            },

            TrainingNotificationEventKinds.AssignmentCompleted => new

            {

                @event = "trainarr.training.assignment_completed",

                tenantId = item.TenantId,

                staffarrPersonId = item.StaffarrPersonId,

                trainingAssignmentId = item.RelatedEntityId,

            },

            TrainingNotificationEventKinds.QualificationExpiring => new

            {

                @event = "trainarr.qualification.expiring",

                tenantId = item.TenantId,

                staffarrPersonId = item.StaffarrPersonId,

                qualificationIssueId = item.RelatedEntityId,

            },

            TrainingNotificationEventKinds.QualificationIssued => new

            {

                @event = "trainarr.qualification.issued",

                tenantId = item.TenantId,

                staffarrPersonId = item.StaffarrPersonId,

                qualificationIssueId = item.RelatedEntityId,

            },

            TrainingNotificationEventKinds.QualificationSuspended => new

            {

                @event = "trainarr.qualification.suspended",

                tenantId = item.TenantId,

                staffarrPersonId = item.StaffarrPersonId,

                qualificationIssueId = item.RelatedEntityId,

            },

            TrainingNotificationEventKinds.QualificationRevoked => new

            {

                @event = "trainarr.qualification.revoked",

                tenantId = item.TenantId,

                staffarrPersonId = item.StaffarrPersonId,

                qualificationIssueId = item.RelatedEntityId,

            },

            TrainingNotificationEventKinds.QualificationExpired => new

            {

                @event = "trainarr.qualification.expired",

                tenantId = item.TenantId,

                staffarrPersonId = item.StaffarrPersonId,

                qualificationIssueId = item.RelatedEntityId,

            },

            TrainingNotificationEventKinds.AssignmentDueReminder => new

            {

                @event = "trainarr.training.assignment_due_reminder",

                tenantId = item.TenantId,

                staffarrPersonId = item.StaffarrPersonId,

                trainingAssignmentId = item.RelatedEntityId,

            },

            TrainingNotificationEventKinds.AssignmentOverdueEscalation => new

            {

                @event = "trainarr.training.assignment_overdue_escalation",

                tenantId = item.TenantId,

                staffarrPersonId = item.StaffarrPersonId,

                trainingAssignmentId = item.RelatedEntityId,

            },

            _ => new

            {

                @event = "trainarr.notification.unknown",

                tenantId = item.TenantId,

                eventKind = item.EventKind,

                relatedEntityId = item.RelatedEntityId,

            },

        };



    private async Task<TrainingNotificationDispatchResult> MarkAsync(

        TrainingNotificationDispatch item,

        string status,

        string? webhookUrl,

        int? httpStatusCode,

        string? errorMessage,

        DateTimeOffset dispatchedAt,

        CancellationToken cancellationToken)

    {

        item.DispatchStatus = status;

        item.WebhookHost = TrainingNotificationRules.TryGetWebhookHost(webhookUrl);

        item.HttpStatusCode = httpStatusCode;

        item.ErrorMessage = errorMessage is null ? null : Truncate(errorMessage, 512);

        item.DispatchedAt = dispatchedAt;

        item.NextRetryAt = null;

        item.UpdatedAt = dispatchedAt;

        await db.SaveChangesAsync(cancellationToken);



        await audit.WriteAsync(

            "trainarr.notification_dispatch",

            item.TenantId,

            WorkerActorUserId,

            item.RelatedEntityType,

            item.RelatedEntityId.ToString(),

            status,

            reasonCode: item.EventKind,

            cancellationToken: cancellationToken);



        return new TrainingNotificationDispatchResult(item.Id, status);

    }



    private static string Truncate(string value, int maxLength) =>

        value.Length <= maxLength ? value : value[..maxLength];

}



