using RoutArr.Api.Contracts;
using RoutArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RoutArr.Api.Endpoints;

public static class AuditPackageEndpoints
{
    public static void MapRoutArrAuditPackageEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/audit-packages"), string.Empty, "/api/audit-packages");
        MapRoutes(app.MapGroup("/api/v1/audit-packages"), "V1", "/api/v1/audit-packages");
    }

    private static void MapRoutes(RouteGroupBuilder packages, string nameSuffix, string routePrefix)
    {
        packages = packages
            .WithTags("AuditPackages")
            .RequireAuthorization();

        packages.MapGet("/manifest", (
            RoutArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            return Results.Ok(service.GetManifest());
        })
        .WithName($"GetRoutArrAuditPackageManifest{nameSuffix}");

        packages.MapGet("/filter-options", async (
            RoutArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetFilterOptionsAsync(tenantId, cancellationToken));
        })
        .WithName($"GetRoutArrAuditPackageFilterOptions{nameSuffix}");

        packages.MapGet("/summary", async (
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            RoutArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetExportSummaryAsync(
                tenantId,
                BuildFilter(from, to, action, result, targetType, actorUserId),
                cancellationToken));
        })
        .WithName($"GetRoutArrAuditPackageExportSummary{nameSuffix}");

        packages.MapGet("/timeline", async (
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            int? page,
            int? pageSize,
            RoutArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            var tenantId = context.User.GetTenantId();
            var resultPage = await service.ListAuditTimelineAsync(
                tenantId,
                BuildFilter(from, to, action, result, targetType, actorUserId),
                page ?? 1,
                pageSize ?? 25,
                cancellationToken);
            return Results.Ok(resultPage);
        })
        .WithName($"GetRoutArrAuditPackageTimeline{nameSuffix}");

        packages.MapGet("/export", async (
            string? format,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            RoutArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actor = context.User.GetUserId();
            var filter = BuildFilter(from, to, action, result, targetType, actorUserId);

            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            {
                var package = await service.BuildExportAsync(tenantId, actor, filter, cancellationToken);
                return Results.Ok(package);
            }

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            {
                var csvBytes = await service.ExportAuditEventsCsvAsync(tenantId, actor, filter, cancellationToken);
                return Results.File(
                    csvBytes,
                    "text/csv",
                    $"routarr-audit-events-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
            }

            var zipBytes = await service.ExportZipAsync(tenantId, actor, filter, cancellationToken);
            return Results.File(
                zipBytes,
                "application/zip",
                $"routarr-audit-package-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        })
        .WithName($"ExportRoutArrAuditPackage{nameSuffix}");

        packages.MapPost("/jobs", async (
            CreateAuditPackageGenerationJobRequest request,
            RoutArrAuthorizationService authorization,
            AuditPackageGenerationService generationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var job = await generationService.CreateJobAsync(tenantId, actorUserId, request, cancellationToken);
            return Results.Accepted($"{routePrefix}/jobs/{job.JobId}", job);
        })
        .WithName($"CreateRoutArrAuditPackageGenerationJob{nameSuffix}");

        packages.MapGet("/jobs/{jobId:guid}", async (
            Guid jobId,
            RoutArrAuthorizationService authorization,
            AuditPackageGenerationService generationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var job = await generationService.GetJobAsync(tenantId, jobId, cancellationToken);
            return Results.Ok(job);
        })
        .WithName($"GetRoutArrAuditPackageGenerationJob{nameSuffix}");

        packages.MapGet("/jobs/{jobId:guid}/download", async (
            Guid jobId,
            RoutArrAuthorizationService authorization,
            AuditPackageGenerationService generationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var job = await generationService.GetJobAsync(tenantId, jobId, cancellationToken);

            if (string.Equals(job.Format, "json", StringComparison.OrdinalIgnoreCase))
            {
                var package = await generationService.DownloadJsonAsync(tenantId, jobId, cancellationToken);
                return Results.Ok(package);
            }

            var (content, contentType, fileName) = await generationService.DownloadZipAsync(
                tenantId,
                jobId,
                cancellationToken);
            return Results.File(content, contentType, fileName);
        })
        .WithName($"DownloadRoutArrAuditPackageGenerationJob{nameSuffix}");
    }

    private static AuditPackageFilter BuildFilter(
        DateTimeOffset? from,
        DateTimeOffset? to,
        string? action,
        string? result,
        string? targetType,
        Guid? actorUserId) =>
        new(from, to, action, result, targetType, actorUserId);
}
