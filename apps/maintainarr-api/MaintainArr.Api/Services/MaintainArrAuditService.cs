using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Services;

public sealed class MaintainArrAuditService(
    MaintainArrDbContext db,
    ICorrelationIdAccessor correlationIdAccessor) : IMaintainArrAuditService
{
    public async Task<Guid> WriteAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? reasonCode = null,
        CancellationToken cancellationToken = default)
    {
        var auditEventId = Guid.NewGuid();
        db.AuditEvents.Add(new MaintainArrAuditEvent
        {
            Id = auditEventId,
            TenantId = tenantId,
            ActorUserId = actorUserId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Result = result,
            ReasonCode = reasonCode,
            CorrelationId = correlationIdAccessor.CorrelationId,
            OccurredAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        return auditEventId;
    }
}
