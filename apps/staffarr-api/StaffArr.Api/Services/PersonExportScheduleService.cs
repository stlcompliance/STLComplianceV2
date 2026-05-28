using Microsoft.EntityFrameworkCore;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace StaffArr.Api.Services;

public sealed class PersonExportScheduleService(
    StaffArrDbContext db,
    IStaffArrAuditService audit,
    IHostEnvironment hostEnvironment)
{
    public async Task<PersonExportScheduleResponse> GetAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await db.TenantPersonExportSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (schedule is null)
        {
            return new PersonExportScheduleResponse(
                IsEnabled: false,
                IntervalHours: PersonExportDeliveryRules.NormalizeIntervalHours(null),
                LastDeliveredAt: null,
                UpdatedAt: null,
                NotificationWebhookUrl: null,
                NotifyOnSuccess: true,
                NotifyOnFailure: true);
        }

        return MapResponse(schedule);
    }

    public async Task<PersonExportScheduleResponse> UpsertAsync(
        Guid tenantId,
        Guid actorUserId,
        UpsertPersonExportScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var intervalHours = PersonExportDeliveryRules.NormalizeIntervalHours(request.IntervalHours);
        var entity = await db.TenantPersonExportSchedules
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        if (entity is null)
        {
            entity = new TenantPersonExportSchedule
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantPersonExportSchedules.Add(entity);
        }

        entity.IsEnabled = request.IsEnabled;
        entity.IntervalHours = intervalHours;
        var allowInsecureHttp = hostEnvironment.IsDevelopment() || hostEnvironment.IsEnvironment("Testing");
        entity.NotificationWebhookUrl = PersonExportDeliveryNotificationRules.NormalizeWebhookUrl(
            request.NotificationWebhookUrl,
            allowInsecureHttp);
        entity.NotifyOnSuccess = request.NotifyOnSuccess;
        entity.NotifyOnFailure = request.NotifyOnFailure;
        entity.UpdatedByUserId = actorUserId;
        entity.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "person.export_schedule.update",
            tenantId,
            actorUserId,
            "tenant_person_export_schedule",
            entity.Id.ToString(),
            "success",
            cancellationToken: cancellationToken);

        return MapResponse(entity);
    }

    private static PersonExportScheduleResponse MapResponse(TenantPersonExportSchedule schedule) =>
        new(
            schedule.IsEnabled,
            schedule.IntervalHours,
            schedule.LastDeliveredAt,
            schedule.UpdatedAt,
            schedule.NotificationWebhookUrl,
            schedule.NotifyOnSuccess,
            schedule.NotifyOnFailure);
}
