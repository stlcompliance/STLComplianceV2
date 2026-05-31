using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Services;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrIntegrationEventTests : IAsyncLifetime
{
    private WebApplicationFactory<global::NexArr.Api.Program> _nexarrFactory = null!;
    private WebApplicationFactory<global::RoutArr.Api.Program> _routarrFactory = null!;
    private HttpClient _nexarrClient = null!;
    private HttpClient _routarrClient = null!;
    private string _sharedWorkerToRoutarrToken = null!;
    private RecordingStaffArrProductIncidentHandler _staffarrIncidentHandler = null!;
    private RecordingTrainArrIncidentRemediationHandler _trainarrIncidentHandler = null!;
    private RecordingMaintainArrRoutarrEventHandler _maintainarrEventHandler = null!;
    private RecordingComplianceCoreProductFactHandler _complianceCoreFactHandler = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var nexArrDbName = $"RoutArrIntegrationEventNexArr-{Guid.NewGuid():N}";
        var routArrDbName = $"RoutArrIntegrationEventRoutArr-{Guid.NewGuid():N}";

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
        _sharedWorkerToRoutarrToken = await IssueServiceTokenAsync(
            adminToken,
            "shared-worker",
            ["routarr"],
            IntegrationEventProcessingService.ProcessEventsActionScope);
        _staffarrIncidentHandler = new RecordingStaffArrProductIncidentHandler();
        _trainarrIncidentHandler = new RecordingTrainArrIncidentRemediationHandler();
        _maintainarrEventHandler = new RecordingMaintainArrRoutarrEventHandler();
        _complianceCoreFactHandler = new RecordingComplianceCoreProductFactHandler();

        _routarrFactory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("StaffArr:BaseUrl", "http://staffarr.test");
            builder.UseSetting("StaffArr:ServiceToken", "routarr-to-staffarr-token");
            builder.UseSetting("TrainArr:BaseUrl", "http://trainarr.test");
            builder.UseSetting("TrainArr:ServiceToken", "routarr-to-trainarr-token");
            builder.UseSetting("MaintainArr:BaseUrl", "http://maintainarr.test");
            builder.UseSetting("MaintainArr:ServiceToken", "routarr-to-maintainarr-token");
            builder.UseSetting("ComplianceCore:BaseUrl", "http://compliancecore.test");
            builder.UseSetting("ComplianceCore:ServiceToken", "routarr-to-compliancecore-token");
            builder.UseSetting("DriverEligibility:CheckStaffArrReadiness", "false");
            builder.UseSetting("DriverEligibility:CheckTrainArrQualification", "false");
            builder.UseSetting("AssetDispatchability:CheckMaintainArrReadiness", "false");
            builder.UseSetting("DispatchWorkflowGates:CheckComplianceCoreWorkflowGates", "false");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(routArrDbName));
                services.AddHttpClient<StaffArrProductIncidentClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _staffarrIncidentHandler);
                services.AddHttpClient<TrainArrIncidentRemediationClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _trainarrIncidentHandler);
                services.AddHttpClient<MaintainArrRoutarrEventClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _maintainarrEventHandler);
                services.AddHttpClient<ComplianceCoreProductFactClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => _complianceCoreFactHandler);
            });
        });

        _routarrClient = _routarrFactory.CreateClient();

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _routarrClient.Dispose();
        _nexarrClient.Dispose();
        await _routarrFactory.DisposeAsync();
        await _nexarrFactory.DisposeAsync();
    }

    [Fact]
    public async Task Trip_lifecycle_and_assignment_changes_enqueue_integration_outbox_events()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid().ToString();
        var vehicleRefKey = $"vehicle-{Guid.NewGuid():N}";

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Integration event trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId,
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var assignVehicleRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-vehicle", adminToken);
        assignVehicleRequest.Content = JsonContent.Create(new AssignTripVehicleRequest(
            vehicleRefKey,
            IgnoreAvailabilityConflicts: false,
            IgnoreDispatchabilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignVehicleRequest)).EnsureSuccessStatusCode();

        var dispatchRequest = Authorized(
            HttpMethod.Patch,
            $"/api/trips/{created.TripId}/status",
            adminToken);
        dispatchRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("dispatched"));
        (await _routarrClient.SendAsync(dispatchRequest)).EnsureSuccessStatusCode();

        var startRequest = Authorized(
            HttpMethod.Patch,
            $"/api/trips/{created.TripId}/status",
            adminToken);
        startRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("in_progress"));
        (await _routarrClient.SendAsync(startRequest)).EnsureSuccessStatusCode();

        var completeRequest = Authorized(
            HttpMethod.Patch,
            $"/api/trips/{created.TripId}/status",
            adminToken);
        completeRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("completed"));
        (await _routarrClient.SendAsync(completeRequest)).EnsureSuccessStatusCode();

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var events = await db.IntegrationOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityId == created.TripId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        Assert.Equal(7, events.Count);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.TripCreated);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.DriverAssignmentChanged);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.EquipmentAssignmentChanged);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.TripReleased);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.TripDispatched);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.TripStarted);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.TripCompleted);
        Assert.All(events, x => Assert.Equal(IntegrationEventStatuses.Pending, x.ProcessingStatus));
    }

    [Fact]
    public async Task Trip_cancellation_enqueues_integration_outbox_event()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Cancelled integration event trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var cancelRequest = Authorized(
            HttpMethod.Patch,
            $"/api/trips/{created.TripId}/status",
            adminToken);
        cancelRequest.Content = JsonContent.Create(new UpdateTripDispatchStatusRequest("cancelled"));
        (await _routarrClient.SendAsync(cancelRequest)).EnsureSuccessStatusCode();

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var events = await db.IntegrationOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityId == created.TripId)
            .ToListAsync();

        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.TripCreated);
        Assert.Contains(events, x => x.EventKind == RoutArrIntegrationOutboxEventKinds.TripCancelled);
    }

    [Fact]
    public async Task Worker_process_batch_marks_outbox_events_processed()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid().ToString();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Worker process trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId,
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/integration-events/process-batch",
            _sharedWorkerToRoutarrToken);
        processRequest.Content = JsonContent.Create(new ProcessIntegrationOutboxEventsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            10));
        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();
        var result = (await processResponse.Content.ReadFromJsonAsync<ProcessIntegrationOutboxEventsResponse>())!;

        Assert.True(result.ProcessedCount >= 1);

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var processed = await db.IntegrationOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId && x.RelatedEntityId == created.TripId)
            .ToListAsync();
        Assert.All(processed, x => Assert.Equal(IntegrationEventStatuses.Processed, x.ProcessingStatus));
    }

    [Fact]
    public async Task Worker_routes_staffarr_incident_outbox_event_to_staffarr()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "StaffArr routed incident trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId.ToString(),
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var incidentRequest = Authorized(HttpMethod.Post, "/api/v1/incidents", adminToken);
        incidentRequest.Content = JsonContent.Create(new CreateDispatchIncidentRequest(
            "Driver injury during unloading",
            "Driver reported a hand injury while unloading.",
            DispatchIncidentTypes.Injury,
            DispatchIncidentSeverities.High,
            trip.TripId,
            null,
            null,
            DispatchIncidentRoutedProducts.StaffArr));
        var incidentResponse = await _routarrClient.SendAsync(incidentRequest);
        incidentResponse.EnsureSuccessStatusCode();
        var incident = (await incidentResponse.Content.ReadFromJsonAsync<DispatchExceptionSummaryResponse>())!;
        Assert.Null(incident.StaffarrPersonnelIncidentId);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/integration-events/process-batch",
            _sharedWorkerToRoutarrToken);
        processRequest.Content = JsonContent.Create(new ProcessIntegrationOutboxEventsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            20));
        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        Assert.Single(_staffarrIncidentHandler.Requests);
        var captured = _staffarrIncidentHandler.Requests[0];
        Assert.Equal("/api/v1/integrations/product-incidents", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", captured.Headers.Authorization!.Scheme);
        Assert.Equal("routarr-to-staffarr-token", captured.Headers.Authorization.Parameter);

        using var payload = JsonDocument.Parse(_staffarrIncidentHandler.Bodies[0]);
        Assert.Equal("routarr", payload.RootElement.GetProperty("sourceProduct").GetString());
        Assert.Equal(incident.ExceptionId, payload.RootElement.GetProperty("sourceIncidentId").GetGuid());
        Assert.Equal(driverPersonId, payload.RootElement.GetProperty("personId").GetGuid());
        Assert.Equal("safety", payload.RootElement.GetProperty("reasonCategoryKey").GetString());
        Assert.Equal(incident.ExceptionKey, payload.RootElement.GetProperty("sourceReferenceKey").GetString());

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var routedIncident = await db.DispatchExceptions.SingleAsync(x => x.Id == incident.ExceptionId);
        Assert.Equal(RecordingStaffArrProductIncidentHandler.StaffArrIncidentId, routedIncident.StaffarrPersonnelIncidentId);
        Assert.Equal("routed", routedIncident.StaffarrIncidentRouteStatus);
        Assert.NotNull(routedIncident.StaffarrIncidentRoutedAt);
    }

    [Fact]
    public async Task Worker_routes_training_related_incident_outbox_event_to_trainarr()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "TrainArr routed incident trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId.ToString(),
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var incidentRequest = Authorized(HttpMethod.Post, "/api/v1/incidents", adminToken);
        incidentRequest.Content = JsonContent.Create(new CreateDispatchIncidentRequest(
            "Load securement retraining needed",
            "Supervisor identified a load securement mistake requiring driver retraining.",
            DispatchIncidentTypes.TrainingRelated,
            DispatchIncidentSeverities.Medium,
            trip.TripId,
            null,
            null,
            DispatchIncidentRoutedProducts.TrainArr));
        var incidentResponse = await _routarrClient.SendAsync(incidentRequest);
        incidentResponse.EnsureSuccessStatusCode();
        var incident = (await incidentResponse.Content.ReadFromJsonAsync<DispatchExceptionSummaryResponse>())!;
        Assert.Null(incident.TrainarrIncidentRemediationId);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/integration-events/process-batch",
            _sharedWorkerToRoutarrToken);
        processRequest.Content = JsonContent.Create(new ProcessIntegrationOutboxEventsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            20));
        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        Assert.Single(_trainarrIncidentHandler.Requests);
        var captured = _trainarrIncidentHandler.Requests[0];
        Assert.Equal("/api/v1/integrations/routarr-incident-remediations", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", captured.Headers.Authorization!.Scheme);
        Assert.Equal("routarr-to-trainarr-token", captured.Headers.Authorization.Parameter);
        Assert.Empty(_staffarrIncidentHandler.Requests);

        using var payload = JsonDocument.Parse(_trainarrIncidentHandler.Bodies[0]);
        Assert.Equal(PlatformSeeder.DemoTenantId, payload.RootElement.GetProperty("tenantId").GetGuid());
        Assert.Equal(incident.ExceptionId, payload.RootElement.GetProperty("sourceEventId").GetGuid());
        Assert.Equal(RoutArrIntegrationOutboxEventKinds.IncidentCreated, payload.RootElement.GetProperty("eventKind").GetString());
        var incidentPayload = payload.RootElement.GetProperty("payload");
        Assert.Equal(driverPersonId.ToString(), incidentPayload.GetProperty("driverPersonId").GetString());
        Assert.Equal(DispatchIncidentTypes.TrainingRelated, incidentPayload.GetProperty("incidentType").GetString());
        Assert.Equal(DispatchIncidentRoutedProducts.TrainArr, incidentPayload.GetProperty("incidentRoutedProduct").GetString());

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var routedIncident = await db.DispatchExceptions.SingleAsync(x => x.Id == incident.ExceptionId);
        Assert.Equal(RecordingTrainArrIncidentRemediationHandler.RemediationId, routedIncident.TrainarrIncidentRemediationId);
        Assert.Equal("routed", routedIncident.TrainarrIncidentRouteStatus);
        Assert.NotNull(routedIncident.TrainarrIncidentRoutedAt);
    }

    [Fact]
    public async Task Worker_routes_driver_reported_defect_outbox_event_to_maintainarr()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid();
        var vehicleRefKey = $"vehicle-{Guid.NewGuid():N}";

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "MaintainArr defect route trip",
            string.Empty,
            vehicleRefKey,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId.ToString(),
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var driverToken = CreateRoutArrAccessToken(["routarr"], "routarr_driver", driverPersonId);
        var dvirRequest = Authorized(HttpMethod.Post, $"/api/trips/{trip.TripId}/dvir", driverToken);
        dvirRequest.Content = JsonContent.Create(new SubmitTripDvirRequest(
            DvirInspectionPhases.PostTrip,
            vehicleRefKey,
            DvirInspectionResults.Fail,
            124000,
            "Air leak at gladhand."));
        var dvirResponse = await _routarrClient.SendAsync(dvirRequest);
        dvirResponse.EnsureSuccessStatusCode();
        var dvir = (await dvirResponse.Content.ReadFromJsonAsync<TripDvirInspectionResponse>())!;
        Assert.Null(dvir.MaintainarrDefectId);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/integration-events/process-batch",
            _sharedWorkerToRoutarrToken);
        processRequest.Content = JsonContent.Create(new ProcessIntegrationOutboxEventsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            20));
        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        Assert.Single(_maintainarrEventHandler.Requests);
        var captured = _maintainarrEventHandler.Requests[0];
        Assert.Equal("/api/v1/integrations/routarr-events", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", captured.Headers.Authorization!.Scheme);
        Assert.Equal("routarr-to-maintainarr-token", captured.Headers.Authorization.Parameter);

        using var payload = JsonDocument.Parse(_maintainarrEventHandler.Bodies[0]);
        Assert.Equal(PlatformSeeder.DemoTenantId, payload.RootElement.GetProperty("tenantId").GetGuid());
        Assert.Equal(RoutArrIntegrationOutboxEventKinds.DriverReportedDefect, payload.RootElement.GetProperty("eventKind").GetString());
        Assert.Equal("trip_dvir", payload.RootElement.GetProperty("relatedEntityType").GetString());
        Assert.Equal(dvir.DvirId, payload.RootElement.GetProperty("relatedEntityId").GetGuid());
        var defectPayload = payload.RootElement.GetProperty("payload");
        Assert.Equal(vehicleRefKey, defectPayload.GetProperty("vehicleRefKey").GetString());
        Assert.Equal(DvirInspectionResults.Fail, defectPayload.GetProperty("dvirResult").GetString());
        Assert.Equal("Air leak at gladhand.", defectPayload.GetProperty("defectNotes").GetString());

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var routedDvir = await db.TripDvirInspections.SingleAsync(x => x.Id == dvir.DvirId);
        Assert.Equal(RecordingMaintainArrRoutarrEventHandler.InboundEventId, routedDvir.MaintainarrInboundEventId);
        Assert.Equal(RecordingMaintainArrRoutarrEventHandler.DefectId, routedDvir.MaintainarrDefectId);
        Assert.Equal("routed", routedDvir.MaintainarrEventRouteStatus);
        Assert.NotNull(routedDvir.MaintainarrEventRoutedAt);
    }

    [Fact]
    public async Task Worker_routes_equipment_incident_outbox_event_to_maintainarr()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid();
        var vehicleRefKey = $"vehicle-{Guid.NewGuid():N}";

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "MaintainArr equipment incident trip",
            string.Empty,
            vehicleRefKey,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId.ToString(),
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var incidentRequest = Authorized(HttpMethod.Post, "/api/v1/incidents", adminToken);
        incidentRequest.Content = JsonContent.Create(new CreateDispatchIncidentRequest(
            "Liftgate damage discovered",
            "Driver reported liftgate damage after delivery.",
            DispatchIncidentTypes.EquipmentAbuse,
            DispatchIncidentSeverities.High,
            trip.TripId,
            null,
            null,
            DispatchIncidentRoutedProducts.MaintainArr));
        var incidentResponse = await _routarrClient.SendAsync(incidentRequest);
        incidentResponse.EnsureSuccessStatusCode();
        var incident = (await incidentResponse.Content.ReadFromJsonAsync<DispatchExceptionSummaryResponse>())!;
        Assert.Null(incident.MaintainarrDefectId);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/integration-events/process-batch",
            _sharedWorkerToRoutarrToken);
        processRequest.Content = JsonContent.Create(new ProcessIntegrationOutboxEventsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            20));
        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        Assert.Single(_maintainarrEventHandler.Requests);
        var captured = _maintainarrEventHandler.Requests[0];
        Assert.Equal("/api/v1/integrations/routarr-events", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", captured.Headers.Authorization!.Scheme);
        Assert.Equal("routarr-to-maintainarr-token", captured.Headers.Authorization.Parameter);
        Assert.Empty(_staffarrIncidentHandler.Requests);
        Assert.Empty(_trainarrIncidentHandler.Requests);

        using var payload = JsonDocument.Parse(_maintainarrEventHandler.Bodies[0]);
        Assert.Equal(incident.ExceptionId, payload.RootElement.GetProperty("sourceEventId").GetGuid());
        Assert.Equal(RoutArrIntegrationOutboxEventKinds.IncidentCreated, payload.RootElement.GetProperty("eventKind").GetString());
        Assert.Equal("dispatch_exception", payload.RootElement.GetProperty("relatedEntityType").GetString());
        var incidentPayload = payload.RootElement.GetProperty("payload");
        Assert.Equal(vehicleRefKey, incidentPayload.GetProperty("vehicleRefKey").GetString());
        Assert.Equal(DispatchIncidentTypes.EquipmentAbuse, incidentPayload.GetProperty("incidentType").GetString());
        Assert.Equal(DispatchIncidentRoutedProducts.MaintainArr, incidentPayload.GetProperty("incidentRoutedProduct").GetString());

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var routedIncident = await db.DispatchExceptions.SingleAsync(x => x.Id == incident.ExceptionId);
        Assert.Equal(RecordingMaintainArrRoutarrEventHandler.InboundEventId, routedIncident.MaintainarrInboundEventId);
        Assert.Equal(RecordingMaintainArrRoutarrEventHandler.DefectId, routedIncident.MaintainarrDefectId);
        Assert.Equal("routed", routedIncident.MaintainarrIncidentRouteStatus);
        Assert.NotNull(routedIncident.MaintainarrIncidentRoutedAt);
    }

    [Fact]
    public async Task Worker_routes_compliance_incident_outbox_event_to_compliancecore_facts()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var driverPersonId = Guid.NewGuid();
        var vehicleRefKey = $"vehicle-{Guid.NewGuid():N}";

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Compliance Core incident route trip",
            string.Empty,
            vehicleRefKey,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var trip = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{trip.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            driverPersonId.ToString(),
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var incidentRequest = Authorized(HttpMethod.Post, "/api/v1/incidents", adminToken);
        incidentRequest.Content = JsonContent.Create(new CreateDispatchIncidentRequest(
            "Hazmat paperwork discrepancy",
            "Shipment moved with incomplete hazmat paperwork.",
            DispatchIncidentTypes.ComplianceRelated,
            DispatchIncidentSeverities.High,
            trip.TripId,
            null,
            null,
            DispatchIncidentRoutedProducts.ComplianceCore));
        var incidentResponse = await _routarrClient.SendAsync(incidentRequest);
        incidentResponse.EnsureSuccessStatusCode();
        var incident = (await incidentResponse.Content.ReadFromJsonAsync<DispatchExceptionSummaryResponse>())!;
        Assert.Null(incident.CompliancecoreFactPublicationId);

        var processRequest = Authorized(
            HttpMethod.Post,
            "/api/internal/integration-events/process-batch",
            _sharedWorkerToRoutarrToken);
        processRequest.Content = JsonContent.Create(new ProcessIntegrationOutboxEventsRequest(
            PlatformSeeder.DemoTenantId,
            null,
            20));
        var processResponse = await _routarrClient.SendAsync(processRequest);
        processResponse.EnsureSuccessStatusCode();

        Assert.Single(_complianceCoreFactHandler.Requests);
        var captured = _complianceCoreFactHandler.Requests[0];
        Assert.Equal("/api/v1/integrations/product-facts/ingest", captured.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", captured.Headers.Authorization!.Scheme);
        Assert.Equal("routarr-to-compliancecore-token", captured.Headers.Authorization.Parameter);
        Assert.Empty(_staffarrIncidentHandler.Requests);
        Assert.Empty(_trainarrIncidentHandler.Requests);
        Assert.Empty(_maintainarrEventHandler.Requests);

        using var payload = JsonDocument.Parse(_complianceCoreFactHandler.Bodies[0]);
        Assert.Equal(PlatformSeeder.DemoTenantId, payload.RootElement.GetProperty("tenantId").GetGuid());
        Assert.Equal("routarr", payload.RootElement.GetProperty("sourceProduct").GetString());
        var publicationId = payload.RootElement.GetProperty("publicationId").GetGuid();
        var facts = payload.RootElement.GetProperty("facts").EnumerateArray().ToList();
        Assert.Contains(facts, fact =>
            fact.GetProperty("factKey").GetString() == "routarr.incident.type"
            && fact.GetProperty("stringValue").GetString() == DispatchIncidentTypes.ComplianceRelated);
        Assert.Contains(facts, fact =>
            fact.GetProperty("factKey").GetString() == "routarr.incident.vehicle_ref"
            && fact.GetProperty("stringValue").GetString() == vehicleRefKey);
        Assert.All(facts, fact =>
        {
            Assert.Equal("dispatch_incident", fact.GetProperty("sourceEntityType").GetString());
            Assert.Equal(incident.ExceptionId, fact.GetProperty("sourceEntityId").GetGuid());
            Assert.Equal(RoutArrIntegrationOutboxEventKinds.IncidentCreated, fact.GetProperty("sourceEventKind").GetString());
        });

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var routedIncident = await db.DispatchExceptions.SingleAsync(x => x.Id == incident.ExceptionId);
        Assert.Equal(publicationId, routedIncident.CompliancecoreFactPublicationId);
        Assert.Equal("routed", routedIncident.CompliancecoreIncidentRouteStatus);
        Assert.NotNull(routedIncident.CompliancecoreIncidentRoutedAt);
    }

    [Fact]
    public async Task Integration_event_settings_disable_skips_enqueue()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");
        var settingsRequest = Authorized(HttpMethod.Put, "/api/integration-event-settings", adminToken);
        settingsRequest.Content = JsonContent.Create(new UpsertIntegrationEventSettingsRequest(
            false,
            null,
            null));
        (await _routarrClient.SendAsync(settingsRequest)).EnsureSuccessStatusCode();

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Disabled integration events",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            Guid.NewGuid().ToString(),
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        using var scope = _routarrFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var count = await db.IntegrationOutboxEvents.CountAsync(x => x.RelatedEntityId == created.TripId);
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task Process_batch_rejects_missing_service_token()
    {
        var response = await _routarrClient.PostAsJsonAsync(
            "/api/internal/integration-events/process-batch",
            new ProcessIntegrationOutboxEventsRequest(PlatformSeeder.DemoTenantId, null, 10));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Events_v1_alias_matches_integration_event_outbox()
    {
        var adminToken = CreateRoutArrAccessToken(["routarr"], "routarr_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/trips", adminToken);
        createRequest.Content = JsonContent.Create(new CreateTripRequest(
            "Events alias trip",
            string.Empty,
            null,
            null,
            null,
            null));
        var createResponse = await _routarrClient.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<TripDetailResponse>())!;

        var assignRequest = Authorized(HttpMethod.Patch, $"/api/trips/{created.TripId}/assign-driver", adminToken);
        assignRequest.Content = JsonContent.Create(new AssignTripDriverRequest(
            Guid.NewGuid().ToString(),
            IgnoreAvailabilityConflicts: false,
            IgnoreEligibilityBlocks: false,
            IgnoreWorkflowGateBlocks: false));
        (await _routarrClient.SendAsync(assignRequest)).EnsureSuccessStatusCode();

        var outboxResponse = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/integration-event-settings/outbox?limit=10", adminToken));
        outboxResponse.EnsureSuccessStatusCode();
        var outbox = (await outboxResponse.Content.ReadFromJsonAsync<IntegrationOutboxEventListResponse>())!;

        var eventsV1Response = await _routarrClient.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/events?limit=10", adminToken));
        eventsV1Response.EnsureSuccessStatusCode();
        var eventsV1 = (await eventsV1Response.Content.ReadFromJsonAsync<IntegrationOutboxEventListResponse>())!;

        Assert.Equal(outbox.Items.Count, eventsV1.Items.Count);
    }

    private string CreateRoutArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey = "tenant_member",
        Guid? personId = null)
    {
        using var scope = _routarrFactory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RoutArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
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
            $"{sourceProduct}-integration-{Guid.NewGuid():N}",
            $"{sourceProduct} integration test",
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

    private sealed class RecordingStaffArrProductIncidentHandler : HttpMessageHandler
    {
        public static readonly Guid StaffArrIncidentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    incidentId = StaffArrIncidentId,
                    personId = Guid.NewGuid(),
                    sourceProduct = "routarr",
                    sourceIncidentId = Guid.NewGuid(),
                    status = "open",
                    idempotentReplay = false,
                }),
            };
        }
    }

    private sealed class RecordingTrainArrIncidentRemediationHandler : HttpMessageHandler
    {
        public static readonly Guid RemediationId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    remediationId = RemediationId,
                    tenantId = PlatformSeeder.DemoTenantId,
                    sourceEventId = Guid.NewGuid(),
                    staffarrPersonId = Guid.NewGuid(),
                    status = "intake_received",
                    idempotentReplay = false,
                }),
            };
        }
    }

    private sealed class RecordingMaintainArrRoutarrEventHandler : HttpMessageHandler
    {
        public static readonly Guid InboundEventId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        public static readonly Guid DefectId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken));

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    inboundEventId = InboundEventId,
                    outcome = "defect_created",
                    defectId = DefectId,
                    idempotentReplay = false,
                }),
            };
        }
    }

    private sealed class RecordingComplianceCoreProductFactHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken));

            var body = Bodies[^1];
            using var payload = JsonDocument.Parse(body);
            var facts = payload.RootElement.GetProperty("facts");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    tenantId = PlatformSeeder.DemoTenantId,
                    publicationId = payload.RootElement.GetProperty("publicationId").GetGuid(),
                    acceptedCount = facts.GetArrayLength(),
                    skippedDuplicateCount = 0,
                }),
            };
        }
    }
}
