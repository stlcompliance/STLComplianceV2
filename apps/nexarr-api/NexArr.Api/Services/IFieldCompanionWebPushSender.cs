using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public interface IFieldCompanionWebPushSender
{
    Task<FieldCompanionWebPushSendResult> SendAsync(
        FieldCompanionPushSubscription subscription,
        string payloadJson,
        CancellationToken cancellationToken = default);
}

public sealed record FieldCompanionWebPushSendResult(bool Success, int? HttpStatusCode, string? ErrorMessage);
