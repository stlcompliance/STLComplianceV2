using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OrdArr.Api.Data;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.OrdArr.Auth.Tests;

public sealed class OrdArrStoreTests
{
    [Fact]
    public void CreateOrder_builds_order_lines_and_summary_fields()
    {
        var store = new OrdArrStore();
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
        Assert.Equal("customer_portal", order.SourceChannel);
        Assert.Equal("high", order.Priority);
        Assert.Single(order.Lines);
        Assert.Equal(2, order.Timeline.Count);

        var summary = store.ListOrders(principal).Single(item => item.OrderId == order.OrderId);
        Assert.Equal(1, summary.LineCount);
        Assert.Equal("Submit order", summary.NextAction);
    }

    [Fact]
    public void Holds_block_readiness_until_released()
    {
        var store = new OrdArrStore();
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
        var store = new OrdArrStore();
        var principal = CreatePrincipal();
        var order = store.ListOrders(principal).First(item => item.LifecycleStatus is "draft" or "submitted");

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
        var store = new OrdArrStore();
        var principal = CreatePrincipal();
        var order = store.ListOrders(principal).First();

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
    public void Seeded_dashboard_and_report_summary_surface_operational_counts()
    {
        var store = new OrdArrStore();
        var principal = CreatePrincipal();

        var dashboard = store.GetDashboard(principal);
        var report = store.GetReportSummary(principal);

        Assert.True(dashboard.OrderCount >= 3);
        Assert.True(report.OrderCount >= 3);
        Assert.NotEmpty(dashboard.FeaturedOrders);
        Assert.NotEmpty(report.FeaturedOrders);
    }

    private static ClaimsPrincipal CreatePrincipal()
    {
        var userId = Guid.NewGuid().ToString();
        var personId = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(StlClaimTypes.TenantId, Guid.NewGuid().ToString()),
            new(StlClaimTypes.SessionId, Guid.NewGuid().ToString()),
            new(StlClaimTypes.TenantRoleKey, "ordarr-ops"),
            new(StlClaimTypes.PlatformAdmin, "true"),
            new(StlClaimTypes.PersonId, personId),
            new(StlClaimTypes.Entitlements, "ordarr"),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }
}
