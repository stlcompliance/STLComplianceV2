using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class AssignmentEscalationSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<AssignmentEscalationSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantAssignmentEscalationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            return new AssignmentEscalationSettingsResponse(
                IsEnabled: false,
                OverdueEscalationAfterHours: AssignmentEscalationRules.NormalizeOverdueHours(null),
                EscalationCooldownHours: AssignmentEscalationRules.NormalizeCooldownHours(null),
                MaxEscalationsPerAssignment: AssignmentEscalationRules.NormalizeMaxEscalations(null),
                UpdatedAt: null);
        }

        return MapResponse(settings);
    }

    public async Task<AssignmentEscalationSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertAssignmentEscalationSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantAssignmentEscalationSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantAssignmentEscalationSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantAssignmentEscalationSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.OverdueEscalationAfterHours = AssignmentEscalationRules.NormalizeOverdueHours(request.OverdueEscalationAfterHours);
        entity.EscalationCooldownHours = AssignmentEscalationRules.NormalizeCooldownHours(request.EscalationCooldownHours);
        entity.MaxEscalationsPerAssignment = AssignmentEscalationRules.NormalizeMaxEscalations(request.MaxEscalationsPerAssignment);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "trainarr.assignment_escalation_settings.update",
            tenantId,
            actorUserId,
            "tenant_assignment_escalation_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantAssignmentEscalationSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantAssignmentEscalationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantAssignmentEscalationSettingsSnapshot ToSnapshot(
        TenantAssignmentEscalationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.OverdueEscalationAfterHours,
            settings.EscalationCooldownHours,
            settings.MaxEscalationsPerAssignment);

    private static AssignmentEscalationSettingsResponse MapResponse(
        TenantAssignmentEscalationSettings settings) =>
        new(
            settings.IsEnabled,
            settings.OverdueEscalationAfterHours,
            settings.EscalationCooldownHours,
            settings.MaxEscalationsPerAssignment,
            settings.UpdatedAt);
}
