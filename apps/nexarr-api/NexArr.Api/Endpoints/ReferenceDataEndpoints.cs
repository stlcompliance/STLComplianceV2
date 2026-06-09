using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Endpoints;

public static class ReferenceDataEndpoints
{
    public static void MapReferenceDataEndpoints(this WebApplication app)
    {
        var admin = app.MapGroup("/api/platform-admin/reference-data")
            .WithTags("ReferenceData")
            .RequireAuthorization();

        admin.MapGet("/dashboard", async (
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetDashboardAsync(context.User, cancellationToken));
        })
        .WithName("ReferenceDataDashboard");

        admin.MapGet("/datasets", async (
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListDatasetsAsync(context.User, cancellationToken));
        })
        .WithName("ReferenceDataListDatasets");

        admin.MapPost("/datasets", async (
            CreateReferenceDatasetRequest request,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Created("/api/platform-admin/reference-data/datasets", await service.CreateDatasetAsync(context.User, request, cancellationToken));
        })
        .WithName("ReferenceDataCreateDataset");

        admin.MapGet("/sources", async (
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListSourcesAsync(context.User, cancellationToken));
        })
        .WithName("ReferenceDataListSources");

        admin.MapPost("/sources", async (
            CreateReferenceSourceRequest request,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Created("/api/platform-admin/reference-data/sources", await service.CreateSourceAsync(context.User, request, cancellationToken));
        })
        .WithName("ReferenceDataCreateSource");

        admin.MapPost("/datasets/{datasetId:guid}/input", async (
            Guid datasetId,
            CreateReferenceDatasetInputRequest request,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.CreateDatasetInputAsync(context.User, datasetId, request, cancellationToken));
        })
        .WithName("ReferenceDataCreateDatasetInput");

        admin.MapGet("/imports", async (
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListImportsAsync(context.User, cancellationToken));
        })
        .WithName("ReferenceDataListImports");

        admin.MapPost("/imports", async (
            CreateReferenceImportRequest request,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Created("/api/platform-admin/reference-data/imports", await service.CreateImportAsync(context.User, request, cancellationToken));
        })
        .WithName("ReferenceDataCreateImport");

        admin.MapPost("/imports/master-csv", async (
            CreateReferenceMasterCsvImportRequest request,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Created("/api/platform-admin/reference-data/imports/master-csv", await service.CreateMasterCsvImportAsync(context.User, request, cancellationToken));
        })
        .WithName("ReferenceDataCreateMasterCsvImport");

        admin.MapGet("/imports/{jobId:guid}", async (
            Guid jobId,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetImportAsync(context.User, jobId, cancellationToken));
        })
        .WithName("ReferenceDataGetImport");

        admin.MapGet("/imports/{jobId:guid}/staging-records", async (
            Guid jobId,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListStagingRecordsAsync(context.User, jobId, cancellationToken));
        })
        .WithName("ReferenceDataListStagingRecords");

        admin.MapPost("/staging-records/{id:guid}/approve", async (
            Guid id,
            ReviewDecisionRequest request,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ApproveAsync(context.User, id, request, cancellationToken));
        })
        .WithName("ReferenceDataApproveStagingRecord");

        admin.MapPost("/staging-records/{id:guid}/reject", async (
            Guid id,
            ReviewDecisionRequest request,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RejectAsync(context.User, id, request, cancellationToken));
        })
        .WithName("ReferenceDataRejectStagingRecord");

        admin.MapPost("/staging-records/{id:guid}/merge", async (
            Guid id,
            ReviewDecisionRequest request,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.MergeAsync(context.User, id, request, cancellationToken));
        })
        .WithName("ReferenceDataMergeStagingRecord");

        admin.MapPost("/staging-records/{id:guid}/escalate", async (
            Guid id,
            ReviewDecisionRequest request,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.EscalateAsync(context.User, id, request, cancellationToken));
        })
        .WithName("ReferenceDataEscalateStagingRecord");

        admin.MapPost("/datasets/{datasetId:guid}/publish", async (
            Guid datasetId,
            string? summary,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.PublishDatasetAsync(context.User, datasetId, summary, cancellationToken));
        })
        .WithName("ReferenceDataPublishDataset");

        admin.MapGet("/publish-history", async (
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListPublishHistoryAsync(context.User, cancellationToken));
        })
        .WithName("ReferenceDataPublishHistory");

        admin.MapGet("/crosswalks", async (
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListCrosswalksAsync(context.User, cancellationToken));
        })
        .WithName("ReferenceDataListCrosswalks");

        admin.MapPost("/crosswalks", async (
            CreateReferenceCrosswalkRequest request,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Created("/api/platform-admin/reference-data/crosswalks", await service.CreateCrosswalkAsync(context.User, request, cancellationToken));
        })
        .WithName("ReferenceDataCreateCrosswalk");

        var product = app.MapGroup("/api/v1/reference-data")
            .WithTags("ReferenceData");

        product.MapGet("/catalogs/{datasetKey}/entities", async (
            string datasetKey,
            string? entityType,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetCatalogEntitiesAsync(datasetKey, context.User, entityType, cancellationToken));
        })
        .WithName("ReferenceDataGetCatalogEntities");

        product.MapGet("/entities/{id:guid}", async (
            Guid id,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetEntityAsync(context.User, id, cancellationToken));
        })
        .WithName("ReferenceDataGetEntity");

        product.MapGet("/lookup", async (
            string? datasetKey,
            string? entityType,
            string? canonicalKey,
            string? externalSystem,
            string? externalKey,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.LookupAsync(context.User, datasetKey, entityType, canonicalKey, externalSystem, externalKey, cancellationToken));
        })
        .WithName("ReferenceDataLookup");

        product.MapGet("/crosswalks/resolve", async (
            string externalSystem,
            string externalKey,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ResolveCrosswalkAsync(context.User, externalSystem, externalKey, cancellationToken));
        })
        .WithName("ReferenceDataResolveCrosswalk");

        product.MapGet("/vehicles/decode-vin", async (
            string vin,
            int modelYear,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.DecodeVehicleAsync(context.User, vin, modelYear, cancellationToken));
        })
        .WithName("ReferenceDataDecodeVin");

        product.MapGet("/products/lookup-gtin", async (
            string gtin,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.LookupGtinAsync(context.User, gtin, cancellationToken));
        })
        .WithName("ReferenceDataLookupGtin");

        product.MapGet("/sds/lookup", async (
            string manufacturer,
            string product,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.LookupSdsAsync(context.User, manufacturer, product, cancellationToken));
        })
        .WithName("ReferenceDataLookupSds");

        product.MapGet("/chemicals/lookup", async (
            string cas,
            HttpContext context,
            ReferenceDataService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.LookupChemicalAsync(context.User, cas, cancellationToken));
        })
        .WithName("ReferenceDataLookupChemical");
    }
}
