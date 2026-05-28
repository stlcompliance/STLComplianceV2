using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Endpoints;

public static class InternalAuditPackageGenerationEndpoints
{
    public static void MapMaintainArrInternalAuditPackageGenerationEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/audit-package-jobs")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            AuditPackageGenerationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            var result = await service.ListPendingAsync(tenantId, asOfUtc, batchSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingMaintainArrAuditPackageGenerationJobs");

        internalApi.MapPost("/process-batch", async (
            ProcessAuditPackageGenerationJobsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            AuditPackageGenerationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessMaintainArrAuditPackageGenerationJobs");
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
                "Service token source product is not authorized for audit package generation processing.",
                403);
        }

        var effectiveTenantId = tenantId ?? preview.TenantScope ?? Guid.Empty;
        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "MaintainArr",
                TenantId = effectiveTenantId,
                RequiredActionScope = AuditPackageGenerationService.ProcessJobsActionScope,
            });
    }
}
