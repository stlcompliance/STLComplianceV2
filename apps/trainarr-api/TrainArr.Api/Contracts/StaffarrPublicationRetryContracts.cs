namespace TrainArr.Api.Contracts;

public sealed record StaffarrPublicationSettingsResponse(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes,
    DateTimeOffset? UpdatedAt);

public sealed record UpsertStaffarrPublicationSettingsRequest(
    bool IsEnabled,
    int MaxAttempts,
    int RetryIntervalMinutes);

public sealed record StaffarrPublicationDeliveryItem(
    Guid DeliveryId,
    Guid CertificationPublicationId,
    string OperationKind,
    string DeliveryStatus,
    Guid StaffarrPersonId,
    int AttemptCount,
    int? HttpStatusCode,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? NextRetryAt,
    DateTimeOffset? DeliveredAt);

public sealed record StaffarrPublicationDeliveriesResponse(
    IReadOnlyList<StaffarrPublicationDeliveryItem> Items);

public sealed record PendingStaffarrPublicationDeliveryItem(
    Guid DeliveryId,
    Guid TenantId,
    Guid CertificationPublicationId,
    string OperationKind,
    Guid StaffarrPersonId,
    int AttemptCount,
    DateTimeOffset? NextRetryAt,
    DateTimeOffset CreatedAt);

public sealed record PendingStaffarrPublicationDeliveriesResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    IReadOnlyList<PendingStaffarrPublicationDeliveryItem> Items);

public sealed record ProcessStaffarrPublicationRetriesRequest(
    Guid? TenantId,
    DateTimeOffset? AsOfUtc,
    int? BatchSize);

public sealed record StaffarrPublicationRetryResult(
    Guid DeliveryId,
    string DeliveryStatus,
    int AttemptCount);

public sealed record StaffarrPublicationRetrySkip(
    Guid DeliveryId,
    string Reason);

public sealed record ProcessStaffarrPublicationRetriesResponse(
    DateTimeOffset AsOfUtc,
    int BatchSize,
    int PendingFound,
    int DeliveredCount,
    int RetriedCount,
    int AbandonedCount,
    int SkippedCount,
    IReadOnlyList<StaffarrPublicationRetryResult> Results,
    IReadOnlyList<StaffarrPublicationRetrySkip> Skipped);
