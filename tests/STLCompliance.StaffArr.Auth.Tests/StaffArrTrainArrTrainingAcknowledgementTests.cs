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

public class StaffArrTrainArrTrainingAcknowledgementTests : IAsyncLifetime
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
        var nexArrDbName = $"TrainAckNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"TrainAckStaffArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"TrainAckTrainArr-{Guid.NewGuid():N}";

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
            $"{StaffArrIntegration.TrainingAcknowledgementIngestActionScope},{StaffArrIntegration.TrainingAcknowledgementReadActionScope},{StaffArrIntegration.TrainingBlockerIngestActionScope}");

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
    public async Task Assignment_create_publishes_acknowledgement_and_gates_evidence_until_member_acknowledges()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Ack Trainee", "ack.trainee@example.com");
        var adminToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "trainarr_admin");
        var definitionId = await CreateTrainingDefinitionAsync(adminToken);

        var assignment = await TrainArrQualificationCheckTestHelper.CreateManualAssignmentAsync(
            _trainarrClient,
            adminToken,
            personId,
            definitionId,
            "ack_training",
            DateTimeOffset.UtcNow.AddDays(7));

        Assert.Equal(assignment.AssignmentId, assignment.StaffarrAcknowledgementRequestId);
        Assert.Equal("pending", assignment.StaffarrAcknowledgementStatus);
        Assert.True(assignment.StaffarrAcknowledgementRequired);

        var memberStaffarrToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "tenant_member", personId: personId);
        var listResponse = await _staffarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-acknowledgements?personId={personId:D}", memberStaffarrToken));
        listResponse.EnsureSuccessStatusCode();
        var acknowledgements = (await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TrainingAcknowledgementResponse>>())!;
        var pending = Assert.Single(acknowledgements);
        Assert.Equal("pending", pending.Status);
        Assert.Equal(assignment.AssignmentId, pending.TrainarrAssignmentId);

        var memberTrainarrToken = CreateTrainArrAccessToken(["trainarr"], tenantRoleKey: "tenant_member", personId: personId);
        var evidenceRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignment.AssignmentId}/evidence",
            memberTrainarrToken);
        evidenceRequest.Content = JsonContent.Create(new CreateTrainingEvidenceRequest(
            "completion_certificate",
            "proof.txt",
            "text/plain",
            Convert.ToBase64String("proof"u8.ToArray()),
            null));
        var blockedEvidence = await _trainarrClient.SendAsync(evidenceRequest);
        Assert.Equal(HttpStatusCode.Conflict, blockedEvidence.StatusCode);

        var acknowledgeRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-acknowledgements/{pending.AcknowledgementId}/acknowledge",
            memberStaffarrToken);
        var acknowledgeResponse = await _staffarrClient.SendAsync(acknowledgeRequest);
        acknowledgeResponse.EnsureSuccessStatusCode();
        var acknowledged = (await acknowledgeResponse.Content.ReadFromJsonAsync<TrainingAcknowledgementResponse>())!;
        Assert.Equal("acknowledged", acknowledged.Status);
        Assert.NotNull(acknowledged.AcknowledgedAt);

        var detailResponse = await _trainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/training-assignments/{assignment.AssignmentId}", memberTrainarrToken));
        detailResponse.EnsureSuccessStatusCode();
        var refreshed = (await detailResponse.Content.ReadFromJsonAsync<TrainingAssignmentDetailResponse>())!;
        Assert.Equal("acknowledged", refreshed.StaffarrAcknowledgementStatus);
        Assert.False(refreshed.StaffarrAcknowledgementRequired);
        Assert.NotNull(refreshed.StaffarrAcknowledgementAt);

        var allowedEvidenceRequest = Authorized(
            HttpMethod.Post,
            $"/api/training-assignments/{assignment.AssignmentId}/evidence",
            memberTrainarrToken);
        allowedEvidenceRequest.Content = JsonContent.Create(new CreateTrainingEvidenceRequest(
            "completion_certificate",
            "proof.txt",
            "text/plain",
            Convert.ToBase64String("proof"u8.ToArray()),
            null));
        var allowedEvidence = await _trainarrClient.SendAsync(allowedEvidenceRequest);
        allowedEvidence.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Integration_status_endpoint_returns_acknowledgement_state_for_trainarr()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Status Trainee", "status.trainee@example.com");
        var requestId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        var ingestRequest = ServiceAuthorized(HttpMethod.Post, "/api/integrations/training-acknowledgements", _trainarrToStaffarrToken);
        ingestRequest.Content = JsonContent.Create(new IngestTrainingAcknowledgementRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            requestId,
            assignmentId,
            "Safety Orientation",
            "manual",
            "Please acknowledge receipt of this training assignment.",
            null));
        (await _staffarrClient.SendAsync(ingestRequest)).EnsureSuccessStatusCode();

        var statusRequest = ServiceAuthorized(
            HttpMethod.Get,
            $"/api/integrations/training-acknowledgements/status?tenantId={PlatformSeeder.DemoTenantId:D}&trainarrAcknowledgementRequestId={requestId:D}",
            _trainarrToStaffarrToken);
        var statusResponse = await _staffarrClient.SendAsync(statusRequest);
        statusResponse.EnsureSuccessStatusCode();
        var status = (await statusResponse.Content.ReadFromJsonAsync<TrainingAcknowledgementStatusResponse>())!;
        Assert.Equal("pending", status.Status);
        Assert.Equal(assignmentId, status.TrainarrAssignmentId);

        var v1RequestId = Guid.NewGuid();
        var v1AssignmentId = Guid.NewGuid();
        var v1IngestRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/v1/integrations/training-acknowledgements",
            _trainarrToStaffarrToken);
        v1IngestRequest.Content = JsonContent.Create(new IngestTrainingAcknowledgementRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            v1RequestId,
            v1AssignmentId,
            "Safety Orientation V1",
            "manual",
            "V1 route acknowledgement request.",
            null));
        (await _staffarrClient.SendAsync(v1IngestRequest)).EnsureSuccessStatusCode();

        var v1StatusRequest = ServiceAuthorized(
            HttpMethod.Get,
            $"/api/v1/integrations/training-acknowledgements/status?tenantId={PlatformSeeder.DemoTenantId:D}&trainarrAcknowledgementRequestId={v1RequestId:D}",
            _trainarrToStaffarrToken);
        var v1StatusResponse = await _staffarrClient.SendAsync(v1StatusRequest);
        v1StatusResponse.EnsureSuccessStatusCode();
        var v1Status = (await v1StatusResponse.Content.ReadFromJsonAsync<TrainingAcknowledgementStatusResponse>())!;
        Assert.Equal("pending", v1Status.Status);
        Assert.Equal(v1AssignmentId, v1Status.TrainarrAssignmentId);
    }

    private async Task<Guid> CreateTrainingDefinitionAsync(string trainarrAdminToken)
    {
        var request = Authorized(HttpMethod.Post, "/api/training-definitions", trainarrAdminToken);
        request.Content = JsonContent.Create(new CreateTrainingDefinitionRequest(
            "ack_training",
            "Acknowledgement Training",
            "Training used to validate StaffArr acknowledgement workflow.",
            "ack_training",
            "Acknowledgement Qualification"));
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

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string url, string serviceToken) =>
        Authorized(method, url, serviceToken);

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
            $"{sourceProduct}-train-ack-{Guid.NewGuid():N}",
            $"{sourceProduct} train acknowledgement test",
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
