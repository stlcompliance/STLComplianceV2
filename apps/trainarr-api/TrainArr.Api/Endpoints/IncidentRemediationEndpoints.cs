using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class IncidentRemediationEndpoints
{
    public static void MapTrainArrIncidentRemediationEndpoints(this WebApplication app)
    {
        MapRoutes(
            app.MapGroup("/api/incident-remediations")
                .WithTags("IncidentRemediations")
                .RequireAuthorization(),
            string.Empty);
        MapRoutes(
            app.MapGroup("/api/v1/incident-remediations")
                .WithTags("IncidentRemediations")
                .RequireAuthorization(),
            "V1IncidentRemediations");
        MapRoutes(
            app.MapGroup("/api/v1/remediation")
                .WithTags("IncidentRemediations")
                .RequireAuthorization(),
            "V1Remediation");
    }

    private static void MapRoutes(RouteGroupBuilder remediations, string nameSuffix)
    {
        remediations.MapGet("/", async (
            string? status,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            StaffarrIncidentRemediationQueryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentRemediationsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, status, cancellationToken));
        })
        .WithName($"ListStaffarrIncidentRemediations{nameSuffix}");

        remediations.MapGet("/{remediationId:guid}", async (
            Guid remediationId,
            HttpContext context,
            TrainArrAuthorizationService authorization,
            StaffarrIncidentRemediationQueryService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireIncidentRemediationsRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetAsync(tenantId, remediationId, cancellationToken));
        })
        .WithName($"GetStaffarrIncidentRemediation{nameSuffix}");
    }
}
