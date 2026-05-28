using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ApprovalReminderSettingsService(
    SupplyArrDbContext db,
    ISupplyArrAuditService audit)
{
    public async Task<ApprovalReminderSettingsResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantApprovalReminderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? DefaultResponse() : MapResponse(settings);
    }

    public async Task<ApprovalReminderSettingsResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertApprovalReminderSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.TenantApprovalReminderSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantApprovalReminderSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantApprovalReminderSettings.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.PrReminderAfterHours = ApprovalReminderRules.NormalizeThresholdHours(
            request.PrReminderAfterHours,
            ApprovalReminderDefaults.PrReminderAfterHours);
        entity.PoReminderAfterHours = ApprovalReminderRules.NormalizeThresholdHours(
            request.PoReminderAfterHours,
            ApprovalReminderDefaults.PoReminderAfterHours);
        entity.ReminderCooldownHours = ApprovalReminderRules.NormalizeCooldownHours(request.ReminderCooldownHours);
        entity.MaxRemindersPerSubject = ApprovalReminderRules.NormalizeMaxReminders(request.MaxRemindersPerSubject);
        entity.NotifyOnPrApprovalReminder = request.NotifyOnPrApprovalReminder;
        entity.NotifyOnPoApprovalReminder = request.NotifyOnPoApprovalReminder;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "supplyarr.approval_reminder_settings.update",
            tenantId,
            actorUserId,
            "tenant_approval_reminder_settings",
            entity.Id.ToString(),
            "Succeeded",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    internal async Task<TenantApprovalReminderSettingsSnapshot?> LoadSnapshotAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.TenantApprovalReminderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToSnapshot(settings);
    }

    internal static TenantApprovalReminderSettingsSnapshot ToSnapshot(TenantApprovalReminderSettings settings) =>
        new(
            settings.IsEnabled,
            settings.PrReminderAfterHours,
            settings.PoReminderAfterHours,
            settings.ReminderCooldownHours,
            settings.MaxRemindersPerSubject,
            settings.NotifyOnPrApprovalReminder,
            settings.NotifyOnPoApprovalReminder);

    private static ApprovalReminderSettingsResponse DefaultResponse() =>
        new(
            IsEnabled: false,
            PrReminderAfterHours: ApprovalReminderDefaults.PrReminderAfterHours,
            PoReminderAfterHours: ApprovalReminderDefaults.PoReminderAfterHours,
            ReminderCooldownHours: ApprovalReminderDefaults.ReminderCooldownHours,
            MaxRemindersPerSubject: ApprovalReminderDefaults.MaxRemindersPerSubject,
            NotifyOnPrApprovalReminder: true,
            NotifyOnPoApprovalReminder: true,
            UpdatedAt: null);

    private static ApprovalReminderSettingsResponse MapResponse(TenantApprovalReminderSettings settings) =>
        new(
            settings.IsEnabled,
            settings.PrReminderAfterHours,
            settings.PoReminderAfterHours,
            settings.ReminderCooldownHours,
            settings.MaxRemindersPerSubject,
            settings.NotifyOnPrApprovalReminder,
            settings.NotifyOnPoApprovalReminder,
            settings.UpdatedAt);
}
