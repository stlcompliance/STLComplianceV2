using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class AuditPackageEndpoints
{
    public static void MapTrainArrAuditPackageEndpoints(this WebApplication app)
    {
        var packages = app.MapGroup("/api/audit-packages")
            .WithTags("AuditPackages")
            .RequireAuthorization();

        packages.MapGet("/manifest", (
            TrainArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            return Results.Ok(service.GetManifest());
        })
        .WithName("GetTrainArrAuditPackageManifest");

        packages.MapGet("/export", async (
            string? format,
            DateTimeOffset? from,
            DateTimeOffset? to,
            TrainArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();

            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            {
                var package = await service.BuildExportAsync(
                    tenantId,
                    actorUserId,
                    from,
                    to,
                    cancellationToken);
                return Results.Ok(package);
            }

            var zipBytes = await service.ExportZipAsync(
                tenantId,
                actorUserId,
                from,
                to,
                cancellationToken);
            return Results.File(
                zipBytes,
                "application/zip",
                $"trainarr-audit-package-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        })
        .WithName("ExportTrainArrAuditPackage");
    }
}
