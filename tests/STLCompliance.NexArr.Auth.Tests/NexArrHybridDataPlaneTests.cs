using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Health;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrHybridDataPlaneTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;
    private readonly DataPlaneValidationStubHandler _validationHandler;

    public NexArrHybridDataPlaneTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
        _validationHandler = new DataPlaneValidationStubHandler();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<NexArrDbContext>)
                        || d.ServiceType == typeof(NexArrDbContext))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<NexArrDbContext>(options =>
                    options.UseInMemoryDatabase("NexArrHybridDataPlaneTests"));
                services.AddHttpClient(HybridDataPlaneService.HttpClientName)
                    .ConfigurePrimaryHttpMessageHandler(() => _validationHandler);
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Platform_admin_can_upsert_and_list_data_plane_profile()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var upsertRequest = Authorized(HttpMethod.Put, "/api/platform-admin/data-plane", token);
        upsertRequest.Content = JsonContent.Create(new UpsertDataPlaneProfileRequest(
            PlatformSeeder.DemoTenantId,
            "staffarr",
            "customer_hosted",
            "https://customer.example/staffarr",
            "untrusted",
            "Pilot customer deployment"));
        var upsertResponse = await _client.SendAsync(upsertRequest);
        Assert.Equal(HttpStatusCode.OK, upsertResponse.StatusCode);
        var profile = await upsertResponse.Content.ReadFromJsonAsync<DataPlaneProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("customer_hosted", profile.DeploymentMode);
        Assert.Equal("untrusted", profile.TrustStatus);

        var listResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/data-plane?tenantId={PlatformSeeder.DemoTenantId}",
                token));
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<DataPlaneProfileResponse>>();
        Assert.NotNull(page);
        Assert.Single(page.Items);
    }

    [Fact]
    public async Task Customer_hosted_cannot_be_marked_trusted_without_validation()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var upsertRequest = Authorized(HttpMethod.Put, "/api/platform-admin/data-plane", token);
        upsertRequest.Content = JsonContent.Create(new UpsertDataPlaneProfileRequest(
            PlatformSeeder.DemoTenantId,
            "staffarr",
            "customer_hosted",
            "https://customer.example/staffarr",
            "trusted",
            null));
        var response = await _client.SendAsync(upsertRequest);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_cannot_manage_data_plane_profiles()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/data-plane", token));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Validation_marks_customer_hosted_endpoint_trusted_when_probe_succeeds()
    {
        await SeedDatabaseAsync();
        _validationHandler.Respond = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new HealthResponse(
                "Healthy",
                "staffarr",
                "1.0.0",
                DateTimeOffset.UtcNow,
                new Dictionary<string, object> { ["self"] = new { status = "Healthy" } })),
        };

        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/platform-admin/data-plane/validate",
            token,
            new ValidateDataPlaneProfileRequest(
                PlatformSeeder.DemoTenantId,
                "staffarr",
                "customer_hosted",
                "https://customer.example/staffarr",
                "Validated customer deployment")));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ValidateDataPlaneProfileResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Trusted", payload.ValidationStatus);
        Assert.Equal("trusted", payload.Profile.TrustStatus);
        Assert.Equal("https://customer.example/staffarr/health/ready", payload.ReadyUrl);

        var listResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/data-plane?tenantId={PlatformSeeder.DemoTenantId}",
                token));
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<DataPlaneProfileResponse>>();
        Assert.NotNull(page);
        Assert.Single(page.Items);
        Assert.Equal("trusted", page.Items[0].TrustStatus);
    }

    [Fact]
    public async Task Validation_sets_pending_validation_when_probe_fails()
    {
        await SeedDatabaseAsync();
        _validationHandler.Respond = _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("maintenance"),
        };

        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/platform-admin/data-plane/validate",
            token,
            new ValidateDataPlaneProfileRequest(
                PlatformSeeder.DemoTenantId,
                "staffarr",
                "customer_hosted",
                "https://customer.example/staffarr",
                null)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ValidateDataPlaneProfileResponse>();
        Assert.NotNull(payload);
        Assert.Equal("PendingValidation", payload.ValidationStatus);
        Assert.Equal("pending_validation", payload.Profile.TrustStatus);
        Assert.Equal("upstream_503", payload.ErrorCode);
        Assert.Contains("maintenance", payload.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken, object body)
    {
        var request = Authorized(method, url, accessToken);
        request.Content = JsonContent.Create(body);
        return request;
    }

    private async Task<string> LoginAsync(string email)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task SeedDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private sealed class DataPlaneValidationStubHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage> Respond { get; set; } = _ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new HealthResponse(
                    "Healthy",
                    "staffarr",
                    "1.0.0",
                    DateTimeOffset.UtcNow,
                    new Dictionary<string, object> { ["self"] = new { status = "Healthy" } })),
            };

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request.RequestUri is null
                || !request.RequestUri.AbsolutePath.EndsWith("/health/ready", StringComparison.Ordinal))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            return Task.FromResult(Respond(request));
        }
    }
}
