using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class InternalPersonLoginDisableEndpoints
{
    public static void MapNexArrInternalPersonLoginDisableEndpoints(this WebApplication app)
    {
        app.MapPost("/api/internal/person-login-disable", async (
            PersonLoginDisableRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PersonLoginDisableService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.DisableLoginAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalPersonLoginDisable")
        .WithTags("Internal");
    }

    private static void ValidateServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        var preview = tokenValidator.TryValidate(bearer);
        if (preview is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        if (!string.Equals(preview.SourceProductKey, "staffarr", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for person login disable.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "staffarr",
                RequiredTargetProduct = "nexarr",
                TenantId = tenantId,
                RequiredActionScope = PersonLoginDisableService.DisableLoginActionScope,
            });
    }
}
