using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;
using System.Text.Json;

namespace SupplyArr.Api.Services;

public class SupplierOrderService(
    SupplyArrDbContext db,
    SupplierOrderSettingsService settingsService,
    IntegrationOutboxEnqueueService integrationOutbox,
    RecordArrSupplierOrderClient recordArrClient,
    ISupplyArrAuditService audit)
{
    public static readonly Guid SupplierPortalActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f1");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public SupplierOrderMetadataResponse GetMetadata() => BuildMetadata();

    public async Task<IReadOnlyList<SupplierOrderListItemResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        Guid? supplierId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.SupplierOrders
            .AsNoTracking()
            .Include(x => x.Supplier)
                .ThenInclude(x => x!.ParentSupplier)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = NormalizeStatus(status);
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (supplierId.HasValue)
        {
            query = query.Where(x => x.SupplierId == supplierId.Value);
        }

        var rows = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(MapListItem).ToList();
    }

    public async Task<SupplierOrderResponse> GetAsync(
        Guid tenantId,
        Guid supplierOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, supplierOrderId, cancellationToken);
        return Map(entity);
    }

    public async Task<IReadOnlyList<SupplierOrderStatusUpdateResponse>> ListHistoryAsync(
        Guid tenantId,
        Guid supplierOrderId,
        CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync(tenantId, supplierOrderId, cancellationToken);
        var rows = await db.SupplierOrderStatusUpdates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SupplierOrderId == supplierOrderId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(MapStatusUpdate).ToList();
    }

    public async Task<SupplierOrderResponse> CreateAsync(
        Guid tenantId,
        string actorPersonId,
        Guid actorUserId,
        CreateSupplierOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var supplier = await EnsureSupplierAsync(tenantId, request.SupplierId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var orderedQuantity = NormalizePositiveQuantity(request.OrderedQuantity, "supplier_order.ordered_quantity_required");

        var entity = new SupplierOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BrokerOrderId = request.BrokerOrderId,
            BrokerOrderNumberSnapshot = NormalizeOptionalText(request.BrokerOrderNumberSnapshot, 128),
            SupplierId = supplier.Id,
            SupplierNameSnapshot = supplier.DisplayName,
            SupplierLocationId = request.SupplierLocationId,
            PickupLocationNameSnapshot = NormalizeOptionalText(request.PickupLocationNameSnapshot, 256),
            PickupAddressSnapshot = NormalizeRequiredText(request.PickupAddressSnapshot, 1024, "supplier_order.pickup_address_required"),
            CustomerIdSnapshot = NormalizeOptionalText(request.CustomerIdSnapshot, 128),
            DeliveryLocationNameSnapshot = NormalizeOptionalText(request.DeliveryLocationNameSnapshot, 256),
            DeliveryAddressSnapshot = NormalizeOptionalText(request.DeliveryAddressSnapshot, 1024),
            ItemDescription = NormalizeRequiredText(request.ItemDescription, 512, "supplier_order.item_description_required"),
            OrderedQuantity = orderedQuantity,
            QuantityReady = 0,
            QuantityRemaining = orderedQuantity,
            QuantityUom = NormalizeQuantityUom(request.QuantityUom),
            ExpectedReadyAt = request.ExpectedReadyAt,
            PickupWindowStart = request.PickupWindowStart,
            PickupWindowEnd = request.PickupWindowEnd,
            PickupInstructions = NormalizeOptionalText(request.PickupInstructions, 2048),
            Status = SupplierOrderStatuses.Draft,
            CreatedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.SupplierOrders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_order.create",
            tenantId,
            actorUserId,
            "supplier_order",
            entity.Id.ToString(),
            entity.Status,
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<SupplierOrderResponse> UpdateAsync(
        Guid tenantId,
        string actorPersonId,
        Guid actorUserId,
        Guid supplierOrderId,
        UpdateSupplierOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, supplierOrderId, cancellationToken);
        EnsureInternallyEditable(entity);

        var orderedQuantity = NormalizePositiveQuantity(request.OrderedQuantity, "supplier_order.ordered_quantity_required");
        if (entity.QuantityReady > orderedQuantity)
        {
            throw new StlApiException(
                "supplier_order.ordered_quantity_too_low",
                "Ordered quantity cannot be reduced below the already confirmed ready quantity.",
                409);
        }

        entity.BrokerOrderNumberSnapshot = NormalizeOptionalText(request.BrokerOrderNumberSnapshot, 128);
        entity.SupplierLocationId = request.SupplierLocationId;
        entity.PickupLocationNameSnapshot = NormalizeOptionalText(request.PickupLocationNameSnapshot, 256);
        entity.PickupAddressSnapshot = NormalizeRequiredText(request.PickupAddressSnapshot, 1024, "supplier_order.pickup_address_required");
        entity.CustomerIdSnapshot = NormalizeOptionalText(request.CustomerIdSnapshot, 128);
        entity.DeliveryLocationNameSnapshot = NormalizeOptionalText(request.DeliveryLocationNameSnapshot, 256);
        entity.DeliveryAddressSnapshot = NormalizeOptionalText(request.DeliveryAddressSnapshot, 1024);
        entity.ItemDescription = NormalizeRequiredText(request.ItemDescription, 512, "supplier_order.item_description_required");
        entity.OrderedQuantity = orderedQuantity;
        entity.QuantityRemaining = Math.Max(0, orderedQuantity - entity.QuantityReady);
        entity.QuantityUom = NormalizeQuantityUom(request.QuantityUom);
        entity.ExpectedReadyAt = request.ExpectedReadyAt;
        entity.PickupWindowStart = request.PickupWindowStart;
        entity.PickupWindowEnd = request.PickupWindowEnd;
        entity.PickupInstructions = NormalizeOptionalText(request.PickupInstructions, 2048);
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "supplier_order.update",
            tenantId,
            actorUserId,
            "supplier_order",
            entity.Id.ToString(),
            entity.Status,
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<SendSupplierOrderResponse> SendToSupplierAsync(
        Guid tenantId,
        string actorPersonId,
        Guid actorUserId,
        Guid supplierOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, supplierOrderId, cancellationToken);
        EnsureInternallyEditable(entity);

        var sentUpdate = await ApplyStatusUpdateInternalAsync(
            entity,
            new UpdateSupplierOrderStatusRequest(
                SupplierOrderStatuses.SentToSupplier,
                entity.QuantityReady,
                entity.ExpectedReadyAt,
                entity.ConfirmedReadyAt,
                entity.PickupWindowStart,
                entity.PickupWindowEnd,
                "Sent to supplier.",
                null,
                false),
            SupplierOrderStatusUpdateSources.BrokerUser,
            actorPersonId,
            null,
            null,
            null,
            null,
            isSupplierScoped: false,
            cancellationToken: cancellationToken);

        await EnqueueStatusEventsAsync(entity, sentUpdate, cancellationToken);

        var pendingUpdate = await ApplyStatusUpdateInternalAsync(
            entity,
            new UpdateSupplierOrderStatusRequest(
                SupplierOrderStatuses.PendingSupplierAcknowledgment,
                entity.QuantityReady,
                entity.ExpectedReadyAt,
                entity.ConfirmedReadyAt,
                entity.PickupWindowStart,
                entity.PickupWindowEnd,
                "Awaiting supplier acknowledgment.",
                null,
                false),
            SupplierOrderStatusUpdateSources.BrokerUser,
            actorPersonId,
            null,
            null,
            null,
            null,
            isSupplierScoped: false,
            cancellationToken: cancellationToken);

        await EnqueueStatusEventsAsync(entity, pendingUpdate, cancellationToken);

        var link = await CreateMagicLinkInternalAsync(tenantId, actorPersonId, entity, cancellationToken);

        await audit.WriteAsync(
            "supplier_order.send_to_supplier",
            tenantId,
            actorUserId,
            "supplier_order",
            entity.Id.ToString(),
            entity.Status,
            cancellationToken: cancellationToken);

        return new SendSupplierOrderResponse(Map(entity), link.Url, link.ExpiresAt);
    }

    public async Task<CreateSupplierOrderMagicLinkResponse> CreateMagicLinkAsync(
        Guid tenantId,
        string actorPersonId,
        Guid actorUserId,
        Guid supplierOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, supplierOrderId, cancellationToken);
        var response = await CreateMagicLinkInternalAsync(tenantId, actorPersonId, entity, cancellationToken);

        await audit.WriteAsync(
            "supplier_order.magic_link.create",
            tenantId,
            actorUserId,
            "supplier_order_magic_link",
            response.MagicLinkId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return response;
    }

    public async Task<SupplierOrderResponse> SubmitStatusAsync(
        Guid tenantId,
        string actorPersonId,
        Guid actorUserId,
        Guid supplierOrderId,
        UpdateSupplierOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, supplierOrderId, cancellationToken);
        var update = await ApplyStatusUpdateInternalAsync(
            entity,
            request,
            SupplierOrderStatusUpdateSources.BrokerUser,
            actorPersonId,
            null,
            null,
            null,
            null,
            isSupplierScoped: false,
            cancellationToken: cancellationToken);

        await EnqueueStatusEventsAsync(entity, update, cancellationToken);

        await audit.WriteAsync(
            "supplier_order.status.update",
            tenantId,
            actorUserId,
            "supplier_order_status_update",
            update.Id.ToString(),
            update.NewStatus,
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<SupplierOrderPortalResponse> GetSupplierAccessAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var (entity, link) = await ResolveMagicLinkAsync(token, cancellationToken);
        var settings = await settingsService.LoadOrDefaultAsync(entity.TenantId, cancellationToken);

        return MapPortal(entity, link.ExpiresAt, settings.AllowDestinationSummaryInSupplierPortal);
    }

    public async Task<SupplierOrderPortalResponse> SubmitSupplierAccessStatusAsync(
        string token,
        UpdateSupplierOrderStatusRequest request,
        string? remoteIp,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var (entity, link) = await ResolveMagicLinkAsync(token, cancellationToken);
        var update = await ApplyStatusUpdateInternalAsync(
            entity,
            request,
            SupplierOrderStatusUpdateSources.MagicLink,
            null,
            null,
            link.Id,
            HashSubmissionValue(remoteIp),
            HashSubmissionValue(userAgent),
            isSupplierScoped: true,
            cancellationToken);

        await EnqueueStatusEventsAsync(entity, update, cancellationToken);

        var settings = await settingsService.LoadOrDefaultAsync(entity.TenantId, cancellationToken);
        return MapPortal(entity, link.ExpiresAt, settings.AllowDestinationSummaryInSupplierPortal);
    }

    public async Task<SupplierOrderResponse> RegisterDocumentAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid supplierOrderId,
        RegisterSupplierOrderDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, supplierOrderId, cancellationToken);
        var document = await RegisterDocumentInternalAsync(
            entity,
            request,
            NormalizeOptionalText(actorPersonId, 128),
            null,
            cancellationToken);

        await audit.WriteAsync(
            "supplier_order.document.register",
            tenantId,
            actorUserId,
            "supplier_order_document",
            document.Id.ToString(),
            document.DocumentType,
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<SupplierOrderPortalResponse> RegisterSupplierAccessDocumentAsync(
        string token,
        RegisterSupplierOrderDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var (entity, link) = await ResolveMagicLinkAsync(token, cancellationToken);
        await RegisterDocumentInternalAsync(entity, request, null, link.Id, cancellationToken);

        var settings = await settingsService.LoadOrDefaultAsync(entity.TenantId, cancellationToken);
        return MapPortal(entity, link.ExpiresAt, settings.AllowDestinationSummaryInSupplierPortal);
    }

    public async Task<SupplierOrderBrokerDecisionResponse> CreateBrokerDecisionAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid supplierOrderId,
        CreateSupplierOrderBrokerDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, supplierOrderId, cancellationToken);
        var decisionType = NormalizeDecisionType(request.DecisionType);
        if (!string.Equals(entity.Status, SupplierOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "supplier_order.partial_decision_requires_partially_ready",
                "Partial dispatch decisions require a partially ready supplier order.",
                409);
        }

        if (string.Equals(decisionType, SupplierOrderBrokerDecisionTypes.DispatchPartial, StringComparison.OrdinalIgnoreCase))
        {
            if (!request.SelectedTripId.HasValue)
            {
                throw new StlApiException(
                    "supplier_order.partial_decision.trip_required",
                    "Dispatch partial decisions require a selected RoutArr trip.",
                    400);
            }

            var authorizedQuantity = request.AuthorizedQuantity ?? entity.QuantityReady;
            if (authorizedQuantity <= 0 || authorizedQuantity > entity.QuantityReady)
            {
                throw new StlApiException(
                    "supplier_order.partial_decision.quantity_invalid",
                    "Authorized quantity must be greater than zero and no more than the quantity ready.",
                    400);
            }
        }

        var decision = new SupplierOrderBrokerDecision
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierOrderId = entity.Id,
            DecisionType = decisionType,
            AuthorizedQuantity = request.AuthorizedQuantity,
            SelectedTripId = request.SelectedTripId,
            Note = NormalizeOptionalText(request.Note, 1024),
            DecidedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.SupplierOrderBrokerDecisions.Add(decision);
        entity.UpdatedAt = decision.CreatedAt;
        await db.SaveChangesAsync(cancellationToken);

        if (string.Equals(decision.DecisionType, SupplierOrderBrokerDecisionTypes.DispatchPartial, StringComparison.OrdinalIgnoreCase))
        {
            await integrationOutbox.TryEnqueueAsync(
                tenantId,
                IntegrationOutboxEventKinds.SupplierOrderPartialDispatchAuthorized,
                "supplier_order_broker_decision",
                decision.Id,
                new IntegrationOutboxPayload(tenantId, $"Supplier order partial dispatch authorized: {entity.Id}", entity.SupplierId),
                cancellationToken: cancellationToken);
        }

        await audit.WriteAsync(
            "supplier_order.partial_decision",
            tenantId,
            actorUserId,
            "supplier_order_broker_decision",
            decision.Id.ToString(),
            decision.DecisionType,
            cancellationToken: cancellationToken);

        return MapDecision(decision);
    }

    public async Task<SplitSupplierOrderResponse> SplitRemainingAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid supplierOrderId,
        SplitSupplierOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, supplierOrderId, cancellationToken);
        if (!string.Equals(entity.Status, SupplierOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "supplier_order.split_requires_partially_ready",
                "Only partially ready supplier orders can be split.",
                409);
        }

        if (entity.QuantityReady <= 0 || entity.QuantityReady >= entity.OrderedQuantity)
        {
            throw new StlApiException(
                "supplier_order.split_quantity_invalid",
                "Split remaining requires a partial quantity that is greater than zero and less than the ordered quantity.",
                409);
        }

        var splitReason = NormalizeRequiredText(
            request.SplitReason ?? entity.SplitReason ?? "Split remaining quantity",
            512,
            "supplier_order.split_reason_required");

        var latestStatusUpdate = entity.StatusUpdates
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault(x => string.Equals(x.NewStatus, SupplierOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase))
            ?? throw new StlApiException(
                "supplier_order.split_status_update_missing",
                "A partial readiness status update is required before splitting the order.",
                409);

        var now = DateTimeOffset.UtcNow;
        var readyChild = CloneForSplit(
            entity,
            now,
            actorPersonId,
            entity.QuantityReady,
            0,
            SupplierOrderStatuses.CompletedReadyForDispatch,
            splitReason,
            latestStatusUpdate.Id,
            entity.ConfirmedReadyAt ?? now);

        var remainingQuantity = entity.OrderedQuantity - entity.QuantityReady;
        var remainingStatus = string.Equals(entity.Status, SupplierOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase)
            ? SupplierOrderStatuses.InProgress
            : entity.Status;

        var remainingChild = CloneForSplit(
            entity,
            now,
            actorPersonId,
            0,
            remainingQuantity,
            remainingStatus,
            splitReason,
            latestStatusUpdate.Id,
            null);

        remainingChild.ExpectedReadyAt = request.RemainingExpectedReadyAt ?? entity.ExpectedReadyAt;
        remainingChild.PickupWindowStart = request.RemainingPickupWindowStart ?? entity.PickupWindowStart;
        remainingChild.PickupWindowEnd = request.RemainingPickupWindowEnd ?? entity.PickupWindowEnd;

        db.SupplierOrders.AddRange(readyChild, remainingChild);

        entity.Status = SupplierOrderStatuses.Split;
        entity.SplitReason = splitReason;
        entity.ClosedAt = now;
        entity.UpdatedAt = now;

        foreach (var link in entity.MagicLinks.Where(x => x.RevokedAt is null))
        {
            link.RevokedAt = now;
        }

        var splitDecision = new SupplierOrderBrokerDecision
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierOrderId = entity.Id,
            DecisionType = SupplierOrderBrokerDecisionTypes.SplitRemaining,
            AuthorizedQuantity = entity.QuantityReady,
            SelectedTripId = request.SelectedTripId,
            Note = splitReason,
            DecidedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = now,
        };
        db.SupplierOrderBrokerDecisions.Add(splitDecision);

        var readyStatusUpdate = new SupplierOrderStatusUpdate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierOrderId = readyChild.Id,
            PreviousStatus = null,
            NewStatus = readyChild.Status,
            OrderedQuantitySnapshot = readyChild.OrderedQuantity,
            QuantityReady = readyChild.QuantityReady,
            QuantityRemaining = readyChild.QuantityRemaining,
            EstimatedReadyAt = readyChild.ExpectedReadyAt,
            ConfirmedReadyAt = readyChild.ConfirmedReadyAt,
            PickupWindowStart = readyChild.PickupWindowStart,
            PickupWindowEnd = readyChild.PickupWindowEnd,
            Note = "Ready child created from split.",
            Source = SupplierOrderStatusUpdateSources.System,
            SubmittedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = now,
        };
        var remainingStatusUpdate = new SupplierOrderStatusUpdate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierOrderId = remainingChild.Id,
            PreviousStatus = null,
            NewStatus = remainingChild.Status,
            OrderedQuantitySnapshot = remainingChild.OrderedQuantity,
            QuantityReady = remainingChild.QuantityReady,
            QuantityRemaining = remainingChild.QuantityRemaining,
            EstimatedReadyAt = remainingChild.ExpectedReadyAt,
            ConfirmedReadyAt = remainingChild.ConfirmedReadyAt,
            PickupWindowStart = remainingChild.PickupWindowStart,
            PickupWindowEnd = remainingChild.PickupWindowEnd,
            Note = "Remaining child created from split.",
            Source = SupplierOrderStatusUpdateSources.System,
            SubmittedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = now,
        };
        db.SupplierOrderStatusUpdates.AddRange(readyStatusUpdate, remainingStatusUpdate);

        await db.SaveChangesAsync(cancellationToken);

        await EnqueueStatusEventsAsync(readyChild, readyStatusUpdate, cancellationToken);
        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.SupplierOrderSplitCreated,
            "supplier_order_broker_decision",
            splitDecision.Id,
            new IntegrationOutboxPayload(tenantId, $"Supplier order split created: {entity.Id}", entity.SupplierId),
            cancellationToken: cancellationToken);

        var remainingLink = await CreateMagicLinkInternalAsync(tenantId, actorPersonId, remainingChild, cancellationToken);

        await audit.WriteAsync(
            "supplier_order.split_remaining",
            tenantId,
            actorUserId,
            "supplier_order",
            entity.Id.ToString(),
            SupplierOrderStatuses.Split,
            cancellationToken: cancellationToken);

        return new SplitSupplierOrderResponse(
            await GetAsync(tenantId, entity.Id, cancellationToken),
            await GetAsync(tenantId, readyChild.Id, cancellationToken),
            await GetAsync(tenantId, remainingChild.Id, cancellationToken),
            remainingLink.Token,
            remainingLink.Url);
    }

    public async Task<IntegrationSupplierOrderResponse> GetForIntegrationAsync(
        Guid tenantId,
        Guid supplierOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.SupplierOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == supplierOrderId, cancellationToken)
            ?? throw new StlApiException("supplier_order.not_found", "Supplier order was not found.", 404);

        return MapIntegration(entity);
    }

    public async Task<IReadOnlyList<IntegrationSupplierOrderResponse>> SearchForIntegrationAsync(
        Guid tenantId,
        Guid? brokerOrderId,
        Guid? supplierId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.SupplierOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (brokerOrderId.HasValue)
        {
            query = query.Where(x => x.BrokerOrderId == brokerOrderId.Value);
        }

        if (supplierId.HasValue)
        {
            query = query.Where(x => x.SupplierId == supplierId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = NormalizeStatus(status);
            query = query.Where(x => x.Status == normalizedStatus);
        }

        var rows = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        return rows.Select(MapIntegration).ToList();
    }

    internal static string HashSubmissionValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value.Trim()));
        return Convert.ToHexString(bytes);
    }

    private async Task<CreateSupplierOrderMagicLinkResponse> CreateMagicLinkInternalAsync(
        Guid tenantId,
        string actorPersonId,
        SupplierOrder entity,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.LoadOrDefaultAsync(tenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        foreach (var current in entity.MagicLinks.Where(x => x.RevokedAt is null))
        {
            current.RevokedAt = now;
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        var tokenHash = HashSubmissionValue(token);
        var magicLink = new SupplierOrderMagicLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierOrderId = entity.Id,
            SupplierId = entity.SupplierId,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(settings.MagicLinkTtlHours),
            CreatedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = now,
        };

        db.SupplierOrderMagicLinks.Add(magicLink);
        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return new CreateSupplierOrderMagicLinkResponse(
            magicLink.Id,
            token,
            BuildSupplierPortalUrl(token),
            magicLink.ExpiresAt);
    }

    private async Task<(SupplierOrder Entity, SupplierOrderMagicLink Link)> ResolveMagicLinkAsync(
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new StlApiException("supplier_order.magic_link_required", "Magic link token is required.", 400);
        }

        var tokenHash = HashSubmissionValue(token);
        var now = DateTimeOffset.UtcNow;
        var link = await db.SupplierOrderMagicLinks
            .Include(x => x.SupplierOrder!)
                .ThenInclude(x => x.Documents)
            .Include(x => x.SupplierOrder!)
                .ThenInclude(x => x.BrokerDecisions)
            .Include(x => x.SupplierOrder!)
                .ThenInclude(x => x.StatusUpdates)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken)
            ?? throw new StlApiException("supplier_order.magic_link_not_found", "Supplier access link is invalid.", 404);

        if (link.RevokedAt.HasValue || link.ExpiresAt <= now)
        {
            throw new StlApiException("supplier_order.magic_link_expired", "Supplier access link has expired.", 410);
        }

        link.LastUsedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var order = link.SupplierOrder
            ?? throw new StlApiException("supplier_order.magic_link_not_found", "Supplier access link is invalid.", 404);

        return (order, link);
    }

    private async Task<SupplierOrderDocumentLink> RegisterDocumentInternalAsync(
        SupplierOrder entity,
        RegisterSupplierOrderDocumentRequest request,
        string? actorPersonId,
        Guid? uploadedByMagicLinkId,
        CancellationToken cancellationToken)
    {
        var documentType = NormalizeDocumentType(request.DocumentType);
        var fileName = NormalizeRequiredText(request.FileName, 256, "supplier_order.document_filename_required");
        var contentType = NormalizeRequiredText(request.ContentType, 128, "supplier_order.document_content_type_required");

        var uploadedByPersonId = actorPersonId ?? "supplier-magic-link";
        var record = await recordArrClient.RegisterDocumentAsync(
            new RecordArrSupplierOrderRecordCreateRequest(
                Title: $"{documentType}: {entity.ItemDescription}",
                Description: $"Supplier order {entity.Id} document {fileName}",
                RecordType: "supplier_order_document",
                DocumentType: documentType,
                Classification: "operational",
                SourceProduct: "supplyarr",
                SourceObjectType: "supplier_order",
                SourceObjectId: entity.Id.ToString(),
                SourceObjectDisplayName: entity.ItemDescription,
                OwnerPersonId: entity.CreatedByPersonId ?? uploadedByPersonId,
                UploadedByPersonId: uploadedByPersonId,
                CurrentFileName: fileName,
                CurrentMimeType: contentType),
            new RecordArrSupplierOrderFileCreateRequest(
                RecordId: string.Empty,
                OriginalFilename: fileName,
                MimeType: contentType,
                UploadedByPersonId: uploadedByPersonId,
                StorageProvider: NormalizeOptionalText(request.StorageProvider, 64),
                StorageKey: NormalizeOptionalText(request.StorageKey, 512),
                SizeBytes: request.SizeBytes,
                PageCount: request.PageCount,
                ImageWidth: request.ImageWidth,
                ImageHeight: request.ImageHeight,
                DurationSeconds: request.DurationSeconds),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var document = new SupplierOrderDocumentLink
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            SupplierOrderId = entity.Id,
            DocumentType = documentType,
            FileName = fileName,
            ContentType = contentType,
            StorageProvider = NormalizeOptionalText(request.StorageProvider, 64),
            StorageKey = NormalizeOptionalText(request.StorageKey, 512),
            RecordArrRecordId = record.RecordId,
            RecordArrRecordNumberSnapshot = record.RecordNumber,
            RecordArrFileId = record.FileId,
            UploadedByPersonId = actorPersonId,
            UploadedByMagicLinkId = uploadedByMagicLinkId,
            UploadedAt = now,
        };

        db.SupplierOrderDocumentLinks.Add(document);
        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return document;
    }

    private async Task<SupplierOrderStatusUpdate> ApplyStatusUpdateInternalAsync(
        SupplierOrder entity,
        UpdateSupplierOrderStatusRequest request,
        string source,
        string? submittedByPersonId,
        Guid? submittedBySupplierContactId,
        Guid? submittedByMagicLinkId,
        string? submittedIpHash,
        string? submittedUserAgentHash,
        bool isSupplierScoped,
        CancellationToken cancellationToken)
    {
        var previousStatus = entity.Status;
        var newStatus = NormalizeStatus(request.NewStatus);
        var quantityReady = request.QuantityReady ?? entity.QuantityReady;
        if (quantityReady < 0)
        {
            throw new StlApiException(
                "supplier_order.quantity_ready_invalid",
                "Quantity ready must be zero or greater.",
                400);
        }

        if (quantityReady > entity.OrderedQuantity)
        {
            throw new StlApiException(
                "supplier_order.quantity_ready_exceeds_ordered",
                "Quantity ready cannot exceed the ordered quantity.",
                400);
        }

        if (isSupplierScoped)
        {
            EnsureSupplierCanTransition(previousStatus, newStatus);
            if (string.Equals(newStatus, SupplierOrderStatuses.CompletedReadyForDispatch, StringComparison.OrdinalIgnoreCase)
                && !request.ReadyForPickupConfirmed)
            {
                throw new StlApiException(
                    "supplier_order.ready_checkbox_required",
                    "Ready for pickup confirmation is required before marking the order ready for dispatch.",
                    400);
            }
        }
        else if (!SupplierOrderStatuses.All.Contains(newStatus))
        {
            throw new StlApiException("supplier_order.invalid_status", "Supplier order status is invalid.", 400);
        }

        if (string.Equals(newStatus, SupplierOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase)
            && (quantityReady <= 0 || quantityReady >= entity.OrderedQuantity))
        {
            throw new StlApiException(
                "supplier_order.partial_quantity_invalid",
                "Partially ready requires a quantity that is greater than zero and less than the ordered quantity.",
                400);
        }

        if (string.Equals(newStatus, SupplierOrderStatuses.CompletedReadyForDispatch, StringComparison.OrdinalIgnoreCase)
            && quantityReady <= 0)
        {
            throw new StlApiException(
                "supplier_order.ready_quantity_invalid",
                "Ready for dispatch requires a quantity that is greater than zero.",
                400);
        }

        var quantityRemaining = Math.Max(0, entity.OrderedQuantity - quantityReady);
        var confirmedReadyAt = request.ConfirmedReadyAt ?? entity.ConfirmedReadyAt;
        if (string.Equals(newStatus, SupplierOrderStatuses.CompletedReadyForDispatch, StringComparison.OrdinalIgnoreCase)
            && confirmedReadyAt is null)
        {
            confirmedReadyAt = DateTimeOffset.UtcNow;
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = newStatus;
        entity.QuantityReady = quantityReady;
        entity.QuantityRemaining = quantityRemaining;
        entity.ExpectedReadyAt = request.EstimatedReadyAt ?? entity.ExpectedReadyAt;
        entity.ConfirmedReadyAt = confirmedReadyAt;
        entity.PickupWindowStart = request.PickupWindowStart ?? entity.PickupWindowStart;
        entity.PickupWindowEnd = request.PickupWindowEnd ?? entity.PickupWindowEnd;
        entity.UpdatedAt = now;
        if (string.Equals(newStatus, SupplierOrderStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            entity.CancelledAt ??= now;
        }

        if (string.Equals(newStatus, SupplierOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(newStatus, SupplierOrderStatuses.Split, StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt ??= now;
        }

        var statusUpdate = new SupplierOrderStatusUpdate
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            SupplierOrderId = entity.Id,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            OrderedQuantitySnapshot = entity.OrderedQuantity,
            QuantityReady = quantityReady,
            QuantityRemaining = quantityRemaining,
            EstimatedReadyAt = entity.ExpectedReadyAt,
            ConfirmedReadyAt = entity.ConfirmedReadyAt,
            PickupWindowStart = entity.PickupWindowStart,
            PickupWindowEnd = entity.PickupWindowEnd,
            Note = NormalizeOptionalText(request.Note, 2048),
            ExceptionReason = NormalizeOptionalText(request.ExceptionReason, 1024),
            Source = source,
            SubmittedBySupplierContactId = submittedBySupplierContactId,
            SubmittedByPersonId = NormalizeOptionalText(submittedByPersonId, 128),
            SubmittedByMagicLinkId = submittedByMagicLinkId,
            SubmittedIpHash = NormalizeOptionalText(submittedIpHash, 128),
            SubmittedUserAgentHash = NormalizeOptionalText(submittedUserAgentHash, 128),
            CreatedAt = now,
        };

        db.SupplierOrderStatusUpdates.Add(statusUpdate);
        await db.SaveChangesAsync(cancellationToken);

        return statusUpdate;
    }

    private async Task EnqueueStatusEventsAsync(
        SupplierOrder entity,
        SupplierOrderStatusUpdate statusUpdate,
        CancellationToken cancellationToken)
    {
        await integrationOutbox.TryEnqueueAsync(
            entity.TenantId,
            IntegrationOutboxEventKinds.SupplierOrderStatusChanged,
            "supplier_order_status_update",
            statusUpdate.Id,
            new IntegrationOutboxPayload(
                entity.TenantId,
                $"Supplier order status changed to {statusUpdate.NewStatus}: {entity.Id}",
                entity.SupplierId),
            cancellationToken: cancellationToken);

        if (string.Equals(statusUpdate.NewStatus, SupplierOrderStatuses.CompletedReadyForDispatch, StringComparison.OrdinalIgnoreCase))
        {
            await integrationOutbox.TryEnqueueAsync(
                entity.TenantId,
                IntegrationOutboxEventKinds.SupplierOrderCompletedForDispatch,
                "supplier_order_status_update",
                statusUpdate.Id,
                new IntegrationOutboxPayload(
                    entity.TenantId,
                    $"Supplier order ready for dispatch: {entity.Id}",
                    entity.SupplierId),
                cancellationToken: cancellationToken);
        }
    }

    private static void EnsureSupplierCanTransition(string previousStatus, string newStatus)
    {
        if (string.Equals(previousStatus, newStatus, StringComparison.OrdinalIgnoreCase)
            && (string.Equals(newStatus, SupplierOrderStatuses.Acknowledged, StringComparison.OrdinalIgnoreCase)
                || string.Equals(newStatus, SupplierOrderStatuses.InProgress, StringComparison.OrdinalIgnoreCase)
                || string.Equals(newStatus, SupplierOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var allowed = previousStatus.ToLowerInvariant() switch
        {
            SupplierOrderStatuses.PendingSupplierAcknowledgment => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SupplierOrderStatuses.Acknowledged,
                SupplierOrderStatuses.UnableToFulfill,
            },
            SupplierOrderStatuses.Acknowledged => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SupplierOrderStatuses.Acknowledged,
                SupplierOrderStatuses.InProgress,
                SupplierOrderStatuses.PartiallyReady,
                SupplierOrderStatuses.CompletedReadyForDispatch,
                SupplierOrderStatuses.UnableToFulfill,
            },
            SupplierOrderStatuses.InProgress => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SupplierOrderStatuses.InProgress,
                SupplierOrderStatuses.PartiallyReady,
                SupplierOrderStatuses.CompletedReadyForDispatch,
                SupplierOrderStatuses.UnableToFulfill,
            },
            SupplierOrderStatuses.PartiallyReady => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SupplierOrderStatuses.PartiallyReady,
                SupplierOrderStatuses.CompletedReadyForDispatch,
                SupplierOrderStatuses.UnableToFulfill,
            },
            _ => [],
        };

        if (!allowed.Contains(newStatus))
        {
            throw new StlApiException(
                "supplier_order.invalid_supplier_transition",
                $"Suppliers cannot move a supplier order from {previousStatus} to {newStatus}.",
                409);
        }
    }

    private static string BuildSupplierPortalUrl(string token) =>
        $"/supplier-order-portal/orders/{Uri.EscapeDataString(token)}";

    private static SupplierOrder CloneForSplit(
        SupplierOrder source,
        DateTimeOffset now,
        string actorPersonId,
        decimal quantityReady,
        decimal quantityRemaining,
        string status,
        string splitReason,
        Guid splitFromStatusUpdateId,
        DateTimeOffset? confirmedReadyAt)
    {
        var orderedQuantity = quantityReady + quantityRemaining;
        return new SupplierOrder
        {
            Id = Guid.NewGuid(),
            TenantId = source.TenantId,
            BrokerOrderId = source.BrokerOrderId,
            BrokerOrderNumberSnapshot = source.BrokerOrderNumberSnapshot,
            SupplierId = source.SupplierId,
            SupplierNameSnapshot = source.SupplierNameSnapshot,
            SupplierLocationId = source.SupplierLocationId,
            PickupLocationNameSnapshot = source.PickupLocationNameSnapshot,
            PickupAddressSnapshot = source.PickupAddressSnapshot,
            CustomerIdSnapshot = source.CustomerIdSnapshot,
            DeliveryLocationNameSnapshot = source.DeliveryLocationNameSnapshot,
            DeliveryAddressSnapshot = source.DeliveryAddressSnapshot,
            ItemDescription = source.ItemDescription,
            OrderedQuantity = orderedQuantity,
            QuantityReady = quantityReady,
            QuantityRemaining = quantityRemaining,
            QuantityUom = source.QuantityUom,
            ExpectedReadyAt = source.ExpectedReadyAt,
            ConfirmedReadyAt = confirmedReadyAt,
            PickupWindowStart = source.PickupWindowStart,
            PickupWindowEnd = source.PickupWindowEnd,
            PickupInstructions = source.PickupInstructions,
            Status = status,
            CreatedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            ParentSupplierOrderId = source.Id,
            SplitReason = splitReason,
            SplitFromStatusUpdateId = splitFromStatusUpdateId,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    private async Task<SupplierOrder> LoadAsync(
        Guid tenantId,
        Guid supplierOrderId,
        CancellationToken cancellationToken)
    {
        var entity = await db.SupplierOrders
            .AsNoTracking()
            .Include(x => x.Supplier)
                .ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.Documents)
            .Include(x => x.BrokerDecisions)
            .Include(x => x.StatusUpdates)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == supplierOrderId, cancellationToken)
            ?? throw new StlApiException("supplier_order.not_found", "Supplier order was not found.", 404);

        return entity;
    }

    private async Task<SupplierOrder> LoadTrackedAsync(
        Guid tenantId,
        Guid supplierOrderId,
        CancellationToken cancellationToken)
    {
        var entity = await db.SupplierOrders
            .Include(x => x.Supplier)
                .ThenInclude(x => x!.ParentSupplier)
            .Include(x => x.Documents)
            .Include(x => x.BrokerDecisions)
            .Include(x => x.StatusUpdates)
            .Include(x => x.MagicLinks)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == supplierOrderId, cancellationToken)
            ?? throw new StlApiException("supplier_order.not_found", "Supplier order was not found.", 404);

        return entity;
    }

    private async Task EnsureExistsAsync(Guid tenantId, Guid supplierOrderId, CancellationToken cancellationToken)
    {
        if (!await db.SupplierOrders.AnyAsync(x => x.TenantId == tenantId && x.Id == supplierOrderId, cancellationToken))
        {
            throw new StlApiException("supplier_order.not_found", "Supplier order was not found.", 404);
        }
    }

    private async Task<Supplier> EnsureSupplierAsync(
        Guid tenantId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == supplierId, cancellationToken)
            ?? throw new StlApiException("supplier_order.supplier_not_found", "Supplier was not found.", 404);

        return supplier;
    }

    private static void EnsureInternallyEditable(SupplierOrder entity)
    {
        if (string.Equals(entity.Status, SupplierOrderStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, SupplierOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, SupplierOrderStatuses.Split, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "supplier_order.not_editable",
                "Closed, cancelled, or split supplier orders cannot be modified.",
                409);
        }
    }

    private static SupplierOrderListItemResponse MapListItem(SupplierOrder entity) =>
        new(
            entity.Id,
            entity.Status,
            entity.SupplierId,
            entity.SupplierNameSnapshot,
            entity.Supplier?.ParentSupplierId,
            entity.Supplier?.ParentSupplier?.DisplayName,
            entity.Supplier?.UnitKind ?? "identity",
            ParseServiceTypes(entity.Supplier?.ServiceTypesJson),
            entity.ItemDescription,
            entity.OrderedQuantity,
            entity.QuantityReady,
            entity.QuantityRemaining,
            entity.QuantityUom,
            entity.ExpectedReadyAt,
            entity.ConfirmedReadyAt,
            entity.ParentSupplierOrderId,
            entity.UpdatedAt);

    private static SupplierOrderResponse Map(SupplierOrder entity) =>
        new(
            entity.Id,
            entity.SupplierId,
            entity.SupplierNameSnapshot,
            entity.Supplier?.ParentSupplierId,
            entity.Supplier?.ParentSupplier?.DisplayName,
            entity.Supplier?.UnitKind ?? "identity",
            ParseServiceTypes(entity.Supplier?.ServiceTypesJson),
            entity.BrokerOrderId,
            entity.BrokerOrderNumberSnapshot,
            entity.SupplierLocationId,
            entity.PickupLocationNameSnapshot,
            entity.PickupAddressSnapshot,
            entity.CustomerIdSnapshot,
            entity.DeliveryLocationNameSnapshot,
            entity.DeliveryAddressSnapshot,
            entity.ItemDescription,
            entity.OrderedQuantity,
            entity.QuantityReady,
            entity.QuantityRemaining,
            entity.QuantityUom,
            entity.ExpectedReadyAt,
            entity.ConfirmedReadyAt,
            entity.PickupWindowStart,
            entity.PickupWindowEnd,
            entity.PickupInstructions,
            entity.Status,
            entity.CreatedByPersonId,
            entity.ParentSupplierOrderId,
            entity.SplitReason,
            entity.SplitFromStatusUpdateId,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CancelledAt,
            entity.ClosedAt,
            entity.Documents.OrderByDescending(x => x.UploadedAt).Select(MapDocument).ToList(),
            entity.BrokerDecisions.OrderByDescending(x => x.CreatedAt).Select(MapDecision).ToList(),
            entity.StatusUpdates.OrderBy(x => x.CreatedAt).Select(MapStatusUpdate).ToList());

    private static SupplierOrderMetadataResponse BuildMetadata() =>
        new(
            FilterStatusOptions: Options(
                "supplyarr",
                "supplyarr.supplier_order.workflow",
                SupplierOrderStatuses.Draft,
                SupplierOrderStatuses.SentToSupplier,
                SupplierOrderStatuses.PendingSupplierAcknowledgment,
                SupplierOrderStatuses.Acknowledged,
                SupplierOrderStatuses.InProgress,
                SupplierOrderStatuses.PartiallyReady,
                SupplierOrderStatuses.CompletedReadyForDispatch,
                SupplierOrderStatuses.UnableToFulfill,
                SupplierOrderStatuses.Cancelled,
                SupplierOrderStatuses.Closed,
                SupplierOrderStatuses.Split),
            InternalStatusOptions: Options(
                "supplyarr",
                "supplyarr.supplier_order.workflow",
                SupplierOrderStatuses.Draft,
                SupplierOrderStatuses.SentToSupplier,
                SupplierOrderStatuses.PendingSupplierAcknowledgment,
                SupplierOrderStatuses.Acknowledged,
                SupplierOrderStatuses.InProgress,
                SupplierOrderStatuses.PartiallyReady,
                SupplierOrderStatuses.CompletedReadyForDispatch,
                SupplierOrderStatuses.UnableToFulfill,
                SupplierOrderStatuses.Cancelled,
                SupplierOrderStatuses.Closed),
            SupplierPortalStatusOptions: Options(
                "supplyarr",
                "supplyarr.supplier_order.workflow",
                SupplierOrderStatuses.Acknowledged,
                SupplierOrderStatuses.InProgress,
                SupplierOrderStatuses.PartiallyReady,
                SupplierOrderStatuses.CompletedReadyForDispatch,
                SupplierOrderStatuses.UnableToFulfill),
            DocumentTypeOptions: Options(
                "recordarr",
                "recordarr.document_type_catalog.mapped_to_supplyarr",
                SupplierOrderDocumentTypes.Photo,
                SupplierOrderDocumentTypes.PackingSlip,
                SupplierOrderDocumentTypes.BillOfLading,
                SupplierOrderDocumentTypes.ScaleTicket,
                SupplierOrderDocumentTypes.ProofOfReadiness,
                SupplierOrderDocumentTypes.Other),
            BrokerDecisionTypeOptions: Options(
                "supplyarr",
                "supplyarr.supplier_order.partial_decision",
                SupplierOrderBrokerDecisionTypes.WaitFull,
                SupplierOrderBrokerDecisionTypes.DispatchPartial,
                SupplierOrderBrokerDecisionTypes.SplitRemaining));

    private static IReadOnlyList<SupplierOrderCatalogOptionResponse> Options(
        string owner,
        string sourceOfTruth,
        params string[] values) =>
        values.Select(value => new SupplierOrderCatalogOptionResponse(value, Humanize(value), owner, sourceOfTruth)).ToList();

    private static string Humanize(string value)
    {
        var words = value.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length == 0)
        {
            return value;
        }

        return string.Join(' ', words.Select((word, index) =>
            index == 0
                ? char.ToUpperInvariant(word[0]) + word[1..]
                : word));
    }

    private static SupplierOrderPortalResponse MapPortal(
        SupplierOrder entity,
        DateTimeOffset expiresAt,
        bool includeDestinationSummary) =>
        new(
            entity.Id,
            entity.Status,
            entity.SupplierId,
            entity.SupplierNameSnapshot,
            entity.Supplier?.ParentSupplierId,
            entity.Supplier?.ParentSupplier?.DisplayName,
            entity.Supplier?.UnitKind ?? "identity",
            ParseServiceTypes(entity.Supplier?.ServiceTypesJson),
            entity.PickupLocationNameSnapshot ?? entity.SupplierNameSnapshot,
            entity.PickupAddressSnapshot,
            includeDestinationSummary ? entity.DeliveryLocationNameSnapshot : null,
            includeDestinationSummary ? entity.DeliveryAddressSnapshot : null,
            entity.ItemDescription,
            entity.OrderedQuantity,
            entity.QuantityReady,
            entity.QuantityRemaining,
            entity.QuantityUom,
            entity.ExpectedReadyAt,
            entity.ConfirmedReadyAt,
            entity.PickupWindowStart,
            entity.PickupWindowEnd,
            entity.PickupInstructions,
            expiresAt,
            entity.Documents
                .Where(x => x.DeletedAt is null)
                .OrderByDescending(x => x.UploadedAt)
                .Select(MapDocument)
                .ToList(),
            entity.StatusUpdates.OrderBy(x => x.CreatedAt).Select(MapStatusUpdate).ToList(),
            BuildMetadata());

    private static IReadOnlyList<string> ParseServiceTypes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(value, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static IntegrationSupplierOrderResponse MapIntegration(SupplierOrder entity) =>
        new(
            entity.Id,
            entity.BrokerOrderId,
            entity.BrokerOrderNumberSnapshot,
            entity.SupplierId,
            entity.SupplierNameSnapshot,
            entity.PickupLocationNameSnapshot,
            entity.PickupAddressSnapshot,
            entity.DeliveryLocationNameSnapshot,
            entity.DeliveryAddressSnapshot,
            entity.ItemDescription,
            entity.OrderedQuantity,
            entity.QuantityReady,
            entity.QuantityRemaining,
            entity.QuantityUom,
            entity.ExpectedReadyAt,
            entity.ConfirmedReadyAt,
            entity.PickupWindowStart,
            entity.PickupWindowEnd,
            entity.PickupInstructions,
            entity.Status,
            entity.UpdatedAt);

    private static SupplierOrderStatusUpdateResponse MapStatusUpdate(SupplierOrderStatusUpdate update) =>
        new(
            update.Id,
            update.PreviousStatus,
            update.NewStatus,
            update.OrderedQuantitySnapshot,
            update.QuantityReady,
            update.QuantityRemaining,
            update.EstimatedReadyAt,
            update.ConfirmedReadyAt,
            update.PickupWindowStart,
            update.PickupWindowEnd,
            update.Note,
            update.ExceptionReason,
            update.Source,
            update.SubmittedByPersonId,
            update.CreatedAt);

    private static SupplierOrderDocumentResponse MapDocument(SupplierOrderDocumentLink document) =>
        new(
            document.Id,
            document.DocumentType,
            document.FileName,
            document.ContentType,
            document.RecordArrRecordId,
            document.RecordArrRecordNumberSnapshot,
            document.RecordArrFileId,
            document.UploadedAt);

    private static SupplierOrderBrokerDecisionResponse MapDecision(SupplierOrderBrokerDecision decision) =>
        new(
            decision.Id,
            decision.DecisionType,
            decision.AuthorizedQuantity,
            decision.SelectedTripId,
            decision.Note,
            decision.DecidedByPersonId,
            decision.CreatedAt);

    private static string NormalizeStatus(string status)
    {
        var normalized = status.Trim().ToLowerInvariant();
        if (!SupplierOrderStatuses.All.Contains(normalized))
        {
            throw new StlApiException("supplier_order.invalid_status", "Supplier order status is invalid.", 400);
        }

        return normalized;
    }

    private static string NormalizeDecisionType(string decisionType)
    {
        var normalized = decisionType.Trim().ToLowerInvariant();
        if (!SupplierOrderBrokerDecisionTypes.All.Contains(normalized))
        {
            throw new StlApiException("supplier_order.invalid_decision", "Supplier order broker decision is invalid.", 400);
        }

        return normalized;
    }

    private static string NormalizeDocumentType(string documentType)
    {
        var normalized = documentType.Trim().ToLowerInvariant();
        if (!SupplierOrderDocumentTypes.All.Contains(normalized))
        {
            throw new StlApiException("supplier_order.invalid_document_type", "Supplier order document type is invalid.", 400);
        }

        return normalized;
    }

    private static decimal NormalizePositiveQuantity(decimal quantity, string code)
    {
        if (quantity <= 0)
        {
            throw new StlApiException(code, "Ordered quantity must be greater than zero.", 400);
        }

        return Math.Round(quantity, 4, MidpointRounding.AwayFromZero);
    }

    private static string NormalizeQuantityUom(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? SupplierOrderDefaults.DefaultQuantityUom
            : value.Trim().ToLowerInvariant();

        if (normalized.Length > 32)
        {
            throw new StlApiException(
                "supplier_order.quantity_uom_too_long",
                "Quantity unit of measure must be 32 characters or fewer.",
                400);
        }

        return normalized;
    }

    private static string NormalizeRequiredText(string value, int maxLength, string code)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StlApiException(code, "Required field is missing.", 400);
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException(code, $"Field must be {maxLength} characters or fewer.", 400);
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new StlApiException("supplier_order.field_too_long", $"Field must be {maxLength} characters or fewer.", 400);
        }

        return normalized;
    }
}

