using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class VocabularyEndpoints
{
    public static void MapComplianceCoreVocabularyEndpoints(this WebApplication app)
    {
        MapVocabularyRoutes(
            app.MapGroup("/api/vocabulary")
                .WithTags("Vocabulary")
                .RequireAuthorization(),
            string.Empty);
        MapVocabularyRoutes(
            app.MapGroup("/api/v1/vocabulary")
                .WithTags("Vocabulary")
                .RequireAuthorization(),
            "V1Vocabulary");
    }

    private static void MapVocabularyRoutes(RouteGroupBuilder vocabulary, string nameSuffix)
    {
        vocabulary.MapGet("/types", async (
            ComplianceCoreAuthorizationService authorization,
            VocabularyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVocabularyRead(context.User);
            return Results.Ok(await service.ListTypesAsync(cancellationToken));
        })
        .WithName($"ListVocabularyTypes{nameSuffix}");

        vocabulary.MapGet("/", async (
            string? vocabularyTypeKey,
            ComplianceCoreAuthorizationService authorization,
            VocabularyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVocabularyRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListTermsAsync(tenantId, vocabularyTypeKey, cancellationToken));
        })
        .WithName($"ListVocabularyTerms{nameSuffix}");

        vocabulary.MapPost("/", async (
            CreateVocabularyTermRequest request,
            ComplianceCoreAuthorizationService authorization,
            VocabularyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVocabularyManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateTermAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/vocabulary/{created.TermId}", created);
        })
        .WithName($"CreateVocabularyTerm{nameSuffix}");

        vocabulary.MapPost("/aliases", async (
            CreateVocabularyAliasRequest request,
            ComplianceCoreAuthorizationService authorization,
            VocabularyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVocabularyManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateAliasAsync(
                tenantId,
                context.User.GetUserId(),
                request,
                cancellationToken);
            return Results.Created($"/api/vocabulary/aliases/{created.AliasId}", created);
        })
        .WithName($"CreateVocabularyAlias{nameSuffix}");
    }
}
