using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class FieldInboxService(MaintainArrDbContext db)
{
    public async Task<FieldInboxResponse> GetAsync(
        Guid tenantId,
        bool viewAll,
        Guid actorUserId,
        string? actorPersonId,
        CancellationToken cancellationToken = default)
    {
        var items = new List<FieldInboxTaskItem>();

        var workOrders = await ListAssignedWorkOrdersAsync(
            tenantId,
            viewAll,
            actorUserId,
            actorPersonId,
            cancellationToken);
        items.AddRange(workOrders);

        var inspections = await ListAssignedInspectionsAsync(
            tenantId,
            viewAll,
            actorUserId,
            cancellationToken);
        items.AddRange(inspections);

        return FieldInboxRules.BuildProductResponse(items);
    }

    private async Task<IReadOnlyList<FieldInboxTaskItem>> ListAssignedWorkOrdersAsync(
        Guid tenantId,
        bool viewAll,
        Guid actorUserId,
        string? actorPersonId,
        CancellationToken cancellationToken)
    {
        var query = db.WorkOrders
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && WorkOrderStatuses.Active.Contains(x.Status));

        if (!viewAll)
        {
            var personId = actorPersonId?.Trim();
            query = query.Where(x =>
                x.CreatedByUserId == actorUserId
                || (personId != null
                    && x.AssignedTechnicianPersonId != null
                    && x.AssignedTechnicianPersonId == personId));
        }

        var workOrders = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (workOrders.Count == 0)
        {
            return [];
        }

        var assetIds = workOrders.Select(x => x.AssetId).Distinct().ToList();
        var assets = await db.Assets
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && assetIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return workOrders.Select(wo =>
        {
            assets.TryGetValue(wo.AssetId, out var asset);
            var subtitle = asset is null ? null : $"{asset.AssetTag} · {asset.Name}";
            return new FieldInboxTaskItem(
                $"maintainarr:work-order:{wo.Id:D}",
                "maintainarr",
                "work_order",
                wo.Title,
                subtitle,
                wo.Status,
                wo.Priority,
                wo.StartedAt ?? wo.CreatedAt,
                wo.UpdatedAt,
                $"/work-orders/{wo.Id:D}");
        }).ToList();
    }

    private async Task<IReadOnlyList<FieldInboxTaskItem>> ListAssignedInspectionsAsync(
        Guid tenantId,
        bool viewAll,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var query = db.InspectionRuns
            .AsNoTracking()
            .Include(x => x.InspectionTemplate)
            .Include(x => x.Asset)
            .Where(x => x.TenantId == tenantId && x.Status == InspectionRunStatuses.InProgress);

        if (!viewAll)
        {
            query = query.Where(x => x.StartedByUserId == actorUserId);
        }

        var runs = await query
            .OrderByDescending(x => x.StartedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        return runs.Select(run =>
        {
            var templateName = run.InspectionTemplate?.Name ?? "Inspection";
            var assetLabel = run.Asset is null
                ? null
                : $"{run.Asset.AssetTag} · {run.Asset.Name}";
            return new FieldInboxTaskItem(
                $"maintainarr:inspection:{run.Id:D}",
                "maintainarr",
                "inspection",
                templateName,
                assetLabel,
                run.Status,
                null,
                null,
                run.StartedAt,
                $"/inspections/{run.Id:D}");
        }).ToList();
    }
}
