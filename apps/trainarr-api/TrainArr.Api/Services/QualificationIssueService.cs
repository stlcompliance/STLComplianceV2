using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class QualificationIssueService(
    TrainArrDbContext db,
    CertificationPublicationService publicationService,
    TrainingNotificationEnqueueService notificationEnqueueService,
    TrainingEventEnqueueService trainingEventEnqueueService,
    ITrainArrAuditService audit)
{
    private static readonly HashSet<string> ActiveStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "issued",
        "suspended"
    };

    private static readonly HashSet<string> TerminalStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "revoked",
        "expired"
    };

    public async Task<QualificationIssueResponse> IssueOnAssignmentCompletionAsync(
        TrainingAssignment assignment,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.QualificationIssues.FirstOrDefaultAsync(
            x => x.TenantId == assignment.TenantId && x.TrainingAssignmentId == assignment.Id,
            cancellationToken);
        if (existing is not null)
        {
            return MapResponse(existing);
        }

        var definition = assignment.TrainingDefinition;
        var grantMessage =
            $"Qualification issued after successful completion of {definition.Name}.";
        var grantExpiresAt = assignment.DueAt;
        var publication = await publicationService.PublishQualificationGrantAsync(
            new PublishQualificationGrantRequest(
                assignment.TenantId,
                assignment.StaffarrPersonId,
                assignment.Id,
                definition.QualificationKey,
                definition.QualificationName,
                definition.Name,
                grantExpiresAt,
                grantMessage),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var issue = new QualificationIssue
        {
            Id = Guid.NewGuid(),
            TenantId = assignment.TenantId,
            TrainingAssignmentId = assignment.Id,
            StaffarrPersonId = assignment.StaffarrPersonId,
            QualificationKey = definition.QualificationKey,
            QualificationName = definition.QualificationName,
            GrantPublicationId = publication.PublicationId,
            Status = "issued",
            IssuedAt = now,
            ExpiresAt = grantExpiresAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.QualificationIssues.Add(issue);
        await db.SaveChangesAsync(cancellationToken);

        await trainingEventEnqueueService.TryEnqueueAsync(
            assignment.TenantId,
            TrainingDomainEventKinds.QualificationIssued,
            TrainingEventPayloadBuilder.ForQualificationIssued(issue, assignment),
            cancellationToken);

        return MapResponse(issue);
    }

    public async Task<IReadOnlyList<QualificationIssueListItemResponse>> ListAsync(
        Guid tenantId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.QualificationIssues.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().ToLowerInvariant();
            query = query.Where(x => x.Status == normalized);
        }

        return await query
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new QualificationIssueListItemResponse(
                x.Id,
                x.TrainingAssignmentId,
                x.StaffarrPersonId,
                x.QualificationKey,
                x.QualificationName,
                x.Status,
                x.IssuedAt,
                x.ExpiresAt,
                x.StatusChangedAt,
                x.LifecycleReason))
            .ToListAsync(cancellationToken);
    }

    public async Task<QualificationIssueResponse> GetByIdAsync(
        Guid tenantId,
        Guid qualificationIssueId,
        CancellationToken cancellationToken = default)
    {
        var issue = await LoadIssueAsync(tenantId, qualificationIssueId, cancellationToken);
        return MapResponse(issue);
    }

    public async Task<QualificationIssueResponse?> GetByAssignmentAsync(
        Guid tenantId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var issue = await db.QualificationIssues.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.TrainingAssignmentId == assignmentId,
                cancellationToken);
        return issue is null ? null : MapResponse(issue);
    }

    public Task<QualificationIssueResponse> SuspendAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid qualificationIssueId,
        QualificationLifecycleActionRequest request,
        CancellationToken cancellationToken = default) =>
        ApplyLifecycleAsync(
            tenantId,
            actorUserId,
            qualificationIssueId,
            "suspended",
            "suspend",
            request.Reason,
            publicationService.PublishQualificationSuspendAsync,
            "qualification_issue.suspend",
            cancellationToken);

    public Task<QualificationIssueResponse> RevokeAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid qualificationIssueId,
        QualificationLifecycleActionRequest request,
        CancellationToken cancellationToken = default) =>
        ApplyLifecycleAsync(
            tenantId,
            actorUserId,
            qualificationIssueId,
            "revoked",
            "revoke",
            request.Reason,
            publicationService.PublishQualificationRevokeAsync,
            "qualification_issue.revoke",
            cancellationToken);

    public Task<QualificationIssueResponse> ExpireAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid qualificationIssueId,
        QualificationLifecycleActionRequest request,
        CancellationToken cancellationToken = default) =>
        ApplyLifecycleAsync(
            tenantId,
            actorUserId,
            qualificationIssueId,
            "expired",
            "expire",
            request.Reason,
            publicationService.PublishQualificationExpireAsync,
            "qualification_issue.expire",
            cancellationToken);

    public Task<QualificationIssueResponse> ExpireByWorkerAsync(
        Guid tenantId,
        Guid qualificationIssueId,
        CancellationToken cancellationToken = default) =>
        ApplyLifecycleAsync(
            tenantId,
            QualificationExpirationService.WorkerActorUserId,
            qualificationIssueId,
            "expired",
            "expire",
            null,
            publicationService.PublishQualificationExpireAsync,
            "qualification_issue.expire.auto",
            cancellationToken);

    private async Task<QualificationIssueResponse> ApplyLifecycleAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid qualificationIssueId,
        string targetStatus,
        string lifecycleAction,
        string? reason,
        Func<PublishQualificationLifecycleRequest, CancellationToken, Task<CertificationPublicationResponse>> publishAsync,
        string auditAction,
        CancellationToken cancellationToken)
    {
        var issue = await LoadIssueAsync(tenantId, qualificationIssueId, cancellationToken);
        if (TerminalStatuses.Contains(issue.Status))
        {
            throw new StlApiException(
                "qualification_issues.invalid_status",
                $"Qualification issue is already {issue.Status} and cannot be changed.",
                409);
        }

        if (string.Equals(issue.Status, targetStatus, StringComparison.OrdinalIgnoreCase))
        {
            return MapResponse(issue);
        }

        if (!ActiveStatuses.Contains(issue.Status))
        {
            throw new StlApiException(
                "qualification_issues.invalid_status",
                $"Qualification issue status '{issue.Status}' does not allow {lifecycleAction}.",
                409);
        }

        var normalizedReason = NormalizeReason(reason, lifecycleAction, issue.QualificationName);
        var publication = await publishAsync(
            new PublishQualificationLifecycleRequest(
                issue.TenantId,
                issue.StaffarrPersonId,
                issue.GrantPublicationId,
                issue.QualificationKey,
                issue.QualificationName,
                lifecycleAction,
                normalizedReason),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        issue.Status = targetStatus;
        issue.StatusChangedAt = now;
        issue.LifecycleReason = normalizedReason;
        issue.LifecyclePublicationId = publication.PublicationId;
        issue.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await audit.WriteAsync(
            auditAction,
            tenantId,
            actorUserId,
            "qualification_issue",
            issue.Id.ToString(),
            "Succeeded",
            reasonCode: targetStatus,
            cancellationToken: cancellationToken);

        if (string.Equals(targetStatus, "expired", StringComparison.OrdinalIgnoreCase))
        {
            await notificationEnqueueService.TryEnqueueAsync(
                tenantId,
                TrainingNotificationEventKinds.QualificationExpired,
                issue.StaffarrPersonId,
                "qualification_issue",
                issue.Id,
                cancellationToken);
        }

        var eventKind = targetStatus switch
        {
            "suspended" => TrainingDomainEventKinds.QualificationSuspended,
            "revoked" => TrainingDomainEventKinds.QualificationRevoked,
            "expired" => TrainingDomainEventKinds.QualificationExpired,
            _ => null
        };

        if (eventKind is not null)
        {
            await trainingEventEnqueueService.TryEnqueueAsync(
                tenantId,
                eventKind,
                TrainingEventPayloadBuilder.ForQualificationLifecycle(issue, lifecycleAction, now),
                cancellationToken);
        }

        return MapResponse(issue);
    }

    private async Task<QualificationIssue> LoadIssueAsync(
        Guid tenantId,
        Guid qualificationIssueId,
        CancellationToken cancellationToken)
    {
        var issue = await db.QualificationIssues.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Id == qualificationIssueId,
            cancellationToken);
        if (issue is null)
        {
            throw new StlApiException(
                "qualification_issues.not_found",
                "Qualification issue was not found.",
                404);
        }

        return issue;
    }

    private static string NormalizeReason(string? reason, string action, string qualificationName)
    {
        if (!string.IsNullOrWhiteSpace(reason))
        {
            var trimmed = reason.Trim();
            if (trimmed.Length >= 16 && trimmed.Length <= 1024)
            {
                return trimmed;
            }
        }

        return action switch
        {
            "suspend" =>
                $"Qualification {qualificationName} suspended pending review or remediation.",
            "revoke" =>
                $"Qualification {qualificationName} revoked and removed from active certification status.",
            "expire" =>
                $"Qualification {qualificationName} expired and must be renewed through training.",
            _ => $"Qualification {qualificationName} lifecycle action recorded."
        };
    }

    private static QualificationIssueResponse MapResponse(QualificationIssue entity) =>
        new(
            entity.Id,
            entity.TrainingAssignmentId,
            entity.StaffarrPersonId,
            entity.QualificationKey,
            entity.QualificationName,
            entity.GrantPublicationId,
            entity.Status,
            entity.IssuedAt,
            entity.ExpiresAt,
            entity.StatusChangedAt,
            entity.LifecycleReason,
            entity.LifecyclePublicationId);
}
