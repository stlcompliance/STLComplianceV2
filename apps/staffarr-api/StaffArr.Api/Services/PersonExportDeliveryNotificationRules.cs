using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public static class PersonExportDeliveryNotificationRules
{
    public const int MaxWebhookUrlLength = 2048;

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
                "person.export_notification.webhook_too_long",
                $"Webhook URL must be at most {MaxWebhookUrlLength} characters.",
                400);
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new StlApiException(
                "person.export_notification.webhook_invalid",
                "Webhook URL must be an absolute URL.",
                400);
        }

        if (uri.Scheme is not ("https" or "http"))
        {
            throw new StlApiException(
                "person.export_notification.webhook_invalid_scheme",
                "Webhook URL must use http or https.",
                400);
        }

        if (!allowInsecureHttp && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "person.export_notification.webhook_https_required",
                "Webhook URL must use https.",
                400);
        }

        return uri.ToString();
    }

    public static string? TryGetWebhookHost(string? webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return null;
        }

        return Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri) ? uri.Host : null;
    }

    public static int NormalizeNotificationListLimit(int? limit) =>
        limit is null or < 1 ? 20 : Math.Min(limit.Value, 100);
}
