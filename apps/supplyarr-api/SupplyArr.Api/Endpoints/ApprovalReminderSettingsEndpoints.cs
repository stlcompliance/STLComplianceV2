using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ApprovalReminderSettingsEndpoints
{
    public static void MapSupplyArrApprovalReminderSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/approval-reminder-settings")
            .WithTags("ApprovalReminderSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            SupplyArrAuthorizationService authorization,
            ApprovalReminderSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireApprovalReminderSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetSupplyArrApprovalReminderSettings");

        group.MapPut("/", async (
            UpsertApprovalReminderSettingsRequest request,
            SupplyArrAuthorizationService authorization,
            ApprovalReminderSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireApprovalReminderSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("UpsertSupplyArrApprovalReminderSettings");

        group.MapGet("/pending", async (
            SupplyArrAuthorizationService authorization,
            ApprovalReminderWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireApprovalReminderSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListPendingAsync(tenantId, null, 25, cancellationToken));
        })
        .WithName("ListSupplyArrPendingApprovalReminders");

        group.MapGet("/runs", async (
            int? limit,
            SupplyArrAuthorizationService authorization,
            ApprovalReminderWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireApprovalReminderSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListSupplyArrApprovalReminderRuns");
    }
}
