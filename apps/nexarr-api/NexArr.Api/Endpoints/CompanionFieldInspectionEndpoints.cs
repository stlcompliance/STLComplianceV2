using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class CompanionFieldInspectionEndpoints
{
    public static void MapCompanionFieldInspectionEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/companion/field-tasks/inspection", "/api/v1/mobile/field-tasks/inspection", (group, isCanonical) =>
        {
            group.WithTags("FieldCompanion").RequireAuthorization();

            var getDetail = group.MapGet("/", async (
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
            });
            if (isCanonical)
            {
                getDetail.WithName("GetCompanionFieldInspectionDetail");
            }

            var submitAnswers = group.MapPost("/answers", async (
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
            });
            if (isCanonical)
            {
                submitAnswers.WithName("SubmitCompanionFieldInspectionAnswers");
            }

            var complete = group.MapPost("/complete", async (
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
            });
            if (isCanonical)
            {
                complete.WithName("CompleteCompanionFieldInspection");
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
