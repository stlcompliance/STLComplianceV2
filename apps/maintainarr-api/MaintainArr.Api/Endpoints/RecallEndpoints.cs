using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using MaintainArr.Api.Services.Recalls;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class RecallEndpoints
{
    public static void MapMaintainArrRecallEndpoints(this WebApplication app)
    {
        MapRecallRoutes(app.MapGroup("/api/recalls").WithTags("Recalls").RequireAuthorization(), string.Empty);
        MapRecallRoutes(app.MapGroup("/api/v1/recalls").WithTags("Recalls").RequireAuthorization(), "V1");

        MapAssetRoutes(app.MapGroup("/api/assets/{assetId:guid}/recalls").WithTags("Recalls").RequireAuthorization(), string.Empty);
        MapAssetRoutes(app.MapGroup("/api/v1/assets/{assetId:guid}/recalls").WithTags("Recalls").RequireAuthorization(), "V1");
    }

    private static void MapRecallRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/providers", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(service.GetProviders());
        })
        .WithName($"ListRecallProviders{nameSuffix}");

        group.MapGet("/providers/health", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await service.GetProviderHealthAsync(cancellationToken));
        })
        .WithName($"GetRecallProviderHealth{nameSuffix}");

        group.MapGet("/dashboard", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetDashboardAsync(tenantId, cancellationToken));
        })
        .WithName($"GetRecallDashboard{nameSuffix}");

        group.MapGet("/campaigns", async (
            string? sourceProvider,
            string? status,
            string? campaignNumber,
            string? component,
            int? limit,
            int? offset,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListCampaignsAsync(
                tenantId,
                sourceProvider,
                status,
                campaignNumber,
                component,
                limit ?? 100,
                offset ?? 0,
                cancellationToken));
        })
        .WithName($"ListRecallCampaigns{nameSuffix}");

        group.MapGet("/campaigns/{campaignId:guid}", async (
            Guid campaignId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetCampaignAsync(tenantId, campaignId, cancellationToken));
        })
        .WithName($"GetRecallCampaign{nameSuffix}");

        group.MapPost("/campaigns/search/vehicle", async (
            RecallVehicleSearchRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.SearchByVehicleAsync(tenantId, request, cancellationToken));
        })
        .WithName($"SearchRecallCampaignsByVehicle{nameSuffix}");

        group.MapPost("/campaigns/search/campaign-number", async (
            RecallCampaignSearchRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.SearchByCampaignAsync(tenantId, request, cancellationToken));
        })
        .WithName($"SearchRecallCampaignsByNumber{nameSuffix}");

        group.MapPost("/campaigns", async (
            CreateRecallCampaignRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            if (request.CreateWorkOrdersNow)
            {
                authorization.RequireWorkOrdersCreate(context.User);
            }

            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.CreateCampaignAsync(tenantId, actorUserId, actorPersonId, request, cancellationToken));
        })
        .WithName($"CreateRecallCampaign{nameSuffix}");

        group.MapPatch("/campaigns/{campaignId:guid}", async (
            Guid campaignId,
            UpdateRecallCampaignRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.UpdateCampaignAsync(tenantId, actorUserId, actorPersonId, campaignId, request, cancellationToken));
        })
        .WithName($"UpdateRecallCampaign{nameSuffix}");
    }

    private static void MapAssetRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, assetId, cancellationToken: cancellationToken));
        })
        .WithName($"ListAssetRecalls{nameSuffix}");

        group.MapPost("/refresh", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.RefreshAssetAsync(tenantId, actorUserId, actorPersonId, assetId, cancellationToken));
        })
        .WithName($"RefreshAssetRecalls{nameSuffix}");

        group.MapPost("/cases/{caseId:guid}/verify", async (
            Guid assetId,
            Guid caseId,
            VerifyAssetRecallRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.VerifyAsync(tenantId, actorUserId, actorPersonId, assetId, caseId, request, cancellationToken));
        })
        .WithName($"VerifyAssetRecall{nameSuffix}");

        group.MapPost("/cases/{caseId:guid}/dismiss", async (
            Guid assetId,
            Guid caseId,
            DismissAssetRecallRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.DismissAsync(tenantId, actorUserId, actorPersonId, assetId, caseId, request, cancellationToken));
        })
        .WithName($"DismissAssetRecall{nameSuffix}");

        group.MapPost("/cases/{caseId:guid}/hold", async (
            Guid assetId,
            Guid caseId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.CreateReadinessHoldAsync(tenantId, actorUserId, actorPersonId, assetId, caseId, cancellationToken));
        })
        .WithName($"CreateAssetRecallHold{nameSuffix}");

        group.MapPost("/cases/{caseId:guid}/hold/release", async (
            Guid assetId,
            Guid caseId,
            ReleaseRecallHoldRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.ReleaseReadinessHoldAsync(tenantId, actorUserId, actorPersonId, assetId, caseId, request, cancellationToken));
        })
        .WithName($"ReleaseAssetRecallHold{nameSuffix}");

        group.MapPost("/cases/{caseId:guid}/work-order", async (
            Guid assetId,
            Guid caseId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.CreateWorkOrderAsync(tenantId, actorUserId, actorPersonId, assetId, caseId, cancellationToken));
        })
        .WithName($"CreateAssetRecallWorkOrder{nameSuffix}");

        group.MapPost("/cases/{caseId:guid}/inspection-item", async (
            Guid assetId,
            Guid caseId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            RecallService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsManage(context.User);
            authorization.RequireWorkOrdersCreate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString("D");
            return Results.Ok(await service.CreateInspectionItemAsync(tenantId, actorUserId, actorPersonId, assetId, caseId, cancellationToken));
        })
        .WithName($"CreateAssetRecallInspectionItem{nameSuffix}");
    }
}
