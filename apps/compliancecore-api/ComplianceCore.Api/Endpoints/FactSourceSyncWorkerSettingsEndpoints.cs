using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class FactSourceSyncWorkerSettingsEndpoints
{
    public static void MapComplianceCoreFactSourceSyncWorkerSettingsEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/fact-source-sync-worker-settings")
            .WithTags("FactSourceSyncWorkerSettings")
            .RequireAuthorization();

        api.MapGet("/", async (
            HttpContext context,
            ComplianceCoreAuthorizationService authorization,
            FactSourceSyncWorkerSettingsService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFactSourceSyncWorkerSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetComplianceCoreFactSourceSyncWorkerSettings");

        api.MapPut("/", async (
            UpsertFactSourceSyncWorkerSettingsRequest request,
            HttpContext context,
            ComplianceCoreAuthorizationService authorization,
            FactSourceSyncWorkerSettingsService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFactSourceSyncWorkerSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("UpsertComplianceCoreFactSourceSyncWorkerSettings");
    }
}
