using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrServiceTokenTrustTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private const string SigningKeyId = "test-rsa-service-token-key";
    private static readonly RsaPemPair RsaKeys = CreateRsaPemPair();

    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrServiceTokenTrustTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "fallback-service-token-key-at-least-32-chars");
            builder.UseSetting("ServiceToken:SigningKeyId", SigningKeyId);
            builder.UseSetting("ServiceToken:RsaPrivateKeyPem", EscapePem(RsaKeys.PrivatePem));
            builder.UseSetting("ServiceToken:RsaPublicKeyPem", EscapePem(RsaKeys.PublicPem));
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
                    options.UseInMemoryDatabase("NexArrServiceTokenTrustTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Jwks_exposes_configured_service_token_public_key_without_authentication()
    {
        var response = await _client.GetAsync("/api/v1/.well-known/jwks.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var key = Assert.Single(document.RootElement.GetProperty("keys").EnumerateArray());
        Assert.Equal("RSA", key.GetProperty("kty").GetString());
        Assert.Equal("sig", key.GetProperty("use").GetString());
        Assert.Equal("RS256", key.GetProperty("alg").GetString());
        Assert.Equal(SigningKeyId, key.GetProperty("kid").GetString());
        Assert.False(string.IsNullOrWhiteSpace(key.GetProperty("n").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(key.GetProperty("e").GetString()));
    }

    [Fact]
    public async Task Rsa_service_token_issue_and_validate_succeeds()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "rsa-staffarr-worker",
            "RSA StaffArr Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", token);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "read",
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(issued.AccessToken);
        Assert.Equal("RS256", jwt.Header.Alg);
        Assert.Equal(SigningKeyId, jwt.Header.Kid);

        var validateRequest = Authorized(HttpMethod.Post, "/api/service-tokens/validate", token);
        validateRequest.Content = JsonContent.Create(new ValidateServiceTokenRequest(issued.AccessToken));
        var validateResponse = await _client.SendAsync(validateRequest);
        validateResponse.EnsureSuccessStatusCode();
        var validation = await validateResponse.Content.ReadFromJsonAsync<ServiceTokenValidationResponse>();

        Assert.NotNull(validation);
        Assert.True(validation.IsValid);
        Assert.Equal(issued.TokenId, validation.TokenId);
        Assert.Equal(PlatformSeeder.DemoTenantId, validation.TenantId);
    }

    [Fact]
    public async Task V1_service_client_and_token_endpoints_issue_and_validate()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "v1-staffarr-worker",
            "V1 StaffArr Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var listRequest = Authorized(HttpMethod.Get, "/api/v1/service-clients?page=1&pageSize=10", token);
        var listResponse = await _client.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();

        var issueRequest = Authorized(HttpMethod.Post, "/api/v1/service-token", token);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "read",
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;

        var validateRequest = Authorized(HttpMethod.Post, "/api/service-tokens/validate", token);
        validateRequest.Content = JsonContent.Create(new ValidateServiceTokenRequest(issued.AccessToken));
        var validateResponse = await _client.SendAsync(validateRequest);
        validateResponse.EnsureSuccessStatusCode();
        var validation = await validateResponse.Content.ReadFromJsonAsync<ServiceTokenValidationResponse>();

        Assert.NotNull(validation);
        Assert.True(validation.IsValid);
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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

    private static RsaPemPair CreateRsaPemPair()
    {
        using var rsa = RSA.Create(2048);
        return new RsaPemPair(
            rsa.ExportRSAPrivateKeyPem(),
            rsa.ExportSubjectPublicKeyInfoPem());
    }

    private static string EscapePem(string pem) =>
        pem.Replace("\r\n", "\\n", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);

    private sealed record RsaPemPair(string PrivatePem, string PublicPem);
}
