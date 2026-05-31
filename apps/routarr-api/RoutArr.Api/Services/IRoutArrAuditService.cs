namespace RoutArr.Api.Services;

public sealed record RoutArrAuditWriteResult(
    Guid AuditEventId,
    DateTimeOffset OccurredAt,
    string Action,
    string Result,
    string? ReasonCode);

public interface IRoutArrAuditService
{
    Task<RoutArrAuditWriteResult> WriteAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? reasonCode = null,
        CancellationToken cancellationToken = default);
}
