using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string TrainingBlockerIngestActionScope = "staffarr.training_blockers.write";

    public const string CertificationGrantIngestActionScope = "staffarr.certification_grants.write";

    public const string CertificationLifecycleIngestActionScope = "staffarr.certification_lifecycle.write";

    public const string RoutarrReadinessDispatchActionScope = "staffarr.readiness.dispatch_gate";

    public const string TrainarrPersonLookupActionScope = "staffarr.person.lookup";

    public const string TrainarrPersonHistoryReadActionScope = PersonnelHistoryService.IntegrationReadActionScope;

    public static void MapStaffArrIntegrationEndpoints(this WebApplication app)
    {
        var integrations = app.MapGroup("/api/integrations").WithTags("Integrations");

        integrations.MapPost("/training-blockers", async (
            IngestTrainingBlockerRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TrainingBlockerIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = TrainingBlockerIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestTrainingBlocker");

        integrations.MapPost("/training-blockers/clear", async (
            ClearTrainingBlockerRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TrainingBlockerIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = TrainingBlockerIngestActionScope
                });

            var result = await service.ClearAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ClearTrainingBlocker");

        integrations.MapPost("/certification-grants", async (
            IngestCertificationGrantRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            CertificationGrantIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = CertificationGrantIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestCertificationGrant");

        integrations.MapPost("/certification-lifecycle", async (
            IngestCertificationLifecycleRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            CertificationLifecycleIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = CertificationLifecycleIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestCertificationLifecycle");

        integrations.MapGet("/routarr-readiness", async (
            Guid tenantId,
            Guid personId,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ReadinessService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "routarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = tenantId,
                    RequiredActionScope = RoutarrReadinessDispatchActionScope
                });

            var result = await service.GetPersonReadinessAsync(tenantId, personId, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("RoutarrReadinessCheck");

        integrations.MapGet("/person-lookup", async (
            Guid tenantId,
            Guid? personId,
            string? email,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PersonLookupService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = tenantId,
                    RequiredActionScope = TrainarrPersonLookupActionScope
                });

            if (personId is null && string.IsNullOrWhiteSpace(email))
            {
                return Results.BadRequest(new
                {
                    code = "person_lookup.validation",
                    message = "Provide personId or email query parameter."
                });
            }

            if (personId is Guid requestedPersonId)
            {
                if (requestedPersonId == Guid.Empty)
                {
                    return Results.BadRequest(new
                    {
                        code = "person_lookup.validation",
                        message = "personId must be a valid identifier."
                    });
                }

                return Results.Ok(await service.GetByPersonIdAsync(tenantId, requestedPersonId, cancellationToken));
            }

            return Results.Ok(await service.GetByEmailAsync(tenantId, email!, cancellationToken));
        })
        .WithName("TrainarrPersonLookup");

        integrations.MapGet("/person-history", async (
            Guid tenantId,
            Guid personId,
            int? page,
            int? pageSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PersonnelHistoryService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = tenantId,
                    RequiredActionScope = TrainarrPersonHistoryReadActionScope
                });

            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new
                {
                    code = "person_history.validation",
                    message = "personId must be a valid identifier."
                });
            }

            return Results.Ok(await service.ListPersonHistoryAsync(
                tenantId,
                personId,
                page ?? 1,
                pageSize ?? 50,
                cancellationToken));
        })
        .WithName("TrainarrPersonHistory");

        integrations.MapGet("/person-history/summary", async (
            Guid tenantId,
            Guid personId,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PersonnelHistoryService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = tenantId,
                    RequiredActionScope = TrainarrPersonHistoryReadActionScope
                });

            if (personId == Guid.Empty)
            {
                return Results.BadRequest(new
                {
                    code = "person_history.validation",
                    message = "personId must be a valid identifier."
                });
            }

            return Results.Ok(await service.GetSummaryAsync(tenantId, personId, cancellationToken));
        })
        .WithName("TrainarrPersonHistorySummary");
    }
}
