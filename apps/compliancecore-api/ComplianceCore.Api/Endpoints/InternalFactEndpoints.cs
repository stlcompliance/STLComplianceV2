using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class InternalFactEndpoints
{
    public static void MapComplianceCoreInternalFactEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal")
            .WithTags("Internal");

        internalApi.MapPost("/resolve", async (
            InternalResolveFactsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            FactResolveService service,
            CancellationToken cancellationToken) =>
        {
            var validated = ValidateServiceToken(
                tokenValidator,
                context,
                request.TenantId,
                FactResolveService.ResolveActionScope);

            var result = await service.ResolveAsync(request, validated.SourceProductKey, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalResolveFacts");

        internalApi.MapPost("/validate", async (
            InternalValidateFactsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            FactResolveService service,
            CancellationToken cancellationToken) =>
        {
            var validated = ValidateServiceToken(
                tokenValidator,
                context,
                request.TenantId,
                FactResolveService.ValidateActionScope);

            var result = await service.ValidateAsync(request, validated.SourceProductKey, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalValidateFacts");

        internalApi.MapPost("/evaluate", async (
            InternalEvaluateRulePackRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            InternalRuleEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            var validated = ValidateServiceToken(
                tokenValidator,
                context,
                request.TenantId,
                InternalRuleEvaluationService.EvaluateActionScope);

            var result = await service.EvaluateAsync(request, validated.SourceProductKey, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalEvaluateRulePack");

        internalApi.MapPost("/evaluate/batch", async (
            InternalEvaluateRulePackBatchRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            InternalRuleEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            var validated = ValidateServiceToken(
                tokenValidator,
                context,
                request.TenantId,
                InternalRuleEvaluationService.EvaluateActionScope);

            var result = await service.EvaluateBatchAsync(request, validated.SourceProductKey, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalEvaluateRulePackBatch");

        internalApi.MapPost("/citations/lookup", async (
            InternalCitationLookupRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            RegulatoryCitationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(
                tokenValidator,
                context,
                request.TenantId,
                InternalCitationLookupService.ReadActionScope);

            if (request.CitationIds.Count > 200)
            {
                throw new StlApiException(
                    "citations.lookup_limit",
                    "Citation lookup supports at most 200 citation ids per request.",
                    400);
            }

            var results = await service.LookupByIdsAsync(
                request.TenantId,
                request.CitationIds,
                cancellationToken);
            return Results.Ok(results);
        })
        .WithName("InternalCitationLookup");

        internalApi.MapPost("/rule-packs/lookup", async (
            InternalRulePackLookupRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            RulePackService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(
                tokenValidator,
                context,
                request.TenantId,
                InternalRulePackLookupService.ReadActionScope);

            if (request.RulePackKeys.Count > 200)
            {
                throw new StlApiException(
                    "rule_packs.lookup_limit",
                    "Rule pack lookup supports at most 200 rule pack keys per request.",
                    400);
            }

            var results = await service.LookupByPackKeysAsync(
                request.TenantId,
                request.RulePackKeys,
                cancellationToken);
            return Results.Ok(results);
        })
        .WithName("InternalRulePackLookup");

        internalApi.MapPost("/workflow-gate-check", async (
            InternalWorkflowGateCheckRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            WorkflowGateService service,
            CancellationToken cancellationToken) =>
        {
            var validated = ValidateServiceToken(
                tokenValidator,
                context,
                request.TenantId,
                WorkflowGateService.CheckActionScope);

            var result = await service.CheckInternalAsync(
                request,
                validated.SourceProductKey,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalWorkflowGateCheck");

        internalApi.MapPost("/workflow-gate-check/batch", async (
            InternalWorkflowGateBatchCheckRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            WorkflowGateService service,
            CancellationToken cancellationToken) =>
        {
            var validated = ValidateServiceToken(
                tokenValidator,
                context,
                request.TenantId,
                WorkflowGateService.CheckActionScope);

            var result = await service.CheckBatchInternalAsync(
                request,
                validated.SourceProductKey,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalWorkflowGateCheckBatch");
    }

    private static ValidatedServiceToken ValidateServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId,
        string requiredActionScope)
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
                RequiredActionScope = requiredActionScope,
            });
    }
}
