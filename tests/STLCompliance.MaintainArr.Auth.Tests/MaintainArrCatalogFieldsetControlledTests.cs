using Microsoft.EntityFrameworkCore;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrCatalogFieldsetControlledTests
{
    private const string ActorPersonId = "person_1001";

    [Fact]
    public async Task Seeded_catalogs_include_required_controlled_groups()
    {
        await using var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var seed = new CatalogSeedService(db);
        await seed.EnsureSeededForTenantAsync(tenantId);

        var service = BuildCatalogService(db, seed);
        var catalogs = await service.ListAsync(
            tenantId,
            ["assetClass", "assetType", "fuelType", "brakeType", "telematicsProvider"],
            CancellationToken.None);

        Assert.Equal(5, catalogs.Count);
        Assert.Contains(catalogs, x => x.Key == "assetClass");
        Assert.Contains(catalogs, x => x.Key == "assetType");
        Assert.Contains(catalogs, x => x.Key == "fuelType");
        Assert.Contains(catalogs, x => x.Key == "brakeType");
        Assert.Contains(catalogs, x => x.Key == "telematicsProvider");
    }

    [Fact]
    public async Task Fieldset_metadata_contains_source_and_drives_flags_for_regulatory_reference()
    {
        await using var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var seed = new CatalogSeedService(db);
        await seed.EnsureSeededForTenantAsync(tenantId);

        var catalogService = BuildCatalogService(db, seed);
        var fieldsetService = BuildFieldsetService(db, catalogService, seed);
        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "create", CancellationToken.None);
        var governingBody = Assert.Single(fieldset.Fields, x => x.Key == "governingBodyKey");

        Assert.Equal("compliancecore_reference", governingBody.Source);
        Assert.Equal("Compliance Core", governingBody.SourceOfTruth);
        Assert.Equal("stable_key", governingBody.StoredValue);
        Assert.Equal("mirrored_label", governingBody.DisplayValue);
        Assert.Equal("compliance", governingBody.SectionKey);
        Assert.NotNull(governingBody.Options);
        Assert.Contains(governingBody.Options!, x => x.Key == "fmcsa");
        Assert.True(governingBody.DrivesLogic);
        Assert.True(governingBody.DrivesInspectionBranching);
        Assert.True(governingBody.DrivesPMApplicability);
        Assert.True(governingBody.DrivesCompliance);
        Assert.True(governingBody.DrivesReporting);
        Assert.True(governingBody.DrivesReadiness);
    }

    [Fact]
    public async Task Controlled_validation_rejects_dependency_mismatch_for_asset_type()
    {
        await using var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var seed = new CatalogSeedService(db);
        await seed.EnsureSeededForTenantAsync(tenantId);

        var catalogService = BuildCatalogService(db, seed);
        var fieldsetService = BuildFieldsetService(db, catalogService, seed);
        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "create", CancellationToken.None);
        var pending = new PendingCatalogValueService(db);
        var validator = BuildValidator(db, catalogService, pending);

        var values = new Dictionary<string, object?>
        {
            ["assetClass"] = "trailer",
            ["assetType"] = "pickup",
            ["assetStatus"] = "active",
            ["lifecycleStatus"] = "in_service",
            ["criticality"] = "medium",
        };

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            validator.ValidateFieldsetValuesAsync(
                tenantId,
                fieldset.Fields,
                values,
                ActorPersonId,
                "asset",
                "new",
                createPendingValues: false,
                CancellationToken.None));

        Assert.Equal("assets.validation", ex.Code);
        Assert.Contains("Expected parent 'assetClass' = 'vehicle'", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Controlled_validation_rejects_dependency_mismatch_for_model_make()
    {
        await using var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var seed = new CatalogSeedService(db);
        await seed.EnsureSeededForTenantAsync(tenantId);

        var catalogService = BuildCatalogService(db, seed);
        var fieldsetService = BuildFieldsetService(db, catalogService, seed);
        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "create", CancellationToken.None);
        var pending = new PendingCatalogValueService(db);
        var validator = BuildValidator(db, catalogService, pending);

        var values = BaseRequiredValues();
        values["assetClass"] = "vehicle";
        values["assetType"] = "semi_tractor";
        values["make"] = "ford";
        values["model"] = "cascadia";

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            validator.ValidateFieldsetValuesAsync(
                tenantId,
                fieldset.Fields,
                values,
                ActorPersonId,
                "asset",
                "new",
                createPendingValues: false,
                CancellationToken.None));

        Assert.Equal("assets.validation", ex.Code);
        Assert.Contains("Expected parent 'make' = 'freightliner'", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Custom_model_value_creates_pending_catalog_value_when_approval_required()
    {
        await using var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var seed = new CatalogSeedService(db);
        await seed.EnsureSeededForTenantAsync(tenantId);

        var catalogService = BuildCatalogService(db, seed);
        var fieldsetService = BuildFieldsetService(db, catalogService, seed);
        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "create", CancellationToken.None);
        var pending = new PendingCatalogValueService(db);
        var validator = BuildValidator(db, catalogService, pending);

        var values = BaseRequiredValues();
        values["assetClass"] = "vehicle";
        values["assetType"] = "pickup";
        values["make"] = "ford";
        values["model"] = "unlisted_lab_variant";

        await validator.ValidateFieldsetValuesAsync(
            tenantId,
            fieldset.Fields,
            values,
            ActorPersonId,
            "asset",
            "new",
            createPendingValues: true,
            CancellationToken.None);

        var pendingValue = await db.PendingCatalogValues.SingleAsync();
        Assert.Equal("model", pendingValue.CatalogKey);
        Assert.Equal("unlisted_lab_variant", pendingValue.ProposedKey);
        Assert.Equal("unlisted_lab_variant", pendingValue.ProposedLabel);
        Assert.Equal(ActorPersonId, pendingValue.ProposedByPersonId);
        Assert.Equal("pending", pendingValue.Status);
    }

    [Fact]
    public async Task Custom_value_is_rejected_when_custom_not_allowed()
    {
        await using var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var seed = new CatalogSeedService(db);
        await seed.EnsureSeededForTenantAsync(tenantId);

        var catalogService = BuildCatalogService(db, seed);
        var fieldsetService = BuildFieldsetService(db, catalogService, seed);
        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "create", CancellationToken.None);
        var pending = new PendingCatalogValueService(db);
        var validator = BuildValidator(db, catalogService, pending);

        var values = BaseRequiredValues();
        values["assetClass"] = "vehicle";
        values["assetType"] = "pickup";
        values["fuelType"] = "mystery_fuel";

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            validator.ValidateFieldsetValuesAsync(
                tenantId,
                fieldset.Fields,
                values,
                ActorPersonId,
                "asset",
                "new",
                createPendingValues: true,
                CancellationToken.None));

        Assert.Equal("assets.validation", ex.Code);
        Assert.Contains("fuelType", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(db.PendingCatalogValues);
    }

    [Fact]
    public async Task External_reference_is_rejected_when_not_in_reference_cache()
    {
        await using var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var seed = new CatalogSeedService(db);
        await seed.EnsureSeededForTenantAsync(tenantId);

        var catalogService = BuildCatalogService(db, seed);
        var fieldsetService = BuildFieldsetService(db, catalogService, seed);
        var fieldset = await fieldsetService.GetAssetsFieldsetAsync(tenantId, "create", CancellationToken.None);
        var pending = new PendingCatalogValueService(db);
        var validator = BuildValidator(db, catalogService, pending);

        var values = BaseRequiredValues();
        values["assetClass"] = "vehicle";
        values["assetType"] = "pickup";
        values["siteId"] = "site_not_seeded";

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            validator.ValidateFieldsetValuesAsync(
                tenantId,
                fieldset.Fields,
                values,
                ActorPersonId,
                "asset",
                "new",
                createPendingValues: true,
                CancellationToken.None));

        Assert.Equal("assets.validation", ex.Code);
        Assert.Contains("siteId", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("StaffArr", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Compliance_core_reference_key_with_wrong_source_raises_ownership_violation()
    {
        await using var db = CreateDbContext();
        var tenantId = Guid.NewGuid();
        var seed = new CatalogSeedService(db);
        await seed.EnsureSeededForTenantAsync(tenantId);

        var catalogService = BuildCatalogService(db, seed);
        var pending = new PendingCatalogValueService(db);
        var validator = BuildValidator(db, catalogService, pending);

        var invalidReferenceField = new FieldMetadataResponse(
            "governingBodyOverride",
            "Governing Body Override",
            "Invalid ownership test field",
            "string",
            "select",
            false,
            null,
            "governingBody",
            "maintainarr_catalog",
            "MaintainArr",
            "catalog_key",
            "catalog_label",
            false,
            false,
            true,
            true,
            true,
            true,
            true,
            true,
            null,
            null,
            null,
            null,
            "compliance",
            null);

        var ex = await Assert.ThrowsAsync<StlApiException>(() =>
            validator.ValidateFieldsetValuesAsync(
                tenantId,
                [invalidReferenceField],
                new Dictionary<string, object?> { ["governingBodyOverride"] = "fmcsa" },
                ActorPersonId,
                "asset",
                "new",
                createPendingValues: true,
                CancellationToken.None));

        Assert.Equal("references.ownership_violation", ex.Code);
    }

    private static MaintainArrDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MaintainArrDbContext>()
            .UseInMemoryDatabase($"maintainarr-controlled-tests-{Guid.NewGuid():N}")
            .Options;
        return new MaintainArrDbContext(options);
    }

    private static CatalogService BuildCatalogService(MaintainArrDbContext db, CatalogSeedService seed) =>
        new(db, seed);

    private static FieldsetService BuildFieldsetService(MaintainArrDbContext db, CatalogService catalogService, CatalogSeedService seed)
    {
        var adapters = BuildAdapters(db);
        return new FieldsetService(db, catalogService, seed, adapters);
    }

    private static ControlledValueValidationService BuildValidator(
        MaintainArrDbContext db,
        CatalogService catalogService,
        PendingCatalogValueService pending)
    {
        var adapters = BuildAdapters(db);
        return new ControlledValueValidationService(catalogService, pending, adapters);
    }

    private static IReadOnlyList<IExternalReferenceAdapter> BuildAdapters(MaintainArrDbContext db)
    {
        return
        [
            new ComplianceCoreReferenceAdapter(db),
            new StaffArrReferenceAdapter(db),
            new SupplyArrReferenceAdapter(db),
        ];
    }

    private static Dictionary<string, object?> BaseRequiredValues()
    {
        return new Dictionary<string, object?>
        {
            ["assetStatus"] = "active",
            ["lifecycleStatus"] = "in_service",
            ["criticality"] = "medium",
        };
    }
}
