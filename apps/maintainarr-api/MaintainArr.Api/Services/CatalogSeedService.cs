using System.Text.Json;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaintainArr.Api.Services;

public sealed class CatalogSeedService(MaintainArrDbContext db)
{
    private const string AssetsFieldsetKey = "assets";

    public async Task SeedDefaultsAsync(CancellationToken cancellationToken = default)
    {
        var tenantIds = await ResolveKnownTenantIdsAsync(cancellationToken);
        foreach (var tenantId in tenantIds)
        {
            await EnsureSeededForTenantAsync(tenantId, cancellationToken);
        }
    }

    public async Task EnsureSeededForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var catalogIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in GetCatalogSeeds())
        {
            var catalogId = await EnsureCatalogAsync(tenantId, seed, now, cancellationToken);
            catalogIds[seed.Key] = catalogId;
        }

        await EnsureDependencySeedsAsync(tenantId, catalogIds, cancellationToken);
        await EnsureAssetFieldsetsAsync(tenantId, cancellationToken);
        await EnsureReferenceFallbackSeedsAsync(tenantId, cancellationToken);
    }

    private async Task<IReadOnlyList<Guid>> ResolveKnownTenantIdsAsync(CancellationToken cancellationToken)
    {
        var tenantIds = await db.Assets.AsNoTracking().Select(x => x.TenantId)
            .Union(db.AssetClasses.AsNoTracking().Select(x => x.TenantId))
            .Union(db.AssetTypes.AsNoTracking().Select(x => x.TenantId))
            .Union(db.CatalogDefinitions.AsNoTracking().Select(x => x.TenantId))
            .Distinct()
            .ToListAsync(cancellationToken);

        return tenantIds;
    }

    private async Task<Guid> EnsureCatalogAsync(
        Guid tenantId,
        CatalogSeed seed,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var existing = await db.CatalogDefinitions.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Key == seed.Key,
            cancellationToken);

        if (existing is null)
        {
            existing = new CatalogDefinition
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Key = seed.Key,
                Label = seed.Label,
                Description = seed.Description,
                Owner = "maintainarr",
                Scope = "tenant",
                IsSystem = true,
                IsTenantExtendable = seed.IsTenantExtendable,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.CatalogDefinitions.Add(existing);
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            existing.Label = seed.Label;
            existing.Description = seed.Description;
            existing.IsTenantExtendable = seed.IsTenantExtendable;
            existing.IsActive = true;
            existing.UpdatedAt = now;
            await db.SaveChangesAsync(cancellationToken);
        }

        var existingOptions = await db.CatalogOptions
            .Where(x => x.TenantId == tenantId && x.CatalogId == existing.Id)
            .ToListAsync(cancellationToken);

        var expectedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < seed.Options.Count; index++)
        {
            var label = seed.Options[index];
            var optionKey = NormalizeOptionKey(label);
            expectedKeys.Add(optionKey);

            var matchingOptions = existingOptions
                .Where(x => string.Equals(x.Key, optionKey, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .ToList();
            var option = matchingOptions.FirstOrDefault();
            if (option is null)
            {
                db.CatalogOptions.Add(new CatalogOption
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CatalogId = existing.Id,
                    Key = optionKey,
                    Label = label,
                    Description = label,
                    SortOrder = index,
                    ParentOptionId = null,
                    MetadataJson = "{}",
                    IsSystem = true,
                    IsTenantSpecific = false,
                    OptionTenantId = null,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
                continue;
            }

            option.Label = label;
            option.Description = label;
            option.SortOrder = index;
            option.IsActive = true;
            option.UpdatedAt = now;

            foreach (var duplicate in matchingOptions.Skip(1))
            {
                duplicate.IsActive = false;
                duplicate.UpdatedAt = now;
            }
        }

        foreach (var option in existingOptions.Where(x => !expectedKeys.Contains(x.Key)))
        {
            option.IsActive = false;
            option.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        return existing.Id;
    }

    private async Task EnsureDependencySeedsAsync(
        Guid tenantId,
        IReadOnlyDictionary<string, Guid> catalogIds,
        CancellationToken cancellationToken)
    {
        var seedRows = BuildDependencySeeds();
        var catalogOptions = await db.CatalogOptions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .ToListAsync(cancellationToken);

        var catalogById = await db.CatalogDefinitions.AsNoTracking()
            .Where(x => x.TenantId == tenantId && catalogIds.Values.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Key, cancellationToken);

        var optionIdByCatalogAndKey = catalogOptions
            .Where(x => catalogById.ContainsKey(x.CatalogId))
            .GroupBy(
                x => $"{catalogById[x.CatalogId]}::{x.Key}",
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.CreatedAt)
                    .ThenBy(x => x.Id)
                    .First().Id,
                StringComparer.OrdinalIgnoreCase);

        var existing = await db.CatalogOptionDependencies
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in seedRows)
        {
            if (!optionIdByCatalogAndKey.TryGetValue($"{row.CatalogKey}::{row.OptionKey}", out var optionId))
            {
                continue;
            }

            var signature = $"{optionId}::{row.DependsOnCatalogKey}::{row.DependsOnOptionKey}";
            expected.Add(signature);
            if (existing.Any(x =>
                    x.CatalogOptionId == optionId
                    && string.Equals(x.DependsOnCatalogKey, row.DependsOnCatalogKey, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.DependsOnOptionKey, row.DependsOnOptionKey, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            db.CatalogOptionDependencies.Add(new CatalogOptionDependency
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CatalogOptionId = optionId,
                DependsOnCatalogKey = row.DependsOnCatalogKey,
                DependsOnOptionKey = row.DependsOnOptionKey,
                RuleJson = "{}",
            });
        }

        foreach (var dependency in existing)
        {
            var signature = $"{dependency.CatalogOptionId}::{dependency.DependsOnCatalogKey}::{dependency.DependsOnOptionKey}";
            if (!expected.Contains(signature))
            {
                db.CatalogOptionDependencies.Remove(dependency);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureAssetFieldsetsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var seeds = GetFieldSeeds().ToList();
        foreach (var purpose in new[] { "default", "create", "edit" })
        {
            var definition = await db.FieldsetDefinitions
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenantId && x.Key == AssetsFieldsetKey && x.Purpose == purpose,
                    cancellationToken);

            if (definition is null)
            {
                definition = new FieldsetDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Key = AssetsFieldsetKey,
                    Label = "Assets",
                    EntityType = "asset",
                    Purpose = purpose,
                    Description = $"Asset {purpose} fieldset",
                    IsActive = true,
                };
                db.FieldsetDefinitions.Add(definition);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                definition.IsActive = true;
                definition.Description = $"Asset {purpose} fieldset";
                await db.SaveChangesAsync(cancellationToken);
            }

            var existing = await db.FieldsetFields
                .Where(x => x.TenantId == tenantId && x.FieldsetId == definition.Id)
                .ToListAsync(cancellationToken);

            var expectedKeys = new HashSet<string>(seeds.Select(x => x.Key), StringComparer.OrdinalIgnoreCase);
            foreach (var item in seeds.Select((seed, index) => new { seed, index }))
            {
                var field = existing.FirstOrDefault(x => string.Equals(x.Key, item.seed.Key, StringComparison.OrdinalIgnoreCase));
                if (field is null)
                {
                    field = new FieldsetField
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        FieldsetId = definition.Id,
                        Key = item.seed.Key,
                    };
                    db.FieldsetFields.Add(field);
                }

                field.Label = item.seed.Label;
                field.Description = item.seed.Description;
                field.DataType = item.seed.DataType;
                field.ControlType = item.seed.ControlType;
                field.Required = item.seed.Required;
                field.CatalogKey = item.seed.CatalogKey;
                field.ReferenceKey = item.seed.ReferenceKey;
                field.SourceType = item.seed.SourceType;
                field.SourceOfTruth = item.seed.SourceOfTruth;
                field.SortOrder = item.index;
                field.SectionKey = item.seed.SectionKey;
                field.DependencyJson = JsonSerializer.Serialize(item.seed.DependsOn);
                field.ValidationJson = JsonSerializer.Serialize(item.seed.Validation);
                field.DefaultValueJson = item.seed.DefaultValueJson;
                field.VisibilityJson = JsonSerializer.Serialize(item.seed.Visibility);
                field.AllowCustom = item.seed.AllowCustom;
                field.CustomRequiresApproval = item.seed.CustomRequiresApproval;
                field.DrivesLogic = item.seed.DrivesLogic;
                field.DrivesInspectionBranching = item.seed.DrivesInspectionBranching;
                field.DrivesPMApplicability = item.seed.DrivesPMApplicability;
                field.DrivesCompliance = item.seed.DrivesCompliance;
                field.DrivesReporting = item.seed.DrivesReporting;
                field.DrivesReadiness = item.seed.DrivesReadiness;
            }

            foreach (var field in existing.Where(x => !expectedKeys.Contains(x.Key)))
            {
                db.FieldsetFields.Remove(field);
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureReferenceFallbackSeedsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        await EnsureReferenceOptionsAsync(tenantId, "Compliance Core", "governingBody", ["FMCSA", "OSHA", "MSHA", "EPA"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "Compliance Core", "rulepackApplicabilityKeys", ["us_dot_vehicle", "osha_industrial", "msha_mobile_equipment"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "Compliance Core", "regulatoryAssetType", ["commercial_motor_vehicle", "powered_industrial_truck", "trailer"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "Compliance Core", "complianceCategory", ["inspection", "evidence", "maintenance"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "Compliance Core", "requiredEvidenceType", ["inspection_photo", "repair_invoice", "annual_certificate"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "Compliance Core", "documentRequirementType", ["registration", "insurance", "annual_inspection"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "Compliance Core", "inspectionRequirementType", ["annual_dot", "pre_trip", "post_trip"], cancellationToken);

        await EnsureReferenceOptionsAsync(tenantId, "StaffArr", "sites", ["site_a", "site_b", "site_c"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "StaffArr", "departments", ["fleet", "shop", "operations"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "StaffArr", "teams", ["team_alpha", "team_bravo"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "StaffArr", "people", ["person_1001", "person_1002", "person_1003"], cancellationToken);

        await EnsureReferenceOptionsAsync(tenantId, "SupplyArr", "vendors", ["vendor_1001", "vendor_1002"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "SupplyArr", "customers", ["customer_1001", "customer_1002"], cancellationToken);
        await EnsureReferenceOptionsAsync(tenantId, "SupplyArr", "parts", ["part_1001", "part_1002", "part_1003"], cancellationToken);
    }

    private async Task EnsureReferenceOptionsAsync(
        Guid tenantId,
        string sourceOfTruth,
        string referenceKey,
        IReadOnlyList<string> values,
        CancellationToken cancellationToken)
    {
        var existing = await db.ReferenceCacheEntries
            .Where(x => x.TenantId == tenantId && x.SourceOfTruth == sourceOfTruth && x.ReferenceKey == referenceKey)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var expectedKeys = new HashSet<string>(values.Select(NormalizeOptionKey), StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < values.Count; index++)
        {
            var label = values[index];
            var key = NormalizeOptionKey(label);
            var item = existing.FirstOrDefault(x => string.Equals(x.ExternalKey, key, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                db.ReferenceCacheEntries.Add(new ReferenceCacheEntry
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    SourceOfTruth = sourceOfTruth,
                    ReferenceKey = referenceKey,
                    ExternalKey = key,
                    ExternalId = key,
                    Label = HumanizeKey(label),
                    Description = null,
                    MetadataJson = "{}",
                    IsActive = true,
                    LastSyncedAt = now,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
                continue;
            }

            item.Label = HumanizeKey(label);
            item.IsActive = true;
            item.LastSyncedAt = now;
            item.UpdatedAt = now;
        }

        foreach (var item in existing.Where(x => !expectedKeys.Contains(x.ExternalKey)))
        {
            item.IsActive = false;
            item.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<CatalogSeed> GetCatalogSeeds()
    {
        return
        [
            new CatalogSeed("assetClass", "Asset Class", ["vehicle", "trailer", "powered_industrial_truck", "heavy_equipment", "production_equipment", "facility_equipment", "tool", "component", "attachment"]),
            new CatalogSeed("assetType", "Asset Type", ["pickup", "service_truck", "cargo_van", "box_truck", "semi_tractor", "yard_truck", "dry_van_trailer", "reefer_trailer", "flatbed_trailer", "tanker_trailer", "dump_trailer", "lowboy_trailer", "chassis_trailer", "dolly", "forklift", "pallet_jack", "scissor_lift", "boom_lift", "skid_steer", "loader", "excavator", "dozer", "conveyor", "compressor", "generator", "pump", "scale", "welding_machine", "custom"]),
            new CatalogSeed("assetSubtype", "Asset Subtype", ["standard", "custom"]),
            new CatalogSeed("assetCategory", "Asset Category", ["mobile", "fixed", "support"]),
            new CatalogSeed("assetStatus", "Asset Status", ["active", "inactive", "out_of_service", "retired", "sold", "archived"]),
            new CatalogSeed("lifecycleStatus", "Lifecycle Status", ["ordered", "received", "in_service", "temporarily_inactive", "pending_disposal", "disposed", "retired"]),
            new CatalogSeed("criticality", "Criticality", ["low", "medium", "high", "critical"]),
            new CatalogSeed("maintenanceClass", "Maintenance Class", ["standard", "regulated", "critical"]),
            new CatalogSeed("serviceGroup", "Service Group", ["fleet", "shop", "yard", "facility"]),
            new CatalogSeed("readinessStatus", "Readiness Status", ["ready", "limited_use", "inspection_due", "pm_due", "defect_open", "out_of_service", "blocked"]),
            new CatalogSeed("operationalStatus", "Operational Status", ["operational", "degraded", "non_operational", "unknown"]),
            new CatalogSeed("availabilityStatus", "Availability Status", ["available", "assigned", "in_shop", "down", "reserved", "unavailable"]),

            new CatalogSeed("make", "Make", ["Ford", "Chevrolet", "GMC", "Ram", "Freightliner", "International", "Kenworth", "Peterbilt", "Volvo", "Mack", "Western Star", "Isuzu", "Hino", "Toyota", "Great Dane", "Wabash", "Utility", "Hyundai Translead", "Stoughton", "Vanguard", "Fontaine", "Reitnouer", "MAC Trailer", "Wilson", "East", "Heil", "Polar", "Toyota Material Handling", "Hyster", "Yale", "Crown", "Raymond", "Mitsubishi Logisnext", "Clark", "Caterpillar", "John Deere", "Komatsu", "JLG", "Genie", "Skyjack", "Bobcat", "Case", "Kubota"]),
            new CatalogSeed("manufacturer", "Manufacturer", ["Ford", "Chevrolet", "GMC", "Ram", "Freightliner", "International", "Kenworth", "Peterbilt", "Volvo", "Mack", "Western Star", "Isuzu", "Hino", "Toyota", "Great Dane", "Wabash", "Utility", "Hyundai Translead", "Stoughton", "Vanguard", "Fontaine", "Reitnouer", "MAC Trailer", "Wilson", "East", "Heil", "Polar", "Toyota Material Handling", "Hyster", "Yale", "Crown", "Raymond", "Mitsubishi Logisnext", "Clark", "Caterpillar", "John Deere", "Komatsu", "JLG", "Genie", "Skyjack", "Bobcat", "Case", "Kubota"]),
            new CatalogSeed("model", "Model", ["F-150", "F-250", "F-350", "Silverado 1500", "Silverado 2500HD", "Silverado 3500HD", "Sierra 1500", "Sierra 2500HD", "Sierra 3500HD", "Ram 1500", "Ram 2500", "Ram 3500", "Cascadia", "M2", "108SD", "114SD", "LT Series", "RH Series", "MV Series", "T680", "T880", "579", "VNL", "Anthem", "Pinnacle", "N-Series"]),
            new CatalogSeed("modelYear", "Model Year", ["2018", "2019", "2020", "2021", "2022", "2023", "2024", "2025", "2026"]),
            new CatalogSeed("trim", "Trim", ["base", "xl", "xlt", "lt", "ltz", "limited", "custom"]),
            new CatalogSeed("series", "Series", ["standard", "hd", "lt_series", "rh_series", "mv_series", "custom"]),
            new CatalogSeed("bodyStyle", "Body Style", ["pickup", "cargo_van", "cab_chassis", "tractor", "trailer", "custom"]),
            new CatalogSeed("bodyType", "Body Type", ["pickup", "service_body", "utility_body", "box", "cargo_van", "flatbed", "dump", "tanker", "tractor", "yard_truck", "bus", "custom"]),
            new CatalogSeed("configuration", "Configuration", ["standard", "offroad", "reefer", "liftgate", "custom"]),

            new CatalogSeed("cabType", "Cab Type", ["regular_cab", "extended_cab", "crew_cab", "day_cab", "sleeper_cab", "cab_over", "cutaway", "chassis_cab"]),
            new CatalogSeed("sleeperCab", "Sleeper Cab", ["yes", "no"]),
            new CatalogSeed("dayCab", "Day Cab", ["yes", "no"]),
            new CatalogSeed("drivetrain", "Drivetrain", ["2wd", "4wd", "awd", "4x2", "4x4", "6x4", "6x2", "8x4", "tracked", "not_applicable"]),
            new CatalogSeed("axleConfiguration", "Axle Configuration", ["single_drive", "tandem_drive", "tri_drive", "steer_plus_single_drive", "steer_plus_tandem_drive", "spread_axle", "tandem_trailer", "tri_axle_trailer", "single_axle_trailer"]),
            new CatalogSeed("steerAxleCount", "Steer Axle Count", ["1", "2", "3"]),
            new CatalogSeed("driveAxleCount", "Drive Axle Count", ["1", "2", "3"]),
            new CatalogSeed("trailerAxleCount", "Trailer Axle Count", ["1", "2", "3", "4"]),
            new CatalogSeed("singleDriveAxle", "Single Drive Axle", ["yes", "no"]),
            new CatalogSeed("tandemDriveAxle", "Tandem Drive Axle", ["yes", "no"]),
            new CatalogSeed("liftAxleEquipped", "Lift Axle Equipped", ["true", "false"]),
            new CatalogSeed("tagAxleEquipped", "Tag Axle Equipped", ["true", "false"]),
            new CatalogSeed("pusherAxleEquipped", "Pusher Axle Equipped", ["true", "false"]),
            new CatalogSeed("dualRearWheel", "Dual Rear Wheel", ["true", "false"]),
            new CatalogSeed("superSinglesEquipped", "Super Singles Equipped", ["true", "false"]),
            new CatalogSeed("dualsEquipped", "Duals Equipped", ["true", "false"]),
            new CatalogSeed("fourWheelDrive", "Four Wheel Drive", ["true", "false"]),
            new CatalogSeed("allWheelDrive", "All Wheel Drive", ["true", "false"]),
            new CatalogSeed("twoWheelDrive", "Two Wheel Drive", ["true", "false"]),
            new CatalogSeed("tireConfiguration", "Tire Configuration", ["single_rear_wheel", "dual_rear_wheel", "duals", "super_single", "mixed", "tracked", "not_applicable"]),

            new CatalogSeed("fuelType", "Fuel Type", ["gasoline", "diesel", "CNG", "LNG", "propane", "electric", "hybrid", "hydrogen", "other"]),
            new CatalogSeed("secondaryFuelType", "Secondary Fuel Type", ["none", "gasoline", "diesel", "CNG", "LNG", "propane", "electric", "hybrid", "hydrogen"]),
            new CatalogSeed("DEFRequired", "DEF Required", ["true", "false"]),
            new CatalogSeed("aftertreatmentType", "Aftertreatment Type", ["none", "DPF", "SCR", "EGR", "DPF_SCR", "DOC", "three_way_catalyst", "unknown"]),
            new CatalogSeed("EVBatteryType", "EV Battery Type", ["lithium_ion", "lead_acid", "solid_state", "not_applicable"]),
            new CatalogSeed("chargingConnectorType", "Charging Connector Type", ["J1772", "CCS1", "CCS2", "NACS", "CHAdeMO", "proprietary", "not_applicable"]),
            new CatalogSeed("hybridType", "Hybrid Type", ["mild_hybrid", "full_hybrid", "plug_in_hybrid", "range_extender", "not_applicable"]),
            new CatalogSeed("CNGEquipped", "CNG Equipped", ["true", "false"]),
            new CatalogSeed("LNGEquipped", "LNG Equipped", ["true", "false"]),
            new CatalogSeed("propaneEquipped", "Propane Equipped", ["true", "false"]),

            new CatalogSeed("brakeSystemType", "Brake System Type", ["air", "hydraulic", "electric", "air_over_hydraulic", "mechanical", "regenerative", "mixed", "not_applicable"]),
            new CatalogSeed("brakeType", "Brake Type", ["drum", "disc", "mixed", "not_applicable"]),
            new CatalogSeed("airBrakes", "Air Brakes", ["true", "false"]),
            new CatalogSeed("hydraulicBrakes", "Hydraulic Brakes", ["true", "false"]),
            new CatalogSeed("electricBrakes", "Electric Brakes", ["true", "false"]),
            new CatalogSeed("drumBrakes", "Drum Brakes", ["true", "false"]),
            new CatalogSeed("discBrakes", "Disc Brakes", ["true", "false"]),
            new CatalogSeed("ABSRequired", "ABS Required", ["true", "false"]),
            new CatalogSeed("ABSEquipped", "ABS Equipped", ["true", "false"]),
            new CatalogSeed("slackAdjusterType", "Slack Adjuster Type", ["automatic", "manual", "not_applicable", "unknown"]),
            new CatalogSeed("brakeChamberType", "Brake Chamber Type", ["type_20", "type_24", "type_30", "type_36", "spring_brake", "not_applicable", "unknown"]),
            new CatalogSeed("parkingBrakeType", "Parking Brake Type", ["air_spring_brake", "mechanical", "electric", "hydraulic", "driveline", "not_applicable"]),

            new CatalogSeed("trailerType", "Trailer Type", ["dry_van", "reefer", "flatbed", "step_deck", "lowboy", "tanker", "dump", "chassis", "utility", "enclosed", "container_chassis", "car_hauler", "dolly", "other"]),
            new CatalogSeed("trailerBodyType", "Trailer Body Type", ["dry_van", "reefer", "flatbed", "tanker", "dump", "chassis", "other"]),
            new CatalogSeed("trailerDoorType", "Trailer Door Type", ["swing", "roll_up", "barn", "side_door", "curtain_side", "none"]),
            new CatalogSeed("roofType", "Roof Type", ["aluminum", "translucent", "steel", "composite", "soft_top", "open", "unknown"]),
            new CatalogSeed("floorType", "Floor Type", ["wood", "aluminum", "steel", "composite", "refrigerated", "unknown"]),
            new CatalogSeed("landingGearType", "Landing Gear Type", ["manual", "two_speed", "air_powered", "hydraulic", "electric", "not_applicable"]),
            new CatalogSeed("kingpinType", "Kingpin Type", ["fixed", "sliding", "adjustable", "not_applicable"]),
            new CatalogSeed("reeferEquipped", "Reefer Equipped", ["true", "false"]),
            new CatalogSeed("liftGateEquipped", "Lift Gate Equipped", ["true", "false"]),
            new CatalogSeed("axleSpreadType", "Axle Spread Type", ["single", "tandem", "spread", "tri_axle", "sliding_tandem", "fixed_tandem"]),
            new CatalogSeed("suspensionType", "Suspension Type", ["air_ride", "leaf_spring", "torsion", "walking_beam", "rubber_block", "hydraulic", "unknown"]),

            new CatalogSeed("engineMake", "Engine Make", ["Cummins", "Detroit", "PACCAR", "Volvo", "Mack", "Ford", "GM", "International", "Isuzu", "Hino", "Caterpillar", "John Deere", "Kubota", "Toyota", "Kohler", "Briggs & Stratton", "Honda"]),
            new CatalogSeed("engineModel", "Engine Model", ["x15", "dd15", "mx13", "d13", "isx", "b6_7", "custom"]),
            new CatalogSeed("engineFamily", "Engine Family", ["light_duty", "medium_duty", "heavy_duty", "offroad", "electric"]),
            new CatalogSeed("emissionsLevel", "Emissions Level", ["pre_emissions", "EPA_2004", "EPA_2007", "EPA_2010", "Tier_4", "Tier_4_Final", "Euro_V", "Euro_VI", "zero_emission", "unknown"]),
            new CatalogSeed("transmissionMake", "Transmission Make", ["Allison", "Eaton", "Detroit", "PACCAR", "Volvo", "Mack", "Ford", "GM", "ZF", "Aisin"]),
            new CatalogSeed("transmissionModel", "Transmission Model", ["1000_series", "2000_series", "3000_series", "ultrashift", "auto_shift", "custom"]),
            new CatalogSeed("transmissionType", "Transmission Type", ["automatic", "manual", "automated_manual", "CVT", "hydrostatic", "direct_drive", "electric_drive", "not_applicable"]),
            new CatalogSeed("PTOEquipped", "PTO Equipped", ["true", "false"]),
            new CatalogSeed("PTOType", "PTO Type", ["transmission_mounted", "engine_mounted", "split_shaft", "electric", "hydraulic", "not_applicable"]),
            new CatalogSeed("driveType", "Drive Type", ["fwd", "rwd", "awd", "4x4", "tracked", "not_applicable"]),

            new CatalogSeed("tireSize", "Tire Size", ["LT245/75R16", "LT265/70R17", "LT275/70R18", "LT275/65R20", "225/70R19.5", "245/70R19.5", "11R22.5", "295/75R22.5", "275/80R22.5", "285/75R24.5", "11R24.5", "445/50R22.5", "455/55R22.5", "215/75R17.5", "235/75R17.5", "8.25R15", "custom"]),
            new CatalogSeed("wheelSize", "Wheel Size", ["16", "17", "18", "19.5", "22.5", "24.5"]),
            new CatalogSeed("wheelMaterial", "Wheel Material", ["steel", "aluminum", "alloy", "composite", "unknown"]),
            new CatalogSeed("tirePositionLayout", "Tire Position Layout", ["steer_2", "steer_2_drive_2", "steer_2_drive_4", "steer_2_drive_8", "steer_2_drive_8_trailer_8", "super_single_drive", "trailer_single", "trailer_tandem", "trailer_tri_axle", "forklift_solid", "forklift_pneumatic", "tracked", "custom"]),
            new CatalogSeed("steerTireSize", "Steer Tire Size", ["11R22.5", "295/75R22.5", "275/80R22.5"]),
            new CatalogSeed("driveTireSize", "Drive Tire Size", ["11R22.5", "295/75R22.5", "275/80R22.5", "445/50R22.5"]),
            new CatalogSeed("trailerTireSize", "Trailer Tire Size", ["11R22.5", "295/75R22.5", "445/50R22.5"]),
            new CatalogSeed("spareTireEquipped", "Spare Tire Equipped", ["true", "false"]),
            new CatalogSeed("retreadAllowed", "Retread Allowed", ["true", "false"]),
            new CatalogSeed("treadDepthMinimumRule", "Tread Depth Minimum Rule", ["2_32", "4_32", "custom"]),
            new CatalogSeed("torqueSpec", "Torque Spec", ["standard", "custom"]),
            new CatalogSeed("pressureSpec", "Pressure Spec", ["standard", "custom"]),

            new CatalogSeed("primaryMeterType", "Primary Meter Type", ["odometer", "engine_hours", "calendar_only"]),
            new CatalogSeed("secondaryMeterTypes", "Secondary Meter Types", ["idle_hours", "pto_hours", "cycle_count", "starts", "fuel_used", "energy_used"]),
            new CatalogSeed("meterType", "Meter Type", ["odometer", "engine_hours", "idle_hours", "pto_hours", "hubometer", "cycle_count", "starts", "fuel_used", "energy_used", "calendar_only"]),
            new CatalogSeed("meterUnit", "Meter Unit", ["miles", "kilometers", "hours", "cycles", "starts", "gallons", "liters", "kwh", "days"]),
            new CatalogSeed("usageProfile", "Usage Profile", ["low_use", "normal", "high_use", "severe_duty", "seasonal", "standby", "rental", "yard_only", "road_service"]),
            new CatalogSeed("meterReadingSource", "Meter Reading Source", ["manual", "inspection", "work_order", "telematics", "import", "estimate", "correction"]),
            new CatalogSeed("meterRolloverBehavior", "Meter Rollover Behavior", ["none", "rollover_supported", "correction_required", "replace_meter", "unknown"]),

            new CatalogSeed("PMProgram", "PM Program", ["standard_fleet", "regulated", "custom"]),
            new CatalogSeed("PMTemplate", "PM Template", ["a_service", "b_service", "annual_service", "custom"]),
            new CatalogSeed("PMType", "PM Type", ["preventive", "scheduled_service", "mileage_based", "hour_based", "calendar_based", "seasonal", "regulatory", "lubrication", "filter_service", "inspection_based", "custom"]),
            new CatalogSeed("PMIntervalType", "PM Interval Type", ["calendar_days", "mileage", "engine_hours", "cycles", "mixed_first_due", "mixed_all_due"]),
            new CatalogSeed("inspectionTemplate", "Inspection Template", ["annual_dot", "dvir", "shop_inspection", "custom"]),
            new CatalogSeed("inspectionType", "Inspection Type", ["annual_dot", "dvir", "pre_trip", "post_trip", "pm_inspection", "shop_inspection", "safety_inspection", "asset_intake", "return_to_service", "damage_inspection", "road_call_inspection", "operator_walkaround", "calibration_check", "custom"]),
            new CatalogSeed("requiredInspectionTypes", "Required Inspection Types", ["annual_dot", "dvir", "pre_trip", "post_trip"]),
            new CatalogSeed("inspectionFrequencyType", "Inspection Frequency Type", ["per_use", "daily", "weekly", "monthly", "quarterly", "semiannual", "annual", "mileage_based", "hour_based", "event_based", "custom"]),
            new CatalogSeed("seasonalPMGroup", "Seasonal PM Group", ["winter", "summer", "all_season"]),
            new CatalogSeed("regulatoryInspectionRequired", "Regulatory Inspection Required", ["true", "false"]),
            new CatalogSeed("annualInspectionRequired", "Annual Inspection Required", ["true", "false"]),
            new CatalogSeed("DVIRRequired", "DVIR Required", ["true", "false"]),
            new CatalogSeed("preTripRequired", "Pre Trip Required", ["true", "false"]),
            new CatalogSeed("postTripRequired", "Post Trip Required", ["true", "false"]),
            new CatalogSeed("shopInspectionRequired", "Shop Inspection Required", ["true", "false"]),

            new CatalogSeed("defectCategory", "Defect Category", ["safety", "compliance", "mechanical", "electrical", "body", "cosmetic", "operational", "documentation", "fluid_leak", "tire_wheel", "brake", "lighting", "steering", "suspension"]),
            new CatalogSeed("defectSystem", "Defect System", ["brakes", "steering", "suspension", "tires_wheels", "lights", "electrical", "engine", "transmission", "aftertreatment", "body", "frame", "coupling", "hydraulic", "pneumatic", "fuel", "cooling", "HVAC", "safety_equipment", "documentation"]),
            new CatalogSeed("defectComponent", "Defect Component", ["brake_chamber", "slack_adjuster", "tire", "wheel", "lamp", "mirror", "custom"]),
            new CatalogSeed("failureMode", "Failure Mode", ["leaking", "cracked", "broken", "missing", "loose", "worn", "out_of_adjustment", "inoperative", "intermittent", "contaminated", "overheating", "abnormal_noise", "abnormal_vibration", "low_pressure", "high_pressure", "out_of_spec", "expired", "unreadable", "damaged", "corroded"]),
            new CatalogSeed("severity", "Severity", ["informational", "minor", "moderate", "major", "critical"]),
            new CatalogSeed("priority", "Priority", ["low", "normal", "high", "urgent", "emergency"]),
            new CatalogSeed("safetyCritical", "Safety Critical", ["true", "false"]),
            new CatalogSeed("operatingRestriction", "Operating Restriction", ["none", "monitor", "limited_use", "shop_only", "yard_only", "no_dispatch", "out_of_service"]),
            new CatalogSeed("repairDisposition", "Repair Disposition", ["defer", "monitor", "repair_now", "repair_before_dispatch", "replace", "adjust", "clean", "lubricate", "diagnose", "vendor_repair", "warranty_claim"]),
            new CatalogSeed("rootCause", "Root Cause", ["wear", "impact_damage", "operator_damage", "improper_installation", "lack_of_maintenance", "contamination", "corrosion", "manufacturing_defect", "abuse", "unknown"]),
            new CatalogSeed("correctiveActionType", "Corrective Action Type", ["repaired", "replaced", "adjusted", "cleaned", "lubricated", "tightened", "calibrated", "inspected_no_fault_found", "deferred", "vendor_repair", "warranty_repair"]),

            new CatalogSeed("workOrderType", "Work Order Type", ["corrective", "preventive", "inspection_generated", "defect_generated", "campaign", "recall", "warranty", "road_call", "emergency", "calibration", "fabrication", "install", "removal"]),
            new CatalogSeed("workOrderPriority", "Work Order Priority", ["low", "normal", "high", "urgent", "emergency"]),
            new CatalogSeed("maintenanceType", "Maintenance Type", ["PM", "inspection", "repair", "diagnostic", "replacement", "adjustment", "calibration", "cleaning", "lubrication", "rebuild"]),
            new CatalogSeed("repairType", "Repair Type", ["repair", "replace", "adjust", "diagnose", "custom"]),
            new CatalogSeed("laborCategory", "Labor Category", ["mechanic", "electrician", "inspector", "vendor"]),
            new CatalogSeed("downtimeReason", "Downtime Reason", ["defect", "PM", "inspection_failure", "parts_delay", "vendor_delay", "accident_damage", "recall", "waiting_approval", "staffing", "unknown"]),
            new CatalogSeed("shopStatus", "Shop Status", ["not_started", "waiting_for_assignment", "assigned", "diagnosis", "waiting_for_parts", "waiting_for_approval", "in_progress", "vendor", "quality_check", "ready_for_return", "completed", "cancelled"]),
            new CatalogSeed("bay", "Bay", ["bay_1", "bay_2", "bay_3", "bay_4"]),
            new CatalogSeed("approvalStatus", "Approval Status", ["not_required", "pending", "approved", "rejected", "cancelled"]),
            new CatalogSeed("completionStatus", "Completion Status", ["open", "completed", "completed_with_followup", "deferred", "cancelled"]),
            new CatalogSeed("returnToServiceStatus", "Return To Service Status", ["not_ready", "ready", "ready_with_restriction", "returned_to_service", "blocked"]),

            new CatalogSeed("documentType", "Document Type", ["registration", "insurance", "annual_inspection", "title", "warranty", "calibration_certificate", "repair_invoice", "inspection_photo", "compliance_evidence", "manufacturer_manual", "permit", "emissions_certificate", "purchase_document", "lease_document", "recall_notice", "service_bulletin", "custom"]),
            new CatalogSeed("documentStatus", "Document Status", ["active", "expired", "missing", "pending_review", "rejected", "superseded", "archived"]),
            new CatalogSeed("verificationStatus", "Verification Status", ["unverified", "pending", "verified", "rejected", "expired", "not_required"]),
            new CatalogSeed("expirationType", "Expiration Type", ["no_expiration", "fixed_date", "interval_from_issue", "meter_based", "event_based"]),

            new CatalogSeed("fluidSpec", "Fluid Spec", ["standard", "custom"]),
            new CatalogSeed("oilSpec", "Oil Spec", ["15w40", "10w30", "synthetic", "custom"]),
            new CatalogSeed("coolantSpec", "Coolant Spec", ["oat", "hoat", "conventional", "custom"]),
            new CatalogSeed("DEFSpec", "DEF Spec", ["iso_22241", "not_applicable"]),
            new CatalogSeed("filterSpec", "Filter Spec", ["standard", "custom"]),
            new CatalogSeed("beltSpec", "Belt Spec", ["standard", "custom"]),
            new CatalogSeed("batterySpec", "Battery Spec", ["group_31", "group_24", "lithium_pack", "custom"]),
            new CatalogSeed("lampSpec", "Lamp Spec", ["halogen", "led", "hid", "custom"]),
            new CatalogSeed("brakePartSpec", "Brake Part Spec", ["standard", "custom"]),
            new CatalogSeed("tireSpec", "Tire Spec", ["11R22.5", "295/75R22.5", "custom"]),
            new CatalogSeed("wheelSpec", "Wheel Spec", ["22.5_aluminum", "22.5_steel", "custom"]),

            new CatalogSeed("telematicsProvider", "Telematics Provider", ["Samsara", "Geotab", "Motive", "Verizon Connect", "Fleet Complete", "Omnitracs", "Zonar", "Platform Science", "OEM", "manual", "none", "other"]),
            new CatalogSeed("diagnosticProtocol", "Diagnostic Protocol", ["OBD2", "J1939", "J1708", "CAN", "ISO_15765", "MID_PID_SID_FMI", "proprietary", "unknown"]),
            new CatalogSeed("ELDProvider", "ELD Provider", ["Samsara", "Geotab", "Motive", "Omnitracs", "other", "none"]),
            new CatalogSeed("GPSProvider", "GPS Provider", ["Samsara", "Geotab", "Motive", "Verizon Connect", "other", "none"]),
            new CatalogSeed("faultCodeSource", "Fault Code Source", ["telematics", "inspection", "work_order", "manual", "other"]),
            new CatalogSeed("faultCodeStandard", "Fault Code Standard", ["DTC", "SPN_FMI", "MID_PID_SID_FMI", "OEM", "unknown"]),
            new CatalogSeed("dataSyncStatus", "Data Sync Status", ["connected", "disconnected", "degraded", "pending", "failed", "disabled"]),
        ];
    }

    private static IEnumerable<FieldSeed> GetFieldSeeds()
    {
        yield return FieldSeed.Catalog("assetClass", "Asset Class", "Asset classification", required: true, catalogKey: "assetClass", sectionKey: "identity", drivesLogic: true);
        yield return FieldSeed.Catalog("assetType", "Asset Type", "Asset type", required: true, catalogKey: "assetType", sectionKey: "identity", drivesLogic: true, dependsOn: new Dictionary<string, string> { ["assetClass"] = "assetClass" });
        yield return FieldSeed.Catalog("assetStatus", "Asset Status", "Operational asset status", required: true, catalogKey: "assetStatus", sectionKey: "identity", drivesLogic: true);
        yield return FieldSeed.Catalog("lifecycleStatus", "Lifecycle Status", "Lifecycle state", required: true, catalogKey: "lifecycleStatus", sectionKey: "identity", drivesLogic: true, defaultValueJson: "\"in_service\"");
        yield return FieldSeed.Catalog("criticality", "Criticality", "Business criticality", required: true, catalogKey: "criticality", sectionKey: "identity", drivesLogic: true);

        yield return FieldSeed.Catalog("make", "Make", "Manufacturer make", required: false, catalogKey: "make", sectionKey: "classification", drivesLogic: true, dependsOn: new Dictionary<string, string> { ["assetClass"] = "assetClass" });
        yield return FieldSeed.Catalog("model", "Model", "Asset model", required: false, catalogKey: "model", sectionKey: "classification", drivesLogic: true, allowCustom: true, customRequiresApproval: true, dependsOn: new Dictionary<string, string> { ["make"] = "make" });
        yield return FieldSeed.Catalog("modelYear", "Model Year", "Model year", required: false, catalogKey: "modelYear", sectionKey: "classification", drivesLogic: true);
        yield return FieldSeed.Catalog("series", "Series", "Series", required: false, catalogKey: "series", sectionKey: "classification", drivesLogic: true);
        yield return FieldSeed.Catalog("trim", "Trim", "Trim", required: false, catalogKey: "trim", sectionKey: "classification", drivesLogic: true);
        yield return FieldSeed.Catalog("configuration", "Configuration", "Configuration", required: false, catalogKey: "configuration", sectionKey: "classification", drivesLogic: true);

        yield return FieldSeed.Catalog("fuelType", "Fuel Type", "Fuel or energy type", required: false, catalogKey: "fuelType", sectionKey: "configuration", drivesLogic: true);
        yield return FieldSeed.Catalog("aftertreatmentType", "Aftertreatment Type", "Aftertreatment profile", required: false, catalogKey: "aftertreatmentType", sectionKey: "configuration", drivesLogic: true, dependsOn: new Dictionary<string, string> { ["fuelType"] = "fuelType" });
        yield return FieldSeed.Catalog("hybridType", "Hybrid Type", "Hybrid profile", required: false, catalogKey: "hybridType", sectionKey: "configuration", drivesLogic: true, dependsOn: new Dictionary<string, string> { ["fuelType"] = "fuelType" });
        yield return FieldSeed.Catalog("brakeType", "Brake Type", "Brake type", required: false, catalogKey: "brakeType", sectionKey: "configuration", drivesLogic: true);
        yield return FieldSeed.Catalog("brakeSystemType", "Brake System Type", "Brake system type", required: false, catalogKey: "brakeSystemType", sectionKey: "configuration", drivesLogic: true);
        yield return FieldSeed.Catalog("axleConfiguration", "Axle Configuration", "Axle arrangement", required: false, catalogKey: "axleConfiguration", sectionKey: "configuration", drivesLogic: true);
        yield return FieldSeed.Catalog("tireConfiguration", "Tire Configuration", "Tire layout", required: false, catalogKey: "tireConfiguration", sectionKey: "configuration", drivesLogic: true);
        yield return FieldSeed.Catalog("trailerType", "Trailer Type", "Trailer profile", required: false, catalogKey: "trailerType", sectionKey: "configuration", drivesLogic: true);
        yield return FieldSeed.Catalog("meterType", "Primary Meter Type", "Primary meter type", required: false, catalogKey: "meterType", sectionKey: "usage", drivesLogic: true);

        yield return FieldSeed.Reference("governingBodyKey", "Governing Body", "Regulatory governing body", required: false, "governingBody", "compliancecore_reference", "Compliance Core", "compliance", true, multi: true, drivesCompliance: true);
        yield return FieldSeed.Reference("rulepackApplicabilityKeys", "Rulepack Applicability", "Compliance Core rulepacks", required: false, "rulepackApplicabilityKeys", "compliancecore_reference", "Compliance Core", "compliance", true, multi: true, drivesCompliance: true);
        yield return FieldSeed.Reference("regulatoryAssetType", "Regulatory Asset Type", "Compliance Core asset type", required: false, "regulatoryAssetType", "compliancecore_reference", "Compliance Core", "compliance", true, multi: true, drivesCompliance: true);
        yield return FieldSeed.Reference("complianceCategory", "Compliance Category", "Compliance Core category", required: false, "complianceCategory", "compliancecore_reference", "Compliance Core", "compliance", true, multi: true, drivesCompliance: true);
        yield return FieldSeed.Reference("requiredEvidenceType", "Required Evidence Type", "Compliance Core evidence requirement", required: false, "requiredEvidenceType", "compliancecore_reference", "Compliance Core", "compliance", true, multi: true, drivesCompliance: true);
        yield return FieldSeed.Reference("documentRequirementType", "Document Requirement Type", "Compliance Core document requirement", required: false, "documentRequirementType", "compliancecore_reference", "Compliance Core", "compliance", true, multi: true, drivesCompliance: true);
        yield return FieldSeed.Reference("inspectionRequirementType", "Inspection Requirement Type", "Compliance Core inspection requirement", required: false, "inspectionRequirementType", "compliancecore_reference", "Compliance Core", "compliance", true, multi: true, drivesCompliance: true);

        yield return FieldSeed.Reference("siteId", "Site", "StaffArr site reference", required: false, "sites", "staffarr_reference", "StaffArr", "assignment", true);
        yield return FieldSeed.Reference("departmentId", "Department", "StaffArr department reference", required: false, "departments", "staffarr_reference", "StaffArr", "assignment", true);
        yield return FieldSeed.Reference("teamId", "Team", "StaffArr team reference", required: false, "teams", "staffarr_reference", "StaffArr", "assignment", true);
        yield return FieldSeed.Reference("assignedPersonId", "Assigned Person", "Assigned person", required: false, "people", "staffarr_reference", "StaffArr", "assignment", true);
        yield return FieldSeed.Reference("operatorPersonId", "Operator", "Operator person", required: false, "people", "staffarr_reference", "StaffArr", "assignment", true);
        yield return FieldSeed.Reference("driverPersonId", "Driver", "Driver person", required: false, "people", "staffarr_reference", "StaffArr", "assignment", true);
        yield return FieldSeed.Reference("purchaseVendorId", "Purchase Vendor", "SupplyArr vendor reference", required: false, "vendors", "supplyarr_reference", "SupplyArr", "assignment", true);
        yield return FieldSeed.Reference("repairVendorId", "Repair Vendor", "SupplyArr vendor reference", required: false, "vendors", "supplyarr_reference", "SupplyArr", "assignment", true);
        yield return FieldSeed.Reference("customerId", "Customer", "SupplyArr customer reference", required: false, "customers", "supplyarr_reference", "SupplyArr", "assignment", true);
        yield return FieldSeed.Reference("compatiblePartIds", "Compatible Parts", "SupplyArr part references", required: false, "parts", "supplyarr_reference", "SupplyArr", "components", true, multi: true);
        yield return FieldSeed.Reference("preferredPartId", "Preferred Part", "SupplyArr part reference", required: false, "parts", "supplyarr_reference", "SupplyArr", "components", true);

        yield return FieldSeed.FreeText("description", "Description", "Description and notes", sectionKey: "free_text");
        yield return FieldSeed.FreeText("notes", "Notes", "Operational notes", sectionKey: "free_text");
        yield return FieldSeed.FreeText("VIN", "VIN", "Vehicle identification number", sectionKey: "identity", validation: new Dictionary<string, object?> { ["maxLength"] = 17, ["pattern"] = "^[A-HJ-NPR-Z0-9]{11,17}$" });
        yield return FieldSeed.FreeText("serialNumber", "Serial Number", "Serial number", sectionKey: "identity", validation: new Dictionary<string, object?> { ["maxLength"] = 64 });
        yield return FieldSeed.FreeText("licensePlate", "License Plate", "Plate number", sectionKey: "identity", validation: new Dictionary<string, object?> { ["maxLength"] = 32 });
        yield return FieldSeed.FreeText("unitNumber", "Unit Number", "Unit number", sectionKey: "identity", validation: new Dictionary<string, object?> { ["maxLength"] = 64 });
        yield return FieldSeed.FreeText("fleetNumber", "Fleet Number", "Fleet number", sectionKey: "identity", validation: new Dictionary<string, object?> { ["maxLength"] = 64 });
    }

    private static IReadOnlyList<DependencySeed> BuildDependencySeeds()
    {
        var rows = new List<DependencySeed>();

        var assetTypeToClass = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["pickup"] = "vehicle",
            ["service_truck"] = "vehicle",
            ["cargo_van"] = "vehicle",
            ["box_truck"] = "vehicle",
            ["semi_tractor"] = "vehicle",
            ["yard_truck"] = "vehicle",
            ["dry_van_trailer"] = "trailer",
            ["reefer_trailer"] = "trailer",
            ["flatbed_trailer"] = "trailer",
            ["tanker_trailer"] = "trailer",
            ["dump_trailer"] = "trailer",
            ["lowboy_trailer"] = "trailer",
            ["chassis_trailer"] = "trailer",
            ["dolly"] = "trailer",
            ["forklift"] = "powered_industrial_truck",
            ["pallet_jack"] = "powered_industrial_truck",
            ["scissor_lift"] = "heavy_equipment",
            ["boom_lift"] = "heavy_equipment",
            ["skid_steer"] = "heavy_equipment",
            ["loader"] = "heavy_equipment",
            ["excavator"] = "heavy_equipment",
            ["dozer"] = "heavy_equipment",
            ["conveyor"] = "production_equipment",
            ["compressor"] = "production_equipment",
            ["generator"] = "facility_equipment",
            ["pump"] = "facility_equipment",
            ["scale"] = "facility_equipment",
            ["welding_machine"] = "tool",
            ["custom"] = "component",
        };

        foreach (var pair in assetTypeToClass)
        {
            rows.Add(new DependencySeed("assetType", pair.Key, "assetClass", pair.Value));
        }

        var vehicleMakes = new[] { "ford", "chevrolet", "gmc", "ram", "freightliner", "international", "kenworth", "peterbilt", "volvo", "mack", "western_star", "isuzu", "hino", "toyota" };
        var trailerMakes = new[] { "great_dane", "wabash", "utility", "hyundai_translead", "stoughton", "vanguard", "fontaine", "reitnouer", "mac_trailer", "wilson", "east", "heil", "polar" };
        var equipmentMakes = new[] { "toyota_material_handling", "hyster", "yale", "crown", "raymond", "mitsubishi_logisnext", "clark", "caterpillar", "john_deere", "komatsu", "jlg", "genie", "skyjack", "bobcat", "case", "kubota" };

        foreach (var make in vehicleMakes)
        {
            rows.Add(new DependencySeed("make", make, "assetClass", "vehicle"));
        }
        foreach (var make in trailerMakes)
        {
            rows.Add(new DependencySeed("make", make, "assetClass", "trailer"));
        }
        foreach (var make in equipmentMakes)
        {
            rows.Add(new DependencySeed("make", make, "assetClass", "heavy_equipment"));
        }

        var modelToMake = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["f_150"] = "ford",
            ["f_250"] = "ford",
            ["f_350"] = "ford",
            ["silverado_1500"] = "chevrolet",
            ["silverado_2500hd"] = "chevrolet",
            ["silverado_3500hd"] = "chevrolet",
            ["sierra_1500"] = "gmc",
            ["sierra_2500hd"] = "gmc",
            ["sierra_3500hd"] = "gmc",
            ["ram_1500"] = "ram",
            ["ram_2500"] = "ram",
            ["ram_3500"] = "ram",
            ["cascadia"] = "freightliner",
            ["m2"] = "freightliner",
            ["108sd"] = "freightliner",
            ["114sd"] = "freightliner",
            ["lt_series"] = "international",
            ["rh_series"] = "international",
            ["mv_series"] = "international",
            ["t680"] = "kenworth",
            ["t880"] = "kenworth",
            ["579"] = "peterbilt",
            ["vnl"] = "volvo",
            ["anthem"] = "mack",
            ["pinnacle"] = "mack",
            ["n_series"] = "isuzu",
        };

        foreach (var pair in modelToMake)
        {
            rows.Add(new DependencySeed("model", pair.Key, "make", pair.Value));
        }

        return rows;
    }

    private static string NormalizeOptionKey(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        var normalized = trimmed
            .Replace("&", "and", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .Replace("-", "_", StringComparison.Ordinal)
            .Replace(".", "_", StringComparison.Ordinal)
            .Replace(" ", "_", StringComparison.Ordinal);

        while (normalized.Contains("__", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("__", "_", StringComparison.Ordinal);
        }

        return normalized.Trim('_').ToLowerInvariant();
    }

    private static string HumanizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Replace('_', ' ').Trim();
        return string.Join(' ', normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(token =>
            {
                if (token.Length <= 3 && token.All(char.IsLetter))
                {
                    return token.ToUpperInvariant();
                }

                return char.ToUpperInvariant(token[0]) + token[1..];
            }));
    }

    private sealed record CatalogSeed(
        string Key,
        string Label,
        IReadOnlyList<string> Options,
        bool IsTenantExtendable = true,
        string Description = "")
    {
        public string Description { get; } = string.IsNullOrWhiteSpace(Description) ? Label : Description;
    }

    private sealed record DependencySeed(
        string CatalogKey,
        string OptionKey,
        string DependsOnCatalogKey,
        string DependsOnOptionKey);

    private sealed record FieldSeed(
        string Key,
        string Label,
        string Description,
        string DataType,
        string ControlType,
        bool Required,
        string? CatalogKey,
        string? ReferenceKey,
        string SourceType,
        string SourceOfTruth,
        string SectionKey,
        IReadOnlyDictionary<string, string> DependsOn,
        IReadOnlyDictionary<string, object?> Validation,
        string DefaultValueJson,
        IReadOnlyDictionary<string, object?> Visibility,
        bool AllowCustom,
        bool CustomRequiresApproval,
        bool DrivesLogic,
        bool DrivesInspectionBranching,
        bool DrivesPMApplicability,
        bool DrivesCompliance,
        bool DrivesReporting,
        bool DrivesReadiness)
    {
        public static FieldSeed Catalog(
            string key,
            string label,
            string description,
            bool required,
            string catalogKey,
            string sectionKey,
            bool drivesLogic,
            IReadOnlyDictionary<string, string>? dependsOn = null,
            IReadOnlyDictionary<string, object?>? validation = null,
            string defaultValueJson = "null",
            bool allowCustom = false,
            bool customRequiresApproval = false) =>
            new(
                key,
                label,
                description,
                "string",
                "select",
                required,
                catalogKey,
                null,
                "maintainarr_catalog",
                "MaintainArr",
                sectionKey,
                dependsOn ?? new Dictionary<string, string>(),
                validation ?? new Dictionary<string, object?>(),
                defaultValueJson,
                new Dictionary<string, object?>(),
                allowCustom,
                customRequiresApproval,
                drivesLogic,
                drivesLogic,
                drivesLogic,
                false,
                drivesLogic,
                drivesLogic);

        public static FieldSeed Reference(
            string key,
            string label,
            string description,
            bool required,
            string referenceKey,
            string sourceType,
            string sourceOfTruth,
            string sectionKey,
            bool drivesLogic,
            bool multi = false,
            bool drivesCompliance = false) =>
            new(
                key,
                label,
                description,
                "string",
                multi ? "multiSelect" : "asyncCombobox",
                required,
                null,
                referenceKey,
                sourceType,
                sourceOfTruth,
                sectionKey,
                new Dictionary<string, string>(),
                new Dictionary<string, object?>(),
                "null",
                new Dictionary<string, object?>(),
                false,
                false,
                drivesLogic,
                drivesLogic,
                drivesLogic,
                drivesCompliance,
                drivesLogic,
                drivesLogic);

        public static FieldSeed FreeText(
            string key,
            string label,
            string description,
            string sectionKey,
            IReadOnlyDictionary<string, object?>? validation = null) =>
            new(
                key,
                label,
                description,
                "string",
                "text",
                false,
                null,
                null,
                "maintainarr_record",
                "MaintainArr",
                sectionKey,
                new Dictionary<string, string>(),
                validation ?? new Dictionary<string, object?>(),
                "null",
                new Dictionary<string, object?>(),
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false);
    }
}
