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
    Guid? InspectionTemplateId = null);

public sealed record UpdatePmProgramRequest(
    string Name,
    string Description,
    string Status,
    bool AutoGenerateWorkOrder = true,
    string? DefaultWorkOrderTemplateRef = null,
    bool AutoGenerateInspection = false,
    Guid? InspectionTemplateId = null);

public sealed record UpdatePmProgramStatusRequest(string Status);

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
    DateTimeOffset UpdatedAt);

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
    DateTimeOffset UpdatedAt);
