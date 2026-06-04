using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class MaintenanceVendorWork : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid WorkOrderId { get; set; }

    public string SupplierRef { get; set; } = string.Empty;

    public string? VendorContactSnapshot { get; set; }

    public string Status { get; set; } = "requested";

    public string? WorkDescription { get; set; }

    public string? QuoteRecordRef { get; set; }

    public string? ApprovalRef { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public string? CostEstimateSnapshot { get; set; }

    public string? InvoiceRecordRef { get; set; }

    public bool WarrantyFlag { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
