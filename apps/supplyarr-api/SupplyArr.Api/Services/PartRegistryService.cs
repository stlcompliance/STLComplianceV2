using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class PartRegistryService(
    SupplyArrDbContext db,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive"
    };

    private static readonly HashSet<string> AllowedSourceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "supplier",
        "manufacturer",
        "internal_fabrication",
        "rebuilt",
        "salvage",
        "customer_supplied",
        "transfer",
        "kit_assembly",
        "unknown"
    };

    public async Task<IReadOnlyList<PartResponse>> ListAsync(
        Guid tenantId,
        Guid? catalogId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Parts
            .AsNoTracking()
            .Include(x => x.PartCatalog)
            .Include(x => x.ManufacturerAliases)
            .Include(x => x.Sources)
            .Include(x => x.SupplierLinks)
            .ThenInclude(x => x.Supplier)
            .ThenInclude(x => x.ParentSupplier)
            .Where(x => x.TenantId == tenantId);

        if (catalogId is not null)
        {
            query = query.Where(x => x.PartCatalogId == catalogId);
        }

        var parts = await query
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        return parts.Select(Map).ToList();
    }

    public async Task<PartResponse> GetAsync(
        Guid tenantId,
        Guid partId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadPartAsync(tenantId, partId, cancellationToken);
        return Map(entity);
    }

    public async Task<PartResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreatePartRequest request,
        CancellationToken cancellationToken = default)
    {
        var partKey = NormalizePartKey(request.PartKey);
        var exists = await db.Parts.AnyAsync(
            x => x.TenantId == tenantId && x.PartKey == partKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "parts.duplicate",
                "A part with this key already exists.",
                409);
        }

        await ValidateCatalogAsync(tenantId, request.CatalogId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var entity = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartCatalogId = request.CatalogId,
            PartKey = partKey,
            DisplayName = NormalizeDisplayName(request.DisplayName),
            Description = NormalizeDescription(request.Description),
            CategoryKey = NormalizeCategoryKey(request.CategoryKey),
            UnitOfMeasure = NormalizeUnitOfMeasure(request.UnitOfMeasure),
            ManufacturerName = NormalizeManufacturerName(request.ManufacturerName),
            ManufacturerPartNumber = NormalizeManufacturerPartNumber(request.ManufacturerPartNumber),
            IsTrackable = request.IsTrackable ?? true,
            IsStocked = request.IsStocked ?? true,
            RequiresSerialLotTracking = request.RequiresSerialLotTracking,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Parts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "part.create",
            tenantId,
            actorUserId,
            "part",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.PartCreated,
            "part",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Part created: {entity.PartKey}"),
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplyArrItemCreated,
            "part",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Item created: {entity.PartKey}"),
            cancellationToken: cancellationToken);

        return Map(await LoadPartAsync(tenantId, entity.Id, cancellationToken));
    }

    public async Task<PartResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        UpdatePartRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.Parts.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == partId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        await ValidateCatalogAsync(tenantId, request.CatalogId, cancellationToken);

        entity.PartCatalogId = request.CatalogId;
        entity.DisplayName = NormalizeDisplayName(request.DisplayName);
        entity.Description = NormalizeDescription(request.Description);
        entity.CategoryKey = NormalizeCategoryKey(request.CategoryKey);
        entity.UnitOfMeasure = NormalizeUnitOfMeasure(request.UnitOfMeasure);
        entity.ManufacturerName = NormalizeManufacturerName(request.ManufacturerName);
        entity.ManufacturerPartNumber = NormalizeManufacturerPartNumber(request.ManufacturerPartNumber);
        entity.IsTrackable = request.IsTrackable ?? entity.IsTrackable;
        entity.IsStocked = request.IsStocked ?? entity.IsStocked;
        entity.RequiresSerialLotTracking = request.RequiresSerialLotTracking;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "part.update",
            tenantId,
            actorUserId,
            "part",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplyArrItemUpdated,
            "part",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Item updated: {entity.PartKey}"),
            cancellationToken: cancellationToken);

        return Map(await LoadPartAsync(tenantId, partId, cancellationToken));
    }

    public async Task<PartResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        UpdatePartStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);
        var entity = await db.Parts.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == partId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "part.status_update",
            tenantId,
            actorUserId,
            "part",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(await LoadPartAsync(tenantId, partId, cancellationToken));
    }

    public async Task<PartManufacturerAliasResponse> AddManufacturerAliasAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        CreatePartManufacturerAliasRequest request,
        CancellationToken cancellationToken = default)
    {
        var partExists = await db.Parts.AnyAsync(
            x => x.TenantId == tenantId && x.Id == partId,
            cancellationToken);
        if (!partExists)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        var aliasKey = NormalizeAliasKey(request.AliasKey);
        var duplicate = await db.PartManufacturerAliases.AnyAsync(
            x => x.TenantId == tenantId && x.PartId == partId && x.AliasKey == aliasKey,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "parts.alias_duplicate",
                "A manufacturer alias with this key already exists for the part.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new PartManufacturerAlias
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = partId,
            AliasKey = aliasKey,
            ManufacturerName = NormalizeManufacturerName(request.ManufacturerName),
            ManufacturerPartNumber = NormalizeManufacturerPartNumber(request.ManufacturerPartNumber),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PartManufacturerAliases.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "part_manufacturer_alias.create",
            tenantId,
            actorUserId,
            "part_manufacturer_alias",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapAlias(entity);
    }

    public async Task<PartSourceResponse> AddSourceAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        CreatePartSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var partExists = await db.Parts.AnyAsync(
            x => x.TenantId == tenantId && x.Id == partId,
            cancellationToken);
        if (!partExists)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new PartSource
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = partId,
            SourceType = NormalizeSourceType(request.SourceType),
            Label = NormalizeSourceLabel(request.Label),
            Notes = NormalizeSourceNotes(request.Notes),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PartSources.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "part_source.create",
            tenantId,
            actorUserId,
            "part_source",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapSource(entity);
    }

    public async Task<PartSupplierLinkResponse> AddSupplierLinkAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        CreatePartSupplierLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        var partExists = await db.Parts.AnyAsync(
            x => x.TenantId == tenantId && x.Id == partId,
            cancellationToken);
        if (!partExists)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        var supplierId = request.SupplierUnitId ?? request.SupplierId;
        if (supplierId is null)
        {
            throw new StlApiException(
                "parts.validation",
                "Supplier identity or sub-unit is required.",
                400);
        }

        var supplier = await db.Suppliers
            .Include(x => x.ParentSupplier)
            .FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == supplierId,
            cancellationToken);
        if (supplier is null)
        {
            throw new StlApiException("suppliers.not_found", "Supplier was not found.", 404);
        }

        var duplicate = await db.PartSupplierLinks.AnyAsync(
            x => x.TenantId == tenantId && x.PartId == partId && x.SupplierId == supplierId,
            cancellationToken);
        if (duplicate)
        {
            throw new StlApiException(
                "parts.supplier_link_duplicate",
                "This part is already linked to the supplier identity or sub-unit.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        if (request.IsPreferred)
        {
            var existingPreferred = await db.PartSupplierLinks
                .Where(x => x.TenantId == tenantId && x.PartId == partId && x.IsPreferred)
                .ToListAsync(cancellationToken);
            foreach (var link in existingPreferred)
            {
                link.IsPreferred = false;
                link.UpdatedAt = now;
            }
        }

        var entity = new PartSupplierLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = partId,
            SupplierId = supplierId.Value,
            SupplierPartNumber = NormalizeSupplierPartNumber(request.SupplierPartNumber),
            IsPreferred = request.IsPreferred,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PartSupplierLinks.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "part_supplier_link.create",
            tenantId,
            actorUserId,
            "part_supplier_link",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapSupplierLink(entity, supplier);
    }

    public async Task<PartSupplierLinkResponse> UpsertSupplierLinkCatalogPriceAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        Guid linkId,
        UpsertPartSupplierLinkCatalogPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        var link = await db.PartSupplierLinks
            .Include(x => x.Supplier)
            .ThenInclude(x => x.ParentSupplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PartId == partId && x.Id == linkId,
                cancellationToken);
        if (link is null)
        {
            throw new StlApiException("parts.supplier_link.not_found", "Part supplier link was not found.", 404);
        }

        link.CatalogUnitPrice = NormalizeCatalogUnitPrice(request.CatalogUnitPrice);
        link.CatalogCurrencyCode = NormalizeCatalogCurrencyCode(request.CatalogCurrencyCode);
        link.CatalogMinimumOrderQuantity = NormalizeOptionalCatalogQuantity(request.CatalogMinimumOrderQuantity);
        link.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "part_supplier_link.catalog_price.upsert",
            tenantId,
            actorUserId,
            "part_supplier_link",
            link.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapSupplierLink(link, link.Supplier);
    }

    public async Task<PartSupplierLinkResponse> UpsertSupplierLinkCatalogLeadTimeAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        Guid linkId,
        UpsertPartSupplierLinkCatalogLeadTimeRequest request,
        CancellationToken cancellationToken = default)
    {
        var link = await db.PartSupplierLinks
            .Include(x => x.Supplier)
            .ThenInclude(x => x.ParentSupplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PartId == partId && x.Id == linkId,
                cancellationToken);
        if (link is null)
        {
            throw new StlApiException("parts.supplier_link.not_found", "Part supplier link was not found.", 404);
        }

        link.CatalogLeadTimeDays = LeadTimeSnapshotCaptureRules.NormalizeLeadTimeDays(request.CatalogLeadTimeDays);
        link.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "part_supplier_link.catalog_lead_time.upsert",
            tenantId,
            actorUserId,
            "part_supplier_link",
            link.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapSupplierLink(link, link.Supplier);
    }

    public async Task<PartSupplierLinkResponse> UpsertSupplierLinkCatalogAvailabilityAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partId,
        Guid linkId,
        UpsertPartSupplierLinkCatalogAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var link = await db.PartSupplierLinks
            .Include(x => x.Supplier)
            .ThenInclude(x => x.ParentSupplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PartId == partId && x.Id == linkId,
                cancellationToken);
        if (link is null)
        {
            throw new StlApiException("parts.supplier_link.not_found", "Part supplier link was not found.", 404);
        }

        if (request.CatalogQuantityAvailable is null && string.IsNullOrWhiteSpace(request.CatalogAvailabilityStatus))
        {
            throw new StlApiException(
                "parts.validation",
                "Catalog availability requires quantity and/or status.",
                400);
        }

        link.CatalogQuantityAvailable = AvailabilitySnapshotCaptureRules.NormalizeOptionalQuantity(
            request.CatalogQuantityAvailable);
        link.CatalogAvailabilityStatus = AvailabilitySnapshotCaptureRules.NormalizeOptionalStatus(
            request.CatalogAvailabilityStatus);
        link.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "part_supplier_link.catalog_availability.upsert",
            tenantId,
            actorUserId,
            "part_supplier_link",
            link.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapSupplierLink(link, link.Supplier);
    }

    private async Task ValidateCatalogAsync(
        Guid tenantId,
        Guid? catalogId,
        CancellationToken cancellationToken)
    {
        if (catalogId is null)
        {
            return;
        }

        var exists = await db.PartCatalogs.AnyAsync(
            x => x.TenantId == tenantId && x.Id == catalogId,
            cancellationToken);
        if (!exists)
        {
            throw new StlApiException("catalogs.not_found", "Part catalog was not found.", 404);
        }
    }

    private async Task<Part> LoadPartAsync(
        Guid tenantId,
        Guid partId,
        CancellationToken cancellationToken)
    {
        var entity = await db.Parts
            .AsNoTracking()
            .Include(x => x.PartCatalog)
            .Include(x => x.ManufacturerAliases.OrderBy(a => a.AliasKey))
            .Include(x => x.Sources.OrderBy(s => s.CreatedAt))
            .Include(x => x.SupplierLinks)
            .ThenInclude(x => x.Supplier)
            .ThenInclude(x => x.ParentSupplier)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == partId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("parts.not_found", "Part was not found.", 404);
        }

        return entity;
    }

    private static PartResponse Map(Part entity) =>
        new(
            entity.Id,
            entity.PartKey,
            entity.PartCatalogId,
            entity.PartCatalog?.CatalogKey,
            entity.DisplayName,
            entity.Description,
            entity.CategoryKey,
            entity.UnitOfMeasure,
            entity.ManufacturerName,
            entity.ManufacturerPartNumber,
            entity.Status,
            entity.IsTrackable,
            entity.IsStocked,
            entity.RequiresSerialLotTracking,
            entity.ReorderPoint,
            entity.ReorderQuantity,
            entity.ManufacturerAliases
                .OrderBy(x => x.AliasKey)
                .Select(MapAlias)
                .ToList(),
            entity.Sources
                .OrderBy(x => x.CreatedAt)
                .Select(MapSource)
                .ToList(),
            entity.SupplierLinks
                .OrderByDescending(x => x.IsPreferred)
                .ThenBy(x => x.Supplier.DisplayName)
                .Select(x => MapSupplierLink(x, x.Supplier))
                .ToList(),
            entity.CreatedAt,
            entity.UpdatedAt);

    private static PartManufacturerAliasResponse MapAlias(PartManufacturerAlias entity) =>
        new(
            entity.Id,
            entity.AliasKey,
            entity.ManufacturerName,
            entity.ManufacturerPartNumber,
            entity.CreatedAt);

    private static PartSourceResponse MapSource(PartSource entity) =>
        new(
            entity.Id,
            entity.SourceType,
            entity.Label,
            entity.Notes,
            entity.CreatedAt);

    private static PartSupplierLinkResponse MapSupplierLink(PartSupplierLink entity, Supplier supplier) =>
        new(
            entity.Id,
            supplier.Id,
            supplier.SupplierKey,
            supplier.DisplayName,
            supplier.ParentSupplierId,
            supplier.ParentSupplier?.SupplierKey,
            supplier.ParentSupplier?.DisplayName,
            supplier.UnitKind,
            ParseServiceTypes(supplier.ServiceTypesJson),
            supplier.AddressLine1,
            supplier.Locality,
            supplier.RegionCode,
            supplier.PostalCode,
            supplier.CountryCode,
            entity.SupplierPartNumber,
            entity.IsPreferred,
            entity.CatalogUnitPrice,
            entity.CatalogCurrencyCode,
            entity.CatalogMinimumOrderQuantity,
            entity.CatalogLeadTimeDays,
            entity.CatalogQuantityAvailable,
            entity.CatalogAvailabilityStatus,
            entity.CreatedAt);

    private static IReadOnlyList<string> ParseServiceTypes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(value) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static decimal NormalizeCatalogUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
        {
            throw new StlApiException(
                "parts.validation",
                "Catalog unit price must be greater than zero.",
                400);
        }

        return decimal.Round(unitPrice, 4, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeCatalogCurrencyCode(string? currencyCode)
    {
        var normalized = string.IsNullOrWhiteSpace(currencyCode)
            ? "USD"
            : currencyCode.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
        {
            throw new StlApiException(
                "parts.validation",
                "Catalog currency code must be a 3-letter ISO code.",
                400);
        }

        return normalized;
    }

    private static decimal? NormalizeOptionalCatalogQuantity(decimal? quantity)
    {
        if (quantity is null)
        {
            return null;
        }

        if (quantity <= 0)
        {
            throw new StlApiException(
                "parts.validation",
                "Catalog minimum order quantity must be greater than zero.",
                400);
        }

        return decimal.Round(quantity.Value, 4, MidpointRounding.AwayFromZero);
    }

    private static string NormalizePartKey(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 128)
        {
            throw new StlApiException(
                "parts.validation",
                "Part key must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeDisplayName(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 256)
        {
            throw new StlApiException(
                "parts.validation",
                "Display name must be between 2 and 256 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeDescription(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeCategoryKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "general";
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 128)
        {
            throw new StlApiException(
                "parts.validation",
                "Category key must be 128 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizeUnitOfMeasure(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "each";
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 32)
        {
            throw new StlApiException(
                "parts.validation",
                "Unit of measure must be 32 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizeManufacturerName(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeManufacturerPartNumber(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeSourceType(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? "unknown"
            : value.Trim().ToLowerInvariant();
        if (!AllowedSourceTypes.Contains(normalized))
        {
            throw new StlApiException(
                "parts.validation",
                "Source type is not supported.",
                400);
        }

        return normalized;
    }

    private static string NormalizeSourceLabel(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length < 2 || trimmed.Length > 256)
        {
            throw new StlApiException(
                "parts.validation",
                "Source label must be between 2 and 256 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeSourceNotes(string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length > 1024)
        {
            throw new StlApiException(
                "parts.validation",
                "Source notes must be 1024 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeAliasKey(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 128)
        {
            throw new StlApiException(
                "parts.validation",
                "Alias key must be between 2 and 128 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeSupplierPartNumber(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "parts.validation",
                "Status must be active or inactive.",
                400);
        }

        return normalized;
    }
}


