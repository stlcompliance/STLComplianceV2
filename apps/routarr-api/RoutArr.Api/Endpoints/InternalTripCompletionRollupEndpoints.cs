using RoutArr.Api.Contracts;

using RoutArr.Api.Services;

using Microsoft.AspNetCore.Mvc;

using STLCompliance.Shared.Auth;

using STLCompliance.Shared.Contracts;



namespace RoutArr.Api.Endpoints;



public static class InternalTripCompletionRollupEndpoints

{

    public static void MapRoutArrInternalTripCompletionRollupEndpoints(this WebApplication app)

    {

        var internalApi = app.MapGroup("/api/internal/trip-completion-rollups").WithTags("Internal");



        internalApi.MapGet("/pending", async (

            Guid? tenantId,

            DateTimeOffset? asOfUtc,

            int? batchSize,

            int? stalenessHours,

            HttpContext context,

            [FromServices] StlServiceTokenValidator tokenValidator,

            [FromServices] TripCompletionRollupWorkerService service,

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

        .WithName("InternalListPendingTripCompletionRollups");



        internalApi.MapPost("/process-batch", async (

            [FromBody] ProcessTripCompletionRollupsRequest request,

            HttpContext context,

            [FromServices] StlServiceTokenValidator tokenValidator,

            [FromServices] TripCompletionRollupWorkerService service,

            CancellationToken cancellationToken) =>

        {

            ValidateServiceToken(tokenValidator, context, request.TenantId);

            var result = await service.ProcessBatchAsync(request, cancellationToken);

            return Results.Ok(result);

        })

        .WithName("InternalProcessTripCompletionRollups");

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

                "Service token source product is not authorized for trip completion rollup processing.",

                403);

        }



        var effectiveTenantId = tenantId ?? preview.TenantScope ?? Guid.Empty;

        tokenValidator.ValidateOrThrow(

            bearer,

            new ServiceTokenRequirements

            {

                ExpectedSourceProduct = "shared-worker",

                RequiredTargetProduct = "routarr",

                TenantId = effectiveTenantId,

                RequiredActionScope = TripCompletionRollupWorkerService.ProcessTripCompletionRollupsActionScope,

            });

    }

}


