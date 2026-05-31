using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class IncidentRoutingService(
    StaffArrDbContext db,
    TrainArrIncidentRemediationClient trainArrClient,
    IStaffArrAuditService audit)
{
    private static readonly HashSet<string> RoutableReasonCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "training_compliance"
    };

    public async Task<RouteIncidentToTrainarrResponse> RouteToTrainarrAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid incidentId,
        CancellationToken cancellationToken = default)
    {
        var incident = await db.PersonnelIncidents
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == incidentId, cancellationToken);

        if (incident is null)
        {
            throw new StlApiException("incidents.not_found", "Incident was not found.", 404);
        }

        if (!RoutableReasonCategories.Contains(incident.ReasonCategoryKey))
        {
            throw new StlApiException(
                "incidents.routing_not_eligible",
                $"Only incidents with reason category {string.Join(", ", RoutableReasonCategories.OrderBy(x => x))} can be routed to TrainArr.",
                400);
        }

        var existingRouting = await db.IncidentTrainarrRoutings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IncidentId == incidentId, cancellationToken);

        if (existingRouting is not null)
        {
            return new RouteIncidentToTrainarrResponse(
                incident.Id,
                incident.PersonId,
                incident.ReasonCategoryKey,
                incident.Status,
                await MapRoutingAsync(tenantId, existingRouting, cancellationToken));
        }

        TrainArrIncidentRemediationResult trainArrResult;
        try
        {
            trainArrResult = await trainArrClient.IngestRemediationAsync(
                new TrainArrIngestIncidentRemediationPayload(
                    tenantId,
                    incident.Id,
                    incident.PersonId,
                    incident.ReasonCategoryKey,
                    incident.Severity,
                    incident.Title,
                    incident.Description,
                    incident.OccurredAt,
                    incident.ReportedAt),
                cancellationToken);
        }
        catch (StlApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new StlApiException(
                "incidents.routing_failed",
                $"TrainArr remediation intake failed: {ex.Message}",
                502);
        }

        var now = DateTimeOffset.UtcNow;
        var routing = new IncidentTrainarrRouting
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IncidentId = incidentId,
            TrainarrRemediationId = trainArrResult.RemediationId,
            RoutingStatus = "routed",
            RoutedAt = now,
            RoutedByUserId = actorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.IncidentTrainarrRoutings.Add(routing);
        incident.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "incident.route_trainarr",
            tenantId,
            actorUserId,
            "personnel_incident",
            incidentId.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new RouteIncidentToTrainarrResponse(
            incident.Id,
            incident.PersonId,
            incident.ReasonCategoryKey,
            incident.Status,
            MapRouting(routing, new IncidentTrainarrRemediationResultResponse(
                trainArrResult.RemediationId,
                trainArrResult.Status,
                trainArrResult.ReasonCategoryKey,
                incident.Severity,
                incident.Title,
                trainArrResult.CreatedAt)));
    }

    public async Task<IncidentTrainarrRoutingResponse?> GetRoutingForIncidentAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken = default)
    {
        var routing = await db.IncidentTrainarrRoutings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.IncidentId == incidentId, cancellationToken);

        return routing is null ? null : await MapRoutingAsync(tenantId, routing, cancellationToken);
    }

    internal static IncidentTrainarrRoutingResponse MapRouting(
        IncidentTrainarrRouting routing,
        IncidentTrainarrRemediationResultResponse? remediationResult = null) =>
        new(
            routing.RoutingStatus,
            routing.TrainarrRemediationId,
            routing.RoutedAt,
            routing.RoutedByUserId,
            remediationResult);

    private async Task<IncidentTrainarrRoutingResponse> MapRoutingAsync(
        Guid tenantId,
        IncidentTrainarrRouting routing,
        CancellationToken cancellationToken)
    {
        IncidentTrainarrRemediationResultResponse? remediationResult = null;
        try
        {
            var detail = await trainArrClient.GetRemediationAsync(
                tenantId,
                routing.TrainarrRemediationId,
                cancellationToken);
            if (detail is not null)
            {
                remediationResult = new IncidentTrainarrRemediationResultResponse(
                    detail.RemediationId,
                    detail.Status,
                    detail.ReasonCategoryKey,
                    detail.Severity,
                    detail.Title,
                    detail.CreatedAt);
            }
        }
        catch (StlApiException)
        {
            remediationResult = null;
        }

        return MapRouting(routing, remediationResult);
    }
}
