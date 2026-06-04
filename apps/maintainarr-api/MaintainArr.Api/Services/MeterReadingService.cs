using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class MeterReadingService(
    MaintainArrDbContext db,
    AssetMeterService assetMeterService,
    MeterPmForecastService forecastService,
    IMaintainArrAuditService audit,
    MaintenancePlatformOutboxEnqueueService platformOutboxEnqueue)
{
    private const int DefaultListLimit = 50;
    private const int MaxListLimit = 200;

    public async Task<IReadOnlyList<MeterReadingResponse>> ListAsync(
        Guid tenantId,
        Guid assetMeterId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        _ = await assetMeterService.GetAsync(tenantId, assetMeterId, cancellationToken);
        var take = NormalizeLimit(limit);

        return await db.MeterReadings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetMeterId == assetMeterId)
            .OrderByDescending(x => x.ReadAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<MeterReadingResponse> RecordAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetMeterId,
        RecordMeterReadingRequest request,
        CancellationToken cancellationToken = default)
    {
        var meter = await assetMeterService.GetEntityAsync(tenantId, assetMeterId, cancellationToken);
        try
        {
            return await RecordInternalAsync(
                tenantId,
                actorUserId,
                meter,
                request,
                cancellationToken);
        }
        catch (StlApiException ex)
        {
            await TryEnqueueRejectedAsync(
                tenantId,
                actorUserId,
                meter,
                request.ReadingValue,
                request.ReadAt,
                ex,
                cancellationToken);
            throw;
        }
    }

    public async Task<MeterReadingResponse> CorrectAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetMeterId,
        CorrectMeterReadingRequest request,
        CancellationToken cancellationToken = default)
    {
        var meter = await assetMeterService.GetEntityAsync(tenantId, assetMeterId, cancellationToken);
        try
        {
            var reason = NormalizeCorrectionReason(request.Reason);
            return await RecordInternalAsync(
                tenantId,
                actorUserId,
                meter,
                new RecordMeterReadingRequest(
                    request.ReadingValue,
                    request.ReadAt,
                    reason,
                    true),
                cancellationToken);
        }
        catch (StlApiException ex)
        {
            await TryEnqueueRejectedAsync(
                tenantId,
                actorUserId,
                meter,
                request.ReadingValue,
                request.ReadAt,
                ex,
                cancellationToken);
            throw;
        }
    }

    private static MeterReadingResponse Map(MeterReading entity) =>
        new(
            entity.Id,
            entity.AssetMeterId,
            entity.AssetId,
            entity.ReadingValue,
            entity.DeltaFromPrevious,
            entity.ReadAt,
            entity.RecordedByUserId,
            entity.Notes,
            entity.IsCorrection,
            entity.CreatedAt);

    private static int NormalizeLimit(int? limit)
    {
        if (limit is null or < 1)
        {
            return DefaultListLimit;
        }

        return Math.Min(limit.Value, MaxListLimit);
    }

    private static string NormalizeNotes(string notes) =>
        notes.Trim().Length <= 512 ? notes.Trim() : notes.Trim()[..512];

    private static string NormalizeCorrectionReason(string reason)
    {
        var normalized = NormalizeNotes(reason);
        if (normalized.Length < 3)
        {
            throw new StlApiException(
                "meter_reading.correction_reason_required",
                "Meter correction reason must be at least 3 characters.",
                400);
        }

        return normalized;
    }

    private async Task<MeterReadingResponse> RecordInternalAsync(
        Guid tenantId,
        Guid actorUserId,
        AssetMeter meter,
        RecordMeterReadingRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(meter.Status, MeterStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "meter.inactive",
                "Cannot record readings on an inactive meter.",
                400);
        }

        var readingValue = AssetMeterService.NormalizeReading(
            request.ReadingValue,
            "meter_reading.invalid_value");
        var readAt = request.ReadAt ?? DateTimeOffset.UtcNow;
        if (readAt > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            throw new StlApiException(
                "meter_reading.future_read_at",
                "Read timestamp cannot be in the future.",
                400);
        }

        if (!request.IsCorrection && readingValue < meter.CurrentReading)
        {
            throw new StlApiException(
                "meter_reading.regression",
                "Reading value cannot be lower than the current meter reading unless recording a correction.",
                400);
        }

        var delta = request.IsCorrection ? 0 : readingValue - meter.CurrentReading;
        var now = DateTimeOffset.UtcNow;
        var entity = new MeterReading
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetMeterId = meter.Id,
            AssetId = meter.AssetId,
            ReadingValue = readingValue,
            DeltaFromPrevious = delta,
            ReadAt = readAt,
            RecordedByUserId = actorUserId,
            Notes = NormalizeNotes(request.Notes),
            IsCorrection = request.IsCorrection,
            CreatedAt = now
        };

        db.MeterReadings.Add(entity);
        await assetMeterService.UpdateReadingStateAsync(
            meter,
            readingValue,
            readAt,
            request.IsCorrection,
            cancellationToken);

        await audit.WriteAsync(
            request.IsCorrection ? "meter_reading.correction" : "meter_reading.record",
            tenantId,
            actorUserId,
            "meter_reading",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await forecastService.ApplyForecastAfterReadingAsync(
            tenantId,
            actorUserId,
            meter.Id,
            readingValue,
            cancellationToken);

        await platformOutboxEnqueue.TryEnqueueMeterReadingEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.MeterReadingRecorded,
            meter,
            entity.Id,
            actorUserId,
            now,
            $"Meter reading recorded for {meter.MeterKey} ({readingValue} {meter.Unit}).",
            eventResult: request.IsCorrection ? "correction" : "recorded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    private async Task TryEnqueueRejectedAsync(
        Guid tenantId,
        Guid actorUserId,
        AssetMeter meter,
        decimal readingValue,
        DateTimeOffset? readAt,
        StlApiException exception,
        CancellationToken cancellationToken)
    {
        await platformOutboxEnqueue.TryEnqueueMeterReadingEventAsync(
            tenantId,
            MaintenancePlatformOutboxEventKinds.MeterReadingRejected,
            meter,
            meter.Id,
            actorUserId,
            DateTimeOffset.UtcNow,
            $"Meter reading rejected for {meter.MeterKey}: {exception.Message}",
            eventResult: exception.Code,
            idempotencyDiscriminator: $"{exception.Code}:{readingValue}:{readAt?.UtcDateTime.Ticks ?? 0}",
            cancellationToken: cancellationToken);
    }
}
