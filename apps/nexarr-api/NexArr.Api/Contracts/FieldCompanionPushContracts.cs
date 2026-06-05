namespace NexArr.Api.Contracts;

public sealed record FieldCompanionPushVapidPublicKeyResponse(string PublicKey);

public sealed record FieldCompanionPushSubscriptionKeysRequest(string P256dh, string Auth);

public sealed record UpsertFieldCompanionPushSubscriptionRequest(
    string Endpoint,
    FieldCompanionPushSubscriptionKeysRequest Keys,
    string? UserAgent);

public sealed record UnsubscribeFieldCompanionPushRequest(string Endpoint);

public sealed record FieldCompanionPushSubscriptionResponse(
    Guid SubscriptionId,
    string Endpoint,
    DateTimeOffset UpdatedAt);
