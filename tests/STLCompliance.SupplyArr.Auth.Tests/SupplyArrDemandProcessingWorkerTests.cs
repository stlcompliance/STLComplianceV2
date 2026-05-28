using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using SupplyArr.Api.Contracts;
using SupplyArr.Api.Data;
using SupplyArr.Api.Entities;
using SupplyArr.Api.Options;
using SupplyArr.Api.Services;

namespace STLCompliance.SupplyArr.Auth.Tests;

public sealed class SupplyArrDemandProcessingWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::SupplyArr.Api.Program> _supplyarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _supplyarrClient = null!;
    private string _sharedWorkerToSupplyArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"DemandProcessingNexArr-{Guid.NewGuid():N}";
        var supplyArrDbName = $"DemandProcessingSupplyArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToSupplyArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["supplyarr"],
            DemandProcessingWorkerService.ProcessDemandProcessingActionScope);

        _supplyarrFactory = new WebApplicationFactory<global::SupplyArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("MaintainArr:BaseUrl", "http://localhost:5999");
            builder.UseSetting("MaintainArr:ServiceToken", "test-maintainarr-token");
            builder.UseSetting("RoutArr:BaseUrl", "http://localhost:5998");
            builder.UseSetting("RoutArr:ServiceToken", "test-routarr-token");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<SupplyArrDbContext>(services);
                services.AddDbContext<SupplyArrDbContext>(options => options.UseInMemoryDatabase(supplyArrDbName));
                services.RemoveAll<MaintainArrDemandStatusClient>();
                services.AddSingleton(_ =>
                    new MaintainArrDemandStatusClient(
                        new HttpClient(new SuccessHttpMessageHandler())
                        {
                            BaseAddress = new Uri("http://localhost:5999/"),
                        },
                        Microsoft.Extensions.Options.Options.Create(new MaintainArrClientOptions
                        {
                            BaseUrl = "http://localhost:5999",
                            ServiceToken = "test-maintainarr-token",
                        })));
                services.RemoveAll<RoutArrDemandStatusClient>();
                services.AddSingleton(_ =>
                    new RoutArrDemandStatusClient(
                        new HttpClient(new SuccessHttpMessageHandler())
                        {
                            BaseAddress = new Uri("http://localhost:5998/"),
                        },
                        Microsoft.Extensions.Options.Options.Create(new RoutArrClientOptions
                        {
                            BaseUrl = "http://localhost:5998",
                            ServiceToken = "test-routarr-token",
                        })));
            });
        });

        _supplyarrClient = _supplyarrFactory.CreateClient();
        _supplyarrClient.BaseAddress = new Uri("http://localhost");
    }

    public async Task DisposeAsync()
    {
        _supplyarrClient.Dispose();
        _nexarrClient.Dispose();
        await _supplyarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _supplyarrClient.PostAsJsonAsync(
            "/api/internal/demand-processing/process-batch",
            new ProcessDemandProcessingRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 25, 4));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_batch_evaluates_stock_short_without_auto_pr()
    {
        var demandRefId = await SeedDemandRefWithShortStockAsync();
        await UpsertSettingsAsync(autoCreatePrDraftWhenShort: false);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/demand-processing/process-batch",
            _sharedWorkerToSupplyArrToken);
        processRequest.Content = JsonContent.Create(new ProcessDemandProcessingRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25,
            4));

        var processResponse = await _supplyarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessDemandProcessingResponse>())!;
        Assert.Equal(1, body.CandidatesFound);
        Assert.Equal(1, body.ProcessedCount);
        Assert.Single(body.Processed);
        Assert.Equal(DemandProcessingOutcomes.StockShort, body.Processed[0].ProcessingOutcome);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var state = await db.DemandProcessingStates.SingleAsync(x => x.DemandRefId == demandRefId);
        Assert.Equal(1, state.LinesShortCount);
    }

    [Fact]
    public async Task Process_batch_auto_creates_pr_when_stock_is_short()
    {
        var demandRefId = await SeedDemandRefWithShortStockAsync();
        await UpsertNotificationSettingsAsync();

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var settingsEntity = await db.TenantDemandProcessingSettings
            .FirstOrDefaultAsync(x => x.TenantId == PlatformSeeder.DemoTenantId);
        if (settingsEntity is null)
        {
            settingsEntity = new TenantDemandProcessingSettings
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                CreatedAt = now,
            };
            db.TenantDemandProcessingSettings.Add(settingsEntity);
        }

        settingsEntity.IsEnabled = true;
        settingsEntity.AutoCreatePrDraftWhenShort = true;
        settingsEntity.ProcessMaintainarrDemandRefs = true;
        settingsEntity.MinHoursBeforeProcessing = 0;
        settingsEntity.StalenessHours = 4;
        settingsEntity.NotifyOnPrDraftCreated = true;
        settingsEntity.UpdatedAt = now;
        await db.SaveChangesAsync();
        Assert.True(settingsEntity.AutoCreatePrDraftWhenShort);

        var worker = scope.ServiceProvider.GetRequiredService<DemandProcessingWorkerService>();
        var result = await worker.ProcessBatchAsync(new ProcessDemandProcessingRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25,
            4));

        Assert.Equal(1, result.CandidatesFound);
        if (result.SkippedCount > 0)
        {
            Assert.Fail($"Processing skipped: {result.Skipped[0].Reason}");
        }

        Assert.Equal(1, result.ProcessedCount);
        Assert.Equal(1, result.PrDraftsCreatedCount);
        Assert.Equal(DemandProcessingOutcomes.PrDrafted, result.Processed[0].ProcessingOutcome);
        Assert.NotNull(result.Processed[0].PurchaseRequestId);

        var state = await db.DemandProcessingStates.SingleAsync(x => x.DemandRefId == demandRefId);
        Assert.Equal(DemandProcessingOutcomes.PrDrafted, state.ProcessingOutcome);
        Assert.Equal(1, state.LinesShortCount);
        Assert.NotNull(state.PurchaseRequestId);

        var demandRef = await db.MaintainArrDemandRefs.SingleAsync(x => x.Id == demandRefId);
        Assert.Equal(MaintainArrDemandRefStatuses.PrDrafted, demandRef.Status);
        Assert.NotNull(demandRef.PurchaseRequestId);
    }

    [Fact]
    public async Task Process_batch_routarr_auto_creates_pr_when_source_enabled()
    {
        var demandRefId = await SeedRoutarrDemandRefWithShortStockAsync();
        await UpsertSettingsAsync(
            autoCreatePrDraftWhenShort: true,
            processMaintainarrDemandRefs: false,
            processRoutarrDemandRefs: true);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var worker = scope.ServiceProvider.GetRequiredService<DemandProcessingWorkerService>();
        var result = await worker.ProcessBatchAsync(new ProcessDemandProcessingRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            25,
            4));

        Assert.Equal(1, result.CandidatesFound);
        if (result.SkippedCount > 0)
        {
            Assert.Fail($"Processing skipped: {result.Skipped[0].Reason}");
        }

        Assert.Equal(1, result.ProcessedCount);
        Assert.Equal(DemandRefSources.RoutArr, result.Processed[0].DemandRefSource);
        Assert.Equal(DemandProcessingOutcomes.PrDrafted, result.Processed[0].ProcessingOutcome);
        Assert.NotNull(result.Processed[0].PurchaseRequestId);

        var state = await db.DemandProcessingStates.SingleAsync(x => x.DemandRefId == demandRefId);
        Assert.Equal(DemandRefSources.RoutArr, state.DemandRefSource);
        Assert.Equal(DemandProcessingOutcomes.PrDrafted, state.ProcessingOutcome);

        var demandRef = await db.RoutArrDemandRefs.SingleAsync(x => x.Id == demandRefId);
        Assert.Equal(RoutArrDemandRefStatuses.PrDrafted, demandRef.Status);
        Assert.NotNull(demandRef.PurchaseRequestId);
    }

    [Fact]
    public async Task Pending_preview_lists_due_demand_refs()
    {
        var demandRefId = await SeedDemandRefWithShortStockAsync();
        await UpsertSettingsAsync(autoCreatePrDraftWhenShort: false);

        var pendingRequest = Authorized(
            HttpMethod.Get,
            $"/api/internal/demand-processing/pending?tenantId={PlatformSeeder.DemoTenantId}&batchSize=25",
            _sharedWorkerToSupplyArrToken);

        var pendingResponse = await _supplyarrClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingDemandProcessingResponse>())!;
        Assert.Contains(pending.Items, x => x.DemandRefId == demandRefId);
    }

    [Fact]
    public async Task Demand_processing_dashboard_requires_purchase_read()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/demand-processing", buyerToken));
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Demand_processing_settings_requires_admin()
    {
        var buyerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_buyer");
        var response = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/demand-processing-settings", buyerToken));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Settings_upsert_rejects_enabled_worker_without_any_source()
    {
        var adminToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_admin");
        var request = Authorized(HttpMethod.Put, "/api/demand-processing-settings", adminToken);
        request.Content = JsonContent.Create(new UpsertDemandProcessingSettingsRequest(
            true,
            false,
            0,
            4,
            true,
            false,
            false,
            false,
            false));
        var response = await _supplyarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Operator_retry_processing_updates_materialized_state()
    {
        var demandRefId = await SeedDemandRefWithShortStockAsync();
        await UpsertSettingsAsync(autoCreatePrDraftWhenShort: false);
        var managerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_manager");

        var retryResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/demand-processing/{demandRefId}/retry-processing", managerToken));
        retryResponse.EnsureSuccessStatusCode();
        var action = (await retryResponse.Content.ReadFromJsonAsync<DemandProcessingOperatorActionResponse>())!;
        Assert.Equal("retry_processing", action.Action);
        Assert.Equal(DemandProcessingOutcomes.StockShort, action.Result.ProcessingOutcome);

        var dashboardResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/demand-processing", managerToken));
        dashboardResponse.EnsureSuccessStatusCode();
        var dashboard = (await dashboardResponse.Content.ReadFromJsonAsync<DemandProcessingDashboardResponse>())!;
        Assert.Contains(dashboard.ProcessedItems, x => x.DemandRefId == demandRefId);
    }

    [Fact]
    public async Task Operator_create_pr_draft_links_purchase_request()
    {
        var demandRefId = await SeedDemandRefWithShortStockAsync();
        await UpsertSettingsAsync(autoCreatePrDraftWhenShort: false);
        var managerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_manager");

        var createResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Post, $"/api/demand-processing/{demandRefId}/create-pr-draft", managerToken));
        createResponse.EnsureSuccessStatusCode();
        var action = (await createResponse.Content.ReadFromJsonAsync<DemandProcessingOperatorActionResponse>())!;
        Assert.Equal("create_pr_draft", action.Action);
        Assert.NotNull(action.Detail.Summary.PurchaseRequestId);

        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var demandRef = await db.MaintainArrDemandRefs.SingleAsync(x => x.Id == demandRefId);
        Assert.NotNull(demandRef.PurchaseRequestId);
    }

    [Fact]
    public async Task Dashboard_returns_pending_and_processed_queues()
    {
        var demandRefId = await SeedDemandRefWithShortStockAsync();
        await UpsertSettingsAsync(autoCreatePrDraftWhenShort: false);
        var managerToken = CreateSupplyArrAccessToken(["supplyarr"], "supplyarr_manager");

        var dashboardResponse = await _supplyarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/demand-processing", managerToken));
        dashboardResponse.EnsureSuccessStatusCode();
        var dashboard = (await dashboardResponse.Content.ReadFromJsonAsync<DemandProcessingDashboardResponse>())!;
        Assert.Contains(dashboard.PendingItems, x => x.DemandRefId == demandRefId);
    }

    private async Task<Guid> SeedDemandRefWithShortStockAsync()
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var location = new InventoryLocation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LocationKey = $"loc-{Guid.NewGuid():N}"[..16],
            Name = "Main warehouse",
            LocationType = "warehouse",
            AddressLine = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var bin = new InventoryBin
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InventoryLocationId = location.Id,
            BinKey = $"bin-{Guid.NewGuid():N}"[..16],
            Name = "A1",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var part = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = $"part-{Guid.NewGuid():N}"[..16],
            DisplayName = "Demand processing part",
            Description = string.Empty,
            CategoryKey = "general",
            UnitOfMeasure = "each",
            ManufacturerName = string.Empty,
            ManufacturerPartNumber = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var stock = new PartStockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            InventoryBinId = bin.Id,
            QuantityOnHand = 1m,
            QuantityReserved = 0m,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var demandRefId = Guid.NewGuid();
        var demandRef = new MaintainArrDemandRef
        {
            Id = demandRefId,
            TenantId = tenantId,
            MaintainarrPublicationId = Guid.NewGuid(),
            MaintainarrWorkOrderId = Guid.NewGuid(),
            MaintainarrWorkOrderNumber = "WO-DP-100",
            MaintainarrAssetId = Guid.NewGuid(),
            Title = "Brake pads demand",
            Notes = string.Empty,
            Status = MaintainArrDemandRefStatuses.Received,
            ProcurementStatus = MaintainArrDemandRefProcurementStatuses.Received,
            ReceivedAt = now.AddHours(-2),
            CreatedAt = now.AddHours(-2),
            UpdatedAt = now.AddHours(-2),
        };

        demandRef.Lines.Add(new MaintainArrDemandRefLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DemandRefId = demandRefId,
            LineNumber = 1,
            MaintainarrDemandLineId = Guid.NewGuid(),
            PartId = part.Id,
            PartNumber = part.PartKey,
            Description = part.DisplayName,
            QuantityRequested = 5m,
            UnitOfMeasure = "each",
            Notes = string.Empty,
        });

        db.InventoryLocations.Add(location);
        db.InventoryBins.Add(bin);
        db.Parts.Add(part);
        db.PartStockLevels.Add(stock);
        db.MaintainArrDemandRefs.Add(demandRef);
        await db.SaveChangesAsync();

        var savedLineCount = await db.MaintainArrDemandRefLines.CountAsync(x => x.DemandRefId == demandRefId);
        if (savedLineCount != 1)
        {
            throw new InvalidOperationException($"Expected 1 demand line, found {savedLineCount}.");
        }

        return demandRefId;
    }

    private async Task UpsertSettingsAsync(
        bool autoCreatePrDraftWhenShort,
        bool processMaintainarrDemandRefs = true,
        bool processRoutarrDemandRefs = false,
        bool processTrainarrDemandRefs = false,
        bool processStaffarrDemandRefs = false)
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        var entity = await db.TenantDemandProcessingSettings
            .FirstOrDefaultAsync(x => x.TenantId == PlatformSeeder.DemoTenantId);
        if (entity is null)
        {
            entity = new TenantDemandProcessingSettings
            {
                Id = Guid.NewGuid(),
                TenantId = PlatformSeeder.DemoTenantId,
                CreatedAt = now,
            };
            db.TenantDemandProcessingSettings.Add(entity);
        }

        entity.IsEnabled = true;
        entity.AutoCreatePrDraftWhenShort = autoCreatePrDraftWhenShort;
        entity.MinHoursBeforeProcessing = 0;
        entity.StalenessHours = 4;
        entity.NotifyOnPrDraftCreated = true;
        entity.ProcessMaintainarrDemandRefs = processMaintainarrDemandRefs;
        entity.ProcessRoutarrDemandRefs = processRoutarrDemandRefs;
        entity.ProcessTrainarrDemandRefs = processTrainarrDemandRefs;
        entity.ProcessStaffarrDemandRefs = processStaffarrDemandRefs;
        entity.UpdatedAt = now;
        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedRoutarrDemandRefWithShortStockAsync()
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var tenantId = PlatformSeeder.DemoTenantId;
        var now = DateTimeOffset.UtcNow;

        var location = new InventoryLocation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LocationKey = $"loc-{Guid.NewGuid():N}"[..16],
            Name = "Main warehouse",
            LocationType = "warehouse",
            AddressLine = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var bin = new InventoryBin
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InventoryLocationId = location.Id,
            BinKey = $"bin-{Guid.NewGuid():N}"[..16],
            Name = "A1",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var part = new Part
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartKey = $"part-{Guid.NewGuid():N}"[..16],
            DisplayName = "RoutArr demand part",
            Description = string.Empty,
            CategoryKey = "general",
            UnitOfMeasure = "each",
            ManufacturerName = string.Empty,
            ManufacturerPartNumber = string.Empty,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now,
        };

        var stock = new PartStockLevel
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartId = part.Id,
            InventoryBinId = bin.Id,
            QuantityOnHand = 0m,
            QuantityReserved = 0m,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var demandRefId = Guid.NewGuid();
        var demandRef = new RoutArrDemandRef
        {
            Id = demandRefId,
            TenantId = tenantId,
            RoutarrPublicationId = Guid.NewGuid(),
            RoutarrTripId = Guid.NewGuid(),
            RoutarrTripNumber = "TRIP-DP-200",
            RoutarrVehicleRefKey = "VEH-1",
            Title = "Trip parts demand",
            Notes = string.Empty,
            Status = RoutArrDemandRefStatuses.Received,
            ProcurementStatus = RoutArrDemandRefProcurementStatuses.Received,
            ReceivedAt = now.AddHours(-2),
            CreatedAt = now.AddHours(-2),
            UpdatedAt = now.AddHours(-2),
        };

        demandRef.Lines.Add(new RoutArrDemandRefLine
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DemandRefId = demandRefId,
            LineNumber = 1,
            RoutarrDemandLineId = Guid.NewGuid(),
            PartId = part.Id,
            PartNumber = part.PartKey,
            Description = part.DisplayName,
            QuantityRequested = 3m,
            UnitOfMeasure = "each",
            Notes = string.Empty,
        });

        db.InventoryLocations.Add(location);
        db.InventoryBins.Add(bin);
        db.Parts.Add(part);
        db.PartStockLevels.Add(stock);
        db.RoutArrDemandRefs.Add(demandRef);
        await db.SaveChangesAsync();

        return demandRefId;
    }

    private async Task UpsertNotificationSettingsAsync()
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupplyArrDbContext>();
        var now = DateTimeOffset.UtcNow;
        db.TenantProcurementNotificationSettings.Add(new TenantProcurementNotificationSettings
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            IsEnabled = true,
            NotificationWebhookUrl = "https://example.test/supplyarr-webhook",
            CreatedAt = now,
            UpdatedAt = now,
        });
        await db.SaveChangesAsync();
    }

    private string CreateSupplyArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member")
    {
        using var scope = _supplyarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<SupplyArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return accessToken;
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
            new LoginRequest(email, PlatformSeeder.DemoAdminPassword, PlatformSeeder.DemoTenantId));
        response.EnsureSuccessStatusCode();
        var payload = (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
        return payload.AccessToken;
    }

    private async Task<string> IssueServiceTokenAsync(
        string adminToken,
        string sourceProduct,
        string[] allowedProducts,
        string actionScope)
    {
        var registerRequest = Authorized(HttpMethod.Post, "/api/service-tokens/clients", adminToken);
        registerRequest.Content = JsonContent.Create(new RegisterServiceClientRequest(
            $"{sourceProduct}-demand-processing-test",
            $"{sourceProduct} demand processing test",
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

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private sealed class SuccessHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
