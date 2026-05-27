using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MeterPmForecastService(MaintainArrDbContext db, IMaintainArrAuditService audit)
{
    public async Task ApplyForecastAfterReadingAsync(
        Guid tenantId,
        Guid actorUserId,
        Guid assetMeterId,
        decimal currentReading,
        CancellationToken cancellationToken = default)
    {
        var schedules = await db.PmSchedules
            .Where(x =>
                x.TenantId == tenantId
                && x.AssetMeterId == assetMeterId
                && x.ScheduleMode == PmScheduleModes.Meter
                && x.Status == "active")
            .ToListAsync(cancellationToken);

        foreach (var schedule in schedules)
        {
            if (!MeterPmForecastRules.ShouldMarkDueFromUsage(
                    schedule.Status,
                    schedule.DueStatus,
                    currentReading,
                    schedule.NextDueAtUsage))
            {
                continue;
            }

            schedule.DueStatus = PmDueStatuses.Due;
            schedule.UpdatedAt = DateTimeOffset.UtcNow;
            await audit.WriteAsync(
                "pm_schedule.meter_forecast.due",
                tenantId,
                actorUserId,
                "pm_schedule",
                schedule.Id.ToString(),
                "Succeeded",
                cancellationToken: cancellationToken);
        }

        if (schedules.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<MeterPmForecastResponse> GetForecastAsync(
        Guid tenantId,
        Guid assetMeterId,
        CancellationToken cancellationToken = default)
    {
        var meter = await db.AssetMeters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assetMeterId, cancellationToken);
        if (meter is null)
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "meter.not_found",
                "Asset meter was not found.",
                404);
        }

        var schedules = await db.PmSchedules.AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.AssetMeterId == assetMeterId
                && x.ScheduleMode == PmScheduleModes.Meter)
            .OrderBy(x => x.ScheduleKey)
            .ToListAsync(cancellationToken);

        var items = schedules.Select(schedule => new MeterPmForecastItem(
            schedule.Id,
            schedule.ScheduleKey,
            schedule.Name,
            schedule.ScheduleMode,
            schedule.DueStatus,
            schedule.NextDueAtUsage,
            schedule.IntervalUsage,
            meter.CurrentReading,
            MeterPmForecastRules.ComputeUsageUntilDue(meter.CurrentReading, schedule.NextDueAtUsage),
            MeterPmForecastRules.ShouldMarkDueFromUsage(
                schedule.Status,
                schedule.DueStatus,
                meter.CurrentReading,
                schedule.NextDueAtUsage)))
            .ToList();

        return new MeterPmForecastResponse(
            meter.Id,
            meter.MeterKey,
            meter.Unit,
            meter.CurrentReading,
            items);
    }
}
