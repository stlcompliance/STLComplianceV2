using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ProcurementExceptionEscalationSettingsEndpoints
{
    public static void MapSupplyArrProcurementExceptionEscalationSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/procurement-exception-escalation-settings")
            .WithTags("ProcurementExceptionEscalationSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionEscalationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementExceptionEscalationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetSupplyArrProcurementExceptionEscalationSettings");

        group.MapPut("/", async (
            UpsertProcurementExceptionEscalationSettingsRequest request,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionEscalationSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementExceptionEscalationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertSupplyArrProcurementExceptionEscalationSettings");

        group.MapGet("/pending", async (
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionEscalationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementExceptionEscalationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListPendingAsync(tenantId, null, 25, cancellationToken));
        })
        .WithName("ListSupplyArrPendingProcurementExceptionEscalations");

        group.MapGet("/runs", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionEscalationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementExceptionEscalationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListSupplyArrProcurementExceptionEscalationRuns");

        group.MapGet("/events", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            ProcurementExceptionEscalationWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireProcurementExceptionEscalationSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentEventsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListSupplyArrProcurementExceptionEscalationEvents");
    }
}
