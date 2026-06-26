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
using TrainArr.Api.Contracts;
using TrainArrIntegration = TrainArr.Api.Endpoints.IntegrationEndpoints;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrTrainingAssignmentTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _staffarrToTrainarrToken = null!;
    private string _trainarrToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"TrainAssignNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"TrainAssignStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"TrainAssignTrainArr-{Guid.NewGuid():N}";

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
        _staffarrToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            ["trainarr"],
            TrainArrIntegration.IncidentRemediationIngestActionScope);
        _trainarrToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            $"{StaffArrIntegration.TrainingBlockerIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementReadActionScope},{StaffArrIntegration.CertificationGrantIngestActionScope}");

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("TrainArr:BaseUrl", "http://localhost:5103");
            builder.UseSetting("TrainArr:ServiceToken", _staffarrToTrainarrToken);
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
                services.AddHttpClient<TrainArr.Api.Services.StaffArrTrainingAcknowledgementClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        _staffarrFactory = _staffarrFactory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("TrainArr:BaseUrl", _trainarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient<global::StaffArr.Api.Services.TrainArrIncidentRemediationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _trainarrFactory.Server.CreateHandler());
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
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
    public async Task Remediation_to_assignment_completion_clears_staffarr_training_blocker()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Assignment Subject", "assignment.subject@example.com");
        var remediationId = await RouteIncidentToTrainarrAsync(personId);
        var trainarrAdminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(trainarrAdminToken);

        var assignment = await TrainArrQualificationCheckTestHelper.CreateRemediationAssignmentAsync(
            _trainarrClient,
            trainarrAdminToken,
            personId,
            definitionId,
            "annual_compliance",
            remediationId,
            DateTimeOffset.UtcNow.AddDays(14));
        Assert.Equal("assigned", assignment.Status);
        Assert.Equal(remediationId, assignment.StaffarrIncidentRemediationId);
        Assert.NotNull(assignment.BlockerPublicationId);

        var readinessBefore = await GetPersonReadinessAsync(personId);
        Assert.Equal("not_ready", readinessBefore.ReadinessStatus);
        Assert.Contains(readinessBefore.Blockers, b => b.BlockerSource == "training");

        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        await TrainArrCompletionTestHelper.SatisfyCompletionRequirementsAsync(
            _trainarrClient,
            assignment.AssignmentId,
            trainarrAdminToken,
            memberToken);

        var completeRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignment.AssignmentId}/complete",
            trainarrAdminToken);
        var completeResponse = await _trainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = (await completeResponse.Content.ReadFromJsonAsync<CompleteTrainingAssignmentResponse>())!;
        Assert.Equal("completed", completed.Status);
        Assert.NotNull(completed.QualificationIssue);
        Assert.Equal("issued", completed.QualificationIssue.Status);

        var staffarrToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var certsResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/people/{personId}/certifications", staffarrToken));
        certsResponse.EnsureSuccessStatusCode();
        var certifications = (await certsResponse.Content.ReadFromJsonAsync<IReadOnlyList<PersonCertificationResponse>>())!;
        Assert.Contains(certifications, c => c.SourceType == "trainarr_publication");

        var readinessAfter = await GetPersonReadinessAsync(personId);
        Assert.DoesNotContain(readinessAfter.Blockers, b => b.BlockerSource == "training");

        using var scope = _trainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrainArr.Api.Data.TrainArrDbContext>();
        var remediation = await db.StaffarrIncidentRemediations.SingleAsync(x => x.Id == remediationId);
        Assert.Equal("completed", remediation.Status);
    }

    [Fact]
    public async Task Training_assignment_create_denies_member_role()
    {
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");
        var request = Authorized(HttpMethod.Post, "/api/training-assignments", memberToken);
        request.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "manual",
            null));
        var response = await _trainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Training_assignment_create_denies_platform_admin_without_trainarr_role()
    {
        var platformAdminToken = CreateTrainArrAccessToken(
            ["trainarr"],
            tenantRoleKey: "tenant_member",
            isPlatformAdmin: true);
        var request = Authorized(HttpMethod.Post, "/api/training-assignments", platformAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "manual",
            null));
        var response = await _trainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Training_assignment_list_allows_member_self_scope()
    {
        var personId = Guid.NewGuid();
        var otherPersonId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Member Trainee", "member.trainee@example.com");
        await SeedStaffPersonAsync(otherPersonId, "Other Trainee", "other.trainee@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        await TrainArrQualificationCheckTestHelper.CreateManualAssignmentAsync(
            _trainarrClient,
            adminToken,
            personId,
            definitionId,
            "annual_compliance");
        await TrainArrQualificationCheckTestHelper.CreateManualAssignmentAsync(
            _trainarrClient,
            adminToken,
            otherPersonId,
            definitionId,
            "annual_compliance");

        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var listResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/training-assignments", memberToken));
        listResponse.EnsureSuccessStatusCode();
        var assignments = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingAssignmentSummaryResponse>>())!;
        Assert.Single(assignments);
        Assert.Equal(personId, assignments[0].StaffarrPersonId);
    }

    [Fact]
    public async Task Training_assignment_list_platform_admin_member_is_still_self_scoped()
    {
        var personId = Guid.NewGuid();
        var otherPersonId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Platform Member Trainee", "platform.member.trainee@example.com");
        await SeedStaffPersonAsync(otherPersonId, "Other Platform Trainee", "other.platform.trainee@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        await TrainArrQualificationCheckTestHelper.CreateManualAssignmentAsync(
            _trainarrClient,
            adminToken,
            personId,
            definitionId,
            "annual_compliance");
        await TrainArrQualificationCheckTestHelper.CreateManualAssignmentAsync(
            _trainarrClient,
            adminToken,
            otherPersonId,
            definitionId,
            "annual_compliance");

        var platformMemberToken = CreateTrainArrAccessToken(
            ["trainarr"],
            tenantRoleKey: "tenant_member",
            personId: personId,
            isPlatformAdmin: true);
        var listResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/training-assignments", platformMemberToken));
        listResponse.EnsureSuccessStatusCode();
        var assignments = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingAssignmentSummaryResponse>>())!;
        Assert.Single(assignments);
        Assert.Equal(personId, assignments[0].StaffarrPersonId);
    }

    [Fact]
    public async Task Training_assignment_rejects_duplicate_active_remediation_assignment()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Duplicate Subject", "duplicate.subject@example.com");
        var remediationId = await RouteIncidentToTrainarrAsync(personId);
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var firstAssignment = await TrainArrQualificationCheckTestHelper.CreateRemediationAssignmentAsync(
            _trainarrClient,
            adminToken,
            personId,
            definitionId,
            "annual_compliance",
            remediationId);
        Assert.Equal("assigned", firstAssignment.Status);

        var secondCheck = await TrainArrQualificationCheckTestHelper.RunQualificationCheckAsync(
            _trainarrClient,
            adminToken,
            personId,
            "annual_compliance",
            definitionId);

        var secondRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        secondRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            remediationId,
            "incident_remediation",
            null,
            secondCheck.CheckId));
        var secondResponse = await _trainarrClient.SendAsync(secondRequest);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    private async Task<Guid> RouteIncidentToTrainarrAsync(Guid personId)
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin");
        var createRequest = Authorized(HttpMethod.Post, "/api/incidents", adminToken);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "training_compliance",
            "high",
            "Missed annual compliance training",
            "Employee missed required annual compliance training and must complete remediation before assignment.",
            DateTimeOffset.UtcNow.AddHours(-2)));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var incident = (await createResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;

        var routeRequest = Authorized(HttpMethod.Post, $"/api/incidents/{incident.IncidentId}/route-to-trainarr", adminToken);
        var routeResponse = await _staffarrClient.SendAsync(routeRequest);
        routeResponse.EnsureSuccessStatusCode();
        var routed = (await routeResponse.Content.ReadFromJsonAsync<RouteIncidentToTrainarrResponse>())!;
        return routed.TrainarrRouting.TrainarrRemediationId;
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string trainarrAdminToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", trainarrAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            "annual_compliance",
            "Annual Compliance Refresher",
            "Required annual compliance training for all operational staff.",
            "annual_compliance",
            "Annual Compliance"));
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
        Guid? personId = null,
        bool isPlatformAdmin = false)
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
            isPlatformAdmin);

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
            $"{sourceProduct}-train-assign-{Guid.NewGuid():N}",
            $"{sourceProduct} train assignment test",
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
