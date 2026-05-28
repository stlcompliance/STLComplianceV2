using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class InternalPlatformAuditPackageGenerationEndpoints
{
    public static void MapNexArrInternalPlatformAuditPackageGenerationEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/platform-audit-package-jobs")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PlatformAuditPackageGenerationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            var result = await service.ListPendingAsync(tenantId, asOfUtc, batchSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingPlatformAuditPackageGenerationJobs");

        internalApi.MapPost("/process-batch", async (
            ProcessPlatformAuditPackageGenerationJobsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PlatformAuditPackageGenerationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessPlatformAuditPackageGenerationJobs");
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
                "Service token source product is not authorized for platform audit package generation processing.",
                403);
        }

        var effectiveTenantId = tenantId ?? preview.TenantScope ?? Guid.Empty;
        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "nexarr",
                TenantId = effectiveTenantId,
                RequiredActionScope = PlatformAuditPackageGenerationService.ProcessJobsActionScope,
            });
    }
}
