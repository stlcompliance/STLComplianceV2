using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;

namespace MaintainArr.Api.Services;

public sealed class PmOccurrenceService(MaintainArrDbContext db)
{
    public async Task<PmOccurrence> EnsureDueOccurrenceAsync(
        PmSchedule schedule,
        string targetDueStatus,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        var occurrence = await GetOrCreateOccurrenceAsync(schedule, occurredAt, cancellationToken);
        if (ShouldUpdateToDueStatus(occurrence.Status, targetDueStatus))
        {
            occurrence.Status = targetDueStatus;
            occurrence.UpdatedAt = occurredAt;
            await db.SaveChangesAsync(cancellationToken);
        }

        return occurrence;
    }

    public async Task<PmOccurrence> MarkSkippedAsync(
        PmSchedule schedule,
        Guid skippedByPersonId,
        DateTimeOffset skippedAt,
        string? skippedReason,
        CancellationToken cancellationToken = default)
    {
        var occurrence = await GetOrCreateOccurrenceAsync(schedule, skippedAt, cancellationToken);
        occurrence.Status = PmOccurrenceStatuses.Skipped;
        occurrence.SkippedByPersonId = skippedByPersonId;
        occurrence.SkippedAt = skippedAt;
        occurrence.SkippedReason = skippedReason;
        occurrence.UpdatedAt = skippedAt;
        await db.SaveChangesAsync(cancellationToken);
        return occurrence;
    }

    public async Task<PmOccurrence> MarkWorkOrderGeneratedAsync(
        PmSchedule schedule,
        string generatedWorkOrderRef,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        var occurrence = await GetOrCreateOccurrenceAsync(schedule, occurredAt, cancellationToken);
        occurrence.GeneratedWorkOrderRef = generatedWorkOrderRef;
        if (!IsTerminal(occurrence.Status))
        {
            occurrence.Status = PmOccurrenceStatuses.Generated;
        }

        occurrence.UpdatedAt = occurredAt;
        await db.SaveChangesAsync(cancellationToken);
        return occurrence;
    }

    public async Task<PmOccurrence> MarkInspectionGeneratedAsync(
        PmSchedule schedule,
        string generatedInspectionRef,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        var occurrence = await GetOrCreateOccurrenceAsync(schedule, occurredAt, cancellationToken);
        occurrence.GeneratedInspectionRef = generatedInspectionRef;
        if (!IsTerminal(occurrence.Status))
        {
            occurrence.Status = PmOccurrenceStatuses.Generated;
        }

        occurrence.UpdatedAt = occurredAt;
        await db.SaveChangesAsync(cancellationToken);
        return occurrence;
    }

    public async Task<PmOccurrence> MarkCompletedAsync(
        PmSchedule schedule,
        string completedByWorkOrderRef,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken = default)
    {
        var occurrence = await GetOrCreateOccurrenceAsync(schedule, completedAt, cancellationToken);
        occurrence.Status = PmOccurrenceStatuses.Completed;
        occurrence.CompletedAt = completedAt;
        occurrence.CompletedByWorkOrderRef = completedByWorkOrderRef;
        occurrence.UpdatedAt = completedAt;
        await db.SaveChangesAsync(cancellationToken);
        return occurrence;
    }

    private async Task<PmOccurrence> GetOrCreateOccurrenceAsync(
        PmSchedule schedule,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var existing = await db.PmOccurrences.FirstOrDefaultAsync(
            x => x.TenantId == schedule.TenantId
                && x.PmScheduleId == schedule.Id
                && x.DueAt == schedule.NextDueAt,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var nextOccurrenceNumber = await db.PmOccurrences
            .AsNoTracking()
            .Where(x => x.TenantId == schedule.TenantId && x.PmScheduleId == schedule.Id)
            .Select(x => (int?)x.OccurrenceNumber)
            .MaxAsync(cancellationToken) ?? 0;

        var dueMeterType = await ResolveDueMeterTypeAsync(schedule, cancellationToken);
        var occurrence = new PmOccurrence
        {
            Id = Guid.NewGuid(),
            TenantId = schedule.TenantId,
            PmScheduleId = schedule.Id,
            AssetId = schedule.AssetId,
            OccurrenceNumber = nextOccurrenceNumber + 1,
            DueAt = schedule.NextDueAt,
            DueMeterType = dueMeterType,
            DueMeterValue = schedule.NextDueAtUsage,
            Status = PmOccurrenceStatuses.Upcoming,
            CreatedAt = occurredAt,
            UpdatedAt = occurredAt,
        };

        db.PmOccurrences.Add(occurrence);
        await db.SaveChangesAsync(cancellationToken);
        return occurrence;
    }

    private async Task<string?> ResolveDueMeterTypeAsync(PmSchedule schedule, CancellationToken cancellationToken)
    {
        if (!schedule.AssetMeterId.HasValue)
        {
            return null;
        }

        var meter = await db.AssetMeters
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == schedule.TenantId && x.Id == schedule.AssetMeterId.Value,
                cancellationToken);

        return meter?.MeterKey;
    }

    private static bool ShouldUpdateToDueStatus(string currentStatus, string targetDueStatus) =>
        !IsTerminal(currentStatus)
        && (string.Equals(targetDueStatus, PmOccurrenceStatuses.Due, StringComparison.OrdinalIgnoreCase)
            || string.Equals(targetDueStatus, PmOccurrenceStatuses.Overdue, StringComparison.OrdinalIgnoreCase));

    private static bool IsTerminal(string status) =>
        string.Equals(status, PmOccurrenceStatuses.Skipped, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, PmOccurrenceStatuses.Completed, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, PmOccurrenceStatuses.Canceled, StringComparison.OrdinalIgnoreCase);
}
