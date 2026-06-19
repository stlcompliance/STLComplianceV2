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
        SeedDemoData();
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
            var now = DateTimeOffset.UtcNow;
            var orders = _orders.ToArray();
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
        EnsureEntitled(principal);

        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            var orders = _orders.ToArray();
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
        EnsureEntitled(principal);

        lock (_gate)
        {
            var query = _orders.AsEnumerable();
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
        EnsureEntitled(principal);

        lock (_gate)
        {
            return _orders.FirstOrDefault(order => string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase));
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
                [new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderCreated, "Canonical order/request record created.", now)]) with
            {
                SourceChannel = NormalizeSourceChannel(request.SourceChannel),
                OrderType = NormalizeHeaderValue(request.OrderType, "customer_order"),
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
            _idempotencyIndex[scopedKey] = order.OrderId;
            return order;
        }
    }

    public OrdArrOrderDetailResponse? SubmitOrder(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrSubmitOrderRequest request,
        string? idempotencyKey)
    {
        EnsureEntitled(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to submit an order/request.", 400);
        }

        lock (_gate)
        {
            var index = _orders.FindIndex(order => string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.submit|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return _orders.Single(order => order.OrderId == existingId);
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
            _idempotencyIndex[scopedKey] = updated.OrderId;
            return updated;
        }
    }

    public OrdArrOrderDetailResponse? AddOrderLine(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrOrderLineRequest request,
        string? idempotencyKey)
    {
        EnsureEntitled(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to add an order line.", 400);
        }

        lock (_gate)
        {
            var index = _orders.FindIndex(order => string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.line.add|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return _orders.Single(order => order.OrderId == existingId);
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
            _idempotencyIndex[scopedKey] = updated.OrderId;
            return updated;
        }
    }

    public OrdArrOrderDetailResponse? AddHold(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrHoldRequest request,
        string? idempotencyKey)
    {
        EnsureEntitled(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to create a hold.", 400);
        }

        lock (_gate)
        {
            var index = _orders.FindIndex(order => string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.hold.add|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return _orders.Single(order => order.OrderId == existingId);
            }

            var order = _orders[index];
            var now = DateTimeOffset.UtcNow;
            var hold = new OrdArrHoldResponse(
                $"hold-{Guid.NewGuid():N}"[..14],
                NormalizeHeaderValue(request.HoldType, "approval"),
                request.Reason.Trim(),
                NormalizeHeaderValue(request.OwnerProductKey, "ordarr"),
                NormalizeHeaderValue(request.OwnerPersonId, principal.GetPersonId().ToString()),
                NormalizeHeaderValue(request.ReleasePermission, "ordarr.order_requests.update"),
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
            _idempotencyIndex[scopedKey] = updated.OrderId;
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
        EnsureEntitled(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to release a hold.", 400);
        }

        lock (_gate)
        {
            var index = _orders.FindIndex(order => string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.hold.release|{orderId}|{holdId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return _orders.Single(order => order.OrderId == existingId);
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
            _idempotencyIndex[scopedKey] = updated.OrderId;
            return updated;
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
                LifecycleStatus = order.Holds.Any(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase)) ? "on_hold" : "accepted",
                ApprovalState = "approved",
                CustomerFacingStatus = order.Holds.Any(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase)) ? "on hold" : "accepted",
                HandoffState = handoffs.Count == 0 ? "not_required" : "requested",
                CompletionState = "in_progress",
                PromisedWindowStart = request.PromisedWindowStart ?? order.PromisedWindowStart,
                PromisedWindowEnd = request.PromisedWindowEnd ?? order.PromisedWindowEnd,
                UpdatedAt = now,
                Handoffs = handoffs,
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
            _idempotencyIndex[scopedKey] = updated.OrderId;
            return updated;
        }
    }

    public OrdArrReturnResponse? CreateReturn(
        ClaimsPrincipal principal,
        string orderId,
        OrdArrReturnRequest request,
        string? idempotencyKey)
    {
        EnsureEntitled(principal);

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new StlApiException("ordarr.idempotency_key_required", "Idempotency-Key header is required to create a return.", 400);
        }

        lock (_gate)
        {
            var index = _orders.FindIndex(order => string.Equals(order.OrderId, orderId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            var scopedKey = $"{principal.GetTenantId()}|ordarr.order.return.create|{orderId}|{idempotencyKey.Trim()}";
            if (_idempotencyIndex.TryGetValue(scopedKey, out var existingId))
            {
                return _orders[index].Returns.SingleOrDefault(ret => ret.ReturnId == existingId);
            }

            var order = _orders[index];
            var now = DateTimeOffset.UtcNow;
            var returnRecord = new OrdArrReturnResponse(
                $"return-{Guid.NewGuid():N}"[..14],
                $"RMA-{now:yyyy}-{order.Returns.Count + 1:0000}",
                NormalizeHeaderValue(request.ReturnType, "rma"),
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
            _idempotencyIndex[scopedKey] = returnRecord.ReturnId;
            return returnRecord;
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

    private void SeedDemoData()
    {
        lock (_gate)
        {
            if (_orders.Count > 0)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;

            var draftOrder = BuildSeedOrder(
                orderId: "order-seed-1001",
                orderNumber: "ORD-2026-1001",
                requestType: "customer_order",
                lifecycleStatus: "draft",
                customerRef: new StlProductObjectReference("customarr", "customer", "cust-1001", "CUST-1001"),
                customerName: "Northwind Maintenance",
                ownerPersonId: "person-order-ops",
                summary: "Replacement pump kit and consumables for next-day customer fulfillment.",
                now,
                sourceChannel: "customer_portal",
                priority: "high",
                orderType: "customer_order",
                lines:
                [
                    CreateOrderLine("line-seed-1001-1", new OrdArrOrderLineRequest(
                        "item",
                        new StlProductObjectReference("supplyarr", "part", "part-pump-kit", "PK-1001"),
                        "Replacement pump kit",
                        2,
                        "ea",
                        "loadarr",
                        now.AddDays(1),
                        now.AddDays(1),
                        1200m,
                        0m,
                        true,
                        true,
                        true,
                        true,
                        "none",
                        "loadarr.fulfillment"), 1, now),
                ]);

            var acceptedOrder = BuildSeedOrder(
                orderId: "order-seed-1002",
                orderNumber: "ORD-2026-1002",
                requestType: "service_request",
                lifecycleStatus: "accepted",
                customerRef: new StlProductObjectReference("customarr", "customer", "cust-2001", "CUST-2001"),
                customerName: "Apex Foods",
                ownerPersonId: "person-service-coord",
                summary: "On-site refrigeration inspection and follow-up service task.",
                now,
                sourceChannel: "internal_portal",
                priority: "normal",
                orderType: "service_request",
                lines:
                [
                    CreateOrderLine("line-seed-1002-1", new OrdArrOrderLineRequest(
                        "service",
                        null,
                        "Refrigeration inspection",
                        1,
                        "visit",
                        "maintainarr",
                        now.AddHours(6),
                        now.AddHours(8),
                        350m,
                        0m,
                        false,
                        false,
                        true,
                        false,
                        "none",
                        "maintainarr.work-request"), 1, now),
                ],
                handoffs:
                [
                    new OrdArrHandoffResponse("handoff-seed-1002-1", "maintainarr", "service_work_request", "requested", "Service work requested from MaintainArr.", now),
                ])
            with
            {
                HandoffState = "requested",
                CompletionState = "in_progress",
                ApprovalState = "approved",
                CustomerFacingStatus = "accepted",
                Handoffs = [new OrdArrHandoffResponse("handoff-seed-1002-1", "maintainarr", "service_work_request", "requested", "Service work requested from MaintainArr.", now)],
                Timeline = [
                    new OrdArrTimelineEntryResponse("tl-seed-1002-1", "order.created", "accepted", "Canonical order/request record created.", "person-service-coord", "ordarr", now),
                    new OrdArrTimelineEntryResponse("tl-seed-1002-2", "order.approved", "accepted", "Order approved for orchestration.", "person-service-coord", "ordarr", now.AddMinutes(5)),
                ],
            };

            var blockedOrder = BuildSeedOrder(
                orderId: "order-seed-1003",
                orderNumber: "ORD-2026-1003",
                requestType: "customer_order",
                lifecycleStatus: "on_hold",
                customerRef: new StlProductObjectReference("customarr", "customer", "cust-3001", "CUST-3001"),
                customerName: "Frontier Manufacturing",
                ownerPersonId: "person-order-ops",
                summary: "Hazmat-sensitive replacement materials waiting on compliance clearance.",
                now,
                sourceChannel: "api",
                priority: "urgent",
                orderType: "customer_order",
                lines:
                [
                    CreateOrderLine("line-seed-1003-1", new OrdArrOrderLineRequest(
                        "regulated",
                        new StlProductObjectReference("supplyarr", "part", "part-hazmat-kit", "HZ-2001"),
                        "Regulated solvent kit",
                        1,
                        "kit",
                        "loadarr",
                        now.AddDays(2),
                        now.AddDays(2),
                        980m,
                        0m,
                        true,
                        true,
                        true,
                        true,
                        "compliance_hold",
                        "compliancecore.rulepack.review"), 1, now),
                ],
                holds:
                [
                    new OrdArrHoldResponse("hold-seed-1003-1", "compliance", "Waiting on SDS verification and shipping clearance.", "compliancecore", "person-compliance-review", "ordarr.order_requests.update", "Manual compliance review required.", "open", now, null, null),
                ],
                returns:
                [
                    new OrdArrReturnResponse("return-seed-1003-1", "RMA-2026-0001", "rma", "requested", "Customer reported defective prior shipment.", 1, ["line-seed-1003-1"], "Inspect and replace", "recordarr.record.package.pending", now, now),
                ]);

            _orders.Add(draftOrder);
            _orders.Add(acceptedOrder);
            _orders.Add(blockedOrder);
        }
    }

    private static OrdArrOrderDetailResponse BuildSeedOrder(
        string orderId,
        string orderNumber,
        string requestType,
        string lifecycleStatus,
        StlProductObjectReference customerRef,
        string customerName,
        string ownerPersonId,
        string summary,
        DateTimeOffset now,
        string sourceChannel,
        string priority,
        string orderType,
        IReadOnlyList<OrdArrOrderLineResponse> lines,
        IReadOnlyList<OrdArrHandoffResponse>? handoffs = null,
        IReadOnlyList<OrdArrHoldResponse>? holds = null,
        IReadOnlyList<OrdArrReturnResponse>? returns = null) =>
        new(
            orderId,
            orderNumber,
            requestType,
            lifecycleStatus,
            customerRef,
            customerName,
            ownerPersonId,
            now.AddDays(-2),
            now,
            now.AddHours(-4),
            now.AddDays(5),
            now.AddDays(1),
            now.AddDays(6),
            "not_required",
            "not_started",
            "not_ready",
            summary,
            handoffs ?? [],
            [new OrdArrCompletionPacketResponse($"packet-{Guid.NewGuid():N}"[..14], "completion", "ready", [customerRef])],
            [new OrdArrEventResponse($"evt-{Guid.NewGuid():N}"[..12], StlSuiteEventCatalog.OrdArr.OrderCreated, "Canonical order/request record created.", now)])
        {
            SourceChannel = sourceChannel,
            OrderType = orderType,
            Priority = priority,
            ApprovalState = lifecycleStatus is "accepted" ? "approved" : lifecycleStatus is "on_hold" ? "held" : "not_submitted",
            CustomerFacingStatus = lifecycleStatus,
            Lines = lines,
            Holds = holds ?? [],
            Returns = returns ?? [],
            Timeline = [
                new OrdArrTimelineEntryResponse($"tl-{Guid.NewGuid():N}"[..14], "order.created", lifecycleStatus, "Canonical order/request record created.", ownerPersonId, "ordarr", now),
            ],
        };

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
            SourceChannel = order.SourceChannel,
            OrderType = order.OrderType,
            Priority = order.Priority,
            LineCount = order.Lines.Count,
            HoldCount = order.Holds.Count(hold => string.Equals(hold.Status, "open", StringComparison.OrdinalIgnoreCase)),
            ApprovalState = order.ApprovalState,
            CustomerFacingStatus = order.CustomerFacingStatus,
            NextAction = DetermineNextAction(order),
        };

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
            NormalizeHeaderValue(request.LineType, "item"),
            request.ItemRef,
            request.Description.Trim(),
            request.Quantity,
            NormalizeHeaderValue(request.UnitOfMeasure, "ea"),
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
