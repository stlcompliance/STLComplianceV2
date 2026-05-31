using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string IncidentRemediationIngestActionScope = "trainarr.incident_remediations.write";
    public const string IncidentRemediationReadActionScope = "trainarr.incident_remediations.read";
    public const string RoutarrIncidentRemediationIngestActionScope = "trainarr.routarr_incident_remediations.write";
    public const string SupplyarrIncidentRemediationIngestActionScope = "trainarr.supplyarr_incident_remediations.write";

    public const string RoutarrQualificationCheckActionScope = "trainarr.qualification_checks.dispatch";
    public const string QualificationCheckReadActionScope = "trainarr.qualification_checks.read";

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

            integrations.MapGet("/incident-remediations/{remediationId:guid}", async (
            Guid remediationId,
            Guid tenantId,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            StaffarrIncidentRemediationQueryService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "staffarr",
                    RequiredTargetProduct = "trainarr",
                    TenantId = tenantId,
                    RequiredActionScope = IncidentRemediationReadActionScope
                });

            var result = await service.GetAsync(tenantId, remediationId, cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"GetStaffarrIncidentRemediationIntegration{(route.Contains("/v1/") ? "V1" : string.Empty)}");

            integrations.MapPost("/routarr-incident-remediations", async (
            IngestRoutarrIncidentRemediationRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            StaffarrIncidentRemediationService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "routarr",
                    RequiredTargetProduct = "trainarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = RoutarrIncidentRemediationIngestActionScope
                });

            var result = await service.IngestRoutarrAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"IngestRoutarrIncidentRemediation{(route.Contains("/v1/") ? "V1" : string.Empty)}");

            integrations.MapPost("/supplyarr-incident-remediations", async (
            IngestSupplyarrIncidentRemediationRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            StaffarrIncidentRemediationService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "supplyarr",
                    RequiredTargetProduct = "trainarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = SupplyarrIncidentRemediationIngestActionScope
                });

            var result = await service.IngestSupplyarrAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"IngestSupplyarrIncidentRemediation{(route.Contains("/v1/") ? "V1" : string.Empty)}");

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

            integrations.MapPost("/qualification-check", async (
            RoutarrQualificationCheckRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            QualificationCheckService service,
            CancellationToken cancellationToken) =>
        {
            ValidateQualificationCheckServiceToken(tokenValidator, context, request.TenantId);

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
        .WithName($"QualificationCheckIntegration{(route.Contains("/v1/") ? "V1" : string.Empty)}");

            integrations.MapPost("/qualification-check/batch", async (
            CreateIntegrationBatchQualificationCheckRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            QualificationCheckService service,
            CancellationToken cancellationToken) =>
        {
            ValidateQualificationCheckServiceToken(tokenValidator, context, request.TenantId);

            var result = await service.CheckBatchAsync(
                request.TenantId,
                actorUserId: null,
                new CreateBatchQualificationCheckRequest(
                    request.QualificationKey,
                    request.RulePackKey,
                    request.Subjects,
                    request.EffectiveAt,
                    request.TrainingDefinitionId,
                    request.TrainingProgramId),
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName($"BatchQualificationCheckIntegration{(route.Contains("/v1/") ? "V1" : string.Empty)}");

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

    private static void ValidateQualificationCheckServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());
        var preview = tokenValidator.TryValidate(bearer)
            ?? throw new StlApiException(
                "auth.service_token_invalid",
                "Service token is invalid.",
                401);

        var source = preview.SourceProductKey?.Trim().ToLowerInvariant();
        if (source is not "maintainarr" and not "routarr" and not "supplyarr" and not "staffarr" and not "compliancecore")
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for qualification checks.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = source,
                RequiredTargetProduct = "trainarr",
                TenantId = tenantId,
                RequiredActionScope = QualificationCheckReadActionScope
            });
    }
}
