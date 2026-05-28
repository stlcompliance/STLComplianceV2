namespace NexArr.Api.Contracts;

public sealed record CompanionPushVapidPublicKeyResponse(string PublicKey);

public sealed record CompanionPushSubscriptionKeysRequest(string P256dh, string Auth);

public sealed record UpsertCompanionPushSubscriptionRequest(
    string Endpoint,
    CompanionPushSubscriptionKeysRequest Keys,
    string? UserAgent);

public sealed record UnsubscribeCompanionPushRequest(string Endpoint);

public sealed record CompanionPushSubscriptionResponse(
    Guid SubscriptionId,
    string Endpoint,
    DateTimeOffset UpdatedAt);
