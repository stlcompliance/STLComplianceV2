using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using STLCompliance.Shared.Health;

namespace STLCompliance.Health.Tests;

public class NexArrHealthTests(WebApplicationFactory<NexArr.Api.Program> factory)
    : ApiHealthEndpointTests<NexArr.Api.Program>(factory, "nexarr");

public class StaffArrHealthTests(WebApplicationFactory<StaffArr.Api.Program> factory)
    : ApiHealthEndpointTests<StaffArr.Api.Program>(factory, "staffarr");

public class TrainArrHealthTests(WebApplicationFactory<TrainArr.Api.Program> factory)
    : ApiHealthEndpointTests<TrainArr.Api.Program>(factory, "trainarr");

public class MaintainArrHealthTests(WebApplicationFactory<MaintainArr.Api.Program> factory)
    : ApiHealthEndpointTests<MaintainArr.Api.Program>(factory, "maintainarr");

public class RecordArrHealthTests(WebApplicationFactory<RecordArr.Api.Program> factory)
    : ApiHealthEndpointTests<RecordArr.Api.Program>(factory, "recordarr");

public class RoutArrHealthTests(WebApplicationFactory<RoutArr.Api.Program> factory)
    : ApiHealthEndpointTests<RoutArr.Api.Program>(factory, "routarr");

public class SupplyArrHealthTests(WebApplicationFactory<SupplyArr.Api.Program> factory)
    : ApiHealthEndpointTests<SupplyArr.Api.Program>(factory, "supplyarr");

public class ComplianceCoreHealthTests(WebApplicationFactory<ComplianceCore.Api.Program> factory)
    : ApiHealthEndpointTests<ComplianceCore.Api.Program>(factory, "compliancecore");

public class AssurArrHealthTests(WebApplicationFactory<global::AssurArr.Api.Program> factory)
    : ApiHealthEndpointTests<global::AssurArr.Api.Program>(factory, "assurarr");

public abstract class ApiHealthEndpointTests<TProgram>(
    WebApplicationFactory<TProgram> factory,
    string expectedProductKey) : IClassFixture<WebApplicationFactory<TProgram>>
    where TProgram : class
{
    private readonly HttpClient         _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseEnvironment("Production");
        }).CreateClient();

    [Fact]
    public async Task Health_liveness_returns_ok_with_product_key()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSecurityHeaders(response);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Healthy", payload.Status);
        Assert.Equal(expectedProductKey, payload.Product);
    }

    [Fact]
    public async Task Health_v1_liveness_returns_ok_with_product_key()
    {
        var response = await _client.GetAsync("/api/v1/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSecurityHeaders(response);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Healthy", payload.Status);
        Assert.Equal(expectedProductKey, payload.Product);
    }

    [Fact]
    public async Task Health_ready_returns_json_without_database()
    {
        var response = await _client.GetAsync("/health/ready");
        Assert.True(response.IsSuccessStatusCode);
        AssertSecurityHeaders(response);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(payload);
        Assert.Equal(expectedProductKey, payload.Product);
        Assert.NotNull(payload.Checks);
        Assert.Contains("self", payload.Checks.Keys);
    }

    [Fact]
    public async Task Health_v1_ready_returns_json_without_database()
    {
        var response = await _client.GetAsync("/api/v1/health/ready");
        Assert.True(response.IsSuccessStatusCode);
        AssertSecurityHeaders(response);

        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(payload);
        Assert.Equal(expectedProductKey, payload.Product);
        Assert.NotNull(payload.Checks);
        Assert.Contains("self", payload.Checks.Keys);
    }

    private static void AssertSecurityHeaders(HttpResponseMessage response)
    {
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Equal("camera=(), microphone=(), geolocation=()", response.Headers.GetValues("Permissions-Policy").Single());
        Assert.Equal("default-src 'none'; base-uri 'none'; frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
    }
}
