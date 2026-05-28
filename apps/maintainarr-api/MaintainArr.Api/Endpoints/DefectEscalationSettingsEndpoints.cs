using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class DefectEscalationSettingsEndpoints
{
    public static void MapMaintainArrDefectEscalationSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/defect-escalation-settings")
            .WithTags("DefectEscalationSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            MaintainArrAuthorizationService authorization,
            DefectEscalationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectEscalationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetMaintainArrDefectEscalationSettings");

        group.MapPut("/", async (
            UpsertDefectEscalationSettingsRequest request,
            MaintainArrAuthorizationService authorization,
            DefectEscalationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectEscalationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpsertMaintainArrDefectEscalationSettings");

        group.MapGet("/pending", async (
            MaintainArrAuthorizationService authorization,
            DefectEscalationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectEscalationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await workerService.ListPendingAsync(tenantId, null, 25, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListMaintainArrPendingDefectEscalations");

        group.MapGet("/runs", async (
            int? limit,
            MaintainArrAuthorizationService authorization,
            DefectEscalationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectEscalationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListMaintainArrDefectEscalationRuns");

        group.MapGet("/events", async (
            int? limit,
            MaintainArrAuthorizationService authorization,
            DefectEscalationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDefectEscalationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentEventsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListMaintainArrDefectEscalationEvents");
    }
}
