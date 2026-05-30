using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class ReadinessRollupEndpoints
{
    public static void MapStaffArrReadinessRollupEndpoints(this WebApplication app)
    {
        static void MapRoutes(RouteGroupBuilder rollups, string suffix)
        {
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
        .WithName($"ListTeamReadinessRollups{suffix}");

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
        .WithName($"ListSiteReadinessRollups{suffix}");

            rollups.MapGet("/departments", async (
            Guid? siteOrgUnitId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessRollupService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListDepartmentRollupsAsync(tenantId, siteOrgUnitId, cancellationToken));
        })
        .WithName($"ListDepartmentReadinessRollups{suffix}");

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
        .WithName($"GetTeamReadinessRollup{suffix}");

            rollups.MapGet("/teams/{teamOrgUnitId:guid}/members", async (
            Guid teamOrgUnitId,
            string? readinessStatus,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessRollupService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListMembersAsync(
                tenantId,
                ReadinessRollupRules.TeamScope,
                teamOrgUnitId,
                readinessStatus,
                cancellationToken));
        })
        .WithName($"ListTeamReadinessRollupMembers{suffix}");

            rollups.MapGet("/departments/{departmentOrgUnitId:guid}", async (
            Guid departmentOrgUnitId,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessRollupService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetRollupAsync(
                tenantId,
                ReadinessRollupRules.DepartmentScope,
                departmentOrgUnitId,
                cancellationToken));
        })
        .WithName($"GetDepartmentReadinessRollup{suffix}");

            rollups.MapGet("/departments/{departmentOrgUnitId:guid}/members", async (
            Guid departmentOrgUnitId,
            string? readinessStatus,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessRollupService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListMembersAsync(
                tenantId,
                ReadinessRollupRules.DepartmentScope,
                departmentOrgUnitId,
                readinessStatus,
                cancellationToken));
        })
        .WithName($"ListDepartmentReadinessRollupMembers{suffix}");

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
        .WithName($"GetSiteReadinessRollup{suffix}");

            rollups.MapGet("/sites/{siteOrgUnitId:guid}/members", async (
            Guid siteOrgUnitId,
            string? readinessStatus,
            HttpContext context,
            StaffArrAuthorizationService authorization,
            ReadinessRollupService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessRollupRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListMembersAsync(
                tenantId,
                ReadinessRollupRules.SiteScope,
                siteOrgUnitId,
                readinessStatus,
                cancellationToken));
        })
        .WithName($"ListSiteReadinessRollupMembers{suffix}");
        }

        MapRoutes(
            app.MapGroup("/api/readiness-rollups")
                .WithTags("ReadinessRollups")
                .RequireAuthorization(),
            string.Empty);

        MapRoutes(
            app.MapGroup("/api/v1/readiness-rollups")
                .WithTags("ReadinessRollups")
                .RequireAuthorization(),
            "V1");
    }
}
