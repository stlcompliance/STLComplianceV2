using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class InternalLaunchDestinationReconciliationEndpoints
{
    public static void MapNexArrInternalLaunchDestinationReconciliationEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            app.MapGroup("/api/internal/launch-destination-reconciliation"),
            app.MapGroup("/api/internal/entitlement-reconciliation"),
            app.MapGroup("/api/internal/launch-availability-reconciliation"),
        };

        foreach (var internalApi in groups)
        {
            internalApi.WithTags("Internal");

            internalApi.MapGet("/pending", async (
                DateTimeOffset? asOfUtc,
                int? batchSize,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                LaunchDestinationReconciliationWorkerService service,
                CancellationToken cancellationToken) =>
            {
                ValidateServiceToken(tokenValidator, context);
                var result = await service.ListPendingAsync(asOfUtc, batchSize, cancellationToken);
                return Results.Ok(result);
            });

            internalApi.MapPost("/process-batch", async (
                ProcessLaunchDestinationReconciliationRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                LaunchDestinationReconciliationWorkerService service,
                CancellationToken cancellationToken) =>
            {
                ValidateServiceToken(tokenValidator, context);
                var result = await service.ProcessBatchAsync(request, cancellationToken);
                return Results.Ok(result);
            });
        }
    }

    public static void MapLegacyNexArrInternalEntitlementReconciliationEndpoints(this WebApplication app) =>
        app.MapNexArrInternalLaunchDestinationReconciliationEndpoints();

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
                "Service token source product is not authorized for launch-destination reconciliation.",
                403);
        }

        var acceptedActionScope = ResolveAcceptedActionScope(preview.ActionScope);
        if (acceptedActionScope is null)
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token action scope is not authorized for launch-destination reconciliation.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "nexarr",
                TenantId = preview.TenantScope ?? Guid.Empty,
                RequiredActionScope = acceptedActionScope,
            });
    }

    private static string? ResolveAcceptedActionScope(string? tokenActionScope)
    {
        if (string.IsNullOrWhiteSpace(tokenActionScope))
        {
            return null;
        }

        if (LaunchDestinationReconciliationWorkerService.AcceptedActionScopes.Contains(
                tokenActionScope,
                StringComparer.OrdinalIgnoreCase))
        {
            return tokenActionScope;
        }

        return tokenActionScope
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(scope =>
                LaunchDestinationReconciliationWorkerService.AcceptedActionScopes.Contains(scope, StringComparer.OrdinalIgnoreCase));
    }
}
