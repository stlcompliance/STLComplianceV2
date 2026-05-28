using SupplyArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Endpoints;

public static class ApprovalReminderEndpoints
{
    public static void MapSupplyArrApprovalReminderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/approval-reminders")
            .WithTags("ApprovalReminders")
            .RequireAuthorization();

        group.MapGet("/", async (
            bool? includeUpcoming,
            SupplyArrAuthorizationService authorization,
            ApprovalReminderService reminderService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireApprovalReminderRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await reminderService.GetDashboardAsync(
                tenantId,
                includeUpcoming ?? false,
                cancellationToken));
        })
        .WithName("GetSupplyArrApprovalRemindersDashboard");
    }
}
