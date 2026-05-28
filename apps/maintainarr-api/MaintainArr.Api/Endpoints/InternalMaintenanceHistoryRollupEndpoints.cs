using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using Microsoft.AspNetCore.Mvc;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class InternalMaintenanceHistoryRollupEndpoints
{
    public static void MapMaintainArrInternalMaintenanceHistoryRollupEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/maintenance-history-rollups").WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            int? stalenessHours,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] MaintenanceHistoryRollupWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            var result = await service.ListPendingAsync(
                tenantId,
                asOfUtc,
                batchSize,
                stalenessHours,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingMaintenanceHistoryRollups");

        internalApi.MapPost("/process-batch", async (
            [FromBody] ProcessMaintenanceHistoryRollupsRequest request,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] MaintenanceHistoryRollupWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessMaintenanceHistoryRollups");
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
                "Service token source product is not authorized for maintenance history rollup processing.",
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
                RequiredActionScope = MaintenanceHistoryRollupWorkerService.ProcessMaintenanceHistoryRollupsActionScope,
            });
    }
}
