using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class MaintainArrTenantSettingsEndpoints
{
    public static void MapMaintainArrTenantSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/settings/tenant/maintainarr")
            .WithTags("MaintainArrTenantSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            MaintainArrAuthorizationService authorization,
            MaintainArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsRead(context.User);
            var tenantId = context.User.GetTenantId();
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await settingsService.GetOrCreateAsync(tenantId, actorPersonId, cancellationToken));
        });

        group.MapPut("/", async (
            UpsertMaintainArrTenantSettingsRequest request,
            MaintainArrAuthorizationService authorization,
            MaintainArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsUpdate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await settingsService.UpsertAsync(tenantId, actorPersonId, request, cancellationToken));
        });

        group.MapGet("/audit", async (
            int? limit,
            MaintainArrAuthorizationService authorization,
            MaintainArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsAuditRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await settingsService.ListAuditAsync(tenantId, limit, cancellationToken));
        });

        group.MapPost("/reset-to-defaults", async (
            ResetMaintainArrTenantSettingsRequest? request,
            MaintainArrAuthorizationService authorization,
            MaintainArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsReset(context.User);
            var tenantId = context.User.GetTenantId();
            var actorPersonId = context.User.GetPersonId().ToString();
            return Results.Ok(await settingsService.ResetToDefaultsAsync(
                tenantId,
                actorPersonId,
                request ?? new ResetMaintainArrTenantSettingsRequest(null),
                cancellationToken));
        });
    }
}
