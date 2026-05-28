using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class InternalEntitlementReconciliationEndpoints
{
    public static void MapNexArrInternalEntitlementReconciliationEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/entitlement-reconciliation")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            EntitlementReconciliationWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context);
            var result = await service.ListPendingAsync(asOfUtc, batchSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingEntitlementReconciliation");

        internalApi.MapPost("/process-batch", async (
            ProcessEntitlementReconciliationRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            EntitlementReconciliationWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessEntitlementReconciliation");
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
                "Service token source product is not authorized for entitlement reconciliation.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "nexarr",
                TenantId = preview.TenantScope ?? Guid.Empty,
                RequiredActionScope = EntitlementReconciliationWorkerService.ProcessReconciliationActionScope,
            });
    }
}
