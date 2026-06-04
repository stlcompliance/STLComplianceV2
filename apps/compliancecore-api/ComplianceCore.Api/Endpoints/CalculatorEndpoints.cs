using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class CalculatorEndpoints
{
    public static void MapComplianceCoreCalculatorEndpoints(this WebApplication app)
    {
        MapTitle49Group(app.MapGroup("/api/calculators/title49")
            .WithTags("Title49Calculators")
            .RequireAuthorization(), string.Empty);

        MapTitle49Group(app.MapGroup("/api/v1/calculators/title49")
            .WithTags("Title49Calculators")
            .RequireAuthorization(), "V1");
    }

    private static void MapTitle49Group(RouteGroupBuilder group, string nameSuffix)
    {
        group.MapGet("/summary", async (
            string? sourceProduct,
            string? sourceEntity,
            ComplianceCoreAuthorizationService authorization,
            Title49CalculatorService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetSummaryAsync(tenantId, sourceProduct, sourceEntity, cancellationToken));
        })
        .WithName($"GetTitle49CalculatorSummary{nameSuffix}");

        group.MapGet("/summary/export", async (
            string? sourceProduct,
            string? sourceEntity,
            ComplianceCoreAuthorizationService authorization,
            Title49CalculatorService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireRegulatoryRead(context.User);
            var tenantId = context.User.GetTenantId();
            var export = await service.ExportSummaryCsvAsync(tenantId, sourceProduct, sourceEntity, cancellationToken);
            return Results.File(export.Content, export.ContentType, export.FileName);
        })
        .WithName($"ExportTitle49CalculatorSummary{nameSuffix}");
    }
}
