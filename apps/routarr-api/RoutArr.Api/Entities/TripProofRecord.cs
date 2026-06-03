using STLCompliance.Shared.Data;

namespace RoutArr.Api.Entities;

public sealed class TripProofRecord : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid TripId { get; set; }

    public string ProofType { get; set; } = TripProofTypes.Pickup;

    public string CapturedByPersonId { get; set; } = string.Empty;

    public string? VehicleRefKey { get; set; }

    public string ReferenceKey { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string ReviewStatus { get; set; } = TripProofReviewStatuses.PendingReview;

    public string? ReviewedByPersonId { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public string ReviewNotes { get; set; } = string.Empty;

    public DateTimeOffset CapturedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Trip Trip { get; set; } = null!;
}

public static class TripProofReviewStatuses
{
    public const string PendingReview = "pending_review";

    public const string Rejected = "rejected";

    public const string Corrected = "corrected";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        PendingReview,
        Rejected,
        Corrected,
    };
}

public static class TripProofTypes
{
    public const string Pickup = "pickup";

    public const string Delivery = "delivery";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Pickup,
        Delivery,
    };
}
