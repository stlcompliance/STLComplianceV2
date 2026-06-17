using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class ExternalPartyService(
    SupplyArrDbContext db,
    IntegrationOutboxEnqueueService integrationOutbox,
    ISupplyArrAuditService audit)
{
    private static readonly HashSet<string> AllowedPartyTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "vendor",
        "dealer",
        "supplier",
        "carrier",
        "customer"
    };

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

    public PartyRegistryMetadataResponse GetMetadata() =>
        new(
            Options(
                ("pending", "Pending"),
                ("approved", "Approved"),
                ("restricted", "Restricted"),
                ("inactive", "Inactive (approval)")),
            Options(
                ("active", "Active"),
                ("inactive", "Inactive")));

    public async Task<IReadOnlyList<ExternalPartyResponse>> ListAsync(
        Guid tenantId,
        string? partyType = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizePartyTypeOrNull(partyType);
        var query = db.ExternalParties
            .AsNoTracking()
            .Include(x => x.Contacts)
            .Where(x => x.TenantId == tenantId);

        if (normalizedType is not null)
        {
            query = query.Where(x => x.PartyType == normalizedType);
        }

        var parties = await query
            .OrderBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

        return parties.Select(Map).ToList();
    }

    public async Task<ExternalPartyResponse> GetAsync(
        Guid tenantId,
        Guid partyId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadPartyAsync(tenantId, partyId, cancellationToken);
        return Map(entity);
    }

    public async Task<ExternalPartyResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateExternalPartyRequest request,
        CancellationToken cancellationToken = default)
    {
        var partyType = NormalizePartyType(request.PartyType);
        return await CreateInternalAsync(
            tenantId,
            actorUserId,
            partyType,
            request.PartyKey,
            request.DisplayName,
            request.LegalName,
            request.TaxIdentifier,
            request.Notes,
            cancellationToken);
    }

    public async Task<ExternalPartyResponse> CreateTypedAsync(
        Guid tenantId,
        Guid actorUserId,
        string partyType,
        CreateTypedExternalPartyRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizePartyType(partyType);
        return await CreateInternalAsync(
            tenantId,
            actorUserId,
            normalizedType,
            request.PartyKey,
            request.DisplayName,
            request.LegalName,
            request.TaxIdentifier,
            request.Notes,
            cancellationToken);
    }

    public async Task<ExternalPartyResponse> UpdateAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partyId,
        UpdateExternalPartyRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.ExternalParties.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == partyId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("parties.not_found", "External party was not found.", 404);
        }

        entity.DisplayName = NormalizeDisplayName(request.DisplayName);
        entity.LegalName = NormalizeLegalName(request.LegalName);
        entity.TaxIdentifier = NormalizeTaxIdentifier(request.TaxIdentifier);
        entity.Notes = NormalizeNotes(request.Notes);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "external_party.update",
            tenantId,
            actorUserId,
            "external_party",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await EnqueuePartyEventAsync(
            tenantId,
            entity,
            ResolvePartyUpdatedEventKind(entity.PartyType),
            $"Party updated: {entity.DisplayName}",
            cancellationToken);

        return Map(await LoadPartyAsync(tenantId, partyId, cancellationToken));
    }

    public async Task<ExternalPartyResponse> UpdateApprovalStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partyId,
        UpdateExternalPartyApprovalStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var approvalStatus = NormalizeApprovalStatus(request.ApprovalStatus);
        var entity = await db.ExternalParties.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == partyId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("parties.not_found", "External party was not found.", 404);
        }

        entity.ApprovalStatus = approvalStatus;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "external_party.approval_status_update",
            tenantId,
            actorUserId,
            "external_party",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await EnqueuePartyEventAsync(
            tenantId,
            entity,
            ResolvePartyApprovalEventKind(entity.PartyType, entity.ApprovalStatus),
            $"Party approval status changed to {entity.ApprovalStatus}: {entity.DisplayName}",
            cancellationToken);

        return Map(await LoadPartyAsync(tenantId, partyId, cancellationToken));
    }

    public async Task<ExternalPartyResponse> UpdateStatusAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partyId,
        UpdateExternalPartyStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = NormalizeStatus(request.Status);
        var entity = await db.ExternalParties.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == partyId,
            cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("parties.not_found", "External party was not found.", 404);
        }

        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "external_party.status_update",
            tenantId,
            actorUserId,
            "external_party",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(await LoadPartyAsync(tenantId, partyId, cancellationToken));
    }

    public async Task<PartyContactResponse> AddContactAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid partyId,
        CreatePartyContactRequest request,
        CancellationToken cancellationToken = default)
    {
        var partyExists = await db.ExternalParties.AnyAsync(
            x => x.TenantId == tenantId && x.Id == partyId,
            cancellationToken);
        if (!partyExists)
        {
            throw new StlApiException("parties.not_found", "External party was not found.", 404);
        }

        var contactName = NormalizeContactName(request.ContactName);
        var now = DateTimeOffset.UtcNow;

        if (request.IsPrimary)
        {
            var existingPrimary = await db.PartyContacts
                .Where(x => x.TenantId == tenantId && x.ExternalPartyId == partyId && x.IsPrimary)
                .ToListAsync(cancellationToken);
            foreach (var contact in existingPrimary)
            {
                contact.IsPrimary = false;
                contact.UpdatedAt = now;
            }
        }

        var entity = new PartyContact
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalPartyId = partyId,
            ContactName = contactName,
            Email = NormalizeEmail(request.Email),
            Phone = NormalizePhone(request.Phone),
            RoleLabel = NormalizeRoleLabel(request.RoleLabel),
            IsPrimary = request.IsPrimary,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.PartyContacts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "party_contact.create",
            tenantId,
            actorUserId,
            "party_contact",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapContact(entity);
    }

    private async Task<ExternalPartyResponse> CreateInternalAsync(
        Guid tenantId,
        Guid actorUserId,
        string partyType,
        string partyKey,
        string displayName,
        string legalName,
        string? taxIdentifier,
        string notes,
        CancellationToken cancellationToken)
    {
        var normalizedKey = NormalizePartyKey(partyKey);
        var exists = await db.ExternalParties.AnyAsync(
            x => x.TenantId == tenantId && x.PartyKey == normalizedKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException(
                "parties.duplicate",
                "An external party with this key already exists.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new ExternalParty
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartyKey = normalizedKey,
            PartyType = partyType,
            DisplayName = NormalizeDisplayName(displayName),
            LegalName = NormalizeLegalName(legalName),
            TaxIdentifier = NormalizeTaxIdentifier(taxIdentifier),
            ApprovalStatus = "pending",
            Status = "active",
            Notes = NormalizeNotes(notes),
            CreatedAt = now,
            UpdatedAt = now
        };

        db.ExternalParties.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "external_party.create",
            tenantId,
            actorUserId,
            "external_party",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.PartyCreated,
            "external_party",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, $"Party created: {entity.DisplayName}"),
            cancellationToken: cancellationToken);

        await EnqueuePartyEventAsync(
            tenantId,
            entity,
            ResolvePartyCreatedEventKind(entity.PartyType),
            $"Party created: {entity.DisplayName}",
            cancellationToken);

        return Map(entity);
    }

    private async Task EnqueuePartyEventAsync(
        Guid tenantId,
        ExternalParty entity,
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
            "external_party",
            entity.Id,
            new IntegrationOutboxPayload(tenantId, summary, entity.Id),
            cancellationToken: cancellationToken);
    }

    private static string? ResolvePartyCreatedEventKind(string partyType) =>
        partyType switch
        {
            "vendor" or "supplier" or "dealer" or "carrier" => IntegrationOutboxEventKinds.SupplyArrVendorCreated,
            "customer" => IntegrationOutboxEventKinds.SupplyArrCustomerCreated,
            _ => null,
        };

    private static string? ResolvePartyUpdatedEventKind(string partyType) =>
        partyType switch
        {
            "vendor" or "supplier" or "dealer" or "carrier" => IntegrationOutboxEventKinds.SupplyArrVendorUpdated,
            _ => null,
        };

    private static string? ResolvePartyApprovalEventKind(string partyType, string approvalStatus)
    {
        if (partyType is not ("vendor" or "supplier" or "dealer" or "carrier"))
        {
            return null;
        }

        return approvalStatus switch
        {
            "approved" => IntegrationOutboxEventKinds.SupplyArrVendorApproved,
            "restricted" or "inactive" => IntegrationOutboxEventKinds.SupplyArrVendorBlocked,
            _ => null,
        };
    }

    private async Task<ExternalParty> LoadPartyAsync(
        Guid tenantId,
        Guid partyId,
        CancellationToken cancellationToken)
    {
        var entity = await db.ExternalParties
            .AsNoTracking()
            .Include(x => x.Contacts.OrderBy(c => c.ContactName))
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == partyId, cancellationToken);
        if (entity is null)
        {
            throw new StlApiException("parties.not_found", "External party was not found.", 404);
        }

        return entity;
    }

    private static ExternalPartyResponse Map(ExternalParty entity) =>
        new(
            entity.Id,
            entity.PartyKey,
            entity.PartyType,
            entity.DisplayName,
            entity.LegalName,
            entity.TaxIdentifier,
            entity.ApprovalStatus,
            entity.Status,
            entity.Notes,
            entity.Contacts
                .OrderBy(x => x.ContactName)
                .Select(MapContact)
                .ToList(),
            entity.CreatedAt,
            entity.UpdatedAt);

    private static PartyContactResponse MapContact(PartyContact entity) =>
        new(
            entity.Id,
            entity.ContactName,
            entity.Email,
            entity.Phone,
            entity.RoleLabel,
            entity.IsPrimary,
            entity.CreatedAt);

    private static string? NormalizePartyTypeOrNull(string? partyType)
    {
        if (string.IsNullOrWhiteSpace(partyType))
        {
            return null;
        }

        return NormalizePartyType(partyType);
    }

    private static string NormalizePartyType(string partyType)
    {
        var normalized = partyType.Trim().ToLowerInvariant();
        if (!AllowedPartyTypes.Contains(normalized))
        {
            throw new StlApiException(
                "parties.validation",
                "Party type must be vendor, dealer, supplier, carrier, or customer.",
                400);
        }

        return normalized;
    }

    private static string NormalizePartyKey(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length < 2 || normalized.Length > 128)
        {
            throw new StlApiException(
                "parties.validation",
                "Party key must be between 2 and 128 characters.",
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
                "parties.validation",
                "Display name must be between 2 and 256 characters.",
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
                "parties.validation",
                "Legal name must be 256 characters or fewer.",
                400);
        }

        return trimmed;
    }

    private static string? NormalizeTaxIdentifier(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeNotes(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new StlApiException(
                "parties.validation",
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
                "parties.validation",
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
                "parties.validation",
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

    private static IReadOnlyList<PartyRegistryCatalogOptionResponse> Options(
        params (string Value, string Label)[] options) =>
        options
            .Select(option => new PartyRegistryCatalogOptionResponse(option.Value, option.Label))
            .ToList();
}
