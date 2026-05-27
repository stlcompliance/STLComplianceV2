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

public class StaffArrTrainArrQualificationGrantTests : IAsyncLifetime
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
        var nexArrDbName = $"QualGrantNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"QualGrantStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"QualGrantTrainArr-{Guid.NewGuid():N}";

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
            $"{StaffArrIntegration.TrainingBlockerIngestActionScope},{StaffArrIntegration.CertificationGrantIngestActionScope}");

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
    public async Task Assignment_completion_issues_qualification_and_grants_staffarr_certification()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Qualification Grantee", "qual.grantee@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(
            adminToken,
            "annual_compliance",
            "Annual Compliance",
            "annual_compliance",
            "Annual Compliance Refresher");

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
        Assert.Equal("completed", completed.Status);
        Assert.NotNull(completed.QualificationIssue);
        Assert.Equal("issued", completed.QualificationIssue.Status);
        Assert.Equal("annual_compliance", completed.QualificationIssue.QualificationKey);

        var detailResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-assignments/{assignment.AssignmentId}", adminToken));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
        Assert.NotNull(detail.QualificationIssue);
        Assert.Equal(completed.QualificationIssue.QualificationIssueId, detail.QualificationIssue!.QualificationIssueId);

        var staffarrToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var certsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/certifications", staffarrToken));
        certsResponse.EnsureSuccessStatusCode();
        var certifications = (await certsResponse.Content.ReadFromJsonAsync<IReadOnlyList<PersonCertificationResponse>>())!;
        var trainarrCert = Assert.Single(certifications, c => c.SourceType == "trainarr_publication");
        Assert.Equal("active", trainarrCert.EffectiveStatus);
        Assert.Equal(completed.QualificationIssue.GrantPublicationId, trainarrCert.ExternalPublicationId);
        Assert.Equal("trainarr.annual_compliance", trainarrCert.CertificationKey);

        using var trainarrScope = _trainarrFactory.Services.CreateScope();
        var trainarrDb = trainarrScope.ServiceProvider.GetRequiredService<TrainArr.Api.Data.TrainArrDbContext>();
        var issue = await trainarrDb.QualificationIssues.SingleAsync(x => x.TrainingAssignmentId == assignment.AssignmentId);
        Assert.Equal(completed.QualificationIssue.QualificationIssueId, issue.Id);
        var publication = await trainarrDb.CertificationPublications.SingleAsync(x => x.Id == issue.GrantPublicationId);
        Assert.Equal("qualification_grant", publication.PublicationType);
    }

    [Fact]
    public async Task Readiness_qualification_grant_satisfies_readiness_requirement()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Readiness Qual Subject", "readiness.qual@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");

        var readinessBefore = await GetPersonReadinessAsync(personId);
        var safetyReq = readinessBefore.Requirements.Single(r => r.CertificationKey == "readiness.safety_orientation");
        Assert.Equal("missing", safetyReq.RequirementStatus);

        var definitionId = await CreateTrainingDefinitionAsync(
            adminToken,
            "safety_orientation_training",
            "Safety Orientation Training",
            "readiness.safety_orientation",
            "Safety Orientation");

        var createRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            null,
            "manual",
            null));
        var createResponseMessage = await _trainarrClient.SendAsync(createRequest);
        createResponseMessage.EnsureSuccessStatusCode();
        var assignment = (await createResponseMessage.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;

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
        (await _trainarrClient.SendAsync(completeRequest)).EnsureSuccessStatusCode();

        var staffarrToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var certsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/certifications", staffarrToken));
        certsResponse.EnsureSuccessStatusCode();
        var certifications = (await certsResponse.Content.ReadFromJsonAsync<IReadOnlyList<PersonCertificationResponse>>())!;
        Assert.Contains(
            certifications,
            c => c.CertificationKey == "readiness.safety_orientation" && c.SourceType == "trainarr_publication");

        var readinessAfter = await GetPersonReadinessAsync(personId);
        var safetyAfter = readinessAfter.Requirements.Single(r => r.CertificationKey == "readiness.safety_orientation");
        Assert.Equal("satisfied", safetyAfter.RequirementStatus);
    }

    [Fact]
    public async Task Certification_grant_ingest_rejects_missing_service_token()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Grant Auth", "grant.auth@example.com");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/certification-grants");
        request.Content = JsonContent.Create(new IngestCertificationGrantRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "annual_compliance",
            "Annual Compliance",
            "Annual Compliance Training",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(1),
            "Test grant without token."));
        var response = await _staffarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Certification_grant_ingest_is_idempotent_by_publication_id()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Idempotent Grant", "idempotent.grant@example.com");
        var publicationId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        var payload = new IngestCertificationGrantRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            publicationId,
            assignmentId,
            "forklift_ops",
            "Forklift Operations",
            "Forklift Operator Training",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(2),
            "Idempotent TrainArr certification grant ingest test.");

        var first = await PostGrantAsync(payload);
        first.EnsureSuccessStatusCode();
        var firstBody = (await first.Content.ReadFromJsonAsync<CertificationGrantIngestionResponse>())!;

        var second = await PostGrantAsync(payload);
        second.EnsureSuccessStatusCode();
        var secondBody = (await second.Content.ReadFromJsonAsync<CertificationGrantIngestionResponse>())!;
        Assert.Equal(firstBody.PersonCertificationId, secondBody.PersonCertificationId);
    }

    private async Task<HttpResponseMessage> PostGrantAsync(IngestCertificationGrantRequest payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/integrations/certification-grants");
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

    private async Task<PersonReadinessResponse> GetPersonReadinessAsync(Guid personId)
    {
        var token = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var response = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", token));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
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
            $"{sourceProduct}-qual-grant-{Guid.NewGuid():N}",
            $"{sourceProduct} qualification grant test",
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
