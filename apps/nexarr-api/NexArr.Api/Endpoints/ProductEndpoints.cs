using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products").RequireAuthorization();

        group.MapGet("/", async (
            HttpContext context,
            ProductCatalogService service,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListAsync(context.User, page == 0 ? 1 : page, pageSize == 0 ? 50 : pageSize, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ListProducts");

        group.MapGet("/{productKey}", async (
            string productKey,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.GetAsync(context.User, productKey, cancellationToken));
        })
        .WithName("GetProduct");

        group.MapPost("/", async (
            CreateProductRequest request,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
        {
            var created = await service.CreateAsync(context.User, request, cancellationToken);
            return Results.Created($"/api/products/{created.ProductKey}", created);
        })
        .WithName("CreateProduct");

        group.MapPut("/{productKey}", async (
            string productKey,
            UpdateProductRequest request,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await service.UpdateAsync(context.User, productKey, request, cancellationToken));
        })
        .WithName("UpdateProduct");
    }
}
