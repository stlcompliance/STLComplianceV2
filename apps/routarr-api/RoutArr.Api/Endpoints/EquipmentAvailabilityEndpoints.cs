using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class EquipmentAvailabilityEndpoints
{
    public static void MapRoutArrEquipmentAvailabilityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/equipment-availability").WithTags("EquipmentAvailability").RequireAuthorization();

        group.MapGet("/", async (
            string? vehicleRefKey,
            string? scope,
            string? start,
            string? end,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            EquipmentAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEquipmentAvailabilityRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(
                tenantId,
                vehicleRefKey,
                scope,
                start,
                end,
                cancellationToken));
        })
        .WithName("ListEquipmentAvailability");

        group.MapGet("/{availabilityId:guid}", async (
            Guid availabilityId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            EquipmentAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEquipmentAvailabilityRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, availabilityId, cancellationToken));
        })
        .WithName("GetEquipmentAvailability");

        group.MapPost("/", async (
            CreateEquipmentAvailabilityRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            EquipmentAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEquipmentAvailabilityWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/equipment-availability/{created.AvailabilityId}", created);
        })
        .WithName("CreateEquipmentAvailability");

        group.MapPatch("/{availabilityId:guid}", async (
            Guid availabilityId,
            UpdateEquipmentAvailabilityRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            EquipmentAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEquipmentAvailabilityWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpdateAsync(
                tenantId,
                actorUserId,
                availabilityId,
                request,
                cancellationToken));
        })
        .WithName("UpdateEquipmentAvailability");

        group.MapDelete("/{availabilityId:guid}", async (
            Guid availabilityId,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            EquipmentAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEquipmentAvailabilityWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            await service.DeleteAsync(tenantId, actorUserId, availabilityId, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteEquipmentAvailability");
    }
}
