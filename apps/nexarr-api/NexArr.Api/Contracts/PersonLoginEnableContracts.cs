namespace NexArr.Api.Contracts;

public sealed record PersonLoginEnableRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    Guid ExternalUserId,
    string? Reason,
    Guid? RequestedByUserId = null);

public sealed record PersonLoginEnableResponse(
    Guid ExternalUserId,
    bool WasAlreadyEnabled);
