namespace SupplyArr.Api.Services;

public sealed record SupplyArrAuditWriteResult(
    Guid AuditEventId,
    DateTimeOffset OccurredAt);

public interface ISupplyArrAuditService
{
    Task<SupplyArrAuditWriteResult> WriteAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? reasonCode = null,
        CancellationToken cancellationToken = default);
}
