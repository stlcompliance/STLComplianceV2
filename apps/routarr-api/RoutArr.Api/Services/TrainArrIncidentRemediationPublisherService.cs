using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Options;

namespace RoutArr.Api.Services;

public sealed class TrainArrIncidentRemediationPublisherService(
    RoutArrDbContext db,
    TrainArrIncidentRemediationClient trainarrClient,
    IOptions<TrainArrClientOptions> trainarrOptions,
    IRoutArrAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static bool ShouldPublishForEventKind(string eventKind) =>
        string.Equals(eventKind, RoutArrIntegrationOutboxEventKinds.IncidentCreated, StringComparison.OrdinalIgnoreCase);

    public async Task TryPublishFromOutboxAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trainarrOptions.Value.ServiceToken)
            || !ShouldPublishForEventKind(outboxEvent.EventKind))
        {
            return;
        }

        var incident = await db.DispatchExceptions
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (incident is null
            || !ShouldRouteToTrainArr(incident))
        {
            return;
        }

        var payload = JsonSerializer.Deserialize<RoutArrIntegrationOutboxPayload>(outboxEvent.PayloadJson, JsonOptions);
        if (payload is null || string.IsNullOrWhiteSpace(payload.DriverPersonId))
        {
            return;
        }

        var response = await trainarrClient.IngestAsync(
            new TrainArrRoutarrIncidentRemediationRequest(
                incident.TenantId,
                incident.Id,
                outboxEvent.EventKind,
                incident.Id,
                outboxEvent.CorrelationId,
                payload,
                incident.CreatedAt),
            cancellationToken);

        incident.TrainarrIncidentRemediationId = response.RemediationId;
        incident.TrainarrIncidentRoutedAt = DateTimeOffset.UtcNow;
        incident.TrainarrIncidentRouteStatus = response.IdempotentReplay ? "replayed" : "routed";
        incident.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "dispatch_incident.route_trainarr",
            incident.TenantId,
            IntegrationEventProcessingService.WorkerActorUserId,
            "dispatch_incident",
            incident.Id.ToString(),
            "Succeeded",
            reasonCode: incident.TrainarrIncidentRouteStatus,
            cancellationToken: cancellationToken);
    }

    private static bool ShouldRouteToTrainArr(DispatchException incident) =>
        string.Equals(incident.IncidentRoutedProduct, DispatchIncidentRoutedProducts.TrainArr, StringComparison.OrdinalIgnoreCase)
        || string.Equals(incident.IncidentType, DispatchIncidentTypes.TrainingRelated, StringComparison.OrdinalIgnoreCase);
}
