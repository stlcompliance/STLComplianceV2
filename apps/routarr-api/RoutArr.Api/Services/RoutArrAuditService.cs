using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Services;

public sealed class RoutArrAuditService(
    RoutArrDbContext db,
    ICorrelationIdAccessor correlationIdAccessor) : IRoutArrAuditService
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
        db.AuditEvents.Add(new RoutArrAuditEvent
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
