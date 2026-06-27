using System.Text.Json;
using NexArr.Api.Entities;

namespace NexArr.Api.Services;

public static class FieldCompanionWebPushPayloadBuilder
{
    public static string Build(FieldCompanionNotificationDispatch item)
    {
        var (title, body, eventName) = item.EventKind switch
        {
            FieldCompanionNotificationEventKinds.HandoffRedeemed => (
                "fieldcompanion session started",
                "A mobile handoff was redeemed for your tenant.",
                "fieldcompanion.handoff.redeemed"),
            FieldCompanionNotificationEventKinds.FieldInboxRefreshed => (
                "Field inbox updated",
                "Your assigned field work was refreshed.",
                "fieldcompanion.field_inbox.refreshed"),
            FieldCompanionNotificationEventKinds.TestNotification => (
                "Field Companion test notification",
                "This is a test notification from the Field Companion settings page.",
                "fieldcompanion.notification.test"),
            _ => (
                "fieldcompanion notification",
                "You have a new fieldcompanion operational notification.",
                "fieldcompanion.notification.unknown"),
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
