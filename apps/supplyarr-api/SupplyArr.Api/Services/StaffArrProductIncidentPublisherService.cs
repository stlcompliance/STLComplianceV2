using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Options;

namespace SupplyArr.Api.Services;

public sealed class StaffArrProductIncidentPublisherService(
    SupplyArrDbContext db,
    StaffArrProductIncidentClient staffarrClient,
    IOptions<StaffArrClientOptions> staffarrOptions,
    ISupplyArrAuditService audit)
{
    public static bool ShouldPublishForEventKind(string eventKind) =>
        string.Equals(eventKind, IntegrationOutboxEventKinds.SupplierIncidentCreated, StringComparison.OrdinalIgnoreCase);

    public async Task TryPublishFromOutboxAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(staffarrOptions.Value.ServiceToken)
            || !ShouldPublishForEventKind(outboxEvent.EventKind))
        {
            return;
        }

        var incident = await db.SupplierIncidents
            .Include(x => x.Supplier)
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (incident is null || incident.InvolvedStaffarrPersonId is not Guid staffarrPersonId)
        {
            return;
        }

        var response = await staffarrClient.IngestAsync(
            new StaffArrProductIncidentIngestRequest(
                incident.TenantId,
                "supplyarr",
                incident.Id,
                outboxEvent.EventKind,
                staffarrPersonId,
                MapReasonCategory(incident.IncidentType),
                incident.Severity,
                incident.Title,
                BuildDescription(incident),
                incident.CreatedAt,
                incident.IncidentKey),
            cancellationToken);

        incident.StaffarrPersonnelIncidentId = response.IncidentId;
        incident.StaffarrIncidentRoutedAt = DateTimeOffset.UtcNow;
        incident.StaffarrIncidentRouteStatus = response.IdempotentReplay ? "replayed" : "routed";
        incident.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplier_incident.route_staffarr",
            incident.TenantId,
            IntegrationEventProcessingService.WorkerActorUserId,
            "supplier_incident",
            incident.Id.ToString(),
            "Succeeded",
            reasonCode: incident.StaffarrIncidentRouteStatus,
            cancellationToken: cancellationToken);
    }

    private static string MapReasonCategory(string incidentType) =>
        incidentType.ToLowerInvariant() switch
        {
            SupplierIncidentTypes.Safety => "safety",
            SupplierIncidentTypes.Compliance => "policy",
            SupplierIncidentTypes.Quality => "equipment",
            _ => "other",
        };

    private static string BuildDescription(SupplierIncident incident)
    {
        var supplierDisplayName = incident.Supplier.DisplayName;
        return $"SupplyArr supplier incident {incident.IncidentKey} for {supplierDisplayName}: {incident.Description}";
    }
}

