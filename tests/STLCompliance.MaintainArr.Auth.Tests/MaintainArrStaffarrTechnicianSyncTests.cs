using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Endpoints;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using NexArr.Api.Data;
using NexArr.Api.Services;
using StaffArr.Api.Contracts;
using StaffArr.Api.Data;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrStaffarrTechnicianSyncTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _staffarrToMaintainarrToken = null!;
    private string _maintainarrToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"StaffarrSyncNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"StaffarrSyncStaffArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"StaffarrSyncMaintainArr-{Guid.NewGuid():N}";

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
        _staffarrToMaintainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "staffarr",
            ["maintainarr"],
            IntegrationEndpoints.StaffarrPersonSyncActionScope);
        _maintainarrToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "maintainarr",
            ["staffarr"],
            "staffarr.person.lookup");
        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("MaintainArr:BaseUrl", "http://localhost:5104");
            builder.UseSetting("MaintainArr:ServiceToken", _staffarrToMaintainarrToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:BaseUrl", _staffarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:ServiceToken", _maintainarrToStaffarrToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrPersonLookupClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();

        _staffarrFactory = _staffarrFactory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("MaintainArr:BaseUrl", _maintainarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.ConfigureServices(services =>
            {
                services.AddHttpClient<MaintainArrTechnicianRefSyncClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _maintainarrFactory.Server.CreateHandler());
            });
        });

        _staffarrClient = _staffarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Staffarr_person_sync_ingest_updates_technician_ref()
    {
        var personId = Guid.NewGuid();
        var request = ServiceAuthorized(
            HttpMethod.Post,
            "/api/integrations/staffarr-person-sync",
            _staffarrToMaintainarrToken);
        request.Content = JsonContent.Create(new IngestStaffarrPersonSyncRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            "Jordan Tech",
            "active",
            "yard-b",
            "staffarr.person.updated",
            DateTimeOffset.UtcNow,
            $"staffarr.person.updated:{personId:D}"));

        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var ingested = (await response.Content.ReadFromJsonAsync<IngestStaffarrPersonSyncResponse>())!;
        Assert.False(ingested.IdempotentReplay);
        Assert.Equal(personId.ToString("D"), ingested.PersonId);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var mirror = await db.StaffPersonRefs
            .AsNoTracking()
            .SingleAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.StaffarrPersonId == personId.ToString("D"));
        Assert.Equal("Jordan Tech", mirror.DisplayNameSnapshot);
        Assert.Equal("active", mirror.ActiveStatusSnapshot);
        Assert.Equal("yard-b", mirror.PrimarySiteSnapshot);
    }

    [Fact]
    public async Task Staffarr_person_sync_ingest_v1_alias_updates_technician_ref()
    {
        var personId = Guid.NewGuid();
        var request = ServiceAuthorized(
            HttpMethod.Post,
            "/api/v1/integrations/staffarr-person-sync",
            _staffarrToMaintainarrToken);
        request.Content = JsonContent.Create(new IngestStaffarrPersonSyncRequest(
            PlatformSeeder.DemoTenantId,
            personId,
            "Jordan Tech V1",
            "active",
            "yard-c",
            "staffarr.person.updated",
            DateTimeOffset.UtcNow,
            $"staffarr.person.updated:{personId:D}:v1"));

        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var mirror = await db.StaffPersonRefs
            .AsNoTracking()
            .SingleAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.StaffarrPersonId == personId.ToString("D"));
        Assert.Equal("Jordan Tech V1", mirror.DisplayNameSnapshot);
        Assert.Equal("yard-c", mirror.PrimarySiteSnapshot);
    }

    [Fact]
    public async Task Staffarr_person_create_pushes_mirror_to_maintainarr()
    {
        using var staffarrScope = _staffarrFactory.Services.CreateScope();
        var staffarrDb = staffarrScope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var personId = Guid.NewGuid();
        var email = $"sync.tech.{Guid.NewGuid():N}@example.com";
        staffarrDb.People.Add(new StaffArr.Api.Entities.StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Sync",
            FamilyName = "Technician",
            DisplayName = "Sync Technician",
            PrimaryEmail = email,
            EmploymentStatus = "active",
            JobTitle = "Maintenance Tech",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await staffarrDb.SaveChangesAsync();

        var syncService = staffarrScope.ServiceProvider.GetRequiredService<StaffArrMaintainArrTechnicianRefSyncService>();
        await syncService.TryPublishPersonChangedAsync(
            PlatformSeeder.DemoTenantId,
            personId,
            "staffarr.person.created",
            CancellationToken.None);

        using var maintainarrScope = _maintainarrFactory.Services.CreateScope();
        var maintainarrDb = maintainarrScope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var mirror = await maintainarrDb.StaffPersonRefs
            .AsNoTracking()
            .SingleAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.StaffarrPersonId == personId.ToString("D"));
        Assert.Equal("Sync Technician", mirror.DisplayNameSnapshot);
        Assert.Equal("active", mirror.ActiveStatusSnapshot);
    }

    [Fact]
    public async Task Internal_refresh_pulls_lookup_from_staffarr()
    {
        using var staffarrScope = _staffarrFactory.Services.CreateScope();
        var staffarrDb = staffarrScope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var personId = Guid.NewGuid();
        staffarrDb.People.Add(new StaffArr.Api.Entities.StaffPerson
        {
            Id = personId,
            TenantId = PlatformSeeder.DemoTenantId,
            GivenName = "Fresh",
            FamilyName = "Lookup",
            DisplayName = "Fresh Lookup",
            PrimaryEmail = $"fresh.lookup.{Guid.NewGuid():N}@example.com",
            EmploymentStatus = "active",
            JobTitle = "Tech",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await staffarrDb.SaveChangesAsync();

        using (var scope = _maintainarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
            db.StaffPersonRefs.Add(new MaintainArrStaffPersonRef
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                StaffarrPersonId = personId.ToString("D"),
                DisplayNameSnapshot = "Stale Name",
                ActiveStatusSnapshot = "inactive",
                LastSeenAt = DateTimeOffset.UtcNow.AddDays(-2),
            });
            await db.SaveChangesAsync();
        }

        var workerToken = await IssueServiceTokenAsync(
            await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail),
            "shared-worker",
            ["maintainarr"],
            TechnicianRefSyncService.RefreshTechnicianRefsActionScope);

        var refreshRequest = ServiceAuthorized(
            HttpMethod.Post,
            "/api/internal/technician-refs/process-refresh",
            workerToken);
        refreshRequest.Content = JsonContent.Create(new ProcessTechnicianRefRefreshRequest(
            PlatformSeeder.DemoTenantId,
            null,
            50,
            TimeSpan.FromHours(1)));
        var refreshResponse = await _maintainarrClient.SendAsync(refreshRequest);
        refreshResponse.EnsureSuccessStatusCode();
        var refreshed = (await refreshResponse.Content.ReadFromJsonAsync<ProcessTechnicianRefRefreshResponse>())!;
        Assert.True(refreshed.RefreshedCount >= 1);

        using var maintainarrScope = _maintainarrFactory.Services.CreateScope();
        var maintainarrDb = maintainarrScope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var mirror = await maintainarrDb.StaffPersonRefs
            .AsNoTracking()
            .SingleAsync(x => x.TenantId == PlatformSeeder.DemoTenantId && x.StaffarrPersonId == personId.ToString("D"));
        Assert.Equal("Fresh Lookup", mirror.DisplayNameSnapshot);
        Assert.Equal("active", mirror.ActiveStatusSnapshot);
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"{sourceProduct}-sync-test-{Guid.NewGuid():N}",
            $"{sourceProduct} sync test",
            sourceProduct,
            allowedProducts));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            actionScope,
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private static HttpRequestMessage ServiceAuthorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new NexArr.Api.Contracts.LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.AuthTokenResponse>())!;
        return login.AccessToken;
    }
}
