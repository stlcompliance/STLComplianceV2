namespace LoadArr.Api.Endpoints;

public static partial class LoadArrWorkspaceEndpoints
{
    public static void MapLoadArrRouteSurfaceEndpoints(this WebApplication app)
    {
        var surface = app.MapGroup("/api/v1/loadarr")
            .WithTags("LoadArr Route Surface")
            .RequireAuthorization();

        surface.MapGet("/dashboard", () => Results.Ok(CreateWorkspaceSummary()))
            .WithName("GetLoadArrRouteSurfaceDashboard");

        surface.MapGet("/expected-receipts", (string? status, string? locationId, string? sourceObjectId) =>
        {
            var records = CreateExpectedReceiptRecords()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.WarehouseLocationId, locationId))
                .Where(record => MatchesOptional(record.SourceObjectId, sourceObjectId))
                .OrderBy(record => record.ExpectedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrExpectedReceipts");

        surface.MapGet("/expected-receipts/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateExpectedReceiptRecords(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrExpectedReceipt");

        surface.MapGet("/dock-appointments", (string? status, string? dockLocationId) =>
        {
            var records = CreateDockAppointments()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.DockLocationId, dockLocationId))
                .OrderBy(record => record.ScheduledStartUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrDockAppointments");

        surface.MapGet("/dock-appointments/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateDockAppointments(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrDockAppointment");

        surface.MapGet("/putaway-tasks", (string? status, string? locationId) =>
        {
            var records = CreatePutawayTasks()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.FromLocationId, locationId) || MatchesOptional(record.ToLocationId, locationId))
                .OrderBy(record => record.DueAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrPutawayTasks");

        surface.MapGet("/putaway-tasks/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreatePutawayTasks(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrPutawayTask");

        surface.MapGet("/reservations", (string? status, string? demandProductKey, string? locationId) =>
        {
            var records = CreateReservationRecords()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.DemandProductKey, demandProductKey))
                .Where(record => MatchesOptional(record.WarehouseLocationId, locationId))
                .OrderBy(record => record.RequiredByUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrReservations");

        surface.MapGet("/reservations/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateReservationRecords(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrReservation");

        surface.MapGet("/picking", (string? status, string? locationId) =>
        {
            var records = CreatePickTasks()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.PickLocationId, locationId))
                .OrderBy(record => record.DueAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrPickTasks");

        surface.MapGet("/picking/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreatePickTasks(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrPickTask");

        surface.MapGet("/staging", (string? status, string? locationId) =>
        {
            var records = CreateStagingAssignments()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.StagingLocationId, locationId))
                .OrderBy(record => record.ReadyByUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrStagingAssignments");

        surface.MapGet("/staging/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateStagingAssignments(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrStagingAssignment");

        surface.MapGet("/shipping", (string? status, string? targetProduct) =>
        {
            var records = CreateLoadouts()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.TargetProduct, targetProduct))
                .OrderBy(record => record.LoadoutWindowStartUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrLoadouts");

        surface.MapGet("/shipping/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateLoadouts(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrLoadout");

        surface.MapGet("/loadouts", (string? status, string? targetProduct) =>
        {
            var records = CreateLoadouts()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.TargetProduct, targetProduct))
                .OrderBy(record => record.LoadoutWindowStartUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrLoadoutAliases");

        surface.MapGet("/loadouts/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateLoadouts(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrLoadoutAlias");

        MapLoadArrExceptionSurface(surface);
        MapLoadArrSupplyCoordinationSurface(surface);
        MapLoadArrSetupSurface(surface);
        MapLoadArrRecordsSurface(surface);
    }

    private static void MapLoadArrExceptionSurface(RouteGroupBuilder surface)
    {
        surface.MapGet("/exceptions", (string? status, string? exceptionType, string? queue) =>
        {
            var records = CreateWarehouseExceptions()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.ExceptionType, exceptionType))
                .Where(record => MatchesOptional(record.Queue, queue))
                .OrderByDescending(record => record.OpenedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrWarehouseExceptions");

        surface.MapGet("/exceptions/receiving", () =>
        {
            var records = CreateWarehouseExceptions()
                .Where(record => record.Queue is "receiving")
                .OrderByDescending(record => record.OpenedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrReceivingExceptions");

        surface.MapGet("/exceptions/inventory-holds", () =>
        {
            var records = CreateWarehouseExceptions()
                .Where(record => record.ExceptionType is "inventory_hold")
                .OrderByDescending(record => record.OpenedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrInventoryHoldExceptions");

        surface.MapGet("/exceptions/quarantine", () =>
        {
            var records = CreateWarehouseExceptions()
                .Where(record => record.Queue is "quarantine")
                .OrderByDescending(record => record.OpenedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrQuarantineExceptions");

        surface.MapGet("/exceptions/pending-quality-review", () =>
        {
            var records = CreateWarehouseExceptions()
                .Where(record => record.QualityReviewStatus is "pending_assurarr_review" or "awaiting_quality_decision")
                .OrderByDescending(record => record.OpenedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrPendingQualityReviewExceptions");

        surface.MapGet("/exceptions/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateWarehouseExceptions(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrWarehouseException");
    }

    private static void MapLoadArrSupplyCoordinationSurface(RouteGroupBuilder surface)
    {
        var supply = surface.MapGroup("/supply-coordination")
            .WithTags("LoadArr Supply Coordination");

        supply.MapGet("/po-receipts", (string? status, string? supplierName) =>
        {
            var records = CreatePoReceiptCoordinationRecords()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => supplierName is null || ContainsInvariant(record.SupplierNameSnapshot, supplierName))
                .OrderBy(record => record.ExpectedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrPoReceiptCoordinationRecords");

        supply.MapGet("/po-receipts/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreatePoReceiptCoordinationRecords(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrPoReceiptCoordinationRecord");

        supply.MapGet("/vendor-returns", (string? status, string? supplierName) =>
        {
            var records = CreateVendorReturnRecords()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => supplierName is null || ContainsInvariant(record.SupplierNameSnapshot, supplierName))
                .OrderByDescending(record => record.OpenedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrVendorReturnRecords");

        supply.MapGet("/vendor-returns/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateVendorReturnRecords(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrVendorReturnRecord");

        supply.MapGet("/backorders", (string? status, string? demandProductKey) =>
        {
            var records = CreateBackorderRecords()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.DemandProductKey, demandProductKey))
                .OrderBy(record => record.RequiredByUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrBackorderRecords");

        supply.MapGet("/backorders/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateBackorderRecords(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrBackorderRecord");

        supply.MapGet("/reorder-signals", (string? status, string? locationId) =>
        {
            var records = CreateReorderSignals()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.WarehouseLocationId, locationId))
                .OrderBy(record => record.Priority, StringComparer.OrdinalIgnoreCase)
                .ThenBy(record => record.SupplyarrItemId, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrReorderSignals");

        supply.MapGet("/reorder-signals/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateReorderSignals(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrReorderSignal");
    }

    private static void MapLoadArrSetupSurface(RouteGroupBuilder surface)
    {
        var setup = surface.MapGroup("/setup")
            .WithTags("LoadArr Setup");

        setup.MapGet("/location-rules", (string? locationType, bool? active) =>
        {
            var records = CreateLocationRules()
                .Where(record => MatchesOptional(record.LocationType, locationType))
                .Where(record => active is null || record.Active == active.Value)
                .OrderBy(record => record.LocationType, StringComparer.OrdinalIgnoreCase)
                .ThenBy(record => record.RuleKey, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrLocationRules");

        setup.MapGet("/location-rules/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateLocationRules(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrLocationRule");

        setup.MapGet("/item-references", (string? query, bool? hazardous) =>
        {
            var records = CreateItemReferenceSetupRecords()
                .Where(record => query is null || ContainsInvariant(record.ItemNumberSnapshot, query) || ContainsInvariant(record.ItemNameSnapshot, query))
                .Where(record => hazardous is null || record.IsHazardous == hazardous.Value)
                .OrderBy(record => record.ItemNumberSnapshot, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrItemReferenceSetupRecords");

        setup.MapGet("/item-references/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateItemReferenceSetupRecords(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrItemReferenceSetupRecord");

        setup.MapGet("/inventory-policies", (string? policyType, bool? active) =>
        {
            var records = CreateInventoryPolicies()
                .Where(record => MatchesOptional(record.PolicyType, policyType))
                .Where(record => active is null || record.Active == active.Value)
                .OrderBy(record => record.PolicyType, StringComparer.OrdinalIgnoreCase)
                .ThenBy(record => record.PolicyKey, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrInventoryPolicies");

        setup.MapGet("/inventory-policies/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateInventoryPolicies(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrInventoryPolicy");

        setup.MapGet("/devices-labels", (string? profileType, bool? active) =>
        {
            var records = CreateDeviceLabelProfiles()
                .Where(record => MatchesOptional(record.ProfileType, profileType))
                .Where(record => active is null || record.Active == active.Value)
                .OrderBy(record => record.ProfileType, StringComparer.OrdinalIgnoreCase)
                .ThenBy(record => record.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(records));
        })
        .WithName("ListLoadArrDeviceLabelProfiles");

        setup.MapGet("/devices-labels/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateDeviceLabelProfiles(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrDeviceLabelProfile");
    }

    private static void MapLoadArrRecordsSurface(RouteGroupBuilder surface)
    {
        var records = surface.MapGroup("/records")
            .WithTags("LoadArr Records");

        records.MapGet("/stock-ledger", (string? itemId, string? locationId, string? entryType) =>
        {
            var entries = CreateStockLedgerEntries()
                .Where(record => MatchesOptional(record.SupplyarrItemId, itemId))
                .Where(record => MatchesOptional(record.WarehouseLocationId, locationId))
                .Where(record => MatchesOptional(record.EntryType, entryType))
                .OrderByDescending(record => record.PostedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(entries));
        })
        .WithName("ListLoadArrStockLedgerEntries");

        records.MapGet("/stock-ledger/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateStockLedgerEntries(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrStockLedgerEntry");

        records.MapGet("/receiving-history", (string? status, string? sourceObjectId) =>
        {
            var entries = CreateReceivingHistoryRecords()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.SourceObjectId, sourceObjectId))
                .OrderByDescending(record => record.RecordedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(entries));
        })
        .WithName("ListLoadArrReceivingHistoryRecords");

        records.MapGet("/receiving-history/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateReceivingHistoryRecords(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrReceivingHistoryRecord");

        records.MapGet("/movement-history", (string? movementType, string? itemId) =>
        {
            var entries = CreateMovementHistoryRecords()
                .Where(record => MatchesOptional(record.MovementType, movementType))
                .Where(record => MatchesOptional(record.SupplyarrItemId, itemId))
                .OrderByDescending(record => record.MovedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(entries));
        })
        .WithName("ListLoadArrMovementHistoryRecords");

        records.MapGet("/movement-history/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateMovementHistoryRecords(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrMovementHistoryRecord");

        records.MapGet("/count-history", (string? status, string? countType) =>
        {
            var entries = CreateCounts()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.CountType, countType))
                .OrderByDescending(record => record.UpdatedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(entries));
        })
        .WithName("ListLoadArrCountHistoryRecords");

        records.MapGet("/count-history/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateCounts(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrCountHistoryRecord");

        records.MapGet("/adjustment-history", (string? status, string? adjustmentType) =>
        {
            var entries = CreateAdjustments()
                .Where(record => MatchesOptional(record.Status, status))
                .Where(record => MatchesOptional(record.AdjustmentType, adjustmentType))
                .OrderByDescending(record => record.UpdatedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(SurfaceList(entries));
        })
        .WithName("ListLoadArrAdjustmentHistoryRecords");

        records.MapGet("/adjustment-history/{id}", (string id) =>
        {
            var record = FindSurfaceRecord(CreateAdjustments(), item => item.Id, id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrAdjustmentHistoryRecord");
    }

    private static LoadArrListResponse<TItem> SurfaceList<TItem>(IReadOnlyCollection<TItem> items) =>
        new(items, items.Count);

    private static TItem? FindSurfaceRecord<TItem>(
        IEnumerable<TItem> items,
        Func<TItem, string> idSelector,
        string id) =>
        items.FirstOrDefault(item => string.Equals(idSelector(item), id, StringComparison.OrdinalIgnoreCase));

    private static bool MatchesOptional(string value, string? filter) =>
        filter is null || string.Equals(value, filter, StringComparison.OrdinalIgnoreCase);

    private static bool ContainsInvariant(string value, string query) =>
        value.Contains(query, StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyCollection<LoadArrExpectedReceiptResponse> CreateExpectedReceiptRecords() =>
        [
            new LoadArrExpectedReceiptResponse(
                "task-receive-24018",
                "EXP-PO-10492",
                "ready_to_receive",
                "supplyarr",
                "purchase_order",
                "PO-10492",
                "Midwest Fleet Supply",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                38m,
                0m,
                "each",
                "2026-06-03T15:00:00Z",
                "2026-06-02T20:10:00Z",
                "recv-24018",
                ["purchase_receipt", "packing_slip_attached"]),
            new LoadArrExpectedReceiptResponse(
                "task-inspect-adh-49",
                "EXP-ASN-8834",
                "inspection_required",
                "supplyarr",
                "asn",
                "ASN-8834",
                "Applied Chemical Partners",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-haz-01",
                "Hazmat Cage A",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                14m,
                14m,
                "case",
                "2026-06-03T18:00:00Z",
                "2026-06-02T21:10:00Z",
                "recv-8834",
                ["hazmat", "sds_check_required", "quality_review"])
        ];

    private static IReadOnlyCollection<LoadArrDockAppointmentResponse> CreateDockAppointments() =>
        [
            new LoadArrDockAppointmentResponse(
                "dock-po-10492",
                "DOCK-10492",
                "scheduled",
                "loc-dock-01",
                "Receiving Dock 1",
                "staff-site-stl-north",
                "STL North Yard",
                "Midwest Fleet Supply",
                "PO-10492",
                "2026-06-03T14:30:00Z",
                "2026-06-03T15:30:00Z",
                "Inventory Clerk",
                "Dock appointment mirrors SupplyArr purchase order timing.",
                ["purchase_receipt", "staffarr_location_snapshot"]),
            new LoadArrDockAppointmentResponse(
                "dock-asn-8834",
                "DOCK-8834",
                "arrived",
                "loc-haz-01",
                "Hazmat Cage A",
                "staff-site-stl-north",
                "STL North Yard",
                "Applied Chemical Partners",
                "ASN-8834",
                "2026-06-02T21:00:00Z",
                "2026-06-02T22:00:00Z",
                "Hazmat-qualified reviewer",
                "Hazmat consignment is isolated pending inspection.",
                ["hazmat", "inspection_required"])
        ];

    private static IReadOnlyCollection<LoadArrPutawayTaskResponse> CreatePutawayTasks() =>
        [
            new LoadArrPutawayTaskResponse(
                "xfer-24018-putaway",
                "PUT-24018",
                "ready",
                "high",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "loc-quarantine-01",
                "Quarantine Bay",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                4m,
                "each",
                "quality_inspection",
                "person-inventory-clerk",
                "2026-06-03T16:00:00Z",
                ["purchase_receipt", "inspection_buffer"]),
            new LoadArrPutawayTaskResponse(
                "task-putaway-adh-49",
                "PUT-ADH-49",
                "blocked",
                "urgent",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-haz-01",
                "Hazmat Cage A",
                "loc-quarantine-01",
                "Quarantine Bay",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                14m,
                "case",
                "quality_hold",
                "person-hazmat-reviewer",
                "2026-06-03T18:00:00Z",
                ["sds_label_mismatch", "assurarr_review_pending"])
        ];

    private static IReadOnlyCollection<LoadArrReservationResponse> CreateReservationRecords() =>
        [
            new LoadArrReservationResponse(
                "res-wo-5530-rotor",
                "reserved",
                "maintainarr",
                "work_order",
                "WO-5530",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "bal-brake-rotor",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                6m,
                "each",
                "2026-06-02T19:40:00Z",
                "2026-06-04T13:30:00Z",
                ["mobile_stock", "route_ready"]),
            new LoadArrReservationResponse(
                "res-out-1204-valve",
                "partially_reserved",
                "supplyarr",
                "outbound_stock_request",
                "OUT-1204",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "bal-valve-kit-a",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                11m,
                "each",
                "2026-06-02T20:20:00Z",
                "2026-06-04T15:30:00Z",
                ["purchase_receipt", "waiting_on_pick"])
        ];

    private static IReadOnlyCollection<LoadArrPickTaskResponse> CreatePickTasks() =>
        [
            new LoadArrPickTaskResponse(
                "task-pick-wo-5530",
                "PICK-WO-5530",
                "in_progress",
                "normal",
                "maintainarr",
                "work_order",
                "WO-5530",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "handoff-rt-7781",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                6m,
                "each",
                "Route Stock Lead",
                "2026-06-04T13:30:00Z",
                ["maintainarr", "route_ready"]),
            new LoadArrPickTaskResponse(
                "task-pick-out-1204",
                "PICK-OUT-1204",
                "ready",
                "high",
                "supplyarr",
                "outbound_stock_request",
                "OUT-1204",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "handoff-out-1204",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                11m,
                "each",
                "Inventory Clerk",
                "2026-06-04T15:30:00Z",
                ["supplyarr", "waiting_on_pick"])
        ];

    private static IReadOnlyCollection<LoadArrStagingAssignmentResponse> CreateStagingAssignments() =>
        [
            new LoadArrStagingAssignmentResponse(
                "truck-stock-17-rotor",
                "STG-TRUCK-17",
                "ready",
                "routarr",
                "route",
                "RT-7781",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "handoff-rt-7781",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                6m,
                "each",
                "WO-5530 parts staged for mobile maintenance route.",
                "2026-06-04T13:30:00Z",
                ["truck_stock", "route_ready"]),
            new LoadArrStagingAssignmentResponse(
                "stg-out-1204",
                "STG-OUT-1204",
                "waiting_on_pick",
                "supplyarr",
                "outbound_stock_request",
                "OUT-1204",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "handoff-out-1204",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                11m,
                "each",
                "Outbound stock movement is pending pick confirmation.",
                "2026-06-04T15:30:00Z",
                ["supplyarr", "waiting_on_pick"])
        ];

    private static IReadOnlyCollection<LoadArrLoadoutResponse> CreateLoadouts() =>
        [
            new LoadArrLoadoutResponse(
                "handoff-rt-7781",
                "LOAD-RT-7781",
                "ready",
                "RoutArr",
                "route",
                "RT-7781",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                6m,
                "each",
                "2026-06-04T13:30:00Z",
                "2026-06-04T14:00:00Z",
                "WO-5530 parts staged for mobile maintenance route.",
                ["truck_stock", "route_ready"]),
            new LoadArrLoadoutResponse(
                "handoff-out-1204",
                "LOAD-OUT-1204",
                "waiting_on_pick",
                "SupplyArr",
                "outbound_stock_request",
                "OUT-1204",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                11m,
                "each",
                "2026-06-04T15:30:00Z",
                "2026-06-04T16:00:00Z",
                "Outbound stock movement pending pick confirmation.",
                ["supplyarr", "waiting_on_pick"])
        ];

    private static IReadOnlyCollection<LoadArrWarehouseExceptionResponse> CreateWarehouseExceptions() =>
        [
            new LoadArrWarehouseExceptionResponse(
                "recv-8834-label-mismatch",
                "receiving_discrepancy",
                "open",
                "receiving",
                "high",
                "label_mismatch",
                "supplyarr",
                "asn",
                "ASN-8834",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-haz-01",
                "Hazmat Cage A",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                14m,
                "case",
                "cc-eval-adh-49",
                "pending_assurarr_review",
                "SDS and label check opened from receiving.",
                "2026-06-02T21:10:00Z"),
            new LoadArrWarehouseExceptionResponse(
                "hold-adh-49",
                "inventory_hold",
                "open",
                "quarantine",
                "critical",
                "sds_label_mismatch",
                "assurarr",
                "quality_hold",
                "cc-eval-adh-49",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-haz-01",
                "Hazmat Cage A",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                14m,
                "case",
                "cc-eval-adh-49",
                "awaiting_quality_decision",
                "Formal quality decision remains with AssurArr; LoadArr keeps operational stock blocked.",
                "2026-06-02T21:12:00Z"),
            new LoadArrWarehouseExceptionResponse(
                "unexplained-count-8021",
                "unexplained_inventory",
                "needs_approval",
                "cycle_count",
                "medium",
                "cycle_count_variance",
                "loadarr",
                "cycle_count",
                "CNT-260602-1945",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                2m,
                "each",
                null,
                "not_quality_related",
                "Positive mobile-stock variance waiting supervisor approval.",
                "2026-06-02T19:45:00Z")
        ];

    private static IReadOnlyCollection<LoadArrPoReceiptCoordinationResponse> CreatePoReceiptCoordinationRecords() =>
        CreateExpectedReceiptRecords()
            .Where(record => record.SourceObjectType is "purchase_order" or "asn")
            .Select(record => new LoadArrPoReceiptCoordinationResponse(
                record.Id,
                record.ExpectedReceiptNumber,
                record.Status,
                record.SourceObjectType,
                record.SourceObjectId,
                record.SupplierNameSnapshot,
                record.StaffarrSiteOrgUnitId,
                record.StaffarrSiteNameSnapshot,
                record.WarehouseLocationId,
                record.LocationNameSnapshot,
                record.SupplyarrItemId,
                record.ItemNameSnapshot,
                record.ExpectedQuantity,
                record.ReceivedQuantity,
                record.UnitOfMeasure,
                record.ExpectedAtUtc,
                record.ReceivingSessionId,
                record.Signals))
            .ToArray();

    private static IReadOnlyCollection<LoadArrVendorReturnResponse> CreateVendorReturnRecords() =>
        [
            new LoadArrVendorReturnResponse(
                "bal-brake-rotor",
                "VRET-BR-ROTOR-22",
                "draft",
                "Midwest Fleet Supply",
                "route_return",
                "RoutArr trip RT-7781",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                2m,
                "each",
                "Cycle count variance and returned route stock require SupplyArr vendor-return coordination.",
                "2026-06-02T19:45:00Z",
                ["route_return", "supplier_credit_review"]),
            new LoadArrVendorReturnResponse(
                "vret-adh-extra",
                "VRET-ADH-49",
                "blocked_by_quality",
                "Applied Chemical Partners",
                "damaged_freight_receipt",
                "UNX-ADH-49",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-quarantine-01",
                "Quarantine Bay",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                1m,
                "case",
                "Quality disposition must complete before vendor return execution.",
                "2026-06-02T20:50:00Z",
                ["quality_hold", "vendor_return_pending"])
        ];

    private static IReadOnlyCollection<LoadArrBackorderResponse> CreateBackorderRecords() =>
        [
            new LoadArrBackorderResponse(
                "truck-stock-17-rotor",
                "BACKORDER-TRK-17-ROTOR",
                "at_risk",
                "maintainarr",
                "work_order",
                "WO-5530",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                6m,
                2m,
                "each",
                "2026-06-04T13:30:00Z",
                "Mobile stock reservation is protected, but replenishment is below max.",
                ["truck_stock", "reorder_signal"]),
            new LoadArrBackorderResponse(
                "backorder-out-1204",
                "BACKORDER-OUT-1204",
                "waiting_on_pick",
                "supplyarr",
                "outbound_stock_request",
                "OUT-1204",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                11m,
                11m,
                "each",
                "2026-06-04T15:30:00Z",
                "Available stock exists but backorder remains open until pick confirmation.",
                ["waiting_on_pick", "purchase_receipt"])
        ];

    private static IReadOnlyCollection<LoadArrReorderSignalResponse> CreateReorderSignals() =>
        [
            new LoadArrReorderSignalResponse(
                "reorder-truck-17-kit",
                "open",
                "high",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                4m,
                3m,
                10m,
                "each",
                "Stock is below route kit maximum after issue and replenishment activity.",
                "2026-06-02T18:50:00Z",
                ["truck_stock", "route_replenishment"]),
            new LoadArrReorderSignalResponse(
                "reorder-haz-ppe-kit",
                "quality_blocked",
                "medium",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "SUP-ADH-49",
                "Regulated adhesive cartridge kit",
                1m,
                1m,
                4m,
                "case",
                "Hazmat response kit needs replenishment after inspection.",
                "2026-06-02T20:30:00Z",
                ["hazmat", "inspection"])
        ];

    private static IReadOnlyCollection<LoadArrLocationRuleResponse> CreateLocationRules() =>
        [
            new LoadArrLocationRuleResponse(
                "rule-quarantine-blocked",
                "quarantine_area",
                "block_allocation_until_release",
                "Block allocation from quarantine locations until an owning workflow releases the stock.",
                true,
                "AssurArr quality decisions may authorize release; LoadArr only reflects operational blocking.",
                "2026-06-01T12:00:00Z"),
            new LoadArrLocationRuleResponse(
                "rule-hazmat-training",
                "hazmat_cage",
                "require_hazmat_signal",
                "Require hazardous-material signals before putaway or pick execution.",
                true,
                "Training qualification remains StaffArr/TrainArr-owned; LoadArr stores only the execution signal.",
                "2026-06-01T12:00:00Z"),
            new LoadArrLocationRuleResponse(
                "rule-truck-stock-route",
                "service_truck",
                "route_ready_snapshot",
                "Mobile stock locations may stage stock to RoutArr handoffs.",
                true,
                "Vehicle and route identities remain StaffArr/RoutArr-owned snapshots.",
                "2026-06-01T12:00:00Z")
        ];

    private static IReadOnlyCollection<LoadArrItemReferenceSetupResponse> CreateItemReferenceSetupRecords() =>
        CreateSupplyArrItemReferences()
            .Select(item => new LoadArrItemReferenceSetupResponse(
                item.SupplyarrItemId,
                item.SupplyarrItemId,
                item.ItemNumberSnapshot,
                item.ItemNameSnapshot,
                item.UnitOfMeasureSnapshot,
                item.ItemTypeSnapshot,
                item.IsLotControlled,
                item.IsSerialControlled,
                item.IsHazardous,
                item.RequiresSds,
                item.UpdatedAtUtc,
                "SupplyArr remains canonical; LoadArr stores operational item snapshots."))
            .ToArray();

    private static IReadOnlyCollection<LoadArrInventoryPolicyResponse> CreateInventoryPolicies() =>
        [
            new LoadArrInventoryPolicyResponse(
                "policy-cycle-count-mobile",
                "cycle_count",
                "mobile_stock_variance_supervisor_approval",
                "Require supervisor approval before posting mobile stock count variances.",
                true,
                "standard",
                "2026-06-01T12:00:00Z"),
            new LoadArrInventoryPolicyResponse(
                "policy-receipt-hazmat-inspection",
                "receiving",
                "hazmat_requires_quarantine_review",
                "Hazardous receipts remain pending inspection until SDS and label checks clear.",
                true,
                "critical",
                "2026-06-01T12:00:00Z"),
            new LoadArrInventoryPolicyResponse(
                "policy-reservation-expiry",
                "reservation",
                "reservation_review_after_48h",
                "Reservations older than 48 hours require operational review before release.",
                true,
                "standard",
                "2026-06-01T12:00:00Z")
        ];

    private static IReadOnlyCollection<LoadArrDeviceLabelProfileResponse> CreateDeviceLabelProfiles() =>
        [
            new LoadArrDeviceLabelProfileResponse(
                "profile-dock-receipt-label",
                "label",
                "Dock receipt label",
                "Receiving Dock 1",
                true,
                "Print item, lot, source document, and receiving session identifiers.",
                ["supplyarrItemId", "lotCode", "receivingNumber"],
                "2026-06-01T12:00:00Z"),
            new LoadArrDeviceLabelProfileResponse(
                "profile-truck-stock-scan",
                "device",
                "Truck stock handheld scan",
                "Truck Stock 17",
                true,
                "Mobile scan profile for route issue, return, count, and stage actions.",
                ["truckStockNumber", "workOrderId", "routeId"],
                "2026-06-01T12:00:00Z")
        ];

    private static IReadOnlyCollection<LoadArrStockLedgerEntryResponse> CreateStockLedgerEntries() =>
        [
            new LoadArrStockLedgerEntryResponse(
                "ledger-rr-24018-valve",
                "receipt",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                38m,
                38m,
                "each",
                "purchase_receipt",
                "recv-24018",
                "person-inventory-clerk",
                "2026-06-02T20:16:00Z"),
            new LoadArrStockLedgerEntryResponse(
                "ledger-hold-adh-49",
                "hold",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-haz-01",
                "Hazmat Cage A",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                14m,
                0m,
                "case",
                "quality_hold",
                "hold-adh-49",
                "person-hazmat-reviewer",
                "2026-06-02T21:12:00Z"),
            new LoadArrStockLedgerEntryResponse(
                "ledger-adj-count-8021",
                "adjustment",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                2m,
                12m,
                "each",
                "cycle_count_variance",
                "adj-count-8021",
                "person-route-stock-lead",
                "2026-06-02T19:54:00Z")
        ];

    private static IReadOnlyCollection<LoadArrReceivingHistoryResponse> CreateReceivingHistoryRecords() =>
        CreateReceivingSessions()
            .Select(session => new LoadArrReceivingHistoryResponse(
                session.Id,
                session.ReceivingNumber,
                session.Status,
                session.ReceivingType,
                session.SourceProductKey,
                session.SourceObjectType,
                session.SourceObjectId,
                session.SupplierNameSnapshot,
                session.StaffarrSiteOrgUnitId,
                session.StaffarrSiteNameSnapshot,
                session.Lines.Sum(line => line.ReceivedQuantity),
                session.Lines.FirstOrDefault()?.UnitOfMeasure ?? "each",
                session.StartedByPersonId,
                session.CompletedByPersonId,
                session.CompletedAtUtc ?? session.StartedAtUtc,
                session.Lines.Select(line => line.Status).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()))
            .ToArray();

    private static IReadOnlyCollection<LoadArrMovementHistoryResponse> CreateMovementHistoryRecords() =>
        [
            new LoadArrMovementHistoryResponse(
                "move-xfer-24018",
                "putaway",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "loc-quarantine-01",
                "Quarantine Bay",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                4m,
                "each",
                "xfer-24018-putaway",
                "person-inventory-clerk",
                "2026-06-03T14:15:00Z"),
            new LoadArrMovementHistoryResponse(
                "move-route-7781",
                "stage_to_route",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                null,
                "RoutArr RT-7781",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                6m,
                "each",
                "handoff-rt-7781",
                "person-route-stock-lead",
                "2026-06-04T13:30:00Z")
        ];
}

public sealed record LoadArrExpectedReceiptResponse(
    string Id,
    string ExpectedReceiptNumber,
    string Status,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string UnitOfMeasure,
    string ExpectedAtUtc,
    string LastUpdatedAtUtc,
    string? ReceivingSessionId,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrDockAppointmentResponse(
    string Id,
    string AppointmentNumber,
    string Status,
    string DockLocationId,
    string DockLocationNameSnapshot,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string SupplierNameSnapshot,
    string SourceObjectId,
    string ScheduledStartUtc,
    string ScheduledEndUtc,
    string AssignedRole,
    string Notes,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrPutawayTaskResponse(
    string Id,
    string PutawayNumber,
    string Status,
    string Priority,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string FromLocationId,
    string FromLocationNameSnapshot,
    string ToLocationId,
    string ToLocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string ReasonCode,
    string AssignedPersonId,
    string DueAtUtc,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrReservationResponse(
    string Id,
    string Status,
    string DemandProductKey,
    string DemandObjectType,
    string DemandObjectId,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string InventoryBalanceId,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal ReservedQuantity,
    string UnitOfMeasure,
    string CreatedAtUtc,
    string RequiredByUtc,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrPickTaskResponse(
    string Id,
    string PickNumber,
    string Status,
    string Priority,
    string DemandProductKey,
    string DemandObjectType,
    string DemandObjectId,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string PickLocationId,
    string PickLocationNameSnapshot,
    string StagingAssignmentId,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string AssignedRole,
    string DueAtUtc,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrStagingAssignmentResponse(
    string Id,
    string StagingNumber,
    string Status,
    string TargetProductKey,
    string TargetObjectType,
    string TargetObjectId,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string StagingLocationId,
    string StagingLocationNameSnapshot,
    string LoadoutId,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string Notes,
    string ReadyByUtc,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrLoadoutResponse(
    string Id,
    string LoadoutNumber,
    string Status,
    string TargetProduct,
    string TargetObjectType,
    string TargetObjectId,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string LoadoutLocationId,
    string LoadoutLocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string LoadoutWindowStartUtc,
    string LoadoutWindowEndUtc,
    string Notes,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrWarehouseExceptionResponse(
    string Id,
    string ExceptionType,
    string Status,
    string Queue,
    string Severity,
    string ReasonCode,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string? ComplianceEvaluationId,
    string QualityReviewStatus,
    string Summary,
    string OpenedAtUtc);

public sealed record LoadArrPoReceiptCoordinationResponse(
    string Id,
    string ExpectedReceiptNumber,
    string Status,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string UnitOfMeasure,
    string ExpectedAtUtc,
    string? ReceivingSessionId,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrVendorReturnResponse(
    string Id,
    string ReturnNumber,
    string Status,
    string SupplierNameSnapshot,
    string SourceEventType,
    string SourceReference,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string Notes,
    string OpenedAtUtc,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrBackorderResponse(
    string Id,
    string BackorderNumber,
    string Status,
    string DemandProductKey,
    string DemandObjectType,
    string DemandObjectId,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal RequestedQuantity,
    decimal ShortQuantity,
    string UnitOfMeasure,
    string RequiredByUtc,
    string Notes,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrReorderSignalResponse(
    string Id,
    string Status,
    string Priority,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal QuantityOnHand,
    decimal MinimumQuantity,
    decimal MaximumQuantity,
    string UnitOfMeasure,
    string Notes,
    string OpenedAtUtc,
    IReadOnlyCollection<string> Signals);

public sealed record LoadArrLocationRuleResponse(
    string Id,
    string LocationType,
    string RuleKey,
    string Description,
    bool Active,
    string OwnershipNote,
    string UpdatedAtUtc);

public sealed record LoadArrItemReferenceSetupResponse(
    string Id,
    string SupplyarrItemId,
    string ItemNumberSnapshot,
    string ItemNameSnapshot,
    string UnitOfMeasureSnapshot,
    string ItemTypeSnapshot,
    bool IsLotControlled,
    bool IsSerialControlled,
    bool IsHazardous,
    bool RequiresSds,
    string UpdatedAtUtc,
    string OwnershipNote);

public sealed record LoadArrInventoryPolicyResponse(
    string Id,
    string PolicyType,
    string PolicyKey,
    string Description,
    bool Active,
    string Sensitivity,
    string UpdatedAtUtc);

public sealed record LoadArrDeviceLabelProfileResponse(
    string Id,
    string ProfileType,
    string Name,
    string LocationScopeSnapshot,
    bool Active,
    string Description,
    IReadOnlyCollection<string> RequiredFields,
    string UpdatedAtUtc);

public sealed record LoadArrStockLedgerEntryResponse(
    string Id,
    string EntryType,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal QuantityDelta,
    decimal BalanceAfter,
    string UnitOfMeasure,
    string ReasonCode,
    string SourceReference,
    string PostedByPersonId,
    string PostedAtUtc);

public sealed record LoadArrReceivingHistoryResponse(
    string Id,
    string ReceivingNumber,
    string Status,
    string ReceivingType,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    decimal ReceivedQuantity,
    string UnitOfMeasure,
    string StartedByPersonId,
    string? CompletedByPersonId,
    string RecordedAtUtc,
    IReadOnlyCollection<string> LineStatuses);

public sealed record LoadArrMovementHistoryResponse(
    string Id,
    string MovementType,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string? FromLocationId,
    string? FromLocationNameSnapshot,
    string? ToLocationId,
    string? ToLocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string SourceReference,
    string MovedByPersonId,
    string MovedAtUtc);
