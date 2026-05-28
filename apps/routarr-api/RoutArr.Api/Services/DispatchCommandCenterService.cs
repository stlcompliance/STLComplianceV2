using System.Security.Claims;
using RoutArr.Api.Contracts;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Services;

public sealed class DispatchCommandCenterService(
    DispatchBoardStateService boardStateService,
    DispatchBoardService boardService,
    TripService tripService,
    StaffarrPersonRefService personRefService,
    RoutArrAuthorizationService authorization,
    IRoutArrAuditService audit)
{
    public const string ReadAction = "dispatch_command_center.read";

    private static readonly (string Status, string Label)[] CommandCenterColumns =
    [
        (TripDispatchStatuses.Planned, "Planned"),
        (TripDispatchStatuses.Assigned, "Assigned"),
        (TripDispatchStatuses.Dispatched, "Dispatched"),
        (TripDispatchStatuses.InProgress, "In progress"),
    ];

    public async Task<DispatchCommandCenterResponse> GetAsync(
        ClaimsPrincipal principal,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        authorization.RequireDispatchBoardRead(principal);
        var tenantId = principal.GetTenantId();
        var viewAll = authorization.CanViewAllTrips(principal);
        var actorUserId = principal.GetUserId();
        var actorPersonId = principal.GetPersonId().ToString();

        var boardStateEntity = await boardStateService.LoadOrCreateAsync(tenantId, cancellationToken);
        var effectiveScope = string.IsNullOrWhiteSpace(scope)
            ? boardStateEntity.DefaultScope
            : scope;

        var boardState = new DispatchBoardStateResponse(
            boardStateEntity.DefaultScope,
            boardStateEntity.UpdatedAt,
            boardStateEntity.UpdatedByUserId);

        var board = await boardService.GetBoardAsync(
            tenantId,
            viewAll,
            actorUserId,
            actorPersonId,
            effectiveScope,
            cancellationToken);

        var tripColumns = new List<DispatchCommandCenterTripColumn>();
        foreach (var (status, label) in CommandCenterColumns)
        {
            var trips = await tripService.ListAsync(
                tenantId,
                viewAll,
                actorUserId,
                actorPersonId,
                status,
                cancellationToken);
            tripColumns.Add(new DispatchCommandCenterTripColumn(status, label, trips.Count, trips));
        }

        var driverRefs = await personRefService.ListAsync(tenantId, cancellationToken);

        var canAssign = authorization.CanAssignTrips(principal);
        var actions = BuildActions(canAssign);

        await audit.WriteAsync(
            ReadAction,
            tenantId,
            actorUserId,
            "dispatch_command_center",
            effectiveScope,
            $"{tripColumns.Sum(x => x.Count)} trips",
            cancellationToken: cancellationToken);

        return new DispatchCommandCenterResponse(
            DateTimeOffset.UtcNow,
            board.Scope,
            boardState,
            board,
            tripColumns,
            driverRefs,
            actions);
    }

    private static IReadOnlyList<DispatchCommandCenterActionDescriptor> BuildActions(bool canAssign)
    {
        var actions = new List<DispatchCommandCenterActionDescriptor>
        {
            new(
                "list_trips",
                "List trips",
                "/api/trips",
                "GET",
                "Filter trips by dispatch status."),
            new(
                "dispatch_board",
                "Dispatch board",
                "/api/dispatch/board",
                "GET",
                "Daily or weekly board aggregates."),
        };

        if (canAssign)
        {
            actions.Add(new(
                "assign_driver",
                "Assign driver",
                "/api/trips/{tripId}/assign-driver",
                "PATCH",
                "Assign StaffArr person id to trip; optional display name upserts mirror."));
            actions.Add(new(
                "update_status",
                "Update dispatch status",
                "/api/trips/{tripId}/status",
                "PATCH",
                "Transition trip through planned → assigned → dispatched → in_progress."));
            actions.Add(new(
                "bulk_dispatch",
                "Bulk dispatch",
                "/api/dispatch/bulk/apply",
                "POST",
                "Apply bulk assignment and status changes."));
        }

        return actions;
    }
}
