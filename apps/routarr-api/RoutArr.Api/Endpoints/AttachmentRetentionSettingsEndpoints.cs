using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class AttachmentRetentionSettingsEndpoints
{
    public static void MapRoutArrAttachmentRetentionSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/attachment-retention-settings")
            .WithTags("AttachmentRetentionSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            RoutArrAuthorizationService authorization,
            AttachmentRetentionSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAttachmentRetentionSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetRoutArrAttachmentRetentionSettings");

        group.MapPut("/", async (
            UpsertAttachmentRetentionSettingsRequest request,
            RoutArrAuthorizationService authorization,
            AttachmentRetentionSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAttachmentRetentionSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var result = await settingsService.UpsertAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("UpsertRoutArrAttachmentRetentionSettings");

        group.MapGet("/runs", async (
            int? limit,
            RoutArrAuthorizationService authorization,
            AttachmentRetentionWorkerService workerService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAttachmentRetentionSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await workerService.ListRecentRunsAsync(tenantId, limit, cancellationToken));
        })
        .WithName("ListRoutArrAttachmentRetentionRuns");
    }
}
