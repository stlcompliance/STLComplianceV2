namespace TrainArr.Api.Contracts;

public sealed record TrainArrTenantSettingsResponse(
    string ProductKey,
    string Scope,
    int SchemaVersion,
    TrainArrTenantSettingsPayload Settings,
    string? UpdatedByDisplayName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    long RowVersion);

public sealed record TrainArrTenantSettingsDefaultsResponse(
    string ProductKey,
    string Scope,
    int SchemaVersion,
    TrainArrTenantSettingsPayload Settings);

public sealed record UpdateTrainArrTenantSettingsRequest(
    TrainArrTenantSettingsPayload Settings,
    long? RowVersion);

public sealed record PatchTrainArrTenantSettingsRequest(
    long? RowVersion,
    System.Text.Json.JsonElement Settings);

public sealed record TrainArrTenantSettingsPayload(
    TrainArrAssignmentSettings Assignment,
    TrainArrProgramVersioningSettings ProgramVersioning,
    TrainArrCertificationLifecycleSettings Certifications,
    TrainArrCompletionSignoffSettings CompletionSignoff,
    TrainArrEvaluationScoringSettings Evaluations,
    TrainArrRemediationSettings Remediation,
    TrainArrEvidenceRecordSettings EvidenceRecords,
    TrainArrNotificationEscalationSettings Notifications,
    TrainArrEnforcementSettings Enforcement,
    TrainArrExternalTrainingSettings ExternalTraining,
    TrainArrTrainerEvaluatorSettings TrainersEvaluators,
    TrainArrComplianceCoreSettings ComplianceCore,
    TrainArrAuditCorrectionSettings AuditCorrection);

public sealed record TrainArrAssignmentSettings(
    bool AutoAssignOnHire,
    bool AutoAssignOnPositionChange,
    bool AutoAssignOnSiteChange,
    bool AutoAssignOnDepartmentChange,
    bool AllowManagerAssignment,
    bool AllowSelfEnrollment,
    bool OptionalEnrollmentRequiresApproval,
    int DefaultAssignmentDueDays,
    int AssignmentGracePeriodDays,
    string AssignmentPriorityDefault);

public sealed record TrainArrProgramVersioningSettings(
    string ProgramVersionChangePolicy,
    bool ReassignOnMajorVersion,
    bool ReassignOnMinorVersion,
    bool AllowInProgressVersionCompletion,
    bool RequireReasonForProgramPublish,
    bool ArchiveSupersededPrograms);

public sealed record TrainArrCertificationLifecycleSettings(
    int? DefaultCertificateValidityDays,
    int DefaultRenewalWindowDays,
    IReadOnlyList<int> DefaultExpirationWarningDays,
    bool AllowEarlyRenewal,
    bool AllowExpiredRenewal,
    bool ExpiredQualificationBlocksWork,
    string CertificateNumberFormat,
    bool RequireCertificatePdf,
    string? CertificateDisplayNameFormat);

public sealed record TrainArrCompletionSignoffSettings(
    string DefaultCompletionMode,
    bool RequireTrainerSignoff,
    bool RequireTraineeAcknowledgement,
    bool RequireManagerApproval,
    bool AllowBulkCompletion,
    bool BulkCompletionRequiresReason,
    bool AllowBackdatedCompletion,
    int BackdatedCompletionMaxDays,
    bool RequireReasonForBackdating,
    string CompletionEditPolicy);

public sealed record TrainArrEvaluationScoringSettings(
    int DefaultPassingScorePercent,
    bool AllowRetakes,
    int MaxRetakeAttempts,
    int RetakeCooldownHours,
    bool RandomizeQuestionOrder,
    bool RandomizeAnswerOrder,
    bool ShowCorrectAnswersAfterAttempt,
    bool RequireEvaluatorCommentOnFail,
    bool RequireEvaluatorCommentOnOverride);

public sealed record TrainArrRemediationSettings(
    bool AcceptIncidentRetrainingRequests,
    int IncidentRetrainingDefaultDueDays,
    bool IncidentRetrainingRequiresReview,
    bool AutoAssignRemediationOnIncident,
    int RepeatIncidentEscalationThreshold,
    int RepeatIncidentLookbackDays,
    bool NotifyManagerOnRemediation,
    bool BlockQualificationDuringRemediation);

public sealed record TrainArrEvidenceRecordSettings(
    bool RequireEvidenceForCompletion,
    IReadOnlyList<string> AllowedEvidenceTypes,
    int MaxEvidenceFileSizeMb,
    int EvidenceRetentionYears,
    bool AllowExternalEvidenceUrl,
    bool RequireEvidenceReview,
    bool AllowTraineeEvidenceUpload,
    bool AllowTrainerEvidenceUpload,
    bool SendFinalRecordsToRecordArr);

public sealed record TrainArrNotificationEscalationSettings(
    bool NotifyOnAssignmentCreated,
    bool NotifyOnDueSoon,
    IReadOnlyList<int> DueSoonReminderDays,
    bool NotifyOnOverdue,
    int OverdueReminderCadenceDays,
    bool NotifyManagerOnOverdue,
    bool NotifyAdminOnCriticalOverdue,
    bool NotifyOnCertificateIssued,
    bool NotifyOnCertificateExpiring,
    IReadOnlyList<int> CertificateExpirationWarningDays);

public sealed record TrainArrEnforcementSettings(
    bool ExposeQualificationStatusToProducts,
    bool AllowProductsToBlockWork,
    string DefaultWorkBlockMode,
    bool AllowManagerOverrideOfBlock,
    bool OverrideRequiresReason,
    int OverrideDurationHours,
    bool PublishQualificationEvents);

public sealed record TrainArrExternalTrainingSettings(
    bool AllowExternalTrainingProvider,
    bool ExternalCompletionRequiresReview,
    bool ExternalCertificateRequiresEvidence,
    IReadOnlyList<string> TrustedProviderIds,
    bool AllowManualExternalCompletionEntry,
    string ExternalRecordConfidenceDefault);

public sealed record TrainArrTrainerEvaluatorSettings(
    bool TrainerMustBeQualified,
    int TrainerQualificationRequiredDays,
    bool AllowTrainerSelfSignoff,
    bool AllowManagerAsEvaluator,
    bool RequireDifferentTrainerAndEvaluator,
    string EvaluatorConflictPolicy,
    string TrainerRosterSource);

public sealed record TrainArrComplianceCoreSettings(
    bool ComplianceCoreEnabled,
    bool RequireComplianceCoreProgramMapping,
    bool AllowUnmappedInternalPrograms,
    string CitationDisplayMode,
    bool RegulatoryChangeReviewRequired,
    bool AutoCreateReviewTasksFromRuleChanges);

public sealed record TrainArrAuditCorrectionSettings(
    bool RequireCorrectionReason,
    bool RequireAdminReasonForDeletion,
    bool AllowCertificateRevocation,
    bool RevocationRequiresReason,
    bool RetainVoidedRecords,
    int AuditEventRetentionYears);
