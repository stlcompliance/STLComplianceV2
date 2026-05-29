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
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrAttachmentRetentionWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _sharedWorkerToRoutarrToken = null!;
    private string _attachmentRootPath = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"AttachmentRetentionNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"AttachmentRetentionRoutArr-{Guid.NewGuid():N}";
        _attachmentRootPath = Path.Combine(Path.GetTempPath(), $"routarr-attachments-{Guid.NewGuid():N}");

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
        _sharedWorkerToRoutarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["routarr"],
            AttachmentRetentionWorkerService.ProcessAttachmentRetentionActionScope);

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("CaptureAttachmentStorage:RootPath", _attachmentRootPath);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));
            });
        });

        _routarrClient = _routarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();

        if (Directory.Exists(_attachmentRootPath))
        {
            Directory.Delete(_attachmentRootPath, recursive: true);
        }
    }

    [Fact]
    public async Task Process_attachment_retention_batch_rejects_missing_service_token()
    {
        var response = await _routarrClient.PostAsJsonAsync(
            "/api/internal/attachment-retention/process-batch",
            new ProcessAttachmentRetentionRequest(null, DateTimeOffset.UtcNow, 50));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_attachment_retention_returns_expired_attachments()
    {
        var attachmentId = await SeedExpiredAttachmentAsync(retentionDays: 30);

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/attachment-retention/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToRoutarrToken);

        var listResponse = await _routarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingAttachmentRetentionResponse>())!;
        Assert.Contains(pending.Items, x => x.AttachmentId == attachmentId);
    }

    [Fact]
    public async Task Process_attachment_retention_batch_purges_expired_files_and_records()
    {
        var attachmentId = await SeedExpiredAttachmentAsync(retentionDays: 30);
        var storageKey = await GetAttachmentStorageKeyAsync(attachmentId);
        var absolutePath = Path.Combine(_attachmentRootPath, storageKey.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(absolutePath));

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/attachment-retention/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToRoutarrToken);
        processRequest.Content = JsonContent.Create(new ProcessAttachmentRetentionRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50));

        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessAttachmentRetentionResponse>())!;
        Assert.Equal(1, body.PurgedCount);
        Assert.Contains(attachmentId, body.PurgedAttachmentIds);
        Assert.False(File.Exists(absolutePath));

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        Assert.False(await db.TripCaptureAttachments.AnyAsync(x => x.Id == attachmentId));

        var run = await db.AttachmentRetentionRuns.SingleAsync(x => x.TenantId == PlatformSeeder.DemoTenantId);
        Assert.Equal("purged", run.Outcome);
        Assert.Equal(1, run.AttachmentsPurgedCount);
        Assert.True(run.BytesReclaimed > 0);
    }

    private async Task<Guid> SeedExpiredAttachmentAsync(int retentionDays)
    {
        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<RoutArrCaptureAttachmentStorageService>();
        var now = DateTimeOffset.UtcNow;
        var tripId = Guid.NewGuid();
        var proofId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var closedAt = now.AddDays(-(retentionDays + 5));

        db.Trips.Add(new Trip
        {
            Id = tripId,
            TenantId = PlatformSeeder.DemoTenantId,
            TripNumber = $"RET-{Guid.NewGuid():N}"[..12],
            Title = "Retention test trip",
            Description = "Attachment retention worker test trip.",
            DispatchStatus = TripDispatchStatuses.Completed,
            CompletedAt = closedAt,
            ClosedAt = closedAt,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = closedAt.AddDays(-1),
            UpdatedAt = closedAt,
        });

        db.TripProofRecords.Add(new TripProofRecord
        {
            Id = proofId,
            TenantId = PlatformSeeder.DemoTenantId,
            TripId = tripId,
            ProofType = TripProofTypes.Pickup,
            ReferenceKey = "RET-PROOF",
            Notes = "Retention test proof",
            CapturedByPersonId = PlatformSeeder.DemoAdminUserId.ToString(),
            CapturedAt = closedAt,
            CreatedAt = closedAt,
            UpdatedAt = closedAt,
        });

        await using var contentStream = new MemoryStream("expired attachment payload"u8.ToArray());
        var storageKey = await storage.SaveAsync(
            PlatformSeeder.DemoTenantId,
            tripId,
            attachmentId,
            "retention-test.bin",
            contentStream);

        db.TripCaptureAttachments.Add(new TripCaptureAttachment
        {
            Id = attachmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            TripId = tripId,
            SubjectType = TripCaptureAttachmentSubjects.Proof,
            SubjectId = proofId,
            AttachmentKind = TripCaptureAttachmentKinds.Photo,
            FileName = "retention-test.bin",
            ContentType = "application/octet-stream",
            SizeBytes = 24,
            StorageKey = storageKey,
            CapturedByPersonId = PlatformSeeder.DemoAdminUserId.ToString(),
            CreatedAt = closedAt,
        });

        db.TenantAttachmentRetentionSettings.Add(new TenantAttachmentRetentionSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            RetentionDaysAfterTripClose = retentionDays,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
        return attachmentId;
    }

    private async Task<string> GetAttachmentStorageKeyAsync(Guid attachmentId)
    {
        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        return (await db.TripCaptureAttachments.SingleAsync(x => x.Id == attachmentId)).StorageKey;
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
            $"{sourceProduct}-attachment-retention-{Guid.NewGuid():N}",
            $"{sourceProduct} attachment retention worker test",
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
