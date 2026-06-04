using System;
using System.Text.Json;

namespace LoadArr.Api.Endpoints;

public static class LoadArrIntegrationEndpoints
{
    public static void MapLoadArrIntegrationEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/integrations"), string.Empty, "/api/integrations");
        MapRoutes(app.MapGroup("/api/v1/integrations"), "V1", "/api/v1/integrations");
    }

    private static void MapRoutes(RouteGroupBuilder integrations, string nameSuffix, string routePrefix)
    {
        integrations = integrations.WithTags("Integrations").RequireAuthorization();

        integrations.MapGet("/items", () =>
        {
            var items = CreateIntegrationItems();
            return Results.Ok(new LoadArrListResponse<LoadArrIntegrationItemResponse>(items, items.Length));
        })
        .WithName($"ListLoadArrIntegrationItems{nameSuffix}");

        integrations.MapGet("/items/{itemId}", (string itemId) =>
        {
            var item = ResolveIntegrationItem(itemId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName($"GetLoadArrIntegrationItem{nameSuffix}");

        integrations.MapPost("/items", (JsonElement request) =>
        {
            var createdAt = TimestampUtc();
            var response = new LoadArrIntegrationItemResponse(
                $"item-{Guid.NewGuid():N}"[..14],
                ReadOptionalString(request, "supplyarrItemId") ?? $"supplyarr-{Guid.NewGuid():N}"[..12],
                ReadOptionalString(request, "itemCode") ?? "item-mock-001",
                ReadOptionalString(request, "itemNameSnapshot") ?? "Mock integration item",
                ReadOptionalString(request, "unitOfMeasureSnapshot") ?? "each",
                "active",
                createdAt);

            return Results.Created($"{routePrefix}/items/{response.ItemId}", response);
        })
        .WithName($"CreateLoadArrIntegrationItem{nameSuffix}");

        integrations.MapGet("/location-profiles", () =>
        {
            var profiles = CreateLocationProfiles();
            return Results.Ok(new LoadArrListResponse<LoadArrIntegrationLocationProfileResponse>(profiles, profiles.Length));
        })
        .WithName($"ListLoadArrIntegrationLocationProfiles{nameSuffix}");

        integrations.MapGet("/location-profiles/{wmsLocationProfileId}", (string wmsLocationProfileId) =>
        {
            var profile = ResolveLocationProfile(wmsLocationProfileId);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        })
        .WithName($"GetLoadArrIntegrationLocationProfile{nameSuffix}");

        integrations.MapPost("/location-profiles", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationLocationProfileResponse(
                $"lpf-{Guid.NewGuid():N}"[..14],
                ReadOptionalString(request, "staffarrLocationId") ?? "location-001",
                ReadOptionalString(request, "wmsLocationType") ?? "BIN",
                true,
                TimestampUtc());

            return Results.Created($"{routePrefix}/location-profiles/{response.WmsLocationProfileId}", response);
        })
        .WithName($"CreateLoadArrIntegrationLocationProfile{nameSuffix}");

        integrations.MapGet("/balances", () =>
        {
            var balances = CreateIntegrationBalances();
            return Results.Ok(new LoadArrListResponse<LoadArrIntegrationBalanceResponse>(balances, balances.Length));
        })
        .WithName($"ListLoadArrIntegrationBalances{nameSuffix}");

        integrations.MapGet("/balances/{balanceId}", (string balanceId) =>
        {
            var balance = ResolveIntegrationBalance(balanceId);
            return balance is null ? Results.NotFound() : Results.Ok(balance);
        })
        .WithName($"GetLoadArrIntegrationBalance{nameSuffix}");

        integrations.MapPost("/availability-checks", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationAvailabilityCheckResponse(
                $"check-{Guid.NewGuid():N}"[..15],
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalString(request, "warehouseLocationId") ?? "location-001",
                ReadOptionalDecimal(request, "requestedQuantity") ?? 1m,
                true,
                "availability_ok",
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"CheckLoadArrIntegrationAvailability{nameSuffix}");

        integrations.MapPost("/expected-receipts", (JsonElement request) =>
        {
            var createdAt = TimestampUtc();
            var response = new LoadArrIntegrationExpectedReceiptResponse(
                $"exp-{Guid.NewGuid():N}"[..15],
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalString(request, "supplierName") ?? "Mock Supplier",
                ReadOptionalString(request, "warehouseLocationId") ?? "location-001",
                ReadOptionalDecimal(request, "expectedQuantity") ?? 10m,
                "pending",
                createdAt);
            return Results.Created($"{routePrefix}/expected-receipts/{response.ExpectedReceiptId}", response);
        })
        .WithName($"CreateLoadArrIntegrationExpectedReceipt{nameSuffix}");

        integrations.MapGet("/expected-receipts/{expectedReceiptId}", (string expectedReceiptId) =>
        {
            var receipt = ResolveExpectedReceipt(expectedReceiptId);
            return receipt is null ? Results.NotFound() : Results.Ok(receipt);
        })
        .WithName($"GetLoadArrIntegrationExpectedReceipt{nameSuffix}");

        integrations.MapPost("/expected-receipts/{expectedReceiptId}/status-updates", (string expectedReceiptId, JsonElement request) =>
        {
            var status = ReadOptionalString(request, "status") ?? "updated";
            var existing = ResolveExpectedReceipt(expectedReceiptId);
            var updated = new LoadArrIntegrationExpectedReceiptResponse(
                expectedReceiptId,
                existing?.SupplyarrItemId ?? "supplyarr-item-001",
                existing?.SupplierName ?? "Mock Supplier",
                existing?.WarehouseLocationId ?? "location-001",
                existing?.ExpectedQuantity ?? 10m,
                status,
                TimestampUtc());
            return Results.Ok(updated);
        })
        .WithName($"UpdateLoadArrIntegrationExpectedReceiptStatus{nameSuffix}");

        integrations.MapGet("/stock-movements", () =>
        {
            var movements = CreateStockMovements();
            return Results.Ok(new LoadArrListResponse<LoadArrIntegrationStockMovementResponse>(movements, movements.Length));
        })
        .WithName($"ListLoadArrIntegrationStockMovements{nameSuffix}");

        integrations.MapGet("/stock-movements/{movementId}", (string movementId) =>
        {
            var movement = ResolveStockMovement(movementId);
            return movement is null ? Results.NotFound() : Results.Ok(movement);
        })
        .WithName($"GetLoadArrIntegrationStockMovement{nameSuffix}");

        integrations.MapPost("/receipts", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationReceiptResponse(
                $"rcpt-{Guid.NewGuid():N}"[..15],
                ReadOptionalString(request, "receiptNumber") ?? "RCPT-0001",
                ReadOptionalString(request, "expectedReceiptId"),
                "open",
                "supplier-created",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "expectedQuantity") ?? 1m,
                TimestampUtc());
            return Results.Created($"{routePrefix}/receipts/{response.ReceiptId}", response);
        })
        .WithName($"CreateLoadArrIntegrationReceipt{nameSuffix}");

        integrations.MapGet("/receipts/{receiptId}", (string receiptId) =>
        {
            var receipt = ResolveReceipt(receiptId);
            return receipt is null ? Results.NotFound() : Results.Ok(receipt);
        })
        .WithName($"GetLoadArrIntegrationReceipt{nameSuffix}");

        integrations.MapPost("/receipts/{receiptId}/lines", (string receiptId, JsonElement request) =>
        {
            var line = new LoadArrIntegrationReceiptLineResponse(
                $"line-{Guid.NewGuid():N}"[..14],
                receiptId,
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "receivedQuantity") ?? 1m,
                TimestampUtc());
            return Results.Ok(line);
        })
        .WithName($"AddLoadArrIntegrationReceiptLine{nameSuffix}");

        integrations.MapPost("/receipts/{receiptId}/close", (string receiptId) =>
        {
            var existing = ResolveReceipt(receiptId);
            var response = new LoadArrIntegrationReceiptCloseResponse(
                receiptId,
                existing?.Status == "open" ? "completed" : existing?.Status ?? "completed",
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"CloseLoadArrIntegrationReceipt{nameSuffix}");

        integrations.MapPost("/putaway-tasks", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationTaskResponse(
                $"pt-{Guid.NewGuid():N}"[..13],
                "putaway",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "quantity") ?? 1m,
                "ready",
                ReadOptionalString(request, "locationId") ?? "location-001",
                TimestampUtc());
            return Results.Created($"{routePrefix}/putaway-tasks/{response.Id}", response);
        })
        .WithName($"CreateLoadArrIntegrationPutawayTask{nameSuffix}");

        integrations.MapPost("/putaway-tasks/{putawayTaskId}/complete", (string putawayTaskId, JsonElement _) =>
        {
            var response = new LoadArrIntegrationTaskResponse(
                putawayTaskId,
                "putaway",
                "supplyarr-item-001",
                1m,
                "completed",
                "location-001",
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"CompleteLoadArrIntegrationPutawayTask{nameSuffix}");

        integrations.MapPost("/reservations", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationReservationResponse(
                $"resv-{Guid.NewGuid():N}"[..15],
                ReadOptionalString(request, "demandReference") ?? "demand-001",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "quantity") ?? 1m,
                "reserved",
                TimestampUtc());
            return Results.Created($"{routePrefix}/reservations/{response.ReservationId}", response);
        })
        .WithName($"CreateLoadArrIntegrationReservation{nameSuffix}");

        integrations.MapGet("/reservations/{reservationId}", (string reservationId) =>
        {
            var reservation = ResolveReservation(reservationId);
            return reservation is null ? Results.NotFound() : Results.Ok(reservation);
        })
        .WithName($"GetLoadArrIntegrationReservation{nameSuffix}");

        integrations.MapPost("/reservations/{reservationId}/release", (string reservationId, JsonElement _) =>
        {
            var reservation = ResolveReservation(reservationId);
            var response = new LoadArrIntegrationReservationResponse(
                reservationId,
                reservation?.DemandReference ?? "demand-001",
                reservation?.SupplyarrItemId ?? "supplyarr-item-001",
                reservation?.Quantity ?? 1m,
                "released",
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"ReleaseLoadArrIntegrationReservation{nameSuffix}");

        integrations.MapPost("/work-order-demands", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationDemandResponse(
                $"wod-{Guid.NewGuid():N}"[..15],
                ReadOptionalString(request, "workOrderId") ?? "wo-001",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "quantity") ?? 1m,
                "received",
                TimestampUtc());
            return Results.Created($"{routePrefix}/work-order-demands/{response.DemandId}", response);
        })
        .WithName($"CreateLoadArrIntegrationWorkOrderDemand{nameSuffix}");

        integrations.MapPost("/order-demands", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationDemandResponse(
                $"od-{Guid.NewGuid():N}"[..13],
                ReadOptionalString(request, "orderId") ?? "order-001",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "quantity") ?? 1m,
                "received",
                TimestampUtc());
            return Results.Created($"{routePrefix}/order-demands/{response.DemandId}", response);
        })
        .WithName($"CreateLoadArrIntegrationOrderDemand{nameSuffix}");

        integrations.MapPost("/pick-tasks", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationTaskResponse(
                $"pk-{Guid.NewGuid():N}"[..13],
                "pick",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "quantity") ?? 1m,
                "ready",
                ReadOptionalString(request, "locationId") ?? "location-001",
                TimestampUtc());
            return Results.Created($"{routePrefix}/pick-tasks/{response.Id}", response);
        })
        .WithName($"CreateLoadArrIntegrationPickTask{nameSuffix}");

        integrations.MapPost("/pick-tasks/{pickTaskId}/complete", (string pickTaskId, JsonElement _) =>
        {
            var response = new LoadArrIntegrationTaskResponse(
                pickTaskId,
                "pick",
                "supplyarr-item-001",
                1m,
                "completed",
                "location-001",
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"CompleteLoadArrIntegrationPickTask{nameSuffix}");

        integrations.MapPost("/issues", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationIssueResponse(
                $"is-{Guid.NewGuid():N}"[..13],
                ReadOptionalString(request, "sourceReference") ?? "pick-task-001",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "quantity") ?? 1m,
                "posted",
                TimestampUtc());
            return Results.Created($"{routePrefix}/issues/{response.IssueId}", response);
        })
        .WithName($"CreateLoadArrIntegrationIssue{nameSuffix}");

        integrations.MapPost("/returns", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationReturnResponse(
                $"ret-{Guid.NewGuid():N}"[..13],
                ReadOptionalString(request, "sourceReference") ?? "issue-001",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "quantity") ?? 1m,
                "received",
                TimestampUtc());
            return Results.Created($"{routePrefix}/returns/{response.ReturnId}", response);
        })
        .WithName($"CreateLoadArrIntegrationReturn{nameSuffix}");

        integrations.MapPost("/transfers", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationTransferResponse(
                $"trf-{Guid.NewGuid():N}"[..13],
                ReadOptionalString(request, "fromLocationId") ?? "location-001",
                ReadOptionalString(request, "toLocationId") ?? "location-002",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "quantity") ?? 1m,
                "created",
                TimestampUtc());
            return Results.Created($"{routePrefix}/transfers/{response.TransferId}", response);
        })
        .WithName($"CreateLoadArrIntegrationTransfer{nameSuffix}");

        integrations.MapPost("/counts", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationCountResponse(
                $"cnt-{Guid.NewGuid():N}"[..13],
                ReadOptionalString(request, "countNumber") ?? "CNT-0001",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "expectedQuantity") ?? 5m,
                ReadOptionalDecimal(request, "countedQuantity") ?? 0m,
                "open",
                TimestampUtc());
            return Results.Created($"{routePrefix}/counts/{response.CountId}", response);
        })
        .WithName($"CreateLoadArrIntegrationCount{nameSuffix}");

        integrations.MapGet("/counts/{countId}", (string countId) =>
        {
            var count = ResolveCount(countId);
            return count is null ? Results.NotFound() : Results.Ok(count);
        })
        .WithName($"GetLoadArrIntegrationCount{nameSuffix}");

        integrations.MapPost("/counts/{countId}/lines", (string countId, JsonElement request) =>
        {
            var response = new LoadArrIntegrationCountLineResponse(
                $"cline-{Guid.NewGuid():N}"[..16],
                countId,
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "expectedQuantity") ?? 5m,
                ReadOptionalDecimal(request, "countedQuantity") ?? 5m,
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"AddLoadArrIntegrationCountLine{nameSuffix}");

        integrations.MapPost("/counts/{countId}/post", (string countId, JsonElement request) =>
        {
            var count = ResolveCount(countId);
            var response = new LoadArrIntegrationCountPostResponse(
                countId,
                count?.Status == "open" ? "posted" : count?.Status ?? "posted",
                ReadOptionalString(request, "personId") ?? "person-001",
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"PostLoadArrIntegrationCount{nameSuffix}");

        integrations.MapPost("/adjustments", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationAdjustmentResponse(
                $"adj-{Guid.NewGuid():N}"[..13],
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "quantityDelta") ?? -1m,
                "created",
                TimestampUtc());
            return Results.Created($"{routePrefix}/adjustments/{response.AdjustmentId}", response);
        })
        .WithName($"CreateLoadArrIntegrationAdjustment{nameSuffix}");

        integrations.MapPost("/discrepancies", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationDiscrepancyResponse(
                $"dsc-{Guid.NewGuid():N}"[..13],
                ReadOptionalString(request, "sourceReference") ?? "receipts/rcpt-001",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "varianceQuantity") ?? 0m,
                "open",
                TimestampUtc());
            return Results.Created($"{routePrefix}/discrepancies/{response.DiscrepancyId}", response);
        })
        .WithName($"CreateLoadArrIntegrationDiscrepancy{nameSuffix}");

        integrations.MapPost("/holds", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationHoldResponse(
                $"hld-{Guid.NewGuid():N}"[..13],
                ReadOptionalString(request, "holdType") ?? "quarantine",
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalDecimal(request, "quantity") ?? 1m,
                "active",
                TimestampUtc());
            return Results.Created($"{routePrefix}/holds/{response.HoldId}", response);
        })
        .WithName($"CreateLoadArrIntegrationHold{nameSuffix}");

        integrations.MapPost("/hold-releases", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationHoldReleaseResponse(
                ReadOptionalString(request, "holdId") ?? "hld-001",
                "released",
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"ReleaseLoadArrIntegrationHold{nameSuffix}");

        integrations.MapPost("/disposition-movements", (JsonElement request) =>
        {
            var response = new LoadArrIntegrationDispositionMovementResponse(
                $"dmv-{Guid.NewGuid():N}"[..14],
                ReadOptionalString(request, "supplyarrItemId") ?? "supplyarr-item-001",
                ReadOptionalString(request, "fromLocationId") ?? "location-001",
                ReadOptionalString(request, "toLocationId") ?? "location-002",
                ReadOptionalDecimal(request, "quantity") ?? 1m,
                ReadOptionalString(request, "dispositionCode") ?? "REWORK",
                TimestampUtc());
            return Results.Created($"{routePrefix}/disposition-movements/{response.DispositionMovementId}", response);
        })
        .WithName($"CreateLoadArrIntegrationDispositionMovement{nameSuffix}");
    }

    private static string TimestampUtc() => DateTimeOffset.UtcNow.ToString("O");

    private static string? ReadOptionalString(JsonElement payload, string propertyName)
    {
        return payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static decimal? ReadOptionalDecimal(JsonElement payload, string propertyName)
    {
        if (payload.ValueKind != JsonValueKind.Object || !payload.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.Number
            ? property.GetDecimal()
            : null;
    }

    private static LoadArrIntegrationItemResponse[] CreateIntegrationItems() =>
        new[]
        {
            new LoadArrIntegrationItemResponse(
                "item-001",
                "supplyarr-item-001",
                "BP-1001",
                "Brake pump",
                "each",
                "active",
                "2026-06-03T00:00:00Z"),
            new LoadArrIntegrationItemResponse(
                "item-002",
                "supplyarr-item-002",
                "GT-2201",
                "Hydraulic hose",
                "each",
                "active",
                "2026-06-03T00:00:00Z")
        };

    private static LoadArrIntegrationItemResponse? ResolveIntegrationItem(string id) =>
        CreateIntegrationItems().SingleOrDefault(item => string.Equals(item.ItemId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationLocationProfileResponse[] CreateLocationProfiles() =>
        new[]
        {
            new LoadArrIntegrationLocationProfileResponse(
                "profile-001",
                "location-001",
                "BIN",
                true,
                "2026-06-03T00:00:00Z"),
            new LoadArrIntegrationLocationProfileResponse(
                "profile-002",
                "location-002",
                "DOCK",
                true,
                "2026-06-03T00:00:00Z")
        };

    private static LoadArrIntegrationLocationProfileResponse? ResolveLocationProfile(string id) =>
        CreateLocationProfiles().SingleOrDefault(profile =>
            string.Equals(profile.WmsLocationProfileId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationBalanceResponse[] CreateIntegrationBalances() =>
        new[]
        {
            new LoadArrIntegrationBalanceResponse("bal-001", "supplyarr-item-001", "location-001", 150m, 120m, "available", "2026-06-03T00:00:00Z"),
            new LoadArrIntegrationBalanceResponse("bal-002", "supplyarr-item-002", "location-002", 40m, 35m, "allocated", "2026-06-03T00:00:00Z")
        };

    private static LoadArrIntegrationBalanceResponse? ResolveIntegrationBalance(string id) =>
        CreateIntegrationBalances().SingleOrDefault(balance => string.Equals(balance.BalanceId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationExpectedReceiptResponse[] CreateExpectedReceipts() =>
        new[]
        {
            new LoadArrIntegrationExpectedReceiptResponse("exp-001", "supplyarr-item-001", "Fleet Supplier", "location-001", 25m, "arrived", "2026-06-03T00:00:00Z"),
            new LoadArrIntegrationExpectedReceiptResponse("exp-002", "supplyarr-item-002", "Depot Supplier", "location-002", 12m, "pending", "2026-06-03T00:00:00Z")
        };

    private static LoadArrIntegrationExpectedReceiptResponse? ResolveExpectedReceipt(string id) =>
        CreateExpectedReceipts().SingleOrDefault(receipt =>
            string.Equals(receipt.ExpectedReceiptId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationReceiptResponse[] CreateReceipts() =>
        new[]
        {
            new LoadArrIntegrationReceiptResponse(
                "rcpt-001",
                "RCPT-1001",
                "exp-001",
                "completed",
                "supplier-created",
                "supplyarr-item-001",
                25m,
                "2026-06-03T00:00:00Z"),
            new LoadArrIntegrationReceiptResponse(
                "rcpt-002",
                "RCPT-1002",
                null,
                "open",
                "supplier-created",
                "supplyarr-item-002",
                8m,
                "2026-06-03T00:00:00Z")
        };

    private static LoadArrIntegrationReceiptResponse? ResolveReceipt(string id) =>
        CreateReceipts().SingleOrDefault(receipt =>
            string.Equals(receipt.ReceiptId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationReservationResponse[] CreateReservations() =>
        new[]
        {
            new LoadArrIntegrationReservationResponse(
                "resv-001",
                "demand-001",
                "supplyarr-item-001",
                2m,
                "reserved",
                "2026-06-03T00:00:00Z"),
            new LoadArrIntegrationReservationResponse(
                "resv-002",
                "wod-001",
                "supplyarr-item-002",
                1m,
                "partially_reserved",
                "2026-06-03T00:00:00Z")
        };

    private static LoadArrIntegrationReservationResponse? ResolveReservation(string id) =>
        CreateReservations().SingleOrDefault(reservation =>
            string.Equals(reservation.ReservationId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationCountResponse[] CreateCounts() =>
        new[]
        {
            new LoadArrIntegrationCountResponse("cnt-001", "CNT-001", "supplyarr-item-001", 10m, 8m, "open", "2026-06-03T00:00:00Z"),
            new LoadArrIntegrationCountResponse("cnt-002", "CNT-002", "supplyarr-item-002", 5m, 5m, "approved", "2026-06-03T00:00:00Z")
        };

    private static LoadArrIntegrationCountResponse? ResolveCount(string id) =>
        CreateCounts().SingleOrDefault(count =>
            string.Equals(count.CountId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationStockMovementResponse[] CreateStockMovements() =>
        new[]
        {
            new LoadArrIntegrationStockMovementResponse(
                "mov-001",
                "inbound",
                "supplyarr-item-001",
                "location-001",
                10m,
                "2026-06-03T00:00:00Z"),
            new LoadArrIntegrationStockMovementResponse(
                "mov-002",
                "outbound",
                "supplyarr-item-002",
                "location-002",
                -3m,
                "2026-06-03T00:00:00Z")
        };

    private static LoadArrIntegrationStockMovementResponse? ResolveStockMovement(string id) =>
        CreateStockMovements().SingleOrDefault(movement =>
            string.Equals(movement.MovementId, id, StringComparison.OrdinalIgnoreCase));
}

public sealed record LoadArrIntegrationItemResponse(
    string ItemId,
    string SupplyarrItemId,
    string ItemCode,
    string ItemNameSnapshot,
    string UnitOfMeasureSnapshot,
    string Status,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationLocationProfileResponse(
    string WmsLocationProfileId,
    string StaffarrLocationId,
    string WmsLocationType,
    bool IsActive,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationBalanceResponse(
    string BalanceId,
    string SupplyarrItemId,
    string LocationId,
    decimal QuantityOnHand,
    decimal QuantityAvailable,
    string State,
    string LastUpdatedUtc);

public sealed record LoadArrIntegrationAvailabilityCheckResponse(
    string AvailabilityCheckId,
    string SupplyarrItemId,
    string WarehouseLocationId,
    decimal RequestedQuantity,
    bool IsAvailable,
    string Status,
    string CheckedAtUtc);

public sealed record LoadArrIntegrationExpectedReceiptResponse(
    string ExpectedReceiptId,
    string SupplyarrItemId,
    string SupplierName,
    string WarehouseLocationId,
    decimal ExpectedQuantity,
    string Status,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationReceiptResponse(
    string ReceiptId,
    string ReceiptNumber,
    string? ExpectedReceiptId,
    string Status,
    string SourceReference,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationReceiptLineResponse(
    string ReceiptLineId,
    string ReceiptId,
    string SupplyarrItemId,
    decimal ReceivedQuantity,
    string ReceivedAtUtc);

public sealed record LoadArrIntegrationReceiptCloseResponse(
    string ReceiptId,
    string Status,
    string CompletedAtUtc);

public sealed record LoadArrIntegrationTaskResponse(
    string Id,
    string TaskType,
    string SupplyarrItemId,
    decimal Quantity,
    string Status,
    string LocationId,
    string UpdatedAtUtc);

public sealed record LoadArrIntegrationReservationResponse(
    string ReservationId,
    string DemandReference,
    string SupplyarrItemId,
    decimal Quantity,
    string Status,
    string UpdatedAtUtc);

public sealed record LoadArrIntegrationDemandResponse(
    string DemandId,
    string SourceReference,
    string SupplyarrItemId,
    decimal Quantity,
    string Status,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationIssueResponse(
    string IssueId,
    string SourceReference,
    string SupplyarrItemId,
    decimal Quantity,
    string Status,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationReturnResponse(
    string ReturnId,
    string SourceReference,
    string SupplyarrItemId,
    decimal Quantity,
    string Status,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationTransferResponse(
    string TransferId,
    string FromLocationId,
    string ToLocationId,
    string SupplyarrItemId,
    decimal Quantity,
    string Status,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationCountResponse(
    string CountId,
    string CountNumber,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal CountedQuantity,
    string Status,
    string UpdatedAtUtc);

public sealed record LoadArrIntegrationCountLineResponse(
    string LineId,
    string CountId,
    string SupplyarrItemId,
    decimal ExpectedQuantity,
    decimal CountedQuantity,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationCountPostResponse(
    string CountId,
    string Status,
    string PostedByPersonId,
    string PostedAtUtc);

public sealed record LoadArrIntegrationAdjustmentResponse(
    string AdjustmentId,
    string SupplyarrItemId,
    decimal QuantityDelta,
    string Status,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationDiscrepancyResponse(
    string DiscrepancyId,
    string SourceReference,
    string SupplyarrItemId,
    decimal VarianceQuantity,
    string Status,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationHoldResponse(
    string HoldId,
    string HoldType,
    string SupplyarrItemId,
    decimal Quantity,
    string Status,
    string UpdatedAtUtc);

public sealed record LoadArrIntegrationHoldReleaseResponse(
    string HoldId,
    string Status,
    string ReleasedAtUtc);

public sealed record LoadArrIntegrationDispositionMovementResponse(
    string DispositionMovementId,
    string SupplyarrItemId,
    string FromLocationId,
    string ToLocationId,
    decimal Quantity,
    string DispositionCode,
    string CreatedAtUtc);

public sealed record LoadArrIntegrationStockMovementResponse(
    string MovementId,
    string MovementType,
    string SupplyarrItemId,
    string LocationId,
    decimal Quantity,
    string CreatedAtUtc);
