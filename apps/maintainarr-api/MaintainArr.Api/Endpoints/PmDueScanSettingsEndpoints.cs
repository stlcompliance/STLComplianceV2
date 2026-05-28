using MaintainArr.Api.Contracts;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class PmDueScanSettingsEndpoints
{
    public static void MapMaintainArrPmDueScanSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/pm-due-scan-settings")
            .WithTags("PmDueScanSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            MaintainArrAuthorizationService authorization,
            PmDueScanSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmDueScanSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetMaintainArrPmDueScanSettings");

        group.MapPut("/", async (
            UpsertPmDueScanSettingsRequest request,
            MaintainArrAuthorizationService authorization,
            PmDueScanSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmDueScanSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("UpsertMaintainArrPmDueScanSettings");

        group.MapGet("/pending", async (
            MaintainArrAuthorizationService authorization,
            PmDueScanService workerService,
            PmDueScanSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmDueScanSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var snapshot = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);
            var batchSize = snapshot?.BatchSize ?? PmDueScanSettingsDefaults.BatchSize;
            return Results.Ok(await workerService.ListPendingAsync(tenantId, null, batchSize, cancellationToken));
        })
        .WithName("ListMaintainArrPendingPmDueScan");

        group.MapGet("/runs", async (
            int? limit,
            MaintainArrAuthorizationService authorization,
            PmDueScanService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmDueScanSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListMaintainArrPmDueScanRuns");

        group.MapPost("/trigger", async (
            MaintainArrAuthorizationService authorization,
            PmDueScanSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequirePmDueScanSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.TriggerManualScanAsync(tenantId, actorUserId, cancellationToken));
        })
        .WithName("TriggerMaintainArrPmDueScan");
    }
}
