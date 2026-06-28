using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using OrdArr.Api.Data;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.OrdArr.Auth.Tests;

public sealed class OrdArrStoreTests
{
    [Fact]
    public void CreateOrder_builds_order_lines_and_summary_fields()
    {
        using var db = CreateDb();
        var store = new OrdArrStore(db);
        var principal = CreatePrincipal();

        var order = store.CreateOrder(
            principal,
            new OrdArrCreateOrderRequest(
                new StlProductObjectReference("customarr", "customer", "cust-9001", "CUST-9001"),
                "Contoso Freight",
                "customer_order",
                "person-ordarr-owner",
                "Replacement pump kit",
                FulfillmentProductKeys: ["loadarr", "routarr"],
                SourceChannel: "customer_portal",
                Priority: "high",
                Lines:
                [
                    new OrdArrOrderLineRequest(
                        "item",
                        new StlProductObjectReference("supplyarr", "part", "part-pump-kit", "PK-9001"),
                        "Replacement pump kit",
                        2,
                        "ea",
                        "loadarr",
                        DateTimeOffset.UtcNow.AddDays(1),
                        DateTimeOffset.UtcNow.AddDays(1),
                        125m,
                        0m,
                        true,
                        true,
                        true,
                        true,
                        "none",
                        "loadarr.fulfillment"),
                ]),
            "test-idempotency-001");

        Assert.Equal("draft", order.LifecycleStatus);
        Assert.Equal(principal.FindFirstValue(StlClaimTypes.TenantId), order.TenantId);
        Assert.Equal("customer_portal", order.SourceChannel);
        Assert.Equal("high", order.Priority);
        Assert.Single(order.Lines);
        Assert.Equal(2, order.Timeline.Count);

        var summary = store.ListOrders(principal).Single(item => item.OrderId == order.OrderId);
        Assert.Equal(1, summary.LineCount);
        Assert.Equal("Submit order", summary.NextAction);
    }

    [Fact]
    public void Tenant_reads_are_scoped_to_the_authenticated_tenant()
    {
        using var db = CreateDb();
        var store = new OrdArrStore(db);
        var tenantOne = Guid.NewGuid().ToString();
        var tenantTwo = Guid.NewGuid().ToString();
        var principalOne = CreatePrincipal(tenantOne);
        var principalTwo = CreatePrincipal(tenantTwo);

        var order = store.CreateOrder(
            principalOne,
            new OrdArrCreateOrderRequest(
                new StlProductObjectReference("customarr", "customer", "cust-tenant-1", "CUST-1"),
                "Tenant One Customer",
                "customer_order",
                "person-ordarr-owner",
                "Tenant-isolated order"),
            "tenant-isolation-001");

        Assert.NotNull(store.GetOrder(principalOne, order.OrderId));
        Assert.Null(store.GetOrder(principalTwo, order.OrderId));
        Assert.DoesNotContain(store.ListOrders(principalTwo), item => item.OrderId == order.OrderId);

        var dashboard = store.GetDashboard(principalTwo);
        var summary = store.GetReportSummary(principalTwo);

        Assert.Equal(0, dashboard.OrderCount);
        Assert.Equal(0, summary.OrderCount);
    }

    [Fact]
    public void Holds_block_readiness_until_released()
    {
        using var db = CreateDb();
        var store = new OrdArrStore(db);
        var principal = CreatePrincipal();
        var order = store.CreateOrder(
            principal,
            new OrdArrCreateOrderRequest(
                new StlProductObjectReference("customarr", "customer", "cust-9002", "CUST-9002"),
                "Litmus Labs",
                "customer_order",
                "person-ordarr-owner",
                "Compliance hold readiness check"),
            "test-hold-order-001");

        var held = store.AddHold(
            principal,
            order.OrderId,
            new OrdArrHoldRequest("compliance", "Waiting on SDS verification", "compliancecore", "ordarr.order_requests.update", "Manual review", principal.FindFirstValue(StlClaimTypes.PersonId)),
            "test-hold-001");

        Assert.NotNull(held);

        var readiness = store.GetIntegrationReadiness(principal, order.OrderId);
        Assert.NotNull(readiness);
        Assert.False(readiness!.IsReady);
        Assert.Contains(readiness.BlockingReasons, reason => reason.StartsWith("hold:compliance", StringComparison.OrdinalIgnoreCase));

        var hold = held!.Holds.Single(item => item.Status == "open");
        var released = store.ReleaseHold(
            principal,
            order.OrderId,
            hold.HoldId,
            new OrdArrReleaseHoldRequest("Cleared", principal.FindFirstValue(StlClaimTypes.PersonId)),
            "test-hold-release-001");

        Assert.NotNull(released);
        var postRelease = store.GetIntegrationReadiness(principal, order.OrderId);
        Assert.DoesNotContain(postRelease!.BlockingReasons, reason => reason.StartsWith("hold:", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ApproveOrder_creates_handoffs_and_marks_release_state()
    {
        using var db = CreateDb();
        var store = new OrdArrStore(db);
        var principal = CreatePrincipal();
        var order = store.CreateOrder(
            principal,
            new OrdArrCreateOrderRequest(
                new StlProductObjectReference("customarr", "customer", "cust-9006", "CUST-9006"),
                "Northwind Maintenance",
                "customer_order",
                "person-ordarr-owner",
                "Replacement pump kit"),
            "test-approve-001");

        var approved = store.AcceptOrder(
            principal,
            order.OrderId,
            new OrdArrAcceptOrderRequest(
                DateTimeOffset.UtcNow.AddDays(2),
                DateTimeOffset.UtcNow.AddDays(3),
                ["loadarr", "routarr"],
                "Approved for downstream execution"),
            "test-approve-001");

        Assert.NotNull(approved);
        Assert.Equal("approved", approved!.ApprovalState);
        Assert.Equal("accepted", approved.LifecycleStatus);
        Assert.NotEmpty(approved.Handoffs);
        Assert.Equal(2, approved.Handoffs.Count);
    }

    [Fact]
    public void Returns_create_basic_rma_records()
    {
        using var db = CreateDb();
        var store = new OrdArrStore(db);
        var principal = CreatePrincipal();
        var order = store.CreateOrder(
            principal,
            new OrdArrCreateOrderRequest(
                new StlProductObjectReference("customarr", "customer", "cust-9007", "CUST-9007"),
                "Litmus Labs",
                "customer_order",
                "person-ordarr-owner",
                "Replacement filters"),
            "test-return-order-001");

        var returnRecord = store.CreateReturn(
            principal,
            order.OrderId,
            new OrdArrReturnRequest("rma", "Damaged on arrival", 1, ["line-1"], "Replace with new unit", "supplyarr.return"),
            "test-return-001");

        Assert.NotNull(returnRecord);
        Assert.StartsWith("RMA-2026", returnRecord!.ReturnNumber);
        Assert.Equal("requested", returnRecord.Status);

        var returns = store.ListOrderReturns(principal, order.OrderId);
        Assert.Contains(returns, item => item.ReturnId == returnRecord.ReturnId);
    }

    [Fact]
    public void Completion_packets_advance_closeout_and_finance_states()
    {
        using var db = CreateDb();
        var store = new OrdArrStore(db);
        var principal = CreatePrincipal();
        var order = store.CreateOrder(
            principal,
            new OrdArrCreateOrderRequest(
                new StlProductObjectReference("customarr", "customer", "cust-9008", "CUST-9008"),
                "Northwind Maintenance",
                "customer_order",
                "person-ordarr-owner",
                "Completion packet readiness check"),
            "test-packet-order-001");

        var approved = store.AcceptOrder(
            principal,
            order.OrderId,
            new OrdArrAcceptOrderRequest(
                null,
                null,
                null,
                "Approved for packet testing"),
            "test-packet-approve-001");

        Assert.NotNull(approved);
        Assert.Equal("approved", approved!.ApprovalState);

        var completionReady = store.UpsertCompletionPacket(
            principal,
            order.OrderId,
            new OrdArrCompletionPacketRequest("completion"),
            "test-packet-completion-001");

        Assert.NotNull(completionReady);
        Assert.Equal("ready", completionReady!.CompletionState);
        Assert.Equal("not_ready", completionReady.FinancialPacketState);
        Assert.Single(completionReady.CompletionPackets, packet => packet.PacketType == "completion");
        Assert.Single(completionReady.CompletionPackets, packet => packet.Status == "ready");

        var invoiceReady = store.UpsertCompletionPacket(
            principal,
            order.OrderId,
            new OrdArrCompletionPacketRequest("invoice_ready"),
            "test-packet-invoice-001");

        Assert.NotNull(invoiceReady);
        Assert.Equal("in_progress", invoiceReady!.FinancialPacketState);
        Assert.Contains(invoiceReady.CompletionPackets, packet => packet.PacketType == "invoice_ready" && packet.Status == "ready");

        var billReady = store.UpsertCompletionPacket(
            principal,
            order.OrderId,
            new OrdArrCompletionPacketRequest("bill_ready"),
            "test-packet-bill-001");

        Assert.NotNull(billReady);
        Assert.Equal("ready", billReady!.FinancialPacketState);
        Assert.Equal(3, billReady.CompletionPackets.Count);
        Assert.Contains(billReady.CompletionPackets, packet => packet.PacketType == "completion" && packet.Status == "ready");
        Assert.Contains(billReady.CompletionPackets, packet => packet.PacketType == "invoice_ready" && packet.Status == "ready");
        Assert.Contains(billReady.CompletionPackets, packet => packet.PacketType == "bill_ready" && packet.Status == "ready");
    }

    [Fact]
    public void Seeded_dashboard_and_report_summary_surface_operational_counts()
    {
        using var db = CreateDb();
        var store = new OrdArrStore(db);
        var principal = CreatePrincipal();

        store.CreateOrder(
            principal,
            new OrdArrCreateOrderRequest(
                new StlProductObjectReference("customarr", "customer", "cust-9003", "CUST-9003"),
                "Northwind Maintenance",
                "customer_order",
                "person-ordarr-owner",
                "Replacement filters",
                Lines:
                [
                    new OrdArrOrderLineRequest(
                        "item",
                        new StlProductObjectReference("supplyarr", "part", "part-filters", "PF-9003"),
                        "Replacement filters",
                        1,
                        "ea",
                        "loadarr",
                        DateTimeOffset.UtcNow.AddDays(1),
                        DateTimeOffset.UtcNow.AddDays(1),
                        25m,
                        0m,
                        true,
                        true,
                        true,
                        true,
                        "none",
                        "loadarr.fulfillment"),
                ]),
            "test-dashboard-001");

        store.CreateOrder(
            principal,
            new OrdArrCreateOrderRequest(
                new StlProductObjectReference("customarr", "customer", "cust-9004", "CUST-9004"),
                "Apex Foods",
                "service_request",
                "person-ordarr-owner",
                "Preventive maintenance visit"),
            "test-dashboard-002");

        store.CreateOrder(
            principal,
            new OrdArrCreateOrderRequest(
                new StlProductObjectReference("customarr", "customer", "cust-9005", "CUST-9005"),
                "Litmus Labs",
                "customer_order",
                "person-ordarr-owner",
                "Rush replacement"),
            "test-dashboard-003");

        var dashboard = store.GetDashboard(principal);
        var report = store.GetReportSummary(principal);

        Assert.Equal(3, dashboard.OrderCount);
        Assert.Equal(3, report.OrderCount);
        Assert.NotEmpty(dashboard.FeaturedOrders);
        Assert.NotEmpty(report.FeaturedOrders);
    }

    [Fact]
    public void Orders_and_idempotency_survive_store_recreation()
    {
        var dbName = $"ordarr-persistence-{Guid.NewGuid():N}";
        var principal = CreatePrincipal();
        string orderId;

        using (var db = CreateDb(dbName))
        {
            var store = new OrdArrStore(db);
            var order = store.CreateOrder(
                principal,
                new OrdArrCreateOrderRequest(
                    new StlProductObjectReference("customarr", "customer", "cust-persist-1", "CUST-PERSIST-1"),
                    "Persisted Customer",
                    "customer_order",
                    "person-ordarr-owner",
                    "Persist this order across store instances"),
                "test-persist-order-001");
            orderId = order.OrderId;
        }

        using (var db = CreateDb(dbName))
        {
            var recreatedStore = new OrdArrStore(db);
            var persisted = recreatedStore.GetOrder(principal, orderId);
            Assert.NotNull(persisted);
            Assert.Equal("Persisted Customer", persisted!.CustomerName);

            var replay = recreatedStore.CreateOrder(
                principal,
                new OrdArrCreateOrderRequest(
                    new StlProductObjectReference("customarr", "customer", "cust-persist-1", "CUST-PERSIST-1"),
                    "Persisted Customer",
                    "customer_order",
                    "person-ordarr-owner",
                    "Duplicate replay should not create a second order"),
                "test-persist-order-001");

            Assert.Equal(orderId, replay.OrderId);
            Assert.Single(recreatedStore.ListOrders(principal));
        }
    }

    private static OrdArrDbContext CreateDb(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<OrdArrDbContext>()
            .UseInMemoryDatabase(dbName ?? $"ordarr-store-{Guid.NewGuid():N}")
            .Options;

        return new OrdArrDbContext(options);
    }

    private static ClaimsPrincipal CreatePrincipal(string? tenantId = null)
    {
        var userId = Guid.NewGuid().ToString();
        var personId = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(StlClaimTypes.TenantId, tenantId ?? Guid.NewGuid().ToString()),
            new(StlClaimTypes.SessionId, Guid.NewGuid().ToString()),
            new(StlClaimTypes.TenantRoleKey, "ordarr-ops"),
            new(StlClaimTypes.PlatformAdmin, "true"),
            new(StlClaimTypes.PersonId, personId),
            new(StlClaimTypes.LaunchableProductKeys, "ordarr"),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }
}

