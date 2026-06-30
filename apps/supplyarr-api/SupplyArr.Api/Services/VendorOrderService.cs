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
    VendorOrderSettingsService settingsService,
    IntegrationOutboxEnqueueService integrationOutbox,
    RecordArrVendorOrderClient recordArrClient,
    ISupplyArrAuditService audit)
{
    public static readonly Guid VendorPortalActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f1");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public VendorOrderMetadataResponse GetMetadata() => BuildMetadata();

    public async Task<IReadOnlyList<VendorOrderListItemResponse>> ListAsync(
        Guid tenantId,
        string? status = null,
        Guid? vendorId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.VendorOrders
            .AsNoTracking()
            .Include(x => x.Vendor)
                .ThenInclude(x => x!.ParentExternalParty)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = NormalizeStatus(status);
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (vendorId.HasValue)
        {
            query = query.Where(x => x.VendorId == vendorId.Value);
        }

        var rows = await query
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(MapListItem).ToList();
    }

    public async Task<VendorOrderResponse> GetAsync(
        Guid tenantId,
        Guid vendorOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(tenantId, vendorOrderId, cancellationToken);
        return Map(entity);
    }

    public async Task<IReadOnlyList<VendorOrderStatusUpdateResponse>> ListHistoryAsync(
        Guid tenantId,
        Guid vendorOrderId,
        CancellationToken cancellationToken = default)
    {
        await EnsureExistsAsync(tenantId, vendorOrderId, cancellationToken);
        var rows = await db.VendorOrderStatusUpdates
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.VendorOrderId == vendorOrderId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(MapStatusUpdate).ToList();
    }

    public async Task<VendorOrderResponse> CreateAsync(
        Guid tenantId,
        string actorPersonId,
        Guid actorUserId,
        CreateVendorOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var vendor = await EnsureVendorAsync(tenantId, request.VendorId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var orderedQuantity = NormalizePositiveQuantity(request.OrderedQuantity, "vendor_order.ordered_quantity_required");

        var entity = new VendorOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BrokerOrderId = request.BrokerOrderId,
            BrokerOrderNumberSnapshot = NormalizeOptionalText(request.BrokerOrderNumberSnapshot, 128),
            VendorId = vendor.Id,
            VendorNameSnapshot = vendor.DisplayName,
            VendorLocationId = request.VendorLocationId,
            PickupLocationNameSnapshot = NormalizeOptionalText(request.PickupLocationNameSnapshot, 256),
            PickupAddressSnapshot = NormalizeRequiredText(request.PickupAddressSnapshot, 1024, "vendor_order.pickup_address_required"),
            CustomerIdSnapshot = NormalizeOptionalText(request.CustomerIdSnapshot, 128),
            DeliveryLocationNameSnapshot = NormalizeOptionalText(request.DeliveryLocationNameSnapshot, 256),
            DeliveryAddressSnapshot = NormalizeOptionalText(request.DeliveryAddressSnapshot, 1024),
            ItemDescription = NormalizeRequiredText(request.ItemDescription, 512, "vendor_order.item_description_required"),
            OrderedQuantity = orderedQuantity,
            QuantityReady = 0,
            QuantityRemaining = orderedQuantity,
            QuantityUom = NormalizeQuantityUom(request.QuantityUom),
            ExpectedReadyAt = request.ExpectedReadyAt,
            PickupWindowStart = request.PickupWindowStart,
            PickupWindowEnd = request.PickupWindowEnd,
            PickupInstructions = NormalizeOptionalText(request.PickupInstructions, 2048),
            Status = VendorOrderStatuses.Draft,
            CreatedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.VendorOrders.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "vendor_order.create",
            tenantId,
            actorUserId,
            "vendor_order",
            entity.Id.ToString(),
            entity.Status,
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<VendorOrderResponse> UpdateAsync(
        Guid tenantId,
        string actorPersonId,
        Guid actorUserId,
        Guid vendorOrderId,
        UpdateVendorOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, vendorOrderId, cancellationToken);
        EnsureInternallyEditable(entity);

        var orderedQuantity = NormalizePositiveQuantity(request.OrderedQuantity, "vendor_order.ordered_quantity_required");
        if (entity.QuantityReady > orderedQuantity)
        {
            throw new StlApiException(
                "vendor_order.ordered_quantity_too_low",
                "Ordered quantity cannot be reduced below the already confirmed ready quantity.",
                409);
        }

        entity.BrokerOrderNumberSnapshot = NormalizeOptionalText(request.BrokerOrderNumberSnapshot, 128);
        entity.VendorLocationId = request.VendorLocationId;
        entity.PickupLocationNameSnapshot = NormalizeOptionalText(request.PickupLocationNameSnapshot, 256);
        entity.PickupAddressSnapshot = NormalizeRequiredText(request.PickupAddressSnapshot, 1024, "vendor_order.pickup_address_required");
        entity.CustomerIdSnapshot = NormalizeOptionalText(request.CustomerIdSnapshot, 128);
        entity.DeliveryLocationNameSnapshot = NormalizeOptionalText(request.DeliveryLocationNameSnapshot, 256);
        entity.DeliveryAddressSnapshot = NormalizeOptionalText(request.DeliveryAddressSnapshot, 1024);
        entity.ItemDescription = NormalizeRequiredText(request.ItemDescription, 512, "vendor_order.item_description_required");
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
            "vendor_order.update",
            tenantId,
            actorUserId,
            "vendor_order",
            entity.Id.ToString(),
            entity.Status,
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<SendVendorOrderResponse> SendToVendorAsync(
        Guid tenantId,
        string actorPersonId,
        Guid actorUserId,
        Guid vendorOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, vendorOrderId, cancellationToken);
        EnsureInternallyEditable(entity);

        var sentUpdate = await ApplyStatusUpdateInternalAsync(
            entity,
            new UpdateVendorOrderStatusRequest(
                VendorOrderStatuses.SentToVendor,
                entity.QuantityReady,
                entity.ExpectedReadyAt,
                entity.ConfirmedReadyAt,
                entity.PickupWindowStart,
                entity.PickupWindowEnd,
                "Sent to vendor.",
                null,
                false),
            VendorOrderStatusUpdateSources.BrokerUser,
            actorPersonId,
            null,
            null,
            null,
            null,
            isVendorScoped: false,
            cancellationToken: cancellationToken);

        await EnqueueStatusEventsAsync(entity, sentUpdate, cancellationToken);

        var pendingUpdate = await ApplyStatusUpdateInternalAsync(
            entity,
            new UpdateVendorOrderStatusRequest(
                VendorOrderStatuses.PendingVendorAcknowledgment,
                entity.QuantityReady,
                entity.ExpectedReadyAt,
                entity.ConfirmedReadyAt,
                entity.PickupWindowStart,
                entity.PickupWindowEnd,
                "Awaiting vendor acknowledgment.",
                null,
                false),
            VendorOrderStatusUpdateSources.BrokerUser,
            actorPersonId,
            null,
            null,
            null,
            null,
            isVendorScoped: false,
            cancellationToken: cancellationToken);

        await EnqueueStatusEventsAsync(entity, pendingUpdate, cancellationToken);

        var link = await CreateMagicLinkInternalAsync(tenantId, actorPersonId, entity, cancellationToken);

        await audit.WriteAsync(
            "vendor_order.send_to_vendor",
            tenantId,
            actorUserId,
            "vendor_order",
            entity.Id.ToString(),
            entity.Status,
            cancellationToken: cancellationToken);

        return new SendVendorOrderResponse(Map(entity), link.Url, link.ExpiresAt);
    }

    public async Task<CreateVendorOrderMagicLinkResponse> CreateMagicLinkAsync(
        Guid tenantId,
        string actorPersonId,
        Guid actorUserId,
        Guid vendorOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, vendorOrderId, cancellationToken);
        var response = await CreateMagicLinkInternalAsync(tenantId, actorPersonId, entity, cancellationToken);

        await audit.WriteAsync(
            "vendor_order.magic_link.create",
            tenantId,
            actorUserId,
            "vendor_order_magic_link",
            response.MagicLinkId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return response;
    }

    public async Task<VendorOrderResponse> SubmitStatusAsync(
        Guid tenantId,
        string actorPersonId,
        Guid actorUserId,
        Guid vendorOrderId,
        UpdateVendorOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, vendorOrderId, cancellationToken);
        var update = await ApplyStatusUpdateInternalAsync(
            entity,
            request,
            VendorOrderStatusUpdateSources.BrokerUser,
            actorPersonId,
            null,
            null,
            null,
            null,
            isVendorScoped: false,
            cancellationToken: cancellationToken);

        await EnqueueStatusEventsAsync(entity, update, cancellationToken);

        await audit.WriteAsync(
            "vendor_order.status.update",
            tenantId,
            actorUserId,
            "vendor_order_status_update",
            update.Id.ToString(),
            update.NewStatus,
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<VendorOrderPortalResponse> GetVendorAccessAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var (entity, link) = await ResolveMagicLinkAsync(token, cancellationToken);
        var settings = await settingsService.LoadOrDefaultAsync(entity.TenantId, cancellationToken);

        return MapPortal(entity, link.ExpiresAt, settings.AllowDestinationSummaryInVendorPortal);
    }

    public async Task<VendorOrderPortalResponse> SubmitVendorAccessStatusAsync(
        string token,
        UpdateVendorOrderStatusRequest request,
        string? remoteIp,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var (entity, link) = await ResolveMagicLinkAsync(token, cancellationToken);
        var update = await ApplyStatusUpdateInternalAsync(
            entity,
            request,
            VendorOrderStatusUpdateSources.MagicLink,
            null,
            null,
            link.Id,
            HashSubmissionValue(remoteIp),
            HashSubmissionValue(userAgent),
            isVendorScoped: true,
            cancellationToken);

        await EnqueueStatusEventsAsync(entity, update, cancellationToken);

        var settings = await settingsService.LoadOrDefaultAsync(entity.TenantId, cancellationToken);
        return MapPortal(entity, link.ExpiresAt, settings.AllowDestinationSummaryInVendorPortal);
    }

    public async Task<VendorOrderResponse> RegisterDocumentAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid vendorOrderId,
        RegisterVendorOrderDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, vendorOrderId, cancellationToken);
        var document = await RegisterDocumentInternalAsync(
            entity,
            request,
            NormalizeOptionalText(actorPersonId, 128),
            null,
            cancellationToken);

        await audit.WriteAsync(
            "vendor_order.document.register",
            tenantId,
            actorUserId,
            "vendor_order_document",
            document.Id.ToString(),
            document.DocumentType,
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    public async Task<VendorOrderPortalResponse> RegisterVendorAccessDocumentAsync(
        string token,
        RegisterVendorOrderDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        var (entity, link) = await ResolveMagicLinkAsync(token, cancellationToken);
        await RegisterDocumentInternalAsync(entity, request, null, link.Id, cancellationToken);

        var settings = await settingsService.LoadOrDefaultAsync(entity.TenantId, cancellationToken);
        return MapPortal(entity, link.ExpiresAt, settings.AllowDestinationSummaryInVendorPortal);
    }

    public async Task<VendorOrderBrokerDecisionResponse> CreateBrokerDecisionAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid vendorOrderId,
        CreateVendorOrderBrokerDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, vendorOrderId, cancellationToken);
        var decisionType = NormalizeDecisionType(request.DecisionType);
        if (!string.Equals(entity.Status, VendorOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "vendor_order.partial_decision_requires_partially_ready",
                "Partial dispatch decisions require a partially ready vendor order.",
                409);
        }

        if (string.Equals(decisionType, VendorOrderBrokerDecisionTypes.DispatchPartial, StringComparison.OrdinalIgnoreCase))
        {
            if (!request.SelectedTripId.HasValue)
            {
                throw new StlApiException(
                    "vendor_order.partial_decision.trip_required",
                    "Dispatch partial decisions require a selected RoutArr trip.",
                    400);
            }

            var authorizedQuantity = request.AuthorizedQuantity ?? entity.QuantityReady;
            if (authorizedQuantity <= 0 || authorizedQuantity > entity.QuantityReady)
            {
                throw new StlApiException(
                    "vendor_order.partial_decision.quantity_invalid",
                    "Authorized quantity must be greater than zero and no more than the quantity ready.",
                    400);
            }
        }

        var decision = new VendorOrderBrokerDecision
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VendorOrderId = entity.Id,
            DecisionType = decisionType,
            AuthorizedQuantity = request.AuthorizedQuantity,
            SelectedTripId = request.SelectedTripId,
            Note = NormalizeOptionalText(request.Note, 1024),
            DecidedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        db.VendorOrderBrokerDecisions.Add(decision);
        entity.UpdatedAt = decision.CreatedAt;
        await db.SaveChangesAsync(cancellationToken);

        if (string.Equals(decision.DecisionType, VendorOrderBrokerDecisionTypes.DispatchPartial, StringComparison.OrdinalIgnoreCase))
        {
            await integrationOutbox.TryEnqueueAsync(
                tenantId,
                IntegrationOutboxEventKinds.VendorOrderPartialDispatchAuthorized,
                "vendor_order_broker_decision",
                decision.Id,
                new IntegrationOutboxPayload(tenantId, $"Vendor order partial dispatch authorized: {entity.Id}", entity.VendorId),
                cancellationToken: cancellationToken);
        }

        await audit.WriteAsync(
            "vendor_order.partial_decision",
            tenantId,
            actorUserId,
            "vendor_order_broker_decision",
            decision.Id.ToString(),
            decision.DecisionType,
            cancellationToken: cancellationToken);

        return MapDecision(decision);
    }

    public async Task<SplitVendorOrderResponse> SplitRemainingAsync(
        Guid tenantId,
        Guid actorUserId,
        string actorPersonId,
        Guid vendorOrderId,
        SplitVendorOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadTrackedAsync(tenantId, vendorOrderId, cancellationToken);
        if (!string.Equals(entity.Status, VendorOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "vendor_order.split_requires_partially_ready",
                "Only partially ready vendor orders can be split.",
                409);
        }

        if (entity.QuantityReady <= 0 || entity.QuantityReady >= entity.OrderedQuantity)
        {
            throw new StlApiException(
                "vendor_order.split_quantity_invalid",
                "Split remaining requires a partial quantity that is greater than zero and less than the ordered quantity.",
                409);
        }

        var splitReason = NormalizeRequiredText(
            request.SplitReason ?? entity.SplitReason ?? "Split remaining quantity",
            512,
            "vendor_order.split_reason_required");

        var latestStatusUpdate = entity.StatusUpdates
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault(x => string.Equals(x.NewStatus, VendorOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase))
            ?? throw new StlApiException(
                "vendor_order.split_status_update_missing",
                "A partial readiness status update is required before splitting the order.",
                409);

        var now = DateTimeOffset.UtcNow;
        var readyChild = CloneForSplit(
            entity,
            now,
            actorPersonId,
            entity.QuantityReady,
            0,
            VendorOrderStatuses.CompletedReadyForDispatch,
            splitReason,
            latestStatusUpdate.Id,
            entity.ConfirmedReadyAt ?? now);

        var remainingQuantity = entity.OrderedQuantity - entity.QuantityReady;
        var remainingStatus = string.Equals(entity.Status, VendorOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase)
            ? VendorOrderStatuses.InProgress
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

        db.VendorOrders.AddRange(readyChild, remainingChild);

        entity.Status = VendorOrderStatuses.Split;
        entity.SplitReason = splitReason;
        entity.ClosedAt = now;
        entity.UpdatedAt = now;

        foreach (var link in entity.MagicLinks.Where(x => x.RevokedAt is null))
        {
            link.RevokedAt = now;
        }

        var splitDecision = new VendorOrderBrokerDecision
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VendorOrderId = entity.Id,
            DecisionType = VendorOrderBrokerDecisionTypes.SplitRemaining,
            AuthorizedQuantity = entity.QuantityReady,
            SelectedTripId = request.SelectedTripId,
            Note = splitReason,
            DecidedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = now,
        };
        db.VendorOrderBrokerDecisions.Add(splitDecision);

        var readyStatusUpdate = new VendorOrderStatusUpdate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VendorOrderId = readyChild.Id,
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
            Source = VendorOrderStatusUpdateSources.System,
            SubmittedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = now,
        };
        var remainingStatusUpdate = new VendorOrderStatusUpdate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VendorOrderId = remainingChild.Id,
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
            Source = VendorOrderStatusUpdateSources.System,
            SubmittedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = now,
        };
        db.VendorOrderStatusUpdates.AddRange(readyStatusUpdate, remainingStatusUpdate);

        await db.SaveChangesAsync(cancellationToken);

        await EnqueueStatusEventsAsync(readyChild, readyStatusUpdate, cancellationToken);
        await integrationOutbox.TryEnqueueAsync(
            tenantId,
            IntegrationOutboxEventKinds.VendorOrderSplitCreated,
            "vendor_order_broker_decision",
            splitDecision.Id,
            new IntegrationOutboxPayload(tenantId, $"Vendor order split created: {entity.Id}", entity.VendorId),
            cancellationToken: cancellationToken);

        var remainingLink = await CreateMagicLinkInternalAsync(tenantId, actorPersonId, remainingChild, cancellationToken);

        await audit.WriteAsync(
            "vendor_order.split_remaining",
            tenantId,
            actorUserId,
            "vendor_order",
            entity.Id.ToString(),
            VendorOrderStatuses.Split,
            cancellationToken: cancellationToken);

        return new SplitVendorOrderResponse(
            await GetAsync(tenantId, entity.Id, cancellationToken),
            await GetAsync(tenantId, readyChild.Id, cancellationToken),
            await GetAsync(tenantId, remainingChild.Id, cancellationToken),
            remainingLink.Token,
            remainingLink.Url);
    }

    public async Task<IntegrationVendorOrderResponse> GetForIntegrationAsync(
        Guid tenantId,
        Guid vendorOrderId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.VendorOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == vendorOrderId, cancellationToken)
            ?? throw new StlApiException("vendor_order.not_found", "Vendor order was not found.", 404);

        return MapIntegration(entity);
    }

    public async Task<IReadOnlyList<IntegrationVendorOrderResponse>> SearchForIntegrationAsync(
        Guid tenantId,
        Guid? brokerOrderId,
        Guid? vendorId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.VendorOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (brokerOrderId.HasValue)
        {
            query = query.Where(x => x.BrokerOrderId == brokerOrderId.Value);
        }

        if (vendorId.HasValue)
        {
            query = query.Where(x => x.VendorId == vendorId.Value);
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

    private async Task<CreateVendorOrderMagicLinkResponse> CreateMagicLinkInternalAsync(
        Guid tenantId,
        string actorPersonId,
        VendorOrder entity,
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
        var magicLink = new VendorOrderMagicLink
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            VendorOrderId = entity.Id,
            VendorId = entity.VendorId,
            TokenHash = tokenHash,
            ExpiresAt = now.AddHours(settings.MagicLinkTtlHours),
            CreatedByPersonId = NormalizeOptionalText(actorPersonId, 128),
            CreatedAt = now,
        };

        db.VendorOrderMagicLinks.Add(magicLink);
        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return new CreateVendorOrderMagicLinkResponse(
            magicLink.Id,
            token,
            BuildVendorPortalUrl(token),
            magicLink.ExpiresAt);
    }

    private async Task<(VendorOrder Entity, VendorOrderMagicLink Link)> ResolveMagicLinkAsync(
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new StlApiException("vendor_order.magic_link_required", "Magic link token is required.", 400);
        }

        var tokenHash = HashSubmissionValue(token);
        var now = DateTimeOffset.UtcNow;
        var link = await db.VendorOrderMagicLinks
            .Include(x => x.VendorOrder!)
                .ThenInclude(x => x.Documents)
            .Include(x => x.VendorOrder!)
                .ThenInclude(x => x.BrokerDecisions)
            .Include(x => x.VendorOrder!)
                .ThenInclude(x => x.StatusUpdates)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken)
            ?? throw new StlApiException("vendor_order.magic_link_not_found", "Vendor access link is invalid.", 404);

        if (link.RevokedAt.HasValue || link.ExpiresAt <= now)
        {
            throw new StlApiException("vendor_order.magic_link_expired", "Vendor access link has expired.", 410);
        }

        link.LastUsedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var order = link.VendorOrder
            ?? throw new StlApiException("vendor_order.magic_link_not_found", "Vendor access link is invalid.", 404);

        return (order, link);
    }

    private async Task<VendorOrderDocumentLink> RegisterDocumentInternalAsync(
        VendorOrder entity,
        RegisterVendorOrderDocumentRequest request,
        string? actorPersonId,
        Guid? uploadedByMagicLinkId,
        CancellationToken cancellationToken)
    {
        var documentType = NormalizeDocumentType(request.DocumentType);
        var fileName = NormalizeRequiredText(request.FileName, 256, "vendor_order.document_filename_required");
        var contentType = NormalizeRequiredText(request.ContentType, 128, "vendor_order.document_content_type_required");

        var uploadedByPersonId = actorPersonId ?? "vendor-magic-link";
        var record = await recordArrClient.RegisterDocumentAsync(
            new RecordArrVendorOrderRecordCreateRequest(
                Title: $"{documentType}: {entity.ItemDescription}",
                Description: $"Vendor order {entity.Id} document {fileName}",
                RecordType: "vendor_order_document",
                DocumentType: documentType,
                Classification: "operational",
                SourceProduct: "supplyarr",
                SourceObjectType: "vendor_order",
                SourceObjectId: entity.Id.ToString(),
                SourceObjectDisplayName: entity.ItemDescription,
                OwnerPersonId: entity.CreatedByPersonId ?? uploadedByPersonId,
                UploadedByPersonId: uploadedByPersonId,
                CurrentFileName: fileName,
                CurrentMimeType: contentType),
            new RecordArrVendorOrderFileCreateRequest(
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
        var document = new VendorOrderDocumentLink
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            VendorOrderId = entity.Id,
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

        db.VendorOrderDocumentLinks.Add(document);
        entity.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return document;
    }

    private async Task<VendorOrderStatusUpdate> ApplyStatusUpdateInternalAsync(
        VendorOrder entity,
        UpdateVendorOrderStatusRequest request,
        string source,
        string? submittedByPersonId,
        Guid? submittedByVendorContactId,
        Guid? submittedByMagicLinkId,
        string? submittedIpHash,
        string? submittedUserAgentHash,
        bool isVendorScoped,
        CancellationToken cancellationToken)
    {
        var previousStatus = entity.Status;
        var newStatus = NormalizeStatus(request.NewStatus);
        var quantityReady = request.QuantityReady ?? entity.QuantityReady;
        if (quantityReady < 0)
        {
            throw new StlApiException(
                "vendor_order.quantity_ready_invalid",
                "Quantity ready must be zero or greater.",
                400);
        }

        if (quantityReady > entity.OrderedQuantity)
        {
            throw new StlApiException(
                "vendor_order.quantity_ready_exceeds_ordered",
                "Quantity ready cannot exceed the ordered quantity.",
                400);
        }

        if (isVendorScoped)
        {
            EnsureVendorCanTransition(previousStatus, newStatus);
            if (string.Equals(newStatus, VendorOrderStatuses.CompletedReadyForDispatch, StringComparison.OrdinalIgnoreCase)
                && !request.ReadyForPickupConfirmed)
            {
                throw new StlApiException(
                    "vendor_order.ready_checkbox_required",
                    "Ready for pickup confirmation is required before marking the order ready for dispatch.",
                    400);
            }
        }
        else if (!VendorOrderStatuses.All.Contains(newStatus))
        {
            throw new StlApiException("vendor_order.invalid_status", "Vendor order status is invalid.", 400);
        }

        if (string.Equals(newStatus, VendorOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase)
            && (quantityReady <= 0 || quantityReady >= entity.OrderedQuantity))
        {
            throw new StlApiException(
                "vendor_order.partial_quantity_invalid",
                "Partially ready requires a quantity that is greater than zero and less than the ordered quantity.",
                400);
        }

        if (string.Equals(newStatus, VendorOrderStatuses.CompletedReadyForDispatch, StringComparison.OrdinalIgnoreCase)
            && quantityReady <= 0)
        {
            throw new StlApiException(
                "vendor_order.ready_quantity_invalid",
                "Ready for dispatch requires a quantity that is greater than zero.",
                400);
        }

        var quantityRemaining = Math.Max(0, entity.OrderedQuantity - quantityReady);
        var confirmedReadyAt = request.ConfirmedReadyAt ?? entity.ConfirmedReadyAt;
        if (string.Equals(newStatus, VendorOrderStatuses.CompletedReadyForDispatch, StringComparison.OrdinalIgnoreCase)
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
        if (string.Equals(newStatus, VendorOrderStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            entity.CancelledAt ??= now;
        }

        if (string.Equals(newStatus, VendorOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(newStatus, VendorOrderStatuses.Split, StringComparison.OrdinalIgnoreCase))
        {
            entity.ClosedAt ??= now;
        }

        var statusUpdate = new VendorOrderStatusUpdate
        {
            Id = Guid.NewGuid(),
            TenantId = entity.TenantId,
            VendorOrderId = entity.Id,
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
            SubmittedByVendorContactId = submittedByVendorContactId,
            SubmittedByPersonId = NormalizeOptionalText(submittedByPersonId, 128),
            SubmittedByMagicLinkId = submittedByMagicLinkId,
            SubmittedIpHash = NormalizeOptionalText(submittedIpHash, 128),
            SubmittedUserAgentHash = NormalizeOptionalText(submittedUserAgentHash, 128),
            CreatedAt = now,
        };

        db.VendorOrderStatusUpdates.Add(statusUpdate);
        await db.SaveChangesAsync(cancellationToken);

        return statusUpdate;
    }

    private async Task EnqueueStatusEventsAsync(
        VendorOrder entity,
        VendorOrderStatusUpdate statusUpdate,
        CancellationToken cancellationToken)
    {
        await integrationOutbox.TryEnqueueAsync(
            entity.TenantId,
            IntegrationOutboxEventKinds.VendorOrderStatusChanged,
            "vendor_order_status_update",
            statusUpdate.Id,
            new IntegrationOutboxPayload(
                entity.TenantId,
                $"Vendor order status changed to {statusUpdate.NewStatus}: {entity.Id}",
                entity.VendorId),
            cancellationToken: cancellationToken);

        if (string.Equals(statusUpdate.NewStatus, VendorOrderStatuses.CompletedReadyForDispatch, StringComparison.OrdinalIgnoreCase))
        {
            await integrationOutbox.TryEnqueueAsync(
                entity.TenantId,
                IntegrationOutboxEventKinds.VendorOrderCompletedForDispatch,
                "vendor_order_status_update",
                statusUpdate.Id,
                new IntegrationOutboxPayload(
                    entity.TenantId,
                    $"Vendor order ready for dispatch: {entity.Id}",
                    entity.VendorId),
                cancellationToken: cancellationToken);
        }
    }

    private static void EnsureVendorCanTransition(string previousStatus, string newStatus)
    {
        if (string.Equals(previousStatus, newStatus, StringComparison.OrdinalIgnoreCase)
            && (string.Equals(newStatus, VendorOrderStatuses.Acknowledged, StringComparison.OrdinalIgnoreCase)
                || string.Equals(newStatus, VendorOrderStatuses.InProgress, StringComparison.OrdinalIgnoreCase)
                || string.Equals(newStatus, VendorOrderStatuses.PartiallyReady, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var allowed = previousStatus.ToLowerInvariant() switch
        {
            VendorOrderStatuses.PendingVendorAcknowledgment => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                VendorOrderStatuses.Acknowledged,
                VendorOrderStatuses.UnableToFulfill,
            },
            VendorOrderStatuses.Acknowledged => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                VendorOrderStatuses.Acknowledged,
                VendorOrderStatuses.InProgress,
                VendorOrderStatuses.PartiallyReady,
                VendorOrderStatuses.CompletedReadyForDispatch,
                VendorOrderStatuses.UnableToFulfill,
            },
            VendorOrderStatuses.InProgress => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                VendorOrderStatuses.InProgress,
                VendorOrderStatuses.PartiallyReady,
                VendorOrderStatuses.CompletedReadyForDispatch,
                VendorOrderStatuses.UnableToFulfill,
            },
            VendorOrderStatuses.PartiallyReady => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                VendorOrderStatuses.PartiallyReady,
                VendorOrderStatuses.CompletedReadyForDispatch,
                VendorOrderStatuses.UnableToFulfill,
            },
            _ => [],
        };

        if (!allowed.Contains(newStatus))
        {
            throw new StlApiException(
                "vendor_order.invalid_vendor_transition",
                $"Vendors cannot move a vendor order from {previousStatus} to {newStatus}.",
                409);
        }
    }

    private static string BuildVendorPortalUrl(string token) =>
        $"/supplier-order-portal/orders/{Uri.EscapeDataString(token)}";

    private static VendorOrder CloneForSplit(
        VendorOrder source,
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
        return new VendorOrder
        {
            Id = Guid.NewGuid(),
            TenantId = source.TenantId,
            BrokerOrderId = source.BrokerOrderId,
            BrokerOrderNumberSnapshot = source.BrokerOrderNumberSnapshot,
            VendorId = source.VendorId,
            VendorNameSnapshot = source.VendorNameSnapshot,
            VendorLocationId = source.VendorLocationId,
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
            ParentVendorOrderId = source.Id,
            SplitReason = splitReason,
            SplitFromStatusUpdateId = splitFromStatusUpdateId,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    private async Task<VendorOrder> LoadAsync(
        Guid tenantId,
        Guid vendorOrderId,
        CancellationToken cancellationToken)
    {
        var entity = await db.VendorOrders
            .AsNoTracking()
            .Include(x => x.Vendor)
                .ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.Documents)
            .Include(x => x.BrokerDecisions)
            .Include(x => x.StatusUpdates)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == vendorOrderId, cancellationToken)
            ?? throw new StlApiException("vendor_order.not_found", "Vendor order was not found.", 404);

        return entity;
    }

    private async Task<VendorOrder> LoadTrackedAsync(
        Guid tenantId,
        Guid vendorOrderId,
        CancellationToken cancellationToken)
    {
        var entity = await db.VendorOrders
            .Include(x => x.Vendor)
                .ThenInclude(x => x!.ParentExternalParty)
            .Include(x => x.Documents)
            .Include(x => x.BrokerDecisions)
            .Include(x => x.StatusUpdates)
            .Include(x => x.MagicLinks)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == vendorOrderId, cancellationToken)
            ?? throw new StlApiException("vendor_order.not_found", "Vendor order was not found.", 404);

        return entity;
    }

    private async Task EnsureExistsAsync(Guid tenantId, Guid vendorOrderId, CancellationToken cancellationToken)
    {
        if (!await db.VendorOrders.AnyAsync(x => x.TenantId == tenantId && x.Id == vendorOrderId, cancellationToken))
        {
            throw new StlApiException("vendor_order.not_found", "Vendor order was not found.", 404);
        }
    }

    private async Task<ExternalParty> EnsureVendorAsync(
        Guid tenantId,
        Guid vendorId,
        CancellationToken cancellationToken)
    {
        var vendor = await db.ExternalParties
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == vendorId, cancellationToken)
            ?? throw new StlApiException("vendor_order.vendor_not_found", "Vendor was not found.", 404);

        return vendor;
    }

    private static void EnsureInternallyEditable(VendorOrder entity)
    {
        if (string.Equals(entity.Status, VendorOrderStatuses.Cancelled, StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, VendorOrderStatuses.Closed, StringComparison.OrdinalIgnoreCase)
            || string.Equals(entity.Status, VendorOrderStatuses.Split, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "vendor_order.not_editable",
                "Closed, cancelled, or split vendor orders cannot be modified.",
                409);
        }
    }

    private static VendorOrderListItemResponse MapListItem(VendorOrder entity) =>
        new(
            entity.Id,
            entity.Status,
            entity.VendorId,
            entity.VendorNameSnapshot,
            entity.Vendor?.ParentExternalPartyId,
            entity.Vendor?.ParentExternalParty?.DisplayName,
            entity.Vendor?.UnitKind ?? "identity",
            ParseServiceTypes(entity.Vendor?.ServiceTypesJson),
            entity.VendorNameSnapshot,
            entity.ItemDescription,
            entity.OrderedQuantity,
            entity.QuantityReady,
            entity.QuantityRemaining,
            entity.QuantityUom,
            entity.ExpectedReadyAt,
            entity.ConfirmedReadyAt,
            entity.ParentVendorOrderId,
            entity.UpdatedAt);

    private static VendorOrderResponse Map(VendorOrder entity) =>
        new(
            entity.Id,
            entity.VendorId,
            entity.VendorNameSnapshot,
            entity.Vendor?.ParentExternalPartyId,
            entity.Vendor?.ParentExternalParty?.DisplayName,
            entity.Vendor?.UnitKind ?? "identity",
            ParseServiceTypes(entity.Vendor?.ServiceTypesJson),
            entity.BrokerOrderId,
            entity.BrokerOrderNumberSnapshot,
            entity.VendorId,
            entity.VendorNameSnapshot,
            entity.VendorLocationId,
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
            entity.ParentVendorOrderId,
            entity.SplitReason,
            entity.SplitFromStatusUpdateId,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CancelledAt,
            entity.ClosedAt,
            entity.Documents.OrderByDescending(x => x.UploadedAt).Select(MapDocument).ToList(),
            entity.BrokerDecisions.OrderByDescending(x => x.CreatedAt).Select(MapDecision).ToList(),
            entity.StatusUpdates.OrderBy(x => x.CreatedAt).Select(MapStatusUpdate).ToList());

    private static VendorOrderMetadataResponse BuildMetadata() =>
        new(
            FilterStatusOptions: Options(
                "supplyarr",
                "supplyarr.vendor_order.workflow",
                VendorOrderStatuses.Draft,
                VendorOrderStatuses.SentToVendor,
                VendorOrderStatuses.PendingVendorAcknowledgment,
                VendorOrderStatuses.Acknowledged,
                VendorOrderStatuses.InProgress,
                VendorOrderStatuses.PartiallyReady,
                VendorOrderStatuses.CompletedReadyForDispatch,
                VendorOrderStatuses.UnableToFulfill,
                VendorOrderStatuses.Cancelled,
                VendorOrderStatuses.Closed,
                VendorOrderStatuses.Split),
            InternalStatusOptions: Options(
                "supplyarr",
                "supplyarr.vendor_order.workflow",
                VendorOrderStatuses.Draft,
                VendorOrderStatuses.SentToVendor,
                VendorOrderStatuses.PendingVendorAcknowledgment,
                VendorOrderStatuses.Acknowledged,
                VendorOrderStatuses.InProgress,
                VendorOrderStatuses.PartiallyReady,
                VendorOrderStatuses.CompletedReadyForDispatch,
                VendorOrderStatuses.UnableToFulfill,
                VendorOrderStatuses.Cancelled,
                VendorOrderStatuses.Closed),
            VendorPortalStatusOptions: Options(
                "supplyarr",
                "supplyarr.vendor_order.workflow",
                VendorOrderStatuses.Acknowledged,
                VendorOrderStatuses.InProgress,
                VendorOrderStatuses.PartiallyReady,
                VendorOrderStatuses.CompletedReadyForDispatch,
                VendorOrderStatuses.UnableToFulfill),
            DocumentTypeOptions: Options(
                "recordarr",
                "recordarr.document_type_catalog.mapped_to_supplyarr",
                VendorOrderDocumentTypes.Photo,
                VendorOrderDocumentTypes.PackingSlip,
                VendorOrderDocumentTypes.BillOfLading,
                VendorOrderDocumentTypes.ScaleTicket,
                VendorOrderDocumentTypes.ProofOfReadiness,
                VendorOrderDocumentTypes.Other),
            BrokerDecisionTypeOptions: Options(
                "supplyarr",
                "supplyarr.vendor_order.partial_decision",
                VendorOrderBrokerDecisionTypes.WaitFull,
                VendorOrderBrokerDecisionTypes.DispatchPartial,
                VendorOrderBrokerDecisionTypes.SplitRemaining));

    private static IReadOnlyList<VendorOrderCatalogOptionResponse> Options(
        string owner,
        string sourceOfTruth,
        params string[] values) =>
        values.Select(value => new VendorOrderCatalogOptionResponse(value, Humanize(value), owner, sourceOfTruth)).ToList();

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

    private static VendorOrderPortalResponse MapPortal(
        VendorOrder entity,
        DateTimeOffset expiresAt,
        bool includeDestinationSummary) =>
        new(
            entity.Id,
            entity.Status,
            entity.VendorId,
            entity.VendorNameSnapshot,
            entity.Vendor?.ParentExternalPartyId,
            entity.Vendor?.ParentExternalParty?.DisplayName,
            entity.Vendor?.UnitKind ?? "identity",
            ParseServiceTypes(entity.Vendor?.ServiceTypesJson),
            entity.VendorNameSnapshot,
            entity.PickupLocationNameSnapshot ?? entity.VendorNameSnapshot,
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

    private static IntegrationVendorOrderResponse MapIntegration(VendorOrder entity) =>
        new(
            entity.Id,
            entity.BrokerOrderId,
            entity.BrokerOrderNumberSnapshot,
            entity.VendorId,
            entity.VendorNameSnapshot,
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

    private static VendorOrderStatusUpdateResponse MapStatusUpdate(VendorOrderStatusUpdate update) =>
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

    private static VendorOrderDocumentResponse MapDocument(VendorOrderDocumentLink document) =>
        new(
            document.Id,
            document.DocumentType,
            document.FileName,
            document.ContentType,
            document.RecordArrRecordId,
            document.RecordArrRecordNumberSnapshot,
            document.RecordArrFileId,
            document.UploadedAt);

    private static VendorOrderBrokerDecisionResponse MapDecision(VendorOrderBrokerDecision decision) =>
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
        if (!VendorOrderStatuses.All.Contains(normalized))
        {
            throw new StlApiException("vendor_order.invalid_status", "Vendor order status is invalid.", 400);
        }

        return normalized;
    }

    private static string NormalizeDecisionType(string decisionType)
    {
        var normalized = decisionType.Trim().ToLowerInvariant();
        if (!VendorOrderBrokerDecisionTypes.All.Contains(normalized))
        {
            throw new StlApiException("vendor_order.invalid_decision", "Vendor order broker decision is invalid.", 400);
        }

        return normalized;
    }

    private static string NormalizeDocumentType(string documentType)
    {
        var normalized = documentType.Trim().ToLowerInvariant();
        if (!VendorOrderDocumentTypes.All.Contains(normalized))
        {
            throw new StlApiException("vendor_order.invalid_document_type", "Vendor order document type is invalid.", 400);
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
            ? VendorOrderDefaults.DefaultQuantityUom
            : value.Trim().ToLowerInvariant();

        if (normalized.Length > 32)
        {
            throw new StlApiException(
                "vendor_order.quantity_uom_too_long",
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
            throw new StlApiException("vendor_order.field_too_long", $"Field must be {maxLength} characters or fewer.", 400);
        }

        return normalized;
    }
}

public sealed class VendorOrderService(
    SupplyArrDbContext db,
    VendorOrderSettingsService settingsService,
    IntegrationOutboxEnqueueService integrationOutbox,
    RecordArrVendorOrderClient recordArrClient,
    ISupplyArrAuditService audit)
    : SupplierOrderService(db, settingsService, integrationOutbox, recordArrClient, audit);
