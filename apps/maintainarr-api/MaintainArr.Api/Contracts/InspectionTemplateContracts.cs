namespace MaintainArr.Api.Contracts;

public sealed record InspectionTemplateSummaryResponse(
    Guid InspectionTemplateId,
    string TemplateKey,
    string Name,
    string Description,
    string TemplateCategoryKey,
    string OwningSiteRef,
    string OwningTeamRef,
    string OwnerPersonId,
    string InspectionType,
    int Version,
    string Status,
    int CategoryCount,
    int ChecklistItemCount,
    int LinkedAssetTypeCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? RetiredAt);

public sealed record InspectionTemplateCategoryResponse(
    Guid CategoryId,
    string CategoryKey,
    string Name,
    string Description,
    bool IsRequired,
    bool CanBeSkipped,
    bool SkipReasonRequired,
    bool TimingTracked,
    int SortOrder,
    IReadOnlyDictionary<string, object?> Settings,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record InspectionChecklistItemResponse(
    Guid ChecklistItemId,
    Guid? CategoryId,
    string? CategoryKey,
    string ItemKey,
    string Prompt,
    string HelpText,
    string ItemType,
    IReadOnlyList<string> ControlledOptions,
    decimal? AcceptableRangeMin,
    decimal? AcceptableRangeMax,
    string? UnitOfMeasure,
    bool IsRequired,
    int SortOrder,
    IReadOnlyDictionary<string, object?> Settings,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record InspectionTemplateAssetTypeLinkResponse(
    Guid AssetTypeId,
    string TypeKey,
    string TypeName,
    string ClassKey,
    string ClassName);

public sealed record InspectionTemplateDetailResponse(
    Guid InspectionTemplateId,
    string TemplateKey,
    string Name,
    string Description,
    string TemplateCategoryKey,
    string? OwningSiteRef,
    string? OwningTeamRef,
    string? OwnerPersonId,
    string? OwnerRoleKey,
    int? EstimatedDurationMinutes,
    IReadOnlyList<string> Tags,
    IReadOnlyDictionary<string, object?> Settings,
    string InspectionType,
    int Version,
    string Status,
    IReadOnlyList<InspectionTemplateCategoryResponse> Categories,
    IReadOnlyList<InspectionChecklistItemResponse> ChecklistItems,
    IReadOnlyList<InspectionTemplateAssetTypeLinkResponse> LinkedAssetTypes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? RetiredAt,
    string? CreatedByPersonId,
    string? UpdatedByPersonId,
    string? PublishedByPersonId,
    string? RetiredByPersonId);

public sealed record CreateInspectionTemplateRequest(
    string TemplateKey,
    string Name,
    string Description,
    string? InspectionType = null,
    string? TemplateCategoryKey = null,
    string? OwningSiteRef = null,
    string? OwningTeamRef = null,
    string? OwnerPersonId = null,
    string? OwnerRoleKey = null,
    int? EstimatedDurationMinutes = null,
    IReadOnlyList<string>? Tags = null,
    IReadOnlyDictionary<string, object?>? Settings = null);

public sealed record UpdateInspectionTemplateRequest(
    string Name,
    string Description,
    string? InspectionType = null,
    string? TemplateCategoryKey = null,
    string? OwningSiteRef = null,
    string? OwningTeamRef = null,
    string? OwnerPersonId = null,
    string? OwnerRoleKey = null,
    int? EstimatedDurationMinutes = null,
    IReadOnlyList<string>? Tags = null,
    IReadOnlyDictionary<string, object?>? Settings = null);

public sealed record UpdateInspectionTemplateStatusRequest(string Status);

public sealed record PublishInspectionTemplateRequest(
    bool ConfirmComplianceRelated = false,
    bool ConfirmReadinessImpact = false,
    bool ConfirmFailureAutomation = false,
    bool ConfirmSupervisorRelease = false);

public sealed record RetireInspectionTemplateRequest(string? Reason = null);

public sealed record InspectionTemplateValidationIssueResponse(
    string Code,
    string Message,
    string Section,
    bool IsBlocking);

public sealed record InspectionTemplateValidationResponse(
    bool IsValid,
    IReadOnlyList<InspectionTemplateValidationIssueResponse> Issues,
    int SectionCount,
    int ChecklistItemCount,
    int CompatibleAssetCount);

public sealed record CompatibleAssetPreviewResponse(
    int CompatibleCount,
    IReadOnlyList<AssetSearchResponse> SampleAssets,
    IReadOnlyList<AssetSearchResponse> ExcludedAssets);

public sealed record InspectionTemplatePreviewResponse(
    InspectionTemplateDetailResponse Template,
    InspectionTemplateValidationResponse Validation,
    CompatibleAssetPreviewResponse Assets,
    string Summary);

public sealed record CreateInspectionTemplateCategoryRequest(
    string CategoryKey,
    string Name,
    string? Description = null,
    bool IsRequired = false,
    bool CanBeSkipped = false,
    bool SkipReasonRequired = false,
    bool TimingTracked = false,
    int SortOrder = 0,
    IReadOnlyDictionary<string, object?>? Settings = null);

public sealed record UpdateInspectionTemplateCategoryRequest(
    string Name,
    string? Description = null,
    bool IsRequired = false,
    bool CanBeSkipped = false,
    bool SkipReasonRequired = false,
    bool TimingTracked = false,
    int SortOrder = 0,
    IReadOnlyDictionary<string, object?>? Settings = null);

public sealed record CreateInspectionChecklistItemRequest(
    string ItemKey,
    string Prompt,
    string? HelpText,
    string ItemType,
    bool IsRequired,
    int SortOrder,
    Guid? CategoryId,
    IReadOnlyList<string>? ControlledOptions = null,
    decimal? AcceptableRangeMin = null,
    decimal? AcceptableRangeMax = null,
    string? UnitOfMeasure = null,
    IReadOnlyDictionary<string, object?>? Settings = null);

public sealed record UpdateInspectionChecklistItemRequest(
    string Prompt,
    string? HelpText,
    string ItemType,
    bool IsRequired,
    int SortOrder,
    Guid? CategoryId,
    IReadOnlyList<string>? ControlledOptions = null,
    decimal? AcceptableRangeMin = null,
    decimal? AcceptableRangeMax = null,
    string? UnitOfMeasure = null,
    IReadOnlyDictionary<string, object?>? Settings = null);

public sealed record ReplaceInspectionTemplateAssetTypesRequest(
    IReadOnlyList<Guid> AssetTypeIds);
