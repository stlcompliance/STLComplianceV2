using System.Security.Claims;
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
                SourceCount: await db.ReferenceSources.CountAsync(cancellationToken),
                EntityCount: await db.ReferenceEntities.CountAsync(x => x.DatasetId == dataset.Id, cancellationToken),
                PendingReviewCount: await db.StagingRecords.CountAsync(x => x.Job.DatasetId == dataset.Id && x.Status == ReferenceStagingStatuses.NeedsReview, cancellationToken),
                FailedImportCount: await db.IngestionJobs.CountAsync(x => x.DatasetId == dataset.Id && x.Status == ReferenceImportStatuses.Failed, cancellationToken),
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

    public async Task<IReadOnlyList<ReferenceImportResponse>> ListImportsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        await authorization.RequirePlatformAdminAsync(principal, cancellationToken);
        var jobs = await db.IngestionJobs.AsNoTracking()
            .Include(x => x.Dataset)
            .Include(x => x.Source)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return await Task.WhenAll(jobs.Select(job => BuildImportResponseAsync(job, cancellationToken)));
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

        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(principal, "reference.import.created", "ingestion_job", job.Id, null, Serialize(job), cancellationToken);
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
            .Where(x => x.JobId == jobId)
            .OrderBy(x => x.RowNumber ?? int.MaxValue)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(ToStagingResponse).ToList();
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

        var nextVersionNumber = await db.ReferencePublishEvents.CountAsync(x => x.DatasetId == datasetId, cancellationToken) + 1;
        var publishedVersion = $"v{nextVersionNumber}";
        var now = DateTimeOffset.UtcNow;
        dataset.CurrentPublishedVersion = publishedVersion;
        dataset.Status = ReferenceDatasetStatuses.Published;
        dataset.UpdatedAt = now;

        var publishEvent = new ReferencePublishEvent
        {
            Id = Guid.NewGuid(),
            DatasetId = datasetId,
            PublishedVersion = publishedVersion,
            PublishedByPersonId = principal.GetUserId(),
            Summary = string.IsNullOrWhiteSpace(summary) ? $"Published {dataset.Key} version {publishedVersion}" : summary.Trim(),
            CreatedAt = now,
        };

        db.ReferencePublishEvents.Add(publishEvent);
        await db.SaveChangesAsync(cancellationToken);
        await WriteAuditAsync(principal, "reference.dataset.published", "reference_dataset", dataset.Id, null, Serialize(dataset), cancellationToken);
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
            .FirstOrDefaultAsync(x => x.Id == stagingId, cancellationToken)
            ?? throw new StlApiException("reference.staging_not_found", "Staging record was not found.", 404);

        var before = Serialize(staging);
        staging.Status = nextStatus;
        staging.ReviewReason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        staging.ReviewerPersonId = principal.GetUserId();
        staging.ReviewedAt = now;
        staging.UpdatedAt = now;

        if (nextStatus is ReferenceStagingStatuses.Approved or ReferenceStagingStatuses.Merged)
        {
            var entity = await UpsertEntityFromStagingAsync(staging, request, cancellationToken);
            staging.ReferenceEntityId = entity.Id;
        }

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
        var canonicalKey = NormalizeKey(request.CanonicalKey ?? staging.ProposedCanonicalKey ?? staging.Job.Dataset.Key);
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? DetermineDisplayName(normalized, canonicalKey)
            : request.DisplayName.Trim();
        var normalizedFieldsJson = NormalizeJson(request.NormalizedFieldsJson ?? staging.NormalizedPayloadJson);
        var sourceId = staging.Job.SourceId;

        ReferenceEntity entity;
        if (!string.IsNullOrWhiteSpace(staging.ProposedCanonicalKey)
            && await db.ReferenceEntities.AnyAsync(x => x.DatasetId == staging.Job.DatasetId && x.CanonicalKey == canonicalKey, cancellationToken))
        {
            entity = await db.ReferenceEntities.FirstAsync(x => x.DatasetId == staging.Job.DatasetId && x.CanonicalKey == canonicalKey, cancellationToken);
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
                DatasetId = staging.Job.DatasetId,
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

        if (staging.Confidence >= 0.75m
            && !await db.ReferenceCrosswalks.AnyAsync(
                x => x.ReferenceEntityId == entity.Id && x.ExternalSystem == staging.Job.Source.Key,
                cancellationToken))
        {
            db.ReferenceCrosswalks.Add(new ReferenceCrosswalk
            {
                Id = Guid.NewGuid(),
                ReferenceEntityId = entity.Id,
                ExternalSystem = staging.Job.Source.Key,
                ExternalKey = canonicalKey,
                SourceId = staging.Job.SourceId,
                Confidence = staging.Confidence,
                Status = ReferenceCrosswalkStatuses.Active,
                CreatedAt = now,
                UpdatedAt = now,
            });
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
        var staging = await db.StagingRecords.AsNoTracking().Where(x => x.JobId == job.Id).ToListAsync(cancellationToken);
        return new ReferenceImportResponse(
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
            staging.Count,
            staging.Count(x => x.Status == ReferenceStagingStatuses.NeedsReview),
            staging.Count(x => x.Status == ReferenceStagingStatuses.Approved),
            staging.Count(x => x.Status == ReferenceStagingStatuses.Rejected),
            job.CreatedAt,
            job.UpdatedAt);
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
            PendingReviewCount: await db.StagingRecords.CountAsync(x => x.Job.DatasetId == datasetId && x.Status == ReferenceStagingStatuses.NeedsReview, cancellationToken),
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
