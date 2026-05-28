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

public class StaffArrTrainArrRecertificationAssignmentWorkerTests : IAsyncLifetime
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
        var nexArrDbName = $"RecertNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"RecertStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"RecertTrainArr-{Guid.NewGuid():N}";

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
            RecertificationAssignmentService.ProcessAssignmentsActionScope);

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
    public async Task Process_recertification_batch_rejects_missing_service_token()
    {
        var response = await _trainarrClient.PostAsJsonAsync(
            "/api/internal/recertification/process-batch",
            new ProcessRecertificationAssignmentsRequest(null, DateTimeOffset.UtcNow, 50));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_recertification_batch_creates_assignment_for_expiring_qualification()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Recert Worker", "recert.worker@example.com");

        var (issue, definitionId) = await SeedExpiringQualificationIssueAsync(personId);
        await SeedRecertificationSettingsAsync(PlatformSeeder.DemoTenantId, leadDays: 30);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/recertification/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessRecertificationAssignmentsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50));

        var processResponse = await _trainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessRecertificationAssignmentsResponse>())!;
        Assert.Equal(1, body.AssignedCount);
        Assert.Single(body.CreatedAssignmentIds);

        using var trainarrScope = _trainarrFactory.Services.CreateScope();
        var trainarrDb = trainarrScope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var assignment = await trainarrDb.TrainingAssignments.SingleAsync(x => x.Id == body.CreatedAssignmentIds[0]);
        Assert.Equal("recertification", assignment.AssignmentReason);
        Assert.Equal(issue.Id, assignment.SourceQualificationIssueId);
        Assert.Equal(definitionId, assignment.TrainingDefinitionId);
        Assert.Equal("assigned", assignment.Status);
        Assert.NotNull(assignment.BlockerPublicationId);

        using var staffarrScope = _staffarrFactory.Services.CreateScope();
        var staffarrDb = staffarrScope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var blocker = await staffarrDb.PersonTrainingBlockers.SingleAsync(
            x => x.TrainarrPublicationId == assignment.BlockerPublicationId);
        Assert.Equal("missing_assignment", blocker.BlockerType);
    }

    [Fact]
    public async Task List_pending_recertification_returns_candidates_before_processing()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Pending Recert", "pending.recert@example.com");
        var (issue, _) = await SeedExpiringQualificationIssueAsync(personId);
        await SeedRecertificationSettingsAsync(PlatformSeeder.DemoTenantId, leadDays: 30);

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/recertification/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sharedWorkerToTrainarrToken);

        var listResponse = await _trainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingRecertificationCandidatesResponse>())!;
        Assert.Contains(pending.Items, x => x.QualificationIssueId == issue.Id);
    }

    private async Task<(QualificationIssue Issue, Guid DefinitionId)> SeedExpiringQualificationIssueAsync(Guid personId)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var grantPublicationId = Guid.NewGuid();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(14);
        var now = DateTimeOffset.UtcNow;

        var definitionId = Guid.NewGuid();
        db.TrainingDefinitions.Add(new TrainingDefinition
        {
            Id = definitionId,
            TenantId = PlatformSeeder.DemoTenantId,
            DefinitionKey = "recert_worker_def",
            Name = "Recertification Worker Training",
            Description = "Seeded definition for recertification worker test.",
            QualificationKey = "recert_worker",
            QualificationName = "Recertification Worker Qualification",
            Status = "active",
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6),
        });

        db.CertificationPublications.Add(new CertificationPublication
        {
            Id = grantPublicationId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            QualificationKey = "recert_worker",
            QualificationName = "Recertification Worker Qualification",
            PublicationType = "qualification_grant",
            BlockerType = "issued",
            Message = "Seeded qualification grant for recertification worker integration test.",
            Status = "published",
            ExpiresAt = expiresAt,
            PublishedAt = now.AddMonths(-6),
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6),
        });

        var assignmentId = Guid.NewGuid();
        db.TrainingAssignments.Add(new TrainingAssignment
        {
            Id = assignmentId,
            TenantId = PlatformSeeder.DemoTenantId,
            StaffarrPersonId = personId,
            TrainingDefinitionId = definitionId,
            AssignmentReason = "manual",
            Status = "completed",
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6),
        });

        var issue = new QualificationIssue
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            TrainingAssignmentId = assignmentId,
            StaffarrPersonId = personId,
            QualificationKey = "recert_worker",
            QualificationName = "Recertification Worker Qualification",
            GrantPublicationId = grantPublicationId,
            Status = "issued",
            IssuedAt = now.AddMonths(-6),
            ExpiresAt = expiresAt,
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6),
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
                "Recertification worker training",
                now.AddMonths(-6),
                expiresAt,
                "Seeded grant for recertification worker integration test."),
            CancellationToken.None);

        return (issue, definitionId);
    }

    private async Task SeedRecertificationSettingsAsync(Guid tenantId, int leadDays)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantRecertificationSettings.Add(new TenantRecertificationSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            IsEnabled = true,
            LeadDays = leadDays,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
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
            UpdatedAt = DateTimeOffset.UtcNow,
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
            $"{sourceProduct}-recert-{Guid.NewGuid():N}",
            $"{sourceProduct} recertification assignment test",
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
