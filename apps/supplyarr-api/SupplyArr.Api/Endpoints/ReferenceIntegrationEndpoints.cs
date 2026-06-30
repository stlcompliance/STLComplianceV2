using System.Globalization;
using System.Text;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace SupplyArr.Api.Endpoints;

public static class ReferenceIntegrationEndpoints
{
    private const string ProductKey = "supplyarr";
    private const string SupplierReferenceType = "supplier";

    public static void MapSupplyArrReferenceIntegrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/integrations")
            .WithTags("Integrations")
            .RequireAuthorization();

        group.MapGet("/reference-types", (
            HttpContext context,
            SupplyArrAuthorizationService authorization) =>
        {
            RequireSupplyArrTenantRole(context.User);
            authorization.RequirePartiesRead(context.User);
            return Results.Ok(new ReferenceTypeDescriptor[]
            {
                SupplierDescriptor(),
                new(
                    ProductKey,
                    "part",
                    "Part",
                    CanQuickCreate: true,
                    QuickCreatePermission: "supplyarr.parts.quick_create",
                    Description: "SupplyArr-owned part or item reference.")
            });
        })
            .WithName("ListSupplyArrReferenceTypes");

        group.MapPost("/references/search", async (
            ReferenceSearchRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService parties,
            PartRegistryService parts,
            CancellationToken cancellationToken) =>
        {
            RequireSupplyArrTenantRole(context.User);
            var referenceType = NormalizeReferenceType(request.ReferenceType);
            var limit = Math.Clamp(request.Limit <= 0 ? 25 : request.Limit, 1, 50);
            var tenantId = context.User.GetTenantId();

            if (IsSupplierReferenceType(referenceType))
            {
                authorization.RequirePartiesRead(context.User);
                var results = (await parties.ListSuppliersAsync(tenantId, cancellationToken))
                    .Where(supplier => MatchesSupplier(supplier, request.Query))
                    .Take(limit)
                    .Select(ToSupplierSummary)
                    .ToArray();
                return Results.Ok(new ReferenceSearchResponse(results));
            }

            if (referenceType == "part")
            {
                authorization.RequirePartsRead(context.User);
                var results = (await parts.ListAsync(tenantId, cancellationToken: cancellationToken))
                    .Where(part => MatchesPart(part, request.Query))
                    .Take(limit)
                    .Select(ToPartSummary)
                    .ToArray();
                return Results.Ok(new ReferenceSearchResponse(results));
            }

            throw UnsupportedReferenceType(referenceType);
        })
        .WithName("SearchSupplyArrReferences");

        group.MapGet("/references/{referenceType}/{id}/summary", async (
            string referenceType,
            string id,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService parties,
            PartRegistryService parts,
            CancellationToken cancellationToken) =>
        {
            RequireSupplyArrTenantRole(context.User);
            var normalizedType = NormalizeReferenceType(referenceType);
            if (!Guid.TryParse(id, out var parsedId))
            {
                throw new StlApiException("supplyarr.references.invalid_id", "SupplyArr reference id must be a GUID.", 400);
            }

            var tenantId = context.User.GetTenantId();
            if (IsSupplierReferenceType(normalizedType))
            {
                authorization.RequirePartiesRead(context.User);
                return Results.Ok(ToSupplierSummary(await parties.GetSupplierAsync(tenantId, parsedId, cancellationToken)));
            }

            if (normalizedType == "part")
            {
                authorization.RequirePartsRead(context.User);
                return Results.Ok(ToPartSummary(await parts.GetAsync(tenantId, parsedId, cancellationToken)));
            }

            throw UnsupportedReferenceType(normalizedType);
        })
        .WithName("GetSupplyArrReferenceSummary");

        group.MapGet("/references/{referenceType}/quick-create-schema", (
            string referenceType,
            HttpContext context,
            SupplyArrAuthorizationService authorization) =>
        {
            RequireSupplyArrTenantRole(context.User);
            var normalizedType = NormalizeReferenceType(referenceType);
            if (IsSupplierReferenceType(normalizedType))
            {
                authorization.RequirePartiesRead(context.User);
                var allowed = CanQuickCreateParty(context.User);
                return Results.Ok(new QuickCreateSchemaResponse(
                    ProductKey,
                    SupplierReferenceType,
                    allowed,
                    "SupplyArr",
                    "supplyarr.parties.quick_create",
                    allowed ? null : "Supplier quick create requires SupplyArr supplier management access.",
                    [
                        new QuickCreateFieldDescriptor("supplierKey", "Supplier key", "text", Placeholder: "acme-midwest"),
                        new QuickCreateFieldDescriptor("parentSupplierId", "Parent supplier identity", "text"),
                        new QuickCreateFieldDescriptor("unitKind", "Hierarchy role", "text", DefaultValue: "identity"),
                        new QuickCreateFieldDescriptor("displayName", "Display name", "text", Required: true),
                        new QuickCreateFieldDescriptor("legalName", "Legal name", "text"),
                        new QuickCreateFieldDescriptor("serviceTypes", "Service coverage", "text", Placeholder: "products,parts"),
                        new QuickCreateFieldDescriptor("taxIdentifier", "Tax identifier", "text"),
                        new QuickCreateFieldDescriptor("notes", "Notes", "textarea")
                    ]));
            }

            if (normalizedType == "part")
            {
                authorization.RequirePartsRead(context.User);
                var allowed = CanQuickCreatePart(context.User);
                return Results.Ok(new QuickCreateSchemaResponse(
                    ProductKey,
                    "part",
                    allowed,
                    "SupplyArr",
                    "supplyarr.parts.quick_create",
                    allowed ? null : "Part quick create requires SupplyArr part management access.",
                    [
                        new QuickCreateFieldDescriptor("partKey", "Part key", "text"),
                        new QuickCreateFieldDescriptor("displayName", "Display name", "text", Required: true),
                        new QuickCreateFieldDescriptor("description", "Description", "textarea"),
                        new QuickCreateFieldDescriptor("categoryKey", "Category", "text", DefaultValue: "quick_create"),
                        new QuickCreateFieldDescriptor("unitOfMeasure", "Unit", "text", DefaultValue: "each"),
                        new QuickCreateFieldDescriptor("manufacturerName", "Manufacturer", "text"),
                        new QuickCreateFieldDescriptor("manufacturerPartNumber", "Manufacturer part number", "text")
                    ]));
            }

            throw UnsupportedReferenceType(normalizedType);
        })
        .WithName("GetSupplyArrQuickCreateSchema");

        group.MapPost("/references/{referenceType}/quick-create", async (
            string referenceType,
            QuickCreateRequest request,
            HttpContext context,
            SupplyArrAuthorizationService authorization,
            SupplierDirectoryService parties,
            PartRegistryService parts,
            CancellationToken cancellationToken) =>
        {
            RequireSupplyArrTenantRole(context.User);
            var normalizedType = NormalizeReferenceType(referenceType);
            if (!string.Equals(normalizedType, NormalizeReferenceType(request.ReferenceType), StringComparison.Ordinal))
            {
                throw new StlApiException("supplyarr.references.type_mismatch", "Reference type path and request must match.", 400);
            }

            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            if (IsSupplierReferenceType(normalizedType))
            {
                authorization.RequirePartiesManage(context.User);
                var duplicates = (await FindPartyDuplicates(
                    parties,
                    tenantId,
                    request,
                    cancellationToken)).ToArray();
                if (duplicates.Length > 0)
                {
                    return Results.Conflict(DuplicateResponse(duplicates));
                }

                var displayName = FirstValue(request.Values, "displayName", "legalName", "name");
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    throw new StlApiException("supplyarr.references.display_name_required", "Display name is required.", 400);
                }

                var supplierKey = FirstValue(request.Values, "supplierKey", "partyKey", "key");
                var parentSupplierIdText = FirstValue(request.Values, "parentSupplierId");
                Guid? parentSupplierId = null;
                if (!string.IsNullOrWhiteSpace(parentSupplierIdText))
                {
                    if (!Guid.TryParse(parentSupplierIdText, out var parsedParentSupplierId))
                    {
                        throw new StlApiException("supplyarr.references.parent_supplier_invalid", "Parent supplier identity must be a GUID.", 400);
                    }

                    parentSupplierId = parsedParentSupplierId;
                }

                var created = await parties.CreateSupplierAsync(
                    tenantId,
                    actorUserId,
                    new CreateSupplierRequest(
                        string.IsNullOrWhiteSpace(supplierKey) ? Slugify(displayName) : supplierKey,
                        parentSupplierId,
                        NullIfWhiteSpace(GetValue(request.Values, "unitKind")),
                        displayName,
                        FirstValue(request.Values, "legalName", "displayName", "name"),
                        NullIfWhiteSpace(GetValue(request.Values, "taxIdentifier", "taxId")),
                        FirstValue(request.Values, "notes"),
                        ParseDelimitedValues(GetValue(request.Values, "serviceTypes", "services", "coverage")),
                        null,
                        null,
                        null,
                        null,
                        null,
                        null),
                    cancellationToken);

                return Results.Created(
                    $"/api/v1/integrations/references/{SupplierReferenceType}/{created.SupplierId}/summary",
                    CreatedResponse(ToSupplierSummary(created)));
            }

            if (normalizedType == "part")
            {
                authorization.RequirePartsManage(context.User);
                var duplicates = (await FindPartDuplicates(parts, tenantId, request, cancellationToken)).ToArray();
                if (duplicates.Length > 0)
                {
                    return Results.Conflict(DuplicateResponse(duplicates));
                }

                var displayName = FirstValue(request.Values, "displayName", "name");
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    throw new StlApiException("supplyarr.references.part_name_required", "Part display name is required.", 400);
                }

                var partKey = FirstValue(request.Values, "partKey", "key");
                var created = await parts.CreateAsync(
                    tenantId,
                    actorUserId,
                    new CreatePartRequest(
                        string.IsNullOrWhiteSpace(partKey) ? Slugify(displayName) : partKey,
                        null,
                        displayName,
                        FirstValue(request.Values, "description"),
                        FirstValue(request.Values, "categoryKey") is { Length: > 0 } category ? category : "quick_create",
                        FirstValue(request.Values, "unitOfMeasure", "uom") is { Length: > 0 } uom ? uom : "each",
                        FirstValue(request.Values, "manufacturerName", "manufacturer"),
                        FirstValue(request.Values, "manufacturerPartNumber", "mpn")),
                    cancellationToken);

                return Results.Created(
                    $"/api/v1/integrations/references/part/{created.PartId}/summary",
                    CreatedResponse(ToPartSummary(created)));
            }

            throw UnsupportedReferenceType(normalizedType);
        })
        .WithName("QuickCreateSupplyArrReference");
    }

    private static ReferenceTypeDescriptor SupplierDescriptor() =>
        new(
            ProductKey,
            SupplierReferenceType,
            "Supplier",
            CanQuickCreate: true,
            QuickCreatePermission: "supplyarr.parties.quick_create",
            Description: "SupplyArr-owned supplier identity or supplier sub-unit reference.");

    private static bool CanQuickCreateParty(System.Security.Claims.ClaimsPrincipal principal)
    {
        var role = principal.GetTenantRoleKey();
        return role.Equals("tenant_admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("supplyarr_admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("supplyarr_manager", StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanQuickCreatePart(System.Security.Claims.ClaimsPrincipal principal) => CanQuickCreateParty(principal);

    private static void RequireSupplyArrTenantRole(System.Security.Claims.ClaimsPrincipal principal)
    {
        if (RoleMatches(
                principal.GetTenantRoleKey(),
                "tenant_admin",
                "supplyarr_admin",
                "supplyarr_manager",
                "supplyarr_clerk",
                "tenant_member"))
        {
            return;
        }

        throw new StlApiException(
            "auth.forbidden",
            "SupplyArr reference integration requires SupplyArr tenant access.",
            403);
    }

    private static bool RoleMatches(string roleKey, params string[] allowedRoles) =>
        allowedRoles.Any(role => string.Equals(roleKey, role, StringComparison.OrdinalIgnoreCase));

    private static ReferenceSummaryResponse ToSupplierSummary(SupplierResponse supplier) =>
        new(
            ProductKey,
            SupplierReferenceType,
            supplier.SupplierId.ToString("D"),
            supplier.DisplayName,
            string.Join(" / ", new[] { supplier.SupplierKey, supplier.LegalName }.Where(value => !string.IsNullOrWhiteSpace(value))),
            supplier.ApprovalStatus,
            supplier.UpdatedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture),
            $"/suppliers/{supplier.SupplierId:D}",
              new Dictionary<string, string>
              {
                  ["supplierKey"] = supplier.SupplierKey,
                  ["unitKind"] = supplier.UnitKind,
                  ["serviceTypes"] = string.Join(",", supplier.ServiceTypes),
                  ["status"] = supplier.Status,
                  ["taxIdentifier"] = supplier.TaxIdentifier ?? string.Empty,
                  ["parentSupplierId"] = supplier.ParentSupplierId?.ToString("D") ?? string.Empty,
                  ["parentSupplierDisplayName"] = supplier.ParentSupplierDisplayName ?? string.Empty
              });

    private static IReadOnlyList<string>? ParseDelimitedValues(string value)
    {
        var parsed = value
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return parsed.Length == 0 ? null : parsed;
    }

    private static ReferenceSummaryResponse ToPartSummary(PartResponse part) =>
        new(
            ProductKey,
            "part",
            part.PartId.ToString("D"),
            part.DisplayName,
            string.Join(" / ", new[] { part.PartKey, part.ManufacturerPartNumber }.Where(value => !string.IsNullOrWhiteSpace(value))),
            part.Status,
            part.UpdatedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture),
            $"/parts/{part.PartId:D}",
            new Dictionary<string, string>
            {
                ["partKey"] = part.PartKey,
                ["manufacturerName"] = part.ManufacturerName,
                ["manufacturerPartNumber"] = part.ManufacturerPartNumber,
                ["unitOfMeasure"] = part.UnitOfMeasure
            });

    private static async Task<IEnumerable<DuplicateCandidateResponse>> FindPartyDuplicates(
        SupplierDirectoryService parties,
        Guid tenantId,
        QuickCreateRequest request,
        CancellationToken cancellationToken)
    {
        var displayName = Normalize(FirstValue(request.Values, "displayName", "legalName", "name"));
        var legalName = Normalize(FirstValue(request.Values, "legalName", "displayName", "name"));
        var taxId = Normalize(GetValue(request.Values, "taxIdentifier", "taxId"));
        var partyKey = Normalize(GetValue(request.Values, "supplierKey", "partyKey", "key"));

        return (await parties.ListSuppliersAsync(tenantId, cancellationToken))
            .Select(supplier =>
            {
                var reasons = new List<string>();
                if (!string.IsNullOrWhiteSpace(displayName) && Normalize(supplier.DisplayName) == displayName)
                {
                    reasons.Add("matching display name");
                }

                if (!string.IsNullOrWhiteSpace(legalName) && Normalize(supplier.LegalName) == legalName)
                {
                    reasons.Add("matching legal name");
                }

                if (!string.IsNullOrWhiteSpace(taxId) && Normalize(supplier.TaxIdentifier ?? string.Empty) == taxId)
                {
                    reasons.Add("matching tax identifier");
                }

                if (!string.IsNullOrWhiteSpace(partyKey) && Normalize(supplier.SupplierKey) == partyKey)
                {
                    reasons.Add("matching supplier key");
                }

                return (supplier, reasons);
            })
            .Where(match => match.reasons.Count > 0)
            .Take(10)
            .Select(match => new DuplicateCandidateResponse(
                match.supplier.SupplierId.ToString("D"),
                match.supplier.DisplayName,
                match.supplier.SupplierKey,
                match.supplier.ApprovalStatus,
                string.Join(", ", match.reasons),
                match.reasons.Count > 1 ? 0.95m : 0.8m));
    }

    private static async Task<IEnumerable<DuplicateCandidateResponse>> FindPartDuplicates(
        PartRegistryService parts,
        Guid tenantId,
        QuickCreateRequest request,
        CancellationToken cancellationToken)
    {
        var partKey = Normalize(GetValue(request.Values, "partKey", "key"));
        var manufacturer = Normalize(GetValue(request.Values, "manufacturerName", "manufacturer"));
        var mpn = Normalize(GetValue(request.Values, "manufacturerPartNumber", "mpn"));

        return (await parts.ListAsync(tenantId, cancellationToken: cancellationToken))
            .Select(part =>
            {
                var reasons = new List<string>();
                if (!string.IsNullOrWhiteSpace(partKey) && Normalize(part.PartKey) == partKey)
                {
                    reasons.Add("matching part key");
                }

                if (!string.IsNullOrWhiteSpace(manufacturer)
                    && !string.IsNullOrWhiteSpace(mpn)
                    && Normalize(part.ManufacturerName) == manufacturer
                    && Normalize(part.ManufacturerPartNumber) == mpn)
                {
                    reasons.Add("matching manufacturer part number");
                }

                return (part, reasons);
            })
            .Where(match => match.reasons.Count > 0)
            .Take(10)
            .Select(match => new DuplicateCandidateResponse(
                match.part.PartId.ToString("D"),
                match.part.DisplayName,
                match.part.PartKey,
                match.part.Status,
                string.Join(", ", match.reasons),
                match.reasons.Count > 1 ? 0.95m : 0.8m));
    }

    private static QuickCreateResponse DuplicateResponse(IReadOnlyList<DuplicateCandidateResponse> duplicates) =>
        new(
            null,
            duplicates,
            Created: false,
            ReviewStatus: "duplicate_candidates",
            Message: "SupplyArr found possible duplicates. Select an existing record or review in SupplyArr.");

    private static QuickCreateResponse CreatedResponse(ReferenceSummaryResponse summary) =>
        new(
            summary.ToCrossProductReference("quick_create"),
            [],
            Created: true,
            ReviewStatus: "needs_review",
            Message: "Reference was created in SupplyArr and marked for owner review.");

    private static bool MatchesSupplier(SupplierResponse supplier, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        var needle = Normalize(query);
        return Normalize(supplier.DisplayName).Contains(needle)
            || Normalize(supplier.LegalName).Contains(needle)
            || Normalize(supplier.SupplierKey).Contains(needle)
            || Normalize(supplier.TaxIdentifier ?? string.Empty).Contains(needle)
            || supplier.Contacts.Any(contact => Normalize(contact.Email).Contains(needle) || Normalize(contact.Phone).Contains(needle));
    }

    private static bool MatchesPart(PartResponse part, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        var needle = Normalize(query);
        return Normalize(part.DisplayName).Contains(needle)
            || Normalize(part.Description).Contains(needle)
            || Normalize(part.PartKey).Contains(needle)
            || Normalize(part.ManufacturerName).Contains(needle)
            || Normalize(part.ManufacturerPartNumber).Contains(needle);
    }

    private static string NormalizeReferenceType(string referenceType)
    {
        var normalized = referenceType.Trim().Replace('-', '_').ToLowerInvariant();
        return normalized switch
        {
            "item" or "material" => "part",
            "party" or "vendor" or "dealer" or "carrier" => SupplierReferenceType,
            _ => normalized
        };
    }

    private static bool IsSupplierReferenceType(string referenceType) =>
        string.Equals(referenceType, SupplierReferenceType, StringComparison.OrdinalIgnoreCase);

    private static StlApiException UnsupportedReferenceType(string referenceType) =>
        new(
            "supplyarr.references.unsupported_type",
            $"SupplyArr does not own reference type '{referenceType}'.",
            404);

    private static string FirstValue(IReadOnlyDictionary<string, string> values, params string[] keys) =>
        keys.Select(key => GetValue(values, key)).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static string GetValue(IReadOnlyDictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            var match = values.FirstOrDefault(entry => string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Value))
            {
                return match.Value.Trim();
            }
        }

        return string.Empty;
    }

    private static string? NullIfWhiteSpace(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static string Slugify(string value)
    {
        var builder = new StringBuilder();
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(ch);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString().Trim('-');
        return slug.Length >= 2 ? slug : $"ref-{Guid.NewGuid():N}"[..10];
    }
}
