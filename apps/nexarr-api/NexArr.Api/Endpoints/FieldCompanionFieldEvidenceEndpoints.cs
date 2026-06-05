using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class FieldCompanionFieldEvidenceEndpoints
{
    public static void MapFieldCompanionFieldEvidenceEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/fieldcompanion/field-tasks/evidence", "/api/v1/mobile/field-tasks/evidence", (group, isCanonical) =>
        {
            group.WithTags("fieldcompanion").RequireAuthorization();

            var submit = group.MapPost("/", async (
                SubmitFieldCompanionFieldEvidenceRequest request,
                FieldCompanionFieldEvidenceService service,
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
            });
            if (isCanonical)
            {
                submit.WithName("SubmitFieldCompanionFieldEvidence");
            }
        });
    }
}
