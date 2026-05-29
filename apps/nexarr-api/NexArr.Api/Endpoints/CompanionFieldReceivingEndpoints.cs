using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class CompanionFieldReceivingEndpoints
{
    public static void MapCompanionFieldReceivingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/companion/field-tasks/receiving")
            .WithTags("CompanionFieldReceiving")
            .RequireAuthorization();

        group.MapGet("/", async (
            string taskKey,
            CompanionFieldReceivingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var accessToken = ExtractBearerToken(context);
            return Results.Ok(await service.GetDetailAsync(
                context.User,
                accessToken,
                taskKey,
                cancellationToken));
        })
        .WithName("GetCompanionFieldReceivingDetail");

        group.MapPost("/line", async (
            UpdateCompanionFieldReceivingLineRequest request,
            CompanionFieldReceivingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var accessToken = ExtractBearerToken(context);
            return Results.Ok(await service.UpdateLineAsync(
                context.User,
                accessToken,
                request,
                cancellationToken));
        })
        .WithName("UpdateCompanionFieldReceivingLine");

        group.MapPost("/post", async (
            PostCompanionFieldReceivingRequest request,
            CompanionFieldReceivingService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var accessToken = ExtractBearerToken(context);
            return Results.Ok(await service.PostAsync(
                context.User,
                accessToken,
                request,
                cancellationToken));
        })
        .WithName("PostCompanionFieldReceiving");
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
