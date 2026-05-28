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

        packages.MapGet("/filter-options", async (
            Guid? tenantId,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageService service,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            return Results.Ok(await service.GetFilterOptionsAsync(
                new PlatformAuditPackageFilter(TenantId: tenantId),
                cancellationToken));
        })
        .WithName("GetPlatformAuditPackageFilterOptions");

        packages.MapGet("/summary", async (
            Guid? tenantId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            string? productKey,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageService service,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            return Results.Ok(await service.GetExportSummaryAsync(
                BuildFilter(tenantId, from, to, action, result, targetType, actorUserId, productKey),
                cancellationToken));
        })
        .WithName("GetPlatformAuditPackageExportSummary");

        packages.MapGet("/timeline", async (
            Guid? tenantId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            string? productKey,
            int? page,
            int? pageSize,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageService service,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var resultPage = await service.ListAuditTimelineAsync(
                BuildFilter(tenantId, from, to, action, result, targetType, actorUserId, productKey),
                page ?? 1,
                pageSize ?? 25,
                cancellationToken);
            return Results.Ok(resultPage);
        })
        .WithName("GetPlatformAuditPackageTimeline");

        packages.MapGet("/export", async (
            string? format,
            Guid? tenantId,
            DateTimeOffset? from,
            DateTimeOffset? to,
            string? action,
            string? result,
            string? targetType,
            Guid? actorUserId,
            string? productKey,
            HttpContext context,
            PlatformAuthorizationService authorization,
            PlatformAuditPackageService service,
            CancellationToken cancellationToken) =>
        {
            await authorization.RequirePlatformAdminAsync(context.User, cancellationToken);
            var actorUserIdClaim = context.User.GetUserId();
            var filter = BuildFilter(tenantId, from, to, action, result, targetType, actorUserId, productKey);

            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            {
                var package = await service.BuildExportAsync(filter, actorUserIdClaim, cancellationToken);
                return Results.Ok(package);
            }

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            {
                var csvBytes = await service.ExportAuditEventsCsvAsync(filter, actorUserIdClaim, cancellationToken);
                return Results.File(
                    csvBytes,
                    "text/csv",
                    $"nexarr-platform-audit-events-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
            }

            var zipBytes = await service.ExportZipAsync(filter, actorUserIdClaim, cancellationToken);
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

    private static PlatformAuditPackageFilter BuildFilter(
        Guid? tenantId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        string? action,
        string? result,
        string? targetType,
        Guid? actorUserId,
        string? productKey) =>
        new(
            TenantId: tenantId,
            From: from,
            To: to,
            Action: action,
            Result: result,
            TargetType: targetType,
            ActorUserId: actorUserId,
            ProductKey: productKey);
}
