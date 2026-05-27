using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class InternalScheduledEvaluationEndpoints
{
    public static void MapComplianceCoreInternalScheduledEvaluationEndpoints(this WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/scheduled-evaluations")
            .WithTags("Internal");

        internalApi.MapGet("/pending", async (
            Guid? tenantId,
            DateTimeOffset? asOfUtc,
            int? batchSize,
            int? intervalHours,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ScheduledRuleEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, tenantId);
            var result = await service.ListPendingAsync(
                tenantId,
                asOfUtc,
                batchSize,
                intervalHours,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalListPendingScheduledRuleEvaluations");

        internalApi.MapPost("/process-batch", async (
            ProcessScheduledRuleEvaluationsRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            ScheduledRuleEvaluationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context, request.TenantId);
            var result = await service.ProcessBatchAsync(request, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("InternalProcessScheduledRuleEvaluations");
    }

    private static void ValidateServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid? tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        var preview = tokenValidator.TryValidate(bearer);
        if (preview is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        if (!string.Equals(preview.SourceProductKey, "shared-worker", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for scheduled rule evaluation.",
                403);
        }

        var effectiveTenantId = tenantId ?? preview.TenantScope ?? Guid.Empty;
        var actionScope = ResolveAcceptedActionScope(preview.ActionScope);

        if (actionScope is null)
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token is missing a scheduled rule evaluation scope.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "shared-worker",
                RequiredTargetProduct = "compliancecore",
                TenantId = effectiveTenantId,
                RequiredActionScope = actionScope,
            });
    }

    private static string? ResolveAcceptedActionScope(string? tokenActionScope)
    {
        if (string.IsNullOrWhiteSpace(tokenActionScope))
        {
            return null;
        }

        if (ScheduledRuleEvaluationService.AcceptedActionScopes.Contains(
                tokenActionScope,
                StringComparer.OrdinalIgnoreCase))
        {
            return tokenActionScope;
        }

        return tokenActionScope
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(scope =>
                ScheduledRuleEvaluationService.AcceptedActionScopes.Contains(scope, StringComparer.OrdinalIgnoreCase));
    }
}
