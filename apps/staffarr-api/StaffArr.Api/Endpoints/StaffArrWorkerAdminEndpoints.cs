using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class StaffArrWorkerAdminEndpoints
{
    public static void MapStaffArrWorkerAdminEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/worker-admin/{workerKey}")
            .WithTags("StaffArrWorkerAdmin")
            .RequireAuthorization();

        group.MapGet("/settings", async (
            string workerKey,
            StaffArrAuthorizationService authorization,
            StaffArrWorkerAdminService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkerAdminSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSettingsAsync(workerKey, tenantId, cancellationToken));
        })
        .WithName("GetStaffArrWorkerAdminSettings");

        group.MapPut("/settings", async (
            string workerKey,
            UpsertStaffArrWorkerSettingsRequest request,
            StaffArrAuthorizationService authorization,
            StaffArrWorkerAdminService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkerAdminSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertSettingsAsync(
                workerKey,
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertStaffArrWorkerAdminSettings");

        group.MapGet("/pending", async (
            string workerKey,
            StaffArrAuthorizationService authorization,
            StaffArrWorkerAdminService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkerAdminSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListPendingPreviewAsync(workerKey, tenantId, cancellationToken));
        })
        .WithName("ListStaffArrWorkerAdminPending");

        group.MapGet("/runs", async (
            string workerKey,
            int? limit,
            StaffArrAuthorizationService authorization,
            StaffArrWorkerAdminService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireWorkerAdminSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListRecentRunsAsync(workerKey, tenantId, limit, cancellationToken));
        })
        .WithName("ListStaffArrWorkerAdminRuns");
    }
}
