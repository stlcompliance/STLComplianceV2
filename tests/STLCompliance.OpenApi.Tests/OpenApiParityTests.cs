using System.Net;
using STLCompliance.OpenApi.Tests.Support;
using STLCompliance.Shared.Operations;

namespace STLCompliance.OpenApi.Tests;

[Trait("Category", "OpenApi")]
public abstract class OpenApiParityTestsBase<TProgram>(string productKey)
    where TProgram : class
{
    [Fact]
    public async Task OpenApi_document_matches_checked_in_snapshot()
    {
        await using var host = new OpenApiTestHost<TProgram>();

        var response = await host.Client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var openApiJson = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(openApiJson));

        var normalized = OpenApiSnapshotHelper.Normalize(openApiJson);
        OpenApiSnapshotHelper.AssertMatchesSnapshot(productKey, normalized);
    }

    [Fact]
    public async Task OpenApi_document_includes_health_and_api_routes()
    {
        await using var host = new OpenApiTestHost<TProgram>();

        var response = await host.Client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();

        var openApiJson = await response.Content.ReadAsStringAsync();
        foreach (var fragment in StlM13ShipGateCatalog.RequiredOpenApiPathFragments)
        {
            Assert.Contains(fragment, openApiJson, StringComparison.Ordinal);
        }
    }
}

public sealed class NexArrOpenApiParityTests() : OpenApiParityTestsBase<global::NexArr.Api.Program>("nexarr");

public sealed class StaffArrOpenApiParityTests() : OpenApiParityTestsBase<global::StaffArr.Api.Program>("staffarr");

public sealed class TrainArrOpenApiParityTests() : OpenApiParityTestsBase<global::TrainArr.Api.Program>("trainarr");

public sealed class MaintainArrOpenApiParityTests() : OpenApiParityTestsBase<global::MaintainArr.Api.Program>("maintainarr");

public sealed class RoutArrOpenApiParityTests() : OpenApiParityTestsBase<global::RoutArr.Api.Program>("routarr");

public sealed class SupplyArrOpenApiParityTests() : OpenApiParityTestsBase<global::SupplyArr.Api.Program>("supplyarr");

public sealed class ComplianceCoreOpenApiParityTests() : OpenApiParityTestsBase<global::ComplianceCore.Api.Program>("compliancecore");

public sealed class LoadArrOpenApiParityTests() : OpenApiParityTestsBase<global::LoadArr.Api.Program>("loadarr");

public sealed class LoadArrWorkspaceOpenApiTests
{
    [Fact]
    public async Task OpenApi_document_includes_wms_workspace_contracts()
    {
        await using var host = new OpenApiTestHost<global::LoadArr.Api.Program>();

        var response = await host.Client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();

        var openApiJson = await response.Content.ReadAsStringAsync();

        var requiredPaths = new[]
        {
            "/api/v1/workspace/summary",
            "/api/v1/workspace/locations",
            "/api/v1/workspace/locations/{id}",
            "/api/v1/workspace/locations/tree",
            "/api/v1/workspace/inventory",
            "/api/v1/workspace/supplyarr-item-references",
            "/api/v1/workspace/tasks",
            "/api/v1/workspace/holds",
            "/api/v1/workspace/route-handoffs",
            "/api/v1/workspace/evidence",
            "/api/v1/receiving",
            "/api/v1/receiving/{id}",
            "/api/v1/receiving/{id}/lines",
            "/api/v1/receiving/{id}/complete",
            "/api/v1/receiving/{id}/cancel",
            "/api/v1/transfers",
            "/api/v1/transfers/{id}",
            "/api/v1/transfers/{id}/complete",
            "/api/v1/transfers/{id}/cancel",
            "/api/v1/holds",
            "/api/v1/holds/{id}",
            "/api/v1/holds/{id}/release",
            "/api/v1/unexplained-inventory",
            "/api/v1/unexplained-inventory/{id}",
            "/api/v1/unexplained-inventory/{id}/resolve",
            "/api/v1/unexplained-inventory/{id}/quarantine",
            "/api/v1/unexplained-inventory/{id}/scrap"
        };

        foreach (var path in requiredPaths)
        {
            Assert.Contains(path, openApiJson, StringComparison.Ordinal);
        }
    }
}
