using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Services;

public sealed class SupplyArrAuditService(
    SupplyArrDbContext db,
    ICorrelationIdAccessor correlationIdAccessor) : ISupplyArrAuditService
{
    public async Task<SupplyArrAuditWriteResult> WriteAsync(
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
        var occurredAt = DateTimeOffset.UtcNow;

        db.AuditEvents.Add(new SupplyArrAuditEvent
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
            OccurredAt = occurredAt
        });

        await db.SaveChangesAsync(cancellationToken);
        return new SupplyArrAuditWriteResult(auditEventId, occurredAt);
    }
}
