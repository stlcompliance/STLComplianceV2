using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrAuditPackageGenerationTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _sharedWorkerToMaintainArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"AuditPackageGenerationNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"AuditPackageGenerationMaintainArr-{Guid.NewGuid():N}";

        _nexarrFactory = new WebApplicationFactory<global::NexArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<NexArrDbContext>(services);
                services.AddDbContext<NexArrDbContext>(options => options.UseInMemoryDatabase(nexArrDbName));
            });
        });

        _nexarrClient = _nexarrFactory.CreateClient();
        await SeedNexArrAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _sharedWorkerToMaintainArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["maintainarr"],
            AuditPackageGenerationService.ProcessJobsActionScope);

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options =>
                    options.UseInMemoryDatabase(maintainArrDbName));
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Create_job_returns_pending_status()
    {
        var adminToken = CreateMaintainArrAccessToken(["maintainarr"], tenantRoleKey: "maintainarr_admin");
        var createRequest = Authorized(HttpMethod.Post, "/api/audit-packages/jobs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateAuditPackageGenerationJobRequest("zip", null, null));

        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);
        var job = (await createResponse.Content.ReadFromJsonAsync<AuditPackageGenerationJobResponse>())!;
        Assert.Equal(AuditPackageGenerationJobStatuses.Pending, job.Status);
        Assert.Equal("zip", job.Format);
        Assert.False(job.DownloadReady);
    }

    [Fact]
    public async Task Process_batch_completes_job_and_download_returns_zip()
    {
        await SeedAuditEventAsync();
        var adminToken = CreateMaintainArrAccessToken(["maintainarr"], tenantRoleKey: "maintainarr_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/audit-packages/jobs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateAuditPackageGenerationJobRequest("zip", null, null));
        var createResponse = await _maintainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<AuditPackageGenerationJobResponse>())!;

        var pendingRequest = Authorized(
            HttpMethod.Get,
            $"/api/internal/audit-package-jobs/pending?tenantId={PlatformSeeder.DemoTenantId}",
            _sharedWorkerToMaintainArrToken);
        var pendingResponse = await _maintainarrClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingAuditPackageGenerationJobsResponse>())!;
        Assert.Contains(pending.Items, item => item.JobId == created.JobId);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/audit-package-jobs/process-batch",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessAuditPackageGenerationJobsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            5));

        var processResponse = await _maintainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessAuditPackageGenerationJobsResponse>())!;
        Assert.Equal(1, batch.CompletedCount);
        Assert.Equal(AuditPackageGenerationJobStatuses.Completed, batch.Results[0].Status);

        var statusRequest = Authorized(
            HttpMethod.Get,
            $"/api/audit-packages/jobs/{created.JobId}",
            adminToken);
        var statusResponse = await _maintainarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();
        var status = (await statusResponse.Content.ReadFromJsonAsync<AuditPackageGenerationJobResponse>())!;
        Assert.Equal(AuditPackageGenerationJobStatuses.Completed, status.Status);
        Assert.True(status.DownloadReady);
        Assert.NotNull(status.PackageId);

        var downloadResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/audit-packages/jobs/{created.JobId}/download", adminToken));
        downloadResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/zip", downloadResponse.Content.Headers.ContentType?.MediaType);

        var zipBytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        Assert.Contains(archive.Entries, entry => entry.Name == "audit_events.json");

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        Assert.True(await db.AuditEvents.AnyAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "audit_package.generation.completed"));
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _maintainarrClient.PostAsJsonAsync(
            "/api/internal/audit-package-jobs/process-batch",
            new ProcessAuditPackageGenerationJobsRequest(PlatformSeeder.DemoTenantId, null, 5));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task SeedAuditEventAsync()
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        db.AuditEvents.Add(new MaintainArrAuditEvent
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ActorUserId = PlatformSeeder.DemoAdminUserId,
            Action = "asset.create",
            TargetType = "asset",
            TargetId = Guid.NewGuid().ToString(),
            Result = "success",
            CorrelationId = Guid.NewGuid(),
            OccurredAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-audit-packages-{Guid.NewGuid():N}",
            $"{sourceProduct} audit package generation test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            allowedProducts,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin")
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
