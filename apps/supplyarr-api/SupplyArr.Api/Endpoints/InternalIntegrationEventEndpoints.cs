using Microsoft.AspNetCore.Mvc;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Endpoints;

public static class InternalIntegrationEventEndpoints
{
    public static void MapSupplyArrInternalIntegrationEventEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/integration-events").WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] IntegrationEventProcessingService service,
            CancellationToken cancellationToken) =>
        {
            ValidateProcessToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.ListPendingAsync(tenantId, asOfUtc, batchSize, cancellationToken));
        })
        .WithName("InternalListPendingIntegrationEvents");

        internalApi.MapPost("/process-batch", async (
            [FromBody] ProcessIntegrationEventsRequest request,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] IntegrationEventProcessingService service,
            CancellationToken cancellationToken) =>
        {
            ValidateProcessToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.ProcessBatchAsync(request, cancellationToken));
        })
        .WithName("InternalProcessIntegrationEvents");

        internalApi.MapPost("/inbox/enqueue", async (
            [FromBody] EnqueueIntegrationInboxRequest request,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] IntegrationInboxEnqueueService service,
            CancellationToken cancellationToken) =>
        {
            ValidateInboxEnqueueToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.TryEnqueueAsync(request, cancellationToken));
        })
        .WithName("InternalEnqueueIntegrationInboxEvent");
    }

    private static void ValidateProcessToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid? tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        var preview = tokenValidator.TryValidate(bearer);
        if (preview is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        if (!string.Equals(preview.SourceProductKey, "shared-worker", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for integration event processing.",
                403);
        }

        var effectiveTenantId = tenantId ?? preview.TenantScope ?? Guid.Empty;
        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "supplyarr",
                TenantId = effectiveTenantId,
                RequiredActionScope = IntegrationEventProcessingService.ProcessEventsActionScope,
            });
    }

    private static void ValidateInboxEnqueueToken(
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

        var source = preview.SourceProductKey ?? string.Empty;
        if (!string.Equals(source, "shared-worker", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source, "maintainarr", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source, "routarr", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source, "trainarr", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source, "staffarr", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized to enqueue integration inbox events.",
                403);
        }

        var requiredScope = string.Equals(source, "maintainarr", StringComparison.OrdinalIgnoreCase)
            || string.Equals(source, "routarr", StringComparison.OrdinalIgnoreCase)
            || string.Equals(source, "trainarr", StringComparison.OrdinalIgnoreCase)
            || string.Equals(source, "staffarr", StringComparison.OrdinalIgnoreCase)
            ? IntegrationEndpoints.MaintainarrDemandIngestActionScope
            : IntegrationEventProcessingService.EnqueueInboxActionScope;

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = source,
                RequiredTargetProduct = "supplyarr",
                TenantId = tenantId,
                RequiredActionScope = requiredScope,
            });
    }
}
