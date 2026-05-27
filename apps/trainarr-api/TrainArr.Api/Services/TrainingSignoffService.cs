using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class TrainingSignoffService(
    TrainArrDbContext db,
    TrainingAssignmentService assignmentService,
    ITrainArrAuditService audit)
{
    public async Task<IReadOnlyList<TrainingSignoffResponse>> ListForAssignmentAsync(
        Guid tenantId,
        Guid? trainingAssignmentId,
        CancellationToken cancellationToken = default)
    {
        var query = db.TrainingSignoffs.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (trainingAssignmentId is Guid assignmentId)
        {
            query = query.Where(x => x.TrainingAssignmentId == assignmentId);
        }

        return await query
            .OrderBy(x => x.SignoffRole)
            .ThenByDescending(x => x.SignedAt)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingSignoffResponse> SubmitAsync(
        Guid tenantId,
        Guid actorUserId,
        SubmitTrainingSignoffRequest request,
        CancellationToken cancellationToken = default)
    {
        var assignment = await assignmentService.LoadAssignmentEntityAsync(
            tenantId,
            request.TrainingAssignmentId,
            cancellationToken);
        EnsureAssignmentOpen(assignment);

        var signoffRole = NormalizeSignoffRole(request.SignoffRole);
        var existing = assignment.Signoffs.FirstOrDefault(x =>
            string.Equals(x.SignoffRole, signoffRole, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            throw new StlApiException(
                "signoffs.already_recorded",
                $"A {signoffRole} signoff already exists for this assignment.",
                409);
        }

        var now = DateTimeOffset.UtcNow;
        var signoff = new TrainingSignoff
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TrainingAssignmentId = assignment.Id,
            SignoffRole = signoffRole,
            SignedByUserId = actorUserId,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            SignedAt = now,
            CreatedAt = now
        };

        db.TrainingSignoffs.Add(signoff);
        assignment.Signoffs.Add(signoff);

        if (string.Equals(assignment.Status, "assigned", StringComparison.OrdinalIgnoreCase))
        {
            assignment.Status = "in_progress";
            assignment.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            "training_signoff.submit",
            tenantId,
            actorUserId,
            "training_signoff",
            signoff.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return Map(signoff);
    }

    private static void EnsureAssignmentOpen(TrainingAssignment assignment)
    {
        if (!TrainingAssignmentService.ActiveAssignmentStatuses.Contains(assignment.Status))
        {
            throw new StlApiException(
                "signoffs.assignment_closed",
                "Signoffs can only be recorded for assigned or in-progress assignments.",
                409);
        }
    }

    private static string NormalizeSignoffRole(string signoffRole)
    {
        var normalized = signoffRole.Trim().ToLowerInvariant();
        if (!TrainingCompletionRequirements.AllowedSignoffRoles.Contains(normalized))
        {
            throw new StlApiException(
                "signoffs.validation",
                $"Signoff role must be one of: {string.Join(", ", TrainingCompletionRequirements.AllowedSignoffRoles.OrderBy(x => x))}.",
                400);
        }

        return normalized;
    }

    private static TrainingSignoffResponse Map(TrainingSignoff entity) =>
        new(
            entity.Id,
            entity.TrainingAssignmentId,
            entity.SignoffRole,
            entity.SignedByUserId,
            entity.Notes,
            entity.SignedAt);
}
