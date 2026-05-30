using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using NexArr.Api.Data;
using NexArr.Api.Services;
using TrainArr.Api.Contracts;
using TrainArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrTrainArrProgramVersionMatrixTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _trainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ProgVerNexArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"ProgVerTrainArr-{Guid.NewGuid():N}";

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

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArr.Api.Data.TrainArrDbContext>(services);
                services.AddDbContext<TrainArr.Api.Data.TrainArrDbContext>(options =>
                    options.UseInMemoryDatabase(trainArrDbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Program_publish_creates_version_and_start_revision_returns_draft()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var createRequest = Authorized(HttpMethod.Post, "/api/training-programs", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTrainingProgramRequest(
            "matrix_bundle",
            "Matrix bundle",
            "Program used for version and matrix coverage tests.",
            [definitionId]));
        var createResponse = await _trainarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TrainingProgramDetailResponse>())!;

        var publishRequest = Authorized(HttpMethod.Put, $"/api/training-programs/{created.ProgramId}", adminToken);
        publishRequest.Content = JsonContent.Create(new UpdateTrainingProgramRequest(
            "Matrix bundle",
            "Program used for version and matrix coverage tests.",
            "published",
            [definitionId]));
        var publishResponse = await _trainarrClient.SendAsync(publishRequest);
        publishResponse.EnsureSuccessStatusCode();

        var versionsResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/program-versions?programId={created.ProgramId}", adminToken));
        versionsResponse.EnsureSuccessStatusCode();
        var versions = (await versionsResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingProgramVersionSummaryResponse>>())!;
        Assert.Single(versions);
        Assert.Equal(1, versions[0].VersionNumber);

        var revisionRequest = Authorized(HttpMethod.Post, "/api/program-versions/start-revision", adminToken);
        revisionRequest.Content = JsonContent.Create(new StartProgramRevisionRequest(created.ProgramId));
        var revisionResponse = await _trainarrClient.SendAsync(revisionRequest);
        revisionResponse.EnsureSuccessStatusCode();
        var revised = (await revisionResponse.Content.ReadFromJsonAsync<TrainingProgramDetailResponse>())!;
        Assert.Equal("draft", revised.Status);
    }

    [Fact]
    public async Task Training_matrix_crud_and_qualification_issue_list()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var matrixCreate = Authorized(HttpMethod.Post, "/api/training-matrix", adminToken);
        matrixCreate.Content = JsonContent.Create(new CreateTrainingMatrixEntryRequest(
            "driver",
            "Commercial driver",
            null,
            definitionId,
            "required",
            0));
        var matrixCreateResponse = await _trainarrClient.SendAsync(matrixCreate);
        matrixCreateResponse.EnsureSuccessStatusCode();
        var matrixEntry = (await matrixCreateResponse.Content.ReadFromJsonAsync<TrainingMatrixEntryResponse>())!;

        var matrixViewResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/training-matrix", adminToken));
        matrixViewResponse.EnsureSuccessStatusCode();
        var matrixView = (await matrixViewResponse.Content.ReadFromJsonAsync<TrainingMatrixViewResponse>())!;
        Assert.Contains(matrixView.Entries, e => e.MatrixEntryId == matrixEntry.MatrixEntryId);

        var listResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/qualification-issues", adminToken));
        listResponse.EnsureSuccessStatusCode();
        var issues = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<QualificationIssueListItemResponse>>())!;
        Assert.NotNull(issues);

        var deleteResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/training-matrix/{matrixEntry.MatrixEntryId}", adminToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Applicability_profiles_and_training_requirements_builder_flow()
    {
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var profileCreate = Authorized(HttpMethod.Post, "/api/applicability-profiles", adminToken);
        profileCreate.Content = JsonContent.Create(new CreateTrainingApplicabilityProfileRequest(
            "Commercial driver",
            TrainingApplicabilityScopeTypes.RoleTemplate,
            "driver",
            "Drivers requiring CDL training",
            "StaffArr",
            null));
        var profileCreateResponse = await _trainarrClient.SendAsync(profileCreate);
        profileCreateResponse.EnsureSuccessStatusCode();
        var profile = (await profileCreateResponse.Content.ReadFromJsonAsync<TrainingApplicabilityProfileResponse>())!;

        var requirementCreate = Authorized(HttpMethod.Post, "/api/training-requirements", adminToken);
        requirementCreate.Content = JsonContent.Create(new CreateTrainingRequirementRequest(
            "cdl_renewal",
            "CDL renewal requirement",
            "Annual CDL renewal",
            TrainingRequirementSources.Internal,
            null,
            null,
            definitionId,
            profile.ApplicabilityProfileId,
            "required",
            0));
        var requirementCreateResponse = await _trainarrClient.SendAsync(requirementCreate);
        requirementCreateResponse.EnsureSuccessStatusCode();
        var requirement = (await requirementCreateResponse.Content.ReadFromJsonAsync<TrainingRequirementResponse>())!;

        var builderViewResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/training-requirements/builder-view", adminToken));
        builderViewResponse.EnsureSuccessStatusCode();
        var builderView = (await builderViewResponse.Content.ReadFromJsonAsync<TrainingRequirementBuilderViewResponse>())!;
        Assert.Contains(builderView.Profiles, p => p.ApplicabilityProfileId == profile.ApplicabilityProfileId);
        Assert.Contains(builderView.Requirements, r => r.RequirementId == requirement.RequirementId);

        var v1BuilderViewResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/requirements/builder-view", adminToken));
        v1BuilderViewResponse.EnsureSuccessStatusCode();
        var v1BuilderView = (await v1BuilderViewResponse.Content.ReadFromJsonAsync<TrainingRequirementBuilderViewResponse>())!;
        Assert.Contains(v1BuilderView.Requirements, r => r.RequirementId == requirement.RequirementId);

        var syncRequest = Authorized(HttpMethod.Post, "/api/training-requirements/sync-to-matrix", adminToken);
        syncRequest.Content = JsonContent.Create(new SyncRequirementToMatrixRequest(requirement.RequirementId));
        var syncResponse = await _trainarrClient.SendAsync(syncRequest);
        syncResponse.EnsureSuccessStatusCode();
        var syncResult = (await syncResponse.Content.ReadFromJsonAsync<SyncRequirementToMatrixResponse>())!;
        Assert.Equal("driver", syncResult.ApplicabilityKey);

        var v1SyncRequest = Authorized(HttpMethod.Post, "/api/v1/requirements/sync-to-matrix", adminToken);
        v1SyncRequest.Content = JsonContent.Create(new SyncRequirementToMatrixRequest(requirement.RequirementId));
        var v1SyncResponse = await _trainarrClient.SendAsync(v1SyncRequest);
        v1SyncResponse.EnsureSuccessStatusCode();

        var deleteRequirementResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/training-requirements/{requirement.RequirementId}", adminToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteRequirementResponse.StatusCode);

        var deleteProfileResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/applicability-profiles/{profile.ApplicabilityProfileId}", adminToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteProfileResponse.StatusCode);
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string accessToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", accessToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"def_{Guid.NewGuid():N}"[..12],
            "Version matrix definition",
            "Definition for program version tests.",
            "version_matrix_qual",
            "Version Matrix Qual"));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return created.TrainingDefinitionId;
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
