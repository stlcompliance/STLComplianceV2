using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;
using System.Text.Json;

namespace SupplyArr.Api.Services;

public sealed class SupplyContractService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    private const int DefaultListLimit = 100;
    private const int MaxListLimit = 500;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<SupplyContractResponse>> ListAsync(
        Guid tenantId,
        Guid? supplierId,
        string? status,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = string.IsNullOrWhiteSpace(status)
            ? null
            : NormalizeStatus(status, "contracts.invalid_status");
        var take = NormalizeLimit(limit);

        var query = QueryContracts(tenantId);
        if (supplierId.HasValue)
        {
            query = query.Where(x => x.SupplierId == supplierId.Value);
        }

        if (normalizedStatus is not null)
        {
            query = query.Where(x => x.Status == normalizedStatus);
        }

        return await query
            .OrderBy(x => x.ExpiresAt ?? DateTimeOffset.MaxValue)
            .ThenBy(x => x.ContractKey)
            .Take(take)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<SupplyContractResponse> GetAsync(
        Guid tenantId,
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        var response = await QueryContracts(tenantId)
            .Where(x => x.Id == contractId)
            .Select(x => Map(x))
            .FirstOrDefaultAsync(cancellationToken);
        return response ?? throw new StlApiException("contracts.not_found", "Contract was not found.", 404);
    }

    public async Task<SupplyContractResponse> CreateAsync(
        Guid tenantId,
        Guid actorUserId,
        CreateSupplyContractRequest request,
        CancellationToken cancellationToken = default)
    {
        var contractKey = NormalizeKey(request.ContractKey);
        var exists = await db.SupplyContracts.AnyAsync(
            x => x.TenantId == tenantId && x.ContractKey == contractKey,
            cancellationToken);
        if (exists)
        {
            throw new StlApiException("contracts.duplicate_key", "A contract with this key already exists.", 409);
        }

        var selectedSupplierId = request.SupplierId
            ?? throw new StlApiException("contracts.supplier_required", "Supplier is required.", 400);

        var supplier = await db.Suppliers.AsNoTracking()
            .Include(x => x.ParentSupplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.Id == selectedSupplierId
                   ,
                cancellationToken)
            ?? throw new StlApiException("contracts.supplier_not_found", "Supplier was not found.", 404);

        var effectiveAt = request.EffectiveAt ?? DateTimeOffset.UtcNow;
        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= effectiveAt)
        {
            throw new StlApiException("contracts.invalid_expiration", "Expiration must be after the effective date.", 400);
        }

        if (request.RenewalAt.HasValue && request.ExpiresAt.HasValue && request.RenewalAt.Value > request.ExpiresAt.Value)
        {
            throw new StlApiException("contracts.invalid_renewal", "Renewal date cannot be after expiration.", 400);
        }

        var status = NormalizeStatus(request.Status, "contracts.invalid_status");
        var approvalStatus = NormalizeApprovalStatus(request.ApprovalStatus);
        var now = DateTimeOffset.UtcNow;
        var entity = new SupplyContract
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContractKey = contractKey,
            ContractType = NormalizeRequired(request.ContractType, "contracts.invalid_type", 64),
            Title = NormalizeRequired(request.Title, "contracts.invalid_title", 256),
            SupplierId = selectedSupplierId,
            EffectiveAt = effectiveAt,
            ExpiresAt = request.ExpiresAt,
            RenewalAt = request.RenewalAt,
            PaymentTerms = NormalizeOptional(request.PaymentTerms, 256),
            FreightTerms = NormalizeOptional(request.FreightTerms, 256),
            WarrantyTerms = NormalizeOptional(request.WarrantyTerms, 512),
            MinimumSpend = NormalizeMoney(request.MinimumSpend),
            ServiceLevelAgreement = NormalizeOptional(request.ServiceLevelAgreement, 1024),
            ApprovalStatus = approvalStatus,
            Status = status,
            Notes = NormalizeOptional(request.Notes, 1024),
            CreatedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.SupplyContracts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "contract.create",
            tenantId,
            actorUserId,
            "contract",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return await GetAsync(tenantId, entity.Id, cancellationToken);
    }

    private IQueryable<SupplyContract> QueryContracts(Guid tenantId) =>
        db.SupplyContracts.AsNoTracking()
            .Include(x => x.Supplier).ThenInclude(x => x!.ParentSupplier)
            .Where(x => x.TenantId == tenantId);

    private static SupplyContractResponse Map(SupplyContract entity) =>
        new(
            entity.Id,
            entity.ContractKey,
            entity.ContractType,
            entity.Title,
            entity.SupplierId,
            entity.Supplier.SupplierKey,
            entity.Supplier.DisplayName,
            entity.Supplier.ParentSupplierId,
            entity.Supplier.ParentSupplier?.DisplayName,
            entity.Supplier.UnitKind,
            ParseServiceTypes(entity.Supplier.ServiceTypesJson),
            entity.EffectiveAt,
            entity.ExpiresAt,
            entity.RenewalAt,
            entity.PaymentTerms,
            entity.FreightTerms,
            entity.WarrantyTerms,
            entity.MinimumSpend,
            entity.ServiceLevelAgreement,
            entity.ApprovalStatus,
            entity.Status,
            entity.Notes,
            entity.CreatedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt);

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

    private static string NormalizeKey(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length is < 3 or > 128)
        {
            throw new StlApiException("contracts.invalid_key", "Contract key must be between 3 and 128 characters.", 400);
        }

        return normalized;
    }

    private static string NormalizeStatus(string value, string errorCode)
    {
        var normalized = value.Trim().ToLowerInvariant().Replace('-', '_').Replace(' ', '_');
        if (!SupplyContractStatuses.All.Contains(normalized))
        {
            throw new StlApiException(errorCode, "Contract status is not supported.", 400);
        }

        return normalized;
    }

    private static string NormalizeApprovalStatus(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Replace('-', '_').Replace(' ', '_');
        if (!SupplyContractApprovalStatuses.All.Contains(normalized))
        {
            throw new StlApiException("contracts.invalid_approval_status", "Contract approval status is not supported.", 400);
        }

        return normalized;
    }

    private static string NormalizeRequired(string value, string errorCode, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length < 2 || normalized.Length > maxLength)
        {
            throw new StlApiException(errorCode, $"Value must be between 2 and {maxLength} characters.", 400);
        }

        return normalized;
    }

    private static string NormalizeOptional(string value, int maxLength)
    {
        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static decimal? NormalizeMoney(decimal? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value.Value < 0)
        {
            throw new StlApiException("contracts.invalid_minimum_spend", "Minimum spend cannot be negative.", 400);
        }

        return decimal.Round(value.Value, 2);
    }

    private static int NormalizeLimit(int? limit)
    {
        if (limit is null or < 1)
        {
            return DefaultListLimit;
        }

        return Math.Min(limit.Value, MaxListLimit);
    }
}


