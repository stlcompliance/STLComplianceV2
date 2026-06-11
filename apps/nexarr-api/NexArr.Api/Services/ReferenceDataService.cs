using System.Security.Claims;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class ReferenceDataService(NexArrDbContext db, PlatformAuthorizationService authorization)
{
    private const string MasterCsvIntakeDatasetKey = "master-reference-intake";
    private const string MasterCsvSourceKey = "master-reference-csv";

    public async Task<ReferenceDataDashboardResponse> GetDashboardAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var generatedAt = DateTimeOffset.UtcNow;

        return new ReferenceDataDashboardResponse(
            DatasetCount: await db.ReferenceDatasets.CountAsync(cancellationToken),
            SourceCount: await db.ReferenceSources.CountAsync(cancellationToken),
            JobCount: await db.IngestionJobs.CountAsync(cancellationToken),
            PendingReviewCount: await db.StagingRecords.CountAsync(x => x.Status == ReferenceStagingStatuses.NeedsReview, cancellationToken),
            FailedImportCount: await db.IngestionJobs.CountAsync(x => x.Status == ReferenceImportStatuses.Failed, cancellationToken),
            PublishedEntityCount: await db.ReferenceEntities.CountAsync(x => x.Status == ReferenceEntityStatuses.Active, cancellationToken),
            CrosswalkCount: await db.ReferenceCrosswalks.CountAsync(cancellationToken),
            PublishEventCount: await db.ReferencePublishEvents.CountAsync(cancellationToken),
            GeneratedAt: generatedAt);
    }

    public async Task<IReadOnlyList<ReferenceDatasetResponse>> ListDatasetsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);

        var lastPublishedByDataset = await db.ReferencePublishEvents
            .AsNoTracking()
            .GroupBy(x => x.DatasetId)
            .Select(g => new
            {
                DatasetId = g.Key,
                LastPublishedAt = g.Max(x => x.CreatedAt)
            })
            .ToListAsync(cancellationToken);
        var lastPublishedMap = lastPublishedByDataset.ToDictionary(x => x.DatasetId, x => x.LastPublishedAt);
        var sourceCount = await db.ReferenceSources.AsNoTracking().CountAsync(cancellationToken);
        var entityCountMap = await db.ReferenceEntities.AsNoTracking()
            .GroupBy(x => x.DatasetId)
            .Select(g => new
            {
                DatasetId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.DatasetId, x => x.Count, cancellationToken);
        var pendingReviewByJobDataset = await db.StagingRecords.AsNoTracking()
            .Where(x => x.Status == ReferenceStagingStatuses.NeedsReview)
            .GroupBy(x => x.Job.DatasetId)
            .Select(g => new
            {
                DatasetId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.DatasetId, x => x.Count, cancellationToken);
        var pendingReviewByTargetDataset = await db.StagingRecords.AsNoTracking()
            .Where(x =>
                x.Status == ReferenceStagingStatuses.NeedsReview
                && x.TargetDatasetId.HasValue
                && x.TargetDatasetId != x.Job.DatasetId)
            .GroupBy(x => x.TargetDatasetId!.Value)
            .Select(g => new
            {
                DatasetId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.DatasetId, x => x.Count, cancellationToken);
        var pendingReviewMap = new Dictionary<Guid, int>();
        AddCounts(pendingReviewMap, pendingReviewByJobDataset);
        AddCounts(pendingReviewMap, pendingReviewByTargetDataset);
        var failedImportCountMap = await db.IngestionJobs.AsNoTracking()
            .Where(x => x.Status == ReferenceImportStatuses.Failed)
            .GroupBy(x => x.DatasetId)
            .Select(g => new
            {
                DatasetId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.DatasetId, x => x.Count, cancellationToken);

        var query = db.ReferenceDatasets.AsNoTracking();
        var datasets = await query.OrderBy(x => x.Key).ToListAsync(cancellationToken);
        var results = new List<ReferenceDatasetResponse>(datasets.Count);

        foreach (var dataset in datasets)
        {
            results.Add(new ReferenceDatasetResponse(
                dataset.Id,
                dataset.Key,
                dataset.Name,
                dataset.Category,
                dataset.OwnerService,
                dataset.Status,
                dataset.CurrentPublishedVersion,
                SourceCount: sourceCount,
                EntityCount: entityCountMap.GetValueOrDefault(dataset.Id),
                PendingReviewCount: pendingReviewMap.GetValueOrDefault(dataset.Id),
                FailedImportCount: failedImportCountMap.GetValueOrDefault(dataset.Id),
                LastPublishedAt: lastPublishedMap.GetValueOrDefault(dataset.Id),
                dataset.CreatedAt,
                dataset.UpdatedAt));
        }

        return results;
    }

    public async Task<ReferenceDatasetResponse> CreateDatasetAsync(
        ClaimsPrincipal principal,
        CreateReferenceDatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var key = NormalizeKey(request.Key);
        if (await db.ReferenceDatasets.AnyAsync(x => x.Key == key, cancellationToken))
        {
            throw new StlApiException("reference.dataset_conflict", "Dataset key already exists.", 409);
        }

        var dataset = new ReferenceDataset
        {
            Id = Guid.NewGuid(),
            Key = key,
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            OwnerService = request.OwnerService.Trim(),
            Status = request.Status.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ReferenceDatasets.Add(dataset);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(principal, "reference.dataset.created", "reference_dataset", dataset.Id, null, Serialize(dataset), cancellationToken);
        return await ToDatasetResponseAsync(dataset.Id, cancellationToken);
    }

    public async Task<ReferenceDatasetResponse> UpdateDatasetAsync(
        ClaimsPrincipal principal,
        Guid datasetId,
        CreateReferenceDatasetRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var dataset = await db.ReferenceDatasets.FirstOrDefaultAsync(x => x.Id == datasetId, cancellationToken)
            ?? throw new StlApiException("reference.dataset_not_found", "Dataset was not found.", 404);

        var normalizedKey = NormalizeKey(request.Key);
        if (!string.Equals(dataset.Key, normalizedKey, StringComparison.OrdinalIgnoreCase)
            && await db.ReferenceDatasets.AnyAsync(x => x.Id != datasetId && x.Key == normalizedKey, cancellationToken))
        {
            throw new StlApiException("reference.dataset_conflict", "Dataset key already exists.", 409);
        }

        if (string.Equals(dataset.Key, MasterCsvIntakeDatasetKey, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(normalizedKey, MasterCsvIntakeDatasetKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "reference.dataset_protected",
                "The master intake dataset key cannot be changed.",
                400);
        }

        var before = Serialize(dataset);
        var now = DateTimeOffset.UtcNow;
        dataset.Key = normalizedKey;
        dataset.Name = request.Name.Trim();
        dataset.Category = request.Category.Trim();
        dataset.OwnerService = request.OwnerService.Trim();
        dataset.Status = request.Status.Trim();
        dataset.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            principal,
            "reference.dataset.updated",
            "reference_dataset",
            dataset.Id,
            before,
            Serialize(dataset),
            cancellationToken);
        return await ToDatasetResponseAsync(dataset.Id, cancellationToken);
    }

    public async Task DeleteDatasetAsync(
        ClaimsPrincipal principal,
        Guid datasetId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var dataset = await db.ReferenceDatasets.FirstOrDefaultAsync(x => x.Id == datasetId, cancellationToken)
            ?? throw new StlApiException("reference.dataset_not_found", "Dataset was not found.", 404);

        if (string.Equals(dataset.Key, MasterCsvIntakeDatasetKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "reference.dataset_protected",
                "The master intake dataset cannot be deleted.",
                400);
        }

        var before = Serialize(dataset);
        var now = DateTimeOffset.UtcNow;
        var entityIds = await db.ReferenceEntities.AsNoTracking()
            .Where(x => x.DatasetId == datasetId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (entityIds.Count > 0)
        {
            var referencedStaging = await db.StagingRecords
                .Where(x => x.ReferenceEntityId.HasValue && entityIds.Contains(x.ReferenceEntityId.Value))
                .ToListAsync(cancellationToken);
            foreach (var staging in referencedStaging)
            {
                staging.ReferenceEntityId = null;
                staging.UpdatedAt = now;
            }

            db.ProductMappings.RemoveRange(await db.ProductMappings
                .Where(x => entityIds.Contains(x.ReferenceEntityId))
                .ToListAsync(cancellationToken));
            db.TenantReferenceOverlays.RemoveRange(await db.TenantReferenceOverlays
                .Where(x => entityIds.Contains(x.ReferenceEntityId))
                .ToListAsync(cancellationToken));
            db.ReferenceCrosswalks.RemoveRange(await db.ReferenceCrosswalks
                .Where(x => entityIds.Contains(x.ReferenceEntityId))
                .ToListAsync(cancellationToken));
            db.ReferenceEntityVersions.RemoveRange(await db.ReferenceEntityVersions
                .Where(x => entityIds.Contains(x.ReferenceEntityId))
                .ToListAsync(cancellationToken));
            db.ReferenceEntities.RemoveRange(await db.ReferenceEntities
                .Where(x => x.DatasetId == datasetId)
                .ToListAsync(cancellationToken));
        }

        db.StagingRecords.RemoveRange(await db.StagingRecords
            .Where(x => x.TargetDatasetId == datasetId || x.Job.DatasetId == datasetId)
            .ToListAsync(cancellationToken));
        db.ReferencePublishEvents.RemoveRange(await db.ReferencePublishEvents
            .Where(x => x.DatasetId == datasetId)
            .ToListAsync(cancellationToken));
        db.IngestionJobs.RemoveRange(await db.IngestionJobs
            .Where(x => x.DatasetId == datasetId)
            .ToListAsync(cancellationToken));
        db.ReferenceDatasets.Remove(dataset);

        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            principal,
            "reference.dataset.deleted",
            "reference_dataset",
            datasetId,
            before,
            null,
            cancellationToken);
    }

    public async Task<IReadOnlyList<ReferenceSourceResponse>> ListSourcesAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        return await db.ReferenceSources.AsNoTracking()
            .OrderByDescending(x => x.AuthorityRank)
            .ThenBy(x => x.Key)
            .Select(x => new ReferenceSourceResponse(
                x.Id,
                x.Key,
                x.Name,
                x.SourceType,
                x.ConnectorType,
                x.AuthorityRank,
                x.RefreshCadence,
                x.TermsNotes,
                x.Enabled,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReferenceSourceResponse> CreateSourceAsync(
        ClaimsPrincipal principal,
        CreateReferenceSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var key = NormalizeKey(request.Key);
        if (await db.ReferenceSources.AnyAsync(x => x.Key == key, cancellationToken))
        {
            throw new StlApiException("reference.source_conflict", "Source key already exists.", 409);
        }

        var source = new ReferenceSource
        {
            Id = Guid.NewGuid(),
            Key = key,
            Name = request.Name.Trim(),
            SourceType = request.SourceType.Trim(),
            ConnectorType = request.ConnectorType.Trim(),
            AuthorityRank = request.AuthorityRank,
            RefreshCadence = request.RefreshCadence.Trim(),
            TermsNotes = request.TermsNotes?.Trim(),
            Enabled = request.Enabled,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ReferenceSources.Add(source);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(principal, "reference.source.created", "reference_source", source.Id, null, Serialize(source), cancellationToken);
        return new ReferenceSourceResponse(
            source.Id,
            source.Key,
            source.Name,
            source.SourceType,
            source.ConnectorType,
            source.AuthorityRank,
            source.RefreshCadence,
            source.TermsNotes,
            source.Enabled,
            source.CreatedAt,
            source.UpdatedAt);
    }

    public async Task<ReferenceImportResponse> CreateDatasetInputAsync(
        ClaimsPrincipal principal,
        Guid datasetId,
        CreateReferenceDatasetInputRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var dataset = await db.ReferenceDatasets.FirstOrDefaultAsync(x => x.Id == datasetId, cancellationToken)
            ?? throw new StlApiException("reference.dataset_not_found", "Dataset was not found.", 404);

        var rawValues = BuildInputValues(request);
        if (rawValues.Count == 0)
        {
            throw new StlApiException(
                "reference.dataset_input_empty",
                "Provide a value or a list of values to import.",
                400);
        }

        var source = await EnsurePlatformInputSourceAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var job = new IngestionJob
        {
            Id = Guid.NewGuid(),
            DatasetId = dataset.Id,
            SourceId = source.Id,
            TenantId = null,
            RequestedByPersonId = principal.GetUserId(),
            Status = ReferenceImportStatuses.InProgress,
            RawObjectKey = request.RawObjectKey?.Trim(),
            FileName = request.FileName?.Trim(),
            StartedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.IngestionJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        var stagingRecords = new List<StagingRecord>(rawValues.Count);
        foreach (var (value, index) in rawValues.Select((value, index) => (value, index)))
        {
            var normalizedValue = value.Trim();
            var staging = new StagingRecord
            {
                Id = Guid.NewGuid(),
                JobId = job.Id,
                RowNumber = index + 1,
                RawPayloadJson = JsonSerializer.Serialize(new
                {
                    dataset = dataset.Key,
                    value = normalizedValue,
                }),
                NormalizedPayloadJson = JsonSerializer.Serialize(new
                {
                    dataset = dataset.Key,
                    value = normalizedValue,
                    source = source.Key,
                }),
                ProposedEntityType = dataset.Category,
                ProposedCanonicalKey = NormalizeKey(normalizedValue),
                Confidence = 1.0m,
                Status = ReferenceStagingStatuses.Approved,
                CreatedAt = now,
                UpdatedAt = now,
                ReviewerPersonId = principal.GetUserId(),
                ReviewedAt = now,
            };

            db.StagingRecords.Add(staging);
            stagingRecords.Add(staging);
        }

        await db.SaveChangesAsync(cancellationToken);
        foreach (var staging in stagingRecords)
        {
            var reviewed = await UpsertEntityFromStagingAsync(
                staging,
                new ReviewDecisionRequest(null, null, null, null, null, null, null),
                cancellationToken);
            staging.ReferenceEntityId = reviewed.Id;
        }

        job.Status = ReferenceImportStatuses.Completed;
        job.CompletedAt = now;
        job.UpdatedAt = now;
        if (!string.Equals(dataset.Status, ReferenceDatasetStatuses.Archived, StringComparison.OrdinalIgnoreCase))
        {
            dataset.Status = ReferenceDatasetStatuses.Ready;
            dataset.UpdatedAt = now;
        }
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            principal,
            "reference.dataset_input.created",
            "ingestion_job",
            job.Id,
            null,
            Serialize(job),
            cancellationToken);

        return await GetImportAsync(principal, job.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<ReferenceImportResponse>> ListImportsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var stagingSummaries = await db.StagingRecords.AsNoTracking()
            .GroupBy(x => x.JobId)
            .Select(g => new
            {
                JobId = g.Key,
                Summary = new ImportStagingSummary(
                    g.Count(),
                    g.Count(x => x.Status == ReferenceStagingStatuses.NeedsReview),
                    g.Count(x => x.Status == ReferenceStagingStatuses.Approved),
                    g.Count(x => x.Status == ReferenceStagingStatuses.Rejected))
            })
            .ToDictionaryAsync(x => x.JobId, x => x.Summary, cancellationToken);
        var jobs = await db.IngestionJobs.AsNoTracking()
            .Include(x => x.Dataset)
            .Include(x => x.Source)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return jobs
            .Select(job => BuildImportResponse(job, stagingSummaries.GetValueOrDefault(job.Id) ?? ImportStagingSummary.Empty))
            .ToList();
    }

    public async Task<ReferenceImportResponse> CreateImportAsync(
        ClaimsPrincipal principal,
        CreateReferenceImportRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var dataset = await db.ReferenceDatasets.FirstOrDefaultAsync(x => x.Id == request.DatasetId, cancellationToken)
            ?? throw new StlApiException("reference.dataset_not_found", "Dataset was not found.", 404);
        var source = await db.ReferenceSources.FirstOrDefaultAsync(x => x.Id == request.SourceId, cancellationToken)
            ?? throw new StlApiException("reference.source_not_found", "Source was not found.", 404);
        var now = DateTimeOffset.UtcNow;
        var job = new IngestionJob
        {
            Id = Guid.NewGuid(),
            DatasetId = dataset.Id,
            SourceId = source.Id,
            TenantId = request.TenantId,
            RequestedByPersonId = request.RequestedByPersonId ?? principal.GetUserId(),
            Status = ReferenceImportStatuses.InProgress,
            RawObjectKey = request.RawObjectKey?.Trim(),
            FileName = request.FileName?.Trim(),
            StartedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.IngestionJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        if (request.Records is { Count: > 0 })
        {
            foreach (var record in request.Records)
            {
                db.StagingRecords.Add(new StagingRecord
                {
                    Id = Guid.NewGuid(),
                    JobId = job.Id,
                    RowNumber = record.RowNumber,
                    RawPayloadJson = NormalizeJson(record.RawPayloadJson),
                    NormalizedPayloadJson = NormalizeJson(record.NormalizedPayloadJson ?? record.RawPayloadJson),
                    ProposedEntityType = record.ProposedEntityType.Trim(),
                    ProposedCanonicalKey = string.IsNullOrWhiteSpace(record.ProposedCanonicalKey) ? null : NormalizeKey(record.ProposedCanonicalKey),
                    Confidence = ClampConfidence(record.Confidence),
                    Status = ReferenceStagingStatuses.NeedsReview,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }
        }
        else
        {
            db.StagingRecords.Add(new StagingRecord
            {
                Id = Guid.NewGuid(),
                JobId = job.Id,
                RowNumber = 1,
                RawPayloadJson = JsonSerializer.Serialize(new { note = "empty import placeholder", dataset = dataset.Key, source = source.Key }),
                NormalizedPayloadJson = JsonSerializer.Serialize(new { dataset = dataset.Key, source = source.Key, status = "pending review" }),
                ProposedEntityType = dataset.Category,
                ProposedCanonicalKey = null,
                Confidence = 0.5m,
                Status = ReferenceStagingStatuses.NeedsReview,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        if (!string.Equals(dataset.Status, ReferenceDatasetStatuses.Archived, StringComparison.OrdinalIgnoreCase))
        {
            dataset.Status = ReferenceDatasetStatuses.Ready;
            dataset.UpdatedAt = now;
        }
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(principal, "reference.import.created", "ingestion_job", job.Id, null, Serialize(job), cancellationToken);
        return await GetImportAsync(principal, job.Id, cancellationToken);
    }

    public async Task<ReferenceImportResponse> CreateMasterCsvImportAsync(
        ClaimsPrincipal principal,
        CreateReferenceMasterCsvImportRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        if (string.IsNullOrWhiteSpace(request.CsvText))
        {
            throw new StlApiException("reference.master_csv_empty", "Provide CSV content to import.", 400);
        }

        var parsedRows = await ParseMasterCsvRowsAsync(request.CsvText, cancellationToken);
        if (parsedRows.Count == 0)
        {
            throw new StlApiException("reference.master_csv_empty", "The CSV must contain at least one data row.", 400);
        }

        var masterDataset = await EnsureMasterCsvIntakeDatasetAsync(cancellationToken);
        var source = await EnsureMasterCsvSourceAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var job = new IngestionJob
        {
            Id = Guid.NewGuid(),
            DatasetId = masterDataset.Id,
            SourceId = source.Id,
            TenantId = null,
            RequestedByPersonId = principal.GetUserId(),
            Status = ReferenceImportStatuses.ReviewRequired,
            RawObjectKey = request.RawObjectKey?.Trim(),
            FileName = request.FileName?.Trim() ?? "master.csv",
            StartedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.IngestionJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var row in parsedRows)
        {
            db.StagingRecords.Add(new StagingRecord
            {
                Id = Guid.NewGuid(),
                JobId = job.Id,
                TargetDatasetId = row.TargetDatasetId,
                RowNumber = row.RowNumber,
                RawPayloadJson = row.RawPayloadJson,
                NormalizedPayloadJson = row.NormalizedPayloadJson,
                ProposedEntityType = row.ProposedEntityType,
                ProposedCanonicalKey = row.ProposedCanonicalKey,
                Confidence = row.Confidence,
                Status = ReferenceStagingStatuses.NeedsReview,
                ReviewReason = row.ReviewReason,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(principal, "reference.master_csv_import.created", "ingestion_job", job.Id, null, Serialize(job), cancellationToken);
        return await GetImportAsync(principal, job.Id, cancellationToken);
    }

    public async Task<ReferenceImportResponse> GetImportAsync(
        ClaimsPrincipal principal,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var job = await db.IngestionJobs.AsNoTracking()
            .Include(x => x.Dataset)
            .Include(x => x.Source)
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken)
            ?? throw new StlApiException("reference.import_not_found", "Import job was not found.", 404);

        return await BuildImportResponseAsync(job, cancellationToken);
    }

    public async Task<IReadOnlyList<ReferenceStagingRecordResponse>> ListStagingRecordsAsync(
        ClaimsPrincipal principal,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var records = await db.StagingRecords.AsNoTracking()
            .Include(x => x.Job).ThenInclude(x => x.Dataset)
            .Include(x => x.Job).ThenInclude(x => x.Source)
            .Include(x => x.TargetDataset)
            .Where(x => x.JobId == jobId)
            .OrderBy(x => x.RowNumber ?? int.MaxValue)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(ToStagingResponse).ToList();
    }

    public async Task<IReadOnlyList<ReferenceEntityListItemResponse>> ListDatasetEntitiesAsync(
        ClaimsPrincipal principal,
        Guid datasetId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var dataset = await db.ReferenceDatasets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == datasetId, cancellationToken)
            ?? throw new StlApiException("reference.dataset_not_found", "Dataset was not found.", 404);

        return await db.ReferenceEntities.AsNoTracking()
            .Where(x => x.DatasetId == dataset.Id && x.Status == ReferenceEntityStatuses.Active)
            .OrderBy(x => x.DisplayName)
            .Select(entity => new ReferenceEntityListItemResponse(
                entity.Id,
                entity.DatasetId,
                entity.Dataset.Key,
                entity.Dataset.Name,
                entity.EntityType,
                entity.CanonicalKey,
                entity.DisplayName,
                entity.Status,
                db.ReferenceEntityVersions
                    .Where(version => version.Id == entity.CurrentVersionId)
                    .Select(version => (int?)version.Version)
                    .FirstOrDefault(),
                db.ReferenceEntityVersions
                    .Where(version => version.Id == entity.CurrentVersionId)
                    .Select(version => version.PublishedAt)
                    .FirstOrDefault(),
                entity.CreatedAt,
                entity.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReferenceEntityResponse> UpdateEntityAsync(
        ClaimsPrincipal principal,
        Guid entityId,
        UpdateReferenceEntityRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var entity = await db.ReferenceEntities
            .Include(x => x.Dataset)
            .FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken)
            ?? throw new StlApiException("reference.entity_not_found", "Reference entity was not found.", 404);

        var normalizedCanonicalKey = NormalizeKey(request.CanonicalKey ?? entity.CanonicalKey);
        if (!string.Equals(entity.CanonicalKey, normalizedCanonicalKey, StringComparison.OrdinalIgnoreCase)
            && await db.ReferenceEntities.AnyAsync(
                x => x.Id != entityId && x.DatasetId == entity.DatasetId && x.CanonicalKey == normalizedCanonicalKey,
                cancellationToken))
        {
            throw new StlApiException(
                "reference.entity_conflict",
                "Another entity in this dataset already uses that canonical key.",
                409);
        }

        var before = Serialize(entity);
        var now = DateTimeOffset.UtcNow;
        var currentVersion = await db.ReferenceEntityVersions
            .Where(x => x.ReferenceEntityId == entity.Id)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);

        entity.CanonicalKey = normalizedCanonicalKey;
        entity.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? entity.DisplayName
            : request.DisplayName.Trim();
        entity.NormalizedFieldsJson = string.IsNullOrWhiteSpace(request.NormalizedFieldsJson)
            ? entity.NormalizedFieldsJson
            : NormalizeJson(request.NormalizedFieldsJson);
        entity.UpdatedAt = now;
        if (!string.Equals(entity.Dataset.Status, ReferenceDatasetStatuses.Archived, StringComparison.OrdinalIgnoreCase))
        {
            entity.Dataset.Status = ReferenceDatasetStatuses.Ready;
            entity.Dataset.UpdatedAt = now;
        }

        var version = new ReferenceEntityVersion
        {
            Id = Guid.NewGuid(),
            ReferenceEntityId = entity.Id,
            Version = (currentVersion?.Version ?? 0) + 1,
            FieldsJson = entity.NormalizedFieldsJson,
            SourceEvidenceJson = NormalizeJson(request.SourceEvidenceJson ?? currentVersion?.SourceEvidenceJson),
            EffectiveDate = request.EffectiveDate ?? currentVersion?.EffectiveDate,
            PublishedAt = null,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ReferenceEntityVersions.Add(version);
        entity.CurrentVersionId = version.Id;

        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            principal,
            "reference.entity.updated",
            "reference_entity",
            entity.Id,
            before,
            Serialize(entity),
            cancellationToken);
        return await ToEntityResponseAsync(entity, cancellationToken);
    }

    public async Task DeleteEntityAsync(
        ClaimsPrincipal principal,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var entity = await db.ReferenceEntities
            .Include(x => x.Dataset)
            .FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken)
            ?? throw new StlApiException("reference.entity_not_found", "Reference entity was not found.", 404);

        var before = Serialize(entity);
        var now = DateTimeOffset.UtcNow;
        var referencedStaging = await db.StagingRecords
            .Where(x => x.ReferenceEntityId == entityId)
            .ToListAsync(cancellationToken);
        foreach (var staging in referencedStaging)
        {
            staging.ReferenceEntityId = null;
            staging.UpdatedAt = now;
        }

        db.ProductMappings.RemoveRange(await db.ProductMappings
            .Where(x => x.ReferenceEntityId == entityId)
            .ToListAsync(cancellationToken));
        db.TenantReferenceOverlays.RemoveRange(await db.TenantReferenceOverlays
            .Where(x => x.ReferenceEntityId == entityId)
            .ToListAsync(cancellationToken));
        db.ReferenceCrosswalks.RemoveRange(await db.ReferenceCrosswalks
            .Where(x => x.ReferenceEntityId == entityId)
            .ToListAsync(cancellationToken));
        db.ReferenceEntityVersions.RemoveRange(await db.ReferenceEntityVersions
            .Where(x => x.ReferenceEntityId == entityId)
            .ToListAsync(cancellationToken));

        if (!string.Equals(entity.Dataset.Status, ReferenceDatasetStatuses.Archived, StringComparison.OrdinalIgnoreCase))
        {
            entity.Dataset.Status = ReferenceDatasetStatuses.Ready;
            entity.Dataset.UpdatedAt = now;
        }

        db.ReferenceEntities.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            principal,
            "reference.entity.deleted",
            "reference_entity",
            entityId,
            before,
            null,
            cancellationToken);
    }

    public async Task<ReferenceStagingRecordResponse> ApproveAsync(
        ClaimsPrincipal principal,
        Guid stagingId,
        ReviewDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        return await ReviewAsync(principal, stagingId, ReferenceStagingStatuses.Approved, request, cancellationToken);
    }

    public async Task<ReferenceStagingRecordResponse> RejectAsync(
        ClaimsPrincipal principal,
        Guid stagingId,
        ReviewDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        return await ReviewAsync(principal, stagingId, ReferenceStagingStatuses.Rejected, request, cancellationToken);
    }

    public async Task<ReferenceStagingRecordResponse> MergeAsync(
        ClaimsPrincipal principal,
        Guid stagingId,
        ReviewDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        return await ReviewAsync(principal, stagingId, ReferenceStagingStatuses.Merged, request, cancellationToken);
    }

    public async Task<ReferenceStagingRecordResponse> EscalateAsync(
        ClaimsPrincipal principal,
        Guid stagingId,
        ReviewDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        return await ReviewAsync(principal, stagingId, ReferenceStagingStatuses.Escalated, request, cancellationToken);
    }

    public async Task<ReferencePublishEventResponse> PublishDatasetAsync(
        ClaimsPrincipal principal,
        Guid datasetId,
        string? summary,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var dataset = await db.ReferenceDatasets.FirstOrDefaultAsync(x => x.Id == datasetId, cancellationToken)
            ?? throw new StlApiException("reference.dataset_not_found", "Dataset was not found.", 404);
        EnsureDatasetPublishable(dataset);
        return await PublishDatasetInternalAsync(principal, dataset, summary, cancellationToken);
    }

    public async Task<ReferencePublishBatchResponse> PublishDatasetsAsync(
        ClaimsPrincipal principal,
        PublishReferenceDatasetsRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var datasetIds = request.DatasetIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (datasetIds.Count == 0)
        {
            throw new StlApiException(
                "reference.publish_batch_empty",
                "Select at least one dataset to publish.",
                400);
        }

        var datasets = await db.ReferenceDatasets
            .Where(x => datasetIds.Contains(x.Id))
            .OrderBy(x => x.Key)
            .ToListAsync(cancellationToken);

        if (datasets.Count != datasetIds.Count)
        {
            throw new StlApiException(
                "reference.dataset_not_found",
                "One or more datasets were not found.",
                404);
        }

        var items = new List<ReferencePublishEventResponse>(datasets.Count);
        foreach (var dataset in datasets)
        {
            EnsureDatasetPublishable(dataset);
            items.Add(await PublishDatasetInternalAsync(principal, dataset, request.Summary, cancellationToken));
        }

        return new ReferencePublishBatchResponse(
            RequestedCount: datasetIds.Count,
            PublishedCount: items.Count,
            Items: items,
            ProcessedAt: DateTimeOffset.UtcNow);
    }

    public async Task<ReferencePublishBatchResponse> PublishAllDatasetsAsync(
        ClaimsPrincipal principal,
        string? summary,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var datasets = await db.ReferenceDatasets
            .Where(x => x.Key != MasterCsvIntakeDatasetKey && x.Status != ReferenceDatasetStatuses.Archived)
            .OrderBy(x => x.Key)
            .ToListAsync(cancellationToken);

        var items = new List<ReferencePublishEventResponse>(datasets.Count);
        foreach (var dataset in datasets)
        {
            items.Add(await PublishDatasetInternalAsync(principal, dataset, summary, cancellationToken));
        }

        return new ReferencePublishBatchResponse(
            RequestedCount: datasets.Count,
            PublishedCount: items.Count,
            Items: items,
            ProcessedAt: DateTimeOffset.UtcNow);
    }

    public async Task<IReadOnlyList<ReferencePublishEventResponse>> ListPublishHistoryAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        return await (
            from publishEvent in db.ReferencePublishEvents.AsNoTracking()
            join dataset in db.ReferenceDatasets.AsNoTracking() on publishEvent.DatasetId equals dataset.Id
            orderby publishEvent.CreatedAt descending
            select new ReferencePublishEventResponse(
                publishEvent.Id,
                publishEvent.DatasetId,
                dataset.Key,
                dataset.Name,
                publishEvent.PublishedVersion,
                publishEvent.PublishedByPersonId,
                publishEvent.Summary,
                publishEvent.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReferenceCrosswalkResponse>> ListCrosswalksAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        return await (
            from crosswalk in db.ReferenceCrosswalks.AsNoTracking()
            join entity in db.ReferenceEntities.AsNoTracking() on crosswalk.ReferenceEntityId equals entity.Id
            join sourceGroup in db.ReferenceSources.AsNoTracking() on crosswalk.SourceId equals sourceGroup.Id into sources
            from source in sources.DefaultIfEmpty()
            orderby crosswalk.UpdatedAt descending
            select new ReferenceCrosswalkResponse(
                crosswalk.Id,
                crosswalk.ReferenceEntityId,
                entity.EntityType,
                entity.CanonicalKey,
                entity.DisplayName,
                crosswalk.ExternalSystem,
                crosswalk.ExternalKey,
                crosswalk.SourceId,
                source != null ? source.Key : null,
                crosswalk.Confidence,
                crosswalk.Status,
                crosswalk.CreatedAt,
                crosswalk.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReferenceCrosswalkResponse> CreateCrosswalkAsync(
        ClaimsPrincipal principal,
        CreateReferenceCrosswalkRequest request,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var entity = await db.ReferenceEntities.Include(x => x.Dataset).FirstOrDefaultAsync(x => x.Id == request.ReferenceEntityId, cancellationToken)
            ?? throw new StlApiException("reference.entity_not_found", "Reference entity was not found.", 404);

        if (await db.ReferenceCrosswalks.AnyAsync(x => x.ExternalSystem == request.ExternalSystem.Trim() && x.ExternalKey == request.ExternalKey.Trim(), cancellationToken))
        {
            throw new StlApiException("reference.crosswalk_conflict", "External crosswalk already exists.", 409);
        }

        var now = DateTimeOffset.UtcNow;
        var crosswalk = new ReferenceCrosswalk
        {
            Id = Guid.NewGuid(),
            ReferenceEntityId = entity.Id,
            ExternalSystem = request.ExternalSystem.Trim(),
            ExternalKey = request.ExternalKey.Trim(),
            SourceId = request.SourceId,
            Confidence = ClampConfidence(request.Confidence),
            Status = request.Status.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ReferenceCrosswalks.Add(crosswalk);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(principal, "reference.crosswalk.created", "reference_crosswalk", crosswalk.Id, null, Serialize(crosswalk), cancellationToken);
        return await GetCrosswalkResponseAsync(crosswalk.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<ReferenceEntityResponse>> GetCatalogEntitiesAsync(
        string datasetKey,
        ClaimsPrincipal principal,
        string? entityType,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);
        var normalizedDatasetKey = NormalizeKey(datasetKey);
        var query = db.ReferenceEntities.AsNoTracking()
            .Include(x => x.Dataset)
            .Where(x => x.Dataset.Key == normalizedDatasetKey && x.Status == ReferenceEntityStatuses.Active);

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var normalizedEntityType = NormalizeKey(entityType);
            query = query.Where(x => x.EntityType == normalizedEntityType);
        }

        var items = await query.OrderBy(x => x.DisplayName).ToListAsync(cancellationToken);
        return await Task.WhenAll(items.Select(entity => ToEntityResponseAsync(entity, cancellationToken)));
    }

    public async Task<ReferenceEntityResponse> GetEntityAsync(
        ClaimsPrincipal principal,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);
        var entity = await db.ReferenceEntities.AsNoTracking()
            .Include(x => x.Dataset)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new StlApiException("reference.entity_not_found", "Reference entity was not found.", 404);

        return await ToEntityResponseAsync(entity, cancellationToken);
    }

    public async Task<ReferenceLookupResponse> LookupAsync(
        ClaimsPrincipal principal,
        string? datasetKey,
        string? entityType,
        string? canonicalKey,
        string? externalSystem,
        string? externalKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);
        var query = db.ReferenceEntities.AsNoTracking().Include(x => x.Dataset).AsQueryable();

        if (!string.IsNullOrWhiteSpace(datasetKey))
        {
            query = query.Where(x => x.Dataset.Key == NormalizeKey(datasetKey));
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(x => x.EntityType == NormalizeKey(entityType));
        }

        if (!string.IsNullOrWhiteSpace(canonicalKey))
        {
            var normalizedCanonicalKey = NormalizeKey(canonicalKey);
            query = query.Where(x => x.CanonicalKey == normalizedCanonicalKey || x.DisplayName.Contains(canonicalKey.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(externalSystem) && !string.IsNullOrWhiteSpace(externalKey))
        {
            var normalizedExternalSystem = NormalizeKey(externalSystem);
            var normalizedExternalKey = externalKey.Trim();
            query = query.Where(x => db.ReferenceCrosswalks.Any(c => c.ReferenceEntityId == x.Id && c.ExternalSystem == normalizedExternalSystem && c.ExternalKey == normalizedExternalKey));
        }

        var items = await query.OrderBy(x => x.DisplayName).Take(25).ToListAsync(cancellationToken);
        return new ReferenceLookupResponse(
            Scope: string.Join(':', new[] { datasetKey, entityType, canonicalKey, externalSystem, externalKey }.Where(x => !string.IsNullOrWhiteSpace(x))),
            Query: canonicalKey ?? externalKey ?? datasetKey ?? entityType ?? string.Empty,
            Matches: await Task.WhenAll(items.Select(item => ToEntityResponseAsync(item, cancellationToken))),
            GeneratedAt: DateTimeOffset.UtcNow);
    }

    public async Task<ReferenceLookupResponse> ResolveCrosswalkAsync(
        ClaimsPrincipal principal,
        string externalSystem,
        string externalKey,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);
        var normalizedSystem = NormalizeKey(externalSystem);
        var normalizedExternalKey = externalKey.Trim();
        var entityIds = await db.ReferenceCrosswalks.AsNoTracking()
            .Where(x => x.ExternalSystem == normalizedSystem && x.ExternalKey == normalizedExternalKey)
            .Select(x => x.ReferenceEntityId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var entities = await db.ReferenceEntities.AsNoTracking()
            .Include(x => x.Dataset)
            .Where(x => entityIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        return new ReferenceLookupResponse(
            Scope: $"{normalizedSystem}:{normalizedExternalKey}",
            Query: normalizedExternalKey,
            Matches: await Task.WhenAll(entities.Select(entity => ToEntityResponseAsync(entity, cancellationToken))),
            GeneratedAt: DateTimeOffset.UtcNow);
    }

    public Task<ReferenceLookupResponse> DecodeVehicleAsync(
        ClaimsPrincipal principal,
        string vin,
        int modelYear,
        CancellationToken cancellationToken = default) =>
        LookupByEntityFieldAsync(principal, "vehicle", "vin", vin, $"{NormalizeVin(vin)}:{modelYear}", cancellationToken);

    public Task<ReferenceLookupResponse> LookupGtinAsync(
        ClaimsPrincipal principal,
        string gtin,
        CancellationToken cancellationToken = default) =>
        LookupByEntityFieldAsync(principal, "product", "gtin", gtin, NormalizeDigits(gtin), cancellationToken);

    public Task<ReferenceLookupResponse> LookupSdsAsync(
        ClaimsPrincipal principal,
        string manufacturer,
        string product,
        CancellationToken cancellationToken = default) =>
        LookupByDualFieldAsync(principal, "sds", "manufacturer", manufacturer, "productName", product, $"{manufacturer}:{product}", cancellationToken);

    public Task<ReferenceLookupResponse> LookupChemicalAsync(
        ClaimsPrincipal principal,
        string cas,
        CancellationToken cancellationToken = default) =>
        LookupByEntityFieldAsync(principal, "chemical", "cas", cas, NormalizeDigits(cas), cancellationToken);

    private async Task<ReferenceLookupResponse> LookupByEntityFieldAsync(
        ClaimsPrincipal principal,
        string entityType,
        string fieldName,
        string rawValue,
        string normalizedValue,
        CancellationToken cancellationToken)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);
        var normalizedEntityType = NormalizeKey(entityType);
        var entities = await db.ReferenceEntities.AsNoTracking()
            .Include(x => x.Dataset)
            .ToListAsync(cancellationToken);
        var matches = entities
            .Where(x => x.EntityType == normalizedEntityType && NormalizedFieldMatches(x.NormalizedFieldsJson, fieldName, normalizedValue))
            .ToList();

        return new ReferenceLookupResponse(
            Scope: $"{normalizedEntityType}:{fieldName}",
            Query: rawValue,
            Matches: await Task.WhenAll(matches.Select(entity => ToEntityResponseAsync(entity, cancellationToken))),
            GeneratedAt: DateTimeOffset.UtcNow);
    }

    private async Task<ReferenceLookupResponse> LookupByDualFieldAsync(
        ClaimsPrincipal principal,
        string entityType,
        string fieldOne,
        string valueOne,
        string fieldTwo,
        string valueTwo,
        string query,
        CancellationToken cancellationToken)
    {
        await authorization.RequireNexArrAccessAsync(principal, cancellationToken);
        var normalizedEntityType = NormalizeKey(entityType);
        var entities = await db.ReferenceEntities.AsNoTracking()
            .Include(x => x.Dataset)
            .ToListAsync(cancellationToken);
        var normalizedValueOne = valueOne.Trim();
        var normalizedValueTwo = valueTwo.Trim();
        var matches = entities
            .Where(x => x.EntityType == normalizedEntityType
                && NormalizedFieldMatches(x.NormalizedFieldsJson, fieldOne, normalizedValueOne)
                && NormalizedFieldMatches(x.NormalizedFieldsJson, fieldTwo, normalizedValueTwo))
            .ToList();

        return new ReferenceLookupResponse(
            Scope: $"{normalizedEntityType}:{fieldOne}:{fieldTwo}",
            Query: query,
            Matches: await Task.WhenAll(matches.Select(entity => ToEntityResponseAsync(entity, cancellationToken))),
            GeneratedAt: DateTimeOffset.UtcNow);
    }

    private async Task<ReferenceStagingRecordResponse> ReviewAsync(
        ClaimsPrincipal principal,
        Guid stagingId,
        string nextStatus,
        ReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var staging = await db.StagingRecords
            .Include(x => x.Job).ThenInclude(x => x.Dataset)
            .Include(x => x.Job).ThenInclude(x => x.Source)
            .Include(x => x.TargetDataset)
            .FirstOrDefaultAsync(x => x.Id == stagingId, cancellationToken)
            ?? throw new StlApiException("reference.staging_not_found", "Staging record was not found.", 404);

        var before = Serialize(staging);
        staging.Status = nextStatus;
        staging.ReviewReason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        staging.ReviewerPersonId = principal.GetUserId();
        staging.ReviewedAt = now;
        staging.UpdatedAt = now;

        if (request.TargetDatasetId is Guid targetDatasetId)
        {
            var targetDataset = await ResolveTargetDatasetAsync(targetDatasetId, cancellationToken);
            staging.TargetDatasetId = targetDataset.Id;
            staging.TargetDataset = targetDataset;
        }

        if (nextStatus is ReferenceStagingStatuses.Approved or ReferenceStagingStatuses.Merged)
        {
            if (string.Equals(staging.Job.Dataset.Key, MasterCsvIntakeDatasetKey, StringComparison.OrdinalIgnoreCase)
                && staging.TargetDatasetId is null)
            {
                throw new StlApiException(
                    "reference.target_dataset_required",
                    "Assign a target dataset before approving this row.",
                    400);
            }

            var entity = await UpsertEntityFromStagingAsync(staging, request, cancellationToken);
            staging.ReferenceEntityId = entity.Id;

            var affectedDataset = staging.TargetDatasetId == staging.Job.DatasetId || staging.TargetDatasetId is null
                ? staging.Job.Dataset
                : staging.TargetDataset;
            if (affectedDataset is not null
                && !string.Equals(affectedDataset.Status, ReferenceDatasetStatuses.Archived, StringComparison.OrdinalIgnoreCase))
            {
                affectedDataset.Status = ReferenceDatasetStatuses.Ready;
                affectedDataset.UpdatedAt = now;
            }
        }

        var remainingReviewCount = await db.StagingRecords.CountAsync(
            x => x.JobId == staging.JobId && x.Status == ReferenceStagingStatuses.NeedsReview,
            cancellationToken);
        if (remainingReviewCount == 0)
        {
            staging.Job.Status = ReferenceImportStatuses.Completed;
            staging.Job.CompletedAt = now;
        }
        else if (!string.Equals(staging.Job.Status, ReferenceImportStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            staging.Job.Status = ReferenceImportStatuses.ReviewRequired;
        }

        staging.Job.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(principal, $"reference.staging.{nextStatus}", "staging_record", staging.Id, before, Serialize(staging), cancellationToken);
        return ToStagingResponse(staging);
    }

    private async Task<ReferenceEntity> UpsertEntityFromStagingAsync(
        StagingRecord staging,
        ReviewDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var normalized = DeserializeObject(staging.NormalizedPayloadJson);
        var targetDatasetId = staging.TargetDatasetId ?? staging.Job.DatasetId;
        var targetDataset = await db.ReferenceDatasets.AsNoTracking().FirstAsync(x => x.Id == targetDatasetId, cancellationToken);
        var canonicalKey = NormalizeKey(request.CanonicalKey ?? staging.ProposedCanonicalKey ?? DetermineFallbackCanonicalKey(normalized, targetDataset));
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? DetermineDisplayName(normalized, canonicalKey)
            : request.DisplayName.Trim();
        var normalizedFieldsJson = NormalizeJson(request.NormalizedFieldsJson ?? staging.NormalizedPayloadJson);
        var sourceId = staging.Job.SourceId;

        ReferenceEntity entity;
        if (!string.IsNullOrWhiteSpace(staging.ProposedCanonicalKey)
            && await db.ReferenceEntities.AnyAsync(x => x.DatasetId == targetDatasetId && x.CanonicalKey == canonicalKey, cancellationToken))
        {
            entity = await db.ReferenceEntities.FirstAsync(x => x.DatasetId == targetDatasetId && x.CanonicalKey == canonicalKey, cancellationToken);
            entity.DisplayName = displayName;
            entity.NormalizedFieldsJson = normalizedFieldsJson;
            entity.Status = ReferenceEntityStatuses.Active;
            entity.UpdatedAt = now;
        }
        else
        {
            entity = new ReferenceEntity
            {
                Id = Guid.NewGuid(),
                DatasetId = targetDatasetId,
                EntityType = NormalizeKey(staging.ProposedEntityType),
                CanonicalKey = canonicalKey,
                DisplayName = displayName,
                Status = ReferenceEntityStatuses.Active,
                NormalizedFieldsJson = normalizedFieldsJson,
                FirstSeenSourceId = sourceId,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.ReferenceEntities.Add(entity);
        }

        await db.SaveChangesAsync(cancellationToken);
        await UpsertEntityVersionAsync(entity, request, staging, cancellationToken);

        if (staging.Confidence >= 0.75m)
        {
            var externalKey = ResolveCrosswalkExternalKey(normalized, canonicalKey);
            var crosswalk = await db.ReferenceCrosswalks.FirstOrDefaultAsync(
                x => x.ExternalSystem == staging.Job.Source.Key && x.ExternalKey == externalKey,
                cancellationToken)
                ?? await db.ReferenceCrosswalks.FirstOrDefaultAsync(
                    x => x.ReferenceEntityId == entity.Id && x.ExternalSystem == staging.Job.Source.Key,
                    cancellationToken);

            if (crosswalk is null)
            {
                crosswalk = new ReferenceCrosswalk
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = now,
                };
                db.ReferenceCrosswalks.Add(crosswalk);
            }

            crosswalk.ReferenceEntityId = entity.Id;
            crosswalk.ExternalSystem = staging.Job.Source.Key;
            crosswalk.ExternalKey = externalKey;
            crosswalk.SourceId = staging.Job.SourceId;
            crosswalk.Confidence = staging.Confidence;
            crosswalk.Status = ReferenceCrosswalkStatuses.Active;
            crosswalk.UpdatedAt = now;
            await db.SaveChangesAsync(cancellationToken);
        }

        return entity;
    }

    private async Task UpsertEntityVersionAsync(
        ReferenceEntity entity,
        ReviewDecisionRequest request,
        StagingRecord staging,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var currentVersion = await db.ReferenceEntityVersions
            .Where(x => x.ReferenceEntityId == entity.Id)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);

        var versionNumber = (currentVersion?.Version ?? 0) + 1;
        var version = new ReferenceEntityVersion
        {
            Id = Guid.NewGuid(),
            ReferenceEntityId = entity.Id,
            Version = versionNumber,
            FieldsJson = NormalizeJson(request.NormalizedFieldsJson ?? staging.NormalizedPayloadJson),
            SourceEvidenceJson = NormalizeJson(request.SourceEvidenceJson ?? staging.RawPayloadJson),
            EffectiveDate = request.EffectiveDate,
            PublishedAt = staging.Status == ReferenceStagingStatuses.Approved ? now : null,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ReferenceEntityVersions.Add(version);
        entity.CurrentVersionId = version.Id;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<ReferenceImportResponse> BuildImportResponseAsync(
        IngestionJob job,
        CancellationToken cancellationToken)
    {
        var stagingSummary = await db.StagingRecords.AsNoTracking()
            .Where(x => x.JobId == job.Id)
            .GroupBy(x => x.JobId)
            .Select(g => new ImportStagingSummary(
                g.Count(),
                g.Count(x => x.Status == ReferenceStagingStatuses.NeedsReview),
                g.Count(x => x.Status == ReferenceStagingStatuses.Approved),
                g.Count(x => x.Status == ReferenceStagingStatuses.Rejected)))
            .FirstOrDefaultAsync(cancellationToken)
            ?? ImportStagingSummary.Empty;

        return BuildImportResponse(job, stagingSummary);
    }

    private static ReferenceImportResponse BuildImportResponse(
        IngestionJob job,
        ImportStagingSummary stagingSummary) =>
        new(
            job.Id,
            job.DatasetId,
            job.Dataset.Key,
            job.Dataset.Name,
            job.SourceId,
            job.Source.Key,
            job.Source.Name,
            job.TenantId,
            job.RequestedByPersonId,
            job.Status,
            job.RawObjectKey,
            job.FileName,
            job.StartedAt,
            job.CompletedAt,
            job.ErrorSummary,
            stagingSummary.StagingRecordCount,
            stagingSummary.PendingReviewCount,
            stagingSummary.ApprovedCount,
            stagingSummary.RejectedCount,
            job.CreatedAt,
            job.UpdatedAt);

    private async Task<ReferenceSource> EnsurePlatformInputSourceAsync(CancellationToken cancellationToken)
    {
        const string sourceKey = "platform-admin-input";
        var now = DateTimeOffset.UtcNow;
        var source = await db.ReferenceSources.FirstOrDefaultAsync(x => x.Key == sourceKey, cancellationToken);
        if (source is not null)
        {
            return source;
        }

        source = new ReferenceSource
        {
            Id = Guid.NewGuid(),
            Key = sourceKey,
            Name = "Platform admin input",
            SourceType = "manual",
            ConnectorType = "platform_admin_input",
            AuthorityRank = 1000,
            RefreshCadence = "on_demand",
            TermsNotes = "Curated via NexArr platform admin dataset input.",
            Enabled = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ReferenceSources.Add(source);
        await db.SaveChangesAsync(cancellationToken);
        return source;
    }

    private static List<string> BuildInputValues(CreateReferenceDatasetInputRequest request)
    {
        var values = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.Value))
        {
            values.Add(request.Value.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.ValuesText))
        {
            values.AddRange(
                request.ValuesText
                    .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        return values;
    }

    private async Task<ReferenceDatasetResponse> ToDatasetResponseAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        var dataset = await db.ReferenceDatasets.AsNoTracking().FirstAsync(x => x.Id == datasetId, cancellationToken);
        return new ReferenceDatasetResponse(
            dataset.Id,
            dataset.Key,
            dataset.Name,
            dataset.Category,
            dataset.OwnerService,
            dataset.Status,
            dataset.CurrentPublishedVersion,
            SourceCount: await db.ReferenceSources.CountAsync(cancellationToken),
            EntityCount: await db.ReferenceEntities.CountAsync(x => x.DatasetId == datasetId, cancellationToken),
            PendingReviewCount: await db.StagingRecords.CountAsync(x =>
                (x.TargetDatasetId == datasetId || x.Job.DatasetId == datasetId)
                && x.Status == ReferenceStagingStatuses.NeedsReview,
                cancellationToken),
            FailedImportCount: await db.IngestionJobs.CountAsync(x => x.DatasetId == datasetId && x.Status == ReferenceImportStatuses.Failed, cancellationToken),
            LastPublishedAt: await db.ReferencePublishEvents.Where(x => x.DatasetId == datasetId).MaxAsync(x => (DateTimeOffset?)x.CreatedAt, cancellationToken),
            dataset.CreatedAt,
            dataset.UpdatedAt);
    }

    private async Task<ReferenceCrosswalkResponse> GetCrosswalkResponseAsync(Guid crosswalkId, CancellationToken cancellationToken)
    {
        return await (
            from crosswalk in db.ReferenceCrosswalks.AsNoTracking()
            join entity in db.ReferenceEntities.AsNoTracking() on crosswalk.ReferenceEntityId equals entity.Id
            join sourceGroup in db.ReferenceSources.AsNoTracking() on crosswalk.SourceId equals sourceGroup.Id into sources
            from source in sources.DefaultIfEmpty()
            where crosswalk.Id == crosswalkId
            select new ReferenceCrosswalkResponse(
                crosswalk.Id,
                crosswalk.ReferenceEntityId,
                entity.EntityType,
                entity.CanonicalKey,
                entity.DisplayName,
                crosswalk.ExternalSystem,
                crosswalk.ExternalKey,
                crosswalk.SourceId,
                source != null ? source.Key : null,
                crosswalk.Confidence,
                crosswalk.Status,
                crosswalk.CreatedAt,
                crosswalk.UpdatedAt))
            .FirstAsync(cancellationToken);
    }

    private async Task<ReferenceEntityResponse> ToEntityResponseAsync(ReferenceEntity entity, CancellationToken cancellationToken)
    {
        var versions = await db.ReferenceEntityVersions.AsNoTracking()
            .Where(x => x.ReferenceEntityId == entity.Id)
            .OrderByDescending(x => x.Version)
            .Select(x => new ReferenceEntityVersionResponse(
                x.Id,
                x.ReferenceEntityId,
                x.Version,
                x.FieldsJson,
                x.SourceEvidenceJson,
                x.EffectiveDate,
                x.PublishedAt,
                x.SupersededByVersionId,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var crosswalks = await (
            from crosswalk in db.ReferenceCrosswalks.AsNoTracking()
            join sourceGroup in db.ReferenceSources.AsNoTracking() on crosswalk.SourceId equals sourceGroup.Id into sources
            from source in sources.DefaultIfEmpty()
            where crosswalk.ReferenceEntityId == entity.Id
            select new ReferenceCrosswalkResponse(
                crosswalk.Id,
                crosswalk.ReferenceEntityId,
                entity.EntityType,
                entity.CanonicalKey,
                entity.DisplayName,
                crosswalk.ExternalSystem,
                crosswalk.ExternalKey,
                crosswalk.SourceId,
                source != null ? source.Key : null,
                crosswalk.Confidence,
                crosswalk.Status,
                crosswalk.CreatedAt,
                crosswalk.UpdatedAt))
            .ToListAsync(cancellationToken);

        var overlays = await db.TenantReferenceOverlays.AsNoTracking()
            .Where(x => x.ReferenceEntityId == entity.Id)
            .Select(x => new ReferenceTenantOverlayResponse(
                x.Id,
                x.TenantId,
                x.ReferenceEntityId,
                entity.EntityType,
                entity.CanonicalKey,
                x.LocalName,
                x.LocalStatus,
                x.Hidden,
                x.Notes,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var mappings = await db.ProductMappings.AsNoTracking()
            .Where(x => x.ReferenceEntityId == entity.Id)
            .Select(x => new ReferenceProductMappingResponse(
                x.Id,
                x.TenantId,
                x.ProductCode,
                x.ReferenceEntityId,
                entity.EntityType,
                entity.CanonicalKey,
                x.LocalEntityType,
                x.LocalEntityId,
                x.MappingStatus,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        var currentVersion = versions.FirstOrDefault(x => x.Id == entity.CurrentVersionId);
        return new ReferenceEntityResponse(
            entity.Id,
            entity.DatasetId,
            entity.Dataset.Key,
            entity.Dataset.Name,
            entity.EntityType,
            entity.CanonicalKey,
            entity.DisplayName,
            entity.Status,
            entity.NormalizedFieldsJson,
            entity.FirstSeenSourceId,
            await ResolveSourceKeyAsync(entity.FirstSeenSourceId, cancellationToken),
            entity.CurrentVersionId,
            currentVersion?.Version,
            currentVersion?.PublishedAt,
            entity.CreatedAt,
            entity.UpdatedAt,
            versions,
            crosswalks,
            overlays,
            mappings);
    }

    private ReferenceStagingRecordResponse ToStagingResponse(StagingRecord staging) =>
        new(
            staging.Id,
            staging.JobId,
            staging.Job.DatasetId,
            staging.Job.Dataset.Key,
            staging.Job.SourceId,
            staging.Job.Source.Key,
            staging.TargetDatasetId,
            staging.TargetDataset?.Key,
            staging.TargetDataset?.Name,
            staging.TargetDataset?.OwnerService,
            staging.RowNumber,
            staging.RawPayloadJson,
            staging.NormalizedPayloadJson,
            staging.ProposedEntityType,
            staging.ProposedCanonicalKey,
            staging.Confidence,
            staging.Status,
            staging.ReviewReason,
            staging.ReviewerPersonId,
            staging.ReviewedAt,
            staging.ReferenceEntityId,
            staging.CreatedAt,
            staging.UpdatedAt);

    private async Task<string?> ResolveSourceKeyAsync(Guid? sourceId, CancellationToken cancellationToken)
    {
        if (sourceId is not Guid id)
        {
            return null;
        }

        return await db.ReferenceSources.AsNoTracking().Where(x => x.Id == id).Select(x => x.Key).FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ReferenceDataset> ResolveTargetDatasetAsync(Guid targetDatasetId, CancellationToken cancellationToken)
    {
        var dataset = await db.ReferenceDatasets.FirstOrDefaultAsync(x => x.Id == targetDatasetId, cancellationToken)
            ?? throw new StlApiException("reference.dataset_not_found", "Target dataset was not found.", 404);

        if (string.Equals(dataset.Key, MasterCsvIntakeDatasetKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException("reference.target_dataset_invalid", "The master intake dataset cannot be used as a target.", 400);
        }

        return dataset;
    }

    private async Task<ReferenceDataset> EnsureMasterCsvIntakeDatasetAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var dataset = await db.ReferenceDatasets.FirstOrDefaultAsync(x => x.Key == MasterCsvIntakeDatasetKey, cancellationToken);
        if (dataset is not null)
        {
            return dataset;
        }

        dataset = new ReferenceDataset
        {
            Id = Guid.NewGuid(),
            Key = MasterCsvIntakeDatasetKey,
            Name = "Master Reference Intake",
            Category = "platform",
            OwnerService = "NexArr",
            Status = ReferenceDatasetStatuses.Ready,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ReferenceDatasets.Add(dataset);
        await db.SaveChangesAsync(cancellationToken);
        return dataset;
    }

    private async Task<ReferenceSource> EnsureMasterCsvSourceAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var source = await db.ReferenceSources.FirstOrDefaultAsync(x => x.Key == MasterCsvSourceKey, cancellationToken);
        if (source is not null)
        {
            return source;
        }

        source = new ReferenceSource
        {
            Id = Guid.NewGuid(),
            Key = MasterCsvSourceKey,
            Name = "Master CSV upload",
            SourceType = "manual",
            ConnectorType = "csv_upload",
            AuthorityRank = 1000,
            RefreshCadence = "on_demand",
            TermsNotes = "Platform admin master CSV intake source.",
            Enabled = true,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.ReferenceSources.Add(source);
        await db.SaveChangesAsync(cancellationToken);
        return source;
    }

    private async Task<IReadOnlyList<ParsedMasterCsvRow>> ParseMasterCsvRowsAsync(
        string csvText,
        CancellationToken cancellationToken)
    {
        var datasetIndex = (await db.ReferenceDatasets.AsNoTracking().ToListAsync(cancellationToken))
            .ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        var rows = ParseCsvRows(csvText);
        if (rows.Count == 0)
        {
            throw new StlApiException("reference.master_csv_empty", "Provide CSV content to import.", 400);
        }

        var headers = rows[0].Select(header => header.Trim()).ToList();
        if (headers.Count == 0)
        {
            throw new StlApiException("reference.master_csv_header", "The CSV must include a header row.", 400);
        }

        var parsedRows = new List<ParsedMasterCsvRow>(rows.Count - 1);
        for (var index = 1; index < rows.Count; index++)
        {
            var row = rows[index];
            var rowNumber = index + 1;
            var columnMap = BuildColumnMap(headers, row);
            if (columnMap.Count == 0)
            {
                continue;
            }

            var targetDataset = ResolveTargetDatasetFromColumns(columnMap, datasetIndex);
            var product = ReadColumn(columnMap, "product", "product_key", "owner_service", "ownerService", "productCode");
            var datasetName = ReadColumn(columnMap, "dataset", "dataset_name", "datasetName", "dataset_label");
            var entityType = ReadColumn(columnMap, "entity_type", "entityType", "record_type", "type") ?? targetDataset?.Category ?? "reference";
            var canonicalKey = ReadColumn(columnMap, "canonical_key", "canonicalKey", "key");
            var displayName = ReadColumn(columnMap, "display_name", "displayName", "name");
            var sourceSystem = ReadColumn(columnMap, "source_system", "sourceSystem", "source");
            var sourceKey = ReadColumn(columnMap, "source_key", "sourceKey");
            var confidence = ReadConfidence(ReadColumn(columnMap, "confidence", "score"), targetDataset is not null);

            var normalizedCanonicalKey =
                !string.IsNullOrWhiteSpace(canonicalKey) ? NormalizeKey(canonicalKey) :
                !string.IsNullOrWhiteSpace(displayName) ? NormalizeKey(displayName) :
                !string.IsNullOrWhiteSpace(datasetName) ? NormalizeKey(datasetName) :
                null;

            var rawPayloadJson = Serialize(new
            {
                rowNumber,
                columns = columnMap,
            });

            var normalizedPayloadJson = Serialize(new
            {
                rowNumber,
                product,
                dataset = datasetName,
                datasetKey = targetDataset?.Key,
                targetDatasetId = targetDataset?.Id,
                targetDatasetName = targetDataset?.Name,
                targetOwnerService = targetDataset?.OwnerService,
                entityType = NormalizeKey(entityType),
                canonicalKey = normalizedCanonicalKey,
                displayName,
                sourceSystem,
                sourceKey,
                confidence,
                data = BuildDataPayload(columnMap),
            });

            parsedRows.Add(new ParsedMasterCsvRow(
                rowNumber,
                targetDataset?.Id,
                rawPayloadJson,
                normalizedPayloadJson,
                NormalizeKey(entityType),
                normalizedCanonicalKey,
                confidence,
                targetDataset is null ? "Assign a target dataset before approving this row." : "Review and approve before upsert."));
        }

        return parsedRows;
    }

    private static ReferenceDataset? ResolveTargetDatasetFromColumns(
        IReadOnlyDictionary<string, string> columns,
        IReadOnlyDictionary<string, ReferenceDataset> datasets)
    {
        var datasetKey = ReadColumn(columns, "dataset_key", "datasetKey", "target_dataset_key", "targetDatasetKey");
        if (!string.IsNullOrWhiteSpace(datasetKey) && datasets.TryGetValue(NormalizeKey(datasetKey), out var directMatch))
        {
            return directMatch;
        }

        var product = ReadColumn(columns, "product", "product_key", "owner_service", "ownerService", "productCode");
        var datasetName = ReadColumn(columns, "dataset", "dataset_name", "datasetName", "dataset_label");
        if (!string.IsNullOrWhiteSpace(product) && !string.IsNullOrWhiteSpace(datasetName))
        {
            var combinedKey = $"{NormalizeKey(product)}-{NormalizeKey(datasetName)}";
            if (datasets.TryGetValue(combinedKey, out var combinedMatch))
            {
                return combinedMatch;
            }
        }

        if (!string.IsNullOrWhiteSpace(datasetName))
        {
            var normalizedName = NormalizeKey(datasetName);
            var nameMatches = datasets.Values
                .Where(x => string.Equals(NormalizeKey(x.Name), normalizedName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (nameMatches.Count == 1)
            {
                return nameMatches[0];
            }
        }

        return null;
    }

    private static IReadOnlyDictionary<string, string> BuildColumnMap(IReadOnlyList<string> headers, IReadOnlyList<string> row)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < headers.Count; index++)
        {
            var header = NormalizeColumnKey(headers[index]);
            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            var value = index < row.Count ? row[index].Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            map[header] = value;
        }

        return map;
    }

    private static IReadOnlyDictionary<string, object?> BuildDataPayload(IReadOnlyDictionary<string, string> columns)
    {
        var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in columns)
        {
            if (IsRoutingColumn(key))
            {
                continue;
            }

            payload[key] = value;
        }

        return payload;
    }

    private static bool IsRoutingColumn(string columnName) =>
        string.Equals(columnName, NormalizeColumnKey("product"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("product_key"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("owner_service"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("product_code"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("dataset"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("dataset_name"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("dataset_label"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("dataset_key"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("target_dataset_key"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("entity_type"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("record_type"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("type"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("canonical_key"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("display_name"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("name"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("source_system"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("source"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("source_key"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("confidence"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("score"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("fields_json"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("normalized_fields_json"), StringComparison.OrdinalIgnoreCase)
        || string.Equals(columnName, NormalizeColumnKey("source_evidence_json"), StringComparison.OrdinalIgnoreCase);

    private static string? ReadColumn(IReadOnlyDictionary<string, string> columns, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            var normalizedAlias = NormalizeColumnKey(alias);
            if (columns.TryGetValue(normalizedAlias, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    private static decimal ReadConfidence(string? confidence, bool targetAssigned)
    {
        if (decimal.TryParse(confidence, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return ClampConfidence(parsed);
        }

        return targetAssigned ? 0.9m : 0.5m;
    }

    private static string DetermineFallbackCanonicalKey(JsonElement normalized, ReferenceDataset targetDataset)
    {
        if (normalized.ValueKind == JsonValueKind.Object)
        {
            foreach (var key in new[] { "canonicalKey", "displayName", "name", "productName", "entityType" })
            {
                if (normalized.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    var text = value.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return NormalizeKey(text);
                    }
                }
            }
        }

        return NormalizeKey(targetDataset.Name);
    }

    private static string ResolveCrosswalkExternalKey(JsonElement normalized, string fallbackCanonicalKey)
    {
        if (normalized.ValueKind == JsonValueKind.Object
            && normalized.TryGetProperty("sourceKey", out var sourceKeyValue)
            && sourceKeyValue.ValueKind == JsonValueKind.String)
        {
            var sourceKey = sourceKeyValue.GetString();
            if (!string.IsNullOrWhiteSpace(sourceKey))
            {
                return NormalizeKey(sourceKey);
            }
        }

        return fallbackCanonicalKey;
    }

    private static string NormalizeColumnKey(string value) =>
        new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    private static IReadOnlyList<IReadOnlyList<string>> ParseCsvRows(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return [];
        }

        return csv
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseCsvRow)
            .ToList();
    }

    private static IReadOnlyList<string> ParseCsvRow(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (inQuotes)
            {
                if (character == '"')
                {
                    if (index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(character);
                }

                continue;
            }

            if (character == '"')
            {
                inQuotes = true;
                continue;
            }

            if (character == ',')
            {
                fields.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        fields.Add(current.ToString().Trim());
        return fields;
    }

    private sealed record ParsedMasterCsvRow(
        int RowNumber,
        Guid? TargetDatasetId,
        string RawPayloadJson,
        string NormalizedPayloadJson,
        string ProposedEntityType,
        string? ProposedCanonicalKey,
        decimal Confidence,
        string ReviewReason);

    private async Task<ReferencePublishEventResponse> PublishDatasetInternalAsync(
        ClaimsPrincipal principal,
        ReferenceDataset dataset,
        string? summary,
        CancellationToken cancellationToken)
    {
        var before = Serialize(dataset);
        var nextVersionNumber = await db.ReferencePublishEvents.CountAsync(x => x.DatasetId == dataset.Id, cancellationToken) + 1;
        var publishedVersion = $"v{nextVersionNumber}";
        var now = DateTimeOffset.UtcNow;

        dataset.CurrentPublishedVersion = publishedVersion;
        dataset.Status = ReferenceDatasetStatuses.Published;
        dataset.UpdatedAt = now;

        var publishEvent = new ReferencePublishEvent
        {
            Id = Guid.NewGuid(),
            DatasetId = dataset.Id,
            PublishedVersion = publishedVersion,
            PublishedByPersonId = principal.GetUserId(),
            Summary = string.IsNullOrWhiteSpace(summary) ? $"Published {dataset.Key} version {publishedVersion}" : summary.Trim(),
            CreatedAt = now,
        };

        db.ReferencePublishEvents.Add(publishEvent);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(
            principal,
            "reference.dataset.published",
            "reference_dataset",
            dataset.Id,
            before,
            Serialize(dataset),
            cancellationToken);

        return new ReferencePublishEventResponse(
            publishEvent.Id,
            dataset.Id,
            dataset.Key,
            dataset.Name,
            publishEvent.PublishedVersion,
            publishEvent.PublishedByPersonId,
            publishEvent.Summary,
            publishEvent.CreatedAt);
    }

    private static void EnsureDatasetPublishable(ReferenceDataset dataset)
    {
        if (string.Equals(dataset.Key, MasterCsvIntakeDatasetKey, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "reference.dataset_protected",
                "The master intake dataset cannot be published.",
                400);
        }

        if (string.Equals(dataset.Status, ReferenceDatasetStatuses.Archived, StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "reference.dataset_archived",
                "Archived datasets must be restored before publishing.",
                400);
        }
    }

    private async Task WriteAuditAsync(
        ClaimsPrincipal principal,
        string action,
        string entityType,
        Guid? entityId,
        string? before,
        string? after,
        CancellationToken cancellationToken)
    {
        db.ReferenceAuditEvents.Add(new ReferenceAuditEvent
        {
            Id = Guid.NewGuid(),
            ActorPersonId = principal.Identity?.IsAuthenticated == true ? principal.GetUserId() : null,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            BeforeSnapshotJson = before,
            AfterSnapshotJson = after,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeKey(string value) =>
        value.Trim().ToLowerInvariant().Replace(' ', '-');

    private static string NormalizeVin(string vin) =>
        vin.Trim().ToUpperInvariant();

    private static string NormalizeDigits(string value) =>
        new(value.Where(char.IsDigit).ToArray());

    private static decimal ClampConfidence(decimal confidence) =>
        confidence < 0 ? 0 : confidence > 1 ? 1 : confidence;

    private static string NormalizeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "{}";
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement);
        }
        catch
        {
            return JsonSerializer.Serialize(new { value = json.Trim() });
        }
    }

    private static string Serialize(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static JsonElement DeserializeObject(string json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return document.RootElement.Clone();
    }

    private static bool NormalizedFieldMatches(string normalizedFieldsJson, string fieldName, string expectedValue)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(normalizedFieldsJson) ? "{}" : normalizedFieldsJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!document.RootElement.TryGetProperty(fieldName, out var property))
            {
                return false;
            }

            return property.ValueKind switch
            {
                JsonValueKind.String => string.Equals(property.GetString()?.Trim(), expectedValue.Trim(), StringComparison.OrdinalIgnoreCase),
                JsonValueKind.Number => string.Equals(property.ToString().Trim(), expectedValue.Trim(), StringComparison.OrdinalIgnoreCase),
                JsonValueKind.True or JsonValueKind.False => string.Equals(property.GetBoolean().ToString(), expectedValue.Trim(), StringComparison.OrdinalIgnoreCase),
                _ => string.Equals(property.ToString().Trim(), expectedValue.Trim(), StringComparison.OrdinalIgnoreCase)
            };
        }
        catch
        {
            return false;
        }
    }

    private static void AddCounts(Dictionary<Guid, int> target, IReadOnlyDictionary<Guid, int> source)
    {
        foreach (var (key, value) in source)
        {
            target[key] = target.TryGetValue(key, out var existing) ? existing + value : value;
        }
    }

    private sealed record ImportStagingSummary(
        int StagingRecordCount,
        int PendingReviewCount,
        int ApprovedCount,
        int RejectedCount)
    {
        public static readonly ImportStagingSummary Empty = new(0, 0, 0, 0);
    }

    private static string DetermineDisplayName(JsonElement normalized, string fallback)
    {
        foreach (var key in new[] { "displayName", "name", "productName", "manufacturer", "make", "model" })
        {
            if (normalized.ValueKind == JsonValueKind.Object && normalized.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.Trim();
                }
            }
        }

        return fallback;
    }
}
