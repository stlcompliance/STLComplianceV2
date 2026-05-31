namespace StaffArr.Api.Services;

public sealed record StaffArrAuditWriteResult(
    Guid AuditEventId,
    DateTimeOffset OccurredAt);

public interface IStaffArrAuditService
{
    Task<StaffArrAuditWriteResult> WriteAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? reasonCode = null,
        CancellationToken cancellationToken = default);
}
