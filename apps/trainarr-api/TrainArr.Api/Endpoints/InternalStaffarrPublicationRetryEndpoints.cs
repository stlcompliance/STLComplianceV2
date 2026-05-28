using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Endpoints;

public static class InternalStaffarrPublicationRetryEndpoints
{
    public static void MapTrainArrInternalStaffarrPublicationRetryEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/staffarr-publication-retries")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            StaffarrPublicationRetryService retryService,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            var result = await retryService.ListPendingAsync(tenantId, asOfUtc, batchSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingStaffarrPublicationRetries");

        internalApi.MapPost("/process-batch", async (
            ProcessStaffarrPublicationRetriesRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            StaffarrPublicationRetryService retryService,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await retryService.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessStaffarrPublicationRetries");
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
                "Service token source product is not authorized for StaffArr publication retries.",
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
                RequiredActionScope = StaffarrPublicationRetryService.ProcessRetriesActionScope
            });
    }
}
