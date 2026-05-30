using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class IncidentSupplyDemandEndpoints
{
    public static void MapStaffArrIncidentSupplyDemandEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/incidents/{incidentId:guid}/supply-demand", Suffix: string.Empty),
            (Route: "/api/v1/incidents/{incidentId:guid}/supply-demand", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route)
                .WithTags("IncidentSupplyDemand")
                .RequireAuthorization();

            group.MapGet("/", async (
                Guid incidentId,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService incidentService,
                IncidentSupplyDemandService supplyDemandService,
                CancellationToken cancellationToken) =>
            {
                var tenantId = context.User.GetTenantId();
                var detail = await incidentService.GetIncidentAsync(tenantId, incidentId, cancellationToken);
                authorization.RequireIncidentsRead(context.User, detail.PersonId);
                return Results.Ok(await supplyDemandService.ListAsync(tenantId, incidentId, cancellationToken));
            })
            .WithName($"ListIncidentSupplyDemand{suffix}");

            group.MapPost("/", async (
                Guid incidentId,
                CreateIncidentSupplyDemandLineRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService incidentService,
                IncidentSupplyDemandService supplyDemandService,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIncidentsManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await incidentService.GetIncidentAsync(tenantId, incidentId, cancellationToken);
                authorization.RequireIncidentsRead(context.User, detail.PersonId);
                var created = await supplyDemandService.CreateAsync(
                    tenantId,
                    actorUserId,
                    incidentId,
                    request,
                    cancellationToken);
                return Results.Created($"{route.Replace("{incidentId:guid}", incidentId.ToString())}/{created.DemandLineId}", created);
            })
            .WithName($"CreateIncidentSupplyDemandLine{suffix}");

            group.MapPost("/publish", async (
                Guid incidentId,
                PublishIncidentSupplyDemandRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                IncidentService incidentService,
                IncidentSupplyDemandService supplyDemandService,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireIncidentsManageWrite(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var detail = await incidentService.GetIncidentAsync(tenantId, incidentId, cancellationToken);
                authorization.RequireIncidentsRead(context.User, detail.PersonId);
                return Results.Ok(await supplyDemandService.PublishAsync(
                    tenantId,
                    actorUserId,
                    incidentId,
                    request,
                    cancellationToken));
            })
            .WithName($"PublishIncidentSupplyDemand{suffix}");
        }
    }
}
