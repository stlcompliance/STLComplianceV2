using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetMeterService(
    MaintainArrDbContext db,
    AssetService assetService,
    IMaintainArrAuditService audit)
{
    public async Task<IReadOnlyList<AssetMeterResponse>> ListForAssetAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        _ = await assetService.GetAsync(tenantId, assetId, cancellationToken);

        return await QueryMeters(tenantId)
            .Where(x => x.AssetId == assetId)
            .OrderBy(x => x.MeterKey)
            .ToListAsync(cancellationToken);
    }

    public async Task<AssetMeterResponse> GetAsync(
        Guid tenantId,
        Guid assetMeterId,
        CancellationToken cancellationToken = default)
    {
        var meter = await QueryMeters(tenantId)
            .FirstOrDefaultAsync(x => x.AssetMeterId == assetMeterId, cancellationToken);
        if (meter is null)
        {
            throw new StlApiException("meter.not_found", "Asset meter was not found.", 404);
        }

        return meter;
    }

    public async Task<AssetMeterResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetId,
        CreateAssetMeterRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = await assetService.GetAsync(tenantId, assetId, cancellationToken);

        var meterKey = NormalizeMeterKey(request.MeterKey);
        var exists = await db.AssetMeters.AnyAsync(
            x => x.TenantId == tenantId && x.AssetId == assetId && x.MeterKey == meterKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "meter.duplicate_key",
                "A meter with this key already exists for the asset.",
                409);
        }

        var baseline = NormalizeReading(request.BaselineReading, "meter.invalid_baseline");
        var now = DateTimeOffset.UtcNow;
        var entity = new AssetMeter
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = assetId,
            MeterKey = meterKey,
            Name = NormalizeName(request.Name),
            Description = NormalizeDescription(request.Description),
            Unit = NormalizeUnit(request.Unit),
            BaselineReading = baseline,
            CurrentReading = baseline,
            Status = MeterStatuses.Active,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.AssetMeters.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "asset_meter.create",
            tenantId,
            actorUserId,
            "asset_meter",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    internal async Task<AssetMeter> GetEntityAsync(
        Guid tenantId,
        Guid assetMeterId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.AssetMeters.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == assetMeterId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("meter.not_found", "Asset meter was not found.", 404);
        }

        return entity;
    }

    internal async Task UpdateReadingStateAsync(
        AssetMeter entity,
        decimal readingValue,
        DateTimeOffset readAt,
        bool isCorrection,
        CancellationToken cancellationToken)
    {
        entity.CurrentReading = readingValue;
        entity.LastReadingAt = readAt;
        if (isCorrection)
        {
            entity.BaselineReading = readingValue;
        }

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<AssetMeterResponse> QueryMeters(Guid tenantId) =>
        db.AssetMeters.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Join(
                db.Assets.AsNoTracking().Where(a => a.TenantId == tenantId),
                meter => meter.AssetId,
                asset => asset.Id,
                (meter, asset) => new AssetMeterResponse(
                    meter.Id,
                    meter.AssetId,
                    asset.AssetTag,
                    asset.Name,
                    meter.MeterKey,
                    meter.Name,
                    meter.Description,
                    meter.Unit,
                    meter.BaselineReading,
                    meter.CurrentReading,
                    meter.LastReadingAt,
                    meter.Status,
                    meter.CreatedAt,
                    meter.UpdatedAt));

    private static string NormalizeMeterKey(string meterKey)
    {
        var normalized = meterKey.Trim().ToLowerInvariant();
        if (normalized.Length is < 2 or > 128)
        {
            throw new StlApiException(
                "meter.invalid_key",
                "Meter key must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeName(string name)
    {
        var normalized = name.Trim();
        if (normalized.Length is < 2 or > 128)
        {
            throw new StlApiException(
                "meter.invalid_name",
                "Meter name must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeDescription(string description) =>
        description.Trim().Length <= 512 ? description.Trim() : description.Trim()[..512];

    private static string NormalizeUnit(string unit)
    {
        var normalized = unit.Trim().ToLowerInvariant();
        if (normalized.Length is < 1 or > 32)
        {
            throw new StlApiException(
                "meter.invalid_unit",
                "Meter unit must be between 1 and 32 characters.",
                400);
        }

        return normalized;
    }

    internal static decimal NormalizeReading(decimal value, string errorCode)
    {
        if (value < 0 || value > 999_999_999)
        {
            throw new StlApiException(errorCode, "Reading value must be between 0 and 999999999.", 400);
        }

        return decimal.Round(value, 4);
    }
}
