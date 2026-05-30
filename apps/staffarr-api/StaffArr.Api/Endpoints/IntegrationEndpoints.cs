using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string TrainingBlockerIngestActionScope = "staffarr.training_blockers.write";

    public const string CertificationGrantIngestActionScope = "staffarr.certification_grants.write";

    public const string CertificationLifecycleIngestActionScope = "staffarr.certification_lifecycle.write";

    public const string RoutarrReadinessDispatchActionScope = "staffarr.readiness.dispatch_gate";

    public const string TrainarrPersonLookupActionScope = "staffarr.person.lookup";

    public const string MaintainarrPersonLookupActionScope = "staffarr.person.lookup";

    public const string TrainarrPersonHistoryReadActionScope = PersonnelHistoryService.IntegrationReadActionScope;

    public const string SupplyarrDemandStatusIngestActionScope = "staffarr.demand_status.write";

    public const string SupplyarrProcurementApprovalAuthorityReadActionScope =
        ProcurementApprovalAuthorityService.ReadAuthorityActionScope;

    public const string TrainingAcknowledgementIngestActionScope = "staffarr.training_acknowledgements.write";

    public const string TrainingAcknowledgementReadActionScope = "staffarr.training_acknowledgements.read";
    public const string PermissionCheckReadActionScope = "staffarr.permission_check.read";

    public static void MapStaffArrIntegrationEndpoints(this WebApplication app)
    {
        var integrations = app.MapGroup("/api/integrations").WithTags("Integrations");
        var integrationsV1 = app.MapGroup("/api/v1/integrations").WithTags("Integrations");

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
            ValidatePersonLookupServiceToken(tokenValidator, context, tenantId);

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

        integrations.MapPost("/supplyarr-demand-status", async (
            IngestSupplyarrDemandStatusRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            IncidentSupplyDemandStatusIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "supplyarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = SupplyarrDemandStatusIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestSupplyarrDemandStatus");

        integrations.MapPost("/training-acknowledgements", async (
            IngestTrainingAcknowledgementRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TrainingAcknowledgementIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = TrainingAcknowledgementIngestActionScope
                });

            return Results.Ok(await service.IngestAsync(request, cancellationToken));
        })
        .WithName("IngestTrainingAcknowledgement");

        integrations.MapPost("/training-acknowledgements/supersede", async (
            SupersedeTrainingAcknowledgementRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TrainingAcknowledgementIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = TrainingAcknowledgementIngestActionScope
                });

            return Results.Ok(await service.SupersedeAsync(request, cancellationToken));
        })
        .WithName("SupersedeTrainingAcknowledgement");

        integrations.MapGet("/training-acknowledgements/status", async (
            Guid tenantId,
            Guid trainarrAcknowledgementRequestId,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TrainingAcknowledgementIngestionService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = tenantId,
                    RequiredActionScope = TrainingAcknowledgementReadActionScope
                });

            var status = await service.GetStatusAsync(tenantId, trainarrAcknowledgementRequestId, cancellationToken);
            return status is null ? Results.NotFound() : Results.Ok(status);
        })
        .WithName("GetTrainingAcknowledgementStatus");

        integrations.MapGet("/procurement-approval-authority", async (
            Guid tenantId,
            Guid? personId,
            Guid? externalUserId,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ProcurementApprovalAuthorityService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "supplyarr",
                    RequiredTargetProduct = "staffarr",
                    TenantId = tenantId,
                    RequiredActionScope = SupplyarrProcurementApprovalAuthorityReadActionScope
                });

            if (personId is null && externalUserId is null)
            {
                return Results.BadRequest(new
                {
                    code = "procurement_approval_authority.validation",
                    message = "Provide personId or externalUserId query parameter."
                });
            }

            if (personId is Guid requestedPersonId)
            {
                return Results.Ok(await service.GetByPersonIdAsync(tenantId, requestedPersonId, cancellationToken));
            }

            return Results.Ok(await service.GetByExternalUserIdAsync(tenantId, externalUserId!.Value, cancellationToken));
        })
        .WithName("SupplyarrProcurementApprovalAuthority");

        integrations.MapGet("/permission-check", CheckPermissionsAsync)
        .WithName("IntegrationPermissionCheck");

        integrationsV1.MapGet("/permission-check", CheckPermissionsAsync)
        .WithName("IntegrationPermissionCheckV1");
    }

    private static async Task<IResult> CheckPermissionsAsync(
        Guid tenantId,
        Guid personId,
        string[] permissionKey,
        HttpContext context,
        StlServiceTokenValidator tokenValidator,
        IntegrationPermissionCheckService service,
        CancellationToken cancellationToken)
    {
        ValidatePermissionCheckServiceToken(tokenValidator, context, tenantId);

        if (personId == Guid.Empty)
        {
            return Results.BadRequest(new
            {
                code = "permission_check.validation",
                message = "personId must be a valid identifier."
            });
        }

        var result = await service.CheckAsync(
            tenantId,
            personId,
            permissionKey,
            cancellationToken);
        return Results.Ok(result);
    }

    private static void ValidatePersonLookupServiceToken(
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
        if (source is not "trainarr" and not "maintainarr")
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for person lookup.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = source,
                RequiredTargetProduct = "staffarr",
                TenantId = tenantId,
                RequiredActionScope = TrainarrPersonLookupActionScope
            });
    }

    private static void ValidatePermissionCheckServiceToken(
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
        if (source is not "maintainarr" and not "routarr" and not "supplyarr" and not "trainarr" and not "compliancecore")
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for permission checks.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = source,
                RequiredTargetProduct = "staffarr",
                TenantId = tenantId,
                RequiredActionScope = PermissionCheckReadActionScope
            });
    }
}
