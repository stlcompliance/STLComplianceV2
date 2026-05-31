namespace NexArr.Api.Contracts;

public sealed record PersonLoginEnableRequest(
    Guid TenantId,
    Guid StaffarrPersonId,
    Guid ExternalUserId,
    string? Reason);

public sealed record PersonLoginEnableResponse(
    Guid ExternalUserId,
    bool WasAlreadyEnabled);
