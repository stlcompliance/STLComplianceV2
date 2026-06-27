namespace MaintainArr.Api.Contracts;

public sealed record MaintenanceVendorWorkListResponse(
    IReadOnlyList<MaintenanceVendorWorkResponse> Items);

public sealed record IngestSupplierWorkStatusRequest(
    Guid TenantId,
    Guid WorkOrderId,
    string SupplierRef,
    string? VendorContactSnapshot,
    string Status,
    string? WorkDescription,
    string? QuoteRecordRef,
    string? ApprovalRef,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? CompletedAt,
    string? CostEstimateSnapshot,
    string? InvoiceRecordRef,
    bool WarrantyFlag,
    string? Notes,
    DateTimeOffset OccurredAt);

public sealed record UpsertMaintenanceVendorWorkRequest(
    string SupplierRef,
    string? VendorContactSnapshot,
    string Status,
    string? WorkDescription,
    string? QuoteRecordRef,
    string? ApprovalRef,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? CompletedAt,
    string? CostEstimateSnapshot,
    string? InvoiceRecordRef,
    bool WarrantyFlag,
    string? Notes);

public sealed record MaintenanceVendorWorkResponse(
    Guid VendorWorkId,
    Guid WorkOrderId,
    string SupplierRef,
    string? VendorContactSnapshot,
    string Status,
    string? WorkDescription,
    string? QuoteRecordRef,
    string? ApprovalRef,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? CompletedAt,
    string? CostEstimateSnapshot,
    string? InvoiceRecordRef,
    bool WarrantyFlag,
    string? Notes,
    string? PortalAccessCode,
    DateTimeOffset? PortalAccessCodeIssuedAt,
    DateTimeOffset? PortalAccessExpiresAt,
    DateTimeOffset? PortalAccessOpenedAt,
    DateTimeOffset? PortalAccessRevokedAt,
    string PortalAccessStatus,
    string? PortalAccessUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    bool Duplicate);

public sealed record UpdateMaintenanceVendorWorkPortalRequest(
    string Status,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? CompletedAt,
    string? Notes);

public sealed record MaintenanceVendorWorkPortalResponse(
    Guid VendorWorkId,
    Guid WorkOrderId,
    string WorkOrderNumber,
    string WorkOrderTitle,
    string WorkOrderPriority,
    string WorkOrderStatus,
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string SupplierRef,
    string? VendorContactSnapshot,
    string Status,
    string? WorkDescription,
    string? QuoteRecordRef,
    string? ApprovalRef,
    DateTimeOffset? ScheduledAt,
    DateTimeOffset? CompletedAt,
    string? CostEstimateSnapshot,
    string? InvoiceRecordRef,
    bool WarrantyFlag,
    string? Notes,
    DateTimeOffset? PortalAccessExpiresAt,
    string PortalAccessStatus,
    IReadOnlyList<string> AllowedActions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
