using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.SmartImport;

namespace STLCompliance.Shared.Endpoints;

public static class StlSmartImportAdapterEndpoints
{
    private static readonly HashSet<string> CommitBlockedProducts = new(StringComparer.OrdinalIgnoreCase)
    {
        "fieldcompanion",
        "ordarr"
    };

    public static void MapStlSmartImportAdapterEndpoints(this WebApplication app)
    {
        var product = app.Services.GetRequiredService<ProductDescriptor>();
        var group = app.MapGroup("/api/v1/integrations/smart-import")
            .WithTags("Smart Import Integrations");

        group.MapPost("/{entityType}/validate", (
            string entityType,
            SmartImportDestinationValidateRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator) =>
        {
            ValidateServiceToken(context, tokenValidator, product.ProductKey, "platform.smart_import.validate", request.TenantId);

            var reviewReasons = BuildRequiredReviewReasons(product.ProductKey, entityType, request.Operation, request.RequiredReviewReasons());
            var response = new SmartImportDestinationValidateResponse(
                Valid: !CommitBlockedProducts.Contains(product.ProductKey),
                RequiredPermissions: [$"{product.ProductKey}.smart_import.validate"],
                MissingPermissions: [],
                FieldResults: [],
                MatchCandidates: [],
                RequiredReviewReasons: reviewReasons,
                DeterministicPayload: request.ProposedPayload,
                Warnings: reviewReasons.Contains(SmartImportReviewReasons.UnsupportedProductApi, StringComparer.OrdinalIgnoreCase)
                    ? ["This destination does not expose a product-specific Smart Import commit adapter yet."]
                    : ["Destination product accepted the Smart Import payload for human review; final write still requires product-specific authorization."]);

            return Results.Ok(response);
        }).WithName($"Validate{product.ProductKey}SmartImport");

        group.MapPost("/{entityType}/commit", (
            string entityType,
            SmartImportDestinationCommitRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator) =>
        {
            ValidateServiceToken(context, tokenValidator, product.ProductKey, "platform.smart_import.commit", request.TenantId);

            return Results.Ok(new SmartImportDestinationCommitResponse(
                Status: "review_required",
                ResultEntityId: null,
                DisplayName: null,
                Links: [],
                Retryable: false,
                ErrorCode: "destination_commit_adapter_not_implemented",
                ErrorMessage: "This product has not implemented a domain-specific Smart Import commit handler yet. The proposed record remains in Smart Import review and was not written."));
        }).WithName($"Commit{product.ProductKey}SmartImport");
    }

    private static void ValidateServiceToken(
        HttpContext context,
        StlServiceTokenValidator tokenValidator,
        string targetProduct,
        string actionScope,
        Guid tenantId)
    {
        var token = ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization);
        tokenValidator.ValidateOrThrow(token, new ServiceTokenRequirements
        {
            ExpectedSourceProduct = "nexarr",
            RequiredTargetProduct = targetProduct,
            TenantId = tenantId,
            RequiredActionScope = actionScope
        });
    }

    private static IReadOnlyList<string> BuildRequiredReviewReasons(
        string productKey,
        string entityType,
        string operation,
        IReadOnlyList<string> incoming)
    {
        var reasons = new HashSet<string>(incoming, StringComparer.OrdinalIgnoreCase);
        if (CommitBlockedProducts.Contains(productKey))
        {
            reasons.Add(SmartImportReviewReasons.UnsupportedProductApi);
        }

        if (string.Equals(productKey, "compliancecore", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.ComplianceCoreImport);
        }

        if (string.Equals(productKey, "staffarr", StringComparison.OrdinalIgnoreCase)
            && entityType.Contains("person", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.PersonCreateOrLink);
        }

        if (string.Equals(productKey, "trainarr", StringComparison.OrdinalIgnoreCase)
            && (entityType.Contains("training", StringComparison.OrdinalIgnoreCase)
                || entityType.Contains("cert", StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add(SmartImportReviewReasons.TrainingOrCertificationRecord);
        }

        if (string.Equals(operation, "update", StringComparison.OrdinalIgnoreCase)
            || string.Equals(operation, "overwrite", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.Overwrite);
        }

        return reasons.Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyList<string> RequiredReviewReasons(this SmartImportDestinationValidateRequest request) =>
        request.ReviewStatus.Contains("review", StringComparison.OrdinalIgnoreCase)
            ? [SmartImportReviewReasons.HumanConfirmationRequired]
            : [];
}
