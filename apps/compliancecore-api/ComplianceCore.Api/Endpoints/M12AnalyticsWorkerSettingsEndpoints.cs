using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class M12AnalyticsWorkerSettingsEndpoints
{
    public static void MapComplianceCoreM12AnalyticsWorkerSettingsEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/m12-analytics-worker-settings")
            .WithTags("M12AnalyticsWorkerSettings")
            .RequireAuthorization();

        api.MapGet("/", async (
            HttpContext context,
            ComplianceCoreAuthorizationService authorization,
            M12AnalyticsWorkerSettingsService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireM12AnalyticsWorkerSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, cancellationToken));
        })
        .WithName("GetComplianceCoreM12AnalyticsWorkerSettings");

        api.MapPut("/", async (
            UpsertM12AnalyticsWorkerSettingsRequest request,
            HttpContext context,
            ComplianceCoreAuthorizationService authorization,
            M12AnalyticsWorkerSettingsService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireM12AnalyticsWorkerSettingsManage(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("UpsertComplianceCoreM12AnalyticsWorkerSettings");
    }
}
