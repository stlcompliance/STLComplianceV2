using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class CompanionScanEndpoints
{
    public static void MapCompanionScanEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/companion/scan")
            .WithTags("CompanionScan")
            .RequireAuthorization();

        group.MapPost("/resolve", async (
            CompanionScanResolveRequest request,
            CompanionScanResolveService service,
            HttpContext context,
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

            return Results.Ok(await service.ResolveAsync(
                context.User,
                accessToken,
                request,
                cancellationToken));
        })
        .WithName("CompanionResolveScan");
    }
}
