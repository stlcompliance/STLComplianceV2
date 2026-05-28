using RoutArr.Api.Contracts;

using RoutArr.Api.Data;

using RoutArr.Api.Entities;

using Microsoft.EntityFrameworkCore;

using STLCompliance.Shared.Contracts;



namespace RoutArr.Api.Services;



public sealed class BulkDispatchService(

    RoutArrDbContext db,

    TripService tripService,

    DriverEligibilityService driverEligibility,

    AssetDispatchabilityService assetDispatchability,

    DispatchWorkflowGateService dispatchWorkflowGates,

    IRoutArrAuditService audit)

{

    public const string PreviewAction = "dispatch_bulk.preview";

    public const string ApplyAction = "dispatch_bulk.apply";



    public async Task<BulkDispatchPreviewResponse> PreviewAsync(

        Guid tenantId,

        BulkDispatchPreviewRequest request,

        bool canManageAny,

        CancellationToken cancellationToken = default)

    {

        var items = ValidateItems(request.Items);

        var tripsById = await LoadTripsByIdAsync(tenantId, items, cancellationToken);

        var allTrips = await LoadActiveTripsForTenantAsync(tenantId, cancellationToken);

        var simulatedTrips = allTrips

            .Select(BulkDispatchRules.CloneForSimulation)

            .ToDictionary(x => x.Id);



        var driverAvailability = await db.DriverAvailabilities

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId)

            .ToListAsync(cancellationToken);



        var equipmentAvailability = await db.EquipmentAvailabilities

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId)

            .ToListAsync(cancellationToken);



        var previews = new List<BulkDispatchItemPreview>();



        foreach (var item in items)

        {

            if (!simulatedTrips.TryGetValue(item.TripId, out var trip))

            {

                previews.Add(new BulkDispatchItemPreview(

                    item.TripId,

                    string.Empty,

                    string.Empty,

                    string.Empty,

                    CanApply: false,

                    HasBlockingConflicts: true,

                    null,

                    null,

                    new BulkDispatchStatusPreview(

                        item.DispatchStatus,

                        false,

                        "trip.not_found",

                        "Trip was not found.")));

                continue;

            }



            var preview = PreviewItem(

                item,

                trip,

                simulatedTrips.Values,

                driverAvailability,

                equipmentAvailability,

                canManageAny);

            if (preview.DriverPreview is not null

                && !string.IsNullOrWhiteSpace(item.DriverPersonId))

            {

                var eligibility = await driverEligibility.CheckAsync(

                    tenantId,

                    actorUserId: null,

                    item.DriverPersonId.Trim(),

                    cancellationToken: cancellationToken);

                var mergedDriverPreview = DriverEligibilityRules.ApplyEligibility(

                    preview.DriverPreview,

                    eligibility);

                preview = preview with { DriverPreview = mergedDriverPreview };

            }

            if (preview.VehiclePreview is not null

                && item.VehicleRefKey is not null

                && !string.IsNullOrWhiteSpace(item.VehicleRefKey))

            {

                var dispatchability = await assetDispatchability.CheckAsync(

                    tenantId,

                    actorUserId: null,

                    item.VehicleRefKey.Trim(),

                    assetTag: null,

                    cancellationToken);

                var mergedVehiclePreview = AssetDispatchabilityRules.ApplyDispatchability(

                    preview.VehiclePreview,

                    dispatchability);

                preview = preview with { VehiclePreview = mergedVehiclePreview };

            }

            if (tripsById.TryGetValue(item.TripId, out var tripForGates))

            {

                if (preview.DriverPreview is not null

                    && !string.IsNullOrWhiteSpace(item.DriverPersonId))

                {

                    var driverWorkflowGates = await dispatchWorkflowGates.CheckForTripAsync(

                        tenantId,

                        actorUserId: null,

                        tripForGates,

                        DispatchAssignmentService.AssignmentKinds.Driver,

                        item.DriverPersonId.Trim(),

                        vehicleRefKey: null,

                        cancellationToken);

                    var mergedDriverWorkflow = DispatchWorkflowGateRules.ApplyWorkflowGates(

                        preview.DriverPreview,

                        driverWorkflowGates);

                    preview = preview with { DriverPreview = mergedDriverWorkflow };

                }

                if (preview.VehiclePreview is not null

                    && item.VehicleRefKey is not null

                    && !string.IsNullOrWhiteSpace(item.VehicleRefKey))

                {

                    var vehicleWorkflowGates = await dispatchWorkflowGates.CheckForTripAsync(

                        tenantId,

                        actorUserId: null,

                        tripForGates,

                        DispatchAssignmentService.AssignmentKinds.Vehicle,

                        driverPersonId: null,

                        item.VehicleRefKey.Trim(),

                        cancellationToken);

                    var mergedVehicleWorkflow = DispatchWorkflowGateRules.ApplyWorkflowGates(

                        preview.VehiclePreview,

                        vehicleWorkflowGates);

                    preview = preview with { VehiclePreview = mergedVehicleWorkflow };

                }

            }

            var hasBlockingConflicts =

                (preview.DriverPreview?.HasBlockingConflicts ?? false)

                || (preview.VehiclePreview?.HasBlockingConflicts ?? false)

                || preview.StatusPreview is { CanTransition: false };

            preview = preview with

            {

                HasBlockingConflicts = hasBlockingConflicts,

                CanApply = !hasBlockingConflicts,

            };

            previews.Add(preview);



            if (preview.CanApply)

            {

                BulkDispatchRules.ApplySimulation(item, trip);

            }

        }



        var canApplyCount = previews.Count(x => x.CanApply);

        return new BulkDispatchPreviewResponse(

            new BulkDispatchPreviewSummary(items.Count, canApplyCount, items.Count - canApplyCount),

            previews);

    }



    public async Task<BulkDispatchApplyResponse> ApplyAsync(

        Guid tenantId,

        Guid actorUserId,

        string? actorPersonId,

        bool canManageAny,

        BulkDispatchApplyRequest request,

        CancellationToken cancellationToken = default)

    {

        var items = ValidateItems(request.Items);

        var results = new List<BulkDispatchApplyItemResult>();



        foreach (var item in items)

        {

            try

            {

                var trip = await ApplyItemAsync(

                    tenantId,

                    actorUserId,

                    actorPersonId,

                    canManageAny,

                    item,

                    request.IgnoreAvailabilityConflicts,

                    request.IgnoreEligibilityBlocks,

                    request.IgnoreDispatchabilityBlocks,

                    request.IgnoreWorkflowGateBlocks,

                    cancellationToken);

                results.Add(new BulkDispatchApplyItemResult(item.TripId, true, null, null, trip));

            }

            catch (StlApiException ex)

            {

                results.Add(new BulkDispatchApplyItemResult(

                    item.TripId,

                    false,

                    ex.Code,

                    ex.Message,

                    null));

            }

        }



        var successCount = results.Count(x => x.Success);

        await audit.WriteAsync(

            ApplyAction,

            tenantId,

            actorUserId,

            "dispatch_bulk",

            Guid.NewGuid().ToString(),

            $"{successCount}/{items.Count}",

            cancellationToken: cancellationToken);



        return new BulkDispatchApplyResponse(

            new BulkDispatchApplySummary(items.Count, successCount, items.Count - successCount),

            results);

    }



    private BulkDispatchItemPreview PreviewItem(

        BulkDispatchActionItem item,

        Trip trip,

        IEnumerable<Trip> simulatedTrips,

        IReadOnlyList<DriverAvailability> driverAvailability,

        IReadOnlyList<EquipmentAvailability> equipmentAvailability,

        bool canManageAny)

    {

        DispatchAssignmentPreviewResponse? driverPreview = null;

        DispatchAssignmentPreviewResponse? vehiclePreview = null;

        BulkDispatchStatusPreview? statusPreview = null;

        var hasBlocking = false;



        if (!string.IsNullOrWhiteSpace(item.DriverPersonId))

        {

            driverPreview = PreviewDriverAssignment(

                trip,

                item.DriverPersonId.Trim(),

                simulatedTrips,

                driverAvailability);

            hasBlocking |= driverPreview.HasBlockingConflicts;

        }



        if (item.VehicleRefKey is not null && !string.IsNullOrWhiteSpace(item.VehicleRefKey))

        {

            vehiclePreview = PreviewVehicleAssignment(

                trip,

                item.VehicleRefKey.Trim(),

                simulatedTrips,

                equipmentAvailability);

            hasBlocking |= vehiclePreview.HasBlockingConflicts;

        }



        var effectiveDriver = !string.IsNullOrWhiteSpace(item.DriverPersonId)

            ? item.DriverPersonId.Trim()

            : trip.AssignedDriverPersonId;



        if (!string.IsNullOrWhiteSpace(item.DispatchStatus))

        {

            var (canTransition, errorCode, errorMessage) = BulkDispatchRules.PreviewStatusTransition(

                trip,

                item.DispatchStatus,

                canManageAny,

                effectiveDriver);

            statusPreview = new BulkDispatchStatusPreview(

                item.DispatchStatus.Trim().ToLowerInvariant(),

                canTransition,

                errorCode,

                errorMessage);

            if (!canTransition)

            {

                hasBlocking = true;

            }

        }



        if (!TripDispatchStatuses.Active.Contains(trip.DispatchStatus)

            && (!string.IsNullOrWhiteSpace(item.DriverPersonId) || item.VehicleRefKey is not null))

        {

            hasBlocking = true;

            if (driverPreview is null && !string.IsNullOrWhiteSpace(item.DriverPersonId))

            {

                driverPreview = new DispatchAssignmentPreviewResponse(

                    trip.Id,

                    DispatchAssignmentService.AssignmentKinds.Driver,

                    false,

                    true,

                    [],

                    [],

                    []);

            }



            if (vehiclePreview is null && item.VehicleRefKey is not null && !string.IsNullOrWhiteSpace(item.VehicleRefKey))

            {

                vehiclePreview = new DispatchAssignmentPreviewResponse(

                    trip.Id,

                    DispatchAssignmentService.AssignmentKinds.Vehicle,

                    false,

                    true,

                    [],

                    [],

                    []);

            }

        }



        return new BulkDispatchItemPreview(

            trip.Id,

            trip.TripNumber,

            trip.Title,

            trip.DispatchStatus,

            CanApply: !hasBlocking,

            hasBlocking,

            driverPreview,

            vehiclePreview,

            statusPreview);

    }



    private static DispatchAssignmentPreviewResponse PreviewDriverAssignment(

        Trip trip,

        string driverPersonId,

        IEnumerable<Trip> simulatedTrips,

        IReadOnlyList<DriverAvailability> availability)

    {

        var blockingAvailability = DispatchAssignmentRules

            .FindBlockingDriverAvailability(

                driverPersonId,

                trip.ScheduledStartAt,

                trip.ScheduledEndAt,

                availability)

            .Select(x => new DispatchAssignmentAvailabilityConflict(

                x.Id,

                x.AvailabilityStatus,

                x.StartsAt,

                x.EndsAt,

                x.Reason))

            .ToList();



        var overlappingTrips = DispatchAssignmentRules

            .FindOverlappingDriverTrips(trip, driverPersonId, simulatedTrips)

            .Select(x => new DispatchAssignmentTripConflict(

                x.Id,

                x.TripNumber,

                x.Title,

                x.DispatchStatus,

                x.ScheduledStartAt,

                x.ScheduledEndAt))

            .ToList();



        var hasBlocking = blockingAvailability.Count > 0 || overlappingTrips.Count > 0;

        return new DispatchAssignmentPreviewResponse(

            trip.Id,

            DispatchAssignmentService.AssignmentKinds.Driver,

            !hasBlocking,

            hasBlocking,

            blockingAvailability,

            [],

            overlappingTrips);

    }



    private static DispatchAssignmentPreviewResponse PreviewVehicleAssignment(

        Trip trip,

        string vehicleRefKey,

        IEnumerable<Trip> simulatedTrips,

        IReadOnlyList<EquipmentAvailability> availability)

    {

        var blockingAvailability = DispatchAssignmentRules

            .FindBlockingEquipmentAvailability(

                vehicleRefKey,

                trip.ScheduledStartAt,

                trip.ScheduledEndAt,

                availability)

            .Select(x => new DispatchAssignmentAvailabilityConflict(

                x.Id,

                x.AvailabilityStatus,

                x.StartsAt,

                x.EndsAt,

                x.Reason))

            .ToList();



        var overlappingTrips = DispatchAssignmentRules

            .FindOverlappingVehicleTrips(trip, vehicleRefKey, simulatedTrips)

            .Select(x => new DispatchAssignmentTripConflict(

                x.Id,

                x.TripNumber,

                x.Title,

                x.DispatchStatus,

                x.ScheduledStartAt,

                x.ScheduledEndAt))

            .ToList();



        var hasBlocking = blockingAvailability.Count > 0 || overlappingTrips.Count > 0;

        return new DispatchAssignmentPreviewResponse(

            trip.Id,

            DispatchAssignmentService.AssignmentKinds.Vehicle,

            !hasBlocking,

            hasBlocking,

            [],

            blockingAvailability,

            overlappingTrips);

    }



    private async Task<TripDetailResponse> ApplyItemAsync(

        Guid tenantId,

        Guid actorUserId,

        string? actorPersonId,

        bool canManageAny,

        BulkDispatchActionItem item,

        bool ignoreAvailabilityConflicts,

        bool ignoreEligibilityBlocks,

        bool ignoreDispatchabilityBlocks,

        bool ignoreWorkflowGateBlocks,

        CancellationToken cancellationToken)

    {

        TripDetailResponse? current = null;



        if (!string.IsNullOrWhiteSpace(item.DriverPersonId))

        {

            current = await tripService.AssignDriverAsync(

                tenantId,

                actorUserId,

                item.TripId,

                new AssignTripDriverRequest(
                    item.DriverPersonId,
                    DriverDisplayName: null,
                    IgnoreAvailabilityConflicts: ignoreAvailabilityConflicts,
                    IgnoreEligibilityBlocks: ignoreEligibilityBlocks,
                    IgnoreWorkflowGateBlocks: ignoreWorkflowGateBlocks),

                cancellationToken);

        }



        if (item.VehicleRefKey is not null)

        {

            current = await tripService.AssignVehicleAsync(

                tenantId,

                actorUserId,

                item.TripId,

                new AssignTripVehicleRequest(

                    string.IsNullOrWhiteSpace(item.VehicleRefKey) ? null : item.VehicleRefKey.Trim(),

                    ignoreAvailabilityConflicts,

                    ignoreDispatchabilityBlocks,

                    ignoreWorkflowGateBlocks),

                cancellationToken);

        }



        if (!string.IsNullOrWhiteSpace(item.DispatchStatus))

        {

            current = await tripService.UpdateDispatchStatusAsync(

                tenantId,

                actorUserId,

                item.TripId,

                new UpdateTripDispatchStatusRequest(item.DispatchStatus),

                canManageAny,

                actorPersonId,

                cancellationToken);

        }



        if (current is null)

        {

            throw new StlApiException(

                "dispatch_bulk.no_action",

                "Bulk dispatch item did not specify any changes.",

                400);

        }



        return current;

    }



    private static IReadOnlyList<BulkDispatchActionItem> ValidateItems(IReadOnlyList<BulkDispatchActionItem>? items)

    {

        if (items is null || items.Count == 0)

        {

            throw new StlApiException(

                "dispatch_bulk.validation",

                "At least one bulk dispatch item is required.",

                400);

        }



        if (items.Count > BulkDispatchRules.MaxBatchItems)

        {

            throw new StlApiException(

                "dispatch_bulk.validation",

                $"Bulk dispatch is limited to {BulkDispatchRules.MaxBatchItems} items per request.",

                400);

        }



        foreach (var item in items)

        {

            if (item.TripId == Guid.Empty)

            {

                throw new StlApiException(

                    "dispatch_bulk.validation",

                    "Each bulk dispatch item must include a trip id.",

                    400);

            }



            if (!BulkDispatchRules.HasAnyAction(item.DriverPersonId, item.VehicleRefKey, item.DispatchStatus))

            {

                throw new StlApiException(

                    "dispatch_bulk.validation",

                    "Each bulk dispatch item must specify a driver, vehicle, or dispatch status change.",

                    400);

            }

        }



        return items;

    }



    private async Task<Dictionary<Guid, Trip>> LoadTripsByIdAsync(

        Guid tenantId,

        IReadOnlyList<BulkDispatchActionItem> items,

        CancellationToken cancellationToken)

    {

        var tripIds = items.Select(x => x.TripId).Distinct().ToList();

        var trips = await db.Trips

            .AsNoTracking()

            .Include(x => x.Loads)

            .Where(x => x.TenantId == tenantId && tripIds.Contains(x.Id))

            .ToListAsync(cancellationToken);



        return trips.ToDictionary(x => x.Id);

    }



    private async Task<List<Trip>> LoadActiveTripsForTenantAsync(

        Guid tenantId,

        CancellationToken cancellationToken) =>

        await db.Trips

            .AsNoTracking()

            .Where(x => x.TenantId == tenantId)

            .ToListAsync(cancellationToken);

}


