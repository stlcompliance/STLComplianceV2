using NexArr.Api.Contracts;
using NexArr.Api.Services;

namespace NexArr.Api.Endpoints;

public static class CompanionFieldWorkOrderEndpoints
{
    public static void MapCompanionFieldWorkOrderEndpoints(this WebApplication app)
    {
        app.MapLegacyAndCanonical("/api/companion/field-tasks/work-order", "/api/v1/mobile/field-tasks/work-order", (group, isCanonical) =>
        {
            group.WithTags("FieldCompanion").RequireAuthorization();

            var getDetail = group.MapGet("/", async (
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
            });
            if (isCanonical)
            {
                getDetail.WithName("GetCompanionFieldWorkOrderDetail");
            }

            var updateStatus = group.MapPost("/status", async (
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
            });
            if (isCanonical)
            {
                updateStatus.WithName("UpdateCompanionFieldWorkOrderStatus");
            }

            var logLabor = group.MapPost("/labor", async (
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
            });
            if (isCanonical)
            {
                logLabor.WithName("LogCompanionFieldWorkOrderLabor");
            }
        });
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
