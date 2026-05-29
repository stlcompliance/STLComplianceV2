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
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrCompletionRuleTests : IAsyncLifetime
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
        var nexArrDbName = $"CompletionRuleNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"CompletionRuleStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"CompletionRuleTrainArr-{Guid.NewGuid():N}";

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
            $"{StaffArrIntegration.TrainingBlockerIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementReadActionScope},{StaffArrIntegration.CertificationGrantIngestActionScope}");

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
    public async Task Completion_rule_catalog_and_crud_round_trip()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var catalogResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/training-completion-rules/catalog", adminToken));
        catalogResponse.EnsureSuccessStatusCode();
        var catalog = (await catalogResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingCompletionRuleCatalogItemResponse>>())!;
        Assert.Contains(catalog, item => item.RuleType == TrainingCompletionRuleTypes.RequiredEvaluatorPass);

        var createRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-definitions/{definitionId}/completion-rules",
            adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingDefinitionCompletionRuleRequest(
            "evaluator_only",
            TrainingCompletionRuleTypes.RequiredEvaluatorPass,
            "Passing evaluation only",
            "{}",
            0));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TrainingDefinitionCompletionRuleResponse>())!;
        Assert.Equal("evaluator_only", created.RuleKey);

        var listResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-definitions/{definitionId}/completion-rules", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var rules = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingDefinitionCompletionRuleResponse>>())!;
        Assert.Single(rules);

        var deleteResponse = await _trainarrClient.SendAsync(
            Authorized(
                HttpMethod.Delete,
                $"/api/training-definitions/{definitionId}/completion-rules/{created.CompletionRuleId}",
                adminToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Custom_evaluator_only_rule_allows_completion_without_signoffs()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Completion Rule Subject", "completion.rule@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var trainerToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_trainer");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        await CreateCompletionRuleAsync(
            adminToken,
            definitionId,
            "evaluator_only",
            TrainingCompletionRuleTypes.RequiredEvaluatorPass,
            "Passing evaluation only",
            "{}");

        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);

        var blockedComplete = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/complete", adminToken));
        Assert.Equal(HttpStatusCode.Conflict, blockedComplete.StatusCode);

        var evaluationRequest = Authorized(HttpMethod.Post, "/api/evaluations", trainerToken);
        evaluationRequest.Content = JsonContent.Create(new SubmitTrainingEvaluationRequest(
            assignmentId,
            "pass",
            95m,
            "Custom rule satisfied."));
        (await _trainarrClient.SendAsync(evaluationRequest)).EnsureSuccessStatusCode();

        var detailBeforeComplete = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-assignments/{assignmentId}", adminToken));
        detailBeforeComplete.EnsureSuccessStatusCode();
        var detail = (await detailBeforeComplete.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
        Assert.True(detail.CompletionRequirementsMet);
        Assert.Empty(detail.Signoffs);

        var completeResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/training-assignments/{assignmentId}/complete", adminToken));
        completeResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Legacy_defaults_apply_when_no_custom_rules_configured()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Legacy Default Subject", "legacy.default@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);

        var evaluationOnlyRequest = Authorized(HttpMethod.Post, "/api/evaluations", adminToken);
        evaluationOnlyRequest.Content = JsonContent.Create(new SubmitTrainingEvaluationRequest(
            assignmentId,
            "pass",
            100m,
            "Evaluation without signoffs."));
        (await _trainarrClient.SendAsync(evaluationOnlyRequest)).EnsureSuccessStatusCode();

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
    }

    private async Task CreateCompletionRuleAsync(
        string accessToken,
        Guid definitionId,
        string ruleKey,
        string ruleType,
        string label,
        string configJson)
    {
        var request = Authorized(
            HttpMethod.Post,
            $"/api/training-definitions/{definitionId}/completion-rules",
            accessToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionCompletionRuleRequest(
            ruleKey,
            ruleType,
            label,
            configJson,
            0));
        (await _trainarrClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateAssignmentAsync(string accessToken, Guid personId, Guid definitionId)
    {
        var created = await TrainArrQualificationCheckTestHelper.CreateManualAssignmentAsync(
            _trainarrClient,
            accessToken,
            personId,
            definitionId,
            "completion_rule_qual");
        return created.AssignmentId;
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string accessToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", accessToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"def_{Guid.NewGuid():N}"[..12],
            "Completion rule definition",
            "Definition for completion rule builder tests.",
            "completion_rule_qual",
            "Completion Rule Qual"));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return created.TrainingDefinitionId;
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string accessToken)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
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
            $"{sourceProduct}-completion-rule-{Guid.NewGuid():N}",
            $"{sourceProduct} completion rule test",
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

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<TContext>)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        var contextDescriptors = services.Where(d => d.ServiceType == typeof(TContext)).ToList();
        foreach (var descriptor in contextDescriptors)
        {
            services.Remove(descriptor);
        }
    }
}
