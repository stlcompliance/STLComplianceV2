using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Endpoints;

public static class InternalIntegrationEventEndpoints
{
    public static void MapRoutArrInternalIntegrationEventEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/integration-events")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            IntegrationEventProcessingService processingService,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await processingService.ListPendingAsync(tenantId, asOfUtc, batchSize, cancellationToken));
        })
        .WithName("InternalListPendingRoutArrIntegrationEvents");

        internalApi.MapPost("/process-batch", async (
            ProcessIntegrationOutboxEventsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            IntegrationEventProcessingService processingService,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await processingService.ProcessBatchAsync(request, cancellationToken));
        })
        .WithName("InternalProcessRoutArrIntegrationEvents");
    }

    private static void ValidateServiceToken(
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
                RequiredTargetProduct = "routarr",
                TenantId = effectiveTenantId,
                RequiredActionScope = IntegrationEventProcessingService.ProcessEventsActionScope,
            });
    }
}
