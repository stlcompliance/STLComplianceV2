using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class ReferenceEndpoints
{
    public static void MapMaintainArrReferenceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/references").WithTags("References").RequireAuthorization();

        group.MapGet("/compliance-core/catalogs/{catalogKey}", async (string catalogKey, HttpContext context, MaintainArrAuthorizationService authorization, ComplianceCoreReferenceAdapter adapter, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await adapter.GetOptionsAsync(context.User.GetTenantId(), catalogKey, cancellationToken));
        });

        group.MapGet("/compliance-core/catalogs", async (HttpContext context, MaintainArrAuthorizationService authorization, ComplianceCoreReferenceAdapter adapter, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            var keys = new[] { "governingBody", "rulepackApplicabilityKeys", "regulatoryAssetType", "complianceCategory", "requiredEvidenceType", "documentRequirementType", "inspectionRequirementType" };
            var data = new Dictionary<string, object>();
            foreach (var key in keys)
            {
                data[key] = await adapter.GetOptionsAsync(context.User.GetTenantId(), key, cancellationToken);
            }
            return Results.Ok(data);
        });

        group.MapGet("/sites", async (HttpContext context, MaintainArrAuthorizationService authorization, StaffArrReferenceAdapter adapter, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await adapter.GetOptionsAsync(context.User.GetTenantId(), "sites", cancellationToken));
        });

        group.MapGet("/departments", async (HttpContext context, MaintainArrAuthorizationService authorization, StaffArrReferenceAdapter adapter, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await adapter.GetOptionsAsync(context.User.GetTenantId(), "departments", cancellationToken));
        });

        group.MapGet("/teams", async (HttpContext context, MaintainArrAuthorizationService authorization, StaffArrReferenceAdapter adapter, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await adapter.GetOptionsAsync(context.User.GetTenantId(), "teams", cancellationToken));
        });

        group.MapGet("/people", async (HttpContext context, MaintainArrAuthorizationService authorization, StaffArrReferenceAdapter adapter, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await adapter.GetOptionsAsync(context.User.GetTenantId(), "people", cancellationToken));
        });

        group.MapGet("/vendors", async (HttpContext context, MaintainArrAuthorizationService authorization, SupplyArrReferenceAdapter adapter, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await adapter.GetOptionsAsync(context.User.GetTenantId(), "vendors", cancellationToken));
        });

        group.MapGet("/customers", async (HttpContext context, MaintainArrAuthorizationService authorization, SupplyArrReferenceAdapter adapter, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await adapter.GetOptionsAsync(context.User.GetTenantId(), "customers", cancellationToken));
        });

        group.MapGet("/parts", async (HttpContext context, MaintainArrAuthorizationService authorization, SupplyArrReferenceAdapter adapter, CancellationToken cancellationToken) =>
        {
            authorization.RequireAssetsRead(context.User);
            return Results.Ok(await adapter.GetOptionsAsync(context.User.GetTenantId(), "parts", cancellationToken));
        });
    }
}
