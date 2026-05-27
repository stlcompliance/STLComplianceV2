using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace SupplyArr.Api.Services;

public sealed class SupplyArrAuditService(
    SupplyArrDbContext db,
    ICorrelationIdAccessor correlationIdAccessor) : ISupplyArrAuditService
{
    public async Task WriteAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? reasonCode = null,
        CancellationToken cancellationToken = default)
    {
        db.AuditEvents.Add(new SupplyArrAuditEvent
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
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
