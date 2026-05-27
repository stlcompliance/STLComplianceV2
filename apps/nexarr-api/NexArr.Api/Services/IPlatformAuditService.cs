namespace NexArr.Api.Services;

public interface IPlatformAuditService
{
    Task WriteAsync(
        string action,
        string targetType,
        string? targetId,
        string result,
        Guid? tenantId = null,
        Guid? actorUserId = null,
        string? reasonCode = null,
        CancellationToken cancellationToken = default);
}
