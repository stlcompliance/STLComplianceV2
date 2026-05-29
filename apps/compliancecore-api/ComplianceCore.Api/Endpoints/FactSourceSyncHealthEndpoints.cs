using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class FactSourceSyncHealthEndpoints
{
    public static void MapComplianceCoreFactSourceSyncHealthEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/fact-source-sync-health")
            .WithTags("FactSourceSyncHealth")
            .RequireAuthorization();

        api.MapGet("/", async (
            HttpContext context,
            ComplianceCoreAuthorizationService authorization,
            FactSourceSyncHealthService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireFactSourceSyncHealthRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetHealthAsync(tenantId, cancellationToken));
        })
        .WithName("GetComplianceCoreFactSourceSyncHealth");
    }
}
