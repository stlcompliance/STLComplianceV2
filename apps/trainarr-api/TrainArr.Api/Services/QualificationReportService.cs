using System.Text;
using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace TrainArr.Api.Services;

public sealed class QualificationReportService(TrainArrDbContext db)
{
    private const int RecentLimit = 25;
    private const int ExpiringSoonDays = 30;

    public async Task<QualificationReportSummaryResponse> GetSummaryAsync(
        Guid tenantId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiringThreshold = now.AddDays(ExpiringSoonDays);

        var qualifications = await db.QualificationIssues
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var filtered = qualifications
            .Where(x => MatchesStatusFilter(x, status))
            .ToList();

        var issuedCount = qualifications.Count(x => string.Equals(x.Status, "issued", StringComparison.OrdinalIgnoreCase));
        var expiredCount = qualifications.Count(x => string.Equals(x.Status, "expired", StringComparison.OrdinalIgnoreCase));
        var suspendedCount = qualifications.Count(x => string.Equals(x.Status, "suspended", StringComparison.OrdinalIgnoreCase));
        var revokedCount = qualifications.Count(x => string.Equals(x.Status, "revoked", StringComparison.OrdinalIgnoreCase));
        var expiringSoon = qualifications.Count(x =>
            string.Equals(x.Status, "issued", StringComparison.OrdinalIgnoreCase)
            && x.ExpiresAt is not null
            && x.ExpiresAt <= expiringThreshold
            && x.ExpiresAt >= now);

        var recent = filtered
            .OrderByDescending(x => x.IssuedAt)
            .Take(RecentLimit)
            .Select(x => new QualificationReportSummaryItem(
                x.Id,
                x.StaffarrPersonId,
                x.QualificationKey,
                x.QualificationName,
                x.Status,
                x.IssuedAt,
                x.ExpiresAt,
                string.Equals(x.Status, "issued", StringComparison.OrdinalIgnoreCase)
                && x.ExpiresAt is not null
                && x.ExpiresAt <= expiringThreshold
                && x.ExpiresAt >= now))
            .ToList();

        return new QualificationReportSummaryResponse(
            filtered.Count,
            issuedCount,
            expiredCount,
            suspendedCount,
            revokedCount,
            expiringSoon,
            recent);
    }

    public async Task<CsvExportResult> ExportSummaryCsvAsync(
        Guid tenantId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var summary = await GetSummaryAsync(tenantId, status, cancellationToken);
        var builder = new StringBuilder();
        builder.AppendLine(
            "qualificationIssueId,staffarrPersonId,qualificationKey,qualificationName,status,issuedAt,expiresAt,expiringSoon");

        foreach (var item in summary.RecentQualifications)
        {
            builder.Append(CsvEscape(item.QualificationIssueId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.StaffarrPersonId.ToString()));
            builder.Append(',');
            builder.Append(CsvEscape(item.QualificationKey));
            builder.Append(',');
            builder.Append(CsvEscape(item.QualificationName));
            builder.Append(',');
            builder.Append(CsvEscape(item.Status));
            builder.Append(',');
            builder.Append(CsvEscape(item.IssuedAt.ToString("O")));
            builder.Append(',');
            builder.Append(CsvEscape(item.ExpiresAt?.ToString("O") ?? string.Empty));
            builder.Append(',');
            builder.AppendLine(item.ExpiringSoon ? "true" : "false");
        }

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        return new CsvExportResult(
            "text/csv",
            $"trainarr-qualification-report-{timestamp}.csv",
            Encoding.UTF8.GetBytes(builder.ToString()));
    }

    public async Task<QualificationExpiringReportResponse> GetExpiringReportAsync(
        Guid tenantId,
        int? windowDays,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var days = Math.Clamp(windowDays ?? ExpiringSoonDays, 1, 365);
        var expiringThreshold = now.AddDays(days);

        var qualifications = await db.QualificationIssues
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.Status == "issued"
                && x.ExpiresAt != null
                && x.ExpiresAt >= now
                && x.ExpiresAt <= expiringThreshold)
            .OrderBy(x => x.ExpiresAt)
            .Take(RecentLimit)
            .ToListAsync(cancellationToken);

        var items = qualifications
            .Select(x => new QualificationReportSummaryItem(
                x.Id,
                x.StaffarrPersonId,
                x.QualificationKey,
                x.QualificationName,
                x.Status,
                x.IssuedAt,
                x.ExpiresAt,
                ExpiringSoon: true))
            .ToList();

        var total = await db.QualificationIssues.CountAsync(
            x => x.TenantId == tenantId
                && x.Status == "issued"
                && x.ExpiresAt != null
                && x.ExpiresAt >= now
                && x.ExpiresAt <= expiringThreshold,
            cancellationToken);

        return new QualificationExpiringReportResponse(now, days, total, items);
    }

    public async Task<QualificationPointInTimeReportResponse> GetPointInTimeReportAsync(
        Guid tenantId,
        Guid staffarrPersonId,
        string qualificationKey,
        string actionTask,
        DateTimeOffset? asOfUtc,
        CancellationToken cancellationToken = default)
    {
        var normalizedQualificationKey = NormalizeQualificationKey(qualificationKey);
        var normalizedActionTask = NormalizeActionTask(actionTask);
        var effectiveAt = asOfUtc ?? DateTimeOffset.UtcNow;
        var generatedAt = DateTimeOffset.UtcNow;

        var issue = await db.QualificationIssues
            .AsNoTracking()
            .Include(x => x.TrainingAssignment)
                .ThenInclude(x => x.TrainingDefinition)
            .Include(x => x.TrainingAssignment)
                .ThenInclude(x => x.EvidenceRecords)
            .Include(x => x.TrainingAssignment)
                .ThenInclude(x => x.Evaluation)
            .Include(x => x.TrainingAssignment)
                .ThenInclude(x => x.Signoffs)
            .Where(x => x.TenantId == tenantId
                && x.StaffarrPersonId == staffarrPersonId
                && x.QualificationKey == normalizedQualificationKey
                && x.IssuedAt <= effectiveAt)
            .OrderByDescending(x => x.IssuedAt)
            .FirstOrDefaultAsync(cancellationToken);

        TrainingAssignment? assignment = issue?.TrainingAssignment;
        if (assignment is null)
        {
            assignment = await db.TrainingAssignments
                .AsNoTracking()
                .Include(x => x.TrainingDefinition)
                .Include(x => x.EvidenceRecords)
                .Include(x => x.Evaluation)
                .Include(x => x.Signoffs)
                .Where(x => x.TenantId == tenantId
                    && x.StaffarrPersonId == staffarrPersonId
                    && x.TrainingDefinition.QualificationKey == normalizedQualificationKey
                    && x.CreatedAt <= effectiveAt)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (issue is null && assignment is null)
        {
            return BuildNoCertificateReport(
                generatedAt,
                staffarrPersonId,
                normalizedActionTask,
                normalizedQualificationKey,
                effectiveAt);
        }

        var sourceDate = issue?.IssuedAt ?? assignment?.CompletedAt ?? assignment?.CreatedAt ?? effectiveAt;
        var statusOnDate = issue is null
            ? "none"
            : await ResolveStatusOnDateAsync(tenantId, issue, effectiveAt, cancellationToken);
        var isQualified = string.Equals(statusOnDate, "issued", StringComparison.OrdinalIgnoreCase);

        var sourceCertificate = issue is null
            ? null
            : new QualificationPointInTimeSourceCertificateResponse(
                issue.Id,
                issue.TrainingAssignmentId,
                issue.GrantPublicationId,
                issue.IssuedAt,
                issue.ExpiresAt,
                statusOnDate,
                issue.LifecycleReason,
                issue.LifecyclePublicationId);

        var programVersion = assignment is null
            ? null
            : await ResolveProgramVersionSnapshotAsync(
                tenantId,
                assignment.TrainingDefinitionId,
                sourceDate,
                cancellationToken);

        var restrictions = BuildRestrictions(issue, statusOnDate, effectiveAt);
        var expirationState = BuildExpirationState(issue, statusOnDate, effectiveAt);
        var evidence = assignment is null
            ? new List<QualificationPointInTimeEvidenceResponse>()
            : assignment.EvidenceRecords
                .Where(x => x.CreatedAt <= effectiveAt)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new QualificationPointInTimeEvidenceResponse(
                    x.Id,
                    x.TrainingAssignmentId,
                    x.EvidenceTypeKey,
                    x.FileName,
                    x.ContentType,
                    x.SizeBytes,
                    x.Notes,
                    x.UploadedByUserId,
                    x.CreatedAt))
                .ToList();

        var signoffs = assignment is null
            ? new List<QualificationPointInTimeSignoffResponse>()
            : assignment.Signoffs
                .Where(x => x.SignedAt <= effectiveAt)
                .OrderByDescending(x => x.SignedAt)
                .Select(x => new QualificationPointInTimeSignoffResponse(
                    x.Id,
                    x.TrainingAssignmentId,
                    x.SignoffRole,
                    x.SignedByUserId,
                    x.Notes,
                    x.SignedAt))
                .ToList();

        var auditTrail = await LoadPointInTimeAuditTrailAsync(
            tenantId,
            issue,
            assignment,
            effectiveAt,
            cancellationToken);

        return new QualificationPointInTimeReportResponse(
            generatedAt,
            staffarrPersonId,
            normalizedActionTask,
            normalizedQualificationKey,
            issue?.QualificationName ?? assignment?.TrainingDefinition.QualificationName ?? normalizedQualificationKey,
            effectiveAt,
            isQualified,
            statusOnDate,
            BuildPointInTimeMessage(issue, statusOnDate, effectiveAt),
            sourceCertificate,
            programVersion,
            expirationState,
            restrictions,
            evidence,
            signoffs,
            auditTrail);
    }

    private static bool MatchesStatusFilter(QualificationIssue issue, string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(issue.Status, status.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task<IReadOnlyList<QualificationPointInTimeAuditTrailItemResponse>> LoadPointInTimeAuditTrailAsync(
        Guid tenantId,
        QualificationIssue? issue,
        TrainingAssignment? assignment,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken)
    {
        var targetIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var targetTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "training_assignment",
            "training_evidence",
            "training_evaluation",
            "training_signoff",
            "qualification_issue",
        };

        if (issue is not null)
        {
            targetIds.Add(issue.Id.ToString());
        }

        if (assignment is not null)
        {
            targetIds.Add(assignment.Id.ToString());
            targetIds.UnionWith(assignment.EvidenceRecords.Select(x => x.Id.ToString()));
            targetIds.UnionWith(assignment.Signoffs.Select(x => x.Id.ToString()));
            if (assignment.Evaluation is not null)
            {
                targetIds.Add(assignment.Evaluation.Id.ToString());
            }
        }

        var query = db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.OccurredAt <= effectiveAt);

        var events = await query
            .Where(x => targetTypes.Contains(x.TargetType)
                        && x.TargetId != null
                        && targetIds.Contains(x.TargetId!))
            .OrderBy(x => x.OccurredAt)
            .ThenBy(x => x.Id)
            .Select(x => new QualificationPointInTimeAuditTrailItemResponse(
                x.Id,
                x.Action,
                x.TargetType,
                x.TargetId,
                x.Result,
                x.ReasonCode,
                x.ActorUserId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        if (issue is not null)
        {
            events.Insert(0, new QualificationPointInTimeAuditTrailItemResponse(
                issue.Id,
                "qualification_issue.issued",
                "qualification_issue",
                issue.Id.ToString(),
                "Succeeded",
                "issued",
                null,
                issue.IssuedAt));
        }

        return events
            .OrderBy(x => x.OccurredAt)
            .ThenBy(x => x.AuditEventId)
            .ToList();
    }

    private async Task<string> ResolveStatusOnDateAsync(
        Guid tenantId,
        QualificationIssue issue,
        DateTimeOffset effectiveAt,
        CancellationToken cancellationToken)
    {
        var auditStatuses = await db.AuditEvents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId
                && x.TargetType == "qualification_issue"
                && x.TargetId == issue.Id.ToString()
                && x.Result == "Succeeded"
                && x.OccurredAt <= effectiveAt)
            .OrderBy(x => x.OccurredAt)
            .Select(x => new
            {
                x.OccurredAt,
                x.Action,
                x.ReasonCode,
            })
            .ToListAsync(cancellationToken);

        var timeline = new List<(DateTimeOffset OccurredAt, string Status)>(auditStatuses.Count + 1)
        {
            (issue.IssuedAt, "issued")
        };

        foreach (var auditEvent in auditStatuses)
        {
            var status = NormalizeLifecycleStatus(auditEvent.ReasonCode ?? auditEvent.Action);
            if (status is not null)
            {
                timeline.Add((auditEvent.OccurredAt, status));
            }
        }

        var statusOnDate = timeline
            .OrderBy(x => x.OccurredAt)
            .Last()
            .Status;

        if (string.Equals(statusOnDate, "issued", StringComparison.OrdinalIgnoreCase)
            && issue.ExpiresAt is DateTimeOffset expiresAt
            && expiresAt <= effectiveAt)
        {
            statusOnDate = "expired";
        }

        return statusOnDate;
    }

    private async Task<QualificationPointInTimeProgramVersionResponse?> ResolveProgramVersionSnapshotAsync(
        Guid tenantId,
        Guid trainingDefinitionId,
        DateTimeOffset sourceDate,
        CancellationToken cancellationToken)
    {
        var version = await db.TrainingProgramVersions
            .AsNoTracking()
            .Include(x => x.TrainingProgram)
            .Include(x => x.VersionDefinitions)
                .ThenInclude(x => x.TrainingDefinition)
            .Where(x => x.TenantId == tenantId
                && x.PublishedAt != null
                && x.PublishedAt <= sourceDate
                && x.VersionDefinitions.Any(v => v.TrainingDefinitionId == trainingDefinitionId))
            .OrderByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (version is null)
        {
            return null;
        }

        var definition = version.VersionDefinitions.FirstOrDefault(x => x.TrainingDefinitionId == trainingDefinitionId)?.TrainingDefinition;
        return new QualificationPointInTimeProgramVersionResponse(
            version.Id,
            version.TrainingProgramId,
            version.TrainingProgram.ProgramKey,
            version.TrainingProgram.Name,
            version.VersionNumber,
            version.Status,
            version.PublishedAt,
            trainingDefinitionId,
            definition?.DefinitionKey ?? string.Empty,
            definition?.Name ?? string.Empty);
    }

    private static IReadOnlyList<string> BuildRestrictions(
        QualificationIssue? issue,
        string statusOnDate,
        DateTimeOffset effectiveAt)
    {
        var restrictions = new List<string>();

        if (issue is null)
        {
            restrictions.Add("No qualification certificate was issued on or before the requested date.");
            return restrictions;
        }

        switch (statusOnDate.ToLowerInvariant())
        {
            case "issued":
                if (issue.ExpiresAt is DateTimeOffset expiresAt && expiresAt > effectiveAt)
                {
                    restrictions.Add("Qualification was active on the requested date.");
                }
                else if (issue.ExpiresAt is null)
                {
                    restrictions.Add("No expiration date was recorded for the certificate.");
                }
                break;
            case "suspended":
                restrictions.Add(issue.LifecycleReason ?? "Qualification was suspended and cannot authorize work.");
                break;
            case "revoked":
                restrictions.Add(issue.LifecycleReason ?? "Qualification was revoked and cannot authorize work.");
                break;
            case "expired":
                restrictions.Add(issue.LifecycleReason ?? "Qualification expired and must be renewed through training.");
                break;
            default:
                restrictions.Add($"Qualification status on the requested date was '{statusOnDate}'.");
                break;
        }

        return restrictions;
    }

    private static QualificationPointInTimeExpirationStateResponse BuildExpirationState(
        QualificationIssue? issue,
        string statusOnDate,
        DateTimeOffset effectiveAt)
    {
        if (issue is null)
        {
            return new QualificationPointInTimeExpirationStateResponse(
                null,
                IsExpired: true,
                DaysUntilExpiration: null,
                "No qualification certificate was issued.");
        }

        if (issue.ExpiresAt is not DateTimeOffset expiresAt)
        {
            return new QualificationPointInTimeExpirationStateResponse(
                null,
                IsExpired: false,
                DaysUntilExpiration: null,
                "No expiration date was recorded for the certificate.");
        }

        var isExpired = expiresAt <= effectiveAt || string.Equals(statusOnDate, "expired", StringComparison.OrdinalIgnoreCase);
        var daysUntilExpiration = (int)Math.Floor((expiresAt - effectiveAt).TotalDays);
        var message = isExpired
            ? $"Qualification expired on {expiresAt:u}."
            : $"Qualification expires on {expiresAt:u} ({Math.Max(daysUntilExpiration, 0)} day(s) remaining).";

        return new QualificationPointInTimeExpirationStateResponse(
            expiresAt,
            isExpired,
            daysUntilExpiration,
            message);
    }

    private static QualificationPointInTimeReportResponse BuildNoCertificateReport(
        DateTimeOffset generatedAt,
        Guid staffarrPersonId,
        string actionTask,
        string qualificationKey,
        DateTimeOffset effectiveAt)
    {
        return new QualificationPointInTimeReportResponse(
            generatedAt,
            staffarrPersonId,
            actionTask,
            qualificationKey,
            qualificationKey,
            effectiveAt,
            false,
            "none",
            "No qualification certificate was found on or before the requested date.",
            null,
            null,
            new QualificationPointInTimeExpirationStateResponse(
                null,
                IsExpired: true,
                DaysUntilExpiration: null,
                "No qualification certificate was issued."),
            ["No qualification certificate was issued on or before the requested date."],
            [],
            [],
            []);
    }

    private static string BuildPointInTimeMessage(
        QualificationIssue? issue,
        string statusOnDate,
        DateTimeOffset effectiveAt)
    {
        if (issue is null)
        {
            return "No qualification certificate was found on or before the requested date.";
        }

        return statusOnDate.ToLowerInvariant() switch
        {
            "issued" =>
                $"Qualification '{issue.QualificationName}' was active on {effectiveAt:u}.",
            "suspended" =>
                $"Qualification '{issue.QualificationName}' was suspended on the requested date.",
            "revoked" =>
                $"Qualification '{issue.QualificationName}' was revoked on the requested date.",
            "expired" =>
                $"Qualification '{issue.QualificationName}' was expired on the requested date.",
            _ =>
                $"Qualification '{issue.QualificationName}' had status '{statusOnDate}' on the requested date.",
        };
    }

    private static string NormalizeQualificationKey(string qualificationKey)
    {
        var normalized = qualificationKey.Trim().ToLowerInvariant();
        if (normalized.Length == 0)
        {
            throw new StlApiException(
                "qualification_report.validation",
                "Qualification key is required.",
                400);
        }

        return normalized;
    }

    private static string NormalizeActionTask(string actionTask)
    {
        var normalized = actionTask.Trim();
        if (normalized.Length < 3)
        {
            throw new StlApiException(
                "qualification_report.validation",
                "Action/task is required.",
                400);
        }

        return normalized;
    }

    private static string? NormalizeLifecycleStatus(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "issued" => "issued",
            "qualification_issue.issued" => "issued",
            "suspend" => "suspended",
            "qualification_issue.suspend" => "suspended",
            "revoke" => "revoked",
            "qualification_issue.revoke" => "revoked",
            "expire" => "expired",
            "qualification_issue.expire" => "expired",
            "qualification_issue.expire.auto" => "expired",
            "reinstate" => "issued",
            "qualification_issue.reinstate" => "issued",
            _ => null,
        };
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
