using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Options;

namespace SupplyArr.Api.Services;

public sealed class TrainArrSupplierIncidentPublisherService(
    SupplyArrDbContext db,
    TrainArrIncidentRemediationClient trainarrClient,
    IOptions<TrainArrClientOptions> trainarrOptions,
    ISupplyArrAuditService audit)
{
    public static bool ShouldPublishForEventKind(string eventKind) =>
        string.Equals(eventKind, IntegrationOutboxEventKinds.SupplierIncidentCreated, StringComparison.OrdinalIgnoreCase);

    public async Task TryPublishFromOutboxAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trainarrOptions.Value.ServiceToken)
            || !ShouldPublishForEventKind(outboxEvent.EventKind))
        {
            return;
        }

        var incident = await db.SupplierIncidents
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (incident is null
            || incident.InvolvedStaffarrPersonId is not Guid staffarrPersonId
            || !ShouldRouteToTrainArr(incident))
        {
            return;
        }

        var response = await trainarrClient.IngestAsync(
            new TrainArrSupplyArrIncidentRemediationRequest(
                incident.TenantId,
                incident.Id,
                outboxEvent.EventKind,
                incident.Id,
                outboxEvent.CorrelationId,
                staffarrPersonId,
                new SupplyArrTrainArrIncidentPayload(
                    incident.TenantId,
                    BuildSummary(incident),
                    incident.Id,
                    incident.IncidentKey,
                    incident.IncidentType,
                    incident.Severity,
                    incident.Status,
                    incident.SupplierId,
                    incident.Supplier.DisplayName,
                    incident.PurchaseRequestId,
                    incident.PurchaseOrderId,
                    incident.ReceivingReceiptId,
                    incident.ReceivingExceptionId),
                incident.CreatedAt),
            cancellationToken);

        incident.TrainarrIncidentRemediationId = response.RemediationId;
        incident.TrainarrIncidentRoutedAt = DateTimeOffset.UtcNow;
        incident.TrainarrIncidentRouteStatus = response.IdempotentReplay ? "replayed" : "routed";
        incident.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_incident.route_trainarr",
            incident.TenantId,
            IntegrationEventProcessingService.WorkerActorUserId,
            "supplier_incident",
            incident.Id.ToString(),
            "Succeeded",
            reasonCode: incident.TrainarrIncidentRouteStatus,
            cancellationToken: cancellationToken);
    }

    private static bool ShouldRouteToTrainArr(SupplierIncident incident) =>
        string.Equals(incident.IncidentType, SupplierIncidentTypes.Safety, StringComparison.OrdinalIgnoreCase)
        || string.Equals(incident.IncidentType, SupplierIncidentTypes.Compliance, StringComparison.OrdinalIgnoreCase)
        || string.Equals(incident.Severity, SupplierIncidentSeverities.High, StringComparison.OrdinalIgnoreCase)
        || string.Equals(incident.Severity, SupplierIncidentSeverities.Critical, StringComparison.OrdinalIgnoreCase);

    private static string BuildSummary(SupplierIncident incident) =>
        $"SupplyArr supplier incident {incident.IncidentKey} for {incident.Supplier.DisplayName}: {incident.Description}";
}

