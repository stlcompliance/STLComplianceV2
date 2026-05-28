using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class TrainingAcknowledgementIngestionService(
    StaffArrDbContext db,
    IStaffArrAuditService audit)
{
    public async Task<TrainingAcknowledgementIngestionResponse> IngestAsync(
        IngestTrainingAcknowledgementRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsurePersonExistsAsync(request.TenantId, request.PersonId, cancellationToken);

        var summary = NormalizeSummary(request.Summary);
        var trainingTitle = NormalizeTitle(request.TrainingTitle);
        var assignmentReason = NormalizeAssignmentReason(request.AssignmentReason);
        var now = DateTimeOffset.UtcNow;

        var existing = await db.PersonTrainingAcknowledgements.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId
                && x.TrainarrAcknowledgementRequestId == request.TrainarrAcknowledgementRequestId,
            cancellationToken);

        if (existing is null)
        {
            existing = new PersonTrainingAcknowledgement
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                PersonId = request.PersonId,
                TrainarrAcknowledgementRequestId = request.TrainarrAcknowledgementRequestId,
                CreatedAt = now,
                RequestedAt = now,
            };
            db.PersonTrainingAcknowledgements.Add(existing);
        }

        existing.PersonId = request.PersonId;
        existing.TrainarrAssignmentId = request.TrainarrAssignmentId;
        existing.TrainingTitle = trainingTitle;
        existing.AssignmentReason = assignmentReason;
        existing.Summary = summary;
        existing.DueAt = request.DueAt;
        if (existing.Status is not TrainingAcknowledgementStatuses.Acknowledged)
        {
            existing.Status = TrainingAcknowledgementStatuses.Pending;
            existing.AcknowledgedAt = null;
            existing.AcknowledgedByUserId = null;
        }

        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_acknowledgement.ingest",
            request.TenantId,
            null,
            "person_training_acknowledgement",
            existing.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new TrainingAcknowledgementIngestionResponse(
            existing.Id,
            existing.TrainarrAcknowledgementRequestId,
            existing.Status);
    }

    public async Task<TrainingAcknowledgementIngestionResponse> SupersedeAsync(
        SupersedeTrainingAcknowledgementRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.PersonTrainingAcknowledgements.FirstOrDefaultAsync(
            x => x.TenantId == request.TenantId
                && x.TrainarrAcknowledgementRequestId == request.TrainarrAcknowledgementRequestId
                && x.PersonId == request.PersonId,
            cancellationToken);

        if (existing is null)
        {
            throw new StlApiException(
                "training_acknowledgements.not_found",
                "Training acknowledgement request was not found.",
                404);
        }

        if (existing.Status == TrainingAcknowledgementStatuses.Acknowledged)
        {
            return new TrainingAcknowledgementIngestionResponse(
                existing.Id,
                existing.TrainarrAcknowledgementRequestId,
                existing.Status);
        }

        var now = DateTimeOffset.UtcNow;
        existing.Status = TrainingAcknowledgementStatuses.Superseded;
        existing.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_acknowledgement.supersede",
            request.TenantId,
            null,
            "person_training_acknowledgement",
            existing.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return new TrainingAcknowledgementIngestionResponse(
            existing.Id,
            existing.TrainarrAcknowledgementRequestId,
            existing.Status);
    }

    public async Task<TrainingAcknowledgementStatusResponse?> GetStatusAsync(
        Guid tenantId,
        Guid trainarrAcknowledgementRequestId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.PersonTrainingAcknowledgements.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.TrainarrAcknowledgementRequestId == trainarrAcknowledgementRequestId,
                cancellationToken);

        return entity is null
            ? null
            : new TrainingAcknowledgementStatusResponse(
                entity.TrainarrAcknowledgementRequestId,
                entity.TrainarrAssignmentId,
                entity.PersonId,
                entity.Status,
                entity.AcknowledgedAt,
                entity.AcknowledgedByUserId);
    }

    private async Task EnsurePersonExistsAsync(
        Guid tenantId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        if (!await db.People.AnyAsync(x => x.TenantId == tenantId && x.Id == personId, cancellationToken))
        {
            throw new StlApiException("training_acknowledgements.person_not_found", "Person was not found.", 404);
        }
    }

    private static string NormalizeSummary(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length is < 3 or > 2048)
        {
            throw new StlApiException(
                "training_acknowledgements.invalid_summary",
                "Summary must be between 3 and 2048 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeTitle(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (normalized.Length is < 1 or > 256)
        {
            throw new StlApiException(
                "training_acknowledgements.invalid_title",
                "Training title must be between 1 and 256 characters.",
                400);
        }

        return normalized;
    }

    private static string NormalizeAssignmentReason(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        return normalized.Length > 64 ? normalized[..64] : normalized;
    }
}
