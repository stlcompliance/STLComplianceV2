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

    public string? PortalAccessCode { get; set; }

    public DateTimeOffset? PortalAccessCodeIssuedAt { get; set; }

    public DateTimeOffset? PortalAccessExpiresAt { get; set; }

    public string PortalAccessStatus { get; set; } = MaintenanceVendorWorkPortalAccessStatuses.Draft;

    public DateTimeOffset? PortalAccessOpenedAt { get; set; }

    public DateTimeOffset? PortalAccessRevokedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public static class MaintenanceVendorWorkPortalAccessStatuses
{
    public const string Draft = "draft";
    public const string Sent = "sent";
    public const string Opened = "opened";
    public const string Used = "used";
    public const string Expired = "expired";
    public const string Revoked = "revoked";
}
