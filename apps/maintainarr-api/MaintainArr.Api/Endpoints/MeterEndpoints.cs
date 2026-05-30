using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MeterEndpoints
{
    public static void MapMaintainArrMeterEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api").WithTags("Meters").RequireAuthorization(), string.Empty);
        MapRoutes(app.MapGroup("/api/v1").WithTags("Meters").RequireAuthorization(), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/assets/{assetId:guid}/meters", async (
            Guid assetId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetMeterService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMetersRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListForAssetAsync(tenantId, assetId, cancellationToken));
        })
        .WithName($"ListAssetMeters{nameSuffix}");

        group.MapPost("/assets/{assetId:guid}/meters", async (
            Guid assetId,
            CreateAssetMeterRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetMeterService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMetersManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, assetId, request, cancellationToken);
            return Results.Created($"/api/meters/{created.AssetMeterId}", created);
        })
        .WithName($"CreateAssetMeter{nameSuffix}");

        group.MapGet("/meters/{assetMeterId:guid}", async (
            Guid assetMeterId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            AssetMeterService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMetersRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, assetMeterId, cancellationToken));
        })
        .WithName($"GetAssetMeter{nameSuffix}");

        group.MapGet("/meters/{assetMeterId:guid}/readings", async (
            Guid assetMeterId,
            int? limit,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MeterReadingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMetersRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, assetMeterId, limit, cancellationToken));
        })
        .WithName($"ListMeterReadings{nameSuffix}");

        group.MapPost("/meters/{assetMeterId:guid}/readings", async (
            Guid assetMeterId,
            RecordMeterReadingRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MeterReadingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMetersRecord(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var recorded = await service.RecordAsync(
                tenantId,
                actorUserId,
                assetMeterId,
                request,
                cancellationToken);
            return Results.Created($"/api/meters/{assetMeterId}/readings/{recorded.MeterReadingId}", recorded);
        })
        .WithName($"RecordMeterReading{nameSuffix}");

        group.MapGet("/meters/{assetMeterId:guid}/pm-forecast", async (
            Guid assetMeterId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MeterPmForecastService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetForecastAsync(tenantId, assetMeterId, cancellationToken));
        })
        .WithName($"GetMeterPmForecast{nameSuffix}");
    }
}
