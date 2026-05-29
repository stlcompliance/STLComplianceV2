using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class InternalPlatformOutboxPublisherEndpoints
{
    public static void MapNexArrInternalPlatformOutboxPublisherEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/platform-outbox")
            .WithTags("Internal");

        internalApi.MapGet("/status", async (
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PlatformOutboxPublisherWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context);
            return Results.Ok(await service.GetStatusAsync(cancellationToken));
        })
        .WithName("InternalGetPlatformOutboxPublisherStatus");

        internalApi.MapGet("/pending", async (
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PlatformOutboxPublisherWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context);
            var result = await service.ListPendingAsync(asOfUtc, batchSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingPlatformOutboxEvents");

        internalApi.MapPost("/process-batch", async (
            ProcessPlatformOutboxPublisherRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PlatformOutboxPublisherWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessPlatformOutboxPublisher");
    }

    private static void ValidateServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        var preview = tokenValidator.TryValidate(bearer);
        if (preview is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        if (!string.Equals(preview.SourceProductKey, "nexarr-worker", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for platform outbox publishing.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "nexarr-worker",
                RequiredTargetProduct = "nexarr",
                TenantId = preview.TenantScope ?? Guid.Empty,
                RequiredActionScope = PlatformOutboxPublisherWorkerService.ProcessPublishActionScope,
            });
    }
}
