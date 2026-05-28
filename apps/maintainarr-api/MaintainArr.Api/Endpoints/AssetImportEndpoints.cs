using MaintainArr.Api.Contracts;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AssetImportEndpoints
{
    public static void MapMaintainArrAssetImportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/imports")
            .WithTags("Imports")
            .RequireAuthorization();

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
        .WithName("ValidateMaintainArrAssetImport")
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
        .WithName("CommitMaintainArrAssetImport")
        .DisableAntiforgery();
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
