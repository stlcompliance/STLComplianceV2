namespace TrainArr.Api.Contracts;

public static class RulePackImpactTriggers
{
    public const string VersionDrift = "version_drift";
    public const string StatusChange = "status_change";
    public const string PackInactive = "pack_inactive";
    public const string PackNotFound = "pack_not_found";
    public const string ManualAssessment = "manual_assessment";
}

public static class RulePackImpactRecommendedActionTypes
{
    public const string ReviewRequirements = "review_requirements";
    public const string ReRunQualificationCheck = "re_run_qualification_check";
    public const string ReviewActiveAssignments = "review_active_assignments";
    public const string ReviewInactiveRulePack = "review_inactive_rule_pack";
    public const string AcknowledgeRulePackChange = "acknowledge_rule_pack_change";
}

public sealed record AssessRulePackImpactRequest(
    string RulePackKey,
    int? ExpectedVersionNumber = null,
    string? ExpectedStatus = null);

public sealed record RulePackImpactAssessmentResponse(
    Guid AssessmentId,
    string RulePackKey,
    DateTimeOffset AssessedAt,
    IReadOnlyList<string> Triggers,
    RulePackImpactCurrentStateResponse? CurrentState,
    RulePackImpactDriftResponse? Drift,
    IReadOnlyList<RulePackImpactAffectedDefinition> AffectedDefinitions,
    IReadOnlyList<RulePackImpactAffectedProgram> AffectedPrograms,
    IReadOnlyList<RulePackImpactAffectedAssignment> AffectedAssignments,
    IReadOnlyList<RulePackImpactAffectedQualification> AffectedQualifications,
    IReadOnlyList<RulePackImpactRecommendedAction> RecommendedActions,
    RulePackImpactSummary Summary);

public sealed record RulePackImpactCurrentStateResponse(
    string Label,
    string Description,
    string RegulatoryProgramKey,
    string RegulatoryProgramLabel,
    int VersionNumber,
    string Status,
    bool IsActive);

public sealed record RulePackImpactDriftResponse(
    bool HasVersionDrift,
    int? BaselineVersionNumber,
    int? CurrentVersionNumber,
    bool HasStatusDrift,
    string? BaselineStatus,
    string? CurrentStatus,
    bool PackInactive,
    bool PackNotFound);

public sealed record RulePackImpactAffectedDefinition(
    Guid TrainingDefinitionId,
    string DefinitionKey,
    string Name,
    string QualificationKey,
    Guid RequirementId,
    int? KnownVersionNumber,
    string? KnownStatus);

public sealed record RulePackImpactAffectedProgram(
    Guid TrainingProgramId,
    string ProgramKey,
    string Name,
    Guid RequirementId,
    int? KnownVersionNumber,
    string? KnownStatus,
    IReadOnlyList<Guid> MemberDefinitionIds);

public sealed record RulePackImpactAffectedAssignment(
    Guid AssignmentId,
    Guid StaffarrPersonId,
    Guid TrainingDefinitionId,
    string TrainingDefinitionName,
    string Status,
    string AssignmentReason,
    DateTimeOffset CreatedAt);

public sealed record RulePackImpactAffectedQualification(
    Guid QualificationIssueId,
    Guid StaffarrPersonId,
    Guid TrainingAssignmentId,
    string QualificationKey,
    string QualificationName,
    string Status,
    DateTimeOffset IssuedAt);

public sealed record RulePackImpactRecommendedAction(
    string ActionType,
    string Priority,
    string Message,
    string? EntityType = null,
    Guid? EntityId = null);

public sealed record RulePackImpactSummary(
    int RequirementCount,
    int DefinitionCount,
    int ProgramCount,
    int ActiveAssignmentCount,
    int ActiveQualificationCount,
    bool HasDrift,
    bool RequiresAttention);
