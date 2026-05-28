using STLCompliance.Shared.Data;

namespace TrainArr.Api.Entities;

public sealed class StaffarrPublicationDelivery : IHasTenant
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid CertificationPublicationId { get; set; }

    public Guid StaffarrPersonId { get; set; }

    public string OperationKind { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public string DeliveryStatus { get; set; } = StaffarrPublicationDeliveryStatuses.Pending;

    public int AttemptCount { get; set; }

    public DateTimeOffset? NextRetryAt { get; set; }

    public int? HttpStatusCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }
}

public static class StaffarrPublicationOperationKinds
{
    public const string TrainingBlockerPublish = "training_blocker_publish";

    public const string TrainingBlockerClear = "training_blocker_clear";

    public const string QualificationGrant = "qualification_grant";

    public const string QualificationLifecycle = "qualification_lifecycle";
}

public static class StaffarrPublicationDeliveryStatuses
{
    public const string Pending = "pending";

    public const string Delivered = "delivered";

    public const string Abandoned = "abandoned";
}
