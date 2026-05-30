using TrainArr.Api.Contracts;
using TrainArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace TrainArr.Api.Endpoints;

public static class AuditPackageEndpoints
{
    public static void MapTrainArrAuditPackageEndpoints(this WebApplication app)
    {
        var routes = new[]
        {
            (Route: "/api/audit-packages", Suffix: string.Empty),
            (Route: "/api/v1/audit-packages", Suffix: "V1"),
        };

        foreach (var (route, suffix) in routes)
        {
            var packages = app.MapGroup(route)
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
        .WithName($"GetTrainArrAuditPackageManifest{suffix}");

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
        .WithName($"ExportTrainArrAuditPackage{suffix}");

        packages.MapPost("/jobs", async (
            CreateAuditPackageGenerationJobRequest request,
            TrainArrAuthorizationService authorization,
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
            return Results.Accepted($"{route}/jobs/{job.JobId}", job);
        })
        .WithName($"CreateTrainArrAuditPackageGenerationJob{suffix}");

        packages.MapGet("/jobs/{jobId:guid}", async (
            Guid jobId,
            TrainArrAuthorizationService authorization,
            AuditPackageGenerationService generationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var job = await generationService.GetJobAsync(tenantId, jobId, cancellationToken);
            return Results.Ok(job);
        })
        .WithName($"GetTrainArrAuditPackageGenerationJob{suffix}");

        packages.MapGet("/jobs/{jobId:guid}/download", async (
            Guid jobId,
            TrainArrAuthorizationService authorization,
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
        .WithName($"DownloadTrainArrAuditPackageGenerationJob{suffix}");
        }
    }
}
