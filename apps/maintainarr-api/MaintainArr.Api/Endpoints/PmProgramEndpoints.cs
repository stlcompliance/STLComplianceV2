using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class PmProgramEndpoints
{
    public static void MapMaintainArrPmProgramEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/preventive-maintenance/programs"),
            app.MapGroup("/api/v1/preventive-maintenance/programs"),
            app.MapGroup("/api/v1/pm-programs"),
        };

        foreach (var group in groups)
        {
            group.WithTags("PmPrograms").RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
            });

            group.MapGet("/{pmProgramId:guid}", async (
                Guid pmProgramId,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmRead(context.User);
                var tenantId = context.User.GetTenantId();
                return Results.Ok(await service.GetAsync(tenantId, pmProgramId, cancellationToken));
            });

            group.MapPost("/", async (
                CreatePmProgramRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
                return Results.Created($"/api/preventive-maintenance/programs/{created.PmProgramId}", created);
            });

            group.MapPut("/{pmProgramId:guid}", async (
                Guid pmProgramId,
                UpdatePmProgramRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var updated = await service.UpdateAsync(tenantId, actorUserId, pmProgramId, request, cancellationToken);
                return Results.Ok(updated);
            });

            group.MapPatch("/{pmProgramId:guid}/status", async (
                Guid pmProgramId,
                UpdatePmProgramStatusRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var updated = await service.UpdateStatusAsync(tenantId, actorUserId, pmProgramId, request, cancellationToken);
                return Results.Ok(updated);
            });

            group.MapPut("/{pmProgramId:guid}/schedules", async (
                Guid pmProgramId,
                ReplacePmProgramSchedulesRequest request,
                HttpContext context,
                MaintainArrAuthorizationService authorization,
                PmProgramService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequirePmManage(context.User);
                var tenantId = context.User.GetTenantId();
                var actorUserId = context.User.GetUserId();
                var updated = await service.ReplaceSchedulesAsync(tenantId, actorUserId, pmProgramId, request, cancellationToken);
                return Results.Ok(updated);
            });
        }
    }
}
