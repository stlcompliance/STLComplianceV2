using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class FieldCompanionClockEndpoints
{
    public static void MapFieldCompanionClockEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/fieldcompanion/clock", "/api/v1/mobile/clock", (group, isCanonical) =>
        {
            group.WithTags("fieldcompanion").RequireAuthorization();

            var getStatus = group.MapGet("/", async (
                FieldCompanionClockService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var accessToken = ExtractBearerToken(context);
                return Results.Ok(await service.GetStatusAsync(context.User, accessToken, cancellationToken));
            });
            if (isCanonical)
            {
                getStatus.WithName("GetFieldCompanionClockStatus");
            }

            var submit = group.MapPost("/", async (
                SubmitFieldCompanionClockEventRequest request,
                FieldCompanionClockService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var accessToken = ExtractBearerToken(context);
                return Results.Ok(await service.SubmitAsync(context.User, accessToken, request, cancellationToken));
            });
            if (isCanonical)
            {
                submit.WithName("SubmitFieldCompanionClockEvent");
            }
        });
    }

    private static string ExtractBearerToken(HttpContext context)
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

        return accessToken;
    }
}
