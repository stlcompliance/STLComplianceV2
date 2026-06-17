using STLCompliance.Shared.Operations;

namespace STLCompliance.Dr.Tests;

[Trait("Category", "Dr")]
public sealed class StlProductDatabaseCatalogTests
{
    [Fact]
    public void All_includes_implemented_product_databases()
    {
        Assert.Equal(13, StlProductDatabaseCatalog.All.Count);
        Assert.Contains(StlProductDatabaseCatalog.NexArr, StlProductDatabaseCatalog.All);
        Assert.Contains(StlProductDatabaseCatalog.CustomArr, StlProductDatabaseCatalog.All);
        Assert.Contains(StlProductDatabaseCatalog.OrdArr, StlProductDatabaseCatalog.All);
        Assert.Contains(StlProductDatabaseCatalog.ComplianceCore, StlProductDatabaseCatalog.All);
        Assert.Contains(StlProductDatabaseCatalog.LoadArr, StlProductDatabaseCatalog.All);
        Assert.Contains(StlProductDatabaseCatalog.RecordArr, StlProductDatabaseCatalog.All);
        Assert.Contains(StlProductDatabaseCatalog.ReportArr, StlProductDatabaseCatalog.All);
        Assert.Contains(StlProductDatabaseCatalog.AssurArr, StlProductDatabaseCatalog.All);
    }

    [Theory]
    [InlineData("nexarr", true)]
    [InlineData("COMPLIANCECORE", true)]
    [InlineData("loadarr", true)]
    [InlineData("recordarr", true)]
    [InlineData("reportarr", true)]
    [InlineData("assurarr", true)]
    [InlineData("customarr", true)]
    [InlineData("ordarr", true)]
    [InlineData("unknown", false)]
    public void IsKnownProductDatabase_matches_catalog(string database, bool expected)
    {
        Assert.Equal(expected, StlProductDatabaseCatalog.IsKnownProductDatabase(database));
    }
}

[Trait("Category", "Dr")]
public sealed class StlDrRestoreDrillSupportTests
{
    [Fact]
    public void GetDrillDatabaseName_appends_suffix()
    {
        var name = StlDrRestoreDrillSupport.GetDrillDatabaseName(
            StlProductDatabaseCatalog.NexArr,
            StlDrRestoreDrillSupport.DefaultDrillSuffix);

        Assert.Equal("nexarr_dr_restore_drill", name);
    }

    [Fact]
    public void ResolveBackupPath_prefers_custom_format()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllText(Path.Combine(directory, "nexarr.sql"), "-- sql");
            File.WriteAllText(Path.Combine(directory, "nexarr.custom"), "custom");

            var resolved = StlDrRestoreDrillSupport.ResolveBackupPath(directory, "nexarr");
            Assert.EndsWith("nexarr.custom", resolved, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ResolveBackupFiles_throws_when_directory_missing()
    {
        var plan = new StlDrRestoreDrillPlan(
            "localhost",
            5432,
            "stl",
            "secret",
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        Assert.Throws<DirectoryNotFoundException>(() => StlDrRestoreDrillSupport.ResolveBackupFiles(plan));
    }

    [Fact]
    public void ResolveBackupFiles_returns_all_product_backups()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            foreach (var database in StlProductDatabaseCatalog.All)
            {
                File.WriteAllText(Path.Combine(directory, $"{database}.dump"), "dump");
            }

            var plan = new StlDrRestoreDrillPlan("localhost", 5432, "stl", "secret", directory);
            var resolved = StlDrRestoreDrillSupport.ResolveBackupFiles(plan);
            Assert.Equal(StlProductDatabaseCatalog.All.Count, resolved.Count);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
