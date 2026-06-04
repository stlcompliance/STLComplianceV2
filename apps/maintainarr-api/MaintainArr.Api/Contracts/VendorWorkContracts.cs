namespace MaintainArr.Api.Contracts;

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
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    bool Duplicate);
