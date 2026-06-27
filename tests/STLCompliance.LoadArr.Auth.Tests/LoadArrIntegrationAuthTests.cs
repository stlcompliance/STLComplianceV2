using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using LoadArr.Api.Data;
using LoadArr.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace STLCompliance.LoadArr.Auth.Tests;

public sealed class LoadArrIntegrationAuthTests : IAsyncLifetime
{
    private WebApplicationFactory<global::LoadArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"LoadArrIntegrationAuth-{Guid.NewGuid():N}";

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
    public async Task Integration_items_allows_warehouse_manager_after_non_loadarr_launch_context()
    {
        var token = CreateLoadArrAccessToken(["nexarr"], "warehouse_manager");

        var response = await _client.SendAsync(
            Authorized(HttpMethod.Get, "/api/v1/integrations/items", token));

        response.EnsureSuccessStatusCode();
        using var payload = await response.Content.ReadFromJsonAsync<JsonDocument>();
        Assert.NotNull(payload);
        Assert.Equal(0, payload!.RootElement.GetProperty("total").GetInt32());
        Assert.Equal(0, payload.RootElement.GetProperty("items").GetArrayLength());
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
    public async Task Count_variance_approval_creates_adjustment_and_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/counts", token);
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
        createResponse.EnsureSuccessStatusCode();
        var created = await ReadJsonObjectAsync(createResponse);
        var countId = GetRequiredProperty(created, "id").GetValue<string>();
        Assert.Equal("open", GetRequiredProperty(created, "status").GetValue<string>());

        var approveRequest = Authorized(HttpMethod.Post, $"/api/v1/counts/{countId}/approve-variance", token);
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
        approveResponse.EnsureSuccessStatusCode();
        var approved = await ReadJsonObjectAsync(approveResponse);

        var count = GetRequiredProperty(approved, "count").AsObject();
        var adjustment = GetRequiredProperty(approved, "adjustment").AsObject();
        var originEvent = GetRequiredProperty(approved, "originEvent").AsObject();
        var movement = GetRequiredProperty(approved, "movement").AsObject();

        Assert.Equal("approved", GetRequiredProperty(count, "status").GetValue<string>());
        Assert.Equal(12m, GetRequiredProperty(count, "countedQuantity").GetValue<decimal>());
        Assert.Equal(2m, GetRequiredProperty(count, "varianceQuantity").GetValue<decimal>());
        Assert.Equal("approved", GetRequiredProperty(adjustment, "status").GetValue<string>());
        Assert.Equal("gain", GetRequiredProperty(adjustment, "adjustmentType").GetValue<string>());
        Assert.Equal("cycle_count_gain", GetRequiredProperty(originEvent, "originType").GetValue<string>());
        Assert.Equal("count_gain", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("available", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Truck_stock_issue_below_minimum_requests_restock()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/truck-stock/truck-stock-17-kit/issue", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 2m,
            reasonCode = "route_replenishment",
            evidenceSummary = "Issued for route kit replenishment"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var truckStock = GetRequiredProperty(body, "truckStock").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();
        var restockTask = GetRequiredProperty(body, "restockTask").AsObject();

        Assert.Equal(2m, GetRequiredProperty(truckStock, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("low_stock", GetRequiredProperty(truckStock, "status").GetValue<string>());
        Assert.Equal("truck_stock_issue", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("low_stock", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
        Assert.Equal("replenish", GetRequiredProperty(restockTask, "taskType").GetValue<string>());
    }

    [Fact]
    public async Task Kit_replenish_promotes_status_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-ppe-hazmat-04/replenish", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-hazmat-reviewer",
            quantity = 1m,
            reasonCode = "inspection_restock",
            evidenceSummary = "Replenished after inspection"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var kit = GetRequiredProperty(body, "kit").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal(2m, GetRequiredProperty(kit, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("built", GetRequiredProperty(kit, "status").GetValue<string>());
        Assert.Equal("kit_replenish", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("built", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Transfer_create_then_complete_updates_balances_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var createRequest = Authorized(HttpMethod.Post, "/api/v1/transfers", token);
        createRequest.Content = JsonContent.Create(new
        {
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
        createResponse.EnsureSuccessStatusCode();
        var created = await ReadJsonObjectAsync(createResponse);
        var transferId = GetRequiredProperty(created, "id").GetValue<string>();

        var createdLines = GetRequiredProperty(created, "lines").AsArray();
        Assert.Single(createdLines);
        Assert.Equal("draft", GetRequiredProperty(created, "status").GetValue<string>());
        Assert.Equal("TRF-", GetRequiredProperty(created, "transferNumber").GetValue<string>()[..4]);

        var completeRequest = Authorized(HttpMethod.Post, $"/api/v1/transfers/{transferId}/complete", token);
        completeRequest.Content = JsonContent.Create(new
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

        var completeResponse = await _client.SendAsync(completeRequest);
        completeResponse.EnsureSuccessStatusCode();
        var completed = await ReadJsonObjectAsync(completeResponse);

        var transfer = GetRequiredProperty(completed, "transfer").AsObject();
        var movement = GetRequiredProperty(completed, "movement").AsObject();
        var sourceBalance = GetRequiredProperty(completed, "sourceBalance").AsObject();
        var destinationBalance = GetRequiredProperty(completed, "destinationBalance").AsObject();
        var transferTask = GetRequiredProperty(completed, "transferTask").AsObject();

        Assert.Equal(transferId, GetRequiredProperty(transfer, "id").GetValue<string>());
        Assert.Equal("completed", GetRequiredProperty(transfer, "status").GetValue<string>());
        Assert.Equal("person-inventory-supervisor", GetRequiredProperty(transfer, "completedByPersonId").GetValue<string>());
        Assert.Equal("transfer", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("available", GetRequiredProperty(movement, "statusBefore").GetValue<string>());
        Assert.Equal("available", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
        Assert.Equal(34m, GetRequiredProperty(sourceBalance, "quantityOnHand").GetValue<decimal>());
        Assert.Equal(4m, GetRequiredProperty(destinationBalance, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("transfer", GetRequiredProperty(transferTask, "taskType").GetValue<string>());
        Assert.Equal("completed", GetRequiredProperty(transferTask, "status").GetValue<string>());
    }

    [Fact]
    public async Task Hold_release_updates_hold_and_balance()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/holds/hold-adh-49/release", token);
        request.Content = JsonContent.Create(new
        {
            releasedByPersonId = "person-hazmat-reviewer",
            reasonCode = "sds_label_mismatch_resolved",
            evidenceSummary = "Compliance review cleared the hold"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var hold = GetRequiredProperty(body, "hold").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();
        var balance = GetRequiredProperty(body, "balance").AsObject();

        Assert.Equal("released", GetRequiredProperty(hold, "status").GetValue<string>());
        Assert.Equal("person-hazmat-reviewer", GetRequiredProperty(hold, "releasedByPersonId").GetValue<string>());
        Assert.Equal("release_hold", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("available", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
        Assert.Equal(0m, GetRequiredProperty(balance, "quantityBlocked").GetValue<decimal>());
    }

    [Fact]
    public async Task Unexplained_inventory_resolve_creates_origin_event_and_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/unexplained-inventory/unexplained-count-8021/resolve", token);
        request.Content = JsonContent.Create(new
        {
            approvedByPersonId = "person-route-stock-supervisor",
            reasonCode = "variance_approved",
            complianceEvaluationId = "cc-eval-unx-8021",
            evidenceSummary = "Supervisor approved the variance"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var record = GetRequiredProperty(body, "record").AsObject();
        var originEvent = GetRequiredProperty(body, "originEvent").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal("resolved_valid_stock", GetRequiredProperty(record, "status").GetValue<string>());
        Assert.Equal("trusted_available", GetRequiredProperty(record, "resolutionState").GetValue<string>());
        Assert.Equal("unexplained_inventory_resolution", GetRequiredProperty(originEvent, "originType").GetValue<string>());
        Assert.Equal("adjust", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("available", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Unexplained_inventory_quarantine_moves_record_to_quarantine_location()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/unexplained-inventory/unexplained-dock-adh/quarantine", token);
        request.Content = JsonContent.Create(new
        {
            quarantineLocationId = "loc-quarantine-01",
            quarantinedByPersonId = "person-inventory-clerk",
            reasonCode = "damaged_freight_review",
            evidenceSummary = "Moved to quarantine for review"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var record = GetRequiredProperty(body, "record").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal("needs_quarantine", GetRequiredProperty(record, "status").GetValue<string>());
        Assert.Equal("loc-quarantine-01", GetRequiredProperty(record, "warehouseLocationId").GetValue<string>());
        Assert.Equal("quarantine", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("quarantined_untrusted", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Receiving_complete_creates_origin_event_movement_balance_and_putaway_task()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/receiving/recv-24018/complete", token);
        request.Content = JsonContent.Create(new
        {
            receivingType = "purchase_order",
            sourceProductKey = "supplyarr",
            sourceObjectType = "purchase_order",
            sourceObjectId = "PO-10492",
            supplierNameSnapshot = "Midwest Fleet Supply",
            completedByPersonId = "person-inventory-clerk",
            supplyarrItemId = "SUP-VALVE-KIT-A",
            expectedQuantity = 38m,
            receivedQuantity = 38m,
            warehouseLocationId = "loc-dock-01",
            lotCode = "L2405-77",
            serialCode = (string?)null,
            condition = "new",
            discrepancyReasonCode = (string?)null,
            complianceEvaluationId = "cc-eval-rcv-24018",
            evidenceSummary = "Dock receipt photo attached"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var session = GetRequiredProperty(body, "session").AsObject();
        var originEvent = GetRequiredProperty(body, "originEvent").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();
        var balance = GetRequiredProperty(body, "balance").AsObject();
        var putawayTask = GetRequiredProperty(body, "putawayTask").AsObject();

        Assert.Equal("completed", GetRequiredProperty(session, "status").GetValue<string>());
        Assert.Equal("purchase_receipt", GetRequiredProperty(originEvent, "originType").GetValue<string>());
        Assert.Equal("receive", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("available", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
        Assert.Equal(38m, GetRequiredProperty(balance, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("putaway", GetRequiredProperty(putawayTask, "taskType").GetValue<string>());
        Assert.Equal("ready", GetRequiredProperty(putawayTask, "status").GetValue<string>());
    }

    [Fact]
    public async Task Truck_stock_return_updates_quantity_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/truck-stock/truck-stock-17-kit/return", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 2m,
            reasonCode = "route_restock_return",
            evidenceSummary = "Returned unused kit stock"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var truckStock = GetRequiredProperty(body, "truckStock").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal(6m, GetRequiredProperty(truckStock, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("ready", GetRequiredProperty(truckStock, "status").GetValue<string>());
        Assert.Equal("truck_stock_return", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("ready", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Kit_return_updates_quantity_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/return", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 1m,
            reasonCode = "kit_return_to_stock",
            evidenceSummary = "Returned unused kit components"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var kit = GetRequiredProperty(body, "kit").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal(5m, GetRequiredProperty(kit, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("returned", GetRequiredProperty(kit, "status").GetValue<string>());
        Assert.Equal("kit_return", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("returned", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Kit_track_location_updates_location_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/track-location", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            targetLocationId = "loc-dock-01",
            reasonCode = "kit_location_correction",
            evidenceSummary = "Tracked kit to dock staging"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var kit = GetRequiredProperty(body, "kit").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal("loc-dock-01", GetRequiredProperty(kit, "locationId").GetValue<string>());
        Assert.Equal("tracked", GetRequiredProperty(kit, "status").GetValue<string>());
        Assert.Equal("kit_track_location", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("tracked", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Kit_assign_updates_assignee_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/assign", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            targetPersonId = "person-maintainarr-tech",
            targetPersonNameSnapshot = "Morgan Ellis",
            reasonCode = "kit_reassignment",
            evidenceSummary = "Assigned to new technician"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var kit = GetRequiredProperty(body, "kit").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal("person-maintainarr-tech", GetRequiredProperty(kit, "assignedPersonId").GetValue<string>());
        Assert.Equal("Morgan Ellis", GetRequiredProperty(kit, "assignedPersonNameSnapshot").GetValue<string>());
        Assert.Equal("assigned", GetRequiredProperty(kit, "status").GetValue<string>());
        Assert.Equal("kit_assign", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("assigned", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Kit_expire_components_sets_status_and_zeroes_quantity()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-ppe-hazmat-04/expire-components", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-hazmat-reviewer",
            quantity = 1m,
            reasonCode = "inspection_expiration",
            evidenceSummary = "Expired components during controlled review"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var kit = GetRequiredProperty(body, "kit").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal(0m, GetRequiredProperty(kit, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("expired", GetRequiredProperty(kit, "status").GetValue<string>());
        Assert.Equal("kit_expire_components", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("expired", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Kit_reserve_updates_quantity_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/reserve", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 1m,
            reasonCode = "kit_reservation",
            evidenceSummary = "Reserved one kit for controlled use"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var kit = GetRequiredProperty(body, "kit").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal(3m, GetRequiredProperty(kit, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("reserved", GetRequiredProperty(kit, "status").GetValue<string>());
        Assert.Equal("kit_reserve", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("reserved", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Kit_pick_updates_quantity_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/pick", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 1m,
            reasonCode = "kit_pick_for_use",
            evidenceSummary = "Picked one kit for controlled use"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var kit = GetRequiredProperty(body, "kit").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal(3m, GetRequiredProperty(kit, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("picked", GetRequiredProperty(kit, "status").GetValue<string>());
        Assert.Equal("kit_pick", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("picked", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Truck_stock_count_updates_quantity_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/truck-stock/truck-stock-17-kit/count", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            countedQuantity = 2m,
            reasonCode = "cycle_count_restock",
            evidenceSummary = "Counted during truck inventory review"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var truckStock = GetRequiredProperty(body, "truckStock").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();
        var restockTask = GetRequiredProperty(body, "restockTask").AsObject();

        Assert.Equal(2m, GetRequiredProperty(truckStock, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("low_stock", GetRequiredProperty(truckStock, "status").GetValue<string>());
        Assert.Equal("truck_stock_count", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("low_stock", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
        Assert.Equal("replenish", GetRequiredProperty(restockTask, "taskType").GetValue<string>());
    }

    [Fact]
    public async Task Kit_break_updates_quantity_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/break", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 1m,
            reasonCode = "kit_breakdown",
            evidenceSummary = "Broken down for component recovery"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var kit = GetRequiredProperty(body, "kit").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();

        Assert.Equal(3m, GetRequiredProperty(kit, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("built", GetRequiredProperty(kit, "status").GetValue<string>());
        Assert.Equal("kit_break", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("built", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
    }

    [Fact]
    public async Task Kit_build_updates_quantity_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-ppe-hazmat-04/build", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-hazmat-reviewer",
            quantity = 1m,
            reasonCode = "kit_build_from_components",
            evidenceSummary = "Built one kit after replenishment"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var kit = GetRequiredProperty(body, "kit").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();
        body.TryGetPropertyValue("followUpTask", out var followUpTask);

        Assert.Equal(2m, GetRequiredProperty(kit, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("built", GetRequiredProperty(kit, "status").GetValue<string>());
        Assert.Equal("kit_build", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("built", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
        Assert.True(followUpTask is null);
    }

    [Fact]
    public async Task Kit_inspect_updates_status_and_records_movement()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/kits/kit-pm-emergency-17/inspect", token);
        request.Content = JsonContent.Create(new
        {
            personId = "person-route-stock-lead",
            quantity = 1m,
            reasonCode = "kit_inspection_readiness",
            evidenceSummary = "Inspected for readiness before deployment"
        });

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var kit = GetRequiredProperty(body, "kit").AsObject();
        var movement = GetRequiredProperty(body, "movement").AsObject();
        body.TryGetPropertyValue("followUpTask", out var followUpTask);

        Assert.Equal(4m, GetRequiredProperty(kit, "quantityOnHand").GetValue<decimal>());
        Assert.Equal("inspected", GetRequiredProperty(kit, "status").GetValue<string>());
        Assert.Equal("kit_inspect", GetRequiredProperty(movement, "movementType").GetValue<string>());
        Assert.Equal("inspected", GetRequiredProperty(movement, "statusAfter").GetValue<string>());
        Assert.True(followUpTask is null);
    }

    [Fact]
    public async Task Count_complete_updates_status_and_records_completion()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var request = Authorized(HttpMethod.Post, "/api/v1/counts/count-dock-01-verified/complete", token);
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
        response.EnsureSuccessStatusCode();
        var body = await ReadJsonObjectAsync(response);

        var count = GetRequiredProperty(body, "count").AsObject();
        body.TryGetPropertyValue("adjustment", out var adjustment);
        body.TryGetPropertyValue("originEvent", out var originEvent);
        body.TryGetPropertyValue("movement", out var movement);

        Assert.Equal("completed", GetRequiredProperty(count, "status").GetValue<string>());
        Assert.Equal(10m, GetRequiredProperty(count, "countedQuantity").GetValue<decimal>());
        Assert.Equal(0m, GetRequiredProperty(count, "varianceQuantity").GetValue<decimal>());
        Assert.Equal("person-inventory-clerk", GetRequiredProperty(count, "countedByPersonId").GetValue<string>());
        Assert.NotNull(GetRequiredProperty(count, "completedAtUtc").GetValue<string>());
        Assert.True(adjustment is null);
        Assert.True(originEvent is null);
        Assert.True(movement is null);
    }

    [Fact]
    public async Task Picking_and_shipping_surfaces_expose_seeded_operational_queue_records()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var expectedReceiptsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/expected-receipts", token));
        expectedReceiptsResponse.EnsureSuccessStatusCode();
        var expectedReceiptsBody = await ReadJsonObjectAsync(expectedReceiptsResponse);

        var expectedReceiptItems = GetRequiredProperty(expectedReceiptsBody, "items").AsArray();
        var expectedReceiptDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/expected-receipts/task-receive-24018", token));
        expectedReceiptDetailResponse.EnsureSuccessStatusCode();
        var expectedReceiptDetail = await ReadJsonObjectAsync(expectedReceiptDetailResponse);

        Assert.Contains(expectedReceiptItems, item => string.Equals(
            item?["id"]?.GetValue<string>(),
            "task-receive-24018",
            StringComparison.OrdinalIgnoreCase));
        Assert.Equal("ready_to_receive", GetRequiredProperty(expectedReceiptDetail, "status").GetValue<string>());
        Assert.Equal("purchase_order", GetRequiredProperty(expectedReceiptDetail, "sourceObjectType").GetValue<string>());

        var reservationsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/reservations", token));
        reservationsResponse.EnsureSuccessStatusCode();
        var reservationsBody = await ReadJsonObjectAsync(reservationsResponse);

        var reservationItems = GetRequiredProperty(reservationsBody, "items").AsArray();
        var reservationDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/reservations/res-wo-5530-rotor", token));
        reservationDetailResponse.EnsureSuccessStatusCode();
        var reservationDetail = await ReadJsonObjectAsync(reservationDetailResponse);

        Assert.Contains(reservationItems, item => string.Equals(
            item?["id"]?.GetValue<string>(),
            "res-wo-5530-rotor",
            StringComparison.OrdinalIgnoreCase));
        Assert.Equal("reserved", GetRequiredProperty(reservationDetail, "status").GetValue<string>());
        Assert.Equal("maintainarr", GetRequiredProperty(reservationDetail, "demandProductKey").GetValue<string>());

        var putawayResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/putaway-tasks", token));
        putawayResponse.EnsureSuccessStatusCode();
        var putawayBody = await ReadJsonObjectAsync(putawayResponse);

        var putawayItems = GetRequiredProperty(putawayBody, "items").AsArray();
        var putawayDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/putaway-tasks/xfer-24018-putaway", token));
        putawayDetailResponse.EnsureSuccessStatusCode();
        var putawayDetail = await ReadJsonObjectAsync(putawayDetailResponse);

        var pickResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/picking", token));
        pickResponse.EnsureSuccessStatusCode();
        var pickBody = await ReadJsonObjectAsync(pickResponse);

        var shippingResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/shipping", token));
        shippingResponse.EnsureSuccessStatusCode();
        var shippingBody = await ReadJsonObjectAsync(shippingResponse);

        Assert.Contains(putawayItems, item => string.Equals(
            item?["id"]?.GetValue<string>(),
            "xfer-24018-putaway",
            StringComparison.OrdinalIgnoreCase));
        Assert.Equal("ready", GetRequiredProperty(putawayDetail, "status").GetValue<string>());
        Assert.Equal("quality_inspection", GetRequiredProperty(putawayDetail, "reasonCode").GetValue<string>());

        var pickItems = GetRequiredProperty(pickBody, "items").AsArray();
        var shippingItems = GetRequiredProperty(shippingBody, "items").AsArray();

        Assert.Contains(pickItems, item => string.Equals(
            item?["id"]?.GetValue<string>(),
            "task-pick-wo-5530",
            StringComparison.OrdinalIgnoreCase));
        Assert.Contains(shippingItems, item => string.Equals(
            item?["id"]?.GetValue<string>(),
            "handoff-rt-7781",
            StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Stock_ledger_and_history_surfaces_expose_seeded_records()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var stockLedgerResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/stock-ledger", token));
        stockLedgerResponse.EnsureSuccessStatusCode();
        var stockLedgerBody = await ReadJsonObjectAsync(stockLedgerResponse);

        var movementHistoryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/movement-history", token));
        movementHistoryResponse.EnsureSuccessStatusCode();
        var movementHistoryBody = await ReadJsonObjectAsync(movementHistoryResponse);

        var countHistoryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/count-history", token));
        countHistoryResponse.EnsureSuccessStatusCode();
        var countHistoryBody = await ReadJsonObjectAsync(countHistoryResponse);

        var adjustmentHistoryResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/adjustment-history", token));
        adjustmentHistoryResponse.EnsureSuccessStatusCode();
        var adjustmentHistoryBody = await ReadJsonObjectAsync(adjustmentHistoryResponse);

        var stockLedgerItems = GetRequiredProperty(stockLedgerBody, "items").AsArray();
        var movementHistoryItems = GetRequiredProperty(movementHistoryBody, "items").AsArray();
        var countHistoryItems = GetRequiredProperty(countHistoryBody, "items").AsArray();
        var adjustmentHistoryItems = GetRequiredProperty(adjustmentHistoryBody, "items").AsArray();

        Assert.Contains(stockLedgerItems, item => string.Equals(item?["id"]?.GetValue<string>(), "ledger-rr-24018-valve", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(movementHistoryItems, item => string.Equals(item?["id"]?.GetValue<string>(), "move-xfer-24018", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(countHistoryItems, item => string.Equals(item?["id"]?.GetValue<string>(), "count-8021", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(adjustmentHistoryItems, item => string.Equals(item?["id"]?.GetValue<string>(), "adj-count-8021", StringComparison.OrdinalIgnoreCase));

        var stockLedgerDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/stock-ledger/ledger-rr-24018-valve", token));
        stockLedgerDetailResponse.EnsureSuccessStatusCode();
        var stockLedgerDetail = await ReadJsonObjectAsync(stockLedgerDetailResponse);

        var movementDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/movement-history/move-xfer-24018", token));
        movementDetailResponse.EnsureSuccessStatusCode();
        var movementDetail = await ReadJsonObjectAsync(movementDetailResponse);

        var countDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/count-history/count-8021", token));
        countDetailResponse.EnsureSuccessStatusCode();
        var countDetail = await ReadJsonObjectAsync(countDetailResponse);

        var adjustmentDetailResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/records/adjustment-history/adj-count-8021", token));
        adjustmentDetailResponse.EnsureSuccessStatusCode();
        var adjustmentDetail = await ReadJsonObjectAsync(adjustmentDetailResponse);

        Assert.Equal("receipt", GetRequiredProperty(stockLedgerDetail, "entryType").GetValue<string>());
        Assert.Equal("putaway", GetRequiredProperty(movementDetail, "movementType").GetValue<string>());
        Assert.Equal("variance_pending_approval", GetRequiredProperty(countDetail, "status").GetValue<string>());
        Assert.Equal("open", GetRequiredProperty(adjustmentDetail, "status").GetValue<string>());
    }

    [Fact]
    public async Task Dashboard_and_setup_surfaces_expose_seeded_workspace_and_admin_records()
    {
        var token = CreateLoadArrAccessToken(["loadarr"], "loadarr_admin");

        var dashboardResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/dashboard", token));
        dashboardResponse.EnsureSuccessStatusCode();
        var dashboardBody = await ReadJsonObjectAsync(dashboardResponse);

        var dashboardMetrics = GetRequiredProperty(dashboardBody, "metrics").AsObject();
        var dashboardLocations = GetRequiredProperty(dashboardBody, "locations").AsArray();

        Assert.True(GetRequiredProperty(dashboardMetrics, "activeLocations").GetValue<int>() >= 2);
        Assert.True(GetRequiredProperty(dashboardMetrics, "openTasks").GetValue<int>() >= 2);
        Assert.Contains(dashboardLocations, item => string.Equals(item?["id"]?.GetValue<string>(), "loc-dock-01", StringComparison.OrdinalIgnoreCase));

        var locationRulesResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/setup/location-rules", token));
        locationRulesResponse.EnsureSuccessStatusCode();
        var locationRulesBody = await ReadJsonObjectAsync(locationRulesResponse);

        var itemReferencesResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/setup/item-references", token));
        itemReferencesResponse.EnsureSuccessStatusCode();
        var itemReferencesBody = await ReadJsonObjectAsync(itemReferencesResponse);

        var inventoryPoliciesResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/setup/inventory-policies", token));
        inventoryPoliciesResponse.EnsureSuccessStatusCode();
        var inventoryPoliciesBody = await ReadJsonObjectAsync(inventoryPoliciesResponse);

        var deviceLabelsResponse = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/loadarr/setup/devices-labels", token));
        deviceLabelsResponse.EnsureSuccessStatusCode();
        var deviceLabelsBody = await ReadJsonObjectAsync(deviceLabelsResponse);

        var locationRules = GetRequiredProperty(locationRulesBody, "items").AsArray();
        var itemReferences = GetRequiredProperty(itemReferencesBody, "items").AsArray();
        var inventoryPolicies = GetRequiredProperty(inventoryPoliciesBody, "items").AsArray();
        var deviceLabels = GetRequiredProperty(deviceLabelsBody, "items").AsArray();

        Assert.Contains(locationRules, item => string.Equals(item?["id"]?.GetValue<string>(), "rule-quarantine-blocked", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(itemReferences, item => string.Equals(item?["id"]?.GetValue<string>(), "SUP-VALVE-KIT-A", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(inventoryPolicies, item => string.Equals(item?["id"]?.GetValue<string>(), "policy-receipt-hazmat-inspection", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(deviceLabels, item => string.Equals(item?["id"]?.GetValue<string>(), "profile-dock-receipt-label", StringComparison.OrdinalIgnoreCase));
    }

    private string CreateLoadArrAccessToken(IReadOnlyList<string> entitlements, string tenantRoleKey, bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<LoadArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            Guid.Parse("22222222-2222-2222-2222-222222222201"),
            Guid.Parse("33333333-3333-3333-3333-333333333301"),
            "warehouse.user@demo.stl",
            "Warehouse User",
            Guid.Parse("11111111-1111-1111-1111-111111111101"),
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
