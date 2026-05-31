using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MeterEndpoints
{
    public static void MapMaintainArrMeterEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api").WithTags("Meters").RequireAuthorization(), string.Empty, "/api");
        MapRoutes(app.MapGroup("/api/v1").WithTags("Meters").RequireAuthorization(), "V1", "/api/v1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix, string routePrefix)
    {
        group.MapGet("/meters", async (
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
        .WithName($"ListMetersByAsset{nameSuffix}");

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
            return Results.Created($"{routePrefix}/meters/{created.AssetMeterId}", created);
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
            return Results.Created($"{routePrefix}/meters/{assetMeterId}/readings/{recorded.MeterReadingId}", recorded);
        })
        .WithName($"RecordMeterReading{nameSuffix}");

        group.MapPost("/meters/{assetMeterId:guid}/readings/corrections", async (
            Guid assetMeterId,
            CorrectMeterReadingRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MeterReadingService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMetersRecord(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var corrected = await service.CorrectAsync(
                tenantId,
                actorUserId,
                assetMeterId,
                request,
                cancellationToken);
            return Results.Created($"{routePrefix}/meters/{assetMeterId}/readings/{corrected.MeterReadingId}", corrected);
        })
        .WithName($"CorrectMeterReading{nameSuffix}");

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

        group.MapGet("/meters/alerts", async (
            int? staleAfterDays,
            int? limit,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            MaintainArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireMetersRead(context.User);
            var tenantId = context.User.GetTenantId();
            var effectiveStaleDays = Math.Clamp(staleAfterDays ?? 14, 0, 3650);
            var effectiveLimit = Math.Clamp(limit ?? 100, 1, 500);
            var now = DateTimeOffset.UtcNow;
            var threshold = now.AddDays(-effectiveStaleDays);

            var alerts = await (
                from meter in db.AssetMeters.AsNoTracking()
                join asset in db.Assets.AsNoTracking() on meter.AssetId equals asset.Id
                where meter.TenantId == tenantId
                    && asset.TenantId == tenantId
                    && meter.Status == MeterStatuses.Active
                    && (meter.LastReadingAt == null || meter.LastReadingAt < threshold)
                orderby meter.LastReadingAt ascending, meter.Name
                select new MeterMissingReadingAlertResponse(
                    meter.Id,
                    asset.Id,
                    asset.AssetTag,
                    asset.Name,
                    meter.MeterKey,
                    meter.Name,
                    meter.LastReadingAt,
                    meter.LastReadingAt == null
                        ? null
                        : (int?)Math.Floor((now - meter.LastReadingAt.Value).TotalDays),
                    meter.LastReadingAt == null
                        ? $"Meter \"{meter.Name}\" has no readings yet."
                        : $"Meter \"{meter.Name}\" has not received a reading in {Math.Floor((now - meter.LastReadingAt.Value).TotalDays)} days."))
                .Take(effectiveLimit)
                .ToListAsync(cancellationToken);

            return Results.Ok(alerts);
        })
        .WithName($"ListMeterMissingReadingAlerts{nameSuffix}");
    }
}
