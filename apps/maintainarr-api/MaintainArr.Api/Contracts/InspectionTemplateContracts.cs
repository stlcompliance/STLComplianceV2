namespace MaintainArr.Api.Contracts;



public sealed record InspectionTemplateSummaryResponse(

    Guid InspectionTemplateId,

    string TemplateKey,

    string Name,

    string Description,

    string InspectionType,

    int Version,

    string Status,

    int CategoryCount,

    int ChecklistItemCount,

    int LinkedAssetTypeCount,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt);



public sealed record InspectionTemplateCategoryResponse(

    Guid CategoryId,

    string CategoryKey,

    string Name,

    int SortOrder,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt);



public sealed record InspectionChecklistItemResponse(

    Guid ChecklistItemId,

    Guid? CategoryId,

    string? CategoryKey,

    string ItemKey,

    string Prompt,

    string ItemType,

    IReadOnlyList<string> ControlledOptions,

    decimal? AcceptableRangeMin,

    decimal? AcceptableRangeMax,

    string? UnitOfMeasure,

    bool IsRequired,

    int SortOrder,

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

    string InspectionType,

    int Version,

    string Status,

    IReadOnlyList<InspectionTemplateCategoryResponse> Categories,

    IReadOnlyList<InspectionChecklistItemResponse> ChecklistItems,

    IReadOnlyList<InspectionTemplateAssetTypeLinkResponse> LinkedAssetTypes,

    DateTimeOffset CreatedAt,

    DateTimeOffset UpdatedAt);



public sealed record CreateInspectionTemplateRequest(

    string TemplateKey,

    string Name,

    string Description,
    string? InspectionType = null);



public sealed record UpdateInspectionTemplateRequest(

    string Name,

    string Description,
    string? InspectionType = null);



public sealed record UpdateInspectionTemplateStatusRequest(string Status);



public sealed record CreateInspectionTemplateCategoryRequest(

    string CategoryKey,

    string Name,

    int SortOrder);



public sealed record UpdateInspectionTemplateCategoryRequest(

    string Name,

    int SortOrder);



public sealed record CreateInspectionChecklistItemRequest(

    string ItemKey,

    string Prompt,

    string ItemType,

    bool IsRequired,

    int SortOrder,

    Guid? CategoryId,

    IReadOnlyList<string>? ControlledOptions = null,

    decimal? AcceptableRangeMin = null,

    decimal? AcceptableRangeMax = null,

    string? UnitOfMeasure = null);



public sealed record UpdateInspectionChecklistItemRequest(

    string Prompt,

    string ItemType,

    bool IsRequired,

    int SortOrder,

    Guid? CategoryId,

    IReadOnlyList<string>? ControlledOptions = null,

    decimal? AcceptableRangeMin = null,

    decimal? AcceptableRangeMax = null,

    string? UnitOfMeasure = null);



public sealed record ReplaceInspectionTemplateAssetTypesRequest(

    IReadOnlyList<Guid> AssetTypeIds);


