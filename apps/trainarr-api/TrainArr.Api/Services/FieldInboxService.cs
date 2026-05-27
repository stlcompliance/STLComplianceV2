using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Data;
using TrainArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class FieldInboxService(TrainArrDbContext db)
{
    public async Task<FieldInboxResponse> GetAsync(
        Guid tenantId,
        Guid? staffarrPersonId,
        CancellationToken cancellationToken = default)
    {
        var query = db.TrainingAssignments
            .AsNoTracking()
            .Include(x => x.TrainingDefinition)
            .Where(x => x.TenantId == tenantId
                && TrainingAssignmentService.ActiveAssignmentStatuses.Contains(x.Status));

        if (staffarrPersonId is Guid personId)
        {
            query = query.Where(x => x.StaffarrPersonId == personId);
        }

        var assignments = await query
            .OrderByDescending(x => x.DueAt ?? x.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var items = assignments.Select(assignment => new FieldInboxTaskItem(
            $"trainarr:assignment:{assignment.Id:D}",
            "trainarr",
            "training_assignment",
            assignment.TrainingDefinition?.Name ?? "Training assignment",
            assignment.AssignmentReason,
            assignment.Status,
            null,
            assignment.DueAt,
            assignment.DueAt ?? assignment.UpdatedAt,
            $"/assignments/{assignment.Id:D}")).ToList();

        return FieldInboxRules.BuildProductResponse(items);
    }
}
