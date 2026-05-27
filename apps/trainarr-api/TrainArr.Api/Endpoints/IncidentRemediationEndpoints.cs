using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class IncidentRemediationEndpoints
{
    public static void MapTrainArrIncidentRemediationEndpoints(this WebApplication app)
    {
        var remediations = app.MapGroup("/api/incident-remediations")
            .WithTags("IncidentRemediations")
            .RequireAuthorization();

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
        .WithName("ListStaffarrIncidentRemediations");

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
        .WithName("GetStaffarrIncidentRemediation");
    }
}
