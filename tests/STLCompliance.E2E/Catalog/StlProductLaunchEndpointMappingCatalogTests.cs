namespace STLCompliance.E2E.Catalog;

[Trait("Category", "E2e")]
[Trait("Area", "ShipGate")]
public sealed class StlProductLaunchEndpointMappingCatalogTests
{
    [Theory]
    [InlineData("apps/staffarr-api/StaffArr.Api/Program.cs")]
    [InlineData("apps/trainarr-api/TrainArr.Api/Program.cs")]
    [InlineData("apps/maintainarr-api/MaintainArr.Api/Program.cs")]
    [InlineData("apps/routarr-api/RoutArr.Api/Program.cs")]
    [InlineData("apps/supplyarr-api/SupplyArr.Api/Program.cs")]
    [InlineData("apps/compliancecore-api/ComplianceCore.Api/Program.cs")]
    [InlineData("apps/loadarr-api/LoadArr.Api/Program.cs")]
    [InlineData("apps/assurarr-api/AssurArr.Api/Program.cs")]
    public void Product_apis_map_shared_launch_endpoints(string programPath)
    {
        var source = File.ReadAllText(Path.Combine(FindRepoRoot(), programPath));

        Assert.Contains("MapStlProductLaunchEndpoints", source);
    }

    [Fact]
    public void Shared_launch_endpoint_maps_v1_catalog_context_and_handoff_routes()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "packages/shared-dotnet/STLCompliance.Shared/Endpoints/StlProductLaunchEndpoints.cs"));

        Assert.Contains("/api/v1/launch", source);
        Assert.Contains("MapGet(\"/catalog\"", source);
        Assert.Contains("MapGet(\"/context\"", source);
        Assert.Contains("MapPost(\"/handoff\"", source);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Directory.Build.props")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
