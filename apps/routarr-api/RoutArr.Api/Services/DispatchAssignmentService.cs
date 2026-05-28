using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public sealed class DispatchAssignmentService(
    RoutArrDbContext db,
    DriverEligibilityService driverEligibility,
    AssetDispatchabilityService assetDispatchability,
    DispatchWorkflowGateService dispatchWorkflowGates,
    IRoutArrAuditService audit)
{
    public const string PreviewAction = "dispatch_assignment.preview";

    public const string BulkPreviewAction = "dispatch_assignment.bulk_preview";

    public const string AuditListAction = "dispatch_assignment.audit.list";

    public const int MaxBulkPreviewItems = 50;

    public static class AssignmentKinds
    {
        public const string Driver = "driver";
        public const string Vehicle = "vehicle";

        public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
        {
            Driver,
            Vehicle,
        };
    }

    public async Task<DispatchAssignmentPreviewResponse> PreviewAsync(
        Guid tenantId,
        DispatchAssignmentPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var trip = await GetTripAsync(tenantId, request.TripId, cancellationToken);
        var kind = NormalizeKind(request.AssignmentKind);

        return kind switch
        {
            AssignmentKinds.Driver => DispatchAssignmentValidationRules.ApplyValidation(
                trip,
                await PreviewDriverAsync(tenantId, trip, request.DriverPersonId, cancellationToken)),
            AssignmentKinds.Vehicle => DispatchAssignmentValidationRules.ApplyValidation(
                trip,
                await PreviewVehicleAsync(tenantId, trip, request.VehicleRefKey, cancellationToken)),
            _ => throw new StlApiException(
                "dispatch.assignment_invalid_kind",
                "Assignment kind must be driver or vehicle.",
                400),
        };
    }

    public async Task<DispatchBoardBulkAssignmentPreviewResponse> BulkPreviewAsync(
        Guid tenantId,
        Guid? actorUserId,
        DispatchBoardBulkAssignmentPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var items = request.Items?.Where(x => x.TripId != Guid.Empty).ToList() ?? [];
        if (items.Count == 0)
        {
            throw new StlApiException(
                "dispatch.assignment_bulk_empty",
                "At least one assignment item is required.",
                400);
        }

        if (items.Count > MaxBulkPreviewItems)
        {
            throw new StlApiException(
                "dispatch.assignment_bulk_too_many",
                $"Bulk assignment preview supports at most {MaxBulkPreviewItems} items.",
                400);
        }

        var previews = new List<DispatchBoardBulkAssignmentItemPreview>();
        foreach (var item in items)
        {
            var preview = await PreviewAsync(
                tenantId,
                new DispatchAssignmentPreviewRequest(
                    item.TripId,
                    item.AssignmentKind,
                    item.DriverPersonId,
                    item.VehicleRefKey),
                cancellationToken);
            previews.Add(new DispatchBoardBulkAssignmentItemPreview(
                item.TripId,
                preview.AssignmentKind,
                preview));
        }

        var response = new DispatchBoardBulkAssignmentPreviewResponse(
            previews.Count,
            previews.Count(x => x.Preview.CanAssign),
            previews.Count(x => !x.Preview.CanAssign),
            previews);

        await audit.WriteAsync(
            BulkPreviewAction,
            tenantId,
            actorUserId,
            "dispatch_assignment",
            "bulk",
            $"{response.CanAssignCount}/{response.ItemCount} can assign",
            cancellationToken: cancellationToken);

        return response;
    }

    public async Task<DispatchAssignmentAuditListResponse> ListAuditAsync(
        Guid tenantId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        limit = limit switch
        {
            < 1 => 25,
            > 100 => 100,
            _ => limit,
        };

        var entries = await db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && (x.Action.StartsWith("dispatch_assignment")
                    || x.Action == "trip.assign_driver"
                    || x.Action == "trip.assign_vehicle"))
            .OrderByDescending(x => x.OccurredAt)
            .Take(limit)
            .Select(x => new DispatchAssignmentAuditEntry(
                x.Id,
                x.ActorUserId,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        return new DispatchAssignmentAuditListResponse(entries);
    }

    public async Task EnsureDriverAssignmentAllowedAsync(
        Guid tenantId,
        Trip trip,
        string driverPersonId,
        bool ignoreAvailabilityConflicts,
        bool ignoreEligibilityBlocks = false,
        bool ignoreWorkflowGateBlocks = false,
        CancellationToken cancellationToken = default)
    {
        var preview = await PreviewDriverAsync(tenantId, trip, driverPersonId, cancellationToken);
        if (!ignoreAvailabilityConflicts && HasAvailabilityBlockingConflicts(preview))
        {
            ThrowBlocked(preview);
        }

        await driverEligibility.EnsureDriverEligibleAsync(
            tenantId,
            driverPersonId,
            ignoreEligibilityBlocks,
            cancellationToken);

        await dispatchWorkflowGates.EnsureWorkflowGatesAllowedAsync(
            tenantId,
            trip,
            AssignmentKinds.Driver,
            driverPersonId,
            vehicleRefKey: null,
            ignoreWorkflowGateBlocks,
            cancellationToken);
    }

    public async Task EnsureVehicleAssignmentAllowedAsync(
        Guid tenantId,
        Trip trip,
        string? vehicleRefKey,
        bool ignoreAvailabilityConflicts,
        bool ignoreDispatchabilityBlocks = false,
        bool ignoreWorkflowGateBlocks = false,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            await assetDispatchability.EnsureAssetDispatchableAsync(
                tenantId,
                vehicleRefKey,
                assetTag: null,
                ignoreDispatchabilityBlocks,
                cancellationToken);
        }

        await dispatchWorkflowGates.EnsureWorkflowGatesAllowedAsync(
            tenantId,
            trip,
            AssignmentKinds.Vehicle,
            driverPersonId: null,
            vehicleRefKey,
            ignoreWorkflowGateBlocks,
            cancellationToken);

        if (ignoreAvailabilityConflicts || string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            return;
        }

        var availabilityPreview = await PreviewVehicleAvailabilityAsync(tenantId, trip, vehicleRefKey, cancellationToken);
        if (availabilityPreview.HasBlockingConflicts)
        {
            ThrowBlocked(availabilityPreview);
        }
    }

    private async Task<DispatchAssignmentPreviewResponse> PreviewDriverAsync(
        Guid tenantId,
        Trip trip,
        string? driverPersonId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(driverPersonId))
        {
            throw new StlApiException("trip.driver_required", "Driver person id is required.", 400);
        }

        var normalizedPersonId = driverPersonId.Trim();
        var availability = await db.DriverAvailabilities
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.PersonId == normalizedPersonId)
            .ToListAsync(cancellationToken);

        var trips = await LoadActiveTripsForTenantAsync(tenantId, cancellationToken);

        var blockingAvailability = DispatchAssignmentRules
            .FindBlockingDriverAvailability(
                normalizedPersonId,
                trip.ScheduledStartAt,
                trip.ScheduledEndAt,
                availability)
            .Select(MapDriverConflict)
            .ToList();

        var overlappingTrips = DispatchAssignmentRules
            .FindOverlappingDriverTrips(trip, normalizedPersonId, trips)
            .Select(MapTripConflict)
            .ToList();

        var hasBlocking = blockingAvailability.Count > 0 || overlappingTrips.Count > 0;

        var preview = new DispatchAssignmentPreviewResponse(
            trip.Id,
            AssignmentKinds.Driver,
            CanAssign: !hasBlocking,
            hasBlocking,
            blockingAvailability,
            [],
            overlappingTrips);

        var eligibility = await driverEligibility.CheckAsync(
            tenantId,
            actorUserId: null,
            normalizedPersonId,
            cancellationToken: cancellationToken);

        var withEligibility = DriverEligibilityRules.ApplyEligibility(preview, eligibility);

        var workflowGates = await dispatchWorkflowGates.CheckForTripAsync(
            tenantId,
            actorUserId: null,
            trip,
            AssignmentKinds.Driver,
            normalizedPersonId,
            vehicleRefKey: null,
            cancellationToken);

        return DispatchWorkflowGateRules.ApplyWorkflowGates(withEligibility, workflowGates);
    }

    private async Task<DispatchAssignmentPreviewResponse> PreviewVehicleAsync(
        Guid tenantId,
        Trip trip,
        string? vehicleRefKey,
        CancellationToken cancellationToken)
    {
        var preview = await PreviewVehicleAvailabilityAsync(tenantId, trip, vehicleRefKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            return preview;
        }

        var dispatchability = await assetDispatchability.CheckAsync(
            tenantId,
            actorUserId: null,
            vehicleRefKey.Trim(),
            assetTag: null,
            cancellationToken);

        var withDispatchability = AssetDispatchabilityRules.ApplyDispatchability(preview, dispatchability);

        var workflowGates = await dispatchWorkflowGates.CheckForTripAsync(
            tenantId,
            actorUserId: null,
            trip,
            AssignmentKinds.Vehicle,
            driverPersonId: null,
            vehicleRefKey.Trim(),
            cancellationToken);

        return DispatchWorkflowGateRules.ApplyWorkflowGates(withDispatchability, workflowGates);
    }

    private async Task<DispatchAssignmentPreviewResponse> PreviewVehicleAvailabilityAsync(
        Guid tenantId,
        Trip trip,
        string? vehicleRefKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(vehicleRefKey))
        {
            return new DispatchAssignmentPreviewResponse(
                trip.Id,
                AssignmentKinds.Vehicle,
                CanAssign: true,
                HasBlockingConflicts: false,
                [],
                [],
                []);
        }

        var normalizedKey = vehicleRefKey.Trim();
        var availability = await db.EquipmentAvailabilities
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.VehicleRefKey == normalizedKey)
            .ToListAsync(cancellationToken);

        var trips = await LoadActiveTripsForTenantAsync(tenantId, cancellationToken);

        var blockingAvailability = DispatchAssignmentRules
            .FindBlockingEquipmentAvailability(
                normalizedKey,
                trip.ScheduledStartAt,
                trip.ScheduledEndAt,
                availability)
            .Select(MapEquipmentConflict)
            .ToList();

        var overlappingTrips = DispatchAssignmentRules
            .FindOverlappingVehicleTrips(trip, normalizedKey, trips)
            .Select(MapTripConflict)
            .ToList();

        var hasBlocking = blockingAvailability.Count > 0 || overlappingTrips.Count > 0;

        return new DispatchAssignmentPreviewResponse(
            trip.Id,
            AssignmentKinds.Vehicle,
            CanAssign: !hasBlocking,
            hasBlocking,
            [],
            blockingAvailability,
            overlappingTrips);
    }

    private async Task<Trip> GetTripAsync(Guid tenantId, Guid tripId, CancellationToken cancellationToken)
    {
        var trip = await db.Trips
            .AsNoTracking()
            .Include(x => x.Loads)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == tripId, cancellationToken);

        if (trip is null)
        {
            throw new StlApiException("trip.not_found", "Trip was not found.", 404);
        }

        return trip;
    }

    private async Task<List<Trip>> LoadActiveTripsForTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken) =>
        await db.Trips
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

    private static string NormalizeKind(string? kind)
    {
        var normalized = kind?.Trim() ?? string.Empty;
        if (!AssignmentKinds.All.Contains(normalized))
        {
            throw new StlApiException(
                "dispatch.assignment_invalid_kind",
                "Assignment kind must be driver or vehicle.",
                400);
        }

        return normalized.ToLowerInvariant();
    }

    private static bool HasAvailabilityBlockingConflicts(DispatchAssignmentPreviewResponse preview) =>
        preview.BlockingDriverAvailability.Count > 0
        || preview.BlockingEquipmentAvailability.Count > 0
        || preview.OverlappingTrips.Count > 0;

    private static void ThrowBlocked(DispatchAssignmentPreviewResponse preview) =>
        throw new StlApiException(
            "dispatch.assignment_blocked",
            "Assignment is blocked by availability or overlapping trips.",
            409,
            preview);

    private static DispatchAssignmentAvailabilityConflict MapDriverConflict(DriverAvailability record) =>
        new(
            record.Id,
            record.AvailabilityStatus,
            record.StartsAt,
            record.EndsAt,
            record.Reason);

    private static DispatchAssignmentAvailabilityConflict MapEquipmentConflict(EquipmentAvailability record) =>
        new(
            record.Id,
            record.AvailabilityStatus,
            record.StartsAt,
            record.EndsAt,
            record.Reason);

    private static DispatchAssignmentTripConflict MapTripConflict(Trip trip) =>
        new(
            trip.Id,
            trip.TripNumber,
            trip.Title,
            trip.DispatchStatus,
            trip.ScheduledStartAt,
            trip.ScheduledEndAt);
}
