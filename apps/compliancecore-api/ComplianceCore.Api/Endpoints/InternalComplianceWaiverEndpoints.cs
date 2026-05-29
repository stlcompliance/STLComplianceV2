using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class InternalComplianceWaiverEndpoints
{
    public static void MapComplianceCoreInternalWaiverEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/waivers")
            .WithTags("Internal");

        internalApi.MapPost("/expire-batch", async (
            ProcessExpiredWaiversRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ComplianceWaiverService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.ProcessExpiredBatchAsync(request, cancellationToken));
        })
        .WithName("InternalProcessExpiredComplianceWaivers");
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
                "Service token source product is not authorized for waiver expiration processing.",
                403);
        }

        var effectiveTenantId = tenantId ?? preview.TenantScope ?? Guid.Empty;

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "compliancecore",
                TenantId = effectiveTenantId,
                RequiredActionScope = ComplianceWaiverService.ExpireBatchActionScope,
            });
    }
}
