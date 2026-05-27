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
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrSignoffsEvaluationsTests : IAsyncLifetime
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
        var nexArrDbName = $"TrainSignoffNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"TrainSignoffStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"TrainSignoffTrainArr-{Guid.NewGuid():N}";

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
    public async Task Evaluation_submit_and_signoffs_list_on_assignment_detail()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Signoff Subject", "signoff.subject@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var trainerToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_trainer");
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);

        var evaluationRequest = Authorized(HttpMethod.Post, "/api/evaluations", trainerToken);
        evaluationRequest.Content = JsonContent.Create(new SubmitTrainingEvaluationRequest(
            assignmentId,
            "pass",
            92.5m,
            "Practical skills verified."));
        var evaluationResponse = await _trainarrClient.SendAsync(evaluationRequest);
        evaluationResponse.EnsureSuccessStatusCode();
        var evaluation = (await evaluationResponse.Content.ReadFromJsonAsync<TrainingEvaluationResponse>())!;
        Assert.Equal("pass", evaluation.Result);

        var traineeSignoffRequest = Authorized(HttpMethod.Post, "/api/signoffs", memberToken);
        traineeSignoffRequest.Content = JsonContent.Create(new SubmitTrainingSignoffRequest(
            assignmentId,
            "trainee",
            "I completed the training."));
        var traineeSignoffResponse = await _trainarrClient.SendAsync(traineeSignoffRequest);
        traineeSignoffResponse.EnsureSuccessStatusCode();

        var trainerSignoffRequest = Authorized(HttpMethod.Post, "/api/signoffs", trainerToken);
        trainerSignoffRequest.Content = JsonContent.Create(new SubmitTrainingSignoffRequest(
            assignmentId,
            "trainer",
            "Trainer approves qualification."));
        var trainerSignoffResponse = await _trainarrClient.SendAsync(trainerSignoffRequest);
        trainerSignoffResponse.EnsureSuccessStatusCode();

        var detailResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-assignments/{assignmentId}", adminToken));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
        Assert.NotNull(detail.Evaluation);
        Assert.Equal(2, detail.Signoffs.Count);
        Assert.True(detail.CompletionRequirementsMet);

        var listEvaluationsResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/evaluations?trainingAssignmentId={assignmentId}", adminToken));
        listEvaluationsResponse.EnsureSuccessStatusCode();
        var evaluations = (await listEvaluationsResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingEvaluationResponse>>())!;
        Assert.Single(evaluations);

        var listSignoffsResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/signoffs?trainingAssignmentId={assignmentId}", adminToken));
        listSignoffsResponse.EnsureSuccessStatusCode();
        var signoffs = (await listSignoffsResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingSignoffResponse>>())!;
        Assert.Equal(2, signoffs.Count);
    }

    [Fact]
    public async Task Complete_denied_until_evaluation_and_signoffs_recorded()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Gate Subject", "gate.subject@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);

        var blockedComplete = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/complete", adminToken));
        Assert.Equal(HttpStatusCode.Conflict, blockedComplete.StatusCode);

        await TrainArrCompletionTestHelper.SatisfyCompletionRequirementsAsync(
            _trainarrClient,
            assignmentId,
            adminToken,
            memberToken);

        var completeResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/complete", adminToken));
        completeResponse.EnsureSuccessStatusCode();

        var readinessResponse = await _staffarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/people/{personId}/readiness",
                CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_admin")));
        readinessResponse.EnsureSuccessStatusCode();
        var readiness = (await readinessResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
        Assert.DoesNotContain(readiness.Blockers, b => b.BlockerSource == "training");
    }

    [Fact]
    public async Task Evaluation_submit_denies_member_role()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Member Eval", "member.eval@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);

        var request = Authorized(HttpMethod.Post, "/api/evaluations", memberToken);
        request.Content = JsonContent.Create(new SubmitTrainingEvaluationRequest(
            assignmentId,
            "pass",
            null,
            null));
        var response = await _trainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Trainee_signoff_denies_trainer_for_other_person()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Trainee Only", "trainee.only@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var trainerToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_trainer");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);

        var request = Authorized(HttpMethod.Post, "/api/signoffs", trainerToken);
        request.Content = JsonContent.Create(new SubmitTrainingSignoffRequest(
            assignmentId,
            "trainee",
            "Trainer cannot sign as trainee."));
        var response = await _trainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Fail_evaluation_blocks_completion_gate()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Fail Eval", "fail.eval@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);

        var evaluationRequest = Authorized(HttpMethod.Post, "/api/evaluations", adminToken);
        evaluationRequest.Content = JsonContent.Create(new SubmitTrainingEvaluationRequest(
            assignmentId,
            "fail",
            40m,
            "Did not meet standard."));
        (await _trainarrClient.SendAsync(evaluationRequest)).EnsureSuccessStatusCode();

        var traineeSignoffRequest = Authorized(HttpMethod.Post, "/api/signoffs", memberToken);
        traineeSignoffRequest.Content = JsonContent.Create(new SubmitTrainingSignoffRequest(
            assignmentId,
            "trainee",
            null));
        (await _trainarrClient.SendAsync(traineeSignoffRequest)).EnsureSuccessStatusCode();

        var trainerSignoffRequest = Authorized(HttpMethod.Post, "/api/signoffs", adminToken);
        trainerSignoffRequest.Content = JsonContent.Create(new SubmitTrainingSignoffRequest(
            assignmentId,
            "trainer",
            null));
        (await _trainarrClient.SendAsync(trainerSignoffRequest)).EnsureSuccessStatusCode();

        var completeResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/complete", adminToken));
        Assert.Equal(HttpStatusCode.Conflict, completeResponse.StatusCode);
    }

    private async Task<Guid> CreateAssignmentAsync(string adminToken, Guid personId, Guid definitionId)
    {
        var createRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            null,
            "manual",
            null));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var assignment = (await createResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
        return assignment.AssignmentId;
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string trainarrAdminToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", trainarrAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"signoff_def_{Guid.NewGuid():N}"[..24],
            "Signoff-linked training",
            "Training definition used by signoff and evaluation tests.",
            "signoff_qualification",
            "Signoff Qualification"));
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
            $"{sourceProduct}-signoff-eval-{Guid.NewGuid():N}",
            $"{sourceProduct} signoff evaluation test",
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
