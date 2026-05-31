using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Options;

namespace RoutArr.Api.Services;

public sealed class MaintainArrRoutarrEventPublisherService(
    RoutArrDbContext db,
    MaintainArrRoutarrEventClient maintainarrClient,
    IOptions<MaintainArrClientOptions> maintainarrOptions,
    IRoutArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static bool ShouldPublishForEventKind(string eventKind) =>
        string.Equals(eventKind, RoutArrIntegrationOutboxEventKinds.DriverReportedDefect, StringComparison.OrdinalIgnoreCase)
        || string.Equals(eventKind, RoutArrIntegrationOutboxEventKinds.IncidentCreated, StringComparison.OrdinalIgnoreCase);

    public async Task TryPublishFromOutboxAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(maintainarrOptions.Value.ServiceToken)
            || !ShouldPublishForEventKind(outboxEvent.EventKind))
        {
            return;
        }

        var payload = JsonSerializer.Deserialize<RoutArrIntegrationOutboxPayload>(outboxEvent.PayloadJson, JsonOptions);
        if (payload is null)
        {
            return;
        }

        if (string.Equals(outboxEvent.EventKind, RoutArrIntegrationOutboxEventKinds.DriverReportedDefect, StringComparison.OrdinalIgnoreCase))
        {
            await PublishDriverReportedDefectAsync(outboxEvent, payload, cancellationToken);
            return;
        }

        await PublishEquipmentIncidentAsync(outboxEvent, payload, cancellationToken);
    }

    private async Task PublishDriverReportedDefectAsync(
        IntegrationOutboxEvent outboxEvent,
        RoutArrIntegrationOutboxPayload payload,
        CancellationToken cancellationToken)
    {
        var dvir = await db.TripDvirInspections
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (dvir is null)
        {
            return;
        }

        var response = await maintainarrClient.IngestAsync(
            new MaintainArrRoutarrEventIngestRequest(
                dvir.TenantId,
                outboxEvent.Id,
                outboxEvent.EventKind,
                outboxEvent.RelatedEntityType,
                outboxEvent.RelatedEntityId,
                outboxEvent.CorrelationId,
                payload,
                dvir.SubmittedAt),
            cancellationToken);

        dvir.MaintainarrInboundEventId = response.InboundEventId;
        dvir.MaintainarrDefectId = response.DefectId;
        dvir.MaintainarrEventRoutedAt = DateTimeOffset.UtcNow;
        dvir.MaintainarrEventRouteStatus = MapRouteStatus(response);
        dvir.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "driver_reported_defect.route_maintainarr",
            dvir.TenantId,
            IntegrationEventProcessingService.WorkerActorUserId,
            "trip_dvir",
            dvir.Id.ToString(),
            "Succeeded",
            reasonCode: dvir.MaintainarrEventRouteStatus,
            cancellationToken: cancellationToken);
    }

    private async Task PublishEquipmentIncidentAsync(
        IntegrationOutboxEvent outboxEvent,
        RoutArrIntegrationOutboxPayload payload,
        CancellationToken cancellationToken)
    {
        var incident = await db.DispatchExceptions
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (incident is null || !ShouldRouteIncidentToMaintainArr(incident))
        {
            return;
        }

        var response = await maintainarrClient.IngestAsync(
            new MaintainArrRoutarrEventIngestRequest(
                incident.TenantId,
                incident.Id,
                outboxEvent.EventKind,
                outboxEvent.RelatedEntityType,
                outboxEvent.RelatedEntityId,
                outboxEvent.CorrelationId,
                payload,
                incident.CreatedAt),
            cancellationToken);

        incident.MaintainarrInboundEventId = response.InboundEventId;
        incident.MaintainarrDefectId = response.DefectId;
        incident.MaintainarrIncidentRoutedAt = DateTimeOffset.UtcNow;
        incident.MaintainarrIncidentRouteStatus = MapRouteStatus(response);
        incident.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "dispatch_incident.route_maintainarr",
            incident.TenantId,
            IntegrationEventProcessingService.WorkerActorUserId,
            "dispatch_incident",
            incident.Id.ToString(),
            "Succeeded",
            reasonCode: incident.MaintainarrIncidentRouteStatus,
            cancellationToken: cancellationToken);
    }

    private static bool ShouldRouteIncidentToMaintainArr(DispatchException incident) =>
        string.Equals(incident.IncidentRoutedProduct, DispatchIncidentRoutedProducts.MaintainArr, StringComparison.OrdinalIgnoreCase)
        || string.Equals(incident.IncidentType, DispatchIncidentTypes.EquipmentAbuse, StringComparison.OrdinalIgnoreCase)
        || string.Equals(incident.Category, DispatchExceptionCategories.Vehicle, StringComparison.OrdinalIgnoreCase);

    private static string MapRouteStatus(MaintainArrRoutarrEventIngestResponse response)
    {
        if (response.IdempotentReplay)
        {
            return "replayed";
        }

        return string.Equals(response.Outcome, "ignored", StringComparison.OrdinalIgnoreCase)
            ? "ignored"
            : "routed";
    }
}
