using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string IncidentRemediationIngestActionScope = "trainarr.incident_remediations.write";

    public const string RoutarrQualificationCheckActionScope = "trainarr.qualification_checks.dispatch";

    public static void MapTrainArrIntegrationEndpoints(this WebApplication app)
    {
        var integrations = app.MapGroup("/api/integrations").WithTags("Integrations");

        integrations.MapPost("/incident-remediations", async (
            IngestStaffarrIncidentRemediationRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            StaffarrIncidentRemediationService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "staffarr",
                    RequiredTargetProduct = "trainarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = IncidentRemediationIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestStaffarrIncidentRemediation");

        integrations.MapPost("/routarr-qualification-check", async (
            RoutarrQualificationCheckRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            IntegrationSettingsService integrationSettingsService,
            QualificationCheckService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "routarr",
                    RequiredTargetProduct = "trainarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = RoutarrQualificationCheckActionScope
                });

            await integrationSettingsService.EnsureRoutarrQualificationDispatchEnabledAsync(
                request.TenantId,
                cancellationToken);

            var result = await service.CheckAsync(
                request.TenantId,
                actorUserId: null,
                new CreateQualificationCheckRequest(
                    request.StaffarrPersonId,
                    request.QualificationKey,
                    request.RulePackKey,
                    request.Context),
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("RoutarrQualificationCheck");
    }
}
