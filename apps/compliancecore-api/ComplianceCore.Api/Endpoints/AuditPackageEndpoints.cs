using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Services;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Endpoints;

public static class AuditPackageEndpoints
{
    public static void MapComplianceCoreAuditPackageEndpoints(this WebApplication app)
    {
        var packages = app.MapGroup("/api/audit-packages")
            .WithTags("AuditPackages")
            .RequireAuthorization();
        var v1Audit = app.MapGroup("/api/v1/audit")
            .WithTags("AuditPackages")
            .RequireAuthorization();
        var events = app.MapGroup("/api/events")
            .WithTags("AuditPackages")
            .RequireAuthorization();
        var v1Events = app.MapGroup("/api/v1/events")
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

        static Task<IResult> ListAuditEventsAsync(
            int? page,
            int? pageSize,
            DateTimeOffset? from,
            DateTimeOffset? to,
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreDbContext db,
            HttpContext context,
            CancellationToken cancellationToken)
        {
            authorization.RequireAuditPackageRead(context.User);
            var tenantId = context.User.GetTenantId();
            var resolvedPage = page is null or < 1 ? 1 : page.Value;
            var resolvedPageSize = pageSize switch
            {
                null or < 1 => 25,
                > 100 => 100,
                _ => pageSize.Value
            };

            var query = db.AuditEvents.AsNoTracking().Where(x => x.TenantId == tenantId);
            if (from is not null)
            {
                query = query.Where(x => x.OccurredAt >= from.Value);
            }

            if (to is not null)
            {
                query = query.Where(x => x.OccurredAt <= to.Value);
            }

            return ExecuteListAsync(query, resolvedPage, resolvedPageSize, cancellationToken);
        }

        static async Task<IResult> ExecuteListAsync(
            IQueryable<ComplianceCore.Api.Entities.ComplianceCoreAuditEvent> query,
            int resolvedPage,
            int resolvedPageSize,
            CancellationToken cancellationToken)
        {
            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.OccurredAt)
                .Skip((resolvedPage - 1) * resolvedPageSize)
                .Take(resolvedPageSize)
                .Select(x => new AuditEventExportItem(
                    x.Id,
                    x.ActorUserId,
                    x.Action,
                    x.TargetType,
                    x.TargetId,
                    x.Result,
                    x.ReasonCode,
                    x.CorrelationId,
                    x.OccurredAt))
                .ToListAsync(cancellationToken);

            return Results.Ok(new PagedResult<AuditEventExportItem>(
                items,
                resolvedPage,
                resolvedPageSize,
                total,
                resolvedPage * resolvedPageSize < total));
        }

        v1Audit.MapGet("/events", ListAuditEventsAsync)
        .WithName("ListComplianceCoreAuditEventsV1");

        events.MapGet("/", ListAuditEventsAsync)
        .WithName("ListComplianceCoreEvents");

        v1Events.MapGet("/", ListAuditEventsAsync)
        .WithName("ListComplianceCoreEventsV1");

        /* v1 audit packages */
        v1Audit.MapPost("/packages", async (
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
            return Results.Accepted($"/api/v1/audit/packages/{job.JobId}", job);
        })
        .WithName("CreateComplianceCoreAuditPackageV1");

        v1Audit.MapGet("/packages", async (
            int? page,
            int? pageSize,
            ComplianceCoreAuthorizationService authorization,
            ComplianceCoreDbContext db,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            var tenantId = context.User.GetTenantId();
            var resolvedPage = page is null or < 1 ? 1 : page.Value;
            var resolvedPageSize = pageSize switch
            {
                null or < 1 => 25,
                > 100 => 100,
                _ => pageSize.Value
            };

            var query = db.AuditPackageGenerationJobs.AsNoTracking()
                .Where(x => x.TenantId == tenantId);
            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((resolvedPage - 1) * resolvedPageSize)
                .Take(resolvedPageSize)
                .Select(x => new AuditPackageGenerationJobResponse(
                    x.Id,
                    x.Status,
                    x.Format,
                    x.FromUtc,
                    x.ToUtc,
                    x.PackageId,
                    x.ErrorMessage,
                    x.CreatedAt,
                    x.StartedAt,
                    x.CompletedAt,
                    AuditPackageGenerationRules.IsDownloadReady(x)))
                .ToListAsync(cancellationToken);

            return Results.Ok(new PagedResult<AuditPackageGenerationJobResponse>(
                items,
                resolvedPage,
                resolvedPageSize,
                total,
                resolvedPage * resolvedPageSize < total));
        })
        .WithName("ListComplianceCoreAuditPackagesV1");

        v1Audit.MapGet("/packages/{jobId:guid}", async (
            Guid jobId,
            ComplianceCoreAuthorizationService authorization,
            AuditPackageGenerationService generationService,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            authorization.RequireAuditPackageRead(context.User);
            var tenantId = context.User.GetTenantId();
            var job = await generationService.GetJobAsync(tenantId, jobId, cancellationToken);
            return Results.Ok(job);
        })
        .WithName("GetComplianceCoreAuditPackageV1");

        v1Audit.MapGet("/packages/{jobId:guid}/download", async (
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
        .WithName("DownloadComplianceCoreAuditPackageV1");
    }
}
