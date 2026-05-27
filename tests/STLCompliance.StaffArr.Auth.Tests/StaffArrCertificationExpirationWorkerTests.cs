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

namespace STLCompliance.StaffArr.Auth.Tests;

public class StaffArrCertificationExpirationWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private string _sharedWorkerToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"CertExpirationNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"CertExpirationStaffArr-{Guid.NewGuid():N}";

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
            CertificationExpirationService.ProcessExpirationsActionScope);

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
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_expirations_rejects_missing_service_token()
    {
        var response = await _staffarrClient.PostAsJsonAsync(
            "/api/internal/certifications/process-expirations",
            new ProcessCertificationExpirationsRequest(null, DateTimeOffset.UtcNow, 50));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_expirations_rejects_trainarr_source_token()
    {
        var adminToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var trainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "trainarr",
            ["staffarr"],
            CertificationExpirationService.ProcessExpirationsActionScope);

        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/certifications/process-expirations");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", trainarrToken);
        processRequest.Content = JsonContent.Create(new ProcessCertificationExpirationsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50));

        var response = await _staffarrClient.SendAsync(processRequest);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Process_expirations_expires_past_due_manual_certification()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Cert Expiration Worker", "cert.expiration@example.com");
        var certification = await SeedExpiredManualCertificationAsync(personId);

        var processRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/internal/certifications/process-expirations");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToStaffarrToken);
        processRequest.Content = JsonContent.Create(new ProcessCertificationExpirationsRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50));

        var processResponse = await _staffarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessCertificationExpirationsResponse>())!;
        Assert.Equal(1, body.ExpiredCount);
        Assert.Contains(certification.Id, body.ExpiredPersonCertificationIds);

        using var staffarrScope = _staffarrFactory.Services.CreateScope();
        var staffarrDb = staffarrScope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var stored = await staffarrDb.PersonCertifications.SingleAsync(x => x.Id == certification.Id);
        Assert.Equal("expired", stored.Status);
    }

    [Fact]
    public async Task List_pending_expiration_returns_candidates_before_processing()
    {
        var personId = Guid.NewGuid();
        await SeedStaffPersonAsync(personId, "Pending Cert Expiration", "pending.cert@example.com");
        var certification = await SeedExpiredManualCertificationAsync(personId);

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/certifications/pending-expiration?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToStaffarrToken);

        var listResponse = await _staffarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingCertificationExpirationsResponse>())!;
        Assert.Contains(pending.Items, x => x.PersonCertificationId == certification.Id);
    }

    private async Task<PersonCertification> SeedExpiredManualCertificationAsync(Guid personId)
    {
        using var scope = _staffarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddDays(-3);

        await StaffArrReadinessCertificationSeed.EnsureBaselineDefinitionsAsync(
            db,
            PlatformSeeder.DemoTenantId,
            CancellationToken.None);

        var definition = await db.CertificationDefinitions.FirstAsync(
            x => x.TenantId == PlatformSeeder.DemoTenantId && x.CertificationKey == "readiness.safety_orientation");

        var certification = new PersonCertification
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            PersonId = personId,
            CertificationDefinitionId = definition.Id,
            SourceType = "manual",
            Status = "active",
            GrantedAt = now.AddMonths(-6),
            ExpiresAt = expiresAt,
            Notes = "Seeded manual certification for expiration worker integration test.",
            GrantedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now.AddMonths(-6),
            UpdatedAt = now.AddMonths(-6)
        };

        db.PersonCertifications.Add(certification);
        await db.SaveChangesAsync();
        return certification;
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
            $"{sourceProduct}-cert-expiration-{Guid.NewGuid():N}",
            $"{sourceProduct} certification expiration test",
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
