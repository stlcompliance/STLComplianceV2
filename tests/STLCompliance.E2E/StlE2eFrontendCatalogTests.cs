using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E;

[Trait("Category", "E2e")]
public sealed class StlE2eFrontendCatalogTests
{
    [Fact]
    public void Product_handoff_frontends_cover_arr_products_excluding_nexarr()
    {
        var keys = StlE2eFrontendCatalog.ProductHandoffFrontends
            .Select(e => e.ProductKey)
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            ["assurarr", "compliancecore", "customarr", "loadarr", "maintainarr", "recordarr", "reportarr", "routarr", "staffarr", "supplyarr", "trainarr"],
            keys);
    }

    [Theory]
    [InlineData("staffarr", 5175)]
    [InlineData("trainarr", 5176)]
    [InlineData("compliancecore", 5177)]
    [InlineData("maintainarr", 5178)]
    [InlineData("supplyarr", 5179)]
    [InlineData("customarr", 5186)]
    [InlineData("routarr", 5180)]
    [InlineData("loadarr", 5182)]
    [InlineData("assurarr", 5183)]
    [InlineData("recordarr", 5184)]
    [InlineData("reportarr", 5185)]
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

    [Fact]
    public void Playwright_catalog_includes_tenant_chrome_handoff_spec()
    {
        Assert.Equal(
            StlE2ePlaywrightSpecCatalog.ProductHandoffTenantChromeSpec,
            StlE2eFrontendCatalog.PlaywrightTenantChromeHandoffSpec);
    }

    [Fact]
    public void All_frontends_include_fieldcompanion_field_inbox_app()
    {
        Assert.Contains(
            StlE2eFrontendCatalog.All,
            endpoint => endpoint.ProductKey == "fieldcompanion" && endpoint.Port == 5181);
    }
}
