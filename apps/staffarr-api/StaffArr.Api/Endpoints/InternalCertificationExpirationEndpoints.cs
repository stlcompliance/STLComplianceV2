using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Endpoints;

public static class InternalCertificationExpirationEndpoints
{
    public static void MapStaffArrInternalCertificationExpirationEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/certifications")
            .WithTags("Internal");

        internalApi.MapGet("/pending-expiration", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            CertificationExpirationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            var result = await service.ListPendingAsync(
                tenantId,
                asOfUtc,
                batchSize ?? 100,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingCertificationExpirations");

        internalApi.MapPost("/process-expirations", async (
            ProcessCertificationExpirationsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            CertificationExpirationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessCertificationExpirations");
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
                "Service token source product is not authorized for certification expiration.",
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
                RequiredActionScope = CertificationExpirationService.ProcessExpirationsActionScope
            });
    }
}
