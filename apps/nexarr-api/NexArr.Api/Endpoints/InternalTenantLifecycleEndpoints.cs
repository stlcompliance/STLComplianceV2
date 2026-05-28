using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class InternalTenantLifecycleEndpoints
{
    public static void MapNexArrInternalTenantLifecycleEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/tenant-lifecycle")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TenantLifecycleWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context);
            var result = await service.ListPendingAsync(asOfUtc, batchSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingTenantLifecycle");

        internalApi.MapPost("/process-batch", async (
            ProcessTenantLifecycleRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TenantLifecycleWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessTenantLifecycle");
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

        if (!string.Equals(preview.SourceProductKey, "shared-worker", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for tenant lifecycle processing.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "nexarr",
                TenantId = preview.TenantScope ?? Guid.Empty,
                RequiredActionScope = TenantLifecycleWorkerService.ProcessLifecycleActionScope,
            });
    }
}
