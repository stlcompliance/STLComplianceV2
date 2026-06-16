namespace STLCompliance.Shared.Scheduling;

public static class StlSchedulingActions
{
    public const string View = "view";
    public const string Validate = "validate";
    public const string Schedule = "schedule";
    public const string Reschedule = "reschedule";
    public const string Unschedule = "unschedule";
    public const string Cancel = "cancel";
    public const string Complete = "complete";
    public const string Override = "override";
    public const string ManageBoard = "manageBoard";
}

public static class StlSchedulingValidationStatuses
{
    public const string Allowed = "allowed";
    public const string Warning = "warning";
    public const string Blocked = "blocked";
    public const string NeedsReview = "needs_review";
    public const string MissingFacts = "missing_facts";
    public const string MissingPermissions = "missing_permissions";
}

public static class StlSchedulingConflictTypes
{
    public const string Resource = "resource_conflict";
    public const string Qualification = "qualification_conflict";
    public const string Compliance = "compliance_conflict";
    public const string AssetReadiness = "asset_readiness_conflict";
    public const string Location = "location_conflict";
    public const string OrderStatus = "order_status_conflict";
    public const string DocumentEvidence = "document_evidence_conflict";
    public const string MissingFacts = "missing_facts";
    public const string Permission = "missing_permissions";
}

public sealed record StlSchedulingWindow(
    DateTimeOffset? Start,
    DateTimeOffset? End,
    string? Timezone);

public sealed record StlSchedulingResourceAssignment(
    string ResourceType,
    string ResourceId,
    string? SourceProductKey,
    string? DisplayName,
    string? Role = null);

public sealed record StlSchedulingLocationAssignment(
    string? SiteId,
    string? LocationId,
    string? SourceProductKey,
    string? DisplayName,
    string? Status = null);

public sealed record StlSchedulingSourceReference(
    string ProductKey,
    string ObjectType,
    string ObjectId,
    string? ObjectNumber = null);

public sealed record StlSchedulingConflict(
    string ConflictType,
    string Code,
    string Severity,
    string Message,
    string? SourceProductKey = null,
    string? SourceObjectType = null,
    string? SourceObjectId = null,
    bool OverrideAllowed = false);

public sealed record StlSchedulingDisplayItem(
    string ProductKey,
    string ItemType,
    string ItemId,
    string Title,
    string? Subtitle,
    string CurrentStatus,
    string ScheduleStatus,
    string Priority,
    StlSchedulingWindow? RequestedWindow,
    StlSchedulingWindow? PromisedWindow,
    StlSchedulingWindow? ScheduledWindow,
    string? CustomerReference,
    string? OrderReference,
    string? SiteId,
    string? LocationId,
    IReadOnlyList<StlSchedulingResourceAssignment> ResourceNeeds,
    IReadOnlyList<StlSchedulingResourceAssignment> AssignedResources,
    IReadOnlyList<StlSchedulingConflict> Blockers,
    IReadOnlyList<StlSchedulingConflict> Warnings,
    IReadOnlyList<StlSchedulingSourceReference> SourceRefs,
    string OwningProductUrl,
    IReadOnlyList<string> AllowedActions,
    IReadOnlyDictionary<string, bool> PermissionFlags,
    string Freshness);

public sealed record StlSchedulingResourceLane(
    string ProductKey,
    string ResourceType,
    string ResourceId,
    string DisplayName,
    string? Subtitle,
    string Status,
    string? SiteId,
    string? LocationId);

public sealed record StlSchedulingBoardResponse(
    Guid TenantId,
    string ProductKey,
    DateTimeOffset GeneratedAt,
    string Freshness,
    IReadOnlyList<StlSchedulingDisplayItem> Items,
    IReadOnlyList<StlSchedulingResourceLane> Resources);

public sealed record StlSchedulingRequest(
    Guid TenantId,
    string ProductKey,
    string ItemType,
    string ItemId,
    DateTimeOffset? RequestedStart,
    DateTimeOffset? RequestedEnd,
    string? Timezone,
    IReadOnlyList<StlSchedulingResourceAssignment> ResourceAssignments,
    IReadOnlyList<StlSchedulingLocationAssignment> LocationAssignments,
    IReadOnlyList<StlSchedulingResourceAssignment> AssetAssignments,
    string? Reason,
    Guid CorrelationId,
    string IdempotencyKey,
    IReadOnlyList<StlSchedulingSourceReference> SourceContext,
    StlSchedulingOverrideRequest? Override,
    bool ValidationOnly);

public sealed record StlSchedulingOverrideRequest(
    bool Requested,
    string? Reason,
    IReadOnlyList<string> ConflictCodes);

public sealed record StlSchedulingValidationResponse(
    string Status,
    bool Allowed,
    IReadOnlyList<StlSchedulingConflict> Blockers,
    IReadOnlyList<StlSchedulingConflict> Warnings,
    IReadOnlyList<string> MissingPermissions,
    Guid CorrelationId);

public sealed record StlSchedulingMutationResponse(
    string Status,
    StlSchedulingDisplayItem Item,
    StlSchedulingValidationResponse Validation,
    Guid? EventId);

public sealed record StlSchedulingAdapterDescriptor(
    string ProductKey,
    string DisplayName,
    IReadOnlyList<string> SupportedItemTypes,
    bool SupportsSchedule,
    bool SupportsReschedule,
    bool SupportsUnschedule,
    bool SupportsCancel,
    bool SupportsComplete,
    bool SupportsResize,
    bool SupportsBulkSchedule,
    string UnscheduledPath,
    string ScheduledPath,
    string ResourcesPath,
    string ValidatePath,
    string SchedulePath,
    string ReschedulePath,
    string UnschedulePath,
    string CancelPath,
    string? CompletePath);

public static class StlSchedulingPermissionKeys
{
    public static string View(string productKey) => $"{productKey}.scheduling.view";
    public static string Schedule(string productKey) => $"{productKey}.scheduling.schedule";
    public static string Reschedule(string productKey) => $"{productKey}.scheduling.reschedule";
    public static string Unschedule(string productKey) => $"{productKey}.scheduling.unschedule";
    public static string Override(string productKey) => $"{productKey}.scheduling.override";
    public static string Cancel(string productKey) => $"{productKey}.scheduling.cancel";
    public static string ManageBoard(string productKey) => $"{productKey}.scheduling.manageBoard";
}
