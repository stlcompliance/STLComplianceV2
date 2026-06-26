using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreQuestionnaireTests : IAsyncLifetime
{
    private const string SigningKey = "test-signing-key-at-least-32-chars-long";

    private WebApplicationFactory<global::ComplianceCore.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var dbName = $"ComplianceCoreQuestionnaire-{Guid.NewGuid():N}";
        _factory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", SigningKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Resolve_onboarding_questionnaire_returns_profile_and_setup_checklist()
    {
        var token = CreateToken("compliance_reviewer");
        var response = await ResolveAsync(token, new QuestionnaireResolveRequest(
            PlatformSeeder.DemoTenantId,
            "compliancecore",
            "tenant_onboarding",
            "tenant",
            SubjectId: PlatformSeeder.DemoTenantId.ToString(),
            SourceRecordId: $"tenant-{PlatformSeeder.DemoTenantId:D}",
            SourceEntity: "tenant"));

        Assert.Equal("tenant_onboarding", response.Run.WorkflowKey);
        Assert.Equal("tenant_onboarding", response.Run.TemplateKey);
        Assert.NotEmpty(response.Questions);
        Assert.Equal("Not captured yet", response.TenantProfile.BusinessProfile);
        Assert.NotEmpty(response.TenantProfile.SetupChecklist);
        Assert.True(response.Summary.RequiresMoreFacts);
        Assert.Equal("warning", response.Summary.RiskGateStatus);
        Assert.Contains(response.Summary.LikelyApplicableAreas, area => area.Equals("Transportation", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Resolve_asset_questionnaire_reuses_known_defaults_and_asks_only_record_deltas()
    {
        var token = CreateToken("compliance_reviewer");
        var response = await ResolveAsync(token, new QuestionnaireResolveRequest(
            PlatformSeeder.DemoTenantId,
            "maintainarr",
            "asset_create",
            "asset",
            SubjectId: "asset-123",
            SourceRecordId: "asset-123",
            SourceEntity: "asset",
            KnownFacts: new Dictionary<string, string>
            {
                ["asset.kind"] = "truck",
                ["asset.base_location"] = "yard",
            }));

        var assetKind = Assert.Single(response.Questions, question => question.QuestionKey == "asset_kind");
        Assert.Equal("truck", assetKind.DefaultOptionKey);
        Assert.DoesNotContain(response.Questions, question => question.QuestionKey == "asset_base_location");
        Assert.Contains(response.Summary.LikelyApplicableAreas, area => area.Equals("Transportation", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Resolve_questionnaire_allows_runtime_access_after_non_compliancecore_launch_context()
    {
        var token = CreateToken("compliance_reviewer", ["staffarr"]);
        var response = await ResolveAsync(token, new QuestionnaireResolveRequest(
            PlatformSeeder.DemoTenantId,
            "compliancecore",
            "tenant_onboarding",
            "tenant",
            SubjectId: PlatformSeeder.DemoTenantId.ToString(),
            SourceRecordId: $"tenant-{PlatformSeeder.DemoTenantId:D}",
            SourceEntity: "tenant"));

        Assert.Equal("tenant_onboarding", response.Run.WorkflowKey);
        Assert.NotEmpty(response.Questions);
    }

    [Fact]
    public async Task Submit_not_sure_creates_reviewable_unknown_fact()
    {
        var token = CreateToken("compliance_reviewer");
        var resolved = await ResolveAsync(token, new QuestionnaireResolveRequest(
            PlatformSeeder.DemoTenantId,
            "staffarr",
            "person_create",
            "person",
            SubjectId: "person-456",
            SourceRecordId: "person-456",
            SourceEntity: "person"));

        var run = resolved.Run;
        var submission = await SubmitAsync(token, run.QuestionnaireRunId, new QuestionnaireSubmitRequest([
            new QuestionnaireAnswerRequest("person_work", SelectedOptionKey: "not_sure"),
        ]));

        var answer = Assert.Single(submission.Answers);
        Assert.Equal("unknown", answer.ReviewStatus);
        Assert.Equal("unknown", answer.NormalizedFactValue);
        Assert.Contains(submission.CreatedFacts, fact => fact.ReviewStatus == "unknown" && fact.Value == "unknown");
        Assert.Contains(submission.Summary.MissingFacts, fact => fact == "person.work");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var persisted = await db.QuestionnaireAnswers.SingleAsync();
        Assert.Equal(QuestionnaireReviewStatuses.Unknown, persisted.ReviewStatus);
        var fact = await db.FactAssertions.SingleAsync();
        Assert.Equal("unknown", fact.Value);
        Assert.Equal("person.work", fact.FactKey);
    }

    [Fact]
    public async Task Submit_conflict_marks_follow_up_and_generated_exception()
    {
        var token = CreateToken("compliance_reviewer");
        var resolved = await ResolveAsync(token, new QuestionnaireResolveRequest(
            PlatformSeeder.DemoTenantId,
            "routarr",
            "route_order_create",
            "trip",
            SubjectId: "trip-789",
            SourceRecordId: "trip-789",
            SourceEntity: "trip",
            KnownFacts: new Dictionary<string, string>
            {
                ["route.company_operated"] = "true",
            }));

        var submission = await SubmitAsync(token, resolved.Run.QuestionnaireRunId, new QuestionnaireSubmitRequest([
            new QuestionnaireAnswerRequest("route_company_operated", SelectedOptionKey: "no"),
            new QuestionnaireAnswerRequest("route_vendor_operated", SelectedOptionKey: "no"),
            new QuestionnaireAnswerRequest("route_brokered", SelectedOptionKey: "no"),
            new QuestionnaireAnswerRequest("route_interstate", SelectedOptionKey: "no"),
            new QuestionnaireAnswerRequest("route_passenger", SelectedOptionKey: "no"),
            new QuestionnaireAnswerRequest("route_property", SelectedOptionKey: "no"),
            new QuestionnaireAnswerRequest("route_hazmat", SelectedOptionKey: "no"),
            new QuestionnaireAnswerRequest("route_driver", SelectedOptionKey: "recorded"),
        ]));

        var conflicted = Assert.Single(submission.Answers, answer => answer.QuestionKey == "route_company_operated");
        Assert.Equal(QuestionnaireReviewStatuses.Conflict, conflicted.ReviewStatus);
        Assert.Contains(submission.Summary.FollowUps, followUp => followUp.TriggerFactKey == "route.company_operated");
        Assert.Contains(submission.Summary.GeneratedExceptions, exception => exception.ExceptionKey == "conflicting_facts");
        Assert.Equal("blocked", submission.Summary.RiskGateStatus);
    }

    private async Task<QuestionnaireResolutionResponse> ResolveAsync(string token, QuestionnaireResolveRequest request)
    {
        var httpRequest = Authorized(HttpMethod.Post, "/api/v1/questionnaires/resolve", token);
        httpRequest.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<QuestionnaireResolutionResponse>())!;
    }

    private async Task<QuestionnaireSubmissionResponse> SubmitAsync(string token, Guid questionnaireRunId, QuestionnaireSubmitRequest request)
    {
        var httpRequest = Authorized(HttpMethod.Post, $"/api/v1/questionnaires/{questionnaireRunId}/submit", token);
        httpRequest.Content = JsonContent.Create(request);
        var response = await _client.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<QuestionnaireSubmissionResponse>())!;
    }

    private string CreateToken(string tenantRoleKey, IReadOnlyList<string>? entitlements = null)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ComplianceCoreTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements ?? ["compliancecore"],
            isPlatformAdmin: false);

        return accessToken;
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
        var descriptors = services
            .Where(descriptor => descriptor.ServiceType == typeof(DbContextOptions<TContext>) || descriptor.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
