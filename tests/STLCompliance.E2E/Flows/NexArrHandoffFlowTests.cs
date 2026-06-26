using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Services;
using RoutArr.Api.Data;
using StaffArr.Api.Data;
using STLCompliance.E2E.Support;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;
using StaffArrRedeemRequest = StaffArr.Api.Contracts.RedeemHandoffRequest;
using StaffArrHandoffSessionResponse = StaffArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.E2E.Flows;

/// <summary>
/// NexArr login → handoff code → product session redeem → launchable /api/me bootstrap.
/// </summary>
[Trait("Category", "Integration")]
public sealed class NexArrHandoffFlowTests : IAsyncLifetime
{
    private E2ENexArrHost _nexarr = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _routarrClient = null!;

    public async Task InitializeAsync()
    {
        _nexarr = new E2ENexArrHost();
        await _nexarr.InitializeAsync();

        var adminToken = await _nexarr.LoginAsync();
        var staffarrHandoffToken = await _nexarr.IssueServiceTokenAsync(adminToken, "staffarr", "launch.redeem");
        var routarrHandoffToken = await _nexarr.IssueServiceTokenAsync(adminToken, "routarr", "launch.redeem");

        var staffArrDbName = $"E2E-StaffArr-Handoff-{Guid.NewGuid():N}";
        _staffarrFactory = new WebApplicationFactory<global::StaffArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarr.Client.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", staffarrHandoffToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<StaffArrDbContext>(services);
                services.AddDbContext<StaffArrDbContext>(options => options.UseInMemoryDatabase(staffArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarr.Factory.Server.CreateHandler());
            });
        });
        _staffarrClient = _staffarrFactory.CreateClient();

        var routArrDbName = $"E2E-RoutArr-Handoff-{Guid.NewGuid():N}";
        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", E2ENexArrHost.SigningKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarr.Client.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", routarrHandoffToken);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));
                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarr.Factory.Server.CreateHandler());
            });
        });
        _routarrClient = _routarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _staffarrClient.Dispose();
        _routarrClient.Dispose();
        await _staffarrFactory.DisposeAsync();
        await _routarrFactory.DisposeAsync();
        await _nexarr.DisposeAsync();
    }

    [Fact]
    public async Task NexArr_login_and_me_returns_launchable_products()
    {
        var loginResponse = await _nexarr.Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(
                PlatformSeeder.DemoAdminEmail,
                PlatformSeeder.DemoAdminPassword,
                PlatformSeeder.DemoTenantId));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var tokens = (await loginResponse.Content.ReadFromJsonAsync<AuthTokenResponse>())!;

        var meRequest = HttpTestClient.Authorized(HttpMethod.Get, "/api/me", tokens.AccessToken);
        var meResponse = await _nexarr.Client.SendAsync(meRequest);
        meResponse.EnsureSuccessStatusCode();
        var me = (await meResponse.Content.ReadFromJsonAsync<MeResponse>())!;

        Assert.Equal(PlatformSeeder.DemoAdminEmail, me.Email);
        Assert.Contains("staffarr", me.LaunchableProductKeys);
        Assert.Contains("routarr", me.LaunchableProductKeys);
    }

    [Theory]
    [InlineData("staffarr", "http://localhost:5175/launch")]
    [InlineData("routarr", "http://localhost:5180/launch")]
    public async Task Handoff_redeem_establishes_product_session(string productKey, string callbackUrl)
    {
        var handoffCode = await _nexarr.CreateHandoffAsync(productKey, callbackUrl);
        var client = productKey == "staffarr" ? _staffarrClient : _routarrClient;

        var redeemResponse = await client.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            productKey == "staffarr"
                ? (object)new StaffArrRedeemRequest(handoffCode)
                : new RoutArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();

        if (productKey == "staffarr")
        {
            var session = (await redeemResponse.Content.ReadFromJsonAsync<StaffArrHandoffSessionResponse>())!;
            Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
            Assert.Contains("staffarr", session.LaunchableProductKeys);
        }
        else
        {
            var session = (await redeemResponse.Content.ReadFromJsonAsync<RoutArrHandoffSessionResponse>())!;
            Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
            Assert.Contains("routarr", session.LaunchableProductKeys);
        }
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

