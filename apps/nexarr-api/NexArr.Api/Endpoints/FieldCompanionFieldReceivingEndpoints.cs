using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class FieldCompanionFieldReceivingEndpoints
{
    public static void MapFieldCompanionFieldReceivingEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/fieldcompanion/field-tasks/receiving", "/api/v1/mobile/field-tasks/receiving", (group, isCanonical) =>
        {
            group.WithTags("fieldcompanion").RequireAuthorization();

            var getDetail = group.MapGet("/", async (
                string taskKey,
                FieldCompanionFieldReceivingService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var accessToken = ExtractBearerToken(context);
                return Results.Ok(await service.GetDetailAsync(
                    context.User,
                    accessToken,
                    taskKey,
                    cancellationToken));
            });
            if (isCanonical)
            {
                getDetail.WithName("GetFieldCompanionFieldReceivingDetail");
            }

            var updateLine = group.MapPost("/line", async (
                UpdateFieldCompanionFieldReceivingLineRequest request,
                FieldCompanionFieldReceivingService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var accessToken = ExtractBearerToken(context);
                return Results.Ok(await service.UpdateLineAsync(
                    context.User,
                    accessToken,
                    request,
                    cancellationToken));
            });
            if (isCanonical)
            {
                updateLine.WithName("UpdateFieldCompanionFieldReceivingLine");
            }

            var post = group.MapPost("/post", async (
                PostFieldCompanionFieldReceivingRequest request,
                FieldCompanionFieldReceivingService service,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var accessToken = ExtractBearerToken(context);
                return Results.Ok(await service.PostAsync(
                    context.User,
                    accessToken,
                    request,
                    cancellationToken));
            });
            if (isCanonical)
            {
                post.WithName("PostFieldCompanionFieldReceiving");
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
