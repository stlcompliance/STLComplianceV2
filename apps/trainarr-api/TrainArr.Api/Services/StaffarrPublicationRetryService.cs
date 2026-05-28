using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class StaffarrPublicationRetryService(
    TrainArrDbContext db,
    StaffarrPublicationSettingsService settingsService,
    IntegrationSettingsService integrationSettingsService,
    StaffArrTrainingBlockerClient staffArrBlockerClient,
    StaffArrCertificationGrantClient staffArrGrantClient,
    StaffArrCertificationLifecycleClient staffArrLifecycleClient,
    ITrainArrAuditService audit)
{
    public const string ProcessRetriesActionScope = "trainarr.staffarr_publications.retry";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f5");

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task EnqueueAndAttemptAsync(
        Guid tenantId,
        Guid certificationPublicationId,
        Guid staffarrPersonId,
        string operationKind,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        var integrationSnapshot = await integrationSettingsService.LoadSnapshotAsync(tenantId, cancellationToken);
        if (!IntegrationSettingsRules.ResolveStaffArrPublicationDeliveryEnabled(integrationSnapshot))
        {
            return;
        }

        var duplicate = await db.StaffarrPublicationDeliveries.AnyAsync(
            x => x.TenantId == tenantId
                && x.CertificationPublicationId == certificationPublicationId
                && x.OperationKind == operationKind
                && x.DeliveryStatus == StaffarrPublicationDeliveryStatuses.Pending,
            cancellationToken);

        if (duplicate)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var delivery = new StaffarrPublicationDelivery
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CertificationPublicationId = certificationPublicationId,
            StaffarrPersonId = staffarrPersonId,
            OperationKind = operationKind,
            PayloadJson = payloadJson,
            DeliveryStatus = StaffarrPublicationDeliveryStatuses.Pending,
            AttemptCount = 0,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.StaffarrPublicationDeliveries.Add(delivery);
        await db.SaveChangesAsync(cancellationToken);

        await AttemptDeliveryAsync(delivery, cancellationToken);
    }

    public async Task<PendingStaffarrPublicationDeliveriesResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = StaffarrPublicationRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingAsync(tenantId, asOf, normalizedBatchSize, cancellationToken);

        return new PendingStaffarrPublicationDeliveriesResponse(
            asOf,
            normalizedBatchSize,
            items.Select(x => new PendingStaffarrPublicationDeliveryItem(
                x.Id,
                x.TenantId,
                x.CertificationPublicationId,
                x.OperationKind,
                x.StaffarrPersonId,
                x.AttemptCount,
                x.NextRetryAt,
                x.CreatedAt)).ToList());
    }

    public async Task<ProcessStaffarrPublicationRetriesResponse> ProcessBatchAsync(
        ProcessStaffarrPublicationRetriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = StaffarrPublicationRules.NormalizeBatchSize(request.BatchSize);
        var pending = await LoadPendingAsync(request.TenantId, asOf, batchSize, cancellationToken);

        var results = new List<StaffarrPublicationRetryResult>();
        var skipped = new List<StaffarrPublicationRetrySkip>();
        var deliveredCount = 0;
        var retriedCount = 0;
        var abandonedCount = 0;

        foreach (var delivery in pending)
        {
            try
            {
                var settings = await settingsService.LoadSnapshotAsync(delivery.TenantId, cancellationToken);
                if (!StaffarrPublicationRules.ShouldRetryForTenant(settings))
                {
                    skipped.Add(new StaffarrPublicationRetrySkip(
                        delivery.Id,
                        "tenant_retry_disabled"));
                    continue;
                }

                var beforeStatus = delivery.DeliveryStatus;
                await AttemptDeliveryAsync(delivery, cancellationToken);

                if (string.Equals(delivery.DeliveryStatus, StaffarrPublicationDeliveryStatuses.Delivered, StringComparison.OrdinalIgnoreCase))
                {
                    deliveredCount++;
                }
                else if (string.Equals(delivery.DeliveryStatus, StaffarrPublicationDeliveryStatuses.Abandoned, StringComparison.OrdinalIgnoreCase))
                {
                    abandonedCount++;
                }
                else if (delivery.AttemptCount > 0)
                {
                    retriedCount++;
                }

                results.Add(new StaffarrPublicationRetryResult(
                    delivery.Id,
                    delivery.DeliveryStatus,
                    delivery.AttemptCount));

                if (!string.Equals(beforeStatus, delivery.DeliveryStatus, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(delivery.DeliveryStatus, StaffarrPublicationDeliveryStatuses.Delivered, StringComparison.OrdinalIgnoreCase))
                {
                    await audit.WriteAsync(
                        "trainarr.staffarr_publication.delivered",
                        delivery.TenantId,
                        WorkerActorUserId,
                        "staffarr_publication_delivery",
                        delivery.Id.ToString(),
                        "delivered",
                        reasonCode: delivery.OperationKind,
                        cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new StaffarrPublicationRetrySkip(delivery.Id, ex.Message));
            }
        }

        if (results.Count > 0 && request.TenantId is Guid tenantId)
        {
            await audit.WriteAsync(
                "trainarr.staffarr_publication_retry.batch",
                tenantId,
                WorkerActorUserId,
                "staffarr_publication_delivery",
                $"{results.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessStaffarrPublicationRetriesResponse(
            asOf,
            batchSize,
            pending.Count,
            deliveredCount,
            retriedCount,
            abandonedCount,
            skipped.Count,
            results,
            skipped);
    }

    public async Task<StaffarrPublicationDeliveriesResponse> ListRecentAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = StaffarrPublicationRules.NormalizeDeliveryListLimit(limit);
        var rows = await db.StaffarrPublicationDeliveries
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new StaffarrPublicationDeliveriesResponse(
            rows.Select(x => new StaffarrPublicationDeliveryItem(
                x.Id,
                x.CertificationPublicationId,
                x.OperationKind,
                x.DeliveryStatus,
                x.StaffarrPersonId,
                x.AttemptCount,
                x.HttpStatusCode,
                x.ErrorMessage,
                x.CreatedAt,
                x.NextRetryAt,
                x.DeliveredAt)).ToList());
    }

    private async Task<List<StaffarrPublicationDelivery>> LoadPendingAsync(
        Guid? tenantId,
        DateTimeOffset asOfUtc,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.StaffarrPublicationDeliveries
            .Where(x => x.DeliveryStatus == StaffarrPublicationDeliveryStatuses.Pending
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

    private async Task AttemptDeliveryAsync(
        StaffarrPublicationDelivery delivery,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.LoadSnapshotAsync(delivery.TenantId, cancellationToken);
        var maxAttempts = StaffarrPublicationRules.NormalizeMaxAttempts(settings?.MaxAttempts);
        var retryIntervalMinutes = StaffarrPublicationRules.NormalizeRetryIntervalMinutes(settings?.RetryIntervalMinutes);
        var now = DateTimeOffset.UtcNow;

        delivery.AttemptCount += 1;
        delivery.UpdatedAt = now;

        try
        {
            await ExecuteDeliveryAsync(delivery.OperationKind, delivery.PayloadJson, cancellationToken);
            delivery.DeliveryStatus = StaffarrPublicationDeliveryStatuses.Delivered;
            delivery.DeliveredAt = now;
            delivery.NextRetryAt = null;
            delivery.HttpStatusCode = 200;
            delivery.ErrorMessage = null;
        }
        catch (StlApiException ex)
        {
            delivery.HttpStatusCode = ex.StatusCode;
            delivery.ErrorMessage = Truncate(ex.Message, 512);
            await ApplyFailureAsync(delivery, maxAttempts, retryIntervalMinutes, now);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            delivery.HttpStatusCode = null;
            delivery.ErrorMessage = Truncate(ex.Message, 512);
            await ApplyFailureAsync(delivery, maxAttempts, retryIntervalMinutes, now);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Task ApplyFailureAsync(
        StaffarrPublicationDelivery delivery,
        int maxAttempts,
        int retryIntervalMinutes,
        DateTimeOffset now)
    {
        if (delivery.AttemptCount >= maxAttempts)
        {
            delivery.DeliveryStatus = StaffarrPublicationDeliveryStatuses.Abandoned;
            delivery.NextRetryAt = null;
        }
        else
        {
            delivery.DeliveryStatus = StaffarrPublicationDeliveryStatuses.Pending;
            delivery.NextRetryAt = StaffarrPublicationRules.ComputeNextRetryAt(now, retryIntervalMinutes);
        }

        return Task.CompletedTask;
    }

    private async Task ExecuteDeliveryAsync(
        string operationKind,
        string payloadJson,
        CancellationToken cancellationToken)
    {
        switch (operationKind)
        {
            case StaffarrPublicationOperationKinds.TrainingBlockerPublish:
            {
                var payload = JsonSerializer.Deserialize<StaffArrIngestTrainingBlockerPayload>(payloadJson, JsonOptions)
                    ?? throw new StlApiException("staffarr_publication.invalid_payload", "Training blocker payload is invalid.", 500);
                await staffArrBlockerClient.PublishBlockerAsync(payload, cancellationToken);
                break;
            }
            case StaffarrPublicationOperationKinds.TrainingBlockerClear:
            {
                var payload = JsonSerializer.Deserialize<StaffArrClearTrainingBlockerPayload>(payloadJson, JsonOptions)
                    ?? throw new StlApiException("staffarr_publication.invalid_payload", "Training blocker clear payload is invalid.", 500);
                await staffArrBlockerClient.ClearBlockerAsync(payload, cancellationToken);
                break;
            }
            case StaffarrPublicationOperationKinds.QualificationGrant:
            {
                var payload = JsonSerializer.Deserialize<StaffArrIngestCertificationGrantPayload>(payloadJson, JsonOptions)
                    ?? throw new StlApiException("staffarr_publication.invalid_payload", "Qualification grant payload is invalid.", 500);
                await staffArrGrantClient.IngestGrantAsync(payload, cancellationToken);
                break;
            }
            case StaffarrPublicationOperationKinds.QualificationLifecycle:
            {
                var payload = JsonSerializer.Deserialize<StaffArrIngestCertificationLifecyclePayload>(payloadJson, JsonOptions)
                    ?? throw new StlApiException("staffarr_publication.invalid_payload", "Qualification lifecycle payload is invalid.", 500);
                await staffArrLifecycleClient.IngestLifecycleAsync(payload, cancellationToken);
                break;
            }
            default:
                throw new StlApiException(
                    "staffarr_publication.unknown_operation",
                    $"Unknown StaffArr publication operation '{operationKind}'.",
                    500);
        }
    }

    public static string SerializePayload<T>(T payload) =>
        JsonSerializer.Serialize(payload, JsonOptions);

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
