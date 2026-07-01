using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Options;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class StaffarrProcurementApprovalAuthorityService(
    SupplyArrDbContext db,
    StaffArrProcurementApprovalAuthorityClient staffarrClient,
    IOptions<StaffArrClientOptions> staffarrOptions)
{
    public const string AuthoritySourceStaffarrMirror = "staffarr_mirror";

    public const string AuthoritySourceStaffarrLive = "staffarr_live";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly TimeSpan MirrorStaleness = TimeSpan.FromHours(1);

    public async Task<ProcurementApprovalAuthorityMirrorResponse> GetMirrorForActorAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        bool forceRefresh,
        bool allowStaleOnRefreshFailure = true,
        CancellationToken cancellationToken = default)
    {
        var mirror = await db.StaffarrProcurementApprovalAuthorityMirrors
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && (x.StaffarrPersonId == actorPersonId || x.ExternalUserId == actorUserId))
            .OrderByDescending(x => x.RefreshedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (!forceRefresh
            && mirror is not null
            && DateTimeOffset.UtcNow - mirror.RefreshedAt < MirrorStaleness)
        {
            return MapMirror(mirror, AuthoritySourceStaffarrMirror);
        }

        try
        {
            return await RefreshMirrorAsync(tenantId, actorUserId, actorPersonId, cancellationToken);
        }
        catch (StlApiException ex) when (allowStaleOnRefreshFailure && mirror is not null && ex.StatusCode >= 500)
        {
            return MapMirror(mirror, AuthoritySourceStaffarrMirror);
        }
        catch (HttpRequestException) when (allowStaleOnRefreshFailure && mirror is not null)
        {
            return MapMirror(mirror, AuthoritySourceStaffarrMirror);
        }
    }

    public async Task<ProcurementApprovalAuthorityMirrorResponse> RefreshMirrorAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var authority = await ResolveAuthorityFromStaffarrAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var staffarrPersonId = authority.PersonId;
        var externalUserId = authority.ExternalUserId ?? actorUserId;

        var mirror = await db.StaffarrProcurementApprovalAuthorityMirrors
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.StaffarrPersonId == staffarrPersonId,
                cancellationToken);

        var grantMirrors = authority.Grants
            .Select(g => new ProcurementApprovalAuthorityGrantMirror(
                g.PermissionKey,
                g.PermissionName,
                g.ScopeType,
                g.ScopeValue,
                g.RoleKey,
                g.RoleName))
            .ToList();
        var grantsJson = JsonSerializer.Serialize(grantMirrors, JsonOptions);
        var orgUnitJson = JsonSerializer.Serialize(authority.OrgUnitScopeIds, JsonOptions);

        if (mirror is null)
        {
            mirror = new StaffarrProcurementApprovalAuthorityMirror
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                StaffarrPersonId = staffarrPersonId,
                CreatedAt = now,
            };
            db.StaffarrProcurementApprovalAuthorityMirrors.Add(mirror);
        }

        mirror.ExternalUserId = externalUserId;
        mirror.CanSubmitPurchaseRequests = authority.CanSubmitPurchaseRequests;
        mirror.CanApprovePurchaseRequests = authority.CanApprovePurchaseRequests;
        mirror.CanIssuePurchaseOrders = authority.CanIssuePurchaseOrders;
        mirror.MaxSubmitAmount = authority.MaxSubmitAmount;
        mirror.MaxApproveAmount = authority.MaxApproveAmount;
        mirror.MaxIssueAmount = authority.MaxIssueAmount;
        mirror.OrgUnitScopeIdsJson = orgUnitJson;
        mirror.GrantsJson = grantsJson;
        mirror.SourceComputedAt = authority.ComputedAt;
        mirror.RefreshedAt = now;
        mirror.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        return MapMirror(mirror, AuthoritySourceStaffarrLive);
    }

    private async Task<StaffArrProcurementApprovalAuthorityPayload> ResolveAuthorityFromStaffarrAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await staffarrClient.GetAuthorityAsync(tenantId, actorPersonId, cancellationToken);
        }
        catch (StlApiException ex) when (ex.StatusCode == 404)
        {
            return await staffarrClient.GetAuthorityByExternalUserIdAsync(tenantId, actorUserId, cancellationToken);
        }
    }

    public async Task EnsureCanSubmitPurchaseRequestAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        PurchaseRequest purchaseRequest,
        CancellationToken cancellationToken = default)
    {
        if (!staffarrOptions.Value.EnforceProcurementApprovalAuthority)
        {
            return;
        }

        var authority = await GetMirrorForActorAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            forceRefresh: false,
            allowStaleOnRefreshFailure: false,
            cancellationToken);
        if (!authority.CanSubmitPurchaseRequests)
        {
            ThrowDenied(
                "procurement_approval_authority.submit_denied",
                "StaffArr does not grant purchase request submission authority for this person.",
                authority);
        }

        await EnsureWithinAmountLimitAsync(
            authority.MaxSubmitAmount,
            await EstimatePurchaseRequestAmountAsync(tenantId, purchaseRequest, cancellationToken),
            "procurement_approval_authority.submit_limit_exceeded",
            "Purchase request estimated total exceeds StaffArr submission limit.",
            authority);
    }

    public async Task EnsureCanApprovePurchaseRequestAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        PurchaseRequest purchaseRequest,
        CancellationToken cancellationToken = default)
    {
        if (!staffarrOptions.Value.EnforceProcurementApprovalAuthority)
        {
            return;
        }

        var authority = await GetMirrorForActorAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            forceRefresh: false,
            allowStaleOnRefreshFailure: false,
            cancellationToken);
        if (!authority.CanApprovePurchaseRequests)
        {
            ThrowDenied(
                "procurement_approval_authority.approve_denied",
                "StaffArr does not grant purchase request approval authority for this person.",
                authority);
        }

        await EnsureWithinAmountLimitAsync(
            authority.MaxApproveAmount,
            await EstimatePurchaseRequestAmountAsync(tenantId, purchaseRequest, cancellationToken),
            "procurement_approval_authority.approve_limit_exceeded",
            "Purchase request estimated total exceeds StaffArr approval limit.",
            authority);
    }

    public async Task EnsureCanIssuePurchaseOrderAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid actorPersonId,
        PurchaseOrder purchaseOrder,
        CancellationToken cancellationToken = default)
    {
        if (!staffarrOptions.Value.EnforceProcurementApprovalAuthority)
        {
            return;
        }

        var authority = await GetMirrorForActorAsync(
            tenantId,
            actorUserId,
            actorPersonId,
            forceRefresh: false,
            allowStaleOnRefreshFailure: false,
            cancellationToken);
        if (!authority.CanIssuePurchaseOrders)
        {
            ThrowDenied(
                "procurement_approval_authority.issue_denied",
                "StaffArr does not grant purchase order issue authority for this person.",
                authority);
        }

        await EnsureWithinAmountLimitAsync(
            authority.MaxIssueAmount,
            await EstimatePurchaseOrderAmountAsync(tenantId, purchaseOrder, cancellationToken),
            "procurement_approval_authority.issue_limit_exceeded",
            "Purchase order estimated total exceeds StaffArr issue limit.",
            authority);
    }

    private static Task EnsureWithinAmountLimitAsync(
        decimal? maxAmount,
        decimal estimatedAmount,
        string errorCode,
        string message,
        ProcurementApprovalAuthorityMirrorResponse authority)
    {
        if (maxAmount is decimal limit && estimatedAmount > limit)
        {
            ThrowDenied(errorCode, message, authority);
        }

        return Task.CompletedTask;
    }

    private static void ThrowDenied(
        string code,
        string message,
        ProcurementApprovalAuthorityMirrorResponse authority)
    {
        throw new StlApiException(
            code,
            message,
            403,
            new Dictionary<string, object?>
            {
                ["authoritySource"] = authority.AuthoritySource,
                ["staffarrPersonId"] = authority.StaffarrPersonId,
                ["denialReason"] = message,
            });
    }

    private async Task<decimal> EstimatePurchaseRequestAmountAsync(
        Guid tenantId,
        PurchaseRequest purchaseRequest,
        CancellationToken cancellationToken)
    {
        if (purchaseRequest.Lines.Count == 0)
        {
            return 0m;
        }

        var partIds = purchaseRequest.Lines.Select(x => x.PartId).Distinct().ToList();
        var supplierId = purchaseRequest.SupplierId;
        if (supplierId is null)
        {
            return 0m;
        }

        var priceByPart = await ResolveLatestUnitPricesAsync(tenantId, supplierId.Value, partIds, cancellationToken);
        return purchaseRequest.Lines.Sum(line =>
        {
            priceByPart.TryGetValue(line.PartId, out var unitPrice);
            return line.QuantityRequested * unitPrice;
        });
    }

    private async Task<decimal> EstimatePurchaseOrderAmountAsync(
        Guid tenantId,
        PurchaseOrder purchaseOrder,
        CancellationToken cancellationToken)
    {
        if (purchaseOrder.Lines.Count == 0)
        {
            return 0m;
        }

        var partIds = purchaseOrder.Lines.Select(x => x.PartId).Distinct().ToList();
        var supplierId = purchaseOrder.SupplierId;
        if (supplierId == Guid.Empty)
        {
            return 0m;
        }

        var priceByPart = await ResolveLatestUnitPricesAsync(tenantId, supplierId, partIds, cancellationToken);
        return purchaseOrder.Lines.Sum(line =>
        {
            priceByPart.TryGetValue(line.PartId, out var unitPrice);
            return line.QuantityOrdered * unitPrice;
        });
    }

    private async Task<Dictionary<Guid, decimal>> ResolveLatestUnitPricesAsync(
        Guid tenantId,
        Guid supplierId,
        IReadOnlyList<Guid> partIds,
        CancellationToken cancellationToken)
    {
        var links = await db.PartSupplierLinks.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SupplierId == supplierId && partIds.Contains(x.PartId))
            .Select(x => new { x.Id, x.PartId, x.CatalogUnitPrice })
            .ToListAsync(cancellationToken);

        if (links.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var linkIds = links.Select(x => x.Id).ToList();
        var snapshots = await db.PartSupplierPricingSnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId && linkIds.Contains(x.PartSupplierLinkId))
            .OrderByDescending(x => x.EffectiveFrom)
            .ToListAsync(cancellationToken);

        var snapshotByLink = snapshots
            .GroupBy(x => x.PartSupplierLinkId)
            .ToDictionary(g => g.Key, g => g.First().UnitPrice);

        var result = new Dictionary<Guid, decimal>();
        foreach (var link in links)
        {
            if (snapshotByLink.TryGetValue(link.Id, out var snapshotPrice))
            {
                result[link.PartId] = snapshotPrice;
            }
            else if (link.CatalogUnitPrice is decimal catalogPrice)
            {
                result[link.PartId] = catalogPrice;
            }
        }

        return result;
    }

    private static ProcurementApprovalAuthorityMirrorResponse MapMirror(
        StaffarrProcurementApprovalAuthorityMirror mirror,
        string authoritySource)
    {
        var orgUnitIds = JsonSerializer.Deserialize<List<Guid>>(mirror.OrgUnitScopeIdsJson, JsonOptions) ?? [];
        var grants = JsonSerializer.Deserialize<List<ProcurementApprovalAuthorityGrantMirror>>(mirror.GrantsJson, JsonOptions) ?? [];

        return new ProcurementApprovalAuthorityMirrorResponse(
            mirror.StaffarrPersonId,
            mirror.ExternalUserId,
            mirror.CanSubmitPurchaseRequests,
            mirror.CanApprovePurchaseRequests,
            mirror.CanIssuePurchaseOrders,
            mirror.MaxSubmitAmount,
            mirror.MaxApproveAmount,
            mirror.MaxIssueAmount,
            orgUnitIds,
            grants,
            mirror.SourceComputedAt,
            mirror.RefreshedAt,
            authoritySource);
    }
}

