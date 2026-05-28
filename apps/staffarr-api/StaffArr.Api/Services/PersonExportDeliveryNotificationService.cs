using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;

namespace StaffArr.Api.Services;

public sealed class PersonExportDeliveryNotificationService(
    StaffArrDbContext db,
    IHttpClientFactory httpClientFactory,
    IHostEnvironment hostEnvironment)
{
    public const string WebhookHttpClientName = "StaffArrPersonExportDeliveryWebhook";

    public async Task NotifySuccessAsync(
        TenantPersonExportSchedule schedule,
        PersonExportDeliveryResult delivery,
        Guid deliveryRunId,
        CancellationToken cancellationToken = default)
    {
        if (!schedule.NotifyOnSuccess)
        {
            await RecordAsync(
                schedule.TenantId,
                deliveryRunId,
                PersonExportDeliveryNotificationEventKinds.Success,
                PersonExportDeliveryNotificationStatuses.Skipped,
                schedule.NotificationWebhookUrl,
                exportId: delivery.ExportId,
                personCount: delivery.PersonCount,
                errorMessage: "notify_on_success_disabled",
                httpStatusCode: null,
                cancellationToken);
            return;
        }

        await DispatchAsync(
            schedule,
            deliveryRunId,
            PersonExportDeliveryNotificationEventKinds.Success,
            new
            {
                @event = "person.export.scheduled_delivery.success",
                tenantId = schedule.TenantId,
                exportId = delivery.ExportId,
                personCount = delivery.PersonCount,
                deliveredAt = delivery.DeliveredAt,
            },
            delivery.ExportId,
            delivery.PersonCount,
            cancellationToken);
    }

    public async Task NotifyFailureAsync(
        TenantPersonExportSchedule schedule,
        string reason,
        Guid? deliveryRunId,
        CancellationToken cancellationToken = default)
    {
        if (!schedule.NotifyOnFailure)
        {
            await RecordAsync(
                schedule.TenantId,
                deliveryRunId,
                PersonExportDeliveryNotificationEventKinds.Failure,
                PersonExportDeliveryNotificationStatuses.Skipped,
                schedule.NotificationWebhookUrl,
                exportId: null,
                personCount: null,
                errorMessage: "notify_on_failure_disabled",
                httpStatusCode: null,
                cancellationToken);
            return;
        }

        await DispatchAsync(
            schedule,
            deliveryRunId,
            PersonExportDeliveryNotificationEventKinds.Failure,
            new
            {
                @event = "person.export.scheduled_delivery.failure",
                tenantId = schedule.TenantId,
                reason,
                attemptedAt = DateTimeOffset.UtcNow,
            },
            exportId: null,
            personCount: null,
            cancellationToken);
    }

    public async Task<PersonExportDeliveryNotificationsResponse> ListRecentAsync(
        Guid tenantId,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var take = PersonExportDeliveryNotificationRules.NormalizeNotificationListLimit(limit);
        var rows = await db.PersonExportDeliveryNotifications
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.AttemptedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(x => new PersonExportDeliveryNotificationItem(
                x.Id,
                x.DeliveryRunId,
                x.EventKind,
                x.DeliveryStatus,
                x.WebhookHost,
                x.HttpStatusCode,
                x.ErrorMessage,
                x.ExportId,
                x.PersonCount,
                x.AttemptedAt))
            .ToList();

        return new PersonExportDeliveryNotificationsResponse(items);
    }

    private async Task DispatchAsync(
        TenantPersonExportSchedule schedule,
        Guid? deliveryRunId,
        string eventKind,
        object payload,
        Guid? exportId,
        int? personCount,
        CancellationToken cancellationToken)
    {
        var webhookUrl = schedule.NotificationWebhookUrl;
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            await RecordAsync(
                schedule.TenantId,
                deliveryRunId,
                eventKind,
                PersonExportDeliveryNotificationStatuses.Skipped,
                webhookUrl,
                exportId,
                personCount,
                errorMessage: "webhook_not_configured",
                httpStatusCode: null,
                cancellationToken);
            return;
        }

        try
        {
            var client = httpClientFactory.CreateClient(WebhookHttpClientName);
            using var response = await client.PostAsJsonAsync(webhookUrl, payload, cancellationToken);
            var statusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                await RecordAsync(
                    schedule.TenantId,
                    deliveryRunId,
                    eventKind,
                    PersonExportDeliveryNotificationStatuses.Sent,
                    webhookUrl,
                    exportId,
                    personCount,
                    errorMessage: null,
                    httpStatusCode: statusCode,
                    cancellationToken);
                return;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var truncated = body.Length > 200 ? body[..200] : body;
            await RecordAsync(
                schedule.TenantId,
                deliveryRunId,
                eventKind,
                PersonExportDeliveryNotificationStatuses.Failed,
                webhookUrl,
                exportId,
                personCount,
                errorMessage: $"http_{statusCode}:{truncated}",
                httpStatusCode: statusCode,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await RecordAsync(
                schedule.TenantId,
                deliveryRunId,
                eventKind,
                PersonExportDeliveryNotificationStatuses.Failed,
                webhookUrl,
                exportId,
                personCount,
                errorMessage: ex.Message,
                httpStatusCode: null,
                cancellationToken);
        }
    }

    private async Task RecordAsync(
        Guid tenantId,
        Guid? deliveryRunId,
        string eventKind,
        string deliveryStatus,
        string? webhookUrl,
        Guid? exportId,
        int? personCount,
        string? errorMessage,
        int? httpStatusCode,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        db.PersonExportDeliveryNotifications.Add(new PersonExportDeliveryNotification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DeliveryRunId = deliveryRunId,
            EventKind = eventKind,
            DeliveryStatus = deliveryStatus,
            WebhookHost = PersonExportDeliveryNotificationRules.TryGetWebhookHost(webhookUrl),
            HttpStatusCode = httpStatusCode,
            ErrorMessage = errorMessage is null ? null : Truncate(errorMessage, 512),
            ExportId = exportId,
            PersonCount = personCount,
            AttemptedAt = now,
            CreatedAt = now,
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    public bool AllowInsecureWebhookHttp() =>
        hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
}
