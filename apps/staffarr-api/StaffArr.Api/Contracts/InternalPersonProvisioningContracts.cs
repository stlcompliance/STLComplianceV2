namespace StaffArr.Api.Contracts;

public sealed record ProvisionStaffArrPersonRequest(
    Guid TenantId,
    Guid ExternalUserId,
    string Email,
    string DisplayName,
    Guid? RequestedByUserId = null);

public sealed record ProvisionStaffArrPersonResponse(
    Guid PersonId,
    Guid TenantId,
    Guid ExternalUserId,
    bool WasCreated,
    bool WasUpdated);
