using STLCompliance.Shared.Operations;

namespace STLCompliance.Dr.Tests;

[Trait("Category", "Dr")]
public sealed class StlRenderStagingDrillCatalogTests
{
    [Fact]
    public void All_maps_seven_render_database_services()
    {
        Assert.Equal(7, StlRenderStagingDrillCatalog.All.Count);
        Assert.Contains(
            StlRenderStagingDrillCatalog.All,
            entry => entry.RenderDatabaseServiceName == "nexarr-db"
                && entry.ProductDatabase == StlProductDatabaseCatalog.NexArr);
    }
}

[Trait("Category", "Dr")]
public sealed class StlRenderStagingDrillSupportTests
{
    [Fact]
    public void ParseDatabaseUrl_parses_render_external_uri()
    {
        const string url = "postgresql://nexarr:secret%21@dpg-example-a.oregon-postgres.render.com:5432/nexarr";

        var target = StlRenderStagingDrillSupport.ParseDatabaseUrl(StlProductDatabaseCatalog.NexArr, url);

        Assert.Equal("nexarr", target.ProductDatabase);
        Assert.Equal("dpg-example-a.oregon-postgres.render.com", target.Host);
        Assert.Equal(5432, target.Port);
        Assert.Equal("nexarr", target.Username);
        Assert.Equal("secret!", target.Password);
        Assert.Equal("nexarr", target.Database);
    }

    [Fact]
    public void ResolveTargetsFromEnvironment_reads_catalog_variables()
    {
        Environment.SetEnvironmentVariable("RENDER_STAGING_NEXARR_DATABASE_URL", "postgresql://nexarr:pw@host.example.com:5432/nexarr");
        Environment.SetEnvironmentVariable("RENDER_STAGING_STAFFARR_DATABASE_URL", "postgresql://staffarr:pw@host.example.com:5432/staffarr");
        Environment.SetEnvironmentVariable("RENDER_STAGING_TRAINARR_DATABASE_URL", "postgresql://trainarr:pw@host.example.com:5432/trainarr");
        Environment.SetEnvironmentVariable("RENDER_STAGING_MAINTAINARR_DATABASE_URL", "postgresql://maintainarr:pw@host.example.com:5432/maintainarr");
        Environment.SetEnvironmentVariable("RENDER_STAGING_ROUTARR_DATABASE_URL", "postgresql://routarr:pw@host.example.com:5432/routarr");
        Environment.SetEnvironmentVariable("RENDER_STAGING_SUPPLYARR_DATABASE_URL", "postgresql://supplyarr:pw@host.example.com:5432/supplyarr");
        Environment.SetEnvironmentVariable("RENDER_STAGING_COMPLIANCECORE_DATABASE_URL", "postgresql://compliancecore:pw@host.example.com:5432/compliancecore");

        try
        {
            var targets = StlRenderStagingDrillSupport.ResolveTargetsFromEnvironment();
            Assert.Equal(7, targets.Count);
        }
        finally
        {
            foreach (var entry in StlRenderStagingDrillCatalog.All)
            {
                Environment.SetEnvironmentVariable(entry.SourceDatabaseUrlEnvironmentVariable, null);
            }
        }
    }

    [Fact]
    public void ResolveTargetsFromEnvironment_throws_when_url_missing()
    {
        foreach (var entry in StlRenderStagingDrillCatalog.All)
        {
            Environment.SetEnvironmentVariable(entry.SourceDatabaseUrlEnvironmentVariable, null);
        }

        var exception = Assert.Throws<InvalidOperationException>(() =>
            StlRenderStagingDrillSupport.ResolveTargetsFromEnvironment([StlProductDatabaseCatalog.NexArr]));

        Assert.Contains("RENDER_STAGING_NEXARR_DATABASE_URL", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateBackupDirectory_reports_missing_files()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllText(Path.Combine(directory, "nexarr.custom"), "backup");
            var target = StlRenderStagingDrillSupport.ParseDatabaseUrl(
                StlProductDatabaseCatalog.NexArr,
                "postgresql://nexarr:pw@host.example.com:5432/nexarr");
            var plan = StlRenderStagingDrillPlan.FromTargets(directory, [target]);

            var missing = StlRenderStagingDrillSupport.ValidateBackupDirectory(plan);

            Assert.Empty(missing);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ToDrRestoreDrillPlan_limits_to_single_database()
    {
        var target = StlRenderStagingDrillSupport.ParseDatabaseUrl(
            StlProductDatabaseCatalog.StaffArr,
            "postgresql://staffarr:pw@host.example.com:5432/staffarr");
        var plan = target.ToDrRestoreDrillPlan(@"C:\backups", StlDrRestoreDrillSupport.DefaultDrillSuffix);

        Assert.Single(plan.TargetDatabases);
        Assert.Equal(StlProductDatabaseCatalog.StaffArr, plan.TargetDatabases[0]);
        Assert.Equal(@"C:\backups", plan.BackupDirectory);
    }
}
