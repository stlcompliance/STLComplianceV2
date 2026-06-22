namespace StaffArr.Api.Contracts;

public sealed record ProductImportManifestResponse(
    string ProductKey,
    string ImportTypeKey,
    string DisplayName,
    string Description,
    IReadOnlyList<string> SupportedFileTypes,
    string TemplateVersion,
    string RequiredPermission,
    string TargetEntity,
    IReadOnlyList<string> AllowedOperations,
    IReadOnlyList<string> RequiredColumns,
    IReadOnlyList<string> OptionalColumns,
    IReadOnlyList<string> ControlledVocabularyColumns,
    IReadOnlyList<string> ReferenceColumns,
    IReadOnlyList<string> UniquenessRules,
    IReadOnlyList<string> DuplicateDetectionRules,
    IReadOnlyList<string> ValidationRules,
    IReadOnlyList<string> PreviewColumns,
    string CommitBehavior,
    IReadOnlyList<string> EmittedEvents,
    bool RollbackSupport,
    string AuditCategory);

public sealed record ProductImportHistoryItemResponse(
    Guid ImportHistoryId,
    string ImportTypeKey,
    string DisplayName,
    string Status,
    bool DryRun,
    int RowCount,
    int SuccessCount,
    int ErrorCount,
    Guid? ActorUserId,
    string? ActorDisplayName,
    DateTimeOffset OccurredAt,
    string? Summary);

public sealed record ProductImportHistoryListResponse(
    IReadOnlyList<ProductImportHistoryItemResponse> Items);
