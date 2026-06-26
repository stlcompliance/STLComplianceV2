using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexArr.Api.Services;
using RoutArr.Api.Contracts;
using RoutArr.Api.Data;
using RoutArr.Api.Entities;
using RoutArr.Api.Services;

namespace STLCompliance.RoutArr.Auth.Tests;

public sealed class RoutArrTmsRuntimeTests : IAsyncLifetime
{
    private WebApplicationFactory<global::RoutArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"RoutArrTmsRuntime-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::RoutArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.UseSetting("ServiceToken:SigningKey", signingKey);
            builder.UseSetting("DriverEligibility:CheckStaffArrReadiness", "false");
            builder.UseSetting("DriverEligibility:CheckTrainArrQualification", "false");
            builder.UseSetting("AssetDispatchability:CheckMaintainArrReadiness", "false");
            builder.UseSetting("DispatchWorkflowGates:CheckComplianceCoreWorkflowGates", "false");
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RoutArrDbContext>(services);
                services.AddDbContext<RoutArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Tms_runtime_creates_related_records_and_emits_implemented_facts()
    {
        var adminToken = CreateRoutArrAccessToken("routarr_admin");

        var demand = await CreateDemandAsync(adminToken, "ORD-1001", "North DC", "South DC");
        Assert.StartsWith("TD-", demand.DemandNumber);
        Assert.Equal("ready_for_planning", demand.Status);
        Assert.Equal("ordarr", demand.SourceProduct);
        Assert.Contains(demand.SourceRefs, sourceRef => sourceRef.SourceProduct == "loadarr");

        var plannedRequest = Authorized(HttpMethod.Patch, $"/api/transportation-demands/{demand.TransportationDemandId}/status", adminToken);
        plannedRequest.Content = JsonContent.Create(new { status = "planned" });
        var plannedResponse = await _client.SendAsync(plannedRequest);
        plannedResponse.EnsureSuccessStatusCode();
        var planned = (await plannedResponse.Content.ReadFromJsonAsync<TransportationDemandResponse>())!;
        Assert.Equal("planned", planned.Status);
        Assert.Equal("planned", planned.PlanningStatus);

        var planRequest = Authorized(HttpMethod.Post, "/api/planning/scenarios", adminToken);
        planRequest.Content = JsonContent.Create(new
        {
            demandRefs = new[] { demand.TransportationDemandId },
            objective = "service_cost_balance"
        });
        var planResponse = await _client.SendAsync(planRequest);
        planResponse.EnsureSuccessStatusCode();
        var scenario = (await planResponse.Content.ReadFromJsonAsync<PlanningScenarioResponse>())!;
        Assert.StartsWith("TPL-", scenario.ScenarioNumber);
        Assert.Single(scenario.Suggestions);

        var tenderRequest = Authorized(HttpMethod.Post, "/api/tenders", adminToken);
        tenderRequest.Content = JsonContent.Create(new
        {
            transportationDemandId = demand.TransportationDemandId,
            routingGuideSequence = 1,
            carrierSupplierRef = "supplyarr:carrier:carrier-a",
            carrierSnapshotJson = """{"name":"Carrier A"}""",
            tenderMethod = "portal"
        });
        var tenderResponse = await _client.SendAsync(tenderRequest);
        tenderResponse.EnsureSuccessStatusCode();
        var tender = (await tenderResponse.Content.ReadFromJsonAsync<CarrierTenderResponse>())!;
        Assert.Equal("created", tender.Status);

        var acceptTenderRequest = Authorized(HttpMethod.Patch, $"/api/tenders/{tender.TenderId}/status", adminToken);
        acceptTenderRequest.Content = JsonContent.Create(new { status = "accepted" });
        var acceptTenderResponse = await _client.SendAsync(acceptTenderRequest);
        acceptTenderResponse.EnsureSuccessStatusCode();
        var acceptedTender = (await acceptTenderResponse.Content.ReadFromJsonAsync<CarrierTenderResponse>())!;
        Assert.Equal("accepted", acceptedTender.Status);

        var estimatedRatingRequest = Authorized(HttpMethod.Post, "/api/freight-ratings", adminToken);
        estimatedRatingRequest.Content = JsonContent.Create(new
        {
            transportationDemandId = demand.TransportationDemandId,
            buyRateEstimate = 900m,
            sellRateEstimate = 1250m,
            plannedFreightCost = 900m,
            currencyCode = "USD",
            rateSourceSnapshot = "spot estimate",
            allocationSnapshotJson = """{"basis":"demand"}"""
        });
        var estimatedRatingResponse = await _client.SendAsync(estimatedRatingRequest);
        estimatedRatingResponse.EnsureSuccessStatusCode();
        var estimatedRating = (await estimatedRatingResponse.Content.ReadFromJsonAsync<FreightRatingResponse>())!;
        Assert.Equal("estimated", estimatedRating.Status);

        var ratingRequest = Authorized(HttpMethod.Post, "/api/freight-ratings", adminToken);
        ratingRequest.Content = JsonContent.Create(new
        {
            transportationDemandId = demand.TransportationDemandId,
            buyRateEstimate = 900m,
            sellRateEstimate = 1250m,
            plannedFreightCost = 900m,
            actualFreightCost = 975m,
            currencyCode = "USD",
            rateSourceSnapshot = "spot estimate",
            allocationSnapshotJson = """{"basis":"demand"}"""
        });
        var ratingResponse = await _client.SendAsync(ratingRequest);
        ratingResponse.EnsureSuccessStatusCode();
        var rating = (await ratingResponse.Content.ReadFromJsonAsync<FreightRatingResponse>())!;
        Assert.Equal("variance_detected", rating.Status);
        Assert.Equal(75m, rating.VarianceAmount);

        var visibilityRequest = Authorized(HttpMethod.Post, "/api/visibility-events", adminToken);
        visibilityRequest.Content = JsonContent.Create(new
        {
            transportationDemandId = demand.TransportationDemandId,
            eventType = "gate_in",
            source = "telematics",
            normalizedStatus = "gate_in",
            freshnessState = "current",
            rawExternalRef = "telematics-evt-1001",
            summary = "Trailer arrived at South DC"
        });
        var visibilityResponse = await _client.SendAsync(visibilityRequest);
        visibilityResponse.EnsureSuccessStatusCode();
        var visibility = (await visibilityResponse.Content.ReadFromJsonAsync<VisibilityEventResponse>())!;
        Assert.True(visibility.UpdatedTrackingState);
        Assert.Equal("current", visibility.FreshnessState);

        var yardRequest = Authorized(HttpMethod.Post, "/api/yard/events", adminToken);
        yardRequest.Content = JsonContent.Create(new
        {
            transportationDemandId = demand.TransportationDemandId,
            eventType = "trailer_dropped",
            trailerAssetRef = "maintainarr:trailer:tr-100",
            staffarrYardLocationRef = "staffarr:yard:south",
            source = "yard_console",
            dispatchImpact = "Trailer dwell clock started"
        });
        var yardResponse = await _client.SendAsync(yardRequest);
        yardResponse.EnsureSuccessStatusCode();
        var yardEvent = (await yardResponse.Content.ReadFromJsonAsync<YardEventResponse>())!;
        Assert.Equal("trailer_dropped", yardEvent.EventType);

        var claimRequest = Authorized(HttpMethod.Post, "/api/freight-claims", adminToken);
        claimRequest.Content = JsonContent.Create(new
        {
            transportationDemandId = demand.TransportationDemandId,
            claimAgainstPartyType = "carrier",
            claimReason = "Late delivery detention dispute",
            claimAmount = 150m,
            currencyCode = "USD"
        });
        var claimResponse = await _client.SendAsync(claimRequest);
        claimResponse.EnsureSuccessStatusCode();
        var claim = (await claimResponse.Content.ReadFromJsonAsync<FreightClaimResponse>())!;
        Assert.StartsWith("FCL-", claim.ClaimNumber);

        var documentRequest = Authorized(HttpMethod.Post, "/api/document-packets", adminToken);
        documentRequest.Content = JsonContent.Create(new
        {
            transportationDemandId = demand.TransportationDemandId,
            packetType = "dispatch_packet",
            requiredDocumentTypes = new[] { "bill_of_lading", "proof_of_delivery" },
            sourceFactsJson = """{"source":"test"}"""
        });
        var documentResponse = await _client.SendAsync(documentRequest);
        documentResponse.EnsureSuccessStatusCode();
        var packet = (await documentResponse.Content.ReadFromJsonAsync<DocumentPacketResponse>())!;
        Assert.Equal("requested", packet.Status);

        var financeRequest = Authorized(HttpMethod.Post, "/api/finance-packet-contributions", adminToken);
        financeRequest.Content = JsonContent.Create(new
        {
            transportationDemandId = demand.TransportationDemandId,
            freightRatingId = rating.FreightRatingId,
            contributionType = "freight_operational_snapshot",
            targetProduct = "ordarr",
            operationalSummary = "Freight contribution ready for order closeout.",
            costSnapshotJson = """{"planned":900,"actual":975}""",
            documentPacketRefs = new[] { packet.DocumentPacketRequestId.ToString("D") },
            claimRefs = new[] { claim.FreightClaimId.ToString("D") }
        });
        var financeResponse = await _client.SendAsync(financeRequest);
        financeResponse.EnsureSuccessStatusCode();
        var contribution = (await financeResponse.Content.ReadFromJsonAsync<FinancePacketContributionResponse>())!;
        Assert.Equal("ready", contribution.Status);
        Assert.Equal("ordarr", contribution.TargetProduct);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoutArrDbContext>();
        var eventKinds = await db.IntegrationOutboxEvents
            .Where(x => x.TenantId == PlatformSeeder.DemoTenantId)
            .Select(x => x.EventKind)
            .ToListAsync();

        Assert.Contains(RoutArrIntegrationOutboxEventKinds.TransportationDemandCreated, eventKinds);
        Assert.Contains(RoutArrIntegrationOutboxEventKinds.TransportationDemandPlanned, eventKinds);
        Assert.Contains(RoutArrIntegrationOutboxEventKinds.TenderAccepted, eventKinds);
        Assert.Contains(RoutArrIntegrationOutboxEventKinds.FreightRateEstimated, eventKinds);
        Assert.Contains(RoutArrIntegrationOutboxEventKinds.FreightCostVarianceDetected, eventKinds);
        Assert.Contains(RoutArrIntegrationOutboxEventKinds.VisibilityEventReceived, eventKinds);
        Assert.Contains(RoutArrIntegrationOutboxEventKinds.TrailerDropped, eventKinds);
        Assert.Contains(RoutArrIntegrationOutboxEventKinds.FreightClaimRequested, eventKinds);
        Assert.Contains(RoutArrIntegrationOutboxEventKinds.FinancePacketContributionReady, eventKinds);
    }

    [Fact]
    public async Task Transportation_demands_are_tenant_scoped_and_driver_actions_are_limited()
    {
        var adminToken = CreateRoutArrAccessToken("routarr_admin");
        var otherTenantToken = CreateRoutArrAccessToken("routarr_admin", Guid.NewGuid());
        var driverToken = CreateRoutArrAccessToken("routarr_driver");

        var demand = await CreateDemandAsync(adminToken, "ORD-2002", "East DC", "West DC");

        var otherTenantList = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/transportation-demands", otherTenantToken));
        otherTenantList.EnsureSuccessStatusCode();
        var otherTenantDemands = (await otherTenantList.Content.ReadFromJsonAsync<List<TransportationDemandResponse>>())!;
        Assert.Empty(otherTenantDemands);

        var forbiddenCreate = Authorized(HttpMethod.Post, "/api/transportation-demands", driverToken);
        forbiddenCreate.Content = JsonContent.Create(new
        {
            title = "Driver-created demand",
            originLocationRef = "A",
            destinationLocationRef = "B"
        });
        var forbiddenCreateResponse = await _client.SendAsync(forbiddenCreate);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenCreateResponse.StatusCode);

        var driverVisibilityRequest = Authorized(HttpMethod.Post, "/api/visibility-events", driverToken);
        driverVisibilityRequest.Content = JsonContent.Create(new
        {
            transportationDemandId = demand.TransportationDemandId,
            eventType = "arrived",
            source = "driver_portal",
            normalizedStatus = "arrived",
            summary = "Driver submitted arrival"
        });
        var driverVisibilityResponse = await _client.SendAsync(driverVisibilityRequest);
        driverVisibilityResponse.EnsureSuccessStatusCode();
        var driverVisibility = (await driverVisibilityResponse.Content.ReadFromJsonAsync<VisibilityEventResponse>())!;
        Assert.Equal("driver_portal", driverVisibility.Source);

        var driverFinanceRequest = Authorized(HttpMethod.Get, "/api/finance-packet-contributions", driverToken);
        var driverFinanceResponse = await _client.SendAsync(driverFinanceRequest);
        Assert.Equal(HttpStatusCode.Forbidden, driverFinanceResponse.StatusCode);
    }

    [Fact]
    public async Task Transportation_demands_reject_platform_admin_without_routarr_role()
    {
        var platformAdminToken = CreateRoutArrAccessToken(
            "staffarr_manager",
            isPlatformAdmin: true);

        var readResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/transportation-demands", platformAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, readResponse.StatusCode);

        var createRequest = Authorized(HttpMethod.Post, "/api/transportation-demands", platformAdminToken);
        createRequest.Content = JsonContent.Create(new
        {
            title = "Platform admin demand",
            originLocationRef = "Origin",
            destinationLocationRef = "Destination"
        });
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);

        var financeResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/finance-packet-contributions", platformAdminToken));
        Assert.Equal(HttpStatusCode.Forbidden, financeResponse.StatusCode);
    }

    private async Task<TransportationDemandResponse> CreateDemandAsync(
        string accessToken,
        string orderNumber,
        string origin,
        string destination)
    {
        var request = Authorized(HttpMethod.Post, "/api/transportation-demands", accessToken);
        request.Content = JsonContent.Create(new
        {
            title = $"Move {orderNumber}",
            status = "ready_for_planning",
            sourceProduct = "ordarr",
            sourceObjectType = "order",
            sourceObjectId = orderNumber,
            sourceObjectNumber = orderNumber,
            originLocationRef = origin,
            destinationLocationRef = destination,
            transportMode = "truckload",
            serviceLevel = "expedited",
            equipmentRequirement = "reefer",
            handlingRequirements = new[] { "temperature_control" },
            customerRefs = new[] { "customarr:customer:alpha" },
            orderRefs = new[] { $"ordarr:order:{orderNumber}" },
            vendorRefs = new[] { "supplyarr:carrier:carrier-a" },
            requirementRefs = new[] { "compliancecore:requirement:temperature" },
            lines = new[]
            {
                new
                {
                    sourceProduct = "loadarr",
                    sourceObjectRef = $"loadarr:load:{orderNumber}",
                    descriptionSnapshot = "Palletized freight",
                    quantitySnapshot = 12m,
                    unitOfMeasure = "pallet",
                    handlingRequirementSnapshot = "temperature_control"
                }
            },
            requirements = new[]
            {
                new
                {
                    requirementType = "temperature_check",
                    sourceProduct = "compliancecore",
                    sourceRequirementRef = "temperature_check",
                    required = true,
                    status = "open",
                    evidenceRefs = Array.Empty<string>()
                }
            },
            sourceRefs = new[]
            {
                new
                {
                    sourceProduct = "ordarr",
                    sourceObjectType = "order",
                    sourceObjectId = orderNumber,
                    sourceObjectNumber = orderNumber,
                    displayNameSnapshot = $"Order {orderNumber}",
                    statusSnapshot = "released",
                    freshnessState = "current"
                },
                new
                {
                    sourceProduct = "loadarr",
                    sourceObjectType = "load",
                    sourceObjectId = $"load-{orderNumber}",
                    sourceObjectNumber = $"LOAD-{orderNumber}",
                    displayNameSnapshot = $"Load {orderNumber}",
                    statusSnapshot = "ready",
                    freshnessState = "current"
                }
            }
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TransportationDemandResponse>())!;
    }

    private string CreateRoutArrAccessToken(
        string tenantRoleKey,
        Guid? tenantId = null,
        Guid? personId = null,
        bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RoutArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            PlatformSeeder.DemoAdminUserId,
            personId ?? PlatformSeeder.DemoAdminUserId,
            PlatformSeeder.DemoAdminEmail,
            "Test Admin",
            tenantId ?? PlatformSeeder.DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            ["routarr"],
            isPlatformAdmin);
        return accessToken;
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
