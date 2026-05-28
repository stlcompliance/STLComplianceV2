using SupplyArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace SupplyArr.Api.Services;

public static class ProcurementNotificationRules
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
                "supplyarr.notification.webhook_too_long",
                $"Webhook URL must be at most {MaxWebhookUrlLength} characters.",
                400);
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            throw new StlApiException(
                "supplyarr.notification.webhook_invalid",
                "Webhook URL must be an absolute URL.",
                400);
        }

        if (uri.Scheme is not ("https" or "http"))
        {
            throw new StlApiException(
                "supplyarr.notification.webhook_invalid_scheme",
                "Webhook URL must use http or https.",
                400);
        }

        if (!allowInsecureHttp && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "supplyarr.notification.webhook_https_required",
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
        TenantProcurementNotificationSettingsSnapshot settings,
        string eventKind) =>
        settings.IsEnabled
        && !string.IsNullOrWhiteSpace(settings.NotificationWebhookUrl)
        && eventKind switch
        {
            ProcurementNotificationEventKinds.PurchaseRequestSubmitted =>
                settings.NotifyOnPurchaseRequestSubmitted,
            ProcurementNotificationEventKinds.PurchaseRequestApproved =>
                settings.NotifyOnPurchaseRequestApproved,
            ProcurementNotificationEventKinds.PurchaseOrderIssued => settings.NotifyOnPurchaseOrderIssued,
            ProcurementNotificationEventKinds.ReceivingReceiptPosted =>
                settings.NotifyOnReceivingReceiptPosted,
            ProcurementNotificationEventKinds.PurchaseRequestApprovalReminder
            or ProcurementNotificationEventKinds.PurchaseOrderApprovalReminder
            or ProcurementNotificationEventKinds.MaintainArrDemandPrDrafted =>
                settings.IsEnabled && !string.IsNullOrWhiteSpace(settings.NotificationWebhookUrl),
            _ => false,
        };
}

public sealed record TenantProcurementNotificationSettingsSnapshot(
    bool IsEnabled,
    string? NotificationWebhookUrl,
    bool NotifyOnPurchaseRequestSubmitted,
    bool NotifyOnPurchaseRequestApproved,
    bool NotifyOnPurchaseOrderIssued,
    bool NotifyOnReceivingReceiptPosted);
