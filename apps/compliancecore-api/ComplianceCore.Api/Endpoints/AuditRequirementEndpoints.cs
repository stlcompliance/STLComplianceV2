using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class AuditRequirementEndpoints
{
    public static void MapComplianceCoreAuditRequirementEndpoints(this WebApplication app)
    {
        var auditRequirements = app.MapGroup("/api/v1/audit-requirements")
            .WithTags("AuditRequirements")
            .RequireAuthorization();

        auditRequirements.MapGet("/matrix/by-pack/{packKey}", async (
            string packKey,
            ComplianceCoreAuthorizationService authorization,
            AuditRequirementService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            return Results.Ok(await service.MatrixByPackAsync(context.User.GetTenantId(), packKey, cancellationToken));
        })
        .WithName("GetAuditRequirementMatrixByPackV1");

        auditRequirements.MapGet("/matrix/by-source-product/{sourceProduct}", async (
            string sourceProduct,
            ComplianceCoreAuthorizationService authorization,
            AuditRequirementService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            return Results.Ok(await service.MatrixBySourceProductAsync(context.User.GetTenantId(), sourceProduct, cancellationToken));
        })
        .WithName("GetAuditRequirementMatrixBySourceProductV1");

        auditRequirements.MapGet("/matrix/by-entity/{sourceEntity}", async (
            string sourceEntity,
            ComplianceCoreAuthorizationService authorization,
            AuditRequirementService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            return Results.Ok(await service.MatrixByEntityAsync(context.User.GetTenantId(), sourceEntity, cancellationToken));
        })
        .WithName("GetAuditRequirementMatrixByEntityV1");

        auditRequirements.MapGet("/matrix/by-citation/{citationKey}", async (
            string citationKey,
            ComplianceCoreAuthorizationService authorization,
            AuditRequirementService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRulePacksRead(context.User);
            return Results.Ok(await service.MatrixByCitationAsync(context.User.GetTenantId(), citationKey, cancellationToken));
        })
        .WithName("GetAuditRequirementMatrixByCitationV1");

        auditRequirements.MapPost("/evaluate", async (
            AuditRequirementEvaluationRequest request,
            ComplianceCoreAuthorizationService authorization,
            AuditRequirementService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRuleEvaluation(context.User);
            return Results.Ok(await service.EvaluateAsync(context.User.GetTenantId(), request, cancellationToken));
        })
        .WithName("EvaluateAuditRequirementsV1");

        app.MapPost("/api/v1/evidence-references", async (
            EvidenceReferenceCreateRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            AuditRequirementService service,
            CancellationToken cancellationToken) =>
        {
            var token = ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.CreateEvidenceReferenceAsync(request, token.SourceProductKey, cancellationToken));
        })
        .WithTags("AuditEvidence")
        .WithName("CreateEvidenceReferenceV1");

        app.MapPost("/api/v1/fact-assertions", async (
            FactAssertionCreateRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            AuditRequirementService service,
            CancellationToken cancellationToken) =>
        {
            var token = ValidateServiceToken(tokenValidator, context, request.TenantId);
            return Results.Ok(await service.CreateFactAssertionAsync(request, token.SourceProductKey, cancellationToken));
        })
        .WithTags("AuditEvidence")
        .WithName("CreateFactAssertionV1");
    }

    private static ValidatedServiceToken ValidateServiceToken(
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

        return tokenValidator.ValidateOrThrow(
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
