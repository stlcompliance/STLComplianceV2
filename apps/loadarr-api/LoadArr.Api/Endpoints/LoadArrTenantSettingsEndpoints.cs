using LoadArr.Api.Settings;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace LoadArr.Api.Endpoints;

public static class LoadArrTenantSettingsEndpoints
{
    public static void MapLoadArrTenantSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/loadarr/tenant-settings")
            .WithTags("TenantSettings")
            .RequireAuthorization();

        group.MapGet("/", async (
            LoadArrAuthorizationService authorization,
            LoadArrTenantSettingsService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsRead(context.User);
            return await ExecuteAsync(async () =>
                Results.Ok(await service.GetCurrentAsync(context.User.GetTenantId(), context.User, cancellationToken)));
        })
        .WithName("GetLoadArrTenantSettings");

        group.MapPut("/", async (
            LoadArrTenantSettingsReplaceRequest request,
            LoadArrAuthorizationService authorization,
            LoadArrTenantSettingsService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsUpdate(context.User);
            return await ExecuteAsync(async () =>
                Results.Ok(await service.ReplaceAsync(context.User.GetTenantId(), context.User, request, cancellationToken)));
        })
        .WithName("ReplaceLoadArrTenantSettings");

        group.MapPatch("/{sectionKey}", async (
            string sectionKey,
            LoadArrTenantSettingsSectionPatchRequest request,
            LoadArrAuthorizationService authorization,
            LoadArrTenantSettingsService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsUpdate(context.User);
            return await ExecuteAsync(async () =>
                Results.Ok(await service.PatchSectionAsync(context.User.GetTenantId(), context.User, sectionKey, request, cancellationToken)));
        })
        .WithName("PatchLoadArrTenantSettingsSection");

        group.MapPost("/{sectionKey}/reset", async (
            string sectionKey,
            LoadArrTenantSettingsResetRequest request,
            LoadArrAuthorizationService authorization,
            LoadArrTenantSettingsService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsReset(context.User);
            return await ExecuteAsync(async () =>
                Results.Ok(await service.ResetSectionAsync(context.User.GetTenantId(), context.User, sectionKey, request, cancellationToken)));
        })
        .WithName("ResetLoadArrTenantSettingsSection");

        group.MapPost("/reset", async (
            LoadArrTenantSettingsFullResetRequest request,
            LoadArrAuthorizationService authorization,
            LoadArrTenantSettingsService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsReset(context.User);
            return await ExecuteAsync(async () =>
                Results.Ok(await service.ResetAllAsync(context.User.GetTenantId(), context.User, request, cancellationToken)));
        })
        .WithName("ResetLoadArrTenantSettings");

        group.MapGet("/audit", async (
            int? limit,
            int? offset,
            LoadArrAuthorizationService authorization,
            LoadArrTenantSettingsService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsAuditRead(context.User);
            return await ExecuteAsync(async () =>
                Results.Ok(await service.ListAuditAsync(context.User.GetTenantId(), limit, offset, cancellationToken)));
        })
        .WithName("ListLoadArrTenantSettingsAudit");

        group.MapGet("/options", (
            LoadArrAuthorizationService authorization,
            LoadArrTenantSettingsService service,
            HttpContext context) =>
        {
            authorization.RequireTenantSettingsRead(context.User);
            return Results.Ok(service.GetOptions());
        })
        .WithName("GetLoadArrTenantSettingsOptions");

        group.MapGet("/export", async (
            LoadArrAuthorizationService authorization,
            LoadArrTenantSettingsService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTenantSettingsAuditRead(context.User);
            return await ExecuteAsync(async () =>
                Results.Ok(await service.ExportAsync(context.User.GetTenantId(), context.User, cancellationToken)));
        })
        .WithName("ExportLoadArrTenantSettings");

        group.MapPost("/validate", (
            LoadArrTenantSettingsSections settings,
            LoadArrAuthorizationService authorization,
            LoadArrTenantSettingsService service,
            HttpContext context) =>
        {
            authorization.RequireTenantSettingsRead(context.User);
            return Results.Ok(service.Validate(settings));
        })
        .WithName("ValidateLoadArrTenantSettings");
    }

    private static async Task<IResult> ExecuteAsync(Func<Task<IResult>> action)
    {
        try
        {
            return await action();
        }
        catch (LoadArrTenantSettingsValidationException ex)
        {
            return Results.Json(
                new LoadArrTenantSettingsProblemResponse(ex.Code, ex.Message, ex.Validation),
                statusCode: ex.StatusCode);
        }
        catch (LoadArrTenantSettingsRequestException ex)
        {
            return Results.Json(
                new LoadArrTenantSettingsProblemResponse(ex.Code, ex.Message),
                statusCode: ex.StatusCode);
        }
        catch (StlApiException ex)
        {
            return Results.Json(
                new LoadArrTenantSettingsProblemResponse(ex.Code, ex.Message),
                statusCode: ex.StatusCode);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Authenticated principal", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Json(
                new LoadArrTenantSettingsProblemResponse("auth.invalid_claims", "Authenticated principal is missing required tenant or actor context."),
                statusCode: 401);
        }
    }
}
