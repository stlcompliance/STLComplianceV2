using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using Microsoft.AspNetCore.Mvc;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class InternalAssetDowntimeSyncEndpoints
{
    public static void MapMaintainArrInternalAssetDowntimeSyncEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/downtime-sync").WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] AssetDowntimeSyncWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            var result = await service.ListPendingAsync(tenantId, asOfUtc, batchSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingAssetDowntimeSync");

        internalApi.MapPost("/process-batch", async (
            [FromBody] ProcessAssetDowntimeSyncRequest request,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] AssetDowntimeSyncWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessAssetDowntimeSync");
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
                "Service token source product is not authorized for downtime sync processing.",
                403);
        }

        var effectiveTenantId = tenantId ?? preview.TenantScope ?? Guid.Empty;
        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "maintainarr",
                TenantId = effectiveTenantId,
                RequiredActionScope = AssetDowntimeSyncWorkerService.ProcessAssetDowntimeSyncActionScope,
            });
    }
}
