using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Services;
using STLCompliance.Shared.Auth;

namespace ComplianceCore.Api.Endpoints;

public static class AuditPackageEndpoints
{
    public static void MapComplianceCoreAuditPackageEndpoints(this WebApplication app)
    {
        var packages = app.MapGroup("/api/audit-packages")
            .WithTags("AuditPackages")
            .RequireAuthorization();

        packages.MapGet("/manifest", (
            ComplianceCoreAuthorizationService authorization,
            AuditPackageService service,
            HttpContext context) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            return Results.Ok(service.GetManifest());
        })
        .WithName("GetAuditPackageManifest");

        packages.MapGet("/export", async (
            string? format,
            DateTimeOffset? from,
            DateTimeOffset? to,
            ComplianceCoreAuthorizationService authorization,
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
                $"compliancecore-audit-package-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
        })
        .WithName("ExportAuditPackage");

        packages.MapPost("/jobs", async (
            CreateAuditPackageGenerationJobRequest request,
            ComplianceCoreAuthorizationService authorization,
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
        .WithName("CreateComplianceCoreAuditPackageGenerationJob");

        packages.MapGet("/jobs/{jobId:guid}", async (
            Guid jobId,
            ComplianceCoreAuthorizationService authorization,
            AuditPackageGenerationService generationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageExport(context.User);
            var tenantId = context.User.GetTenantId();
            var job = await generationService.GetJobAsync(tenantId, jobId, cancellationToken);
            return Results.Ok(job);
        })
        .WithName("GetComplianceCoreAuditPackageGenerationJob");

        packages.MapGet("/jobs/{jobId:guid}/download", async (
            Guid jobId,
            ComplianceCoreAuthorizationService authorization,
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
        .WithName("DownloadComplianceCoreAuditPackageGenerationJob");
    }
}
