using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using STLCompliance.Shared.Integration;

namespace STLCompliance.Shared.Endpoints;

public static class StlProductLaunchEndpoints
{
    public static void MapStlProductLaunchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/launch").WithTags("Launch").RequireAuthorization();

        group.MapGet("/context", async (
            string productKey,
            HttpContext context,
            StlNexArrLaunchClient client,
            CancellationToken cancellationToken) =>
        {
            var (statusCode, body, contentType) = await client.ForwardAsync(
                HttpMethod.Get,
                $"/api/launch/context?productKey={Uri.EscapeDataString(productKey)}",
                context.Request.Headers.Authorization.ToString(),
                null,
                cancellationToken);

            return Results.Content(body, contentType, statusCode: statusCode);
        })
        .WithName("GetProductLaunchContext");

        group.MapPost("/handoff", async (
            HttpContext context,
            StlNexArrLaunchClient client,
            CancellationToken cancellationToken) =>
        {
            using var reader = new StreamReader(context.Request.Body);
            var jsonBody = await reader.ReadToEndAsync(cancellationToken);

            var (statusCode, body, contentType) = await client.ForwardAsync(
                HttpMethod.Post,
                "/api/launch/handoff",
                context.Request.Headers.Authorization.ToString(),
                jsonBody,
                cancellationToken);

            return Results.Content(body, contentType, statusCode: statusCode);
        })
        .WithName("CreateProductLaunchHandoff");
    }
}
