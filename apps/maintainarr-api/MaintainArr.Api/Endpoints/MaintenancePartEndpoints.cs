using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MaintenancePartEndpoints
{
    public static void MapMaintainArrMaintenancePartEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/parts"),
            app.MapGroup("/api/v1/parts"),
        };

        foreach (var group in groups)
        {
            group.WithTags("MaintenanceParts").RequireAuthorization();

            group.MapGet("/", async (
                string? search,
                string? status,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                MaintenancePartService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartsRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListAsync(tenantId, search, status, cancellationToken));
            });

            group.MapPost("/", async (
                CreateMaintenancePartRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                MaintenancePartService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartsCreate(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString();
                var created = await service.CreateAsync(
                    tenantId,
                    actorUserId,
                    actorPersonId,
                    request,
                    cancellationToken);
                return Results.Created($"/api/v1/parts/{created.PartId}", created);
            });

            group.MapGet("/{partId:guid}", async (
                Guid partId,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                MaintenancePartService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartsRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetAsync(tenantId, partId, cancellationToken));
            });

            group.MapPatch("/{partId:guid}", async (
                Guid partId,
                UpdateMaintenancePartRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                MaintenancePartService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartsUpdate(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString();
                return Results.Ok(await service.UpdateAsync(
                    tenantId,
                    actorUserId,
                    actorPersonId,
                    partId,
                    request,
                    cancellationToken));
            });

            group.MapDelete("/{partId:guid}", async (
                Guid partId,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                MaintenancePartService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePartsArchive(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var actorPersonId = context.User.GetPersonId().ToString();
                return Results.Ok(await service.ArchiveAsync(
                    tenantId,
                    actorUserId,
                    actorPersonId,
                    partId,
                    cancellationToken));
            });
        }
    }
}
