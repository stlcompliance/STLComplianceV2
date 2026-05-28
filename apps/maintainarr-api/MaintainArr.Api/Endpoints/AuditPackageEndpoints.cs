using MaintainArr.Api.Contracts;
using MaintainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace MaintainArr.Api.Endpoints;

public static class AuditPackageEndpoints
{
    public static void MapMaintainArrAuditPackageEndpoints(this WebApplication app)
    {
        var packages = app.MapGroup("/api/audit-packages")
            .WithTags("AuditPackages")
            .RequireAuthorization();

        packages.MapGet("/manifest", (
            MaintainArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            return Results.Ok(service.GetManifest());
        })
        .WithName("GetAuditPackageManifest");

        packages.MapGet("/filter-options", async (
            MaintainArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            var tenantId = context.User.GetTenantId();
            return Results.Ok(await service.GetFilterOptionsAsync(tenantId, cancellationToken));
        })
        .WithName("GetMaintainArrAuditPackageFilterOptions");

        packages.MapGet("/summary", async (
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            MaintainArrAuthorizationService authorization,
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
        .WithName("GetMaintainArrAuditPackageExportSummary");

        packages.MapGet("/timeline", async (
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            int? page,
            int? pageSize,
            MaintainArrAuthorizationService authorization,
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
        .WithName("GetMaintainArrAuditPackageTimeline");

        packages.MapGet("/export", async (
            string? format,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            MaintainArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actor = context.User.GetUserId();
            var filter = BuildFilter(from, to, action, result, targetType, actorUserId);

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            {
                var csvBytes = await service.ExportAuditEventsCsvAsync(
                    tenantId,
                    actor,
                    filter,
                    cancellationToken);
                return Results.File(
                    csvBytes,
                    "text/csv",
                    $"maintainarr-audit-events-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
            }

            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            {
                var package = await service.BuildExportAsync(
                    tenantId,
                    actor,
                    filter,
                    cancellationToken);
                return Results.Ok(package);
            }

            var zipBytes = await service.ExportZipAsync(
                tenantId,
                actor,
                filter,
                cancellationToken);
            return Results.File(
                zipBytes,
                "application/zip",
                $"maintainarr-audit-package-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        })
        .WithName("ExportAuditPackage");

        packages.MapPost("/jobs", async (
            CreateAuditPackageGenerationJobRequest request,
            MaintainArrAuthorizationService authorization,
            AuditPackageGenerationService generationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var actorUserId = context.User.GetUserId();
            var job = await generationService.CreateJobAsync(
                tenantId,
                actorUserId,
                request,
                cancellationToken);
            return Results.Accepted($"/api/audit-packages/jobs/{job.JobId}", job);
        })
        .WithName("CreateMaintainArrAuditPackageGenerationJob");

        packages.MapGet("/jobs/{jobId:guid}", async (
            Guid jobId,
            MaintainArrAuthorizationService authorization,
            AuditPackageGenerationService generationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var job = await generationService.GetJobAsync(tenantId, jobId, cancellationToken);
            return Results.Ok(job);
        })
        .WithName("GetMaintainArrAuditPackageGenerationJob");

        packages.MapGet("/jobs/{jobId:guid}/download", async (
            Guid jobId,
            MaintainArrAuthorizationService authorization,
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
        .WithName("DownloadMaintainArrAuditPackageGenerationJob");
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
