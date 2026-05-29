using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ProcurementExceptionEscalationSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<ProcurementExceptionEscalationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantProcurementExceptionEscalationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<ProcurementExceptionEscalationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertProcurementExceptionEscalationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantProcurementExceptionEscalationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantProcurementExceptionEscalationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantProcurementExceptionEscalationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.EscalationCooldownHours = ProcurementExceptionEscalationRules.NormalizeCooldownHours(
            request.EscalationCooldownHours);
        entity.MaxEscalationsPerException = ProcurementExceptionEscalationRules.NormalizeMaxEscalations(
            request.MaxEscalationsPerException);
        entity.NotifyOnProcurementExceptionSlaEscalation = request.NotifyOnProcurementExceptionSlaEscalation;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.procurement_exception_escalation_settings.update",
            tenantId,
            actorUserId,
            "tenant_procurement_exception_escalation_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantProcurementExceptionEscalationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantProcurementExceptionEscalationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantProcurementExceptionEscalationSettingsSnapshot ToSnapshot(
        TenantProcurementExceptionEscalationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.EscalationCooldownHours,
            settings.MaxEscalationsPerException,
            settings.NotifyOnProcurementExceptionSlaEscalation);

    private static ProcurementExceptionEscalationSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            EscalationCooldownHours: ProcurementExceptionEscalationDefaults.EscalationCooldownHours,
            MaxEscalationsPerException: ProcurementExceptionEscalationDefaults.MaxEscalationsPerException,
            NotifyOnProcurementExceptionSlaEscalation: true,
            UpdatedAt: null);

    private static ProcurementExceptionEscalationSettingsResponse MapResponse(
        TenantProcurementExceptionEscalationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.EscalationCooldownHours,
            settings.MaxEscalationsPerException,
            settings.NotifyOnProcurementExceptionSlaEscalation,
            settings.UpdatedAt);
}
