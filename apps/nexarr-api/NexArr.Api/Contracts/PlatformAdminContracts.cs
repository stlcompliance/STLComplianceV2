namespace NexArr.Api.Contracts;

public sealed record PlatformAdminDashboardResponse(
    int TenantCount,
    int ActiveTenantCount,
    int ProductCount,
    int ActiveProductCount,
    int ActiveEntitlementCount,
    int TotalEntitlementCount,
    int ServiceClientCount,
    int ActiveServiceTokenCount,
    int LaunchProfileCount,
    int PendingHandoffCount,
    int ExpiredUnredeemedHandoffCount,
    int AuditEventsLast24Hours,
    DateTimeOffset GeneratedAt);

public sealed record LaunchDiagnosticIssueResponse(
    string IssueCode,
    string Severity,
    string Message,
    Guid? TenantId,
    string? TenantSlug,
    string? ProductKey);

public sealed record LaunchDiagnosticRowResponse(
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    string TenantStatus,
    string ProductKey,
    string ProductDisplayName,
    bool HasActiveEntitlement,
    bool HasLaunchProfile,
    bool LaunchProfileActive,
    int CallbackAllowlistEntryCount,
    int PendingHandoffCount,
    int ExpiredHandoffCount,
    string LaunchReadiness);

public sealed record LaunchDiagnosticsResponse(
    IReadOnlyList<LaunchDiagnosticRowResponse> Rows,
    IReadOnlyList<LaunchDiagnosticIssueResponse> Issues,
    DateTimeOffset GeneratedAt);

public sealed record LaunchAttemptTimelineItemResponse(
    Guid AuditEventId,
    Guid? TenantId,
    string? TenantSlug,
    string? TenantDisplayName,
    Guid? ActorUserId,
    string? ActorEmail,
    string? ActorDisplayName,
    string? ProductKey,
    string? ProductDisplayName,
    string Action,
    string Result,
    string? ReasonCode,
    string TargetType,
    string? TargetId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    string? RemediationHint);

public sealed record TenantOverviewRowResponse(
    Guid TenantId,
    string Slug,
    string DisplayName,
    string Status,
    int ActiveEntitlementCount,
    int MembershipCount,
    DateTimeOffset CreatedAt);

public sealed record ProductOverviewRowResponse(
    string ProductKey,
    string DisplayName,
    bool IsActive,
    int ActiveEntitlementCount,
    bool HasLaunchProfile,
    bool LaunchProfileActive,
    string? BaseUrl);

public sealed record PlatformUserAccessHistoryItemResponse(
    Guid AuditEventId,
    Guid UserId,
    string? UserEmail,
    string? UserDisplayName,
    Guid? TenantId,
    string? TenantSlug,
    string Action,
    string Result,
    string? ReasonCode,
    string TargetType,
    string? TargetId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt,
    string? ProductKey,
    string? ProductDisplayName);

public sealed record PlatformUserIdentityAuditHistoryItemResponse(
    Guid AuditEventId,
    Guid UserId,
    string? UserEmail,
    string? UserDisplayName,
    Guid? TenantId,
    string? TenantSlug,
    Guid? ActorUserId,
    string? ActorEmail,
    string? ActorDisplayName,
    string Action,
    string Result,
    string? ReasonCode,
    string TargetType,
    string? TargetId,
    Guid CorrelationId,
    DateTimeOffset OccurredAt);
