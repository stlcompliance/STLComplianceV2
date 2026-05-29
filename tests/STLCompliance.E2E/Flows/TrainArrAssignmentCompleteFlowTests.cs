using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Entities;
using StaffArr.Api.Services;
using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.E2E.Support;
using StaffArrIntegration = StaffArr.Api.Endpoints.IntegrationEndpoints;
using TrainArrIntegration = TrainArr.Api.Endpoints.IntegrationEndpoints;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// StaffArr incident → TrainArr assignment → completion → StaffArr certification + readiness unblock.
/// </summary>
[Trait("Category", "Integration")]
public sealed class TrainArrAssignmentCompleteFlowTests : IAsyncLifetime
{
    private E2ENexArrHost _nexarr = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private string _trainarrToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        _nexarr = new E2ENexArrHost();
        await _nexarr.InitializeAsync();

        var adminToken = await _nexarr.LoginAsync();
        _trainarrToStaffarrToken = await _nexarr.IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            $"{StaffArrIntegration.TrainingBlockerIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementReadActionScope},{StaffArrIntegration.CertificationGrantIngestActionScope}",
            ["staffarr"]);

        var staffArrDbName = $"E2E-StaffArr-Train-{Guid.NewGuid():N}";
        var trainArrDbName = $"E2E-TrainArr-{Guid.NewGuid():N}";

        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", E2ENexArrHost.SigningKey);
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
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("ServiceToken:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("StaffArr:BaseUrl", _staffarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:ServiceToken", _trainarrToStaffarrToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArr.Api.Data.TrainArrDbContext>(services);
                services.AddDbContext<TrainArr.Api.Data.TrainArrDbContext>(options =>
                    options.UseInMemoryDatabase(trainArrDbName));

                services.AddHttpClient<StaffArrTrainingBlockerClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrCertificationGrantClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrTrainingAcknowledgementClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
            });
        });
        _trainarrClient = _trainarrFactory.CreateClient();

        var staffarrToTrainarrToken = await _nexarr.IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            TrainArrIntegration.IncidentRemediationIngestActionScope,
            ["trainarr"]);

        _staffarrFactory = _staffarrFactory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("TrainArr:BaseUrl", _trainarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("TrainArr:ServiceToken", staffarrToTrainarrToken);
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient<TrainArrIncidentRemediationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _trainarrFactory.Server.CreateHandler());
            });
        });
        _staffarrClient = _staffarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _trainarrClient.Dispose();
        _staffarrClient.Dispose();
        await _trainarrFactory.DisposeAsync();
        await _staffarrFactory.DisposeAsync();
        await _nexarr.DisposeAsync();
    }

    [Fact]
    public async Task Assignment_completion_clears_training_blocker_and_grants_certification()
    {
        var personId = Guid.NewGuid();
        await SeedPersonAsync(personId, "E2E Trainee", "e2e.trainee@example.com");
        var remediationId = await RouteIncidentToTrainarrAsync(personId);

        var trainarrAdminToken = CreateTrainArrAccessToken(["trainarr"], "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(trainarrAdminToken);

        var assignment = await TrainArrQualificationCheckHelper.CreateRemediationAssignmentAsync(
            _trainarrClient,
            trainarrAdminToken,
            personId,
            definitionId,
            "annual_compliance",
            remediationId,
            DateTimeOffset.UtcNow.AddDays(14));
        Assert.Equal("assigned", assignment.Status);

        var staffarrAdminToken = CreateStaffArrAccessToken(["staffarr"], "tenant_admin");
        var readinessBeforeResponse = await _staffarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", staffarrAdminToken));
        readinessBeforeResponse.EnsureSuccessStatusCode();
        var readinessBefore = (await readinessBeforeResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
        Assert.Contains(readinessBefore.Blockers, b => b.BlockerSource == "training");

        var memberToken = CreateTrainArrAccessToken(["trainarr"], "tenant_member", personId);
        await TrainArrCompletionHelper.SatisfyCompletionRequirementsAsync(
            _trainarrClient,
            assignment.AssignmentId,
            trainarrAdminToken,
            memberToken);

        var completeRequest = HttpTestClient.Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignment.AssignmentId}/complete",
            trainarrAdminToken);
        (await _trainarrClient.SendAsync(completeRequest)).EnsureSuccessStatusCode();

        var certsResponse = await _staffarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/people/{personId}/certifications", staffarrAdminToken));
        certsResponse.EnsureSuccessStatusCode();
        var certifications = (await certsResponse.Content.ReadFromJsonAsync<IReadOnlyList<PersonCertificationResponse>>())!;
        Assert.Contains(certifications, c => c.SourceType == "trainarr_publication");

        var readinessAfterResponse = await _staffarrClient.SendAsync(
            HttpTestClient.Authorized(HttpMethod.Get, $"/api/people/{personId}/readiness", staffarrAdminToken));
        readinessAfterResponse.EnsureSuccessStatusCode();
        var readinessAfter = (await readinessAfterResponse.Content.ReadFromJsonAsync<PersonReadinessResponse>())!;
        Assert.DoesNotContain(readinessAfter.Blockers, b => b.BlockerSource == "training");
    }

    private async Task<Guid> RouteIncidentToTrainarrAsync(Guid personId)
    {
        var adminToken = CreateStaffArrAccessToken(["staffarr"], "tenant_admin");
        var createRequest = HttpTestClient.Authorized(HttpMethod.Post, "/api/incidents", adminToken);
        createRequest.Content = JsonContent.Create(new CreatePersonnelIncidentRequest(
            personId,
            "training_compliance",
            "high",
            "Missed annual compliance training",
            "E2E remediation routing.",
            DateTimeOffset.UtcNow.AddHours(-2)));
        var createResponse = await _staffarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var incident = (await createResponse.Content.ReadFromJsonAsync<PersonnelIncidentDetailResponse>())!;

        var routeRequest = HttpTestClient.Authorized(
            HttpMethod.Post,
            $"/api/incidents/{incident.IncidentId}/route-to-trainarr",
            adminToken);
        var routeResponse = await _staffarrClient.SendAsync(routeRequest);
        routeResponse.EnsureSuccessStatusCode();
        var routed = (await routeResponse.Content.ReadFromJsonAsync<RouteIncidentToTrainarrResponse>())!;
        return routed.TrainarrRouting.TrainarrRemediationId;
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string trainarrAdminToken)
    {
        var request = HttpTestClient.Authorized(HttpMethod.Post, "/api/training-definitions", trainarrAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            $"e2e_compliance_{Guid.NewGuid():N}".Substring(0, 20),
            "E2E Compliance Refresher",
            "E2E cross-product assignment flow.",
            "annual_compliance",
            "Annual Compliance"));
        var response = await _trainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var definition = (await response.Content.ReadFromJsonAsync<TrainingDefinitionResponse>())!;
        return definition.TrainingDefinitionId;
    }

    private async Task SeedPersonAsync(Guid personId, string displayName, string email)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            DisplayName = displayName,
            PrimaryEmail = email,
            EmploymentStatus = "active",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    private string CreateTrainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey,
        Guid? personId = null)
    {
        using var scope = _trainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TrainArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "E2E Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
    }

    private string CreateStaffArrAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "E2E Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
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
}
