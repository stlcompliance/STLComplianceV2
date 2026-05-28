using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class LoadTestJourneySeedEndpoints
{
    public static void MapComplianceCoreLoadTestJourneySeedEndpoints(this WebApplication app)
    {
        app.MapPost("/api/load-test-journey/seed", async (
            ComplianceCoreAuthorizationService authorization,
            LoadTestJourneySeedService seedService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksCreate(context.User);
            authorization.RequireWorkflowGatesManage(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await seedService.EnsureSeededAsync(
                tenantId,
                context.User.GetUserId(),
                cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("LoadTestJourney")
        .RequireAuthorization()
        .WithName("SeedLoadTestJourney");
    }
}
