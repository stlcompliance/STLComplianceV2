using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public class SupplierDirectoryService(
    SupplyArrDbContext db,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive"
    };

    private static readonly HashSet<string> AllowedApprovalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending",
        "approved",
        "restricted",
        "inactive"
    };

    private static readonly HashSet<string> AllowedUnitKinds = new(StringComparer.OrdinalIgnoreCase)
    {
        "identity",
        "sub_unit"
    };

    private static readonly HashSet<string> AllowedServiceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "products",
        "parts",
        "maintenance",
        "repair",
        "warranty",
        "field_service",
        "logistics"
    };

    public SupplierDirectoryMetadataResponse GetSupplierMetadata() =>
        new(
            SupplierOptions(
                ("pending", "Pending"),
                ("approved", "Approved"),
                ("restricted", "Restricted"),
                ("inactive", "Inactive (approval)")),
            SupplierOptions(
                ("active", "Active"),
                ("inactive", "Inactive")),
            SupplierOptions(
                ("identity", "Supplier identity"),
                ("sub_unit", "Supplier sub-unit")),
            SupplierOptions(
                ("products", "Products"),
                ("parts", "Parts"),
                ("maintenance", "Maintenance"),
                ("repair", "Repair"),
                ("warranty", "Warranty"),
                ("field_service", "Field service"),
                ("logistics", "Logistics")));

    public async Task<IReadOnlyList<SupplierResponse>> ListSuppliersAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var suppliers = await db.Suppliers
            .AsNoTracking()
            .Include(x => x.Contacts)
            .Include(x => x.ParentSupplier)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.ParentSupplierId.HasValue)
            .ThenBy(x => x.ParentSupplierId)
            .ThenBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        var childCounts = await db.Suppliers
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ParentSupplierId.HasValue)
            .GroupBy(x => x.ParentSupplierId!.Value)
            .Select(x => new { ParentId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.ParentId, x => x.Count, cancellationToken);

        return suppliers
            .Select(supplier => MapSupplier(supplier, childCounts.GetValueOrDefault(supplier.Id)))
            .ToList();
    }

    public async Task<SupplierResponse> GetSupplierAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        var supplier = await LoadSupplierAsync(tenantId, supplierId, cancellationToken);
        var childCount = await db.Suppliers
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == tenantId
                   
                    && x.ParentSupplierId == supplierId,
                cancellationToken);
        return MapSupplier(supplier, childCount);
    }

    public async Task<SupplierResponse> CreateSupplierAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        return await CreateSupplierInternalAsync(tenantId, actorUserId, request, cancellationToken);
    }

    public async Task<SupplierResponse> UpdateSupplierAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.Suppliers.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == supplierId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("suppliers.not_found", "Supplier was not found.", 404);
        }

        entity.DisplayName = NormalizeDisplayName(request.DisplayName);
        entity.LegalName = NormalizeLegalName(request.LegalName);
        entity.TaxIdentifier = NormalizeTaxIdentifier(request.TaxIdentifier);
        entity.Notes = NormalizeNotes(request.Notes);
        entity.ParentSupplierId = await ResolveParentSupplierIdAsync(tenantId, request.ParentSupplierId, supplierId, cancellationToken);
        entity.UnitKind = NormalizeUnitKind(request.UnitKind, entity.ParentSupplierId);
        entity.ServiceTypesJson = SerializeServiceTypes(request.ServiceTypes);
        entity.AddressLine1 = NormalizeAddressLine(request.AddressLine1, "Address line 1", 256);
        entity.AddressLine2 = NormalizeAddressLine(request.AddressLine2, "Address line 2", 256);
        entity.Locality = NormalizeAddressLine(request.Locality, "City/locality", 128);
        entity.RegionCode = NormalizeAddressLine(request.RegionCode, "Region/state", 64);
        entity.PostalCode = NormalizeAddressLine(request.PostalCode, "Postal code", 32);
        entity.CountryCode = NormalizeCountryCode(request.CountryCode);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "supplier.update",
            tenantId,
            actorUserId,
            "supplier",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await EnqueueSupplierEventAsync(
            tenantId,
            entity,
            IntegrationOutboxEventKinds.SupplyArrSupplierUpdated,
            $"Supplier record updated: {entity.DisplayName}",
            cancellationToken);

        return await GetSupplierAsync(tenantId, supplierId, cancellationToken);
    }

    public async Task<SupplierResponse> UpdateSupplierApprovalStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        UpdateSupplierApprovalStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var approvalStatus = NormalizeApprovalStatus(request.ApprovalStatus);
        var entity = await db.Suppliers.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == supplierId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("suppliers.not_found", "Supplier was not found.", 404);
        }

        entity.ApprovalStatus = approvalStatus;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "supplier.approval_status_update",
            tenantId,
            actorUserId,
            "supplier",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        var approvalEventKind = entity.ApprovalStatus switch
        {
            "approved" => IntegrationOutboxEventKinds.SupplyArrSupplierApproved,
            "restricted" or "inactive" => IntegrationOutboxEventKinds.SupplyArrSupplierBlocked,
            _ => null,
        };
        await EnqueueSupplierEventAsync(
            tenantId,
            entity,
            approvalEventKind,
            $"Supplier approval status changed to {entity.ApprovalStatus}: {entity.DisplayName}",
            cancellationToken);

        return await GetSupplierAsync(tenantId, supplierId, cancellationToken);
    }

    public async Task<SupplierResponse> UpdateSupplierStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        UpdateSupplierStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);
        var entity = await db.Suppliers.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == supplierId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("suppliers.not_found", "Supplier was not found.", 404);
        }

        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "supplier.status_update",
            tenantId,
            actorUserId,
            "supplier",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetSupplierAsync(tenantId, supplierId, cancellationToken);
    }

    public async Task<SupplierContactResponse> AddSupplierContactAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid supplierId,
        CreateSupplierContactRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplierExists = await db.Suppliers.AnyAsync(
            x => x.TenantId == tenantId && x.Id == supplierId,
            cancellationToken);
        if (!supplierExists)
        {
            throw new StlApiException("suppliers.not_found", "Supplier was not found.", 404);
        }

        var contactName = NormalizeContactName(request.ContactName);
        var now = DateTimeOffset.UtcNow;

        if (request.IsPrimary)
        {
            var existingPrimary = await db.SupplierContacts
                .Where(x => x.TenantId == tenantId && x.SupplierId == supplierId && x.IsPrimary)
                .ToListAsync(cancellationToken);
            foreach (var contact in existingPrimary)
            {
                contact.IsPrimary = false;
                contact.UpdatedAt = now;
            }
        }

        var entity = new SupplierContact
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierId = supplierId,
            ContactName = contactName,
            Email = NormalizeEmail(request.Email),
            Phone = NormalizePhone(request.Phone),
            RoleLabel = NormalizeRoleLabel(request.RoleLabel),
            IsPrimary = request.IsPrimary,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.SupplierContacts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "supplier_contact.create",
            tenantId,
            actorUserId,
            "supplier_contact",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapSupplierContact(entity);
    }

    private async Task<SupplierResponse> CreateSupplierInternalAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedKey = NormalizeSupplierKey(request.SupplierKey);
        var exists = await db.Suppliers.AnyAsync(
            x => x.TenantId == tenantId && x.SupplierKey == normalizedKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "suppliers.duplicate",
                "A supplier record with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var resolvedParentSupplierId = await ResolveParentSupplierIdAsync(tenantId, request.ParentSupplierId, null, cancellationToken);
        var entity = new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierKey = normalizedKey,
            
            ParentSupplierId = resolvedParentSupplierId,
            UnitKind = NormalizeUnitKind(request.UnitKind, resolvedParentSupplierId),
            DisplayName = NormalizeDisplayName(request.DisplayName),
            LegalName = NormalizeLegalName(request.LegalName),
            TaxIdentifier = NormalizeTaxIdentifier(request.TaxIdentifier),
            ApprovalStatus = "pending",
            Status = "active",
            Notes = NormalizeNotes(request.Notes),
            ServiceTypesJson = SerializeServiceTypes(request.ServiceTypes),
            AddressLine1 = NormalizeAddressLine(request.AddressLine1, "Address line 1", 256),
            AddressLine2 = NormalizeAddressLine(request.AddressLine2, "Address line 2", 256),
            Locality = NormalizeAddressLine(request.Locality, "City/locality", 128),
            RegionCode = NormalizeAddressLine(request.RegionCode, "Region/state", 64),
            PostalCode = NormalizeAddressLine(request.PostalCode, "Postal code", 32),
            CountryCode = NormalizeCountryCode(request.CountryCode),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Suppliers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "supplier.create",
            tenantId,
            actorUserId,
            "supplier",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplierCreated,
            "supplier",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Supplier record created: {entity.DisplayName}"),
            cancellationToken: cancellationToken);

        await EnqueueSupplierEventAsync(
            tenantId,
            entity,
            IntegrationOutboxEventKinds.SupplyArrSupplierCreated,
            $"Supplier record created: {entity.DisplayName}",
            cancellationToken);

        return await GetSupplierAsync(tenantId, entity.Id, cancellationToken);
    }

    private async Task EnqueueSupplierEventAsync(
        Guid tenantId,
        Supplier entity,
        string? eventKind,
        string summary,
        CancellationToken cancellationToken)
    {
        if (eventKind is null)
        {
            return;
        }

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            eventKind,
            "supplier",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, summary, entity.Id),
            cancellationToken: cancellationToken);
    }

    private async Task<Supplier> LoadSupplierAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        var entity = await db.Suppliers
            .AsNoTracking()
            .Include(x => x.Contacts.OrderBy(c => c.ContactName))
            .Include(x => x.ParentSupplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.Id == supplierId
                   ,
                cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("suppliers.not_found", "Supplier was not found.", 404);
        }

        return entity;
    }

    private static SupplierResponse MapSupplier(Supplier entity, int childUnitCount) =>
        new(
            entity.Id,
            entity.SupplierKey,
            entity.ParentSupplierId,
            entity.ParentSupplier?.DisplayName,
            entity.UnitKind,
            entity.DisplayName,
            entity.LegalName,
            entity.TaxIdentifier,
            entity.ApprovalStatus,
            entity.Status,
            entity.Notes,
            DeserializeServiceTypes(entity.ServiceTypesJson),
            entity.AddressLine1,
            entity.AddressLine2,
            entity.Locality,
            entity.RegionCode,
            entity.PostalCode,
            entity.CountryCode,
            childUnitCount,
            entity.Contacts
                .OrderBy(x => x.ContactName)
                .Select(MapSupplierContact)
                .ToList(),
            entity.CreatedAt,
            entity.UpdatedAt);

    private static SupplierContactResponse MapSupplierContact(SupplierContact entity) =>
        new(
            entity.Id,
            entity.ContactName,
            entity.Email,
            entity.Phone,
            entity.RoleLabel,
            entity.IsPrimary,
            entity.CreatedAt);

    private static string NormalizeSupplierKey(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 128)
        {
            throw new StlApiException(
                "suppliers.validation",
                "Supplier key must be between 2 and 128 characters.",
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
                "suppliers.validation",
                "Supplier display name must be between 2 and 256 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeLegalName(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length > 256)
        {
            throw new StlApiException(
                "suppliers.validation",
                "Supplier legal name must be 256 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeTaxIdentifier(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeNotes(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeUnitKind(string? unitKind, Guid? parentSupplierId)
    {
        var normalized = string.IsNullOrWhiteSpace(unitKind)
            ? (parentSupplierId.HasValue ? "sub_unit" : "identity")
            : unitKind.Trim().ToLowerInvariant();
        if (!AllowedUnitKinds.Contains(normalized))
        {
            throw new StlApiException(
                "suppliers.validation",
                "Supplier unit kind must be identity or sub_unit.",
                400);
        }

        if (parentSupplierId.HasValue && normalized != "sub_unit")
        {
            throw new StlApiException(
                "suppliers.validation",
                "Child supplier records must use the sub_unit kind.",
                400);
        }

        if (!parentSupplierId.HasValue && normalized == "sub_unit")
        {
            throw new StlApiException(
                "suppliers.validation",
                "Supplier locations must be assigned to a parent supplier identity.",
                400);
        }

        return normalized;
    }

    private static string SerializeServiceTypes(IReadOnlyList<string>? serviceTypes)
    {
        var normalized = (serviceTypes ?? [])
            .Select(x => x?.Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var serviceType in normalized)
        {
            if (!AllowedServiceTypes.Contains(serviceType!))
            {
                throw new StlApiException(
                    "suppliers.validation",
                    "Service types must be products, parts, maintenance, repair, warranty, field_service, or logistics.",
                    400);
            }
        }

        return JsonSerializer.Serialize(normalized);
    }

    private static IReadOnlyList<string> DeserializeServiceTypes(string? serviceTypesJson)
    {
        if (string.IsNullOrWhiteSpace(serviceTypesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(serviceTypesJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string NormalizeAddressLine(string? value, string fieldLabel, int maxLength)
    {
        var trimmed = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new StlApiException(
                "suppliers.validation",
                $"{fieldLabel} must be {maxLength} characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeCountryCode(string? value)
    {
        var trimmed = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
        if (trimmed.Length > 2)
        {
            throw new StlApiException(
                "suppliers.validation",
                "Country code must be 2 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "suppliers.validation",
                "Status must be active or inactive.",
                400);
        }

        return normalized;
    }

    private static string NormalizeApprovalStatus(string approvalStatus)
    {
        var normalized = approvalStatus.Trim().ToLowerInvariant();
        if (!AllowedApprovalStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "suppliers.validation",
                "Approval status must be pending, approved, restricted, or inactive.",
                400);
        }

        return normalized;
    }

    private static string NormalizeContactName(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length < 2 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "suppliers.validation",
                "Contact name must be between 2 and 128 characters.",
                400);
        }

        return trimmed;
    }

    private static string NormalizeEmail(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizePhone(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeRoleLabel(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private async Task<Guid?> ResolveParentSupplierIdAsync(
        Guid tenantId,
        Guid? requestedParentSupplierId,
        Guid? currentSupplierId,
        CancellationToken cancellationToken)
    {
        if (!requestedParentSupplierId.HasValue)
        {
            return null;
        }

        if (currentSupplierId.HasValue && requestedParentSupplierId == currentSupplierId)
        {
            throw new StlApiException(
                "suppliers.validation",
                "A supplier cannot be its own parent.",
                400);
        }

        var parent = await db.Suppliers
            .AsNoTracking()
            .Where(
                x => x.TenantId == tenantId
                    && x.Id == requestedParentSupplierId.Value)
            .Select(x => new
            {
                x.Id,
                x.ParentSupplierId,
                x.Status,
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (parent is null)
        {
            throw new StlApiException(
                "suppliers.validation",
                "Parent supplier identity was not found.",
                400);
        }

        if (parent.ParentSupplierId.HasValue)
        {
            throw new StlApiException(
                "suppliers.validation",
                "Sub-units must belong directly to a parent supplier identity.",
                400);
        }

        if (!string.Equals(parent.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "suppliers.validation",
                "Parent supplier identity must be active before adding sub-units.",
                400);
        }

        return requestedParentSupplierId.Value;
    }

    private static IReadOnlyList<SupplierDirectoryCatalogOptionResponse> SupplierOptions(
        params (string Value, string Label)[] options) =>
        options
            .Select(option => new SupplierDirectoryCatalogOptionResponse(option.Value, option.Label))
            .ToList();
}


