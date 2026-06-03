namespace TrainArr.Api.Contracts;

public sealed record AssignmentReportSummaryResponse(
    int TotalAssignments,
    int OpenAssignments,
    int CompletedAssignments,
    int OverdueAssignments,
    decimal CompletionRatePercent,
    AssignmentEffectivenessAnalyticsResponse Analytics,
    IReadOnlyList<AssignmentReportSummaryItem> RecentAssignments);

public sealed record AssignmentEffectivenessAnalyticsResponse(
    decimal? AverageCompletionDays,
    decimal? EvaluationPassRatePercent,
    decimal? AverageEvaluationScore,
    decimal EvidenceCoveragePercent,
    decimal SignoffCoveragePercent,
    decimal TotalLaborHours,
    decimal TotalLaborCost,
    decimal? AverageLaborHoursPerCompletedAssignment,
    decimal? AverageLaborCostPerCompletedAssignment,
    int LocalizedContentReferenceCount,
    int DistinctContentLocaleCount);

public sealed record AssignmentReportSummaryItem(
    Guid AssignmentId,
    Guid StaffarrPersonId,
    string DefinitionKey,
    string DefinitionName,
    string Status,
    DateTimeOffset? DueAt,
    bool IsOverdue,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

public sealed record AssignmentOverdueReportResponse(
    DateTimeOffset GeneratedAt,
    int TotalOverdueAssignments,
    IReadOnlyList<AssignmentReportSummaryItem> Items);

public sealed record QualificationReportSummaryResponse(
    int TotalQualifications,
    int IssuedCount,
    int ExpiredCount,
    int SuspendedCount,
    int RevokedCount,
    int ExpiringWithin30Days,
    IReadOnlyList<QualificationReportSummaryItem> RecentQualifications);

public sealed record QualificationReportSummaryItem(
    Guid QualificationIssueId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    string Status,
    DateTimeOffset IssuedAt,
    DateTimeOffset? ExpiresAt,
    bool ExpiringSoon);

public sealed record QualificationExpiringReportResponse(
    DateTimeOffset GeneratedAt,
    int WindowDays,
    int TotalExpiringQualifications,
    IReadOnlyList<QualificationReportSummaryItem> Items);

public sealed record QualificationPointInTimeReportResponse(
    DateTimeOffset GeneratedAt,
    Guid StaffarrPersonId,
    string ActionTask,
    string QualificationKey,
    string QualificationName,
    DateTimeOffset AsOfUtc,
    bool IsQualified,
    string StatusOnDate,
    string QualificationMessage,
    QualificationPointInTimeSourceCertificateResponse? SourceCertificate,
    QualificationPointInTimeProgramVersionResponse? ProgramVersion,
    QualificationPointInTimeExpirationStateResponse ExpirationState,
    IReadOnlyList<string> Restrictions,
    IReadOnlyList<QualificationPointInTimeEvidenceResponse> Evidence,
    IReadOnlyList<QualificationPointInTimeSignoffResponse> Signoffs,
    IReadOnlyList<QualificationPointInTimeAuditTrailItemResponse> AuditTrail);

public sealed record QualificationPointInTimeSourceCertificateResponse(
    Guid QualificationIssueId,
    Guid TrainingAssignmentId,
    Guid GrantPublicationId,
    DateTimeOffset IssuedAt,
    DateTimeOffset? ExpiresAt,
    string StatusOnDate,
    string? LifecycleReason,
    Guid? LifecyclePublicationId);

public sealed record QualificationPointInTimeProgramVersionResponse(
    Guid TrainingProgramVersionId,
    Guid TrainingProgramId,
    string ProgramKey,
    string ProgramName,
    int VersionNumber,
    string Status,
    DateTimeOffset? PublishedAt,
    Guid TrainingDefinitionId,
    string DefinitionKey,
    string DefinitionName);

public sealed record QualificationPointInTimeExpirationStateResponse(
    DateTimeOffset? ExpiresAt,
    bool IsExpired,
    int? DaysUntilExpiration,
    string Message);

public sealed record QualificationPointInTimeEvidenceResponse(
    Guid EvidenceId,
    Guid TrainingAssignmentId,
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Notes,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);

public sealed record QualificationPointInTimeSignoffResponse(
    Guid SignoffId,
    Guid TrainingAssignmentId,
    string SignoffRole,
    Guid SignedByUserId,
    string? Notes,
    DateTimeOffset SignedAt);

public sealed record QualificationPointInTimeAuditTrailItemResponse(
    Guid AuditEventId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    string? ReasonCode,
    Guid? ActorUserId,
    DateTimeOffset OccurredAt);

public sealed record TrainingGapReportItem(
    Guid AssignmentId,
    Guid StaffarrPersonId,
    Guid TrainingDefinitionId,
    string DefinitionKey,
    string DefinitionName,
    string QualificationKey,
    string QualificationName,
    string AssignmentStatus,
    DateTimeOffset? DueAt,
    string GapReasonCode,
    string GapMessage);

public sealed record TrainingGapReportResponse(
    DateTimeOffset GeneratedAt,
    int TotalGaps,
    IReadOnlyList<TrainingGapReportItem> Items);

public sealed record AssignmentLaborReportItem(
    Guid LaborEntryId,
    Guid TrainingAssignmentId,
    string AssignmentDefinitionName,
    string LaborTypeKey,
    decimal HoursWorked,
    decimal CostPerHour,
    decimal TotalCost,
    string? Notes,
    Guid? LoggedByUserId,
    DateTimeOffset LoggedAt);

public sealed record AssignmentLaborReportResponse(
    DateTimeOffset GeneratedAt,
    int TotalEntries,
    decimal TotalLaborHours,
    decimal TotalLaborCost,
    IReadOnlyList<AssignmentLaborReportItem> Items);

public sealed record ProgramCitationGapReportItem(
    Guid TrainingProgramId,
    string ProgramKey,
    string ProgramName,
    string ProgramStatus,
    int CitationAttachmentCount,
    DateTimeOffset UpdatedAt);

public sealed record ProgramCitationGapReportResponse(
    DateTimeOffset GeneratedAt,
    int TotalPrograms,
    int ProgramsMissingCitationCount,
    IReadOnlyList<ProgramCitationGapReportItem> Items);

public sealed record ComplianceReportSummaryResponse(
    int CitationAttachmentCount,
    int RulePackRequirementCount,
    int OpenRemediationCount,
    int TotalRemediationCount,
    int AttentionItemCount,
    IReadOnlyList<ComplianceReportRemediationItem> RecentRemediations);

public sealed record ComplianceReportRemediationItem(
    Guid RemediationId,
    Guid StaffarrPersonId,
    string ReasonCategoryKey,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TrainArrReadinessAlertResponse(
    string AlertType,
    string Severity,
    Guid StaffarrPersonId,
    string Message,
    DateTimeOffset DetectedAt,
    Guid? AssignmentId,
    Guid? QualificationIssueId,
    Guid? EvaluationId,
    Guid? RemediationId);

public sealed record TrainArrCommandCenterResponse(
    DateTimeOffset GeneratedAt,
    AssignmentReportSummaryResponse Assignments,
    QualificationReportSummaryResponse Qualifications,
    ComplianceReportSummaryResponse Compliance,
    int FailedEvaluationCount,
    int RemediationBacklogCount,
    int UpcomingRecertificationCount,
    int ProgramsNeedingReviewCount,
    int UnqualifiedAssignmentRiskCount,
    int AuditReadinessScore,
    IReadOnlyList<TrainArrCommandCenterRiskItem> Risks);

public sealed record TrainArrCommandCenterRiskItem(
    string RiskKey,
    string Severity,
    int Count,
    string Message,
    string? ReportPath);

public sealed record EntityExportFormatDescriptor(
    string FormatKey,
    string ContentType,
    string FileNameTemplate,
    string Description);

public sealed record EntityExportManifestEntity(
    string EntityKey,
    string ExportPath,
    string DisplayName,
    string CsvHeader,
    string Description,
    IReadOnlyList<EntityExportFormatDescriptor> Formats);

public sealed record EntityExportReportDescriptor(
    string ReportKey,
    string ExportPath,
    string DisplayName,
    string Description);

public sealed record EntityExportManifestResponse(
    string PackageVersion,
    IReadOnlyList<EntityExportManifestEntity> Entities,
    IReadOnlyList<EntityExportReportDescriptor> ReportExports,
    IReadOnlyList<string> AuditPackageFormats);

public sealed record CsvExportResult(string ContentType, string FileName, byte[] Content);
