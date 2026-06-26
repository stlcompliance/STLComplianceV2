using System.Reflection;
using STLCompliance.E2E.Flows;
using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E.Catalog;

[Trait("Category", "E2e")]
[Trait("Area", "ShipGate")]
public sealed class StlM13ShipGateCatalogTests
{
    [Fact]
    public void OpenApi_product_keys_match_snapshot_count()
    {
        Assert.Equal(11, StlM13ShipGateCatalog.OpenApiProductKeys.Count);
        Assert.Contains("loadarr", StlM13ShipGateCatalog.OpenApiProductKeys);
        Assert.Contains("reportarr", StlM13ShipGateCatalog.OpenApiProductKeys);
        Assert.Contains("assurarr", StlM13ShipGateCatalog.OpenApiProductKeys);
    }

    [Fact]
    public void Ordinary_product_access_probes_cover_supported_product_me_surfaces()
    {
        var keys = StlM13ShipGateCatalog.ProductApiOrdinaryAccessProbes
            .Select(p => p.ProductKey)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var productKey in new[] { "staffarr", "trainarr", "maintainarr", "routarr", "supplyarr", "reportarr" })
        {
            Assert.Contains(productKey, keys);
        }
    }

    [Fact]
    public void Tenant_isolation_integration_tests_meet_ship_gate_minimum()
    {
        var factCount = typeof(TenantIsolationFlowTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Count(m => m.GetCustomAttribute<FactAttribute>() is not null);

        Assert.True(
            factCount >= StlM13ShipGateCatalog.MinimumTenantIsolationIntegrationTests,
            $"Expected at least {StlM13ShipGateCatalog.MinimumTenantIsolationIntegrationTests} tenant isolation facts, found {factCount}.");
    }

    [Fact]
    public void Access_model_integration_tests_meet_ship_gate_minimum()
    {
        var theoryCount = typeof(LaunchContextAccessFlowTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Count(m => m.GetCustomAttribute<TheoryAttribute>() is not null);

        var factCount = typeof(LaunchContextAccessFlowTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Count(m => m.GetCustomAttribute<FactAttribute>() is not null);

        var probeCount = StlM13ShipGateCatalog.ProductApiOrdinaryAccessProbes.Count;
        Assert.True(theoryCount >= 1, "Expected at least one ordinary product access theory.");
        Assert.True(
            probeCount + factCount >= StlM13ShipGateCatalog.MinimumAccessModelIntegrationTests,
            $"Expected at least {StlM13ShipGateCatalog.MinimumAccessModelIntegrationTests} access-model cases.");
    }
}
