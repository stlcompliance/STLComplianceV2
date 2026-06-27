using STLCompliance.Shared.Data;

namespace MaintainArr.Api.Entities;

public sealed class AssetReservation : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetId { get; set; }

    public string AssetTag { get; set; } = string.Empty;

    public string AssetName { get; set; } = string.Empty;

    public string ReservationNumber { get; set; } = string.Empty;

    public string Status { get; set; } = AssetReservationStatuses.Requested;

    public string Purpose { get; set; } = string.Empty;

    public DateTimeOffset RequestedStartAt { get; set; }

    public DateTimeOffset RequestedEndAt { get; set; }

    public string? PickupLocationRef { get; set; }

    public string? PickupLocationNameSnapshot { get; set; }

    public string? ReturnLocationRef { get; set; }

    public string? ReturnLocationNameSnapshot { get; set; }

    public string? CapacityNotes { get; set; }

    public string? EquipmentNotes { get; set; }

    public string? OperatorPersonId { get; set; }

    public string? OperatorDisplayNameSnapshot { get; set; }

    public string? DriverPersonId { get; set; }

    public string? DriverDisplayNameSnapshot { get; set; }

    public string? RequestedByPersonId { get; set; }

    public string? RequestedByDisplayNameSnapshot { get; set; }

    public string? Notes { get; set; }

    public decimal? CheckOutMeterReading { get; set; }

    public decimal? ReturnMeterReading { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public DateTimeOffset? ReservedAt { get; set; }

    public DateTimeOffset? CheckedOutAt { get; set; }

    public DateTimeOffset? InUseAt { get; set; }

    public DateTimeOffset? ReturnedAt { get; set; }

    public DateTimeOffset? InspectedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }

    public DateTimeOffset? CanceledAt { get; set; }

    public DateTimeOffset? NoShowAt { get; set; }

    public string? CancelReason { get; set; }

    public string? NoShowReason { get; set; }

    public string? InspectionNotes { get; set; }

    public string? DamageNotes { get; set; }

    public string? ChargeNotes { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Asset Asset { get; set; } = null!;

    public ICollection<AssetReservationStatusEvent> StatusEvents { get; set; } = [];
}

public sealed class AssetReservationStatusEvent : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AssetReservationId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string FromStatus { get; set; } = string.Empty;

    public string ToStatus { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? ActorPersonId { get; set; }

    public string? ActorDisplayNameSnapshot { get; set; }

    public string? Notes { get; set; }

    public decimal? MeterReading { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public AssetReservation AssetReservation { get; set; } = null!;
}

public static class AssetReservationStatuses
{
    public const string Requested = "requested";

    public const string Approved = "approved";

    public const string Reserved = "reserved";

    public const string CheckedOut = "checked_out";

    public const string InUse = "in_use";

    public const string Returned = "returned";

    public const string Inspection = "inspection";

    public const string Closed = "closed";

    public const string Canceled = "canceled";

    public const string NoShow = "no_show";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Requested,
        Approved,
        Reserved,
        CheckedOut,
        InUse,
        Returned,
        Inspection,
        Closed,
        Canceled,
        NoShow,
    };

    public static readonly IReadOnlySet<string> Terminal = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Closed,
        Canceled,
        NoShow,
    };
}

public static class AssetReservationEventTypes
{
    public const string Requested = "requested";

    public const string Approved = "approved";

    public const string Reserved = "reserved";

    public const string CheckedOut = "checked_out";

    public const string InUse = "in_use";

    public const string Returned = "returned";

    public const string Inspection = "inspection";

    public const string Closed = "closed";

    public const string Canceled = "canceled";

    public const string NoShow = "no_show";
}
