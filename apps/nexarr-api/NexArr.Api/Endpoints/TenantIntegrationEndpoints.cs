using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class TenantIntegrationEndpoints
{
    public static void MapTenantIntegrationEndpoints(this WebApplication app)
    {
        var catalog = app.MapGroup("/api/v1/integrations")
            .WithTags("TenantIntegrations")
            .RequireAuthorization();

        catalog.MapGet("/catalog", (TenantIntegrationService service) =>
            service.GetCatalogAsync())
            .WithName("ListTenantIntegrationCatalog");

        var tenantIntegrations = app.MapGroup("/api/v1/tenants/{tenantId:guid}/integrations")
            .WithTags("TenantIntegrations")
            .RequireAuthorization();

        tenantIntegrations.MapGet("/", async (
            Guid tenantId,
            string? providerKey,
            int? page,
            int? pageSize,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListAsync(
                context.User,
                tenantId,
                providerKey,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                platformScope: false,
                cancellationToken));
        })
        .WithName("ListTenantIntegrations");

        tenantIntegrations.MapGet("/{providerKey}", async (
            Guid tenantId,
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetAsync(context.User, tenantId, providerKey, cancellationToken));
        })
        .WithName("GetTenantIntegration");

        tenantIntegrations.MapPut("/{providerKey}", async (
            Guid tenantId,
            string providerKey,
            UpsertTenantIntegrationConnectionRequest request,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpsertAsync(
                context.User,
                tenantId,
                providerKey,
                request,
                cancellationToken));
        })
        .WithName("UpsertTenantIntegration");

        tenantIntegrations.MapPut("/{providerKey}/credentials", async (
            Guid tenantId,
            string providerKey,
            UpsertTenantIntegrationCredentialRequest request,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpsertCredentialAsync(
                context.User,
                tenantId,
                providerKey,
                request,
                cancellationToken));
        })
        .WithName("UpsertTenantIntegrationCredential");

        tenantIntegrations.MapDelete("/{providerKey}/credentials", async (
            Guid tenantId,
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            await service.DeleteCredentialAsync(context.User, tenantId, providerKey, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteTenantIntegrationCredential");

        tenantIntegrations.MapPost("/{providerKey}/test", async (
            Guid tenantId,
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.TestAsync(context.User, tenantId, providerKey, cancellationToken));
        })
        .WithName("TestTenantIntegration");

        tenantIntegrations.MapPost("/{providerKey}/sync-runs", async (
            Guid tenantId,
            string providerKey,
            TriggerTenantIntegrationSyncRequest request,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.CreateSyncRunAsync(
                context.User,
                tenantId,
                providerKey,
                request,
                cancellationToken));
        })
        .WithName("TriggerTenantIntegrationSync");

        tenantIntegrations.MapGet("/{providerKey}/sync-runs", async (
            Guid tenantId,
            string providerKey,
            int? limit,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListSyncRunsAsync(
                context.User,
                tenantId,
                providerKey,
                limit is null or 0 ? 10 : limit.Value,
                cancellationToken));
        })
        .WithName("ListTenantIntegrationSyncRuns");

        tenantIntegrations.MapGet("/{providerKey}/mappings", async (
            Guid tenantId,
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListMappingTemplatesAsync(
                context.User,
                tenantId,
                providerKey,
                cancellationToken));
        })
        .WithName("ListTenantIntegrationMappings");

        tenantIntegrations.MapPut("/{providerKey}/mappings", async (
            Guid tenantId,
            string providerKey,
            UpsertTenantIntegrationMappingTemplateRequest request,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpsertMappingTemplateAsync(
                context.User,
                tenantId,
                providerKey,
                request,
                cancellationToken));
        })
        .WithName("UpsertTenantIntegrationMapping");

        tenantIntegrations.MapDelete("/{providerKey}/mappings/{mappingTemplateId:guid}", async (
            Guid tenantId,
            string providerKey,
            Guid mappingTemplateId,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            await service.DeleteMappingTemplateAsync(
                context.User,
                tenantId,
                providerKey,
                mappingTemplateId,
                cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteTenantIntegrationMapping");

        tenantIntegrations.MapGet("/{providerKey}/external-mappings", async (
            Guid tenantId,
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListExternalMappingsAsync(
                context.User,
                tenantId,
                providerKey,
                cancellationToken));
        })
        .WithName("ListTenantIntegrationExternalMappings");

        tenantIntegrations.MapPut("/{providerKey}/external-mappings", async (
            Guid tenantId,
            string providerKey,
            UpsertTenantIntegrationExternalMappingRequest request,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpsertExternalMappingAsync(
                context.User,
                tenantId,
                providerKey,
                request,
                cancellationToken));
        })
        .WithName("UpsertTenantIntegrationExternalMapping");

        var platformAdmin = app.MapGroup("/api/platform-admin/integrations")
            .WithTags("TenantIntegrations")
            .RequireAuthorization();

        platformAdmin.MapGet("/", async (
            Guid? tenantId,
            string? providerKey,
            int? page,
            int? pageSize,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.ListAsync(
                context.User,
                tenantId,
                providerKey,
                page is null or 0 ? 1 : page.Value,
                pageSize is null or 0 ? 50 : pageSize.Value,
                platformScope: true,
                cancellationToken));
        })
        .WithName("PlatformAdminListTenantIntegrations");

        MapExternalIntegrationRoutes(app);
        MapInternalIntegrationRoutes(app);
    }

    private static void MapExternalIntegrationRoutes(WebApplication app)
    {
        static IResult IdentityProtocolNotReady(string protocol) => Results.Problem(
            title: "Identity protocol endpoint is not production-ready",
            detail: $"{protocol} setup is retained R12 scope. No sign-in, provisioning, account, or tenant record was created or changed.",
            statusCode: StatusCodes.Status501NotImplemented);

        var external = app.MapGroup("/api/v1/integrations")
            .WithTags("TenantIntegrationIntake")
            .AllowAnonymous();

        external.MapMethods("/{providerKey}/oauth/callback", ["GET", "POST"], async (
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RecordCallbackAsync(providerKey, context, cancellationToken));
        })
        .WithName("TenantIntegrationOAuthCallback");

        external.MapMethods("/{providerKey}/oidc/callback", ["GET", "POST"], async (
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            await Task.CompletedTask;
            return IdentityProtocolNotReady("OIDC callback");
        })
        .WithName("TenantIntegrationOidcCallback");

        external.MapGet("/{providerKey}/saml/metadata", (
            string providerKey,
            TenantIntegrationService service) =>
        {
            return IdentityProtocolNotReady("SAML metadata");
        })
        .WithName("TenantIntegrationSamlMetadata");

        external.MapPost("/{providerKey}/saml/acs", async (
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            await Task.CompletedTask;
            return IdentityProtocolNotReady("SAML assertion consumer");
        })
        .WithName("TenantIntegrationSamlAcs");

        external.MapMethods("/{providerKey}/scim/{*path}", ["GET", "POST", "PUT", "PATCH", "DELETE"], async (
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            await Task.CompletedTask;
            return IdentityProtocolNotReady("SCIM provisioning");
        })
        .WithName("TenantIntegrationScim");

        external.MapPost("/{providerKey}/webhooks/{*path}", async (
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RecordExternalIntakeAsync(
                providerKey,
                "webhook",
                context,
                cancellationToken));
        })
        .WithName("TenantIntegrationWebhook");

        external.MapPost("/{providerKey}/as2/receive", async (
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RecordExternalIntakeAsync(
                providerKey,
                "as2",
                context,
                cancellationToken));
        })
        .WithName("TenantIntegrationAs2Receive");

        external.MapPost("/{providerKey}/sftp/intake", async (
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RecordExternalIntakeAsync(
                providerKey,
                "sftp",
                context,
                cancellationToken));
        })
        .WithName("TenantIntegrationSftpIntake");

        external.MapPost("/{providerKey}/csv-xlsx/import", async (
            string providerKey,
            HttpContext context,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.RecordExternalIntakeAsync(
                providerKey,
                "csv_xlsx",
                context,
                cancellationToken));
        })
        .WithName("TenantIntegrationCsvXlsxImport");
    }

    private static void MapInternalIntegrationRoutes(WebApplication app)
    {
        var internalApi = app.MapGroup("/api/internal/integrations")
            .WithTags("Internal");

        internalApi.MapPost("/process-batch", async (
            ProcessTenantIntegrationSyncRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            TenantIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            ValidateServiceToken(tokenValidator, context);
            return Results.Ok(await service.ProcessBatchAsync(request, cancellationToken));
        })
        .WithName("InternalProcessTenantIntegrationSync");
    }

    private static void ValidateServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        var preview = tokenValidator.TryValidate(bearer);
        if (preview is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        if (!string.Equals(preview.SourceProductKey, "nexarr-worker", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized for tenant integration sync.",
                403);
        }

        tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = "nexarr-worker",
                RequiredTargetProduct = "nexarr",
                TenantId = preview.TenantScope ?? Guid.Empty,
                RequiredActionScope = TenantIntegrationService.ProcessSyncActionScope,
            });
    }
}
