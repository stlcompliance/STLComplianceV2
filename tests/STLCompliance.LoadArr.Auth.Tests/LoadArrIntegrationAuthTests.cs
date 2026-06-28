using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using LoadArr.Api.Data;
using LoadArr.Api.Endpoints;
using LoadArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace STLCompliance.LoadArr.Auth.Tests;

public sealed class LoadArrIntegrationAuthTests : IAsyncLifetime
{
    private static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    private static readonly Guid SecondaryTenantId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    private static readonly IReadOnlyCollection<LoadArrLocationResponse> DemoTenantLocations =
    [
        new(
            "loc-dock-01",
            "STL North Yard",
            "stl-north",
            "Receiving Dock 1",
            "dock",
            "STL North Yard / Main Warehouse / Receiving Dock 1",
            true,
            [],
            0,
            "StaffArr owns this location reference. Capacity and inventory utilization remain unavailable until the warehouse read model is ready."),
        new(
            "loc-quarantine-01",
            "STL North Yard",
            "stl-north",
            "Quarantine Bay",
            "quarantine_area",
            "STL North Yard / Quality / Quarantine Bay",
            true,
            ["quality_hold"],
            0,
            "StaffArr owns this location reference. Capacity and inventory utilization remain unavailable until the warehouse read model is ready.")
    ];
    private static readonly IReadOnlyCollection<LoadArrLocationResponse> SecondaryTenantLocations =
    [
        new(
            "loc-south-truck-17",
            "South Service Depot",
            "south-depot",
            "Truck Stock 17",
            "service_truck",
            "South Service Depot / Route Fleet / Truck Stock 17",
            true,
            [],
            0,
            "StaffArr owns this location reference. Capacity and inventory utilization remain unavailable until the warehouse read model is ready.")
    ];
    private static readonly IReadOnlyCollection<LoadArrSiteSourceResponse> DemoTenantSites =
    [
        new(
            "stl-north",
            "STL North Yard",
            "active",
            true,
            "StaffArr owns this site reference. Location utilization remains unavailable until the warehouse read model is ready.")
    ];
    private static readonly IReadOnlyCollection<LoadArrSiteSourceResponse> SecondaryTenantSites =
    [
        new(
            "south-depot",
            "South Service Depot",
            "active",
            true,
            "StaffArr owns this site reference. Location utilization remains unavailable until the warehouse read model is ready.")
    ];
    private static readonly IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse> DemoTenantItemReferences =
    [
        new(
            "SUP-VALVE-KIT-A",
            "SUP-VALVE-KIT-A",
            "Valve repair kit A",
            "each",
            "maintenance_part",
            false,
            false,
            false,
            false,
            "2026-06-27T09:00:00Z",
            false),
        new(
            "SUP-ADH-49",
            "SUP-ADH-49",
            "Regulated adhesive cartridge",
            "case",
            "regulated_consumable",
            false,
            false,
            false,
            false,
            "2026-06-27T09:05:00Z",
            true)
    ];
    private static readonly IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse> SecondaryTenantItemReferences =
    [
        new(
            "SUP-SOUTH-BRAKE-01",
            "SUP-SOUTH-BRAKE-01",
            "South depot brake kit",
            "each",
            "maintenance_part",
            false,
            false,
            false,
            false,
            "2026-06-27T09:10:00Z",
            false)
    ];

    private WebApplicationFactory<global::LoadArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;
    private FakeLoadArrLocationReferenceService _locationReferences = null!;
    private FakeLoadArrSiteSourceService _siteSources = null!;
    private FakeLoadArrSupplyArrItemReferenceService _itemReferences = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"LoadArrIntegrationAuth-{Guid.NewGuid():N}";
        _locationReferences = new FakeLoadArrLocationReferenceService();
        _locationReferences.SetLocations(DemoTenantId, DemoTenantLocations);
        _locationReferences.SetLocations(SecondaryTenantId, SecondaryTenantLocations);
        _siteSources = new FakeLoadArrSiteSourceService();
        _siteSources.SetSites(DemoTenantId, DemoTenantSites);
        _siteSources.SetSites(SecondaryTenantId, SecondaryTenantSites);
        _itemReferences = new FakeLoadArrSupplyArrItemReferenceService();
        _itemReferences.SetItemReferences(DemoTenantId, DemoTenantItemReferences);
        _itemReferences.SetItemReferences(SecondaryTenantId, SecondaryTenantItemReferences);

        _factory = new WebApplicationFactory<global::LoadArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<LoadArrDbContext>(services);
                services.AddDbContext<LoadArrDbContext>(options => options.UseInMemoryDatabase(dbName));
                services.RemoveAll<ILoadArrSiteSourceService>();
                services.AddSingleton<ILoadArrSiteSourceService>(_siteSources);
                services.RemoveAll<ILoadArrLocationReferenceService>();
                services.AddSingleton<ILoadArrLocationReferenceService>(_locationReferences);
                services.RemoveAll<ILoadArrSupplyArrItemReferenceService>();
                services.AddSingleton<ILoadArrSupplyArrItemReferenceService>(_itemReferences);
            });
        });

        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Integration_read_surfaces_report_dependency_unavailable_for_warehouse_manager_after_non_loadarr_launch_context()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "warehouse_manager");
        string[] paths =
        [
            "/api/v1/integrations/items",
            "/api/v1/integrations/items/item-1",
            "/api/v1/integrations/location-profiles",
            "/api/v1/integrations/location-profiles/lpf-1",
            "/api/v1/integrations/balances",
            "/api/v1/integrations/balances/balance-1",
            "/api/v1/integrations/expected-receipts/exp-1",
            "/api/v1/integrations/stock-movements",
            "/api/v1/integrations/stock-movements/move-1",
            "/api/v1/integrations/receipts/rcpt-1",
            "/api/v1/integrations/reservations/resv-1",
            "/api/v1/integrations/counts/cnt-1"
        ];

        HttpResponseMessage? firstResponse = null;
        foreach (var path in paths)
        {
            var response = await _client.SendAsync(Authorized(HttpMethod.Get, path, token));
            firstResponse ??= response;
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        Assert.NotNull(firstResponse);
        var body = await ReadJsonObjectAsync(firstResponse!);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
    }

    [Fact]
    public async Task Integration_write_surfaces_report_dependency_unavailable_for_warehouse_manager_after_non_loadarr_launch_context()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "warehouse_manager");
        string[] paths =
        [
            "/api/v1/integrations/items",
            "/api/v1/integrations/location-profiles",
            "/api/v1/integrations/availability-checks",
            "/api/v1/integrations/expected-receipts",
            "/api/v1/integrations/expected-receipts/exp-1/status-updates",
            "/api/v1/integrations/receipts",
            "/api/v1/integrations/receipts/rcpt-1/lines",
            "/api/v1/integrations/receipts/rcpt-1/close",
            "/api/v1/integrations/putaway-tasks",
            "/api/v1/integrations/putaway-tasks/pt-1/complete",
            "/api/v1/integrations/reservations",
            "/api/v1/integrations/reservations/resv-1/release",
            "/api/v1/integrations/work-order-demands",
            "/api/v1/integrations/order-demands",
            "/api/v1/integrations/pick-tasks",
            "/api/v1/integrations/pick-tasks/pk-1/complete",
            "/api/v1/integrations/issues",
            "/api/v1/integrations/returns",
            "/api/v1/integrations/transfers",
            "/api/v1/integrations/counts",
            "/api/v1/integrations/counts/cnt-1/lines",
            "/api/v1/integrations/counts/cnt-1/post",
            "/api/v1/integrations/adjustments",
            "/api/v1/integrations/discrepancies",
            "/api/v1/integrations/holds",
            "/api/v1/integrations/hold-releases",
            "/api/v1/integrations/disposition-movements"
        ];

        HttpResponseMessage? firstResponse = null;
        foreach (var path in paths)
        {
            using var request = Authorized(HttpMethod.Post, path, token);
            request.Content = JsonContent.Create(new { });

            var response = await _client.SendAsync(request);
            firstResponse ??= response;
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        Assert.NotNull(firstResponse);
        var body = await ReadJsonObjectAsync(firstResponse!);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
    }

    [Fact]
    public async Task Integration_items_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "tenant_member");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/items", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Integration_item_create_denies_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "warehouse_supervisor");
        using var request = Authorized(HttpMethod.Post, "/api/v1/integrations/items", token);
        request.Content = JsonContent.Create(new
        {
            supplyarrItemId = "item-1",
            itemCode = "SKU-1",
            itemNameSnapshot = "Widget",
            unitOfMeasureSnapshot = "each"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Integration_items_reject_platform_admin_without_loadarr_role()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "tenant_member", isPlatformAdmin: true);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/items", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_summary_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/summary", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_summary_reports_dependency_unavailable_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/summary", token));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_location_references_deny_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/locations", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_location_references_return_staffarr_backed_records_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor", tenantId: DemoTenantId);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/locations?staffarrSiteOrgUnitId=stl-north&locationType=dock&active=true", token));

        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal(1, GetRequiredProperty(body, "total").GetValue<int>());

        var items = body["items"]?.AsArray() ?? throw new InvalidOperationException("Expected location items.");
        var location = items.Single()?.AsObject() ?? throw new InvalidOperationException("Expected one location.");
        Assert.Equal("loc-dock-01", GetRequiredProperty(location, "id").GetValue<string>());
        Assert.Equal("stl-north", GetRequiredProperty(location, "staffarrSiteOrgUnitId").GetValue<string>());
        Assert.Equal("Receiving Dock 1", GetRequiredProperty(location, "name").GetValue<string>());
        Assert.Equal("dock", GetRequiredProperty(location, "locationType").GetValue<string>());
        Assert.True(GetRequiredProperty(location, "active").GetValue<bool>());
    }

    [Fact]
    public async Task Workspace_location_reference_detail_returns_not_found_for_unknown_location()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor", tenantId: DemoTenantId);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/locations/loc-missing-01", token));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_location_tree_is_scoped_by_tenant()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor", tenantId: SecondaryTenantId);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/locations/tree", token));

        response.EnsureSuccessStatusCode();
        var nodes = (await JsonNode.ParseAsync(await response.Content.ReadAsStreamAsync()))?.AsArray()
            ?? throw new InvalidOperationException("Expected location tree nodes.");
        var siteNode = nodes.Single()?.AsObject() ?? throw new InvalidOperationException("Expected one site node.");
        Assert.Equal("South Service Depot", GetRequiredProperty(siteNode, "label").GetValue<string>());
        var children = siteNode["children"]?.AsArray() ?? throw new InvalidOperationException("Expected child nodes.");
        var firstChild = children.Single()?.AsObject() ?? throw new InvalidOperationException("Expected one child node.");
        Assert.Equal("loc-south-truck-17", GetRequiredProperty(firstChild, "locationId").GetValue<string>());
        Assert.DoesNotContain(children, child => string.Equals(child?["locationId"]?.GetValue<string>(), "loc-dock-01", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Workspace_site_sources_deny_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/site-sources", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_site_sources_return_staffarr_backed_records_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor", tenantId: DemoTenantId);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/site-sources", token));

        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal(1, GetRequiredProperty(body, "total").GetValue<int>());

        var items = body["items"]?.AsArray() ?? throw new InvalidOperationException("Expected site sources.");
        var item = items.Single()?.AsObject() ?? throw new InvalidOperationException("Expected one site source.");
        Assert.Equal("stl-north", GetRequiredProperty(item, "staffarrSiteOrgUnitId").GetValue<string>());
        Assert.Equal("STL North Yard", GetRequiredProperty(item, "staffarrSiteNameSnapshot").GetValue<string>());
        Assert.Equal("active", GetRequiredProperty(item, "status").GetValue<string>());
        Assert.True(GetRequiredProperty(item, "active").GetValue<bool>());
    }

    [Fact]
    public async Task Workspace_site_sources_are_scoped_by_tenant()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor", tenantId: SecondaryTenantId);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/site-sources", token));

        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal(1, GetRequiredProperty(body, "total").GetValue<int>());

        var items = body["items"]?.AsArray() ?? throw new InvalidOperationException("Expected site sources.");
        var item = items.Single()?.AsObject() ?? throw new InvalidOperationException("Expected one site source.");
        Assert.Equal("south-depot", GetRequiredProperty(item, "staffarrSiteOrgUnitId").GetValue<string>());
        Assert.DoesNotContain(items, candidate =>
            string.Equals(candidate?["staffarrSiteOrgUnitId"]?.GetValue<string>(), "stl-north", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Workspace_site_sources_report_dependency_unavailable_when_staffarr_lookup_fails()
    {
        _siteSources.Failure = new STLCompliance.Shared.Contracts.StlApiException(
            "staffarr.sites.lookup_failed",
            "StaffArr lookup is unavailable.",
            503);
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/site-sources", token));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
    }

    [Fact]
    public async Task Workspace_supplyarr_item_references_deny_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/supplyarr-item-references", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Workspace_supplyarr_item_references_return_supplyarr_backed_records_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor", tenantId: DemoTenantId);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/supplyarr-item-references?query=adh", token));

        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal(1, GetRequiredProperty(body, "total").GetValue<int>());

        var items = body["items"]?.AsArray() ?? throw new InvalidOperationException("Expected item references.");
        var item = items.Single()?.AsObject() ?? throw new InvalidOperationException("Expected one item reference.");
        Assert.Equal("SUP-ADH-49", GetRequiredProperty(item, "supplyarrItemId").GetValue<string>());
        Assert.Equal("Regulated adhesive cartridge", GetRequiredProperty(item, "itemNameSnapshot").GetValue<string>());
        Assert.Equal("case", GetRequiredProperty(item, "unitOfMeasureSnapshot").GetValue<string>());
        Assert.Equal("regulated_consumable", GetRequiredProperty(item, "itemTypeSnapshot").GetValue<string>());
        Assert.True(GetRequiredProperty(item, "requiresTraceabilityCapture").GetValue<bool>());
    }

    [Fact]
    public async Task Workspace_supplyarr_item_references_are_scoped_by_tenant()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor", tenantId: SecondaryTenantId);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/supplyarr-item-references", token));

        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal(1, GetRequiredProperty(body, "total").GetValue<int>());

        var items = body["items"]?.AsArray() ?? throw new InvalidOperationException("Expected item references.");
        var item = items.Single()?.AsObject() ?? throw new InvalidOperationException("Expected one item reference.");
        Assert.Equal("SUP-SOUTH-BRAKE-01", GetRequiredProperty(item, "supplyarrItemId").GetValue<string>());
        Assert.DoesNotContain(items, candidate =>
            string.Equals(candidate?["supplyarrItemId"]?.GetValue<string>(), "SUP-VALVE-KIT-A", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Workspace_supplyarr_item_references_report_dependency_unavailable_when_supplyarr_lookup_fails()
    {
        _itemReferences.Failure = new STLCompliance.Shared.Contracts.StlApiException(
            "supplyarr.item_references.lookup_failed",
            "SupplyArr lookup is unavailable.",
            503);
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor", tenantId: DemoTenantId);

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/workspace/supplyarr-item-references", token));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
    }

    [Fact]
    public async Task Receiving_completion_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");
        using var request = Authorized(HttpMethod.Post, "/api/v1/receiving/recv-24018/complete", token);
        request.Content = JsonContent.Create(new
        {
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-24018",
            supplierNameSnapshot = "Midwest Freight Supply",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 4m,
            receivedQuantity = 4m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "pending_inspection",
            discrepancyReasonCode = (string?)null,
            complianceEvaluationId = "cc-eval-receiving-24018",
            evidenceSummary = "Received and staged for putaway"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Receiving_cancel_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");
        using var request = Authorized(HttpMethod.Post, "/api/v1/receiving/recv-24018/cancel", token);
        request.Content = JsonContent.Create(new
        {
            canceledByPersonId = "person-inventory-supervisor",
            reasonCode = "supplier_rejected",
            notes = "Cancel until authoritative cancellation audit persistence exists."
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Receiving_cancel_dependency_unavailable_does_not_mutate_existing_session()
    {
        const string sessionId = "recv-r0-no-cancel";
        await SeedReceivingSessionAsync(
            DemoTenantId,
            new LoadArrReceivingSessionResponse(
                sessionId,
                "RCV-R0-NO-CANCEL",
                "purchase_order",
                "open",
                "staff-site-stl-north",
                "STL North Yard",
                "supplyarr",
                "purchase_order",
                "PO-R0-NO-CANCEL",
                "Midwest Fleet Supply",
                "person-inventory-clerk",
                null,
                "2026-06-27T10:00:00Z",
                null,
                new[]
                {
                    new LoadArrReceivingLineResponse(
                        "line-r0-no-cancel",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        4m,
                        0m,
                        "each",
                        "loc-dock-01",
                        "Receiving Dock 1",
                        "L2405-77",
                        null,
                        "new",
                        "ready_to_complete",
                        null,
                        "Awaiting completion")
                }));

        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        using var request = Authorized(HttpMethod.Post, $"/api/v1/receiving/{sessionId}/cancel", token);
        request.Content = JsonContent.Create(new
        {
            canceledByPersonId = "person-inventory-supervisor",
            reasonCode = "supplier_rejected",
            notes = "Cancel until authoritative cancellation audit persistence exists."
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());

        var entity = await FindReceivingSessionRowAsync(DemoTenantId, sessionId);
        Assert.NotNull(entity);
        Assert.Equal("open", entity!.Status);
        Assert.Null(entity.CompletedByPersonId);
        Assert.Null(entity.CompletedAtUtc);
    }

    [Fact]
    public async Task Receiving_list_and_detail_return_persisted_sessions_and_isolate_tenant()
    {
        const string sessionId = "recv-persisted-01";
        await SeedReceivingSessionAsync(
            DemoTenantId,
            new LoadArrReceivingSessionResponse(
                sessionId,
                "RCV-260627-001",
                "purchase_order",
                "completed",
                "staff-site-stl-north",
                "STL North Yard",
                "supplyarr",
                "purchase_order",
                "PO-24018",
                "Midwest Freight Supply",
                "person-inventory-clerk",
                "person-inventory-supervisor",
                "2026-06-27T09:00:00Z",
                "2026-06-27T09:15:00Z",
                new[]
                {
                    new LoadArrReceivingLineResponse(
                        "line-persisted-01",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        4m,
                        4m,
                        "each",
                        "loc-dock-01",
                        "Receiving Dock 1",
                        "L2405-77",
                        null,
                        "pending_inspection",
                        "ready_to_complete",
                        null,
                        "Received and staged for putaway")
                }));

        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);

        var getResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/receiving/{sessionId}", token));
        getResponse.EnsureSuccessStatusCode();
        var storedSession = await ReadJsonObjectAsync(getResponse);

        Assert.Equal("completed", GetRequiredProperty(storedSession, "status").GetValue<string>());
        Assert.Equal("RCV-260627-001", GetRequiredProperty(storedSession, "receivingNumber").GetValue<string>());

        var listResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/receiving", token));
        listResponse.EnsureSuccessStatusCode();
        var listBody = await ReadJsonObjectAsync(listResponse);
        Assert.Contains(
            GetRequiredProperty(listBody, "items").AsArray(),
            item => string.Equals(item?["id"]?.GetValue<string>(), sessionId, StringComparison.OrdinalIgnoreCase));

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LoadArrDbContext>();
            var entity = await db.LoadArrReceivingSessions.SingleAsync(
                x => x.TenantId == DemoTenantId && x.SessionId == sessionId);
            Assert.Equal("completed", entity.Status);
        }

        var otherTenantToken = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: SecondaryTenantId);
        var otherTenantResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/receiving/{sessionId}", otherTenantToken));

        Assert.Equal(HttpStatusCode.NotFound, otherTenantResponse.StatusCode);
    }

    [Fact]
    public async Task Receiving_draft_complete_reports_dependency_unavailable()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        using var request = Authorized(HttpMethod.Post, "/api/v1/receiving/draft/complete", token);
        request.Content = JsonContent.Create(new
        {
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-24018",
            supplierNameSnapshot = "Midwest Freight Supply",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 4m,
            receivedQuantity = 4m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "pending_inspection",
            discrepancyReasonCode = (string?)null,
            complianceEvaluationId = "cc-eval-receiving-24018",
            evidenceSummary = "Received and staged for putaway"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Receiving_list_starts_empty_without_fixture_operational_records()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/receiving", token));

        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);
        var items = GetRequiredProperty(body, "items").AsArray();

        Assert.Empty(items);
        Assert.Equal(0, GetRequiredProperty(body, "total").GetValue<int>());
    }

    [Fact]
    public async Task Receiving_complete_returns_not_found_for_missing_session()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        using var request = Authorized(HttpMethod.Post, "/api/v1/receiving/recv-24018/complete", token);
        request.Content = JsonContent.Create(new
        {
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-24018",
            supplierNameSnapshot = "Midwest Freight Supply",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 4m,
            receivedQuantity = 4m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "pending_inspection",
            discrepancyReasonCode = (string?)null,
            complianceEvaluationId = "cc-eval-receiving-24018",
            evidenceSummary = "Received and staged for putaway"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_completion_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");
        using var request = Authorized(HttpMethod.Post, "/api/v1/transfers/xfer-24018/complete", token);
        request.Content = JsonContent.Create(new
        {
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 4m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection",
            complianceEvaluationId = "cc-eval-transfer-24018",
            evidenceSummary = "Approved dock-to-quarantine transfer"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_cancel_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");
        using var request = Authorized(HttpMethod.Post, "/api/v1/transfers/xfer-24018/cancel", token);
        request.Content = JsonContent.Create(new
        {
            canceledByPersonId = "person-inventory-supervisor",
            reasonCode = "destination_closed",
            notes = "Cancel until authoritative cancellation audit persistence exists."
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_cancel_dependency_unavailable_does_not_mutate_existing_order()
    {
        const string orderId = "xfer-r0-no-cancel";
        await SeedTransferOrderAsync(
            DemoTenantId,
            new LoadArrTransferOrderResponse(
                orderId,
                "TRF-R0-NO-CANCEL",
                "draft",
                "bin_to_bin",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "loc-quarantine-01",
                "Quarantine Bay",
                "person-inventory-clerk",
                null,
                "quality_inspection",
                "2026-06-27T10:15:00Z",
                null,
                new[]
                {
                    new LoadArrTransferLineResponse(
                        "xfer-line-r0-no-cancel",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        4m,
                        "each",
                        "L2405-77",
                        null,
                        "draft")
                }));

        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        using var request = Authorized(HttpMethod.Post, $"/api/v1/transfers/{orderId}/cancel", token);
        request.Content = JsonContent.Create(new
        {
            canceledByPersonId = "person-inventory-supervisor",
            reasonCode = "destination_closed",
            notes = "Cancel until authoritative cancellation audit persistence exists."
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());

        var entity = await FindTransferOrderRowAsync(DemoTenantId, orderId);
        Assert.NotNull(entity);
        Assert.Equal("draft", entity!.Status);
        Assert.Null(entity.CompletedByPersonId);
        Assert.Null(entity.CompletedAtUtc);
    }

    [Fact]
    public async Task Transfer_list_and_detail_return_persisted_orders_and_isolate_tenant()
    {
        const string transferId = "xfer-persisted-01";
        await SeedTransferOrderAsync(
            DemoTenantId,
            new LoadArrTransferOrderResponse(
                transferId,
                "TRF-260627-001",
                "completed",
                "bin_to_bin",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "loc-quarantine-01",
                "Quarantine Bay",
                "person-inventory-clerk",
                "person-inventory-supervisor",
                "quality_inspection",
                "2026-06-27T09:30:00Z",
                "2026-06-27T09:45:00Z",
                new[]
                {
                    new LoadArrTransferLineResponse(
                        "xfer-line-persisted-01",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        4m,
                        "each",
                        "L2405-77",
                        null,
                        "ready")
                }));

        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);

        var getResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/transfers/{transferId}", token));
        getResponse.EnsureSuccessStatusCode();
        var storedTransfer = await ReadJsonObjectAsync(getResponse);

        Assert.Equal("completed", GetRequiredProperty(storedTransfer, "status").GetValue<string>());
        Assert.Equal("TRF-260627-001", GetRequiredProperty(storedTransfer, "transferNumber").GetValue<string>());

        var listResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/transfers", token));
        listResponse.EnsureSuccessStatusCode();
        var listBody = await ReadJsonObjectAsync(listResponse);
        Assert.Contains(
            GetRequiredProperty(listBody, "items").AsArray(),
            item => string.Equals(item?["id"]?.GetValue<string>(), transferId, StringComparison.OrdinalIgnoreCase));

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LoadArrDbContext>();
            var entity = await db.LoadArrTransferOrders.SingleAsync(
                x => x.TenantId == DemoTenantId && x.OrderId == transferId);
            Assert.Equal("completed", entity.Status);
        }

        var otherTenantToken = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: SecondaryTenantId);
        var otherTenantResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/transfers/{transferId}", otherTenantToken));

        Assert.Equal(HttpStatusCode.NotFound, otherTenantResponse.StatusCode);
    }

    [Fact]
    public async Task Transfer_draft_complete_reports_dependency_unavailable()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        using var request = Authorized(HttpMethod.Post, "/api/v1/transfers/draft/complete", token);
        request.Content = JsonContent.Create(new
        {
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 4m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection",
            complianceEvaluationId = "cc-eval-transfer-24018",
            evidenceSummary = "Approved dock-to-quarantine transfer"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_list_starts_empty_without_fixture_operational_records()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/transfers", token));

        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);
        var items = GetRequiredProperty(body, "items").AsArray();

        Assert.Empty(items);
        Assert.Equal(0, GetRequiredProperty(body, "total").GetValue<int>());
    }

    [Fact]
    public async Task Transfer_complete_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        using var request = Authorized(HttpMethod.Post, "/api/v1/transfers/xfer-24018/complete", token);
        request.Content = JsonContent.Create(new
        {
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 4m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection",
            complianceEvaluationId = "cc-eval-transfer-24018",
            evidenceSummary = "Approved dock-to-quarantine transfer"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Count_list_reports_dependency_unavailable_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/counts", token));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Count_create_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");

        using var createRequest = Authorized(HttpMethod.Post, "/api/v1/counts", token);
        createRequest.Content = JsonContent.Create(new
        {
            countType = "cycle_count",
            staffarrSiteOrgUnitId = "staff-site-south-depot",
            warehouseLocationId = "loc-truck-17",
            supplyarrItemId = "SUP-BR-ROTOR-22",
            expectedQuantity = 10m,
            countedByPersonId = "person-route-stock-lead",
            reasonCode = "cycle_count_variance",
            evidenceSummary = "Cycle count variance above mobile-stock threshold"
        });

        var createResponse = await _client.SendAsync(createRequest);

        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    [Fact]
    public async Task Count_create_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var createRequest = Authorized(HttpMethod.Post, "/api/v1/counts", token);
        createRequest.Content = JsonContent.Create(new
        {
            countType = "cycle_count",
            staffarrSiteOrgUnitId = "staff-site-south-depot",
            warehouseLocationId = "loc-truck-17",
            supplyarrItemId = "SUP-BR-ROTOR-22",
            expectedQuantity = 10m,
            countedByPersonId = "person-route-stock-lead",
            reasonCode = "cycle_count_variance",
            evidenceSummary = "Cycle count variance above mobile-stock threshold"
        });

        var createResponse = await _client.SendAsync(createRequest);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, createResponse.StatusCode);
    }

    [Fact]
    public async Task Count_complete_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/counts/count-dock-01-verified/complete", token);
        request.Content = JsonContent.Create(new
        {
            countType = "cycle_count",
            staffarrSiteOrgUnitId = "staff-site-stl-north",
            warehouseLocationId = "loc-dock-01",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 10m,
            countedQuantity = 10m,
            countedByPersonId = "person-inventory-clerk",
            reasonCode = "cycle_count_verified",
            evidenceSummary = "Completed dock count with no variance",
            complianceEvaluationId = (string?)null
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Count_variance_approval_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var approveRequest = Authorized(HttpMethod.Post, "/api/v1/counts/count-dock-01-verified/approve-variance", token);
        approveRequest.Content = JsonContent.Create(new
        {
            countType = "cycle_count",
            staffarrSiteOrgUnitId = "staff-site-south-depot",
            warehouseLocationId = "loc-truck-17",
            supplyarrItemId = "SUP-BR-ROTOR-22",
            expectedQuantity = 10m,
            countedQuantity = 12m,
            approvedByPersonId = "person-route-stock-supervisor",
            reasonCode = "cycle_count_variance",
            evidenceSummary = "Supervisor approved positive variance",
            complianceEvaluationId = "cc-eval-8021"
        });

        var approveResponse = await _client.SendAsync(approveRequest);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, approveResponse.StatusCode);
    }

    [Fact]
    public async Task Adjustment_list_reports_dependency_unavailable_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/adjustments", token));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Adjustment_create_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");

        using var request = Authorized(HttpMethod.Post, "/api/v1/adjustments", token);
        request.Content = JsonContent.Create(new
        {
            adjustmentType = "gain",
            warehouseLocationId = "loc-dock-01",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantityDelta = 2m,
            createdByPersonId = "person-inventory-clerk",
            reasonCode = "cycle_count_variance",
            evidenceSummary = "Manual gain request"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Adjustment_create_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/adjustments", token);
        request.Content = JsonContent.Create(new
        {
            adjustmentType = "gain",
            warehouseLocationId = "loc-dock-01",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantityDelta = 2m,
            createdByPersonId = "person-inventory-clerk",
            reasonCode = "cycle_count_variance",
            evidenceSummary = "Manual gain request"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Adjustment_approval_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/adjustments/adj-cycle-variance-01/approve", token);
        request.Content = JsonContent.Create(new
        {
            adjustmentType = "gain",
            staffarrSiteOrgUnitId = "staff-site-stl-north",
            warehouseLocationId = "loc-dock-01",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantityDelta = 2m,
            createdByPersonId = "person-inventory-clerk",
            approvedByPersonId = "person-inventory-supervisor",
            reasonCode = "cycle_count_variance",
            evidenceSummary = "Approved after recount",
            complianceEvaluationId = (string?)null
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Truck_stock_list_reports_dependency_unavailable_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/truck-stock", token));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Truck_stock_issue_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");

        using var request = Authorized(HttpMethod.Post, "/api/v1/truck-stock/truck-stock-17-kit/issue", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 2m,
            reasonCode = "route_replenishment",
            evidenceSummary = "Issued for route kit replenishment"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Truck_stock_issue_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/truck-stock/truck-stock-17-kit/issue", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 2m,
            reasonCode = "route_replenishment",
            evidenceSummary = "Issued for route kit replenishment"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_list_reports_dependency_unavailable_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/kits", token));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_replenish_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-ppe-hazmat-04/replenish", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-hazmat-reviewer",
            quantity = 1m,
            reasonCode = "inspection_restock",
            evidenceSummary = "Replenished after inspection"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Kit_replenish_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-ppe-hazmat-04/replenish", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-hazmat-reviewer",
            quantity = 1m,
            reasonCode = "inspection_restock",
            evidenceSummary = "Replenished after inspection"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_create_persists_draft_with_authoritative_references()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/transfers", token);
        createRequest.Content = JsonContent.Create(new
        {
            clientRequestId = "xfer-create-001",
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            requestedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 4m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection"
        });

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var body = await ReadJsonObjectAsync(createResponse);
        var orderId = GetRequiredProperty(body, "id").GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(orderId));
        Assert.Equal("draft", GetRequiredProperty(body, "status").GetValue<string>());
        Assert.Equal("person-inventory-clerk", GetRequiredProperty(body, "requestedByPersonId").GetValue<string>());
        Assert.Equal("Receiving Dock 1", GetRequiredProperty(body, "fromLocationNameSnapshot").GetValue<string>());
        Assert.Equal("Quarantine Bay", GetRequiredProperty(body, "toLocationNameSnapshot").GetValue<string>());

        var lines = GetRequiredProperty(body, "lines").AsArray();
        var line = lines.Single()?.AsObject() ?? throw new InvalidOperationException("Expected one transfer line.");
        Assert.Equal("SUP-VALVE-KIT-A", GetRequiredProperty(line, "supplyarrItemId").GetValue<string>());
        Assert.Equal("each", GetRequiredProperty(line, "unitOfMeasure").GetValue<string>());

        var persisted = await FindTransferOrderRowAsync(DemoTenantId, orderId);
        Assert.NotNull(persisted);
        Assert.Equal("draft", persisted!.Status);
        Assert.Null(persisted.CompletedAtUtc);

        var otherTenantResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/transfers/{orderId}", CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: SecondaryTenantId)));
        Assert.Equal(HttpStatusCode.NotFound, otherTenantResponse.StatusCode);
    }

    [Fact]
    public async Task Transfer_create_reuses_existing_draft_when_client_request_id_is_retried()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");
        var beforeCount = await CountTransferOrderRowsAsync(DemoTenantId);

        using var firstRequest = Authorized(HttpMethod.Post, "/api/v1/transfers", token);
        firstRequest.Content = JsonContent.Create(new
        {
            clientRequestId = "xfer-retry-001",
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            requestedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 4m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection"
        });

        var firstResponse = await _client.SendAsync(firstRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        var firstBody = await ReadJsonObjectAsync(firstResponse);
        var firstOrderId = GetRequiredProperty(firstBody, "id").GetValue<string>();

        using var secondRequest = Authorized(HttpMethod.Post, "/api/v1/transfers", token);
        secondRequest.Content = JsonContent.Create(new
        {
            clientRequestId = "xfer-retry-001",
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            requestedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 4m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection"
        });

        var secondResponse = await _client.SendAsync(secondRequest);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var secondBody = await ReadJsonObjectAsync(secondResponse);
        Assert.Equal(firstOrderId, GetRequiredProperty(secondBody, "id").GetValue<string>());
        Assert.Equal(beforeCount + 1, await CountTransferOrderRowsAsync(DemoTenantId));
    }

    [Fact]
    public async Task Transfer_create_conflicts_when_client_request_id_is_reused_for_different_payload()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");
        var beforeCount = await CountTransferOrderRowsAsync(DemoTenantId);

        using var firstRequest = Authorized(HttpMethod.Post, "/api/v1/transfers", token);
        firstRequest.Content = JsonContent.Create(new
        {
            clientRequestId = "xfer-conflict-001",
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            requestedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 4m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection"
        });

        var firstResponse = await _client.SendAsync(firstRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var secondRequest = Authorized(HttpMethod.Post, "/api/v1/transfers", token);
        secondRequest.Content = JsonContent.Create(new
        {
            clientRequestId = "xfer-conflict-001",
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            requestedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 6m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection"
        });

        var secondResponse = await _client.SendAsync(secondRequest);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        var secondBody = await ReadJsonObjectAsync(secondResponse);
        Assert.Equal("client_request_conflict", GetRequiredProperty(secondBody, "errorCode").GetValue<string>());
        Assert.Equal(beforeCount + 1, await CountTransferOrderRowsAsync(DemoTenantId));
    }

    [Fact]
    public async Task Transfer_create_reports_dependency_unavailable_when_location_lookup_fails()
    {
        _locationReferences.Failure = new STLCompliance.Shared.Contracts.StlApiException(
            "staffarr.locations.lookup_failed",
            "StaffArr location lookup is unavailable.",
            503);
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/transfers", token);
        createRequest.Content = JsonContent.Create(new
        {
            clientRequestId = "xfer-create-lookup-fail-001",
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            requestedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 4m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection"
        });

        var response = await _client.SendAsync(createRequest);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
    }

    [Fact]
    public async Task Transfer_draft_complete_dependency_unavailable_does_not_create_order()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        var beforeCount = await CountTransferOrderRowsAsync(DemoTenantId);

        using var request = Authorized(HttpMethod.Post, "/api/v1/transfers/draft/complete", token);
        request.Content = JsonContent.Create(new
        {
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 4m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection",
            complianceEvaluationId = "cc-eval-transfer-24018",
            evidenceSummary = "Approved dock-to-quarantine transfer"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
        Assert.Equal(beforeCount, await CountTransferOrderRowsAsync(DemoTenantId));
    }

    [Fact]
    public async Task Transfer_complete_dependency_unavailable_does_not_mutate_existing_order()
    {
        const string orderId = "xfer-r0-no-mutate";
        await SeedTransferOrderAsync(
            DemoTenantId,
            new LoadArrTransferOrderResponse(
                orderId,
                "TRF-R0-NO-MUTATE",
                "ready",
                "bin_to_bin",
                "staff-site-stl-north",
                "STL North Yard",
                "loc-dock-01",
                "Receiving Dock 1",
                "loc-quarantine-01",
                "Quarantine Bay",
                "person-inventory-clerk",
                null,
                "quality_inspection",
                "2026-06-27T10:15:00Z",
                null,
                new[]
                {
                    new LoadArrTransferLineResponse(
                        "xfer-line-r0-no-mutate",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        4m,
                        "each",
                        "L2405-77",
                        null,
                        "ready")
                }));

        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        var beforeCount = await CountTransferOrderRowsAsync(DemoTenantId);

        using var request = Authorized(HttpMethod.Post, $"/api/v1/transfers/{orderId}/complete", token);
        request.Content = JsonContent.Create(new
        {
            transferType = "bin_to_bin",
            fromLocationId = "loc-dock-01",
            toLocationId = "loc-quarantine-01",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 4m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            reasonCode = "quality_inspection",
            complianceEvaluationId = "cc-eval-transfer-24018",
            evidenceSummary = "Approved dock-to-quarantine transfer"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(beforeCount, await CountTransferOrderRowsAsync(DemoTenantId));

        var entity = await FindTransferOrderRowAsync(DemoTenantId, orderId);
        Assert.NotNull(entity);
        Assert.Equal("ready", entity!.Status);
        Assert.Null(entity.CompletedByPersonId);
        Assert.Null(entity.CompletedAtUtc);
    }

    [Fact]
    public async Task Hold_list_reports_dependency_unavailable_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/holds", token));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Hold_create_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");
        using var request = Authorized(HttpMethod.Post, "/api/v1/holds", token);
        request.Content = JsonContent.Create(new
        {
            holdType = "quality",
            warehouseLocationId = "loc-dock-01",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 2m,
            reasonCode = "sds_label_mismatch",
            description = "Hold for label review",
            createdByPersonId = "person-hazmat-reviewer",
            complianceEvaluationId = (string?)null,
            evidenceSummary = "Flagged during dock inspection"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Hold_create_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/holds", token);
        request.Content = JsonContent.Create(new
        {
            holdType = "quality",
            warehouseLocationId = "loc-dock-01",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            quantity = 2m,
            reasonCode = "sds_label_mismatch",
            description = "Hold for label review",
            createdByPersonId = "person-hazmat-reviewer",
            complianceEvaluationId = (string?)null,
            evidenceSummary = "Flagged during dock inspection"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Hold_release_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/holds/hold-adh-49/release", token);
        request.Content = JsonContent.Create(new
        {
            releasedByPersonId = "person-hazmat-reviewer",
            reasonCode = "sds_label_mismatch_resolved",
            evidenceSummary = "Compliance review cleared the hold"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Unexplained_inventory_list_reports_dependency_unavailable_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/unexplained-inventory", token));

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Unexplained_inventory_create_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/unexplained-inventory", token);
        request.Content = JsonContent.Create(new
        {
            discoverySource = "dock_found_stock",
            warehouseLocationId = "loc-dock-01",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 3m,
            quantity = 5m,
            lotCode = "L2405-77",
            serialCode = (string?)null,
            discoveredByPersonId = "person-inventory-clerk",
            reasonCode = "unknown_origin_review",
            evidenceSummary = "Found during dock walkthrough",
            complianceEvaluationId = (string?)null
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Unexplained_inventory_resolve_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/unexplained-inventory/unexplained-count-8021/resolve", token);
        request.Content = JsonContent.Create(new
        {
            approvedByPersonId = "person-route-stock-supervisor",
            reasonCode = "variance_approved",
            complianceEvaluationId = "cc-eval-unx-8021",
            evidenceSummary = "Supervisor approved the variance"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Unexplained_inventory_quarantine_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/unexplained-inventory/unexplained-dock-adh/quarantine", token);
        request.Content = JsonContent.Create(new
        {
            quarantineLocationId = "loc-quarantine-01",
            quarantinedByPersonId = "person-inventory-clerk",
            reasonCode = "damaged_freight_review",
            evidenceSummary = "Moved to quarantine for review"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Unexplained_inventory_scrap_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/unexplained-inventory/unexplained-dock-adh/scrap", token);
        request.Content = JsonContent.Create(new
        {
            scrappedByPersonId = "person-inventory-clerk",
            reasonCode = "damaged_beyond_recovery",
            evidenceSummary = "Disposed after damage review"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Receiving_create_persists_draft_with_authoritative_references()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/receiving", token);
        request.Content = JsonContent.Create(new
        {
            clientRequestId = "recv-create-001",
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-10492",
            supplierNameSnapshot = "Midwest Fleet Supply",
            startedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 38m,
            receivedQuantity = 38m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "new",
            discrepancyReasonCode = (string?)null,
            evidenceSummary = "Dock receipt photo attached"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        var sessionId = GetRequiredProperty(body, "id").GetValue<string>();
        Assert.False(string.IsNullOrWhiteSpace(sessionId));
        Assert.Equal("open", GetRequiredProperty(body, "status").GetValue<string>());
        Assert.Equal("person-inventory-clerk", GetRequiredProperty(body, "startedByPersonId").GetValue<string>());
        Assert.Equal("STL North Yard", GetRequiredProperty(body, "staffarrSiteNameSnapshot").GetValue<string>());

        var lines = GetRequiredProperty(body, "lines").AsArray();
        var line = lines.Single()?.AsObject() ?? throw new InvalidOperationException("Expected one receiving line.");
        Assert.Equal("SUP-VALVE-KIT-A", GetRequiredProperty(line, "supplyarrItemId").GetValue<string>());
        Assert.Equal("each", GetRequiredProperty(line, "unitOfMeasure").GetValue<string>());
        Assert.Equal("Receiving Dock 1", GetRequiredProperty(line, "locationNameSnapshot").GetValue<string>());

        var persisted = await FindReceivingSessionRowAsync(DemoTenantId, sessionId);
        Assert.NotNull(persisted);
        Assert.Equal("open", persisted!.Status);
        Assert.Null(persisted.CompletedAtUtc);

        var otherTenantResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/receiving/{sessionId}", CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: SecondaryTenantId)));
        Assert.Equal(HttpStatusCode.NotFound, otherTenantResponse.StatusCode);
    }

    [Fact]
    public async Task Receiving_create_reuses_existing_draft_when_client_request_id_is_retried()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");
        var beforeCount = await CountReceivingSessionRowsAsync(DemoTenantId);

        using var firstRequest = Authorized(HttpMethod.Post, "/api/v1/receiving", token);
        firstRequest.Content = JsonContent.Create(new
        {
            clientRequestId = "recv-retry-001",
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-10492",
            supplierNameSnapshot = "Midwest Fleet Supply",
            startedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 38m,
            receivedQuantity = 38m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "new",
            discrepancyReasonCode = (string?)null,
            evidenceSummary = "Dock receipt photo attached"
        });

        var firstResponse = await _client.SendAsync(firstRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        var firstBody = await ReadJsonObjectAsync(firstResponse);
        var firstSessionId = GetRequiredProperty(firstBody, "id").GetValue<string>();

        using var secondRequest = Authorized(HttpMethod.Post, "/api/v1/receiving", token);
        secondRequest.Content = JsonContent.Create(new
        {
            clientRequestId = "recv-retry-001",
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-10492",
            supplierNameSnapshot = "Midwest Fleet Supply",
            startedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 38m,
            receivedQuantity = 38m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "new",
            discrepancyReasonCode = (string?)null,
            evidenceSummary = "Dock receipt photo attached"
        });

        var secondResponse = await _client.SendAsync(secondRequest);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var secondBody = await ReadJsonObjectAsync(secondResponse);
        Assert.Equal(firstSessionId, GetRequiredProperty(secondBody, "id").GetValue<string>());
        Assert.Equal(beforeCount + 1, await CountReceivingSessionRowsAsync(DemoTenantId));
    }

    [Fact]
    public async Task Receiving_create_conflicts_when_client_request_id_is_reused_for_different_payload()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");
        var beforeCount = await CountReceivingSessionRowsAsync(DemoTenantId);

        using var firstRequest = Authorized(HttpMethod.Post, "/api/v1/receiving", token);
        firstRequest.Content = JsonContent.Create(new
        {
            clientRequestId = "recv-conflict-001",
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-10492",
            supplierNameSnapshot = "Midwest Fleet Supply",
            startedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 38m,
            receivedQuantity = 38m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "new",
            discrepancyReasonCode = (string?)null,
            evidenceSummary = "Dock receipt photo attached"
        });

        var firstResponse = await _client.SendAsync(firstRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        using var secondRequest = Authorized(HttpMethod.Post, "/api/v1/receiving", token);
        secondRequest.Content = JsonContent.Create(new
        {
            clientRequestId = "recv-conflict-001",
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-10492",
            supplierNameSnapshot = "Midwest Fleet Supply",
            startedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 38m,
            receivedQuantity = 36m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "new",
            discrepancyReasonCode = (string?)null,
            evidenceSummary = "Dock receipt photo attached"
        });

        var secondResponse = await _client.SendAsync(secondRequest);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        var secondBody = await ReadJsonObjectAsync(secondResponse);
        Assert.Equal("client_request_conflict", GetRequiredProperty(secondBody, "errorCode").GetValue<string>());
        Assert.Equal(beforeCount + 1, await CountReceivingSessionRowsAsync(DemoTenantId));
    }

    [Fact]
    public async Task Receiving_create_validation_failure_does_not_persist_session()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");
        var beforeCount = await CountReceivingSessionRowsAsync(DemoTenantId);

        var request = Authorized(HttpMethod.Post, "/api/v1/receiving", token);
        request.Content = JsonContent.Create(new
        {
            clientRequestId = "recv-create-invalid-location-001",
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-10492",
            supplierNameSnapshot = "Midwest Fleet Supply",
            startedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 38m,
            receivedQuantity = 38m,
            warehouseLocationId = "loc-missing-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "new",
            discrepancyReasonCode = (string?)null,
            evidenceSummary = "Dock receipt photo attached"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("invalid_receiving_location", GetRequiredProperty(body, "errorCode").GetValue<string>());
        Assert.Equal(beforeCount, await CountReceivingSessionRowsAsync(DemoTenantId));
    }

    [Fact]
    public async Task Receiving_create_reports_dependency_unavailable_when_item_lookup_fails()
    {
        _itemReferences.Failure = new STLCompliance.Shared.Contracts.StlApiException(
            "supplyarr.item_references.lookup_failed",
            "SupplyArr lookup is unavailable.",
            503);
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/receiving", token);
        request.Content = JsonContent.Create(new
        {
            clientRequestId = "recv-create-lookup-fail-001",
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-10492",
            supplierNameSnapshot = "Midwest Fleet Supply",
            startedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 38m,
            receivedQuantity = 38m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "new",
            discrepancyReasonCode = (string?)null,
            evidenceSummary = "Dock receipt photo attached"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
    }

    [Fact]
    public async Task Receiving_draft_complete_dependency_unavailable_does_not_create_session()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        var beforeCount = await CountReceivingSessionRowsAsync(DemoTenantId);

        using var request = Authorized(HttpMethod.Post, "/api/v1/receiving/draft/complete", token);
        request.Content = JsonContent.Create(new
        {
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-24018",
            supplierNameSnapshot = "Midwest Freight Supply",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 4m,
            receivedQuantity = 4m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "pending_inspection",
            discrepancyReasonCode = (string?)null,
            complianceEvaluationId = "cc-eval-receiving-24018",
            evidenceSummary = "Received and staged for putaway"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
        Assert.Equal(beforeCount, await CountReceivingSessionRowsAsync(DemoTenantId));
    }

    [Fact]
    public async Task Receiving_complete_persists_authoritative_warehouse_truth_and_retries_idempotently()
    {
        const string sessionId = "recv-r0-complete";
        await SeedReceivingSessionAsync(
            DemoTenantId,
            new LoadArrReceivingSessionResponse(
                sessionId,
                "RCV-R0-COMPLETE",
                "purchase_order",
                "open",
                "staff-site-stl-north",
                "STL North Yard",
                "supplyarr",
                "purchase_order",
                "PO-R0-COMPLETE",
                "Midwest Fleet Supply",
                "person-inventory-clerk",
                null,
                "2026-06-27T10:00:00Z",
                null,
                new[]
                {
                    new LoadArrReceivingLineResponse(
                        "line-r0-complete",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        4m,
                        4m,
                        "each",
                        "loc-dock-01",
                        "Receiving Dock 1",
                        "L2405-77",
                        null,
                        "new",
                        "ready_to_complete",
                        null,
                        "Received and staged for putaway")
                }));

        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        using var firstRequest = Authorized(HttpMethod.Post, $"/api/v1/receiving/{sessionId}/complete", token);
        firstRequest.Content = JsonContent.Create(new
        {
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-R0-COMPLETE",
            supplierNameSnapshot = "Midwest Fleet Supply",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 4m,
            receivedQuantity = 4m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "new",
            discrepancyReasonCode = (string?)null,
            complianceEvaluationId = "cc-eval-receiving-r0-complete",
            evidenceSummary = "Received and staged for putaway"
        });

        var firstResponse = await _client.SendAsync(firstRequest);

        firstResponse.EnsureSuccessStatusCode();
        var firstBody = await ReadJsonObjectAsync(firstResponse);
        Assert.Equal("completed", GetRequiredProperty(firstBody, "session")["status"]!.GetValue<string>());
        Assert.Equal("person-inventory-supervisor", GetRequiredProperty(firstBody, "session")["completedByPersonId"]!.GetValue<string>());
        Assert.Equal("PO-R0-COMPLETE", GetRequiredProperty(firstBody, "originEvent")["originObjectId"]!.GetValue<string>());
        Assert.Equal(sessionId, GetRequiredProperty(firstBody, "movement")["relatedObjectId"]!.GetValue<string>());
        Assert.Equal(4m, GetRequiredProperty(firstBody, "balance")["quantityOnHand"]!.GetValue<decimal>());
        Assert.Equal("putaway", GetRequiredProperty(firstBody, "putawayTask")["taskType"]!.GetValue<string>());

        var firstOriginEventId = GetRequiredProperty(firstBody, "originEvent")["id"]!.GetValue<string>();
        var firstMovementId = GetRequiredProperty(firstBody, "movement")["id"]!.GetValue<string>();
        var firstTaskId = GetRequiredProperty(firstBody, "putawayTask")["id"]!.GetValue<string>();

        using var retryRequest = Authorized(HttpMethod.Post, $"/api/v1/receiving/{sessionId}/complete", token);
        retryRequest.Content = JsonContent.Create(new
        {
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-R0-COMPLETE",
            supplierNameSnapshot = "Midwest Fleet Supply",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 4m,
            receivedQuantity = 4m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "new",
            discrepancyReasonCode = (string?)null,
            complianceEvaluationId = "cc-eval-receiving-r0-complete",
            evidenceSummary = "Received and staged for putaway"
        });

        var retryResponse = await _client.SendAsync(retryRequest);

        retryResponse.EnsureSuccessStatusCode();
        var retryBody = await ReadJsonObjectAsync(retryResponse);
        Assert.Equal(firstOriginEventId, GetRequiredProperty(retryBody, "originEvent")["id"]!.GetValue<string>());
        Assert.Equal(firstMovementId, GetRequiredProperty(retryBody, "movement")["id"]!.GetValue<string>());
        Assert.Equal(firstTaskId, GetRequiredProperty(retryBody, "putawayTask")["id"]!.GetValue<string>());

        var entity = await FindReceivingSessionRowAsync(DemoTenantId, sessionId);
        Assert.NotNull(entity);
        Assert.Equal("completed", entity!.Status);
        Assert.Equal("person-inventory-supervisor", entity.CompletedByPersonId);
        Assert.NotNull(entity.CompletedAtUtc);
        Assert.Equal(1, await CountInventoryOriginEventRowsAsync(DemoTenantId));
        Assert.Equal(1, await CountInventoryMovementRowsAsync(DemoTenantId));
        Assert.Equal(1, await CountInventoryBalanceRowsAsync(DemoTenantId));
        Assert.Equal(1, await CountWarehouseTaskRowsAsync(DemoTenantId));

        var inventoryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/inventory?query=valve&locationId=loc-dock-01", token));
        inventoryResponse.EnsureSuccessStatusCode();
        var inventoryBody = await ReadJsonObjectAsync(inventoryResponse);
        var inventoryItem = GetRequiredProperty(inventoryBody, "items").AsArray().Single()?.AsObject()
            ?? throw new InvalidOperationException("Expected one inventory balance.");
        Assert.Equal("SUP-VALVE-KIT-A", GetRequiredProperty(inventoryItem, "supplyarrItemId").GetValue<string>());
        Assert.Equal(4m, GetRequiredProperty(inventoryItem, "quantityOnHand").GetValue<decimal>());

        var taskResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/tasks?taskType=putaway&status=ready", token));
        taskResponse.EnsureSuccessStatusCode();
        var taskBody = await ReadJsonObjectAsync(taskResponse);
        var taskItem = GetRequiredProperty(taskBody, "items").AsArray().Single()?.AsObject()
            ?? throw new InvalidOperationException("Expected one putaway task.");
        Assert.Equal(firstTaskId, GetRequiredProperty(taskItem, "id").GetValue<string>());
        Assert.Equal("putaway", GetRequiredProperty(taskItem, "taskType").GetValue<string>());

        var ledgerResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/stock-ledger?itemId=SUP-VALVE-KIT-A", token));
        ledgerResponse.EnsureSuccessStatusCode();
        var ledgerBody = await ReadJsonObjectAsync(ledgerResponse);
        var ledgerItem = GetRequiredProperty(ledgerBody, "items").AsArray().Single()?.AsObject()
            ?? throw new InvalidOperationException("Expected one stock-ledger entry.");
        Assert.Equal(firstMovementId, GetRequiredProperty(ledgerItem, "id").GetValue<string>());
        Assert.Equal("receive", GetRequiredProperty(ledgerItem, "entryType").GetValue<string>());
        Assert.Equal("receiving_session:recv-r0-complete", GetRequiredProperty(ledgerItem, "sourceReference").GetValue<string>());

        var receivingHistoryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/receiving-history?sourceObjectId=PO-R0-COMPLETE", token));
        receivingHistoryResponse.EnsureSuccessStatusCode();
        var receivingHistoryBody = await ReadJsonObjectAsync(receivingHistoryResponse);
        var receivingHistoryItem = GetRequiredProperty(receivingHistoryBody, "items").AsArray().Single()?.AsObject()
            ?? throw new InvalidOperationException("Expected one receiving-history row.");
        Assert.Equal(sessionId, GetRequiredProperty(receivingHistoryItem, "id").GetValue<string>());
        Assert.Equal(4m, GetRequiredProperty(receivingHistoryItem, "receivedQuantity").GetValue<decimal>());

        var movementHistoryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/movement-history?movementType=receive", token));
        movementHistoryResponse.EnsureSuccessStatusCode();
        var movementHistoryBody = await ReadJsonObjectAsync(movementHistoryResponse);
        var movementHistoryItem = GetRequiredProperty(movementHistoryBody, "items").AsArray().Single()?.AsObject()
            ?? throw new InvalidOperationException("Expected one movement-history row.");
        Assert.Equal(firstMovementId, GetRequiredProperty(movementHistoryItem, "id").GetValue<string>());
        Assert.Equal("receiving_session:recv-r0-complete", GetRequiredProperty(movementHistoryItem, "sourceReference").GetValue<string>());

        var otherTenantInventoryResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get,
            "/api/v1/workspace/inventory",
            CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: SecondaryTenantId)));
        otherTenantInventoryResponse.EnsureSuccessStatusCode();
        var otherTenantInventoryBody = await ReadJsonObjectAsync(otherTenantInventoryResponse);
        Assert.Empty(GetRequiredProperty(otherTenantInventoryBody, "items").AsArray());
    }

    [Fact]
    public async Task Receiving_complete_blocks_inspection_required_draft_without_mutation()
    {
        const string sessionId = "recv-r0-no-mutate";
        await SeedReceivingSessionAsync(
            DemoTenantId,
            new LoadArrReceivingSessionResponse(
                sessionId,
                "RCV-R0-NO-MUTATE",
                "purchase_order",
                "open",
                "staff-site-stl-north",
                "STL North Yard",
                "supplyarr",
                "purchase_order",
                "PO-R0-NO-MUTATE",
                "Midwest Fleet Supply",
                "person-inventory-clerk",
                null,
                "2026-06-27T10:00:00Z",
                null,
                new[]
                {
                    new LoadArrReceivingLineResponse(
                        "line-r0-no-mutate",
                        "SUP-VALVE-KIT-A",
                        "Valve repair kit A",
                        4m,
                        4m,
                        "each",
                        "loc-dock-01",
                        "Receiving Dock 1",
                        "L2405-77",
                        null,
                        "pending_inspection",
                        "ready_to_complete",
                        null,
                        "Received and staged for putaway")
                }));

        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin", tenantId: DemoTenantId);
        var beforeCount = await CountReceivingSessionRowsAsync(DemoTenantId);

        using var request = Authorized(HttpMethod.Post, $"/api/v1/receiving/{sessionId}/complete", token);
        request.Content = JsonContent.Create(new
        {
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-R0-NO-MUTATE",
            supplierNameSnapshot = "Midwest Fleet Supply",
            completedByPersonId = "person-inventory-supervisor",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 4m,
            receivedQuantity = 4m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "pending_inspection",
            discrepancyReasonCode = (string?)null,
            complianceEvaluationId = "cc-eval-receiving-24018",
            evidenceSummary = "Received and staged for putaway"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadJsonObjectAsync(response);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
        Assert.Equal(beforeCount, await CountReceivingSessionRowsAsync(DemoTenantId));
        Assert.Equal(0, await CountInventoryOriginEventRowsAsync(DemoTenantId));
        Assert.Equal(0, await CountInventoryMovementRowsAsync(DemoTenantId));
        Assert.Equal(0, await CountInventoryBalanceRowsAsync(DemoTenantId));
        Assert.Equal(0, await CountWarehouseTaskRowsAsync(DemoTenantId));

        var entity = await FindReceivingSessionRowAsync(DemoTenantId, sessionId);
        Assert.NotNull(entity);
        Assert.Equal("open", entity!.Status);
        Assert.Null(entity.CompletedByPersonId);
        Assert.Null(entity.CompletedAtUtc);
    }

    [Fact]
    public async Task Truck_stock_return_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/truck-stock/truck-stock-17-kit/return", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 2m,
            reasonCode = "route_restock_return",
            evidenceSummary = "Returned unused kit stock"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_return_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/return", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 1m,
            reasonCode = "kit_return_to_stock",
            evidenceSummary = "Returned unused kit components"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_track_location_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/track-location", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            targetLocationId = "loc-dock-01",
            reasonCode = "kit_location_correction",
            evidenceSummary = "Tracked kit to dock staging"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_assign_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/assign", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            targetPersonId = "person-maintainarr-tech",
            targetPersonNameSnapshot = "Morgan Ellis",
            reasonCode = "kit_reassignment",
            evidenceSummary = "Assigned to new technician"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_expire_components_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-ppe-hazmat-04/expire-components", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-hazmat-reviewer",
            quantity = 1m,
            reasonCode = "inspection_expiration",
            evidenceSummary = "Expired components during controlled review"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_reserve_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/reserve", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 1m,
            reasonCode = "kit_reservation",
            evidenceSummary = "Reserved one kit for controlled use"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_pick_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/pick", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 1m,
            reasonCode = "kit_pick_for_use",
            evidenceSummary = "Picked one kit for controlled use"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Truck_stock_count_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/truck-stock/truck-stock-17-kit/count", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            countedQuantity = 2m,
            reasonCode = "cycle_count_restock",
            evidenceSummary = "Counted during truck inventory review"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_break_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/break", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 1m,
            reasonCode = "kit_breakdown",
            evidenceSummary = "Broken down for component recovery"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_build_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-ppe-hazmat-04/build", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-hazmat-reviewer",
            quantity = 1m,
            reasonCode = "kit_build_from_components",
            evidenceSummary = "Built one kit after replenishment"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Kit_inspect_reports_dependency_unavailable_until_read_model_sync()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        using var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/inspect", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 1m,
            reasonCode = "kit_inspection_readiness",
            evidenceSummary = "Inspected for readiness before deployment"
        });

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Route_surface_dashboard_denies_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/dashboard", token));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Route_surface_operational_queue_surfaces_report_dependency_unavailable_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");
        string[] paths =
        [
            "/api/v1/loadarr/expected-receipts",
            "/api/v1/loadarr/expected-receipts/task-receive-24018",
            "/api/v1/loadarr/dock-appointments",
            "/api/v1/loadarr/dock-appointments/dock-appt-24018",
            "/api/v1/loadarr/putaway-tasks",
            "/api/v1/loadarr/putaway-tasks/xfer-24018-putaway",
            "/api/v1/loadarr/reservations",
            "/api/v1/loadarr/reservations/res-wo-5530-rotor",
            "/api/v1/loadarr/picking",
            "/api/v1/loadarr/picking/task-pick-wo-5530",
            "/api/v1/loadarr/staging",
            "/api/v1/loadarr/staging/stage-rt-7781",
            "/api/v1/loadarr/shipping",
            "/api/v1/loadarr/shipping/handoff-rt-7781",
            "/api/v1/loadarr/loadouts",
            "/api/v1/loadarr/loadouts/handoff-rt-7781"
        ];

        HttpResponseMessage? firstResponse = null;
        foreach (var path in paths)
        {
            var response = await _client.SendAsync(Authorized(HttpMethod.Get, path, token));
            firstResponse ??= response;
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        Assert.NotNull(firstResponse);
        var body = await ReadJsonObjectAsync(firstResponse!);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
    }

    [Fact]
    public async Task Records_surfaces_deny_plain_tenant_member()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "tenant_member");

        var stockLedgerResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/stock-ledger", token));
        Assert.Equal(HttpStatusCode.Forbidden, stockLedgerResponse.StatusCode);
    }

    [Fact]
    public async Task Durable_records_surfaces_return_empty_lists_for_warehouse_supervisor_until_activity_exists()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");

        var stockLedgerResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/stock-ledger", token));
        stockLedgerResponse.EnsureSuccessStatusCode();
        var stockLedgerBody = await ReadJsonObjectAsync(stockLedgerResponse);
        Assert.Empty(GetRequiredProperty(stockLedgerBody, "items").AsArray());
        Assert.Equal(0, GetRequiredProperty(stockLedgerBody, "total").GetValue<int>());

        var stockLedgerDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/stock-ledger/ledger-rr-24018-valve", token));
        Assert.Equal(HttpStatusCode.NotFound, stockLedgerDetailResponse.StatusCode);

        var receivingHistoryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/receiving-history", token));
        receivingHistoryResponse.EnsureSuccessStatusCode();
        var receivingHistoryBody = await ReadJsonObjectAsync(receivingHistoryResponse);
        Assert.Empty(GetRequiredProperty(receivingHistoryBody, "items").AsArray());
        Assert.Equal(0, GetRequiredProperty(receivingHistoryBody, "total").GetValue<int>());

        var receivingHistoryDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/receiving-history/recv-24018", token));
        Assert.Equal(HttpStatusCode.NotFound, receivingHistoryDetailResponse.StatusCode);

        var movementHistoryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/movement-history", token));
        movementHistoryResponse.EnsureSuccessStatusCode();
        var movementHistoryBody = await ReadJsonObjectAsync(movementHistoryResponse);
        Assert.Empty(GetRequiredProperty(movementHistoryBody, "items").AsArray());
        Assert.Equal(0, GetRequiredProperty(movementHistoryBody, "total").GetValue<int>());

        var movementHistoryDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/movement-history/move-xfer-24018", token));
        Assert.Equal(HttpStatusCode.NotFound, movementHistoryDetailResponse.StatusCode);

        var countHistoryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/count-history", token));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, countHistoryResponse.StatusCode);

        var countHistoryDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/count-history/count-8021", token));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, countHistoryDetailResponse.StatusCode);

        var adjustmentHistoryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/adjustment-history", token));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, adjustmentHistoryResponse.StatusCode);

        var adjustmentHistoryDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/adjustment-history/adj-count-8021", token));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, adjustmentHistoryDetailResponse.StatusCode);
    }

    [Fact]
    public async Task Route_surface_dashboard_exception_supply_and_setup_surfaces_report_dependency_unavailable_for_warehouse_supervisor()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "warehouse_supervisor");
        string[] paths =
        [
            "/api/v1/loadarr/dashboard",
            "/api/v1/loadarr/exceptions",
            "/api/v1/loadarr/exceptions/receiving",
            "/api/v1/loadarr/exceptions/inventory-holds",
            "/api/v1/loadarr/exceptions/quarantine",
            "/api/v1/loadarr/exceptions/pending-quality-review",
            "/api/v1/loadarr/exceptions/exc-count-8021",
            "/api/v1/loadarr/supply-coordination/po-receipts",
            "/api/v1/loadarr/supply-coordination/po-receipts/coord-po-10492",
            "/api/v1/loadarr/supply-coordination/vendor-returns",
            "/api/v1/loadarr/supply-coordination/vendor-returns/ret-adh-49",
            "/api/v1/loadarr/supply-coordination/backorders",
            "/api/v1/loadarr/supply-coordination/backorders/backorder-rt-7781",
            "/api/v1/loadarr/supply-coordination/reorder-signals",
            "/api/v1/loadarr/supply-coordination/reorder-signals/reorder-hazmat-01",
            "/api/v1/loadarr/setup/location-rules",
            "/api/v1/loadarr/setup/location-rules/rule-quarantine-blocked",
            "/api/v1/loadarr/setup/item-references",
            "/api/v1/loadarr/setup/item-references/SUP-VALVE-KIT-A",
            "/api/v1/loadarr/setup/inventory-policies",
            "/api/v1/loadarr/setup/inventory-policies/policy-receipt-hazmat-inspection",
            "/api/v1/loadarr/setup/devices-labels",
            "/api/v1/loadarr/setup/devices-labels/profile-dock-receipt-label"
        ];

        HttpResponseMessage? firstResponse = null;
        foreach (var path in paths)
        {
            var response = await _client.SendAsync(Authorized(HttpMethod.Get, path, token));
            firstResponse ??= response;
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        Assert.NotNull(firstResponse);
        var body = await ReadJsonObjectAsync(firstResponse!);
        Assert.Equal("dependency_unavailable", GetRequiredProperty(body, "errorCode").GetValue<string>());
    }

    private async Task SeedReceivingSessionAsync(Guid tenantId, LoadArrReceivingSessionResponse session)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<LoadArrOperationalWorkflowStore>();
        await store.SaveReceivingSessionAsync(tenantId, session, cancellationToken: CancellationToken.None);
    }

    private async Task SeedTransferOrderAsync(Guid tenantId, LoadArrTransferOrderResponse order)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<LoadArrOperationalWorkflowStore>();
        await store.SaveTransferOrderAsync(tenantId, order, cancellationToken: CancellationToken.None);
    }

    private async Task<int> CountReceivingSessionRowsAsync(Guid tenantId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoadArrDbContext>();
        return await db.LoadArrReceivingSessions.CountAsync(x => x.TenantId == tenantId);
    }

    private async Task<LoadArrReceivingSessionEntity?> FindReceivingSessionRowAsync(Guid tenantId, string sessionId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoadArrDbContext>();
        return await db.LoadArrReceivingSessions.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.SessionId == sessionId);
    }

    private async Task<int> CountInventoryOriginEventRowsAsync(Guid tenantId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoadArrDbContext>();
        return await db.LoadArrInventoryOriginEvents.CountAsync(x => x.TenantId == tenantId);
    }

    private async Task<int> CountInventoryMovementRowsAsync(Guid tenantId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoadArrDbContext>();
        return await db.LoadArrInventoryMovements.CountAsync(x => x.TenantId == tenantId);
    }

    private async Task<int> CountInventoryBalanceRowsAsync(Guid tenantId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoadArrDbContext>();
        return await db.LoadArrInventoryBalances.CountAsync(x => x.TenantId == tenantId);
    }

    private async Task<int> CountWarehouseTaskRowsAsync(Guid tenantId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoadArrDbContext>();
        return await db.LoadArrWarehouseTasks.CountAsync(x => x.TenantId == tenantId);
    }

    private async Task<int> CountTransferOrderRowsAsync(Guid tenantId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoadArrDbContext>();
        return await db.LoadArrTransferOrders.CountAsync(x => x.TenantId == tenantId);
    }

    private async Task<LoadArrTransferOrderEntity?> FindTransferOrderRowAsync(Guid tenantId, string orderId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LoadArrDbContext>();
        return await db.LoadArrTransferOrders.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.OrderId == orderId);
    }

    private string CreateLoadArrAccessToken(
        IReadOnlyList<string> entitlements,
        string tenantRoleKey,
        bool isPlatformAdmin = false,
        Guid? tenantId = null)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<LoadArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            Guid.Parse("22222222-2222-2222-2222-222222222201"),
            Guid.Parse("33333333-3333-3333-3333-333333333301"),
            "warehouse.user@demo.stl",
            "Warehouse User",
            tenantId ?? DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            entitlements,
            isPlatformAdmin);
        return accessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task<JsonObject> ReadJsonObjectAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?.AsObject()
            ?? throw new InvalidOperationException("Expected a JSON object response.");
    }

    private static JsonNode GetRequiredProperty(JsonObject json, string propertyName)
    {
        if (json.TryGetPropertyValue(propertyName, out var value) && value is not null)
        {
            return value;
        }

        var match = json.FirstOrDefault(
            entry => string.Equals(entry.Key, propertyName, StringComparison.OrdinalIgnoreCase));
        return match.Value ?? throw new InvalidOperationException($"Missing property '{propertyName}'.");
    }

    private sealed class FakeLoadArrLocationReferenceService : ILoadArrLocationReferenceService
    {
        private readonly Dictionary<Guid, IReadOnlyCollection<LoadArrLocationResponse>> _locationsByTenant = new();

        public STLCompliance.Shared.Contracts.StlApiException? Failure { get; set; }

        public void SetLocations(Guid tenantId, IReadOnlyCollection<LoadArrLocationResponse> locations) =>
            _locationsByTenant[tenantId] = locations;

        public Task<IReadOnlyCollection<LoadArrLocationResponse>> ListLocationsAsync(
            Guid tenantId,
            string? siteReference,
            string? locationType,
            bool? active,
            CancellationToken cancellationToken)
        {
            if (Failure is not null)
            {
                throw Failure;
            }

            var locations = GetLocations(tenantId)
                .Where(location => siteReference is null
                    || string.Equals(location.StaffarrSiteOrgUnitId, siteReference, StringComparison.OrdinalIgnoreCase))
                .Where(location => locationType is null
                    || string.Equals(location.LocationType, locationType, StringComparison.OrdinalIgnoreCase))
                .Where(location => active is null || location.Active == active.Value)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<LoadArrLocationResponse>>(locations);
        }

        public Task<LoadArrLocationResponse?> GetLocationAsync(
            Guid tenantId,
            string id,
            CancellationToken cancellationToken)
        {
            if (Failure is not null)
            {
                throw Failure;
            }

            var location = GetLocations(tenantId)
                .FirstOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(location);
        }

        public Task<IReadOnlyCollection<LoadArrLocationTreeNodeResponse>> GetLocationTreeAsync(
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            if (Failure is not null)
            {
                throw Failure;
            }

            var locations = GetLocations(tenantId);
            var siteGroups = locations
                .GroupBy(location => location.StaffarrSiteOrgUnitId, StringComparer.OrdinalIgnoreCase)
                .Select(group => new LoadArrLocationTreeNodeResponse(
                    $"site:{group.Key}",
                    group.First().StaffarrSiteNameSnapshot,
                    "site",
                    null,
                    group.Select(location => new LoadArrLocationTreeNodeResponse(
                        $"location:{location.Id}",
                        location.Name,
                        location.LocationType,
                        location.Id,
                        []))
                    .ToArray()))
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<LoadArrLocationTreeNodeResponse>>(siteGroups);
        }

        private IReadOnlyCollection<LoadArrLocationResponse> GetLocations(Guid tenantId) =>
            _locationsByTenant.TryGetValue(tenantId, out var locations) ? locations : [];
    }

    private sealed class FakeLoadArrSiteSourceService : ILoadArrSiteSourceService
    {
        private readonly Dictionary<Guid, IReadOnlyCollection<LoadArrSiteSourceResponse>> _sitesByTenant = new();

        public STLCompliance.Shared.Contracts.StlApiException? Failure { get; set; }

        public void SetSites(Guid tenantId, IReadOnlyCollection<LoadArrSiteSourceResponse> sites) =>
            _sitesByTenant[tenantId] = sites;

        public Task<IReadOnlyCollection<LoadArrSiteSourceResponse>> ListSitesAsync(
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            if (Failure is not null)
            {
                throw Failure;
            }

            return Task.FromResult<IReadOnlyCollection<LoadArrSiteSourceResponse>>(
                _sitesByTenant.TryGetValue(tenantId, out var sites) ? sites : []);
        }
    }

    private sealed class FakeLoadArrSupplyArrItemReferenceService : ILoadArrSupplyArrItemReferenceService
    {
        private readonly Dictionary<Guid, IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse>> _itemsByTenant = new();

        public STLCompliance.Shared.Contracts.StlApiException? Failure { get; set; }

        public void SetItemReferences(Guid tenantId, IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse> items) =>
            _itemsByTenant[tenantId] = items;

        public Task<IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse>> ListItemReferencesAsync(
            Guid tenantId,
            string? query,
            CancellationToken cancellationToken)
        {
            if (Failure is not null)
            {
                throw Failure;
            }

            var normalizedQuery = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
            var items = GetItems(tenantId)
                .Where(item => normalizedQuery is null
                    || item.SupplyarrItemId.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || item.ItemNumberSnapshot.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || item.ItemNameSnapshot.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    || item.ItemTypeSnapshot.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse>>(items);
        }

        public async Task<LoadArrSupplyArrItemReferenceResponse?> GetItemReferenceAsync(
            Guid tenantId,
            string supplyarrItemId,
            CancellationToken cancellationToken)
        {
            var items = await ListItemReferencesAsync(tenantId, supplyarrItemId, cancellationToken);
            return items.FirstOrDefault(item =>
                string.Equals(item.SupplyarrItemId, supplyarrItemId, StringComparison.OrdinalIgnoreCase));
        }

        private IReadOnlyCollection<LoadArrSupplyArrItemReferenceResponse> GetItems(Guid tenantId) =>
            _itemsByTenant.TryGetValue(tenantId, out var items) ? items : [];
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
