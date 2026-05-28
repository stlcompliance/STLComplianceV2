using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace NexArr.Api.Services;

public sealed class PlatformAuditPackageGenerationService(
    NexArrDbContext db,
    PlatformAuditPackageService auditPackageService,
    IPlatformAuditService audit)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    public const string ProcessJobsActionScope = "nexarr.platform_audit_packages.generate";

    public static readonly Guid WorkerActorUserId = Guid.Parse("00000000-0000-0000-0000-0000000000f8");

    public async Task<PlatformAuditPackageGenerationJobResponse> CreateJobAsync(
        Guid requestedByUserId,
        CreatePlatformAuditPackageGenerationJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var format = PlatformAuditPackageGenerationRules.NormalizeFormat(request.Format);
        ValidateDateRange(request.From, request.To);

        var now = DateTimeOffset.UtcNow;
        var job = new PlatformAuditPackageGenerationJob
        {
            Id = Guid.NewGuid(),
            ScopeTenantId = request.TenantId,
            RequestedByUserId = requestedByUserId,
            Status = PlatformAuditPackageGenerationJobStatuses.Pending,
            Format = format,
            FromUtc = request.From,
            ToUtc = request.To,
            CreatedAt = now,
        };

        db.PlatformAuditPackageGenerationJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        await audit.WriteAsync(
            "platform_audit_package.generation.enqueued",
            "platform_audit_package_generation_job",
            job.Id.ToString(),
            "success",
            tenantId: job.ScopeTenantId,
            actorUserId: requestedByUserId,
            reasonCode: format,
            cancellationToken: cancellationToken);

        return MapResponse(job);
    }

    public async Task<PlatformAuditPackageGenerationJobResponse> GetJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await db.PlatformAuditPackageGenerationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken)
            ?? throw new StlApiException(
                "platform_audit_package_generation.job_not_found",
                "Platform audit package generation job was not found.",
                404);

        return MapResponse(job);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> DownloadZipAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await LoadJobForDownloadAsync(jobId, PlatformAuditPackageGenerationFormats.Zip, cancellationToken);
        return (
            job.ArtifactZip!,
            "application/zip",
            $"nexarr-platform-audit-package-{job.PackageId:N}.zip");
    }

    public async Task<PlatformAuditPackageExportResponse> DownloadJsonAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await LoadJobForDownloadAsync(jobId, PlatformAuditPackageGenerationFormats.Json, cancellationToken);
        return JsonSerializer.Deserialize<PlatformAuditPackageExportResponse>(job.ArtifactJson!, JsonOptions)
            ?? throw new StlApiException(
                "platform_audit_package_generation.artifact_invalid",
                "Stored JSON artifact could not be deserialized.",
                500);
    }

    public async Task<PendingPlatformAuditPackageGenerationJobsResponse> ListPendingAsync(
        Guid? scopeTenantId,
        DateTimeOffset? asOfUtc,
        int? batchSize,
        CancellationToken cancellationToken = default)
    {
        var asOf = asOfUtc ?? DateTimeOffset.UtcNow;
        var normalizedBatchSize = PlatformAuditPackageGenerationRules.NormalizeBatchSize(batchSize);
        var items = await LoadPendingAsync(scopeTenantId, normalizedBatchSize, cancellationToken);

        return new PendingPlatformAuditPackageGenerationJobsResponse(
            asOf,
            normalizedBatchSize,
            items.Select(x => new PendingPlatformAuditPackageGenerationJobItem(
                x.Id,
                x.ScopeTenantId,
                x.Format,
                x.CreatedAt)).ToList());
    }

    public async Task<ProcessPlatformAuditPackageGenerationJobsResponse> ProcessBatchAsync(
        ProcessPlatformAuditPackageGenerationJobsRequest request,
        CancellationToken cancellationToken = default)
    {
        var asOf = request.AsOfUtc ?? DateTimeOffset.UtcNow;
        var batchSize = PlatformAuditPackageGenerationRules.NormalizeBatchSize(request.BatchSize);
        var pending = await LoadPendingAsync(request.TenantId, batchSize, cancellationToken);

        var results = new List<PlatformAuditPackageGenerationJobResult>();
        var skipped = new List<PlatformAuditPackageGenerationJobSkip>();

        foreach (var candidate in pending)
        {
            try
            {
                var result = await ProcessOneAsync(candidate.Id, cancellationToken);
                results.Add(result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                skipped.Add(new PlatformAuditPackageGenerationJobSkip(candidate.Id, ex.Message));
            }
        }

        if (results.Count > 0)
        {
            await audit.WriteAsync(
                "platform_audit_package.generation.batch",
                "platform_audit_package_generation_job",
                $"{results.Count}",
                "success",
                tenantId: request.TenantId,
                actorUserId: WorkerActorUserId,
                cancellationToken: cancellationToken);
        }

        return new ProcessPlatformAuditPackageGenerationJobsResponse(
            asOf,
            batchSize,
            pending.Count,
            results.Count(x => x.Status == PlatformAuditPackageGenerationJobStatuses.Completed),
            results.Count(x => x.Status == PlatformAuditPackageGenerationJobStatuses.Failed),
            skipped.Count,
            results,
            skipped);
    }

    private async Task<PlatformAuditPackageGenerationJobResult> ProcessOneAsync(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var job = await db.PlatformAuditPackageGenerationJobs
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {jobId} was not found.");

        if (job.Status != PlatformAuditPackageGenerationJobStatuses.Pending)
        {
            return new PlatformAuditPackageGenerationJobResult(job.Id, job.ScopeTenantId, job.Status, job.PackageId);
        }

        job.Status = PlatformAuditPackageGenerationJobStatuses.Processing;
        job.StartedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            var package = await auditPackageService.MaterializeExportAsync(
                job.ScopeTenantId,
                job.FromUtc,
                job.ToUtc,
                cancellationToken);

            if (job.Format == PlatformAuditPackageGenerationFormats.Zip)
            {
                var zipBytes = await auditPackageService.CreateZipBytesAsync(package, cancellationToken);
                if (zipBytes.Length > PlatformAuditPackageGenerationRules.MaxArtifactZipBytes)
                {
                    throw new StlApiException(
                        "platform_audit_package_generation.artifact_too_large",
                        "Generated ZIP exceeds the maximum allowed size.",
                        413);
                }

                job.ArtifactZip = zipBytes;
                job.ArtifactJson = null;
            }
            else
            {
                var json = JsonSerializer.Serialize(package, JsonOptions);
                if (json.Length > PlatformAuditPackageGenerationRules.MaxArtifactJsonChars)
                {
                    throw new StlApiException(
                        "platform_audit_package_generation.artifact_too_large",
                        "Generated JSON exceeds the maximum allowed size.",
                        413);
                }

                job.ArtifactJson = json;
                job.ArtifactZip = null;
            }

            job.PackageId = package.PackageId;
            job.Status = PlatformAuditPackageGenerationJobStatuses.Completed;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ErrorMessage = null;
            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "platform_audit_package.generation.completed",
                "platform_audit_package",
                package.PackageId.ToString(),
                "success",
                tenantId: job.ScopeTenantId,
                actorUserId: WorkerActorUserId,
                reasonCode: job.Format,
                cancellationToken: cancellationToken);

            return new PlatformAuditPackageGenerationJobResult(
                job.Id,
                job.ScopeTenantId,
                job.Status,
                job.PackageId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            job.Status = PlatformAuditPackageGenerationJobStatuses.Failed;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.ErrorMessage = PlatformAuditPackageGenerationRules.TruncateErrorMessage(ex.Message);
            job.ArtifactZip = null;
            job.ArtifactJson = null;
            await db.SaveChangesAsync(cancellationToken);

            await audit.WriteAsync(
                "platform_audit_package.generation.failed",
                "platform_audit_package_generation_job",
                job.Id.ToString(),
                "failed",
                tenantId: job.ScopeTenantId,
                actorUserId: WorkerActorUserId,
                reasonCode: job.ErrorMessage,
                cancellationToken: cancellationToken);

            return new PlatformAuditPackageGenerationJobResult(job.Id, job.ScopeTenantId, job.Status, job.PackageId);
        }
    }

    private async Task<List<PlatformAuditPackageGenerationJob>> LoadPendingAsync(
        Guid? scopeTenantId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var query = db.PlatformAuditPackageGenerationJobs
            .Where(x => x.Status == PlatformAuditPackageGenerationJobStatuses.Pending);

        if (scopeTenantId is Guid tenantId)
        {
            query = query.Where(x => x.ScopeTenantId == tenantId);
        }

        return await query
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private async Task<PlatformAuditPackageGenerationJob> LoadJobForDownloadAsync(
        Guid jobId,
        string expectedFormat,
        CancellationToken cancellationToken)
    {
        var job = await db.PlatformAuditPackageGenerationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken)
            ?? throw new StlApiException(
                "platform_audit_package_generation.job_not_found",
                "Platform audit package generation job was not found.",
                404);

        if (!string.Equals(job.Format, expectedFormat, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "platform_audit_package_generation.format_mismatch",
                $"Job artifact format is {job.Format}, not {expectedFormat}.",
                400);
        }

        if (!PlatformAuditPackageGenerationRules.IsDownloadReady(job))
        {
            throw new StlApiException(
                "platform_audit_package_generation.not_ready",
                "Platform audit package generation is not complete or the artifact is unavailable.",
                409);
        }

        return job;
    }

    private static PlatformAuditPackageGenerationJobResponse MapResponse(PlatformAuditPackageGenerationJob job) =>
        new(
            job.Id,
            job.ScopeTenantId,
            job.Status,
            job.Format,
            job.FromUtc,
            job.ToUtc,
            job.PackageId,
            job.ErrorMessage,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt,
            PlatformAuditPackageGenerationRules.IsDownloadReady(job));

    private static void ValidateDateRange(DateTimeOffset? from, DateTimeOffset? to)
    {
        if (from is not null && to is not null && from > to)
        {
            throw new StlApiException(
                "platform_audit_package.invalid_date_range",
                "The 'from' date must be before or equal to the 'to' date.",
                400);
        }
    }
}
