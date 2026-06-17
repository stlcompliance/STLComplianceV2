using System.Globalization;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace MaintainArr.Api.Endpoints;

public static class ReferenceIntegrationEndpoints
{
    private const string ProductKey = "maintainarr";
    private const string AssetReferenceType = "asset";

    public static void MapMaintainArrReferenceIntegrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/integrations")
            .WithTags("Integrations")
            .RequireAuthorization();

        group.MapGet("/reference-types", () =>
            Results.Ok(new[]
            {
                new ReferenceTypeDescriptor(
                    ProductKey,
                    AssetReferenceType,
                    "Asset",
                    CanQuickCreate: true,
                    QuickCreatePermission: "maintainarr.assets.quick_create",
                    Description: "MaintainArr-owned asset reference.")
            }))
            .WithName("ListMaintainArrReferenceTypes");

        group.MapPost("/references/search", async (
            ReferenceSearchRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService assets,
            CancellationToken cancellationToken) =>
        {
            RequireAssetReference(request.ReferenceType);
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var results = (await assets.SearchAsync(
                    tenantId,
                    request.Query,
                    request.Filters?.GetValueOrDefault("status"),
                    request.Filters?.GetValueOrDefault("siteRef"),
                    request.Limit <= 0 ? 25 : request.Limit,
                    cancellationToken))
                .Select(ToSearchSummary)
                .ToArray();

            return Results.Ok(new ReferenceSearchResponse(results));
        })
        .WithName("SearchMaintainArrReferences");

        group.MapGet("/references/{referenceType}/{id}/summary", async (
            string referenceType,
            string id,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService assets,
            CancellationToken cancellationToken) =>
        {
            RequireAssetReference(referenceType);
            if (!Guid.TryParse(id, out var assetId))
            {
                throw new StlApiException("maintainarr.references.invalid_id", "MaintainArr asset reference id must be a GUID.", 400);
            }

            authorization.RequireAssetsRead(context.User);
            return Results.Ok(ToAssetSummary(await assets.GetAsync(context.User.GetTenantId(), assetId, cancellationToken)));
        })
        .WithName("GetMaintainArrReferenceSummary");

        group.MapGet("/references/{referenceType}/quick-create-schema", (
            string referenceType,
            HttpContext context,
            MaintainArrAuthorizationService authorization) =>
        {
            RequireAssetReference(referenceType);
            authorization.RequireAssetsRead(context.User);
            var allowed = CanQuickCreateAsset(context.User);
            return Results.Ok(new QuickCreateSchemaResponse(
                ProductKey,
                AssetReferenceType,
                allowed,
                "MaintainArr",
                "maintainarr.assets.quick_create",
                allowed ? null : "Asset quick create requires MaintainArr asset management access.",
                [
                    new QuickCreateFieldDescriptor("assetTag", "Asset tag / unit number", "text", Required: true),
                    new QuickCreateFieldDescriptor("name", "Name", "text", Required: true),
                    new QuickCreateFieldDescriptor("assetClass", "Asset class", "text", Required: true, DefaultValue: "quick_create"),
                    new QuickCreateFieldDescriptor("assetType", "Asset type", "text", Required: true, DefaultValue: "quick_create"),
                    new QuickCreateFieldDescriptor("siteId", "StaffArr site org unit ID", "text"),
                    new QuickCreateFieldDescriptor("description", "Description", "textarea")
                ]));
        })
        .WithName("GetMaintainArrQuickCreateSchema");

        group.MapPost("/references/{referenceType}/quick-create", async (
            string referenceType,
            QuickCreateRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetService assets,
            CancellationToken cancellationToken) =>
        {
            RequireAssetReference(referenceType);
            RequireAssetReference(request.ReferenceType);
            authorization.RequireAssetsManage(context.User);

            var tenantId = context.User.GetTenantId();
            var duplicates = (await FindAssetDuplicates(assets, tenantId, request, cancellationToken)).ToArray();
            if (duplicates.Length > 0)
            {
                return Results.Conflict(new QuickCreateResponse(
                    null,
                    duplicates,
                    Created: false,
                    ReviewStatus: "duplicate_candidates",
                    Message: "MaintainArr found possible duplicate assets. Select the existing asset or review in MaintainArr."));
            }

            var created = await assets.CreateV1Async(
                tenantId,
                context.User.GetUserId(),
                context.User.GetPersonId().ToString("D"),
                BuildAssetCreateRequest(request),
                cancellationToken);

            return Results.Created(
                $"/api/v1/integrations/references/asset/{created.AssetId:D}/summary",
                new QuickCreateResponse(
                    ToAssetSummary(created).ToCrossProductReference("quick_create"),
                    [],
                    Created: true,
                    ReviewStatus: created.LifecycleStatus,
                    Message: "Asset was created in MaintainArr through the asset registry."));
        })
        .WithName("QuickCreateMaintainArrReference");
    }

    private static bool CanQuickCreateAsset(System.Security.Claims.ClaimsPrincipal principal)
    {
        if (principal.IsPlatformAdmin())
        {
            return true;
        }

        var role = principal.GetTenantRoleKey();
        return role.Equals("tenant_admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("maintainarr_admin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("maintainarr_manager", StringComparison.OrdinalIgnoreCase);
    }

    private static ReferenceSummaryResponse ToSearchSummary(AssetSearchResponse asset) =>
        new(
            ProductKey,
            AssetReferenceType,
            asset.AssetId.ToString("D"),
            BuildAssetDisplayLabel(asset.AssetTag, asset.Name),
            string.Join(" / ", new[] { asset.TypeName, asset.StaffarrSiteNameSnapshot }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            asset.LifecycleStatus,
            asset.UpdatedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture),
            $"/assets/{asset.AssetId:D}",
            new Dictionary<string, string>
            {
                ["assetTag"] = asset.AssetTag,
                ["assetType"] = asset.TypeKey,
                ["assetClass"] = asset.ClassKey,
                ["siteRef"] = asset.SiteRef ?? string.Empty,
                ["readinessStatus"] = asset.ReadinessStatus
            });

    private static ReferenceSummaryResponse ToAssetSummary(AssetResponse asset) =>
        new(
            ProductKey,
            AssetReferenceType,
            asset.AssetId.ToString("D"),
            BuildAssetDisplayLabel(asset.AssetTag, asset.Name),
            string.Join(" / ", new[] { asset.TypeName, asset.StaffarrSiteNameSnapshot }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            asset.LifecycleStatus,
            asset.UpdatedAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture),
            $"/assets/{asset.AssetId:D}",
            new Dictionary<string, string>
            {
                ["assetTag"] = asset.AssetTag,
                ["assetType"] = asset.TypeKey,
                ["assetClass"] = asset.ClassKey,
                ["siteRef"] = asset.SiteRef ?? string.Empty
            });

    private static AssetUpsertV1Request BuildAssetCreateRequest(QuickCreateRequest request)
    {
        var assetTag = FirstValue(request.Values, "assetTag", "unitNumber", "assetNumber");
        if (string.IsNullOrWhiteSpace(assetTag))
        {
            throw new StlApiException("maintainarr.references.asset_tag_required", "Asset tag or unit number is required.", 400);
        }

        var name = FirstValue(request.Values, "name", "displayName");
        if (string.IsNullOrWhiteSpace(name))
        {
            name = assetTag;
        }

        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["unitNumber"] = assetTag,
            ["assetNumber"] = assetTag,
            ["displayName"] = name,
            ["assetClass"] = FirstValue(request.Values, "assetClass") is { Length: > 0 } assetClass
                ? assetClass
                : "quick_create",
            ["assetType"] = FirstValue(request.Values, "assetType") is { Length: > 0 } assetType
                ? assetType
                : "quick_create",
            ["lifecycleStatus"] = FirstValue(request.Values, "lifecycleStatus") is { Length: > 0 } lifecycleStatus
                ? lifecycleStatus
                : "ordered"
        };

        var siteId = FirstValue(request.Values, "siteId", "siteRef", "staffarrSiteOrgUnitId");
        if (!string.IsNullOrWhiteSpace(siteId))
        {
            values["siteId"] = siteId;
        }

        var description = FirstValue(request.Values, "description");
        if (!string.IsNullOrWhiteSpace(description))
        {
            values["description"] = description;
        }

        return new AssetUpsertV1Request(assetTag, name, description, values);
    }

    private static async Task<IEnumerable<DuplicateCandidateResponse>> FindAssetDuplicates(
        AssetService assets,
        Guid tenantId,
        QuickCreateRequest request,
        CancellationToken cancellationToken)
    {
        var assetTag = Normalize(FirstValue(request.Values, "assetTag", "unitNumber", "assetNumber"));
        if (string.IsNullOrWhiteSpace(assetTag))
        {
            return [];
        }

        return (await assets.SearchAsync(tenantId, assetTag, null, null, 25, cancellationToken))
            .Where(asset => Normalize(asset.AssetTag) == assetTag)
            .Select(asset => new DuplicateCandidateResponse(
                asset.AssetId.ToString("D"),
                BuildAssetDisplayLabel(asset.AssetTag, asset.Name),
                asset.TypeName,
                asset.LifecycleStatus,
                "matching asset tag / unit number",
                0.95m));
    }

    private static void RequireAssetReference(string referenceType)
    {
        if (!string.Equals(referenceType, AssetReferenceType, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "maintainarr.references.unsupported_type",
                $"MaintainArr does not own reference type '{referenceType}'.",
                404);
        }
    }

    private static string BuildAssetDisplayLabel(string assetTag, string name) =>
        string.Equals(assetTag, name, StringComparison.OrdinalIgnoreCase) ? assetTag : $"{assetTag} - {name}";

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
