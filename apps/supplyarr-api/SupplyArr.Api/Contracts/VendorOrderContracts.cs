namespace SupplyArr.Api.Contracts;

public sealed record VendorOrderStatusUpdateResponse(
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

public sealed record VendorOrderDocumentResponse(
    Guid DocumentId,
    string DocumentType,
    string FileName,
    string ContentType,
    string RecordArrRecordId,
    string RecordArrRecordNumberSnapshot,
    string RecordArrFileId,
    DateTimeOffset UploadedAt);

public sealed record VendorOrderBrokerDecisionResponse(
    Guid DecisionId,
    string DecisionType,
    decimal? AuthorizedQuantity,
    Guid? SelectedTripId,
    string? Note,
    string? DecidedByPersonId,
    DateTimeOffset CreatedAt);

public sealed record VendorOrderCatalogOptionResponse(
    string Value,
    string Label,
    string Owner,
    string SourceOfTruth);

public sealed record VendorOrderMetadataResponse(
    IReadOnlyList<VendorOrderCatalogOptionResponse> FilterStatusOptions,
    IReadOnlyList<VendorOrderCatalogOptionResponse> InternalStatusOptions,
    IReadOnlyList<VendorOrderCatalogOptionResponse> VendorPortalStatusOptions,
    IReadOnlyList<VendorOrderCatalogOptionResponse> DocumentTypeOptions,
    IReadOnlyList<VendorOrderCatalogOptionResponse> BrokerDecisionTypeOptions);

public sealed record VendorOrderResponse(
    Guid VendorOrderId,
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    Guid VendorId,
    string VendorNameSnapshot,
    Guid? VendorLocationId,
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
    Guid? ParentVendorOrderId,
    string? SplitReason,
    Guid? SplitFromStatusUpdateId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CancelledAt,
    DateTimeOffset? ClosedAt,
    IReadOnlyList<VendorOrderDocumentResponse> Documents,
    IReadOnlyList<VendorOrderBrokerDecisionResponse> BrokerDecisions,
    IReadOnlyList<VendorOrderStatusUpdateResponse> StatusHistory);

public sealed record VendorOrderListItemResponse(
    Guid VendorOrderId,
    string Status,
    string VendorNameSnapshot,
    string ItemDescription,
    decimal OrderedQuantity,
    decimal QuantityReady,
    decimal QuantityRemaining,
    string QuantityUom,
    DateTimeOffset? ExpectedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    Guid? ParentVendorOrderId,
    DateTimeOffset UpdatedAt);

public sealed record CreateVendorOrderRequest(
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    Guid VendorId,
    Guid? VendorLocationId,
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

public sealed record UpdateVendorOrderRequest(
    string? BrokerOrderNumberSnapshot,
    Guid? VendorLocationId,
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

public sealed record UpdateVendorOrderStatusRequest(
    string NewStatus,
    decimal? QuantityReady,
    DateTimeOffset? EstimatedReadyAt,
    DateTimeOffset? ConfirmedReadyAt,
    DateTimeOffset? PickupWindowStart,
    DateTimeOffset? PickupWindowEnd,
    string? Note,
    string? ExceptionReason,
    bool ReadyForPickupConfirmed);

public sealed record SendVendorOrderResponse(
    VendorOrderResponse VendorOrder,
    string MagicLinkUrl,
    DateTimeOffset ExpiresAt);

public sealed record CreateVendorOrderMagicLinkResponse(
    Guid MagicLinkId,
    string Token,
    string Url,
    DateTimeOffset ExpiresAt);

public sealed record RegisterVendorOrderDocumentRequest(
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

public sealed record VendorOrderPortalResponse(
    Guid VendorOrderId,
    string Status,
    string VendorNameSnapshot,
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
    IReadOnlyList<VendorOrderDocumentResponse> Documents,
    IReadOnlyList<VendorOrderStatusUpdateResponse> StatusHistory,
    VendorOrderMetadataResponse Metadata);

public sealed record CreateVendorOrderBrokerDecisionRequest(
    string DecisionType,
    decimal? AuthorizedQuantity,
    Guid? SelectedTripId,
    string? Note);

public sealed record SplitVendorOrderRequest(
    Guid? SelectedTripId,
    string? SplitReason,
    DateTimeOffset? RemainingExpectedReadyAt,
    DateTimeOffset? RemainingPickupWindowStart,
    DateTimeOffset? RemainingPickupWindowEnd);

public sealed record SplitVendorOrderResponse(
    VendorOrderResponse ParentVendorOrder,
    VendorOrderResponse ReadyVendorOrder,
    VendorOrderResponse RemainingVendorOrder,
    string RemainingVendorOrderToken,
    string RemainingVendorOrderUrl);

public sealed record VendorOrderSettingsResponse(
    bool AllowDestinationSummaryInVendorPortal,
    int MagicLinkTtlHours,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertVendorOrderSettingsRequest(
    bool AllowDestinationSummaryInVendorPortal,
    int MagicLinkTtlHours);

public sealed record IntegrationVendorOrderResponse(
    Guid VendorOrderId,
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    Guid VendorId,
    string VendorNameSnapshot,
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

public sealed record SupplyArrVendorOrderEventEnvelope(
    Guid EventId,
    string EventType,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid VendorOrderId,
    Guid? BrokerOrderId,
    string? BrokerOrderNumberSnapshot,
    string? PreviousStatus,
    string? NewStatus,
    Guid VendorId,
    string VendorNameSnapshot,
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
    Guid? ReadyChildVendorOrderId,
    Guid? RemainingChildVendorOrderId);
