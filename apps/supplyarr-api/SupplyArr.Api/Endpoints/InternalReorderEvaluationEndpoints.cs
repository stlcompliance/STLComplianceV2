using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using Microsoft.AspNetCore.Mvc;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Endpoints;

public static class InternalReorderEvaluationEndpoints
{
    public static void MapSupplyArrInternalReorderEvaluationEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/reorder").WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            int? batchSize,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] ReorderEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            var result = await service.ListPendingAsync(
                tenantId,
                batchSize ?? 100,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingReorderEvaluation");

        internalApi.MapPost("/process-evaluation", async (
            [FromBody] ProcessReorderEvaluationRequest request,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] ReorderEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessReorderEvaluation");
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
                "Service token source product is not authorized for reorder evaluation.",
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
                RequiredActionScope = ReorderEvaluationService.ProcessEvaluationActionScope
            });
    }
}
