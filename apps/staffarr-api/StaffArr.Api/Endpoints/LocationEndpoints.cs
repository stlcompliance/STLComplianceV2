using Microsoft.AspNetCore.Mvc;
using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class LocationEndpoints
{
    public static void MapStaffArrLocationEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/v1/locations", Suffix: string.Empty),
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route).WithTags("Locations").RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                InternalLocationService service,
                [AsParameters] LocationQuery request,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                authorization.RequireLocationRead(context.User, projection);
                return Results.Ok(await service.ListAsync(
                    tenantId,
                    request.IncludeArchived,
                    request.Search,
                    request.Type,
                    request.SiteOrgUnitId,
                    cancellationToken));
            })
            .WithName($"ListLocations{suffix}");

            group.MapGet("/tree", async (
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                InternalLocationService service,
                [AsParameters] LocationQuery request,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                authorization.RequireLocationRead(context.User, projection);
                return Results.Ok(await service.ListTreeAsync(
                    tenantId,
                    request.IncludeArchived,
                    request.Search,
                    request.Type,
                    request.SiteOrgUnitId,
                    cancellationToken));
            })
            .WithName($"ListLocationTree{suffix}");

            group.MapGet("/{locationId:guid}", async (
                Guid locationId,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                InternalLocationService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                authorization.RequireLocationRead(context.User, projection);
                return Results.Ok(await service.GetAsync(tenantId, locationId, cancellationToken));
            })
            .WithName($"GetLocation{suffix}");

            group.MapPost("/", async (
                CreateInternalLocationRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                InternalLocationService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                authorization.RequireLocationCreate(context.User, projection);
                var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
                return Results.Created($"{route}/{created.LocationId}", created);
            })
            .WithName($"CreateLocation{suffix}");

            group.MapPut("/{locationId:guid}", async (
                Guid locationId,
                UpdateInternalLocationRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                InternalLocationService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                authorization.RequireLocationUpdate(context.User, projection);
                var updated = await service.UpdateAsync(tenantId, actorUserId, locationId, request, cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"UpdateLocation{suffix}");

            group.MapPost("/{locationId:guid}/archive", async (
                Guid locationId,
                ArchiveInternalLocationRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                PermissionProjectionService permissionProjectionService,
                InternalLocationService service,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var projection = await permissionProjectionService.GetEffectivePermissionProjectionAsync(
                    tenantId,
                    context.User.GetPersonId(),
                    cancellationToken);

                authorization.RequireLocationArchive(context.User, projection);
                var updated = await service.ArchiveAsync(tenantId, actorUserId, locationId, request, cancellationToken);
                return Results.Ok(updated);
            })
            .WithName($"ArchiveLocation{suffix}");
        }
    }

    private sealed record LocationQuery(
        bool IncludeArchived = false,
        string? Search = null,
        string? Type = null,
        Guid? SiteOrgUnitId = null);
}
