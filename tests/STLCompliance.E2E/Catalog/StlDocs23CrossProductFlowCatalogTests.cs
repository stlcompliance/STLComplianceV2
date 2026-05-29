using System.Reflection;
using STLCompliance.E2E.Flows;
using STLCompliance.Shared.Operations;

namespace STLCompliance.E2E.Catalog;

[Trait("Category", "E2e")]
[Trait("Area", "ShipGate")]
public sealed class StlDocs23CrossProductFlowCatalogTests
{
    private static readonly Assembly E2eAssembly = typeof(NexArrHandoffFlowTests).Assembly;

    [Fact]
    public void Integration_flow_catalog_meets_docs_11_minimum()
    {
        Assert.Equal(
            StlDocs23CrossProductFlowCatalog.MinimumIntegrationFlowTestClasses,
            StlDocs23CrossProductFlowCatalog.IntegrationFlows.Count);
    }

    [Theory]
    [MemberData(nameof(IntegrationFlowTypeNames))]
    public void Integration_flow_test_class_exists_with_facts(string typeName)
    {
        var flowType = E2eAssembly.GetType($"STLCompliance.E2E.Flows.{typeName}", throwOnError: false);
        Assert.NotNull(flowType);

        var factCount = flowType!
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Count(m => m.GetCustomAttribute<FactAttribute>() is not null);

        Assert.True(factCount >= 1, $"Expected at least one integration fact on {typeName}.");
    }

    [Fact]
    public void Playwright_only_flows_reference_catalog_specs()
    {
        foreach (var flow in StlDocs23CrossProductFlowCatalog.PlaywrightOnlyFlows)
        {
            Assert.False(string.IsNullOrWhiteSpace(flow.PlaywrightSpec));
            Assert.Contains(flow.PlaywrightSpec!, StlE2ePlaywrightSpecCatalog.All);
        }
    }

    [Fact]
    public void Integration_flows_with_playwright_specs_are_cataloged()
    {
        foreach (var flow in StlDocs23CrossProductFlowCatalog.IntegrationFlows)
        {
            if (flow.PlaywrightSpec is null)
            {
                continue;
            }

            Assert.Contains(flow.PlaywrightSpec, StlE2ePlaywrightSpecCatalog.All);
        }
    }

    public static IEnumerable<object[]> IntegrationFlowTypeNames =>
        StlDocs23CrossProductFlowCatalog.IntegrationFlows
            .Where(x => !string.IsNullOrWhiteSpace(x.IntegrationFlowTestTypeName))
            .Select(x => new object[] { x.IntegrationFlowTestTypeName! });
}
