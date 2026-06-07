using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Endpoints;

public static class IntegrationValidationEndpoints
{
    public static void MapRoutArrIntegrationValidationEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/integrations"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/integrations"), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("Integrations").RequireAuthorization();

        group.MapPost("/assignment-validations", async (
            DispatchAssignmentPreviewRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchAssignmentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.PreviewAsync(tenantId, request, cancellationToken));
        })
        .WithName($"CreateAssignmentValidation{nameSuffix}");

        group.MapPost("/driver-readiness-checks", async (
            DriverEligibilityCheckRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DriverEligibilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CheckAsync(
                tenantId,
                actorUserId,
                request.PersonId,
                request.QualificationKey,
                request.RulePackKey,
                cancellationToken));
        })
        .WithName($"CreateDriverReadinessCheck{nameSuffix}");

        group.MapPost("/equipment-readiness-checks", async (
            AssetDispatchabilityCheckRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            AssetDispatchabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.CheckAsync(
                tenantId,
                actorUserId,
                request.VehicleRefKey,
                request.AssetTag,
                cancellationToken));
        })
        .WithName($"CreateEquipmentReadinessCheck{nameSuffix}");

        group.MapPost("/load-readiness-checks", async (
            LoadReadinessCheckRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RoutArrDbContext db,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var trip = await db.Trips
                .AsNoTracking()
                .Include(x => x.Loads)
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.TripId, cancellationToken)
                ?? throw new StlApiException("trip.not_found", "Trip was not found.", 404);

            var pendingDemandCount = await db.TripPartsDemandLines
                .AsNoTracking()
                .CountAsync(x => x.TenantId == tenantId
                    && x.TripId == request.TripId
                    && x.Status == TripPartsDemandStatuses.Pending, cancellationToken);

            var status = trip.Loads.Count == 0
                ? "not_ready"
                : pendingDemandCount > 0
                    ? "partially_ready"
                    : "ready";

            var blockers = pendingDemandCount > 0
                ? new[] { "pending_trip_parts_demand" }
                : Array.Empty<string>();

            return Results.Ok(new LoadReadinessCheckResponse(
                request.TripId,
                status,
                trip.Loads.Count == 0
                    ? "No load records are linked to this trip."
                    : pendingDemandCount > 0
                        ? "Trip has pending parts demand lines."
                        : "Trip load context is ready.",
                blockers,
                DateTimeOffset.UtcNow));
        })
        .WithName($"CreateLoadReadinessCheck{nameSuffix}");
    }
}
