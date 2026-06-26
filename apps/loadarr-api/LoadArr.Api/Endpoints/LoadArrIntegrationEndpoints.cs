using System;
using System.Text.Json;
using LoadArr.Api.Settings;

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

        integrations.MapGet("/items", (HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var items = CreateIntegrationItems();
            return Results.Ok(new LoadArrListResponse<LoadArrIntegrationItemResponse>(items, items.Length));
        })
        .WithName($"ListLoadArrIntegrationItems{nameSuffix}");

        integrations.MapGet("/items/{itemId}", (string itemId, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var item = ResolveIntegrationItem(itemId);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName($"GetLoadArrIntegrationItem{nameSuffix}");

        integrations.MapPost("/items", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var itemCode = ReadOptionalString(request, "itemCode");
            if (string.IsNullOrWhiteSpace(itemCode))
            {
                return MissingRequired("itemCode");
            }

            var itemNameSnapshot = ReadOptionalString(request, "itemNameSnapshot");
            if (string.IsNullOrWhiteSpace(itemNameSnapshot))
            {
                return MissingRequired("itemNameSnapshot");
            }

            var unitOfMeasureSnapshot = ReadOptionalString(request, "unitOfMeasureSnapshot");
            if (string.IsNullOrWhiteSpace(unitOfMeasureSnapshot))
            {
                return MissingRequired("unitOfMeasureSnapshot");
            }

            var createdAt = TimestampUtc();
            var response = new LoadArrIntegrationItemResponse(
                $"item-{Guid.NewGuid():N}"[..14],
                supplyarrItemId,
                itemCode,
                itemNameSnapshot,
                unitOfMeasureSnapshot,
                "active",
                createdAt);

            return Results.Created($"{routePrefix}/items/{response.ItemId}", response);
        })
        .WithName($"CreateLoadArrIntegrationItem{nameSuffix}");

        integrations.MapGet("/location-profiles", (HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var profiles = CreateLocationProfiles();
            return Results.Ok(new LoadArrListResponse<LoadArrIntegrationLocationProfileResponse>(profiles, profiles.Length));
        })
        .WithName($"ListLoadArrIntegrationLocationProfiles{nameSuffix}");

        integrations.MapGet("/location-profiles/{wmsLocationProfileId}", (string wmsLocationProfileId, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var profile = ResolveLocationProfile(wmsLocationProfileId);
            return profile is null ? Results.NotFound() : Results.Ok(profile);
        })
        .WithName($"GetLoadArrIntegrationLocationProfile{nameSuffix}");

        integrations.MapPost("/location-profiles", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var staffarrLocationId = ReadOptionalString(request, "staffarrLocationId");
            if (string.IsNullOrWhiteSpace(staffarrLocationId))
            {
                return MissingRequired("staffarrLocationId");
            }

            var wmsLocationType = ReadOptionalString(request, "wmsLocationType");
            if (string.IsNullOrWhiteSpace(wmsLocationType))
            {
                return MissingRequired("wmsLocationType");
            }

            var response = new LoadArrIntegrationLocationProfileResponse(
                $"lpf-{Guid.NewGuid():N}"[..14],
                staffarrLocationId,
                wmsLocationType,
                true,
                TimestampUtc());

            return Results.Created($"{routePrefix}/location-profiles/{response.WmsLocationProfileId}", response);
        })
        .WithName($"CreateLoadArrIntegrationLocationProfile{nameSuffix}");

        integrations.MapGet("/balances", (HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var balances = CreateIntegrationBalances();
            return Results.Ok(new LoadArrListResponse<LoadArrIntegrationBalanceResponse>(balances, balances.Length));
        })
        .WithName($"ListLoadArrIntegrationBalances{nameSuffix}");

        integrations.MapGet("/balances/{balanceId}", (string balanceId, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var balance = ResolveIntegrationBalance(balanceId);
            return balance is null ? Results.NotFound() : Results.Ok(balance);
        })
        .WithName($"GetLoadArrIntegrationBalance{nameSuffix}");

        integrations.MapPost("/availability-checks", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var warehouseLocationId = ReadOptionalString(request, "warehouseLocationId");
            if (string.IsNullOrWhiteSpace(warehouseLocationId))
            {
                return MissingRequired("warehouseLocationId");
            }

            var requestedQuantity = ReadOptionalDecimal(request, "requestedQuantity");
            if (requestedQuantity is null)
            {
                return MissingRequired("requestedQuantity");
            }

            var response = new LoadArrIntegrationAvailabilityCheckResponse(
                $"check-{Guid.NewGuid():N}"[..15],
                supplyarrItemId,
                warehouseLocationId,
                requestedQuantity.Value,
                true,
                "availability_ok",
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"CheckLoadArrIntegrationAvailability{nameSuffix}");

        integrations.MapPost("/expected-receipts", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var supplierName = ReadOptionalString(request, "supplierName");
            if (string.IsNullOrWhiteSpace(supplierName))
            {
                return MissingRequired("supplierName");
            }

            var warehouseLocationId = ReadOptionalString(request, "warehouseLocationId");
            if (string.IsNullOrWhiteSpace(warehouseLocationId))
            {
                return MissingRequired("warehouseLocationId");
            }

            var expectedQuantity = ReadOptionalDecimal(request, "expectedQuantity");
            if (expectedQuantity is null)
            {
                return MissingRequired("expectedQuantity");
            }

            var createdAt = TimestampUtc();
            var response = new LoadArrIntegrationExpectedReceiptResponse(
                $"exp-{Guid.NewGuid():N}"[..15],
                supplyarrItemId,
                supplierName,
                warehouseLocationId,
                expectedQuantity.Value,
                "pending",
                createdAt);
            return Results.Created($"{routePrefix}/expected-receipts/{response.ExpectedReceiptId}", response);
        })
        .WithName($"CreateLoadArrIntegrationExpectedReceipt{nameSuffix}");

        integrations.MapGet("/expected-receipts/{expectedReceiptId}", (string expectedReceiptId, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var receipt = ResolveExpectedReceipt(expectedReceiptId);
            return receipt is null ? Results.NotFound() : Results.Ok(receipt);
        })
        .WithName($"GetLoadArrIntegrationExpectedReceipt{nameSuffix}");

        integrations.MapPost("/expected-receipts/{expectedReceiptId}/status-updates", (string expectedReceiptId, JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var existing = ResolveExpectedReceipt(expectedReceiptId);
            if (existing is null)
            {
                return Results.NotFound();
            }

            var status = ReadOptionalString(request, "status");
            if (string.IsNullOrWhiteSpace(status))
            {
                return MissingRequired("status");
            }

            var updated = new LoadArrIntegrationExpectedReceiptResponse(
                expectedReceiptId,
                existing.SupplyarrItemId,
                existing.SupplierName,
                existing.WarehouseLocationId,
                existing.ExpectedQuantity,
                status,
                TimestampUtc());
            return Results.Ok(updated);
        })
        .WithName($"UpdateLoadArrIntegrationExpectedReceiptStatus{nameSuffix}");

        integrations.MapGet("/stock-movements", (HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var movements = CreateStockMovements();
            return Results.Ok(new LoadArrListResponse<LoadArrIntegrationStockMovementResponse>(movements, movements.Length));
        })
        .WithName($"ListLoadArrIntegrationStockMovements{nameSuffix}");

        integrations.MapGet("/stock-movements/{movementId}", (string movementId, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var movement = ResolveStockMovement(movementId);
            return movement is null ? Results.NotFound() : Results.Ok(movement);
        })
        .WithName($"GetLoadArrIntegrationStockMovement{nameSuffix}");

        integrations.MapPost("/receipts", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var receiptNumber = ReadOptionalString(request, "receiptNumber");
            if (string.IsNullOrWhiteSpace(receiptNumber))
            {
                return MissingRequired("receiptNumber");
            }

            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var expectedQuantity = ReadOptionalDecimal(request, "expectedQuantity");
            if (expectedQuantity is null)
            {
                return MissingRequired("expectedQuantity");
            }

            var response = new LoadArrIntegrationReceiptResponse(
                $"rcpt-{Guid.NewGuid():N}"[..15],
                receiptNumber,
                ReadOptionalString(request, "expectedReceiptId"),
                "open",
                "supplier-created",
                supplyarrItemId,
                expectedQuantity.Value,
                TimestampUtc());
            return Results.Created($"{routePrefix}/receipts/{response.ReceiptId}", response);
        })
        .WithName($"CreateLoadArrIntegrationReceipt{nameSuffix}");

        integrations.MapGet("/receipts/{receiptId}", (string receiptId, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var receipt = ResolveReceipt(receiptId);
            return receipt is null ? Results.NotFound() : Results.Ok(receipt);
        })
        .WithName($"GetLoadArrIntegrationReceipt{nameSuffix}");

        integrations.MapPost("/receipts/{receiptId}/lines", (string receiptId, JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var receivedQuantity = ReadOptionalDecimal(request, "receivedQuantity");
            if (receivedQuantity is null)
            {
                return MissingRequired("receivedQuantity");
            }

            var line = new LoadArrIntegrationReceiptLineResponse(
                $"line-{Guid.NewGuid():N}"[..14],
                receiptId,
                supplyarrItemId,
                receivedQuantity.Value,
                TimestampUtc());
            return Results.Ok(line);
        })
        .WithName($"AddLoadArrIntegrationReceiptLine{nameSuffix}");

        integrations.MapPost("/receipts/{receiptId}/close", (string receiptId, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var existing = ResolveReceipt(receiptId);
            if (existing is null)
            {
                return Results.NotFound();
            }

            var response = new LoadArrIntegrationReceiptCloseResponse(
                receiptId,
                existing.Status == "open" ? "completed" : existing.Status,
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"CloseLoadArrIntegrationReceipt{nameSuffix}");

        integrations.MapPost("/putaway-tasks", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var quantity = ReadOptionalDecimal(request, "quantity");
            if (quantity is null)
            {
                return MissingRequired("quantity");
            }

            var locationId = ReadOptionalString(request, "locationId");
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return MissingRequired("locationId");
            }

            var response = new LoadArrIntegrationTaskResponse(
                $"pt-{Guid.NewGuid():N}"[..13],
                "putaway",
                supplyarrItemId,
                quantity.Value,
                "ready",
                locationId,
                TimestampUtc());
            return Results.Created($"{routePrefix}/putaway-tasks/{response.Id}", response);
        })
        .WithName($"CreateLoadArrIntegrationPutawayTask{nameSuffix}");

        integrations.MapPost("/putaway-tasks/{putawayTaskId}/complete", (string putawayTaskId, JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            return Results.NotFound();
        })
        .WithName($"CompleteLoadArrIntegrationPutawayTask{nameSuffix}");

        integrations.MapPost("/reservations", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var demandReference = ReadOptionalString(request, "demandReference");
            if (string.IsNullOrWhiteSpace(demandReference))
            {
                return MissingRequired("demandReference");
            }

            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var quantity = ReadOptionalDecimal(request, "quantity");
            if (quantity is null)
            {
                return MissingRequired("quantity");
            }

            var response = new LoadArrIntegrationReservationResponse(
                $"resv-{Guid.NewGuid():N}"[..15],
                demandReference,
                supplyarrItemId,
                quantity.Value,
                "reserved",
                TimestampUtc());
            return Results.Created($"{routePrefix}/reservations/{response.ReservationId}", response);
        })
        .WithName($"CreateLoadArrIntegrationReservation{nameSuffix}");

        integrations.MapGet("/reservations/{reservationId}", (string reservationId, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var reservation = ResolveReservation(reservationId);
            return reservation is null ? Results.NotFound() : Results.Ok(reservation);
        })
        .WithName($"GetLoadArrIntegrationReservation{nameSuffix}");

        integrations.MapPost("/reservations/{reservationId}/release", (string reservationId, JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var reservation = ResolveReservation(reservationId);
            if (reservation is null)
            {
                return Results.NotFound();
            }

            var response = new LoadArrIntegrationReservationResponse(
                reservationId,
                reservation.DemandReference,
                reservation.SupplyarrItemId,
                reservation.Quantity,
                "released",
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"ReleaseLoadArrIntegrationReservation{nameSuffix}");

        integrations.MapPost("/work-order-demands", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var workOrderId = ReadOptionalString(request, "workOrderId");
            if (string.IsNullOrWhiteSpace(workOrderId))
            {
                return MissingRequired("workOrderId");
            }

            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var quantity = ReadOptionalDecimal(request, "quantity");
            if (quantity is null)
            {
                return MissingRequired("quantity");
            }

            var response = new LoadArrIntegrationDemandResponse(
                $"wod-{Guid.NewGuid():N}"[..15],
                workOrderId,
                supplyarrItemId,
                quantity.Value,
                "received",
                TimestampUtc());
            return Results.Created($"{routePrefix}/work-order-demands/{response.DemandId}", response);
        })
        .WithName($"CreateLoadArrIntegrationWorkOrderDemand{nameSuffix}");

        integrations.MapPost("/order-demands", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var orderId = ReadOptionalString(request, "orderId");
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return MissingRequired("orderId");
            }

            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var quantity = ReadOptionalDecimal(request, "quantity");
            if (quantity is null)
            {
                return MissingRequired("quantity");
            }

            var response = new LoadArrIntegrationDemandResponse(
                $"od-{Guid.NewGuid():N}"[..13],
                orderId,
                supplyarrItemId,
                quantity.Value,
                "received",
                TimestampUtc());
            return Results.Created($"{routePrefix}/order-demands/{response.DemandId}", response);
        })
        .WithName($"CreateLoadArrIntegrationOrderDemand{nameSuffix}");

        integrations.MapPost("/pick-tasks", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var quantity = ReadOptionalDecimal(request, "quantity");
            if (quantity is null)
            {
                return MissingRequired("quantity");
            }

            var locationId = ReadOptionalString(request, "locationId");
            if (string.IsNullOrWhiteSpace(locationId))
            {
                return MissingRequired("locationId");
            }

            var response = new LoadArrIntegrationTaskResponse(
                $"pk-{Guid.NewGuid():N}"[..13],
                "pick",
                supplyarrItemId,
                quantity.Value,
                "ready",
                locationId,
                TimestampUtc());
            return Results.Created($"{routePrefix}/pick-tasks/{response.Id}", response);
        })
        .WithName($"CreateLoadArrIntegrationPickTask{nameSuffix}");

        integrations.MapPost("/pick-tasks/{pickTaskId}/complete", (string pickTaskId, JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            return Results.NotFound();
        })
        .WithName($"CompleteLoadArrIntegrationPickTask{nameSuffix}");

        integrations.MapPost("/issues", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var sourceReference = ReadOptionalString(request, "sourceReference");
            if (string.IsNullOrWhiteSpace(sourceReference))
            {
                return MissingRequired("sourceReference");
            }

            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var quantity = ReadOptionalDecimal(request, "quantity");
            if (quantity is null)
            {
                return MissingRequired("quantity");
            }

            var response = new LoadArrIntegrationIssueResponse(
                $"is-{Guid.NewGuid():N}"[..13],
                sourceReference,
                supplyarrItemId,
                quantity.Value,
                "posted",
                TimestampUtc());
            return Results.Created($"{routePrefix}/issues/{response.IssueId}", response);
        })
        .WithName($"CreateLoadArrIntegrationIssue{nameSuffix}");

        integrations.MapPost("/returns", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var sourceReference = ReadOptionalString(request, "sourceReference");
            if (string.IsNullOrWhiteSpace(sourceReference))
            {
                return MissingRequired("sourceReference");
            }

            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var quantity = ReadOptionalDecimal(request, "quantity");
            if (quantity is null)
            {
                return MissingRequired("quantity");
            }

            var response = new LoadArrIntegrationReturnResponse(
                $"ret-{Guid.NewGuid():N}"[..13],
                sourceReference,
                supplyarrItemId,
                quantity.Value,
                "received",
                TimestampUtc());
            return Results.Created($"{routePrefix}/returns/{response.ReturnId}", response);
        })
        .WithName($"CreateLoadArrIntegrationReturn{nameSuffix}");

        integrations.MapPost("/transfers", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var fromLocationId = ReadOptionalString(request, "fromLocationId");
            if (string.IsNullOrWhiteSpace(fromLocationId))
            {
                return MissingRequired("fromLocationId");
            }

            var toLocationId = ReadOptionalString(request, "toLocationId");
            if (string.IsNullOrWhiteSpace(toLocationId))
            {
                return MissingRequired("toLocationId");
            }

            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var quantity = ReadOptionalDecimal(request, "quantity");
            if (quantity is null)
            {
                return MissingRequired("quantity");
            }

            var response = new LoadArrIntegrationTransferResponse(
                $"trf-{Guid.NewGuid():N}"[..13],
                fromLocationId,
                toLocationId,
                supplyarrItemId,
                quantity.Value,
                "created",
                TimestampUtc());
            return Results.Created($"{routePrefix}/transfers/{response.TransferId}", response);
        })
        .WithName($"CreateLoadArrIntegrationTransfer{nameSuffix}");

        integrations.MapPost("/counts", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var countNumber = ReadOptionalString(request, "countNumber");
            if (string.IsNullOrWhiteSpace(countNumber))
            {
                return MissingRequired("countNumber");
            }

            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var expectedQuantity = ReadOptionalDecimal(request, "expectedQuantity");
            if (expectedQuantity is null)
            {
                return MissingRequired("expectedQuantity");
            }

            var countedQuantity = ReadOptionalDecimal(request, "countedQuantity");
            if (countedQuantity is null)
            {
                return MissingRequired("countedQuantity");
            }

            var response = new LoadArrIntegrationCountResponse(
                $"cnt-{Guid.NewGuid():N}"[..13],
                countNumber,
                supplyarrItemId,
                expectedQuantity.Value,
                countedQuantity.Value,
                "open",
                TimestampUtc());
            return Results.Created($"{routePrefix}/counts/{response.CountId}", response);
        })
        .WithName($"CreateLoadArrIntegrationCount{nameSuffix}");

        integrations.MapGet("/counts/{countId}", (string countId, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationRead(context.User);
            var count = ResolveCount(countId);
            return count is null ? Results.NotFound() : Results.Ok(count);
        })
        .WithName($"GetLoadArrIntegrationCount{nameSuffix}");

        integrations.MapPost("/counts/{countId}/lines", (string countId, JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var expectedQuantity = ReadOptionalDecimal(request, "expectedQuantity");
            if (expectedQuantity is null)
            {
                return MissingRequired("expectedQuantity");
            }

            var countedQuantity = ReadOptionalDecimal(request, "countedQuantity");
            if (countedQuantity is null)
            {
                return MissingRequired("countedQuantity");
            }

            var response = new LoadArrIntegrationCountLineResponse(
                $"cline-{Guid.NewGuid():N}"[..16],
                countId,
                supplyarrItemId,
                expectedQuantity.Value,
                countedQuantity.Value,
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"AddLoadArrIntegrationCountLine{nameSuffix}");

        integrations.MapPost("/counts/{countId}/post", (string countId, JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var count = ResolveCount(countId);
            if (count is null)
            {
                return Results.NotFound();
            }

            var personId = ReadOptionalString(request, "personId");
            if (string.IsNullOrWhiteSpace(personId))
            {
                return MissingRequired("personId");
            }

            var response = new LoadArrIntegrationCountPostResponse(
                countId,
                count.Status == "open" ? "posted" : count.Status,
                personId,
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"PostLoadArrIntegrationCount{nameSuffix}");

        integrations.MapPost("/adjustments", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var quantityDelta = ReadOptionalDecimal(request, "quantityDelta");
            if (quantityDelta is null)
            {
                return MissingRequired("quantityDelta");
            }

            var response = new LoadArrIntegrationAdjustmentResponse(
                $"adj-{Guid.NewGuid():N}"[..13],
                supplyarrItemId,
                quantityDelta.Value,
                "created",
                TimestampUtc());
            return Results.Created($"{routePrefix}/adjustments/{response.AdjustmentId}", response);
        })
        .WithName($"CreateLoadArrIntegrationAdjustment{nameSuffix}");

        integrations.MapPost("/discrepancies", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var sourceReference = ReadOptionalString(request, "sourceReference");
            if (string.IsNullOrWhiteSpace(sourceReference))
            {
                return MissingRequired("sourceReference");
            }

            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var varianceQuantity = ReadOptionalDecimal(request, "varianceQuantity");
            if (varianceQuantity is null)
            {
                return MissingRequired("varianceQuantity");
            }

            var response = new LoadArrIntegrationDiscrepancyResponse(
                $"dsc-{Guid.NewGuid():N}"[..13],
                sourceReference,
                supplyarrItemId,
                varianceQuantity.Value,
                "open",
                TimestampUtc());
            return Results.Created($"{routePrefix}/discrepancies/{response.DiscrepancyId}", response);
        })
        .WithName($"CreateLoadArrIntegrationDiscrepancy{nameSuffix}");

        integrations.MapPost("/holds", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var holdType = ReadOptionalString(request, "holdType");
            if (string.IsNullOrWhiteSpace(holdType))
            {
                return MissingRequired("holdType");
            }

            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var quantity = ReadOptionalDecimal(request, "quantity");
            if (quantity is null)
            {
                return MissingRequired("quantity");
            }

            var response = new LoadArrIntegrationHoldResponse(
                $"hld-{Guid.NewGuid():N}"[..13],
                holdType,
                supplyarrItemId,
                quantity.Value,
                "active",
                TimestampUtc());
            return Results.Created($"{routePrefix}/holds/{response.HoldId}", response);
        })
        .WithName($"CreateLoadArrIntegrationHold{nameSuffix}");

        integrations.MapPost("/hold-releases", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var holdId = ReadOptionalString(request, "holdId");
            if (string.IsNullOrWhiteSpace(holdId))
            {
                return MissingRequired("holdId");
            }

            var response = new LoadArrIntegrationHoldReleaseResponse(
                holdId,
                "released",
                TimestampUtc());
            return Results.Ok(response);
        })
        .WithName($"ReleaseLoadArrIntegrationHold{nameSuffix}");

        integrations.MapPost("/disposition-movements", (JsonElement request, HttpContext context, LoadArrAuthorizationService authorization) =>
        {
            authorization.RequireIntegrationManage(context.User);
            var supplyarrItemId = ReadOptionalString(request, "supplyarrItemId");
            if (string.IsNullOrWhiteSpace(supplyarrItemId))
            {
                return MissingRequired("supplyarrItemId");
            }

            var fromLocationId = ReadOptionalString(request, "fromLocationId");
            if (string.IsNullOrWhiteSpace(fromLocationId))
            {
                return MissingRequired("fromLocationId");
            }

            var toLocationId = ReadOptionalString(request, "toLocationId");
            if (string.IsNullOrWhiteSpace(toLocationId))
            {
                return MissingRequired("toLocationId");
            }

            var quantity = ReadOptionalDecimal(request, "quantity");
            if (quantity is null)
            {
                return MissingRequired("quantity");
            }

            var dispositionCode = ReadOptionalString(request, "dispositionCode");
            if (string.IsNullOrWhiteSpace(dispositionCode))
            {
                return MissingRequired("dispositionCode");
            }

            var response = new LoadArrIntegrationDispositionMovementResponse(
                $"dmv-{Guid.NewGuid():N}"[..14],
                supplyarrItemId,
                fromLocationId,
                toLocationId,
                quantity.Value,
                dispositionCode,
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

    private static IResult MissingRequired(string propertyName) =>
        Results.BadRequest(new { error = $"{propertyName} is required." });

    private static LoadArrIntegrationItemResponse[] CreateIntegrationItems() =>
        [];

    private static LoadArrIntegrationItemResponse? ResolveIntegrationItem(string id) =>
        CreateIntegrationItems().SingleOrDefault(item => string.Equals(item.ItemId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationLocationProfileResponse[] CreateLocationProfiles() =>
        [];

    private static LoadArrIntegrationLocationProfileResponse? ResolveLocationProfile(string id) =>
        CreateLocationProfiles().SingleOrDefault(profile =>
            string.Equals(profile.WmsLocationProfileId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationBalanceResponse[] CreateIntegrationBalances() =>
        [];

    private static LoadArrIntegrationBalanceResponse? ResolveIntegrationBalance(string id) =>
        CreateIntegrationBalances().SingleOrDefault(balance => string.Equals(balance.BalanceId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationExpectedReceiptResponse[] CreateExpectedReceipts() =>
        [];

    private static LoadArrIntegrationExpectedReceiptResponse? ResolveExpectedReceipt(string id) =>
        CreateExpectedReceipts().SingleOrDefault(receipt =>
            string.Equals(receipt.ExpectedReceiptId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationReceiptResponse[] CreateReceipts() =>
        [];

    private static LoadArrIntegrationReceiptResponse? ResolveReceipt(string id) =>
        CreateReceipts().SingleOrDefault(receipt =>
            string.Equals(receipt.ReceiptId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationReservationResponse[] CreateReservations() =>
        [];

    private static LoadArrIntegrationReservationResponse? ResolveReservation(string id) =>
        CreateReservations().SingleOrDefault(reservation =>
            string.Equals(reservation.ReservationId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationCountResponse[] CreateCounts() =>
        [];

    private static LoadArrIntegrationCountResponse? ResolveCount(string id) =>
        CreateCounts().SingleOrDefault(count =>
            string.Equals(count.CountId, id, StringComparison.OrdinalIgnoreCase));

    private static LoadArrIntegrationStockMovementResponse[] CreateStockMovements() =>
        [];

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
