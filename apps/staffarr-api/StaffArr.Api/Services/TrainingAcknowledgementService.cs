using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class TrainingAcknowledgementService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public async Task<IReadOnlyList<TrainingAcknowledgementResponse>> ListAsync(
        Guid tenantId,
        Guid? personId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.PersonTrainingAcknowledgements
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (personId is Guid filterPersonId)
        {
            query = query.Where(x => x.PersonId == filterPersonId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status.Trim().ToLowerInvariant());
        }

        var rows = await query
            .OrderByDescending(x => x.RequestedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<TrainingAcknowledgementResponse> GetAsync(
        Guid tenantId,
        Guid acknowledgementId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PersonTrainingAcknowledgements.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == acknowledgementId, cancellationToken)
            ?? throw new StlApiException(
                "training_acknowledgements.not_found",
                "Training acknowledgement was not found.",
                404);

        return Map(entity);
    }

    public async Task<TrainingAcknowledgementResponse> AcknowledgeAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid acknowledgementId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PersonTrainingAcknowledgements.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == acknowledgementId,
            cancellationToken)
            ?? throw new StlApiException(
                "training_acknowledgements.not_found",
                "Training acknowledgement was not found.",
                404);

        if (entity.Status != TrainingAcknowledgementStatuses.Pending)
        {
            throw new StlApiException(
                "training_acknowledgements.invalid_status",
                "Only pending acknowledgements can be confirmed.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = TrainingAcknowledgementStatuses.Acknowledged;
        entity.AcknowledgedAt = now;
        entity.AcknowledgedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_acknowledgement.acknowledge",
            tenantId,
            actorUserId,
            "person_training_acknowledgement",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(entity);
    }

    private static TrainingAcknowledgementResponse Map(PersonTrainingAcknowledgement entity) =>
        new(
            entity.Id,
            entity.PersonId,
            entity.TrainarrAcknowledgementRequestId,
            entity.TrainarrAssignmentId,
            entity.TrainingTitle,
            entity.AssignmentReason,
            entity.Summary,
            entity.Status,
            entity.DueAt,
            entity.RequestedAt,
            entity.AcknowledgedAt,
            entity.AcknowledgedByUserId,
            entity.CreatedAt,
            entity.UpdatedAt);
}
