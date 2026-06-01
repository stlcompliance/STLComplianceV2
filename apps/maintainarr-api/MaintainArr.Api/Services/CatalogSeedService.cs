using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaintainArr.Api.Services;

public sealed class CatalogSeedService(MaintainArrDbContext db)
{
    public async Task SeedDefaultsAsync(CancellationToken cancellationToken = default)
    {
        var tenantIds = await db.AssetClasses.Select(x => x.TenantId).Distinct().ToListAsync(cancellationToken);
        foreach (var tenantId in tenantIds)
        {
            await EnsureCatalogAsync(tenantId, "assetClass", "Asset Class", ["vehicle", "trailer", "powered_industrial_truck", "heavy_equipment", "production_equipment", "facility_equipment", "tool", "component", "attachment"], cancellationToken);
            await EnsureCatalogAsync(tenantId, "assetType", "Asset Type", ["pickup", "service_truck", "cargo_van", "box_truck", "semi_tractor", "yard_truck", "dry_van_trailer", "reefer_trailer", "flatbed_trailer", "tanker_trailer", "dump_trailer", "lowboy_trailer", "chassis_trailer", "dolly", "forklift", "pallet_jack", "scissor_lift", "boom_lift", "skid_steer", "loader", "excavator", "dozer", "conveyor", "compressor", "generator", "pump", "scale", "welding_machine", "custom"], cancellationToken);
            await EnsureCatalogAsync(tenantId, "fuelType", "Fuel Type", ["gasoline", "diesel", "CNG", "LNG", "propane", "electric", "hybrid", "hydrogen", "other"], cancellationToken);
            await EnsureCatalogAsync(tenantId, "brakeType", "Brake Type", ["drum", "disc", "mixed", "not_applicable"], cancellationToken);
            await EnsureCatalogAsync(tenantId, "tireConfiguration", "Tire Configuration", ["single_rear_wheel", "dual_rear_wheel", "duals", "super_single", "mixed", "tracked", "not_applicable"], cancellationToken);
            await EnsureCatalogAsync(tenantId, "make", "Make", ["Ford", "Chevrolet", "GMC", "Ram", "Freightliner", "International", "Kenworth", "Peterbilt", "Volvo", "Mack", "Western Star", "Isuzu", "Hino", "Toyota"], cancellationToken);
            await EnsureCatalogAsync(tenantId, "model", "Model", ["F-150", "F-250", "F-350", "Silverado 1500", "Silverado 2500HD", "Silverado 3500HD", "Sierra 1500", "Sierra 2500HD", "Sierra 3500HD", "Ram 1500", "Ram 2500", "Ram 3500", "Cascadia", "M2", "108SD", "114SD", "LT Series", "RH Series", "MV Series", "T680", "T880", "579", "VNL", "Anthem", "Pinnacle", "N-Series"], cancellationToken);

            await EnsureFieldsetAsync(tenantId, cancellationToken);
        }
    }

    private async Task EnsureCatalogAsync(Guid tenantId, string key, string label, IReadOnlyList<string> options, CancellationToken cancellationToken)
    {
        var catalog = await db.CatalogDefinitions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == key, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (catalog is null)
        {
            catalog = new CatalogDefinition
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Key = key,
                Label = label,
                Description = label,
                Owner = "maintainarr",
                Scope = "tenant",
                IsSystem = true,
                IsTenantExtendable = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.CatalogDefinitions.Add(catalog);
            await db.SaveChangesAsync(cancellationToken);
        }

        foreach (var (value, index) in options.Select((value, i) => (value, i)))
        {
            var optionKey = value.Trim().ToLowerInvariant().Replace(" ", "_").Replace("/", "_");
            var exists = await db.CatalogOptions.AnyAsync(x => x.TenantId == tenantId && x.CatalogId == catalog.Id && x.Key == optionKey, cancellationToken);
            if (!exists)
            {
                db.CatalogOptions.Add(new CatalogOption
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CatalogId = catalog.Id,
                    Key = optionKey,
                    Label = value,
                    Description = value,
                    SortOrder = index,
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureFieldsetAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var definition = await db.FieldsetDefinitions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == "assets" && x.Purpose == "create", cancellationToken);
        if (definition is null)
        {
            definition = new FieldsetDefinition
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Key = "assets",
                Label = "Assets",
                EntityType = "asset",
                Purpose = "create",
                Description = "Asset create fieldset",
                IsActive = true,
            };
            db.FieldsetDefinitions.Add(definition);
            await db.SaveChangesAsync(cancellationToken);
        }

        var fields = new (string key, string label, string control, bool required, string? catalogKey, string? referenceKey, bool drivesLogic)[]
        {
            ("assetClass","Asset Class","select", true, "assetClass", null, true),
            ("assetType","Asset Type","select", true, "assetType", null, true),
            ("make","Make","searchableSelect", true, "make", null, true),
            ("model","Model","searchableSelect", true, "model", null, true),
            ("fuelType","Fuel Type","select", false, "fuelType", null, true),
            ("brakeType","Brake Type","select", false, "brakeType", null, true),
            ("tireConfiguration","Tire Configuration","select", false, "tireConfiguration", null, true),
            ("governingBodyKey","Governing Body","multiSelect", false, null, "governingBody", true),
            ("siteId","Site","asyncCombobox", false, null, "sites", true),
            ("assignedPersonId","Assigned Person","asyncCombobox", false, null, "people", true),
            ("description","Description","textArea", false, null, null, false),
            ("VIN","VIN","text", false, null, null, false),
        };

        foreach (var (key, label, control, required, catalogKey, referenceKey, drivesLogic) in fields)
        {
            if (await db.FieldsetFields.AnyAsync(x => x.TenantId == tenantId && x.FieldsetId == definition.Id && x.Key == key, cancellationToken))
            {
                continue;
            }

            db.FieldsetFields.Add(new FieldsetField
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                FieldsetId = definition.Id,
                Key = key,
                Label = label,
                DataType = "string",
                ControlType = control,
                Required = required,
                CatalogKey = catalogKey,
                ReferenceKey = referenceKey,
                SourceType = referenceKey == "governingBody" ? "compliancecore_reference" : referenceKey == "sites" || referenceKey == "people" ? "staffarr_reference" : "maintainarr_catalog",
                SourceOfTruth = referenceKey == "governingBody" ? "Compliance Core" : referenceKey == "sites" || referenceKey == "people" ? "StaffArr" : "MaintainArr",
                SortOrder = Array.FindIndex(fields, f => f.key == key),
                SectionKey = "core",
                DependencyJson = "{}",
                ValidationJson = "{}",
                DefaultValueJson = "null",
                VisibilityJson = "{}",
                AllowCustom = key == "model",
                CustomRequiresApproval = key == "model",
                DrivesLogic = drivesLogic,
                DrivesInspectionBranching = drivesLogic,
                DrivesPMApplicability = drivesLogic,
                DrivesCompliance = key == "governingBodyKey",
                DrivesReporting = drivesLogic,
                DrivesReadiness = drivesLogic,
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        await EnsureFieldsetPurposeCloneAsync(tenantId, definition, "edit", cancellationToken);
        await EnsureFieldsetPurposeCloneAsync(tenantId, definition, "default", cancellationToken);
    }

    private async Task EnsureFieldsetPurposeCloneAsync(Guid tenantId, FieldsetDefinition source, string purpose, CancellationToken cancellationToken)
    {
        var existing = await db.FieldsetDefinitions.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Key == source.Key && x.Purpose == purpose, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var clone = new FieldsetDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Key = source.Key,
            Label = source.Label,
            EntityType = source.EntityType,
            Purpose = purpose,
            Description = source.Description,
            IsActive = true,
        };
        db.FieldsetDefinitions.Add(clone);
        await db.SaveChangesAsync(cancellationToken);

        var fields = await db.FieldsetFields.Where(x => x.TenantId == tenantId && x.FieldsetId == source.Id).ToListAsync(cancellationToken);
        foreach (var field in fields)
        {
            db.FieldsetFields.Add(new FieldsetField
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                FieldsetId = clone.Id,
                Key = field.Key,
                Label = field.Label,
                Description = field.Description,
                DataType = field.DataType,
                ControlType = field.ControlType,
                Required = field.Required,
                CatalogKey = field.CatalogKey,
                ReferenceKey = field.ReferenceKey,
                SourceType = field.SourceType,
                SourceOfTruth = field.SourceOfTruth,
                SortOrder = field.SortOrder,
                SectionKey = field.SectionKey,
                DependencyJson = field.DependencyJson,
                ValidationJson = field.ValidationJson,
                DefaultValueJson = field.DefaultValueJson,
                VisibilityJson = field.VisibilityJson,
                AllowCustom = field.AllowCustom,
                CustomRequiresApproval = field.CustomRequiresApproval,
                DrivesLogic = field.DrivesLogic,
                DrivesInspectionBranching = field.DrivesInspectionBranching,
                DrivesPMApplicability = field.DrivesPMApplicability,
                DrivesCompliance = field.DrivesCompliance,
                DrivesReporting = field.DrivesReporting,
                DrivesReadiness = field.DrivesReadiness,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
