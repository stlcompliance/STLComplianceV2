using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetReadinessCheckService(
    MaintainArrDbContext db,
    AssetReadinessService assetReadiness,
    IMaintainArrAuditService audit)
{
    public async Task<AssetReadinessCheckResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateAssetReadinessCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        var sourceProduct = Require(request.SourceProduct, "asset_readiness_check.source_product_required", 64);
        var requestedBy = Require(request.RequestedBy, "asset_readiness_check.requested_by_required", 128);
        var status = string.IsNullOrWhiteSpace(request.Status) ? "requested" : Require(request.Status, "asset_readiness_check.status_required", 64).ToLowerInvariant();
        var asOf = DateTimeOffset.UtcNow;

        var asset = request.AssetId.HasValue
            ? await db.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.AssetId.Value, cancellationToken)
            : await assetReadiness.ResolveAssetForDispatchAsync(tenantId, request.AssetId, request.VehicleRefKey, request.AssetTag, cancellationToken);

        if (asset is null)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        var readiness = await assetReadiness.GetByDispatchRefAsync(
            tenantId,
            asset.Id,
            request.VehicleRefKey,
            request.AssetTag,
            cancellationToken,
            actorUserId);

        var entity = new AssetReadinessCheck
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = asset.Id,
            AssetTag = asset.AssetTag,
            VehicleRefKey = NormalizeOptional(request.VehicleRefKey, 128),
            SourceProduct = sourceProduct,
            RequestedBy = requestedBy,
            Status = status,
            ReadinessStatus = readiness.ReadinessStatus,
            ReadinessBasis = readiness.ReadinessBasis,
            CreatedAt = asOf,
        };

        db.AssetReadinessChecks.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "asset_readiness_check.create",
            tenantId,
            actorUserId,
            "asset_readiness_check",
            entity.Id.ToString(),
            entity.ReadinessStatus,
            cancellationToken: cancellationToken);

        return new AssetReadinessCheckResponse(
            entity.Id,
            entity.AssetId,
            entity.AssetTag,
            entity.VehicleRefKey,
            entity.SourceProduct,
            entity.RequestedBy,
            entity.Status,
            entity.ReadinessStatus,
            entity.ReadinessBasis,
            entity.CreatedAt);
    }

    private static string Require(string? value, string code, int maxLength)
    {
        var normalized = NormalizeOptional(value, maxLength);
        if (normalized is null)
        {
            throw new StlApiException(code, "A value is required.", 400);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("asset_readiness_check.validation", $"Value must be {maxLength} characters or fewer.", 400);
        }

        return normalized;
    }
}
