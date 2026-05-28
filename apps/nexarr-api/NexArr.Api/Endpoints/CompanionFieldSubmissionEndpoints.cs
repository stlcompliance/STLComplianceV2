using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class CompanionFieldSubmissionEndpoints
{
    public static void MapCompanionFieldSubmissionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/companion/field-tasks")
            .WithTags("CompanionFieldSubmissions")
            .RequireAuthorization();

        group.MapGet("/submission-status", async (
            string? taskKeys,
            CompanionFieldSubmissionService service,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var keys = ParseTaskKeys(taskKeys);
            var response = await service.ListLatestAsync(httpContext.User, keys, cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetCompanionFieldTaskSubmissionStatus");
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
