namespace MaintainArr.Api.Contracts;

public sealed record CreatePmProgramRequest(
    string ProgramKey,
    string Name,
    string Description,
    string ScopeType,
    Guid? AssetTypeId,
    Guid? AssetId,
    IReadOnlyList<Guid>? PmScheduleIds = null,
    bool AutoGenerateWorkOrder = true,
    string? DefaultWorkOrderTemplateRef = null,
    bool AutoGenerateInspection = false,
    Guid? InspectionTemplateId = null,
    string? CategoryKey = null,
    string? WorkTypeKey = null,
    string? PriorityKey = null,
    string? OwningSiteRef = null,
    string? OwningTeamRef = null,
    string? OwningDepartmentRef = null,
    string? OwnerPersonId = null,
    string? OwnerRoleKey = null,
    IReadOnlyList<string>? Tags = null,
    PmProgramScopeDefinitionRequest? ScopeDefinition = null,
    PmProgramDueDefinitionRequest? DueDefinition = null,
    PmProgramWorkPackageDefinitionRequest? WorkPackageDefinition = null,
    PmProgramInspectionDefinitionRequest? InspectionDefinition = null,
    PmProgramComplianceDefinitionRequest? ComplianceDefinition = null,
    PmProgramAutomationDefinitionRequest? AutomationDefinition = null);

public sealed record UpdatePmProgramRequest(
    string Name,
    string Description,
    string Status,
    bool AutoGenerateWorkOrder = true,
    string? DefaultWorkOrderTemplateRef = null,
    bool AutoGenerateInspection = false,
    Guid? InspectionTemplateId = null,
    string? CategoryKey = null,
    string? WorkTypeKey = null,
    string? PriorityKey = null,
    string? OwningSiteRef = null,
    string? OwningTeamRef = null,
    string? OwningDepartmentRef = null,
    string? OwnerPersonId = null,
    string? OwnerRoleKey = null,
    IReadOnlyList<string>? Tags = null,
    PmProgramScopeDefinitionRequest? ScopeDefinition = null,
    PmProgramDueDefinitionRequest? DueDefinition = null,
    PmProgramWorkPackageDefinitionRequest? WorkPackageDefinition = null,
    PmProgramInspectionDefinitionRequest? InspectionDefinition = null,
    PmProgramComplianceDefinitionRequest? ComplianceDefinition = null,
    PmProgramAutomationDefinitionRequest? AutomationDefinition = null);

public sealed record UpdatePmProgramStatusRequest(string Status);

public sealed record ActivatePmProgramRequest(
    bool ConfirmReadinessImpact = false,
    bool ConfirmComplianceImpact = false,
    bool ConfirmZeroMatch = false);

public sealed record ReplacePmProgramSchedulesRequest(IReadOnlyList<Guid> PmScheduleIds);

public sealed record PmProgramSummaryResponse(
    Guid PmProgramId,
    string ProgramKey,
    string Name,
    string ScopeType,
    Guid? AssetTypeId,
    string? AssetTypeName,
    Guid? AssetId,
    string? AssetTag,
    string Status,
    bool AutoGenerateWorkOrder,
    string? DefaultWorkOrderTemplateRef,
    bool AutoGenerateInspection,
    Guid? InspectionTemplateId,
    string? InspectionTemplateKey,
    string? InspectionTemplateName,
    int ScheduleCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? CategoryKey = null,
    string? WorkTypeKey = null,
    string? PriorityKey = null,
    string? OwningSiteRef = null,
    string? OwningTeamRef = null,
    string? OwningDepartmentRef = null,
    string? OwnerPersonId = null,
    string? OwnerRoleKey = null,
    IReadOnlyList<string>? Tags = null,
    DateTimeOffset? ActivatedAt = null,
    DateTimeOffset? PausedAt = null,
    DateTimeOffset? RetiredAt = null,
    int? MatchedAssetCount = null,
    string? ScopeSummary = null,
    string? DueSummary = null,
    string? WorkPackageSummary = null,
    string? InspectionSummary = null,
    string? ComplianceSummary = null,
    string? AutomationSummary = null);

public sealed record PmProgramScheduleLinkResponse(
    Guid PmScheduleId,
    string ScheduleKey,
    string Name,
    string AssetTag,
    string AssetName,
    string DueStatus,
    string Status,
    int SortOrder);

public sealed record PmProgramDetailResponse(
    Guid PmProgramId,
    string ProgramKey,
    string Name,
    string Description,
    string ScopeType,
    Guid? AssetTypeId,
    string? AssetTypeKey,
    string? AssetTypeName,
    Guid? AssetId,
    string? AssetTag,
    string? AssetName,
    string Status,
    bool AutoGenerateWorkOrder,
    string? DefaultWorkOrderTemplateRef,
    bool AutoGenerateInspection,
    Guid? InspectionTemplateId,
    string? InspectionTemplateKey,
    string? InspectionTemplateName,
    IReadOnlyList<PmProgramScheduleLinkResponse> Schedules,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? CategoryKey = null,
    string? WorkTypeKey = null,
    string? PriorityKey = null,
    string? OwningSiteRef = null,
    string? OwningTeamRef = null,
    string? OwningDepartmentRef = null,
    string? OwnerPersonId = null,
    string? OwnerRoleKey = null,
    IReadOnlyList<string>? Tags = null,
    DateTimeOffset? ActivatedAt = null,
    string? ActivatedByPersonId = null,
    DateTimeOffset? PausedAt = null,
    string? PausedByPersonId = null,
    DateTimeOffset? RetiredAt = null,
    string? RetiredByPersonId = null,
    int? MatchedAssetCount = null,
    string? ScopeSummary = null,
    string? DueSummary = null,
    string? WorkPackageSummary = null,
    string? InspectionSummary = null,
    string? ComplianceSummary = null,
    string? AutomationSummary = null);

public sealed record PmProgramScopeDefinitionRequest(
    IReadOnlyList<string>? AssetClassKeys = null,
    IReadOnlyList<Guid>? AssetTypeIds = null,
    IReadOnlyList<string>? AssetCategoryKeys = null,
    IReadOnlyList<string>? AssetStatusKeys = null,
    IReadOnlyList<string>? ReadinessStateKeys = null,
    IReadOnlyList<string>? SiteRefs = null,
    IReadOnlyList<string>? DepartmentRefs = null,
    IReadOnlyList<string>? LocationRefs = null,
    IReadOnlyList<string>? MakeKeys = null,
    IReadOnlyList<string>? ModelKeys = null,
    int? YearFrom = null,
    int? YearTo = null,
    IReadOnlyList<string>? FuelTypeKeys = null,
    IReadOnlyList<string>? Tags = null,
    IReadOnlyList<Guid>? IncludedAssetIds = null,
    IReadOnlyList<Guid>? ExcludedAssetIds = null);

public sealed record PmProgramCalendarTriggerRequest(
    int IntervalValue,
    string IntervalUnit,
    DateOnly? AnchorDate = null,
    DateOnly? FirstDueDate = null,
    string CalendarBehavior = "fixed",
    int EarlyWindowDays = 0,
    int GracePeriodDays = 0,
    string PastDueBehavior = "warn");

public sealed record PmProgramMeterTriggerRequest(
    decimal IntervalValue,
    string IntervalUnit,
    decimal? AnchorReading = null,
    decimal? FirstDueReading = null,
    string CurrentReadingSource = "asset_meter",
    decimal? EarlyThreshold = null,
    decimal? GraceThreshold = null,
    bool RollingFromCompletion = false,
    string MissingDataBehavior = "warn");

public sealed record PmProgramOneTimeTriggerRequest(DateOnly DueDate);

public sealed record PmProgramDueTriggerRequest(
    string TriggerType,
    PmProgramCalendarTriggerRequest? Calendar = null,
    PmProgramMeterTriggerRequest? Meter = null,
    PmProgramOneTimeTriggerRequest? OneTime = null,
    bool? ManualOnly = null);

public sealed record PmProgramDueDefinitionRequest(
    string MatchLogic,
    IReadOnlyList<PmProgramDueTriggerRequest> Triggers,
    bool WarnWhenAnyApproaching = false,
    bool MarkDueBasedOnMostUrgent = false);

public sealed record PmProgramWorkPackagePartDemandRequest(
    string ItemRef,
    string Description,
    decimal Quantity,
    string UnitOfMeasure);

public sealed record PmProgramChecklistTaskRequest(
    string TaskKey,
    string Title,
    string? Description = null,
    int SortOrder = 0);

public sealed record PmProgramWorkPackageDefinitionRequest(
    bool GenerateWorkOrder,
    string? WorkOrderTitleTemplate = null,
    string? WorkOrderDescription = null,
    string? DefaultPriority = null,
    string? DefaultWorkType = null,
    decimal? EstimatedLaborHours = null,
    IReadOnlyList<string>? RequiredSkills = null,
    IReadOnlyList<string>? SafetyNotes = null,
    IReadOnlyList<string>? TechnicianNotes = null,
    IReadOnlyList<string>? RequiredAttachments = null,
    IReadOnlyList<PmProgramWorkPackagePartDemandRequest>? PartsDemand = null,
    IReadOnlyList<PmProgramChecklistTaskRequest>? ChecklistTasks = null);

public sealed record PmProgramInspectionDefinitionRequest(
    bool AttachInspectionTemplate,
    Guid? InspectionTemplateId = null,
    bool InspectionRequiredBeforeWorkOrderCompletion = false,
    string InspectionResultBehavior = "pass_completes",
    bool ResumeBehaviorRespectsEngineRules = true,
    bool VoiceCompatible = false);

public sealed record PmProgramComplianceDefinitionRequest(
    bool IsComplianceRelated,
    string? GoverningBodyCatalogKey = null,
    IReadOnlyList<string>? CitationReferences = null,
    string ReadinessImpact = "no_impact",
    IReadOnlyList<string>? CertificateRequirements = null);

public sealed record PmProgramAutomationDefinitionRequest(
    int LeadTimeDays = 0,
    decimal? LeadThresholdValue = null,
    string? LeadThresholdUnit = null,
    int DuplicatePreventionWindowDays = 1,
    string AssignmentBehavior = "unassigned",
    string? AssignmentRef = null,
    IReadOnlyList<string>? NotificationTargets = null,
    IReadOnlyList<string>? EscalationTargets = null,
    IReadOnlyList<string>? BlackoutWindows = null,
    int? MaxOpenGeneratedItemsPerAsset = null);

public sealed record PmProgramPreviewAssetResponse(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string AssetTypeName,
    string SiteName,
    string LifecycleStatus,
    string? ReadinessStatus,
    string? DueStatus,
    DateTimeOffset? LastPmAt,
    string? LastWorkOrderNumber);

public sealed record PmProgramScopePreviewResponse(
    int MatchedAssetCount,
    int ExcludedAssetCount,
    IReadOnlyList<PmProgramPreviewAssetResponse> SampleAssets,
    IReadOnlyList<string> Warnings,
    bool CanActivate);

public sealed record PmProgramDuePreviewItemResponse(
    Guid AssetId,
    string AssetTag,
    string AssetName,
    string TriggerSummary,
    string? EstimatedNextDueDate,
    string? EstimatedNextDueReading,
    string DueState);

public sealed record PmProgramDuePreviewResponse(
    string DueLogic,
    IReadOnlyList<PmProgramDuePreviewItemResponse> Items,
    IReadOnlyList<string> Warnings,
    bool RequiresExplicitConfirmation);
