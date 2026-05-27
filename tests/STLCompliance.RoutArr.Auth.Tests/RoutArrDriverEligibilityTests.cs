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
using RoutArr.Api.Endpoints;
using RoutArr.Api.Services;
using StaffArr.Api.Data;
using StaffArr.Api.Endpoints;
using StaffArr.Api.Entities;
using RoutArrRedeemRequest = RoutArr.Api.Contracts.RedeemHandoffRequest;
using RoutArrHandoffSessionResponse = RoutArr.Api.Contracts.HandoffSessionResponse;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrDriverEligibilityTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::StaffArr.Api.Program> _staffarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _staffarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _routarrToStaffarrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrEligNexArr-{Guid.NewGuid():N}";
        var staffArrDbName = $"RoutArrEligStaffArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrElig-{Guid.NewGuid():N}";

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
        var routarrHandoffToken = await IssueServiceTokenAsync(adminToken, "routarr", "launch.redeem");
        _routarrToStaffarrToken = await IssueServiceTokenAsync(
            adminToken,
            "routarr",
            IntegrationEndpoints.RoutarrReadinessDispatchActionScope,
            ["staffarr"]);

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
        using (var scope = _staffarrFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<StaffArrDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("NexArr:BaseUrl", _nexarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("Handoff:ServiceToken", routarrHandoffToken);
            builder.UseSetting("StaffArr:BaseUrl", _staffarrClient.BaseAddress!.ToString().TrimEnd('/'));
            builder.UseSetting("StaffArr:ServiceToken", _routarrToStaffarrToken);
            builder.UseSetting("DriverEligibility:CheckTrainArrQualification", "false");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));

                services.AddHttpClient<NexArrHandoffClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _nexarrFactory.Server.CreateHandler());
                services.AddHttpClient<StaffArrReadinessClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrFactory.Server.CreateHandler());
            });
        });

        _routarrClient = _routarrFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _staffarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _staffarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Driver_eligibility_check_reports_staffarr_not_ready()
    {
        var personId = await SeedStaffPersonAsync("Not Ready Driver", "not.ready.driver@example.com");
        var dispatcherToken = await RedeemRoutArrTokenAsync();

        var checkRequest = Authorized(HttpMethod.Post, "/api/driver-eligibility/check", dispatcherToken);
        checkRequest.Content = JsonContent.Create(new DriverEligibilityCheckRequest(personId.ToString()));
        var checkResponse = await _routarrClient.SendAsync(checkRequest);
        checkResponse.EnsureSuccessStatusCode();
        var check = (await checkResponse.Content.ReadFromJsonAsync<DriverEligibilityCheckResponse>())!;

        Assert.Equal(DriverEligibilityOutcomes.Block, check.Outcome);
        Assert.Equal("staffarr_not_ready", check.ReasonCode);
        Assert.True(check.IsBlocking);
        Assert.NotNull(check.StaffArr);
        Assert.Equal("not_ready", check.StaffArr!.ReadinessStatus);
    }

    [Fact]
    public async Task Assign_driver_blocked_when_staffarr_not_ready_and_override_succeeds()
    {
        var personId = await SeedStaffPersonAsync("Blocked Driver", "blocked.driver@example.com");
        var dispatcherToken = await RedeemRoutArrTokenAsync();
        var now = DateTimeOffset.UtcNow;
        var trip = await CreateTripAsync(dispatcherToken, now.AddHours(2), now.AddHours(6));

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(personId.ToString()));
        var blocked = await _routarrClient.SendAsync(assignRequest);
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);

        var previewRequest = Authorized(HttpMethod.Post, "/api/dispatch/assignments/preview", dispatcherToken);
        previewRequest.Content = JsonContent.Create(new DispatchAssignmentPreviewRequest(
            trip.TripId,
            "driver",
            personId.ToString(),
            null));
        var previewResponse = await _routarrClient.SendAsync(previewRequest);
        previewResponse.EnsureSuccessStatusCode();
        var preview = (await previewResponse.Content.ReadFromJsonAsync<DispatchAssignmentPreviewResponse>())!;
        Assert.True(preview.HasBlockingConflicts);
        Assert.NotNull(preview.DriverEligibility);
        Assert.Equal(DriverEligibilityOutcomes.Block, preview.DriverEligibility!.Outcome);

        assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", dispatcherToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            personId.ToString(),
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: true));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();
    }

    private async Task<Guid> SeedStaffPersonAsync(string displayName, string email)
    {
        var personId = Guid.NewGuid();
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
        return personId;
    }

    private async Task<TripDetailResponse> CreateTripAsync(
        string dispatcherToken,
        DateTimeOffset tripStart,
        DateTimeOffset tripEnd)
    {
        var createTripRequest = Authorized(HttpMethod.Post, "/api/trips", dispatcherToken);
        createTripRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Eligibility trip",
            "Driver eligibility test",
            null,
            tripStart,
            tripEnd,
            null));
        var createTripResponse = await _routarrClient.SendAsync(createTripRequest);
        createTripResponse.EnsureSuccessStatusCode();
        return (await createTripResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;
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
        var request = Authorized(HttpMethod.Post, "/api/launch/handoff", token);
        request.Content = JsonContent.Create(new CreateHandoffRequest("routarr", "http://localhost:5180/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string productKey,
        string actionScope,
        string[]? targetProducts = null)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{productKey}-eligibility-{Guid.NewGuid():N}",
            $"{productKey} Eligibility Test",
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
            actionScope,
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
