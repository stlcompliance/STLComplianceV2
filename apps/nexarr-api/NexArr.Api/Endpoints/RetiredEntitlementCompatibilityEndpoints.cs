using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Endpoints;

public static class RetiredEntitlementCompatibilityEndpoints
{
    private const string RetiredCompatibilityTag = "RetiredCompatibility";
    private const string RetirementCode = "entitlements.retired";
    private const string RetirementMessage =
        "Legacy entitlement management is retired. Product access now follows active tenant membership, product operational state, and product-local permissions.";
    private const string RetirementSummary = "Retired entitlement compatibility endpoint.";

    public static void MapRetiredEntitlementCompatibilityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/entitlements").WithTags(RetiredCompatibilityTag).RequireAuthorization();
        var v1TenantEntitlements = app.MapGroup("/api/v1/tenants/{tenantId:guid}/entitlements").WithTags(RetiredCompatibilityTag).RequireAuthorization();
        var v1Entitlements = app.MapGroup("/api/v1/entitlements").WithTags(RetiredCompatibilityTag).RequireAuthorization();

        group.MapGet("/", RetiredAsync)
            .WithName("ListRetiredEntitlementCompatibility")
            .WithSummary(RetirementSummary)
            .WithDescription(RetirementMessage);
        group.MapPost("/", RetiredAsync)
            .WithName("CreateRetiredEntitlementCompatibility")
            .WithSummary(RetirementSummary)
            .WithDescription(RetirementMessage);
        group.MapPost("/{entitlementId:guid}/revoke", RetiredAsync)
            .WithName("RevokeRetiredEntitlementCompatibility")
            .WithSummary(RetirementSummary)
            .WithDescription(RetirementMessage);

        v1TenantEntitlements.MapGet("/", RetiredAsync)
            .WithName("ListRetiredTenantEntitlementCompatibilityV1")
            .WithSummary(RetirementSummary)
            .WithDescription(RetirementMessage);
        v1TenantEntitlements.MapPost("/", RetiredAsync)
            .WithName("CreateRetiredTenantEntitlementCompatibilityV1")
            .WithSummary(RetirementSummary)
            .WithDescription(RetirementMessage);
        v1TenantEntitlements.MapPatch("/{productKey}", RetiredAsync)
            .WithName("UpdateRetiredTenantEntitlementCompatibilityV1")
            .WithSummary(RetirementSummary)
            .WithDescription(RetirementMessage);
        v1TenantEntitlements.MapDelete("/{productKey}", RetiredAsync)
            .WithName("DeleteRetiredTenantEntitlementCompatibilityV1")
            .WithSummary(RetirementSummary)
            .WithDescription(RetirementMessage);

        app.MapGet("/api/v1/platform/tenants/{tenantId:guid}/entitlements/{productKey}", RetiredAsync)
            .WithTags(RetiredCompatibilityTag)
            .RequireAuthorization()
            .WithName("CheckRetiredPlatformTenantEntitlementCompatibilityV1")
            .WithSummary(RetirementSummary)
            .WithDescription(RetirementMessage);

        v1Entitlements.MapGet("/check", RetiredAsync)
            .WithName("CheckRetiredEntitlementCompatibilityV1")
            .WithSummary(RetirementSummary)
            .WithDescription(RetirementMessage);
    }

    private static async Task<IResult> RetiredAsync(
        HttpContext context,
        PlatformAuthorizationService authorization,
        IPlatformAuditService audit,
        CancellationToken cancellationToken)
    {
        await authorization.RequireNexArrAccessAsync(context.User, cancellationToken);

        await audit.WriteAsync(
            "entitlement.endpoint.retired",
            "entitlement_endpoint",
            context.Request.Path,
            "Denied",
            tenantId: context.User.GetTenantId(),
            actorUserId: context.User.GetUserId(),
            reasonCode: RetirementCode,
            cancellationToken: cancellationToken);

        throw new StlApiException(RetirementCode, RetirementMessage, 410);
    }
}
