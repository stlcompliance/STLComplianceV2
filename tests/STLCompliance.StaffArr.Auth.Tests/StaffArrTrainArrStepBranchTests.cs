using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using StaffArr.Api.Data;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using StaffArr.Api.Entities;
using TrainArr.Api.Contracts;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrStepBranchTests : IAsyncLifetime
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
        var nexArrDbName = $"StepBranchNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"StepBranchStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"StepBranchTrainArr-{Guid.NewGuid():N}";

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

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        _trainarrToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            $"{StaffArrIntegration.TrainingBlockerIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementReadActionScope},{StaffArrIntegration.CertificationGrantIngestActionScope}");

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
    public async Task Step_branch_catalog_and_crud_round_trip()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var quizStepId = await CreateQuizStepAsync(adminToken, definitionId, "gate-quiz", "Gate quiz");
        await CreateContentStepAsync(adminToken, definitionId, "remediation-review", "Remediation review", sortOrder: 1);

        var catalogResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/training-step-branches/catalog", adminToken));
        catalogResponse.EnsureSuccessStatusCode();
        var catalog = (await catalogResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingStepBranchCatalogItemResponse>>())!;
        Assert.Contains(catalog, item => item.BranchType == TrainingStepBranchTypes.QuizFailedRemediation);

        var createRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-definitions/{definitionId}/steps/{quizStepId}/branches",
            adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingDefinitionStepBranchRequest(
            "quiz_fail_remediation",
            TrainingStepBranchTypes.QuizFailedRemediation,
            "Unlock remediation on quiz fail",
            """{"targetStepKey":"remediation-review"}""",
            0));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TrainingDefinitionStepBranchResponse>())!;
        Assert.Equal("quiz_fail_remediation", created.BranchKey);

        var listResponse = await _trainarrClient.SendAsync(
            Authorized(
                HttpMethod.Get,
                $"/api/training-definitions/{definitionId}/steps/{quizStepId}/branches",
                adminToken));
        listResponse.EnsureSuccessStatusCode();
        var branches = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingDefinitionStepBranchResponse>>())!;
        Assert.Single(branches);

        var deleteResponse = await _trainarrClient.SendAsync(
            Authorized(
                HttpMethod.Delete,
                $"/api/training-definitions/{definitionId}/steps/{quizStepId}/branches/{created.BranchId}",
                adminToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Quiz_failure_unlocks_remediation_step_and_marks_assignment()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Branch Subject", "branch.subject@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var quizStepId = await CreateQuizStepAsync(adminToken, definitionId, "safety-quiz", "Safety quiz", sortOrder: 0);
        await CreateContentStepAsync(adminToken, definitionId, "remediation-review", "Remediation review", sortOrder: 1);
        await CreateStepBranchAsync(
            adminToken,
            definitionId,
            quizStepId,
            "quiz_fail_remediation",
            TrainingStepBranchTypes.QuizFailedRemediation,
            """{"targetStepKey":"remediation-review"}""");

        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);
        var stepsBefore = await LoadAssignmentStepsAsync(adminToken, assignmentId);
        var remediationBefore = stepsBefore.Single(x => x.StepKey == "remediation-review");
        Assert.Equal("hidden", remediationBefore.Status);
        Assert.False(remediationBefore.IsVisible);

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/steps/{quizStepId}/submit",
            memberToken);
        submitRequest.Content = JsonContent.Create(new SubmitTrainingAssignmentStepRequest(
            [2],
            null,
            "Failed attempt."));
        var submitResponse = await _trainarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        var detailResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-assignments/{assignmentId}", adminToken));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
        Assert.Equal("remediation_required", detail.Status);

        var stepsAfter = await LoadAssignmentStepsAsync(adminToken, assignmentId);
        var remediationAfter = stepsAfter.Single(x => x.StepKey == "remediation-review");
        Assert.Equal("pending", remediationAfter.Status);
        Assert.True(remediationAfter.IsVisible);
    }

    [Fact]
    public async Task Practical_step_requires_failure_comments_and_persists_structured_observation_fields()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Practical Subject", "practical.subject@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var trainerToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_trainer", personId: personId);
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var practicalStepId = await CreatePracticalStepAsync(adminToken, definitionId, "practical-check", "Practical check", sortOrder: 0);
        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);

        var failRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/steps/{practicalStepId}/submit",
            trainerToken);
        failRequest.Content = JsonContent.Create(new SubmitTrainingAssignmentStepRequest(
            SelectedOptionIndexes: null,
            PracticalResult: "fail",
            Notes: "Observed the trainee under evaluation.",
            PracticalObservationNotes: "Approach, setup, and shutdown were observed.",
            SafetyCriticalFailure: true,
            FailureComments: "The trainee skipped a required safety check.",
            TraineeAcknowledged: true,
            RetestRequired: true));
        var failResponse = await _trainarrClient.SendAsync(failRequest);
        failResponse.EnsureSuccessStatusCode();

        var stepsAfter = await LoadAssignmentStepsAsync(adminToken, assignmentId);
        var practicalAfter = stepsAfter.Single(x => x.StepKey == "practical-check");
        Assert.Equal("failed", practicalAfter.Status);
        Assert.NotNull(practicalAfter.ResponseJson);
        Assert.Contains("safetyCriticalFailure", practicalAfter.ResponseJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("failureComments", practicalAfter.ResponseJson, StringComparison.OrdinalIgnoreCase);

        var assignmentResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-assignments/{assignmentId}", adminToken));
        assignmentResponse.EnsureSuccessStatusCode();
        var detail = (await assignmentResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
        Assert.False(detail.CompletionRequirementsMet);

        var missingCommentsRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/steps/{practicalStepId}/submit",
            trainerToken);
        missingCommentsRequest.Content = JsonContent.Create(new SubmitTrainingAssignmentStepRequest(
            SelectedOptionIndexes: null,
            PracticalResult: "fail",
            Notes: "Observed the trainee under evaluation.",
            PracticalObservationNotes: "Approach, setup, and shutdown were observed.",
            SafetyCriticalFailure: false,
            FailureComments: null,
            TraineeAcknowledged: true,
            RetestRequired: true));
        var missingCommentsResponse = await _trainarrClient.SendAsync(missingCommentsRequest);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, missingCommentsResponse.StatusCode);
    }

    [Fact]
    public async Task Content_step_renders_lesson_config_and_requires_acknowledgement_when_configured()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Lesson Subject", "lesson.subject@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var contentStepId = await CreateLessonContentStepAsync(adminToken, definitionId, "lesson-ack", "Lesson acknowledgment", sortOrder: 0);
        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);

        var missingAckRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/steps/{contentStepId}/submit",
            memberToken);
        missingAckRequest.Content = JsonContent.Create(new SubmitTrainingAssignmentStepRequest(
            SelectedOptionIndexes: null,
            PracticalResult: null,
            Notes: "Lesson reviewed.",
            ContentAcknowledged: false));
        var missingAckResponse = await _trainarrClient.SendAsync(missingAckRequest);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, missingAckResponse.StatusCode);

        var submitRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/steps/{contentStepId}/submit",
            memberToken);
        submitRequest.Content = JsonContent.Create(new SubmitTrainingAssignmentStepRequest(
            SelectedOptionIndexes: null,
            PracticalResult: null,
            Notes: "Lesson reviewed.",
            ContentAcknowledged: true));
        var submitResponse = await _trainarrClient.SendAsync(submitRequest);
        submitResponse.EnsureSuccessStatusCode();

        var stepsAfter = await LoadAssignmentStepsAsync(adminToken, assignmentId);
        var contentAfter = stepsAfter.Single(x => x.StepKey == "lesson-ack");
        Assert.Equal("completed", contentAfter.Status);
        Assert.NotNull(contentAfter.ResponseJson);
        Assert.Contains("acknowledged", contentAfter.ResponseJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("contentTitle", contentAfter.ResponseJson, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Visibility_branch_hides_step_until_dependency_status_met()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Visibility Subject", "visibility.subject@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);
        var introStepId = await CreateContentStepAsync(adminToken, definitionId, "intro", "Intro", sortOrder: 0);
        var followUpStepId = await CreateContentStepAsync(adminToken, definitionId, "follow-up", "Follow up", sortOrder: 1);
        await CreateStepBranchAsync(
            adminToken,
            definitionId,
            followUpStepId,
            "show_after_intro",
            TrainingStepBranchTypes.StepVisibility,
            """{"dependsOnStepKey":"intro","requiredStatus":"completed"}""");

        var assignmentId = await CreateAssignmentAsync(adminToken, personId, definitionId);
        var initialSteps = await LoadAssignmentStepsAsync(adminToken, assignmentId);
        var followUpInitial = initialSteps.Single(x => x.StepKey == "follow-up");
        Assert.Equal("hidden", followUpInitial.Status);

        var completeIntro = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignmentId}/steps/{introStepId}/submit",
            memberToken);
        completeIntro.Content = JsonContent.Create(new SubmitTrainingAssignmentStepRequest(null, null, "Done."));
        (await _trainarrClient.SendAsync(completeIntro)).EnsureSuccessStatusCode();

        var stepsAfterIntro = await LoadAssignmentStepsAsync(adminToken, assignmentId);
        var followUpAfter = stepsAfterIntro.Single(x => x.StepKey == "follow-up");
        Assert.Equal("pending", followUpAfter.Status);
        Assert.True(followUpAfter.IsVisible);
    }

    private async Task<IReadOnlyList<TrainingAssignmentStepProgressResponse>> LoadAssignmentStepsAsync(
        string accessToken,
        Guid assignmentId)
    {
        var response = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-assignments/{assignmentId}/steps", accessToken));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IReadOnlyList<TrainingAssignmentStepProgressResponse>>())!;
    }

    private async Task CreateStepBranchAsync(
        string accessToken,
        Guid definitionId,
        Guid stepId,
        string branchKey,
        string branchType,
        string configJson)
    {
        var request = Authorized(
            HttpMethod.Post,
            $"/api/training-definitions/{definitionId}/steps/{stepId}/branches",
            accessToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionStepBranchRequest(
            branchKey,
            branchType,
            "Branch rule",
            configJson,
            0));
        (await _trainarrClient.SendAsync(request)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateQuizStepAsync(
        string accessToken,
        Guid definitionId,
        string stepKey,
        string name,
        int sortOrder = 0)
    {
        var configJson = """
            {
              "passingScorePercent": 80,
              "questions": [
                {
                  "questionKey": "q1",
                  "prompt": "Pick the safe action.",
                  "options": ["Evacuate", "Ignore", "Disable alarm"],
                  "correctOptionIndex": 0
                }
              ]
            }
            """;
        return await CreateStepAsync(accessToken, definitionId, stepKey, name, TrainingStepTypes.Quiz, configJson, sortOrder);
    }

    private async Task<Guid> CreatePracticalStepAsync(
        string accessToken,
        Guid definitionId,
        string stepKey,
        string name,
        int sortOrder = 0)
    {
        var configJson = """
            {
              "skillTaskName": "Demonstrate the required procedure under evaluator observation.",
              "passCriteria": "Complete the task safely, in the correct order, without critical errors.",
              "observationPrompts": [
                "Setup",
                "Execution",
                "Shutdown"
              ],
              "requiresEvaluatorSignoff": true,
              "requireTraineeAcknowledgement": true,
              "requireFailureComments": true,
              "requireRetestOnFailure": true
            }
            """;
        return await CreateStepAsync(accessToken, definitionId, stepKey, name, TrainingStepTypes.Practical, configJson, sortOrder);
    }

    private async Task<Guid> CreateContentStepAsync(
        string accessToken,
        Guid definitionId,
        string stepKey,
        string name,
        int sortOrder = 0)
    {
        var configJson = """{"body":"Review material."}""";
        return await CreateStepAsync(accessToken, definitionId, stepKey, name, TrainingStepTypes.Content, configJson, sortOrder);
    }

    private async Task<Guid> CreateLessonContentStepAsync(
        string accessToken,
        Guid definitionId,
        string stepKey,
        string name,
        int sortOrder = 0)
    {
        var configJson = """
            {
              "title": "Lesson overview",
              "body": "Review the assigned material and acknowledge the lesson.",
              "externalUrl": "https://example.com/lesson",
              "requireAcknowledgement": true
            }
            """;
        return await CreateStepAsync(accessToken, definitionId, stepKey, name, TrainingStepTypes.Content, configJson, sortOrder);
    }

    private async Task<Guid> CreateStepAsync(
        string accessToken,
        Guid definitionId,
        string stepKey,
        string name,
        string stepType,
        string configJson,
        int sortOrder)
    {
        var request = Authorized(HttpMethod.Post, $"/api/training-definitions/{definitionId}/steps", accessToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionStepRequest(
            stepKey,
            name,
            $"{name} description",
            stepType,
            configJson,
            sortOrder));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<TrainingDefinitionStepResponse>())!;
        return created.StepId;
    }

    private async Task<Guid> CreateAssignmentAsync(string accessToken, Guid personId, Guid definitionId)
    {
        var created = await TrainArrQualificationCheckTestHelper.CreateManualAssignmentAsync(
            _trainarrClient,
            accessToken,
            personId,
            definitionId,
            "step_branch_qual");
        return created.AssignmentId;
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string accessToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", accessToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"def_{Guid.NewGuid():N}"[..12],
            "Step branch definition",
            "Definition for conditional branching tests.",
            "step_branch_qual",
            "Step Branch Qual"));
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
            "Test User",
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
            $"{sourceProduct}-step-branch-{Guid.NewGuid():N}",
            $"{sourceProduct} step branch test",
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
