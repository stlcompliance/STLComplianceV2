using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class TenantSettingsEndpoints
{
    public static void MapStaffArrTenantSettingsEndpoints(this WebApplication app)
    {
        var groups = new[]
        {
            (Group: app.MapGroup("/api/v1/staffarr/tenant-settings"), Suffix: "V1"),
            (Group: app.MapGroup("/api/staffarr/tenant-settings"), Suffix: string.Empty),
        };

        foreach (var (group, suffix) in groups)
        {
            group.WithTags("TenantSettings").RequireAuthorization();

            group.MapGet("/", async (
                HttpContext context,
                StaffArrAuthorizationService authorization,
                StaffArrTenantSettingsService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireTenantSettingsView(context.User);
                return Results.Ok(await service.GetAsync(context.User.GetTenantId(), cancellationToken));
            })
            .WithName($"GetStaffArrTenantSettings{suffix}");

            group.MapGet("/defaults", (
                HttpContext context,
                StaffArrAuthorizationService authorization,
                StaffArrTenantSettingsService service) =>
            {
                authorization.RequireTenantSettingsView(context.User);
                return Results.Ok(service.GetDefaults(context.User.GetTenantId()));
            })
            .WithName($"GetStaffArrTenantSettingsDefaults{suffix}");

            group.MapPut("/", async (
                UpsertStaffArrTenantSettingsRequest request,
                HttpContext context,
                StaffArrAuthorizationService authorization,
                StaffArrTenantSettingsService service,
                CancellationToken cancellationToken) =>
            {
                authorization.RequireTenantSettingsManage(context.User);
                return Results.Ok(await service.UpsertAsync(
                    context.User.GetTenantId(),
                    context.User.GetUserId(),
                    request,
                    cancellationToken));
            })
            .WithName($"UpdateStaffArrTenantSettings{suffix}");
        }
    }
}
