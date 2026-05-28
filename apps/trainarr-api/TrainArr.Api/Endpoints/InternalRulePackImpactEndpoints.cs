using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Endpoints;

public static class InternalRulePackImpactEndpoints
{
    public static void MapTrainArrInternalRulePackImpactEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/rule-pack-impact")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            int? stalenessHours,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            RulePackImpactWorkerService service,
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
        .WithName("InternalListPendingRulePackImpactScans");

        internalApi.MapPost("/process-batch", async (
            ProcessRulePackImpactScansRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            RulePackImpactWorkerService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessRulePackImpactScans");
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
                "Service token source product is not authorized for rule pack impact scans.",
                403);
        }

        var effectiveTenantId = tenantId ?? preview.TenantScope ?? Guid.Empty;
        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "trainarr",
                TenantId = effectiveTenantId,
                RequiredActionScope = RulePackImpactWorkerService.ProcessImpactScansActionScope
            });
    }
}
