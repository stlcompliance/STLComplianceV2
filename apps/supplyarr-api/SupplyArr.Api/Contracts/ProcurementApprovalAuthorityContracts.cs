namespace SupplyArr.Api.Contracts;

public sealed record ProcurementApprovalAuthorityGrantMirror(
    string PermissionKey,
    string PermissionName,
    string ScopeType,
    string? ScopeValue,
    string RoleKey,
    string RoleName);

public sealed record ProcurementApprovalAuthorityMirrorResponse(
    Guid StaffarrPersonId,
    Guid ExternalUserId,
    bool CanSubmitPurchaseRequests,
    bool CanApprovePurchaseRequests,
    bool CanIssuePurchaseOrders,
    decimal? MaxSubmitAmount,
    decimal? MaxApproveAmount,
    decimal? MaxIssueAmount,
    IReadOnlyList<Guid> OrgUnitScopeIds,
    IReadOnlyList<ProcurementApprovalAuthorityGrantMirror> Grants,
    DateTimeOffset SourceComputedAt,
    DateTimeOffset RefreshedAt,
    string AuthoritySource);

public sealed record ProcurementApprovalAuthorityCheckResponse(
    bool Allowed,
    string AuthoritySource,
    string? DenialReason,
    ProcurementApprovalAuthorityMirrorResponse? Authority);
