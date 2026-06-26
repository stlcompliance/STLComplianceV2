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
using MaintainArrRedeemRequest = MaintainArr.Api.Contracts.RedeemHandoffRequest;
using MaintainArrHandoffSessionResponse = MaintainArr.Api.Contracts.HandoffSessionResponse;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrExecutiveReportTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _managerToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"ExecReportNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"ExecReportMaintainArr-{Guid.NewGuid():N}";

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
        _managerToken = CreateMaintainArrAccessToken(["maintainarr"], "tenant_admin");
    }

    public async Task DisposeAsync()
    {
        _maintainarrClient.Dispose();
        _nexarrClient.Dispose();
        await _maintainarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Executive_report_summary_returns_kpis()
    {
        await SeedReliabilityDowntimeAsync();
        await SeedPartsDemandForecastAsync();

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/executive/summary", _managerToken));
        response.EnsureSuccessStatusCode();

        var summary = (await response.Content.ReadFromJsonAsync<ExecutiveReportSummaryResponse>())!;
        Assert.Equal("supplyarr", summary.SupplyDemand.SourceProduct);
        Assert.True(summary.FleetReadiness.TotalAssets >= 0);
        Assert.NotNull(summary.OperationalTotals);
        Assert.True(summary.DowntimeTrend.PeriodDays >= 1);
        Assert.NotNull(summary.DowntimeTrend.CurrentPeriod);
        Assert.NotNull(summary.DowntimeTrend.PreviousPeriod);
        Assert.Equal(3, summary.Reliability.FailureEventCount);
        Assert.Equal(3, summary.Reliability.ClosedRepairEventCount);
        Assert.Equal(1, summary.Reliability.RepeatDowntimeAssetCount);
        Assert.Equal(1, summary.Reliability.ChronicAssetCount);
        Assert.True(summary.Reliability.MeanTimeToRepairHours > 0);
        Assert.True(summary.Reliability.MeanTimeBetweenFailuresHours > 0);
        Assert.Contains(summary.Reliability.ChronicAssets, x => x.AssetTag == "REL-001");
        Assert.Equal(3, summary.PartsDemandForecast.OpenLineCount);
        Assert.Equal(2, summary.PartsDemandForecast.DistinctPartCount);
        Assert.True(summary.PartsDemandForecast.ForecastQuantity > 0);
        Assert.Contains(summary.PartsDemandForecast.TopParts, x => x.PartNumber == "BRK-001");
        Assert.Equal(2, summary.PartsDemandForecast.TopParts.Single(x => x.PartNumber == "BRK-001").OpenLineCount);
    }

    [Fact]
    public async Task Dashboard_v1_returns_fleet_command_center_metrics_and_actions()
    {
        await SeedReliabilityDowntimeAsync();

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/dashboard", _managerToken));
        response.EnsureSuccessStatusCode();

        var dashboard = (await response.Content.ReadFromJsonAsync<MaintainArrDashboardResponse>())!;
        Assert.Equal(2, dashboard.Readiness.TotalAssets);
        Assert.Equal(1, dashboard.Readiness.NotReadyAssets);
        Assert.Equal(1, dashboard.Operations.OpenWorkOrders);
        Assert.Equal(1, dashboard.Operations.CriticalDefects);
        Assert.Equal(1, dashboard.Operations.OverduePm);
        Assert.Equal(1, dashboard.Downtime.ChronicAssetCount);
        Assert.Contains(dashboard.Downtime.TopProblemAssets, asset => asset.AssetTag == "REL-001");
        Assert.Contains(dashboard.ActionItems, item => item.Key == "critical_defects");
        Assert.Contains(dashboard.ActionItems, item => item.Key == "overdue_pm");
        Assert.Contains(dashboard.ActionItems, item => item.Key == "chronic_assets");
    }

    [Fact]
    public async Task Executive_report_summary_export_returns_csv()
    {
        await SeedReliabilityDowntimeAsync();
        await SeedPartsDemandForecastAsync();

        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/reports/executive/summary/export", _managerToken));
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var csv = await response.Content.ReadAsStringAsync();
        Assert.Contains("fleet,total_assets", csv, StringComparison.Ordinal);
        Assert.Contains("downtime,current_hours", csv, StringComparison.Ordinal);
        Assert.Contains("reliability,mean_time_to_repair_hours", csv, StringComparison.Ordinal);
        Assert.Contains("supplyarr,published_demand_lines", csv, StringComparison.Ordinal);
        Assert.Contains("parts_demand_forecast,forecast_quantity", csv, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Executive_report_v1_aliases_work()
    {
        var summaryResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/executive/summary", _managerToken));
        summaryResponse.EnsureSuccessStatusCode();

        var exportResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports/executive/summary/export", _managerToken));
        exportResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Reports_index_v1_alias_lists_report_groups()
    {
        var response = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/reports", _managerToken));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("maintenance", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("executive", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("compliance", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dashboard", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Executive_report_summary_denies_unauthenticated()
    {
        var response = await _maintainarrClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, "/api/reports/executive/summary"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<string> RedeemMaintainArrTokenAsync()
    {
        var handoffCode = await CreateHandoffAsync();
        var redeemResponse = await _maintainarrClient.PostAsJsonAsync(
            "/api/auth/handoff/redeem",
            new MaintainArrRedeemRequest(handoffCode));
        redeemResponse.EnsureSuccessStatusCode();
        var session = (await redeemResponse.Content.ReadFromJsonAsync<MaintainArrHandoffSessionResponse>())!;
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
            $"{productKey}-exec-report-test",
            $"{productKey} exec report test",
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

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin",
        Guid? userIdOverride = null)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var userId = userIdOverride ?? PlatformSeeder.DemoAdminUserId;
        var (accessToken, _) = tokenService.CreateAccessToken(
            userId,
            userId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
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

    private async Task SeedReliabilityDowntimeAsync()
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        var assetClass = new AssetClass
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ClassKey = $"reliability-class-{Guid.NewGuid():N}",
            Name = "Reliability Class",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var assetType = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetClassId = assetClass.Id,
            TypeKey = $"reliability-type-{Guid.NewGuid():N}",
            Name = "Reliability Type",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetTypeId = assetType.Id,
            AssetTag = "REL-001",
            Name = "Reliability Report Asset",
            LifecycleStatus = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.AssetClasses.Add(assetClass);
        db.AssetTypes.Add(assetType);
        db.Assets.Add(asset);

        db.AssetDowntimeEvents.AddRange(
            ClosedUnplannedDowntime(asset, now.AddDays(-20), hours: 4),
            ClosedUnplannedDowntime(asset, now.AddDays(-12), hours: 6),
            ClosedUnplannedDowntime(asset, now.AddDays(-2), hours: 8));

        db.AssetStatusRollups.Add(new AssetStatusRollup
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetId = asset.Id,
            AssetTag = asset.AssetTag,
            AssetName = asset.Name,
            LifecycleStatus = "active",
            ReadinessStatus = "not_ready",
            ReadinessBasis = "maintenance_blockers",
            BlockerCount = 3,
            PrimaryBlockerMessage = "Critical defect, overdue PM, and failed inspection.",
            OpenCriticalDefectCount = 1,
            OpenHighDefectCount = 0,
            ActiveWorkOrderCount = 1,
            PmDueCount = 1,
            PmOverdueCount = 1,
            FailedInspectionCount = 1,
            ComputedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        });

        db.AssetStatusScopeRollups.Add(new AssetStatusScopeRollup
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ScopeType = AssetStatusRollupScopeTypes.Fleet,
            ScopeEntityId = PlatformSeeder.DemoTenantId,
            ScopeLabel = "Fleet",
            TotalAssets = 2,
            ReadyCount = 1,
            NotReadyCount = 1,
            ReadyPercent = 50m,
            ComputedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        });

        await db.SaveChangesAsync();
    }

    private async Task SeedPartsDemandForecastAsync()
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        var assetClass = new AssetClass
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ClassKey = $"forecast-class-{Guid.NewGuid():N}",
            Name = "Forecast Class",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var assetType = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetClassId = assetClass.Id,
            TypeKey = $"forecast-type-{Guid.NewGuid():N}",
            Name = "Forecast Type",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetTypeId = assetType.Id,
            AssetTag = "FORECAST-001",
            Name = "Forecast Asset",
            LifecycleStatus = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var manualWorkOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetId = asset.Id,
            WorkOrderNumber = "WO-FORECAST-1",
            Title = "Replace brake pads",
            Description = "Forecast manual maintenance work order",
            Priority = WorkOrderPriorities.High,
            Status = WorkOrderStatuses.Open,
            Source = WorkOrderSources.Manual,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now.AddDays(-2),
            UpdatedAt = now,
        };
        var pmWorkOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetId = asset.Id,
            PmScheduleId = Guid.NewGuid(),
            WorkOrderNumber = "WO-FORECAST-2",
            Title = "PM inspection",
            Description = "Forecast PM maintenance work order",
            Priority = WorkOrderPriorities.Medium,
            Status = WorkOrderStatuses.InProgress,
            Source = WorkOrderSources.PmSchedule,
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now,
        };

        db.AssetClasses.Add(assetClass);
        db.AssetTypes.Add(assetType);
        db.Assets.Add(asset);
        db.WorkOrders.AddRange(manualWorkOrder, pmWorkOrder);
        var brakePartId = Guid.NewGuid();
        var oilPartId = Guid.NewGuid();
        db.WorkOrderPartsDemandLines.AddRange(
            new WorkOrderPartsDemandLine
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                WorkOrderId = manualWorkOrder.Id,
                LineNumber = 1,
                SupplyarrPartId = brakePartId,
                PartNumber = "BRK-001",
                Description = "Brake pads",
                QuantityRequested = 2m,
                UnitOfMeasure = "each",
                Notes = "Front axle",
                Status = WorkOrderPartsDemandStatuses.Pending,
                ProcurementStatus = WorkOrderPartsDemandProcurementStatuses.AwaitingProcurement,
                QuantityReceived = 0m,
                CreatedByUserId = PlatformSeeder.DemoAdminUserId,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-2),
            },
            new WorkOrderPartsDemandLine
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                WorkOrderId = pmWorkOrder.Id,
                LineNumber = 1,
                SupplyarrPartId = brakePartId,
                PartNumber = "BRK-001",
                Description = "Brake pads",
                QuantityRequested = 1m,
                UnitOfMeasure = "each",
                Notes = "Rear axle",
                Status = WorkOrderPartsDemandStatuses.Published,
                MaintainarrPublicationId = Guid.NewGuid(),
                SupplyarrDemandRefId = Guid.NewGuid(),
                PublishedAt = now.AddDays(-1),
                ProcurementStatus = WorkOrderPartsDemandProcurementStatuses.PrSubmitted,
                QuantityReceived = 0m,
                CreatedByUserId = PlatformSeeder.DemoAdminUserId,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now,
            },
            new WorkOrderPartsDemandLine
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                WorkOrderId = pmWorkOrder.Id,
                LineNumber = 2,
                SupplyarrPartId = oilPartId,
                PartNumber = "OIL-123",
                Description = "Hydraulic oil",
                QuantityRequested = 4m,
                UnitOfMeasure = "liter",
                Notes = "Top-off",
                Status = WorkOrderPartsDemandStatuses.Pending,
                ProcurementStatus = WorkOrderPartsDemandProcurementStatuses.AwaitingProcurement,
                QuantityReceived = 1m,
                CreatedByUserId = PlatformSeeder.DemoAdminUserId,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now,
            });

        await db.SaveChangesAsync();
    }

    private static AssetDowntimeEvent ClosedUnplannedDowntime(
        Asset asset,
        DateTimeOffset startedAt,
        int hours) =>
        new()
        {
            Id = Guid.NewGuid(),
            TenantId = asset.TenantId,
            AssetId = asset.Id,
            AssetTag = asset.AssetTag,
            AssetName = asset.Name,
            Source = AssetDowntimeSources.Manual,
            Reason = AssetDowntimeReasons.InRepair,
            IsPlanned = false,
            StartedAt = startedAt,
            EndedAt = startedAt.AddHours(hours),
            CreatedByUserId = PlatformSeeder.DemoAdminUserId,
            ClosedByUserId = PlatformSeeder.DemoAdminUserId,
            CreatedAt = startedAt,
            UpdatedAt = startedAt.AddHours(hours),
        };

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        foreach (var descriptor in services
                     .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
                     .ToList())
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
