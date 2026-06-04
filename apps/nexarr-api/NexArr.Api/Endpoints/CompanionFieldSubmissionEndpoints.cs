using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class CompanionFieldSubmissionEndpoints
{
    public static void MapCompanionFieldSubmissionEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/companion/field-tasks", "/api/v1/mobile/field-tasks", (group, isCanonical) =>
        {
            group.WithTags("FieldCompanion").RequireAuthorization();

            var submissionStatus = group.MapGet("/submission-status", async (
                string? taskKeys,
                CompanionFieldSubmissionService service,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                var keys = ParseTaskKeys(taskKeys);
                var response = await service.ListLatestAsync(httpContext.User, keys, cancellationToken);
                return Results.Ok(response);
            });
            if (isCanonical)
            {
                submissionStatus.WithName("GetCompanionFieldTaskSubmissionStatus");
            }

            var validate = group.MapPost("/validate", async (
                ValidateCompanionFieldTaskRequest request,
                CompanionFieldTaskValidationService service,
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
                validate.WithName("ValidateCompanionFieldTask");
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
