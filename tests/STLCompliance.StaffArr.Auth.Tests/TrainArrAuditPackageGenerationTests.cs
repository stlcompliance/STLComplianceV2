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
using NexArr.Api.Services;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public sealed class TrainArrAuditPackageGenerationTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _sharedWorkerToTrainArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"AuditPackageGenerationNexArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"AuditPackageGenerationTrainArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToTrainArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["trainarr"],
            AuditPackageGenerationService.ProcessJobsActionScope);

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options =>
                    options.UseInMemoryDatabase(trainArrDbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Create_job_returns_pending_status()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var createRequest = Authorized(HttpMethod.Post, "/api/audit-packages/jobs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateAuditPackageGenerationJobRequest("zip", null, null));

        var createResponse = await _trainarrClient.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);
        var job = (await createResponse.Content.ReadFromJsonAsync<AuditPackageGenerationJobResponse>())!;
        Assert.Equal(AuditPackageGenerationJobStatuses.Pending, job.Status);
        Assert.Equal("zip", job.Format);
        Assert.False(job.DownloadReady);
    }

    [Fact]
    public async Task Process_batch_completes_job_and_download_returns_zip()
    {
        await SeedTrainingDataAsync();
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/audit-packages/jobs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateAuditPackageGenerationJobRequest("zip", null, null));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<AuditPackageGenerationJobResponse>())!;

        var pendingRequest = Authorized(
            HttpMethod.Get,
            $"/api/internal/audit-package-jobs/pending?tenantId={PlatformSeeder.DemoTenantId}",
            _sharedWorkerToTrainArrToken);
        var pendingResponse = await _trainarrClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingAuditPackageGenerationJobsResponse>())!;
        Assert.Contains(pending.Items, item => item.JobId == created.JobId);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/audit-package-jobs/process-batch",
            _sharedWorkerToTrainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessAuditPackageGenerationJobsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            5));

        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var batch = (await processResponse.Content.ReadFromJsonAsync<ProcessAuditPackageGenerationJobsResponse>())!;
        Assert.Equal(1, batch.CompletedCount);
        Assert.Equal(AuditPackageGenerationJobStatuses.Completed, batch.Results[0].Status);

        var statusRequest = Authorized(
            HttpMethod.Get,
            $"/api/audit-packages/jobs/{created.JobId}",
            adminToken);
        var statusResponse = await _trainarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();
        var status = (await statusResponse.Content.ReadFromJsonAsync<AuditPackageGenerationJobResponse>())!;
        Assert.Equal(AuditPackageGenerationJobStatuses.Completed, status.Status);
        Assert.True(status.DownloadReady);
        Assert.NotNull(status.PackageId);

        var downloadResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/audit-packages/jobs/{created.JobId}/download", adminToken));
        downloadResponse.EnsureSuccessStatusCode();
        Assert.Equal("application/zip", downloadResponse.Content.Headers.ContentType?.MediaType);

        var zipBytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        Assert.Contains(archive.Entries, entry => entry.Name == "training_definitions.json");

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        Assert.True(await db.AuditEvents.AnyAsync(x =>
            x.TenantId == PlatformSeeder.DemoTenantId && x.Action == "audit_package.generation.completed"));
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _trainarrClient.PostAsJsonAsync(
            "/api/internal/audit-package-jobs/process-batch",
            new ProcessAuditPackageGenerationJobsRequest(PlatformSeeder.DemoTenantId, null, 5));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task SeedTrainingDataAsync()
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TrainingDefinitions.Add(new TrainingDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            DefinitionKey = "hazmat_awareness",
            Name = "Hazmat Awareness",
            Description = "Seeded for async audit package generation test.",
            QualificationKey = "hazmat_endorsement",
            QualificationName = "Hazmat Endorsement",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
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

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin")
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArrTokenService>();
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
