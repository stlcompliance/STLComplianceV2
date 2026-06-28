using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Integration;

namespace OrdArr.Api.Data;

public sealed class OrdArrStore
{
    private const string IdempotencyOperationKey = "ordarr.store";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly OrdArrDbContext db;
    private readonly object _gate = new();
    private readonly List<OrdArrOrderDetailResponse> _orders;
    private readonly Dictionary<string, string> _idempotencyIndex = new(StringComparer.OrdinalIgnoreCase);

    public OrdArrStore(OrdArrDbContext db)
    {
        this.db = db;
        _orders = db.OrderRecords
            .AsNoTracking()
            .AsEnumerable()
            .Select(record => JsonSerializer.Deserialize<OrdArrOrderDetailResponse>(record.PayloadJson, JsonOptions))
            .Where(order => order is not null)
            .Select(order => order!)
            .ToList();
        _idempotencyIndex = db.IdempotencyRecords
            .AsNoTracking()
            .ToDictionary(record => record.IdempotencyKey, record => record.ResourceId, StringComparer.OrdinalIgnoreCase);
    }

    public OrdArrSessionBootstrapResponse BuildSession(
        string userId,
        string personId,
        string tenantId,
        string tenantRoleKey,
        bool isPlatformAdmin,
        IEnumerable<string> launchableProductKeys) =>
        new(userId, personId, tenantId, $"session-{userId}", tenantRoleKey, isPlatformAdmin, "ordarr", launchableProductKeys.ToArray());

    public OrdArrDashboardResponse GetDashboard(ClaimsPrincipal principal)
    {
        EnsureOrdArrRead(principal);

        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            var orders = GetTenantOrders(principal).ToArray();
            var activeOrders = orders.Where(order => !IsClosedLike(order.LifecycleStatus)).ToArray();
            var blockedOrders = orders.Where(order => IsBlocked(order)).ToArray();
            var lateOrders = orders.Where(order => IsLate(order, now)).ToArray();
            var activity = orders
                .SelectMany(order => order.Timeline.Select(item => new OrdArrRecentActivityResponse(
                    item.TimelineId,
                    order.OrderId,
                    order.OrderNumber,
                    item.EventType,
                    item.Message,
                    item.OccurredAt)))
                .OrderByDescending(item => item.OccurredAt)
                .Take(8)
                .ToArray();

            var response = new OrdArrDashboardResponse(
                now,
                orders.Length,
                orders.Count(order => !string.Equals(order.RequestType, "customer_order", StringComparison.OrdinalIgnoreCase)),
                orders.Sum(order => order.Handoffs.Count(handoff => IsOpenHandoff(handoff.State))),
                orders.Sum(order => order.CompletionPackets.Count(packet => packet.PacketType == "completion")),
                orders.Sum(order => order.CompletionPackets.Count(packet => packet.PacketType == "invoice_ready" && packet.Status == "ready")),
                orders.Sum(order => order.CompletionPackets.Count(packet => packet.PacketType == "bill_ready" && packet.Status == "ready")),
                activeOrders.Length,
                orders.Count(order => order.Holds.Any(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase))),
                blockedOrders.Length,
                lateOrders.Length,
                orders.Sum(order => order.Returns.Count),
                orders.OrderByDescending(order => order.UpdatedAt).Select(ProjectSummary).ToArray(),
                activity)
            {
                OpenOrderCount = activeOrders.Length,
                OpenHoldCount = orders.Sum(order => order.Holds.Count(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase))),
                BlockedOrderCount = blockedOrders.Length,
                LateOrderCount = lateOrders.Length,
                ReturnCount = orders.Sum(order => order.Returns.Count),
            };

            return response;
        }
    }

    public OrdArrReportSummaryResponse GetReportSummary(ClaimsPrincipal principal)
    {
        EnsureOrdArrRead(principal);

        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            var orders = GetTenantOrders(principal).ToArray();
            var openOrders = orders.Count(order => !IsClosedLike(order.LifecycleStatus));
            var closedOrders = orders.Count(order => string.Equals(order.LifecycleStatus, "closed", StringComparison.OrdinalIgnoreCase));
            var blockedOrders = orders.Count(IsBlocked);
            var openHolds = orders.Sum(order => order.Holds.Count(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase)));
            var lateOrders = orders.Count(order => IsLate(order, now));
            var lines = orders.Sum(order => order.Lines.Count);
            var returnedLines = orders.Sum(order => order.Returns.Sum(ret => ret.Quantity));
            var fillRate = orders.Length == 0 ? 0m : Math.Round((decimal)closedOrders / orders.Length * 100m, 1);
            var onTimeRate = orders.Length == 0 ? 0m : Math.Round((decimal)(orders.Length - lateOrders) / orders.Length * 100m, 1);

            return new OrdArrReportSummaryResponse(
                now,
                orders.Length,
                openOrders,
                closedOrders,
                blockedOrders,
                openHolds,
                lateOrders,
                lines,
                returnedLines,
                fillRate,
                onTimeRate,
                orders.Sum(order => order.Handoffs.Count(handoff => IsOpenHandoff(handoff.State))),
                orders.Sum(order => order.Returns.Count),
                orders.Sum(order => order.CompletionPackets.Count(packet => packet.PacketType == "completion" && packet.Status == "ready")),
                orders.Sum(order => order.CompletionPackets.Count(packet => packet.PacketType == "invoice_ready" && packet.Status == "ready")),
                orders.Sum(order => order.CompletionPackets.Count(packet => packet.PacketType == "bill_ready" && packet.Status == "ready")),
                orders.OrderByDescending(order => order.UpdatedAt).Select(ProjectSummary).ToArray());
        }
    }

    public IReadOnlyList<OrdArrOrderSummaryResponse> ListOrders(ClaimsPrincipal principal, string? status = null)
    {
        EnsureOrdArrRead(principal);

        lock (_gate)
        {
            var query = GetTenantOrders(principal);
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(order => string.Equals(order.LifecycleStatus, status.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderByDescending(order => order.UpdatedAt)
                .Select(ProjectSummary)
                .ToArray();
        }
    }

    public OrdArrOrderDetailResponse? GetOrder(ClaimsPrincipal principal, string orderId)
    {
        EnsureOrdArrRead(principal);

        lock (_gate)
        {
            return GetTenantOrder(principal, orderId);
        }
    }

    public IReadOnlyList<OrdArrOrderLineResponse> ListOrderLines(ClaimsPrincipal principal, string orderId)
    {
        var order = GetOrder(principal, orderId);
        return order?.Lines ?? [];
    }

    public IReadOnlyList<OrdArrHoldResponse> ListOrderHolds(ClaimsPrincipal principal, string orderId)
    {
        var order = GetOrder(principal, orderId);
        return order?.Holds ?? [];
    }

    public IReadOnlyList<OrdArrTimelineEntryResponse> ListOrderTimeline(ClaimsPrincipal principal, string orderId)
    {
        var order = GetOrder(principal, orderId);
        return order?.Timeline ?? [];
    }

    public IReadOnlyList<OrdArrReturnResponse> ListOrderReturns(ClaimsPrincipal principal, string orderId)
    {
        var order = GetOrder(principal, orderId);
        return order?.Returns ?? [];
    }

    public OrdArrOrderDetailResponse CreateOrder(
        ClaimsPrincipal principal,
        OrdArrCreateOrderRequest request,
        string? idempotencyKey)
    {
        EnsureOrdArrManage(principal);

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
                return GetTenantOrder(principal, existingId)!;
            }

            var now = DateTimeOffset.UtcNow;
            var next = _orders.Count + 1001;
            var order = new OrdArrOrderDetailResponse(
                $"order-{Guid.NewGuid():N}"[..14],
                $"ORD-{now:yyyy}-{next}",
                NormalizeRequestType(request.RequestType),
                "draft",
                request.CustomerRef,
                request.CustomerName.Trim(),
                string.IsNullOrWhiteSpace(request.OwnerPersonId) ? principal.GetPersonId().ToString() : request.OwnerPersonId.Trim(),
                now,
                now,
                request.RequestedWindowStart,
                request.RequestedWindowEnd,
                request.PromisedWindowStart,
                request.PromisedWindowEnd,
                "not_required",
                "not_started",
                "not_ready",
                request.Summary.Trim(),
                [],
                [new OrdArrCompletionPacketResponse($"packet-{Guid.NewGuid():N}"[..14], "completion", "draft", [])],
                [
                    new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderRequested, "Order/request received by OrdArr.", now),
                    new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderCreated, "Canonical order/request record created.", now),
                ]) with
            {
                SourceChannel = NormalizeSourceChannel(request.SourceChannel),
                OrderType = NormalizeRequiredHeaderValue(request.OrderType, "customer_order"),
                Priority = NormalizePriority(request.Priority),
                BuyerPoNumber = NormalizeHeaderValue(request.BuyerPoNumber, null),
                BillToRef = request.BillToRef,
                ShipToRef = request.ShipToRef,
                ShippingMethodPreference = NormalizeHeaderValue(request.ShippingMethodPreference, null),
                PaymentTerms = NormalizeHeaderValue(request.PaymentTerms, null),
                CustomerNotes = NormalizeHeaderValue(request.CustomerNotes, null),
                InternalNotes = NormalizeHeaderValue(request.InternalNotes, null),
                SourceReference = NormalizeHeaderValue(request.SourceReference, null),
                ApprovalState = "not_submitted",
                CustomerFacingStatus = "draft",
                Lines = (request.Lines ?? []).Select((line, index) => CreateOrderLine($"line-{Guid.NewGuid():N}"[..14], line, index + 1, now)).ToArray(),
                Holds = [],
                Timeline = [new OrdArrTimelineEntryResponse($"tl-{Guid.NewGuid():N}"[..14], "order.created", "draft", "Order/request created in OrdArr.", principal.GetPersonId().ToString(), "ordarr", now)],
                Handoffs = [],
                Returns = [],
                TenantId = principal.GetTenantId().ToString(),
            };

            if (order.Lines.Count > 0)
            {
                order = order with
                {
                    Timeline = order.Timeline.Concat(order.Lines.Select(line => new OrdArrTimelineEntryResponse(
                        $"tl-{Guid.NewGuid():N}"[..14],
                        "order.line.created",
                        order.LifecycleStatus,
                        $"Line {line.LineNumber} added for {order.OrderNumber}.",
                        principal.GetPersonId().ToString(),
                        "ordarr",
                        now))).ToArray(),
                };
            }

            _orders.Insert(0, order);
            PersistOrder(order);
            PersistIdempotency(principal.GetTenantId(), scopedKey, order.OrderId, now);
            return order;
        }
    }

    public OrdArrOrderDetailResponse? SubmitOrder(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrSubmitOrderRequest request,
        string? idempotencyKey)
    {
        EnsureOrdArrManage(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to submit an order/request.", 400);
        }

        lock (_gate)
        {
            var index = FindTenantOrderIndex(principal, orderId);
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.submit|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return GetTenantOrder(principal, existingId)!;
            }

            var order = _orders[index];
            var now = DateTimeOffset.UtcNow;
            var blocked = order.Holds.Any(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase));
            var nextStatus = blocked ? "on_hold" : "submitted";
            var updated = order with
            {
                LifecycleStatus = nextStatus,
                ApprovalState = blocked ? "on_hold" : "pending_approval",
                CustomerFacingStatus = blocked ? "on hold" : "submitted",
                UpdatedAt = now,
                Timeline = order.Timeline.Concat([
                    new OrdArrTimelineEntryResponse(
                        $"tl-{Guid.NewGuid():N}"[..14],
                        "order.submitted",
                        nextStatus,
                        request.Comment ?? "Order/request submitted for review.",
                        principal.GetPersonId().ToString(),
                        "ordarr",
                        now),
                ]).ToArray(),
            };

            _orders[index] = updated;
            PersistOrder(updated);
            PersistIdempotency(principal.GetTenantId(), scopedKey, updated.OrderId, now);
            return updated;
        }
    }

    public OrdArrOrderDetailResponse? AddOrderLine(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrOrderLineRequest request,
        string? idempotencyKey)
    {
        EnsureOrdArrManage(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to add an order line.", 400);
        }

        lock (_gate)
        {
            var index = FindTenantOrderIndex(principal, orderId);
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.line.add|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return GetTenantOrder(principal, existingId)!;
            }

            var order = _orders[index];
            var now = DateTimeOffset.UtcNow;
            var nextLine = CreateOrderLine($"line-{Guid.NewGuid():N}"[..14], request, order.Lines.Count + 1, now);
            var updated = order with
            {
                Lines = order.Lines.Append(nextLine).ToArray(),
                UpdatedAt = now,
                Timeline = order.Timeline.Concat([
                    new OrdArrTimelineEntryResponse(
                        $"tl-{Guid.NewGuid():N}"[..14],
                        "order.line.created",
                        order.LifecycleStatus,
                        $"Line {nextLine.LineNumber} added to {order.OrderNumber}.",
                        principal.GetPersonId().ToString(),
                        "ordarr",
                        now),
                ]).ToArray(),
            };

            _orders[index] = updated;
            PersistOrder(updated);
            PersistIdempotency(principal.GetTenantId(), scopedKey, updated.OrderId, now);
            return updated;
        }
    }

    public OrdArrOrderDetailResponse? AddHold(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrHoldRequest request,
        string? idempotencyKey)
    {
        EnsureOrdArrManage(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to create a hold.", 400);
        }

        lock (_gate)
        {
            var index = FindTenantOrderIndex(principal, orderId);
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.hold.add|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return GetTenantOrder(principal, existingId)!;
            }

            var order = _orders[index];
            var now = DateTimeOffset.UtcNow;
            var hold = new OrdArrHoldResponse(
                $"hold-{Guid.NewGuid():N}"[..14],
                NormalizeRequiredHeaderValue(request.HoldType, "approval"),
                request.Reason.Trim(),
                NormalizeRequiredHeaderValue(request.OwnerProductKey, "ordarr"),
                NormalizeRequiredHeaderValue(request.OwnerPersonId, principal.GetPersonId().ToString()),
                NormalizeRequiredHeaderValue(request.ReleasePermission, "ordarr.order_requests.update"),
                NormalizeHeaderValue(request.Comment, null),
                "open",
                now,
                null,
                null);

            var updated = order with
            {
                Holds = order.Holds.Append(hold).ToArray(),
                LifecycleStatus = "on_hold",
                ApprovalState = "held",
                CustomerFacingStatus = "on hold",
                UpdatedAt = now,
                Timeline = order.Timeline.Concat([
                    new OrdArrTimelineEntryResponse(
                        $"tl-{Guid.NewGuid():N}"[..14],
                        "order.hold.placed",
                        "on_hold",
                        $"{hold.HoldType} hold placed: {hold.Reason}",
                        principal.GetPersonId().ToString(),
                        "ordarr",
                        now),
                ]).ToArray(),
            };

            _orders[index] = updated;
            PersistOrder(updated);
            PersistIdempotency(principal.GetTenantId(), scopedKey, updated.OrderId, now);
            return updated;
        }
    }

    public OrdArrOrderDetailResponse? ReleaseHold(
        ClaimsPrincipal principal,
        string orderId,
        string holdId,
        OrdArrReleaseHoldRequest request,
        string? idempotencyKey)
    {
        EnsureOrdArrManage(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to release a hold.", 400);
        }

        lock (_gate)
        {
            var index = FindTenantOrderIndex(principal, orderId);
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.hold.release|{orderId}|{holdId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return GetTenantOrder(principal, existingId)!;
            }

            var order = _orders[index];
            var now = DateTimeOffset.UtcNow;
            var holds = order.Holds.ToList();
            var holdIndex = holds.FindIndex(hold => string.Equals(hold.HoldId, holdId, StringComparison.OrdinalIgnoreCase));
            if (holdIndex < 0)
            {
                return null;
            }

            holds[holdIndex] = holds[holdIndex] with
            {
                Status = "released",
                ReleasedAt = now,
                ReleasedByPersonId = NormalizeHeaderValue(request.ReleasedByPersonId, principal.GetPersonId().ToString()),
                Comment = NormalizeHeaderValue(request.Comment, holds[holdIndex].Comment),
            };

            var openHolds = holds.Any(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase));
            var nextStatus = openHolds
                ? "on_hold"
                : string.Equals(order.ApprovalState, "approved", StringComparison.OrdinalIgnoreCase)
                    ? "accepted"
                    : "submitted";

            var updated = order with
            {
                Holds = holds.ToArray(),
                LifecycleStatus = nextStatus,
                ApprovalState = openHolds ? "held" : order.ApprovalState,
                CustomerFacingStatus = openHolds ? "on hold" : nextStatus,
                UpdatedAt = now,
                Timeline = order.Timeline.Concat([
                    new OrdArrTimelineEntryResponse(
                        $"tl-{Guid.NewGuid():N}"[..14],
                        "order.hold.released",
                        nextStatus,
                        $"Hold released: {holds[holdIndex].Reason}",
                        principal.GetPersonId().ToString(),
                        "ordarr",
                        now),
                ]).ToArray(),
            };

            _orders[index] = updated;
            PersistOrder(updated);
            PersistIdempotency(principal.GetTenantId(), scopedKey, updated.OrderId, now);
            return updated;
        }
    }

    public OrdArrOrderDetailResponse? AcceptOrder(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrAcceptOrderRequest request,
        string? idempotencyKey)
    {
        EnsureOrdArrManage(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to accept an order/request.", 400);
        }

        lock (_gate)
        {
            var index = FindTenantOrderIndex(principal, orderId);
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.accept|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return GetTenantOrder(principal, existingId)!;
            }

            var order = _orders[index];
            var now = DateTimeOffset.UtcNow;
            var handoffs = BuildFulfillmentHandoffs(order, request.FulfillmentProductKeys, now);
            var updated = order with
            {
                LifecycleStatus = order.Holds.Any(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase)) ? "on_hold" : "accepted",
                ApprovalState = "approved",
                CustomerFacingStatus = order.Holds.Any(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase)) ? "on hold" : "accepted",
                HandoffState = handoffs.Count == 0 ? "not_required" : "requested",
                CompletionState = "in_progress",
                PromisedWindowStart = request.PromisedWindowStart ?? order.PromisedWindowStart,
                PromisedWindowEnd = request.PromisedWindowEnd ?? order.PromisedWindowEnd,
                UpdatedAt = now,
                Handoffs = handoffs,
                Events = order.Events.Concat([
                    new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderAccepted, request.Reason ?? "Order/request approved.", now),
                    new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderFulfillmentRequested, "Downstream fulfillment demand requested through owning products.", now),
                ]).ToArray(),
                Timeline = order.Timeline.Concat([
                    new OrdArrTimelineEntryResponse(
                        $"tl-{Guid.NewGuid():N}"[..14],
                        "order.approved",
                        "accepted",
                        request.Reason ?? "Order/request approved.",
                        principal.GetPersonId().ToString(),
                        "ordarr",
                        now),
                    new OrdArrTimelineEntryResponse(
                        $"tl-{Guid.NewGuid():N}"[..14],
                        "order.fulfillment.requested",
                        "accepted",
                        "Downstream fulfillment demand requested through owning products.",
                        principal.GetPersonId().ToString(),
                        "ordarr",
                        now),
                ]).ToArray(),
            };

            _orders[index] = updated;
            PersistOrder(updated);
            PersistIdempotency(principal.GetTenantId(), scopedKey, updated.OrderId, now);
            return updated;
        }
    }

    public OrdArrOrderDetailResponse? CancelOrder(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrCancelOrderRequest request,
        string? idempotencyKey)
    {
        EnsureOrdArrManage(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to cancel an order/request.", 400);
        }

        lock (_gate)
        {
            var index = FindTenantOrderIndex(principal, orderId);
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.cancel|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return GetTenantOrder(principal, existingId)!;
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
                ApprovalState = "cancelled",
                CustomerFacingStatus = "cancelled",
                HandoffState = handoffs.Any(handoff => handoff.State != "completed") ? "cancelled" : order.HandoffState,
                UpdatedAt = now,
                Handoffs = handoffs,
                Timeline = order.Timeline.Concat([
                    new OrdArrTimelineEntryResponse(
                        $"tl-{Guid.NewGuid():N}"[..14],
                        "order.cancelled",
                        "cancelled",
                        request.Reason.Trim(),
                        principal.GetPersonId().ToString(),
                        "ordarr",
                        now),
                ]).ToArray(),
            };

            _orders[index] = updated;
            PersistOrder(updated);
            PersistIdempotency(principal.GetTenantId(), scopedKey, updated.OrderId, now);
            return updated;
        }
    }

    public OrdArrReturnResponse? CreateReturn(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrReturnRequest request,
        string? idempotencyKey)
    {
        EnsureOrdArrManage(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to create a return.", 400);
        }

        lock (_gate)
        {
            var index = FindTenantOrderIndex(principal, orderId);
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.return.create|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return GetTenantOrder(principal, orderId)?.Returns.SingleOrDefault(ret => ret.ReturnId == existingId);
            }

            var order = _orders[index];
            var now = DateTimeOffset.UtcNow;
            var returnRecord = new OrdArrReturnResponse(
                $"return-{Guid.NewGuid():N}"[..14],
                $"RMA-{now:yyyy}-{order.Returns.Count + 1:0000}",
                NormalizeRequiredHeaderValue(request.ReturnType, "rma"),
                "requested",
                request.Reason.Trim(),
                request.Quantity,
                (request.OrderLineIds ?? []).ToArray(),
                NormalizeHeaderValue(request.Notes, null),
                NormalizeHeaderValue(request.SourceReference, null),
                now,
                now);

            var updated = order with
            {
                Returns = order.Returns.Append(returnRecord).ToArray(),
                UpdatedAt = now,
                Timeline = order.Timeline.Concat([
                    new OrdArrTimelineEntryResponse(
                        $"tl-{Guid.NewGuid():N}"[..14],
                        "order.return.requested",
                        order.LifecycleStatus,
                        $"Return requested: {request.Reason}",
                        principal.GetPersonId().ToString(),
                        "ordarr",
                        now),
                ]).ToArray(),
            };

            _orders[index] = updated;
            PersistOrder(updated);
            PersistIdempotency(principal.GetTenantId(), scopedKey, returnRecord.ReturnId, now);
            return returnRecord;
        }
    }

    public OrdArrOrderDetailResponse? UpsertCompletionPacket(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrCompletionPacketRequest request,
        string? idempotencyKey)
    {
        EnsureOrdArrManage(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to update a completion packet.", 400);
        }

        var packetType = NormalizeCompletionPacketType(request.PacketType);
        var recordRefs = NormalizeCompletionPacketRecordRefs(request.RecordRefs);

        lock (_gate)
        {
            var index = FindTenantOrderIndex(principal, orderId);
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.completion.packet.upsert|{orderId}|{packetType}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return GetTenantOrder(principal, existingId)!;
            }

            var order = _orders[index];
            if (!string.Equals(order.ApprovalState, "approved", StringComparison.OrdinalIgnoreCase))
            {
                throw new StlApiException(
                    "ordarr.completion_packet_requires_approved_order",
                    "Completion packets can only be advanced after the order is approved.",
                    409);
            }

            var now = DateTimeOffset.UtcNow;
            var packets = order.CompletionPackets.ToList();
            var packetIndex = packets.FindIndex(packet => string.Equals(packet.PacketType, packetType, StringComparison.OrdinalIgnoreCase));
            var packet = packetIndex >= 0
                ? packets[packetIndex] with { Status = "ready", RecordRefs = recordRefs }
                : new OrdArrCompletionPacketResponse($"packet-{Guid.NewGuid():N}"[..14], packetType, "ready", recordRefs);

            if (packetIndex >= 0)
            {
                packets[packetIndex] = packet;
            }
            else
            {
                packets.Add(packet);
            }

            var completionState = ResolveCompletionState(order, packets);
            var financialPacketState = ResolveFinancialPacketState(packets);
            var resultingState = packetType == "completion" ? completionState : financialPacketState;
            var eventType = PacketEventType(packetType);
            var message = PacketMessage(packetType);

            var updated = order with
            {
                CompletionPackets = packets.ToArray(),
                CompletionState = completionState,
                FinancialPacketState = financialPacketState,
                UpdatedAt = now,
                Events = order.Events.Concat([
                    new OrdArrEventResponse(
                        $"evt-{Guid.NewGuid():N}"[..12],
                        eventType,
                        message,
                        now),
                ]).ToArray(),
                Timeline = order.Timeline.Concat([
                    new OrdArrTimelineEntryResponse(
                        $"tl-{Guid.NewGuid():N}"[..14],
                        eventType,
                        resultingState,
                        message,
                        principal.GetPersonId().ToString(),
                        "ordarr",
                        now),
                ]).ToArray(),
            };

            _orders[index] = updated;
            PersistOrder(updated);
            PersistIdempotency(principal.GetTenantId(), scopedKey, updated.OrderId, now);
            return updated;
        }
    }

    public IReadOnlyList<OrdArrHandoffResponse> ListHandoffs(ClaimsPrincipal principal)
    {
        EnsureOrdArrRead(principal);

        lock (_gate)
        {
            return GetTenantOrders(principal)
                .SelectMany(order => order.Handoffs.Select(handoff => handoff with { OrderNumber = order.OrderNumber }))
                .OrderByDescending(handoff => handoff.RequestedAt)
                .ToArray();
        }
    }

    public IReadOnlyList<OrdArrCompletionPacketResponse> ListCompletionPackets(ClaimsPrincipal principal)
    {
        EnsureOrdArrRead(principal);

        lock (_gate)
        {
            return GetTenantOrders(principal)
                .SelectMany(order => order.CompletionPackets.Select(packet => packet with { OrderNumber = order.OrderNumber }))
                .OrderBy(packet => packet.PacketId)
                .ToArray();
        }
    }

    public OrdArrReadinessResponse? GetIntegrationReadiness(ClaimsPrincipal principal, string orderId)
    {
        var order = GetOrder(principal, orderId);
        if (order is null)
        {
            return null;
        }

        var blocking = order.Holds
            .Where(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase))
            .Select(hold => $"hold:{hold.HoldType}:{hold.Reason}")
            .Concat(order.Handoffs.Where(handoff => IsOpenHandoff(handoff.State))
                .Select(handoff => $"{handoff.TargetProductKey}:{handoff.HandoffType}:{handoff.State}"))
            .Concat(order.Lines.Where(line => !string.IsNullOrWhiteSpace(line.ComplianceFlag)).Select(line => $"line:{line.LineNumber}:{line.ComplianceFlag}"))
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
                blocking.Length == 0 ? "live" : "blocked",
                DateTimeOffset.UtcNow));
    }

    private void PersistOrder(OrdArrOrderDetailResponse order)
    {
        var tenantId = Guid.Parse(order.TenantId);
        var record = db.OrderRecords.SingleOrDefault(candidate => candidate.OrderId == order.OrderId);
        if (record is null)
        {
            record = new OrdArrOrderRecord
            {
                OrderId = order.OrderId,
                TenantId = tenantId,
                CreatedAt = order.RequestedAt
            };
            db.OrderRecords.Add(record);
        }

        record.TenantId = tenantId;
        record.OrderNumber = order.OrderNumber;
        record.LifecycleStatus = order.LifecycleStatus;
        record.CustomerDisplayName = order.CustomerName;
        record.OwnerPersonId = order.OwnerPersonId;
        record.UpdatedAt = order.UpdatedAt;
        record.PayloadJson = JsonSerializer.Serialize(order, JsonOptions);
        db.SaveChanges();
    }

    private void PersistIdempotency(Guid tenantId, string scopedKey, string resourceId, DateTimeOffset now)
    {
        if (_idempotencyIndex.ContainsKey(scopedKey))
        {
            return;
        }

        db.IdempotencyRecords.Add(new OrdArrIdempotencyRecord
        {
            Id = $"idem-{Guid.NewGuid():N}"[..18],
            TenantId = tenantId,
            OperationKey = IdempotencyOperationKey,
            IdempotencyKey = scopedKey,
            ResourceId = resourceId,
            CreatedAt = now
        });
        db.SaveChanges();
        _idempotencyIndex[scopedKey] = resourceId;
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
            order.Summary)
        {
            TenantId = order.TenantId,
            SourceChannel = order.SourceChannel,
            OrderType = order.OrderType,
            Priority = order.Priority,
            LineCount = order.Lines.Count,
            HoldCount = order.Holds.Count(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase)),
            ApprovalState = order.ApprovalState,
            CustomerFacingStatus = order.CustomerFacingStatus,
            NextAction = DetermineNextAction(order),
        };

    private IEnumerable<OrdArrOrderDetailResponse> GetTenantOrders(ClaimsPrincipal principal)
    {
        var tenantId = principal.GetTenantId().ToString();
        return _orders.Where(order => string.Equals(order.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));
    }

    private OrdArrOrderDetailResponse? GetTenantOrder(ClaimsPrincipal principal, string orderId)
    {
        var tenantId = principal.GetTenantId().ToString();
        return _orders.FirstOrDefault(order =>
            string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(order.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));
    }

    private int FindTenantOrderIndex(ClaimsPrincipal principal, string orderId)
    {
        var tenantId = principal.GetTenantId().ToString();
        return _orders.FindIndex(order =>
            string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(order.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));
    }

    private static string DetermineNextAction(OrdArrOrderDetailResponse order)
    {
        if (order.Holds.Any(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase)))
        {
            return "Release hold";
        }

        if (string.Equals(order.LifecycleStatus, "draft", StringComparison.OrdinalIgnoreCase))
        {
            return "Submit order";
        }

        if (string.Equals(order.LifecycleStatus, "submitted", StringComparison.OrdinalIgnoreCase))
        {
            return "Approve or triage";
        }

        if (order.Handoffs.Any(handoff => IsOpenHandoff(handoff.State)))
        {
            return "Monitor handoff";
        }

        if (string.Equals(order.CompletionState, "in_progress", StringComparison.OrdinalIgnoreCase))
        {
            return "Finalize completion packet";
        }

        if (string.Equals(order.FinancialPacketState, "in_progress", StringComparison.OrdinalIgnoreCase))
        {
            return "Finalize finance packet";
        }

        if (order.Returns.Any(ret => string.Equals(ret.Status, "requested", StringComparison.OrdinalIgnoreCase)))
        {
            return "Review return";
        }

        return "Open detail";
    }

    private static OrdArrOrderLineResponse CreateOrderLine(string orderLineId, OrdArrOrderLineRequest request, int lineNumber, DateTimeOffset now) =>
        new(
            orderLineId,
            lineNumber,
            NormalizeRequiredHeaderValue(request.LineType, "item"),
            request.ItemRef,
            request.Description.Trim(),
            request.Quantity,
            NormalizeRequiredHeaderValue(request.UnitOfMeasure, "ea"),
            request.RequestedDate,
            request.PromisedDate,
            request.UnitPrice,
            request.Discount,
            request.Taxable,
            request.AllowSubstitution,
            request.CanCancel,
            request.CanReturn,
            NormalizeHeaderValue(request.TargetProductKey, null),
            NormalizeHeaderValue(request.ComplianceFlag, null),
            NormalizeHeaderValue(request.LinkedDemandReference, null),
            "open",
            "unallocated",
            now);

    private static bool IsBlocked(OrdArrOrderDetailResponse order) =>
        string.Equals(order.LifecycleStatus, "on_hold", StringComparison.OrdinalIgnoreCase)
        || order.Holds.Any(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase))
        || order.Handoffs.Any(handoff => string.Equals(handoff.State, "blocked", StringComparison.OrdinalIgnoreCase));

    private static bool IsLate(OrdArrOrderDetailResponse order, DateTimeOffset now) =>
        order.PromisedWindowEnd.HasValue && order.PromisedWindowEnd.Value < now && !IsClosedLike(order.LifecycleStatus);

    private static bool IsClosedLike(string lifecycleStatus) =>
        string.Equals(lifecycleStatus, "closed", StringComparison.OrdinalIgnoreCase)
        || string.Equals(lifecycleStatus, "cancelled", StringComparison.OrdinalIgnoreCase)
        || string.Equals(lifecycleStatus, "archived", StringComparison.OrdinalIgnoreCase);

    private static bool IsOpenHandoff(string state) =>
        state is "requested" or "received" or "accepted" or "blocked" or "in_progress" or "waiting_on_source" or "waiting_on_target";

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

    private static string NormalizeCompletionPacketType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "completion" or "invoice_ready" or "bill_ready"
            ? normalized
            : throw new StlApiException(
                "ordarr.completion_packet_type_invalid",
                "Completion packet type must be completion, invoice_ready, or bill_ready.",
                400);
    }

    private static IReadOnlyList<StlProductObjectReference> NormalizeCompletionPacketRecordRefs(IReadOnlyList<StlProductObjectReference>? recordRefs)
    {
        var normalized = (recordRefs ?? [])
            .Select(recordRef => new StlProductObjectReference(
                NormalizeHeaderValue(recordRef.ProductKey, string.Empty)!,
                NormalizeHeaderValue(recordRef.ObjectType, string.Empty)!,
                NormalizeHeaderValue(recordRef.ObjectId, string.Empty)!,
                NormalizeHeaderValue(recordRef.ObjectNumber, null)))
            .Where(recordRef => !string.IsNullOrWhiteSpace(recordRef.ProductKey) && !string.IsNullOrWhiteSpace(recordRef.ObjectType) && !string.IsNullOrWhiteSpace(recordRef.ObjectId))
            .ToArray();

        if (normalized.Any(recordRef => !string.Equals(recordRef.ProductKey, "recordarr", StringComparison.OrdinalIgnoreCase)))
        {
            throw new StlApiException(
                "ordarr.completion_packet_record_ref_invalid",
                "Completion packet record references must point to RecordArr evidence.",
                400);
        }

        return normalized;
    }

    private static string ResolveCompletionState(OrdArrOrderDetailResponse order, IReadOnlyList<OrdArrCompletionPacketResponse> packets)
    {
        if (packets.Any(packet => string.Equals(packet.PacketType, "completion", StringComparison.OrdinalIgnoreCase) && string.Equals(packet.Status, "ready", StringComparison.OrdinalIgnoreCase)))
        {
            return "ready";
        }

        return string.Equals(order.ApprovalState, "approved", StringComparison.OrdinalIgnoreCase)
            ? "in_progress"
            : "not_started";
    }

    private static string ResolveFinancialPacketState(IReadOnlyList<OrdArrCompletionPacketResponse> packets)
    {
        var invoiceReady = packets.Any(packet => string.Equals(packet.PacketType, "invoice_ready", StringComparison.OrdinalIgnoreCase) && string.Equals(packet.Status, "ready", StringComparison.OrdinalIgnoreCase));
        var billReady = packets.Any(packet => string.Equals(packet.PacketType, "bill_ready", StringComparison.OrdinalIgnoreCase) && string.Equals(packet.Status, "ready", StringComparison.OrdinalIgnoreCase));

        if (invoiceReady && billReady)
        {
            return "ready";
        }

        return invoiceReady || billReady
            ? "in_progress"
            : "not_ready";
    }

    private static string PacketEventType(string packetType) =>
        packetType switch
        {
            "completion" => StlSuiteEventCatalog.OrdArr.OrderChanged,
            "invoice_ready" => StlSuiteEventCatalog.OrdArr.OrderChanged,
            "bill_ready" => StlSuiteEventCatalog.OrdArr.OrderChanged,
            _ => StlSuiteEventCatalog.OrdArr.OrderChanged,
        };

    private static string PacketMessage(string packetType) =>
        packetType switch
        {
            "completion" => "Completion packet marked ready.",
            "invoice_ready" => "Invoice-ready packet marked ready.",
            "bill_ready" => "Bill-ready packet marked ready.",
            _ => "Completion packet updated.",
        };

    private static string NormalizePriority(string? value)
    {
        var normalized = NormalizeHeaderValue(value, "normal")!;
        return normalized is "low" or "normal" or "high" or "urgent" or "emergency" ? normalized : "normal";
    }

    private static string NormalizeSourceChannel(string? value)
    {
        var normalized = NormalizeHeaderValue(value, "manual_entry")!;
        return normalized is "manual_entry" or "customer_portal" or "internal_portal" or "api" or "import" or "integration" or "product_handoff"
            ? normalized
            : "manual_entry";
    }

    private static string? NormalizeHeaderValue(string? value, string? fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var trimmed = value.Trim();
        return trimmed.Length == 0 ? fallback : trimmed;
    }

    private static string NormalizeRequiredHeaderValue(string? value, string fallback) =>
        NormalizeHeaderValue(value, fallback) ?? fallback;

    private static void EnsureOrdArrRead(ClaimsPrincipal principal)
    {
        _ = principal.GetTenantId();

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "ordarr_admin", "ordarr_manager", "ordarr-ops"))
        {
            return;
        }

        throw new StlApiException(
            "ordarr.forbidden",
            "OrdArr read access requires OrdArr operations or tenant admin access.",
            403);
    }

    private static void EnsureOrdArrManage(ClaimsPrincipal principal)
    {
        _ = principal.GetTenantId();

        if (MatchesRole(principal.GetTenantRoleKey(), "tenant_admin", "ordarr_admin", "ordarr_manager", "ordarr-ops"))
        {
            return;
        }

        throw new StlApiException(
            "ordarr.forbidden",
            "OrdArr changes require OrdArr operations or tenant admin access.",
            403);
    }

    private static bool MatchesRole(string roleKey, params string[] expectedRoleKeys) =>
        expectedRoleKeys.Any(expected => string.Equals(roleKey, expected, StringComparison.OrdinalIgnoreCase));

}

public sealed record OrdArrSessionBootstrapResponse(
    string UserId,
    string PersonId,
    string TenantId,
    string SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    IReadOnlyList<string> LaunchableProductKeys);

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
    IReadOnlyList<string> LaunchableProductKeys,
    string ThemePreference,
    string? CallbackUrl);

public sealed record OrdArrDashboardResponse(
    DateTimeOffset GeneratedAt,
    int OrderCount,
    int RequestCount,
    int ActiveHandoffCount,
    int CompletionPacketCount,
    int InvoiceReadyPacketCount,
    int BillReadyPacketCount,
    int OpenOrderCount,
    int OpenHoldCount,
    int BlockedOrderCount,
    int LateOrderCount,
    int ReturnCount,
    IReadOnlyList<OrdArrOrderSummaryResponse> FeaturedOrders,
    IReadOnlyList<OrdArrRecentActivityResponse> RecentActivity)
{
}

public sealed record OrdArrReportSummaryResponse(
    DateTimeOffset GeneratedAt,
    int OrderCount,
    int OpenOrderCount,
    int ClosedOrderCount,
    int BlockedOrderCount,
    int OpenHoldCount,
    int LateOrderCount,
    int LineCount,
    decimal ReturnedQuantity,
    decimal FillRatePercent,
    decimal OnTimePercent,
    int ActiveHandoffCount,
    int ReturnCount,
    int CompletionPacketCount,
    int InvoiceReadyPacketCount,
    int BillReadyPacketCount,
    IReadOnlyList<OrdArrOrderSummaryResponse> FeaturedOrders);

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
    string Summary)
{
    public string TenantId { get; init; } = string.Empty;
    public string SourceChannel { get; init; } = "manual_entry";
    public string OrderType { get; init; } = "customer_order";
    public string Priority { get; init; } = "normal";
    public int LineCount { get; init; }
    public int HoldCount { get; init; }
    public string ApprovalState { get; init; } = "not_submitted";
    public string CustomerFacingStatus { get; init; } = "draft";
    public string NextAction { get; init; } = "Open detail";
}

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
    IReadOnlyList<OrdArrEventResponse> Events)
{
    public string TenantId { get; init; } = string.Empty;
    public string SourceChannel { get; init; } = "manual_entry";
    public string OrderType { get; init; } = "customer_order";
    public string Priority { get; init; } = "normal";
    public string? BuyerPoNumber { get; init; }
    public StlProductObjectReference? BillToRef { get; init; }
    public StlProductObjectReference? ShipToRef { get; init; }
    public string? ShippingMethodPreference { get; init; }
    public string? PaymentTerms { get; init; }
    public string? CustomerNotes { get; init; }
    public string? InternalNotes { get; init; }
    public string? SourceReference { get; init; }
    public string ApprovalState { get; init; } = "not_submitted";
    public string CustomerFacingStatus { get; init; } = "draft";
    public IReadOnlyList<OrdArrOrderLineResponse> Lines { get; init; } = [];
    public IReadOnlyList<OrdArrHoldResponse> Holds { get; init; } = [];
    public IReadOnlyList<OrdArrTimelineEntryResponse> Timeline { get; init; } = [];
    public IReadOnlyList<OrdArrReturnResponse> Returns { get; init; } = [];
}

public sealed record OrdArrOrderLineResponse(
    string OrderLineId,
    int LineNumber,
    string LineType,
    StlProductObjectReference? ItemRef,
    string Description,
    decimal Quantity,
    string UnitOfMeasure,
    DateTimeOffset? RequestedDate,
    DateTimeOffset? PromisedDate,
    decimal UnitPrice,
    decimal Discount,
    bool Taxable,
    bool AllowSubstitution,
    bool CanCancel,
    bool CanReturn,
    string? TargetProductKey,
    string? ComplianceFlag,
    string? LinkedDemandReference,
    string FulfillmentStatus,
    string AllocationStatus,
    DateTimeOffset CreatedAt);

public sealed record OrdArrHoldResponse(
    string HoldId,
    string HoldType,
    string Reason,
    string OwnerProductKey,
    string OwnerPersonId,
    string ReleasePermission,
    string? Comment,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReleasedAt,
    string? ReleasedByPersonId);

public sealed record OrdArrTimelineEntryResponse(
    string TimelineId,
    string EventType,
    string Status,
    string Message,
    string ActorPersonId,
    string SourceProductKey,
    DateTimeOffset OccurredAt);

public sealed record OrdArrCompletionPacketResponse(
    string PacketId,
    string PacketType,
    string Status,
    IReadOnlyList<StlProductObjectReference> RecordRefs)
{
    public string? OrderNumber { get; init; }
}

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

public sealed record OrdArrEventResponse(
    string EventId,
    string EventType,
    string Message,
    DateTimeOffset OccurredAt);

public sealed record OrdArrReturnResponse(
    string ReturnId,
    string ReturnNumber,
    string ReturnType,
    string Status,
    string Reason,
    decimal Quantity,
    IReadOnlyList<string> OrderLineIds,
    string? Notes,
    string? SourceReference,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

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
    IReadOnlyList<string>? FulfillmentProductKeys = null,
    string SourceChannel = "manual_entry",
    string OrderType = "customer_order",
    string Priority = "normal",
    string? BuyerPoNumber = null,
    StlProductObjectReference? BillToRef = null,
    StlProductObjectReference? ShipToRef = null,
    string? ShippingMethodPreference = null,
    string? PaymentTerms = null,
    string? CustomerNotes = null,
    string? InternalNotes = null,
    string? SourceReference = null,
    IReadOnlyList<OrdArrOrderLineRequest>? Lines = null);

public sealed record OrdArrOrderLineRequest(
    string LineType,
    StlProductObjectReference? ItemRef,
    string Description,
    decimal Quantity,
    string UnitOfMeasure,
    string? TargetProductKey = null,
    DateTimeOffset? RequestedDate = null,
    DateTimeOffset? PromisedDate = null,
    decimal UnitPrice = 0m,
    decimal Discount = 0m,
    bool Taxable = true,
    bool AllowSubstitution = true,
    bool CanCancel = true,
    bool CanReturn = true,
    string? ComplianceFlag = null,
    string? LinkedDemandReference = null);

public sealed record OrdArrSubmitOrderRequest(string? Comment = null);

public sealed record OrdArrHoldRequest(
    string HoldType,
    string Reason,
    string OwnerProductKey,
    string? ReleasePermission = null,
    string? Comment = null,
    string? OwnerPersonId = null);

public sealed record OrdArrReleaseHoldRequest(
    string? Comment = null,
    string? ReleasedByPersonId = null);

public sealed record OrdArrAcceptOrderRequest(
    DateTimeOffset? PromisedWindowStart,
    DateTimeOffset? PromisedWindowEnd,
    IReadOnlyList<string>? FulfillmentProductKeys,
    string? Reason);

public sealed record OrdArrCancelOrderRequest(string Reason);

public sealed record OrdArrReturnRequest(
    string ReturnType,
    string Reason,
    decimal Quantity,
    IReadOnlyList<string>? OrderLineIds = null,
    string? Notes = null,
    string? SourceReference = null);

public sealed record OrdArrCompletionPacketRequest(
    string PacketType,
    IReadOnlyList<StlProductObjectReference>? RecordRefs = null);

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
