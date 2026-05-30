using System.ComponentModel.DataAnnotations.Schema;
using STLCompliance.Shared.Data;

namespace ComplianceCore.Api.Entities;

public sealed class ImportSession : IHasTenant
{
    public Guid ImportSessionId { get; set; }

    public Guid TenantId { get; set; }

    public Guid? UploadedByPersonId { get; set; }

    public string SourceFilename { get; set; } = string.Empty;

    public string SourceHash { get; set; } = string.Empty;

    public string ImportType { get; set; } = ImportSessionImportTypes.ComplianceCoreCsvBundle;

    public string Status { get; set; } = ImportSessionStatuses.Uploaded;

    public string ValidationStatus { get; set; } = ImportSessionValidationStatuses.NotValidated;

    public string MappingStatus { get; set; } = ImportSessionMappingStatuses.NotStarted;

    public string CommitStatus { get; set; } = ImportSessionCommitStatuses.NotCommitted;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ValidatedAt { get; set; }

    public DateTimeOffset? MappedAt { get; set; }

    public DateTimeOffset? CommittedAt { get; set; }

    public DateTimeOffset? RejectedAt { get; set; }

    public string Notes { get; set; } = string.Empty;
}

public sealed class ImportSessionSourceFile : IHasTenant
{
    public Guid ImportSessionSourceFileId { get; set; }

    public Guid TenantId { get; set; }

    public Guid ImportSessionId { get; set; }

    public string SourceFile { get; set; } = string.Empty;

    public string OriginalFilename { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string FileHash { get; set; } = string.Empty;

    public long ByteLength { get; set; }

    public string ValidationStatus { get; set; } = ImportRowValidationStatuses.Pending;

    public string ValidationErrorsJson { get; set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; }

    public ImportSession? ImportSession { get; set; }
}

[NotMapped]
public abstract class ImportStagedRowBase : IHasTenant
{
    public Guid StagedRowId { get; set; }

    public Guid TenantId { get; set; }

    public Guid ImportSessionId { get; set; }

    public string SourceFile { get; set; } = string.Empty;

    public int RowNumber { get; set; }

    public string RawRowJson { get; set; } = "{}";

    public string NormalizedRowJson { get; set; } = "{}";

    public string RowHash { get; set; } = string.Empty;

    public string ValidationStatus { get; set; } = ImportRowValidationStatuses.Pending;

    public string ValidationErrorsJson { get; set; } = "[]";

    public string CanonicalKeyCandidate { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public ImportSession? ImportSession { get; set; }
}

public sealed class ImportStagedRulePack : ImportStagedRowBase;

public sealed class ImportStagedRuleRequirement : ImportStagedRowBase;

public sealed class ImportStagedFactRequirement : ImportStagedRowBase;

public sealed class ImportStagedRegulatoryMapping : ImportStagedRowBase;

public sealed class ImportStagedControlledVocabulary : ImportStagedRowBase;

public sealed class ImportStagedVocabularyAlias : ImportStagedRowBase;

public sealed class ImportStagedComplianceKey : ImportStagedRowBase;

public sealed class ImportStagedMaterialKey : ImportStagedRowBase;

public sealed class ImportStagedSdsReference : ImportStagedRowBase;

public sealed class ImportStagedEvidenceReference : ImportStagedRowBase;

public sealed class ImportStagedExceptionExemption : ImportStagedRowBase;

public sealed class ImportStagedMappingCandidate : IHasTenant
{
    public Guid MappingCandidateId { get; set; }

    public Guid TenantId { get; set; }

    public Guid ImportSessionId { get; set; }

    public Guid StagedRowId { get; set; }

    public string StagedSourceFile { get; set; } = string.Empty;

    public int StagedRowNumber { get; set; }

    public string SourceKey { get; set; } = string.Empty;

    public string SourceLabel { get; set; } = string.Empty;

    public Guid? EvidenceOptionId { get; set; }

    public string EvidenceOptionKey { get; set; } = string.Empty;

    public string EvidenceOptionLabel { get; set; } = string.Empty;

    public string OptionLogicGroup { get; set; } = EvidenceOptionLogicTypes.AnyOf;

    public string TargetKind { get; set; } = MappingTargetKinds.NewEvidenceReference;

    public string TargetId { get; set; } = string.Empty;

    public string TargetKey { get; set; } = string.Empty;

    public string TargetLabel { get; set; } = string.Empty;

    public decimal ConfidenceScore { get; set; }

    public string ConfidenceBand { get; set; } = MappingConfidenceBands.NoMatch;

    public string MatchReasonsJson { get; set; } = "[]";

    public string RiskFlagsJson { get; set; } = "[]";

    public string ProposedAction { get; set; } = MappingProposedActions.CreateNew;

    public bool SatisfiesRequirementIfConfirmed { get; set; }

    public bool RequiresAdditionalSupportingEvidence { get; set; }

    public bool RequiresConfirmation { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public ImportSession? ImportSession { get; set; }
}

public sealed class ImportStagedMappingDecision : IHasTenant
{
    public Guid MappingDecisionId { get; set; }

    public Guid TenantId { get; set; }

    public Guid ImportSessionId { get; set; }

    public Guid? MappingCandidateId { get; set; }

    public Guid StagedRowId { get; set; }

    public string Decision { get; set; } = ImportMappingDecisions.Skip;

    public Guid? SelectedEvidenceOptionId { get; set; }

    public string SelectedEvidenceOptionKey { get; set; } = string.Empty;

    public string SelectedTargetKind { get; set; } = string.Empty;

    public string SelectedTargetId { get; set; } = string.Empty;

    public string SelectedTargetKey { get; set; } = string.Empty;

    public string CreateNewPayloadJson { get; set; } = "{}";

    public string EvidenceMappingPurpose { get; set; } = ImportEvidenceMappingPurposes.NormalRequirement;

    public string ExceptionExemptionKey { get; set; } = string.Empty;

    public string ResidualRequirementsJson { get; set; } = "[]";

    public bool OverrideUsed { get; set; }

    public string OverrideReason { get; set; } = string.Empty;

    public Guid DecidedByPersonId { get; set; }

    public DateTimeOffset DecidedAt { get; set; }

    public ImportSession? ImportSession { get; set; }

    public ImportStagedMappingCandidate? MappingCandidate { get; set; }
}

public static class ImportSessionImportTypes
{
    public const string ComplianceCoreCsvBundle = "compliancecore_csv_bundle";
}

public static class ImportSessionStatuses
{
    public const string Uploaded = "uploaded";
    public const string Parsed = "parsed";
    public const string ValidationFailed = "validation_failed";
    public const string ValidationPassed = "validation_passed";
    public const string MappingRequired = "mapping_required";
    public const string MappingConfirmed = "mapping_confirmed";
    public const string PartiallyConfirmed = "partially_confirmed";
    public const string Committed = "committed";
    public const string Rejected = "rejected";
    public const string Superseded = "superseded";
}

public static class ImportSessionValidationStatuses
{
    public const string NotValidated = "not_validated";
    public const string Passed = "passed";
    public const string Failed = "failed";
}

public static class ImportSessionMappingStatuses
{
    public const string NotStarted = "not_started";
    public const string Required = "required";
    public const string PartiallyConfirmed = "partially_confirmed";
    public const string Confirmed = "confirmed";
}

public static class ImportSessionCommitStatuses
{
    public const string NotCommitted = "not_committed";
    public const string Previewed = "previewed";
    public const string Committed = "committed";
    public const string Failed = "failed";
}

public static class ImportRowValidationStatuses
{
    public const string Pending = "pending";
    public const string Valid = "valid";
    public const string Invalid = "invalid";
}

public static class MappingConfidenceBands
{
    public const string Exact = "exact";
    public const string High = "high";
    public const string Medium = "medium";
    public const string Low = "low";
    public const string NoMatch = "no_match";
}

public static class MappingProposedActions
{
    public const string MapExisting = "map_existing";
    public const string CreateNew = "create_new";
    public const string ReferenceOnly = "reference_only";
    public const string NoDocumentRequired = "no_document_required";
}

public static class ImportMappingDecisions
{
    public const string ConfirmCandidate = "confirm_candidate";
    public const string SelectEvidenceOption = "select_evidence_option";
    public const string SelectExisting = "select_existing";
    public const string CreateNew = "create_new";
    public const string NoDocumentRequired = "no_document_required";
    public const string AddSupportingEvidence = "add_supporting_evidence";
    public const string Reject = "reject";
    public const string Skip = "skip";
    public const string NotApplicable = "not_applicable";
    public const string ReferenceOnly = "reference_only";
    public const string ForceMap = "force_map";
    public const string Split = "split";
    public const string Merge = "merge";
    public const string MapAsNormalEvidence = "map_as_normal_evidence";
    public const string MapAsExceptionProof = "map_as_exception_proof";
    public const string MapAsExemptionProof = "map_as_exemption_proof";
    public const string MapAsSpecialPermitApprovalProof = "map_as_special_permit_approval_proof";
    public const string CreateNewExceptionExemptionRecord = "create_new_exception_exemption_record";
    public const string SelectExistingExceptionExemptionRecord = "select_existing_exception_exemption_record";
    public const string MarkExceptionNotApplicable = "mark_exception_not_applicable";
}

public static class ImportEvidenceMappingPurposes
{
    public const string NormalRequirement = "normal_requirement";
    public const string AlternateEvidencePath = "alternate_evidence_path";
    public const string ExceptionProof = "exception_proof";
    public const string ExemptionProof = "exemption_proof";
    public const string WaiverVarianceSpecialPermitProof = "waiver_variance_special_permit_proof";
    public const string ChangesApplicability = "changes_applicability";
    public const string ChangesRequiredEvidence = "changes_required_evidence";
    public const string ChangesExpectedValue = "changes_expected_value";
}
