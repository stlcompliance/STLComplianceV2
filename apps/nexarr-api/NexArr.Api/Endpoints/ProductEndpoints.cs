using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products").RequireAuthorization();
        var v1 = app.MapGroup("/api/v1/products").WithTags("Products").RequireAuthorization();

        static async Task<IResult> ListProductsEndpoint(
            HttpContext context,
            ProductCatalogService service,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var result = await service.ListAsync(context.User, page == 0 ? 1 : page, pageSize == 0 ? 50 : pageSize, cancellationToken);
            return Results.Ok(result);
        }

        group.MapGet("/", ListProductsEndpoint)
        .WithName("ListProducts");

        v1.MapGet("/", ListProductsEndpoint)
        .WithName("ListProductsV1");

        static async Task<IResult> GetProductEndpoint(
            string productKey,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAsync(context.User, productKey, cancellationToken));

        group.MapGet("/{productKey}", GetProductEndpoint)
        .WithName("GetProduct");

        v1.MapGet("/{productCode}", async (
            string productCode,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAsync(context.User, productCode, cancellationToken)))
        .WithName("GetProductV1");

        static async Task<IResult> CreateProductEndpoint(
            CreateProductRequest request,
            HttpContext context,
            ProductCatalogService service,
            string locationPrefix,
            CancellationToken cancellationToken)
        {
            var created = await service.CreateAsync(context.User, request, cancellationToken);
            return Results.Created($"{locationPrefix}/{created.ProductKey}", created);
        }

        group.MapPost("/", (
            CreateProductRequest request,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
            CreateProductEndpoint(request, context, service, "/api/products", cancellationToken))
        .WithName("CreateProduct");

        v1.MapPost("/", (
            CreateProductRequest request,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
            CreateProductEndpoint(request, context, service, "/api/v1/products", cancellationToken))
        .WithName("CreateProductV1");

        static async Task<IResult> UpdateProductEndpoint(
            string productKey,
            UpdateProductRequest request,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateAsync(context.User, productKey, request, cancellationToken));

        group.MapPut("/{productKey}", UpdateProductEndpoint)
        .WithName("UpdateProduct");

        v1.MapPatch("/{productCode}", async (
            string productCode,
            UpdateProductRequest request,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.UpdateAsync(context.User, productCode, request, cancellationToken)))
        .WithName("UpdateProductV1");

        v1.MapPost("/{productCode}/disable", async (
            string productCode,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.SetActiveAsync(context.User, productCode, false, cancellationToken)))
        .WithName("DisableProductV1");

        v1.MapPost("/{productCode}/enable", async (
            string productCode,
            HttpContext context,
            ProductCatalogService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.SetActiveAsync(context.User, productCode, true, cancellationToken)))
        .WithName("EnableProductV1");
    }
}
