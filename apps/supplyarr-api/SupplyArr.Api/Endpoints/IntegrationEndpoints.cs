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
        static void MapRoutes(RouteGroupBuilder integrations, string nameSuffix)
        {
            integrations = integrations.WithTags("Integrations");

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
        .WithName($"IngestMaintainarrDemand{nameSuffix}");

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
        .WithName($"IngestRoutarrDemand{nameSuffix}");

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
        .WithName($"IngestTrainarrDemand{nameSuffix}");

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
        .WithName($"IngestStaffarrDemand{nameSuffix}");

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
            return Results.Ok(await service.GetPartReadinessAsync(
                tenantId,
                partId,
                quantity,
                cancellationToken,
                auditSnapshotKind: SupplyReadinessService.PartSnapshotKind));
        })
        .WithName($"IntegrationGetPartSupplyReadiness{nameSuffix}");

        integrations.MapGet("/vendor-supply-readiness", async (
            Guid tenantId,
            Guid externalPartyId,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            SupplyReadinessService service,
            CancellationToken cancellationToken) =>
        {
            ValidateReadinessServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.GetVendorReadinessAsync(
                tenantId,
                externalPartyId,
                cancellationToken,
                auditSnapshotKind: SupplyReadinessService.VendorSnapshotKind));
        })
        .WithName($"IntegrationGetVendorSupplyReadiness{nameSuffix}");

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
                cancellationToken,
                auditSnapshotKind: SupplyReadinessService.ProcurementPathSnapshotKind));
        })
        .WithName($"IntegrationGetProcurementPathReadiness{nameSuffix}");

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
        .WithName($"IntegrationResolveSupplyReferenceById{nameSuffix}");

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
        .WithName($"IntegrationResolveSupplyReferenceByKey{nameSuffix}");

        integrations.MapGet("/vendor-orders/{vendorOrderId:guid}", async (
            Guid tenantId,
            Guid vendorOrderId,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            VendorOrderService service,
            CancellationToken cancellationToken) =>
        {
            ValidateVendorOrderReadServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.GetForIntegrationAsync(tenantId, vendorOrderId, cancellationToken));
        })
        .WithName($"IntegrationGetVendorOrder{nameSuffix}");

        integrations.MapGet("/vendor-orders/search", async (
            Guid tenantId,
            Guid? brokerOrderId,
            Guid? vendorId,
            string? status,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            VendorOrderService service,
            CancellationToken cancellationToken) =>
        {
            ValidateVendorOrderReadServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.SearchForIntegrationAsync(
                tenantId,
                brokerOrderId,
                vendorId,
                status,
                cancellationToken));
        })
        .WithName($"IntegrationSearchVendorOrders{nameSuffix}");
        }

        MapRoutes(app.MapGroup("/api/integrations"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/integrations"), "V1");
    }

    public const string SupplyReadinessReadActionScope = "supplyarr.readiness.read";

    public const string SupplyReferenceReadActionScope = "supplyarr.references.read";

    public const string VendorOrderReadActionScope = "supplyarr.vendor_orders.read";

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

    private static void ValidateVendorOrderReadServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "routarr",
                RequiredTargetProduct = "supplyarr",
                TenantId = tenantId,
                RequiredActionScope = VendorOrderReadActionScope,
            });
    }
}
