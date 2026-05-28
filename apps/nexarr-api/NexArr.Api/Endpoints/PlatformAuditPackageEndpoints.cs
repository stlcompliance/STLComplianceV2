using NexArr.Api.Contracts;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Endpoints;

public static class PlatformAuditPackageEndpoints
{
    public static void MapPlatformAuditPackageEndpoints(this WebApplication app)
    {
        var packages = app.MapGroup("/api/platform-admin/audit-packages")
            .WithTags("PlatformAuditPackages")
            .RequireAuthorization();

        packages.MapGet("/manifest", async (
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageService service,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            return Results.Ok(service.GetManifest());
        })
        .WithName("GetPlatformAuditPackageManifest");

        packages.MapGet("/timeline", async (
            Guid? tenantId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int? page,
            int? pageSize,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageService service,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var result = await service.ListAuditTimelineAsync(
                tenantId,
                from,
                to,
                page ?? 1,
                pageSize ?? 25,
                cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetPlatformAuditPackageTimeline");

        packages.MapGet("/export", async (
            string? format,
            Guid? tenantId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageService service,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
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
                $"nexarr-platform-audit-package-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        })
        .WithName("ExportPlatformAuditPackage");

        packages.MapPost("/jobs", async (
            CreatePlatformAuditPackageGenerationJobRequest request,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageGenerationService generationService,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var actorUserId = context.User.GetUserId();
            var job = await generationService.CreateJobAsync(actorUserId, request, cancellationToken);
            return Results.Accepted($"/api/platform-admin/audit-packages/jobs/{job.JobId}", job);
        })
        .WithName("CreatePlatformAuditPackageGenerationJob");

        packages.MapGet("/jobs/{jobId:guid}", async (
            Guid jobId,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageGenerationService generationService,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var job = await generationService.GetJobAsync(jobId, cancellationToken);
            return Results.Ok(job);
        })
        .WithName("GetPlatformAuditPackageGenerationJob");

        packages.MapGet("/jobs/{jobId:guid}/download", async (
            Guid jobId,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageGenerationService generationService,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var job = await generationService.GetJobAsync(jobId, cancellationToken);

            if (string.Equals(job.Format, "json", StringComparison.OrdinalIgnoreCase))
            {
                var package = await generationService.DownloadJsonAsync(jobId, cancellationToken);
                return Results.Ok(package);
            }

            var (content, contentType, fileName) = await generationService.DownloadZipAsync(jobId, cancellationToken);
            return Results.File(content, contentType, fileName);
        })
        .WithName("DownloadPlatformAuditPackageGenerationJob");
    }
}
