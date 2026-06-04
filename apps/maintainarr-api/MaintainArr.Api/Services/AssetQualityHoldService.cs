using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetQualityHoldService(
    MaintainArrDbContext db,
    IMaintainArrAuditService audit)
{
    public async Task<AssetQualityHoldResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateAssetQualityHoldRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureAssetExistsAsync(tenantId, request.AssetId, cancellationToken);

        var holdType = RequireTrimmed(request.HoldType, "quality_hold.hold_type_required", 64);
        var sourceProduct = RequireTrimmed(request.SourceProduct, "quality_hold.source_product_required", 64);
        var sourceObjectRef = NormalizeOptional(request.SourceObjectRef, 256);
        var title = RequireTrimmed(request.Title, "quality_hold.title_required", 256);
        var description = RequireTrimmed(request.Description, "quality_hold.description_required", 1024);
        var severity = RequireTrimmed(request.Severity, "quality_hold.severity_required", 32);
        var createdByPersonId = NormalizeOptional(request.CreatedByPersonId, 128);

        var existing = await db.AssetQualityHolds
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.AssetId == request.AssetId
                    && x.SourceProduct == sourceProduct
                    && x.SourceObjectRef == sourceObjectRef,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new AssetQualityHold
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                AssetId = request.AssetId,
                HoldType = holdType,
                SourceProduct = sourceProduct,
                SourceObjectRef = sourceObjectRef,
                Title = title,
                Description = description,
                Severity = severity,
                Status = "active",
                CreatedAt = now,
                CreatedByPersonId = createdByPersonId,
            };
            db.AssetQualityHolds.Add(existing);
        }
        else
        {
            existing.HoldType = holdType;
            existing.SourceProduct = sourceProduct;
            existing.SourceObjectRef = sourceObjectRef;
            existing.Title = title;
            existing.Description = description;
            existing.Severity = severity;
            existing.Status = "active";
            existing.ReleasedAt = null;
            existing.ReleasedByPersonId = null;
            existing.ReleaseReason = null;
            if (existing.CreatedByPersonId is null)
            {
                existing.CreatedByPersonId = createdByPersonId;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "asset_quality_hold.create",
            tenantId,
            actorUserId,
            "asset_quality_hold",
            existing.Id.ToString(),
            existing.Status,
            cancellationToken: cancellationToken);

        return Map(existing);
    }

    public async Task<AssetQualityHoldResponse> ReleaseAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid holdId,
        ReleaseAssetQualityHoldRequest request,
        CancellationToken cancellationToken = default)
    {
        var hold = await db.AssetQualityHolds
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == holdId, cancellationToken)
            ?? throw new StlApiException("quality_hold.not_found", "Quality hold was not found.", 404);

        if (!string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            return Map(hold);
        }

        hold.Status = "resolved";
        hold.ReleasedAt = DateTimeOffset.UtcNow;
        hold.ReleasedByPersonId = NormalizeOptional(request.ReleasedByPersonId, 128);
        hold.ReleaseReason = NormalizeOptional(request.ReleaseReason, 1024);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "asset_quality_hold.release",
            tenantId,
            actorUserId,
            "asset_quality_hold",
            hold.Id.ToString(),
            hold.Status,
            cancellationToken: cancellationToken);

        return Map(hold);
    }

    private async Task EnsureAssetExistsAsync(Guid tenantId, Guid assetId, CancellationToken cancellationToken)
    {
        var exists = await db.Assets.AnyAsync(x => x.TenantId == tenantId && x.Id == assetId, cancellationToken);
        if (!exists)
        {
            throw new StlApiException("asset.not_found", "Asset was not found.", 404);
        }
    }

    private static string RequireTrimmed(string? value, string code, int maxLength)
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
            throw new StlApiException("quality_hold.validation", $"Value must be {maxLength} characters or fewer.", 400);
        }

        return normalized;
    }

    private static AssetQualityHoldResponse Map(AssetQualityHold hold) =>
        new(
            hold.Id,
            hold.AssetId,
            hold.HoldType,
            hold.SourceProduct,
            hold.SourceObjectRef,
            hold.Title,
            hold.Description,
            hold.Severity,
            hold.Status,
            hold.CreatedAt,
            hold.CreatedByPersonId,
            hold.ReleasedAt,
            hold.ReleasedByPersonId,
            hold.ReleaseReason);
}
