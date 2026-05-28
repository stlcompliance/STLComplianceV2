namespace StaffArr.Api.Contracts;

public sealed record ProcurementApprovalAuthorityGrantResponse(
    string PermissionKey,
    string PermissionName,
    string ScopeType,
    string? ScopeValue,
    string RoleKey,
    string RoleName);

public sealed record ProcurementApprovalAuthorityResponse(
    Guid PersonId,
    Guid? ExternalUserId,
    DateTimeOffset ComputedAt,
    bool CanSubmitPurchaseRequests,
    bool CanApprovePurchaseRequests,
    bool CanIssuePurchaseOrders,
    decimal? MaxSubmitAmount,
    decimal? MaxApproveAmount,
    decimal? MaxIssueAmount,
    IReadOnlyList<Guid> OrgUnitScopeIds,
    IReadOnlyList<ProcurementApprovalAuthorityGrantResponse> Grants);
