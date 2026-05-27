using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class DispatchEndpoints
{
    public static void MapRoutArrDispatchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dispatch").WithTags("Dispatch").RequireAuthorization();

        group.MapGet("/board", async (
            string? scope,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchBoardService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var response = await service.GetBoardAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                scope,
                cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetDispatchBoard");

        group.MapGet("/calendar", async (
            string? scope,
            string? start,
            string? end,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            RouteCalendarService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRouteCalendarRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var response = await service.GetCalendarAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                scope,
                start,
                end,
                cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetRouteCalendar");

        group.MapGet("/driver-availability", async (
            string? scope,
            string? start,
            string? end,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DriverAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDriverAvailabilityRead(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorPersonId = context.User.GetPersonId().ToString();
            var response = await service.GetPanelAsync(
                tenantId,
                viewAll,
                actorPersonId,
                scope,
                start,
                end,
                cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetDriverAvailabilityPanel");

        group.MapGet("/equipment-availability", async (
            string? scope,
            string? start,
            string? end,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            EquipmentAvailabilityService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireEquipmentAvailabilityRead(context.User);
            var tenantId = context.User.GetTenantId();
            var response = await service.GetPanelAsync(
                tenantId,
                scope,
                start,
                end,
                cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetEquipmentAvailabilityPanel");

        group.MapPost("/assignments/preview", async (
            DispatchAssignmentPreviewRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchAssignmentService service,
            IRoutArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var preview = await service.PreviewAsync(tenantId, request, cancellationToken);

            await audit.WriteAsync(
                DispatchAssignmentService.PreviewAction,
                tenantId,
                actorUserId,
                "trip",
                request.TripId.ToString(),
                request.AssignmentKind,
                cancellationToken: cancellationToken);

            return Results.Ok(preview);
        })
        .WithName("PreviewDispatchAssignment");

        group.MapPost("/bulk/preview", async (
            BulkDispatchPreviewRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            BulkDispatchService service,
            IRoutArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var canManageAny = authorization.CanViewAllTrips(context.User)
                && !string.Equals(context.User.GetTenantRoleKey(), "routarr_driver", StringComparison.OrdinalIgnoreCase);

            var preview = await service.PreviewAsync(tenantId, request, canManageAny, cancellationToken);

            await audit.WriteAsync(
                BulkDispatchService.PreviewAction,
                tenantId,
                actorUserId,
                "dispatch_bulk",
                preview.Summary.Total.ToString(),
                $"{preview.Summary.CanApplyCount}/{preview.Summary.Total}",
                cancellationToken: cancellationToken);

            return Results.Ok(preview);
        })
        .WithName("PreviewBulkDispatch");

        group.MapPost("/bulk/apply", async (
            BulkDispatchApplyRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            BulkDispatchService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var canManageAny = authorization.CanViewAllTrips(context.User)
                && !string.Equals(context.User.GetTenantRoleKey(), "routarr_driver", StringComparison.OrdinalIgnoreCase);

            if (request.Items.Any(item =>
                    !string.IsNullOrWhiteSpace(item.DispatchStatus)
                    && string.Equals(item.DispatchStatus, "cancelled", StringComparison.OrdinalIgnoreCase)))
            {
                authorization.RequireTripsManage(context.User);
                canManageAny = true;
            }

            var response = await service.ApplyAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                canManageAny,
                request,
                cancellationToken);
            return Results.Ok(response);
        })
        .WithName("ApplyBulkDispatch");

        group.MapGet("/closeout/summary", async (
            string? scope,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchCloseoutService service,
            IRoutArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var response = await service.GetSummaryAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                scope,
                cancellationToken);

            await audit.WriteAsync(
                DispatchCloseoutService.SummaryAction,
                tenantId,
                actorUserId,
                "dispatch_closeout",
                response.Scope,
                $"{response.Counts.OpenTrips} trips",
                cancellationToken: cancellationToken);

            return Results.Ok(response);
        })
        .WithName("GetDispatchCloseoutSummary");

        group.MapPost("/closeout/preview", async (
            DispatchCloseoutRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchCloseoutService service,
            IRoutArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var viewAll = authorization.CanViewAllTrips(context.User);
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();

            var response = await service.PreviewAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                request,
                cancellationToken);

            await audit.WriteAsync(
                DispatchCloseoutService.PreviewAction,
                tenantId,
                actorUserId,
                "dispatch_closeout",
                response.Scope,
                $"{response.Summary.TripsCanApply}/{response.Summary.TripCount} trips",
                cancellationToken: cancellationToken);

            return Results.Ok(response);
        })
        .WithName("PreviewDispatchCloseout");

        group.MapPost("/closeout/apply", async (
            DispatchCloseoutRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchCloseoutService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var actorPersonId = context.User.GetPersonId().ToString();
            var viewAll = authorization.CanViewAllTrips(context.User);

            var response = await service.ApplyAsync(
                tenantId,
                actorUserId,
                actorPersonId,
                viewAll,
                request,
                cancellationToken);
            return Results.Ok(response);
        })
        .WithName("ApplyDispatchCloseout");
    }
}
