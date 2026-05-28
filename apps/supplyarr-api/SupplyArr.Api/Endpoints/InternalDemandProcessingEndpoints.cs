using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using Microsoft.AspNetCore.Mvc;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Endpoints;

public static class InternalDemandProcessingEndpoints
{
    public static void MapSupplyArrInternalDemandProcessingEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/demand-processing").WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            int? stalenessHours,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] DemandProcessingWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.ListPendingAsync(
                tenantId,
                asOfUtc,
                batchSize,
                stalenessHours,
                cancellationToken));
        })
        .WithName("InternalListPendingDemandProcessing");

        internalApi.MapPost("/process-batch", async (
            [FromBody] ProcessDemandProcessingRequest request,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] DemandProcessingWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.ProcessBatchAsync(request, cancellationToken));
        })
        .WithName("InternalProcessDemandProcessing");
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
                "Service token source product is not authorized for demand processing.",
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
                RequiredActionScope = DemandProcessingWorkerService.ProcessDemandProcessingActionScope,
            });
    }
}
