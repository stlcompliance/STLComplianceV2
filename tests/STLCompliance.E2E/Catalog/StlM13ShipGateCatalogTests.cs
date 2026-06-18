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
    public void Entitlement_denial_probes_cover_product_apis_with_me_surfaces()
    {
        var keys = StlM13ShipGateCatalog.ProductApiEntitlementDenialProbes
            .Select(p => p.ProductKey)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var productKey in StlM13ShipGateCatalog.OpenApiProductKeys)
        {
            if (productKey is "nexarr" or "loadarr" or "assurarr")
            {
                continue;
            }

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
    public void Entitlement_denial_integration_tests_meet_ship_gate_minimum()
    {
        var theoryCount = typeof(EntitlementDenialFlowTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Count(m => m.GetCustomAttribute<TheoryAttribute>() is not null);

        var factCount = typeof(EntitlementDenialFlowTests)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Count(m => m.GetCustomAttribute<FactAttribute>() is not null);

        var probeCount = StlM13ShipGateCatalog.ProductApiEntitlementDenialProbes.Count;
        Assert.True(theoryCount >= 1, "Expected at least one entitlement denial theory.");
        Assert.True(
            probeCount + factCount >= StlM13ShipGateCatalog.MinimumEntitlementDenialIntegrationTests,
            $"Expected at least {StlM13ShipGateCatalog.MinimumEntitlementDenialIntegrationTests} entitlement denial cases.");
    }
}
