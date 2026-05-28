using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class VendorRestrictionService(
    SupplyArrDbContext db,
    VendorProcurementGuardService procurementGuard,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<VendorRestrictionResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.VendorRestrictions
            .AsNoTracking()
            .Include(x => x.ExternalParty)
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

    public async Task<IReadOnlyList<VendorRestrictionResponse>> ListByPartyAsync(
        Guid tenantId,
        Guid externalPartyId,
        CancellationToken cancellationToken = default)
    {
        await EnsureRestrictablePartyExistsAsync(tenantId, externalPartyId, cancellationToken);

        var rows = await db.VendorRestrictions
            .AsNoTracking()
            .Include(x => x.ExternalParty)
            .Where(x => x.TenantId == tenantId && x.ExternalPartyId == externalPartyId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<VendorRestrictionResponse> GetAsync(
        Guid tenantId,
        Guid restrictionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, restrictionId, cancellationToken);
        return Map(entity);
    }

    public async Task<VendorRestrictionResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid externalPartyId,
        CreateVendorRestrictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var party = await EnsureRestrictablePartyExistsAsync(tenantId, externalPartyId, cancellationToken);
        var restrictionKey = VendorRestrictionRules.NormalizeRestrictionKey(request.RestrictionKey);
        var scopes = VendorRestrictionRules.NormalizeScopes(request.Scopes);
        var reason = VendorRestrictionRules.NormalizeReason(request.Reason);

        var duplicateKey = await db.VendorRestrictions.AnyAsync(
            x => x.TenantId == tenantId
                && x.ExternalPartyId == externalPartyId
                && x.RestrictionKey == restrictionKey
                && x.Status == VendorRestrictionStatuses.Active,
            cancellationToken);
        if (duplicateKey)
        {
            throw new StlApiException(
                "vendor_restrictions.duplicate_key",
                "An active restriction with this key already exists for the party.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var effectiveFrom = request.EffectiveFrom ?? now;
        if (request.EffectiveUntil is { } until && until <= effectiveFrom)
        {
            throw new StlApiException(
                "vendor_restrictions.invalid_effective_range",
                "Effective until must be after effective from.",
                400);
        }

        var entity = new VendorRestriction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalPartyId = party.Id,
            RestrictionKey = restrictionKey,
            ScopesJson = JsonSerializer.Serialize(scopes, JsonOptions),
            Reason = reason,
            Status = VendorRestrictionStatuses.Active,
            EffectiveFrom = effectiveFrom,
            EffectiveUntil = request.EffectiveUntil,
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.VendorRestrictions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "vendor_restriction.create",
            tenantId,
            actorUserId,
            "vendor_restriction",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.VendorRestrictionCreated,
            "vendor_restriction",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Vendor restriction created: {restrictionKey}", party.Id),
            cancellationToken: cancellationToken);

        if (string.Equals(party.ApprovalStatus, "approved", StringComparison.OrdinalIgnoreCase))
        {
            party.ApprovalStatus = "restricted";
            party.UpdatedAt = now;
            await db.SaveChangesAsync(cancellationToken);
        }

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<VendorRestrictionResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid restrictionId,
        UpdateVendorRestrictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, restrictionId, cancellationToken);
        EnsureActive(entity);

        var scopes = VendorRestrictionRules.NormalizeScopes(request.Scopes);
        var reason = VendorRestrictionRules.NormalizeReason(request.Reason);
        if (request.EffectiveUntil is { } until && until <= entity.EffectiveFrom)
        {
            throw new StlApiException(
                "vendor_restrictions.invalid_effective_range",
                "Effective until must be after effective from.",
                400);
        }

        entity.ScopesJson = JsonSerializer.Serialize(scopes, JsonOptions);
        entity.Reason = reason;
        entity.EffectiveUntil = request.EffectiveUntil;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "vendor_restriction.update",
            tenantId,
            actorUserId,
            "vendor_restriction",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.VendorRestrictionUpdated,
            "vendor_restriction",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Vendor restriction updated: {entity.RestrictionKey}", entity.ExternalPartyId),
            cancellationToken: cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<VendorRestrictionResponse> LiftAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid restrictionId,
        LiftVendorRestrictionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, restrictionId, cancellationToken);
        EnsureActive(entity);

        var now = DateTimeOffset.UtcNow;
        entity.Status = VendorRestrictionStatuses.Lifted;
        entity.LiftedAt = now;
        entity.LiftedByUserId = actorUserId;
        entity.LiftNotes = VendorRestrictionRules.NormalizeLiftNotes(request.LiftNotes);
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "vendor_restriction.lift",
            tenantId,
            actorUserId,
            "vendor_restriction",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.VendorRestrictionLifted,
            "vendor_restriction",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Vendor restriction lifted: {entity.RestrictionKey}", entity.ExternalPartyId),
            cancellationToken: cancellationToken);

        await TryClearPartyRestrictedStatusAsync(tenantId, entity.ExternalPartyId, cancellationToken);

        return Map(await LoadAsync(tenantId, entity.Id, cancellationToken));
    }

    public Task<VendorRestrictionEnforcementResponse> GetEnforcementAsync(
        Guid tenantId,
        Guid externalPartyId,
        CancellationToken cancellationToken = default) =>
        procurementGuard.GetEnforcementAsync(tenantId, externalPartyId, cancellationToken);

    private async Task TryClearPartyRestrictedStatusAsync(
        Guid tenantId,
        Guid externalPartyId,
        CancellationToken cancellationToken)
    {
        var hasActive = await db.VendorRestrictions.AnyAsync(
            x => x.TenantId == tenantId
                && x.ExternalPartyId == externalPartyId
                && x.Status == VendorRestrictionStatuses.Active,
            cancellationToken);
        if (hasActive)
        {
            return;
        }

        var party = await db.ExternalParties
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == externalPartyId, cancellationToken);
        if (party is null || !string.Equals(party.ApprovalStatus, "restricted", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        party.ApprovalStatus = "approved";
        party.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureActive(VendorRestriction entity)
    {
        if (!string.Equals(entity.Status, VendorRestrictionStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "vendor_restrictions.not_active",
                "Only active restrictions can be updated or lifted.",
                409);
        }
    }

    private async Task<ExternalParty> EnsureRestrictablePartyExistsAsync(
        Guid tenantId,
        Guid externalPartyId,
        CancellationToken cancellationToken)
    {
        var party = await db.ExternalParties
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == externalPartyId, cancellationToken)
            ?? throw new StlApiException(
                "vendor_restrictions.party_not_found",
                "Party was not found.",
                404);

        if (!VendorRestrictionPartyTypes.Allowed.Contains(party.PartyType))
        {
            throw new StlApiException(
                "vendor_restrictions.party_type_not_allowed",
                "Restrictions apply only to vendor or supplier parties.",
                400);
        }

        return party;
    }

    private async Task<VendorRestriction> LoadAsync(
        Guid tenantId,
        Guid restrictionId,
        CancellationToken cancellationToken) =>
        await db.VendorRestrictions
            .AsNoTracking()
            .Include(x => x.ExternalParty)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == restrictionId, cancellationToken)
            ?? throw new StlApiException(
                "vendor_restrictions.not_found",
                "Vendor restriction was not found.",
                404);

    private async Task<VendorRestriction> LoadTrackedAsync(
        Guid tenantId,
        Guid restrictionId,
        CancellationToken cancellationToken) =>
        await db.VendorRestrictions
            .Include(x => x.ExternalParty)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == restrictionId, cancellationToken)
            ?? throw new StlApiException(
                "vendor_restrictions.not_found",
                "Vendor restriction was not found.",
                404);

    private static VendorRestrictionResponse Map(VendorRestriction entity)
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

        return new VendorRestrictionResponse(
            entity.Id,
            entity.ExternalPartyId,
            entity.ExternalParty.PartyKey,
            entity.ExternalParty.DisplayName,
            entity.ExternalParty.PartyType,
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
            entity.UpdatedAt);
    }
}
