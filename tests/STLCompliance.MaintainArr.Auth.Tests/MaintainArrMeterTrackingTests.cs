using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using AssetClassResponse = MaintainArr.Api.Contracts.AssetClassResponse;
using AssetTypeResponse = MaintainArr.Api.Contracts.AssetTypeResponse;
using AssetResponse = MaintainArr.Api.Contracts.AssetResponse;
using CreateAssetClassRequest = MaintainArr.Api.Contracts.CreateAssetClassRequest;
using CreateAssetTypeRequest = MaintainArr.Api.Contracts.CreateAssetTypeRequest;
using CreateAssetRequest = MaintainArr.Api.Contracts.CreateAssetRequest;
using RedeemHandoffRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using HandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrMeterTrackingTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"MeterNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"MeterMaintainArr-{Guid.NewGuid():N}";

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

                services.AddHttpClient<StlNexArrHandoffClient>()
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
    public async Task Create_meter_record_reading_and_list_history()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetAsync(token);

        var createMeterRequest = Authorized(HttpMethod.Post, $"/api/assets/{assetId}/meters", token);
        createMeterRequest.Content = JsonContent.Create(new CreateAssetMeterRequest(
            "engine-hours",
            "Engine hours",
            "Primary hour meter",
            "hours",
            1000m));
        var createMeterResponse = await _maintainarrClient.SendAsync(createMeterRequest);
        createMeterResponse.EnsureSuccessStatusCode();
        var meter = (await createMeterResponse.Content.ReadFromJsonAsync<AssetMeterResponse>())!;
        Assert.Equal(1000m, meter.CurrentReading);

        var recordRequest = Authorized(HttpMethod.Post, $"/api/meters/{meter.AssetMeterId}/readings", token);
        recordRequest.Content = JsonContent.Create(new RecordMeterReadingRequest(1050m, null, "Monthly reading", false));
        var recordResponse = await _maintainarrClient.SendAsync(recordRequest);
        recordResponse.EnsureSuccessStatusCode();
        var reading = (await recordResponse.Content.ReadFromJsonAsync<MeterReadingResponse>())!;
        Assert.Equal(50m, reading.DeltaFromPrevious);

        var listRequest = Authorized(HttpMethod.Get, $"/api/meters/{meter.AssetMeterId}/readings", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var readings = (await listResponse.Content.ReadFromJsonAsync<List<MeterReadingResponse>>())!;
        Assert.Single(readings);
        Assert.Equal(1050m, readings[0].ReadingValue);

        var getMeterRequest = Authorized(HttpMethod.Get, $"/api/meters/{meter.AssetMeterId}", token);
        var getMeterResponse = await _maintainarrClient.SendAsync(getMeterRequest);
        getMeterResponse.EnsureSuccessStatusCode();
        var updatedMeter = (await getMeterResponse.Content.ReadFromJsonAsync<AssetMeterResponse>())!;
        Assert.Equal(1050m, updatedMeter.CurrentReading);
    }

    [Fact]
    public async Task Create_meter_record_reading_and_list_history_v1_alias()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetAsync(token);

        var createMeterRequest = Authorized(HttpMethod.Post, $"/api/v1/assets/{assetId}/meters", token);
        createMeterRequest.Content = JsonContent.Create(new CreateAssetMeterRequest(
            "engine-hours-v1",
            "Engine hours v1",
            "Primary hour meter",
            "hours",
            2000m));
        var createMeterResponse = await _maintainarrClient.SendAsync(createMeterRequest);
        createMeterResponse.EnsureSuccessStatusCode();
        var meter = (await createMeterResponse.Content.ReadFromJsonAsync<AssetMeterResponse>())!;

        var recordRequest = Authorized(HttpMethod.Post, $"/api/v1/meters/{meter.AssetMeterId}/readings", token);
        recordRequest.Content = JsonContent.Create(new RecordMeterReadingRequest(2050m, null, "v1 monthly", false));
        var recordResponse = await _maintainarrClient.SendAsync(recordRequest);
        recordResponse.EnsureSuccessStatusCode();

        var listRequest = Authorized(HttpMethod.Get, $"/api/v1/meters/{meter.AssetMeterId}/readings", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var readings = (await listResponse.Content.ReadFromJsonAsync<List<MeterReadingResponse>>())!;
        Assert.Single(readings);
    }

    [Fact]
    public async Task Meter_reading_marks_linked_pm_schedule_due_from_usage()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetAsync(token);

        var meter = await CreateMeterAsync(token, assetId, 1000m);
        var schedule = await CreateMeterPmScheduleAsync(token, assetId, meter.AssetMeterId, intervalUsage: 100m, nextDueAtUsage: 1100m);

        var recordRequest = Authorized(HttpMethod.Post, $"/api/meters/{meter.AssetMeterId}/readings", token);
        recordRequest.Content = JsonContent.Create(new RecordMeterReadingRequest(1100m, null, string.Empty, false));
        var recordResponse = await _maintainarrClient.SendAsync(recordRequest);
        recordResponse.EnsureSuccessStatusCode();

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var stored = await db.PmSchedules.SingleAsync(x => x.Id == schedule.PmScheduleId);
        Assert.Equal(PmDueStatuses.Due, stored.DueStatus);

        var forecastRequest = Authorized(HttpMethod.Get, $"/api/meters/{meter.AssetMeterId}/pm-forecast", token);
        var forecastResponse = await _maintainarrClient.SendAsync(forecastRequest);
        forecastResponse.EnsureSuccessStatusCode();
        var forecast = (await forecastResponse.Content.ReadFromJsonAsync<MeterPmForecastResponse>())!;
        Assert.Contains(forecast.LinkedSchedules, x => x.PmScheduleId == schedule.PmScheduleId && x.DueStatus == PmDueStatuses.Due);
    }

    [Fact]
    public async Task Record_reading_rejects_regression_without_correction()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetAsync(token);
        var meter = await CreateMeterAsync(token, assetId, 500m);

        var recordRequest = Authorized(HttpMethod.Post, $"/api/meters/{meter.AssetMeterId}/readings", token);
        recordRequest.Content = JsonContent.Create(new RecordMeterReadingRequest(400m, null, string.Empty, false));
        var recordResponse = await _maintainarrClient.SendAsync(recordRequest);
        Assert.Equal(HttpStatusCode.BadRequest, recordResponse.StatusCode);
    }

    [Fact]
    public async Task Record_reading_requires_authentication()
    {
        var response = await _maintainarrClient.PostAsJsonAsync(
            $"/api/meters/{Guid.NewGuid()}/readings",
            new RecordMeterReadingRequest(100m, null, string.Empty, false));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<AssetMeterResponse> CreateMeterAsync(string token, Guid assetId, decimal baseline)
    {
        var createMeterRequest = Authorized(HttpMethod.Post, $"/api/assets/{assetId}/meters", token);
        createMeterRequest.Content = JsonContent.Create(new CreateAssetMeterRequest(
            $"meter-{Guid.NewGuid():N}".Substring(0, 10),
            "Hour meter",
            string.Empty,
            "hours",
            baseline));
        var createMeterResponse = await _maintainarrClient.SendAsync(createMeterRequest);
        createMeterResponse.EnsureSuccessStatusCode();
        return (await createMeterResponse.Content.ReadFromJsonAsync<AssetMeterResponse>())!;
    }

    private async Task<PmScheduleResponse> CreateMeterPmScheduleAsync(
        string token,
        Guid assetId,
        Guid assetMeterId,
        decimal intervalUsage,
        decimal nextDueAtUsage)
    {
        var createScheduleRequest = Authorized(HttpMethod.Post, "/api/preventive-maintenance/schedules", token);
        createScheduleRequest.Content = JsonContent.Create(new CreatePmScheduleRequest(
            assetId,
            $"pm-{Guid.NewGuid():N}".Substring(0, 10),
            "Oil change (hours)",
            "Meter-based oil change",
            90,
            DateTimeOffset.UtcNow.AddDays(90),
            PmScheduleModes.Meter,
            assetMeterId,
            intervalUsage,
            nextDueAtUsage));
        var createScheduleResponse = await _maintainarrClient.SendAsync(createScheduleRequest);
        createScheduleResponse.EnsureSuccessStatusCode();
        return (await createScheduleResponse.Content.ReadFromJsonAsync<PmScheduleResponse>())!;
    }

    private async Task<Guid> SeedAssetAsync(string token)
    {
        var createClassRequest = Authorized(HttpMethod.Post, "/api/asset-classes", token);
        createClassRequest.Content = JsonContent.Create(new CreateAssetClassRequest(
            $"vehicles-{Guid.NewGuid():N}".Substring(0, 12),
            "Vehicles",
            string.Empty));
        var createClassResponse = await _maintainarrClient.SendAsync(createClassRequest);
        createClassResponse.EnsureSuccessStatusCode();
        var assetClass = (await createClassResponse.Content.ReadFromJsonAsync<AssetClassResponse>())!;

        var createTypeRequest = Authorized(HttpMethod.Post, "/api/asset-types", token);
        createTypeRequest.Content = JsonContent.Create(new CreateAssetTypeRequest(
            assetClass.AssetClassId,
            $"forklift-{Guid.NewGuid():N}".Substring(0, 12),
            "Forklift",
            string.Empty));
        var createTypeResponse = await _maintainarrClient.SendAsync(createTypeRequest);
        createTypeResponse.EnsureSuccessStatusCode();
        var assetType = (await createTypeResponse.Content.ReadFromJsonAsync<AssetTypeResponse>())!;

        var createAssetRequest = Authorized(HttpMethod.Post, "/api/assets", token);
        createAssetRequest.Content = JsonContent.Create(new CreateAssetRequest(
            assetType.AssetTypeId,
            $"MTR-{Guid.NewGuid():N}".Substring(0, 10),
            "Meter Test Asset",
            string.Empty,
            null));
        var createAssetResponse = await _maintainarrClient.SendAsync(createAssetRequest);
        createAssetResponse.EnsureSuccessStatusCode();
        var asset = (await createAssetResponse.Content.ReadFromJsonAsync<AssetResponse>())!;
        return asset.AssetId;
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new RedeemHandoffRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<HandoffSessionResponse>())!;
        return session.AccessToken;
    }

    private async Task<string> CreateHandoffAsync()
    {
        var token = await LoginNexArrAsync(PlatformSeeder.DemoAdminEmail);
        var request = Authorized(HttpMethod.Post, "/api/v1/launch/handoff", token);
        request.Content = JsonContent.Create(new NexArr.Api.Contracts.CreateHandoffRequest(
            "maintainarr",
            "http://localhost:5178/launch"));
        var response = await _nexarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var handoff = (await response.Content.ReadFromJsonAsync<NexArr.Api.Contracts.HandoffCreatedResponse>())!;
        return handoff.HandoffCode;
    }

    private async Task<string> IssueServiceTokenAsync(string adminToken, string productKey)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.RegisterServiceClientRequest(
            $"{productKey}-meter-test",
            $"{productKey} Meter Test",
            productKey,
            [productKey]));
        var registerResponse = await _nexarrClient.SendAsync(registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var client = (await registerResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceClientResponse>())!;

        var issueRequest = Authorized(HttpMethod.Post, "/api/service-tokens", adminToken);
        issueRequest.Content = JsonContent.Create(new NexArr.Api.Contracts.IssueServiceTokenRequest(
            client.ServiceClientId,
            PlatformSeeder.DemoTenantId,
            null,
            "launch.redeem",
            30));
        var issueResponse = await _nexarrClient.SendAsync(issueRequest);
        issueResponse.EnsureSuccessStatusCode();
        var issued = (await issueResponse.Content.ReadFromJsonAsync<NexArr.Api.Contracts.ServiceTokenIssueResponse>())!;
        return issued.AccessToken;
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

    private async Task SeedNexArrAsync()
    {
        using var scope = _nexarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await PlatformSeeder.SeedAsync(db, hasher);
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
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
