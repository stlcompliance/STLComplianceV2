using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class ProductFactIntegrationEndpoints
{
    public static void MapComplianceCoreProductFactIntegrationEndpoints(this WebApplication app)
    {
        MapProductFactIngestGroup(
            app.MapGroup("/api/integrations/product-facts").WithTags("Integrations"),
            "IngestProductFacts");
        MapProductFactIngestGroup(
            app.MapGroup("/api/v1/integrations/product-facts").WithTags("Integrations"),
            "IngestProductFactsV1");
    }

    private static void MapProductFactIngestGroup(RouteGroupBuilder integrations, string endpointName)
    {
        integrations.MapPost("/ingest", IngestProductFactsAsync)
            .WithName(endpointName);
    }

    private static async Task<IResult> IngestProductFactsAsync(
        IngestProductFactsRequest request,
        HttpContext context,
        StlServiceTokenValidator tokenValidator,
        ProductFactIngestionService service,
        CancellationToken cancellationToken)
    {
        ValidateServiceToken(tokenValidator, context, request.TenantId);
        var result = await service.IngestAsync(request, cancellationToken);
        return Results.Ok(result);
    }

    private static void ValidateServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        var preview = tokenValidator.TryValidate(bearer);
        if (preview is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = preview.SourceProductKey,
                RequiredTargetProduct = "compliancecore",
                TenantId = tenantId,
                RequiredActionScope = ProductFactIngestionService.IngestFactsActionScope,
            });
    }
}
