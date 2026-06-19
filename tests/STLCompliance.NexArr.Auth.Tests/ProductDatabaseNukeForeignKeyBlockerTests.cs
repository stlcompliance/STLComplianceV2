using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public sealed class ProductDatabaseNukeForeignKeyBlockerTests
{
    [Fact]
    public void FindBlockers_flags_preserved_tables_that_reference_truncatable_tables()
    {
        var tables = new[]
        {
            new DatabaseNukeTablePreviewResponse("public", "maintainarr_assets", ProductDatabaseNukeTableDispositions.Truncate, "product data", 10),
            new DatabaseNukeTablePreviewResponse("public", "maintainarr_asset_history", ProductDatabaseNukeTableDispositions.Preserve, "audit trail", 2),
        };
        var foreignKeys = new[]
        {
            new ProductDatabaseNukeForeignKeyReference("public", "maintainarr_asset_history", "public", "maintainarr_assets"),
        };

        var blockers = ProductDatabaseNukeForeignKeyBlockerFinder.FindBlockers(tables, foreignKeys);

        Assert.Single(blockers);
        var blocker = blockers[0];
        Assert.Equal("public", blocker.ParentSchema);
        Assert.Equal("maintainarr_assets", blocker.ParentTable);
        Assert.Single(blocker.ReferencingTables);
        Assert.Equal("maintainarr_asset_history", blocker.ReferencingTables[0].ChildTable);
        Assert.Contains(
            "foreign key blockers prevent truncation",
            ProductDatabaseNukeForeignKeyBlockerFinder.BuildBlockerMessage("maintainarr", blockers),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FindBlockers_ignores_references_from_tables_that_are_also_truncated()
    {
        var tables = new[]
        {
            new DatabaseNukeTablePreviewResponse("public", "maintainarr_assets", ProductDatabaseNukeTableDispositions.Truncate, "product data", 10),
            new DatabaseNukeTablePreviewResponse("public", "maintainarr_asset_tags", ProductDatabaseNukeTableDispositions.Truncate, "product data", 4),
        };
        var foreignKeys = new[]
        {
            new ProductDatabaseNukeForeignKeyReference("public", "maintainarr_asset_tags", "public", "maintainarr_assets"),
        };

        var blockers = ProductDatabaseNukeForeignKeyBlockerFinder.FindBlockers(tables, foreignKeys);

        Assert.Empty(blockers);
    }
}
