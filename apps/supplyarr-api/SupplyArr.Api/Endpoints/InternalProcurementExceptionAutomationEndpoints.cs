using Microsoft.AspNetCore.Mvc;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Endpoints;

public static class InternalProcurementExceptionAutomationEndpoints
{
    public static void MapSupplyArrInternalProcurementExceptionAutomationEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/procurement-exception-automation").WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] ProcurementExceptionAutomationWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.ListPendingAsync(tenantId, asOfUtc, batchSize, cancellationToken));
        })
        .WithName("InternalListPendingProcurementExceptionAutoCloses");

        internalApi.MapPost("/process-batch", async (
            [FromBody] ProcessProcurementExceptionAutoClosesRequest request,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] ProcurementExceptionAutomationWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.ProcessBatchAsync(request, cancellationToken));
        })
        .WithName("InternalProcessProcurementExceptionAutoCloses");
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
                "Service token source product is not authorized for procurement exception automation.",
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
                RequiredActionScope = ProcurementExceptionAutomationWorkerService
                    .ProcessProcurementExceptionAutoClosesActionScope,
            });
    }
}
