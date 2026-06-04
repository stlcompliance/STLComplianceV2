using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using MaintainArr.Api.Services;
using static MaintainArr.Api.Entities.MaintenancePlatformEventRelatedEntityTypes;
using static MaintainArr.Api.Entities.MaintenancePlatformOutboxEventKinds;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class MaintainArrPmDueScanWorkerTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::MaintainArr.Api.Program> _maintainarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _maintainarrClient = null!;
    private string _sharedWorkerToMaintainArrToken = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"PmDueScanNexArr-{Guid.NewGuid():N}";
        var maintainArrDbName = $"PmDueScanMaintainArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToMaintainArrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["maintainarr"],
            PmDueScanService.ProcessDueScanActionScope);

        _maintainarrFactory = new WebApplicationFactory<global::MaintainArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<MaintainArrDbContext>(services);
                services.AddDbContext<MaintainArrDbContext>(options => options.UseInMemoryDatabase(maintainArrDbName));
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
    public async Task Settings_get_returns_defaults_and_pending_count()
    {
        await SeedPastDuePmScheduleAsync(daysPastDue: 0);
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");
        var request = Authorized(HttpMethod.Get, "/api/pm-due-scan-settings", token);
        var response = await _maintainarrClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = (await response.Content.ReadFromJsonAsync<PmDueScanSettingsResponse>())!;
        Assert.False(body.IsEnabled);
        Assert.Equal(15, body.ScanIntervalMinutes);
        Assert.True(body.PendingPmCount >= 1);
        Assert.Null(body.LastRunAt);
    }

    [Fact]
    public async Task Settings_put_trigger_records_run_and_updates_last_run()
    {
        var schedule = await SeedPastDuePmScheduleAsync(daysPastDue: 0);
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");

        var putRequest = Authorized(HttpMethod.Put, "/api/pm-due-scan-settings", token);
        putRequest.Content = JsonContent.Create(
            new UpsertPmDueScanSettingsRequest(true, 15, 50, 1));
        (await _maintainarrClient.SendAsync(putRequest)).EnsureSuccessStatusCode();

        var triggerRequest = Authorized(HttpMethod.Post, "/api/pm-due-scan-settings/trigger", token);
        var triggerResponse = await _maintainarrClient.SendAsync(triggerRequest);
        triggerResponse.EnsureSuccessStatusCode();
        var triggerBody = (await triggerResponse.Content.ReadFromJsonAsync<TriggerPmDueScanResponse>())!;
        Assert.True(triggerBody.Result.CandidatesFound >= 1);

        var getRequest = Authorized(HttpMethod.Get, "/api/pm-due-scan-settings", token);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var settings = (await getResponse.Content.ReadFromJsonAsync<PmDueScanSettingsResponse>())!;
        Assert.NotNull(settings.LastRunAt);

        var runsRequest = Authorized(HttpMethod.Get, "/api/pm-due-scan-settings/runs?limit=5", token);
        var runsResponse = await _maintainarrClient.SendAsync(runsRequest);
        runsResponse.EnsureSuccessStatusCode();
        var runs = (await runsResponse.Content.ReadFromJsonAsync<PmDueScanRunsResponse>())!;
        Assert.NotEmpty(runs.Items);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var updated = await db.PmSchedules.AsNoTracking().FirstAsync(x => x.Id == schedule.Id);
        Assert.Equal(PmDueStatuses.Due, updated.DueStatus);
    }

    [Fact]
    public async Task Pm_schedule_skip_sets_skipped_status_and_enqueues_occurrence_event()
    {
        var schedule = await SeedPastDuePmScheduleAsync(daysPastDue: -5);
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");

        var skipRequest = Authorized(
            HttpMethod.Patch,
            $"/api/preventive-maintenance/schedules/{schedule.Id}/skip",
            token);
        skipRequest.Content = JsonContent.Create(new SkipPmScheduleRequest("Deferred by supervisor"));
        var skipResponse = await _maintainarrClient.SendAsync(skipRequest);
        skipResponse.EnsureSuccessStatusCode();

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();

        var skipped = await db.PmSchedules.SingleAsync(x => x.Id == schedule.Id);
        Assert.Equal(PmDueStatuses.Skipped, skipped.DueStatus);
        Assert.NotNull(skipped.SkippedAt);
        Assert.NotNull(skipped.SkippedByPersonId);
        Assert.Equal("Deferred by supervisor", skipped.SkippedReason);

        var outbox = await db.MaintenancePlatformOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityId == schedule.Id)
            .ToListAsync();
        Assert.Contains(outbox, x =>
            x.EventKind == PmOccurrenceSkipped
            && x.RelatedEntityType == PmOccurrence);
    }

    [Fact]
    public async Task Process_due_scan_auto_generates_pm_inspection_and_history_entry()
    {
        var schedule = await SeedPastDuePmScheduleAsync(daysPastDue: 0);
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");

        var inspectionTemplate = await SeedInspectionProgramAsync(schedule, token);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));

        var processResponse = await _maintainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessPmDueScanResponse>())!;
        Assert.Equal(1, body.WorkOrdersCreatedCount);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();

        var inspectionRun = await db.InspectionRuns.SingleAsync(x => x.PmScheduleId == schedule.Id);
        Assert.Equal(inspectionTemplate, inspectionRun.InspectionTemplateId);
        Assert.Equal(InspectionRunStatuses.InProgress, inspectionRun.Status);

        var outbox = await db.MaintenancePlatformOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.RelatedEntityId == schedule.Id)
            .ToListAsync();
        Assert.Contains(outbox, x =>
            x.EventKind == InspectionStarted
            && x.RelatedEntityType == InspectionRun
            && x.RelatedEntityId == inspectionRun.Id);
        Assert.Contains(outbox, x =>
            x.EventKind == PmOccurrenceInspectionGenerated
            && x.RelatedEntityType == PmOccurrence
            && x.RelatedEntityId == schedule.Id
            && x.PayloadJson.Contains(inspectionRun.Id.ToString("D"), StringComparison.OrdinalIgnoreCase));

        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var historyResponse = await _maintainarrClient.SendAsync(
            Authorized(HttpMethod.Get, $"/api/maintenance-history?assetId={schedule.AssetId}&page=1&pageSize=50", managerToken));
        historyResponse.EnsureSuccessStatusCode();
        var history = (await historyResponse.Content.ReadFromJsonAsync<PagedResult<MaintenanceHistoryEntryResponse>>())!;
        Assert.Contains(history.Items, x =>
            x.Category == "inspection"
            && x.EventType == "pm_occurrence_inspection_generated"
            && x.RelatedEntityId == schedule.Id.ToString("D"));
    }

    [Fact]
    public async Task Settings_put_requires_admin()
    {
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var request = Authorized(HttpMethod.Put, "/api/pm-due-scan-settings", token);
        request.Content = JsonContent.Create(new UpsertPmDueScanSettingsRequest(true, 15, 100, 1));
        var response = await _maintainarrClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Pm_due_scan_settings_v1_put_get_pending_runs_and_trigger_work_for_admin()
    {
        await SeedPastDuePmScheduleAsync(daysPastDue: 0);
        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_admin");

        var putRequest = Authorized(HttpMethod.Put, "/api/v1/pm-due-scan-settings", token);
        putRequest.Content = JsonContent.Create(new UpsertPmDueScanSettingsRequest(true, 20, 40, 1));
        (await _maintainarrClient.SendAsync(putRequest)).EnsureSuccessStatusCode();

        var getRequest = Authorized(HttpMethod.Get, "/api/v1/pm-due-scan-settings", token);
        var getResponse = await _maintainarrClient.SendAsync(getRequest);
        getResponse.EnsureSuccessStatusCode();
        var settings = (await getResponse.Content.ReadFromJsonAsync<PmDueScanSettingsResponse>())!;
        Assert.True(settings.IsEnabled);
        Assert.Equal(20, settings.ScanIntervalMinutes);

        var pendingRequest = Authorized(HttpMethod.Get, "/api/v1/pm-due-scan-settings/pending", token);
        var pendingResponse = await _maintainarrClient.SendAsync(pendingRequest);
        pendingResponse.EnsureSuccessStatusCode();
        var pending = (await pendingResponse.Content.ReadFromJsonAsync<PendingPmDueResponse>())!;
        Assert.NotNull(pending.Items);

        var triggerRequest = Authorized(HttpMethod.Post, "/api/v1/pm-due-scan-settings/trigger", token);
        var triggerResponse = await _maintainarrClient.SendAsync(triggerRequest);
        triggerResponse.EnsureSuccessStatusCode();
        var trigger = (await triggerResponse.Content.ReadFromJsonAsync<TriggerPmDueScanResponse>())!;
        Assert.True(trigger.Result.CandidatesFound >= 1);

        var runsRequest = Authorized(HttpMethod.Get, "/api/v1/pm-due-scan-settings/runs?limit=5", token);
        var runsResponse = await _maintainarrClient.SendAsync(runsRequest);
        runsResponse.EnsureSuccessStatusCode();
        var runs = (await runsResponse.Content.ReadFromJsonAsync<PmDueScanRunsResponse>())!;
        Assert.NotNull(runs.Items);
    }

    [Fact]
    public async Task Process_due_scan_rejects_missing_service_token()
    {
        var response = await _maintainarrClient.PostAsJsonAsync(
            "/api/internal/pm/process-due-scan",
            new ProcessPmDueScanRequest(PlatformSeeder.DemoTenantId, DateTimeOffset.UtcNow, 50, 1));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Process_due_scan_marks_past_due_schedule_as_due()
    {
        var schedule = await SeedPastDuePmScheduleAsync(daysPastDue: 0);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));

        var processResponse = await _maintainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessPmDueScanResponse>())!;
        Assert.Equal(1, body.MarkedDueCount);
        Assert.Contains(schedule.Id, body.UpdatedPmScheduleIds);
        Assert.Equal(1, body.WorkOrdersCreatedCount);
        Assert.Single(body.CreatedWorkOrderIds);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var stored = await db.PmSchedules.SingleAsync(x => x.Id == schedule.Id);
        Assert.Equal(PmDueStatuses.Due, stored.DueStatus);
        Assert.NotNull(stored.LastDueScanAt);

        var workOrder = await db.WorkOrders.SingleAsync(x => x.PmScheduleId == schedule.Id);
        Assert.Equal(WorkOrderSources.PmSchedule, workOrder.Source);
        Assert.Equal(WorkOrderStatuses.Open, workOrder.Status);
        Assert.StartsWith("PM:", workOrder.Title);

        var outbox = await db.MaintenancePlatformOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId)
            .ToListAsync();
        Assert.Contains(outbox, x =>
            x.EventKind == PmOccurrenceCreated
            && x.RelatedEntityType == PmOccurrence
            && x.RelatedEntityId == schedule.Id);
        Assert.Contains(outbox, x =>
            x.EventKind == PmDue
            && x.RelatedEntityType == MaintenancePlatformEventRelatedEntityTypes.PmSchedule
            && x.RelatedEntityId == schedule.Id
            && x.ProcessingStatus == MaintenancePlatformEventStatuses.Processed);
        Assert.Contains(outbox, x =>
            x.EventKind == WorkOrderCreated
            && x.RelatedEntityType == MaintenancePlatformEventRelatedEntityTypes.WorkOrder
            && x.RelatedEntityId == workOrder.Id);
        Assert.Contains(outbox, x =>
            x.EventKind == PmOccurrenceWorkOrderGenerated
            && x.RelatedEntityType == PmOccurrence
            && x.RelatedEntityId == schedule.Id);
    }

    [Fact]
    public async Task Process_due_scan_enqueues_pm_overdue_platform_event()
    {
        var schedule = await SeedPastDuePmScheduleAsync(daysPastDue: 3);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));

        var processResponse = await _maintainarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var body = (await processResponse.Content.ReadFromJsonAsync<ProcessPmDueScanResponse>())!;
        Assert.Equal(1, body.MarkedOverdueCount);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var outbox = await db.MaintenancePlatformOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId
                && x.RelatedEntityType == MaintenancePlatformEventRelatedEntityTypes.PmSchedule
                && x.RelatedEntityId == schedule.Id)
            .ToListAsync();

        Assert.Contains(outbox, x => x.EventKind == PmOccurrenceCreated);
        Assert.Contains(outbox, x => x.EventKind == PmOverdue);
        Assert.Contains(outbox, x => x.EventKind == PmOccurrenceOverdue);
        Assert.All(outbox, x => Assert.Equal(MaintenancePlatformEventStatuses.Processed, x.ProcessingStatus));
    }

    [Fact]
    public async Task Process_due_scan_work_order_generation_is_idempotent()
    {
        var schedule = await SeedPastDuePmScheduleAsync(daysPastDue: 0);

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));

        var firstResponse = await _maintainarrClient.SendAsync(processRequest);
        firstResponse.EnsureSuccessStatusCode();
        var firstBody = (await firstResponse.Content.ReadFromJsonAsync<ProcessPmDueScanResponse>())!;
        Assert.Equal(1, firstBody.WorkOrdersCreatedCount);

        var secondRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        secondRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        secondRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));

        var secondResponse = await _maintainarrClient.SendAsync(secondRequest);
        secondResponse.EnsureSuccessStatusCode();
        var secondBody = (await secondResponse.Content.ReadFromJsonAsync<ProcessPmDueScanResponse>())!;
        Assert.Equal(0, secondBody.WorkOrdersCreatedCount);
        Assert.Equal(1, secondBody.WorkOrdersLinkedCount);

        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var workOrder = await db.WorkOrders.SingleAsync(x => x.PmScheduleId == schedule.Id);
        Assert.Equal(1, await db.WorkOrders.CountAsync(x => x.PmScheduleId == schedule.Id));

        var managerToken = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var completeRequest = Authorized(HttpMethod.Patch, $"/api/work-orders/{workOrder.Id}/status", managerToken);
        completeRequest.Content = JsonContent.Create(new UpdateWorkOrderStatusRequest("completed"));
        var completeResponse = await _maintainarrClient.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();

        var completedSchedule = await db.PmSchedules.SingleAsync(x => x.Id == schedule.Id);
        Assert.Equal(PmDueStatuses.Completed, completedSchedule.DueStatus);
        Assert.NotNull(completedSchedule.LastCompletedAt);

        var completedOutbox = await db.MaintenancePlatformOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityId == schedule.Id)
            .ToListAsync();
        Assert.Contains(completedOutbox, x => x.EventKind == PmOccurrenceWorkOrderGenerated);
        Assert.Contains(completedOutbox, x => x.EventKind == PmOccurrenceCompleted);
    }

    [Fact]
    public async Task Due_list_includes_linked_work_order()
    {
        var schedule = await SeedPastDuePmScheduleAsync();

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));
        await _maintainarrClient.SendAsync(processRequest);

        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var dueRequest = Authorized(HttpMethod.Get, "/api/preventive-maintenance/due", token);
        var dueResponse = await _maintainarrClient.SendAsync(dueRequest);
        dueResponse.EnsureSuccessStatusCode();
        var dueItems = (await dueResponse.Content.ReadFromJsonAsync<List<PmScheduleResponse>>())!;

        var dueItem = dueItems.Single(x => x.PmScheduleId == schedule.Id);
        Assert.NotNull(dueItem.LinkedWorkOrderId);
        Assert.False(string.IsNullOrWhiteSpace(dueItem.LinkedWorkOrderNumber));
        Assert.Equal(WorkOrderStatuses.Open, dueItem.LinkedWorkOrderStatus);
    }

    [Fact]
    public async Task List_pending_due_returns_candidates_before_processing()
    {
        var schedule = await SeedPastDuePmScheduleAsync();

        var listRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/internal/pm/pending-due?tenantId={PlatformSeeder.DemoTenantId}&batchSize=10");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);

        var listResponse = await _maintainarrClient.SendAsync(listRequest);
        listResponse.EnsureSuccessStatusCode();
        var pending = (await listResponse.Content.ReadFromJsonAsync<PendingPmDueResponse>())!;
        Assert.Contains(pending.Items, x => x.PmScheduleId == schedule.Id);
    }

    [Fact]
    public async Task Due_list_returns_marked_due_schedules_for_managers()
    {
        var schedule = await SeedPastDuePmScheduleAsync();

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));
        await _maintainarrClient.SendAsync(processRequest);

        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var dueRequest = Authorized(HttpMethod.Get, "/api/preventive-maintenance/due", token);
        var dueResponse = await _maintainarrClient.SendAsync(dueRequest);
        dueResponse.EnsureSuccessStatusCode();
        var dueItems = (await dueResponse.Content.ReadFromJsonAsync<List<PmScheduleResponse>>())!;
        Assert.Contains(
            dueItems,
            x => x.PmScheduleId == schedule.Id
                && (x.DueStatus == PmDueStatuses.Due || x.DueStatus == PmDueStatuses.Overdue));
    }

    [Fact]
    public async Task Due_list_v1_alias_returns_marked_due_schedules_for_managers()
    {
        var schedule = await SeedPastDuePmScheduleAsync();

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));
        await _maintainarrClient.SendAsync(processRequest);

        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var dueRequest = Authorized(HttpMethod.Get, "/api/v1/preventive-maintenance/due", token);
        var dueResponse = await _maintainarrClient.SendAsync(dueRequest);
        dueResponse.EnsureSuccessStatusCode();
        var dueItems = (await dueResponse.Content.ReadFromJsonAsync<List<PmScheduleResponse>>())!;
        Assert.Contains(
            dueItems,
            x => x.PmScheduleId == schedule.Id
                && (x.DueStatus == PmDueStatuses.Due || x.DueStatus == PmDueStatuses.Overdue));
    }

    [Fact]
    public async Task Pm_events_v1_alias_returns_marked_due_schedules_for_managers()
    {
        var schedule = await SeedPastDuePmScheduleAsync();

        var processRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/pm/process-due-scan");
        processRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _sharedWorkerToMaintainArrToken);
        processRequest.Content = JsonContent.Create(new ProcessPmDueScanRequest(
            PlatformSeeder.DemoTenantId,
            DateTimeOffset.UtcNow,
            50,
            1));
        await _maintainarrClient.SendAsync(processRequest);

        var token = CreateMaintainArrAccessToken(["maintainarr"], "maintainarr_manager");
        var eventsRequest = Authorized(HttpMethod.Get, "/api/v1/pm-events", token);
        var eventsResponse = await _maintainarrClient.SendAsync(eventsRequest);
        eventsResponse.EnsureSuccessStatusCode();
        var events = (await eventsResponse.Content.ReadFromJsonAsync<List<PmScheduleResponse>>())!;
        Assert.Contains(
            events,
            x => x.PmScheduleId == schedule.Id
                && (x.DueStatus == PmDueStatuses.Due || x.DueStatus == PmDueStatuses.Overdue));
    }

    private async Task<PmSchedule> SeedPastDuePmScheduleAsync(int daysPastDue = 2)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var now = DateTimeOffset.UtcNow;

        var assetClass = new AssetClass
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            ClassKey = "pm-scan-class",
            Name = "PM Scan Class",
            Description = "Test class",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };
        var assetType = new AssetType
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetClassId = assetClass.Id,
            TypeKey = "pm-scan-type",
            Name = "PM Scan Type",
            Description = "Test type",
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetTypeId = assetType.Id,
            AssetTag = "PM-1001",
            Name = "PM Scan Asset",
            Description = "Seeded for PM due scan worker test",
            LifecycleStatus = "active",
            CreatedAt = now,
            UpdatedAt = now
        };
        var schedule = new PmSchedule
        {
            Id = Guid.NewGuid(),
            TenantId = PlatformSeeder.DemoTenantId,
            AssetId = asset.Id,
            ScheduleKey = "oil-change",
            Name = "Oil Change",
            Description = "Quarterly oil change",
            IntervalDays = 90,
            NextDueAt = now.AddDays(-daysPastDue),
            DueStatus = PmDueStatuses.Scheduled,
            Status = "active",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.AssetClasses.Add(assetClass);
        db.AssetTypes.Add(assetType);
        db.Assets.Add(asset);
        db.PmSchedules.Add(schedule);
        await db.SaveChangesAsync();
        return schedule;
    }

    private async Task<Guid> SeedInspectionProgramAsync(PmSchedule schedule, string token)
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MaintainArrDbContext>();
        var asset = await db.Assets.AsNoTracking().FirstAsync(x => x.Id == schedule.AssetId);

        var createTemplateRequest = Authorized(HttpMethod.Post, "/api/inspection-templates", token);
        createTemplateRequest.Content = JsonContent.Create(new CreateInspectionTemplateRequest(
            "pm-inspection",
            "PM Inspection",
            "PM generated inspection template"));
        var createTemplateResponse = await _maintainarrClient.SendAsync(createTemplateRequest);
        createTemplateResponse.EnsureSuccessStatusCode();
        var template = (await createTemplateResponse.Content.ReadFromJsonAsync<InspectionTemplateDetailResponse>())!;

        var createItemRequest = Authorized(
            HttpMethod.Post,
            $"/api/inspection-templates/{template.InspectionTemplateId}/checklist-items",
            token);
        createItemRequest.Content = JsonContent.Create(new CreateInspectionChecklistItemRequest(
            "pm-check",
            "PM check",
            "pass_fail",
            true,
            10,
            null));
        var createItemResponse = await _maintainarrClient.SendAsync(createItemRequest);
        createItemResponse.EnsureSuccessStatusCode();

        var replaceAssetTypesRequest = Authorized(
            HttpMethod.Put,
            $"/api/inspection-templates/{template.InspectionTemplateId}/asset-types",
            token);
        replaceAssetTypesRequest.Content = JsonContent.Create(new ReplaceInspectionTemplateAssetTypesRequest([asset.AssetTypeId]));
        var replaceAssetTypesResponse = await _maintainarrClient.SendAsync(replaceAssetTypesRequest);
        replaceAssetTypesResponse.EnsureSuccessStatusCode();

        var activateTemplateRequest = Authorized(
            HttpMethod.Patch,
            $"/api/inspection-templates/{template.InspectionTemplateId}/status",
            token);
        activateTemplateRequest.Content = JsonContent.Create(new UpdateInspectionTemplateStatusRequest("active"));
        var activateTemplateResponse = await _maintainarrClient.SendAsync(activateTemplateRequest);
        activateTemplateResponse.EnsureSuccessStatusCode();

        var createProgramRequest = Authorized(HttpMethod.Post, "/api/preventive-maintenance/programs", token);
        createProgramRequest.Content = JsonContent.Create(new CreatePmProgramRequest(
            "pm-inspection-program",
            "PM Inspection Program",
            "Auto-generated PM inspection program",
            "asset_type",
            asset.AssetTypeId,
            null,
            [schedule.Id],
            true,
            template.InspectionTemplateId));
        var createProgramResponse = await _maintainarrClient.SendAsync(createProgramRequest);
        createProgramResponse.EnsureSuccessStatusCode();
        var program = (await createProgramResponse.Content.ReadFromJsonAsync<PmProgramDetailResponse>())!;
        Assert.True(program.AutoGenerateInspection);
        Assert.Equal(template.InspectionTemplateId, program.InspectionTemplateId);

        var activateProgramRequest = Authorized(
            HttpMethod.Patch,
            $"/api/preventive-maintenance/programs/{program.PmProgramId}/status",
            token);
        activateProgramRequest.Content = JsonContent.Create(new UpdatePmProgramStatusRequest("active"));
        var activateProgramResponse = await _maintainarrClient.SendAsync(activateProgramRequest);
        activateProgramResponse.EnsureSuccessStatusCode();

        return template.InspectionTemplateId;
    }

    private string CreateMaintainArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_admin")
    {
        using var scope = _maintainarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<MaintainArrTokenService>();
        var (token, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Demo Admin",
            PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin: false);
        return token;
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
            $"{sourceProduct}-pm-due-{Guid.NewGuid():N}",
            $"{sourceProduct} PM due scan test",
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
