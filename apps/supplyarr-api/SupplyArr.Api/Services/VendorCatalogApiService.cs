using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class VendorCatalogApiService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    private const string SyncType = "vendor_catalog_api";

    public async Task<VendorCatalogApiSyncResponse> SyncAsync(
        Guid tenantId,
        Guid actorUserId,
        VendorCatalogApiSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<VendorCatalogApiSyncIssue>();
        var supplierKey = NormalizeSupplierKey(request.SupplierKey, request.VendorPartyKey, issues);
        var items = request.Items?.ToList() ?? [];
        if (items.Count == 0)
        {
            issues.Add(new VendorCatalogApiSyncIssue(1, "supplier_catalog.empty", "At least one supplier catalog item is required."));
        }

        ExternalParty? supplier = null;
        if (!string.IsNullOrWhiteSpace(supplierKey))
        {
            supplier = await db.ExternalParties.FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.PartyKey == supplierKey
                    && (x.PartyType == "vendor" || x.PartyType == "supplier"),
                cancellationToken);
            if (supplier is null)
            {
                issues.Add(new VendorCatalogApiSyncIssue(1, "supplier.not_found", "Supplier identity or sub-unit was not found."));
            }
        }

        var normalizedItems = new List<NormalizedItem>(items.Count);
        var seenPartKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedPartKeys = items
            .Select(item => NormalizePartKey(item.PartKey))
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var parts = await db.Parts
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && normalizedPartKeys.Contains(x.PartKey))
            .ToDictionaryAsync(x => x.PartKey, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var (item, index) in items.Select((value, index) => (value, index)))
        {
            var itemNumber = index + 1;
            var partKey = NormalizePartKey(item.PartKey);
            var vendorPartNumber = NormalizeVendorPartNumber(item.VendorPartNumber);

            if (string.IsNullOrWhiteSpace(partKey))
            {
                issues.Add(new VendorCatalogApiSyncIssue(itemNumber, "part.key_required", "Part key is required."));
                continue;
            }

            if (!parts.TryGetValue(partKey, out var part))
            {
                issues.Add(new VendorCatalogApiSyncIssue(itemNumber, "part.not_found", "Part was not found."));
                continue;
            }

            if (!seenPartKeys.Add(partKey))
            {
                issues.Add(new VendorCatalogApiSyncIssue(itemNumber, "supplier_catalog.duplicate_in_file", "Part appears more than once in the sync payload."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(vendorPartNumber))
            {
                issues.Add(new VendorCatalogApiSyncIssue(itemNumber, "supplier_part_number.required", "Supplier part number is required."));
            }

            ValidateLength(itemNumber, "part_key", partKey, 2, 128, issues);
            ValidateLength(itemNumber, "supplier_part_number", vendorPartNumber, 1, 128, issues);
            ValidateMaxLength(itemNumber, "catalog_availability_status", item.CatalogAvailabilityStatus, 32, issues);

            var hasFacts = item.IsPreferred
                || item.CatalogUnitPrice is not null
                || item.CatalogLeadTimeDays is not null
                || item.CatalogQuantityAvailable is not null
                || !string.IsNullOrWhiteSpace(item.CatalogAvailabilityStatus);
            if (!hasFacts)
            {
                issues.Add(new VendorCatalogApiSyncIssue(
                    itemNumber,
                    "supplier_catalog.empty_facts",
                    "Supplier catalog sync requires price, lead time, quantity available, availability status, or preferred source flag."));
            }

            normalizedItems.Add(new NormalizedItem(
                itemNumber,
                part,
                vendorPartNumber,
                item.IsPreferred,
                item.CatalogUnitPrice,
                item.CatalogCurrencyCode,
                item.CatalogMinimumOrderQuantity,
                item.CatalogLeadTimeDays,
                item.CatalogQuantityAvailable,
                item.CatalogAvailabilityStatus));
        }

        var accepted = normalizedItems.Count;
        if (issues.Count > 0 || request.DryRun || supplier is null)
        {
            return new VendorCatalogApiSyncResponse(
                SyncType,
                request.DryRun,
                issues.Count == 0,
                items.Count,
                accepted,
                0,
                issues);
        }

        var now = DateTimeOffset.UtcNow;
        var created = 0;
        var updated = 0;
        var supplierId = supplier.Id;

        foreach (var item in normalizedItems)
        {
            var part = item.Part ?? throw new InvalidOperationException("Normalized supplier catalog items must include a resolved part.");
            var link = await db.PartVendorLinks
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.PartId == part.Id && x.ExternalPartyId == supplierId,
                    cancellationToken);

            var isNew = link is null;
            if (isNew)
            {
                link = new PartVendorLink
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PartId = part.Id,
                    ExternalPartyId = supplierId,
                    CreatedAt = now,
                    UpdatedAt = now,
                };
                db.PartVendorLinks.Add(link);
                created++;
            }
            else
            {
                updated++;
            }

            ArgumentNullException.ThrowIfNull(link);

            if (item.IsPreferred)
            {
                var existingPreferred = await db.PartVendorLinks
                    .Where(x => x.TenantId == tenantId && x.PartId == part.Id && x.IsPreferred && x.Id != link.Id)
                    .ToListAsync(cancellationToken);
                foreach (var preferred in existingPreferred)
                {
                    preferred.IsPreferred = false;
                    preferred.UpdatedAt = now;
                }
            }

            link.VendorPartNumber = item.VendorPartNumber;
            link.IsPreferred = item.IsPreferred;

            if (item.CatalogUnitPrice is not null)
            {
                link.CatalogUnitPrice = NormalizeUnitPrice(item.CatalogUnitPrice.Value);
            }
            else if (isNew)
            {
                link.CatalogUnitPrice = null;
            }

            if (!string.IsNullOrWhiteSpace(item.CatalogCurrencyCode))
            {
                link.CatalogCurrencyCode = NormalizeCurrencyCode(item.CatalogCurrencyCode);
            }
            else if (isNew)
            {
                link.CatalogCurrencyCode = "USD";
            }

            if (item.CatalogMinimumOrderQuantity is not null)
            {
                link.CatalogMinimumOrderQuantity = NormalizeOptionalQuantity(item.CatalogMinimumOrderQuantity.Value);
            }

            if (item.CatalogLeadTimeDays is not null)
            {
                link.CatalogLeadTimeDays = LeadTimeSnapshotCaptureRules.NormalizeLeadTimeDays(item.CatalogLeadTimeDays.Value);
            }

            if (item.CatalogQuantityAvailable is not null)
            {
                link.CatalogQuantityAvailable = NormalizeOptionalQuantity(item.CatalogQuantityAvailable.Value);
            }

            if (!string.IsNullOrWhiteSpace(item.CatalogAvailabilityStatus))
            {
                link.CatalogAvailabilityStatus = NormalizeAvailabilityStatus(item.CatalogAvailabilityStatus);
            }

            link.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "vendor_catalog.sync",
            tenantId,
            actorUserId,
            "vendor_catalog",
            supplierKey,
            "Succeeded",
            reasonCode: request.DryRun ? "dry_run" : $"applied:{created + updated}",
            cancellationToken: cancellationToken);

        return new VendorCatalogApiSyncResponse(
            SyncType,
            false,
            true,
            items.Count,
            accepted,
            created + updated,
            issues);
    }

    private static string NormalizeSupplierKey(
        string? supplierKey,
        string? vendorPartyKey,
        ICollection<VendorCatalogApiSyncIssue> issues)
    {
        var normalized = string.IsNullOrWhiteSpace(supplierKey)
            ? (string.IsNullOrWhiteSpace(vendorPartyKey) ? string.Empty : vendorPartyKey.Trim().ToLowerInvariant())
            : supplierKey.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            issues.Add(new VendorCatalogApiSyncIssue(1, "supplier.key_required", "Supplier key is required."));
            return string.Empty;
        }

        if (normalized.Length is < 2 or > 128)
        {
            issues.Add(new VendorCatalogApiSyncIssue(1, "supplier.key_invalid", "Supplier key must be between 2 and 128 characters."));
            return string.Empty;
        }

        return normalized;
    }

    private static string NormalizePartKey(string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();

    private static string NormalizeVendorPartNumber(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static decimal NormalizeUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
        {
            throw new StlApiException(
                "vendor_catalog.validation",
                "Catalog unit price must be greater than zero.",
                400);
        }

        return decimal.Round(unitPrice, 4, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeCurrencyCode(string? currencyCode)
    {
        var normalized = string.IsNullOrWhiteSpace(currencyCode)
            ? "USD"
            : currencyCode.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
        {
            throw new StlApiException(
                "vendor_catalog.validation",
                "Catalog currency code must be a 3-letter ISO code.",
                400);
        }

        return normalized;
    }

    private static decimal NormalizeOptionalQuantity(decimal quantity)
    {
        if (quantity < 0)
        {
            throw new StlApiException(
                "vendor_catalog.validation",
                "Catalog quantity must be non-negative.",
                400);
        }

        return decimal.Round(quantity, 4, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeAvailabilityStatus(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new StlApiException(
                "vendor_catalog.validation",
                "Catalog availability status cannot be blank.",
                400);
        }

        if (normalized.Length > 32)
        {
            throw new StlApiException(
                "vendor_catalog.validation",
                "Catalog availability status must be 32 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static void ValidateLength(
        int itemNumber,
        string column,
        string value,
        int minLength,
        int maxLength,
        ICollection<VendorCatalogApiSyncIssue> issues)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            issues.Add(new VendorCatalogApiSyncIssue(
                itemNumber,
                "vendor_catalog.validation",
                $"{column} must be between {minLength} and {maxLength} characters."));
        }
    }

    private static void ValidateMaxLength(
        int itemNumber,
        string column,
        string? value,
        int maxLength,
        ICollection<VendorCatalogApiSyncIssue> issues)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length > maxLength)
        {
            issues.Add(new VendorCatalogApiSyncIssue(
                itemNumber,
                "vendor_catalog.validation",
                $"{column} must be {maxLength} characters or fewer."));
        }
    }

    private sealed record NormalizedItem(
        int ItemNumber,
        Part Part,
        string VendorPartNumber,
        bool IsPreferred,
        decimal? CatalogUnitPrice,
        string? CatalogCurrencyCode,
        decimal? CatalogMinimumOrderQuantity,
        int? CatalogLeadTimeDays,
        decimal? CatalogQuantityAvailable,
        string? CatalogAvailabilityStatus);
}
