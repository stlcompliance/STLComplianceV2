namespace NexArr.Api.Contracts;

public sealed record TenantMemberResponse(
    Guid MembershipId,
    Guid UserId,
    string Email,
    string DisplayName,
    string RoleKey,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record TenantMembersListResponse(
    Guid TenantId,
    IReadOnlyList<TenantMemberResponse> Members);

public sealed record AddTenantMemberRequest(
    Guid UserId,
    string RoleKey = "tenant_user");

public sealed record AddTenantMemberResponse(
    Guid MembershipId,
    Guid UserId,
    bool WasReactivated);

public sealed record RemoveTenantMemberResponse(
    Guid MembershipId,
    Guid UserId,
    bool WasAlreadyRemoved);
