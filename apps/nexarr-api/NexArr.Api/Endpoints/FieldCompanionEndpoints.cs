using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Endpoints;

public static class FieldCompanionEndpoints
{
    public static void MapFieldCompanionEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/fieldcompanion", "/api/v1/mobile", (group, isCanonical) =>
        {
            group.WithTags("fieldcompanion").RequireAuthorization();

            var me = group.MapGet("/me", (
                FieldCompanionAuthService service,
                HttpContext context) =>
            {
                return Results.Ok(service.GetMe(context.User));
            });
            if (isCanonical)
            {
                me.WithName("FieldCompanionGetMe");
            }

            var fieldInbox = group.MapGet("/field-inbox", async (
                HttpContext context,
                FieldCompanionFieldInboxService service,
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
                fieldInbox.WithName("FieldCompanionGetFieldInbox");
            }
        });

        var auth = app.MapGroup("/api/fieldcompanion/auth")
            .WithTags("FieldCompanionAuth")
            .ExcludeFromDescription();

        auth.MapPost("/handoff/redeem", async (
            FieldCompanionRedeemHandoffRequest request,
            HttpContext httpContext,
            FieldCompanionAuthService service,
            CancellationToken cancellationToken) =>
        {
            var response = await service.RedeemHandoffAsync(request, cancellationToken);
            if (BrowserSessionCookieService.WantsCookieSession(httpContext.Request))
            {
                BrowserSessionCookieService.SetRefreshTokenCookie(
                    httpContext,
                    response.RefreshToken,
                    response.RefreshExpiresAt);
                return Results.Ok(response with { RefreshToken = string.Empty });
            }

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithName("FieldCompanionRedeemHandoff");
    }
}
