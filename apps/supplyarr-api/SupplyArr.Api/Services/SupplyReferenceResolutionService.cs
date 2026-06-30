using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class SupplyReferenceResolutionService(SupplyArrDbContext db)
{
    public async Task<SupplyReferenceResolutionResponse> ResolveByIdAsync(
        Guid tenantId,
        string referenceType,
        Guid referenceId,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeReferenceType(referenceType);

        return normalizedType switch
        {
            SupplyReferenceTypes.Supplier => BuildSupplierResponse(
                await db.ExternalParties.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == referenceId, cancellationToken)),
            SupplyReferenceTypes.ExternalParty => BuildSupplierResponse(
                await db.ExternalParties.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == referenceId, cancellationToken)),
            SupplyReferenceTypes.Part => BuildPartResponse(
                await db.Parts.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == referenceId, cancellationToken)),
            SupplyReferenceTypes.PurchaseRequest => BuildPurchaseRequestResponse(
                await db.PurchaseRequests.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == referenceId, cancellationToken)),
            SupplyReferenceTypes.PurchaseOrder => BuildPurchaseOrderResponse(
                await db.PurchaseOrders.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == referenceId, cancellationToken)),
            SupplyReferenceTypes.ReceivingReceipt => BuildReceivingReceiptResponse(
                await db.ReceivingReceipts.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == referenceId, cancellationToken)),
            SupplyReferenceTypes.WarrantyClaim => BuildWarrantyClaimResponse(
                await db.WarrantyClaims.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == referenceId, cancellationToken)),
            SupplyReferenceTypes.VendorReturn => BuildVendorReturnResponse(
                await db.VendorReturns.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == referenceId, cancellationToken)),
            _ => throw UnsupportedReferenceType(normalizedType),
        };
    }

    public async Task<SupplyReferenceResolutionResponse> ResolveByKeyAsync(
        Guid tenantId,
        string referenceType,
        string referenceKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeReferenceType(referenceType);
        var normalizedKey = NormalizeReferenceKey(referenceKey).ToLowerInvariant();

        return normalizedType switch
        {
            SupplyReferenceTypes.Supplier => BuildSupplierResponse(
                await db.ExternalParties.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PartyKey.ToLower() == normalizedKey, cancellationToken)),
            SupplyReferenceTypes.ExternalParty => BuildSupplierResponse(
                await db.ExternalParties.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PartyKey.ToLower() == normalizedKey, cancellationToken)),
            SupplyReferenceTypes.Part => BuildPartResponse(
                await db.Parts.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.PartKey.ToLower() == normalizedKey, cancellationToken)),
            SupplyReferenceTypes.PurchaseRequest => BuildPurchaseRequestResponse(
                await db.PurchaseRequests.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.RequestKey.ToLower() == normalizedKey, cancellationToken)),
            SupplyReferenceTypes.PurchaseOrder => BuildPurchaseOrderResponse(
                await db.PurchaseOrders.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.OrderKey.ToLower() == normalizedKey, cancellationToken)),
            SupplyReferenceTypes.ReceivingReceipt => BuildReceivingReceiptResponse(
                await db.ReceivingReceipts.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ReceiptKey.ToLower() == normalizedKey, cancellationToken)),
            SupplyReferenceTypes.WarrantyClaim => BuildWarrantyClaimResponse(
                await db.WarrantyClaims.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ClaimKey.ToLower() == normalizedKey, cancellationToken)),
            SupplyReferenceTypes.VendorReturn => BuildVendorReturnResponse(
                await db.VendorReturns.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ReturnKey.ToLower() == normalizedKey, cancellationToken)),
            _ => throw UnsupportedReferenceType(normalizedType),
        };
    }

    private static string NormalizeReferenceType(string referenceType)
    {
        var normalized = referenceType.Trim().Replace('-', '_').ToLowerInvariant();
        return normalized switch
        {
            "party" or "vendor" or "dealer" or "carrier" or "supplier" => SupplyReferenceTypes.Supplier,
            "external_party" or "customer" => SupplyReferenceTypes.ExternalParty,
            "item" or "material" => SupplyReferenceTypes.Part,
            "pr" => SupplyReferenceTypes.PurchaseRequest,
            "po" => SupplyReferenceTypes.PurchaseOrder,
            "receipt" or "receiving" => SupplyReferenceTypes.ReceivingReceipt,
            "warranty" => SupplyReferenceTypes.WarrantyClaim,
            "return" => SupplyReferenceTypes.VendorReturn,
            _ => normalized,
        };
    }

    private static string NormalizeReferenceKey(string referenceKey)
    {
        var normalized = referenceKey.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "supplyarr.reference_key_required",
                "Reference key is required.",
                400);
        }

        return normalized;
    }

    private static StlApiException UnsupportedReferenceType(string referenceType) =>
        new(
            "supplyarr.reference_type_unsupported",
            $"Reference type '{referenceType}' is not supported.",
            400);

    private static StlApiException NotFound(string referenceType) =>
        new(
            "supplyarr.reference_not_found",
            $"SupplyArr {referenceType} reference was not found.",
            404);

    private static SupplyReferenceResolutionResponse BuildSupplierResponse(ExternalParty? entity)
    {
        if (entity is null)
        {
            throw NotFound(SupplyReferenceTypes.Supplier);
        }

        var apiPath = $"/api/suppliers/{entity.Id}";
        var appPath = $"/suppliers/{entity.Id}";
        var metadata = new Dictionary<string, string>
        {
            ["supplierType"] = entity.PartyType,
            ["approvalStatus"] = entity.ApprovalStatus,
            ["unitKind"] = entity.UnitKind,
            ["parentSupplierId"] = entity.ParentExternalPartyId?.ToString("D") ?? string.Empty,
            ["partyType"] = entity.PartyType,
        };

        return BuildResponse(
            entity.TenantId,
            SupplyReferenceTypes.Supplier,
            entity.Id,
            entity.PartyKey,
            entity.DisplayName,
            entity.Status,
            apiPath,
            appPath,
            entity.UpdatedAt,
            metadata);
    }

    private static SupplyReferenceResolutionResponse BuildPartResponse(Part? entity)
    {
        if (entity is null)
        {
            throw NotFound(SupplyReferenceTypes.Part);
        }

        return BuildResponse(
            entity.TenantId,
            SupplyReferenceTypes.Part,
            entity.Id,
            entity.PartKey,
            entity.DisplayName,
            entity.Status,
            $"/api/parts/{entity.Id}",
            $"/parts/{entity.Id}",
            entity.UpdatedAt,
            new Dictionary<string, string>
            {
                ["categoryKey"] = entity.CategoryKey,
                ["unitOfMeasure"] = entity.UnitOfMeasure,
                ["manufacturerName"] = entity.ManufacturerName,
                ["manufacturerPartNumber"] = entity.ManufacturerPartNumber,
            });
    }

    private static SupplyReferenceResolutionResponse BuildPurchaseRequestResponse(PurchaseRequest? entity)
    {
        if (entity is null)
        {
            throw NotFound(SupplyReferenceTypes.PurchaseRequest);
        }

        return BuildResponse(
            entity.TenantId,
            SupplyReferenceTypes.PurchaseRequest,
            entity.Id,
            entity.RequestKey,
            entity.Title,
            entity.Status,
            $"/api/purchase-requests/{entity.Id}",
            $"/purchase-requests/{entity.Id}",
            entity.UpdatedAt,
            new Dictionary<string, string>
            {
                ["isEmergency"] = entity.IsEmergency.ToString(),
                ["supplierId"] = entity.VendorPartyId?.ToString() ?? string.Empty,
                ["vendorPartyId"] = entity.VendorPartyId?.ToString() ?? string.Empty,
            });
    }

    private static SupplyReferenceResolutionResponse BuildPurchaseOrderResponse(PurchaseOrder? entity)
    {
        if (entity is null)
        {
            throw NotFound(SupplyReferenceTypes.PurchaseOrder);
        }

        return BuildResponse(
            entity.TenantId,
            SupplyReferenceTypes.PurchaseOrder,
            entity.Id,
            entity.OrderKey,
            entity.Title,
            entity.Status,
            $"/api/purchase-orders/{entity.Id}",
            $"/purchase-orders/{entity.Id}",
            entity.UpdatedAt,
            new Dictionary<string, string>
            {
                ["purchaseRequestId"] = entity.PurchaseRequestId.ToString(),
                ["supplierId"] = entity.VendorPartyId.ToString(),
                ["vendorPartyId"] = entity.VendorPartyId.ToString(),
            });
    }

    private static SupplyReferenceResolutionResponse BuildReceivingReceiptResponse(ReceivingReceipt? entity)
    {
        if (entity is null)
        {
            throw NotFound(SupplyReferenceTypes.ReceivingReceipt);
        }

        return BuildResponse(
            entity.TenantId,
            SupplyReferenceTypes.ReceivingReceipt,
            entity.Id,
            entity.ReceiptKey,
            entity.ReceiptKey,
            entity.Status,
            $"/api/receiving/{entity.Id}",
            $"/receiving/{entity.Id}",
            entity.UpdatedAt,
            new Dictionary<string, string>
            {
                ["purchaseOrderId"] = entity.PurchaseOrderId.ToString(),
                ["inventoryBinId"] = entity.InventoryBinId.ToString(),
            });
    }

    private static SupplyReferenceResolutionResponse BuildWarrantyClaimResponse(WarrantyClaim? entity)
    {
        if (entity is null)
        {
            throw NotFound(SupplyReferenceTypes.WarrantyClaim);
        }

        return BuildResponse(
            entity.TenantId,
            SupplyReferenceTypes.WarrantyClaim,
            entity.Id,
            entity.ClaimKey,
            entity.ProblemDescription,
            entity.Status,
            $"/api/warranty-claims/{entity.Id}",
            $"/warranty-claims/{entity.Id}",
            entity.UpdatedAt,
            new Dictionary<string, string>
            {
                ["claimType"] = entity.ClaimType,
                ["supplierId"] = entity.VendorPartyId.ToString(),
                ["vendorPartyId"] = entity.VendorPartyId.ToString(),
                ["partId"] = entity.PartId.ToString(),
            });
    }

    private static SupplyReferenceResolutionResponse BuildVendorReturnResponse(VendorReturn? entity)
    {
        if (entity is null)
        {
            throw NotFound(SupplyReferenceTypes.VendorReturn);
        }

        return BuildResponse(
            entity.TenantId,
            SupplyReferenceTypes.VendorReturn,
            entity.Id,
            entity.ReturnKey,
            entity.ReturnKey,
            entity.Status,
            $"/api/returns/{entity.Id}",
            $"/returns/{entity.Id}",
            entity.UpdatedAt,
            new Dictionary<string, string>
            {
                ["sourceType"] = entity.SourceType,
                ["supplierId"] = entity.VendorPartyId.ToString(),
                ["vendorPartyId"] = entity.VendorPartyId.ToString(),
                ["purchaseOrderId"] = entity.PurchaseOrderId?.ToString() ?? string.Empty,
            });
    }

    private static SupplyReferenceResolutionResponse BuildResponse(
        Guid tenantId,
        string referenceType,
        Guid referenceId,
        string displayCode,
        string displayName,
        string status,
        string apiPath,
        string deepLinkPath,
        DateTimeOffset updatedAt,
        IReadOnlyDictionary<string, string> metadata) =>
        new(
            tenantId,
            referenceType,
            referenceId,
            displayCode,
            displayName,
            status,
            "supplyarr",
            apiPath,
            deepLinkPath,
            updatedAt,
            metadata);
}
