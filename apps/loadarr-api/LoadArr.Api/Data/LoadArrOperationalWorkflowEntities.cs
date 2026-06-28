namespace LoadArr.Api.Data;

public sealed class LoadArrReceivingSessionEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public string ReceivingNumber { get; set; } = string.Empty;

    public string ReceivingType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? ClientRequestId { get; set; }

    public string? RequestFingerprint { get; set; }

    public string? CompletedByPersonId { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public string PayloadJson { get; set; } = "{}";

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class LoadArrTransferOrderEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string OrderId { get; set; } = string.Empty;

    public string TransferNumber { get; set; } = string.Empty;

    public string TransferType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? ClientRequestId { get; set; }

    public string? RequestFingerprint { get; set; }

    public string? CompletedByPersonId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public string PayloadJson { get; set; } = "{}";

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class LoadArrInventoryOriginEventEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string OriginEventId { get; set; } = string.Empty;

    public string OriginType { get; set; } = string.Empty;

    public string OriginProductKey { get; set; } = string.Empty;

    public string OriginObjectType { get; set; } = string.Empty;

    public string OriginObjectId { get; set; } = string.Empty;

    public string WarehouseLocationId { get; set; } = string.Empty;

    public string SupplyarrItemId { get; set; } = string.Empty;

    public string PersonId { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string PayloadJson { get; set; } = "{}";
}

public sealed class LoadArrInventoryMovementEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string MovementId { get; set; } = string.Empty;

    public string MovementType { get; set; } = string.Empty;

    public string? FromLocationId { get; set; }

    public string ToLocationId { get; set; } = string.Empty;

    public string SupplyarrItemId { get; set; } = string.Empty;

    public string PersonId { get; set; } = string.Empty;

    public string RelatedObjectType { get; set; } = string.Empty;

    public string RelatedObjectId { get; set; } = string.Empty;

    public string? InventoryOriginEventId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string PayloadJson { get; set; } = "{}";
}

public sealed class LoadArrInventoryBalanceEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string BalanceId { get; set; } = string.Empty;

    public string SupplyarrItemId { get; set; } = string.Empty;

    public string LocationId { get; set; } = string.Empty;

    public string LotCode { get; set; } = string.Empty;

    public string SerialCode { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public decimal QuantityOnHand { get; set; }

    public decimal QuantityReserved { get; set; }

    public decimal QuantityAllocated { get; set; }

    public decimal QuantityBlocked { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public string PayloadJson { get; set; } = "{}";
}

public sealed class LoadArrWarehouseTaskEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string TaskId { get; set; } = string.Empty;

    public string TaskType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string SupplyarrItemId { get; set; } = string.Empty;

    public string SourceObjectType { get; set; } = string.Empty;

    public string SourceObjectId { get; set; } = string.Empty;

    public DateTimeOffset DueAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public string PayloadJson { get; set; } = "{}";
}
