using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Services;

public sealed class StaffArrAuditService(
    StaffArrDbContext db,
    ICorrelationIdAccessor correlationIdAccessor) : IStaffArrAuditService
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
        db.AuditEvents.Add(new StaffArrAuditEvent
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
