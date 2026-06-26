using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using ComplianceCoreRedeemRequest = ComplianceCore.Api.Contracts.RedeemHandoffRequest;
using ComplianceCoreHandoffSessionResponse = ComplianceCore.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.ComplianceCore.Auth.Tests;

public sealed class ComplianceCoreHandoffApiTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::ComplianceCore.Api.Program> _complianceFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _complianceClient = null!;
    private string _serviceToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ComplianceCoreHandoffNexArrTests-{Guid.NewGuid():N}";
        var complianceDbName = $"ComplianceCoreHandoffTests-{Guid.NewGuid():N}";

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
        _serviceToken = await IssueServiceTokenAsync(adminToken, "compliancecore");

        _complianceFactory = new WebApplicationFactory<global::ComplianceCore.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", _serviceToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<ComplianceCoreDbContext>(services);
                services.AddDbContext<ComplianceCoreDbContext>(options => options.UseInMemoryDatabase(complianceDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<StlNexArrLaunchClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _complianceClient = _complianceFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _complianceClient.Dispose();
        _nexarrClient.Dispose();
        await _complianceFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Handoff_redeem_happy_path_returns_session_and_me_works()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _complianceClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new ComplianceCoreRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<ComplianceCoreHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Equal(PlatformSeeder.DemoAdminUserId, session.UserId);
        Assert.Contains("compliancecore", session.LaunchableProductKeys);

        var meRequest = Authorized(HttpMethod.Get, "/api/me", session.AccessToken);
        var meResponse = await _complianceClient.SendAsync(meRequest);
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<ComplianceCoreMeResponse>();
        Assert.NotNull(me);
        Assert.True(me.HasComplianceCoreAccess);
    }

    [Fact]
    public async Task Handoff_redeem_nexarr_alias_happy_path_returns_session()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _complianceClient.PostAsJsonAsync(
            "/api/auth/nexarr/redeem",
            new ComplianceCoreRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<ComplianceCoreHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Contains("compliancecore", session.LaunchableProductKeys);
    }

    [Fact]
    public async Task Handoff_redeem_forbids_non_platform_admin_users()
    {
        var handoffCode = await SeedHandoffCodeAsync(PlatformSeeder.DemoTenantAdminUserId);
        var redeemResponse = await _complianceClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new ComplianceCoreRedeemRequest(handoffCode));

        Assert.Equal(HttpStatusCode.Forbidden, redeemResponse.StatusCode);
    }

    [Fact]
    public async Task V1_handoff_session_and_me_aliases_work()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _complianceClient.PostAsJsonAsync(
            "/api/v1/auth/handoff/redeem",
            new ComplianceCoreRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<ComplianceCoreHandoffSessionResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Contains("compliancecore", session.LaunchableProductKeys);

        var meResponse = await _complianceClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/me", session.AccessToken));
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<ComplianceCoreMeResponse>();
        Assert.NotNull(me);
        Assert.True(me.HasComplianceCoreAccess);

        var sessionResponse = await _complianceClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/session", session.AccessToken));
        sessionResponse.EnsureSuccessStatusCode();
        var bootstrap = await sessionResponse.Content.ReadFromJsonAsync<ComplianceCoreSessionBootstrapResponse>();
        Assert.NotNull(bootstrap);
        Assert.True(bootstrap.HasComplianceCoreAccess);
    }

    [Fact]
    public async Task V1_launch_handoff_proxy_returns_handoff_code()
    {
        var nexarrToken = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", nexarrToken);
        request.Content = JsonContent.Create(new CreateHandoffRequest("compliancecore", "http://localhost:5177/launch"));
        var response = await _complianceClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        Assert.False(string.IsNullOrWhiteSpace(handoff.HandoffCode));
    }

    [Fact]
    public async Task Session_bootstrap_returns_claim_backed_identity_after_non_compliancecore_launch_context()
    {
        var token = CreateComplianceCoreAccessToken(["nexarr"], "compliance_admin", isPlatformAdmin: true);
        var request = Authorized(HttpMethod.Get, "/api/session", token);
        var response = await _complianceClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ComplianceCoreSessionBootstrapResponse>();
        Assert.NotNull(payload);
        Assert.Equal(PlatformSeeder.DemoAdminUserId, payload.UserId);
        Assert.True(payload.HasComplianceCoreAccess);
        Assert.True(payload.IsPlatformAdmin);
    }

    [Fact]
    public async Task Session_and_me_forbid_non_platform_admin_users_even_with_compliancecore_launch_context()
    {
        var token = CreateComplianceCoreAccessToken(["compliancecore"], "compliance_admin");

        var sessionResponse = await _complianceClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/session", token));
        Assert.Equal(HttpStatusCode.Forbidden, sessionResponse.StatusCode);

        var meResponse = await _complianceClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/me", token));
        Assert.Equal(HttpStatusCode.Forbidden, meResponse.StatusCode);

        using var scope = _complianceFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ComplianceCoreDbContext>();
        var deniedEvents = await db.AuditEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.Action == "compliancecore.admin_access.denied")
            .ToListAsync();

        Assert.Contains(deniedEvents, x => x.TargetType == "session" && x.ReasonCode == "auth.platform_admin_required");
        Assert.Contains(deniedEvents, x => x.TargetType == "me" && x.ReasonCode == "auth.platform_admin_required");
    }

    [Fact]
    public async Task Me_forbids_users_without_platform_admin_access()
    {
        var token = CreateComplianceCoreAccessToken(["nexarr"], "compliance_admin");
        var request = Authorized(HttpMethod.Get, "/api/me", token);
        var response = await _complianceClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<string> CreateHandoffAsync(string email = PlatformSeeder.DemoAdminEmail)
    {
        var token = await LoginNexArrAsync(email);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("compliancecore", "http://localhost:5177/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> SeedHandoffCodeAsync(Guid userId)
    {
        var handoffCode = $"compliancecore-handoff-{Guid.NewGuid():N}";
        var sessionId = Guid.NewGuid();

        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        db.UserSessions.Add(new UserSession
        {
            Id = sessionId,
            UserId = userId,
            RefreshTokenHash = Guid.NewGuid().ToString("N"),
            ActiveTenantId = PlatformSeeder.DemoTenantId,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
            CreatedAt = DateTimeOffset.UtcNow
        });
        db.HandoffCodes.Add(new HandoffCodeRecord
        {
            Id = Guid.NewGuid(),
            CodeHash = LaunchService.HashHandoffCode(handoffCode),
            UserId = userId,
            RequestedByPersonId = userId,
            TenantId = PlatformSeeder.DemoTenantId,
            SessionId = sessionId,
            TargetProductKey = "compliancecore",
            CallbackUrl = "http://localhost:5177/launch",
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();
        return handoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-compliancecore-handoff-test",
            $"{productKey} ComplianceCore Handoff Test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
    }

    private string CreateComplianceCoreAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey,
        bool isPlatformAdmin = false)
    {
        using var scope = _complianceFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ComplianceCoreTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin);
        return token;
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var login = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return login.AccessToken;
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
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>)
                || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}

