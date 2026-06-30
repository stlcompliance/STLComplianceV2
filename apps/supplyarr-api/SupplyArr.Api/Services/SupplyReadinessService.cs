using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;
using System.Text.Json;

namespace SupplyArr.Api.Services;

public sealed class SupplyReadinessService(
    SupplyArrDbContext db,
    VendorProcurementGuardService vendorProcurementGuard,
    ISupplyArrAuditService audit)
{
    private const int AttentionItemLimit = 25;
    private const int PredictiveStockoutLimit = 10;
    private static readonly TimeSpan StaleSourceWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ComplianceExpiringSoonWindow = TimeSpan.FromDays(30);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public const string PartReadinessAction = "supplyarr.supply_readiness.part";

    public const string SupplierReadinessAction = "supplyarr.supply_readiness.supplier";
    public const string VendorReadinessAction = SupplierReadinessAction;

    public const string ProcurementPathReadinessAction = "supplyarr.supply_readiness.procurement_path";

    public const string PartSnapshotKind = "part_supply_readiness";

    public const string SupplierSnapshotKind = "supplier_supply_readiness";
    public const string VendorSnapshotKind = SupplierSnapshotKind;

    public const string ProcurementPathSnapshotKind = "procurement_path_supply_readiness";

    private static readonly string[] OpenPurchaseRequestStatuses =
    [
        PurchaseRequestStatuses.Draft,
        PurchaseRequestStatuses.Submitted,
    ];

    private static readonly string[] OpenPurchaseOrderStatuses =
    [
        PurchaseOrderStatuses.Draft,
        PurchaseOrderStatuses.Approved,
    ];

    private static readonly string[] ActiveProcurementExceptionStatuses =
    [
        ProcurementExceptionStatuses.Open,
        ProcurementExceptionStatuses.Investigating,
        ProcurementExceptionStatuses.Resolved,
        ProcurementExceptionStatuses.WaivePending,
        ProcurementExceptionStatuses.Waived,
    ];

    public async Task<SupplyReadinessDashboardResponse> GetDashboardAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var parts = await db.Parts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var activeParts = parts
            .Where(x => string.Equals(x.Status, "active", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var stockRows = await db.PartStockLevels
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new { x.PartId, x.QuantityOnHand, x.QuantityReserved })
            .ToListAsync(cancellationToken);

        var stockByPart = stockRows
            .GroupBy(x => x.PartId)
            .ToDictionary(
                g => g.Key,
                g => (
                    OnHand: g.Sum(x => x.QuantityOnHand),
                    Reserved: g.Sum(x => x.QuantityReserved)));

        var partsBelowReorder = activeParts.Count(part =>
        {
            if (part.ReorderPoint is not { } reorderPoint)
            {
                return false;
            }

            stockByPart.TryGetValue(part.Id, out var stock);
            var available = stock.OnHand - stock.Reserved;
            return available < reorderPoint;
        });

        var totalOnHand = stockRows.Sum(x => x.QuantityOnHand);
        var totalReserved = stockRows.Sum(x => x.QuantityReserved);

        var openBackorderCount = await db.Backorders.CountAsync(
            x => x.TenantId == tenantId && x.Status == BackorderStatuses.Open,
            cancellationToken);

        var purchaseRequests = await db.PurchaseRequests
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new DocumentRow(
                x.Id,
                x.RequestKey,
                x.Title,
                x.Status,
                x.UpdatedAt,
                null))
            .ToListAsync(cancellationToken);

        var openPurchaseRequestCount = purchaseRequests.Count(x =>
            OpenPurchaseRequestStatuses.Contains(x.Status));

        var purchaseOrders = await db.PurchaseOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new DocumentRow(
                x.Id,
                x.OrderKey,
                x.Title,
                x.Status,
                x.UpdatedAt,
                x.IssuedAt))
            .ToListAsync(cancellationToken);

        var openPurchaseOrderCount = purchaseOrders.Count(x =>
            OpenPurchaseOrderStatuses.Contains(x.Status));

        var issuedPurchaseOrderCount = purchaseOrders.Count(x =>
            string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase));

        var maintainarrOpen = await db.MaintainArrDemandRefs.CountAsync(
            x => x.TenantId == tenantId
                && (x.Status == MaintainArrDemandRefStatuses.Received
                    || x.Status == MaintainArrDemandRefStatuses.PrDrafted),
            cancellationToken);
        var routarrOpen = await db.RoutArrDemandRefs.CountAsync(
            x => x.TenantId == tenantId
                && (x.Status == RoutArrDemandRefStatuses.Received
                    || x.Status == RoutArrDemandRefStatuses.PrDrafted),
            cancellationToken);
        var trainarrOpen = await db.TrainArrDemandRefs.CountAsync(
            x => x.TenantId == tenantId
                && (x.Status == TrainArrDemandRefStatuses.Received
                    || x.Status == TrainArrDemandRefStatuses.PrDrafted),
            cancellationToken);
        var staffarrOpen = await db.StaffArrDemandRefs.CountAsync(
            x => x.TenantId == tenantId
                && (x.Status == StaffArrDemandRefStatuses.Received
                    || x.Status == StaffArrDemandRefStatuses.PrDrafted),
            cancellationToken);

        var openDemandRefCount = maintainarrOpen + routarrOpen + trainarrOpen + staffarrOpen;

        var openDemandLines = await LoadOpenDemandLinesAsync(tenantId, cancellationToken);
        var openDemandByPart = openDemandLines
            .GroupBy(x => x.PartId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.QuantityRequested));

        var openBackordersByPart = await db.Backorders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == BackorderStatuses.Open)
            .GroupBy(x => x.PartId)
            .Select(g => new { PartId = g.Key, Quantity = g.Sum(x => x.QuantityBackordered) })
            .ToListAsync(cancellationToken);
        var openBackordersByPartMap = openBackordersByPart.ToDictionary(x => x.PartId, x => x.Quantity);

        var complianceDocuments = await db.PartyComplianceDocuments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => new ComplianceDocRow(
                x.Id,
                x.ExternalPartyId,
                x.DocumentKey,
                x.ReviewStatus,
                x.ExpiresAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var complianceAttentionCount = complianceDocuments.Count(doc =>
            IsComplianceAttention(doc.ReviewStatus, doc.ExpiresAt, now));

        var activeVendorRestrictionCount = await db.VendorRestrictions.CountAsync(
            x => x.TenantId == tenantId && x.Status == VendorRestrictionStatuses.Active,
            cancellationToken);

        var procurementExceptions = await db.ProcurementExceptions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && ActiveProcurementExceptionStatuses.Contains(x.Status))
            .Select(x => new DocumentRow(
                x.Id,
                x.ExceptionKey,
                x.Title,
                x.Status,
                x.UpdatedAt,
                null))
            .ToListAsync(cancellationToken);

        var activeProcurementExceptionCount = procurementExceptions.Count;

        var backorders = openBackorderCount > 0
            ? await db.Backorders
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.Status == BackorderStatuses.Open)
                .OrderByDescending(x => x.UpdatedAt)
                .Take(AttentionItemLimit)
                .Select(x => new DocumentRow(
                    x.Id,
                    x.BackorderKey,
                    x.Notes,
                    x.Status,
                    x.UpdatedAt,
                    null))
                .ToListAsync(cancellationToken)
            : [];

        var attentionItems = BuildAttentionItems(
            now,
            activeParts,
            stockByPart,
            purchaseRequests,
            purchaseOrders,
            complianceDocuments,
            backorders,
            procurementExceptions,
            openBackorderCount,
            activeVendorRestrictionCount,
            activeProcurementExceptionCount);

        var predictiveStockoutItems = BuildPredictiveStockoutItems(
            now,
            parts,
            stockByPart,
            openDemandByPart,
            openBackordersByPartMap);

        return new SupplyReadinessDashboardResponse(
            now,
            new SupplyReadinessTotalsResponse(
                activeParts.Count,
                partsBelowReorder,
                stockRows.Count,
                totalOnHand,
                totalReserved,
                totalOnHand - totalReserved,
                openBackorderCount,
                openPurchaseRequestCount,
                openPurchaseOrderCount,
                issuedPurchaseOrderCount,
                openDemandRefCount,
                complianceAttentionCount,
                activeVendorRestrictionCount,
                activeProcurementExceptionCount),
            [
                new(DemandRefSources.MaintainArr, maintainarrOpen),
                new(DemandRefSources.RoutArr, routarrOpen),
                new(DemandRefSources.TrainArr, trainarrOpen),
                new(DemandRefSources.StaffArr, staffarrOpen),
            ],
            attentionItems,
            predictiveStockoutItems);
    }

    public async Task<PartSupplyReadinessResponse> GetPartReadinessAsync(
        Guid tenantId,
        Guid partId,
        decimal? requestedQuantity = null,
        CancellationToken cancellationToken = default,
        Guid? actorUserId = null,
        string? auditSnapshotKind = null)
    {
        var part = await db.Parts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == partId, cancellationToken)
            ?? throw new StlApiException("supply_readiness.part_not_found", "Part was not found.", 404);

        var asOf = DateTimeOffset.UtcNow;
        var availability = await LoadPartAvailabilityAsync(tenantId, partId, part.ReorderPoint, cancellationToken);
        var blockers = BuildPartBlockers(part, availability, requestedQuantity);
        var substitutes = await LoadSubstituteRecommendationsAsync(tenantId, part, cancellationToken);
        var substituteSourceTimestamp = substitutes
            .Select(x => x.SourceTimestamp)
            .Where(x => x is not null)
            .DefaultIfEmpty()
            .Max();
        var sourceTimestamp = MaxTimestamp(
            MaxTimestamp(part.UpdatedAt, availability.SourceTimestamp),
            substituteSourceTimestamp);
        var sourceSnapshot = BuildSourceSnapshot(sourceTimestamp, asOf);

        var isReady = SupplyReadinessRules.IsReady(blockers.Count);
        var readinessStatus = SupplyReadinessRules.ResolveReadinessStatus(isReady);
        var readinessBasis = SupplyReadinessRules.ResolveReadinessBasis(isReady);
        var auditSnapshot = await WriteDecisionSnapshotAsync(
            auditSnapshotKind,
            PartReadinessAction,
            tenantId,
            actorUserId,
            "part",
            partId.ToString(),
            readinessStatus,
            readinessBasis,
            cancellationToken);

        return new PartSupplyReadinessResponse(
            part.Id,
            part.PartKey,
            part.DisplayName,
            part.Status,
            readinessStatus,
            readinessBasis,
            asOf,
            blockers,
            availability,
            substitutes,
            sourceSnapshot,
            auditSnapshot);
    }

    public async Task<VendorSupplyReadinessResponse> GetVendorReadinessAsync(
        Guid tenantId,
        Guid externalPartyId,
        CancellationToken cancellationToken = default,
        Guid? actorUserId = null,
        string? auditSnapshotKind = null)
    {
        var supplier = await GetSupplierReadinessAsync(
            tenantId,
            externalPartyId,
            cancellationToken,
            actorUserId,
            auditSnapshotKind);

        return new VendorSupplyReadinessResponse(
            supplier.SupplierId,
            supplier.SupplierKey,
            supplier.DisplayName,
            supplier.ParentSupplierId,
            supplier.ParentSupplierDisplayName,
            supplier.SupplierUnitKind,
            supplier.SupplierServiceTypes,
            supplier.ApprovalStatus,
            supplier.Status,
            supplier.ReadinessStatus,
            supplier.ReadinessBasis,
            supplier.CalculatedAt,
            supplier.Blockers,
            supplier.SourceSnapshot,
            supplier.AuditSnapshot);
    }

    public async Task<SupplierSupplyReadinessResponse> GetSupplierReadinessAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken = default,
        Guid? actorUserId = null,
        string? auditSnapshotKind = null)
    {
        var party = await db.ExternalParties
            .AsNoTracking()
            .Include(x => x.ParentExternalParty)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == supplierId, cancellationToken)
            ?? throw new StlApiException("supply_readiness.supplier_not_found", "Supplier was not found.", 404);

        if (!VendorRestrictionPartyTypes.Allowed.Contains(party.PartyType))
        {
            throw new StlApiException(
                "supply_readiness.supplier_type_not_allowed",
                "Supply readiness checks apply only to supplier records.",
                400);
        }

        var asOf = DateTimeOffset.UtcNow;
        var blockers = await BuildSupplierBlockersAsync(tenantId, party, asOf, cancellationToken);
        var isReady = SupplyReadinessRules.IsReady(blockers.Count);
        var readinessStatus = SupplyReadinessRules.ResolveReadinessStatus(isReady);
        var readinessBasis = SupplyReadinessRules.ResolveReadinessBasis(isReady);
        var sourceSnapshot = BuildSourceSnapshot(party.UpdatedAt, asOf);
        var auditSnapshot = await WriteDecisionSnapshotAsync(
            auditSnapshotKind,
            SupplierReadinessAction,
            tenantId,
            actorUserId,
            "supplier",
            supplierId.ToString(),
            readinessStatus,
            readinessBasis,
            cancellationToken);

        var serviceTypes = ParseServiceTypes(party.ServiceTypesJson);

        return new SupplierSupplyReadinessResponse(
            party.Id,
            party.PartyKey,
            party.DisplayName,
            party.ParentExternalPartyId,
            party.ParentExternalParty?.DisplayName,
            party.UnitKind,
            serviceTypes,
            party.ApprovalStatus,
            party.Status,
            readinessStatus,
            readinessBasis,
            asOf,
            blockers,
            sourceSnapshot,
            auditSnapshot);
    }

    public async Task<ProcurementPathReadinessResponse> GetProcurementPathReadinessAsync(
        Guid tenantId,
        Guid partId,
        Guid supplierId,
        decimal? requestedQuantity = null,
        CancellationToken cancellationToken = default,
        Guid? actorUserId = null,
        string? auditSnapshotKind = null)
    {
        var partReadiness = await GetPartReadinessAsync(tenantId, partId, requestedQuantity, cancellationToken);
        var supplierReadiness = await GetSupplierReadinessAsync(tenantId, supplierId, cancellationToken);

        var blockers = new List<SupplyReadinessBlockerResponse>();
        blockers.AddRange(partReadiness.Blockers);
        blockers.AddRange(supplierReadiness.Blockers);

        var supplierLink = await db.PartVendorLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.PartId == partId && x.ExternalPartyId == supplierId,
            cancellationToken);
        if (supplierLink is null)
        {
            blockers.Add(new SupplyReadinessBlockerResponse(
                SupplyReadinessReasonCodes.NoVendorPartLink,
                "No supplier part link exists for this part and supplier location.",
                "part_vendor_link",
                $"{partId}:{supplierId}",
                partId.ToString()));
        }

        var pricingLeadTime = supplierLink is null
            ? null
            : await LoadPricingLeadTimeSnapshotAsync(tenantId, supplierLink, cancellationToken);
        var isReady = SupplyReadinessRules.IsReady(blockers.Count);
        var asOf = DateTimeOffset.UtcNow;
        var readinessStatus = SupplyReadinessRules.ResolveReadinessStatus(isReady);
        var readinessBasis = SupplyReadinessRules.ResolveReadinessBasis(isReady);
        var sourceSnapshot = BuildSourceSnapshot(
            MaxTimestamp(
                MaxTimestamp(
                    partReadiness.SourceSnapshot?.SourceTimestamp,
                    supplierReadiness.SourceSnapshot?.SourceTimestamp),
                MaxTimestamp(
                    pricingLeadTime?.PriceSourceTimestamp,
                    pricingLeadTime?.LeadTimeSourceTimestamp)),
            asOf);
        var auditSnapshot = await WriteDecisionSnapshotAsync(
            auditSnapshotKind,
            ProcurementPathReadinessAction,
            tenantId,
            actorUserId,
            "procurement_path",
            $"{partId}:{supplierId}",
            readinessStatus,
            readinessBasis,
            cancellationToken);

        return new ProcurementPathReadinessResponse(
            partId,
            partReadiness.PartKey,
            supplierReadiness.SupplierId,
            supplierReadiness.SupplierKey,
            supplierReadiness.ParentSupplierId,
            supplierReadiness.ParentSupplierDisplayName,
            supplierReadiness.SupplierUnitKind,
            supplierReadiness.SupplierServiceTypes,
            requestedQuantity,
            readinessStatus,
            readinessBasis,
            asOf,
            blockers,
            pricingLeadTime,
            sourceSnapshot,
            auditSnapshot);
    }

    private async Task<IReadOnlyList<SupplyReadinessSubstituteRecommendationResponse>> LoadSubstituteRecommendationsAsync(
        Guid tenantId,
        Part part,
        CancellationToken cancellationToken)
    {
        if (part.PartCatalogId is null)
        {
            return [];
        }

        var candidateParts = await db.Parts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.PartCatalogId == part.PartCatalogId
                && x.Id != part.Id
                && x.Status == "active")
            .Select(x => new { x.Id, x.PartKey, x.DisplayName, x.UpdatedAt })
            .ToListAsync(cancellationToken);

        var candidateIds = candidateParts.Select(x => x.Id).ToList();
        if (candidateIds.Count == 0)
        {
            return [];
        }

        var stockRows = await db.PartStockLevels
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && candidateIds.Contains(x.PartId))
            .Select(x => new { x.PartId, x.QuantityOnHand, x.QuantityReserved, x.UpdatedAt })
            .ToListAsync(cancellationToken);

        var stockByPart = stockRows
            .GroupBy(x => x.PartId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    QuantityAvailable = g.Sum(x => x.QuantityOnHand - x.QuantityReserved),
                    SourceTimestamp = g.Max(x => (DateTimeOffset?)x.UpdatedAt)
                });

        return candidateParts
            .Select(candidate =>
            {
                stockByPart.TryGetValue(candidate.Id, out var stock);
                var quantityAvailable = stock?.QuantityAvailable ?? 0m;
                return new SupplyReadinessSubstituteRecommendationResponse(
                    candidate.Id,
                    candidate.PartKey,
                    candidate.DisplayName,
                    quantityAvailable,
                    "same_catalog_available_stock",
                    MaxTimestamp(candidate.UpdatedAt, stock?.SourceTimestamp));
            })
            .Where(x => x.QuantityAvailable > 0m)
            .OrderByDescending(x => x.QuantityAvailable)
            .ThenBy(x => x.PartKey)
            .Take(5)
            .ToList();
    }

    private async Task<SupplyReadinessPricingLeadTimeSnapshotResponse> LoadPricingLeadTimeSnapshotAsync(
        Guid tenantId,
        PartVendorLink vendorLink,
        CancellationToken cancellationToken)
    {
        var pricing = await db.PartVendorPricingSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.PartVendorLinkId == vendorLink.Id
                && x.EffectiveFrom <= DateTimeOffset.UtcNow
                && (x.EffectiveTo == null || x.EffectiveTo > DateTimeOffset.UtcNow))
            .OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                x.UnitPrice,
                x.CurrencyCode,
                x.MinimumOrderQuantity,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        var leadTime = await db.PartVendorLeadTimeSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.PartVendorLinkId == vendorLink.Id
                && x.EffectiveFrom <= DateTimeOffset.UtcNow
                && (x.EffectiveTo == null || x.EffectiveTo > DateTimeOffset.UtcNow))
            .OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new { x.LeadTimeDays, x.UpdatedAt })
            .FirstOrDefaultAsync(cancellationToken);

        return new SupplyReadinessPricingLeadTimeSnapshotResponse(
            vendorLink.Id,
            pricing?.UnitPrice ?? vendorLink.CatalogUnitPrice,
            pricing?.CurrencyCode ?? vendorLink.CatalogCurrencyCode,
            pricing?.MinimumOrderQuantity ?? vendorLink.CatalogMinimumOrderQuantity,
            leadTime?.LeadTimeDays ?? vendorLink.CatalogLeadTimeDays,
            pricing?.UpdatedAt ?? (vendorLink.CatalogUnitPrice is null ? null : vendorLink.UpdatedAt),
            leadTime?.UpdatedAt ?? (vendorLink.CatalogLeadTimeDays is null ? null : vendorLink.UpdatedAt),
            pricing is null || leadTime is null);
    }

    private async Task<SupplyReadinessAvailabilitySnapshotResponse> LoadPartAvailabilityAsync(
        Guid tenantId,
        Guid partId,
        decimal? reorderPoint,
        CancellationToken cancellationToken)
    {
        var stockRows = await db.PartStockLevels
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PartId == partId)
            .Select(x => new { x.QuantityOnHand, x.QuantityReserved, x.UpdatedAt })
            .ToListAsync(cancellationToken);

        var onHand = stockRows.Sum(x => x.QuantityOnHand);
        var reserved = stockRows.Sum(x => x.QuantityReserved);
        var available = onHand - reserved;
        DateTimeOffset? sourceTimestamp = stockRows.Count == 0
            ? null
            : stockRows.Max(x => x.UpdatedAt);

        var activeReservationCount = await db.PartStockReservations.CountAsync(
            x => x.TenantId == tenantId
                && x.PartId == partId
                && x.Status == StockReservationStatuses.Active,
            cancellationToken);

        var openBackorderCount = await db.Backorders.CountAsync(
            x => x.TenantId == tenantId && x.PartId == partId && x.Status == BackorderStatuses.Open,
            cancellationToken);

        return new SupplyReadinessAvailabilitySnapshotResponse(
            onHand,
            reserved,
            available,
            reorderPoint,
            activeReservationCount,
            openBackorderCount,
            sourceTimestamp);
    }

    private static List<SupplyReadinessBlockerResponse> BuildPartBlockers(
        Part part,
        SupplyReadinessAvailabilitySnapshotResponse availability,
        decimal? requestedQuantity)
    {
        var blockers = new List<SupplyReadinessBlockerResponse>();

        if (!SupplyReadinessRules.IsActivePartStatus(part.Status))
        {
            blockers.Add(new SupplyReadinessBlockerResponse(
                SupplyReadinessReasonCodes.PartInactive,
                $"Part status is {part.Status}.",
                "part",
                part.Id.ToString(),
                null));
        }

        if (availability.QuantityAvailable <= 0)
        {
            blockers.Add(new SupplyReadinessBlockerResponse(
                SupplyReadinessReasonCodes.PartStockout,
                "No available stock for this part.",
                "part",
                part.Id.ToString(),
                null));
        }
        else if (availability.ReorderPoint is { } reorderPoint
            && availability.QuantityAvailable < reorderPoint)
        {
            blockers.Add(new SupplyReadinessBlockerResponse(
                SupplyReadinessReasonCodes.PartBelowReorder,
                $"Available {availability.QuantityAvailable} is below reorder point {reorderPoint}.",
                "part",
                part.Id.ToString(),
                null));
        }

        if (requestedQuantity is { } qty && qty > availability.QuantityAvailable)
        {
            blockers.Add(new SupplyReadinessBlockerResponse(
                SupplyReadinessReasonCodes.InsufficientAvailableQuantity,
                $"Requested quantity {qty} exceeds available {availability.QuantityAvailable}.",
                "part",
                part.Id.ToString(),
                null));
        }

        if (availability.OpenBackorderCount > 0)
        {
            blockers.Add(new SupplyReadinessBlockerResponse(
                SupplyReadinessReasonCodes.OpenBackorder,
                $"{availability.OpenBackorderCount} open backorder(s) exist for this part.",
                "part",
                part.Id.ToString(),
                null));
        }

        return blockers;
    }

    private async Task<SupplyReadinessDecisionSnapshotResponse?> WriteDecisionSnapshotAsync(
        string? auditSnapshotKind,
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string targetId,
        string result,
        string reasonCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(auditSnapshotKind))
        {
            return null;
        }

        var auditResult = await audit.WriteAsync(
            action,
            tenantId,
            actorUserId,
            targetType,
            targetId,
            result,
            reasonCode,
            cancellationToken);
        return new SupplyReadinessDecisionSnapshotResponse(
            auditResult.AuditEventId,
            auditSnapshotKind,
            auditResult.OccurredAt);
    }

    private static SupplyReadinessSourceSnapshotResponse BuildSourceSnapshot(
        DateTimeOffset? sourceTimestamp,
        DateTimeOffset calculatedAt)
    {
        TimeSpan? staleness = sourceTimestamp is null
            ? null
            : calculatedAt - sourceTimestamp.Value;
        return new SupplyReadinessSourceSnapshotResponse(
            sourceTimestamp,
            staleness is null || staleness.Value > StaleSourceWindow,
            staleness is null ? null : Math.Max(0, (int)Math.Ceiling(staleness.Value.TotalMinutes)));
    }

    private static DateTimeOffset? MaxTimestamp(DateTimeOffset? left, DateTimeOffset? right) =>
        left is null
            ? right
            : right is null || left.Value >= right.Value
                ? left
                : right;

    private static DateTimeOffset MaxTimestamp(DateTimeOffset left, DateTimeOffset? right) =>
        right is null || left >= right.Value ? left : right.Value;

    private static IReadOnlyList<string> ParseServiceTypes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(value, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private async Task<List<DemandLineRow>> LoadOpenDemandLinesAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var maintain = await db.MaintainArrDemandRefs
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.TenantId == tenantId
                && (x.Status == MaintainArrDemandRefStatuses.Received
                    || x.Status == MaintainArrDemandRefStatuses.PrDrafted))
            .ToListAsync(cancellationToken);

        var rout = await db.RoutArrDemandRefs
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.TenantId == tenantId
                && (x.Status == RoutArrDemandRefStatuses.Received
                    || x.Status == RoutArrDemandRefStatuses.PrDrafted))
            .ToListAsync(cancellationToken);

        var train = await db.TrainArrDemandRefs
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.TenantId == tenantId
                && (x.Status == TrainArrDemandRefStatuses.Received
                    || x.Status == TrainArrDemandRefStatuses.PrDrafted))
            .ToListAsync(cancellationToken);

        var staff = await db.StaffArrDemandRefs
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.TenantId == tenantId
                && (x.Status == StaffArrDemandRefStatuses.Received
                    || x.Status == StaffArrDemandRefStatuses.PrDrafted))
            .ToListAsync(cancellationToken);

        return [
            .. FlattenMaintainDemandLines(maintain),
            .. FlattenRoutDemandLines(rout),
            .. FlattenTrainDemandLines(train),
            .. FlattenStaffDemandLines(staff),
        ];
    }

    private IReadOnlyList<SupplyReadinessPredictiveStockoutResponse> BuildPredictiveStockoutItems(
        DateTimeOffset now,
        IReadOnlyCollection<Part> parts,
        IReadOnlyDictionary<Guid, (decimal OnHand, decimal Reserved)> stockByPart,
        IReadOnlyDictionary<Guid, decimal> openDemandByPart,
        IReadOnlyDictionary<Guid, decimal> openBackordersByPart)
    {
        var predictiveItems = new List<SupplyReadinessPredictiveStockoutResponse>();

        foreach (var part in parts.Where(x => x.Status == "active"))
        {
            stockByPart.TryGetValue(part.Id, out var stock);
            openDemandByPart.TryGetValue(part.Id, out var demandQty);
            openBackordersByPart.TryGetValue(part.Id, out var backorderQty);

            var available = stock.OnHand - stock.Reserved;
            var projected = available - demandQty - backorderQty;
            var shortage = Math.Max(0m, -projected);

            if (shortage <= 0m && available > (part.ReorderPoint ?? 0m))
            {
                continue;
            }

            var riskLevel = shortage > 0m
                ? "critical"
                : available <= (part.ReorderPoint ?? 0m)
                    ? "watch"
                    : "info";
            var reason = shortage > 0m
                ? "Projected shortage after open demand and backorders"
                : "Available stock is at or below the reorder point";
            var sourceTimestamp = part.UpdatedAt;

            predictiveItems.Add(new SupplyReadinessPredictiveStockoutResponse(
                part.Id,
                part.PartKey,
                part.DisplayName,
                available,
                demandQty,
                backorderQty,
                projected,
                shortage,
                part.ReorderPoint,
                riskLevel,
                reason,
                sourceTimestamp));
        }

        return predictiveItems
            .OrderByDescending(x => x.ShortageQuantity)
            .ThenByDescending(x => x.QuantityAvailable)
            .ThenBy(x => x.PartKey)
            .Take(PredictiveStockoutLimit)
            .ToList();
    }

    private sealed record DemandLineRow(Guid PartId, decimal QuantityRequested);

    private static IEnumerable<DemandLineRow> FlattenMaintainDemandLines(IEnumerable<MaintainArrDemandRef> refs) =>
        refs.SelectMany(x => x.Lines.Where(line => line.PartId != null).Select(line => new DemandLineRow(line.PartId!.Value, line.QuantityRequested)));

    private static IEnumerable<DemandLineRow> FlattenRoutDemandLines(IEnumerable<RoutArrDemandRef> refs) =>
        refs.SelectMany(x => x.Lines.Where(line => line.PartId != null).Select(line => new DemandLineRow(line.PartId!.Value, line.QuantityRequested)));

    private static IEnumerable<DemandLineRow> FlattenTrainDemandLines(IEnumerable<TrainArrDemandRef> refs) =>
        refs.SelectMany(x => x.Lines.Where(line => line.PartId != null).Select(line => new DemandLineRow(line.PartId!.Value, line.QuantityRequested)));

    private static IEnumerable<DemandLineRow> FlattenStaffDemandLines(IEnumerable<StaffArrDemandRef> refs) =>
        refs.SelectMany(x => x.Lines.Where(line => line.PartId != null).Select(line => new DemandLineRow(line.PartId!.Value, line.QuantityRequested)));

    private async Task<List<SupplyReadinessBlockerResponse>> BuildSupplierBlockersAsync(
        Guid tenantId,
        ExternalParty party,
        DateTimeOffset asOf,
        CancellationToken cancellationToken)
    {
        var blockers = new List<SupplyReadinessBlockerResponse>();

        if (!SupplyReadinessRules.IsActivePartyStatus(party.Status))
        {
            blockers.Add(new SupplyReadinessBlockerResponse(
                SupplyReadinessReasonCodes.VendorInactive,
                $"Supplier status is {party.Status}.",
                "supplier",
                party.Id.ToString(),
                null));
        }

        var approvalReasonCode = SupplyReadinessRules.ResolveApprovalBlockerReasonCode(party.ApprovalStatus);
        if (approvalReasonCode is not null)
        {
            blockers.Add(new SupplyReadinessBlockerResponse(
                approvalReasonCode,
                $"Supplier approval status is {party.ApprovalStatus}.",
                "supplier",
                party.Id.ToString(),
                null));
        }

        var enforcement = await vendorProcurementGuard.GetEnforcementAsync(tenantId, party.Id, cancellationToken);
        if (enforcement.IsBlocked)
        {
            blockers.Add(new SupplyReadinessBlockerResponse(
                SupplyReadinessReasonCodes.VendorProcurementRestriction,
                enforcement.BlockReason ?? "Active procurement restriction blocks this supplier location.",
                "vendor_restriction",
                party.Id.ToString(),
                null));
        }

        var complianceDocs = await db.PartyComplianceDocuments
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ExternalPartyId == party.Id)
            .Select(x => new { x.Id, x.DocumentKey, x.ReviewStatus, x.ExpiresAt })
            .ToListAsync(cancellationToken);

        foreach (var doc in complianceDocs)
        {
            if (string.Equals(doc.ReviewStatus, PartyComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase))
            {
                blockers.Add(new SupplyReadinessBlockerResponse(
                    SupplyReadinessReasonCodes.ComplianceDocumentPending,
                    $"Compliance document {doc.DocumentKey} is pending review.",
                    "party_compliance_document",
                    doc.Id.ToString(),
                    party.Id.ToString()));
            }
            else if (string.Equals(doc.ReviewStatus, PartyComplianceDocumentReviewStatuses.Expired, StringComparison.OrdinalIgnoreCase)
                || (doc.ExpiresAt is not null && doc.ExpiresAt <= asOf))
            {
                blockers.Add(new SupplyReadinessBlockerResponse(
                    SupplyReadinessReasonCodes.ComplianceDocumentExpired,
                    $"Compliance document {doc.DocumentKey} is expired.",
                    "party_compliance_document",
                    doc.Id.ToString(),
                    party.Id.ToString()));
            }
        }

        var openIncidents = await db.SupplierIncidents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.ExternalPartyId == party.Id
                && SupplierIncidentStatuses.Active.Contains(x.Status)
                && (x.Severity == SupplierIncidentSeverities.High || x.Severity == SupplierIncidentSeverities.Critical))
            .Select(x => new { x.Id, x.IncidentKey, x.Title, x.Severity })
            .ToListAsync(cancellationToken);

        foreach (var incident in openIncidents)
        {
            blockers.Add(new SupplyReadinessBlockerResponse(
                SupplyReadinessReasonCodes.OpenSupplierIncident,
                $"{incident.Severity} supplier incident {incident.IncidentKey}: {incident.Title}",
                "supplier_incident",
                incident.Id.ToString(),
                party.Id.ToString()));
        }

        return blockers;
    }

    private static bool IsComplianceAttention(string reviewStatus, DateTimeOffset? expiresAt, DateTimeOffset now)
    {
        if (string.Equals(reviewStatus, PartyComplianceDocumentReviewStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (expiresAt is null)
        {
            return false;
        }

        if (expiresAt.Value <= now)
        {
            return true;
        }

        return expiresAt.Value <= now.Add(ComplianceExpiringSoonWindow);
    }

    private static List<SupplyReadinessAttentionItemResponse> BuildAttentionItems(
        DateTimeOffset now,
        IReadOnlyList<Part> activeParts,
        IReadOnlyDictionary<Guid, (decimal OnHand, decimal Reserved)> stockByPart,
        IReadOnlyList<DocumentRow> purchaseRequests,
        IReadOnlyList<DocumentRow> purchaseOrders,
        IReadOnlyList<ComplianceDocRow> complianceDocuments,
        IReadOnlyList<DocumentRow> backorders,
        IReadOnlyList<DocumentRow> procurementExceptions,
        int openBackorderCount,
        int activeVendorRestrictionCount,
        int activeProcurementExceptionCount)
    {
        var items = new List<SupplyReadinessAttentionItemResponse>();

        foreach (var part in activeParts)
        {
            if (part.ReorderPoint is not { } reorderPoint)
            {
                continue;
            }

            stockByPart.TryGetValue(part.Id, out var stock);
            var available = stock.OnHand - stock.Reserved;
            if (available >= reorderPoint)
            {
                continue;
            }

            items.Add(new SupplyReadinessAttentionItemResponse(
                "stock",
                $"{part.PartKey} below reorder",
                $"Available {available} · reorder point {reorderPoint}",
                "below_reorder",
                part.UpdatedAt,
                "part",
                part.Id));
        }

        foreach (var row in purchaseRequests.Where(x => OpenPurchaseRequestStatuses.Contains(x.Status))
            .OrderByDescending(x => x.UpdatedAt)
            .Take(5))
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "procurement",
                $"PR {row.Key} · {row.Status}",
                row.Title,
                row.Status,
                row.UpdatedAt,
                "purchase_request",
                row.Id));
        }

        foreach (var row in purchaseOrders.Where(x =>
                OpenPurchaseOrderStatuses.Contains(x.Status)
                    || string.Equals(x.Status, PurchaseOrderStatuses.Issued, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.IssuedAt ?? x.UpdatedAt)
            .Take(5))
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "procurement",
                $"PO {row.Key} · {row.Status}",
                row.Title,
                row.Status,
                row.IssuedAt ?? row.UpdatedAt,
                "purchase_order",
                row.Id));
        }

        foreach (var row in backorders.Take(5))
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "backorder",
                $"Backorder {row.Key}",
                string.IsNullOrWhiteSpace(row.Title) ? "Open backorder" : row.Title,
                row.Status,
                row.UpdatedAt,
                "backorder",
                row.Id));
        }

        if (openBackorderCount > backorders.Count)
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "backorder",
                $"{openBackorderCount} open backorders",
                "Review receiving and purchasing queues for fulfillment.",
                "summary",
                now,
                null,
                null));
        }

        foreach (var doc in complianceDocuments.Where(x => IsComplianceAttention(x.ReviewStatus, x.ExpiresAt, now))
            .OrderByDescending(x => x.UpdatedAt)
            .Take(5))
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "compliance",
                $"Compliance doc {doc.DocumentKey}",
                $"Review status {doc.ReviewStatus}",
                doc.ReviewStatus,
                doc.UpdatedAt,
                "party_compliance_document",
                doc.Id));
        }

        foreach (var row in procurementExceptions.Take(5))
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "exception",
                $"Exception {row.Key}",
                row.Title,
                row.Status,
                row.UpdatedAt,
                "procurement_exception",
                row.Id));
        }

        if (activeVendorRestrictionCount > 0)
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "restriction",
                $"{activeVendorRestrictionCount} active vendor restrictions",
                "Procurement may be blocked for affected vendors.",
                "summary",
                now,
                null,
                null));
        }

        if (activeProcurementExceptionCount > procurementExceptions.Count)
        {
            items.Add(new SupplyReadinessAttentionItemResponse(
                "exception",
                $"{activeProcurementExceptionCount} active procurement exceptions",
                "Investigate open exceptions on PR/PO/RFQ subjects.",
                "summary",
                now,
                null,
                null));
        }

        return items
            .OrderByDescending(x => x.OccurredAt ?? DateTimeOffset.MinValue)
            .Take(AttentionItemLimit)
            .ToList();
    }

    private sealed record DocumentRow(
        Guid Id,
        string Key,
        string Title,
        string Status,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? IssuedAt);

    private sealed record ComplianceDocRow(
        Guid Id,
        Guid ExternalPartyId,
        string DocumentKey,
        string ReviewStatus,
        DateTimeOffset? ExpiresAt,
        DateTimeOffset UpdatedAt);

}
