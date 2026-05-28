using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class CompanionFieldEvidenceEndpoints
{
    public static void MapCompanionFieldEvidenceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/companion/field-tasks/evidence")
            .WithTags("CompanionFieldEvidence")
            .RequireAuthorization();

        group.MapPost("/", async (
            SubmitCompanionFieldEvidenceRequest request,
            CompanionFieldEvidenceService service,
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

            return Results.Ok(await service.SubmitAsync(
                context.User,
                accessToken,
                request,
                cancellationToken));
        })
        .WithName("SubmitCompanionFieldEvidence");
    }
}
