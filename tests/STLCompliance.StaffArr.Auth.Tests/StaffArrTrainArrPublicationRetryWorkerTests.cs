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
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using TrainArr.Api.Contracts;
using TrainArr.Api.Endpoints;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrPublicationRetryWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _staffarrIntegrationToken = null!;
    private string _trainarrPublicationToken = null!;
    private string _sharedWorkerToTrainarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PubRetryNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"PubRetryStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"PubRetryTrainArr-{Guid.NewGuid():N}";

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
        _staffarrIntegrationToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            "staffarr.training_blockers.write,staffarr.certification_grants.write,staffarr.certification_lifecycle.write");
        _trainarrPublicationToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["trainarr", "staffarr"],
            CertificationPublicationEndpoints.PublicationActionScope);
        _sharedWorkerToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["trainarr"],
            StaffarrPublicationRetryService.ProcessRetriesActionScope);

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("StaffArr:BaseUrl", _staffarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:ServiceToken", _staffarrIntegrationToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));

                services.AddHttpClient<StaffArrTrainingBlockerClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrCertificationGrantClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrCertificationLifecycleClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_retry_batch_rejects_missing_service_token()
    {
        var response = await _trainarrClient.PostAsJsonAsync(
            "/api/internal/staffarr-publication-retries/process-batch",
            new ProcessStaffarrPublicationRetriesRequest(null, DateTimeOffset.UtcNow, 25));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_retry_batch_delivers_pending_publication_to_staffarr()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Retry Delivery User", "retry.delivery@example.com");
        var publicationId = Guid.NewGuid();
        await SeedPendingBlockerDeliveryAsync(personId, publicationId);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/staffarr-publication-retries/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessStaffarrPublicationRetriesRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25));

        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessStaffarrPublicationRetriesResponse>())!;
        Assert.Equal(1, body.PendingFound);
        Assert.Equal(1, body.DeliveredCount);

        using var trainarrScope = _trainarrFactory.Services.CreateScope();
        var trainarrDb = trainarrScope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var delivery = await trainarrDb.StaffarrPublicationDeliveries.SingleAsync(x => x.CertificationPublicationId == publicationId);
        Assert.Equal(StaffarrPublicationDeliveryStatuses.Delivered, delivery.DeliveryStatus);
        Assert.Equal(2, delivery.AttemptCount);
        Assert.NotNull(delivery.DeliveredAt);

        var userToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var readinessResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", userToken));
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Contains(readiness.Blockers, x => x.BlockerSource == "training" && x.QualificationKey == "qual.retry_worker");
    }

    [Fact]
    public async Task Publication_enqueue_marks_delivery_delivered_when_staffarr_available()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Immediate Delivery User", "immediate.delivery@example.com");

        var publishRequest = Authorized(HttpMethod.Post, "/api/certification-publications", _trainarrPublicationToken);
        publishRequest.Content = JsonContent.Create(new CreateCertificationPublicationRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            "qual.immediate_retry",
            "Immediate Retry Qualification",
            "missing_assignment",
            "Immediate retry test requires assignment completion before gated work can resume.",
            null));

        var publishResponse = await _trainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var publication = (await publishResponse.Content.ReadFromJsonAsync<CertificationPublicationResponse>())!;

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var delivery = await db.StaffarrPublicationDeliveries.SingleAsync(
            x => x.CertificationPublicationId == publication.PublicationId);
        Assert.Equal(StaffarrPublicationDeliveryStatuses.Delivered, delivery.DeliveryStatus);
    }

    [Fact]
    public async Task Certificates_v1_alias_accepts_publication_payload()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "V1 Certificate User", "v1.certificate@example.com");

        var publishRequest = Authorized(HttpMethod.Post, "/api/v1/certificates", _trainarrPublicationToken);
        publishRequest.Content = JsonContent.Create(new CreateCertificationPublicationRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            "qual.v1_certificate",
            "V1 Certificate Qualification",
            "missing_assignment",
            "V1 certificate alias test.",
            null));

        var publishResponse = await _trainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var publication = (await publishResponse.Content.ReadFromJsonAsync<CertificationPublicationResponse>())!;

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var delivery = await db.StaffarrPublicationDeliveries.SingleAsync(
            x => x.CertificationPublicationId == publication.PublicationId);
        Assert.Equal(StaffarrPublicationDeliveryStatuses.Delivered, delivery.DeliveryStatus);
    }

    private async Task SeedPendingBlockerDeliveryAsync(Guid personId, Guid publicationId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        db.TenantStaffarrPublicationSettings.Add(new TenantStaffarrPublicationSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            MaxAttempts = 10,
            RetryIntervalMinutes = 5,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.CertificationPublications.Add(new CertificationPublication
        {
            Id = publicationId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            QualificationKey = "qual.retry_worker",
            QualificationName = "Retry Worker Qualification",
            PublicationType = "training_blocker",
            BlockerType = "missing_assignment",
            Message = "Retry worker test requires assignment completion before gated work can resume.",
            Status = "published",
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        });

        var payload = StaffarrPublicationRetryService.SerializePayload(
            new StaffArrIngestTrainingBlockerPayload(
                PlatformSeeder.DemoTenantId,
                personId,
                publicationId,
                "qual.retry_worker",
                "Retry Worker Qualification",
                "missing_assignment",
                "Retry worker test requires assignment completion before gated work can resume.",
                null));

        db.StaffarrPublicationDeliveries.Add(new StaffarrPublicationDelivery
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            CertificationPublicationId = publicationId,
            StaffarrPersonId = personId,
            OperationKind = StaffarrPublicationOperationKinds.TrainingBlockerPublish,
            PayloadJson = payload,
            DeliveryStatus = StaffarrPublicationDeliveryStatuses.Pending,
            AttemptCount = 1,
            NextRetryAt = now.AddMinutes(-1),
            HttpStatusCode = 503,
            ErrorMessage = "Simulated StaffArr outage",
            CreatedAt = now.AddMinutes(-10),
            UpdatedAt = now.AddMinutes(-10),
        });

        await db.SaveChangesAsync();
    }

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<global::StaffArr.Api.Services.StaffArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);

        return accessToken;
    }

    private async Task SeedStaffPersonAsync(Guid personId, string displayName, string email)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var split = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = split.FirstOrDefault() ?? "User",
            FamilyName = split.Length > 1 ? string.Join(' ', split.Skip(1)) : "Test",
            DisplayName = displayName,
            PrimaryEmail = email,
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
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
            $"{sourceProduct}-pub-retry-{Guid.NewGuid():N}",
            $"{sourceProduct} publication retry test",
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
