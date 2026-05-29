using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class InternalFactSourceSyncEndpoints
{
    public static void MapComplianceCoreInternalFactSourceSyncEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/fact-source-sync")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? intervalMinutes,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            FactSourceSyncWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            return Results.Ok(await service.ListPendingAsync(
                tenantId,
                asOfUtc,
                intervalMinutes,
                batchSize,
                cancellationToken));
        })
        .WithName("InternalListPendingFactSourceSyncs");

        internalApi.MapPost("/process-batch", async (
            ProcessFactSourceSyncsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            FactSourceSyncWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.ProcessBatchAsync(request, cancellationToken));
        })
        .WithName("InternalProcessFactSourceSyncs");
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
                "Service token source product is not authorized for fact source sync processing.",
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
                RequiredActionScope = FactSourceSyncWorkerService.ProcessBatchActionScope,
            });
    }
}
