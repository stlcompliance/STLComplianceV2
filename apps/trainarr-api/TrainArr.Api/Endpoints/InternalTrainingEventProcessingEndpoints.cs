using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Endpoints;

public static class InternalTrainingEventProcessingEndpoints
{
    public static void MapTrainArrInternalTrainingEventProcessingEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/training-events")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TrainingEventProcessingService processingService,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            var result = await processingService.ListPendingAsync(tenantId, asOfUtc, batchSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingTrainingDomainEvents");

        internalApi.MapPost("/process-batch", async (
            ProcessTrainingDomainEventsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TrainingEventProcessingService processingService,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await processingService.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessTrainingDomainEvents");
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
                "Service token source product is not authorized for TrainArr training event processing.",
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
                RequiredActionScope = TrainingEventProcessingService.ProcessEventsActionScope
            });
    }
}
