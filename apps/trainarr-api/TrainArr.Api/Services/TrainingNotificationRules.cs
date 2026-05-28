using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public static class TrainingNotificationRules
{
    public const int MaxWebhookUrlLength = 2048;

    public static int NormalizeBatchSize(int? batchSize) =>
        Math.Clamp(batchSize ?? 25, 1, 200);

    public static int NormalizeExpiringLeadDays(int? leadDays) =>
        Math.Clamp(leadDays ?? 30, 1, 365);

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
                "trainarr.notification.webhook_too_long",
                $"Webhook URL must be at most {MaxWebhookUrlLength} characters.",
                400);
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new StlApiException(
                "trainarr.notification.webhook_invalid",
                "Webhook URL must be an absolute URL.",
                400);
        }

        if (uri.Scheme is not ("https" or "http"))
        {
            throw new StlApiException(
                "trainarr.notification.webhook_invalid_scheme",
                "Webhook URL must use http or https.",
                400);
        }

        if (!allowInsecureHttp && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "trainarr.notification.webhook_https_required",
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
        TenantTrainingNotificationSettingsSnapshot settings,
        string eventKind) =>
        settings.IsEnabled
        && !string.IsNullOrWhiteSpace(settings.NotificationWebhookUrl)
        && eventKind switch
        {
            TrainingNotificationEventKinds.AssignmentCreated => settings.NotifyOnAssignmentCreated,
            TrainingNotificationEventKinds.QualificationExpiring => settings.NotifyOnQualificationExpiring,
            TrainingNotificationEventKinds.QualificationExpired => settings.NotifyOnQualificationExpired,
            _ => false,
        };
}

public sealed record TenantTrainingNotificationSettingsSnapshot(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnAssignmentCreated,
    bool NotifyOnQualificationExpiring,
    bool NotifyOnQualificationExpired,
    int ExpiringLeadDays);
