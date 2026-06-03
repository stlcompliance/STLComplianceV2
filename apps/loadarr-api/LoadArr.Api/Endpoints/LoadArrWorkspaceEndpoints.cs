namespace LoadArr.Api.Endpoints;

public static class LoadArrWorkspaceEndpoints
{
    public static void MapLoadArrWorkspaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/workspace")
            .WithTags("Workspace")
            .RequireAuthorization();

        group.MapGet("/site-sources", () => Results.Ok(new
        {
            canonicalInternalSites = "staffarr",
            canonicalLocations = "staffarr",
            canonicalSiteField = "staffarrSiteOrgUnitId",
            siteSnapshotField = "staffarrSiteNameSnapshot",
            canonicalLocationField = "staffarrLocationId",
            locationSnapshotField = "staffarrLocationNameSnapshot",
            consumingProduct = "loadarr"
        }))
        .WithName("GetLoadArrSiteSources");

        group.MapGet("/summary", () => Results.Ok(CreateWorkspaceSummary()))
            .WithName("GetLoadArrWorkspaceSummary");

        group.MapGet("/locations", (
            string? staffarrSiteOrgUnitId,
            string? locationType,
            bool? active) =>
        {
            var locations = CreateWorkspaceSummary().Locations
                .Where(location => staffarrSiteOrgUnitId is null
                    || string.Equals(location.StaffarrSiteOrgUnitId, staffarrSiteOrgUnitId, StringComparison.OrdinalIgnoreCase))
                .Where(location => locationType is null
                    || string.Equals(location.LocationType, locationType, StringComparison.OrdinalIgnoreCase))
                .Where(location => active is null || location.Active == active.Value)
                .OrderBy(location => location.StaffarrSiteNameSnapshot, StringComparer.OrdinalIgnoreCase)
                .ThenBy(location => location.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrLocationResponse>(locations, locations.Length));
        })
        .WithName("ListLoadArrLocations");

        group.MapGet("/locations/{id}", (string id) =>
        {
            var location = CreateWorkspaceSummary().Locations
                .SingleOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            return location is null ? Results.NotFound() : Results.Ok(location);
        })
        .WithName("GetLoadArrLocation");

        group.MapGet("/locations/tree", () =>
        {
            var nodes = CreateWorkspaceSummary().Locations
                .GroupBy(location => new
                {
                    location.StaffarrSiteOrgUnitId,
                    location.StaffarrSiteNameSnapshot
                })
                .OrderBy(group => group.Key.StaffarrSiteNameSnapshot, StringComparer.OrdinalIgnoreCase)
                .Select(group => new LoadArrLocationTreeNodeResponse(
                    group.Key.StaffarrSiteOrgUnitId,
                    group.Key.StaffarrSiteNameSnapshot,
                    "staffarr_site",
                    null,
                    group
                        .OrderBy(location => location.Path, StringComparer.OrdinalIgnoreCase)
                        .Select(location => new LoadArrLocationTreeNodeResponse(
                            location.Id,
                            location.Name,
                            location.LocationType,
                            location.Id,
                            Array.Empty<LoadArrLocationTreeNodeResponse>()))
                        .ToArray()))
                .ToArray();

            return Results.Ok(nodes);
        })
        .WithName("GetLoadArrLocationTree");

        group.MapGet("/inventory", (
            string? query,
            string? state,
            string? locationId) =>
        {
            var inventory = CreateWorkspaceSummary().Inventory
                .Where(item => state is null
                    || string.Equals(item.State, state, StringComparison.OrdinalIgnoreCase))
                .Where(item => locationId is null
                    || string.Equals(item.LocationId, locationId, StringComparison.OrdinalIgnoreCase))
                .Where(item => query is null || InventoryMatchesQuery(item, query))
                .OrderBy(item => item.LocationNameSnapshot, StringComparer.OrdinalIgnoreCase)
                .ThenBy(item => item.ItemNameSnapshot, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrInventoryBalanceResponse>(inventory, inventory.Length));
        })
        .WithName("ListLoadArrInventory");

        group.MapGet("/supplyarr-item-references", (string? query) =>
        {
            var items = CreateSupplyArrItemReferences()
                .Where(item => query is null || SupplyArrItemReferenceMatchesQuery(item, query))
                .OrderBy(item => item.ItemNameSnapshot, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrSupplyArrItemReferenceResponse>(items, items.Length));
        })
        .WithName("ListLoadArrSupplyArrItemReferences");

        group.MapGet("/tasks", (
            string? status,
            string? priority,
            string? taskType) =>
        {
            var tasks = CreateWorkspaceSummary().Tasks
                .Where(task => status is null
                    || string.Equals(task.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(task => priority is null
                    || string.Equals(task.Priority, priority, StringComparison.OrdinalIgnoreCase))
                .Where(task => taskType is null
                    || string.Equals(task.TaskType, taskType, StringComparison.OrdinalIgnoreCase))
                .OrderBy(task => task.DueAtUtc, StringComparer.OrdinalIgnoreCase)
                .ThenBy(task => task.Priority, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrWarehouseTaskResponse>(tasks, tasks.Length));
        })
        .WithName("ListLoadArrWarehouseTasks");

        group.MapGet("/holds", (string? status, string? holdType) =>
        {
            var holds = CreateWorkspaceSummary().Holds
                .Where(hold => status is null
                    || string.Equals(hold.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(hold => holdType is null
                    || string.Equals(hold.HoldType, holdType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(hold => hold.OpenedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrHoldResponse>(holds, holds.Length));
        })
        .WithName("ListLoadArrHolds");

        group.MapGet("/route-handoffs", (string? targetProduct, string? status) =>
        {
            var handoffs = CreateWorkspaceSummary().RouteHandoffs
                .Where(handoff => targetProduct is null
                    || string.Equals(handoff.TargetProduct, targetProduct, StringComparison.OrdinalIgnoreCase))
                .Where(handoff => status is null
                    || string.Equals(handoff.Status, status, StringComparison.OrdinalIgnoreCase))
                .OrderBy(handoff => handoff.TargetProduct, StringComparer.OrdinalIgnoreCase)
                .ThenBy(handoff => handoff.TargetReference, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrRouteHandoffResponse>(handoffs, handoffs.Length));
        })
        .WithName("ListLoadArrRouteHandoffs");

        group.MapGet("/evidence", (string? evidenceType, string? locationNameSnapshot) =>
        {
            var evidence = CreateWorkspaceSummary().Evidence
                .Where(item => evidenceType is null
                    || string.Equals(item.EvidenceType, evidenceType, StringComparison.OrdinalIgnoreCase))
                .Where(item => locationNameSnapshot is null
                    || string.Equals(item.LocationNameSnapshot, locationNameSnapshot, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(item => item.CapturedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrEvidenceResponse>(evidence, evidence.Length));
        })
        .WithName("ListLoadArrEvidence");

        group.MapGet("/unexplained-inventory", (string? status, string? locationId) =>
        {
            var records = CreateUnexplainedInventoryRecords()
                .Where(record => status is null
                    || string.Equals(record.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(record => locationId is null
                    || string.Equals(record.WarehouseLocationId, locationId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(record => record.DiscoveredAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrUnexplainedInventoryRecordResponse>(records, records.Length));
        })
        .WithName("ListLoadArrWorkspaceUnexplainedInventory");

        var receiving = app.MapGroup("/api/v1/receiving")
            .WithTags("Receiving")
            .RequireAuthorization();

        receiving.MapGet("/", (string? status, string? receivingType) =>
        {
            var sessions = CreateReceivingSessions()
                .Where(session => status is null
                    || string.Equals(session.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(session => receivingType is null
                    || string.Equals(session.ReceivingType, receivingType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(session => session.StartedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrReceivingSessionResponse>(sessions, sessions.Length));
        })
        .WithName("ListLoadArrReceivingSessions");

        receiving.MapGet("/{id}", (string id) =>
        {
            var session = CreateReceivingSessions()
                .SingleOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            return session is null ? Results.NotFound() : Results.Ok(session);
        })
        .WithName("GetLoadArrReceivingSession");

        receiving.MapPost("/", (CreateLoadArrReceivingSessionRequest request) =>
        {
            var location = CreateWorkspaceSummary().Locations
                .FirstOrDefault(candidate => string.Equals(candidate.Id, request.WarehouseLocationId, StringComparison.OrdinalIgnoreCase));
            var itemSnapshot = ResolveSupplyArrItemReference(request.SupplyarrItemId);

            if (location is null)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_location",
                    "Receiving requires a valid StaffArr-owned location reference."));
            }

            if (itemSnapshot is null)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_supplyarr_item_reference",
                    "Receiving requires an item reference that exists in SupplyArr."));
            }

            var session = new LoadArrReceivingSessionResponse(
                $"recv-{Guid.NewGuid():N}"[..13],
                $"RCV-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                string.IsNullOrWhiteSpace(request.ReceivingType) ? "manual" : request.ReceivingType,
                "open",
                location.StaffarrSiteOrgUnitId,
                location.StaffarrSiteNameSnapshot,
                request.SourceProductKey,
                request.SourceObjectType,
                request.SourceObjectId,
                request.SupplierNameSnapshot,
                request.StartedByPersonId,
                null,
                DateTimeOffset.UtcNow.ToString("O"),
                null,
                new[]
                {
                    new LoadArrReceivingLineResponse(
                        $"line-{Guid.NewGuid():N}"[..13],
                        request.SupplyarrItemId,
                        itemSnapshot.ItemNameSnapshot,
                        request.ExpectedQuantity,
                        request.ReceivedQuantity,
                        itemSnapshot.UnitOfMeasureSnapshot,
                        location.Id,
                        location.Name,
                        request.LotCode,
                        request.SerialCode,
                        request.Condition,
                        "ready_to_complete",
                        request.DiscrepancyReasonCode,
                        request.EvidenceSummary)
                });

            return Results.Created($"/api/v1/receiving/{session.Id}", session);
        })
        .WithName("CreateLoadArrReceivingSession");

        receiving.MapPost("/{id}/lines", (string id, AddLoadArrReceivingLineRequest request) =>
        {
            var session = CreateReceivingSessions()
                .FirstOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            if (session is null)
            {
                return Results.NotFound();
            }

            var location = CreateWorkspaceSummary().Locations
                .FirstOrDefault(candidate => string.Equals(candidate.Id, request.WarehouseLocationId, StringComparison.OrdinalIgnoreCase));
            var itemSnapshot = ResolveSupplyArrItemReference(request.SupplyarrItemId);

            if (location is null)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_location",
                    "Receiving lines require a valid StaffArr-owned location reference."));
            }

            if (itemSnapshot is null)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_supplyarr_item_reference",
                    "Receiving lines require an item reference that exists in SupplyArr."));
            }

            var line = new LoadArrReceivingLineResponse(
                $"line-{Guid.NewGuid():N}"[..13],
                request.SupplyarrItemId,
                itemSnapshot.ItemNameSnapshot,
                request.ExpectedQuantity,
                request.ReceivedQuantity,
                itemSnapshot.UnitOfMeasureSnapshot,
                location.Id,
                location.Name,
                request.LotCode,
                request.SerialCode,
                request.Condition,
                "ready_to_complete",
                request.DiscrepancyReasonCode,
                request.EvidenceSummary);

            return Results.Ok(line);
        })
        .WithName("AddLoadArrReceivingLine");

        receiving.MapPost("/{id}/complete", (string id, CompleteLoadArrReceivingSessionRequest request) =>
        {
            if (ResolveSupplyArrItemReference(request.SupplyarrItemId) is null)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_supplyarr_item_reference",
                    "Receiving completion requires an item reference that exists in SupplyArr."));
            }

            var session = CreateReceivingSessions()
                .FirstOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase))
                ?? CreateReceivingSessionFromCompletion(id, request);

            if (session.Lines.Count == 0)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "empty_receiving_session",
                    "Receiving cannot complete without at least one line."));
            }

            var line = session.Lines.First();
            if (line.ReceivedQuantity <= 0)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_received_quantity",
                    "Received quantity must be greater than zero."));
            }

            var origin = new LoadArrInventoryOriginEventResponse(
                $"origin-{Guid.NewGuid():N}"[..15],
                "purchase_receipt",
                session.SourceProductKey,
                session.SourceObjectType,
                session.SourceObjectId,
                session.StaffarrSiteOrgUnitId,
                session.StaffarrSiteNameSnapshot,
                line.WarehouseLocationId,
                line.LocationNameSnapshot,
                line.SupplyarrItemId,
                line.ItemNameSnapshot,
                line.ReceivedQuantity,
                line.UnitOfMeasure,
                line.LotCode,
                line.SerialCode,
                line.Condition,
                "available",
                request.CompletedByPersonId,
                request.ComplianceEvaluationId,
                request.EvidenceSummary ?? line.EvidenceSummary,
                DateTimeOffset.UtcNow.ToString("O"));

            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "receive",
                session.StaffarrSiteOrgUnitId,
                null,
                line.WarehouseLocationId,
                line.SupplyarrItemId,
                line.ItemNameSnapshot,
                line.ReceivedQuantity,
                line.UnitOfMeasure,
                null,
                "available",
                session.SourceProductKey,
                session.SourceObjectType,
                session.SourceObjectId,
                "manual_receiving_complete",
                request.CompletedByPersonId,
                origin.Id,
                DateTimeOffset.UtcNow.ToString("O"));

            var balance = new LoadArrInventoryBalanceResponse(
                $"bal-{Guid.NewGuid():N}"[..12],
                line.SupplyarrItemId,
                line.ItemNameSnapshot,
                line.UnitOfMeasure,
                "available",
                line.WarehouseLocationId,
                line.LocationNameSnapshot,
                line.ReceivedQuantity,
                0,
                0,
                0,
                origin.OriginType,
                session.ReceivingNumber,
                new[] { $"origin:{origin.Id}", $"receiving:{session.Id}" },
                "Created from completed receiving session");

            var putawayTask = new LoadArrWarehouseTaskResponse(
                $"task-{Guid.NewGuid():N}"[..13],
                "putaway",
                $"Put away {line.ItemNameSnapshot}",
                "normal",
                "ready",
                line.LocationNameSnapshot,
                "Warehouse Associate",
                line.SupplyarrItemId,
                line.ReceivedQuantity,
                DateTimeOffset.UtcNow.AddHours(4).ToString("O"),
                new[] { "origin_event_created", "movement_recorded", "location_scan_required" });

            return Results.Ok(new LoadArrReceivingCompletionResponse(
                session with
                {
                    Status = "completed",
                    CompletedByPersonId = request.CompletedByPersonId,
                    CompletedAtUtc = DateTimeOffset.UtcNow.ToString("O")
                },
                origin,
                movement,
                balance,
                putawayTask));
        })
        .WithName("CompleteLoadArrReceivingSession");

        receiving.MapPost("/{id}/cancel", (string id, CancelLoadArrReceivingSessionRequest request) =>
        {
            var session = CreateReceivingSessions()
                .FirstOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            return session is null
                ? Results.NotFound()
                : Results.Ok(session with
                {
                    Status = "canceled",
                    CompletedByPersonId = request.CanceledByPersonId,
                    CompletedAtUtc = DateTimeOffset.UtcNow.ToString("O")
                });
        })
        .WithName("CancelLoadArrReceivingSession");

        var transfers = app.MapGroup("/api/v1/transfers")
            .WithTags("Transfers")
            .RequireAuthorization();

        transfers.MapGet("/", (string? status, string? transferType) =>
        {
            var orders = CreateTransferOrders()
                .Where(order => status is null
                    || string.Equals(order.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(order => transferType is null
                    || string.Equals(order.TransferType, transferType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(order => order.CreatedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrTransferOrderResponse>(orders, orders.Length));
        })
        .WithName("ListLoadArrTransferOrders");

        transfers.MapGet("/{id}", (string id) =>
        {
            var order = CreateTransferOrders()
                .SingleOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            return order is null ? Results.NotFound() : Results.Ok(order);
        })
        .WithName("GetLoadArrTransferOrder");

        transfers.MapPost("/", (CreateLoadArrTransferOrderRequest request) =>
        {
            var validation = ValidateTransferRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            var fromLocation = ResolveLocation(request.FromLocationId)!;
            var toLocation = ResolveLocation(request.ToLocationId)!;
            var itemSnapshot = ResolveSupplyArrItemReference(request.SupplyarrItemId)!;

            var order = new LoadArrTransferOrderResponse(
                $"xfer-{Guid.NewGuid():N}"[..13],
                $"TRF-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                "draft",
                string.IsNullOrWhiteSpace(request.TransferType) ? "bin_to_bin" : request.TransferType,
                fromLocation.StaffarrSiteOrgUnitId,
                fromLocation.StaffarrSiteNameSnapshot,
                fromLocation.Id,
                fromLocation.Name,
                toLocation.Id,
                toLocation.Name,
                request.RequestedByPersonId,
                null,
                request.ReasonCode,
                DateTimeOffset.UtcNow.ToString("O"),
                null,
                new[]
                {
                    new LoadArrTransferLineResponse(
                        $"xfer-line-{Guid.NewGuid():N}"[..18],
                        itemSnapshot.SupplyarrItemId,
                        itemSnapshot.ItemNameSnapshot,
                        request.Quantity,
                        itemSnapshot.UnitOfMeasureSnapshot,
                        request.LotCode,
                        request.SerialCode,
                        "ready")
                });

            return Results.Created($"/api/v1/transfers/{order.Id}", order);
        })
        .WithName("CreateLoadArrTransferOrder");

        transfers.MapPost("/{id}/complete", (string id, CompleteLoadArrTransferOrderRequest request) =>
        {
            var validation = ValidateTransferRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            var existingOrder = CreateTransferOrders()
                .FirstOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));
            var fromLocation = ResolveLocation(request.FromLocationId)!;
            var toLocation = ResolveLocation(request.ToLocationId)!;
            var itemSnapshot = ResolveSupplyArrItemReference(request.SupplyarrItemId)!;
            var sourceBalance = ResolveInventoryBalance(request.SupplyarrItemId, request.FromLocationId);

            if (sourceBalance is null || sourceBalance.QuantityOnHand - sourceBalance.QuantityReserved - sourceBalance.QuantityAllocated < request.Quantity)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "insufficient_available_quantity",
                    "Transfer quantity must be available at the source StaffArr-owned location used by LoadArr."));
            }

            var order = existingOrder ?? new LoadArrTransferOrderResponse(
                id,
                $"TRF-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                "draft",
                string.IsNullOrWhiteSpace(request.TransferType) ? "bin_to_bin" : request.TransferType,
                fromLocation.StaffarrSiteOrgUnitId,
                fromLocation.StaffarrSiteNameSnapshot,
                fromLocation.Id,
                fromLocation.Name,
                toLocation.Id,
                toLocation.Name,
                request.CompletedByPersonId,
                null,
                request.ReasonCode,
                DateTimeOffset.UtcNow.ToString("O"),
                null,
                new[]
                {
                    new LoadArrTransferLineResponse(
                        $"xfer-line-{Guid.NewGuid():N}"[..18],
                        itemSnapshot.SupplyarrItemId,
                        itemSnapshot.ItemNameSnapshot,
                        request.Quantity,
                        itemSnapshot.UnitOfMeasureSnapshot,
                        request.LotCode,
                        request.SerialCode,
                        "ready")
                });

            var completedOrder = order with
            {
                Status = "completed",
                CompletedByPersonId = request.CompletedByPersonId,
                CompletedAtUtc = DateTimeOffset.UtcNow.ToString("O")
            };

            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "transfer",
                fromLocation.StaffarrSiteOrgUnitId,
                fromLocation.Id,
                toLocation.Id,
                itemSnapshot.SupplyarrItemId,
                itemSnapshot.ItemNameSnapshot,
                request.Quantity,
                itemSnapshot.UnitOfMeasureSnapshot,
                "available",
                "available",
                "loadarr",
                "transfer_order",
                completedOrder.Id,
                request.ReasonCode,
                request.CompletedByPersonId,
                null,
                DateTimeOffset.UtcNow.ToString("O"));

            var sourceBalanceAfter = sourceBalance with
            {
                QuantityOnHand = sourceBalance.QuantityOnHand - request.Quantity,
                TraceTags = sourceBalance.TraceTags.Concat(new[] { $"transfer-out:{completedOrder.Id}" }).ToArray(),
                Notes = $"Transferred {request.Quantity} {sourceBalance.UnitOfMeasureSnapshot} to {toLocation.Name}"
            };

            var destinationBalance = new LoadArrInventoryBalanceResponse(
                $"bal-{Guid.NewGuid():N}"[..12],
                itemSnapshot.SupplyarrItemId,
                itemSnapshot.ItemNameSnapshot,
                itemSnapshot.UnitOfMeasureSnapshot,
                "available",
                toLocation.Id,
                toLocation.Name,
                request.Quantity,
                0,
                0,
                0,
                sourceBalance.OriginEventType,
                sourceBalance.OriginReference,
                new[] { $"transfer-in:{completedOrder.Id}", $"movement:{movement.Id}" },
                $"Created from transfer {completedOrder.TransferNumber}");

            var task = new LoadArrWarehouseTaskResponse(
                $"task-{Guid.NewGuid():N}"[..13],
                "transfer",
                $"Move {itemSnapshot.ItemNameSnapshot} to {toLocation.Name}",
                "normal",
                "completed",
                toLocation.Name,
                "Warehouse Associate",
                itemSnapshot.SupplyarrItemId,
                request.Quantity,
                DateTimeOffset.UtcNow.ToString("O"),
                new[] { "source_scan_required", "destination_scan_required", "movement_recorded" });

            return Results.Ok(new LoadArrTransferCompletionResponse(
                completedOrder,
                movement,
                sourceBalanceAfter,
                destinationBalance,
                task));
        })
        .WithName("CompleteLoadArrTransferOrder");

        transfers.MapPost("/{id}/cancel", (string id, CancelLoadArrTransferOrderRequest request) =>
        {
            var order = CreateTransferOrders()
                .FirstOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            return order is null
                ? Results.NotFound()
                : Results.Ok(order with
                {
                    Status = "canceled",
                    CompletedByPersonId = request.CanceledByPersonId,
                    CompletedAtUtc = DateTimeOffset.UtcNow.ToString("O")
                });
        })
        .WithName("CancelLoadArrTransferOrder");

        var holds = app.MapGroup("/api/v1/holds")
            .WithTags("Holds")
            .RequireAuthorization();

        holds.MapGet("/", (string? status, string? holdType) =>
        {
            var records = CreateInventoryHolds()
                .Where(hold => status is null
                    || string.Equals(hold.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(hold => holdType is null
                    || string.Equals(hold.HoldType, holdType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(hold => hold.CreatedAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrInventoryHoldResponse>(records, records.Length));
        })
        .WithName("ListLoadArrInventoryHolds");

        holds.MapGet("/{id}", (string id) =>
        {
            var hold = CreateInventoryHolds()
                .SingleOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            return hold is null ? Results.NotFound() : Results.Ok(hold);
        })
        .WithName("GetLoadArrInventoryHold");

        holds.MapPost("/", (CreateLoadArrInventoryHoldRequest request) =>
        {
            var validation = ValidateHoldRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            var location = ResolveLocation(request.WarehouseLocationId)!;
            var itemSnapshot = ResolveSupplyArrItemReference(request.SupplyarrItemId)!;
            var sourceBalance = ResolveInventoryBalance(request.SupplyarrItemId, request.WarehouseLocationId);

            if (sourceBalance is null || sourceBalance.QuantityOnHand - sourceBalance.QuantityReserved - sourceBalance.QuantityAllocated < request.Quantity)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "insufficient_available_quantity",
                    "Hold quantity must be available at the StaffArr-owned location used by LoadArr."));
            }

            var hold = new LoadArrInventoryHoldResponse(
                $"hold-{Guid.NewGuid():N}"[..13],
                $"HLD-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                "open",
                request.HoldType,
                location.StaffarrSiteOrgUnitId,
                location.StaffarrSiteNameSnapshot,
                location.Id,
                location.Name,
                itemSnapshot.SupplyarrItemId,
                itemSnapshot.ItemNameSnapshot,
                sourceBalance.Id,
                request.Quantity,
                itemSnapshot.UnitOfMeasureSnapshot,
                request.ReasonCode,
                request.Description,
                request.CreatedByPersonId,
                null,
                request.ComplianceEvaluationId,
                request.EvidenceSummary,
                DateTimeOffset.UtcNow.ToString("O"),
                null);

            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "hold",
                location.StaffarrSiteOrgUnitId,
                location.Id,
                location.Id,
                itemSnapshot.SupplyarrItemId,
                itemSnapshot.ItemNameSnapshot,
                request.Quantity,
                itemSnapshot.UnitOfMeasureSnapshot,
                sourceBalance.State,
                "blocked",
                "loadarr",
                "inventory_hold",
                hold.Id,
                request.ReasonCode,
                request.CreatedByPersonId,
                null,
                DateTimeOffset.UtcNow.ToString("O"));

            var balance = sourceBalance with
            {
                QuantityBlocked = sourceBalance.QuantityBlocked + request.Quantity,
                TraceTags = sourceBalance.TraceTags.Concat(new[] { $"hold:{hold.Id}", $"movement:{movement.Id}" }).ToArray(),
                Notes = $"Held {request.Quantity} {sourceBalance.UnitOfMeasureSnapshot}: {request.ReasonCode}"
            };

            return Results.Created($"/api/v1/holds/{hold.Id}", new LoadArrHoldMutationResponse(
                hold,
                movement,
                balance));
        })
        .WithName("CreateLoadArrInventoryHold");

        holds.MapPost("/{id}/release", (string id, ReleaseLoadArrInventoryHoldRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.ReasonCode))
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "missing_reason_code",
                    "Hold release requires a controlled reason code."));
            }

            var existingHold = CreateInventoryHolds()
                .FirstOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            if (existingHold is null)
            {
                return Results.NotFound();
            }

            var location = ResolveLocation(existingHold.WarehouseLocationId)!;
            var balance = ResolveInventoryBalance(existingHold.SupplyarrItemId, existingHold.WarehouseLocationId)
                ?? new LoadArrInventoryBalanceResponse(
                    $"bal-{Guid.NewGuid():N}"[..12],
                    existingHold.SupplyarrItemId,
                    existingHold.ItemNameSnapshot,
                    existingHold.UnitOfMeasure,
                    "available",
                    existingHold.WarehouseLocationId,
                    existingHold.LocationNameSnapshot,
                    existingHold.Quantity,
                    0,
                    0,
                    existingHold.Quantity,
                    "purchase_receipt",
                    existingHold.HoldNumber,
                    new[] { $"hold:{existingHold.Id}" },
                    "Balance snapshot for hold release");

            var releasedHold = existingHold with
            {
                Status = "released",
                ReleasedByPersonId = request.ReleasedByPersonId,
                ReleasedAtUtc = DateTimeOffset.UtcNow.ToString("O")
            };

            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "release_hold",
                location.StaffarrSiteOrgUnitId,
                location.Id,
                location.Id,
                existingHold.SupplyarrItemId,
                existingHold.ItemNameSnapshot,
                existingHold.Quantity,
                existingHold.UnitOfMeasure,
                "blocked",
                "available",
                "loadarr",
                "inventory_hold",
                releasedHold.Id,
                request.ReasonCode,
                request.ReleasedByPersonId,
                null,
                DateTimeOffset.UtcNow.ToString("O"));

            var releasedBalance = balance with
            {
                QuantityBlocked = Math.Max(0, balance.QuantityBlocked - existingHold.Quantity),
                TraceTags = balance.TraceTags.Concat(new[] { $"released-hold:{releasedHold.Id}", $"movement:{movement.Id}" }).ToArray(),
                Notes = $"Released hold {releasedHold.HoldNumber}: {request.ReasonCode}"
            };

            return Results.Ok(new LoadArrHoldMutationResponse(
                releasedHold,
                movement,
                releasedBalance));
        })
        .WithName("ReleaseLoadArrInventoryHold");

        var unexplainedInventory = app.MapGroup("/api/v1/unexplained-inventory")
            .WithTags("Unexplained Inventory")
            .RequireAuthorization();

        unexplainedInventory.MapGet("/", (string? status, string? locationId) =>
        {
            var records = CreateUnexplainedInventoryRecords()
                .Where(record => status is null
                    || string.Equals(record.Status, status, StringComparison.OrdinalIgnoreCase))
                .Where(record => locationId is null
                    || string.Equals(record.WarehouseLocationId, locationId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(record => record.DiscoveredAtUtc, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return Results.Ok(new LoadArrListResponse<LoadArrUnexplainedInventoryRecordResponse>(records, records.Length));
        })
        .WithName("ListLoadArrUnexplainedInventory");

        unexplainedInventory.MapGet("/{id}", (string id) =>
        {
            var record = CreateUnexplainedInventoryRecords()
                .SingleOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));

            return record is null ? Results.NotFound() : Results.Ok(record);
        })
        .WithName("GetLoadArrUnexplainedInventory");

        unexplainedInventory.MapPost("/", (CreateLoadArrUnexplainedInventoryRequest request) =>
        {
            var validation = ValidateUnexplainedInventoryRequest(request);
            if (validation is not null)
            {
                return validation;
            }

            var location = ResolveLocation(request.WarehouseLocationId)!;
            var itemSnapshot = ResolveSupplyArrItemReference(request.SupplyarrItemId)!;
            var status = request.Quantity > request.ExpectedQuantity ? "needs_approval" : "needs_review";
            var variance = request.Quantity - request.ExpectedQuantity;

            var record = new LoadArrUnexplainedInventoryRecordResponse(
                $"unexplained-{Guid.NewGuid():N}"[..20],
                $"UNX-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                status,
                request.DiscoverySource,
                location.StaffarrSiteOrgUnitId,
                location.StaffarrSiteNameSnapshot,
                location.Id,
                location.Name,
                itemSnapshot.SupplyarrItemId,
                itemSnapshot.ItemNameSnapshot,
                request.ExpectedQuantity,
                request.Quantity,
                variance,
                itemSnapshot.UnitOfMeasureSnapshot,
                request.LotCode,
                request.SerialCode,
                request.DiscoveredByPersonId,
                request.ReasonCode,
                request.EvidenceSummary,
                request.ComplianceEvaluationId,
                "not_trusted_available",
                DateTimeOffset.UtcNow.ToString("O"),
                null);

            var task = new LoadArrWarehouseTaskResponse(
                $"task-{Guid.NewGuid():N}"[..13],
                "unexplained_inventory_review",
                $"Resolve unexplained {itemSnapshot.ItemNameSnapshot}",
                status is "needs_approval" ? "urgent" : "high",
                "ready",
                location.Name,
                "Inventory Supervisor",
                itemSnapshot.SupplyarrItemId,
                request.Quantity,
                DateTimeOffset.UtcNow.AddHours(2).ToString("O"),
                new[] { "approval_required", "origin_unknown", "stock_not_available" });

            return Results.Created($"/api/v1/unexplained-inventory/{record.Id}", new LoadArrUnexplainedInventoryMutationResponse(
                record,
                null,
                null,
                task));
        })
        .WithName("CreateLoadArrUnexplainedInventory");

        unexplainedInventory.MapPost("/{id}/resolve", (string id, ResolveLoadArrUnexplainedInventoryRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.ReasonCode))
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "missing_reason_code",
                    "Resolving unexplained inventory requires a controlled reason code."));
            }

            if (string.IsNullOrWhiteSpace(request.ApprovedByPersonId))
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "missing_approval_person",
                    "Resolving unexplained inventory as stock requires approval."));
            }

            var record = ResolveUnexplainedInventoryRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var resolved = record with
            {
                Status = "resolved_valid_stock",
                ResolutionState = "trusted_available",
                ResolvedAtUtc = DateTimeOffset.UtcNow.ToString("O")
            };

            var origin = new LoadArrInventoryOriginEventResponse(
                $"origin-{Guid.NewGuid():N}"[..15],
                "unexplained_inventory_resolution",
                "loadarr",
                "unexplained_inventory",
                resolved.Id,
                resolved.StaffarrSiteOrgUnitId,
                resolved.StaffarrSiteNameSnapshot,
                resolved.WarehouseLocationId,
                resolved.LocationNameSnapshot,
                resolved.SupplyarrItemId,
                resolved.ItemNameSnapshot,
                resolved.Quantity,
                resolved.UnitOfMeasure,
                resolved.LotCode,
                resolved.SerialCode,
                "available",
                "approved",
                request.ApprovedByPersonId,
                request.ComplianceEvaluationId,
                request.EvidenceSummary,
                DateTimeOffset.UtcNow.ToString("O"));

            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "adjust",
                resolved.StaffarrSiteOrgUnitId,
                null,
                resolved.WarehouseLocationId,
                resolved.SupplyarrItemId,
                resolved.ItemNameSnapshot,
                resolved.Quantity,
                resolved.UnitOfMeasure,
                "untrusted",
                "available",
                "loadarr",
                "unexplained_inventory",
                resolved.Id,
                request.ReasonCode,
                request.ApprovedByPersonId,
                origin.Id,
                DateTimeOffset.UtcNow.ToString("O"));

            return Results.Ok(new LoadArrUnexplainedInventoryMutationResponse(
                resolved,
                origin,
                movement,
                null));
        })
        .WithName("ResolveLoadArrUnexplainedInventory");

        unexplainedInventory.MapPost("/{id}/quarantine", (string id, QuarantineLoadArrUnexplainedInventoryRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.ReasonCode))
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "missing_reason_code",
                    "Quarantining unexplained inventory requires a controlled reason code."));
            }

            var record = ResolveUnexplainedInventoryRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var quarantineLocation = ResolveLocation(request.QuarantineLocationId);
            if (quarantineLocation is null)
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "invalid_quarantine_location",
                    "Unexplained inventory quarantine requires a valid StaffArr-owned quarantine location."));
            }

            var quarantined = record with
            {
                Status = "needs_quarantine",
                WarehouseLocationId = quarantineLocation.Id,
                LocationNameSnapshot = quarantineLocation.Name,
                ResolutionState = "quarantined_untrusted"
            };

            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "quarantine",
                quarantineLocation.StaffarrSiteOrgUnitId,
                record.WarehouseLocationId,
                quarantineLocation.Id,
                record.SupplyarrItemId,
                record.ItemNameSnapshot,
                record.Quantity,
                record.UnitOfMeasure,
                "untrusted",
                "quarantined_untrusted",
                "loadarr",
                "unexplained_inventory",
                record.Id,
                request.ReasonCode,
                request.QuarantinedByPersonId,
                null,
                DateTimeOffset.UtcNow.ToString("O"));

            return Results.Ok(new LoadArrUnexplainedInventoryMutationResponse(
                quarantined,
                null,
                movement,
                null));
        })
        .WithName("QuarantineLoadArrUnexplainedInventory");

        unexplainedInventory.MapPost("/{id}/scrap", (string id, ScrapLoadArrUnexplainedInventoryRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.ReasonCode))
            {
                return Results.BadRequest(new LoadArrProblemResponse(
                    "missing_reason_code",
                    "Scrapping unexplained inventory requires a controlled reason code."));
            }

            var record = ResolveUnexplainedInventoryRecord(id);
            if (record is null)
            {
                return Results.NotFound();
            }

            var scrapped = record with
            {
                Status = "resolved_scrap",
                ResolutionState = "scrapped",
                ResolvedAtUtc = DateTimeOffset.UtcNow.ToString("O")
            };

            var movement = new LoadArrInventoryMovementResponse(
                $"move-{Guid.NewGuid():N}"[..13],
                "scrap",
                scrapped.StaffarrSiteOrgUnitId,
                scrapped.WarehouseLocationId,
                scrapped.WarehouseLocationId,
                scrapped.SupplyarrItemId,
                scrapped.ItemNameSnapshot,
                scrapped.Quantity,
                scrapped.UnitOfMeasure,
                "untrusted",
                "scrapped",
                "loadarr",
                "unexplained_inventory",
                scrapped.Id,
                request.ReasonCode,
                request.ScrappedByPersonId,
                null,
                DateTimeOffset.UtcNow.ToString("O"));

            return Results.Ok(new LoadArrUnexplainedInventoryMutationResponse(
                scrapped,
                null,
                movement,
                null));
        })
        .WithName("ScrapLoadArrUnexplainedInventory");
    }

    private static bool InventoryMatchesQuery(LoadArrInventoryBalanceResponse item, string query)
    {
        var normalizedQuery = query.Trim();
        if (normalizedQuery.Length == 0)
        {
            return true;
        }

        return Contains(item.SupplyarrItemId)
            || Contains(item.ItemNameSnapshot)
            || Contains(item.LocationNameSnapshot)
            || Contains(item.OriginReference)
            || item.TraceTags.Any(Contains);

        bool Contains(string value) =>
            value.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase);
    }

    private static bool SupplyArrItemReferenceMatchesQuery(
        LoadArrSupplyArrItemReferenceResponse item,
        string query)
    {
        var normalizedQuery = query.Trim();
        if (normalizedQuery.Length == 0)
        {
            return true;
        }

        return item.SupplyarrItemId.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
            || item.ItemNumberSnapshot.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
            || item.ItemNameSnapshot.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
            || item.ItemTypeSnapshot.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase);
    }

    private static LoadArrSupplyArrItemReferenceResponse? ResolveSupplyArrItemReference(string supplyarrItemId) =>
        CreateSupplyArrItemReferences()
            .FirstOrDefault(item => string.Equals(item.SupplyarrItemId, supplyarrItemId, StringComparison.OrdinalIgnoreCase));

    private static LoadArrLocationResponse? ResolveLocation(string locationId) =>
        CreateWorkspaceSummary().Locations
            .FirstOrDefault(location => string.Equals(location.Id, locationId, StringComparison.OrdinalIgnoreCase));

    private static LoadArrInventoryBalanceResponse? ResolveInventoryBalance(
        string supplyarrItemId,
        string locationId) =>
        CreateWorkspaceSummary().Inventory
            .FirstOrDefault(balance =>
                string.Equals(balance.SupplyarrItemId, supplyarrItemId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(balance.LocationId, locationId, StringComparison.OrdinalIgnoreCase));

    private static LoadArrUnexplainedInventoryRecordResponse? ResolveUnexplainedInventoryRecord(string id) =>
        CreateUnexplainedInventoryRecords()
            .FirstOrDefault(record => string.Equals(record.Id, id, StringComparison.OrdinalIgnoreCase));

    private static IResult? ValidateTransferRequest(ILoadArrTransferMutationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Transfers require a controlled reason code."));
        }

        if (request.Quantity <= 0)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_transfer_quantity",
                "Transfer quantity must be greater than zero."));
        }

        var fromLocation = ResolveLocation(request.FromLocationId);
        var toLocation = ResolveLocation(request.ToLocationId);

        if (fromLocation is null || toLocation is null)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_transfer_location",
                "Transfers require valid StaffArr-owned source and destination locations."));
        }

        if (string.Equals(fromLocation.Id, toLocation.Id, StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "same_transfer_location",
                "Transfer source and destination must be different StaffArr locations."));
        }

        if (!string.Equals(request.TransferType, "site_to_site", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(fromLocation.StaffarrSiteOrgUnitId, toLocation.StaffarrSiteOrgUnitId, StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "site_transfer_type_required",
                "Transfers across StaffArr sites must use the site_to_site transfer type."));
        }

        if (ResolveSupplyArrItemReference(request.SupplyarrItemId) is null)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_supplyarr_item_reference",
                "Transfers require an item reference that exists in SupplyArr."));
        }

        return null;
    }

    private static IResult? ValidateHoldRequest(CreateLoadArrInventoryHoldRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Inventory holds require a controlled reason code."));
        }

        if (request.Quantity <= 0)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_hold_quantity",
                "Hold quantity must be greater than zero."));
        }

        if (ResolveLocation(request.WarehouseLocationId) is null)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_hold_location",
                "Inventory holds require a valid StaffArr-owned location reference."));
        }

        if (ResolveSupplyArrItemReference(request.SupplyarrItemId) is null)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_supplyarr_item_reference",
                "Inventory holds require an item reference that exists in SupplyArr."));
        }

        return null;
    }

    private static IResult? ValidateUnexplainedInventoryRequest(CreateLoadArrUnexplainedInventoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReasonCode))
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_reason_code",
                "Unexplained inventory requires a controlled reason code."));
        }

        if (request.Quantity <= 0)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_unexplained_quantity",
                "Unexplained inventory quantity must be greater than zero."));
        }

        if (request.Quantity == request.ExpectedQuantity)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "missing_variance",
                "Unexplained inventory requires a variance from expected quantity."));
        }

        if (ResolveLocation(request.WarehouseLocationId) is null)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_unexplained_location",
                "Unexplained inventory requires a valid StaffArr-owned location reference."));
        }

        if (ResolveSupplyArrItemReference(request.SupplyarrItemId) is null)
        {
            return Results.BadRequest(new LoadArrProblemResponse(
                "invalid_supplyarr_item_reference",
                "Unexplained inventory requires an item reference that exists in SupplyArr."));
        }

        return null;
    }

    private static LoadArrWorkspaceSummaryResponse CreateWorkspaceSummary()
    {
        var generatedAt = DateTimeOffset.UtcNow;

        var locations = new[]
        {
            new LoadArrLocationResponse(
                "loc-dock-01",
                "STL North Yard",
                "staff-site-stl-north",
                "Receiving Dock 1",
                "dock",
                "STL North Yard / Main Warehouse / Dock 1",
                true,
                new[] { "ambient", "forklift" },
                78,
                "Open for receipts and outbound staging"),
            new LoadArrLocationResponse(
                "loc-haz-01",
                "STL North Yard",
                "staff-site-stl-north",
                "Hazmat Cage A",
                "hazmat_cage",
                "STL North Yard / Secure Storage / Hazmat Cage A",
                true,
                new[] { "hazmat", "controlled_access", "inspection_required" },
                63,
                "Authorized staff and current hazmat training required"),
            new LoadArrLocationResponse(
                "loc-quarantine-01",
                "STL North Yard",
                "staff-site-stl-north",
                "Quarantine Bay",
                "quarantine_area",
                "STL North Yard / Quality / Quarantine Bay",
                true,
                new[] { "quality_hold", "blocked" },
                41,
                "Blocked from allocation until investigation closes"),
            new LoadArrLocationResponse(
                "loc-truck-17",
                "South Service Depot",
                "staff-site-south-depot",
                "Truck Stock 17",
                "service_truck",
                "South Service Depot / Mobile Stock / Truck 17",
                true,
                new[] { "mobile_stock", "route_ready" },
                55,
                "Assigned to field maintenance route handoff")
        };

        var inventory = new[]
        {
            new LoadArrInventoryBalanceResponse(
                "bal-valve-kit-a",
                "SUP-VALVE-KIT-A",
                "Valve repair kit A",
                "each",
                "available",
                "loc-dock-01",
                "Receiving Dock 1",
                38,
                11,
                4,
                0,
                "purchase_receipt",
                "PO-10492 / RR-24018",
                new[] { "lot:L2405-77", "supplyarr:part:SUP-VALVE-KIT-A" },
                "Ready for putaway"),
            new LoadArrInventoryBalanceResponse(
                "bal-adhesive-haz",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                "case",
                "pending_inspection",
                "loc-haz-01",
                "Hazmat Cage A",
                14,
                0,
                0,
                14,
                "vendor_consignment_receipt",
                "ASN-8834",
                new[] { "hazmat", "sds:current", "lot:ADH-991" },
                "SDS and label check required"),
            new LoadArrInventoryBalanceResponse(
                "bal-brake-rotor",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                "each",
                "reserved",
                "loc-truck-17",
                "Truck Stock 17",
                12,
                2,
                6,
                0,
                "route_return",
                "RoutArr trip RT-7781",
                new[] { "maintainarr:work-order:WO-5530", "truck_stock" },
                "Reserved for maintenance work orders")
        };

        var tasks = new[]
        {
            new LoadArrWarehouseTaskResponse(
                "task-receive-24018",
                "receive",
                "Receive PO-10492",
                "high",
                "ready",
                "Receiving Dock 1",
                "Inventory Clerk",
                "SUP-VALVE-KIT-A",
                38,
                "2026-06-03T15:00:00Z",
                new[] { "purchase_receipt", "packing_slip_attached" }),
            new LoadArrWarehouseTaskResponse(
                "task-inspect-adh-49",
                "quality_inspection",
                "Inspect regulated adhesive lot",
                "urgent",
                "blocked_by_training",
                "Hazmat Cage A",
                "Hazmat-qualified reviewer",
                "SUP-ADH-49",
                14,
                "2026-06-03T18:00:00Z",
                new[] { "hazmat", "trainarr_required", "compliancecore_sds_check" }),
            new LoadArrWarehouseTaskResponse(
                "task-pick-wo-5530",
                "pick",
                "Pick parts for WO-5530",
                "normal",
                "in_progress",
                "Truck Stock 17",
                "Route Stock Lead",
                "SUP-BR-ROTOR-22",
                6,
                "2026-06-04T13:30:00Z",
                new[] { "maintainarr", "route_ready" })
        };

        var holds = new[]
        {
            new LoadArrHoldResponse(
                "hold-adh-49",
                "quality_hold",
                "SUP-ADH-49",
                "Hazmat Cage A",
                "Open",
                "SDS label mismatch requires Compliance Core review",
                "ComplianceCore rule title49.hazmat.labeling",
                "2026-06-02T21:10:00Z"),
            new LoadArrHoldResponse(
                "hold-count-rotor",
                "investigation",
                "SUP-BR-ROTOR-22",
                "Truck Stock 17",
                "Review",
                "Cycle count variance above mobile-stock threshold",
                "Count CC-8021",
                "2026-06-02T19:40:00Z")
        };

        var handoffs = new[]
        {
            new LoadArrRouteHandoffResponse(
                "handoff-rt-7781",
                "RoutArr",
                "RT-7781",
                "Truck Stock 17",
                "ready",
                6,
                "WO-5530 parts staged for mobile maintenance route"),
            new LoadArrRouteHandoffResponse(
                "handoff-out-1204",
                "SupplyArr",
                "OUT-1204",
                "Receiving Dock 1",
                "waiting_on_pick",
                11,
                "Outbound stock movement pending pick confirmation")
        };

        var evidence = new[]
        {
            new LoadArrEvidenceResponse(
                "ev-rr-24018-photo",
                "photo",
                "Receiving Dock 1",
                "Dock receipt photo attached to RR-24018",
                "2026-06-02T20:16:00Z",
                "Inventory Clerk"),
            new LoadArrEvidenceResponse(
                "ev-adh-sds-check",
                "rule_evaluation",
                "Hazmat Cage A",
                "SDS and label rule check opened",
                "2026-06-02T21:12:00Z",
                "ComplianceCore"),
            new LoadArrEvidenceResponse(
                "ev-count-8021",
                "cycle_count",
                "Truck Stock 17",
                "Mobile stock variance captured for review",
                "2026-06-02T19:42:00Z",
                "Route Stock Lead")
        };

        var unexplainedInventory = CreateUnexplainedInventoryRecords();

        return new LoadArrWorkspaceSummaryResponse(
            generatedAt,
            new LoadArrWorkspaceMetricsResponse(
                locations.Length,
                inventory.Sum(item => item.QuantityOnHand),
                inventory.Sum(item => item.QuantityReserved + item.QuantityAllocated),
                inventory.Sum(item => item.QuantityBlocked),
                tasks.Count(task => task.Status is "ready" or "in_progress"),
                holds.Count(hold => hold.Status is "Open" or "Review"),
                unexplainedInventory.Count(record => record.Status is "needs_review" or "needs_approval" or "needs_quarantine")),
            locations,
            inventory,
            tasks,
            holds,
            handoffs,
            evidence,
            unexplainedInventory);
    }

    private static IReadOnlyCollection<LoadArrReceivingSessionResponse> CreateReceivingSessions() =>
        new[]
        {
            new LoadArrReceivingSessionResponse(
                "recv-24018",
                "RCV-24018",
                "purchase_order",
                "open",
                "staff-site-stl-north",
                "STL North Yard",
                "supplyarr",
                "purchase_order",
                "PO-10492",
                "Midwest Fleet Supply",
                "person-inventory-clerk",
                null,
                "2026-06-02T20:10:00Z",
                null,
                new[]
                {
                    new LoadArrReceivingLineResponse(
                        "line-24018-1",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        38,
                        38,
                        "each",
                        "loc-dock-01",
                        "Receiving Dock 1",
                        "L2405-77",
                        null,
                        "new",
                        "ready_to_complete",
                        null,
                        "Dock receipt photo attached")
                }),
            new LoadArrReceivingSessionResponse(
                "recv-8834",
                "RCV-8834",
                "vendor_consignment",
                "inspection_required",
                "staff-site-stl-north",
                "STL North Yard",
                "supplyarr",
                "asn",
                "ASN-8834",
                "Applied Chemical Partners",
                "person-hazmat-reviewer",
                null,
                "2026-06-02T21:05:00Z",
                null,
                new[]
                {
                    new LoadArrReceivingLineResponse(
                        "line-8834-1",
                        "SUP-ADH-49",
                        "Regulated adhesive cartridge",
                        14,
                        14,
                        "case",
                        "loc-haz-01",
                        "Hazmat Cage A",
                        "ADH-991",
                        null,
                        "pending_inspection",
                        "blocked_by_compliance",
                        "label_mismatch",
                        "SDS and label check opened")
                })
        };

    private static LoadArrReceivingSessionResponse CreateReceivingSessionFromCompletion(
        string id,
        CompleteLoadArrReceivingSessionRequest request)
    {
        var location = CreateWorkspaceSummary().Locations
            .FirstOrDefault(candidate => string.Equals(candidate.Id, request.WarehouseLocationId, StringComparison.OrdinalIgnoreCase))
            ?? CreateWorkspaceSummary().Locations.First();
        var itemSnapshot = ResolveSupplyArrItemReference(request.SupplyarrItemId)
            ?? CreateSupplyArrItemReferences().First();

        return new LoadArrReceivingSessionResponse(
            id,
            $"RCV-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
            string.IsNullOrWhiteSpace(request.ReceivingType) ? "manual" : request.ReceivingType,
            "open",
            location.StaffarrSiteOrgUnitId,
            location.StaffarrSiteNameSnapshot,
            request.SourceProductKey,
            request.SourceObjectType,
            request.SourceObjectId,
            request.SupplierNameSnapshot,
            request.CompletedByPersonId,
            null,
            DateTimeOffset.UtcNow.ToString("O"),
            null,
            new[]
            {
                new LoadArrReceivingLineResponse(
                    $"line-{Guid.NewGuid():N}"[..13],
                    itemSnapshot.SupplyarrItemId,
                    itemSnapshot.ItemNameSnapshot,
                    request.ExpectedQuantity,
                    request.ReceivedQuantity,
                    itemSnapshot.UnitOfMeasureSnapshot,
                    location.Id,
                    location.Name,
                    request.LotCode,
                    request.SerialCode,
                    request.Condition,
                    "ready_to_complete",
                    request.DiscrepancyReasonCode,
                    request.EvidenceSummary)
            });
    }

    private static IReadOnlyCollection<LoadArrTransferOrderResponse> CreateTransferOrders() =>
        new[]
        {
            new LoadArrTransferOrderResponse(
                "xfer-24018-putaway",
                "TRF-24018",
                "ready",
                "bin_to_bin",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "loc-quarantine-01",
                "Quarantine Bay",
                "person-inventory-clerk",
                null,
                "quality_inspection",
                "2026-06-03T14:15:00Z",
                null,
                new[]
                {
                    new LoadArrTransferLineResponse(
                        "xfer-line-24018",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        4,
                        "each",
                        "L2405-77",
                        null,
                        "ready")
                }),
            new LoadArrTransferOrderResponse(
                "xfer-truck-17",
                "TRF-TRUCK-17",
                "completed",
                "warehouse_to_truck",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "loc-dock-01",
                "Receiving Dock 1",
                "person-route-stock-lead",
                "person-route-stock-lead",
                "route_replenishment",
                "2026-06-02T18:00:00Z",
                "2026-06-02T18:35:00Z",
                new[]
                {
                    new LoadArrTransferLineResponse(
                        "xfer-line-truck-17",
                        "SUP-BR-ROTOR-22",
                        "Brake rotor assembly",
                        2,
                        "each",
                        null,
                        "BR-SN-7781",
                        "completed")
                })
        };

    private static IReadOnlyCollection<LoadArrInventoryHoldResponse> CreateInventoryHolds() =>
        new[]
        {
            new LoadArrInventoryHoldResponse(
                "hold-adh-49",
                "HLD-ADH-49",
                "open",
                "quality",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-haz-01",
                "Hazmat Cage A",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                "bal-adhesive-haz",
                14,
                "case",
                "sds_label_mismatch",
                "SDS label mismatch requires Compliance Core review",
                "person-hazmat-reviewer",
                null,
                "cc-eval-adh-49",
                "SDS and label rule check opened",
                "2026-06-02T21:10:00Z",
                null),
            new LoadArrInventoryHoldResponse(
                "hold-count-rotor",
                "HLD-ROTOR-22",
                "review",
                "investigation",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                "bal-brake-rotor",
                2,
                "each",
                "cycle_count_variance",
                "Cycle count variance above mobile-stock threshold",
                "person-route-stock-lead",
                null,
                null,
                "Mobile stock variance captured for review",
                "2026-06-02T19:40:00Z",
                null)
        };

    private static IReadOnlyCollection<LoadArrUnexplainedInventoryRecordResponse> CreateUnexplainedInventoryRecords() =>
        new[]
        {
            new LoadArrUnexplainedInventoryRecordResponse(
                "unexplained-count-8021",
                "UNX-8021",
                "needs_approval",
                "cycle_count_variance",
                "staff-site-south-depot",
                "South Service Depot",
                "loc-truck-17",
                "Truck Stock 17",
                "SUP-BR-ROTOR-22",
                "Brake rotor assembly",
                10,
                12,
                2,
                "each",
                null,
                "BR-SN-7781",
                "person-route-stock-lead",
                "cycle_count_variance",
                "Positive mobile-stock variance captured; stock is not trusted available until supervisor approval.",
                null,
                "not_trusted_available",
                "2026-06-02T19:45:00Z",
                null),
            new LoadArrUnexplainedInventoryRecordResponse(
                "unexplained-dock-adh",
                "UNX-ADH-49",
                "needs_quarantine",
                "damaged_freight_receipt",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-quarantine-01",
                "Quarantine Bay",
                "SUP-ADH-49",
                "Regulated adhesive cartridge",
                0,
                1,
                1,
                "case",
                "ADH-991",
                null,
                "person-inventory-clerk",
                "unknown_origin_review",
                "One extra case found beside freight paperwork; moved to StaffArr quarantine location for review.",
                "cc-eval-adh-extra",
                "quarantined_untrusted",
                "2026-06-02T20:50:00Z",
                null)
        };

    private static IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse> CreateSupplyArrItemReferences() =>
        new[]
        {
            new LoadArrSupplyArrItemReferenceResponse(
                "SUP-VALVE-KIT-A",
                "VALVE-KIT-A",
                "Valve repair kit A",
                "each",
                "maintenance_part",
                true,
                false,
                false,
                false,
                "2026-06-01T12:00:00Z"),
            new LoadArrSupplyArrItemReferenceResponse(
                "SUP-ADH-49",
                "ADH-49",
                "Regulated adhesive cartridge",
                "case",
                "regulated_consumable",
                true,
                false,
                true,
                true,
                "2026-06-01T12:00:00Z"),
            new LoadArrSupplyArrItemReferenceResponse(
                "SUP-BR-ROTOR-22",
                "BR-ROTOR-22",
                "Brake rotor assembly",
                "each",
                "maintenance_part",
                false,
                true,
                false,
                false,
                "2026-06-01T12:00:00Z")
        };
}

public sealed record LoadArrWorkspaceSummaryResponse(
    DateTimeOffset GeneratedAt,
    LoadArrWorkspaceMetricsResponse Metrics,
    IReadOnlyCollection<LoadArrLocationResponse> Locations,
    IReadOnlyCollection<LoadArrInventoryBalanceResponse> Inventory,
    IReadOnlyCollection<LoadArrWarehouseTaskResponse> Tasks,
    IReadOnlyCollection<LoadArrHoldResponse> Holds,
    IReadOnlyCollection<LoadArrRouteHandoffResponse> RouteHandoffs,
    IReadOnlyCollection<LoadArrEvidenceResponse> Evidence,
    IReadOnlyCollection<LoadArrUnexplainedInventoryRecordResponse> UnexplainedInventory);

public sealed record LoadArrListResponse<TItem>(
    IReadOnlyCollection<TItem> Items,
    int Total);

public sealed record LoadArrLocationTreeNodeResponse(
    string Id,
    string Label,
    string NodeType,
    string? LocationId,
    IReadOnlyCollection<LoadArrLocationTreeNodeResponse> Children);

public sealed record LoadArrSupplyArrItemReferenceResponse(
    string SupplyarrItemId,
    string ItemNumberSnapshot,
    string ItemNameSnapshot,
    string UnitOfMeasureSnapshot,
    string ItemTypeSnapshot,
    bool IsLotControlled,
    bool IsSerialControlled,
    bool IsHazardous,
    bool RequiresSds,
    string UpdatedAtUtc);

public sealed record LoadArrWorkspaceMetricsResponse(
    int ActiveLocations,
    decimal QuantityOnHand,
    decimal QuantityCommitted,
    decimal QuantityBlocked,
    int OpenTasks,
    int OpenHolds,
    int UnexplainedInventory);

public sealed record LoadArrLocationResponse(
    string Id,
    string StaffarrSiteNameSnapshot,
    string StaffarrSiteOrgUnitId,
    string Name,
    string LocationType,
    string Path,
    bool Active,
    IReadOnlyCollection<string> ComplianceRestrictions,
    int CapacityPercent,
    string Notes);

public sealed record LoadArrInventoryBalanceResponse(
    string Id,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    string UnitOfMeasureSnapshot,
    string State,
    string LocationId,
    string LocationNameSnapshot,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAllocated,
    decimal QuantityBlocked,
    string OriginEventType,
    string OriginReference,
    IReadOnlyCollection<string> TraceTags,
    string Notes);

public sealed record LoadArrWarehouseTaskResponse(
    string Id,
    string TaskType,
    string Title,
    string Priority,
    string Status,
    string LocationNameSnapshot,
    string AssignedRole,
    string SupplyarrItemId,
    decimal Quantity,
    string DueAtUtc,
    IReadOnlyCollection<string> RequiredSignals);

public sealed record LoadArrReceivingSessionResponse(
    string Id,
    string ReceivingNumber,
    string ReceivingType,
    string Status,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string StartedByPersonId,
    string? CompletedByPersonId,
    string StartedAtUtc,
    string? CompletedAtUtc,
    IReadOnlyCollection<LoadArrReceivingLineResponse> Lines);

public sealed record LoadArrReceivingLineResponse(
    string Id,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string UnitOfMeasure,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string Status,
    string? DiscrepancyReasonCode,
    string? EvidenceSummary);

public sealed record CreateLoadArrReceivingSessionRequest(
    string ReceivingType,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string StartedByPersonId,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string WarehouseLocationId,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string? DiscrepancyReasonCode,
    string? EvidenceSummary);

public sealed record AddLoadArrReceivingLineRequest(
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string WarehouseLocationId,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string? DiscrepancyReasonCode,
    string? EvidenceSummary);

public sealed record CompleteLoadArrReceivingSessionRequest(
    string ReceivingType,
    string SourceProductKey,
    string SourceObjectType,
    string SourceObjectId,
    string SupplierNameSnapshot,
    string CompletedByPersonId,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal ReceivedQuantity,
    string WarehouseLocationId,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string? DiscrepancyReasonCode,
    string? ComplianceEvaluationId,
    string? EvidenceSummary);

public sealed record CancelLoadArrReceivingSessionRequest(
    string CanceledByPersonId,
    string ReasonCode,
    string? Notes);

public interface ILoadArrTransferMutationRequest
{
    string TransferType { get; }
    string FromLocationId { get; }
    string ToLocationId { get; }
    string SupplyarrItemId { get; }
    decimal Quantity { get; }
    string ReasonCode { get; }
}

public sealed record LoadArrTransferOrderResponse(
    string Id,
    string TransferNumber,
    string Status,
    string TransferType,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string FromLocationId,
    string FromLocationNameSnapshot,
    string ToLocationId,
    string ToLocationNameSnapshot,
    string RequestedByPersonId,
    string? CompletedByPersonId,
    string ReasonCode,
    string CreatedAtUtc,
    string? CompletedAtUtc,
    IReadOnlyCollection<LoadArrTransferLineResponse> Lines);

public sealed record LoadArrTransferLineResponse(
    string Id,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string? LotCode,
    string? SerialCode,
    string Status);

public sealed record CreateLoadArrTransferOrderRequest(
    string TransferType,
    string FromLocationId,
    string ToLocationId,
    string RequestedByPersonId,
    string SupplyarrItemId,
    decimal Quantity,
    string? LotCode,
    string? SerialCode,
    string ReasonCode) : ILoadArrTransferMutationRequest;

public sealed record CompleteLoadArrTransferOrderRequest(
    string TransferType,
    string FromLocationId,
    string ToLocationId,
    string CompletedByPersonId,
    string SupplyarrItemId,
    decimal Quantity,
    string? LotCode,
    string? SerialCode,
    string ReasonCode,
    string? ComplianceEvaluationId,
    string? EvidenceSummary) : ILoadArrTransferMutationRequest;

public sealed record CancelLoadArrTransferOrderRequest(
    string CanceledByPersonId,
    string ReasonCode,
    string? Notes);

public sealed record LoadArrTransferCompletionResponse(
    LoadArrTransferOrderResponse Transfer,
    LoadArrInventoryMovementResponse Movement,
    LoadArrInventoryBalanceResponse SourceBalance,
    LoadArrInventoryBalanceResponse DestinationBalance,
    LoadArrWarehouseTaskResponse TransferTask);

public sealed record LoadArrInventoryHoldResponse(
    string Id,
    string HoldNumber,
    string Status,
    string HoldType,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    string InventoryBalanceId,
    decimal Quantity,
    string UnitOfMeasure,
    string ReasonCode,
    string Description,
    string CreatedByPersonId,
    string? ReleasedByPersonId,
    string? ComplianceEvaluationId,
    string? EvidenceSummary,
    string CreatedAtUtc,
    string? ReleasedAtUtc);

public sealed record CreateLoadArrInventoryHoldRequest(
    string HoldType,
    string WarehouseLocationId,
    string SupplyarrItemId,
    decimal Quantity,
    string ReasonCode,
    string Description,
    string CreatedByPersonId,
    string? ComplianceEvaluationId,
    string? EvidenceSummary);

public sealed record ReleaseLoadArrInventoryHoldRequest(
    string ReleasedByPersonId,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record LoadArrHoldMutationResponse(
    LoadArrInventoryHoldResponse Hold,
    LoadArrInventoryMovementResponse Movement,
    LoadArrInventoryBalanceResponse Balance);

public sealed record LoadArrUnexplainedInventoryRecordResponse(
    string Id,
    string RecordNumber,
    string Status,
    string DiscoverySource,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal ExpectedQuantity,
    decimal Quantity,
    decimal VarianceQuantity,
    string UnitOfMeasure,
    string? LotCode,
    string? SerialCode,
    string DiscoveredByPersonId,
    string ReasonCode,
    string EvidenceSummary,
    string? ComplianceEvaluationId,
    string ResolutionState,
    string DiscoveredAtUtc,
    string? ResolvedAtUtc);

public sealed record CreateLoadArrUnexplainedInventoryRequest(
    string DiscoverySource,
    string WarehouseLocationId,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal Quantity,
    string? LotCode,
    string? SerialCode,
    string DiscoveredByPersonId,
    string ReasonCode,
    string EvidenceSummary,
    string? ComplianceEvaluationId);

public sealed record ResolveLoadArrUnexplainedInventoryRequest(
    string ApprovedByPersonId,
    string ReasonCode,
    string? ComplianceEvaluationId,
    string? EvidenceSummary);

public sealed record QuarantineLoadArrUnexplainedInventoryRequest(
    string QuarantineLocationId,
    string QuarantinedByPersonId,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record ScrapLoadArrUnexplainedInventoryRequest(
    string ScrappedByPersonId,
    string ReasonCode,
    string? EvidenceSummary);

public sealed record LoadArrUnexplainedInventoryMutationResponse(
    LoadArrUnexplainedInventoryRecordResponse Record,
    LoadArrInventoryOriginEventResponse? OriginEvent,
    LoadArrInventoryMovementResponse? Movement,
    LoadArrWarehouseTaskResponse? ReviewTask);

public sealed record LoadArrReceivingCompletionResponse(
    LoadArrReceivingSessionResponse Session,
    LoadArrInventoryOriginEventResponse OriginEvent,
    LoadArrInventoryMovementResponse Movement,
    LoadArrInventoryBalanceResponse Balance,
    LoadArrWarehouseTaskResponse PutawayTask);

public sealed record LoadArrInventoryOriginEventResponse(
    string Id,
    string OriginType,
    string OriginProductKey,
    string OriginObjectType,
    string OriginObjectId,
    string StaffarrSiteOrgUnitId,
    string StaffarrSiteNameSnapshot,
    string WarehouseLocationId,
    string LocationNameSnapshot,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string? LotCode,
    string? SerialCode,
    string Condition,
    string Status,
    string PersonId,
    string? ComplianceEvaluationId,
    string? EvidenceSummary,
    string CreatedAtUtc);

public sealed record LoadArrInventoryMovementResponse(
    string Id,
    string MovementType,
    string StaffarrSiteOrgUnitId,
    string? FromLocationId,
    string ToLocationId,
    string SupplyarrItemId,
    string ItemNameSnapshot,
    decimal Quantity,
    string UnitOfMeasure,
    string? StatusBefore,
    string StatusAfter,
    string RelatedProductKey,
    string RelatedObjectType,
    string RelatedObjectId,
    string ReasonCode,
    string PersonId,
    string? InventoryOriginEventId,
    string CreatedAtUtc);

public sealed record LoadArrHoldResponse(
    string Id,
    string HoldType,
    string SupplyarrItemId,
    string LocationNameSnapshot,
    string Status,
    string Reason,
    string SourceReference,
    string OpenedAtUtc);

public sealed record LoadArrRouteHandoffResponse(
    string Id,
    string TargetProduct,
    string TargetReference,
    string LocationNameSnapshot,
    string Status,
    decimal Quantity,
    string Notes);

public sealed record LoadArrEvidenceResponse(
    string Id,
    string EvidenceType,
    string LocationNameSnapshot,
    string Summary,
    string CapturedAtUtc,
    string CapturedBy);

public sealed record LoadArrProblemResponse(
    string ErrorCode,
    string Message);
