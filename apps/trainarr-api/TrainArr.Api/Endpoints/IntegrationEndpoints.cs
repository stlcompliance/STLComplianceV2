using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string IncidentRemediationIngestActionScope = "trainarr.incident_remediations.write";

    public const string RoutarrQualificationCheckActionScope = "trainarr.qualification_checks.dispatch";

    public const string SupplyarrDemandStatusIngestActionScope = "trainarr.demand_status.write";

    public const string PersonTrainingHistoryReadActionScope = PersonTrainingHistoryService.IntegrationReadActionScope;

    public static void MapTrainArrIntegrationEndpoints(this WebApplication app)
    {
        var routes = new[] { "/api/integrations", "/api/v1/integrations" };
        foreach (var route in routes)
        {
            var integrations = app.MapGroup(route).WithTags("Integrations");

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
        .WithName($"IngestStaffarrIncidentRemediation{(route.Contains("/v1/") ? "V1" : string.Empty)}");

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
        .WithName($"RoutarrQualificationCheck{(route.Contains("/v1/") ? "V1" : string.Empty)}");

            integrations.MapPost("/supplyarr-demand-status", async (
            IngestSupplyarrDemandStatusRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TrainingAssignmentMaterialDemandStatusIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "supplyarr",
                    RequiredTargetProduct = "trainarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = SupplyarrDemandStatusIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"IngestSupplyarrDemandStatus{(route.Contains("/v1/") ? "V1" : string.Empty)}");

            integrations.MapGet("/person-training-history", async (
            Guid tenantId,
            Guid staffarrPersonId,
            int? limit,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PersonTrainingHistoryService historyService,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "staffarr",
                    RequiredTargetProduct = "trainarr",
                    TenantId = tenantId,
                    RequiredActionScope = PersonTrainingHistoryReadActionScope
                });

            return Results.Ok(await historyService.GetForPersonAsync(
                tenantId,
                staffarrPersonId,
                limit,
                cancellationToken));
        })
        .WithName($"GetTrainArrPersonTrainingHistoryIntegration{(route.Contains("/v1/") ? "V1" : string.Empty)}");
        }
    }
}
