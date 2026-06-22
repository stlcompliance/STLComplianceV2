using System.Text;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetImportEndpoints
{
    private const string ProductKey = "maintainarr";
    private const string TemplateVersion = "2026-06";
    private const string ImportPermission = "maintainarr.import.assets";
    private const string TemplateFileName = "maintainarr-assets-import-template-v2026-06.csv";
    private const string TemplateCsv =
        """
        assetTag,name,description,assetClass,assetType,make,model,fuelType,brakeType,tireConfiguration,siteId,governingBodyKey,lifecycleStatus
        FLT-101,Forklift 101,Main shop forklift,powered_industrial_truck,forklift,toyota_material_handling,custom,electric,disc,forklift_pneumatic,site_a,FMCSA;OSHA,in_service
        TR-240,Trailer 240,Reefer trailer,trailer,reefer_trailer,great_dane,custom,diesel,drum,trailer_tandem,site_b,FMCSA,in_service
        """;

    public static void MapMaintainArrAssetImportEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/imports", Suffix: string.Empty),
            (Route: "/api/v1/imports", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
                .WithTags("Imports")
                .RequireAuthorization();

            group.MapGet("/", (
                MaintainArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireAssetImportManage(context.User);
                return Results.Ok(new
                {
                    items = new[]
                    {
                        new { key = "manifests", path = $"{route}/manifests" },
                        new { key = "history", path = $"{route}/history" },
                        new { key = "assets-validate", path = $"{route}/assets/validate" },
                        new { key = "assets-commit", path = $"{route}/assets/commit" },
                    }
                });
            })
            .WithName($"GetMaintainArrImportIndex{suffix}");

            group.MapGet("/manifests", (
                MaintainArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireAssetImportManage(context.User);
                return Results.Ok(new[] { BuildAssetManifest() });
            })
            .WithName($"ListMaintainArrImportManifests{suffix}");

            group.MapGet("/manifests/{importTypeKey}/template", (
                string importTypeKey,
                MaintainArrAuthorizationService authorization,
                HttpContext context) =>
            {
                authorization.RequireAssetImportManage(context.User);
                if (!string.Equals(importTypeKey, MaintainArrImportTypes.Assets, StringComparison.OrdinalIgnoreCase))
                {
                    return Results.NotFound();
                }

                return Results.File(
                    Encoding.UTF8.GetBytes(TemplateCsv),
                    "text/csv",
                    TemplateFileName);
            })
            .WithName($"DownloadMaintainArrImportTemplate{suffix}");

            group.MapGet("/history", async (
                int? limit,
                MaintainArrAuthorizationService authorization,
                MaintainArrDbContext db,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetImportManage(context.User);
                var tenantId = context.User.GetTenantId();
                var take = Math.Clamp(limit ?? 25, 1, 100);
                var items = await db.MaintainArrImportBatches
                    .AsNoTracking()
                    .Where(x => x.TenantId == tenantId)
                    .OrderByDescending(x => x.CompletedAt ?? x.CreatedAt)
                    .Take(take)
                    .Select(x => new ProductImportHistoryItemResponse(
                        x.Id,
                        x.ImportType,
                        "Assets",
                        x.Status,
                        x.DryRun,
                        x.TotalRows,
                        x.SuccessCount,
                        x.ErrorCount,
                        x.CreatedByUserId,
                        null,
                        x.CompletedAt ?? x.CreatedAt,
                        x.DryRun
                            ? $"Validated {x.SuccessCount} of {x.TotalRows} rows during {x.Phase}."
                            : $"Committed {x.SuccessCount} of {x.TotalRows} rows during {x.Phase}."))
                    .ToListAsync(cancellationToken);

                return Results.Ok(new ProductImportHistoryListResponse(items));
            })
            .WithName($"ListMaintainArrImportHistory{suffix}");

            group.MapPost("/assets/validate", async (
                AssetBulkImportRequest? request,
                HttpRequest httpRequest,
                AssetBulkImportService importService,
                MaintainArrAuthorizationService authorization,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetImportManage(context.User);
                var rows = await ResolveAssetRowsAsync(httpRequest, request, cancellationToken);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await importService.ImportAsync(
                    tenantId,
                    actorUserId,
                    context.User.GetPersonId().ToString("D"),
                    rows,
                    dryRun: true,
                    MaintainArrImportPhases.Validate,
                    cancellationToken));
            })
            .WithName($"ValidateMaintainArrAssetImport{suffix}")
            .DisableAntiforgery();

            group.MapPost("/assets/commit", async (
                AssetBulkImportRequest? request,
                HttpRequest httpRequest,
                AssetBulkImportService importService,
                MaintainArrAuthorizationService authorization,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireAssetImportManage(context.User);
                var rows = await ResolveAssetRowsAsync(httpRequest, request, cancellationToken);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await importService.ImportAsync(
                    tenantId,
                    actorUserId,
                    context.User.GetPersonId().ToString("D"),
                    rows,
                    dryRun: false,
                    MaintainArrImportPhases.Commit,
                    cancellationToken));
            })
            .WithName($"CommitMaintainArrAssetImport{suffix}")
            .DisableAntiforgery();
        }
    }

    private static ProductImportManifestResponse BuildAssetManifest() =>
        new(
            ProductKey,
            MaintainArrImportTypes.Assets,
            "Assets",
            "Create MaintainArr asset master records from a product-owned CSV template with deterministic validation.",
            ["csv"],
            TemplateVersion,
            ImportPermission,
            "asset",
            ["create"],
            ["assetTag", "name"],
            [
                "description",
                "assetClass",
                "assetType",
                "make",
                "model",
                "fuelType",
                "brakeType",
                "tireConfiguration",
                "siteId",
                "governingBodyKey",
                "lifecycleStatus"
            ],
            [
                "assetClass",
                "assetType",
                "fuelType",
                "brakeType",
                "tireConfiguration",
                "lifecycleStatus"
            ],
            ["siteId"],
            ["assetTag"],
            ["assetTag", "serialNumber when collected", "equipmentNumber when collected"],
            [
                "Required field missing",
                "Duplicate asset tag in file",
                "Duplicate asset tag against existing assets",
                "Invalid controlled value",
                "Unknown site reference",
                "Product rule violation"
            ],
            ["assetTag", "name", "assetClass", "assetType", "siteId", "lifecycleStatus"],
            "Uses MaintainArr asset services and controlled value validators before final write.",
            ["maintainarr.asset.created"],
            false,
            "maintainarr.assets.import");

    private static async Task<IReadOnlyList<AssetImportRowRequest>> ResolveAssetRowsAsync(
        HttpRequest httpRequest,
        AssetBulkImportRequest? request,
        CancellationToken cancellationToken)
    {
        if (httpRequest.HasFormContentType)
        {
            var file = httpRequest.Form.Files.GetFile("file");
            if (file is null || file.Length == 0)
            {
                throw new STLCompliance.Shared.Contracts.StlApiException(
                    "imports.upload.required",
                    "Upload a CSV file using form field 'file'.",
                    400);
            }

            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var csv = await reader.ReadToEndAsync(cancellationToken);
            return AssetImportCsvParser.Parse(csv);
        }

        if (request?.Assets is { Count: > 0 })
        {
            return request.Assets;
        }

        throw new STLCompliance.Shared.Contracts.StlApiException(
            "imports.validation",
            "Provide JSON assets array or multipart CSV upload.",
            400);
    }
}
