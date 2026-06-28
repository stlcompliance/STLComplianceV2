using System;
using System.Text.Json;
using LoadArr.Api.Settings;

namespace LoadArr.Api.Endpoints;

public static class LoadArrIntegrationEndpoints
{
    private static IResult IntegrationDependencyUnavailable(string surface) =>
        Results.Json(
            new LoadArrProblemResponse(
                "dependency_unavailable",
                $"{surface} is unavailable because LoadArr does not yet have authoritative tenant-scoped integration synchronization for this tenant."),
            statusCode: StatusCodes.Status503ServiceUnavailable);

    private static IResult IntegrationReadUnavailable(
        HttpContext context,
        LoadArrAuthorizationService authorization,
        string surface)
    {
        authorization.RequireIntegrationRead(context.User);
        return IntegrationDependencyUnavailable(surface);
    }

    private static IResult IntegrationManageUnavailable(
        HttpContext context,
        LoadArrAuthorizationService authorization,
        string surface)
    {
        authorization.RequireIntegrationManage(context.User);
        return IntegrationDependencyUnavailable(surface);
    }

    public static void MapLoadArrIntegrationEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/integrations"), string.Empty, "/api/integrations");
        MapRoutes(app.MapGroup("/api/v1/integrations"), "V1", "/api/v1/integrations");
    }

    private static void MapRoutes(RouteGroupBuilder integrations, string nameSuffix, string routePrefix)
    {
        integrations = integrations.WithTags("Integrations").RequireAuthorization();

        integrations.MapGet("/items", (HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration items"))
        .WithName($"ListLoadArrIntegrationItems{nameSuffix}");

        integrations.MapGet("/items/{itemId}", (string itemId, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration item detail"))
        .WithName($"GetLoadArrIntegrationItem{nameSuffix}");

        integrations.MapPost("/items", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration items"))
        .WithName($"CreateLoadArrIntegrationItem{nameSuffix}");

        integrations.MapGet("/location-profiles", (HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration location profiles"))
        .WithName($"ListLoadArrIntegrationLocationProfiles{nameSuffix}");

        integrations.MapGet("/location-profiles/{wmsLocationProfileId}", (string wmsLocationProfileId, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration location profile detail"))
        .WithName($"GetLoadArrIntegrationLocationProfile{nameSuffix}");

        integrations.MapPost("/location-profiles", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration location profiles"))
        .WithName($"CreateLoadArrIntegrationLocationProfile{nameSuffix}");

        integrations.MapGet("/balances", (HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration balances"))
        .WithName($"ListLoadArrIntegrationBalances{nameSuffix}");

        integrations.MapGet("/balances/{balanceId}", (string balanceId, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration balance detail"))
        .WithName($"GetLoadArrIntegrationBalance{nameSuffix}");

        integrations.MapPost("/availability-checks", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration availability checks"))
        .WithName($"CheckLoadArrIntegrationAvailability{nameSuffix}");

        integrations.MapPost("/expected-receipts", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration expected receipts"))
        .WithName($"CreateLoadArrIntegrationExpectedReceipt{nameSuffix}");

        integrations.MapGet("/expected-receipts/{expectedReceiptId}", (string expectedReceiptId, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration expected receipt detail"))
        .WithName($"GetLoadArrIntegrationExpectedReceipt{nameSuffix}");

        integrations.MapPost("/expected-receipts/{expectedReceiptId}/status-updates", (string expectedReceiptId, JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration expected receipt status updates"))
        .WithName($"UpdateLoadArrIntegrationExpectedReceiptStatus{nameSuffix}");

        integrations.MapGet("/stock-movements", (HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration stock movements"))
        .WithName($"ListLoadArrIntegrationStockMovements{nameSuffix}");

        integrations.MapGet("/stock-movements/{movementId}", (string movementId, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration stock movement detail"))
        .WithName($"GetLoadArrIntegrationStockMovement{nameSuffix}");

        integrations.MapPost("/receipts", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration receipts"))
        .WithName($"CreateLoadArrIntegrationReceipt{nameSuffix}");

        integrations.MapGet("/receipts/{receiptId}", (string receiptId, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration receipt detail"))
        .WithName($"GetLoadArrIntegrationReceipt{nameSuffix}");

        integrations.MapPost("/receipts/{receiptId}/lines", (string receiptId, JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration receipt lines"))
        .WithName($"AddLoadArrIntegrationReceiptLine{nameSuffix}");

        integrations.MapPost("/receipts/{receiptId}/close", (string receiptId, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration receipt close"))
        .WithName($"CloseLoadArrIntegrationReceipt{nameSuffix}");

        integrations.MapPost("/putaway-tasks", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration putaway tasks"))
        .WithName($"CreateLoadArrIntegrationPutawayTask{nameSuffix}");

        integrations.MapPost("/putaway-tasks/{putawayTaskId}/complete", (string putawayTaskId, JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration putaway-task completion"))
        .WithName($"CompleteLoadArrIntegrationPutawayTask{nameSuffix}");

        integrations.MapPost("/reservations", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration reservations"))
        .WithName($"CreateLoadArrIntegrationReservation{nameSuffix}");

        integrations.MapGet("/reservations/{reservationId}", (string reservationId, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration reservation detail"))
        .WithName($"GetLoadArrIntegrationReservation{nameSuffix}");

        integrations.MapPost("/reservations/{reservationId}/release", (string reservationId, JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration reservation release"))
        .WithName($"ReleaseLoadArrIntegrationReservation{nameSuffix}");

        integrations.MapPost("/work-order-demands", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration work-order demands"))
        .WithName($"CreateLoadArrIntegrationWorkOrderDemand{nameSuffix}");

        integrations.MapPost("/order-demands", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration order demands"))
        .WithName($"CreateLoadArrIntegrationOrderDemand{nameSuffix}");

        integrations.MapPost("/pick-tasks", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration pick tasks"))
        .WithName($"CreateLoadArrIntegrationPickTask{nameSuffix}");

        integrations.MapPost("/pick-tasks/{pickTaskId}/complete", (string pickTaskId, JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration pick-task completion"))
        .WithName($"CompleteLoadArrIntegrationPickTask{nameSuffix}");

        integrations.MapPost("/issues", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration issues"))
        .WithName($"CreateLoadArrIntegrationIssue{nameSuffix}");

        integrations.MapPost("/returns", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration returns"))
        .WithName($"CreateLoadArrIntegrationReturn{nameSuffix}");

        integrations.MapPost("/transfers", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration transfers"))
        .WithName($"CreateLoadArrIntegrationTransfer{nameSuffix}");

        integrations.MapPost("/counts", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration counts"))
        .WithName($"CreateLoadArrIntegrationCount{nameSuffix}");

        integrations.MapGet("/counts/{countId}", (string countId, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationReadUnavailable(context, authorization, "LoadArr integration count detail"))
        .WithName($"GetLoadArrIntegrationCount{nameSuffix}");

        integrations.MapPost("/counts/{countId}/lines", (string countId, JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration count lines"))
        .WithName($"AddLoadArrIntegrationCountLine{nameSuffix}");

        integrations.MapPost("/counts/{countId}/post", (string countId, JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration count posting"))
        .WithName($"PostLoadArrIntegrationCount{nameSuffix}");

        integrations.MapPost("/adjustments", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration adjustments"))
        .WithName($"CreateLoadArrIntegrationAdjustment{nameSuffix}");

        integrations.MapPost("/discrepancies", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration discrepancies"))
        .WithName($"CreateLoadArrIntegrationDiscrepancy{nameSuffix}");

        integrations.MapPost("/holds", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration holds"))
        .WithName($"CreateLoadArrIntegrationHold{nameSuffix}");

        integrations.MapPost("/hold-releases", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration hold releases"))
        .WithName($"ReleaseLoadArrIntegrationHold{nameSuffix}");

        integrations.MapPost("/disposition-movements", (JsonElement _, HttpContext context, LoadArrAuthorizationService authorization) =>
            IntegrationManageUnavailable(context, authorization, "LoadArr integration disposition movements"))
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
