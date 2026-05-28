using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class ReadinessForecastEndpoints
{
    public static void MapComplianceCoreReadinessForecastEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/readiness-forecasts")
            .WithTags("ReadinessForecasts")
            .RequireAuthorization();

        group.MapGet("/summary", async (
            ComplianceCoreAuthorizationService authorization,
            ReadinessForecastService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessForecastRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSummaryAsync(tenantId, cancellationToken));
        })
        .WithName("GetReadinessForecastSummary");

        group.MapGet("/", async (
            string? scopeKey,
            string? rulePackKey,
            string? readinessLevel,
            Guid? runId,
            int? limit,
            ComplianceCoreAuthorizationService authorization,
            ReadinessForecastService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessForecastRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.ListForecastsAsync(
                tenantId,
                scopeKey,
                rulePackKey,
                readinessLevel,
                runId,
                limit,
                cancellationToken));
        })
        .WithName("ListReadinessForecasts");

        group.MapPost("/evaluate", async (
            EvaluateReadinessForecastRequest request,
            ComplianceCoreAuthorizationService authorization,
            ReadinessForecastService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireReadinessForecastEvaluate(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            return Results.Ok(await service.EvaluateAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken));
        })
        .WithName("EvaluateReadinessForecast");
    }
}
