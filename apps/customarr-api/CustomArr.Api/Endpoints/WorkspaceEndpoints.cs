using CustomArr.Api.Data;

namespace CustomArr.Api.Endpoints;

public static class WorkspaceEndpoints
{
    public static void MapCustomArrWorkspaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/workspace").WithTags("Workspace").RequireAuthorization();

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

        group.MapPost("/customers", (CustomArrCreateCustomerRequest request, CustomArrStore store) =>
        {
            var customer = store.CreateCustomer(request);
            return Results.Created($"/api/v1/workspace/customers/{customer.CustomerId}", customer);
        }).WithName("CreateCustomArrCustomer");

        group.MapGet("/requirements", (CustomArrStore store) =>
            Results.Ok(store.GetRequirements()))
            .WithName("ListCustomArrRequirements");
    }
}
