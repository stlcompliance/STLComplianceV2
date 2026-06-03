using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class MeterPmForecastService(MaintainArrDbContext db, IMaintainArrAuditService audit)
{
    private const int VelocitySampleLimit = 12;
    private const decimal DueSoonWindowDays = 30m;

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

        var now = DateTimeOffset.UtcNow;

        var schedules = await db.PmSchedules.AsNoTracking()
            .Where(x =>
                x.TenantId == tenantId
                && x.AssetMeterId == assetMeterId
                && x.ScheduleMode == PmScheduleModes.Meter)
            .OrderBy(x => x.ScheduleKey)
            .ToListAsync(cancellationToken);

        var readings = await db.MeterReadings.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.AssetMeterId == assetMeterId && !x.IsCorrection)
            .OrderByDescending(x => x.ReadAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(VelocitySampleLimit)
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

        var velocitySample = BuildVelocitySample(meter, readings);
        var nextDueItem = items
            .Where(item => item.UsageUntilDue.HasValue)
            .OrderBy(item => item.UsageUntilDue)
            .ThenBy(item => item.NextDueAtUsage ?? decimal.MaxValue)
            .FirstOrDefault();

        var usageVelocityPerDay = velocitySample?.UsageVelocityPerDay;
        var predictedUsageUntilDue = nextDueItem?.UsageUntilDue;
        var predictedDaysUntilDue = usageVelocityPerDay.HasValue && usageVelocityPerDay.Value > 0 && predictedUsageUntilDue.HasValue
            ? (decimal?)decimal.Round(predictedUsageUntilDue.Value / usageVelocityPerDay.Value, 1)
            : null;
        var predictedDueAt = predictedDaysUntilDue.HasValue
            ? (DateTimeOffset?)now.AddDays((double)predictedDaysUntilDue.Value)
            : null;
        var confidenceScore = BuildConfidenceScore(readings.Count, velocitySample, nextDueItem);
        var isDueSoon = nextDueItem is not null && (
            nextDueItem.IsDueFromUsage
            || nextDueItem.DueStatus.Equals(PmDueStatuses.Due, StringComparison.OrdinalIgnoreCase)
            || (predictedDaysUntilDue.HasValue && predictedDaysUntilDue.Value <= DueSoonWindowDays));

        return new MeterPmForecastResponse(
            meter.Id,
            meter.MeterKey,
            meter.Unit,
            meter.CurrentReading,
            usageVelocityPerDay,
            predictedUsageUntilDue,
            predictedDaysUntilDue,
            predictedDueAt,
            confidenceScore,
            isDueSoon,
            items);
    }

    private static MeterVelocitySample? BuildVelocitySample(
        AssetMeter meter,
        IReadOnlyList<MeterReading> readings)
    {
        if (readings.Count > 0)
        {
            var latest = readings[0];
            var oldest = readings[^1];
            var spanDays = (latest.ReadAt - oldest.ReadAt).TotalDays;
            if (spanDays > 0.25d)
            {
                var delta = latest.ReadingValue - oldest.ReadingValue;
                var velocity = delta / (decimal)spanDays;
                if (velocity > 0)
                {
                    return new MeterVelocitySample(velocity, readings.Count, spanDays);
                }
            }
        }

        if (meter.LastReadingAt.HasValue)
        {
            var spanDays = (meter.LastReadingAt.Value - meter.CreatedAt).TotalDays;
            if (spanDays > 0.25d)
            {
                var delta = meter.CurrentReading - meter.BaselineReading;
                var velocity = delta / (decimal)spanDays;
                if (velocity > 0)
                {
                    return new MeterVelocitySample(velocity, readings.Count + 1, spanDays);
                }
            }
        }

        return null;
    }

    private static decimal BuildConfidenceScore(
        int readingCount,
        MeterVelocitySample? velocitySample,
        MeterPmForecastItem? nextDueItem)
    {
        decimal score = 18m;

        if (readingCount >= 1)
        {
            score += 12m;
        }

        if (readingCount >= 2)
        {
            score += 20m;
        }

        if (readingCount >= 5)
        {
            score += 12m;
        }

        if (velocitySample is not null)
        {
            score += 22m;
            if (velocitySample.SpanDays >= 7)
            {
                score += 8m;
            }

            if (velocitySample.ReadingCount >= 5)
            {
                score += 8m;
            }
        }

        if (nextDueItem is not null)
        {
            score += 10m;
            if (nextDueItem.IsDueFromUsage)
            {
                score += 5m;
            }
        }

        return Math.Min(100m, score);
    }

    private sealed record MeterVelocitySample(
        decimal UsageVelocityPerDay,
        int ReadingCount,
        double SpanDays);
}
