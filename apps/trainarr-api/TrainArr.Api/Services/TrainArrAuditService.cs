using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Services;

public sealed class TrainArrAuditService(
    TrainArrDbContext db,
    ICorrelationIdAccessor correlationIdAccessor) : ITrainArrAuditService
{
    public async Task<TrainArrAuditWriteResult> WriteAsync(
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

        db.AuditEvents.Add(new TrainArrAuditEvent
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
        return new TrainArrAuditWriteResult(auditEventId, occurredAt);
    }
}
