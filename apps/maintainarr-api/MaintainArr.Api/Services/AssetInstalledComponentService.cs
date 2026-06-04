using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AssetInstalledComponentService(MaintainArrDbContext db)
{
    public async Task<IReadOnlyList<AssetInstalledComponentResponse>> ListAsync(
        Guid tenantId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var assetExists = await db.Assets.AnyAsync(
            x => x.TenantId == tenantId && x.Id == assetId,
            cancellationToken);
        if (!assetExists)
        {
            throw new StlApiException("assets.not_found", "Asset was not found.", 404);
        }

        var components = await db.AssetInstalledComponents
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ParentAssetId == assetId)
            .OrderBy(x => x.ParentComponentId)
            .ThenBy(x => x.ComponentType)
            .ThenBy(x => x.ComponentNumber)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return components.Select(MapResponse).ToList();
    }

    private static AssetInstalledComponentResponse MapResponse(AssetInstalledComponent entity) =>
        new(
            entity.Id,
            entity.ComponentNumber,
            entity.ParentAssetId,
            entity.ParentComponentId,
            entity.Name,
            entity.Description,
            entity.ComponentType,
            entity.Status,
            entity.Make,
            entity.Model,
            entity.SerialNumber,
            entity.PartNumberSnapshot,
            entity.InstalledPartUsageRef,
            entity.InstallDate,
            entity.InstalledByPersonId,
            entity.InstalledMeterReading,
            entity.RemovedDate,
            entity.RemovedByPersonId,
            entity.RemovedMeterReading,
            entity.RemovalReason,
            entity.WarrantyStartDate,
            entity.WarrantyEndDate,
            entity.ExpectedLifeHours,
            entity.ExpectedLifeMiles,
            entity.ExpectedLifeCycles,
            entity.Condition,
            DeserializeList(entity.ReplacementPartRefsJson),
            DeserializeList(entity.DocumentRefsJson),
            DeserializeList(entity.DefectRefsJson),
            DeserializeList(entity.WorkOrderRefsJson),
            entity.CreatedAt,
            entity.UpdatedAt);

    private static IReadOnlyList<string> DeserializeList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
