using Microsoft.EntityFrameworkCore;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using STLCompliance.Shared.Operations.LoadTesting;

namespace SupplyArr.Api.Services;

public sealed class LoadTestJourneySeedService(
    SupplyArrDbContext db,
    ISupplyArrAuditService auditService)
{
    public async Task<LoadTestJourneySeedResponse> EnsureSeededAsync(
        Guid tenantId,
        Guid? actorUserId,
        CancellationToken cancellationToken = default)
    {
        var title = StlSupplyArrLoadTestJourneySeedCatalog.JourneyDemandRefTitle;
        var workOrderNumber = StlSupplyArrLoadTestJourneySeedCatalog.JourneyWorkOrderNumber;
        var now = DateTimeOffset.UtcNow;

        var existing = await db.MaintainArrDemandRefs
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.Title == title,
                cancellationToken);

        var demandRefCreated = false;
        Guid demandRefId;

        if (existing is not null)
        {
            demandRefId = existing.Id;
        }
        else
        {
            demandRefId = await CreateMaintainarrDemandRefAsync(tenantId, title, workOrderNumber, now, cancellationToken);
            demandRefCreated = true;

            await auditService.WriteAsync(
                "load_test_journey.seed",
                tenantId,
                actorUserId,
                "maintainarr_demand_ref",
                demandRefId.ToString(),
                MaintainArrDemandRefStatuses.Received,
                cancellationToken: cancellationToken);
        }

        var settingsEnsured = await EnsureDemandProcessingSettingsAsync(tenantId, now, cancellationToken);

        return new LoadTestJourneySeedResponse(
            demandRefId,
            DemandRefSources.MaintainArr,
            workOrderNumber,
            title,
            demandRefCreated,
            settingsEnsured);
    }

    private async Task<Guid> CreateMaintainarrDemandRefAsync(
        Guid tenantId,
        string title,
        string workOrderNumber,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var partKey = StlSupplyArrLoadTestJourneySeedCatalog.JourneyPartKey;
        var part = await db.Parts
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PartKey == partKey,
                cancellationToken);

        if (part is null)
        {
            part = new Part
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PartKey = partKey,
                DisplayName = "Load test journey demand part",
                Description = "Idempotent load-test journey part for demand processing smokes.",
                CategoryKey = "general",
                UnitOfMeasure = "each",
                ManufacturerName = string.Empty,
                ManufacturerPartNumber = string.Empty,
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.Parts.Add(part);
        }

        var stock = await db.PartStockLevels
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.PartId == part.Id,
                cancellationToken);

        if (stock is null)
        {
            var location = new InventoryLocation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                LocationKey = "ltj-dp-wh",
                Name = "Load test journey warehouse",
                LocationType = "warehouse",
                AddressLine = string.Empty,
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now,
            };

            var bin = new InventoryBin
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InventoryLocationId = location.Id,
                BinKey = "ltj-dp-a1",
                Name = "A1",
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now,
            };

            stock = new PartStockLevel
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PartId = part.Id,
                InventoryBinId = bin.Id,
                QuantityOnHand = 1m,
                QuantityReserved = 0m,
                CreatedAt = now,
                UpdatedAt = now,
            };

            db.InventoryLocations.Add(location);
            db.InventoryBins.Add(bin);
            db.PartStockLevels.Add(stock);
        }
        else if (stock.QuantityOnHand > 2m)
        {
            stock.QuantityOnHand = 1m;
            stock.UpdatedAt = now;
        }

        var demandRefId = Guid.NewGuid();
        var demandRef = new MaintainArrDemandRef
        {
            Id = demandRefId,
            TenantId = tenantId,
            MaintainarrPublicationId = Guid.NewGuid(),
            MaintainarrWorkOrderId = Guid.NewGuid(),
            MaintainarrWorkOrderNumber = workOrderNumber,
            MaintainarrAssetId = Guid.NewGuid(),
            Title = title,
            Notes = "Idempotent load-test journey demand ref for Playwright and k6 smokes.",
            Status = MaintainArrDemandRefStatuses.Received,
            ProcurementStatus = MaintainArrDemandRefProcurementStatuses.Received,
            ReceivedAt = now.AddHours(-2),
            CreatedAt = now.AddHours(-2),
            UpdatedAt = now.AddHours(-2),
        };

        demandRef.Lines.Add(new MaintainArrDemandRefLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DemandRefId = demandRefId,
            LineNumber = 1,
            MaintainarrDemandLineId = Guid.NewGuid(),
            PartId = part.Id,
            PartNumber = part.PartKey,
            Description = part.DisplayName,
            QuantityRequested = 5m,
            UnitOfMeasure = "each",
            Notes = string.Empty,
        });

        db.MaintainArrDemandRefs.Add(demandRef);
        await db.SaveChangesAsync(cancellationToken);

        return demandRefId;
    }

    private async Task<bool> EnsureDemandProcessingSettingsAsync(
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var entity = await db.TenantDemandProcessingSettings
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        var created = false;
        if (entity is null)
        {
            entity = new TenantDemandProcessingSettings
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CreatedAt = now,
            };
            db.TenantDemandProcessingSettings.Add(entity);
            created = true;
        }

        var changed = created
            || !entity.IsEnabled
            || entity.AutoCreatePrDraftWhenShort
            || !entity.ProcessMaintainarrDemandRefs
            || entity.MinHoursBeforeProcessing != 0;

        entity.IsEnabled = true;
        entity.AutoCreatePrDraftWhenShort = false;
        entity.MinHoursBeforeProcessing = 0;
        entity.StalenessHours = 4;
        entity.NotifyOnPrDraftCreated = false;
        entity.ProcessMaintainarrDemandRefs = true;
        entity.ProcessRoutarrDemandRefs = false;
        entity.ProcessTrainarrDemandRefs = false;
        entity.ProcessStaffarrDemandRefs = false;
        entity.UpdatedAt = now;

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return changed || created;
    }
}
