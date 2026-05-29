using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class CompanionFieldWorkOrderEndpoints
{
    public static void MapCompanionFieldWorkOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/companion/field-tasks/work-order")
            .WithTags("CompanionFieldWorkOrder")
            .RequireAuthorization();

        group.MapGet("/", async (
            string taskKey,
            CompanionFieldWorkOrderService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var accessToken = ExtractBearerToken(context);
            return Results.Ok(await service.GetDetailAsync(
                context.User,
                accessToken,
                taskKey,
                cancellationToken));
        })
        .WithName("GetCompanionFieldWorkOrderDetail");

        group.MapPost("/status", async (
            UpdateCompanionFieldWorkOrderStatusRequest request,
            CompanionFieldWorkOrderService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var accessToken = ExtractBearerToken(context);
            return Results.Ok(await service.UpdateStatusAsync(
                context.User,
                accessToken,
                request,
                cancellationToken));
        })
        .WithName("UpdateCompanionFieldWorkOrderStatus");

        group.MapPost("/labor", async (
            LogCompanionFieldWorkOrderLaborRequest request,
            CompanionFieldWorkOrderService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var accessToken = ExtractBearerToken(context);
            return Results.Ok(await service.LogLaborAsync(
                context.User,
                accessToken,
                request,
                cancellationToken));
        })
        .WithName("LogCompanionFieldWorkOrderLabor");
    }

    private static string ExtractBearerToken(HttpContext context)
    {
        var authorization = context.Request.Headers.Authorization.ToString();
        var accessToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..].Trim()
            : string.Empty;

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new STLCompliance.Shared.Contracts.StlApiException(
                "auth.unauthorized",
                "Bearer access token is required.",
                401);
        }

        return accessToken;
    }
}
