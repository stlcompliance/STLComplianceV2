using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Services;

public sealed class StaffArrAuditService(
    StaffArrDbContext db,
    ICorrelationIdAccessor correlationIdAccessor) : IStaffArrAuditService
{
    public async Task<StaffArrAuditWriteResult> WriteAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? reasonCode = null,
        CancellationToken cancellationToken = default) =>
        await WriteCoreAsync(
            action,
            tenantId,
            actorUserId,
            targetType,
            targetId,
            result,
            reasonCode,
            metadataJson: null,
            cancellationToken);

    public async Task<StaffArrAuditWriteResult> WriteWithMetadataAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? metadataJson,
        string? reasonCode = null,
        CancellationToken cancellationToken = default) =>
        await WriteCoreAsync(
            action,
            tenantId,
            actorUserId,
            targetType,
            targetId,
            result,
            reasonCode,
            metadataJson,
            cancellationToken);

    private async Task<StaffArrAuditWriteResult> WriteCoreAsync(
        string action,
        Guid tenantId,
        Guid? actorUserId,
        string targetType,
        string? targetId,
        string result,
        string? reasonCode,
        string? metadataJson,
        CancellationToken cancellationToken)
    {
        var auditEventId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;

        db.AuditEvents.Add(new StaffArrAuditEvent
        {
            Id = auditEventId,
            TenantId = tenantId,
            ActorUserId = actorUserId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Result = result,
            ReasonCode = reasonCode,
            MetadataJson = metadataJson,
            CorrelationId = correlationIdAccessor.CorrelationId,
            OccurredAt = occurredAt
        });

        await db.SaveChangesAsync(cancellationToken);
        return new StaffArrAuditWriteResult(auditEventId, occurredAt);
    }
}
