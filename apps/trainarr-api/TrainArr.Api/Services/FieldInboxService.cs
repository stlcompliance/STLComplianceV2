using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class FieldInboxService(TrainArrDbContext db, IConfiguration configuration)
{
    public async Task<FieldInboxResponse> GetAsync(
        Guid tenantId,
        Guid? staffarrPersonId,
        CancellationToken cancellationToken = default)
    {
        var frontendBaseUrl = configuration["TrainArr:FrontendBaseUrl"]
            ?? configuration["Cors:TrainArrFrontendOrigin"];

        var query = db.TrainingAssignments
            .AsNoTracking()
            .Include(x => x.TrainingDefinition)
            .Include(x => x.Evaluation)
            .Include(x => x.Signoffs)
            .Include(x => x.EvidenceRecords)
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

        var items = assignments.Select(assignment => MapTaskItem(assignment, frontendBaseUrl)).ToList();

        return FieldInboxRules.BuildProductResponse(items);
    }

    private static FieldInboxTaskItem MapTaskItem(TrainingAssignment assignment, string? frontendBaseUrl)
    {
        var (deepLinkPath, blockedReason) = ResolveDeepLink(assignment);
        return new FieldInboxTaskItem(
            $"trainarr:assignment:{assignment.Id:D}",
            "trainarr",
            "training_assignment",
            assignment.TrainingDefinition?.Name ?? "Training assignment",
            assignment.AssignmentReason,
            assignment.Status,
            null,
            assignment.DueAt,
            assignment.DueAt ?? assignment.UpdatedAt,
            deepLinkPath,
            blockedReason,
            FieldInboxDeepLinkBuilder.BuildProductDeepLinkUrl(frontendBaseUrl, deepLinkPath));
    }

    private static (string DeepLinkPath, string? BlockedReason) ResolveDeepLink(TrainingAssignment assignment)
    {
        var assignmentPath = $"/assignments/{assignment.Id:D}";
        var evidencePath = $"{assignmentPath}/evidence";

        if (string.Equals(assignment.Status, "assigned", StringComparison.OrdinalIgnoreCase)
            && assignment.EvidenceRecords.Count == 0)
        {
            return (evidencePath, "Upload training evidence to begin");
        }

        if (string.Equals(assignment.Status, "in_progress", StringComparison.OrdinalIgnoreCase)
            && assignment.EvidenceRecords.Count == 0)
        {
            return (evidencePath, "Evidence required");
        }

        if (string.Equals(assignment.Status, "in_progress", StringComparison.OrdinalIgnoreCase)
            && !TrainingCompletionRequirements.AreMet(assignment))
        {
            return (assignmentPath, "Evaluation and signoffs required");
        }

        return (assignmentPath, null);
    }
}
