using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class InternalPlatformIdentityEndpoints
{
    private static readonly HashSet<string> ReadAllowedSources = new(StringComparer.OrdinalIgnoreCase)
    {
        "staffarr",
        "trainarr",
        "maintainarr",
        "routarr",
        "supplyarr",
        "compliancecore",
    };

    public static void MapNexArrInternalPlatformIdentityEndpoints(this WebApplication app)
    {
        app.MapGet("/api/internal/platform-identities/{personId:guid}", async (
            Guid personId,
            Guid tenantId,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PlatformIdentityIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            var token = ValidateServiceToken(
                tokenValidator,
                context,
                tenantId,
                PlatformIdentityIntegrationService.ReadIdentityActionScope,
                requireStaffArrSource: false);

            return Results.Ok(await service.ResolveAsync(personId, tenantId, token.SourceProductKey, cancellationToken));
        })
        .WithName("InternalResolvePlatformIdentity")
        .WithTags("Internal");

        app.MapPost("/api/internal/platform-identities", async (
            CreatePlatformIdentityRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PlatformIdentityIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            var token = ValidateServiceToken(
                tokenValidator,
                context,
                request.TenantId,
                PlatformIdentityIntegrationService.CreateIdentityActionScope,
                requireStaffArrSource: true);

            var result = await service.CreateMinimalAsync(request, token.SourceProductKey, cancellationToken);
            return result.WasCreated
                ? Results.Created($"/api/internal/platform-identities/{result.Identity.PersonId}", result)
                : Results.Ok(result);
        })
        .WithName("InternalCreatePlatformIdentity")
        .WithTags("Internal");

        app.MapPut("/api/internal/platform-identities/{personId:guid}", async (
            Guid personId,
            SyncPlatformIdentityRequest request,
            HttpContext context,
            StlServiceTokenValidator tokenValidator,
            PlatformIdentityIntegrationService service,
            CancellationToken cancellationToken) =>
        {
            var token = ValidateServiceToken(
                tokenValidator,
                context,
                request.TenantId,
                PlatformIdentityIntegrationService.CreateIdentityActionScope,
                requireStaffArrSource: true);

            return Results.Ok(await service.SyncAsync(personId, request, token.SourceProductKey, cancellationToken));
        })
        .WithName("InternalSyncPlatformIdentity")
        .WithTags("Internal");
    }

    private static ValidatedServiceToken ValidateServiceToken(
        StlServiceTokenValidator tokenValidator,
        HttpContext context,
        Guid tenantId,
        string actionScope,
        bool requireStaffArrSource)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(
            context.Request.Headers.Authorization.ToString());

        var preview = tokenValidator.TryValidate(bearer);
        if (preview is null)
        {
            throw new StlApiException("auth.service_token_invalid", "Service token is invalid.", 401);
        }

        if (requireStaffArrSource
            && !string.Equals(preview.SourceProductKey, "staffarr", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Only StaffArr is authorized to create platform identities.",
                403);
        }

        if (!requireStaffArrSource && !ReadAllowedSources.Contains(preview.SourceProductKey))
        {
            throw new StlApiException(
                "auth.service_token_scope",
                "Service token source product is not authorized to resolve platform identities.",
                403);
        }

        return tokenValidator.ValidateOrThrow(
            bearer,
            new ServiceTokenRequirements
            {
                ExpectedSourceProduct = preview.SourceProductKey,
                RequiredTargetProduct = "nexarr",
                TenantId = tenantId,
                RequiredActionScope = actionScope,
            });
    }
}
