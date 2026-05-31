using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;
using TrainArr.Api.Data;
using TrainArr.Api.Endpoints;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrDriverEligibilityTrainArrConsumptionTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::TrainArr.Api.Program> _trainarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _trainarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _routarrToTrainarrToken = null!;
    private string _crossProductToTrainarrQualificationToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrTrainArrEligNexArr-{Guid.NewGuid():N}";
        var trainArrDbName = $"RoutArrTrainArrEligTrainArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrTrainArrElig-{Guid.NewGuid():N}";

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
        var routarrHandoffToken = await IssueServiceTokenAsync(adminToken, "routarr", actionScope: "launch.redeem");
        _routarrToTrainarrToken = await IssueServiceTokenAsync(
            adminToken,
            "routarr",
            ["trainarr"],
            IntegrationEndpoints.RoutarrQualificationCheckActionScope);
        _crossProductToTrainarrQualificationToken = await IssueServiceTokenAsync(
            adminToken,
            "routarr",
            ["trainarr"],
            IntegrationEndpoints.QualificationCheckReadActionScope);

        _trainarrFactory = new WebApplicationFactory<global::TrainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<TrainArrDbContext>(services);
                services.AddDbContext<TrainArrDbContext>(options => options.UseInMemoryDatabase(trainArrDbName));
            });
        });

        _trainarrClient = _trainarrFactory.CreateClient();

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", routarrHandoffToken);
            builder.UseSetting("TrainArr:BaseUrl", _trainarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("TrainArr:ServiceToken", _routarrToTrainarrToken);
            builder.UseSetting("DriverEligibility:CheckTrainArrQualification", "true");
            builder.UseSetting("DriverEligibility:CheckStaffArrReadiness", "false");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));

                services.AddHttpClient<StlNexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<TrainArrQualificationCheckClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _trainarrFactory.Server.CreateHandler());
            });
        });

        _routarrClient = _routarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _trainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _trainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Driver_eligibility_check_consumes_trainarr_qualification_authorization()
    {
        var personId = Guid.NewGuid();
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var checkRequest = Authorized(HttpMethod.Post, "/api/driver-eligibility/check", dispatcherToken);
        checkRequest.Content = JsonContent.Create(new DriverEligibilityCheckRequest(personId.ToString()));
        var checkResponse = await _routarrClient.SendAsync(checkRequest);
        checkResponse.EnsureSuccessStatusCode();
        var check = (await checkResponse.Content.ReadFromJsonAsync<DriverEligibilityCheckResponse>())!;

        Assert.NotNull(check.TrainArr);
        Assert.Equal("driver_qualification", check.TrainArr!.QualificationKey);
        Assert.False(string.IsNullOrWhiteSpace(check.TrainArr.Outcome));
        Assert.False(string.IsNullOrWhiteSpace(check.TrainArr.Message));
        Assert.Equal(check.TrainArr.Outcome, check.Outcome);
    }

    [Fact]
    public async Task Cross_product_qualification_check_endpoint_supports_v1_alias()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/integrations/qualification-check");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _crossProductToTrainarrQualificationToken);
        request.Content = JsonContent.Create(new TrainArr.Api.Contracts.RoutarrQualificationCheckRequest(
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            "driver_qualification",
            null,
            null));

        var response = await _trainarrClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Qualification-check integration failed: {(int)response.StatusCode} {error}");
        }
        var payload = await response.Content.ReadFromJsonAsync<TrainArr.Api.Contracts.QualificationCheckResponse>();
        Assert.NotNull(payload);
        Assert.Equal("driver_qualification", payload!.QualificationKey);
    }

    [Fact]
    public async Task Cross_product_batch_qualification_check_endpoint_supports_assignment_boards()
    {
        var firstPersonId = Guid.NewGuid();
        var secondPersonId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/integrations/qualification-check/batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _crossProductToTrainarrQualificationToken);
        request.Content = JsonContent.Create(new TrainArr.Api.Contracts.CreateIntegrationBatchQualificationCheckRequest(
            PlatformSeeder.DemoTenantId,
            "driver_qualification",
            null,
            [
                new TrainArr.Api.Contracts.BatchQualificationCheckSubject(
                    firstPersonId,
                    new Dictionary<string, string> { ["board"] = "dispatch" }),
                new TrainArr.Api.Contracts.BatchQualificationCheckSubject(
                    secondPersonId,
                    new Dictionary<string, string> { ["board"] = "dispatch" }),
            ]));

        var response = await _trainarrClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Batch qualification-check integration failed: {(int)response.StatusCode} {error}");
        }

        var payload = (await response.Content.ReadFromJsonAsync<TrainArr.Api.Contracts.BatchQualificationCheckResponse>())!;
        Assert.Equal("driver_qualification", payload.QualificationKey);
        Assert.Equal(2, payload.Summary.Total);
        Assert.Equal(2, payload.Results.Count);
        Assert.Contains(payload.Results, x => x.StaffarrPersonId == firstPersonId);
        Assert.Contains(payload.Results, x => x.StaffarrPersonId == secondPersonId);
    }

    [Fact]
    public async Task Cross_product_batch_qualification_check_requires_read_scope()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/integrations/qualification-check/batch");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _routarrToTrainarrToken);
        request.Content = JsonContent.Create(new TrainArr.Api.Contracts.CreateIntegrationBatchQualificationCheckRequest(
            PlatformSeeder.DemoTenantId,
            "driver_qualification",
            null,
            [new TrainArr.Api.Contracts.BatchQualificationCheckSubject(Guid.NewGuid(), null)]));

        var response = await _trainarrClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<string> RedeemRoutArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/handoff/redeem")
        {
            Content = JsonContent.Create(new RoutArrRedeemRequest(handoffCode)),
        };
        var redeemResponse = await _routarrClient.SendAsync(redeemRequest);
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<RoutArrHandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("routarr", "http://localhost:5180/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string productKey,
        string[]? targetProducts = null,
        string? actionScope = null)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-trainarr-elig-{Guid.NewGuid():N}",
            $"{productKey} TrainArr Eligibility Test",
            productKey,
            targetProducts ?? [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            actionScope ?? "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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
