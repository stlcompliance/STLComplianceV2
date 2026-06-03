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

        var truckStock = app.MapGroup("/api/v1/truck-stock")
            .WithTags("TruckStock")
            .RequireAuthorization();

        truckStock.MapGet("/", (string? status, string? locationId) =>
        {
            var records = CreateTruckStockRecords()
                .Where(record => status is null
                    || string.Equals(record.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(record => locationId is null
                    || string.Equals(record.TruckLocationId, locationId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(record => record.TruckLocationNameSnapshot, StringComparer.OrdinalIgnoreCase)
                .ThenBy(record => record.TruckStockNumber, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrTruckStockResponse>(records, records.Length));
        })
        .WithName("ListLoadArrTruckStock");

        truckStock.MapGet("/{id}", (string id) =>
        {
            var record = ResolveTruckStockRecord(id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrTruckStock");

        truckStock.MapPost("/{id}/issue", (string id, TruckStockIssueRequest request) =>
        {
            var validation = ValidateTruckStockRequest(request.PersonId, request.ReasonCode, request.Quantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveTruckStockRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var issuedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var issuedQuantity = Math.Min(record.QuantityOnHand, request.Quantity);
            var quantityOnHand = Math.Max(0m, record.QuantityOnHand - issuedQuantity);
            var status = quantityOnHand == 0m
                ? "empty"
                : quantityOnHand < record.MinimumQuantity
                    ? "low_stock"
                    : "ready";
            var updated = record with
            {
                QuantityOnHand = quantityOnHand,
                Status = status,
                LastMovementAtUtc = issuedAtUtc,
                Notes = $"Issued {issuedQuantity} {record.UnitOfMeasure} from truck stock.",
                TraceTags = record.TraceTags.Concat(new[] { $"truck_stock:issue:{issuedAtUtc}" }).ToArray()
            };
            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "truck_stock_issue",
                record.StaffarrSiteOrgUnitId,
                record.TruckLocationId,
                record.TruckLocationId,
                record.SupplyarrItemId,
                record.ItemNameSnapshot,
                issuedQuantity,
                record.UnitOfMeasure,
                record.Status,
                status,
                "loadarr",
                "truck_stock",
                record.Id,
                request.ReasonCode,
                request.PersonId,
                null,
                issuedAtUtc);
            var restockTask = quantityOnHand < record.MinimumQuantity
                ? new LoadArrWarehouseTaskResponse(
                    $"task-{Guid.NewGuid():N}"[..13],
                    "replenish",
                    $"Restock {record.ItemNameSnapshot} on {record.TruckStockNumber}",
                    "normal",
                    "ready",
                    record.TruckLocationNameSnapshot,
                    "Truck Stock User",
                    record.SupplyarrItemId,
                    Math.Max(0m, record.MinimumQuantity - quantityOnHand),
                    DateTimeOffset.UtcNow.AddHours(2).ToString("O"),
                    new[] { "truck_stock_low", "restock_requested" })
                : null;

            return Results.Ok(new LoadArrTruckStockMutationResponse(updated, movement, restockTask));
        })
        .WithName("IssueLoadArrTruckStock");

        truckStock.MapPost("/{id}/return", (string id, TruckStockReturnRequest request) =>
        {
            var validation = ValidateTruckStockRequest(request.PersonId, request.ReasonCode, request.Quantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveTruckStockRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var returnedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var quantityOnHand = record.QuantityOnHand + request.Quantity;
            var status = quantityOnHand < record.MinimumQuantity ? "low_stock" : "ready";
            var updated = record with
            {
                QuantityOnHand = quantityOnHand,
                Status = status,
                LastMovementAtUtc = returnedAtUtc,
                Notes = $"Returned {request.Quantity} {record.UnitOfMeasure} to truck stock.",
                TraceTags = record.TraceTags.Concat(new[] { $"truck_stock:return:{returnedAtUtc}" }).ToArray()
            };
            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "truck_stock_return",
                record.StaffarrSiteOrgUnitId,
                record.TruckLocationId,
                record.TruckLocationId,
                record.SupplyarrItemId,
                record.ItemNameSnapshot,
                request.Quantity,
                record.UnitOfMeasure,
                record.Status,
                status,
                "loadarr",
                "truck_stock",
                record.Id,
                request.ReasonCode,
                request.PersonId,
                null,
                returnedAtUtc);

            return Results.Ok(new LoadArrTruckStockMutationResponse(updated, movement, null));
        })
        .WithName("ReturnLoadArrTruckStock");

        truckStock.MapPost("/{id}/count", (string id, TruckStockCountRequest request) =>
        {
            var validation = ValidateTruckStockCountRequest(request.PersonId, request.ReasonCode, request.CountedQuantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveTruckStockRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var countedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var variance = request.CountedQuantity - record.QuantityOnHand;
            var status = request.CountedQuantity == 0m
                ? "empty"
                : request.CountedQuantity < record.MinimumQuantity
                    ? "low_stock"
                    : "ready";
            var updated = record with
            {
                QuantityOnHand = request.CountedQuantity,
                Status = status,
                LastCountedAtUtc = countedAtUtc,
                LastMovementAtUtc = countedAtUtc,
                Notes = $"Counted at {countedAtUtc}; variance {variance:+0.##;-0.##;0}.",
                TraceTags = record.TraceTags.Concat(new[] { $"truck_stock:count:{countedAtUtc}" }).ToArray()
            };
            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "truck_stock_count",
                record.StaffarrSiteOrgUnitId,
                record.TruckLocationId,
                record.TruckLocationId,
                record.SupplyarrItemId,
                record.ItemNameSnapshot,
                Math.Abs(variance),
                record.UnitOfMeasure,
                record.Status,
                status,
                "loadarr",
                "truck_stock",
                record.Id,
                request.ReasonCode,
                request.PersonId,
                null,
                countedAtUtc);
            var restockTask = request.CountedQuantity < record.MinimumQuantity
                ? new LoadArrWarehouseTaskResponse(
                    $"task-{Guid.NewGuid():N}"[..13],
                    "replenish",
                    $"Restock {record.ItemNameSnapshot} on {record.TruckStockNumber}",
                    "normal",
                    "ready",
                    record.TruckLocationNameSnapshot,
                    "Truck Stock User",
                    record.SupplyarrItemId,
                    Math.Max(0m, record.MinimumQuantity - request.CountedQuantity),
                    DateTimeOffset.UtcNow.AddHours(2).ToString("O"),
                    new[] { "truck_stock_low", "restock_requested" })
                : null;

            return Results.Ok(new LoadArrTruckStockMutationResponse(updated, movement, restockTask));
        })
        .WithName("CountLoadArrTruckStock");

        var kits = app.MapGroup("/api/v1/kits")
            .WithTags("Kits")
            .RequireAuthorization();

        kits.MapGet("/", (string? status, string? locationId) =>
        {
            var records = CreateKitRecords()
                .Where(record => status is null
                    || string.Equals(record.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(record => locationId is null
                    || string.Equals(record.LocationId, locationId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(record => record.LocationNameSnapshot, StringComparer.OrdinalIgnoreCase)
                .ThenBy(record => record.KitNumber, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrKitResponse>(records, records.Length));
        })
        .WithName("ListLoadArrKits");

        kits.MapGet("/{id}", (string id) =>
        {
            var record = ResolveKitRecord(id);
            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrKit");

        kits.MapPost("/{id}/build", (string id, KitMutationRequest request) =>
        {
            var validation = ValidateKitMutationRequest(request.PersonId, request.ReasonCode, request.Quantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveKitRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var changedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var builtQuantity = record.QuantityOnHand + request.Quantity;
            var status = builtQuantity <= 0m ? "broken" : builtQuantity < record.MinimumQuantity ? "needs_replenishment" : "built";
            var updated = record with
            {
                QuantityOnHand = builtQuantity,
                Status = status,
                LastActionAtUtc = changedAtUtc,
                Notes = $"Built {request.Quantity} kit(s) from LoadArr components.",
                TraceTags = record.TraceTags.Concat(new[] { $"kit:build:{changedAtUtc}" }).ToArray()
            };
            var followUpTask = status == "needs_replenishment"
                ? new LoadArrWarehouseTaskResponse(
                    $"task-{Guid.NewGuid():N}"[..13],
                    "replenish",
                    $"Replenish {record.KitNameSnapshot}",
                    "normal",
                    "ready",
                    record.LocationNameSnapshot,
                    "Kit Coordinator",
                    record.PrimaryItemId,
                    Math.Max(0m, record.MinimumQuantity - builtQuantity),
                    DateTimeOffset.UtcNow.AddHours(2).ToString("O"),
                    new[] { "kit_low", "replenish_requested" })
                : null;
            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "kit_build",
                record.StaffarrSiteOrgUnitId,
                record.LocationId,
                record.LocationId,
                record.PrimaryItemId,
                record.KitNameSnapshot,
                request.Quantity,
                record.UnitOfMeasure,
                record.Status,
                status,
                "loadarr",
                "kit",
                record.Id,
                request.ReasonCode,
                request.PersonId,
                null,
                changedAtUtc);

            return Results.Ok(new LoadArrKitMutationResponse(updated, movement, followUpTask));
        })
        .WithName("BuildLoadArrKit");

        kits.MapPost("/{id}/break", (string id, KitMutationRequest request) =>
        {
            var validation = ValidateKitMutationRequest(request.PersonId, request.ReasonCode, request.Quantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveKitRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var changedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var brokenQuantity = Math.Max(0m, record.QuantityOnHand - request.Quantity);
            var status = brokenQuantity == 0m ? "broken" : brokenQuantity < record.MinimumQuantity ? "needs_replenishment" : "built";
            var updated = record with
            {
                QuantityOnHand = brokenQuantity,
                Status = status,
                LastActionAtUtc = changedAtUtc,
                Notes = $"Broke down {request.Quantity} kit(s) for component recovery.",
                TraceTags = record.TraceTags.Concat(new[] { $"kit:break:{changedAtUtc}" }).ToArray()
            };
            var followUpTask = status == "needs_replenishment"
                ? new LoadArrWarehouseTaskResponse(
                    $"task-{Guid.NewGuid():N}"[..13],
                    "replenish",
                    $"Replenish {record.KitNameSnapshot}",
                    "normal",
                    "ready",
                    record.LocationNameSnapshot,
                    "Kit Coordinator",
                    record.PrimaryItemId,
                    Math.Max(0m, record.MinimumQuantity - brokenQuantity),
                    DateTimeOffset.UtcNow.AddHours(2).ToString("O"),
                    new[] { "kit_low", "replenish_requested" })
                : null;
            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "kit_break",
                record.StaffarrSiteOrgUnitId,
                record.LocationId,
                record.LocationId,
                record.PrimaryItemId,
                record.KitNameSnapshot,
                request.Quantity,
                record.UnitOfMeasure,
                record.Status,
                status,
                "loadarr",
                "kit",
                record.Id,
                request.ReasonCode,
                request.PersonId,
                null,
                changedAtUtc);

            return Results.Ok(new LoadArrKitMutationResponse(updated, movement, followUpTask));
        })
        .WithName("BreakLoadArrKit");

        kits.MapPost("/{id}/replenish", (string id, KitMutationRequest request) =>
        {
            var validation = ValidateKitMutationRequest(request.PersonId, request.ReasonCode, request.Quantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveKitRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var changedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var replenishedQuantity = record.QuantityOnHand + request.Quantity;
            var status = replenishedQuantity < record.MinimumQuantity ? "needs_replenishment" : "built";
            var updated = record with
            {
                QuantityOnHand = replenishedQuantity,
                Status = status,
                LastActionAtUtc = changedAtUtc,
                Notes = $"Replenished {request.Quantity} kit(s) at the warehouse.",
                TraceTags = record.TraceTags.Concat(new[] { $"kit:replenish:{changedAtUtc}" }).ToArray()
            };
            var followUpTask = status == "needs_replenishment"
                ? new LoadArrWarehouseTaskResponse(
                    $"task-{Guid.NewGuid():N}"[..13],
                    "replenish",
                    $"Replenish {record.KitNameSnapshot}",
                    "normal",
                    "ready",
                    record.LocationNameSnapshot,
                    "Kit Coordinator",
                    record.PrimaryItemId,
                    Math.Max(0m, record.MinimumQuantity - replenishedQuantity),
                    DateTimeOffset.UtcNow.AddHours(2).ToString("O"),
                    new[] { "kit_low", "replenish_requested" })
                : null;
            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "kit_replenish",
                record.StaffarrSiteOrgUnitId,
                record.LocationId,
                record.LocationId,
                record.PrimaryItemId,
                record.KitNameSnapshot,
                request.Quantity,
                record.UnitOfMeasure,
                record.Status,
                status,
                "loadarr",
                "kit",
                record.Id,
                request.ReasonCode,
                request.PersonId,
                null,
                changedAtUtc);

            return Results.Ok(new LoadArrKitMutationResponse(updated, movement, followUpTask));
        })
        .WithName("ReplenishLoadArrKit");

        kits.MapPost("/{id}/reserve", (string id, KitLifecycleActionRequest request) =>
        {
            var validation = ValidateKitLifecycleActionRequest(request.PersonId, request.ReasonCode, request.Quantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveKitRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var changedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var reservedQuantity = Math.Max(0m, record.QuantityOnHand - request.Quantity);
            var status = reservedQuantity < record.MinimumQuantity ? "needs_replenishment" : "reserved";
            var updated = record with
            {
                QuantityOnHand = reservedQuantity,
                Status = status,
                LastActionAtUtc = changedAtUtc,
                LastMovementAtUtc = changedAtUtc,
                Notes = $"Reserved {request.Quantity} kit(s) for controlled use.",
                TraceTags = record.TraceTags.Concat(new[] { $"kit:reserve:{changedAtUtc}" }).ToArray()
            };
            var movement = CreateKitMovement(record, "kit_reserve", request.Quantity, request.ReasonCode, request.PersonId, changedAtUtc, record.Status, updated.Status, record.LocationId, record.LocationNameSnapshot, record.LocationId, record.LocationNameSnapshot, record.KitNameSnapshot);
            var followUpTask = status == "needs_replenishment"
                ? CreateKitFollowUpTask(updated, record.MinimumQuantity - reservedQuantity)
                : null;

            return Results.Ok(new LoadArrKitMutationResponse(updated, movement, followUpTask));
        })
        .WithName("ReserveLoadArrKit");

        kits.MapPost("/{id}/pick", (string id, KitLifecycleActionRequest request) =>
        {
            var validation = ValidateKitLifecycleActionRequest(request.PersonId, request.ReasonCode, request.Quantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveKitRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var changedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var pickedQuantity = Math.Max(0m, record.QuantityOnHand - request.Quantity);
            var status = pickedQuantity < record.MinimumQuantity ? "needs_replenishment" : "picked";
            var updated = record with
            {
                QuantityOnHand = pickedQuantity,
                Status = status,
                LastActionAtUtc = changedAtUtc,
                LastMovementAtUtc = changedAtUtc,
                Notes = $"Picked {request.Quantity} kit(s) from controlled stock.",
                TraceTags = record.TraceTags.Concat(new[] { $"kit:pick:{changedAtUtc}" }).ToArray()
            };
            var movement = CreateKitMovement(record, "kit_pick", request.Quantity, request.ReasonCode, request.PersonId, changedAtUtc, record.Status, updated.Status, record.LocationId, record.LocationNameSnapshot, record.LocationId, record.LocationNameSnapshot, record.KitNameSnapshot);
            var followUpTask = status == "needs_replenishment"
                ? CreateKitFollowUpTask(updated, record.MinimumQuantity - pickedQuantity)
                : null;

            return Results.Ok(new LoadArrKitMutationResponse(updated, movement, followUpTask));
        })
        .WithName("PickLoadArrKit");

        kits.MapPost("/{id}/inspect", (string id, KitLifecycleActionRequest request) =>
        {
            var validation = ValidateKitLifecycleActionRequest(request.PersonId, request.ReasonCode, request.Quantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveKitRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var changedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var status = record.QuantityOnHand < record.MinimumQuantity ? "needs_replenishment" : "inspected";
            var updated = record with
            {
                Status = status,
                LastActionAtUtc = changedAtUtc,
                LastMovementAtUtc = changedAtUtc,
                Notes = $"Inspected by {request.PersonId} for readiness and condition.",
                TraceTags = record.TraceTags.Concat(new[] { $"kit:inspect:{changedAtUtc}" }).ToArray()
            };
            var movement = CreateKitMovement(record, "kit_inspect", request.Quantity, request.ReasonCode, request.PersonId, changedAtUtc, record.Status, updated.Status, record.LocationId, record.LocationNameSnapshot, record.LocationId, record.LocationNameSnapshot, record.KitNameSnapshot);
            var followUpTask = status == "needs_replenishment"
                ? CreateKitFollowUpTask(updated, record.MinimumQuantity - record.QuantityOnHand)
                : null;

            return Results.Ok(new LoadArrKitMutationResponse(updated, movement, followUpTask));
        })
        .WithName("InspectLoadArrKit");

        kits.MapPost("/{id}/assign", (string id, KitAssignRequest request) =>
        {
            var validation = ValidateKitAssignRequest(request.PersonId, request.TargetPersonId, request.TargetPersonNameSnapshot, request.ReasonCode);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveKitRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var changedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var updated = record with
            {
                AssignedPersonId = request.TargetPersonId,
                AssignedPersonNameSnapshot = request.TargetPersonNameSnapshot,
                Status = "assigned",
                LastActionAtUtc = changedAtUtc,
                LastMovementAtUtc = changedAtUtc,
                Notes = $"Assigned kit to {request.TargetPersonNameSnapshot}.",
                TraceTags = record.TraceTags.Concat(new[] { $"kit:assign:{changedAtUtc}" }).ToArray()
            };
            var movement = CreateKitMovement(record, "kit_assign", 0m, request.ReasonCode, request.PersonId, changedAtUtc, record.Status, updated.Status, record.LocationId, record.LocationNameSnapshot, record.LocationId, record.LocationNameSnapshot, record.KitNameSnapshot);

            return Results.Ok(new LoadArrKitMutationResponse(updated, movement, null));
        })
        .WithName("AssignLoadArrKit");

        kits.MapPost("/{id}/return", (string id, KitLifecycleActionRequest request) =>
        {
            var validation = ValidateKitLifecycleActionRequest(request.PersonId, request.ReasonCode, request.Quantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveKitRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var changedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var returnedQuantity = record.QuantityOnHand + request.Quantity;
            var status = returnedQuantity < record.MinimumQuantity ? "needs_replenishment" : "returned";
            var updated = record with
            {
                QuantityOnHand = returnedQuantity,
                Status = status,
                LastActionAtUtc = changedAtUtc,
                LastMovementAtUtc = changedAtUtc,
                Notes = $"Returned {request.Quantity} kit(s) to stock.",
                TraceTags = record.TraceTags.Concat(new[] { $"kit:return:{changedAtUtc}" }).ToArray()
            };
            var movement = CreateKitMovement(record, "kit_return", request.Quantity, request.ReasonCode, request.PersonId, changedAtUtc, record.Status, updated.Status, record.LocationId, record.LocationNameSnapshot, record.LocationId, record.LocationNameSnapshot, record.KitNameSnapshot);
            var followUpTask = status == "needs_replenishment"
                ? CreateKitFollowUpTask(updated, record.MinimumQuantity - returnedQuantity)
                : null;

            return Results.Ok(new LoadArrKitMutationResponse(updated, movement, followUpTask));
        })
        .WithName("ReturnLoadArrKit");

        kits.MapPost("/{id}/expire-components", (string id, KitLifecycleActionRequest request) =>
        {
            var validation = ValidateKitLifecycleActionRequest(request.PersonId, request.ReasonCode, request.Quantity);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveKitRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var changedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var updated = record with
            {
                QuantityOnHand = 0m,
                Status = "expired",
                LastActionAtUtc = changedAtUtc,
                LastMovementAtUtc = changedAtUtc,
                Notes = $"Expired kit components as of controlled review.",
                TraceTags = record.TraceTags.Concat(new[] { $"kit:expire:{changedAtUtc}" }).ToArray()
            };
            var movement = CreateKitMovement(record, "kit_expire_components", request.Quantity, request.ReasonCode, request.PersonId, changedAtUtc, record.Status, updated.Status, record.LocationId, record.LocationNameSnapshot, record.LocationId, record.LocationNameSnapshot, record.KitNameSnapshot);

            return Results.Ok(new LoadArrKitMutationResponse(updated, movement, null));
        })
        .WithName("ExpireKitComponents");

        kits.MapPost("/{id}/track-location", (string id, KitTrackLocationRequest request) =>
        {
            var validation = ValidateKitTrackLocationRequest(request.PersonId, request.TargetLocationId, request.ReasonCode);
            if (validation is not null)
            {
                return validation;
            }

            var record = ResolveKitRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var targetLocation = ResolveLocation(request.TargetLocationId);
            if (targetLocation is null)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_location",
                    "Kit location tracking requires a valid StaffArr-owned location reference."));
            }

            var changedAtUtc = DateTimeOffset.UtcNow.ToString("O");
            var updated = record with
            {
                LocationId = targetLocation.Id,
                LocationNameSnapshot = targetLocation.Name,
                Status = "tracked",
                LastActionAtUtc = changedAtUtc,
                LastMovementAtUtc = changedAtUtc,
                Notes = $"Tracked kit location to {targetLocation.Name}.",
                TraceTags = record.TraceTags.Concat(new[] { $"kit:track:{changedAtUtc}" }).ToArray()
            };
            var movement = CreateKitMovement(record, "kit_track_location", 0m, request.ReasonCode, request.PersonId, changedAtUtc, record.Status, updated.Status, targetLocation.Id, targetLocation.Name, targetLocation.Id, targetLocation.Name, record.KitNameSnapshot);

            return Results.Ok(new LoadArrKitMutationResponse(updated, movement, null));
        })
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
