using Microsoft.EntityFrameworkCore;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.SmartImport;

namespace RoutArr.Api.Services;

public sealed class RoutArrSmartImportCommitHandler(RoutArrDbContext db) : ISmartImportDestinationCommitHandler
{
    public string ProductKey => "routarr";

    public async Task<SmartImportDestinationCommitResponse> CommitAsync(
        string entityType,
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SmartImportDestinationCommitResponses.IsCreateOperation(request.Operation))
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "routarr.smart_import.operation_not_supported",
                "RoutArr Smart Import commits currently support reviewed create operations only.");
        }

        if (entityType.Contains("proof", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("pod", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("delivery", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitTripProofAsync(request, cancellationToken);
        }

        if (entityType.Contains("trip", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("dispatch", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("route", StringComparison.OrdinalIgnoreCase))
        {
            return await CommitTripAsync(request, cancellationToken);
        }

        return SmartImportDestinationCommitResponses.ReviewRequired(
            "routarr.smart_import.entity_type_not_supported",
            $"RoutArr does not have a Smart Import commit handler for entity type '{entityType}'.");
    }

    private async Task<SmartImportDestinationCommitResponse> CommitTripAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.Trips.FirstOrDefaultAsync(
            trip => trip.TenantId == request.TenantId && trip.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.Title);
        }

        var payload = request.DeterministicPayload;
        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var tripNumber = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "tripNumber", "dispatchNumber", "routeNumber", "loadNumber"),
            $"SI-TRIP-{shortId}");
        var duplicate = await db.Trips.FirstOrDefaultAsync(
            trip => trip.TenantId == request.TenantId && trip.TripNumber == tripNumber,
            cancellationToken);
        if (duplicate is not null)
        {
            return Committed(duplicate.Id, duplicate.Title);
        }

        var now = DateTimeOffset.UtcNow;
        var title = SmartImportPayloadReader.DisplayName(payload, $"Imported trip {shortId}");
        var tripEntity = new Trip
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            TripNumber = SmartImportPayloadReader.Truncate(tripNumber, 64),
            Title = SmartImportPayloadReader.Truncate(title, 256),
            Description = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "description", "notes", "routeSummary"),
                1024),
            DispatchStatus = NormalizeDispatchStatus(SmartImportPayloadReader.GetString(payload, "dispatchStatus", "status")),
            AssignedDriverPersonId = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "assignedDriverPersonId", "driverPersonId", "driverId"),
                128),
            VehicleRefKey = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "vehicleRefKey", "vehicleId", "assetTag", "unitNumber"),
                128),
            ScheduledStartAt = SmartImportPayloadReader.GetDateTimeOffset(payload, "scheduledStartAt", "pickupAt", "startAt"),
            ScheduledEndAt = SmartImportPayloadReader.GetDateTimeOffset(payload, "scheduledEndAt", "deliveryAt", "endAt"),
            CreatedByUserId = request.ApprovedByPersonId,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Trips.Add(tripEntity);
        AddAudit(request, "smart_import.trip_created", "trip", tripEntity.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(tripEntity.Id, tripEntity.Title);
    }

    private async Task<SmartImportDestinationCommitResponse> CommitTripProofAsync(
        SmartImportDestinationCommitRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await db.TripProofRecords.FirstOrDefaultAsync(
            proof => proof.TenantId == request.TenantId && proof.Id == request.CommitStepId,
            cancellationToken);
        if (existing is not null)
        {
            return Committed(existing.Id, existing.ReferenceKey);
        }

        var payload = request.DeterministicPayload;
        var tripId = SmartImportPayloadReader.GetGuid(payload, "tripId", "routarrTripId");
        if (tripId is null)
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "routarr.smart_import.trip_reference_required",
                "RoutArr proof imports require an existing tripId in the approved payload.");
        }

        var trip = await db.Trips.FirstOrDefaultAsync(
            candidate => candidate.TenantId == request.TenantId && candidate.Id == tripId.Value,
            cancellationToken);
        if (trip is null)
        {
            return SmartImportDestinationCommitResponses.ReviewRequired(
                "routarr.smart_import.trip_not_found",
                "RoutArr proof imports require a tripId that resolves to an existing RoutArr trip.");
        }

        var shortId = SmartImportPayloadReader.ShortId(request.CommitStepId);
        var now = DateTimeOffset.UtcNow;
        var referenceKey = SmartImportPayloadReader.FirstNonEmpty(
            SmartImportPayloadReader.GetString(payload, "referenceKey", "proofNumber", "documentNumber"),
            $"SI-PROOF-{shortId}");
        var proof = new TripProofRecord
        {
            Id = request.CommitStepId,
            TenantId = request.TenantId,
            TripId = trip.Id,
            ProofType = NormalizeProofType(SmartImportPayloadReader.GetString(payload, "proofType", "type")),
            CapturedByPersonId = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "capturedByPersonId", "driverPersonId") ?? request.ApprovedByPersonId.ToString("D"),
                128),
            VehicleRefKey = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "vehicleRefKey", "vehicleId", "assetTag", "unitNumber"),
                128),
            ReferenceKey = SmartImportPayloadReader.Truncate(referenceKey, 128),
            Notes = SmartImportPayloadReader.Truncate(
                SmartImportPayloadReader.GetString(payload, "notes", "description"),
                1024),
            ReviewStatus = TripProofReviewStatuses.PendingReview,
            ReviewNotes = "Created by reviewed Smart Import commit.",
            CapturedAt = SmartImportPayloadReader.GetDateTimeOffset(payload, "capturedAt", "deliveredAt", "pickupAt") ?? now,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.TripProofRecords.Add(proof);
        AddAudit(request, "smart_import.trip_proof_created", "trip_proof", proof.Id.ToString("D"), now);
        await db.SaveChangesAsync(cancellationToken);
        return Committed(proof.Id, proof.ReferenceKey);
    }

    private void AddAudit(
        SmartImportDestinationCommitRequest request,
        string action,
        string targetType,
        string targetId,
        DateTimeOffset occurredAt)
    {
        db.AuditEvents.Add(new RoutArrAuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            ActorUserId = request.ApprovedByPersonId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Result = "success",
            ReasonCode = "smart_import",
            CorrelationId = request.CommitPlanId,
            OccurredAt = occurredAt
        });
    }

    private static string NormalizeDispatchStatus(string? status) =>
        TripDispatchStatuses.All.Contains(status ?? string.Empty) ? status!.ToLowerInvariant() : TripDispatchStatuses.Planned;

    private static string NormalizeProofType(string? proofType) =>
        TripProofTypes.All.Contains(proofType ?? string.Empty) ? proofType!.ToLowerInvariant() : TripProofTypes.Delivery;

    private static SmartImportDestinationCommitResponse Committed(Guid id, string displayName) =>
        SmartImportDestinationCommitResponses.Committed(id.ToString("D"), displayName);
}
