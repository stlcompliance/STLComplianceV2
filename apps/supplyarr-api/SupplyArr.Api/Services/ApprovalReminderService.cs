using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;

namespace SupplyArr.Api.Services;

public sealed class ApprovalReminderService(
    SupplyArrDbContext db,
    ApprovalReminderSettingsService settingsService)
{
    public async Task<ApprovalRemindersDashboardResponse> GetDashboardAsync(
        Guid tenantId,
        bool includeUpcoming,
        CancellationToken cancellationToken = default)
    {
        var asOf = DateTimeOffset.UtcNow;
        var settings = await settingsService.LoadSnapshotAsync(tenantId, cancellationToken);

        var submittedPrStatus = PurchaseRequestStatuses.Submitted;
        var draftPoStatus = PurchaseOrderStatuses.Draft;

        var purchaseRequests = await db.PurchaseRequests.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == submittedPrStatus)
            .Select(x => new ApprovalReminderSubjectRow(
                ApprovalReminderSubjectTypes.PurchaseRequest,
                x.Id,
                x.RequestKey,
                x.Title,
                x.Status,
                x.VendorPartyId,
                x.SubmittedAt ?? x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var purchaseOrders = await db.PurchaseOrders.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Status == draftPoStatus)
            .Select(x => new ApprovalReminderSubjectRow(
                ApprovalReminderSubjectTypes.PurchaseOrder,
                x.Id,
                x.OrderKey,
                x.Title,
                x.Status,
                x.VendorPartyId,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var subjects = purchaseRequests.Concat(purchaseOrders).ToList();
        var stateLookup = await db.ApprovalReminderStates.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ToDictionaryAsync(
                x => (x.SubjectType, x.SubjectId),
                x => x,
                cancellationToken);

        var items = new List<ApprovalReminderSummaryResponse>();
        var overdueCount = 0;
        var pendingCount = 0;

        foreach (var subject in subjects)
        {
            stateLookup.TryGetValue((subject.SubjectType, subject.SubjectId), out var state);
            var reminderCount = state?.ReminderCount ?? 0;
            var lastReminderSentAt = state?.LastReminderSentAt;
            var pendingSince = subject.PendingSince;

            var thresholdHours = settings is null
                ? ApprovalReminderDefaults.PrReminderAfterHours
                : ApprovalReminderRules.GetThresholdHours(settings, subject.SubjectType);
            var cooldownHours = settings?.ReminderCooldownHours ?? ApprovalReminderDefaults.ReminderCooldownHours;
            var maxReminders = settings?.MaxRemindersPerSubject ?? ApprovalReminderDefaults.MaxRemindersPerSubject;

            var isOverdue = ApprovalReminderRules.IsOverdue(pendingSince, thresholdHours, asOf);
            var isDue = ApprovalReminderRules.IsDueForReminder(
                pendingSince,
                lastReminderSentAt,
                thresholdHours,
                cooldownHours,
                reminderCount,
                maxReminders,
                asOf);

            if (isOverdue)
            {
                overdueCount++;
            }

            if (isDue)
            {
                pendingCount++;
            }

            if (!includeUpcoming && !isOverdue && !isDue)
            {
                continue;
            }

            var hoursPending = ApprovalReminderRules.ComputeHoursPending(pendingSince, asOf);
            items.Add(new ApprovalReminderSummaryResponse(
                state?.Id ?? Guid.Empty,
                subject.SubjectType,
                subject.SubjectId,
                subject.DocumentKey,
                subject.Title,
                subject.DocumentStatus,
                subject.VendorPartyId,
                pendingSince,
                lastReminderSentAt,
                reminderCount,
                hoursPending,
                isOverdue));
        }

        return new ApprovalRemindersDashboardResponse(
            overdueCount,
            pendingCount,
            items.OrderByDescending(x => x.HoursPending).ToList());
    }

    private sealed record ApprovalReminderSubjectRow(
        string SubjectType,
        Guid SubjectId,
        string DocumentKey,
        string Title,
        string DocumentStatus,
        Guid? VendorPartyId,
        DateTimeOffset PendingSince);
}
