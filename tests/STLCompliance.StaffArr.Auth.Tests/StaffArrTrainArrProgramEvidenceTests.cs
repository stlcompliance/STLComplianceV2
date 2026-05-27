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

public class StaffArrTrainArrProgramEvidenceTests : IAsyncLifetime
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
        var nexArrDbName = $"TrainProgramNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"TrainProgramStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"TrainProgramTrainArr-{Guid.NewGuid():N}";

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
            builder.UseSetting("EvidenceStorage:RootPath", Path.Combine(Path.GetTempPath(), $"trainarr-evidence-{Guid.NewGuid():N}"));
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
    public async Task Training_program_create_list_and_publish()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/training-programs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingProgramRequest(
            "annual_bundle",
            "Annual training bundle",
            "Bundles annual compliance training definitions for operational staff.",
            [definitionId]));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TrainingProgramDetailResponse>())!;
        Assert.Equal("draft", created.Status);
        Assert.Single(created.Definitions);

        var listResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/training-programs", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var programs = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingProgramSummaryResponse>>())!;
        Assert.Contains(programs, p => p.ProgramId == created.ProgramId);

        var publishRequest = Authorized(HttpMethod.Put, $"/api/training-programs/{created.ProgramId}", adminToken);
        publishRequest.Content = JsonContent.Create(new UpdateTrainingProgramRequest(
            "Annual training bundle",
            "Bundles annual compliance training definitions for operational staff.",
            "published",
            [definitionId]));
        var publishResponse = await _trainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();
        var published = (await publishResponse.Content.ReadFromJsonAsync<TrainingProgramDetailResponse>())!;
        Assert.Equal("published", published.Status);
    }

    [Fact]
    public async Task Training_program_create_denies_member_role()
    {
        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member");
        var request = Authorized(HttpMethod.Post, "/api/training-programs", memberToken);
        request.Content = JsonContent.Create(new CreateTrainingProgramRequest(
            "blocked",
            "Blocked program",
            "Member should not be able to create training programs in this slice.",
            [Guid.NewGuid()]));
        var response = await _trainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Training_evidence_upload_list_and_complete_clears_blocker()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Evidence Subject", "evidence.subject@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var createAssignmentRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        createAssignmentRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            null,
            "manual",
            null));
        var createAssignmentResponse = await _trainarrClient.SendAsync(createAssignmentRequest);
        createAssignmentResponse.EnsureSuccessStatusCode();
        var assignment = (await createAssignmentResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
        Assert.Equal("assigned", assignment.Status);
        Assert.Equal(0, assignment.EvidenceCount);

        var evidenceRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignment.AssignmentId}/evidence",
            adminToken);
        evidenceRequest.Content = JsonContent.Create(new CreateTrainingEvidenceRequest(
            "completion_certificate",
            "certificate.txt",
            "text/plain",
            Convert.ToBase64String("signed completion proof"u8.ToArray()),
            "Trainer signed completion"));
        var evidenceResponse = await _trainarrClient.SendAsync(evidenceRequest);
        evidenceResponse.EnsureSuccessStatusCode();
        var evidence = (await evidenceResponse.Content.ReadFromJsonAsync<TrainingEvidenceResponse>())!;
        Assert.Equal("completion_certificate", evidence.EvidenceTypeKey);

        var listEvidenceResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-assignments/{assignment.AssignmentId}/evidence", adminToken));
        listEvidenceResponse.EnsureSuccessStatusCode();
        var evidenceItems = (await listEvidenceResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingEvidenceResponse>>())!;
        Assert.Single(evidenceItems);

        var detailResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-assignments/{assignment.AssignmentId}", adminToken));
        detailResponse.EnsureSuccessStatusCode();
        var detail = (await detailResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
        Assert.Equal("in_progress", detail.Status);
        Assert.Equal(1, detail.EvidenceCount);

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
    public async Task Training_evidence_upload_allows_member_self()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Member Evidence", "member.evidence@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var createAssignmentRequest = Authorized(HttpMethod.Post, "/api/training-assignments", adminToken);
        createAssignmentRequest.Content = JsonContent.Create(new CreateTrainingAssignmentRequest(
            personId,
            definitionId,
            null,
            "manual",
            null));
        var createAssignmentResponse = await _trainarrClient.SendAsync(createAssignmentRequest);
        createAssignmentResponse.EnsureSuccessStatusCode();
        var assignment = (await createAssignmentResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;

        var memberToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var evidenceRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignment.AssignmentId}/evidence",
            memberToken);
        evidenceRequest.Content = JsonContent.Create(new CreateTrainingEvidenceRequest(
            "quiz_result",
            "quiz.pdf",
            "application/pdf",
            Convert.ToBase64String("quiz-pass"u8.ToArray()),
            null));
        var evidenceResponse = await _trainarrClient.SendAsync(evidenceRequest);
        evidenceResponse.EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string trainarrAdminToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", trainarrAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"program_def_{Guid.NewGuid():N}"[..24],
            "Program-linked training",
            "Training definition used by program builder tests.",
            "program_qualification",
            "Program Qualification"));
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
            $"{sourceProduct}-program-evidence-{Guid.NewGuid():N}",
            $"{sourceProduct} program evidence test",
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
