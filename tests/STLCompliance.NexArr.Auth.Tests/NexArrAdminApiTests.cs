using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrAdminApiTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;

    public NexArrAdminApiTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
    {
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
                    options.UseInMemoryDatabase("NexArrAdminTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Tenants_list_requires_authentication()
    {
        var response = await _client.GetAsync("/api/tenants");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_create_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/tenants", token);
        request.Content = JsonContent.Create(new CreateTenantRequest("acme-corp", "Acme Corporation"));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var tenant = await response.Content.ReadFromJsonAsync<TenantDetailResponse>();
        Assert.NotNull(tenant);
        Assert.Equal("acme-corp", tenant.Slug);
    }

    [Fact]
    public async Task Tenant_admin_cannot_create_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/tenants", token);
        request.Content = JsonContent.Create(new CreateTenantRequest("blocked-corp", "Blocked Corporation"));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_can_read_own_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/tenants/{PlatformSeeder.DemoTenantId}", token));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tenant = await response.Content.ReadFromJsonAsync<TenantDetailResponse>();
        Assert.NotNull(tenant);
        Assert.Equal(PlatformSeeder.DemoTenantId, tenant.TenantId);
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_other_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var otherTenantId = Guid.Parse("99999999-9999-9999-9999-999999999901");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/tenants/{otherTenantId}", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_archive_tenant_with_outbox_event()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/tenants", token);
        createRequest.Content = JsonContent.Create(new CreateTenantRequest("archive-corp", "Archive Corporation"));
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;
        Assert.StartsWith($"/api/v1/tenants/{created.TenantId}", createResponse.Headers.Location?.OriginalString);

        var archiveResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/tenants/{created.TenantId}/archive", token));

        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);
        var archived = (await archiveResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;
        Assert.Equal(TenantStatuses.Archived, archived.Status);

        var enableResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/tenants/{created.TenantId}/enable", token));

        Assert.Equal(HttpStatusCode.Conflict, enableResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var tenant = await db.Tenants.AsNoTracking().SingleAsync(x => x.Id == created.TenantId);
        Assert.Equal(TenantStatuses.Archived, tenant.Status);

        var outboxEvent = await db.PlatformOutboxEvents
            .AsNoTracking()
            .SingleAsync(x => x.EventType == PlatformOutboxEventKinds.TenantArchived
                && x.TenantId == created.TenantId);

        Assert.Contains("archive-corp", outboxEvent.PayloadJson);
        Assert.Contains(TenantStatuses.Active, outboxEvent.PayloadJson);
        Assert.Contains(TenantStatuses.Archived, outboxEvent.PayloadJson);
    }

    [Fact]
    public async Task Platform_admin_can_create_product()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var request = Authorized(HttpMethod.Post, "/api/products", token);
        request.Content = JsonContent.Create(new CreateProductRequest("audit-portal", "Audit Portal", 85));
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task V1_product_detail_exposes_integration_metadata()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/products/staffarr", token));
        response.EnsureSuccessStatusCode();
        var product = (await response.Content.ReadFromJsonAsync<ProductDetailResponse>())!;

        Assert.Equal("staffarr", product.ProductKey);
        Assert.Equal("workforce", product.ProductCategory);
        Assert.Equal("People Operations", product.ProductOwner);
        Assert.Equal("available", product.ProductStatus);
        Assert.Equal("/auth/nexarr/callback", product.CanonicalCallbackPath);
        Assert.Equal("http://localhost:5102", product.ApiBaseUrl);
        Assert.Equal("http://localhost:5102/health/ready", product.HealthUrl);
        Assert.Equal("stl:staffarr:api", product.ServiceAudience);
        Assert.Contains("/products/staffarr", product.MarketingUrl, StringComparison.Ordinal);
        Assert.Contains("/docs/staffarr", product.DocumentationUrl, StringComparison.Ordinal);
        Assert.Equal("local", product.EnvironmentKey);
        Assert.Equal("tenant-product-entitlement-required", product.EntitlementDependencyRules);
    }

    [Fact]
    public async Task Product_lifecycle_changes_enqueue_platform_events()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/products", token);
        createRequest.Content = JsonContent.Create(new CreateProductRequest(
            "event-portal",
            "Event Portal",
            86));
        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<ProductDetailResponse>())!;
        Assert.StartsWith($"/api/v1/products/{created.ProductKey}", createResponse.Headers.Location?.OriginalString);

        var updateRequest = Authorized(HttpMethod.Patch, "/api/v1/products/event-portal", token);
        updateRequest.Content = JsonContent.Create(new UpdateProductRequest(
            "Event Portal Updated",
            87,
            true));
        var updateResponse = await _client.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        var disableResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/products/event-portal/disable", token));
        disableResponse.EnsureSuccessStatusCode();

        var enableResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/products/event-portal/enable", token));
        enableResponse.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var outboxEvents = await db.PlatformOutboxEvents
            .AsNoTracking()
            .Where(x => x.ProductCode == "event-portal")
            .ToListAsync();

        Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.ProductCreated);
        Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.ProductUpdated);
        Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.ProductDisabled);
        Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.ProductEnabled);
        Assert.All(outboxEvents, outboxEvent =>
        {
            Assert.Equal(PlatformOutboxEventStatuses.Pending, outboxEvent.ProcessingStatus);
            Assert.Null(outboxEvent.TenantId);
            Assert.Contains("event-portal", outboxEvent.PayloadJson);
            Assert.Contains("\"displayName\"", outboxEvent.PayloadJson);
        });
    }

    [Fact]
    public async Task Platform_admin_can_read_product_manifest_contract()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var response = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/product-manifests?tenantId={PlatformSeeder.DemoTenantId}&productKey=staffarr",
                token));

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<PagedResult<ProductManifestResponse>>();

        Assert.NotNull(payload);
        var manifest = Assert.Single(payload.Items);
        Assert.Equal("staffarr", manifest.ProductKey);
        Assert.Equal("StaffArr", manifest.DisplayName);
        Assert.Equal("workforce", manifest.ProductCategory);
        Assert.Equal("People Operations", manifest.ProductOwner);
        Assert.Equal("available", manifest.ProductStatus);
        Assert.True(manifest.IsActive);
        Assert.Equal("local", manifest.EnvironmentKey);
        Assert.Equal("/auth/nexarr/callback", manifest.CanonicalCallbackPath);
        Assert.Equal("http://localhost:5102", manifest.ApiBaseUrl);
        Assert.Equal("http://localhost:5102/health/ready", manifest.HealthUrl);
        Assert.Equal("stl:staffarr:api", manifest.ServiceAudience);
        Assert.Equal("tenant-product-entitlement-required", manifest.EntitlementDependencyRules);
        Assert.Contains("nexarr", manifest.ProductDependencyMetadata);
        Assert.Equal("http://localhost:5175", manifest.LaunchBaseUrl);
        Assert.Equal("/launch", manifest.LaunchPath);
        Assert.Equal("http://localhost:5175/launch", manifest.LaunchUrl);
        Assert.NotEmpty(manifest.CallbackAllowlist);
    }

    [Fact]
    public async Task Tenant_admin_cannot_read_product_manifest_contract()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/product-manifests", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_grant_entitlement()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createTenant = Authorized(HttpMethod.Post, "/api/tenants", token);
        createTenant.Content = JsonContent.Create(new CreateTenantRequest("grant-test", "Grant Test Tenant"));
        var tenantResponse = await _client.SendAsync(createTenant);
        tenantResponse.EnsureSuccessStatusCode();
        var tenant = (await tenantResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;

        var grantRequest = Authorized(HttpMethod.Post, "/api/entitlements", token);
        grantRequest.Content = JsonContent.Create(new GrantEntitlementRequest(tenant.TenantId, "staffarr"));
        var grantResponse = await _client.SendAsync(grantRequest);

        Assert.Equal(HttpStatusCode.Created, grantResponse.StatusCode);
        var entitlement = await grantResponse.Content.ReadFromJsonAsync<EntitlementDetailResponse>();
        Assert.NotNull(entitlement);
        Assert.Equal("Active", entitlement.Status);
    }

    [Fact]
    public async Task Tenant_admin_cannot_grant_entitlement_for_other_tenant()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);
        var otherTenantId = Guid.Parse("99999999-9999-9999-9999-999999999901");

        var grantRequest = Authorized(HttpMethod.Post, "/api/entitlements", token);
        grantRequest.Content = JsonContent.Create(new GrantEntitlementRequest(otherTenantId, "staffarr"));
        var response = await _client.SendAsync(grantRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Service_token_issue_and_validate_succeeds()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "staffarr-worker",
            "StaffArr Worker",
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
    public async Task V1_platform_admin_can_get_service_client_detail()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "detail-worker",
            "Detail Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var registered = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var detailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/service-clients/{registered.ServiceClientId}", token));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        Assert.Equal(registered.ServiceClientId, detail.ServiceClientId);
        Assert.Equal("detail-worker", detail.ClientKey);
        Assert.Equal("Detail Worker", detail.DisplayName);
        Assert.Equal("staffarr", detail.SourceProductKey);
        Assert.Contains("staffarr", detail.AllowedProductKeys);
        Assert.True(detail.IsActive);
        Assert.Equal(0, detail.FailedAuthenticationAttempts);
    }

    [Fact]
    public async Task Revoked_service_token_fails_validation()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "trainarr-worker",
            "TrainArr Worker",
            "trainarr",
            ["trainarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", token);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            null,
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;

        var revokeRequest = Authorized(HttpMethod.Post, $"/api/service-tokens/{issued.TokenId}/revoke", token);
        var revokeResponse = await _client.SendAsync(revokeRequest);
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var validateRequest = Authorized(HttpMethod.Post, "/api/service-tokens/validate", token);
        validateRequest.Content = JsonContent.Create(new ValidateServiceTokenRequest(issued.AccessToken));
        var validateResponse = await _client.SendAsync(validateRequest);
        validateResponse.EnsureSuccessStatusCode();
        var validation = await validateResponse.Content.ReadFromJsonAsync<ServiceTokenValidationResponse>();

        Assert.NotNull(validation);
        Assert.False(validation.IsValid);
        Assert.Equal("token_revoked", validation.ReasonCode);
    }

    [Fact]
    public async Task Platform_admin_can_view_service_token_audit_log()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "audit-worker",
            "Audit Worker",
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
            null,
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;

        var validateRequest = Authorized(HttpMethod.Post, "/api/service-tokens/validate", token);
        validateRequest.Content = JsonContent.Create(new ValidateServiceTokenRequest(issued.AccessToken));
        var validateResponse = await _client.SendAsync(validateRequest);
        validateResponse.EnsureSuccessStatusCode();
        var valid = (await validateResponse.Content.ReadFromJsonAsync<ServiceTokenValidationResponse>())!;
        Assert.True(valid.IsValid);

        var revokeResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/service-tokens/{issued.TokenId}/revoke", token));
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var deniedValidateRequest = Authorized(HttpMethod.Post, "/api/service-tokens/validate", token);
        deniedValidateRequest.Content = JsonContent.Create(new ValidateServiceTokenRequest(issued.AccessToken));
        var deniedValidateResponse = await _client.SendAsync(deniedValidateRequest);
        deniedValidateResponse.EnsureSuccessStatusCode();
        var denied = (await deniedValidateResponse.Content.ReadFromJsonAsync<ServiceTokenValidationResponse>())!;
        Assert.False(denied.IsValid);
        Assert.Equal("token_revoked", denied.ReasonCode);

        var auditResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/service-tokens/audit?serviceClientId={client.ServiceClientId}&page=1&pageSize=100",
                token));
        auditResponse.EnsureSuccessStatusCode();

        var auditPage = await auditResponse.Content.ReadFromJsonAsync<PagedResult<PlatformAuditEventExportItem>>();
        Assert.NotNull(auditPage);
        Assert.Contains(auditPage!.Items, x => x.Action == "service_client.register" && x.Result == "Success");
        Assert.Contains(auditPage.Items, x => x.Action == "service_token.issue" && x.Result == "Success");
        Assert.Contains(auditPage.Items, x => x.Action == "service_token.validate" && x.Result == "Success");
        Assert.Contains(auditPage.Items, x => x.Action == "service_token.revoke" && x.Result == "Success");
        Assert.Contains(auditPage.Items, x => x.Action == "service_token.validate" && x.Result == "Denied" && x.ReasonCode == "token_revoked");
    }

    [Fact]
    public async Task Tenant_admin_cannot_view_service_token_audit_log()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/service-tokens/audit?page=1&pageSize=20", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Service_client_usage_and_failed_auth_telemetry_is_reported()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "telemetry-worker",
            "Telemetry Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;
        Assert.Null(client.LastUsedAt);
        Assert.Equal(0, client.FailedAuthenticationAttempts);

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

        var revokeRequest = Authorized(HttpMethod.Post, $"/api/service-tokens/{issued.TokenId}/revoke", token);
        (await _client.SendAsync(revokeRequest)).EnsureSuccessStatusCode();

        var validateRequest = Authorized(HttpMethod.Post, "/api/service-tokens/validate", token);
        validateRequest.Content = JsonContent.Create(new ValidateServiceTokenRequest(issued.AccessToken));
        var validateResponse = await _client.SendAsync(validateRequest);
        validateResponse.EnsureSuccessStatusCode();
        var failedValidation = (await validateResponse.Content.ReadFromJsonAsync<ServiceTokenValidationResponse>())!;
        Assert.False(failedValidation.IsValid);
        Assert.Equal("token_revoked", failedValidation.ReasonCode);

        var listResponseAfterFailure = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/service-clients?page=1&pageSize=50", token));
        listResponseAfterFailure.EnsureSuccessStatusCode();
        var listAfterFailure = (await listResponseAfterFailure.Content.ReadFromJsonAsync<PagedResult<ServiceClientResponse>>())!;
        var listedAfterFailure = Assert.Single(listAfterFailure.Items.Where(x => x.ServiceClientId == client.ServiceClientId));
        Assert.Null(listedAfterFailure.LastUsedAt);
        Assert.Equal(1, listedAfterFailure.FailedAuthenticationAttempts);

        var successValidateRequest = Authorized(HttpMethod.Post, "/api/service-tokens/validate", token);
        successValidateRequest.Content = JsonContent.Create(new ValidateServiceTokenRequest(
            (await IssueServiceTokenForClientAsync(token, client.ServiceClientId)).AccessToken));
        var successValidateResponse = await _client.SendAsync(successValidateRequest);
        successValidateResponse.EnsureSuccessStatusCode();
        var successValidation = (await successValidateResponse.Content.ReadFromJsonAsync<ServiceTokenValidationResponse>())!;
        Assert.True(successValidation.IsValid);

        var listResponseAfterSuccess = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/service-clients?page=1&pageSize=50", token));
        listResponseAfterSuccess.EnsureSuccessStatusCode();
        var listAfterSuccess = (await listResponseAfterSuccess.Content.ReadFromJsonAsync<PagedResult<ServiceClientResponse>>())!;
        var listedAfterSuccess = Assert.Single(listAfterSuccess.Items.Where(x => x.ServiceClientId == client.ServiceClientId));
        Assert.NotNull(listedAfterSuccess.LastUsedAt);
        Assert.Equal(0, listedAfterSuccess.FailedAuthenticationAttempts);
    }

    [Fact]
    public async Task Tenant_admin_cannot_register_service_client()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "blocked-worker",
            "Blocked Worker",
            "staffarr",
            ["staffarr"]));
        var response = await _client.SendAsync(registerRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_update_service_client_audience()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "audience-worker",
            "Audience Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;
        Assert.Equal(["staffarr"], client.AllowedProductKeys);

        var updateRequest = Authorized(HttpMethod.Patch, $"/api/v1/service-clients/{client.ServiceClientId}/audience", token);
        updateRequest.Content = JsonContent.Create(new UpdateServiceClientAudienceRequest(["staffarr", "trainarr"]));
        var updateResponse = await _client.SendAsync(updateRequest);
        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;
        Assert.Equal(2, updated.AllowedProductKeys.Count);
        Assert.Contains("staffarr", updated.AllowedProductKeys);
        Assert.Contains("trainarr", updated.AllowedProductKeys);
    }

    [Fact]
    public async Task Tenant_admin_cannot_update_service_client_audience()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "audience-blocked-worker",
            "Audience Blocked Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var updateRequest = Authorized(HttpMethod.Patch, $"/api/v1/service-clients/{client.ServiceClientId}/audience", tenantToken);
        updateRequest.Content = JsonContent.Create(new UpdateServiceClientAudienceRequest(["staffarr", "trainarr"]));
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_assign_service_client_tenant_scope_and_enforce_issue_scope()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var createTenantRequest = Authorized(HttpMethod.Post, "/api/v1/tenants", token);
        createTenantRequest.Content = JsonContent.Create(new CreateTenantRequest(
            $"scope-other-{Guid.NewGuid():N}",
            "Scope Other Tenant"));
        var createTenantResponse = await _client.SendAsync(createTenantRequest);
        createTenantResponse.EnsureSuccessStatusCode();
        var otherTenant = (await createTenantResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "tenant-scope-worker",
            "Tenant Scope Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var scopeRequest = Authorized(HttpMethod.Patch, $"/api/v1/service-clients/{client.ServiceClientId}/tenant-scope", token);
        scopeRequest.Content = JsonContent.Create(new UpdateServiceClientTenantScopeRequest([PlatformSeeder.DemoTenantId]));
        var scopeResponse = await _client.SendAsync(scopeRequest);
        scopeResponse.EnsureSuccessStatusCode();
        var scopePayload = (await scopeResponse.Content.ReadFromJsonAsync<ServiceClientTenantScopeResponse>())!;
        Assert.Equal(client.ServiceClientId, scopePayload.ServiceClientId);
        Assert.Equal([PlatformSeeder.DemoTenantId], scopePayload.TenantIds);

        var listClientsResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/service-clients?page=1&pageSize=50", token));
        listClientsResponse.EnsureSuccessStatusCode();
        var clientsPage = (await listClientsResponse.Content.ReadFromJsonAsync<PagedResult<ServiceClientResponse>>())!;
        var listedClient = Assert.Single(clientsPage.Items.Where(x => x.ServiceClientId == client.ServiceClientId));
        Assert.Equal([PlatformSeeder.DemoTenantId], listedClient.AllowedTenantIds);

        var allowedIssueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", token);
        allowedIssueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            null,
            30));
        var allowedIssueResponse = await _client.SendAsync(allowedIssueRequest);
        allowedIssueResponse.EnsureSuccessStatusCode();

        var deniedIssueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", token);
        deniedIssueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            otherTenant.TenantId,
            null,
            null,
            30));
        var deniedIssueResponse = await _client.SendAsync(deniedIssueRequest);
        Assert.Equal(HttpStatusCode.Forbidden, deniedIssueResponse.StatusCode);
    }

    [Fact]
    public async Task Tenant_admin_cannot_assign_service_client_tenant_scope()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        var tenantToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "tenant-scope-blocked-worker",
            "Tenant Scope Blocked Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var scopeRequest = Authorized(HttpMethod.Patch, $"/api/v1/service-clients/{client.ServiceClientId}/tenant-scope", tenantToken);
        scopeRequest.Content = JsonContent.Create(new UpdateServiceClientTenantScopeRequest([PlatformSeeder.DemoTenantId]));
        var scopeResponse = await _client.SendAsync(scopeRequest);
        Assert.Equal(HttpStatusCode.Forbidden, scopeResponse.StatusCode);
    }

    [Fact]
    public async Task V1_rotate_service_client_revokes_existing_tokens_but_allows_new_issue()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "rotate-worker",
            "Rotate Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        Assert.StartsWith("/api/v1/service-clients/", registerResponse.Headers.Location?.ToString());
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/v1/service-token", token);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "read",
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        Assert.StartsWith("/api/v1/service-token/", issueResponse.Headers.Location?.ToString());
        var issuedBeforeRotate = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;

        var rotateRequest = Authorized(HttpMethod.Post, $"/api/v1/service-clients/{client.ServiceClientId}/rotate", token);
        var rotateResponse = await _client.SendAsync(rotateRequest);
        Assert.Equal(HttpStatusCode.NoContent, rotateResponse.StatusCode);

        var validateOldRequest = Authorized(HttpMethod.Post, "/api/service-tokens/validate", token);
        validateOldRequest.Content = JsonContent.Create(new ValidateServiceTokenRequest(issuedBeforeRotate.AccessToken));
        var validateOldResponse = await _client.SendAsync(validateOldRequest);
        validateOldResponse.EnsureSuccessStatusCode();
        var oldValidation = await validateOldResponse.Content.ReadFromJsonAsync<ServiceTokenValidationResponse>();
        Assert.NotNull(oldValidation);
        Assert.False(oldValidation.IsValid);
        Assert.Equal("token_revoked", oldValidation.ReasonCode);

        var issueAfterRotateRequest = Authorized(HttpMethod.Post, "/api/v1/service-token", token);
        issueAfterRotateRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "read",
            30));
        var issueAfterRotateResponse = await _client.SendAsync(issueAfterRotateRequest);
        issueAfterRotateResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task V1_service_client_create_and_rotate_enqueue_platform_events()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "created-rotated-event-worker",
            "Created Rotated Event Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        Assert.StartsWith("/api/v1/service-clients/", registerResponse.Headers.Location?.ToString());
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/v1/service-token", token);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "read",
            30));
        (await _client.SendAsync(issueRequest)).EnsureSuccessStatusCode();

        var rotateRequest = Authorized(HttpMethod.Post, $"/api/v1/service-clients/{client.ServiceClientId}/rotate", token);
        var rotateResponse = await _client.SendAsync(rotateRequest);
        Assert.Equal(HttpStatusCode.NoContent, rotateResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var outboxEvents = await db.PlatformOutboxEvents
            .AsNoTracking()
            .Where(x => x.TenantId == null && x.PayloadJson.Contains(client.ServiceClientId.ToString()))
            .ToListAsync();

        var createdEvent = Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.ServiceClientCreated);
        Assert.Equal(PlatformOutboxEventStatuses.Pending, createdEvent.ProcessingStatus);
        Assert.Contains("created-rotated-event-worker", createdEvent.PayloadJson);
        Assert.Contains("\"allowedProductKeys\":\"staffarr\"", createdEvent.PayloadJson);
        Assert.DoesNotContain("accessToken", createdEvent.PayloadJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", createdEvent.PayloadJson, StringComparison.OrdinalIgnoreCase);

        var rotatedEvent = Assert.Single(outboxEvents, x => x.EventType == PlatformOutboxEventKinds.ServiceClientRotated);
        Assert.Equal(PlatformOutboxEventStatuses.Pending, rotatedEvent.ProcessingStatus);
        Assert.Contains("created-rotated-event-worker", rotatedEvent.PayloadJson);
        Assert.Contains("\"revokedTokenCount\":\"1\"", rotatedEvent.PayloadJson);
        Assert.DoesNotContain("accessToken", rotatedEvent.PayloadJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", rotatedEvent.PayloadJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task V1_revoke_service_client_disables_client_and_blocks_new_issue()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "revoke-worker",
            "Revoke Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var revokeRequest = Authorized(HttpMethod.Post, $"/api/v1/service-clients/{client.ServiceClientId}/revoke", token);
        var revokeResponse = await _client.SendAsync(revokeRequest);
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        var issueAfterRevokeRequest = Authorized(HttpMethod.Post, "/api/v1/service-token", token);
        issueAfterRevokeRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "read",
            30));
        var issueAfterRevokeResponse = await _client.SendAsync(issueAfterRevokeRequest);
        Assert.Equal(HttpStatusCode.NotFound, issueAfterRevokeResponse.StatusCode);
    }

    [Fact]
    public async Task V1_revoke_service_client_enqueues_platform_revoked_event()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var registerRequest = Authorized(HttpMethod.Post, "/api/v1/service-clients", token);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            "revoked-event-worker",
            "Revoked Event Worker",
            "staffarr",
            ["staffarr"]));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/v1/service-token", token);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "read",
            30));
        (await _client.SendAsync(issueRequest)).EnsureSuccessStatusCode();

        var revokeRequest = Authorized(HttpMethod.Post, $"/api/v1/service-clients/{client.ServiceClientId}/revoke", token);
        var revokeResponse = await _client.SendAsync(revokeRequest);
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var outboxEvent = await db.PlatformOutboxEvents
            .AsNoTracking()
            .SingleAsync(x =>
                x.EventType == PlatformOutboxEventKinds.ServiceClientRevoked
                && x.TenantId == null
                && x.PayloadJson.Contains(client.ServiceClientId.ToString())
                && x.PayloadJson.Contains("revoked-event-worker"));

        Assert.Equal(PlatformOutboxEventStatuses.Pending, outboxEvent.ProcessingStatus);
        Assert.DoesNotContain("accessToken", outboxEvent.PayloadJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"revokedTokenCount\":\"1\"", outboxEvent.PayloadJson);
    }

    [Fact]
    public async Task V1_tenant_disable_and_enable_updates_status()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var disableResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/disable", token));
        disableResponse.EnsureSuccessStatusCode();
        var disabled = (await disableResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;
        Assert.Equal("Suspended", disabled.Status);

        var enableResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/enable", token));
        enableResponse.EnsureSuccessStatusCode();
        var enabled = (await enableResponse.Content.ReadFromJsonAsync<TenantDetailResponse>())!;
        Assert.Equal("Active", enabled.Status);
    }

    [Fact]
    public async Task V1_product_disable_and_enable_updates_active_flag()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var disableResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/products/staffarr/disable", token));
        disableResponse.EnsureSuccessStatusCode();
        var disabled = (await disableResponse.Content.ReadFromJsonAsync<ProductDetailResponse>())!;
        Assert.False(disabled.IsActive);

        var enableResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/products/staffarr/enable", token));
        enableResponse.EnsureSuccessStatusCode();
        var enabled = (await enableResponse.Content.ReadFromJsonAsync<ProductDetailResponse>())!;
        Assert.True(enabled.IsActive);
    }

    [Fact]
    public async Task V1_tenant_entitlement_routes_grant_check_and_revoke()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var grantRequest = Authorized(HttpMethod.Post, $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/entitlements", token);
        grantRequest.Content = JsonContent.Create(new GrantEntitlementRequest(PlatformSeeder.DemoTenantId, "trainarr"));
        var grantResponse = await _client.SendAsync(grantRequest);
        grantResponse.EnsureSuccessStatusCode();
        Assert.StartsWith($"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/entitlements/trainarr", grantResponse.Headers.Location?.OriginalString);

        var checkGrantedResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/entitlements/check?tenantId={PlatformSeeder.DemoTenantId}&productCode=trainarr",
                token));
        checkGrantedResponse.EnsureSuccessStatusCode();
        var granted = (await checkGrantedResponse.Content.ReadFromJsonAsync<EntitlementCheckResponse>())!;
        Assert.True(granted.IsEntitled);

        var revokeResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/tenants/{PlatformSeeder.DemoTenantId}/entitlements/trainarr", token));
        revokeResponse.EnsureSuccessStatusCode();

        var checkRevokedResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/v1/entitlements/check?tenantId={PlatformSeeder.DemoTenantId}&productCode=trainarr",
                token));
        checkRevokedResponse.EnsureSuccessStatusCode();
        var revoked = (await checkRevokedResponse.Content.ReadFromJsonAsync<EntitlementCheckResponse>())!;
        Assert.False(revoked.IsEntitled);
    }

    [Fact]
    public async Task V1_audit_events_and_event_by_id_routes_return_data()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var eventsResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit/events?page=1&pageSize=10", token));
        eventsResponse.EnsureSuccessStatusCode();
        var eventsPage = await eventsResponse.Content.ReadFromJsonAsync<PagedResult<PlatformAuditEventExportItem>>();
        Assert.NotNull(eventsPage);
        Assert.NotEmpty(eventsPage.Items);

        var eventId = eventsPage.Items[0].AuditEventId;
        var eventResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/audit/events/{eventId}", token));
        eventResponse.EnsureSuccessStatusCode();
        var item = await eventResponse.Content.ReadFromJsonAsync<PlatformAuditEventExportItem>();
        Assert.NotNull(item);
        Assert.Equal(eventId, item.AuditEventId);

    }

    [Fact]
    public async Task V1_audit_tenant_user_and_product_routes_filter_data()
    {
        await SeedDatabaseAsync();
        var token = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var disableProductResponse = await _client.SendAsync(
            Authorized(HttpMethod.Post, "/api/v1/products/staffarr/disable", token));
        disableProductResponse.EnsureSuccessStatusCode();

        var tenantResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/audit/tenants/{PlatformSeeder.DemoTenantId}?page=1&pageSize=20", token));
        tenantResponse.EnsureSuccessStatusCode();
        var tenantPage = await tenantResponse.Content.ReadFromJsonAsync<PagedResult<PlatformAuditEventExportItem>>();
        Assert.NotNull(tenantPage);

        var userResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/audit/users/{PlatformSeeder.DemoAdminUserId}?page=1&pageSize=20", token));
        userResponse.EnsureSuccessStatusCode();
        var userPage = await userResponse.Content.ReadFromJsonAsync<PagedResult<PlatformAuditEventExportItem>>();
        Assert.NotNull(userPage);
        Assert.Contains(userPage.Items, x => x.ActorUserId == PlatformSeeder.DemoAdminUserId);

        var productResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/audit/products/staffarr?page=1&pageSize=20", token));
        productResponse.EnsureSuccessStatusCode();
        var productPage = await productResponse.Content.ReadFromJsonAsync<PagedResult<PlatformAuditEventExportItem>>();
        Assert.NotNull(productPage);
        Assert.Contains(productPage.Items, x => string.Equals(x.TargetId, "staffarr", StringComparison.OrdinalIgnoreCase));
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

    private async Task<ServiceTokenIssueResponse> IssueServiceTokenForClientAsync(string adminToken, Guid serviceClientId)
    {
        var issueRequest = Authorized(HttpMethod.Post, "/api/v1/service-token", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            serviceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "read",
            30));
        var issueResponse = await _client.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        return (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
    }
}
