using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class FieldCompanionFieldSubmissionEndpoints
{
    public static void MapFieldCompanionFieldSubmissionEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/fieldcompanion/field-tasks", "/api/v1/mobile/field-tasks", (group, isCanonical) =>
        {
            group.WithTags("fieldcompanion").RequireAuthorization();

            var submissionStatus = group.MapGet("/submission-status", async (
                string? taskKeys,
                FieldCompanionFieldSubmissionService service,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                var keys = ParseTaskKeys(taskKeys);
                var response = await service.ListLatestAsync(httpContext.User, keys, cancellationToken);
                return Results.Ok(response);
            });
            if (isCanonical)
            {
                submissionStatus.WithName("GetFieldCompanionFieldTaskSubmissionStatus");
            }

            var validate = group.MapPost("/validate", async (
                ValidateFieldCompanionFieldTaskRequest request,
                FieldCompanionFieldTaskValidationService service,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                var authorization = httpContext.Request.Headers.Authorization.ToString();
                var accessToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authorization["Bearer ".Length..].Trim()
                    : string.Empty;
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new StlApiException("auth.unauthorized", "Bearer access token is required.", 401);
                }

                var response = await service.ValidateAsync(
                    httpContext.User,
                    accessToken,
                    request,
                    cancellationToken);
                return Results.Ok(response);
            });
            if (isCanonical)
            {
                validate.WithName("ValidateFieldCompanionFieldTask");
            }
        });
    }

    private static IReadOnlyList<string> ParseTaskKeys(string? taskKeys)
    {
        if (string.IsNullOrWhiteSpace(taskKeys))
        {
            return [];
        }

        return taskKeys
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.Ordinal)
            .Take(50)
            .ToList();
    }
}
