using System.Globalization;
using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace StaffArr.Api.Endpoints;

public static class ReferenceIntegrationEndpoints
{
    private const string ProductKey = "staffarr";
    private const string LocationReferenceType = "location";
    private const string PersonReferenceType = "person";

    public static void MapStaffArrReferenceIntegrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/integrations")
            .WithTags("Integrations")
            .RequireAuthorization();

        group.MapGet("/reference-types", () =>
            Results.Ok(new[]
            {
                new ReferenceTypeDescriptor(
                    ProductKey,
                    LocationReferenceType,
                    "Location",
                    CanQuickCreate: true,
                    QuickCreatePermission: "staffarr.locations.quick_create",
                    Description: "StaffArr-owned internal location reference."),
                new ReferenceTypeDescriptor(
                    ProductKey,
                    PersonReferenceType,
                    "Person",
                    CanQuickCreate: false,
                    Description: "StaffArr-owned workforce person reference. Quick create is intentionally disabled.")
            }))
            .WithName("ListStaffArrReferenceTypes");

        group.MapPost("/references/search", async (
            ReferenceSearchRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            InternalLocationService locations,
            PeopleService people,
            CancellationToken cancellationToken) =>
        {
            var referenceType = NormalizeReferenceType(request.ReferenceType);
            var tenantId = context.User.GetTenantId();
            var limit = Math.Clamp(request.Limit <= 0 ? 25 : request.Limit, 1, 50);

            if (referenceType == LocationReferenceType)
            {
                authorization.RequireLocationRead(context.User);
                var results = (await locations.ListAsync(
                        tenantId,
                        includeArchived: false,
                        request.Query,
                        type: null,
                        siteOrgUnitId: null,
                        cancellationToken))
                    .Take(limit)
                    .Select(ToLocationSummary)
                    .ToArray();
                return Results.Ok(new ReferenceSearchResponse(results));
            }

            if (referenceType == PersonReferenceType)
            {
                authorization.RequirePeopleRead(context.User);
                var results = (await people.ListAsync(tenantId, request.Query, null, limit, cancellationToken))
                    .Select(ToPersonSummary)
                    .ToArray();
                return Results.Ok(new ReferenceSearchResponse(results));
            }

            throw UnsupportedReferenceType(referenceType);
        })
        .WithName("SearchStaffArrReferences");

        group.MapGet("/references/{referenceType}/{id}/summary", async (
            string referenceType,
            string id,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            InternalLocationService locations,
            PeopleService people,
            CancellationToken cancellationToken) =>
        {
            var normalizedType = NormalizeReferenceType(referenceType);
            if (!Guid.TryParse(id, out var parsedId))
            {
                throw new StlApiException("staffarr.references.invalid_id", "StaffArr reference id must be a GUID.", 400);
            }

            var tenantId = context.User.GetTenantId();
            if (normalizedType == LocationReferenceType)
            {
                authorization.RequireLocationRead(context.User);
                return Results.Ok(ToLocationSummary(await locations.GetAsync(tenantId, parsedId, cancellationToken)));
            }

            if (normalizedType == PersonReferenceType)
            {
                authorization.RequirePeopleRead(context.User);
                return Results.Ok(ToPersonSummary(await people.GetByIdAsync(tenantId, parsedId, cancellationToken)));
            }

            throw UnsupportedReferenceType(normalizedType);
        })
        .WithName("GetStaffArrReferenceSummary");

        group.MapGet("/references/{referenceType}/quick-create-schema", (
            string referenceType,
            HttpContext context,
            StaffArrAuthorizationService authorization) =>
        {
            var normalizedType = NormalizeReferenceType(referenceType);
            if (normalizedType == PersonReferenceType)
            {
                authorization.RequirePeopleRead(context.User);
                return Results.Ok(new QuickCreateSchemaResponse(
                    ProductKey,
                    PersonReferenceType,
                    Allowed: false,
                    ManagedByLabel: "StaffArr",
                    DisabledReason: "Person quick create is disabled until StaffArr defines the governed identity workflow.",
                    Fields: []));
            }

            if (normalizedType == LocationReferenceType)
            {
                authorization.RequireLocationRead(context.User);
                var allowed = CanQuickCreateLocation(context.User);
                return Results.Ok(new QuickCreateSchemaResponse(
                    ProductKey,
                    LocationReferenceType,
                    allowed,
                    "StaffArr",
                    "staffarr.locations.quick_create",
                    allowed ? null : "Location quick create requires StaffArr location creation access.",
                    [
                        new QuickCreateFieldDescriptor("name", "Name", "text", Required: true),
                        new QuickCreateFieldDescriptor("code", "Code / location number", "text"),
                        new QuickCreateFieldDescriptor(
                            "locationType",
                            "Location type",
                            "select",
                            Required: true,
                            DefaultValue: "other",
                            Options:
                            [
                                new QuickCreateOptionDescriptor("warehouse", "Warehouse"),
                                new QuickCreateOptionDescriptor("dock", "Dock"),
                                new QuickCreateOptionDescriptor("room", "Room"),
                                new QuickCreateOptionDescriptor("yard", "Yard"),
                                new QuickCreateOptionDescriptor("parts_room", "Parts room"),
                                new QuickCreateOptionDescriptor("staging_area", "Staging area"),
                                new QuickCreateOptionDescriptor("other", "Other")
                            ]),
                        new QuickCreateFieldDescriptor("siteOrgUnitId", "Site org unit ID", "text", Required: true),
                        new QuickCreateFieldDescriptor("parentLocationId", "Parent location ID", "text"),
                        new QuickCreateFieldDescriptor("description", "Description", "textarea")
                    ]));
            }

            throw UnsupportedReferenceType(normalizedType);
        })
        .WithName("GetStaffArrQuickCreateSchema");

        group.MapPost("/references/{referenceType}/quick-create", async (
            string referenceType,
            QuickCreateRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            InternalLocationService locations,
            CancellationToken cancellationToken) =>
        {
            var normalizedType = NormalizeReferenceType(referenceType);
            if (normalizedType == PersonReferenceType)
            {
                return Results.StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            if (normalizedType != LocationReferenceType)
            {
                throw UnsupportedReferenceType(normalizedType);
            }

            if (!string.Equals(normalizedType, NormalizeReferenceType(request.ReferenceType), StringComparison.Ordinal))
            {
                throw new StlApiException("staffarr.references.type_mismatch", "Reference type path and request must match.", 400);
            }

            authorization.RequireLocationCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var duplicates = (await FindLocationDuplicates(locations, tenantId, request, cancellationToken)).ToArray();
            if (duplicates.Length > 0)
            {
                return Results.Conflict(new QuickCreateResponse(
                    null,
                    duplicates,
                    Created: false,
                    ReviewStatus: "duplicate_candidates",
                    Message: "StaffArr found possible duplicate locations. Select an existing location or review in StaffArr."));
            }

            var created = await locations.CreateAsync(
                tenantId,
                context.User.GetUserId(),
                BuildCreateLocationRequest(request),
                cancellationToken);

            return Results.Created(
                $"/api/v1/integrations/references/location/{created.LocationId:D}/summary",
                new QuickCreateResponse(
                    ToLocationSummary(created).ToCrossProductReference("quick_create"),
                    [],
                    Created: true,
                    ReviewStatus: "planned",
                    Message: "Location was created in StaffArr as a planned internal location."));
        })
        .WithName("QuickCreateStaffArrReference");
    }

    private static bool CanQuickCreateLocation(System.Security.Claims.ClaimsPrincipal principal)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        var role = principal.GetTenantRoleKey();
        return role.Equals("tenant_admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("staffarr_admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("hr_admin", StringComparison.OrdinalIgnoreCase);
    }

    private static ReferenceSummaryResponse ToLocationSummary(InternalLocationResponse location) =>
        new(
            ProductKey,
            LocationReferenceType,
            location.LocationId.ToString("D"),
            location.Name,
            string.Join(" / ", new[] { location.LocationNumber, location.SiteNameSnapshot, location.ParentPathSnapshot }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            location.Status,
            null,
            $"/locations/{location.LocationId:D}",
            new Dictionary<string, string>
            {
                ["locationNumber"] = location.LocationNumber,
                ["locationType"] = location.LocationType,
                ["siteOrgUnitId"] = location.SiteOrgUnitId?.ToString("D") ?? string.Empty,
                ["siteName"] = location.SiteNameSnapshot
            });

    private static ReferenceSummaryResponse ToPersonSummary(StaffPersonSummaryResponse person) =>
        new(
            ProductKey,
            PersonReferenceType,
            person.PersonId.ToString("D"),
            person.DisplayName,
            string.Join(" / ", new[] { person.PrimaryEmail, person.JobTitle, person.PrimaryOrgUnitName }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            person.EmploymentStatus,
            null,
            $"/people/{person.PersonId:D}",
            new Dictionary<string, string>
            {
                ["primaryEmail"] = person.PrimaryEmail,
                ["jobTitle"] = person.JobTitle ?? string.Empty,
                ["primaryOrgUnitName"] = person.PrimaryOrgUnitName ?? string.Empty
            });

    private static ReferenceSummaryResponse ToPersonSummary(StaffPersonDetailResponse person) =>
        new(
            ProductKey,
            PersonReferenceType,
            person.PersonId.ToString("D"),
            person.DisplayName,
            string.Join(" / ", new[] { person.PrimaryEmail, person.JobTitle, person.PrimaryOrgUnitName }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            person.EmploymentStatus,
            person.UpdatedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture),
            $"/people/{person.PersonId:D}",
            new Dictionary<string, string>
            {
                ["primaryEmail"] = person.PrimaryEmail,
                ["jobTitle"] = person.JobTitle ?? string.Empty,
                ["primaryOrgUnitName"] = person.PrimaryOrgUnitName ?? string.Empty
            });

    private static CreateInternalLocationRequest BuildCreateLocationRequest(QuickCreateRequest request)
    {
        var name = FirstValue(request.Values, "name", "displayName");
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new StlApiException("staffarr.references.location_name_required", "Location name is required.", 400);
        }

        if (!Guid.TryParse(GetValue(request.Values, "siteOrgUnitId", "siteId"), out var siteOrgUnitId))
        {
            throw new StlApiException("staffarr.references.site_required", "Site org unit ID is required.", 400);
        }

        Guid? parentLocationId = null;
        var parentRaw = GetValue(request.Values, "parentLocationId", "parentId");
        if (!string.IsNullOrWhiteSpace(parentRaw))
        {
            if (!Guid.TryParse(parentRaw, out var parsedParent))
            {
                throw new StlApiException("staffarr.references.parent_invalid", "Parent location ID must be a GUID.", 400);
            }

            parentLocationId = parsedParent;
        }

        return new CreateInternalLocationRequest(
            name,
            FirstValue(request.Values, "locationType", "type") is { Length: > 0 } type ? type : "other",
            parentLocationId,
            siteOrgUnitId,
            GetValue(request.Values, "code", "locationNumber"),
            GetValue(request.Values, "description"),
            Status: "planned",
            AllowedProductUsage: "all");
    }

    private static async Task<IEnumerable<DuplicateCandidateResponse>> FindLocationDuplicates(
        InternalLocationService locations,
        Guid tenantId,
        QuickCreateRequest request,
        CancellationToken cancellationToken)
    {
        var name = Normalize(FirstValue(request.Values, "name", "displayName"));
        var code = Normalize(GetValue(request.Values, "code", "locationNumber"));
        var siteRaw = GetValue(request.Values, "siteOrgUnitId", "siteId");
        var parentRaw = GetValue(request.Values, "parentLocationId", "parentId");

        return (await locations.ListAsync(tenantId, includeArchived: false, null, null, null, cancellationToken))
            .Select(location =>
            {
                var reasons = new List<string>();
                var sameSite = string.IsNullOrWhiteSpace(siteRaw)
                    || string.Equals(location.SiteOrgUnitId?.ToString("D"), siteRaw, StringComparison.OrdinalIgnoreCase);
                var sameParent = string.IsNullOrWhiteSpace(parentRaw)
                    || string.Equals(location.ParentLocationId?.ToString("D"), parentRaw, StringComparison.OrdinalIgnoreCase);

                if (sameSite && sameParent && !string.IsNullOrWhiteSpace(name) && Normalize(location.Name) == name)
                {
                    reasons.Add("matching name in site/parent");
                }

                if (sameSite && !string.IsNullOrWhiteSpace(code) && Normalize(location.LocationNumber) == code)
                {
                    reasons.Add("matching code in site");
                }

                return (location, reasons);
            })
            .Where(match => match.reasons.Count > 0)
            .Take(10)
            .Select(match => new DuplicateCandidateResponse(
                match.location.LocationId.ToString("D"),
                match.location.Name,
                match.location.LocationNumber,
                match.location.Status,
                string.Join(", ", match.reasons),
                match.reasons.Count > 1 ? 0.95m : 0.8m));
    }

    private static string NormalizeReferenceType(string referenceType) =>
        referenceType.Trim().Replace('-', '_').ToLowerInvariant();

    private static StlApiException UnsupportedReferenceType(string referenceType) =>
        new(
            "staffarr.references.unsupported_type",
            $"StaffArr does not own reference type '{referenceType}'.",
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

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();
}
