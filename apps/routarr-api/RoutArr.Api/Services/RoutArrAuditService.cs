using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Services;

public sealed class RoutArrAuditService(
    RoutArrDbContext db,
    ICorrelationIdAccessor correlationIdAccessor) : IRoutArrAuditService
{
    public async Task<RoutArrAuditWriteResult> WriteAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? reasonCode = null,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = new RoutArrAuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ActorUserId = actorUserId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Result = result,
            ReasonCode = reasonCode,
            CorrelationId = correlationIdAccessor.CorrelationId,
            OccurredAt = DateTimeOffset.UtcNow
        };

        db.AuditEvents.Add(auditEvent);

        await db.SaveChangesAsync(cancellationToken);

        return new RoutArrAuditWriteResult(
            auditEvent.Id,
            auditEvent.OccurredAt,
            auditEvent.Action,
            auditEvent.Result,
            auditEvent.ReasonCode);
    }
}
