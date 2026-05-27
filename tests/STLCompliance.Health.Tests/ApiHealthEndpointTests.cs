using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using STLCompliance.Shared.Health;

namespace STLCompliance.Health.Tests;

public class NexArrHealthTests : ApiHealthEndpointTests<NexArr.Api.Program>
{
    protected override string ExpectedProductKey => "nexarr";
}

public class StaffArrHealthTests : ApiHealthEndpointTests<StaffArr.Api.Program>
{
    protected override string ExpectedProductKey => "staffarr";
}

public class TrainArrHealthTests : ApiHealthEndpointTests<TrainArr.Api.Program>
{
    protected override string ExpectedProductKey => "trainarr";
}

public class MaintainArrHealthTests : ApiHealthEndpointTests<MaintainArr.Api.Program>
{
    protected override string ExpectedProductKey => "maintainarr";
}

public class RoutArrHealthTests : ApiHealthEndpointTests<RoutArr.Api.Program>
{
    protected override string ExpectedProductKey => "routarr";
}

public class SupplyArrHealthTests : ApiHealthEndpointTests<SupplyArr.Api.Program>
{
    protected override string ExpectedProductKey => "supplyarr";
}

public class ComplianceCoreHealthTests : ApiHealthEndpointTests<ComplianceCore.Api.Program>
{
    protected override string ExpectedProductKey => "compliancecore";
}

public abstract class ApiHealthEndpointTests<TProgram> : IClassFixture<WebApplicationFactory<TProgram>>
    where TProgram : class
{
    private readonly HttpClient _client;

    protected ApiHealthEndpointTests(WebApplicationFactory<TProgram> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseEnvironment("Production");
        }).CreateClient();
    }

    protected abstract string ExpectedProductKey { get; }

    [Fact]
    public async Task Health_liveness_returns_ok_with_product_key()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Healthy", payload.Status);
        Assert.Equal(ExpectedProductKey, payload.Product);
    }

    [Fact]
    public async Task Health_ready_returns_json_without_database()
    {
        var response = await _client.GetAsync("/health/ready");
        Assert.True(response.IsSuccessStatusCode);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(payload);
        Assert.Equal(ExpectedProductKey, payload.Product);
        Assert.NotNull(payload.Checks);
        Assert.Contains("self", payload.Checks.Keys);
    }
}
