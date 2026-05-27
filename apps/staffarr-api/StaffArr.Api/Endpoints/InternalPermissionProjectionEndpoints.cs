using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Endpoints;

public static class InternalPermissionProjectionEndpoints
{
    public static void MapStaffArrInternalPermissionProjectionEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/permission-projections")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            int? stalenessHours,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PermissionProjectionService service,
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
        .WithName("InternalListPendingPermissionProjections");

        internalApi.MapPost("/process-batch", async (
            ProcessPermissionProjectionsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PermissionProjectionService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessPermissionProjections");
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
                "Service token source product is not authorized for permission projection processing.",
                403);
        }

        var effectiveTenantId = tenantId ?? preview.TenantScope ?? Guid.Empty;
        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "staffarr",
                TenantId = effectiveTenantId,
                RequiredActionScope = PermissionProjectionService.ProjectPermissionsActionScope
            });
    }
}
