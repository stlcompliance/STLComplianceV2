using NexArr.Api.Services;
using STLCompliance.Shared.Operations;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class ProductDatabaseNukeTableClassifierTests
{
    [Theory]
    [InlineData("supplyarr", "supplyarr_purchase_orders")]
    [InlineData("customarr", "customarr_customers")]
    [InlineData("ordarr", "ordarr_orders")]
    public void Classify_truncates_product_data_tables(string productDatabase, string table)
    {
        var result = ProductDatabaseNukeTableClassifier.Classify(productDatabase, "public", table);

        Assert.Equal(ProductDatabaseNukeTableDispositions.Truncate, result.Disposition);
        Assert.False(result.Preserve);
    }

    [Theory]
    [InlineData("nexarr", "reference_entities", "platform reference data")]
    [InlineData("nexarr", "platform_users", "NexArr platform control plane")]
    [InlineData("compliancecore", "compliancecore_rule_packs", "Compliance Core reference data")]
    [InlineData("maintainarr", "maintainarr_asset_types", "MaintainArr reference data")]
    [InlineData("maintainarr", "maintainarr_catalogs", "MaintainArr reference data")]
    [InlineData("routarr", "routarr_vehicle_refs", "RoutArr reference mirror data")]
    [InlineData("supplyarr", "supplyarr_part_catalogs", "SupplyArr reference data")]
    [InlineData("trainarr", "trainarr_training_program_definitions", "TrainArr training reference data")]
    public void Classify_preserves_reference_and_control_plane_tables(
        string productDatabase,
        string table,
        string expectedReason)
    {
        var result = ProductDatabaseNukeTableClassifier.Classify(productDatabase, "public", table);

        Assert.Equal(ProductDatabaseNukeTableDispositions.Preserve, result.Disposition);
        Assert.True(result.Preserve);
        Assert.Equal(expectedReason, result.Reason);
    }

    [Fact]
    public void Classify_preserves_audit_tables()
    {
        var result = ProductDatabaseNukeTableClassifier.Classify(
            StlProductDatabaseCatalog.SupplyArr,
            "public",
            "supplyarr_external_provider_audit_log_entries");

        Assert.Equal(ProductDatabaseNukeTableDispositions.Preserve, result.Disposition);
        Assert.Equal("audit trail", result.Reason);
    }

    [Fact]
    public void Classify_does_not_preserve_domain_tables_named_audits()
    {
        var result = ProductDatabaseNukeTableClassifier.Classify(
            StlProductDatabaseCatalog.AssurArr,
            "public",
            "assurarr_quality_audits");

        Assert.Equal(ProductDatabaseNukeTableDispositions.Truncate, result.Disposition);
    }

    [Fact]
    public void Classify_preserves_schema_infrastructure()
    {
        var result = ProductDatabaseNukeTableClassifier.Classify(
            StlProductDatabaseCatalog.StaffArr,
            "public",
            "__EFMigrationsHistory");

        Assert.Equal(ProductDatabaseNukeTableDispositions.Preserve, result.Disposition);
        Assert.Equal("schema infrastructure", result.Reason);
    }
}
