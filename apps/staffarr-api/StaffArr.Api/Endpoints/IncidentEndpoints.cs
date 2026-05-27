using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class IncidentEndpoints
{
    public static void MapStaffArrIncidentEndpoints(this WebApplication app)
    {
        var incidents = app.MapGroup("/api/incidents")
            .WithTags("Incidents")
            .RequireAuthorization();

        incidents.MapGet("/", async (
            Guid? personId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentsRead(context.User, personId);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListIncidentsAsync(tenantId, personId, cancellationToken));
        })
        .WithName("ListPersonnelIncidents");

        incidents.MapPost("/", async (
            CreatePersonnelIncidentRequest request,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentsManageWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateIncidentAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/incidents/{created.IncidentId}", created);
        })
        .WithName("CreatePersonnelIncident");

        incidents.MapGet("/{incidentId:guid}", async (
            Guid incidentId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentService service,
            CancellationToken cancellationToken) =>
        {
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetIncidentAsync(tenantId, incidentId, cancellationToken);
            authorization.RequireIncidentsRead(context.User, detail.PersonId);
            return Results.Ok(detail);
        })
        .WithName("GetPersonnelIncident");

        incidents.MapPost("/{incidentId:guid}/route-to-trainarr", async (
            Guid incidentId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            IncidentRoutingService routingService,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentsManageWrite(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await routingService.RouteToTrainarrAsync(
                tenantId,
                actorUserId,
                incidentId,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("RoutePersonnelIncidentToTrainarr");
    }
}
