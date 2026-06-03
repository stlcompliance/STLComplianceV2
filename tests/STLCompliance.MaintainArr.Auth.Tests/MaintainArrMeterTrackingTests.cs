using STLCompliance.Shared.Integration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
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
    public async Task List_and_get_meters_translate_on_relational_provider()
    {
        await using var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<MaintainArrDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new MaintainArrDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var tenantId = Guid.NewGuid();
        var assetClassId = Guid.NewGuid();
        var assetTypeId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var meterId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        db.AssetClasses.Add(new AssetClass
        {
            Id = assetClassId,
            TenantId = tenantId,
            ClassKey = "vehicles",
            Name = "Vehicles",
            Description = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });
        db.AssetTypes.Add(new AssetType
        {
            Id = assetTypeId,
            TenantId = tenantId,
            AssetClassId = assetClassId,
            TypeKey = "forklift",
            Name = "Forklift",
            Description = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        });
        db.Assets.Add(new Asset
        {
            Id = assetId,
            TenantId = tenantId,
            AssetTypeId = assetTypeId,
            AssetTag = "MTR-001",
            Name = "Meter Test Asset",
            Description = string.Empty,
            LifecycleStatus = "active",
            SiteRef = null,
            CreatedAt = now,
            UpdatedAt = now
        });
        db.AssetMeters.Add(new AssetMeter
        {
            Id = meterId,
            TenantId = tenantId,
            AssetId = assetId,
            MeterKey = "engine-hours",
            Name = "Engine hours",
            Description = string.Empty,
            Unit = "hours",
            BaselineReading = 1000m,
            CurrentReading = 1000m,
            Status = MeterStatuses.Active,
            CreatedAt = now,
            UpdatedAt = now
        });
        await db.SaveChangesAsync();

        var assetService = new AssetService(db, null!, null!, null!, null!, null!);
        var service = new AssetMeterService(db, assetService, null!);

        var meters = await service.ListForAssetAsync(tenantId, assetId);
        var meter = await service.GetAsync(tenantId, meterId);

        Assert.Single(meters);
        Assert.Equal(meterId, meters[0].AssetMeterId);
        Assert.Equal(assetId, meter.AssetId);
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
        Assert.StartsWith($"/api/v1/meters/", createMeterResponse.Headers.Location?.ToString());
        var meter = (await createMeterResponse.Content.ReadFromJsonAsync<AssetMeterResponse>())!;

        var recordRequest = Authorized(HttpMethod.Post, $"/api/v1/meters/{meter.AssetMeterId}/readings", token);
        recordRequest.Content = JsonContent.Create(new RecordMeterReadingRequest(2050m, null, "v1 monthly", false));
        var recordResponse = await _maintainarrClient.SendAsync(recordRequest);
        recordResponse.EnsureSuccessStatusCode();
        Assert.StartsWith($"/api/v1/meters/{meter.AssetMeterId}/readings/", recordResponse.Headers.Location?.ToString());

        var listRequest = Authorized(HttpMethod.Get, $"/api/v1/meters/{meter.AssetMeterId}/readings", token);
        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var readings = (await listResponse.Content.ReadFromJsonAsync<List<MeterReadingResponse>>())!;
        Assert.Single(readings);

        var listByAssetRequest = Authorized(HttpMethod.Get, $"/api/v1/meters?assetId={assetId:D}", token);
        var listByAssetResponse = await _maintainarrClient.SendAsync(listByAssetRequest);
        listByAssetResponse.EnsureSuccessStatusCode();
        var meters = (await listByAssetResponse.Content.ReadFromJsonAsync<List<AssetMeterResponse>>())!;
        Assert.Contains(meters, x => x.AssetMeterId == meter.AssetMeterId);
    }

    [Fact]
    public async Task Missing_reading_alerts_include_active_meter_without_readings_v1()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetAsync(token);
        var meter = await CreateMeterAsync(token, assetId, 750m);

        var alertsRequest = Authorized(HttpMethod.Get, "/api/v1/meters/alerts?staleAfterDays=0", token);
        var alertsResponse = await _maintainarrClient.SendAsync(alertsRequest);
        alertsResponse.EnsureSuccessStatusCode();
        var alerts = (await alertsResponse.Content.ReadFromJsonAsync<List<MeterMissingReadingAlertResponse>>())!;

        Assert.Contains(alerts, x => x.AssetMeterId == meter.AssetMeterId);
    }

    [Fact]
    public async Task Correction_workflow_records_audited_v1_meter_correction()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetAsync(token);
        var meter = await CreateMeterAsync(token, assetId, 500m);

        var correctionRequest = Authorized(HttpMethod.Post, $"/api/v1/meters/{meter.AssetMeterId}/readings/corrections", token);
        correctionRequest.Content = JsonContent.Create(new CorrectMeterReadingRequest(450m, null, "Odometer rollover correction"));
        var correctionResponse = await _maintainarrClient.SendAsync(correctionRequest);
        correctionResponse.EnsureSuccessStatusCode();
        Assert.StartsWith($"/api/v1/meters/{meter.AssetMeterId}/readings/", correctionResponse.Headers.Location?.ToString());

        var correction = (await correctionResponse.Content.ReadFromJsonAsync<MeterReadingResponse>())!;
        Assert.True(correction.IsCorrection);
        Assert.Equal(0m, correction.DeltaFromPrevious);
        Assert.Equal("Odometer rollover correction", correction.Notes);

        var getMeterRequest = Authorized(HttpMethod.Get, $"/api/v1/meters/{meter.AssetMeterId}", token);
        var getMeterResponse = await _maintainarrClient.SendAsync(getMeterRequest);
        getMeterResponse.EnsureSuccessStatusCode();
        var correctedMeter = (await getMeterResponse.Content.ReadFromJsonAsync<AssetMeterResponse>())!;
        Assert.Equal(450m, correctedMeter.CurrentReading);
        Assert.Equal(450m, correctedMeter.BaselineReading);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        Assert.True(await db.AuditEvents.AnyAsync(x =>
            x.Action == "meter_reading.correction" &&
            x.TargetType == "meter_reading" &&
            x.TargetId == correction.MeterReadingId.ToString()));
    }

    [Fact]
    public async Task Correction_workflow_requires_reason()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetAsync(token);
        var meter = await CreateMeterAsync(token, assetId, 500m);

        var correctionRequest = Authorized(HttpMethod.Post, $"/api/v1/meters/{meter.AssetMeterId}/readings/corrections", token);
        correctionRequest.Content = JsonContent.Create(new CorrectMeterReadingRequest(450m, null, " "));
        var correctionResponse = await _maintainarrClient.SendAsync(correctionRequest);

        Assert.Equal(HttpStatusCode.BadRequest, correctionResponse.StatusCode);
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
    public async Task Meter_pm_forecast_uses_usage_velocity_and_confidence()
    {
        var token = await RedeemMaintainArrTokenAsync();
        var assetId = await SeedAssetAsync(token);

        var meter = await CreateMeterAsync(token, assetId, 1000m);
        _ = await CreateMeterPmScheduleAsync(token, assetId, meter.AssetMeterId, intervalUsage: 200m, nextDueAtUsage: 1200m);

        var now = DateTimeOffset.UtcNow;

        var firstReadingRequest = Authorized(HttpMethod.Post, $"/api/meters/{meter.AssetMeterId}/readings", token);
        firstReadingRequest.Content = JsonContent.Create(new RecordMeterReadingRequest(1000m, now.AddDays(-20), string.Empty, false));
        (await _maintainarrClient.SendAsync(firstReadingRequest)).EnsureSuccessStatusCode();

        var secondReadingRequest = Authorized(HttpMethod.Post, $"/api/meters/{meter.AssetMeterId}/readings", token);
        secondReadingRequest.Content = JsonContent.Create(new RecordMeterReadingRequest(1080m, now, string.Empty, false));
        (await _maintainarrClient.SendAsync(secondReadingRequest)).EnsureSuccessStatusCode();

        var forecastResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/meters/{meter.AssetMeterId}/pm-forecast", token));
        forecastResponse.EnsureSuccessStatusCode();
        var forecast = (await forecastResponse.Content.ReadFromJsonAsync<MeterPmForecastResponse>())!;

        Assert.Equal(4m, forecast.UsageVelocityPerDay);
        Assert.Equal(120m, forecast.PredictedUsageUntilDue);
        Assert.Equal(30m, forecast.PredictedDaysUntilDue);
        Assert.NotNull(forecast.PredictedDueAt);
        Assert.True(forecast.ConfidenceScore >= 70m);
        Assert.True(forecast.IsDueSoon);
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
