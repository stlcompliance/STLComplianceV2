using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ProcurementExceptionEscalationSettingsEndpoints
{
    public static void MapSupplyArrProcurementExceptionEscalationSettingsEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
        {
        group = group.WithTags("ProcurementExceptionEscalationSettings").RequireAuthorization();

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
        .WithName($"GetSupplyArrProcurementExceptionEscalationSettings{nameSuffix}");

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
        .WithName($"UpsertSupplyArrProcurementExceptionEscalationSettings{nameSuffix}");

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
        .WithName($"ListSupplyArrPendingProcurementExceptionEscalations{nameSuffix}");

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
        .WithName($"ListSupplyArrProcurementExceptionEscalationRuns{nameSuffix}");

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
        .WithName($"ListSupplyArrProcurementExceptionEscalationEvents{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/procurement-exception-escalation-settings"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/procurement-exception-escalation-settings"), "V1");
    }
}
