using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class FieldCompanionFieldInspectionEndpoints
{
    public static void MapFieldCompanionFieldInspectionEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/fieldcompanion/field-tasks/inspection", "/api/v1/mobile/field-tasks/inspection", (group, isCanonical) =>
        {
            group.WithTags("fieldcompanion").RequireAuthorization();

            var getDetail = group.MapGet("/", async (
                string taskKey,
                FieldCompanionFieldInspectionService service,
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
                getDetail.WithName("GetFieldCompanionFieldInspectionDetail");
            }

            var submitAnswers = group.MapPost("/answers", async (
                SubmitFieldCompanionFieldInspectionAnswersRequest request,
                FieldCompanionFieldInspectionService service,
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
                submitAnswers.WithName("SubmitFieldCompanionFieldInspectionAnswers");
            }

            var complete = group.MapPost("/complete", async (
                CompleteFieldCompanionFieldInspectionRequest request,
                FieldCompanionFieldInspectionService service,
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
                complete.WithName("CompleteFieldCompanionFieldInspection");
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
