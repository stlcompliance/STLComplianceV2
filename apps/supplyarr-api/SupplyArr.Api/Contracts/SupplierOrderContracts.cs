namespace SupplyArr.Api.Contracts;

public sealed record SupplierOrderStatusUpdateResponse(
    Guid StatusUpdateId,
    string? PreviousStatus,
    string NewStatus,
    decimal OrderedQuantitySnapshot,
    decimal QuantityReady,
    decimal QuantityRemaining,
    DateTimeOffset? EstimatedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? Note,
    string? ExceptionReason,
    string Source,
    string? SubmittedByPersonId,
    DateTimeOffset CreatedAt);

public sealed record SupplierOrderDocumentResponse(
    Guid DocumentId,
    string DocumentType,
    string FileName,
    string ContentType,
    string RecordArrRecordId,
    string RecordArrRecordNumberSnapshot,
    string RecordArrFileId,
    DateTimeOffset UploadedAt);

public sealed record SupplierOrderBrokerDecisionResponse(
    Guid DecisionId,
    string DecisionType,
    decimal? AuthorizedQuantity,
    Guid? SelectedTripId,
    string? Note,
    string? DecidedByPersonId,
    DateTimeOffset CreatedAt);

public sealed record SupplierOrderCatalogOptionResponse(
    string Value,
    string Label,
    string Owner,
    string SourceOfTruth);

public sealed record SupplierOrderMetadataResponse(
    IReadOnlyList<SupplierOrderCatalogOptionResponse> FilterStatusOptions,
    IReadOnlyList<SupplierOrderCatalogOptionResponse> InternalStatusOptions,
    IReadOnlyList<SupplierOrderCatalogOptionResponse> SupplierPortalStatusOptions,
    IReadOnlyList<SupplierOrderCatalogOptionResponse> DocumentTypeOptions,
    IReadOnlyList<SupplierOrderCatalogOptionResponse> BrokerDecisionTypeOptions);

public sealed record SupplierOrderResponse(
    Guid SupplierOrderId,
    Guid SupplierId,
    string SupplierNameSnapshot,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    Guid? SupplierLocationId,
    string? PickupLocationNameSnapshot,
    string PickupAddressSnapshot,
    string? CustomerIdSnapshot,
    string? DeliveryLocationNameSnapshot,
    string? DeliveryAddressSnapshot,
    string ItemDescription,
    decimal OrderedQuantity,
    decimal QuantityReady,
    decimal QuantityRemaining,
    string QuantityUom,
    DateTimeOffset? ExpectedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? PickupInstructions,
    string Status,
    string? CreatedByPersonId,
    Guid? ParentSupplierOrderId,
    string? SplitReason,
    Guid? SplitFromStatusUpdateId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CancelledAt,
    DateTimeOffset? ClosedAt,
    IReadOnlyList<SupplierOrderDocumentResponse> Documents,
    IReadOnlyList<SupplierOrderBrokerDecisionResponse> BrokerDecisions,
    IReadOnlyList<SupplierOrderStatusUpdateResponse> StatusHistory);

public sealed record SupplierOrderListItemResponse(
    Guid SupplierOrderId,
    string Status,
    Guid SupplierId,
    string SupplierNameSnapshot,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string ItemDescription,
    decimal OrderedQuantity,
    decimal QuantityReady,
    decimal QuantityRemaining,
    string QuantityUom,
    DateTimeOffset? ExpectedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    Guid? ParentSupplierOrderId,
    DateTimeOffset UpdatedAt);

public sealed record CreateSupplierOrderRequest(
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    Guid SupplierId,
    Guid? SupplierLocationId,
    string? PickupLocationNameSnapshot,
    string PickupAddressSnapshot,
    string? CustomerIdSnapshot,
    string? DeliveryLocationNameSnapshot,
    string? DeliveryAddressSnapshot,
    string ItemDescription,
    decimal OrderedQuantity,
    string? QuantityUom,
    DateTimeOffset? ExpectedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? PickupInstructions);

public sealed record UpdateSupplierOrderRequest(
    string? BrokerOrderNumberSnapshot,
    Guid? SupplierLocationId,
    string? PickupLocationNameSnapshot,
    string PickupAddressSnapshot,
    string? CustomerIdSnapshot,
    string? DeliveryLocationNameSnapshot,
    string? DeliveryAddressSnapshot,
    string ItemDescription,
    decimal OrderedQuantity,
    string? QuantityUom,
    DateTimeOffset? ExpectedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? PickupInstructions);

public sealed record UpdateSupplierOrderStatusRequest(
    string NewStatus,
    decimal? QuantityReady,
    DateTimeOffset? EstimatedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? Note,
    string? ExceptionReason,
    bool ReadyForPickupConfirmed);

public sealed record SendSupplierOrderResponse(
    SupplierOrderResponse SupplierOrder,
    string MagicLinkUrl,
    DateTimeOffset ExpiresAt);

public sealed record CreateSupplierOrderMagicLinkResponse(
    Guid MagicLinkId,
    string Token,
    string Url,
    DateTimeOffset ExpiresAt);

public sealed record RegisterSupplierOrderDocumentRequest(
    string DocumentType,
    string FileName,
    string ContentType,
    string? StorageProvider,
    string? StorageKey,
    long? SizeBytes,
    int? PageCount,
    int? ImageWidth,
    int? ImageHeight,
    int? DurationSeconds);

public sealed record SupplierOrderPortalResponse(
    Guid SupplierOrderId,
    string Status,
    Guid SupplierId,
    string SupplierNameSnapshot,
    Guid? ParentSupplierId,
    string? ParentSupplierDisplayName,
    string SupplierUnitKind,
    IReadOnlyList<string> SupplierServiceTypes,
    string PickupLocationNameSnapshot,
    string PickupAddressSnapshot,
    string? DeliveryLocationNameSnapshot,
    string? DeliveryAddressSnapshot,
    string ItemDescription,
    decimal OrderedQuantity,
    decimal QuantityReady,
    decimal QuantityRemaining,
    string QuantityUom,
    DateTimeOffset? ExpectedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? PickupInstructions,
    DateTimeOffset LinkExpiresAt,
    IReadOnlyList<SupplierOrderDocumentResponse> Documents,
    IReadOnlyList<SupplierOrderStatusUpdateResponse> StatusHistory,
    SupplierOrderMetadataResponse Metadata);

public sealed record CreateSupplierOrderBrokerDecisionRequest(
    string DecisionType,
    decimal? AuthorizedQuantity,
    Guid? SelectedTripId,
    string? Note);

public sealed record SplitSupplierOrderRequest(
    Guid? SelectedTripId,
    string? SplitReason,
    DateTimeOffset? RemainingExpectedReadyAt,
    DateTimeOffset? RemainingPickupWindowStart,
    DateTimeOffset? RemainingPickupWindowEnd);

public sealed record SplitSupplierOrderResponse(
    SupplierOrderResponse ParentSupplierOrder,
    SupplierOrderResponse ReadySupplierOrder,
    SupplierOrderResponse RemainingSupplierOrder,
    string RemainingSupplierOrderToken,
    string RemainingSupplierOrderUrl);

public sealed record SupplierOrderSettingsResponse(
    bool AllowDestinationSummaryInSupplierPortal,
    int MagicLinkTtlHours,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertSupplierOrderSettingsRequest(
    bool AllowDestinationSummaryInSupplierPortal,
    int MagicLinkTtlHours);

public sealed record IntegrationSupplierOrderResponse(
    Guid SupplierOrderId,
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    Guid SupplierId,
    string SupplierNameSnapshot,
    string? PickupLocationNameSnapshot,
    string PickupAddressSnapshot,
    string? DeliveryLocationNameSnapshot,
    string? DeliveryAddressSnapshot,
    string ItemDescription,
    decimal OrderedQuantity,
    decimal QuantityReady,
    decimal QuantityRemaining,
    string QuantityUom,
    DateTimeOffset? ExpectedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? PickupInstructions,
    string Status,
    DateTimeOffset UpdatedAt);

public sealed record SupplyArrSupplierOrderEventEnvelope(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid SupplierOrderId,
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    string? PreviousStatus,
    string? NewStatus,
    Guid SupplierId,
    string SupplierNameSnapshot,
    string? PickupLocationNameSnapshot,
    string PickupAddressSnapshot,
    string? DeliveryLocationNameSnapshot,
    string? DeliveryAddressSnapshot,
    string ItemDescription,
    decimal OrderedQuantity,
    decimal QuantityReady,
    decimal QuantityRemaining,
    DateTimeOffset? ExpectedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? PickupInstructions,
    string? Source,
    Guid? SelectedTripId,
    decimal? AuthorizedQuantity,
    Guid? ReadyChildSupplierOrderId,
    Guid? RemainingChildSupplierOrderId);
