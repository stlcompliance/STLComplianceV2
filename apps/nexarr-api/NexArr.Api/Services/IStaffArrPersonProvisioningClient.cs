namespace NexArr.Api.Services;

public interface IStaffArrPersonProvisioningClient
{
    Task EnsurePersonAsync(
        Guid tenantId,
        Guid externalUserId,
        string email,
        string displayName,
        Guid? requestedByUserId,
        CancellationToken cancellationToken = default);
}
