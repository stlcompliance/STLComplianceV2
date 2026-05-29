using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class DispatchEndpoints
{
    public static void MapRoutArrDispatchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/dispatch").WithTags("Dispatch").RequireAuthorization();

        group.MapGet("/command-center", async (
            string? scope,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchCommandCenterService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            return Results.Ok(await service.GetAsync(context.User, scope, cancellationToken));
        })
        .WithName("GetDispatchCommandCenter");

        group.MapGet("/board-state", async (
            HttpContext context,
            DispatchBoardStateService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetAsync(context.User, cancellationToken));
        })
        .WithName("GetDispatchBoardState");

        group.MapPut("/board-state", async (
            UpsertDispatchBoardStateRequest request,
            HttpContext context,
            DispatchBoardStateService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpsertAsync(context.User, request, cancellationToken));
        })
        .WithName("UpsertDispatchBoardState");

        group.MapGet("/driver-refs", async (
            HttpContext context,
            RoutArrAuthorizationService authorization,
            StaffarrPersonRefService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListAsync(tenantId, cancellationToken));
        })
        .WithName("ListStaffarrPersonRefs");

        group.MapPut("/driver-refs", async (
            UpsertStaffarrPersonRefRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            StaffarrPersonRefService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.UpsertAsync(tenantId, actorUserId, request, cancellationToken));
        })
        .WithName("UpsertStaffarrPersonRef");

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

        group.MapGet("/active-trips", async (
            string? scope,
            bool? attentionOnly,
            string? statusFilter,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            ActiveTripsService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            return Results.Ok(await service.GetAsync(
                context.User,
                scope,
                attentionOnly == true,
                statusFilter,
                cancellationToken));
        })
        .WithName("GetActiveTrips");

        group.MapGet("/unassigned-work-queue", async (
            string? scope,
            bool? attentionOnly,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            UnassignedWorkQueueService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchBoardRead(context.User);
            return Results.Ok(await service.GetAsync(
                context.User,
                scope,
                attentionOnly == true,
                cancellationToken));
        })
        .WithName("GetUnassignedWorkQueue");

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

        group.MapPost("/assignments/bulk-preview", async (
            DispatchBoardBulkAssignmentPreviewRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchAssignmentService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var response = await service.BulkPreviewAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Ok(response);
        })
        .WithName("PreviewDispatchBoardBulkAssignment");

        group.MapGet("/assignments/audit", async (
            int? limit,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchAssignmentService service,
            IRoutArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var response = await service.ListAuditAsync(tenantId, limit ?? 25, cancellationToken);

            await audit.WriteAsync(
                DispatchAssignmentService.AuditListAction,
                tenantId,
                actorUserId,
                "dispatch_assignment",
                "audit",
                $"{response.Entries.Count} entries",
                cancellationToken: cancellationToken);

            return Results.Ok(response);
        })
        .WithName("ListDispatchAssignmentAudit");

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

        group.MapGet("/closeout/checklists", async (
            string? scope,
            string? remainingTripDisposition,
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
            var response = await service.GetChecklistsAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                scope,
                remainingTripDisposition,
                cancellationToken);

            await audit.WriteAsync(
                DispatchCloseoutService.ChecklistsAction,
                tenantId,
                actorUserId,
                "dispatch_closeout",
                response.Scope,
                $"{response.Trips.Count} trips",
                cancellationToken: cancellationToken);

            return Results.Ok(response);
        })
        .WithName("GetDispatchCloseoutChecklists");

        group.MapGet("/closeout/audit", async (
            int? limit,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchCloseoutService service,
            IRoutArrAuditService audit,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireTripsAssign(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var response = await service.ListAuditAsync(tenantId, limit ?? 25, cancellationToken);

            await audit.WriteAsync(
                DispatchCloseoutService.AuditListAction,
                tenantId,
                actorUserId,
                "dispatch_closeout",
                "audit",
                $"{response.Entries.Count} entries",
                cancellationToken: cancellationToken);

            return Results.Ok(response);
        })
        .WithName("ListDispatchCloseoutAudit");

        group.MapGet("/exceptions", async (
            string? status,
            bool? overdueOnly,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionRead(context.User);
            return Results.Ok(await service.ListOpenAsync(
                context.User,
                status,
                overdueOnly == true,
                cancellationToken));
        })
        .WithName("ListDispatchExceptions");

        group.MapGet("/exceptions/resolution-templates", (
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service) =>
        {
            authorization.RequireDispatchExceptionRead(context.User);
            return Results.Ok(service.ListResolutionTemplates());
        })
        .WithName("ListDispatchExceptionResolutionTemplates");

        group.MapPost("/exceptions/bulk/assign", async (
            BulkAssignDispatchExceptionsRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionTriage(context.User);
            return Results.Ok(await service.BulkAssignAsync(context.User, request, cancellationToken));
        })
        .WithName("BulkAssignDispatchExceptions");

        group.MapPost("/exceptions/bulk/resolve", async (
            BulkResolveDispatchExceptionsRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionTriage(context.User);
            return Results.Ok(await service.BulkResolveAsync(context.User, request, cancellationToken));
        })
        .WithName("BulkResolveDispatchExceptions");

        group.MapPost("/exceptions", async (
            CreateDispatchExceptionRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionTriage(context.User);
            return Results.Ok(await service.CreateAsync(context.User, request, cancellationToken));
        })
        .WithName("CreateDispatchException");

        group.MapPatch("/exceptions/{exceptionId:guid}/assign", async (
            Guid exceptionId,
            AssignDispatchExceptionRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionTriage(context.User);
            return Results.Ok(
                await service.AssignAsync(context.User, exceptionId, request, cancellationToken));
        })
        .WithName("AssignDispatchException");

        group.MapPatch("/exceptions/{exceptionId:guid}/resolve", async (
            Guid exceptionId,
            ResolveDispatchExceptionRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionTriage(context.User);
            return Results.Ok(
                await service.ResolveAsync(context.User, exceptionId, request, cancellationToken));
        })
        .WithName("ResolveDispatchException");

        group.MapPatch("/exceptions/{exceptionId:guid}/link-trip", async (
            Guid exceptionId,
            LinkDispatchExceptionTripRequest request,
            HttpContext context,
            RoutArrAuthorizationService authorization,
            DispatchExceptionService service,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireDispatchExceptionTriage(context.User);
            return Results.Ok(
                await service.LinkTripAsync(context.User, exceptionId, request, cancellationToken));
        })
        .WithName("LinkDispatchExceptionTrip");
    }
}
