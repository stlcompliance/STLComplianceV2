using CustomArr.Api.Data;
using CustomArr.Api.Services;
using STLCompliance.Shared.Contracts;

namespace CustomArr.Api.Endpoints;

public static class WorkspaceEndpoints
{
    public static void MapCustomArrWorkspaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/workspace").WithTags("Workspace").RequireAuthorization();
        var portal = app.MapGroup("/api/v1/portal-submissions").WithTags("Portal submissions").RequireAuthorization();

        group.MapGet("/summary", (HttpContext context, CustomArrStore store) =>
            Results.Ok(store.GetDashboard(context.User)))
            .WithName("GetCustomArrWorkspaceSummary");

        group.MapGet("/customers", (HttpContext context, string? search, CustomArrStore store) =>
            Results.Ok(store.GetCustomers(context.User, search)))
            .WithName("ListCustomArrCustomers");

        group.MapGet("/customers/{customerId}", (HttpContext context, string customerId, CustomArrStore store) =>
        {
            var customer = store.GetCustomer(context.User, customerId);
            return customer is null ? Results.NotFound() : Results.Ok(customer);
        }).WithName("GetCustomArrCustomer");

        group.MapPost("/customers", (HttpContext context, CustomArrCreateCustomerRequest request, CustomArrStore store) =>
        {
            var idempotencyKey = context.Request.Headers["Idempotency-Key"].ToString();
            var customer = store.CreateCustomer(context.User, request, idempotencyKey);
            return Results.Created($"/api/v1/workspace/customers/{customer.CustomerId}", customer);
        }).WithName("CreateCustomArrCustomer");

        group.MapGet("/requirements", (CustomArrStore store) =>
            Results.Ok(store.GetRequirements()))
            .WithName("ListCustomArrRequirements");

        portal.MapGet("/", (HttpContext context, CustomArrStore store) =>
            Results.Ok(store.ListPortalSubmissions(context.User)))
            .WithName("ListCustomArrPortalSubmissions");

        portal.MapPost("/order", async (
            HttpContext context,
            CustomArrPortalOrderSubmissionRequest request,
            CustomArrStore store,
            CustomArrCrmWorkspaceService crm,
            OrdArrOrderRequestClient ordArr,
            CancellationToken cancellationToken) =>
        {
            var idempotencyKey = context.Request.Headers["Idempotency-Key"].ToString();
            var bearerToken = ResolveBearerToken(context);
            var eligibility = await crm.CheckEligibilityAsync(
                context.User,
                new CustomArrEligibilityCheckRequest(
                    request.CustomerId,
                    request.CustomerAddressId,
                    null,
                    "ordarr.order.create",
                    "customarr",
                    "portal_order_submission"),
                cancellationToken: cancellationToken);
            if (eligibility.ResultKey == "blocked")
            {
                throw new StlApiException(
                    "customarr.portal_order.customer_blocked",
                    $"Customer is blocked for portal order forwarding: {string.Join("; ", eligibility.Blockers)}",
                    409);
            }

            var submission = store.CreatePortalOrderSubmission(context.User, request, idempotencyKey);
            var order = await ordArr.CreateOrderAsync(submission, bearerToken, idempotencyKey, cancellationToken);
            var forwarded = store.MarkPortalSubmissionForwarded(context.User, submission.SubmissionId, order.OrderId, order.OrderNumber)
                ?? submission;

            return Results.Created(
                $"/api/v1/portal-submissions/{forwarded.SubmissionId}",
                new CustomArrPortalOrderHandoffResponse(forwarded, order));
        }).WithName("CreateCustomArrPortalOrderSubmission");
    }

    private static string ResolveBearerToken(HttpContext context)
    {
        var authorization = context.Request.Headers.Authorization.ToString();
        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorization["Bearer ".Length..].Trim();
        }

        throw new StlApiException("customarr.bearer_token_required", "A bearer token is required to forward portal order submissions to OrdArr.", 401);
    }
}
