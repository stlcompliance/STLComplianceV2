using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CustomArr.Api.Data;
using OrdArr.Api.Data;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Integration;

namespace STLCompliance.MaintainArr.Auth.Tests;

public sealed class OrdArrCustomArrHandoffTests
{
    private static readonly Guid TenantId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid UserId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private static readonly Guid PersonId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

    [Fact]
    public void CustomArr_portal_submission_hands_customer_reference_to_OrdArr_order()
    {
        var customArr = new CustomArrStore();
        var ordArr = new OrdArrStore();
        var customPrincipal = Principal("customarr");
        var ordPrincipal = Principal("ordarr");

        var submission = customArr.CreatePortalOrderSubmission(
            customPrincipal,
            new CustomArrPortalOrderSubmissionRequest(
                CustomerId: "cust-1001",
                CustomerName: "Acme Freight",
                RequestType: "service_request",
                OwnerPersonId: string.Empty,
                Summary: "Customer requested expedited yard service.",
                RequestedWindowStart: DateTimeOffset.Parse("2026-06-18T14:00:00Z"),
                RequestedWindowEnd: DateTimeOffset.Parse("2026-06-18T16:00:00Z"),
                PromisedWindowStart: DateTimeOffset.Parse("2026-06-20T14:00:00Z"),
                PromisedWindowEnd: DateTimeOffset.Parse("2026-06-20T16:00:00Z"),
                FulfillmentProductKeys: [StlProductKeys.RoutArr, StlProductKeys.LoadArr]),
            "portal-order-1");

        var order = ordArr.CreateOrder(
            ordPrincipal,
            new OrdArrCreateOrderRequest(
                submission.CustomerRef,
                submission.CustomerName,
                submission.RequestType,
                submission.OwnerPersonId,
                submission.Summary,
                submission.RequestedWindowStart,
                submission.RequestedWindowEnd,
                submission.PromisedWindowStart,
                submission.PromisedWindowEnd,
                submission.FulfillmentProductKeys),
            "portal-order-1");

        var forwarded = customArr.MarkPortalSubmissionForwarded(
            customPrincipal,
            submission.SubmissionId,
            order.OrderId,
            order.OrderNumber);

        Assert.Equal(StlSuiteEventCatalog.CustomArr.PortalSubmissionCreated, submission.CreatedEventType);
        Assert.Equal(StlProductKeys.CustomArr, order.CustomerRef.ProductKey);
        Assert.Equal("cust-1001", order.CustomerRef.ObjectId);
        Assert.Contains(order.Events, evt => evt.EventType == StlSuiteEventCatalog.OrdArr.OrderRequested);
        Assert.Contains(order.Events, evt => evt.EventType == StlSuiteEventCatalog.OrdArr.OrderCreated);
        Assert.Empty(order.Handoffs);
        Assert.NotNull(forwarded?.OrdArrOrderRef);
        Assert.Equal(StlProductKeys.OrdArr, forwarded!.OrdArrOrderRef!.ProductKey);
    }

    [Fact]
    public void OrdArr_acceptance_owns_downstream_fulfillment_handoffs_and_events()
    {
        var ordArr = new OrdArrStore();
        var principal = Principal("ordarr");
        var order = ordArr.CreateOrder(
            principal,
            new OrdArrCreateOrderRequest(
                new StlProductObjectReference(StlProductKeys.CustomArr, "customer", "cust-1001", "CUS-1001"),
                "Acme Freight",
                "customer_order",
                "person-100",
                "Move customer freight.",
                null,
                null,
                null,
                null,
                [StlProductKeys.RoutArr, StlProductKeys.LoadArr]),
            "order-create-1");

        var accepted = ordArr.AcceptOrder(
            principal,
            order.OrderId,
            new OrdArrAcceptOrderRequest(
                DateTimeOffset.Parse("2026-06-20T14:00:00Z"),
                DateTimeOffset.Parse("2026-06-20T18:00:00Z"),
                [StlProductKeys.RoutArr, StlProductKeys.LoadArr],
                "Ready for fulfillment"),
            "order-accept-1");

        Assert.NotNull(accepted);
        Assert.Equal("accepted", accepted!.LifecycleStatus);
        Assert.Equal("requested", accepted.HandoffState);
        Assert.Contains(accepted.Events, evt => evt.EventType == StlSuiteEventCatalog.OrdArr.OrderAccepted);
        Assert.Contains(accepted.Events, evt => evt.EventType == StlSuiteEventCatalog.OrdArr.OrderFulfillmentRequested);
        Assert.Contains(accepted.Handoffs, handoff => handoff.TargetProductKey == StlProductKeys.RoutArr);
        Assert.Contains(accepted.Handoffs, handoff => handoff.TargetProductKey == StlProductKeys.LoadArr);
        Assert.All(accepted.Handoffs, handoff => Assert.Equal("requested", handoff.State));
    }

    private static ClaimsPrincipal Principal(string entitlement)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, UserId.ToString("D")),
                new Claim(StlClaimTypes.TenantId, TenantId.ToString("D")),
                new Claim(StlClaimTypes.PersonId, PersonId.ToString("D")),
                new Claim(StlClaimTypes.SessionId, Guid.NewGuid().ToString("D")),
                new Claim(StlClaimTypes.TenantRoleKey, "tenant_admin"),
                new Claim(StlClaimTypes.PlatformAdmin, "false"),
                new Claim(StlClaimTypes.Entitlements, entitlement),
            ],
            "test");

        return new ClaimsPrincipal(identity);
    }
}
