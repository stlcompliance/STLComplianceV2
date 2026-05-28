using Microsoft.EntityFrameworkCore;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;

namespace TrainArr.Api.Services;

public sealed class AssignmentDueReminderSettingsService(
    TrainArrDbContext db,
    ITrainArrAuditService audit)
{
    public async Task<AssignmentDueReminderSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantAssignmentDueReminderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (settings is null)
        {
            return new AssignmentDueReminderSettingsResponse(
                IsEnabled: false,
                DueSoonLeadDays: AssignmentDueReminderRules.NormalizeDueSoonLeadDays(null),
                ReminderCooldownHours: AssignmentDueReminderRules.NormalizeCooldownHours(null),
                MaxRemindersPerAssignment: AssignmentDueReminderRules.NormalizeMaxReminders(null),
                UpdatedAt: null);
        }

        return MapResponse(settings);
    }

    public async Task<AssignmentDueReminderSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertAssignmentDueReminderSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantAssignmentDueReminderSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantAssignmentDueReminderSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantAssignmentDueReminderSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.DueSoonLeadDays = AssignmentDueReminderRules.NormalizeDueSoonLeadDays(request.DueSoonLeadDays);
        entity.ReminderCooldownHours = AssignmentDueReminderRules.NormalizeCooldownHours(request.ReminderCooldownHours);
        entity.MaxRemindersPerAssignment = AssignmentDueReminderRules.NormalizeMaxReminders(request.MaxRemindersPerAssignment);
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "trainarr.assignment_due_reminder_settings.update",
            tenantId,
            actorUserId,
            "tenant_assignment_due_reminder_settings",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantAssignmentDueReminderSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantAssignmentDueReminderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantAssignmentDueReminderSettingsSnapshot ToSnapshot(
        TenantAssignmentDueReminderSettings settings) =>
        new(
            settings.IsEnabled,
            settings.DueSoonLeadDays,
            settings.ReminderCooldownHours,
            settings.MaxRemindersPerAssignment);

    private static AssignmentDueReminderSettingsResponse MapResponse(
        TenantAssignmentDueReminderSettings settings) =>
        new(
            settings.IsEnabled,
            settings.DueSoonLeadDays,
            settings.ReminderCooldownHours,
            settings.MaxRemindersPerAssignment,
            settings.UpdatedAt);
}
