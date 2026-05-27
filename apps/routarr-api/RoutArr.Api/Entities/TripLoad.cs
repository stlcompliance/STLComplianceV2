using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TripLoad : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TripId { get; set; }

    public string LoadKey { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string LoadType { get; set; } = TripLoadTypes.General;

    public string Status { get; set; } = TripLoadStatuses.Pending;

    public int SequenceNumber { get; set; }

    public string OriginLabel { get; set; } = string.Empty;

    public string DestinationLabel { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Trip Trip { get; set; } = null!;
}

public static class TripLoadTypes
{
    public const string General = "general";

    public const string Pickup = "pickup";

    public const string Delivery = "delivery";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        General,
        Pickup,
        Delivery,
    };
}

public static class TripLoadStatuses
{
    public const string Pending = "pending";

    public const string Loaded = "loaded";

    public const string Delivered = "delivered";

    public const string Cancelled = "cancelled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Loaded,
        Delivered,
        Cancelled,
    };
}
