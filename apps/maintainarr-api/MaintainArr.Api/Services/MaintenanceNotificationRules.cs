using MaintainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public static class MaintenanceNotificationRules
{
    public const int MaxWebhookUrlLength = 2048;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 25, 1, 200);

    public static string? NormalizeWebhookUrl(string? raw, bool allowInsecureHttp)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        if (trimmed.Length > MaxWebhookUrlLength)
        {
            throw new StlApiException(
                "maintainarr.notification.webhook_too_long",
                $"Webhook URL must be at most {MaxWebhookUrlLength} characters.",
                400);
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new StlApiException(
                "maintainarr.notification.webhook_invalid",
                "Webhook URL must be an absolute URL.",
                400);
        }

        if (uri.Scheme is not ("https" or "http"))
        {
            throw new StlApiException(
                "maintainarr.notification.webhook_invalid_scheme",
                "Webhook URL must use http or https.",
                400);
        }

        if (!allowInsecureHttp && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "maintainarr.notification.webhook_https_required",
                "Webhook URL must use https.",
                400);
        }

        return uri.ToString();
    }

    public static string? TryGetWebhookHost(string? webhookUrl) =>
        string.IsNullOrWhiteSpace(webhookUrl) || !Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri)
            ? null
            : uri.Host;

    public static int NormalizeDispatchListLimit(int? limit) =>
        limit is null or < 1 ? 20 : Math.Min(limit.Value, 100);

    public static bool ShouldNotifyForEvent(
        TenantMaintenanceNotificationSettingsSnapshot settings,
        string eventKind) =>
        settings.IsEnabled
        && !string.IsNullOrWhiteSpace(settings.NotificationWebhookUrl)
        && eventKind switch
        {
            MaintenanceNotificationEventKinds.WorkOrderCreated => settings.NotifyOnWorkOrderCreated,
            MaintenanceNotificationEventKinds.PmScheduleDue => settings.NotifyOnPmScheduleDue,
            MaintenanceNotificationEventKinds.PmScheduleOverdue => settings.NotifyOnPmScheduleOverdue,
            _ => false,
        };

    public static string? MapPmDueStatusToEventKind(string dueStatus) =>
        dueStatus switch
        {
            PmDueStatuses.Due => MaintenanceNotificationEventKinds.PmScheduleDue,
            PmDueStatuses.Overdue => MaintenanceNotificationEventKinds.PmScheduleOverdue,
            _ => null,
        };
}

public sealed record TenantMaintenanceNotificationSettingsSnapshot(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnWorkOrderCreated,
    bool NotifyOnPmScheduleDue,
    bool NotifyOnPmScheduleOverdue);
