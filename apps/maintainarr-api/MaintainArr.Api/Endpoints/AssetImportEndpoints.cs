using MaintainArr.Api.Contracts;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetImportEndpoints
{
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
                        new { key = "assets-validate", path = $"{route}/assets/validate" },
                        new { key = "assets-commit", path = $"{route}/assets/commit" },
                    }
                });
            })
            .WithName($"GetMaintainArrImportIndex{suffix}");

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
                    rows,
                    dryRun: false,
                    MaintainArrImportPhases.Commit,
                    cancellationToken));
            })
            .WithName($"CommitMaintainArrAssetImport{suffix}")
            .DisableAntiforgery();
        }
    }

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
