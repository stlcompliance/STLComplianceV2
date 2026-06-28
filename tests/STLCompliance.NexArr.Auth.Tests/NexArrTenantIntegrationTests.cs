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

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrTenantIntegrationTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrTenantIntegrationTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("ServiceToken:SigningKey", "test-signing-key-at-least-32-chars-long");
            builder.UseSetting("TenantIntegrations:EncryptionKey", "tenant-integration-test-key-at-least-32-chars");
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
                    options.UseInMemoryDatabase("NexArrTenantIntegrationTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Catalog_contains_all_requested_providers_and_hardcoded_routes()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/catalog", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var catalog = (await response.Content.ReadFromJsonAsync<TenantIntegrationCatalogResponse>())!;
        var keys = catalog.Providers.Select(x => x.ProviderKey).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var requestedKeys = new[]
        {
            "microsoft-entra", "okta", "google-workspace", "quickbooks", "xero", "samsara",
            "geotab", "motive", "fleetio-fuel-imports", "shipstation", "easypost", "shopify",
            "fedex", "ups", "usps", "google-drive", "sharepoint", "docusign", "bamboohr",
            "gusto", "ecfr", "fmcsa", "nhtsa", "power-bi", "wex", "comdata-corpay",
            "us-bank-voyager", "dat", "truckstop", "project44", "fourkites", "macropoint",
            "eroad", "teletrac-navman", "adp", "workday", "ukg", "paychex", "paylocity",
            "netsuite", "sap", "oracle", "manhattan", "extensiv", "edi-x12", "as2", "sftp",
            "csv-xlsx", "webhooks", "openapi", "oauth2", "scim", "saml-oidc",
        };

        foreach (var key in requestedKeys)
        {
            Assert.Contains(key, keys);
        }

        foreach (var provider in catalog.Providers)
        {
            Assert.False(string.IsNullOrWhiteSpace(provider.Brand.Mark));
            Assert.Matches("^#[0-9A-Fa-f]{6}$", provider.Brand.AccentColor);
            Assert.StartsWith("https://", provider.Brand.AssetSourceUrl, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("trademark", provider.Brand.UsageNote, StringComparison.OrdinalIgnoreCase);
        }

        var quickbooks = catalog.Providers.Single(x => x.ProviderKey == "quickbooks");
        Assert.False(quickbooks.RequiresManualMapping);
        Assert.Equal("QB", quickbooks.Brand.Mark);
        Assert.Equal("#2CA01C", quickbooks.Brand.AccentColor);
        Assert.Contains(quickbooks.Routes, route =>
            route.Path == "/api/v1/tenants/{tenantId}/integrations/quickbooks");
        Assert.Contains(quickbooks.Routes, route =>
            route.Path == "/app/nexarr/integrations/quickbooks/mappings");
    }

    [Fact]
    public async Task Tenant_admin_can_configure_current_tenant_and_writebacks_default_off()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/integrations/quickbooks",
            token,
            new UpsertTenantIntegrationConnectionRequest(
                "configured",
                "read_only",
                null,
                null,
                "{\"healthUrl\":\"https://provider.example/health\"}")));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var connection = (await response.Content.ReadFromJsonAsync<TenantIntegrationConnectionResponse>())!;
        Assert.Equal("quickbooks", connection.ProviderKey);
        Assert.Equal("configured", connection.Status);
        Assert.False(connection.WritebacksEnabled);
        Assert.False(connection.ManualMappingRequired);
    }

    [Fact]
    public async Task Tenant_admin_cannot_configure_another_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{Guid.NewGuid()}/integrations/quickbooks",
            token,
            new UpsertTenantIntegrationConnectionRequest("configured", "read_only", null, null, "{}")));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_configure_any_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantId = await CreateTenantAsync(token);

        var upsert = await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{tenantId}/integrations/xero",
            token,
            new UpsertTenantIntegrationConnectionRequest("configured", "read_only", false, false, "{}")));
        Assert.Equal(HttpStatusCode.OK, upsert.StatusCode);

        var list = await _client.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/platform-admin/integrations?tenantId={tenantId}&providerKey=xero",
            token));
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var page = (await list.Content.ReadFromJsonAsync<PagedResult<TenantIntegrationConnectionResponse>>())!;
        Assert.Single(page.Items);
        Assert.Equal("xero", page.Items[0].ProviderKey);
    }

    [Fact]
    public async Task Credential_updates_are_encrypted_and_redacted()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        const string secret = "super-secret-token-1234";

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/integrations/quickbooks/credentials",
            token,
            new UpsertTenantIntegrationCredentialRequest(
                "oauth2",
                "QuickBooks production",
                new Dictionary<string, string> { ["accessToken"] = secret },
                null)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(secret, body, StringComparison.Ordinal);
        Assert.Contains("****1234", body, StringComparison.Ordinal);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var credential = await db.TenantIntegrationCredentials.SingleAsync();
        Assert.StartsWith("v1.", credential.EncryptedPayload, StringComparison.Ordinal);
        Assert.DoesNotContain(secret, credential.EncryptedPayload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OAuth_callback_rejects_missing_or_invalid_state()
    {
        await SeedDatabaseAsync();

        var response = await _client.GetAsync("/api/v1/integrations/quickbooks/oauth/callback?code=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task R12_identity_protocol_routes_are_truthful_until_sso_and_scim_are_ready()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/integrations/scim",
            token,
            new UpsertTenantIntegrationConnectionRequest("configured", "inbound", false, true, "{}")));
        await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/integrations/scim/credentials",
            token,
            new UpsertTenantIntegrationCredentialRequest(
                "shared_secret",
                "SCIM bearer",
                new Dictionary<string, string> { ["scimBearerToken"] = "scim-secret" },
                null)));

        var metadata = await _client.GetAsync("/api/v1/integrations/saml-oidc/saml/metadata");
        var acs = await _client.PostAsJsonAsync(
            $"/api/v1/integrations/saml-oidc/saml/acs?tenantId={PlatformSeeder.DemoTenantId}",
            new { SAMLResponse = "placeholder" });
        using var scimRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/integrations/scim/scim/Users?tenantId={PlatformSeeder.DemoTenantId}");
        scimRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "scim-secret");
        scimRequest.Content = JsonContent.Create(new { userName = "external@example.test" });
        var scim = await _client.SendAsync(scimRequest);

        Assert.Equal(HttpStatusCode.NotImplemented, metadata.StatusCode);
        Assert.Equal(HttpStatusCode.NotImplemented, acs.StatusCode);
        Assert.Equal(HttpStatusCode.NotImplemented, scim.StatusCode);
        Assert.Contains("No sign-in, provisioning, account, or tenant record was created or changed", await scim.Content.ReadAsStringAsync());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        Assert.Empty(await db.TenantIntegrationIntakeAttempts.ToListAsync());
        Assert.Empty(await db.TenantIntegrationSyncRuns.ToListAsync());
    }

    [Fact]
    public async Task Webhook_intake_is_idempotent_and_creates_one_sync_run()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/integrations/webhooks",
            token,
            new UpsertTenantIntegrationConnectionRequest("configured", "inbound", false, true, "{}")));
        await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/integrations/webhooks/credentials",
            token,
            new UpsertTenantIntegrationCredentialRequest(
                "shared_secret",
                "Webhook secret",
                new Dictionary<string, string> { ["webhookSecret"] = "hook-secret" },
                null)));

        var first = await PostWebhookAsync();
        var second = await PostWebhookAsync();

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var firstPayload = (await first.Content.ReadFromJsonAsync<TenantIntegrationIntakeAttemptResponse>())!;
        var secondPayload = (await second.Content.ReadFromJsonAsync<TenantIntegrationIntakeAttemptResponse>())!;
        Assert.Equal(firstPayload.IntakeAttemptId, secondPayload.IntakeAttemptId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        Assert.Equal(1, await db.TenantIntegrationIntakeAttempts.CountAsync());
        Assert.Equal(1, await db.TenantIntegrationSyncRuns.CountAsync());
    }

    [Fact]
    public async Task Manual_mapping_validation_rejects_unowned_target_product()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/integrations/edi-x12/mappings",
            token,
            new UpsertTenantIntegrationMappingTemplateRequest(
                "default",
                "x12_204",
                "staffarr",
                "load",
                "{\"fields\":[]}")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Manual_sync_failure_is_visible_when_required_credentials_are_missing()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post,
            $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/integrations/quickbooks/sync-runs",
            token,
            new TriggerTenantIntegrationSyncRequest("manual-test", true)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var run = (await response.Content.ReadFromJsonAsync<TenantIntegrationSyncRunResponse>())!;
        Assert.Equal("failed", run.Status);
        Assert.Equal("credentials_missing", run.ErrorCategory);
    }

    [Fact]
    public async Task Worker_process_batch_requires_service_token_scope_and_is_duplicate_safe()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var missingTokenResponse = await _client.PostAsJsonAsync(
            "/api/internal/integrations/process-batch",
            new ProcessTenantIntegrationSyncRequest(null, 10));
        Assert.Equal(HttpStatusCode.Unauthorized, missingTokenResponse.StatusCode);

        await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/integrations/quickbooks",
            tenantToken,
            new UpsertTenantIntegrationConnectionRequest("configured", "read_only", false, false, "{}")));
        await _client.SendAsync(Authorized(
            HttpMethod.Put,
            $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/integrations/quickbooks/credentials",
            tenantToken,
            new UpsertTenantIntegrationCredentialRequest(
                "oauth2",
                "QuickBooks worker",
                new Dictionary<string, string> { ["accessToken"] = "worker-token" },
                null)));

        var workerToken = await IssueServiceTokenAsync(adminToken, TenantIntegrationService.ProcessSyncActionScope);
        var asOf = DateTimeOffset.UtcNow;
        var first = await _client.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/internal/integrations/process-batch",
            workerToken,
            new ProcessTenantIntegrationSyncRequest(asOf, 10)));
        first.EnsureSuccessStatusCode();
        var firstBatch = (await first.Content.ReadFromJsonAsync<ProcessTenantIntegrationSyncResponse>())!;
        Assert.Equal(1, firstBatch.SucceededCount);

        var second = await _client.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/internal/integrations/process-batch",
            workerToken,
            new ProcessTenantIntegrationSyncRequest(asOf, 10)));
        second.EnsureSuccessStatusCode();
        var secondBatch = (await second.Content.ReadFromJsonAsync<ProcessTenantIntegrationSyncResponse>())!;
        Assert.Equal(0, secondBatch.CandidatesFound);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        Assert.Equal(1, await db.TenantIntegrationSyncRuns.CountAsync(x => x.ProviderKey == "quickbooks"));
    }

    private async Task<HttpResponseMessage> PostWebhookAsync()
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/integrations/webhooks/webhooks/order?tenantId={PlatformSeeder.DemoTenantId}");
        request.Headers.Add("X-STL-Integration-Secret", "hook-secret");
        request.Headers.Add("Idempotency-Key", "webhook-1");
        request.Content = JsonContent.Create(new { orderId = "external-1" });
        return await _client.SendAsync(request);
    }

    private async Task<Guid> CreateTenantAsync(string adminToken)
    {
        var slug = $"integrations-{Guid.NewGuid():N}";
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/tenants",
            adminToken,
            new CreateTenantRequest(slug, "Integration Tenant")));
        response.EnsureSuccessStatusCode();
        var tenant = (await response.Content.ReadFromJsonAsync<TenantDetailResponse>())!;
        return tenant.TenantId;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"nexarr-worker-integrations-{Guid.NewGuid():N}",
            "nexarr-worker integrations test",
            "nexarr-worker",
            ["nexarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            null,
            ["nexarr"],
            actionScope,
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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
}
