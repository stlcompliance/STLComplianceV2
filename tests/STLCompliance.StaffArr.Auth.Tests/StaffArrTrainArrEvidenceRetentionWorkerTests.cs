using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrEvidenceRetentionWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _sharedWorkerToTrainarrToken = null!;
    private string _evidenceRootPath = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"EvidenceRetentionNexArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"EvidenceRetentionTrainArr-{Guid.NewGuid():N}";
        _evidenceRootPath = Path.Combine(Path.GetTempPath(), $"trainarr-evidence-{Guid.NewGuid():N}");

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
        _sharedWorkerToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["trainarr"],
            EvidenceRetentionWorkerService.ProcessEvidenceRetentionActionScope);

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("EvidenceStorage:RootPath", _evidenceRootPath);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();

        if (Directory.Exists(_evidenceRootPath))
        {
            Directory.Delete(_evidenceRootPath, recursive: true);
        }
    }

    [Fact]
    public async Task Process_evidence_retention_batch_rejects_missing_service_token()
    {
        var response = await _trainarrClient.PostAsJsonAsync(
            "/api/internal/evidence-retention/process-batch",
            new ProcessEvidenceRetentionRequest(null, DateTimeOffset.UtcNow, 50));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_evidence_retention_returns_expired_evidence()
    {
        var evidenceId = await SeedExpiredEvidenceAsync(retentionDays: 30);

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/evidence-retention/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);

        var listResponse = await _trainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingEvidenceRetentionResponse>())!;
        Assert.Contains(pending.Items, x => x.EvidenceId == evidenceId);
    }

    [Fact]
    public async Task Process_evidence_retention_batch_purges_expired_files_and_records()
    {
        var evidenceId = await SeedExpiredEvidenceAsync(retentionDays: 30);
        var storageKey = await GetEvidenceStorageKeyAsync(evidenceId);
        var absolutePath = Path.Combine(_evidenceRootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(absolutePath));

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/evidence-retention/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessEvidenceRetentionRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50));

        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessEvidenceRetentionResponse>())!;
        Assert.Equal(1, body.PurgedCount);
        Assert.Contains(evidenceId, body.PurgedEvidenceIds);
        Assert.False(File.Exists(absolutePath));

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        Assert.False(await db.TrainingEvidence.AnyAsync(x => x.Id == evidenceId));

        var run = await db.EvidenceRetentionRuns.SingleAsync(x => x.TenantId == PlatformSeeder.DemoTenantId);
        Assert.Equal("purged", run.Outcome);
        Assert.Equal(1, run.EvidencePurgedCount);
        Assert.True(run.BytesReclaimed > 0);
    }

    private async Task<Guid> SeedExpiredEvidenceAsync(int retentionDays)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<TrainArrEvidenceStorageService>();
        var now = DateTimeOffset.UtcNow;
        var definitionId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var evidenceId = Guid.NewGuid();
        var closedAt = now.AddDays(-(retentionDays + 5));

        db.TrainingDefinitions.Add(new TrainingDefinition
        {
            Id = definitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            DefinitionKey = $"retention_{Guid.NewGuid():N}"[..20],
            Name = "Retention test definition",
            Description = "Evidence retention worker test definition.",
            QualificationKey = "hazmat_endorsement",
            QualificationName = "Hazmat Endorsement",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.TrainingAssignments.Add(new TrainingAssignment
        {
            Id = assignmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = PlatformSeeder.DemoAdminUserId,
            TrainingDefinitionId = definitionId,
            AssignmentReason = "manual",
            Status = "completed",
            CompletedAt = closedAt,
            CreatedAt = closedAt.AddDays(-1),
            UpdatedAt = closedAt,
        });

        await using var contentStream = new MemoryStream("expired evidence payload"u8.ToArray());
        var storageKey = await storage.SaveAsync(
            PlatformSeeder.DemoTenantId,
            assignmentId,
            evidenceId,
            "retention-test.bin",
            contentStream);

        db.TrainingEvidence.Add(new TrainingEvidence
        {
            Id = evidenceId,
            TenantId = PlatformSeeder.DemoTenantId,
            TrainingAssignmentId = assignmentId,
            EvidenceTypeKey = "document",
            FileName = "retention-test.bin",
            ContentType = "application/octet-stream",
            SizeBytes = 24,
            StorageKey = storageKey,
            UploadedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = closedAt,
        });

        db.TenantEvidenceRetentionSettings.Add(new TenantEvidenceRetentionSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            RetentionDaysAfterAssignmentClose = retentionDays,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
        return evidenceId;
    }

    private async Task<string> GetEvidenceStorageKeyAsync(Guid evidenceId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        return (await db.TrainingEvidence.SingleAsync(x => x.Id == evidenceId)).StorageKey;
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
            $"{sourceProduct}-evidence-retention-{Guid.NewGuid():N}",
            $"{sourceProduct} evidence retention worker test",
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
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
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(DbContextOptions<TContext>)
            || d.ServiceType == typeof(TContext)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
