namespace NexArr.Api.Contracts;

public sealed record PersonLoginDisableRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    Guid ExternalUserId,
    string Reason,
    Guid? RequestedByUserId = null);

public sealed record PersonLoginDisableResponse(
    Guid ExternalUserId,
    bool WasAlreadyDisabled,
    int SessionsRevokedCount);
