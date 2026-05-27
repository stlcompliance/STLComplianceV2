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
    IMaintainArrAuditService audit)
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
            assetMeterId,
            readingValue,
            cancellationToken);

        return Map(entity);
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
}
