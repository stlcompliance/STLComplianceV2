using System.Security.Claims;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace OrdArr.Api.Data;

public sealed class OrdArrStore
{
    private readonly object _gate = new();
    private readonly List<OrdArrOrderDetailResponse> _orders;
    private readonly Dictionary<string, string> _idempotencyIndex = new(StringComparer.OrdinalIgnoreCase);

    public OrdArrStore()
    {
        _orders = [];
    }

    public OrdArrSessionBootstrapResponse BuildSession(
        string userId,
        string personId,
        string tenantId,
        string tenantRoleKey,
        bool isPlatformAdmin,
        IEnumerable<string> entitlements) =>
        new(userId, personId, tenantId, $"session-{userId}", tenantRoleKey, isPlatformAdmin, "ordarr", true, entitlements.ToArray());

    public OrdArrDashboardResponse GetDashboard(ClaimsPrincipal principal)
    {
        EnsureEntitled(principal);

        lock (_gate)
        {
            var orders = _orders.ToArray();
            var activity = orders
                .SelectMany(order => order.Events.Select(item => new OrdArrRecentActivityResponse(
                    item.EventId,
                    order.OrderId,
                    order.OrderNumber,
                    item.EventType,
                    item.Message,
                    item.OccurredAt)))
                .OrderByDescending(item => item.OccurredAt)
                .Take(8)
                .ToArray();

            return new OrdArrDashboardResponse(
                DateTimeOffset.UtcNow,
                orders.Length,
                orders.Count(order => string.Equals(order.RequestType, "service_request", StringComparison.OrdinalIgnoreCase)),
                orders.Sum(order => order.Handoffs.Count(handoff => handoff.State is "pending" or "accepted" or "blocked")),
                orders.Sum(order => order.CompletionPackets.Count(packet => packet.PacketType == "completion")),
                orders.Sum(order => order.CompletionPackets.Count(packet => packet.PacketType == "invoice_ready" && packet.Status == "ready")),
                orders.Sum(order => order.CompletionPackets.Count(packet => packet.PacketType == "bill_ready" && packet.Status == "ready")),
                orders.OrderBy(order => order.RequestedAt).Select(ProjectSummary).ToArray(),
                activity);
        }
    }

    public IReadOnlyList<OrdArrOrderSummaryResponse> ListOrders(ClaimsPrincipal principal, string? status = null)
    {
        EnsureEntitled(principal);

        lock (_gate)
        {
            var query = _orders.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(order => string.Equals(order.LifecycleStatus, status.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderBy(order => order.OrderNumber)
                .Select(ProjectSummary)
                .ToArray();
        }
    }

    public OrdArrOrderDetailResponse? GetOrder(ClaimsPrincipal principal, string orderId)
    {
        EnsureEntitled(principal);

        lock (_gate)
        {
            return _orders.FirstOrDefault(order => string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public OrdArrOrderDetailResponse CreateOrder(
        ClaimsPrincipal principal,
        OrdArrCreateOrderRequest request,
        string? idempotencyKey)
    {
        EnsureEntitled(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to create an order/request.", 400);
        }

        if (request.CustomerRef is null || !string.Equals(request.CustomerRef.ProductKey, "customarr", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("ordarr.customer_ref_required", "OrdArr order/request creation requires a CustomArr customer reference.", 400);
        }

        lock (_gate)
        {
            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.create|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return _orders.Single(order => order.OrderId == existingId);
            }

            var next = _orders.Count + 1001;
            var now = DateTimeOffset.UtcNow;
            var order = new OrdArrOrderDetailResponse(
                $"order-{Guid.NewGuid():N}"[..14],
                $"ORD-{now:yyyy}-{next}",
                NormalizeRequestType(request.RequestType),
                "intake",
                request.CustomerRef,
                request.CustomerName.Trim(),
                string.IsNullOrWhiteSpace(request.OwnerPersonId) ? principal.GetPersonId().ToString() : request.OwnerPersonId.Trim(),
                now,
                now,
                request.RequestedWindowStart,
                request.RequestedWindowEnd,
                request.PromisedWindowStart,
                request.PromisedWindowEnd,
                "not_started",
                "not_started",
                "not_ready",
                request.Summary.Trim(),
                [],
                [new OrdArrCompletionPacketResponse($"packet-{Guid.NewGuid():N}"[..14], "completion", "not_started", [])],
                [
                    new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderRequested, "Order/request created in OrdArr.", now),
                    new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderCreated, "Canonical order/request record created.", now),
                ]);

            _orders.Insert(0, order);
            _idempotencyIndex[scopedKey] = order.OrderId;
            return order;
        }
    }

    public IReadOnlyList<OrdArrHandoffResponse> ListHandoffs(ClaimsPrincipal principal)
    {
        EnsureEntitled(principal);

        lock (_gate)
        {
            return _orders
                .SelectMany(order => order.Handoffs.Select(handoff => handoff with { OrderNumber = order.OrderNumber }))
                .OrderByDescending(handoff => handoff.RequestedAt)
                .ToArray();
        }
    }

    public IReadOnlyList<OrdArrCompletionPacketResponse> ListCompletionPackets(ClaimsPrincipal principal)
    {
        EnsureEntitled(principal);

        lock (_gate)
        {
            return _orders
                .SelectMany(order => order.CompletionPackets.Select(packet => packet with { OrderNumber = order.OrderNumber }))
                .OrderBy(packet => packet.PacketId)
                .ToArray();
        }
    }

    public OrdArrOrderDetailResponse? AcceptOrder(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrAcceptOrderRequest request,
        string? idempotencyKey)
    {
        EnsureEntitled(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to accept an order/request.", 400);
        }

        lock (_gate)
        {
            var index = _orders.FindIndex(order => string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.accept|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return _orders.Single(order => order.OrderId == existingId);
            }

            var order = _orders[index];
            var now = DateTimeOffset.UtcNow;
            var handoffs = BuildFulfillmentHandoffs(order, request.FulfillmentProductKeys, now);
            var updated = order with
            {
                LifecycleStatus = "accepted",
                HandoffState = handoffs.Count == 0 ? "not_required" : "requested",
                CompletionState = "in_progress",
                PromisedWindowStart = request.PromisedWindowStart ?? order.PromisedWindowStart,
                PromisedWindowEnd = request.PromisedWindowEnd ?? order.PromisedWindowEnd,
                UpdatedAt = now,
                Handoffs = handoffs,
                Events = order.Events.Concat([
                    new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderAccepted, "Order/request accepted.", now),
                    new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderFulfillmentRequested, "Downstream fulfillment demand requested through owning products.", now),
                ]).ToArray(),
            };

            _orders[index] = updated;
            _idempotencyIndex[scopedKey] = updated.OrderId;
            return updated;
        }
    }

    public OrdArrOrderDetailResponse? CancelOrder(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrCancelOrderRequest request,
        string? idempotencyKey)
    {
        EnsureEntitled(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to cancel an order/request.", 400);
        }

        lock (_gate)
        {
            var index = _orders.FindIndex(order => string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.cancel|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return _orders.Single(order => order.OrderId == existingId);
            }

            var order = _orders[index];
            var now = DateTimeOffset.UtcNow;
            var handoffs = order.Handoffs
                .Select(handoff => string.Equals(handoff.State, "completed", StringComparison.OrdinalIgnoreCase)
                    ? handoff
                    : handoff with { State = "cancelled", Summary = $"{handoff.Summary} Cancel reason: {request.Reason}" })
                .ToArray();
            var updated = order with
            {
                LifecycleStatus = "cancelled",
                HandoffState = handoffs.Any(handoff => handoff.State != "completed") ? "cancelled" : order.HandoffState,
                UpdatedAt = now,
                Handoffs = handoffs,
                Events = order.Events.Concat([
                    new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderCancelRequested, "Order cancellation requested.", now),
                    new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderCancelled, "Order cancelled by OrdArr. Completed downstream work was preserved.", now),
                ]).ToArray(),
            };

            _orders[index] = updated;
            _idempotencyIndex[scopedKey] = updated.OrderId;
            return updated;
        }
    }

    public OrdArrReadinessResponse? GetIntegrationReadiness(ClaimsPrincipal principal, string orderId)
    {
        var order = GetOrder(principal, orderId);
        if (order is null)
        {
            return null;
        }

        var blocking = order.Handoffs
            .Where(handoff => handoff.State is "pending" or "blocked" or "rejected")
            .Select(handoff => $"{handoff.TargetProductKey}:{handoff.HandoffType}:{handoff.State}")
            .ToArray();

        return new OrdArrReadinessResponse(
            order.OrderId,
            order.OrderNumber,
            blocking.Length == 0,
            order.LifecycleStatus,
            order.HandoffState,
            order.CompletionState,
            order.FinancialPacketState,
            blocking,
            new OrdArrMetadataResponse(
                principal.GetTenantId().ToString(),
                "ordarr",
                "order",
                order.OrderId,
                "live",
                DateTimeOffset.UtcNow));
    }

    private static OrdArrOrderSummaryResponse ProjectSummary(OrdArrOrderDetailResponse order) =>
        new(
            order.OrderId,
            order.OrderNumber,
            order.RequestType,
            order.LifecycleStatus,
            order.CustomerRef,
            order.CustomerName,
            order.OwnerPersonId,
            order.RequestedAt,
            order.UpdatedAt,
            order.RequestedWindowStart,
            order.RequestedWindowEnd,
            order.PromisedWindowStart,
            order.PromisedWindowEnd,
            order.HandoffState,
            order.CompletionState,
            order.FinancialPacketState,
            order.Summary);

    private static IReadOnlyList<OrdArrHandoffResponse> BuildFulfillmentHandoffs(
        OrdArrOrderDetailResponse order,
        IReadOnlyList<string>? requestedProductKeys,
        DateTimeOffset requestedAt)
    {
        var productKeys = requestedProductKeys is { Count: > 0 }
            ? requestedProductKeys
            : ["routarr", "loadarr"];

        return productKeys
            .Select(productKey => productKey.Trim().ToLowerInvariant())
            .Where(productKey => productKey is "routarr" or "loadarr" or "supplyarr" or "maintainarr" or "assurarr" or "trainarr" or "recordarr")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(productKey => new OrdArrHandoffResponse(
                $"handoff-{Guid.NewGuid():N}"[..16],
                productKey,
                FulfillmentHandoffType(productKey),
                "requested",
                $"OrdArr requested {productKey} fulfillment demand for {order.OrderNumber}.",
                requestedAt))
            .ToArray();
    }

    private static string FulfillmentHandoffType(string productKey) =>
        productKey switch
        {
            "routarr" => "transport_demand",
            "loadarr" => "warehouse_or_dock_demand",
            "supplyarr" => "procurement_or_vendor_follow_up",
            "maintainarr" => "asset_readiness_or_maintenance_demand",
            "assurarr" => "quality_review_demand",
            "trainarr" => "training_or_qualification_demand",
            "recordarr" => "document_or_evidence_link",
            _ => "fulfillment_demand",
        };

    private static string NormalizeRequestType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "customer_order" or "service_request" or "internal_request"
            ? normalized
            : "customer_order";
    }

    private static void EnsureEntitled(ClaimsPrincipal principal)
    {
        if (!principal.HasProductEntitlement("ordarr"))
        {
            throw new StlApiException("ordarr.not_entitled", "Active OrdArr entitlement is required.", 403);
        }
    }
}

public sealed record OrdArrSessionBootstrapResponse(
    string UserId,
    string PersonId,
    string TenantId,
    string SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    bool HasOrdArrEntitlement,
    IReadOnlyList<string> Entitlements);

public sealed record OrdArrHandoffSessionResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string UserId,
    string PersonId,
    string Email,
    string DisplayName,
    string TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> Entitlements,
    string? CallbackUrl);

public sealed record OrdArrDashboardResponse(
    DateTimeOffset GeneratedAt,
    int OrderCount,
    int RequestCount,
    int ActiveHandoffCount,
    int CompletionPacketCount,
    int InvoiceReadyPacketCount,
    int BillReadyPacketCount,
    IReadOnlyList<OrdArrOrderSummaryResponse> FeaturedOrders,
    IReadOnlyList<OrdArrRecentActivityResponse> RecentActivity);

public sealed record OrdArrRecentActivityResponse(
    string ActivityId,
    string OrderId,
    string OrderNumber,
    string EventType,
    string Message,
    DateTimeOffset OccurredAt);

public sealed record OrdArrOrderSummaryResponse(
    string OrderId,
    string OrderNumber,
    string RequestType,
    string LifecycleStatus,
    StlProductObjectReference CustomerRef,
    string CustomerName,
    string OwnerPersonId,
    DateTimeOffset RequestedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? RequestedWindowStart,
    DateTimeOffset? RequestedWindowEnd,
    DateTimeOffset? PromisedWindowStart,
    DateTimeOffset? PromisedWindowEnd,
    string HandoffState,
    string CompletionState,
    string FinancialPacketState,
    string Summary);

public sealed record OrdArrOrderDetailResponse(
    string OrderId,
    string OrderNumber,
    string RequestType,
    string LifecycleStatus,
    StlProductObjectReference CustomerRef,
    string CustomerName,
    string OwnerPersonId,
    DateTimeOffset RequestedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? RequestedWindowStart,
    DateTimeOffset? RequestedWindowEnd,
    DateTimeOffset? PromisedWindowStart,
    DateTimeOffset? PromisedWindowEnd,
    string HandoffState,
    string CompletionState,
    string FinancialPacketState,
    string Summary,
    IReadOnlyList<OrdArrHandoffResponse> Handoffs,
    IReadOnlyList<OrdArrCompletionPacketResponse> CompletionPackets,
    IReadOnlyList<OrdArrEventResponse> Events);

public sealed record OrdArrHandoffResponse(
    string HandoffId,
    string TargetProductKey,
    string HandoffType,
    string State,
    string Summary,
    DateTimeOffset RequestedAt)
{
    public string? OrderNumber { get; init; }
}

public sealed record OrdArrCompletionPacketResponse(
    string PacketId,
    string PacketType,
    string Status,
    IReadOnlyList<StlProductObjectReference> RecordRefs)
{
    public string? OrderNumber { get; init; }
}

public sealed record OrdArrEventResponse(
    string EventId,
    string EventType,
    string Message,
    DateTimeOffset OccurredAt);

public sealed record OrdArrCreateOrderRequest(
    StlProductObjectReference CustomerRef,
    string CustomerName,
    string RequestType,
    string OwnerPersonId,
    string Summary,
    DateTimeOffset? RequestedWindowStart = null,
    DateTimeOffset? RequestedWindowEnd = null,
    DateTimeOffset? PromisedWindowStart = null,
    DateTimeOffset? PromisedWindowEnd = null,
    IReadOnlyList<string>? FulfillmentProductKeys = null);

public sealed record OrdArrAcceptOrderRequest(
    DateTimeOffset? PromisedWindowStart,
    DateTimeOffset? PromisedWindowEnd,
    IReadOnlyList<string>? FulfillmentProductKeys,
    string? Reason);

public sealed record OrdArrCancelOrderRequest(string Reason);

public sealed record OrdArrReadinessResponse(
    string OrderId,
    string OrderNumber,
    bool IsReady,
    string LifecycleStatus,
    string HandoffState,
    string CompletionState,
    string FinancialPacketState,
    IReadOnlyList<string> BlockingReasons,
    OrdArrMetadataResponse Meta);

public sealed record OrdArrMetadataResponse(
    string TenantId,
    string SourceProduct,
    string ResourceType,
    string ResourceId,
    string Freshness,
    DateTimeOffset FetchedAt);
