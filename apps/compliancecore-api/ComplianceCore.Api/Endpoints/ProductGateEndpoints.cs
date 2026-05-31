using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class ProductGateEndpoints
{
    public static void MapComplianceCoreProductGateEndpoints(this WebApplication app)
    {
        MapGroup(app, "/api/gates");
        MapGroup(app, "/api/v1/gates");
    }

    private static void MapGroup(WebApplication app, string prefix)
    {
        var gates = app.MapGroup(prefix)
            .WithTags("ProductGates");
        var routeSuffix = RouteSuffix(prefix);

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
        .WithName($"EvaluateProductGate{routeSuffix}");

        static Task<IResult> EvaluateCompatibilityByActionKey(
            string actionKey,
            ProductGateCompatibilityRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ProductGateEvaluationService service,
            CancellationToken cancellationToken)
        {
            return EvaluateCompatibilityAsync(
                ResolveCompatibilityActionKey(actionKey.Trim().ToLowerInvariant()),
                request,
                context,
                tokenValidator,
                service,
                cancellationToken);
        }

        gates.MapPost("/{actionKey:regex(^can_[a-z0-9_]+$)}", async (
            string actionKey,
            ProductGateCompatibilityRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ProductGateEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            return await EvaluateCompatibilityByActionKey(
                actionKey,
                request,
                context,
                tokenValidator,
                service,
                cancellationToken);
        })
        .WithName($"EvaluateProductGateByActionKey{routeSuffix}");

        gates.MapPost("/{actionKey:regex(^can-[a-z0-9-]+$)}", async (
            string actionKey,
            ProductGateCompatibilityRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ProductGateEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            return await EvaluateCompatibilityByActionKey(
                actionKey.Replace('-', '_'),
                request,
                context,
                tokenValidator,
                service,
                cancellationToken);
        })
        .WithName($"EvaluateProductGateByHyphenatedActionKey{routeSuffix}");

        var documentedActionAliases = new[]
        {
            "can-dispatch-route",
            "can-release-trip",
            "can-start-work",
            "can-close-work-order",
            "can-assign-person",
            "can-issue-training-qualification",
            "can-approve-purchase",
            "can-use-vendor",
            "can-use-asset",
            "can-accept-evidence",
            "can-serve-customer",
            "can-operate-asset",
        };

        foreach (var actionAlias in documentedActionAliases)
        {
            gates.MapPost($"/{actionAlias}", async (
                ProductGateCompatibilityRequest request,
                HttpContext context,
                StlServiceTokenValidator tokenValidator,
                ProductGateEvaluationService service,
                CancellationToken cancellationToken) =>
            {
                return await EvaluateCompatibilityByActionKey(
                    actionAlias.Replace('-', '_'),
                    request,
                    context,
                    tokenValidator,
                    service,
                    cancellationToken);
            })
            .WithName($"EvaluateProductGate{actionAlias.Replace("-", string.Empty)}{routeSuffix}");
        }
    }

    private static async Task<IResult> EvaluateCompatibilityAsync(
        string normalizedActionKey,
        ProductGateCompatibilityRequest request,
        HttpContext context,
        StlServiceTokenValidator tokenValidator,
        ProductGateEvaluationService service,
        CancellationToken cancellationToken)
    {
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
    }

    private static string ResolveCompatibilityActionKey(string actionKey) =>
        actionKey switch
        {
            "can_assign_person" => "can_assign_person_to_task",
            "can_start_work" => "can_start_work_order",
            "can_close_work_order" => "can_close_work_order",
            "can_operate_asset" => "can_dispatch_asset",
            _ => actionKey,
        };

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

    private static string RouteSuffix(string routePrefix) =>
        routePrefix.Contains("/v1/", StringComparison.OrdinalIgnoreCase) ? "V1" : string.Empty;
}
