using System.Text.Json;
using MaintainArr.Api.Contracts;
using MaintainArr.Api.Data;
using MaintainArr.Api.Entities;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Contracts;

namespace MaintainArr.Api.Services;

public sealed class AuditPackageGenerationService(
    MaintainArrDbContext db,
    AuditPackageService auditPackageService,
    IMaintainArrAuditService auditService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    public const string ProcessJobsActionScope = "maintainarr.audit_packages.generate";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000fb");

    public async Task<AuditPackageGenerationJobResponse> CreateJobAsync(
        Guid tenantId,
        Guid requestedByUserId,
        CreateAuditPackageGenerationJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var format = AuditPackageGenerationRules.NormalizeFormat(request.Format);
        ValidateDateRange(request.From, request.To);

        var now = DateTimeOffset.UtcNow;
        var job = new AuditPackageGenerationJob
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequestedByUserId = requestedByUserId,
            Status = AuditPackageGenerationJobStatuses.Pending,
            Format = format,
            FromUtc = request.From,
            ToUtc = request.To,
            CreatedAt = now,
        };

        db.AuditPackageGenerationJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            "audit_package.generation.enqueued",
            tenantId,
            requestedByUserId,
            "audit_package_generation_job",
            job.Id.ToString(),
            "success",
            reasonCode: format,
            cancellationToken: cancellationToken);

        return MapResponse(job);
    }

    public async Task<AuditPackageGenerationJobResponse> GetJobAsync(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await db.AuditPackageGenerationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == jobId && x.TenantId == tenantId, cancellationToken)
            ?? throw new StlApiException(
                "audit_package_generation.job_not_found",
                "Audit package generation job was not found.",
                404);

        return MapResponse(job);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> DownloadZipAsync(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await LoadJobForDownloadAsync(tenantId, jobId, AuditPackageGenerationFormats.Zip, cancellationToken);
        return (
            job.ArtifactZip!,
            "application/zip",
            $"MaintainArr-audit-package-{job.PackageId:N}.zip");
    }

    public async Task<AuditPackageExportResponse> DownloadJsonAsync(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await LoadJobForDownloadAsync(tenantId, jobId, AuditPackageGenerationFormats.Json, cancellationToken);
        return JsonSerializer.Deserialize<AuditPackageExportResponse>(job.ArtifactJson!, JsonOptions)
            ?? throw new StlApiException(
                "audit_package_generation.artifact_invalid",
                "Stored JSON artifact could not be deserialized.",
                500);
    }

    public async Task<PendingAuditPackageGenerationJobsResponse> ListPendingAsync(
        Guid? tenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = AuditPackageGenerationRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingAsync(tenantId, normalizedBatchSize, cancellationToken);

        return new PendingAuditPackageGenerationJobsResponse(
            asOf,
            normalizedBatchSize,
            items.Select(x => new PendingAuditPackageGenerationJobItem(
                x.Id,
                x.TenantId,
                x.Format,
                x.CreatedAt)).ToList());
    }

    public async Task<ProcessAuditPackageGenerationJobsResponse> ProcessBatchAsync(
        ProcessAuditPackageGenerationJobsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = AuditPackageGenerationRules.NormalizeBatchSize(request.BatchSize);
        var pending = await LoadPendingAsync(request.TenantId, batchSize, cancellationToken);

        var results = new List<AuditPackageGenerationJobResult>();
        var skipped = new List<AuditPackageGenerationJobSkip>();

        foreach (var candidate in pending)
        {
            try
            {
                var result = await ProcessOneAsync(candidate.Id, cancellationToken);
                results.Add(result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new AuditPackageGenerationJobSkip(candidate.Id, ex.Message));
            }
        }

        if (results.Count > 0 && request.TenantId is Guid tenantId)
        {
            await auditService.WriteAsync(
                "audit_package.generation.batch",
                tenantId,
                WorkerActorUserId,
                "audit_package_generation_job",
                $"{results.Count}",
                "success",
                cancellationToken: cancellationToken);
        }

        return new ProcessAuditPackageGenerationJobsResponse(
            asOf,
            batchSize,
            pending.Count,
            results.Count(x => x.Status == AuditPackageGenerationJobStatuses.Completed),
            results.Count(x => x.Status == AuditPackageGenerationJobStatuses.Failed),
            skipped.Count,
            results,
            skipped);
    }

    private async Task<AuditPackageGenerationJobResult> ProcessOneAsync(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var job = await db.AuditPackageGenerationJobs
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {jobId} was not found.");

        if (job.Status != AuditPackageGenerationJobStatuses.Pending)
        {
            return new AuditPackageGenerationJobResult(job.Id, job.TenantId, job.Status, job.PackageId);
        }

        job.Status = AuditPackageGenerationJobStatuses.Processing;
        job.StartedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var package = await auditPackageService.MaterializeExportAsync(
                job.TenantId,
                job.FromUtc,
                job.ToUtc,
                cancellationToken);

            if (job.Format == AuditPackageGenerationFormats.Zip)
            {
                var zipBytes = await auditPackageService.CreateZipBytesAsync(package, cancellationToken);
                if (zipBytes.Length > AuditPackageGenerationRules.MaxArtifactZipBytes)
                {
                    throw new StlApiException(
                        "audit_package_generation.artifact_too_large",
                        "Generated ZIP exceeds the maximum allowed size.",
                        413);
                }

                job.ArtifactZip = zipBytes;
                job.ArtifactJson = null;
            }
            else
            {
                var json = JsonSerializer.Serialize(package, JsonOptions);
                if (json.Length > AuditPackageGenerationRules.MaxArtifactJsonChars)
                {
                    throw new StlApiException(
                        "audit_package_generation.artifact_too_large",
                        "Generated JSON exceeds the maximum allowed size.",
                        413);
                }

                job.ArtifactJson = json;
                job.ArtifactZip = null;
            }

            job.PackageId = package.PackageId;
            job.Status = AuditPackageGenerationJobStatuses.Completed;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ErrorMessage = null;
            await db.SaveChangesAsync(cancellationToken);

            await auditService.WriteAsync(
                "audit_package.generation.completed",
                job.TenantId,
                WorkerActorUserId,
                "audit_package",
                package.PackageId.ToString(),
                "success",
                reasonCode: job.Format,
                cancellationToken: cancellationToken);

            return new AuditPackageGenerationJobResult(
                job.Id,
                job.TenantId,
                job.Status,
                job.PackageId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            job.Status = AuditPackageGenerationJobStatuses.Failed;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ErrorMessage = AuditPackageGenerationRules.TruncateErrorMessage(ex.Message);
            job.ArtifactZip = null;
            job.ArtifactJson = null;
            await db.SaveChangesAsync(cancellationToken);

            await auditService.WriteAsync(
                "audit_package.generation.failed",
                job.TenantId,
                WorkerActorUserId,
                "audit_package_generation_job",
                job.Id.ToString(),
                "failed",
                reasonCode: job.ErrorMessage,
                cancellationToken: cancellationToken);

            return new AuditPackageGenerationJobResult(job.Id, job.TenantId, job.Status, job.PackageId);
        }
    }

    private async Task<List<AuditPackageGenerationJob>> LoadPendingAsync(
        Guid? tenantId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.AuditPackageGenerationJobs
            .Where(x => x.Status == AuditPackageGenerationJobStatuses.Pending);

        if (tenantId is Guid scopedTenantId)
        {
            query = query.Where(x => x.TenantId == scopedTenantId);
        }

        return await query
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task<AuditPackageGenerationJob> LoadJobForDownloadAsync(
        Guid tenantId,
        Guid jobId,
        string expectedFormat,
        CancellationToken cancellationToken)
    {
        var job = await db.AuditPackageGenerationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == jobId && x.TenantId == tenantId, cancellationToken)
            ?? throw new StlApiException(
                "audit_package_generation.job_not_found",
                "Audit package generation job was not found.",
                404);

        if (!string.Equals(job.Format, expectedFormat, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "audit_package_generation.format_mismatch",
                $"Job artifact format is {job.Format}, not {expectedFormat}.",
                400);
        }

        if (!AuditPackageGenerationRules.IsDownloadReady(job))
        {
            throw new StlApiException(
                "audit_package_generation.not_ready",
                "Audit package generation is not complete or the artifact is unavailable.",
                409);
        }

        return job;
    }

    private static AuditPackageGenerationJobResponse MapResponse(AuditPackageGenerationJob job) =>
        new(
            job.Id,
            job.Status,
            job.Format,
            job.FromUtc,
            job.ToUtc,
            job.PackageId,
            job.ErrorMessage,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt,
            AuditPackageGenerationRules.IsDownloadReady(job));

    private static void ValidateDateRange(DateTimeOffset? from, DateTimeOffset? to)
    {
        if (from is not null && to is not null && from > to)
        {
            throw new StlApiException(
                "audit_package.invalid_date_range",
                "The 'from' date must be before or equal to the 'to' date.",
                400);
        }
    }
}
