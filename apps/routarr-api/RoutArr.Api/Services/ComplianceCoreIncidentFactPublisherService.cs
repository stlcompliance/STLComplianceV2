using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Options;

namespace RoutArr.Api.Services;

public sealed class ComplianceCoreIncidentFactPublisherService(
    RoutArrDbContext db,
    ComplianceCoreProductFactClient complianceCoreClient,
    IOptions<ComplianceCoreClientOptions> complianceCoreOptions,
    IRoutArrAuditService audit)
{
    private const string SourceProduct = "routarr";
    private const string SourceEntityType = "dispatch_incident";
    private const string ValueTypeString = "string";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static bool ShouldPublishForEventKind(string eventKind) =>
        string.Equals(eventKind, RoutArrIntegrationOutboxEventKinds.IncidentCreated, StringComparison.OrdinalIgnoreCase);

    public async Task TryPublishFromOutboxAsync(
        IntegrationOutboxEvent outboxEvent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(complianceCoreOptions.Value.ServiceToken)
            || !ShouldPublishForEventKind(outboxEvent.EventKind))
        {
            return;
        }

        var incident = await db.DispatchExceptions
            .FirstOrDefaultAsync(
                x => x.TenantId == outboxEvent.TenantId && x.Id == outboxEvent.RelatedEntityId,
                cancellationToken);
        if (incident is null || !ShouldRouteToComplianceCore(incident))
        {
            return;
        }

        var payload = JsonSerializer.Deserialize<RoutArrIntegrationOutboxPayload>(outboxEvent.PayloadJson, JsonOptions);
        var facts = BuildFacts(outboxEvent, incident, payload);
        if (facts.Count == 0)
        {
            return;
        }

        var response = await complianceCoreClient.IngestAsync(
            new ComplianceCoreProductFactIngestRequest(
                incident.TenantId,
                outboxEvent.Id,
                SourceProduct,
                outboxEvent.CreatedAt,
                facts),
            cancellationToken);

        incident.CompliancecoreFactPublicationId = response.PublicationId;
        incident.CompliancecoreIncidentRoutedAt = DateTimeOffset.UtcNow;
        incident.CompliancecoreIncidentRouteStatus = response.AcceptedCount > 0 ? "routed" : "replayed";
        incident.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "dispatch_incident.route_compliancecore",
            incident.TenantId,
            IntegrationEventProcessingService.WorkerActorUserId,
            "dispatch_incident",
            incident.Id.ToString(),
            "Succeeded",
            reasonCode: incident.CompliancecoreIncidentRouteStatus,
            cancellationToken: cancellationToken);
    }

    private static bool ShouldRouteToComplianceCore(DispatchException incident) =>
        string.Equals(incident.IncidentRoutedProduct, DispatchIncidentRoutedProducts.ComplianceCore, StringComparison.OrdinalIgnoreCase)
        || string.Equals(incident.IncidentType, DispatchIncidentTypes.ComplianceRelated, StringComparison.OrdinalIgnoreCase)
        || string.Equals(incident.Category, DispatchExceptionCategories.Compliance, StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<ComplianceCoreProductFactPublicationItem> BuildFacts(
        IntegrationOutboxEvent outboxEvent,
        DispatchException incident,
        RoutArrIntegrationOutboxPayload? payload)
    {
        var scopeKey = $"dispatch_incident:{incident.Id:N}";
        var facts = new List<ComplianceCoreProductFactPublicationItem>
        {
            Fact(outboxEvent, incident, scopeKey, "routarr.incident.type", incident.IncidentType),
            Fact(outboxEvent, incident, scopeKey, "routarr.incident.severity", incident.IncidentSeverity),
            Fact(outboxEvent, incident, scopeKey, "routarr.incident.category", incident.Category),
            Fact(outboxEvent, incident, scopeKey, "routarr.incident.status", incident.Status),
            Fact(outboxEvent, incident, scopeKey, "routarr.incident.review_status", incident.IncidentReviewStatus),
            Fact(outboxEvent, incident, scopeKey, "routarr.incident.routed_product", incident.IncidentRoutedProduct),
        };

        if (incident.TripId is Guid tripId)
        {
            facts.Add(Fact(outboxEvent, incident, scopeKey, "routarr.incident.trip_id", tripId.ToString("D")));
        }

        if (!string.IsNullOrWhiteSpace(payload?.DriverPersonId))
        {
            facts.Add(Fact(outboxEvent, incident, scopeKey, "routarr.incident.driver_person_id", payload.DriverPersonId));
        }

        if (!string.IsNullOrWhiteSpace(payload?.VehicleRefKey))
        {
            facts.Add(Fact(outboxEvent, incident, scopeKey, "routarr.incident.vehicle_ref", payload.VehicleRefKey));
        }

        return facts;
    }

    private static ComplianceCoreProductFactPublicationItem Fact(
        IntegrationOutboxEvent outboxEvent,
        DispatchException incident,
        string scopeKey,
        string factKey,
        string value) =>
        new(
            factKey,
            ValueTypeString,
            scopeKey,
            value,
            BooleanValue: null,
            NumberValue: null,
            DateValue: null,
            SourceEntityType,
            incident.Id,
            outboxEvent.EventKind,
            $"{outboxEvent.Id:N}:{factKey}");
}
