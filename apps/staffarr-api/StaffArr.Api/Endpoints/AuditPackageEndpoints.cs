using StaffArr.Api.Contracts;
using StaffArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace StaffArr.Api.Endpoints;

public static class AuditPackageEndpoints
{
    public static void MapStaffArrAuditPackageEndpoints(this WebApplication app)
    {
        var packages = app.MapGroup("/api/audit-packages")
            .WithTags("AuditPackages")
            .RequireAuthorization();

        packages.MapGet("/manifest", (
            StaffArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            return Results.Ok(service.GetManifest());
        })
        .WithName("GetStaffArrAuditPackageManifest");

        packages.MapGet("/timeline", async (
            DateTimeOffset? from,
            DateTimeOffset? to,
            int? page,
            int? pageSize,
            StaffArrAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            var tenantId = context.User.GetTenantId();
            var result = await service.ListAuditTimelineAsync(
                tenantId,
                from,
                to,
                page ?? 1,
                pageSize ?? 25,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetStaffArrAuditPackageTimeline");

        packages.MapGet("/export", async (
            string? format,
            DateTimeOffset? from,
            DateTimeOffset? to,
            StaffArrAuthorizationService authorization,
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
                $"staffarr-audit-package-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        })
        .WithName("ExportStaffArrAuditPackage");

        packages.MapPost("/jobs", async (
            CreateAuditPackageGenerationJobRequest request,
            StaffArrAuthorizationService authorization,
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
        .WithName("CreateStaffArrAuditPackageGenerationJob");

        packages.MapGet("/jobs/{jobId:guid}", async (
            Guid jobId,
            StaffArrAuthorizationService authorization,
            AuditPackageGenerationService generationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var job = await generationService.GetJobAsync(tenantId, jobId, cancellationToken);
            return Results.Ok(job);
        })
        .WithName("GetStaffArrAuditPackageGenerationJob");

        packages.MapGet("/jobs/{jobId:guid}/download", async (
            Guid jobId,
            StaffArrAuthorizationService authorization,
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
        .WithName("DownloadStaffArrAuditPackageGenerationJob");
    }
}
