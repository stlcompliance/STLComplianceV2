using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class SupplierRestrictionService(
    SupplyArrDbContext db,
    SupplierProcurementGuardService procurementGuard,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<SupplierRestrictionResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.SupplierRestrictions
            .AsNoTracking()
            .Include(x => x.Supplier)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalized);
        }

        var rows = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<SupplierRestrictionResponse>> ListBySupplierAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRestrictableSupplierExistsAsync(tenantId, supplierId, cancellationToken);

        var rows = await db.SupplierRestrictions
            .AsNoTracking()
            .Include(x => x.Supplier)
            .ThenInclude(x => x.ParentSupplier)
            .Where(x => x.TenantId == tenantId && x.SupplierId == supplierId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<SupplierRestrictionResponse> GetAsync(
        Guid tenantId,
        Guid restrictionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, restrictionId, cancellationToken);
        return Map(entity);
    }

    public async Task<SupplierRestrictionResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        CreateSupplierRestrictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplier = await EnsureRestrictableSupplierExistsAsync(tenantId, supplierId, cancellationToken);
        var restrictionKey = SupplierRestrictionRules.NormalizeRestrictionKey(request.RestrictionKey);
        var scopes = SupplierRestrictionRules.NormalizeScopes(request.Scopes);
        var reason = SupplierRestrictionRules.NormalizeReason(request.Reason);

        var duplicateKey = await db.SupplierRestrictions.AnyAsync(
            x => x.TenantId == tenantId
                && x.SupplierId == supplierId
                && x.RestrictionKey == restrictionKey
                && x.Status == SupplierRestrictionStatuses.Active,
            cancellationToken);
        if (duplicateKey)
        {
            throw new StlApiException(
                "supplier_restrictions.duplicate_key",
                "An active restriction with this key already exists for the supplier.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var effectiveFrom = request.EffectiveFrom ?? now;
        if (request.EffectiveUntil is { } until && until <= effectiveFrom)
        {
            throw new StlApiException(
                "supplier_restrictions.invalid_effective_range",
                "Effective until must be after effective from.",
                400);
        }

        var entity = new SupplierRestriction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierId = supplier.Id,
            RestrictionKey = restrictionKey,
            ScopesJson = JsonSerializer.Serialize(scopes, JsonOptions),
            Reason = reason,
            Status = SupplierRestrictionStatuses.Active,
            EffectiveFrom = effectiveFrom,
            EffectiveUntil = request.EffectiveUntil,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.SupplierRestrictions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_restriction.create",
            tenantId,
            actorUserId,
            "supplier_restriction",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplierRestrictionCreated,
            "supplier_restriction",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Supplier restriction created: {restrictionKey}", supplier.Id),
            cancellationToken: cancellationToken);

        if (string.Equals(supplier.ApprovalStatus, "approved", StringComparison.OrdinalIgnoreCase))
        {
            supplier.ApprovalStatus = "restricted";
            supplier.UpdatedAt = now;
            await db.SaveChangesAsync(cancellationToken);
        }

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<SupplierRestrictionResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid restrictionId,
        UpdateSupplierRestrictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, restrictionId, cancellationToken);
        EnsureActive(entity);

        var scopes = SupplierRestrictionRules.NormalizeScopes(request.Scopes);
        var reason = SupplierRestrictionRules.NormalizeReason(request.Reason);
        if (request.EffectiveUntil is { } until && until <= entity.EffectiveFrom)
        {
            throw new StlApiException(
                "supplier_restrictions.invalid_effective_range",
                "Effective until must be after effective from.",
                400);
        }

        entity.ScopesJson = JsonSerializer.Serialize(scopes, JsonOptions);
        entity.Reason = reason;
        entity.EffectiveUntil = request.EffectiveUntil;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_restriction.update",
            tenantId,
            actorUserId,
            "supplier_restriction",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplierRestrictionUpdated,
            "supplier_restriction",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Supplier restriction updated: {entity.RestrictionKey}", entity.SupplierId),
            cancellationToken: cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<SupplierRestrictionResponse> LiftAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid restrictionId,
        LiftSupplierRestrictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, restrictionId, cancellationToken);
        EnsureActive(entity);

        var now = DateTimeOffset.UtcNow;
        entity.Status = SupplierRestrictionStatuses.Lifted;
        entity.LiftedAt = now;
        entity.LiftedByUserId = actorUserId;
        entity.LiftNotes = SupplierRestrictionRules.NormalizeLiftNotes(request.LiftNotes);
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_restriction.lift",
            tenantId,
            actorUserId,
            "supplier_restriction",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplierRestrictionLifted,
            "supplier_restriction",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Supplier restriction lifted: {entity.RestrictionKey}", entity.SupplierId),
            cancellationToken: cancellationToken);

        await TryClearSupplierRestrictedStatusAsync(tenantId, entity.SupplierId, cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public Task<SupplierRestrictionEnforcementResponse> GetEnforcementAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken = default) =>
        procurementGuard.GetEnforcementAsync(tenantId, supplierId, cancellationToken);

    private async Task TryClearSupplierRestrictedStatusAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        var hasActive = await db.SupplierRestrictions.AnyAsync(
            x => x.TenantId == tenantId
                && x.SupplierId == supplierId
                && x.Status == SupplierRestrictionStatuses.Active,
            cancellationToken);
        if (hasActive)
        {
            return;
        }

        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == supplierId, cancellationToken);
        if (supplier is null || !string.Equals(supplier.ApprovalStatus, "restricted", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        supplier.ApprovalStatus = "approved";
        supplier.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureActive(SupplierRestriction entity)
    {
        if (!string.Equals(entity.Status, SupplierRestrictionStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "supplier_restrictions.not_active",
                "Only active restrictions can be updated or lifted.",
                409);
        }
    }

    private async Task<Supplier> EnsureRestrictableSupplierExistsAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == supplierId, cancellationToken)
            ?? throw new StlApiException(
                "supplier_restrictions.supplier_not_found",
                "Supplier was not found.",
                404);

        return supplier;
    }

    private async Task<SupplierRestriction> LoadAsync(
        Guid tenantId,
        Guid restrictionId,
        CancellationToken cancellationToken) =>
        await db.SupplierRestrictions
            .AsNoTracking()
            .Include(x => x.Supplier)
            .ThenInclude(x => x.ParentSupplier)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == restrictionId, cancellationToken)
            ?? throw new StlApiException(
                "supplier_restrictions.not_found",
                "Supplier restriction was not found.",
                404);

    private async Task<SupplierRestriction> LoadTrackedAsync(
        Guid tenantId,
        Guid restrictionId,
        CancellationToken cancellationToken) =>
        await db.SupplierRestrictions
            .Include(x => x.Supplier)
            .ThenInclude(x => x.ParentSupplier)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == restrictionId, cancellationToken)
            ?? throw new StlApiException(
                "supplier_restrictions.not_found",
                "Supplier restriction was not found.",
                404);

    private static SupplierRestrictionResponse Map(SupplierRestriction entity)
    {
        IReadOnlyList<string> scopes;
        try
        {
            scopes = JsonSerializer.Deserialize<List<string>>(entity.ScopesJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            scopes = [];
        }

        IReadOnlyList<string> serviceTypes;
        try
        {
            serviceTypes = JsonSerializer.Deserialize<List<string>>(entity.Supplier.ServiceTypesJson ?? "[]", JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            serviceTypes = [];
        }

        return new SupplierRestrictionResponse(
            entity.Id,
            entity.SupplierId,
            entity.Supplier.SupplierKey,
            entity.Supplier.DisplayName,
            entity.Supplier.ParentSupplierId,
            entity.Supplier.ParentSupplier?.DisplayName,
            entity.Supplier.UnitKind,
            serviceTypes,
            entity.RestrictionKey,
            scopes,
            entity.Reason,
            entity.Status,
            entity.EffectiveFrom,
            entity.EffectiveUntil,
            entity.CreatedByUserId,
            entity.LiftedByUserId,
            entity.LiftedAt,
            entity.LiftNotes,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.Id);
    }
}


