using STLCompliance.OpenApi.Tests.Support;
using STLCompliance.Shared.Operations;

namespace STLCompliance.OpenApi.Tests.Catalog;

[Trait("Category", "OpenApi")]
[Trait("Area", "ShipGate")]
public sealed class ShipGateOpenApiCatalogTests
{
    [Fact]
    public void Every_ship_gate_product_has_checked_in_openapi_snapshot()
    {
        foreach (var productKey in StlM13ShipGateCatalog.OpenApiProductKeys)
        {
            var path = OpenApiSnapshotHelper.RepoSnapshotPath(productKey);
            Assert.True(File.Exists(path), $"Missing OpenAPI snapshot for '{productKey}' at {path}.");
        }
    }

    [Fact]
    public void OpenApi_product_keys_are_unique_and_lowercase()
    {
        var keys = StlM13ShipGateCatalog.OpenApiProductKeys;
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
        Assert.All(keys, key => Assert.Equal(key, key.ToLowerInvariant()));
    }
}
