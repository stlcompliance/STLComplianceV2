using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Endpoints;

public static class CompanionEndpoints
{
    public static void MapCompanionEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/companion", "/api/v1/mobile", (group, isCanonical) =>
        {
            group.WithTags("FieldCompanion").RequireAuthorization();

            var me = group.MapGet("/me", (
                CompanionAuthService service,
                HttpContext context) =>
            {
                return Results.Ok(service.GetMe(context.User));
            });
            if (isCanonical)
            {
                me.WithName("CompanionGetMe");
            }

            var fieldInbox = group.MapGet("/field-inbox", async (
                HttpContext context,
                CompanionFieldInboxService service,
                CancellationToken cancellationToken) =>
            {
                var authorization = context.Request.Headers.Authorization.ToString();
                var accessToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authorization["Bearer ".Length..].Trim()
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new STLCompliance.Shared.Contracts.StlApiException(
                        "auth.unauthorized",
                        "Bearer access token is required.",
                        401);
                }

                return Results.Ok(await service.GetAsync(context.User, accessToken, cancellationToken));
            });
            if (isCanonical)
            {
                fieldInbox.WithName("CompanionGetFieldInbox");
            }
        });

        var auth = app.MapGroup("/api/companion/auth")
            .WithTags("CompanionAuth")
            .ExcludeFromDescription();

        auth.MapPost("/handoff/redeem", async (
            CompanionRedeemHandoffRequest request,
            CompanionAuthService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RedeemHandoffAsync(request, cancellationToken));
        })
        .AllowAnonymous()
        .WithName("CompanionRedeemHandoff");
    }
}
