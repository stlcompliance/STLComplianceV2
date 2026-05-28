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
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrQualificationLifecycleTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _trainarrToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"QualLifecycleNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"QualLifecycleStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"QualLifecycleTrainArr-{Guid.NewGuid():N}";

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
            $"{StaffArrIntegration.TrainingBlockerIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementReadActionScope},{StaffArrIntegration.CertificationGrantIngestActionScope},{StaffArrIntegration.CertificationLifecycleIngestActionScope}");

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
                RemoveDbContext<TrainArr.Api.Data.TrainArrDbContext>(services);
                services.AddDbContext<TrainArr.Api.Data.TrainArrDbContext>(options =>
                    options.UseInMemoryDatabase(trainArrDbName));

                services.AddHttpClient<TrainArr.Api.Services.StaffArrTrainingBlockerClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
                services.AddHttpClient<TrainArr.Api.Services.StaffArrCertificationGrantClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
                services.AddHttpClient<TrainArr.Api.Services.StaffArrCertificationLifecycleClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
                services.AddHttpClient<TrainArr.Api.Services.StaffArrTrainingAcknowledgementClient>()
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
    public async Task Qualification_suspend_publishes_staffarr_training_blocker()
    {
        var issue = await IssueQualificationAsync("lifecycle_suspend", "Lifecycle Suspend Subject");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var suspendRequest = Authorized(
            HttpMethod.Post,
            $"/api/qualification-issues/{issue.QualificationIssueId}/suspend",
            adminToken);
        suspendRequest.Content = JsonContent.Create(new QualificationLifecycleActionRequest(
            "Qualification suspended pending incident review and remediation."));
        var suspendResponse = await _trainarrClient.SendAsync(suspendRequest);
        suspendResponse.EnsureSuccessStatusCode();
        var suspended = (await suspendResponse.Content.ReadFromJsonAsync<QualificationIssueResponse>())!;
        Assert.Equal("suspended", suspended.Status);
        Assert.NotNull(suspended.LifecyclePublicationId);

        var staffarrToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var readinessResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{issue.StaffarrPersonId}/readiness", staffarrToken));
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
        Assert.Equal("not_ready", readiness.ReadinessStatus);
        Assert.Contains(
            readiness.Blockers,
            b => b.BlockerSource == "training" && b.BlockerType == "suspended");
    }

    [Fact]
    public async Task Qualification_revoke_updates_staffarr_certification_status()
    {
        var issue = await IssueQualificationAsync("lifecycle_revoke", "Lifecycle Revoke Subject");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var revokeRequest = Authorized(
            HttpMethod.Post,
            $"/api/qualification-issues/{issue.QualificationIssueId}/revoke",
            adminToken);
        revokeRequest.Content = JsonContent.Create(new QualificationLifecycleActionRequest(
            "Qualification revoked after policy violation and compliance review."));
        var revokeResponse = await _trainarrClient.SendAsync(revokeRequest);
        revokeResponse.EnsureSuccessStatusCode();
        var revoked = (await revokeResponse.Content.ReadFromJsonAsync<QualificationIssueResponse>())!;
        Assert.Equal("revoked", revoked.Status);

        var staffarrToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var certsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{issue.StaffarrPersonId}/certifications", staffarrToken));
        certsResponse.EnsureSuccessStatusCode();
        var certifications = (await certsResponse.Content.ReadFromJsonAsync<IReadOnlyList<PersonCertificationResponse>>())!;
        var trainarrCert = Assert.Single(certifications, c => c.ExternalPublicationId == issue.GrantPublicationId);
        Assert.Equal("revoked", trainarrCert.EffectiveStatus);
    }

    [Fact]
    public async Task Qualification_expire_updates_staffarr_certification_status()
    {
        var issue = await IssueQualificationAsync("lifecycle_expire", "Lifecycle Expire Subject");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var expireRequest = Authorized(
            HttpMethod.Post,
            $"/api/qualification-issues/{issue.QualificationIssueId}/expire",
            adminToken);
        expireRequest.Content = JsonContent.Create(new QualificationLifecycleActionRequest(
            "Qualification expired after validity period ended and renewal is required."));
        var expireResponse = await _trainarrClient.SendAsync(expireRequest);
        expireResponse.EnsureSuccessStatusCode();
        var expired = (await expireResponse.Content.ReadFromJsonAsync<QualificationIssueResponse>())!;
        Assert.Equal("expired", expired.Status);

        var staffarrToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var certsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{issue.StaffarrPersonId}/certifications", staffarrToken));
        certsResponse.EnsureSuccessStatusCode();
        var certifications = (await certsResponse.Content.ReadFromJsonAsync<IReadOnlyList<PersonCertificationResponse>>())!;
        var trainarrCert = Assert.Single(certifications, c => c.ExternalPublicationId == issue.GrantPublicationId);
        Assert.Equal("expired", trainarrCert.EffectiveStatus);
    }

    [Fact]
    public async Task Qualification_lifecycle_rejects_terminal_status_transition()
    {
        var issue = await IssueQualificationAsync("lifecycle_terminal", "Lifecycle Terminal Subject");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var revokeRequest = Authorized(
            HttpMethod.Post,
            $"/api/qualification-issues/{issue.QualificationIssueId}/revoke",
            adminToken);
        revokeRequest.Content = JsonContent.Create(new QualificationLifecycleActionRequest(
            "Qualification revoked after policy violation and compliance review."));
        (await _trainarrClient.SendAsync(revokeRequest)).EnsureSuccessStatusCode();

        var secondRevoke = Authorized(
            HttpMethod.Post,
            $"/api/qualification-issues/{issue.QualificationIssueId}/revoke",
            adminToken);
        secondRevoke.Content = JsonContent.Create(new QualificationLifecycleActionRequest(
            "Attempted duplicate revoke should be rejected by terminal status guard."));
        var secondResponse = await _trainarrClient.SendAsync(secondRevoke);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Certification_lifecycle_ingest_rejects_missing_service_token()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Lifecycle Auth", "lifecycle.auth@example.com");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/certification-lifecycle");
        request.Content = JsonContent.Create(new IngestCertificationLifecycleRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "revoke",
            "annual_compliance",
            "Annual Compliance",
            "Lifecycle ingest without service token should be rejected.",
            null));
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Certification_lifecycle_ingest_is_idempotent_by_lifecycle_publication_id()
    {
        var issue = await IssueQualificationAsync("lifecycle_idempotent", "Lifecycle Idempotent Subject");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var expireRequest = Authorized(
            HttpMethod.Post,
            $"/api/qualification-issues/{issue.QualificationIssueId}/expire",
            adminToken);
        expireRequest.Content = JsonContent.Create(new QualificationLifecycleActionRequest(
            "Qualification expired after validity period ended and renewal is required."));
        (await _trainarrClient.SendAsync(expireRequest)).EnsureSuccessStatusCode();

        using var trainarrScope = _trainarrFactory.Services.CreateScope();
        var trainarrDb = trainarrScope.ServiceProvider.GetRequiredService<TrainArr.Api.Data.TrainArrDbContext>();
        var storedIssue = await trainarrDb.QualificationIssues.SingleAsync(x => x.Id == issue.QualificationIssueId);
        Assert.NotNull(storedIssue.LifecyclePublicationId);

        var payload = new IngestCertificationLifecycleRequest(
            PlatformSeeder.DemoTenantId,
            issue.StaffarrPersonId,
            issue.GrantPublicationId,
            storedIssue.LifecyclePublicationId!.Value,
            "expire",
            issue.QualificationKey,
            issue.QualificationName,
            "Idempotent TrainArr certification lifecycle ingest replay test.",
            null);

        var first = await PostLifecycleAsync(payload);
        first.EnsureSuccessStatusCode();
        var firstBody = (await first.Content.ReadFromJsonAsync<CertificationLifecycleIngestionResponse>())!;

        var second = await PostLifecycleAsync(payload);
        second.EnsureSuccessStatusCode();
        var secondBody = (await second.Content.ReadFromJsonAsync<CertificationLifecycleIngestionResponse>())!;
        Assert.Equal(firstBody.PersonCertificationId, secondBody.PersonCertificationId);
    }

    private async Task<QualificationIssueResponse> IssueQualificationAsync(string qualificationKey, string displayName)
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, displayName, $"{qualificationKey}@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(
            adminToken,
            $"{qualificationKey}_training",
            $"{displayName} Training",
            qualificationKey,
            qualificationKey.Replace('_', ' '));

        var createRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            null,
            "manual",
            DateTimeOffset.UtcNow.AddDays(30)));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var assignment = (await createResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;

        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        await TrainArrCompletionTestHelper.SatisfyCompletionRequirementsAsync(
            _trainarrClient,
            assignment.AssignmentId,
            adminToken,
            memberToken);

        var completeRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignment.AssignmentId}/complete",
            adminToken);
        var completeResponse = await _trainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<CompleteTrainingAssignmentResponse>())!;
        Assert.NotNull(completed.QualificationIssue);
        return completed.QualificationIssue;
    }

    private async Task<HttpResponseMessage> PostLifecycleAsync(IngestCertificationLifecycleRequest payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/certification-lifecycle");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _trainarrToStaffarrToken);
        request.Content = JsonContent.Create(payload);
        return await _staffarrClient.SendAsync(request);
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(
        string trainarrAdminToken,
        string definitionKey,
        string name,
        string qualificationKey,
        string qualificationName)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", trainarrAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            definitionKey,
            name,
            $"Training for {qualificationName}.",
            qualificationKey,
            qualificationName));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var definition = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return definition.TrainingDefinitionId;
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArr.Api.Services.TrainArrTokenService>();
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
            $"{sourceProduct}-qual-lifecycle-{Guid.NewGuid():N}",
            $"{sourceProduct} qualification lifecycle test",
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
