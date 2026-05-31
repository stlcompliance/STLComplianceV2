namespace TrainArr.Api.Services;

public sealed record TrainArrAuditWriteResult(
    Guid AuditEventId,
    DateTimeOffset OccurredAt);

public interface ITrainArrAuditService
{
    Task<TrainArrAuditWriteResult> WriteAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? reasonCode = null,
        CancellationToken cancellationToken = default);
}
