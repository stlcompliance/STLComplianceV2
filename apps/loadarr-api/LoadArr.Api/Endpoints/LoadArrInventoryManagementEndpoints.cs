using LoadArr.Api.Settings;

namespace LoadArr.Api.Endpoints;

public static partial class LoadArrWorkspaceEndpoints
{
    private static void ApplyInventoryManagementAuthorization(RouteGroupBuilder group)
    {
        group.AddEndpointFilterFactory((_, next) => async invocationContext =>
        {
            var authorization = invocationContext.HttpContext.RequestServices.GetRequiredService<LoadArrAuthorizationService>();
            var requestMethod = invocationContext.HttpContext.Request.Method;

            if (HttpMethods.IsGet(requestMethod))
            {
                authorization.RequireWorkspaceRead(invocationContext.HttpContext.User);
            }
            else
            {
                authorization.RequireOperationalWrite(invocationContext.HttpContext.User);
            }

            return await next(invocationContext);
        });
    }

    public static void MapLoadArrInventoryManagementEndpoints(this WebApplication app)
    {
        var locations = app.MapGroup("/api/v1/locations")
            .WithTags("Locations")
            .RequireAuthorization();
        ApplyInventoryManagementAuthorization(locations);

        locations.MapGet("/{id}/utilization", (string id) =>
            WorkspaceReadModelUnavailable("LoadArr location utilization"))
        .WithName("GetLoadArrLocationUtilization");

        var counts = app.MapGroup("/api/v1/counts")
            .WithTags("Counts")
            .RequireAuthorization();
        ApplyInventoryManagementAuthorization(counts);

        counts.MapGet("/", (string? status, string? countType) =>
            WorkspaceReadModelUnavailable("LoadArr cycle counts"))
        .WithName("ListLoadArrCounts");

        counts.MapGet("/{id}", (string id) =>
            WorkspaceReadModelUnavailable("LoadArr cycle count detail"))
        .WithName("GetLoadArrCount");

        counts.MapPost("/", (CreateLoadArrCountRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr cycle count workflow"))
        .WithName("CreateLoadArrCount");

        counts.MapPost("/{id}/complete", (string id, CompleteLoadArrCountRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr cycle count workflow"))
        .WithName("CompleteLoadArrCount");

        counts.MapPost("/{id}/approve-variance", (string id, ApproveLoadArrCountVarianceRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr cycle count workflow"))
        .WithName("ApproveLoadArrCountVariance");

        var adjustments = app.MapGroup("/api/v1/adjustments")
            .WithTags("Adjustments")
            .RequireAuthorization();
        ApplyInventoryManagementAuthorization(adjustments);

        adjustments.MapGet("/", (string? status, string? adjustmentType) =>
            WorkspaceReadModelUnavailable("LoadArr adjustments"))
        .WithName("ListLoadArrAdjustments");

        adjustments.MapGet("/{id}", (string id) =>
            WorkspaceReadModelUnavailable("LoadArr adjustment detail"))
        .WithName("GetLoadArrAdjustment");

        adjustments.MapPost("/", (CreateLoadArrAdjustmentRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr adjustment workflow"))
        .WithName("CreateLoadArrAdjustment");

        adjustments.MapPost("/{id}/approve", (string id, ApproveLoadArrAdjustmentRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr adjustment workflow"))
        .WithName("ApproveLoadArrAdjustment");

        var truckStock = app.MapGroup("/api/v1/truck-stock")
            .WithTags("TruckStock")
            .RequireAuthorization();
        ApplyInventoryManagementAuthorization(truckStock);

        truckStock.MapGet("/", (string? status, string? locationId) =>
            WorkspaceReadModelUnavailable("LoadArr truck stock"))
        .WithName("ListLoadArrTruckStock");

        truckStock.MapGet("/{id}", (string id) =>
            WorkspaceReadModelUnavailable("LoadArr truck stock detail"))
        .WithName("GetLoadArrTruckStock");

        truckStock.MapPost("/{id}/issue", (string id, TruckStockIssueRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr truck stock workflow"))
        .WithName("IssueLoadArrTruckStock");

        truckStock.MapPost("/{id}/return", (string id, TruckStockReturnRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr truck stock workflow"))
        .WithName("ReturnLoadArrTruckStock");

        truckStock.MapPost("/{id}/count", (string id, TruckStockCountRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr truck stock workflow"))
        .WithName("CountLoadArrTruckStock");

        var kits = app.MapGroup("/api/v1/kits")
            .WithTags("Kits")
            .RequireAuthorization();
        ApplyInventoryManagementAuthorization(kits);

        kits.MapGet("/", (string? status, string? locationId) =>
            WorkspaceReadModelUnavailable("LoadArr kits"))
        .WithName("ListLoadArrKits");

        kits.MapGet("/{id}", (string id) =>
            WorkspaceReadModelUnavailable("LoadArr kit detail"))
        .WithName("GetLoadArrKit");

        kits.MapPost("/{id}/build", (string id, KitMutationRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr kit workflow"))
        .WithName("BuildLoadArrKit");

        kits.MapPost("/{id}/break", (string id, KitMutationRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr kit workflow"))
        .WithName("BreakLoadArrKit");

        kits.MapPost("/{id}/replenish", (string id, KitMutationRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr kit workflow"))
        .WithName("ReplenishLoadArrKit");

        kits.MapPost("/{id}/reserve", (string id, KitLifecycleActionRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr kit workflow"))
        .WithName("ReserveLoadArrKit");

        kits.MapPost("/{id}/pick", (string id, KitLifecycleActionRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr kit workflow"))
        .WithName("PickLoadArrKit");

        kits.MapPost("/{id}/inspect", (string id, KitLifecycleActionRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr kit workflow"))
        .WithName("InspectLoadArrKit");

        kits.MapPost("/{id}/assign", (string id, KitAssignRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr kit workflow"))
        .WithName("AssignLoadArrKit");

        kits.MapPost("/{id}/return", (string id, KitLifecycleActionRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr kit workflow"))
        .WithName("ReturnLoadArrKit");

        kits.MapPost("/{id}/expire-components", (string id, KitLifecycleActionRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr kit workflow"))
        .WithName("ExpireKitComponents");

        kits.MapPost("/{id}/track-location", (string id, KitTrackLocationRequest request) =>
            WorkspaceReadModelUnavailable("LoadArr kit workflow"))
        .WithName("TrackLoadArrKitLocation");
    }

    private static LoadArrLocationUtilizationResponse? CreateLocationUtilization(string locationId)
    {
        var location = ResolveLocation(locationId);
        if (location is null)
        {
            return null;
        }

        var workspace = CreateWorkspaceSummary();
        var inventory = workspace.Inventory.Where(item => string.Equals(item.LocationId, location.Id, StringComparison.OrdinalIgnoreCase)).ToArray();
        var tasks = workspace.Tasks.Where(task => string.Equals(task.LocationNameSnapshot, location.Name, StringComparison.OrdinalIgnoreCase)).ToArray();
        var holds = workspace.Holds.Where(hold => string.Equals(hold.LocationNameSnapshot, location.Name, StringComparison.OrdinalIgnoreCase)).ToArray();
        var unexplained = workspace.UnexplainedInventory.Where(record => string.Equals(record.WarehouseLocationId, location.Id, StringComparison.OrdinalIgnoreCase)).ToArray();
        var lastActivityAtUtc = new[]
        {
            workspace.Evidence.Select(item => item.CapturedAtUtc),
            tasks.Select(task => task.DueAtUtc),
            holds.Select(hold => hold.OpenedAtUtc),
            unexplained.Select(record => record.DiscoveredAtUtc)
        }
        .SelectMany(values => values)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .OrderByDescending(value => value, StringComparer.OrdinalIgnoreCase)
        .FirstOrDefault() ?? DateTimeOffset.UtcNow.ToString("O");

        return new LoadArrLocationUtilizationResponse(
            location.Id,
            location.Name,
            location.StaffarrSiteOrgUnitId,
            location.StaffarrSiteNameSnapshot,
            location.LocationType,
            location.Active,
            location.CapacityPercent,
            inventory.Sum(item => item.QuantityOnHand),
            inventory.Sum(item => item.QuantityBlocked),
            tasks.Length,
            holds.Length,
            unexplained.Length,
            inventory.Length,
            inventory.Select(item => item.State).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            new[]
            {
                location.ComplianceRestrictions.Count == 0 ? "no restrictions" : string.Join(", ", location.ComplianceRestrictions),
                $"{tasks.Length} task(s) open",
                $"{holds.Length} hold(s) active"
            },
            location.Notes,
            lastActivityAtUtc);
    }

    private static IReadOnlyCollection<LoadArrCountResponse> CreateCounts() =>
        [
            new LoadArrCountResponse(
                "count-8021",
                "CNT-260602-1945",
                "variance_pending_approval",
                "cycle_count",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                10m,
                12m,
                2m,
                "each",
                "person-route-stock-lead",
                null,
                "cycle_count_variance",
                null,
                "Positive variance waiting supervisor approval",
                "2026-06-02T19:45:00Z",
                "2026-06-02T19:54:00Z",
                null,
                "2026-06-02T19:54:00Z"),
            new LoadArrCountResponse(
                "count-adh-49",
                "CNT-260602-2120",
                "approved",
                "compliance",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-haz-01",
                "Hazmat Cage A",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                14m,
                14m,
                0m,
                "case",
                "person-hazmat-reviewer",
                "person-hazmat-supervisor",
                "sds_label_mismatch",
                "adj-count-adh-49",
                "SDS and label rule check completed",
                "2026-06-02T21:10:00Z",
                "2026-06-02T21:18:00Z",
                "2026-06-02T21:20:00Z",
                "2026-06-02T21:20:00Z")
        ];

    private static IReadOnlyCollection<LoadArrAdjustmentResponse> CreateAdjustments() =>
        [
            new LoadArrAdjustmentResponse(
                "adj-count-adh-49",
                "ADJ-260602-2118",
                "approved",
                "loss",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-haz-01",
                "Hazmat Cage A",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                -0.5m,
                "case",
                "sds_label_mismatch",
                "person-hazmat-reviewer",
                "person-hazmat-supervisor",
                null,
                "SDS and label review correction",
                "2026-06-02T21:18:00Z",
                "2026-06-02T21:20:00Z",
                "2026-06-02T21:20:00Z"),
            new LoadArrAdjustmentResponse(
                "adj-count-8021",
                "ADJ-260602-1954",
                "open",
                "gain",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                2m,
                "each",
                "cycle_count_variance",
                "person-route-stock-lead",
                null,
                null,
                "Supervisor approval pending",
                "2026-06-02T19:54:00Z",
                null,
                "2026-06-02T19:54:00Z")
        ];

    private static IReadOnlyCollection<LoadArrTruckStockResponse> CreateTruckStockRecords() =>
        [
            new LoadArrTruckStockResponse(
                "truck-stock-17-rotor",
                "TRK-17-ROTOR",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                "each",
                "person-route-stock-lead",
                "Jordan Reed",
                12m,
                6m,
                18m,
                "ready",
                "2026-06-02T19:40:00Z",
                "2026-06-02T19:48:00Z",
                "Reserved for maintenance work orders and route returns.",
                new[] { "truck_stock", "route_ready", "maintainarr:work-order:WO-5530" }),
            new LoadArrTruckStockResponse(
                "truck-stock-17-kit",
                "TRK-17-KIT",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                "each",
                "person-route-stock-lead",
                "Jordan Reed",
                4m,
                3m,
                10m,
                "low_stock",
                "2026-06-02T18:15:00Z",
                "2026-06-02T18:50:00Z",
                "Needs restock after route replenishment and technician issue.",
                new[] { "truck_stock", "restock_required", "route_replenishment" })
        ];

    private static IReadOnlyCollection<LoadArrKitResponse> CreateKitRecords() =>
        [
            new LoadArrKitResponse(
                "kit-pm-emergency-17",
                "KIT-PM-17",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                "each",
                "person-route-stock-lead",
                "Jordan Reed",
                4m,
                3m,
                10m,
                "built",
                "2026-06-02T18:15:00Z",
                "2026-06-02T18:50:00Z",
                "Maintenance and route response kit assigned to mobile stock.",
                new[] { "kit", "mobile_stock", "pm_ready" }),
            new LoadArrKitResponse(
                "kit-ppe-hazmat-04",
                "KIT-PPE-04",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "SUP-ADH-49",
                "Regulated adhesive cartridge kit",
                "case",
                "person-hazmat-reviewer",
                "Taylor Brooks",
                1m,
                1m,
                4m,
                "needs_replenishment",
                "2026-06-02T20:00:00Z",
                "2026-06-02T20:30:00Z",
                "Hazmat response kit pending replenishment after inspection.",
                new[] { "kit", "hazmat", "inspection" })
        ];

    private static LoadArrCountResponse? ResolveCount(string id) =>
        CreateCounts()
            .FirstOrDefault(record => string.Equals(record.Id, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrAdjustmentResponse? ResolveAdjustment(string id) =>
        CreateAdjustments()
            .FirstOrDefault(record => string.Equals(record.Id, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrTruckStockResponse? ResolveTruckStockRecord(string id) =>
        CreateTruckStockRecords()
            .FirstOrDefault(record => string.Equals(record.Id, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrKitResponse? ResolveKitRecord(string id) =>
        CreateKitRecords()
            .FirstOrDefault(record => string.Equals(record.Id, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrWarehouseTaskResponse? CreateKitFollowUpTask(LoadArrKitResponse record, decimal quantityNeeded) =>
        quantityNeeded > 0m
            ? new LoadArrWarehouseTaskResponse(
                $"task-{Guid.NewGuid():N}"[..13],
                "replenish",
                $"Replenish {record.KitNameSnapshot}",
                "normal",
                "ready",
                record.LocationNameSnapshot,
                "Kit Coordinator",
                record.PrimaryItemId,
                quantityNeeded,
                DateTimeOffset.UtcNow.AddHours(2).ToString("O"),
                new[] { "kit_low", "replenish_requested" })
            : null;

    private static LoadArrInventoryMovementResponse CreateKitMovement(
        LoadArrKitResponse record,
        string movementType,
        decimal quantity,
        string reasonCode,
        string personId,
        string changedAtUtc,
        string previousStatus,
        string nextStatus,
        string fromLocationId,
        string fromLocationName,
        string toLocationId,
        string toLocationName,
        string sourceObjectType)
    {
        return new LoadArrInventoryMovementResponse(
            $"move-{Guid.NewGuid():N}"[..13],
            movementType,
            record.StaffarrSiteOrgUnitId,
            fromLocationId,
            toLocationId,
            record.PrimaryItemId,
            record.KitNameSnapshot,
            quantity,
            record.UnitOfMeasure,
            previousStatus,
            nextStatus,
            "loadarr",
            sourceObjectType,
            record.Id,
            reasonCode,
            personId,
            null,
            changedAtUtc);
    }

    private static IResult? ValidateKitLifecycleActionRequest(string personId, string reasonCode, decimal quantity)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_person",
                "Kit operations require the acting person reference."));
        }

        if (string.IsNullOrWhiteSpace(reasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Kit operations require a controlled reason code."));
        }

        if (quantity < 0m)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_quantity",
                "Kit quantity cannot be negative."));
        }

        return null;
    }

    private static IResult? ValidateKitAssignRequest(string personId, string targetPersonId, string targetPersonNameSnapshot, string reasonCode)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_person",
                "Kit assignment requires the acting person reference."));
        }

        if (string.IsNullOrWhiteSpace(targetPersonId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_target_person",
                "Kit assignment requires a target person reference."));
        }

        if (string.IsNullOrWhiteSpace(targetPersonNameSnapshot))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_target_person_snapshot",
                "Kit assignment requires a target person snapshot."));
        }

        if (string.IsNullOrWhiteSpace(reasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Kit assignment requires a controlled reason code."));
        }

        return null;
    }

    private static IResult? ValidateKitTrackLocationRequest(string personId, string targetLocationId, string reasonCode)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_person",
                "Kit location tracking requires the acting person reference."));
        }

        if (string.IsNullOrWhiteSpace(targetLocationId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_location",
                "Kit location tracking requires a target location reference."));
        }

        if (string.IsNullOrWhiteSpace(reasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Kit location tracking requires a controlled reason code."));
        }

        return null;
    }

    private static LoadArrAdjustmentResponse CreateAdjustmentFromVariance(
        LoadArrCountResponse count,
        string adjustmentType,
        string approvedByPersonId,
        string reasonCode,
        string? evidenceSummary)
    {
        var createdAtUtc = DateTimeOffset.UtcNow.ToString("O");

        return new LoadArrAdjustmentResponse(
            $"adj-{Guid.NewGuid():N}"[..15],
            $"ADJ-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
            "approved",
            adjustmentType,
            count.StaffarrSiteOrgUnitId,
            count.StaffarrSiteNameSnapshot,
            count.WarehouseLocationId,
            count.LocationNameSnapshot,
            count.SupplyarrItemId,
            count.ItemNameSnapshot,
            count.VarianceQuantity,
            count.UnitOfMeasure,
            reasonCode,
            count.CountedByPersonId,
            approvedByPersonId,
            null,
            evidenceSummary ?? count.EvidenceSummary,
            createdAtUtc,
            createdAtUtc,
            createdAtUtc);
    }

    private static IResult? ValidateTruckStockRequest(string personId, string reasonCode, decimal quantity)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_person",
                "Truck stock operations require the acting person reference."));
        }

        if (string.IsNullOrWhiteSpace(reasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Truck stock operations require a controlled reason code."));
        }

        if (quantity <= 0m)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_quantity",
                "Truck stock quantity must be greater than zero."));
        }

        return null;
    }

    private static IResult? ValidateTruckStockCountRequest(string personId, string reasonCode, decimal countedQuantity)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_person",
                "Truck stock counts require the counting person reference."));
        }

        if (string.IsNullOrWhiteSpace(reasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Truck stock counts require a controlled reason code."));
        }

        if (countedQuantity < 0m)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_counted_quantity",
                "Truck stock counted quantity cannot be negative."));
        }

        return null;
    }

    private static IResult? ValidateKitMutationRequest(string personId, string reasonCode, decimal quantity)
    {
        if (string.IsNullOrWhiteSpace(personId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_person",
                "Kit operations require the acting person reference."));
        }

        if (string.IsNullOrWhiteSpace(reasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Kit operations require a controlled reason code."));
        }

        if (quantity <= 0m)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_quantity",
                "Kit quantity must be greater than zero."));
        }

        return null;
    }

    private static IResult? ValidateCountRequest(CreateLoadArrCountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SupplyarrItemId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_supplyarr_item_reference",
                "Counts require a valid SupplyArr item reference."));
        }

        if (ResolveSupplyArrItemReference(request.SupplyarrItemId) is null)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_supplyarr_item_reference",
                "Counts require an item reference that exists in SupplyArr."));
        }

        if (ResolveLocation(request.WarehouseLocationId) is null)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_location",
                "Counts require a valid StaffArr-owned location reference."));
        }

        if (string.IsNullOrWhiteSpace(request.CountedByPersonId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_counted_by",
                "Counts require the counting person reference."));
        }

        if (string.IsNullOrWhiteSpace(request.ReasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Counts require a controlled reason code."));
        }

        return null;
    }

    private static IResult? ValidateCountCompletionRequest(CompleteLoadArrCountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CountedByPersonId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_counted_by",
                "Count completion requires the counting person reference."));
        }

        if (string.IsNullOrWhiteSpace(request.ReasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Count completion requires a controlled reason code."));
        }

        if (request.CountedQuantity < 0m)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_counted_quantity",
                "Counted quantity cannot be negative."));
        }

        return null;
    }

    private static IResult? ValidateAdjustmentRequest(CreateLoadArrAdjustmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SupplyarrItemId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_supplyarr_item_reference",
                "Adjustments require a valid SupplyArr item reference."));
        }

        if (ResolveSupplyArrItemReference(request.SupplyarrItemId) is null)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_supplyarr_item_reference",
                "Adjustments require an item reference that exists in SupplyArr."));
        }

        if (ResolveLocation(request.WarehouseLocationId) is null)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_location",
                "Adjustments require a valid StaffArr-owned location reference."));
        }

        if (request.QuantityDelta == 0m)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_quantity_delta",
                "Adjustments require a non-zero quantity delta."));
        }

        if (string.IsNullOrWhiteSpace(request.CreatedByPersonId))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_created_by",
                "Adjustments require the creating person reference."));
        }

        if (string.IsNullOrWhiteSpace(request.ReasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Adjustments require a controlled reason code."));
        }

        return null;
    }
}

public sealed record LoadArrLocationUtilizationResponse(
    string Id,
    string Name,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string LocationType,
    bool Active,
    int CapacityPercent,
    decimal QuantityOnHand,
    decimal QuantityBlocked,
    int OpenTasks,
    int OpenHolds,
    int UnexplainedInventory,
    int ItemCount,
    IReadOnlyCollection<string> InventoryStates,
    IReadOnlyCollection<string> Signals,
    string Notes,
    string LastActivityAtUtc);

public sealed record LoadArrCountResponse(
    string Id,
    string CountNumber,
    string Status,
    string CountType,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal ExpectedQuantity,
    decimal CountedQuantity,
    decimal VarianceQuantity,
    string UnitOfMeasure,
    string CountedByPersonId,
    string? ApprovedByPersonId,
    string ReasonCode,
    string? InventoryAdjustmentId,
    string EvidenceSummary,
    string CreatedAtUtc,
    string? CompletedAtUtc,
    string? ApprovedAtUtc,
    string UpdatedAtUtc);

public sealed record CreateLoadArrCountRequest(
    string CountType,
    string StaffarrSiteOrgUnitId,
    string WarehouseLocationId,
    string SupplyarrItemId,
    decimal? ExpectedQuantity,
    string CountedByPersonId,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record CompleteLoadArrCountRequest(
    string CountType,
    string StaffarrSiteOrgUnitId,
    string WarehouseLocationId,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal CountedQuantity,
    string CountedByPersonId,
    string ReasonCode,
    string? EvidenceSummary,
    string? ComplianceEvaluationId);

public sealed record ApproveLoadArrCountVarianceRequest(
    string CountType,
    string StaffarrSiteOrgUnitId,
    string WarehouseLocationId,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal CountedQuantity,
    string ApprovedByPersonId,
    string ReasonCode,
    string? EvidenceSummary,
    string? ComplianceEvaluationId);

public sealed record LoadArrCountCompletionResponse(
    LoadArrCountResponse Count,
    LoadArrAdjustmentResponse? Adjustment,
    LoadArrInventoryOriginEventResponse? OriginEvent,
    LoadArrInventoryMovementResponse? Movement);

public sealed record LoadArrAdjustmentResponse(
    string Id,
    string AdjustmentNumber,
    string Status,
    string AdjustmentType,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal QuantityDelta,
    string UnitOfMeasure,
    string ReasonCode,
    string CreatedByPersonId,
    string? ApprovedByPersonId,
    string? InventoryOriginEventId,
    string EvidenceSummary,
    string CreatedAtUtc,
    string? ApprovedAtUtc,
    string UpdatedAtUtc);

public sealed record CreateLoadArrAdjustmentRequest(
    string AdjustmentType,
    string StaffarrSiteOrgUnitId,
    string WarehouseLocationId,
    string SupplyarrItemId,
    decimal QuantityDelta,
    string CreatedByPersonId,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record ApproveLoadArrAdjustmentRequest(
    string AdjustmentType,
    string StaffarrSiteOrgUnitId,
    string WarehouseLocationId,
    string SupplyarrItemId,
    decimal QuantityDelta,
    string CreatedByPersonId,
    string ApprovedByPersonId,
    string ReasonCode,
    string? EvidenceSummary,
    string? ComplianceEvaluationId);

public sealed record LoadArrAdjustmentMutationResponse(
    LoadArrAdjustmentResponse Adjustment,
    LoadArrInventoryOriginEventResponse? OriginEvent,
    LoadArrInventoryMovementResponse? Movement);

public sealed record LoadArrTruckStockResponse(
    string Id,
    string TruckStockNumber,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string TruckLocationId,
    string TruckLocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    string UnitOfMeasure,
    string AssignedPersonId,
    string AssignedPersonNameSnapshot,
    decimal QuantityOnHand,
    decimal MinimumQuantity,
    decimal MaximumQuantity,
    string Status,
    string LastCountedAtUtc,
    string LastMovementAtUtc,
    string Notes,
    IReadOnlyCollection<string> TraceTags);

public sealed record TruckStockIssueRequest(
    string PersonId,
    decimal Quantity,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record TruckStockReturnRequest(
    string PersonId,
    decimal Quantity,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record TruckStockCountRequest(
    string PersonId,
    decimal CountedQuantity,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record LoadArrTruckStockMutationResponse(
    LoadArrTruckStockResponse TruckStock,
    LoadArrInventoryMovementResponse? Movement,
    LoadArrWarehouseTaskResponse? RestockTask);

public sealed record LoadArrKitResponse(
    string Id,
    string KitNumber,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string LocationId,
    string LocationNameSnapshot,
    string PrimaryItemId,
    string KitNameSnapshot,
    string UnitOfMeasure,
    string AssignedPersonId,
    string AssignedPersonNameSnapshot,
    decimal QuantityOnHand,
    decimal MinimumQuantity,
    decimal MaximumQuantity,
    string Status,
    string LastActionAtUtc,
    string LastMovementAtUtc,
    string Notes,
    IReadOnlyCollection<string> TraceTags);

public sealed record KitMutationRequest(
    string PersonId,
    decimal Quantity,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record KitLifecycleActionRequest(
    string PersonId,
    decimal Quantity,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record KitAssignRequest(
    string PersonId,
    string TargetPersonId,
    string TargetPersonNameSnapshot,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record KitTrackLocationRequest(
    string PersonId,
    string TargetLocationId,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record LoadArrKitMutationResponse(
    LoadArrKitResponse Kit,
    LoadArrInventoryMovementResponse? Movement,
    LoadArrWarehouseTaskResponse? FollowUpTask);
