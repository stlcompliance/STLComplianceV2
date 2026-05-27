using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E;

[Trait("Category", "E2e")]
public sealed class StlE2eFrontendCatalogTests
{
    [Fact]
    public void Product_handoff_frontends_cover_six_arr_products_excluding_nexarr()
    {
        var keys = StlE2eFrontendCatalog.ProductHandoffFrontends
            .Select(e => e.ProductKey)
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["compliancecore", "maintainarr", "routarr", "staffarr", "supplyarr", "trainarr"],
            keys);
    }

    [Theory]
    [InlineData("staffarr", 5175)]
    [InlineData("trainarr", 5176)]
    [InlineData("compliancecore", 5177)]
    [InlineData("maintainarr", 5178)]
    [InlineData("supplyarr", 5179)]
    [InlineData("routarr", 5180)]
    public void Handoff_frontend_ports_match_vite_defaults(string productKey, int expectedPort)
    {
        var endpoint = StlE2eFrontendCatalog.TryGetHandoffFrontend(productKey);
        Assert.NotNull(endpoint);
        Assert.Equal(expectedPort, endpoint.Port);
        Assert.Contains($":{expectedPort}", endpoint.DefaultBaseUrl, StringComparison.Ordinal);
    }

    [Fact]
    public void Suite_frontend_uses_port_5174()
    {
        Assert.Equal(5174, StlE2eFrontendCatalog.SuitePort);
        Assert.Equal("http://localhost:5174", StlE2eFrontendCatalog.SuiteDefaultBaseUrl);
    }
}
