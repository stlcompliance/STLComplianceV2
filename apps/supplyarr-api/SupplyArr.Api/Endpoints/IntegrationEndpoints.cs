using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Endpoints;

public static class IntegrationEndpoints
{
    public const string MaintainarrDemandIngestActionScope = "supplyarr.demand_intake.write";

    public const string RoutarrDemandIngestActionScope = "supplyarr.demand_intake.write";

    public const string TrainarrDemandIngestActionScope = "supplyarr.demand_intake.write";

    public const string StaffarrDemandIngestActionScope = "supplyarr.demand_intake.write";

    public static void MapSupplyArrIntegrationEndpoints(this WebApplication app)
    {
        var integrations = app.MapGroup("/api/integrations").WithTags("Integrations");

        integrations.MapPost("/maintainarr-demand", async (
            IngestMaintainarrDemandRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            MaintainArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "maintainarr",
                    RequiredTargetProduct = "supplyarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = MaintainarrDemandIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestMaintainarrDemand");

        integrations.MapPost("/routarr-demand", async (
            IngestRoutarrDemandRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            RoutArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "routarr",
                    RequiredTargetProduct = "supplyarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = RoutarrDemandIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestRoutarrDemand");

        integrations.MapPost("/trainarr-demand", async (
            IngestTrainarrDemandRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TrainArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "trainarr",
                    RequiredTargetProduct = "supplyarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = TrainarrDemandIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestTrainarrDemand");

        integrations.MapPost("/staffarr-demand", async (
            IngestStaffarrDemandRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            StaffArrDemandIntakeService service,
            CancellationToken cancellationToken) =>
        {
            tokenValidator.ValidateOrThrow(
                ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization.ToString()),
                new ServiceTokenRequirements
                {
                    ExpectedSourceProduct = "staffarr",
                    RequiredTargetProduct = "supplyarr",
                    TenantId = request.TenantId,
                    RequiredActionScope = StaffarrDemandIngestActionScope
                });

            var result = await service.IngestAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("IngestStaffarrDemand");

        integrations.MapGet("/part-supply-readiness", async (
            Guid tenantId,
            Guid partId,
            decimal? quantity,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            SupplyReadinessService service,
            CancellationToken cancellationToken) =>
        {
            ValidateReadinessServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.GetPartReadinessAsync(tenantId, partId, quantity, cancellationToken));
        })
        .WithName("IntegrationGetPartSupplyReadiness");

        integrations.MapGet("/vendor-supply-readiness", async (
            Guid tenantId,
            Guid externalPartyId,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            SupplyReadinessService service,
            CancellationToken cancellationToken) =>
        {
            ValidateReadinessServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.GetVendorReadinessAsync(tenantId, externalPartyId, cancellationToken));
        })
        .WithName("IntegrationGetVendorSupplyReadiness");

        integrations.MapGet("/procurement-path-readiness", async (
            Guid tenantId,
            Guid partId,
            Guid externalPartyId,
            decimal? quantity,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            SupplyReadinessService service,
            CancellationToken cancellationToken) =>
        {
            ValidateReadinessServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.GetProcurementPathReadinessAsync(
                tenantId,
                partId,
                externalPartyId,
                quantity,
                cancellationToken));
        })
        .WithName("IntegrationGetProcurementPathReadiness");

        integrations.MapGet("/references/{referenceType}/{referenceId:guid}", async (
            Guid tenantId,
            string referenceType,
            Guid referenceId,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            SupplyReferenceResolutionService service,
            CancellationToken cancellationToken) =>
        {
            ValidateReferenceReadServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.ResolveByIdAsync(
                tenantId,
                referenceType,
                referenceId,
                cancellationToken));
        })
        .WithName("IntegrationResolveSupplyReferenceById");

        integrations.MapGet("/references/by-key", async (
            Guid tenantId,
            string referenceType,
            string referenceKey,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            SupplyReferenceResolutionService service,
            CancellationToken cancellationToken) =>
        {
            ValidateReferenceReadServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.ResolveByKeyAsync(
                tenantId,
                referenceType,
                referenceKey,
                cancellationToken));
        })
        .WithName("IntegrationResolveSupplyReferenceByKey");
    }

    public const string SupplyReadinessReadActionScope = "supplyarr.readiness.read";

    public const string SupplyReferenceReadActionScope = "supplyarr.references.read";

    private static void ValidateReadinessServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        var preview = tokenValidator.TryValidate(bearer);
        if (preview is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        var allowedSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "maintainarr",
            "routarr",
        };

        if (!allowedSources.Contains(preview.SourceProductKey))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for supply readiness reads.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = preview.SourceProductKey,
                RequiredTargetProduct = "supplyarr",
                TenantId = tenantId,
                RequiredActionScope = SupplyReadinessReadActionScope,
            });
    }

    private static void ValidateReferenceReadServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        var preview = tokenValidator.TryValidate(bearer);
        if (preview is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        var allowedSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "maintainarr",
            "routarr",
            "staffarr",
            "trainarr",
            "compliancecore",
        };

        if (!allowedSources.Contains(preview.SourceProductKey))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for SupplyArr reference resolution.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = preview.SourceProductKey,
                RequiredTargetProduct = "supplyarr",
                TenantId = tenantId,
                RequiredActionScope = SupplyReferenceReadActionScope,
            });
    }
}
