using Microsoft.EntityFrameworkCore;
using ComplianceCore.Api.Contracts;
using ComplianceCore.Api.Data;
using ComplianceCore.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace ComplianceCore.Api.Services;

public sealed class SourceIngestionService(
    ComplianceCoreDbContext db,
    FactSourceService factSourceService,
    ProductFactIngestionService productFactIngestionService,
    IComplianceCoreAuditService auditService)
{
    public const string IngestSourcesActionScope = "compliancecore.sources.ingest";

    public const int MaxBatchSize = 50;

    public async Task<IReadOnlyList<SourceIngestionBatchSummary>> ListBatchesAsync(
        Guid tenantId,
        string? ingestionType,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var cappedLimit = Math.Clamp(limit, 1, 50);
        var query = db.SourceIngestionBatches
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(ingestionType))
        {
            var normalized = ingestionType.Trim().ToLowerInvariant();
            query = query.Where(x => x.IngestionType == normalized);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(cappedLimit)
            .Select(x => new SourceIngestionBatchSummary(
                x.Id,
                x.IngestionType,
                x.Phase,
                x.DryRun,
                x.Status,
                x.TotalJobs,
                x.SuccessCount,
                x.ErrorCount,
                x.SkippedCount,
                x.SourceProduct,
                x.PublicationId,
                x.CreatedAt,
                x.CompletedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<SourceIngestionBatchDetailResponse?> GetBatchAsync(
        Guid tenantId,
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var batch = await db.SourceIngestionBatches
            .AsNoTracking()
            .Include(x => x.Jobs.OrderBy(j => j.RowIndex))
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == batchId, cancellationToken);

        if (batch is null)
        {
            return null;
        }

        return MapDetail(batch);
    }

    public async Task<SourceIngestionBatchResponse> IngestFactSourcesAsync(
        Guid tenantId,
        Guid? actorUserId,
        IReadOnlyList<FactSourceIngestionRowRequest> sources,
        bool dryRun,
        string phase,
        CancellationToken cancellationToken = default)
    {
        EnsureBatchSize(sources.Count);

        var batchId = Guid.NewGuid();
        var jobs = new List<SourceIngestionJob>();
        var results = new List<SourceIngestionJobResult>();
        var batchKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var successCount = 0;
        var errorCount = 0;

        for (var index = 0; index < sources.Count; index++)
        {
            var row = sources[index];
            var jobKey = row.SourceKey.Trim().ToLowerInvariant();

            if (!batchKeys.Add(jobKey))
            {
                errorCount++;
                RecordJobFailure(
                    jobs,
                    results,
                    batchId,
                    index,
                    jobKey,
                    "fact_sources.duplicate_batch",
                    "Duplicate source key within the ingestion batch.");
                continue;
            }

            var createRequest = new CreateFactSourceRequest(
                row.FactDefinitionId,
                row.SourceKey,
                row.SourceType,
                row.Label,
                row.Description,
                row.ProductKey,
                row.ProductReference,
                row.ConfigJson,
                row.Priority);

            var validation = await factSourceService.TryValidateCreateAsync(
                tenantId,
                createRequest,
                cancellationToken);

            if (!validation.Ok)
            {
                errorCount++;
                RecordJobFailure(
                    jobs,
                    results,
                    batchId,
                    index,
                    jobKey,
                    validation.ErrorCode ?? "fact_sources.validation",
                    validation.Message ?? "Validation failed.");
                continue;
            }

            if (dryRun)
            {
                successCount++;
                RecordJobSuccess(
                    jobs,
                    results,
                    batchId,
                    index,
                    jobKey,
                    SourceIngestionJobStatuses.Validated,
                    null,
                    null);
                continue;
            }

            try
            {
                var created = await factSourceService.CreateAsync(
                    tenantId,
                    actorUserId,
                    createRequest,
                    cancellationToken);
                successCount++;
                RecordJobSuccess(
                    jobs,
                    results,
                    batchId,
                    index,
                    jobKey,
                    SourceIngestionJobStatuses.Created,
                    "fact_source",
                    created.FactSourceId);
            }
            catch (StlApiException ex)
            {
                errorCount++;
                RecordJobFailure(jobs, results, batchId, index, jobKey, ex.Code, ex.Message);
            }
        }

        return await PersistBatchAsync(
            tenantId,
            actorUserId,
            batchId,
            SourceIngestionTypes.FactSources,
            phase,
            dryRun,
            sources.Count,
            successCount,
            errorCount,
            skippedCount: 0,
            sourceProduct: null,
            publicationId: null,
            jobs,
            results,
            dryRun ? "source_ingestion.fact_sources.validate" : "source_ingestion.fact_sources.commit",
            cancellationToken);
    }

    public async Task<SourceIngestionBatchResponse> IngestProductFactsAsync(
        Guid tenantId,
        Guid? actorUserId,
        ProductFactBulkIngestionRequest request,
        bool dryRun,
        string phase,
        CancellationToken cancellationToken = default)
    {
        EnsureBatchSize(request.Facts.Count);

        var batchId = Guid.NewGuid();
        var jobs = new List<SourceIngestionJob>();
        var results = new List<SourceIngestionJobResult>();
        var successCount = 0;
        var errorCount = 0;
        var skippedCount = 0;

        if (dryRun)
        {
            for (var index = 0; index < request.Facts.Count; index++)
            {
                var item = request.Facts[index];
                var jobKey = ProductFactMirrorRules.NormalizeFactKey(item.FactKey);

                try
                {
                    ValidateProductFactItem(item);
                    var idempotencyKey = ProductFactMirrorRules.NormalizeIdempotencyKey(item.IdempotencyKey);
                    var duplicate = await db.ProductFactMirrors.AnyAsync(
                        x => x.TenantId == tenantId && x.IdempotencyKey == idempotencyKey,
                        cancellationToken);

                    if (duplicate)
                    {
                        skippedCount++;
                        RecordJobSuccess(
                            jobs,
                            results,
                            batchId,
                            index,
                            jobKey,
                            SourceIngestionJobStatuses.Skipped,
                            "product_fact_mirror",
                            null);
                        continue;
                    }

                    successCount++;
                    RecordJobSuccess(
                        jobs,
                        results,
                        batchId,
                        index,
                        jobKey,
                        SourceIngestionJobStatuses.Validated,
                        null,
                        null);
                }
                catch (StlApiException ex)
                {
                    errorCount++;
                    RecordJobFailure(jobs, results, batchId, index, jobKey, ex.Code, ex.Message);
                }
            }

            return await PersistBatchAsync(
                tenantId,
                actorUserId,
                batchId,
                SourceIngestionTypes.ProductFacts,
                phase,
                dryRun: true,
                request.Facts.Count,
                successCount,
                errorCount,
                skippedCount,
                request.SourceProduct.Trim().ToLowerInvariant(),
                request.PublicationId,
                jobs,
                results,
                "source_ingestion.product_facts.validate",
                cancellationToken);
        }

        var ingestRequest = new IngestProductFactsRequest(
            tenantId,
            request.PublicationId,
            request.SourceProduct,
            request.PublishedAt,
            request.Facts);

        try
        {
            var ingestResult = await productFactIngestionService.IngestAsync(ingestRequest, cancellationToken);
            successCount = ingestResult.AcceptedCount;
            skippedCount = ingestResult.SkippedDuplicateCount;

            for (var index = 0; index < request.Facts.Count; index++)
            {
                var item = request.Facts[index];
                var jobKey = ProductFactMirrorRules.NormalizeFactKey(item.FactKey);
                RecordJobSuccess(
                    jobs,
                    results,
                    batchId,
                    index,
                    jobKey,
                    SourceIngestionJobStatuses.Accepted,
                    "product_fact_publication",
                    request.PublicationId);
            }

            errorCount = Math.Max(0, request.Facts.Count - successCount - skippedCount);
        }
        catch (StlApiException ex)
        {
            errorCount = request.Facts.Count;
            for (var index = 0; index < request.Facts.Count; index++)
            {
                var item = request.Facts[index];
                var jobKey = ProductFactMirrorRules.NormalizeFactKey(item.FactKey);
                RecordJobFailure(jobs, results, batchId, index, jobKey, ex.Code, ex.Message);
            }
        }

        return await PersistBatchAsync(
            tenantId,
            actorUserId,
            batchId,
            SourceIngestionTypes.ProductFacts,
            phase,
            dryRun: false,
            request.Facts.Count,
            successCount,
            errorCount,
            skippedCount,
            request.SourceProduct.Trim().ToLowerInvariant(),
            request.PublicationId,
            jobs,
            results,
            "source_ingestion.product_facts.commit",
            cancellationToken);
    }

    private async Task<SourceIngestionBatchResponse> PersistBatchAsync(
        Guid tenantId,
        Guid? actorUserId,
        Guid batchId,
        string ingestionType,
        string phase,
        bool dryRun,
        int totalJobs,
        int successCount,
        int errorCount,
        int skippedCount,
        string? sourceProduct,
        Guid? publicationId,
        List<SourceIngestionJob> jobs,
        List<SourceIngestionJobResult> results,
        string auditAction,
        CancellationToken cancellationToken)
    {
        var status = errorCount == 0
            ? SourceIngestionBatchStatuses.Completed
            : successCount == 0 && skippedCount == 0
                ? SourceIngestionBatchStatuses.Failed
                : SourceIngestionBatchStatuses.Partial;

        var now = DateTimeOffset.UtcNow;
        var batch = new SourceIngestionBatch
        {
            Id = batchId,
            TenantId = tenantId,
            IngestionType = ingestionType,
            Phase = phase,
            DryRun = dryRun,
            Status = status,
            TotalJobs = totalJobs,
            SuccessCount = successCount,
            ErrorCount = errorCount,
            SkippedCount = skippedCount,
            CreatedByUserId = actorUserId,
            SourceProduct = sourceProduct,
            PublicationId = publicationId,
            CreatedAt = now,
            CompletedAt = now,
            Jobs = jobs,
        };

        db.SourceIngestionBatches.Add(batch);
        await db.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync(
            auditAction,
            tenantId,
            actorUserId,
            "source_ingestion_batch",
            batchId.ToString(),
            status,
            reasonCode: $"{ingestionType}:{successCount}/{totalJobs}",
            cancellationToken: cancellationToken);

        return new SourceIngestionBatchResponse(
            batchId,
            ingestionType,
            phase,
            dryRun,
            totalJobs,
            successCount,
            errorCount,
            skippedCount,
            status,
            results);
    }

    private static void EnsureBatchSize(int count)
    {
        if (count == 0)
        {
            throw new StlApiException(
                "source_ingestion.validation",
                "At least one ingestion row is required.",
                400);
        }

        if (count > MaxBatchSize)
        {
            throw new StlApiException(
                "source_ingestion.validation",
                $"Source ingestion supports at most {MaxBatchSize} rows per batch.",
                400);
        }
    }

    private static void ValidateProductFactItem(ProductFactPublicationItemRequest item)
    {
        var valueType = item.ValueType.Trim().ToLowerInvariant();
        if (!FactValueTypes.All.Contains(valueType))
        {
            throw new StlApiException(
                "product_facts.invalid_value_type",
                "Value type is not supported.",
                400);
        }

        var hasValue = valueType switch
        {
            FactValueTypes.Boolean => item.BooleanValue.HasValue,
            FactValueTypes.Number => item.NumberValue.HasValue,
            FactValueTypes.Date => !string.IsNullOrWhiteSpace(item.DateValue),
            _ => !string.IsNullOrWhiteSpace(item.StringValue),
        };

        if (!hasValue)
        {
            throw new StlApiException(
                "product_facts.missing_value",
                $"A value is required for fact type {valueType}.",
                400);
        }

        ProductFactMirrorRules.NormalizeIdempotencyKey(item.IdempotencyKey);
        ProductFactMirrorRules.NormalizeFactKey(item.FactKey);
        ProductFactMirrorRules.NormalizeScopeKey(item.ScopeKey);
    }

    private static void RecordJobFailure(
        List<SourceIngestionJob> jobs,
        List<SourceIngestionJobResult> results,
        Guid batchId,
        int rowIndex,
        string jobKey,
        string errorCode,
        string message)
    {
        var jobId = Guid.NewGuid();
        jobs.Add(new SourceIngestionJob
        {
            Id = jobId,
            BatchId = batchId,
            RowIndex = rowIndex,
            JobKey = jobKey,
            Status = SourceIngestionJobStatuses.Error,
            ErrorCode = errorCode,
            Message = message,
        });
        results.Add(new SourceIngestionJobResult(
            rowIndex,
            jobKey,
            SourceIngestionJobStatuses.Error,
            null,
            null,
            errorCode,
            message));
    }

    private static void RecordJobSuccess(
        List<SourceIngestionJob> jobs,
        List<SourceIngestionJobResult> results,
        Guid batchId,
        int rowIndex,
        string jobKey,
        string status,
        string? entityType,
        Guid? entityId)
    {
        var jobId = Guid.NewGuid();
        jobs.Add(new SourceIngestionJob
        {
            Id = jobId,
            BatchId = batchId,
            RowIndex = rowIndex,
            JobKey = jobKey,
            Status = status,
            EntityType = entityType,
            EntityId = entityId,
        });
        results.Add(new SourceIngestionJobResult(
            rowIndex,
            jobKey,
            status,
            entityType,
            entityId,
            null,
            null));
    }

    private static SourceIngestionBatchDetailResponse MapDetail(SourceIngestionBatch batch) =>
        new(
            batch.Id,
            batch.IngestionType,
            batch.Phase,
            batch.DryRun,
            batch.Status,
            batch.TotalJobs,
            batch.SuccessCount,
            batch.ErrorCount,
            batch.SkippedCount,
            batch.SourceProduct,
            batch.PublicationId,
            batch.CreatedAt,
            batch.CompletedAt,
            batch.Jobs
                .OrderBy(x => x.RowIndex)
                .Select(x => new SourceIngestionJobResult(
                    x.RowIndex,
                    x.JobKey,
                    x.Status,
                    x.EntityType,
                    x.EntityId,
                    x.ErrorCode,
                    x.Message))
                .ToList());
}
