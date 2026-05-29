using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using Microsoft.AspNetCore.Mvc;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class InternalTechnicianRefSyncEndpoints
{
    public static void MapMaintainArrInternalTechnicianRefSyncEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/technician-refs").WithTags("Internal");

        internalApi.MapGet("/pending-stale", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            TimeSpan? staleAfter,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] TechnicianRefSyncService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.ListPendingStaleAsync(
                tenantId,
                asOfUtc,
                batchSize,
                staleAfter,
                cancellationToken));
        })
        .WithName("InternalListPendingStaleTechnicianRefs");

        internalApi.MapPost("/process-refresh", async (
            [FromBody] ProcessTechnicianRefRefreshRequest request,
            HttpContext context,
            [FromServices] StlServiceTokenValidator tokenValidator,
            [FromServices] TechnicianRefSyncService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.ProcessBatchAsync(request, cancellationToken));
        })
        .WithName("InternalProcessTechnicianRefRefresh");
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
                "Service token source product is not authorized for technician ref refresh.",
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
                RequiredActionScope = TechnicianRefSyncService.RefreshTechnicianRefsActionScope
            });
    }
}
