using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class RoutArrTenantSettingsEndpoints
{
    public static void MapRoutArrTenantSettingsEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/tenant-settings"), string.Empty);
        MapRoutes(app.MapGroup("/api/v1/tenant-settings"), "V1");
    }

    private static void MapRoutes(RouteGroupBuilder group, string nameSuffix)
    {
        group.WithTags("RoutArrTenantSettings").RequireAuthorization();

        group.MapGet("/effective", async (
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrAccess(context.User);
            return Results.Ok(await settingsService.GetEffectiveAsync(
                context.User.GetTenantId(),
                [],
                context.User.GetPersonId().ToString(),
                cancellationToken));
        })
        .WithName($"GetEffectiveRoutArrTenantSettings{nameSuffix}");

        group.MapPost("/preview", async (
            PreviewRoutArrEffectiveSettingsRequest request,
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrSettingsPreview(context.User);
            return Results.Ok(await settingsService.GetEffectiveAsync(
                context.User.GetTenantId(),
                request.Scopes,
                context.User.GetPersonId().ToString(),
                cancellationToken));
        })
        .WithName($"PreviewEffectiveRoutArrTenantSettings{nameSuffix}");

        group.MapGet("/editable", async (
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrSettingsRead(context.User);
            return Results.Ok(await settingsService.GetEditableAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId().ToString(),
                cancellationToken));
        })
        .WithName($"GetEditableRoutArrTenantSettings{nameSuffix}");

        group.MapGet("/options", (
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context) =>
        {
            authorization.RequireRoutArrSettingsRead(context.User);
            return Results.Ok(settingsService.GetOptions());
        })
        .WithName($"GetRoutArrTenantSettingsOptions{nameSuffix}");

        group.MapPost("/validate", async (
            ValidateRoutArrTenantSettingGroupRequest request,
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrSettingsWrite(context.User);
            return Results.Ok(await settingsService.ValidateGroupAsync(
                context.User.GetTenantId(),
                request,
                context.User.GetPersonId().ToString(),
                cancellationToken));
        })
        .WithName($"ValidateRoutArrTenantSettings{nameSuffix}");

        group.MapPut("/groups/{settingGroup}", async (
            string settingGroup,
            UpdateRoutArrTenantSettingGroupRequest request,
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrSettingsWrite(context.User);
            return Results.Ok(await settingsService.UpdateGroupAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId().ToString(),
                settingGroup,
                request,
                cancellationToken));
        })
        .WithName($"UpdateRoutArrTenantSettingGroup{nameSuffix}");

        group.MapPost("/groups/{settingGroup}/reset", async (
            string settingGroup,
            ResetRoutArrTenantSettingGroupRequest request,
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrSettingsReset(context.User);
            return Results.Ok(await settingsService.ResetGroupAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId().ToString(),
                settingGroup,
                request,
                cancellationToken));
        })
        .WithName($"ResetRoutArrTenantSettingGroup{nameSuffix}");

        group.MapGet("/audit", async (
            string? settingGroup,
            int? limit,
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrSettingsAuditRead(context.User);
            return Results.Ok(await settingsService.ListAuditHistoryAsync(
                context.User.GetTenantId(),
                settingGroup,
                limit,
                cancellationToken));
        })
        .WithName($"ListRoutArrTenantSettingAuditHistory{nameSuffix}");

        group.MapPost("/overrides", async (
            CreateRoutArrTenantSettingOverrideRequest request,
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrSettingsOverrideWrite(context.User);
            var created = await settingsService.CreateOverrideAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId().ToString(),
                request,
                cancellationToken);
            return Results.Created($"/api/v1/tenant-settings/overrides/{created.OverrideKey}", created);
        })
        .WithName($"CreateRoutArrTenantSettingOverride{nameSuffix}");

        group.MapPut("/overrides/{overrideKey}", async (
            string overrideKey,
            UpdateRoutArrTenantSettingOverrideRequest request,
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrSettingsOverrideWrite(context.User);
            return Results.Ok(await settingsService.UpdateOverrideAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId().ToString(),
                overrideKey,
                request,
                cancellationToken));
        })
        .WithName($"UpdateRoutArrTenantSettingOverride{nameSuffix}");

        group.MapDelete("/overrides/{overrideKey}", async (
            string overrideKey,
            RoutArrAuthorizationService authorization,
            RoutArrTenantSettingsService settingsService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRoutArrSettingsOverrideWrite(context.User);
            await settingsService.DeleteOverrideAsync(
                context.User.GetTenantId(),
                context.User.GetPersonId().ToString(),
                overrideKey,
                cancellationToken);
            return Results.NoContent();
        })
        .WithName($"DeleteRoutArrTenantSettingOverride{nameSuffix}");
    }
}
