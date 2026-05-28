using Microsoft.Extensions.Options;
using NexArr.Api.Entities;
using NexArr.Api.Options;
using WebPush;

namespace NexArr.Api.Services;

public sealed class CompanionWebPushSender(IOptions<CompanionWebPushOptions> options) : ICompanionWebPushSender
{
    public async Task<CompanionWebPushSendResult> SendAsync(
        CompanionPushSubscription subscription,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (!settings.IsConfigured)
        {
            return new CompanionWebPushSendResult(false, null, "web_push_not_configured");
        }

        try
        {
            var vapid = new VapidDetails(settings.Subject!, settings.PublicKey!, settings.PrivateKey!);
            var pushSubscription = new PushSubscription(
                subscription.Endpoint,
                subscription.P256dhKey,
                subscription.AuthKey);
            var client = new WebPushClient();
            await client.SendNotificationAsync(pushSubscription, payloadJson, vapid, cancellationToken);
            return new CompanionWebPushSendResult(true, 201, null);
        }
        catch (WebPushException ex)
        {
            return new CompanionWebPushSendResult(false, (int)ex.StatusCode, ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new CompanionWebPushSendResult(false, null, ex.Message);
        }
    }
}
