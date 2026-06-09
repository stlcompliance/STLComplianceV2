using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using MaintainArr.Api.Services.ExternalIntelligence;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class ExternalIntelligenceEndpoints
{
    public static void MapMaintainArrExternalIntelligenceEndpoints(this WebApplication app)
    {
        MapAssetRoutes(app.MapGroup("/api/assets/{assetId:guid}/external-intelligence").WithTags("External Intelligence").RequireAuthorization(), string.Empty);
        MapAssetRoutes(app.MapGroup("/api/v1/assets/{assetId:guid}/external-intelligence").WithTags("External Intelligence").RequireAuthorization(), "V1");

        MapReferenceRoutes(app.MapGroup("/api/external-intelligence").WithTags("External Intelligence").RequireAuthorization(), string.Empty);
        MapReferenceRoutes(app.MapGroup("/api/v1/external-intelligence").WithTags("External Intelligence").RequireAuthorization(), "V1");
    }

    private static void MapAssetRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetOverviewAsync(tenantId, assetId, cancellationToken));
        })
        .WithName($"GetAssetExternalIntelligenceOverview{nameSuffix}");

        group.MapGet("/identifiers", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListIdentifiersAsync(tenantId, assetId, cancellationToken));
        })
        .WithName($"ListAssetExternalIntelligenceIdentifiers{nameSuffix}");

        group.MapGet("/snapshots", async (
            Guid assetId,
            string? snapshotType,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListSnapshotsAsync(tenantId, assetId, snapshotType, cancellationToken));
        })
        .WithName($"ListAssetExternalIntelligenceSnapshots{nameSuffix}");

        group.MapGet("/suggestions", async (
            Guid assetId,
            string? status,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListSuggestionsAsync(tenantId, assetId, status, cancellationToken));
        })
        .WithName($"ListAssetExternalIntelligenceSuggestions{nameSuffix}");

        group.MapGet("/recalls", async (
            Guid assetId,
            string? status,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListRecallsAsync(tenantId, assetId, status, cancellationToken));
        })
        .WithName($"ListAssetExternalIntelligenceRecalls{nameSuffix}");

        group.MapGet("/complaints", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var overview = await service.GetOverviewAsync(tenantId, assetId, cancellationToken);
            return Results.Ok(overview.Complaints);
        })
        .WithName($"ListAssetExternalIntelligenceComplaints{nameSuffix}");

        group.MapPost("/refresh", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.RefreshAssetAsync(tenantId, actorUserId, actorPersonId, assetId, cancellationToken));
        })
        .WithName($"RefreshAssetExternalIntelligence{nameSuffix}");

        group.MapPost("/suggestions/{suggestionId:guid}/accept", async (
            Guid assetId,
            Guid suggestionId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.AcceptSuggestionAsync(tenantId, actorUserId, actorPersonId, assetId, suggestionId, cancellationToken));
        })
        .WithName($"AcceptAssetExternalIntelligenceSuggestion{nameSuffix}");

        group.MapPost("/suggestions/{suggestionId:guid}/reject", async (
            Guid assetId,
            Guid suggestionId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.RejectSuggestionAsync(tenantId, actorUserId, actorPersonId, assetId, suggestionId, cancellationToken));
        })
        .WithName($"RejectAssetExternalIntelligenceSuggestion{nameSuffix}");

        group.MapPost("/recalls/{recallId:guid}/work-order", async (
            Guid assetId,
            Guid recallId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CreateRecallWorkOrderAsync(tenantId, actorUserId, assetId, recallId, cancellationToken));
        })
        .WithName($"CreateAssetRecallWorkOrder{nameSuffix}");
    }

    private static void MapReferenceRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/providers", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(service.GetProviders());
        })
        .WithName($"ListExternalIntelligenceProviders{nameSuffix}");

        group.MapGet("/providers/health", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await service.GetProviderHealthAsync(cancellationToken));
        })
        .WithName($"GetExternalIntelligenceProviderHealth{nameSuffix}");

        group.MapPost("/vin/decode", async (
            ExternalVinDecodeRequest request,
            Guid? assetId,
            bool persist,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.DecodeVinAsync(tenantId, request.Vin, request.ModelYear, assetId, persist, cancellationToken));
        })
        .WithName($"DecodeVin{nameSuffix}");

        group.MapPost("/vin/batch-decode", async (
            ExternalVinDecodeBatchRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.DecodeVinBatchAsync(tenantId, request, cancellationToken));
        })
        .WithName($"DecodeVinBatch{nameSuffix}");

        group.MapGet("/references/makes", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetMakesAsync(tenantId, cancellationToken));
        })
        .WithName($"ListExternalIntelligenceMakes{nameSuffix}");

        group.MapGet("/references/manufacturers", async (
            string? manufacturerType,
            int page,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetManufacturersAsync(tenantId, manufacturerType, page <= 0 ? 1 : page, cancellationToken));
        })
        .WithName($"ListExternalIntelligenceManufacturers{nameSuffix}");

        group.MapGet("/references/models", async (
            string make,
            int? modelYear,
            string? vehicleType,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetModelsForMakeAsync(tenantId, make, modelYear, vehicleType, cancellationToken));
        })
        .WithName($"ListExternalIntelligenceModels{nameSuffix}");

        group.MapGet("/references/equipment-plant-codes", async (
            int year,
            string? equipmentType,
            string? reportType,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            ExternalIntelligenceService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            if (year <= 0)
            {
                throw new StlApiException(
                    "external_intelligence.validation",
                    "year is required for equipment plant code lookups.",
                    400);
            }

            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetEquipmentPlantCodesAsync(tenantId, year, equipmentType, reportType, cancellationToken));
        })
        .WithName($"ListExternalIntelligenceEquipmentPlantCodes{nameSuffix}");
    }
}
