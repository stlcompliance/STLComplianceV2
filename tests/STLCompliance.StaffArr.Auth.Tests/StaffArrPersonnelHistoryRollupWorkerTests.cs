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
using StaffArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrPersonnelHistoryRollupWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _supervisorToken = null!;
    private string _sharedWorkerToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PersonnelHistoryNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"PersonnelHistoryStaffArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["staffarr"],
            PersonnelHistoryService.RollupActionScope);

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
        _supervisorToken = CreateStaffArrAccessToken(["staffarr"], tenantRoleKey: "supervisor");
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _staffarrClient.PostAsJsonAsync(
            "/api/internal/personnel-history/process-batch",
            new ProcessPersonnelHistoryRequest(PlatformSeeder.DemoTenantId, null, 50, 1));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_rejects_trainarr_source_token()
    {
        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var trainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            PersonnelHistoryService.RollupActionScope);

        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/personnel-history/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessPersonnelHistoryRequest(
            PlatformSeeder.DemoTenantId,
            null,
            50,
            1));

        var response = await _staffarrClient.SendAsync(processRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task List_pending_returns_active_people_before_processing()
    {
        var personId = await SeedPersonWithCertificationAsync();

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/personnel-history/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=20&stalenessHours=1");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToStaffarrToken);

        var listResponse = await _staffarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingPersonnelHistoryResponse>())!;
        Assert.Contains(pending.Items, x => x.PersonId == personId);
    }

    [Fact]
    public async Task Process_batch_materializes_history_and_supervisor_can_read_summary()
    {
        var personId = await SeedPersonWithCertificationAsync();

        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/personnel-history/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToStaffarrToken);
        processRequest.Content = JsonContent.Create(new ProcessPersonnelHistoryRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));

        var processResponse = await _staffarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessPersonnelHistoryResponse>())!;
        Assert.True(body.RefreshedCount >= 1);
        Assert.Contains(body.RefreshedRollups, x => x.PersonId == personId && x.IsMaterialized);

        var summaryRequest = Authorized(
            HttpMethod.Get,
            $"/api/person-history/summary?personId={personId}",
            _supervisorToken);
        var summaryResponse = await _staffarrClient.SendAsync(summaryRequest);
        summaryResponse.EnsureSuccessStatusCode();
        var summary = (await summaryResponse.Content.ReadFromJsonAsync<PersonnelHistorySummaryResponse>())!;
        Assert.True(summary.IsMaterialized);
        Assert.True(summary.EventCount >= 1);
        Assert.True(summary.CertificationCount >= 1);
    }

    [Fact]
    public async Task Person_history_returns_materialized_events_after_rollup()
    {
        var personId = await SeedPersonWithCertificationAsync();
        await ProcessBatchAsync();

        var historyRequest = Authorized(
            HttpMethod.Get,
            $"/api/people/{personId}/person-history?page=1&pageSize=20",
            _supervisorToken);
        var historyResponse = await _staffarrClient.SendAsync(historyRequest);
        historyResponse.EnsureSuccessStatusCode();
        var history = (await historyResponse.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;
        Assert.True(history.TotalCount >= 1);
        Assert.Contains(history.Items, x => x.Category == "certification");
    }

    [Fact]
    public async Task Integration_person_history_allows_trainarr_service_token()
    {
        var personId = await SeedPersonWithCertificationAsync();
        await ProcessBatchAsync();

        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var trainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            PersonnelHistoryService.IntegrationReadActionScope);

        var integrationRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/integrations/person-history?tenantId={PlatformSeeder.DemoTenantId}&personId={personId}");
        integrationRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trainarrToken);

        var response = await _staffarrClient.SendAsync(integrationRequest);
        response.EnsureSuccessStatusCode();
        var history = (await response.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;
        Assert.True(history.TotalCount >= 1);

        var v1IntegrationRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/integrations/person-history?tenantId={PlatformSeeder.DemoTenantId}&personId={personId}");
        v1IntegrationRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trainarrToken);

        var v1Response = await _staffarrClient.SendAsync(v1IntegrationRequest);
        v1Response.EnsureSuccessStatusCode();
        var v1History = (await v1Response.Content.ReadFromJsonAsync<PagedResult<PersonTimelineEntryResponse>>())!;
        Assert.Equal(history.TotalCount, v1History.TotalCount);
    }

    [Fact]
    public async Task Integration_person_history_rejects_shared_worker_rollup_token()
    {
        var personId = await SeedPersonWithCertificationAsync();
        await ProcessBatchAsync();

        var integrationRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/integrations/person-history?tenantId={PlatformSeeder.DemoTenantId}&personId={personId}");
        integrationRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToStaffarrToken);

        var response = await _staffarrClient.SendAsync(integrationRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Person_history_summary_denies_unrelated_tenant_member()
    {
        var personId = await SeedPersonWithCertificationAsync();
        var memberToken = CreateStaffArrAccessToken(
            ["staffarr"],
            tenantRoleKey: "tenant_member",
            personId: Guid.NewGuid());

        var response = await _staffarrClient.SendAsync(Authorized(
            HttpMethod.Get,
            $"/api/person-history/summary?personId={personId}",
            memberToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task ProcessBatchAsync()
    {
        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/personnel-history/process-batch");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToStaffarrToken);
        processRequest.Content = JsonContent.Create(new ProcessPersonnelHistoryRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));
        var processResponse = await _staffarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
    }

    private async Task<Guid> SeedPersonWithCertificationAsync()
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var personId = Guid.NewGuid();

        db.People.Add(new StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "History",
            FamilyName = "Rollup",
            DisplayName = "History Rollup",
            PrimaryEmail = $"history.rollup.{personId:N}@example.com",
            EmploymentStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        });

        await StaffArrReadinessCertificationSeed.EnsureBaselineDefinitionsAsync(
            db,
            PlatformSeeder.DemoTenantId,
            CancellationToken.None);

        var definition = await db.CertificationDefinitions
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId)
            .FirstAsync();

        db.PersonCertifications.Add(new PersonCertification
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = personId,
            CertificationDefinitionId = definition.Id,
            SourceType = "manual",
            Status = "active",
            GrantedAt = now.AddMonths(-1),
            ExpiresAt = now.AddYears(1),
            GrantedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now,
            UpdatedAt = now
        });

        await db.SaveChangesAsync();
        return personId;
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

    private string CreateStaffArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<StaffArrTokenService>();
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

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        IReadOnlyList<string> allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-personnel-history-{Guid.NewGuid():N}",
            $"{sourceProduct} personnel history test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
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
