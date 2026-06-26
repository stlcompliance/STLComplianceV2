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
    private static readonly string[] PartyReferenceTypes = ["vendor", "supplier", "carrier"];

    public static void MapSupplyArrReferenceIntegrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/integrations")
            .WithTags("Integrations")
            .RequireAuthorization();

        group.MapGet("/reference-types", (
            HttpContext context,
            SupplyArrAuthorizationService authorization) =>
        {
            authorization.RequirePartiesRead(context.User);
            return Results.Ok(new ReferenceTypeDescriptor[]
            {
                PartyDescriptor("vendor"),
                PartyDescriptor("supplier"),
                PartyDescriptor("carrier"),
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
            ExternalPartyService parties,
            PartRegistryService parts,
            CancellationToken cancellationToken) =>
        {
            var referenceType = NormalizeReferenceType(request.ReferenceType);
            var limit = Math.Clamp(request.Limit <= 0 ? 25 : request.Limit, 1, 50);
            var tenantId = context.User.GetTenantId();

            if (IsPartyType(referenceType))
            {
                authorization.RequirePartiesRead(context.User);
                var results = (await parties.ListAsync(tenantId, referenceType, cancellationToken))
                    .Where(party => MatchesParty(party, request.Query))
                    .Take(limit)
                    .Select(party => ToPartySummary(referenceType, party))
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
            ExternalPartyService parties,
            PartRegistryService parts,
            CancellationToken cancellationToken) =>
        {
            var normalizedType = NormalizeReferenceType(referenceType);
            if (!Guid.TryParse(id, out var parsedId))
            {
                throw new StlApiException("supplyarr.references.invalid_id", "SupplyArr reference id must be a GUID.", 400);
            }

            var tenantId = context.User.GetTenantId();
            if (IsPartyType(normalizedType))
            {
                authorization.RequirePartiesRead(context.User);
                var party = await parties.GetAsync(tenantId, parsedId, cancellationToken);
                return string.Equals(party.PartyType, normalizedType, StringComparison.OrdinalIgnoreCase)
                    ? Results.Ok(ToPartySummary(normalizedType, party))
                    : Results.NotFound();
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
            var normalizedType = NormalizeReferenceType(referenceType);
            if (IsPartyType(normalizedType))
            {
                authorization.RequirePartiesRead(context.User);
                var allowed = CanQuickCreateParty(context.User);
                return Results.Ok(new QuickCreateSchemaResponse(
                    ProductKey,
                    normalizedType,
                    allowed,
                    "SupplyArr",
                    "supplyarr.parties.quick_create",
                    allowed ? null : "Party quick create requires SupplyArr party management access.",
                    [
                        new QuickCreateFieldDescriptor("partyKey", "Party key", "text", Placeholder: $"{normalizedType}-acme"),
                        new QuickCreateFieldDescriptor("displayName", "Display name", "text", Required: true),
                        new QuickCreateFieldDescriptor("legalName", "Legal name", "text"),
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
            ExternalPartyService parties,
            PartRegistryService parts,
            CancellationToken cancellationToken) =>
        {
            var normalizedType = NormalizeReferenceType(referenceType);
            if (!string.Equals(normalizedType, NormalizeReferenceType(request.ReferenceType), StringComparison.Ordinal))
            {
                throw new StlApiException("supplyarr.references.type_mismatch", "Reference type path and request must match.", 400);
            }

            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            if (IsPartyType(normalizedType))
            {
                authorization.RequirePartiesManage(context.User);
                var duplicates = (await FindPartyDuplicates(
                    parties,
                    tenantId,
                    normalizedType,
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

                var partyKey = FirstValue(request.Values, "partyKey", "key");
                var created = await parties.CreateTypedAsync(
                    tenantId,
                    actorUserId,
                    normalizedType,
                    new CreateTypedExternalPartyRequest(
                        string.IsNullOrWhiteSpace(partyKey) ? Slugify(displayName) : partyKey,
                        displayName,
                        FirstValue(request.Values, "legalName", "displayName", "name"),
                        NullIfWhiteSpace(GetValue(request.Values, "taxIdentifier", "taxId")),
                        FirstValue(request.Values, "notes")),
                    cancellationToken);

                return Results.Created(
                    $"/api/v1/integrations/references/{normalizedType}/{created.PartyId}/summary",
                    CreatedResponse(ToPartySummary(normalizedType, created)));
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

    private static ReferenceTypeDescriptor PartyDescriptor(string referenceType) =>
        new(
            ProductKey,
            referenceType,
            CultureInfo.InvariantCulture.TextInfo.ToTitleCase(referenceType),
            CanQuickCreate: true,
            QuickCreatePermission: "supplyarr.parties.quick_create",
            Description: $"SupplyArr-owned {referenceType} party reference.");

    private static bool CanQuickCreateParty(System.Security.Claims.ClaimsPrincipal principal)
    {
        var role = principal.GetTenantRoleKey();
        return role.Equals("tenant_admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("supplyarr_admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("supplyarr_manager", StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanQuickCreatePart(System.Security.Claims.ClaimsPrincipal principal) => CanQuickCreateParty(principal);

    private static ReferenceSummaryResponse ToPartySummary(string referenceType, ExternalPartyResponse party) =>
        new(
            ProductKey,
            referenceType,
            party.PartyId.ToString("D"),
            party.DisplayName,
            string.Join(" / ", new[] { party.PartyKey, party.LegalName }.Where(value => !string.IsNullOrWhiteSpace(value))),
            party.ApprovalStatus,
            party.UpdatedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture),
            $"/{referenceType}s/{party.PartyId:D}",
            new Dictionary<string, string>
            {
                ["partyKey"] = party.PartyKey,
                ["partyType"] = party.PartyType,
                ["status"] = party.Status,
                ["taxIdentifier"] = party.TaxIdentifier ?? string.Empty
            });

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
        ExternalPartyService parties,
        Guid tenantId,
        string partyType,
        QuickCreateRequest request,
        CancellationToken cancellationToken)
    {
        var displayName = Normalize(FirstValue(request.Values, "displayName", "legalName", "name"));
        var legalName = Normalize(FirstValue(request.Values, "legalName", "displayName", "name"));
        var taxId = Normalize(GetValue(request.Values, "taxIdentifier", "taxId"));
        var partyKey = Normalize(GetValue(request.Values, "partyKey", "key"));

        return (await parties.ListAsync(tenantId, partyType, cancellationToken))
            .Select(party =>
            {
                var reasons = new List<string>();
                if (!string.IsNullOrWhiteSpace(displayName) && Normalize(party.DisplayName) == displayName)
                {
                    reasons.Add("matching display name");
                }

                if (!string.IsNullOrWhiteSpace(legalName) && Normalize(party.LegalName) == legalName)
                {
                    reasons.Add("matching legal name");
                }

                if (!string.IsNullOrWhiteSpace(taxId) && Normalize(party.TaxIdentifier ?? string.Empty) == taxId)
                {
                    reasons.Add("matching tax identifier");
                }

                if (!string.IsNullOrWhiteSpace(partyKey) && Normalize(party.PartyKey) == partyKey)
                {
                    reasons.Add("matching party key");
                }

                return (party, reasons);
            })
            .Where(match => match.reasons.Count > 0)
            .Take(10)
            .Select(match => new DuplicateCandidateResponse(
                match.party.PartyId.ToString("D"),
                match.party.DisplayName,
                match.party.PartyKey,
                match.party.ApprovalStatus,
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

    private static bool MatchesParty(ExternalPartyResponse party, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        var needle = Normalize(query);
        return Normalize(party.DisplayName).Contains(needle)
            || Normalize(party.LegalName).Contains(needle)
            || Normalize(party.PartyKey).Contains(needle)
            || Normalize(party.TaxIdentifier ?? string.Empty).Contains(needle)
            || party.Contacts.Any(contact => Normalize(contact.Email).Contains(needle) || Normalize(contact.Phone).Contains(needle));
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
            _ => normalized
        };
    }

    private static bool IsPartyType(string referenceType) =>
        PartyReferenceTypes.Contains(referenceType, StringComparer.OrdinalIgnoreCase);

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
