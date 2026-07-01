using STLCompliance.Shared.Data;

namespace SupplyArr.Api.Entities;

public sealed class SupplierEmailInboxMessage : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string MessageKey { get; set; } = string.Empty;

    public string MessageKind { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string BodyPreview { get; set; } = string.Empty;

    public string MatchStatus { get; set; } = SupplierEmailInboxMatchStatuses.Unmatched;

    public string MatchReason { get; set; } = string.Empty;

    public Guid? SupplierId { get; set; }

    public string? SupplierKey { get; set; }

    public string? SupplierDisplayName { get; set; }

    public string? LinkedReferenceType { get; set; }

    public Guid? LinkedReferenceId { get; set; }

    public string? LinkedReferenceKey { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}

public static class SupplierEmailInboxMessageKinds
{
    public const string QuoteReceived = "quote_received";

    public const string OrderConfirmationReceived = "order_confirmation_received";
}

public static class SupplierEmailInboxMatchStatuses
{
    public const string Matched = "matched";

    public const string Unmatched = "unmatched";
}

