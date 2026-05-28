using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public interface ICompanionWebPushSender
{
    Task<CompanionWebPushSendResult> SendAsync(
        CompanionPushSubscription subscription,
        string payloadJson,
        CancellationToken cancellationToken = default);
}

public sealed record CompanionWebPushSendResult(bool Success, int? HttpStatusCode, string? ErrorMessage);
