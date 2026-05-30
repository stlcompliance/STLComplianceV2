using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class ProductGateEndpoints
{
    public static void MapComplianceCoreProductGateEndpoints(this WebApplication app)
    {
        var gates = app.MapGroup("/api/v1/gates")
            .WithTags("ProductGates");

        gates.MapPost("/evaluate", async (
            ProductGateEvaluationRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ProductGateEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            var validated = ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.EvaluateAsync(
                request,
                validated.SourceProductKey,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("EvaluateProductGate");

        gates.MapPost("/{actionKey:regex(^can_[a-z0-9_]+$)}", async (
            string actionKey,
            ProductGateCompatibilityRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ProductGateEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            var normalizedActionKey = actionKey.Trim().ToLowerInvariant();
            var evaluationRequest = new ProductGateEvaluationRequest(
                request.TenantId,
                request.WorkflowKey ?? normalizedActionKey,
                normalizedActionKey,
                request.ActivityContextKey,
                request.SubjectReferences,
                request.RuleContext,
                request.FactSnapshotReferences,
                request.EmitFindings);
            var validated = ValidateServiceToken(tokenValidator, context, evaluationRequest.TenantId);
            var result = await service.EvaluateAsync(
                evaluationRequest,
                validated.SourceProductKey,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("EvaluateProductGateByActionKey");
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
                RequiredActionScope = ProductGateEvaluationService.EvaluateActionScope,
            });
    }
}
