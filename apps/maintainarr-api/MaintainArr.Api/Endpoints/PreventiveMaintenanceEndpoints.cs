using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class PreventiveMaintenanceEndpoints
{
    public static void MapMaintainArrPreventiveMaintenanceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/preventive-maintenance").WithTags("PreventiveMaintenance").RequireAuthorization();

        group.MapGet("/schedules", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            PmScheduleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName("ListPmSchedules");

        group.MapGet("/due", async (
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            PmScheduleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListDueAsync(tenantId, cancellationToken));
        })
        .WithName("ListDuePmSchedules");

        group.MapGet("/schedules/{pmScheduleId:guid}", async (
            Guid pmScheduleId,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            PmScheduleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, pmScheduleId, cancellationToken));
        })
        .WithName("GetPmSchedule");

        group.MapPost("/schedules", async (
            CreatePmScheduleRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            PmScheduleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var created = await service.CreateAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Created($"/api/preventive-maintenance/schedules/{created.PmScheduleId}", created);
        })
        .WithName("CreatePmSchedule");

        group.MapPut("/schedules/{pmScheduleId:guid}", async (
            Guid pmScheduleId,
            UpdatePmScheduleRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            PmScheduleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateAsync(tenantId, actorUserId, pmScheduleId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdatePmSchedule");

        group.MapPatch("/schedules/{pmScheduleId:guid}/status", async (
            Guid pmScheduleId,
            UpdatePmScheduleStatusRequest request,
            HttpContext context,
            MaintainArrAuthorizationService authorization,
            PmScheduleService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var updated = await service.UpdateStatusAsync(tenantId, actorUserId, pmScheduleId, request, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("UpdatePmScheduleStatus");
    }
}
