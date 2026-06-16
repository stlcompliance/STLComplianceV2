using OrdArr.Api.Data;

namespace OrdArr.Api.Endpoints;

public static class WorkspaceEndpoints
{
    public static void MapOrdArrWorkspaceEndpoints(this WebApplication app)
    {
        var workspace = app.MapGroup("/api/v1/workspace").WithTags("Workspace").RequireAuthorization();
        var orders = app.MapGroup("/api/v1/orders").WithTags("Orders").RequireAuthorization();
        var integrations = app.MapGroup("/api/v1/integrations").WithTags("Integrations").RequireAuthorization();

        workspace.MapGet("/summary", (HttpContext context, OrdArrStore store) =>
            Results.Ok(store.GetDashboard(context.User)))
            .WithName("GetOrdArrWorkspaceSummary");

        workspace.MapGet("/orders", (HttpContext context, string? status, OrdArrStore store) =>
            Results.Ok(store.ListOrders(context.User, status)))
            .WithName("ListOrdArrWorkspaceOrders");

        workspace.MapGet("/orders/{orderId}", (HttpContext context, string orderId, OrdArrStore store) =>
        {
            var order = store.GetOrder(context.User, orderId);
            return order is null ? Results.NotFound() : Results.Ok(order);
        }).WithName("GetOrdArrWorkspaceOrder");

        workspace.MapGet("/handoffs", (HttpContext context, OrdArrStore store) =>
            Results.Ok(store.ListHandoffs(context.User)))
            .WithName("ListOrdArrWorkspaceHandoffs");

        workspace.MapGet("/completion-packets", (HttpContext context, OrdArrStore store) =>
            Results.Ok(store.ListCompletionPackets(context.User)))
            .WithName("ListOrdArrWorkspaceCompletionPackets");

        orders.MapGet("/", (HttpContext context, string? status, OrdArrStore store) =>
            Results.Ok(store.ListOrders(context.User, status)))
            .WithName("ListOrdArrOrders");

        orders.MapGet("/{orderId}", (HttpContext context, string orderId, OrdArrStore store) =>
        {
            var order = store.GetOrder(context.User, orderId);
            return order is null ? Results.NotFound() : Results.Ok(order);
        }).WithName("GetOrdArrOrder");

        orders.MapPost("/", (HttpContext context, OrdArrCreateOrderRequest request, OrdArrStore store) =>
        {
            var order = store.CreateOrder(context.User, request, context.Request.Headers["Idempotency-Key"].ToString());
            return Results.Created($"/api/v1/orders/{order.OrderId}", order);
        }).WithName("CreateOrdArrOrder");

        orders.MapPost("/{orderId}/accept", (HttpContext context, string orderId, OrdArrAcceptOrderRequest request, OrdArrStore store) =>
        {
            var order = store.AcceptOrder(context.User, orderId, request, context.Request.Headers["Idempotency-Key"].ToString());
            return order is null ? Results.NotFound() : Results.Ok(order);
        }).WithName("AcceptOrdArrOrder");

        orders.MapPost("/{orderId}/cancel", (HttpContext context, string orderId, OrdArrCancelOrderRequest request, OrdArrStore store) =>
        {
            var order = store.CancelOrder(context.User, orderId, request, context.Request.Headers["Idempotency-Key"].ToString());
            return order is null ? Results.NotFound() : Results.Ok(order);
        }).WithName("CancelOrdArrOrder");

        integrations.MapGet("/orders/{orderId}/readiness", (HttpContext context, string orderId, OrdArrStore store) =>
        {
            var readiness = store.GetIntegrationReadiness(context.User, orderId);
            return readiness is null ? Results.NotFound() : Results.Ok(readiness);
        }).WithName("GetOrdArrIntegrationOrderReadiness");
    }
}
