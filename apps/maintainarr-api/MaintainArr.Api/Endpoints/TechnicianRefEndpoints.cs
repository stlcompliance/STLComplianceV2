using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class TechnicianRefEndpoints
{
    public static void MapMaintainArrTechnicianRefEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Route: "/api/technician-refs", Suffix: string.Empty),
            (Route: "/api/v1/technician-refs", Suffix: "V1")
        };

        foreach (var (route, suffix) in groups)
        {
            var group = app.MapGroup(route).WithTags("TechnicianRefs").RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                TechnicianRefService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireWorkOrdersRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
            })
            .WithName($"ListTechnicianRefs{suffix}");

            group.MapPut("/", async (
                UpsertTechnicianRefRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                TechnicianRefService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireWorkOrdersPerform(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                return Results.Ok(await service.UpsertAsync(tenantId, actorUserId, request, cancellationToken));
            })
            .WithName($"UpsertTechnicianRef{suffix}");
        }
    }
}
