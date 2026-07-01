using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public sealed class SupplierProcurementGuardService(SupplyArrDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> PartsProcurementServiceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "products",
        "parts",
    };

    private static readonly HashSet<string> BlockedApprovalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "restricted",
        "inactive",
    };

    public async Task EnsureSupplierAllowedForScopeAsync(
        Guid tenantId,
        Guid supplierId,
        string scope,
        CancellationToken cancellationToken = default)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .Include(x => x.ParentSupplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == supplierId,
                cancellationToken);

        if (supplier is null
           
            || !string.Equals(supplier.Status, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "supplier_restrictions.supplier_not_found",
                "Active supplier identity or sub-unit was not found.",
                404);
        }

        if (BlockedApprovalStatuses.Contains(supplier.ApprovalStatus))
        {
            throw new StlApiException(
                "supplier_restrictions.approval_blocked",
                $"Supplier approval status '{supplier.ApprovalStatus}' blocks procurement activity.",
                409);
        }

        var serviceTypes = DeserializeServiceTypes(supplier.ServiceTypesJson);
        if (RequiresPartsProcurementCoverage(scope) && BlocksPartsProcurement(serviceTypes))
        {
            throw new StlApiException(
                "supplier_restrictions.service_coverage_blocked",
                $"Supplier service coverage must include products or parts for parts procurement. Current coverage: {string.Join(", ", serviceTypes)}.",
                409);
        }

        if (string.Equals(scope, SupplierRestrictionScopes.PurchaseOrders, StringComparison.OrdinalIgnoreCase))
        {
            await EnsureNoExpiredRequiredSupplierDocumentsAsync(tenantId, supplierId, cancellationToken);
        }

        var asOfUtc = DateTimeOffset.UtcNow;
        var restrictions = await db.SupplierRestrictions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.SupplierId == supplierId
                && x.Status == SupplierRestrictionStatuses.Active)
            .ToListAsync(cancellationToken);

        foreach (var restriction in restrictions)
        {
            if (!SupplierRestrictionRules.IsRestrictionEffective(restriction, asOfUtc))
            {
                continue;
            }

            var scopes = DeserializeScopes(restriction.ScopesJson);
            if (SupplierRestrictionRules.ScopeBlocks(scopes, scope))
            {
                throw new StlApiException(
                    "supplier_restrictions.scope_blocked",
                    $"Supplier is restricted for scope '{scope}': {restriction.Reason}",
                    409);
            }
        }
    }

    public async Task<SupplierRestrictionEnforcementResponse> GetEnforcementAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .Include(x => x.ParentSupplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Id == supplierId,
                cancellationToken)
            ?? throw new StlApiException(
                "supplier_restrictions.supplier_not_found",
                "Supplier was not found.",
                404);

        var asOfUtc = DateTimeOffset.UtcNow;
        var activeScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (BlockedApprovalStatuses.Contains(supplier.ApprovalStatus))
        {
            activeScopes.Add(SupplierRestrictionScopes.AllProcurement);
            var serviceTypes = DeserializeServiceTypes(supplier.ServiceTypesJson);
            return new SupplierRestrictionEnforcementResponse(
                supplierId,
                supplier.SupplierKey,
                supplier.DisplayName,
                supplier.ParentSupplierId,
                supplier.ParentSupplier?.DisplayName,
                supplier.UnitKind,
                serviceTypes,
                IsBlocked: true,
                BlockReason: $"Supplier approval status is {supplier.ApprovalStatus}.",
                activeScopes.OrderBy(x => x).ToList());
        }

        var restrictions = await db.SupplierRestrictions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.SupplierId == supplierId
                && x.Status == SupplierRestrictionStatuses.Active)
            .ToListAsync(cancellationToken);

        string? blockReason = null;
        foreach (var restriction in restrictions)
        {
            if (!SupplierRestrictionRules.IsRestrictionEffective(restriction, asOfUtc))
            {
                continue;
            }

            foreach (var activeScope in DeserializeScopes(restriction.ScopesJson))
            {
                activeScopes.Add(activeScope);
            }

            blockReason ??= restriction.Reason;
        }

        var activeServiceTypes = DeserializeServiceTypes(supplier.ServiceTypesJson);
        return new SupplierRestrictionEnforcementResponse(
            supplierId,
            supplier.SupplierKey,
            supplier.DisplayName,
            supplier.ParentSupplierId,
            supplier.ParentSupplier?.DisplayName,
            supplier.UnitKind,
            activeServiceTypes,
            IsBlocked: activeScopes.Count > 0,
            blockReason,
            activeScopes.OrderBy(x => x).ToList());
    }

    public static bool BlocksPartsProcurement(IReadOnlyList<string> serviceTypes) =>
        serviceTypes.Count > 0
        && !serviceTypes.Any(serviceType => PartsProcurementServiceTypes.Contains(serviceType));

    private static bool RequiresPartsProcurementCoverage(string scope) =>
        string.Equals(scope, SupplierRestrictionScopes.PurchaseRequests, StringComparison.OrdinalIgnoreCase)
        || string.Equals(scope, SupplierRestrictionScopes.PurchaseOrders, StringComparison.OrdinalIgnoreCase)
        || string.Equals(scope, SupplierRestrictionScopes.RfqInvitations, StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<string> DeserializeScopes(string scopesJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(scopesJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static IReadOnlyList<string> DeserializeServiceTypes(string? serviceTypesJson)
    {
        if (string.IsNullOrWhiteSpace(serviceTypesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(serviceTypesJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private async Task EnsureNoExpiredRequiredSupplierDocumentsAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        var requiredDocumentTypeKeys = await LoadRequiredDocumentTypeKeysAsync(tenantId, cancellationToken);
        if (requiredDocumentTypeKeys.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var documentTypeKey in requiredDocumentTypeKeys)
        {
            var latest = await db.SupplierComplianceDocuments
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId
                    && x.SupplierId == supplierId
                    && x.DocumentTypeKey == documentTypeKey
                    && x.ReviewStatus == SupplierComplianceDocumentReviewStatuses.Approved)
                .OrderByDescending(x => x.Version)
                .ThenByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (latest is null || latest.ExpiresAt is null || latest.ExpiresAt.Value > now)
            {
                continue;
            }

            throw new StlApiException(
                "supplier_restrictions.required_document_expired",
                $"Supplier required document '{documentTypeKey}' is expired and blocks purchase order issuance.",
                409);
        }
    }

    private async Task<IReadOnlyList<string>> LoadRequiredDocumentTypeKeysAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var settings = await db.TenantSupplierOnboardingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null || string.IsNullOrWhiteSpace(settings.RequiredDocumentTypeKeysJson))
        {
            return SupplierOnboardingRules.DefaultRequirements.Select(x => x.DocumentTypeKey).ToList();
        }

        var parsed = JsonSerializer.Deserialize<List<string>>(settings.RequiredDocumentTypeKeysJson, JsonOptions) ?? [];
        var normalized = SupplierOnboardingRules.NormalizeRequiredTypeKeys(parsed);
        return normalized.Count == 0
            ? SupplierOnboardingRules.DefaultRequirements.Select(x => x.DocumentTypeKey).ToList()
            : normalized;
    }
}


