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

        vocabulary.MapGet("/core-keys", (
            ComplianceCoreAuthorizationService authorization,
            HttpContext context) =>
        {
            authorization.RequireVocabularyRead(context.User);
            return Results.Ok(CoreVocabularyKeyCatalog.GetRegistry());
        })
        .WithName($"ListCoreVocabularyKeys{nameSuffix}");

        vocabulary.MapPost("/core-keys/validate", (
            ValidateCoreVocabularyKeysRequest request,
            ComplianceCoreAuthorizationService authorization,
            HttpContext context) =>
        {
            authorization.RequireVocabularyRead(context.User);
            return Results.Ok(CoreVocabularyKeyCatalog.Validate(request.Keys));
        })
        .WithName($"ValidateCoreVocabularyKeys{nameSuffix}");

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

        vocabulary.MapPost("/validate-keys", async (
            ValidateVocabularyKeysRequest request,
            ComplianceCoreAuthorizationService authorization,
            VocabularyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVocabularyRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ValidateKeysAsync(tenantId, request, cancellationToken));
        })
        .WithName($"ValidateVocabularyKeys{nameSuffix}");

        vocabulary.MapGet("/{family}", async (
            string family,
            ComplianceCoreAuthorizationService authorization,
            VocabularyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVocabularyRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListTermsAsync(tenantId, family, cancellationToken));
        })
        .WithName($"ListVocabularyTermsByFamily{nameSuffix}");

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

        vocabulary.MapPost("/{family}", async (
            string family,
            CreateVocabularyTermRequest request,
            ComplianceCoreAuthorizationService authorization,
            VocabularyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVocabularyManage(context.User);
            var tenantId = context.User.GetTenantId();
            var created = await service.CreateTermForFamilyAsync(
                tenantId,
                context.User.GetUserId(),
                family,
                request,
                cancellationToken);
            return Results.Created($"/api/v1/vocabulary/{created.VocabularyTypeKey}/{created.TermKey}", created);
        })
        .WithName($"CreateVocabularyTermByFamily{nameSuffix}");

        vocabulary.MapPatch("/{family}/{key}", async (
            string family,
            string key,
            UpdateVocabularyTermRequest request,
            ComplianceCoreAuthorizationService authorization,
            VocabularyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVocabularyManage(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.UpdateTermForFamilyAsync(
                tenantId,
                context.User.GetUserId(),
                family,
                key,
                request,
                cancellationToken));
        })
        .WithName($"UpdateVocabularyTermByFamily{nameSuffix}");

        vocabulary.MapGet("/{family}/{key}/usage", async (
            string family,
            string key,
            ComplianceCoreAuthorizationService authorization,
            VocabularyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVocabularyRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetUsageForFamilyAsync(tenantId, family, key, cancellationToken));
        })
        .WithName($"GetVocabularyTermUsage{nameSuffix}");

        vocabulary.MapGet("/{family}/{key}/history", async (
            string family,
            string key,
            ComplianceCoreAuthorizationService authorization,
            VocabularyService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireVocabularyRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListHistoryForFamilyAsync(tenantId, family, key, cancellationToken));
        })
        .WithName($"ListVocabularyTermHistory{nameSuffix}");

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
