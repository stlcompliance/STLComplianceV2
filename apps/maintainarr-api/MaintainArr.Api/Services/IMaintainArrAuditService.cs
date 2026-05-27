namespace MaintainArr.Api.Services;

public interface IMaintainArrAuditService
{
    Task WriteAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? reasonCode = null,
        CancellationToken cancellationToken = default);
}
