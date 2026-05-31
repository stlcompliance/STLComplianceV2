using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Options;

namespace RoutArr.Api.Services;

public sealed class StaffArrProductIncidentPublisherService(
    RoutArrDbContext db,
    StaffArrProductIncidentClient staffarrClient,
    IOptions<StaffArrClientOptions> staffarrOptions,
    IRoutArrAuditService audit)
{
    public static bool ShouldPublishForEventKind(string eventKind) =>
        string.Equals(eventKind, RoutArrIntegrationOutboxEventKinds.IncidentCreated, StringComparison.OrdinalIgnoreCase);

    public async Task TryPublishFromOutboxAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(staffarrOptions.Value.ServiceToken)
            || !ShouldPublishForEventKind(outboxEvent.EventKind))
        {
            return;
        }

        var incident = await db.DispatchExceptions
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        var trip = incident?.TripId is Guid tripId
            ? await db.Trips.FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == tripId,
                cancellationToken)
            : null;
        if (incident is null
            || !string.Equals(incident.IncidentRoutedProduct, DispatchIncidentRoutedProducts.StaffArr, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(trip?.AssignedDriverPersonId)
            || !Guid.TryParse(trip.AssignedDriverPersonId, out var staffarrPersonId))
        {
            return;
        }

        var response = await staffarrClient.IngestAsync(
            new StaffArrProductIncidentIngestRequest(
                incident.TenantId,
                "routarr",
                incident.Id,
                outboxEvent.EventKind,
                staffarrPersonId,
                MapReasonCategory(incident.IncidentType),
                incident.IncidentSeverity,
                incident.Title,
                BuildDescription(incident, trip),
                incident.CreatedAt,
                incident.ExceptionKey),
            cancellationToken);

        incident.StaffarrPersonnelIncidentId = response.IncidentId;
        incident.StaffarrIncidentRoutedAt = DateTimeOffset.UtcNow;
        incident.StaffarrIncidentRouteStatus = response.IdempotentReplay ? "replayed" : "routed";
        incident.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "dispatch_incident.route_staffarr",
            incident.TenantId,
            IntegrationEventProcessingService.WorkerActorUserId,
            "dispatch_incident",
            incident.Id.ToString(),
            "Succeeded",
            reasonCode: incident.StaffarrIncidentRouteStatus,
            cancellationToken: cancellationToken);
    }

    private static string MapReasonCategory(string incidentType) =>
        incidentType.ToLowerInvariant() switch
        {
            DispatchIncidentTypes.Injury => "safety",
            DispatchIncidentTypes.SafetyConcern => "safety",
            DispatchIncidentTypes.Accident => "safety",
            DispatchIncidentTypes.NearMiss => "safety",
            DispatchIncidentTypes.ComplianceRelated => "policy",
            DispatchIncidentTypes.TrainingRelated => "training",
            _ => "other",
        };

    private static string BuildDescription(DispatchException incident, Trip? trip)
    {
        var tripNumber = string.IsNullOrWhiteSpace(trip?.TripNumber)
            ? "unlinked trip"
            : $"trip {trip.TripNumber}";
        return $"RoutArr transportation incident {incident.ExceptionKey} on {tripNumber}: {incident.Description}";
    }
}
