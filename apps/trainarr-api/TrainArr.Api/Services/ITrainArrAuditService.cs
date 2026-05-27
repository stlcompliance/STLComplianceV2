namespace TrainArr.Api.Services;

public interface ITrainArrAuditService
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
