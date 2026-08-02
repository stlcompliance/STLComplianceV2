using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using STLCompliance.Shared.Integration;

namespace STLCompliance.Shared.Endpoints;

public static class StlProductLaunchEndpoints
{
    public static void MapStlProductLaunchEndpoints(this WebApplication app)
    {
        static async Task<IResult> GetLaunchContextAsync(
            string productKey,
            HttpContext context,
            StlNexArrLaunchClient client,
            CancellationToken cancellationToken)
        {
            var (statusCode, body, _) = await client.ForwardAsync(
                HttpMethod.Get,
                $"/api/launch/context?productKey={Uri.EscapeDataString(productKey)}",
                context.Request.Headers.Authorization.ToString(),
                null,
                cancellationToken);

            return Results.Content(body, "application/json", statusCode: statusCode);
        }

        static async Task<IResult> GetLaunchCatalogAsync(
            string? currentProductKey,
            HttpContext context,
            StlNexArrLaunchClient client,
            CancellationToken cancellationToken)
        {
            var path = "/api/launch/catalog";
            if (!string.IsNullOrWhiteSpace(currentProductKey))
            {
                path += $"?currentProductKey={Uri.EscapeDataString(currentProductKey)}";
            }

            var (statusCode, body, contentType) = await client.ForwardAsync(
                HttpMethod.Get,
                path,
                context.Request.Headers.Authorization.ToString(),
                null,
                cancellationToken);

            return Results.Content(body, contentType, statusCode: statusCode);
        }

        static async Task<IResult> CreateLaunchHandoffAsync(
            HttpContext context,
            StlNexArrLaunchClient client,
            CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(context.Request.Body);
            var jsonBody = await reader.ReadToEndAsync(cancellationToken);

            var (statusCode, body, _) = await client.ForwardAsync(
                HttpMethod.Post,
                "/api/launch/handoff",
                context.Request.Headers.Authorization.ToString(),
                jsonBody,
                cancellationToken);

            return Results.Content(body, "application/json", statusCode: statusCode);
        }

        var group = app.MapGroup("/api/launch").WithTags("Launch").RequireAuthorization();

        group.MapGet("/context", GetLaunchContextAsync)
        .WithName("GetProductLaunchContext");

        group.MapGet("/catalog", GetLaunchCatalogAsync)
            .WithName("GetProductLaunchCatalog");

        group.MapPost("/handoff", CreateLaunchHandoffAsync)
        .WithName("CreateProductLaunchHandoff");

        var v1Group = app.MapGroup("/api/v1/launch").WithTags("Launch").RequireAuthorization();

        v1Group.MapGet("/context", GetLaunchContextAsync)
            .WithName("GetProductLaunchContextV1");

        v1Group.MapGet("/catalog", GetLaunchCatalogAsync)
            .WithName("GetProductLaunchCatalogV1");

        v1Group.MapPost("/handoff", CreateLaunchHandoffAsync)
            .WithName("CreateProductLaunchHandoffV1");
    }
}
