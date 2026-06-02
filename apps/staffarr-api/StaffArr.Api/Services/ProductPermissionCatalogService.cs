using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class ProductPermissionCatalogService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public const string SyncActionScope = "staffarr.permission_catalog.sync";

    private static readonly HashSet<string> AllowedProducts = new(StringComparer.OrdinalIgnoreCase)
    {
        "compliancecore",
        "maintainarr",
        "routarr",
        "staffarr",
        "supplyarr",
        "trainarr"
    };

    private static readonly HashSet<string> AllowedScopes = new(StringComparer.OrdinalIgnoreCase)
    {
        "tenant",
        "site",
        "department",
        "team",
        "position",
        "product",
        "record"
    };

    private static readonly HashSet<string> AllowedSensitivities = new(StringComparer.OrdinalIgnoreCase)
    {
        "standard",
        "sensitive",
        "critical"
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "deprecated",
        "inactive"
    };

    public async Task<IReadOnlyList<ProductPermissionCatalogItemResponse>> ListAsync(
        Guid tenantId,
        string? productKey,
        CancellationToken cancellationToken = default)
    {
        var query = db.PermissionTemplates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(productKey))
        {
            var normalizedProductKey = NormalizeProductKey(productKey);
            query = query.Where(x => x.ProductKey == normalizedProductKey);
        }

        return await query
            .OrderBy(x => x.ProductKey)
            .ThenBy(x => x.PermissionKey)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<SyncProductPermissionCatalogResponse> SyncAsync(
        SyncProductPermissionCatalogRequest request,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (request.TenantId == Guid.Empty)
        {
            throw new StlApiException(
                "permission_catalog.validation",
                "Tenant id is required.",
                400);
        }

        var productKey = NormalizeProductKey(request.ProductKey);
        if (request.Permissions is null || request.Permissions.Count == 0)
        {
            throw new StlApiException(
                "permission_catalog.validation",
                "At least one permission is required.",
                400);
        }

        var normalized = request.Permissions
            .Select(item => NormalizeItem(productKey, item))
            .ToList();
        var duplicate = normalized
            .GroupBy(x => x.PermissionKey, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null)
        {
            throw new StlApiException(
                "permission_catalog.validation",
                $"Duplicate permission key '{duplicate.Key}' was provided.",
                400);
        }

        var permissionKeys = normalized.Select(x => x.PermissionKey).ToArray();
        var existingByKey = await db.PermissionTemplates
            .Where(x => x.TenantId == request.TenantId && permissionKeys.Contains(x.PermissionKey))
            .ToDictionaryAsync(x => x.PermissionKey, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var responses = new List<ProductPermissionCatalogItemResponse>(normalized.Count);
        foreach (var item in normalized)
        {
            if (!existingByKey.TryGetValue(item.PermissionKey, out var entity))
            {
                entity = new PermissionTemplate
                {
                    Id = Guid.NewGuid(),
                    TenantId = request.TenantId,
                    PermissionKey = item.PermissionKey,
                    CreatedAt = now
                };
                db.PermissionTemplates.Add(entity);
            }

            entity.Name = item.Label;
            entity.Description = item.Description;
            entity.Status = item.Status;
            entity.ProductKey = productKey;
            entity.PermissionScope = item.Scope;
            entity.Sensitivity = item.Sensitivity;
            entity.LastSyncedAt = now;
            entity.UpdatedAt = now;

            responses.Add(Map(entity, now));
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "permission_catalog.sync",
            request.TenantId,
            actorUserId,
            "permission_catalog",
            productKey,
            $"upserted={responses.Count}",
            cancellationToken: cancellationToken);

        return new SyncProductPermissionCatalogResponse(
            request.TenantId,
            productKey,
            responses.Count,
            now,
            responses
                .OrderBy(x => x.PermissionKey, StringComparer.OrdinalIgnoreCase)
                .ToList());
    }

    private static (
        string PermissionKey,
        string Label,
        string? Description,
        string Scope,
        string Sensitivity,
        string Status) NormalizeItem(
            string productKey,
            ProductPermissionCatalogItemRequest item)
    {
        var permissionKey = NormalizePermissionKey(productKey, item.PermissionKey);
        var label = NormalizeRequired(item.Label, 128, "Permission label");
        var description = NormalizeOptional(item.Description, 512, "Permission description");
        var scope = NormalizeControlled(item.Scope, AllowedScopes, "Permission scope");
        var sensitivity = NormalizeControlled(item.Sensitivity, AllowedSensitivities, "Permission sensitivity");
        var status = NormalizeControlled(item.Status, AllowedStatuses, "Permission status");
        return (permissionKey, label, description, scope, sensitivity, status);
    }

    private static string NormalizeProductKey(string? productKey)
    {
        var normalized = (productKey ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedProducts.Contains(normalized))
        {
            throw new StlApiException(
                "permission_catalog.validation",
                $"Product key must be one of: {string.Join(", ", AllowedProducts.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizePermissionKey(string productKey, string permissionKey)
    {
        var normalized = NormalizeRequired(permissionKey, 128, "Permission key").ToLowerInvariant();
        if (!normalized.StartsWith(productKey + ".", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "permission_catalog.validation",
                $"Permission key '{normalized}' must be owned by product prefix '{productKey}.'.",
                400);
        }

        return normalized;
    }

    private static string NormalizeControlled(
        string? value,
        HashSet<string> allowedValues,
        string displayName)
    {
        var normalized = NormalizeRequired(value, 64, displayName).ToLowerInvariant();
        if (!allowedValues.Contains(normalized))
        {
            throw new StlApiException(
                "permission_catalog.validation",
                $"{displayName} must be one of: {string.Join(", ", allowedValues.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static string NormalizeRequired(string? value, int maxLength, string displayName)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new StlApiException(
                "permission_catalog.validation",
                $"{displayName} is required.",
                400);
        }

        if (trimmed.Length > maxLength)
        {
            throw new StlApiException(
                "permission_catalog.validation",
                $"{displayName} must be {maxLength} characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string displayName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return NormalizeRequired(value, maxLength, displayName);
    }

    private static ProductPermissionCatalogItemResponse Map(PermissionTemplate entity) =>
        Map(entity, entity.LastSyncedAt ?? entity.UpdatedAt);

    private static ProductPermissionCatalogItemResponse Map(PermissionTemplate entity, DateTimeOffset syncedAt) =>
        new(
            entity.Id,
            entity.ProductKey,
            entity.PermissionKey,
            entity.Name,
            entity.Description,
            entity.PermissionScope,
            entity.Sensitivity,
            entity.Status,
            syncedAt);
}
