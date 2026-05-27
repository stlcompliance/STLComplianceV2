using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class ReadinessRollupEndpoints
{
    public static void MapStaffArrReadinessRollupEndpoints(this WebApplication app)
    {
        var rollups = app.MapGroup("/api/readiness-rollups")
            .WithTags("ReadinessRollups")
            .RequireAuthorization();

        rollups.MapGet("/teams", async (
            Guid? siteOrgUnitId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessRollupService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListTeamRollupsAsync(tenantId, siteOrgUnitId, cancellationToken));
        })
        .WithName("ListTeamReadinessRollups");

        rollups.MapGet("/sites", async (
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessRollupService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListSiteRollupsAsync(tenantId, cancellationToken));
        })
        .WithName("ListSiteReadinessRollups");

        rollups.MapGet("/teams/{teamOrgUnitId:guid}", async (
            Guid teamOrgUnitId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessRollupService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetRollupAsync(
                tenantId,
                ReadinessRollupRules.TeamScope,
                teamOrgUnitId,
                cancellationToken));
        })
        .WithName("GetTeamReadinessRollup");

        rollups.MapGet("/sites/{siteOrgUnitId:guid}", async (
            Guid siteOrgUnitId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessRollupService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetRollupAsync(
                tenantId,
                ReadinessRollupRules.SiteScope,
                siteOrgUnitId,
                cancellationToken));
        })
        .WithName("GetSiteReadinessRollup");
    }
}
