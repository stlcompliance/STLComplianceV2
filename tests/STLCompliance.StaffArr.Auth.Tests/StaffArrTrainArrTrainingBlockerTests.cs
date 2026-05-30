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
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using TrainArr.Api.Contracts;
using TrainArr.Api.Endpoints;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrTrainingBlockerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _staffarrIntegrationToken = null!;
    private string _trainarrPublicationToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"StaffArrTrainBlockerNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"StaffArrTrainBlockerStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"StaffArrTrainBlockerTrainArr-{Guid.NewGuid():N}";

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
            StaffArrIntegration.TrainingBlockerIngestActionScope);
        _trainarrPublicationToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["trainarr", "staffarr"],
            $"{CertificationPublicationEndpoints.PublicationActionScope},{StaffArrIntegration.TrainingBlockerIngestActionScope}");

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
                RemoveDbContext<TrainArr.Api.Data.TrainArrDbContext>(services);
                services.AddDbContext<TrainArr.Api.Data.TrainArrDbContext>(options =>
                    options.UseInMemoryDatabase(trainArrDbName));

                services.AddHttpClient<TrainArr.Api.Services.StaffArrTrainingBlockerClient>()
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
    public async Task Training_blocker_ingest_shows_on_person_readiness()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Training Blocked User", "training.blocked@example.com");
        var publicationId = Guid.NewGuid();

        var ingestRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/training-blockers",
            _staffarrIntegrationToken);
        ingestRequest.Content = JsonContent.Create(new IngestTrainingBlockerRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            publicationId,
            "qual.hazmat_remediation",
            "Hazmat Remediation",
            "overdue",
            "Required hazmat remediation training is overdue and must be completed before assignment.",
            null));

        var ingestResponse = await _staffarrClient.SendAsync(ingestRequest);
        ingestResponse.EnsureSuccessStatusCode();

        var userToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var readinessResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", userToken));
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Equal("training_blockers", readiness.ReadinessBasis);
        var trainingBlocker = Assert.Single(readiness.Blockers.Where(x => x.BlockerSource == "training"));
        Assert.Equal("qual.hazmat_remediation", trainingBlocker.QualificationKey);
        Assert.Equal("overdue", trainingBlocker.BlockerType);
        Assert.Contains("overdue", trainingBlocker.Message, StringComparison.OrdinalIgnoreCase);

        var v1PersonId = Guid.NewGuid();
        await SeedStaffPersonAsync(v1PersonId, "Training Blocked V1 User", "training.blocked.v1@example.com");
        var v1PublicationId = Guid.NewGuid();

        var v1IngestRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/v1/integrations/training-blockers",
            _staffarrIntegrationToken);
        v1IngestRequest.Content = JsonContent.Create(new IngestTrainingBlockerRequest(
            PlatformSeeder.DemoTenantId,
            v1PersonId,
            v1PublicationId,
            "qual.hazmat_remediation.v1",
            "Hazmat Remediation V1",
            "overdue",
            "V1 route should ingest training blockers successfully.",
            null));

        var v1IngestResponse = await _staffarrClient.SendAsync(v1IngestRequest);
        v1IngestResponse.EnsureSuccessStatusCode();

        var v1ReadinessResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{v1PersonId}/readiness", userToken));
        v1ReadinessResponse.EnsureSuccessStatusCode();
        var v1Readiness = (await v1ReadinessResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
        Assert.Contains(v1Readiness.Blockers, x => x.BlockerSource == "training" && x.QualificationKey == "qual.hazmat_remediation.v1");
    }

    [Fact]
    public async Task Trainarr_publication_publishes_training_blocker_to_staffarr_readiness()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Publication Blocked User", "publication.blocked@example.com");

        var publishRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/certification-publications",
            _trainarrPublicationToken);
        publishRequest.Content = JsonContent.Create(new CreateCertificationPublicationRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            "qual.forklift_practical",
            "Forklift Practical Evaluation",
            "missing_assignment",
            "Forklift practical evaluation must be assigned and completed before this person can operate equipment.",
            null));

        var publishResponse = await _trainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var publication = (await publishResponse.Content.ReadFromJsonAsync<CertificationPublicationResponse>())!;

        var userToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var readinessResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", userToken));
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.Equal("not_ready", readiness.ReadinessStatus);
        var trainingBlocker = Assert.Single(readiness.Blockers.Where(x => x.BlockerSource == "training"));
        Assert.Equal("qual.forklift_practical", trainingBlocker.QualificationKey);
        Assert.Equal("missing_assignment", trainingBlocker.BlockerType);
    }

    [Fact]
    public async Task Training_blocker_ingest_rejects_missing_service_token()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Token Denied User", "token.denied@example.com");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/training-blockers");
        request.Content = JsonContent.Create(new IngestTrainingBlockerRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            Guid.NewGuid(),
            "qual.test",
            "Test Qualification",
            "failed",
            "Training attempt failed and must be remediated before assignment.",
            null));

        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Training_blocker_clear_removes_active_blocker_from_readiness()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Cleared Blocker User", "cleared.blocker@example.com");
        var publicationId = Guid.NewGuid();

        var ingestRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/training-blockers",
            _staffarrIntegrationToken);
        ingestRequest.Content = JsonContent.Create(new IngestTrainingBlockerRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            publicationId,
            "qual.clear_me",
            "Clear Me Training",
            "failed",
            "Training remediation is required before this person can be assigned to gated work.",
            null));
        (await _staffarrClient.SendAsync(ingestRequest)).EnsureSuccessStatusCode();

        var clearRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/training-blockers/clear",
            _staffarrIntegrationToken);
        clearRequest.Content = JsonContent.Create(new ClearTrainingBlockerRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            publicationId));
        (await _staffarrClient.SendAsync(clearRequest)).EnsureSuccessStatusCode();

        var userToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var readinessResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", userToken));
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;

        Assert.DoesNotContain(readiness.Blockers, x => x.BlockerSource == "training");

        var v1PersonId = Guid.NewGuid();
        await SeedStaffPersonAsync(v1PersonId, "Cleared V1 Blocker User", "cleared.v1.blocker@example.com");
        var v1PublicationId = Guid.NewGuid();

        var v1IngestRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/v1/integrations/training-blockers",
            _staffarrIntegrationToken);
        v1IngestRequest.Content = JsonContent.Create(new IngestTrainingBlockerRequest(
            PlatformSeeder.DemoTenantId,
            v1PersonId,
            v1PublicationId,
            "qual.clear_me_v1",
            "Clear Me Training V1",
            "failed",
            "V1 route blocker should be clearable.",
            null));
        (await _staffarrClient.SendAsync(v1IngestRequest)).EnsureSuccessStatusCode();

        var v1ClearRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/v1/integrations/training-blockers/clear",
            _staffarrIntegrationToken);
        v1ClearRequest.Content = JsonContent.Create(new ClearTrainingBlockerRequest(
            PlatformSeeder.DemoTenantId,
            v1PersonId,
            v1PublicationId));
        (await _staffarrClient.SendAsync(v1ClearRequest)).EnsureSuccessStatusCode();

        var v1ReadinessResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{v1PersonId}/readiness", userToken));
        v1ReadinessResponse.EnsureSuccessStatusCode();
        var v1Readiness = (await v1ReadinessResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
        Assert.DoesNotContain(v1Readiness.Blockers, x => x.BlockerSource == "training" && x.QualificationKey == "qual.clear_me_v1");
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-integration-test-{Guid.NewGuid():N}",
            $"{sourceProduct} integration test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string url, string serviceToken) =>
        Authorized(method, url, serviceToken);

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
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
}
