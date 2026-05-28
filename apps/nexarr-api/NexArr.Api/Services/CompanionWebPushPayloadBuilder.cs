using System.Text.Json;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public static class CompanionWebPushPayloadBuilder
{
    public static string Build(CompanionNotificationDispatch item)
    {
        var (title, body, eventName) = item.EventKind switch
        {
            CompanionNotificationEventKinds.HandoffRedeemed => (
                "Companion session started",
                "A mobile handoff was redeemed for your tenant.",
                "companion.handoff.redeemed"),
            CompanionNotificationEventKinds.FieldInboxRefreshed => (
                "Field inbox updated",
                "Your assigned field work was refreshed.",
                "companion.field_inbox.refreshed"),
            _ => (
                "Companion notification",
                "You have a new Companion operational notification.",
                "companion.notification.unknown"),
        };

        var payload = new
        {
            title,
            body,
            data = new
            {
                @event = eventName,
                tenantId = item.TenantId,
                notificationId = item.Id,
                eventKind = item.EventKind,
            },
        };

        return JsonSerializer.Serialize(payload);
    }
}
