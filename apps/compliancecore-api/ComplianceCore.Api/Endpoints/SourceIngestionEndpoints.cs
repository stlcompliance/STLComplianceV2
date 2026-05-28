using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class SourceIngestionEndpoints
{
    public static void MapComplianceCoreSourceIngestionEndpoints(this WebApplication app)
    {
        var batches = app.MapGroup("/api/source-ingestion")
            .WithTags("SourceIngestion")
            .RequireAuthorization();

        batches.MapGet("/batches", async (
            string? ingestionType,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            SourceIngestionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSourceIngestionRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListBatchesAsync(
                tenantId,
                ingestionType,
                limit ?? 20,
                cancellationToken));
        })
        .WithName("ListSourceIngestionBatches");

        batches.MapGet("/batches/{batchId:guid}", async (
            Guid batchId,
            ComplianceCoreAuthorizationService authorization,
            SourceIngestionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSourceIngestionRead(context.User);
            var tenantId = context.User.GetTenantId();
            var detail = await service.GetBatchAsync(tenantId, batchId, cancellationToken);
            return detail is null ? Results.NotFound() : Results.Ok(detail);
        })
        .WithName("GetSourceIngestionBatch");

        batches.MapPost("/fact-sources/validate", IngestFactSourcesAsync(dryRun: true, SourceIngestionPhases.Validate))
            .WithName("ValidateFactSourceIngestion");

        batches.MapPost("/fact-sources/commit", IngestFactSourcesAsync(dryRun: false, SourceIngestionPhases.Commit))
            .WithName("CommitFactSourceIngestion");

        var integrations = app.MapGroup("/api/integrations/source-ingestion")
            .WithTags("Integrations");

        integrations.MapPost("/product-facts/validate", IngestProductFactsIntegrationAsync(
                dryRun: true,
                SourceIngestionPhases.Validate))
            .WithName("ValidateProductFactSourceIngestion");

        integrations.MapPost("/product-facts/commit", IngestProductFactsIntegrationAsync(
                dryRun: false,
                SourceIngestionPhases.Commit))
            .WithName("CommitProductFactSourceIngestion");
    }

    private static Delegate IngestFactSourcesAsync(bool dryRun, string phase) =>
        async (
            FactSourceBulkIngestionRequest? request,
            ComplianceCoreAuthorizationService authorization,
            SourceIngestionService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireSourceIngestionManage(context.User);
            if (request?.Sources is not { Count: > 0 })
            {
                throw new StlApiException(
                    "source_ingestion.validation",
                    "Provide a sources array with at least one row.",
                    400);
            }

            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.IngestFactSourcesAsync(
                tenantId,
                actorUserId,
                request.Sources,
                dryRun,
                phase,
                cancellationToken));
        };

    private static Delegate IngestProductFactsIntegrationAsync(bool dryRun, string phase) =>
        async (
            ProductFactBulkIngestionRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            SourceIngestionService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.IngestProductFactsAsync(
                request.TenantId,
                actorUserId: null,
                request,
                dryRun,
                phase,
                cancellationToken));
        };

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
                RequiredActionScope = SourceIngestionService.IngestSourcesActionScope,
            });
    }
}
