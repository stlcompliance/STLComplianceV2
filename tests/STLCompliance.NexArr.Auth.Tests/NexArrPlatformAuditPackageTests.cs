using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;

namespace STLCompliance.NexArr.Auth.Tests;

public class NexArrPlatformAuditPackageTests : IClassFixture<WebApplicationFactory<global::NexArr.Api.Program>>
{
    private readonly WebApplicationFactory<global::NexArr.Api.Program> _factory;
    private readonly HttpClient _client;
    private string _workerToken = null!;

    public NexArrPlatformAuditPackageTests(WebApplicationFactory<global::NexArr.Api.Program> factory)
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
                    options.UseInMemoryDatabase("NexArrPlatformAuditPackageTests"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Manifest_requires_platform_admin()
    {
        await SeedDatabaseAsync();
        var tenantAdminToken = await LoginAsync(PlatformSeeder.DemoTenantAdminEmail);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/audit-packages/manifest", tenantAdminToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Platform_admin_can_export_zip_and_timeline()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);

        var manifestResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/audit-packages/manifest", adminToken));
        manifestResponse.EnsureSuccessStatusCode();
        var manifest = (await manifestResponse.Content.ReadFromJsonAsync<PlatformAuditPackageManifestResponse>())!;
        Assert.Contains(manifest.Sections, s => s.Key == "platform_audit_events");

        var timelineResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/audit-packages/timeline?pageSize=5", adminToken));
        timelineResponse.EnsureSuccessStatusCode();

        var exportResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/platform-admin/audit-packages/export?format=zip", adminToken));
        exportResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/zip", exportResponse.Content.Headers.ContentType?.MediaType);

        await using var zipStream = await exportResponse.Content.ReadAsStreamAsync();
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        Assert.NotNull(archive.GetEntry("platform_audit_events.json"));
        Assert.NotNull(archive.GetEntry("tenants.json"));
    }

    [Fact]
    public async Task Background_job_completes_via_internal_process_batch()
    {
        await SeedDatabaseAsync();
        var adminToken = await LoginAsync(PlatformSeeder.DemoAdminEmail);
        _workerToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["nexarr"],
            PlatformAuditPackageGenerationService.ProcessJobsActionScope);

        var createRequest = Authorized(HttpMethod.Post, "/api/platform-admin/audit-packages/jobs", adminToken);
        createRequest.Content = JsonContent.Create(
            new CreatePlatformAuditPackageGenerationJobRequest("zip", null, null, null));
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<PlatformAuditPackageGenerationJobResponse>())!;
        Assert.Equal(PlatformAuditPackageGenerationJobStatuses.Pending, created.Status);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/platform-audit-package-jobs/process-batch",
            _workerToken);
        processRequest.Content = JsonContent.Create(
            new ProcessPlatformAuditPackageGenerationJobsRequest(null, null, 5));
        var processResponse = await _client.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessPlatformAuditPackageGenerationJobsResponse>())!;
        Assert.Equal(PlatformAuditPackageGenerationJobStatuses.Completed, batch.Results[0].Status);

        var statusResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/audit-packages/jobs/{created.JobId}",
                adminToken));
        var status = (await statusResponse.Content.ReadFromJsonAsync<PlatformAuditPackageGenerationJobResponse>())!;
        Assert.Equal(PlatformAuditPackageGenerationJobStatuses.Completed, status.Status);
        Assert.True(status.DownloadReady);

        var downloadResponse = await _client.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/platform-admin/audit-packages/jobs/{created.JobId}/download",
                adminToken));
        downloadResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/zip", downloadResponse.Content.Headers.ContentType?.MediaType);
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-platform-audit-{Guid.NewGuid():N}",
            $"{sourceProduct} platform audit package test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _client.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            allowedProducts,
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
