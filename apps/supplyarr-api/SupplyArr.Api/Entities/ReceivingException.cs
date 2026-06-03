using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class ReceivingException : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ReceivingReceiptId { get; set; }

    public Guid ReceivingReceiptLineId { get; set; }

    public string ExceptionType { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = ReceivingExceptionStatuses.Open;

    public Guid CreatedByUserId { get; set; }

    public Guid? ResolvedByUserId { get; set; }

    public DateTimeOffset? ResolvedAt { get; set; }

    public Guid? CancelledByUserId { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }

    public string CancellationReason { get; set; } = string.Empty;

    public Guid? ReopenedByUserId { get; set; }

    public DateTimeOffset? ReopenedAt { get; set; }

    public string LastReopenReason { get; set; } = string.Empty;

    public int ReopenCount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ReceivingReceipt ReceivingReceipt { get; set; } = null!;

    public ReceivingReceiptLine ReceivingReceiptLine { get; set; } = null!;
}
