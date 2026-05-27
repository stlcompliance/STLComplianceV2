using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrFieldInboxTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"MaintainArrFieldInboxNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"MaintainArrFieldInbox-{Guid.NewGuid():N}";

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
        var serviceToken = await IssueServiceTokenAsync(adminToken, "maintainarr");

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", serviceToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));

                services.AddHttpClient<NexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
            });
        });

        _maintainarrClient = _maintainarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Field_inbox_returns_assigned_open_work_order_for_technician()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetAsync(token);

        var createResponse = await _maintainarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/work-orders",
            token,
            new CreateWorkOrderRequest(
                assetId,
                "Replace conveyor belt",
                "Field task for inbox",
                WorkOrderPriorities.High,
                PlatformSeeder.DemoAdminUserId.ToString(),
                null)));
        createResponse.EnsureSuccessStatusCode();

        var inboxResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/field-inbox", token));
        inboxResponse.EnsureSuccessStatusCode();

        var inbox = (await inboxResponse.Content.ReadFromJsonAsync<FieldInboxResponse>())!;
        Assert.Equal(1, inbox.Summary.TotalCount);
        Assert.Equal("work_order", inbox.Items[0].TaskType);
        Assert.Contains("Replace conveyor belt", inbox.Items[0].Title, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Field_inbox_requires_authentication()
    {
        var response = await _maintainarrClient.GetAsync("/api/field-inbox");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<Guid> SeedAssetAsync(string token)
    {
        var classResponse = await _maintainarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/asset-classes",
            token,
            new CreateAssetClassRequest(
                "production",
                "Production equipment",
                "Production line assets")));
        classResponse.EnsureSuccessStatusCode();
        var assetClass = (await classResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var typeResponse = await _maintainarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/asset-types",
            token,
            new CreateAssetTypeRequest(
                assetClass.AssetClassId,
                "conveyor",
                "Conveyor",
                "Belt conveyor")));
        typeResponse.EnsureSuccessStatusCode();
        var assetType = (await typeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;

        var assetResponse = await _maintainarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/assets",
            token,
            new CreateAssetRequest(
                assetType.AssetTypeId,
                "PMP-100",
                "Primary conveyor",
                "Main line",
                null)));
        assetResponse.EnsureSuccessStatusCode();
        var asset = (await assetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        return asset.AssetId;
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffResponse = await _nexarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/launch/handoff",
            await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail),
            new CreateHandoffRequest("maintainarr", null)));
        handoffResponse.EnsureSuccessStatusCode();
        var handoff = (await handoffResponse.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;

        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoff.HandoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private static HttpRequestMessage Authorized(
        HttpMethod method,
        string path,
        string token,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private async Task SeedNexArrAsync()
    {
        await using var scope = _nexarrFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, passwordHasher);
    }

    private async Task<string> LoginNexArrAsync(string email)
    {
        var response = await _nexarrClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return auth.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerResponse = await _nexarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/service-tokens/clients",
            adminToken,
            new RegisterServiceClientRequest(
                $"{productKey}-field-inbox-test",
                $"{productKey} field inbox test",
                productKey,
                [productKey])));
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueResponse = await _nexarrClient.SendAsync(Authorized(
            HttpMethod.Post,
            "/api/service-tokens",
            adminToken,
            new IssueServiceTokenRequest(
                client.ServiceClientId,
                PlatformSeeder.DemoTenantId,
                null,
                "launch.redeem",
                30)));
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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
