using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class TechnicianRefEndpoints
{
    public static void MapMaintainArrTechnicianRefEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/technician-refs").WithTags("TechnicianRefs").RequireAuthorization();

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
        .WithName("ListTechnicianRefs");

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
        .WithName("UpsertTechnicianRef");
    }
}
