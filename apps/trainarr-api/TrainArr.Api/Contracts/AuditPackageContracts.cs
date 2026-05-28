namespace TrainArr.Api.Contracts;

public sealed record AuditPackageManifestResponse(
    string PackageVersion,
    IReadOnlyList<AuditPackageSectionDescriptor> Sections);

public sealed record AuditPackageSectionDescriptor(
    string Key,
    string FileName,
    string Label,
    string Description);

public sealed record AuditPackageExportResponse(
    Guid PackageId,
    Guid TenantId,
    DateTimeOffset GeneratedAt,
    AuditPackageDateRangeResponse? DateRange,
    AuditPackageCountsResponse Counts,
    IReadOnlyList<TrainArrAuditEventExportItem> AuditEvents,
    IReadOnlyList<AuditPackageTrainingDefinitionItem> TrainingDefinitions,
    IReadOnlyList<AuditPackageTrainingProgramItem> TrainingPrograms,
    IReadOnlyList<AuditPackageTrainingProgramDefinitionItem> TrainingProgramDefinitions,
    IReadOnlyList<AuditPackageTrainingRulePackRequirementItem> TrainingRulePackRequirements,
    IReadOnlyList<AuditPackageTrainingAssignmentItem> TrainingAssignments,
    IReadOnlyList<AuditPackageTrainingEvidenceItem> TrainingEvidence,
    IReadOnlyList<AuditPackageTrainingEvaluationItem> TrainingEvaluations,
    IReadOnlyList<AuditPackageTrainingSignoffItem> TrainingSignoffs,
    IReadOnlyList<AuditPackageQualificationIssueItem> QualificationIssues,
    IReadOnlyList<AuditPackageCertificationPublicationItem> CertificationPublications,
    IReadOnlyList<AuditPackagePersonTrainingHistoryItem> PersonTrainingHistory);

public sealed record AuditPackageDateRangeResponse(
    DateTimeOffset? From,
    DateTimeOffset? To);

public sealed record AuditPackageCountsResponse(
    int AuditEvents,
    int TrainingDefinitions,
    int TrainingPrograms,
    int TrainingProgramDefinitions,
    int TrainingRulePackRequirements,
    int TrainingAssignments,
    int TrainingEvidence,
    int TrainingEvaluations,
    int TrainingSignoffs,
    int QualificationIssues,
    int CertificationPublications,
    int PersonTrainingHistory);

public sealed record TrainArrAuditEventExportItem(
    Guid AuditEventId,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetId,
    string Result,
    string? ReasonCode,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);

public sealed record AuditPackageTrainingDefinitionItem(
    Guid TrainingDefinitionId,
    string DefinitionKey,
    string Name,
    string QualificationKey,
    string QualificationName,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AuditPackageTrainingProgramItem(
    Guid TrainingProgramId,
    string ProgramKey,
    string Name,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AuditPackageTrainingProgramDefinitionItem(
    Guid TrainingProgramId,
    Guid TrainingDefinitionId,
    int SortOrder);

public sealed record AuditPackageTrainingRulePackRequirementItem(
    Guid RequirementId,
    string EntityType,
    Guid EntityId,
    string RulePackKey,
    int? KnownVersionNumber,
    string? KnownStatus,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AuditPackageTrainingAssignmentItem(
    Guid TrainingAssignmentId,
    Guid StaffarrPersonId,
    Guid TrainingDefinitionId,
    Guid? StaffarrIncidentRemediationId,
    Guid? SourceQualificationIssueId,
    string AssignmentReason,
    string Status,
    DateTimeOffset? DueAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AuditPackageTrainingEvidenceItem(
    Guid TrainingEvidenceId,
    Guid TrainingAssignmentId,
    string EvidenceTypeKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    Guid UploadedByUserId,
    DateTimeOffset CreatedAt);

public sealed record AuditPackageTrainingEvaluationItem(
    Guid TrainingEvaluationId,
    Guid TrainingAssignmentId,
    string Result,
    decimal? Score,
    Guid EvaluatorUserId,
    DateTimeOffset EvaluatedAt,
    DateTimeOffset CreatedAt);

public sealed record AuditPackageTrainingSignoffItem(
    Guid TrainingSignoffId,
    Guid TrainingAssignmentId,
    string SignoffRole,
    Guid SignedByUserId,
    DateTimeOffset SignedAt,
    DateTimeOffset CreatedAt);

public sealed record AuditPackageQualificationIssueItem(
    Guid QualificationIssueId,
    Guid TrainingAssignmentId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    string Status,
    DateTimeOffset IssuedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? StatusChangedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AuditPackageCertificationPublicationItem(
    Guid CertificationPublicationId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    string PublicationType,
    string BlockerType,
    string Status,
    DateTimeOffset PublishedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AuditPackagePersonTrainingHistoryItem(
    Guid PersonTrainingHistoryEntryId,
    Guid StaffarrPersonId,
    string EventKind,
    string Summary,
    string RelatedEntityType,
    Guid RelatedEntityId,
    DateTimeOffset OccurredAt,
    DateTimeOffset CreatedAt);
