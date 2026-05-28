using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class InternalMaintenanceNotificationEndpoints
{
    public static void MapMaintainArrInternalMaintenanceNotificationEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/maintenance-notifications")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            MaintenanceNotificationDispatchService dispatchService,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            var result = await dispatchService.ListPendingAsync(tenantId, asOfUtc, batchSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingMaintenanceNotifications");

        internalApi.MapPost("/process-batch", async (
            ProcessMaintenanceNotificationsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            MaintenanceNotificationDispatchService dispatchService,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await dispatchService.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessMaintenanceNotifications");
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
                "Service token source product is not authorized for maintenance notification dispatch.",
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
                RequiredActionScope = MaintenanceNotificationDispatchService.ProcessNotificationsActionScope,
            });
    }
}
