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
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using TrainArr.Api.Contracts;
using TrainArr.Api.Data;
using TrainArr.Api.Entities;
using TrainArr.Api.Services;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrQualificationExpirationWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _trainarrToStaffarrToken = null!;
    private string _sharedWorkerToTrainarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"QualExpirationNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"QualExpirationStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"QualExpirationTrainArr-{Guid.NewGuid():N}";

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
        _trainarrToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            $"{StaffArrIntegration.TrainingBlockerIngestActionScope},{StaffArrIntegration.CertificationGrantIngestActionScope},{StaffArrIntegration.CertificationLifecycleIngestActionScope}");

        _sharedWorkerToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["trainarr"],
            QualificationExpirationService.ProcessExpirationsActionScope);

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
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("StaffArr:BaseUrl", _staffarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:ServiceToken", _trainarrToStaffarrToken);
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
    public async Task Process_expirations_rejects_missing_service_token()
    {
        var response = await _trainarrClient.PostAsJsonAsync(
            "/api/internal/qualifications/process-expirations",
            new ProcessQualificationExpirationsRequest(null, DateTimeOffset.UtcNow, 50));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_expirations_expires_past_due_qualification_and_updates_staffarr()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Expiration Worker", "expiration.worker@example.com");

        var issue = await SeedExpiredQualificationIssueAsync(personId);

        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/qualifications/process-expirations");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToTrainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessQualificationExpirationsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50));

        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessQualificationExpirationsResponse>())!;
        Assert.Equal(1, body.ExpiredCount);
        Assert.Contains(issue.Id, body.ExpiredQualificationIssueIds);

        using var trainarrScope = _trainarrFactory.Services.CreateScope();
        var trainarrDb = trainarrScope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var storedIssue = await trainarrDb.QualificationIssues.SingleAsync(x => x.Id == issue.Id);
        Assert.Equal("expired", storedIssue.Status);

        using var staffarrScope = _staffarrFactory.Services.CreateScope();
        var staffarrDb = staffarrScope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var certification = await staffarrDb.PersonCertifications.SingleAsync(
            x => x.ExternalPublicationId == issue.GrantPublicationId);
        Assert.Equal("expired", certification.Status);
    }

    [Fact]
    public async Task List_pending_expiration_returns_candidates_before_processing()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Pending Expiration", "pending.expiration@example.com");
        var issue = await SeedExpiredQualificationIssueAsync(personId);

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/qualifications/pending-expiration?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToTrainarrToken);

        var listResponse = await _trainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingQualificationExpirationsResponse>())!;
        Assert.Contains(pending.Items, x => x.QualificationIssueId == issue.Id);
    }

    private async Task<QualificationIssue> SeedExpiredQualificationIssueAsync(Guid personId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var grantPublicationId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(-2);
        var now = DateTimeOffset.UtcNow;

        db.CertificationPublications.Add(new CertificationPublication
        {
            Id = grantPublicationId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            QualificationKey = "expiration_worker",
            QualificationName = "Expiration Worker Qualification",
            PublicationType = "qualification_grant",
            BlockerType = "issued",
            Message = "Seeded qualification grant for expiration worker integration test.",
            Status = "published",
            ExpiresAt = expiresAt,
            PublishedAt = now.AddMonths(-6),
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6)
        });

        var assignmentId = Guid.NewGuid();
        db.TrainingAssignments.Add(new TrainingAssignment
        {
            Id = assignmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            TrainingDefinitionId = Guid.NewGuid(),
            AssignmentReason = "manual",
            Status = "completed",
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6)
        });

        var issue = new QualificationIssue
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            TrainingAssignmentId = assignmentId,
            StaffarrPersonId = personId,
            QualificationKey = "expiration_worker",
            QualificationName = "Expiration Worker Qualification",
            GrantPublicationId = grantPublicationId,
            Status = "issued",
            IssuedAt = now.AddMonths(-6),
            ExpiresAt = expiresAt,
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6)
        };
        db.QualificationIssues.Add(issue);

        await db.SaveChangesAsync();

        var grantClient = scope.ServiceProvider.GetRequiredService<StaffArrCertificationGrantClient>();
        await grantClient.IngestGrantAsync(
            new StaffArrIngestCertificationGrantPayload(
                PlatformSeeder.DemoTenantId,
                personId,
                grantPublicationId,
                assignmentId,
                issue.QualificationKey,
                issue.QualificationName,
                "Expiration worker training",
                now.AddMonths(-6),
                expiresAt,
                "Seeded grant for expiration worker integration test."),
            CancellationToken.None);

        return issue;
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
            $"{sourceProduct}-qual-expiration-{Guid.NewGuid():N}",
            $"{sourceProduct} qualification expiration test",
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
