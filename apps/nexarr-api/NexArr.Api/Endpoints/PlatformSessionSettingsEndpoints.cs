using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class PlatformSessionSettingsEndpoints
{
    public static void MapPlatformSessionSettingsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/platform-admin/session-settings")
            .WithTags("PlatformAdmin")
            .RequireAuthorization();

        group.MapGet("", async (
            HttpContext context,
            PlatformSessionSettingsService settingsService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await settingsService.GetAsync(context.User, cancellationToken));
        })
        .WithName("GetPlatformSessionSettings");

        group.MapPut("", async (
            UpsertPlatformSessionSettingsRequest request,
            HttpContext context,
            PlatformSessionSettingsService settingsService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await settingsService.UpsertAsync(context.User, request, cancellationToken));
        })
        .WithName("UpsertPlatformSessionSettings");
    }
}
