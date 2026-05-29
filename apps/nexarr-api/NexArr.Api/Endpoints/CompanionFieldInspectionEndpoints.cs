using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class CompanionFieldInspectionEndpoints
{
    public static void MapCompanionFieldInspectionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/companion/field-tasks/inspection")
            .WithTags("CompanionFieldInspection")
            .RequireAuthorization();

        group.MapGet("/", async (
            string taskKey,
            CompanionFieldInspectionService service,
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
        .WithName("GetCompanionFieldInspectionDetail");

        group.MapPost("/answers", async (
            SubmitCompanionFieldInspectionAnswersRequest request,
            CompanionFieldInspectionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var accessToken = ExtractBearerToken(context);
            return Results.Ok(await service.SubmitAnswersAsync(
                context.User,
                accessToken,
                request,
                cancellationToken));
        })
        .WithName("SubmitCompanionFieldInspectionAnswers");

        group.MapPost("/complete", async (
            CompleteCompanionFieldInspectionRequest request,
            CompanionFieldInspectionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var accessToken = ExtractBearerToken(context);
            return Results.Ok(await service.CompleteAsync(
                context.User,
                accessToken,
                request,
                cancellationToken));
        })
        .WithName("CompleteCompanionFieldInspection");
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
