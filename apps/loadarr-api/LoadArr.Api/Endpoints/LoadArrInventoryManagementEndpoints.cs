namespace LoadArr.Api.Endpoints;

public static partial class LoadArrWorkspaceEndpoints
{
    public static void MapLoadArrInventoryManagementEndpoints(this WebApplication app)
    {
        var locations = app.MapGroup("/api/v1/locations")
            .WithTags("Locations")
            .RequireAuthorization();

        locations.MapGet("/{id}/utilization", (string id) =>
        {
            var utilization = CreateLocationUtilization(id);
            return utilization is null ? Results.NotFound() : Results.Ok(utilization);
        })
        .WithName("GetLoadArrLocationUtilization");

        var counts = app.MapGroup("/api/v1/counts")
            .WithTags("Counts")
            .RequireAuthorization();

        counts.MapGet("/", (string? status, string? countType) =>
        {
            var records = CreateCounts()
                .Where(record => status is null
                    || string.Equals(record.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(record => countType is null
                    || string.Equals(record.CountType, countType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(record => record.CreatedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrCountResponse>(records, records.Length));
        })
        .WithName("ListLoadArrCounts");

        counts.MapGet("/{id}", (string id) =>
        {
            var record = CreateCounts()
                .SingleOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrCount");

        counts.MapPost("/", (CreateLoadArrCountRequest request) =>
        {
            var validation = ValidateCountRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            var location = ResolveLocation(request.WarehouseLocationId)!;
            var item = ResolveSupplyArrItemReference(request.SupplyarrItemId)!;
            var expectedQuantity = request.ExpectedQuantity ?? ResolveInventoryBalance(item.SupplyarrItemId, location.Id)?.QuantityOnHand ?? 0m;

            var count = new LoadArrCountResponse(
                $"count-{Guid.NewGuid():N}"[..17],
                $"CNT-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                "open",
                string.IsNullOrWhiteSpace(request.CountType) ? "cycle_count" : request.CountType,
                location.StaffarrSiteOrgUnitId,
                location.StaffarrSiteNameSnapshot,
                location.Id,
                location.Name,
                item.SupplyarrItemId,
                item.ItemNameSnapshot,
                expectedQuantity,
                0m,
                expectedQuantity,
                item.UnitOfMeasureSnapshot,
                request.CountedByPersonId,
                null,
                request.ReasonCode,
                null,
                request.EvidenceSummary ?? string.Empty,
                DateTimeOffset.UtcNow.ToString("O"),
                null,
                null,
                DateTimeOffset.UtcNow.ToString("O"));

            return Results.Created($"/api/v1/counts/{count.Id}", count);
        })
        .WithName("CreateLoadArrCount");

        counts.MapPost("/{id}/complete", (string id, CompleteLoadArrCountRequest request) =>
        {
            var validation = ValidateCountCompletionRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            var location = ResolveLocation(request.WarehouseLocationId);
            var item = ResolveSupplyArrItemReference(request.SupplyarrItemId);
            if (location is null || item is null)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_reference",
                    "Count completion requires valid StaffArr location and SupplyArr item references."));
            }

            var existingCount = ResolveCount(id) ?? new LoadArrCountResponse(
                id,
                $"CNT-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                "open",
                string.IsNullOrWhiteSpace(request.CountType) ? "cycle_count" : request.CountType,
                location.StaffarrSiteOrgUnitId,
                location.StaffarrSiteNameSnapshot,
                location.Id,
                location.Name,
                item.SupplyarrItemId,
                item.ItemNameSnapshot,
                request.ExpectedQuantity,
                0m,
                request.ExpectedQuantity,
                item.UnitOfMeasureSnapshot,
                request.CountedByPersonId,
                null,
                request.ReasonCode,
                null,
                request.EvidenceSummary ?? string.Empty,
                DateTimeOffset.UtcNow.ToString("O"),
                null,
                null,
                DateTimeOffset.UtcNow.ToString("O"));

            if (existingCount is null)
            {
                return Results.NotFound();
            }

            var countedQuantity = request.CountedQuantity;
            var variance = countedQuantity - existingCount.ExpectedQuantity;
            var completedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var status = variance == 0m ? "completed" : "variance_pending_approval";

            var completed = existingCount with
            {
                Status = status,
                CountedQuantity = countedQuantity,
                VarianceQuantity = variance,
                ApprovedByPersonId = null,
                UpdatedAtUtc = completedAtUtc,
                CompletedAtUtc = completedAtUtc
            };

            var response = new LoadArrCountCompletionResponse(
                completed,
                null,
                null,
                null);

            return Results.Ok(response);
        })
        .WithName("CompleteLoadArrCount");

        counts.MapPost("/{id}/approve-variance", (string id, ApproveLoadArrCountVarianceRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.ApprovedByPersonId))
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "missing_approval_person",
                    "Count variance approval requires an approver."));
            }

            if (string.IsNullOrWhiteSpace(request.ReasonCode))
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "missing_reason_code",
                    "Count variance approval requires a controlled reason code."));
            }

            var location = ResolveLocation(request.WarehouseLocationId);
            var item = ResolveSupplyArrItemReference(request.SupplyarrItemId);
            if (location is null || item is null)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_reference",
                    "Count variance approval requires valid StaffArr location and SupplyArr item references."));
            }

            var count = ResolveCount(id) ?? new LoadArrCountResponse(
                id,
                $"CNT-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                "variance_pending_approval",
                string.IsNullOrWhiteSpace(request.CountType) ? "cycle_count" : request.CountType,
                location.StaffarrSiteOrgUnitId,
                location.StaffarrSiteNameSnapshot,
                location.Id,
                location.Name,
                item.SupplyarrItemId,
                item.ItemNameSnapshot,
                request.ExpectedQuantity,
                request.CountedQuantity,
                request.CountedQuantity - request.ExpectedQuantity,
                item.UnitOfMeasureSnapshot,
                request.ApprovedByPersonId,
                null,
                request.ReasonCode,
                null,
                request.EvidenceSummary ?? string.Empty,
                DateTimeOffset.UtcNow.ToString("O"),
                DateTimeOffset.UtcNow.ToString("O"),
                null,
                DateTimeOffset.UtcNow.ToString("O"));

            if (count is null)
            {
                return Results.NotFound();
            }

            if (count.VarianceQuantity == 0m)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "no_variance",
                    "Counts without variance do not require approval."));
            }

            var adjustmentType = count.VarianceQuantity > 0m ? "gain" : "loss";
            var originType = count.VarianceQuantity > 0m ? "cycle_count_gain" : "count_loss";
            var movementType = count.VarianceQuantity > 0m ? "count_gain" : "count_loss";
            var adjustment = CreateAdjustmentFromVariance(
                count,
                adjustmentType,
                request.ApprovedByPersonId,
                request.ReasonCode,
                request.EvidenceSummary);
            var approvedAtUtc = adjustment.ApprovedAtUtc ?? DateTimeOffset.UtcNow.ToString("O");
            var completedCount = count with
            {
                Status = "approved",
                ApprovedByPersonId = request.ApprovedByPersonId,
                ApprovedAtUtc = approvedAtUtc,
                InventoryAdjustmentId = adjustment.Id,
                UpdatedAtUtc = approvedAtUtc
            };

            var origin = count.VarianceQuantity > 0m
                ? new LoadArrInventoryOriginEventResponse(
                    $"origin-{Guid.NewGuid():N}"[..15],
                    originType,
                    "loadarr",
                    "cycle_count",
                    count.Id,
                    count.StaffarrSiteOrgUnitId,
                    count.StaffarrSiteNameSnapshot,
                    count.WarehouseLocationId,
                    count.LocationNameSnapshot,
                    count.SupplyarrItemId,
                    count.ItemNameSnapshot,
                    Math.Abs(count.VarianceQuantity),
                    count.UnitOfMeasure,
                    null,
                    null,
                    "available",
                    "approved",
                    request.ApprovedByPersonId,
                    request.ComplianceEvaluationId,
                    request.EvidenceSummary,
                    approvedAtUtc)
                : null;

            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                movementType,
                count.StaffarrSiteOrgUnitId,
                count.WarehouseLocationId,
                count.WarehouseLocationId,
                count.SupplyarrItemId,
                count.ItemNameSnapshot,
                Math.Abs(count.VarianceQuantity),
                count.UnitOfMeasure,
                "untrusted",
                count.VarianceQuantity > 0m ? "available" : "blocked",
                "loadarr",
                "cycle_count",
                count.Id,
                request.ReasonCode,
                request.ApprovedByPersonId,
                origin?.Id,
                approvedAtUtc);

            return Results.Ok(new LoadArrCountCompletionResponse(
                completedCount,
                adjustment,
                origin,
                movement));
        })
        .WithName("ApproveLoadArrCountVariance");

        var adjustments = app.MapGroup("/api/v1/adjustments")
            .WithTags("Adjustments")
            .RequireAuthorization();

        adjustments.MapGet("/", (string? status, string? adjustmentType) =>
        {
            var records = CreateAdjustments()
                .Where(record => status is null
                    || string.Equals(record.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(record => adjustmentType is null
                    || string.Equals(record.AdjustmentType, adjustmentType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(record => record.CreatedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrAdjustmentResponse>(records, records.Length));
        })
        .WithName("ListLoadArrAdjustments");

        adjustments.MapGet("/{id}", (string id) =>
        {
            var record = CreateAdjustments()
                .SingleOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrAdjustment");

        adjustments.MapPost("/", (CreateLoadArrAdjustmentRequest request) =>
        {
            var validation = ValidateAdjustmentRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            var location = ResolveLocation(request.WarehouseLocationId)!;
            var item = ResolveSupplyArrItemReference(request.SupplyarrItemId)!;
            var adjustment = new LoadArrAdjustmentResponse(
                $"adj-{Guid.NewGuid():N}"[..15],
                $"ADJ-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                "open",
                string.IsNullOrWhiteSpace(request.AdjustmentType) ? "migration_correction" : request.AdjustmentType,
                location.StaffarrSiteOrgUnitId,
                location.StaffarrSiteNameSnapshot,
                location.Id,
                location.Name,
                item.SupplyarrItemId,
                item.ItemNameSnapshot,
                request.QuantityDelta,
                item.UnitOfMeasureSnapshot,
                request.ReasonCode,
                request.CreatedByPersonId,
                null,
                null,
                request.EvidenceSummary ?? string.Empty,
                DateTimeOffset.UtcNow.ToString("O"),
                null,
                DateTimeOffset.UtcNow.ToString("O"));

            return Results.Created($"/api/v1/adjustments/{adjustment.Id}", adjustment);
        })
        .WithName("CreateLoadArrAdjustment");

        adjustments.MapPost("/{id}/approve", (string id, ApproveLoadArrAdjustmentRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.ApprovedByPersonId))
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "missing_approval_person",
                    "Adjustment approval requires an approver."));
            }

            if (string.IsNullOrWhiteSpace(request.ReasonCode))
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "missing_reason_code",
                    "Adjustment approval requires a controlled reason code."));
            }

            var location = ResolveLocation(request.WarehouseLocationId);
            var item = ResolveSupplyArrItemReference(request.SupplyarrItemId);
            if (location is null || item is null)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_reference",
                    "Adjustment approval requires valid StaffArr location and SupplyArr item references."));
            }

            var adjustment = ResolveAdjustment(id) ?? new LoadArrAdjustmentResponse(
                id,
                $"ADJ-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                "open",
                string.IsNullOrWhiteSpace(request.AdjustmentType) ? "migration_correction" : request.AdjustmentType,
                location.StaffarrSiteOrgUnitId,
                location.StaffarrSiteNameSnapshot,
                location.Id,
                location.Name,
                item.SupplyarrItemId,
                item.ItemNameSnapshot,
                request.QuantityDelta,
                item.UnitOfMeasureSnapshot,
                request.ReasonCode,
                request.CreatedByPersonId,
                null,
                null,
                request.EvidenceSummary ?? string.Empty,
                DateTimeOffset.UtcNow.ToString("O"),
                null,
                DateTimeOffset.UtcNow.ToString("O"));

            if (adjustment is null)
            {
                return Results.NotFound();
            }

            var approvedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var approved = adjustment with
            {
                Status = "approved",
                ApprovedByPersonId = request.ApprovedByPersonId,
                ApprovedAtUtc = approvedAtUtc,
                UpdatedAtUtc = approvedAtUtc
            };

            var movementType = approved.QuantityDelta >= 0m ? "count_gain" : "count_loss";
            var origin = approved.QuantityDelta > 0m
                ? new LoadArrInventoryOriginEventResponse(
                    $"origin-{Guid.NewGuid():N}"[..15],
                    "manual_adjustment",
                    "loadarr",
                    "inventory_adjustment",
                    approved.Id,
                    approved.StaffarrSiteOrgUnitId,
                    approved.StaffarrSiteNameSnapshot,
                    approved.WarehouseLocationId,
                    approved.LocationNameSnapshot,
                    approved.SupplyarrItemId,
                    approved.ItemNameSnapshot,
                    Math.Abs(approved.QuantityDelta),
                    approved.UnitOfMeasure,
                    null,
                    null,
                    "available",
                    "approved",
                    request.ApprovedByPersonId,
                    request.ComplianceEvaluationId,
                    request.EvidenceSummary,
                    approvedAtUtc)
                : null;

            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                movementType,
                approved.StaffarrSiteOrgUnitId,
                approved.WarehouseLocationId,
                approved.WarehouseLocationId,
                approved.SupplyarrItemId,
                approved.ItemNameSnapshot,
                Math.Abs(approved.QuantityDelta),
                approved.UnitOfMeasure,
                "untrusted",
                approved.QuantityDelta >= 0m ? "available" : "blocked",
                "loadarr",
                "inventory_adjustment",
                approved.Id,
                request.ReasonCode,
                request.ApprovedByPersonId,
                origin?.Id,
                approvedAtUtc);

            return Results.Ok(new LoadArrAdjustmentMutationResponse(
                approved,
                origin,
                movement));
        })
        .WithName("ApproveLoadArrAdjustment");
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

    private static LoadArrCountResponse? ResolveCount(string id) =>
        CreateCounts()
            .FirstOrDefault(record => string.Equals(record.Id, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrAdjustmentResponse? ResolveAdjustment(string id) =>
        CreateAdjustments()
            .FirstOrDefault(record => string.Equals(record.Id, id, StringComparison.OrdinalIgnoreCase));

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
