using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class InternalIntegrationTokenEndpoints
{
    public static void MapInternalIntegrationTokenEndpoints(this WebApplication app)
    {
        app.MapGet("/api/internal/integration-tokens", async (
            string consumer,
            HttpContext context,
            IConfiguration configuration,
            IntegrationTokenBootstrapService bootstrapService,
            CancellationToken cancellationToken) =>
        {
            var providedSecret = context.Request.Headers["X-Integration-Bootstrap-Secret"].ToString();
            if (!IntegrationBootstrapSecretValidator.IsValid(configuration, providedSecret))
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(consumer))
            {
                return Results.BadRequest(new { error = "consumer query parameter is required." });
            }

            var tokens = await bootstrapService.GetTokensForConsumerAsync(consumer.Trim(), cancellationToken);
            return Results.Ok(new IntegrationTokenProvisionResponse(tokens));
        })
        .WithName("GetIntegrationTokensForConsumer")
        .WithTags("InternalIntegrationTokens")
        .AllowAnonymous();
    }
}
