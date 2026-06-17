using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using NexArr.Api.Data;
using NexArr.Api.Entities;
using STLCompliance.Shared.Ai;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Contracts;
using STLCompliance.Shared.SmartImport;

namespace NexArr.Api.Services;

public sealed class SmartImportService(
    NexArrDbContext db,
    RecordArrSmartImportClient recordArrClient,
    SmartImportDestinationClient destinationClient,
    IAiProvider aiProvider,
    IAiPromptRenderer promptRenderer,
    IAiResponseValidator responseValidator,
    IOptions<AiProviderOptions> aiOptions,
    ICorrelationIdAccessor correlationIdAccessor)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> ImplementedProducts = new(StringComparer.OrdinalIgnoreCase)
    {
        "nexarr",
        "staffarr",
        "trainarr",
        "maintainarr",
        "routarr",
        "supplyarr",
        "customarr",
        "ordarr",
        "compliancecore",
        "loadarr",
        "recordarr",
        "reportarr",
        "assurarr",
        "fieldcompanion"
    };

    public async Task<SmartImportUploadResponse> CreateBatchAsync(
        ClaimsPrincipal principal,
        IFormFile file,
        string? destinationProductHint,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new StlApiException("smart_import.empty_file", "Smart Import requires a non-empty file.", 400);
        }

        var tenantId = principal.GetTenantId();
        var actorPersonId = principal.GetPersonId();
        var destinationProduct = NormalizeProduct(destinationProductHint);
        RequireImportAccess(principal, tenantId, destinationProduct);

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();
        var sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;
        var batchId = Guid.NewGuid();
        var importFileId = Guid.NewGuid();

        var retained = await recordArrClient.RetainSourceAsync(
            new RecordArrSmartImportRetainSourceRequest(
                tenantId,
                actorPersonId,
                batchId,
                file.FileName,
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                file.Length,
                sha256,
                Convert.ToBase64String(bytes),
                destinationProduct),
            authorizationHeader,
            cancellationToken);

        var batch = new ImportBatch
        {
            Id = batchId,
            TenantId = tenantId,
            ActorPersonId = actorPersonId,
            Status = SmartImportStatuses.Uploaded,
            DestinationProductHint = destinationProduct,
            SourceLabel = file.FileName,
            ReviewPolicyJson = JsonSerializer.Serialize(BuildDefaultReviewPolicy(), JsonOptions),
            CreatedAt = now,
            UpdatedAt = now
        };
        var importFile = new ImportFile
        {
            Id = importFileId,
            ImportBatchId = batch.Id,
            TenantId = tenantId,
            FileName = file.FileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            SizeBytes = file.Length,
            Sha256 = sha256,
            RecordArrRecordId = retained.RecordId,
            RecordArrFileId = retained.FileId,
            RecordArrStorageKey = retained.StorageKey,
            Status = "retained",
            CreatedAt = now
        };

        db.ImportBatches.Add(batch);
        db.ImportFiles.Add(importFile);
        db.ImportAuditEvents.Add(ImportAudit(batch.Id, tenantId, actorPersonId, "smart_import.uploaded", "success", null, new
        {
            file = file.FileName,
            retained.RecordId,
            retained.FileId
        }));

        var csvRows = ParseCsvImportRows(bytes, file.FileName, importFile.ContentType);
        if (csvRows.Count > 0)
        {
            importFile.RowCount = csvRows.Count;
            StageCsvRowsForReview(batch, importFile, csvRows, now);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new SmartImportUploadResponse(
            batch.Id,
            importFile.Id,
            batch.Status,
            batch.DestinationProductHint,
            retained.RecordId,
            retained.FileId,
            csvRows.Count > 0
                ? $"Source file was retained in RecordArr and {csvRows.Count} delimited rows were staged for Smart Import review."
                : "Source file was retained in RecordArr and queued for Smart Import processing.");
    }

    public async Task<IReadOnlyList<SmartImportBatchSummary>> ListBatchesAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        return await db.ImportBatches.AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(100)
            .Select(x => new SmartImportBatchSummary(
                x.Id,
                x.TenantId,
                x.ActorPersonId,
                x.Status,
                x.DestinationProductHint,
                x.SourceLabel,
                db.ImportFiles.Count(file => file.ImportBatchId == x.Id),
                db.ImportProposedRecords.Count(record => record.ImportBatchId == x.Id),
                x.CreatedAt,
                x.UpdatedAt,
                x.ErrorCode,
                x.ErrorMessage))
            .ToListAsync(cancellationToken);
    }

    public async Task<SmartImportBatchDetail> GetBatchAsync(
        ClaimsPrincipal principal,
        Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var batch = await db.ImportBatches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == batchId && x.TenantId == tenantId, cancellationToken)
            ?? throw new StlApiException("smart_import.batch_not_found", "Smart Import batch was not found.", 404);

        return await BuildBatchDetailAsync(batch, cancellationToken);
    }

    public async Task<SmartImportBatchDetail> ProcessBatchAsync(
        Guid? tenantId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var candidates = await db.ImportBatches
            .Where(x => (tenantId == null || x.TenantId == tenantId)
                && (x.Status == SmartImportStatuses.Uploaded || x.Status == SmartImportStatuses.Failed))
            .OrderBy(x => x.CreatedAt)
            .Take(Math.Clamp(batchSize, 1, 20))
            .ToListAsync(cancellationToken);

        foreach (var batch in candidates)
        {
            await ProcessOneBatchAsync(batch, cancellationToken);
        }

        var first = candidates.FirstOrDefault();
        if (first is null)
        {
            throw new StlApiException("smart_import.no_pending_batches", "No Smart Import batches are pending.", 404);
        }

        return await BuildBatchDetailAsync(first, cancellationToken);
    }

    public async Task<SmartImportProposedRecordSummary> DecideAsync(
        ClaimsPrincipal principal,
        SmartImportReviewDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var actorPersonId = principal.GetPersonId();
        var proposed = await db.ImportProposedRecords.FirstOrDefaultAsync(
            x => x.Id == request.ProposedRecordId && x.TenantId == tenantId,
            cancellationToken)
            ?? throw new StlApiException("smart_import.proposed_record_not_found", "Proposed import record was not found.", 404);

        var decision = NormalizeDecision(request.Decision);
        var decidedAt = DateTimeOffset.UtcNow;
        proposed.ReviewStatus = decision;
        proposed.UpdatedAt = decidedAt;
        if (request.CorrectedPayload is JsonElement corrected)
        {
            proposed.DeterministicPayloadJson = corrected.GetRawText();
        }

        var batch = await db.ImportBatches.FirstAsync(x => x.Id == proposed.ImportBatchId, cancellationToken);
        var batchRecords = await db.ImportProposedRecords
            .Where(x => x.ImportBatchId == proposed.ImportBatchId && x.TenantId == tenantId)
            .ToListAsync(cancellationToken);
        if (batchRecords.Count > 0
            && batchRecords.All(x => x.ReviewStatus.Equals("rejected", StringComparison.OrdinalIgnoreCase)))
        {
            batch.Status = SmartImportStatuses.Rejected;
        }
        batch.UpdatedAt = decidedAt;

        db.ImportReviewDecisions.Add(new ImportReviewDecision
        {
            Id = Guid.NewGuid(),
            ImportBatchId = proposed.ImportBatchId,
            ImportProposedRecordId = proposed.Id,
            TenantId = tenantId,
            ReviewerPersonId = actorPersonId,
            Decision = decision,
            Notes = request.Notes,
            CorrectedPayloadJson = request.CorrectedPayload?.GetRawText(),
            DecidedAt = decidedAt
        });
        db.ImportAuditEvents.Add(ImportAudit(proposed.ImportBatchId, tenantId, actorPersonId, "smart_import.review_decision", "success", decision, new
        {
            proposed.Id,
            decision,
            batchStatus = batch.Status
        }));

        await db.SaveChangesAsync(cancellationToken);
        return ToProposedRecordSummary(proposed);
    }

    public async Task<SmartImportBulkReviewDecisionResponse> DecideBulkAsync(
        ClaimsPrincipal principal,
        Guid batchId,
        SmartImportBulkReviewDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var actorPersonId = principal.GetPersonId();
        var decision = NormalizeDecision(request.Decision);
        if (!decision.Equals("approved", StringComparison.OrdinalIgnoreCase))
        {
            throw new StlApiException(
                "smart_import.bulk_review_decision_not_supported",
                "Bulk Smart Import review currently supports approve all only.",
                400);
        }

        var batch = await db.ImportBatches.FirstOrDefaultAsync(
            x => x.Id == batchId && x.TenantId == tenantId,
            cancellationToken)
            ?? throw new StlApiException("smart_import.batch_not_found", "Smart Import batch was not found.", 404);

        var proposedQuery = db.ImportProposedRecords
            .Where(x => x.ImportBatchId == batchId && x.TenantId == tenantId);
        var totalCount = await proposedQuery.CountAsync(cancellationToken);

        var requestedIds = request.ProposedRecordIds?
            .Distinct()
            .ToHashSet();
        if (requestedIds is { Count: > 0 })
        {
            proposedQuery = proposedQuery.Where(x => requestedIds.Contains(x.Id));
        }

        var candidates = await proposedQuery
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        var requestedCount = requestedIds is { Count: > 0 } ? requestedIds.Count : candidates.Count;
        var now = DateTimeOffset.UtcNow;
        var updated = candidates
            .Where(x => !x.ReviewStatus.Equals(decision, StringComparison.OrdinalIgnoreCase)
                && !x.ReviewStatus.Equals("rejected", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var proposed in updated)
        {
            proposed.ReviewStatus = decision;
            proposed.UpdatedAt = now;
            db.ImportReviewDecisions.Add(new ImportReviewDecision
            {
                Id = Guid.NewGuid(),
                ImportBatchId = proposed.ImportBatchId,
                ImportProposedRecordId = proposed.Id,
                TenantId = tenantId,
                ReviewerPersonId = actorPersonId,
                Decision = decision,
                Notes = request.Notes,
                DecidedAt = now
            });
        }

        batch.UpdatedAt = now;
        db.ImportAuditEvents.Add(ImportAudit(batch.Id, tenantId, actorPersonId, "smart_import.bulk_review_decision", "success", decision, new
        {
            decision,
            requestedCount,
            updatedCount = updated.Count,
            skippedCount = Math.Max(0, requestedCount - updated.Count),
            totalProposedRecordCount = totalCount
        }));

        await db.SaveChangesAsync(cancellationToken);
        return new SmartImportBulkReviewDecisionResponse(
            batch.Id,
            decision,
            requestedCount,
            updated.Count,
            Math.Max(0, requestedCount - updated.Count),
            totalCount);
    }

    public async Task<SmartImportManualMappingOverrideResponse> ApplyManualMappingOverrideAsync(
        ClaimsPrincipal principal,
        Guid batchId,
        SmartImportManualMappingOverrideRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var actorPersonId = principal.GetPersonId();
        var mappings = NormalizeManualFieldMappings(request.FieldMappings);

        var batch = await db.ImportBatches.FirstOrDefaultAsync(
            x => x.Id == batchId && x.TenantId == tenantId,
            cancellationToken)
            ?? throw new StlApiException("smart_import.batch_not_found", "Smart Import batch was not found.", 404);

        var destinationProduct = NormalizeProduct(batch.DestinationProductHint);
        if (destinationProduct == "unknown")
        {
            destinationProduct = await db.ImportProposedRecords.AsNoTracking()
                .Where(x => x.ImportBatchId == batchId && x.TenantId == tenantId)
                .OrderBy(x => x.CreatedAt)
                .Select(x => x.DestinationProduct)
                .FirstOrDefaultAsync(cancellationToken)
                ?? "unknown";
        }

        RequireImportAccess(principal, tenantId, destinationProduct);

        var commitPlanExists = await db.ImportCommitPlans.AnyAsync(
            x => x.ImportBatchId == batchId && x.TenantId == tenantId,
            cancellationToken);
        if (commitPlanExists)
        {
            throw new StlApiException(
                "smart_import.mapping_override_after_commit_plan",
                "Manual mapping overrides must be applied before commit planning.",
                409);
        }

        var proposedRecords = await db.ImportProposedRecords
            .Where(x => x.ImportBatchId == batchId && x.TenantId == tenantId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var updatedCount = 0;

        foreach (var proposed in proposedRecords)
        {
            if (proposed.ReviewStatus.Equals("rejected", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!TryApplyManualFieldMappings(proposed, mappings, now))
            {
                continue;
            }

            updatedCount++;
        }

        batch.Status = SmartImportStatuses.ReviewRequired;
        batch.UpdatedAt = now;
        db.ImportAuditEvents.Add(ImportAudit(batch.Id, tenantId, actorPersonId, "smart_import.manual_mapping_override", "success", null, new
        {
            mappingCount = mappings.Count,
            updatedCount,
            skippedCount = Math.Max(0, proposedRecords.Count - updatedCount),
            totalProposedRecordCount = proposedRecords.Count,
            notes = request.Notes,
            mappings = mappings.Select(x => new { x.SourceField, x.TargetField }).ToArray()
        }));

        await db.SaveChangesAsync(cancellationToken);
        return new SmartImportManualMappingOverrideResponse(
            batch.Id,
            mappings.Count,
            updatedCount,
            Math.Max(0, proposedRecords.Count - updatedCount),
            proposedRecords.Count);
    }

    public async Task<SmartImportCommitPlanSummary> CreateCommitPlanAsync(
        ClaimsPrincipal principal,
        Guid batchId,
        SmartImportCreateCommitPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var actorPersonId = principal.GetPersonId();
        var batch = await db.ImportBatches.FirstOrDefaultAsync(
            x => x.Id == batchId && x.TenantId == tenantId,
            cancellationToken)
            ?? throw new StlApiException("smart_import.batch_not_found", "Smart Import batch was not found.", 404);

        var query = db.ImportProposedRecords.Where(x => x.ImportBatchId == batchId && x.TenantId == tenantId);
        if (request.ProposedRecordIds is { Count: > 0 })
        {
            query = query.Where(x => request.ProposedRecordIds.Contains(x.Id));
        }

        var approved = await query
            .Where(x => x.ReviewStatus == "approved")
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        if (approved.Count == 0)
        {
            throw new StlApiException(
                "smart_import.no_approved_records",
                "At least one reviewed and approved proposed record is required before creating a commit plan.",
                400);
        }

        var now = DateTimeOffset.UtcNow;
        var plan = new ImportCommitPlan
        {
            Id = Guid.NewGuid(),
            ImportBatchId = batchId,
            TenantId = tenantId,
            CreatedByPersonId = actorPersonId,
            Status = "draft",
            CreatedAt = now
        };
        db.ImportCommitPlans.Add(plan);

        var order = 0;
        foreach (var proposed in approved)
        {
            order++;
            db.ImportCommitSteps.Add(new ImportCommitStep
            {
                Id = Guid.NewGuid(),
                ImportCommitPlanId = plan.Id,
                ImportBatchId = batchId,
                ImportProposedRecordId = proposed.Id,
                TenantId = tenantId,
                StepOrder = order,
                DestinationProduct = proposed.DestinationProduct,
                EntityType = proposed.EntityType,
                Operation = proposed.Operation,
                Status = "pending",
                IdempotencyKey = $"smart-import:{batchId:D}:{proposed.Id:D}",
                PayloadJson = proposed.DeterministicPayloadJson ?? proposed.ProposedPayloadJson,
                CreatedAt = now
            });
        }

        batch.Status = SmartImportStatuses.ReadyToCommit;
        batch.UpdatedAt = now;
        db.ImportAuditEvents.Add(ImportAudit(batchId, tenantId, actorPersonId, "smart_import.commit_plan_created", "success", null, new
        {
            plan.Id,
            stepCount = approved.Count
        }));
        await db.SaveChangesAsync(cancellationToken);

        return await GetCommitPlanSummaryAsync(plan.Id, cancellationToken);
    }

    public async Task<SmartImportCommitPlanSummary> ApproveCommitPlanAsync(
        ClaimsPrincipal principal,
        Guid commitPlanId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var actorPersonId = principal.GetPersonId();
        var plan = await db.ImportCommitPlans.FirstOrDefaultAsync(
            x => x.Id == commitPlanId && x.TenantId == tenantId,
            cancellationToken)
            ?? throw new StlApiException("smart_import.commit_plan_not_found", "Smart Import commit plan was not found.", 404);

        plan.Status = "approved";
        plan.ApprovedByPersonId = actorPersonId;
        plan.ApprovedAt = DateTimeOffset.UtcNow;
        db.ImportAuditEvents.Add(ImportAudit(plan.ImportBatchId, tenantId, actorPersonId, "smart_import.commit_plan_approved", "success", null, new
        {
            plan.Id
        }));
        await db.SaveChangesAsync(cancellationToken);
        return await GetCommitPlanSummaryAsync(plan.Id, cancellationToken);
    }

    public async Task<SmartImportCommitResult> CommitAsync(
        ClaimsPrincipal principal,
        Guid commitPlanId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var actorPersonId = principal.GetPersonId();
        var plan = await db.ImportCommitPlans.FirstOrDefaultAsync(
            x => x.Id == commitPlanId && x.TenantId == tenantId,
            cancellationToken)
            ?? throw new StlApiException("smart_import.commit_plan_not_found", "Smart Import commit plan was not found.", 404);
        if (plan.Status == "committed")
        {
            var completedSteps = await db.ImportCommitSteps.AsNoTracking()
                .Where(x => x.ImportCommitPlanId == commitPlanId && x.TenantId == tenantId)
                .OrderBy(x => x.StepOrder)
                .ToListAsync(cancellationToken);
            return ToCommitResult(plan.Id, plan.Status, completedSteps);
        }

        if (plan.Status is not ("approved" or "failed" or "partially_committed"))
        {
            throw new StlApiException("smart_import.commit_plan_not_approved", "Commit plan must be approved before commit.", 400);
        }

        var batch = await db.ImportBatches.FirstAsync(x => x.Id == plan.ImportBatchId, cancellationToken);
        batch.Status = SmartImportStatuses.Committing;
        batch.UpdatedAt = DateTimeOffset.UtcNow;
        plan.Status = "committing";

        var steps = await db.ImportCommitSteps
            .Where(x => x.ImportCommitPlanId == commitPlanId && x.TenantId == tenantId)
            .OrderBy(x => x.StepOrder)
            .ToListAsync(cancellationToken);

        foreach (var step in steps.Where(x => x.Status == "pending" || x is { Status: "failed", Retryable: true }))
        {
            step.Status = "committing";
            step.ErrorCode = null;
            step.ErrorMessage = null;
            step.Retryable = false;
            step.CompletedAt = null;
            await db.SaveChangesAsync(cancellationToken);

            try
            {
                using var payload = JsonDocument.Parse(step.PayloadJson);
                var request = new SmartImportDestinationCommitRequest(
                    TenantId: tenantId,
                    ActorPersonId: batch.ActorPersonId,
                    ApprovedByPersonId: plan.ApprovedByPersonId ?? actorPersonId,
                    ImportBatchId: batch.Id,
                    CommitPlanId: plan.Id,
                    CommitStepId: step.Id,
                    DestinationProduct: step.DestinationProduct,
                    EntityType: step.EntityType,
                    Operation: step.Operation,
                    DeterministicPayload: payload.RootElement.Clone(),
                    RecordArrSourceRecordId: ResolveRecordArrSourceRecordId(payload.RootElement),
                    IdempotencyKey: step.IdempotencyKey);

                var response = await destinationClient.CommitAsync(request, cancellationToken);
                ApplyDestinationResponse(step, response);
            }
            catch (SmartImportDestinationCommitException ex)
            {
                MarkStepFailed(step, ex.ErrorCode, ex.Message, ex.Retryable);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                MarkStepFailed(
                    step,
                    "smart_import.destination_commit_unhandled",
                    ex.Message,
                    retryable: true);
            }

            db.ImportAuditEvents.Add(ImportAudit(batch.Id, tenantId, actorPersonId, "smart_import.commit_step", step.Status == "completed" ? "success" : "failed", step.ErrorCode, new
            {
                planId = plan.Id,
                stepId = step.Id,
                step.DestinationProduct,
                step.EntityType,
                step.ResultEntityId,
                step.ErrorMessage
            }));
            await db.SaveChangesAsync(cancellationToken);
        }

        var completedCount = steps.Count(x => x.Status == "completed");
        var failedCount = steps.Count(x => x.Status == "failed");
        var finishedAt = DateTimeOffset.UtcNow;
        if (completedCount == steps.Count && failedCount == 0)
        {
            plan.Status = "committed";
            plan.CommittedAt = finishedAt;
            batch.Status = SmartImportStatuses.Committed;
            batch.ErrorCode = null;
            batch.ErrorMessage = null;
        }
        else if (completedCount > 0)
        {
            plan.Status = "partially_committed";
            plan.CommittedAt ??= finishedAt;
            batch.Status = SmartImportStatuses.PartiallyCommitted;
            batch.ErrorCode = "smart_import.commit_partially_failed";
            batch.ErrorMessage = "One or more Smart Import commit steps failed after other steps were committed.";
        }
        else
        {
            plan.Status = "failed";
            batch.Status = SmartImportStatuses.Failed;
            batch.ErrorCode = steps.FirstOrDefault(x => x.Status == "failed")?.ErrorCode
                ?? "smart_import.commit_failed";
            batch.ErrorMessage = steps.FirstOrDefault(x => x.Status == "failed")?.ErrorMessage
                ?? "No Smart Import commit steps completed.";
        }

        batch.UpdatedAt = finishedAt;
        db.ImportAuditEvents.Add(ImportAudit(batch.Id, tenantId, actorPersonId, "smart_import.commit_completed", failedCount == 0 ? "success" : "failed", batch.ErrorCode, new
        {
            plan.Id,
            completedCount,
            failedCount
        }));
        await db.SaveChangesAsync(cancellationToken);

        return ToCommitResult(plan.Id, plan.Status, steps);
    }

    public async Task<SmartImportCommitResult> RetryStepAsync(
        ClaimsPrincipal principal,
        Guid commitPlanId,
        Guid stepId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = principal.GetTenantId();
        var step = await db.ImportCommitSteps.FirstOrDefaultAsync(
            x => x.Id == stepId && x.ImportCommitPlanId == commitPlanId && x.TenantId == tenantId,
            cancellationToken)
            ?? throw new StlApiException("smart_import.commit_step_not_found", "Smart Import commit step was not found.", 404);
        if (!step.Retryable)
        {
            throw new StlApiException("smart_import.commit_step_not_retryable", "This Smart Import commit step is not retryable.", 400);
        }

        step.Status = "pending";
        step.ErrorCode = null;
        step.ErrorMessage = null;
        step.CompletedAt = null;
        await db.SaveChangesAsync(cancellationToken);
        return await CommitAsync(principal, commitPlanId, cancellationToken);
    }

    private static void ApplyDestinationResponse(
        ImportCommitStep step,
        SmartImportDestinationCommitResponse response)
    {
        if (IsCommittedStatus(response.Status))
        {
            step.Status = "completed";
            step.ResultEntityId = response.ResultEntityId;
            step.ResultDisplayName = response.DisplayName;
            step.ErrorCode = null;
            step.ErrorMessage = null;
            step.Retryable = false;
            step.CompletedAt = DateTimeOffset.UtcNow;
            return;
        }

        MarkStepFailed(
            step,
            response.ErrorCode ?? "smart_import.destination_commit_not_completed",
            response.ErrorMessage ?? $"Destination returned Smart Import status '{response.Status}'.",
            response.Retryable);
    }

    private static void MarkStepFailed(
        ImportCommitStep step,
        string errorCode,
        string errorMessage,
        bool retryable)
    {
        step.Status = "failed";
        step.ErrorCode = errorCode;
        step.ErrorMessage = errorMessage.Length > 1024 ? errorMessage[..1024] : errorMessage;
        step.Retryable = retryable;
        step.CompletedAt = DateTimeOffset.UtcNow;
    }

    private static bool IsCommittedStatus(string status) =>
        status.Equals("committed", StringComparison.OrdinalIgnoreCase)
        || status.Equals("completed", StringComparison.OrdinalIgnoreCase)
        || status.Equals("created", StringComparison.OrdinalIgnoreCase)
        || status.Equals("updated", StringComparison.OrdinalIgnoreCase)
        || status.Equals("success", StringComparison.OrdinalIgnoreCase)
        || status.Equals("ok", StringComparison.OrdinalIgnoreCase);

    private static string? ResolveRecordArrSourceRecordId(JsonElement payload)
    {
        if (TryReadString(payload, "recordArrSourceRecordId", out var direct))
        {
            return direct;
        }

        if (payload.TryGetProperty("source", out var source))
        {
            if (TryReadString(source, "recordArrRecordId", out var camel))
            {
                return camel;
            }

            if (TryReadString(source, "RecordArrRecordId", out var pascal))
            {
                return pascal;
            }
        }

        return null;
    }

    private static bool TryReadString(JsonElement root, string propertyName, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(propertyName, out var element)
            || element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var candidate = element.GetString();
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        value = candidate.Trim();
        return true;
    }

    private async Task ProcessOneBatchAsync(ImportBatch batch, CancellationToken cancellationToken)
    {
        batch.Status = SmartImportStatuses.Processing;
        batch.ProcessingStartedAt = DateTimeOffset.UtcNow;
        batch.UpdatedAt = batch.ProcessingStartedAt.Value;
        await db.SaveChangesAsync(cancellationToken);

        var file = await db.ImportFiles.FirstAsync(x => x.ImportBatchId == batch.Id, cancellationToken);
        var destinationProduct = ResolveDestinationProduct(batch.DestinationProductHint, file.FileName);
        var entityType = ResolveEntityType(destinationProduct, file.FileName);
        var confidence = batch.DestinationProductHint == "unknown" ? 65m : 85m;
        var reviewReasons = BuildReviewReasons(destinationProduct, entityType, "create", confidence, file.ContentType);
        var providerOutcome = "deterministic";
        string proposedPayloadJson;

        if (!string.IsNullOrWhiteSpace(aiOptions.Value.ApiKey))
        {
            var ai = await aiProvider.CompleteAsync(
                new AiProviderRequest(
                    Purpose: "smart_import",
                    Category: AiRequestCategories.SmartImportClassification,
                    Model: aiOptions.Value.SmartImportModel,
                    Instructions: promptRenderer.RenderInstructions(
                        AiRequestCategories.SmartImportClassification,
                        destinationProduct,
                        ["classify", "extract", "map", "prepare_review_only_records"]),
                    Input: JsonSerializer.Serialize(new
                    {
                        file.FileName,
                        file.ContentType,
                        file.SizeBytes,
                        file.Sha256,
                        destinationProductHint = batch.DestinationProductHint,
                        recordArr = new { file.RecordArrRecordId, file.RecordArrFileId }
                    }, JsonOptions),
                    JsonSchemaName: "stl_smart_import_classification",
                    JsonSchema: SmartImportSchema,
                    MaxOutputTokens: aiOptions.Value.MaxOutputTokens,
                    CorrelationId: correlationIdAccessor.CorrelationId.ToString("N"),
                    RateLimitScope: "smart-import"),
                cancellationToken);

            providerOutcome = ai.Outcome;
            if (ai.Outcome == AiProviderOutcomes.Success)
            {
                var validation = responseValidator.ValidateJsonObject(ai.OutputText, ImplementedProducts);
                if (validation.Valid && validation.Json is not null)
                {
                    var root = validation.Json.RootElement;
                    destinationProduct = ReadString(root, "destinationProduct", destinationProduct);
                    entityType = ReadString(root, "entityType", entityType);
                    confidence = ReadDecimal(root, "confidence", confidence);
                    reviewReasons = BuildReviewReasons(destinationProduct, entityType, "create", confidence, file.ContentType);
                    proposedPayloadJson = ExtractAiProposedPayload(root, file, destinationProduct, entityType, confidence);
                    validation.Json.Dispose();
                }
                else
                {
                    proposedPayloadJson = BuildDeterministicPayload(file, destinationProduct, entityType, confidence);
                }
            }
            else
            {
                proposedPayloadJson = BuildDeterministicPayload(file, destinationProduct, entityType, confidence);
                if (ai.Outcome == AiProviderOutcomes.MissingConfig)
                {
                    reviewReasons = reviewReasons.Append("ai_not_configured").Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                }
            }
        }
        else
        {
            proposedPayloadJson = BuildDeterministicPayload(file, destinationProduct, entityType, confidence);
            reviewReasons = reviewReasons.Append("ai_not_configured").Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            providerOutcome = AiProviderOutcomes.MissingConfig;
        }

        db.ImportClassifications.Add(new ImportClassification
        {
            Id = Guid.NewGuid(),
            ImportBatchId = batch.Id,
            ImportFileId = file.Id,
            TenantId = batch.TenantId,
            DestinationProduct = destinationProduct,
            EntityType = entityType,
            Confidence = confidence,
            RequiresReview = true,
            ReviewReasonsJson = JsonSerializer.Serialize(reviewReasons, JsonOptions),
            Notes = providerOutcome == AiProviderOutcomes.MissingConfig
                ? "AI-assisted import is not configured. You can continue with manual import review or contact your administrator."
                : "Classification prepared for human review.",
            ProviderOutcome = providerOutcome,
            CreatedAt = DateTimeOffset.UtcNow
        });

        db.ImportProposedRecords.Add(new ImportProposedRecord
        {
            Id = Guid.NewGuid(),
            ImportBatchId = batch.Id,
            TenantId = batch.TenantId,
            DestinationProduct = destinationProduct,
            EntityType = entityType,
            Operation = "create",
            Confidence = confidence,
            ReviewStatus = "review_required",
            RequiresReview = true,
            ReviewReasonsJson = JsonSerializer.Serialize(reviewReasons, JsonOptions),
            ProposedPayloadJson = proposedPayloadJson,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        batch.Status = SmartImportStatuses.ReviewRequired;
        batch.ProcessingCompletedAt = DateTimeOffset.UtcNow;
        batch.UpdatedAt = batch.ProcessingCompletedAt.Value;
        db.ImportAuditEvents.Add(ImportAudit(batch.Id, batch.TenantId, null, "smart_import.processed", "success", providerOutcome, new
        {
            destinationProduct,
            entityType,
            confidence,
            reviewReasons
        }));
        await db.SaveChangesAsync(cancellationToken);
    }

    private void StageCsvRowsForReview(
        ImportBatch batch,
        ImportFile file,
        IReadOnlyList<CsvImportRow> rows,
        DateTimeOffset now)
    {
        var destinationProduct = ResolveDestinationProduct(batch.DestinationProductHint, file.FileName);
        var entityType = ResolveEntityType(destinationProduct, file.FileName);
        var confidence = batch.DestinationProductHint == "unknown" ? 75m : 90m;
        var reviewReasons = BuildReviewReasons(destinationProduct, entityType, "create", confidence, file.ContentType);

        db.ImportClassifications.Add(new ImportClassification
        {
            Id = Guid.NewGuid(),
            ImportBatchId = batch.Id,
            ImportFileId = file.Id,
            TenantId = batch.TenantId,
            DestinationProduct = destinationProduct,
            EntityType = entityType,
            Confidence = confidence,
            RequiresReview = true,
            ReviewReasonsJson = JsonSerializer.Serialize(reviewReasons, JsonOptions),
            Notes = $"{rows.Count} delimited rows were parsed into row-level import candidates for human review.",
            ProviderOutcome = "deterministic_csv",
            CreatedAt = now
        });

        foreach (var row in rows)
        {
            db.ImportProposedRecords.Add(new ImportProposedRecord
            {
                Id = Guid.NewGuid(),
                ImportBatchId = batch.Id,
                TenantId = batch.TenantId,
                DestinationProduct = destinationProduct,
                EntityType = entityType,
                Operation = "create",
                Confidence = confidence,
                ReviewStatus = "review_required",
                RequiresReview = true,
                ReviewReasonsJson = JsonSerializer.Serialize(reviewReasons, JsonOptions),
                ProposedPayloadJson = BuildCsvRowPayload(file, row, destinationProduct, entityType, confidence),
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        batch.Status = SmartImportStatuses.ReviewRequired;
        batch.ProcessingStartedAt = now;
        batch.ProcessingCompletedAt = now;
        batch.UpdatedAt = now;
        db.ImportAuditEvents.Add(ImportAudit(batch.Id, batch.TenantId, batch.ActorPersonId, "smart_import.csv_rows_staged", "success", "deterministic_csv", new
        {
            destinationProduct,
            entityType,
            rowCount = rows.Count,
            reviewReasons
        }));
    }

    private async Task<SmartImportBatchDetail> BuildBatchDetailAsync(ImportBatch batch, CancellationToken cancellationToken)
    {
        var files = await db.ImportFiles.AsNoTracking()
            .Where(x => x.ImportBatchId == batch.Id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new SmartImportFileSummary(
                x.Id,
                x.FileName,
                x.ContentType,
                x.SizeBytes,
                x.Sha256,
                x.RecordArrRecordId,
                x.RecordArrFileId,
                x.Status))
            .ToListAsync(cancellationToken);

        var classifications = await db.ImportClassifications.AsNoTracking()
            .Where(x => x.ImportBatchId == batch.Id)
            .OrderByDescending(x => x.Confidence)
            .ToListAsync(cancellationToken);

        var proposed = await db.ImportProposedRecords.AsNoTracking()
            .Where(x => x.ImportBatchId == batch.Id)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var plans = await db.ImportCommitPlans.AsNoTracking()
            .Where(x => x.ImportBatchId == batch.Id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var events = await db.ImportAuditEvents.AsNoTracking()
            .Where(x => x.ImportBatchId == batch.Id)
            .OrderByDescending(x => x.OccurredAt)
            .Take(100)
            .Select(x => new SmartImportAuditEventSummary(
                x.Id,
                x.EventType,
                x.ActorType,
                x.ActorPersonId,
                x.Result,
                x.ReasonCode,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        var summary = new SmartImportBatchSummary(
            batch.Id,
            batch.TenantId,
            batch.ActorPersonId,
            batch.Status,
            batch.DestinationProductHint,
            batch.SourceLabel,
            files.Count,
            proposed.Count,
            batch.CreatedAt,
            batch.UpdatedAt,
            batch.ErrorCode,
            batch.ErrorMessage);

        return new SmartImportBatchDetail(
            summary,
            files,
            classifications.Select(ToClassificationSummary).ToArray(),
            proposed.Select(ToProposedRecordSummary).ToArray(),
            plans.Select(plan => ToCommitPlanSummary(plan, db.ImportCommitSteps.Count(step => step.ImportCommitPlanId == plan.Id), db.ImportCommitSteps.Count(step => step.ImportCommitPlanId == plan.Id && step.Status == "completed"), db.ImportCommitSteps.Count(step => step.ImportCommitPlanId == plan.Id && step.Status == "failed"))).ToArray(),
            events);
    }

    private async Task<SmartImportCommitPlanSummary> GetCommitPlanSummaryAsync(
        Guid planId,
        CancellationToken cancellationToken)
    {
        var plan = await db.ImportCommitPlans.AsNoTracking().FirstAsync(x => x.Id == planId, cancellationToken);
        var steps = await db.ImportCommitSteps.AsNoTracking()
            .Where(x => x.ImportCommitPlanId == planId)
            .ToListAsync(cancellationToken);
        return ToCommitPlanSummary(
            plan,
            steps.Count,
            steps.Count(x => x.Status == "completed"),
            steps.Count(x => x.Status == "failed"));
    }

    private static SmartImportClassificationSummary ToClassificationSummary(ImportClassification classification) =>
        new(
            classification.Id,
            classification.DestinationProduct,
            classification.EntityType,
            classification.Confidence,
            classification.RequiresReview,
            DeserializeStringArray(classification.ReviewReasonsJson),
            classification.Notes);

    private static SmartImportProposedRecordSummary ToProposedRecordSummary(ImportProposedRecord proposed) =>
        new(
            proposed.Id,
            proposed.DestinationProduct,
            proposed.EntityType,
            proposed.Operation,
            proposed.Confidence,
            proposed.ReviewStatus,
            proposed.RequiresReview,
            DeserializeStringArray(proposed.ReviewReasonsJson),
            JsonDocument.Parse(proposed.ProposedPayloadJson).RootElement.Clone());

    private static SmartImportCommitPlanSummary ToCommitPlanSummary(
        ImportCommitPlan plan,
        int stepCount,
        int completedCount,
        int failedCount) =>
        new(plan.Id, plan.Status, stepCount, completedCount, failedCount, plan.CreatedAt, plan.ApprovedAt);

    private static SmartImportCommitResult ToCommitResult(Guid planId, string status, IReadOnlyList<ImportCommitStep> steps) =>
        new(
            planId,
            status,
            steps.Count(x => x.Status == "completed"),
            steps.Count(x => x.Status == "failed"),
            steps.Select(x => new SmartImportCommitStepResult(
                x.Id,
                x.DestinationProduct,
                x.EntityType,
                x.Operation,
                x.Status,
                x.ResultEntityId,
                x.ErrorCode,
                x.ErrorMessage,
                x.Retryable)).ToArray());

    private static IReadOnlyList<string> BuildReviewReasons(
        string destinationProduct,
        string entityType,
        string operation,
        decimal confidence,
        string contentType)
    {
        var reasons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!ImplementedProducts.Contains(destinationProduct) || destinationProduct is "ordarr")
        {
            reasons.Add(SmartImportReviewReasons.UnsupportedProductApi);
        }

        if (SmartImportConfidencePolicy.RequiresReview(confidence))
        {
            reasons.Add(confidence < 70m ? "low_confidence" : "review_required_by_confidence");
        }

        if (destinationProduct.Equals("staffarr", StringComparison.OrdinalIgnoreCase)
            && entityType.Contains("person", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.PersonCreateOrLink);
        }

        if (destinationProduct.Equals("trainarr", StringComparison.OrdinalIgnoreCase)
            || entityType.Contains("cert", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.TrainingOrCertificationRecord);
        }

        if (destinationProduct.Equals("compliancecore", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.ComplianceCoreImport);
        }

        if (destinationProduct.Equals("maintainarr", StringComparison.OrdinalIgnoreCase)
            && operation.Equals("update", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.AssetUpdate);
        }

        if (contentType.Contains("image", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add(SmartImportReviewReasons.ScanOrHandwriting);
            reasons.Add(SmartImportReviewReasons.RegulatoryRetention);
        }

        reasons.Add(SmartImportReviewReasons.HumanConfirmationRequired);
        return reasons.Order(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IReadOnlyDictionary<string, object> BuildDefaultReviewPolicy() => new Dictionary<string, object>
    {
        ["confidence95To100"] = SmartImportConfidencePolicy.AutofillPreviewed,
        ["confidence85To94"] = SmartImportConfidencePolicy.Preselected,
        ["confidence70To84"] = SmartImportConfidencePolicy.ReviewRequired,
        ["confidence50To69"] = SmartImportConfidencePolicy.WeakNotPreselected,
        ["below50"] = SmartImportConfidencePolicy.NoteOnly,
        ["allWritesRequireOwningProductApi"] = true
    };

    private static ImportAuditEvent ImportAudit(
        Guid batchId,
        Guid tenantId,
        Guid? actorPersonId,
        string eventType,
        string result,
        string? reasonCode,
        object metadata) =>
        new()
        {
            Id = Guid.NewGuid(),
            ImportBatchId = batchId,
            TenantId = tenantId,
            EventType = eventType,
            ActorType = actorPersonId is null ? "system" : "human",
            ActorPersonId = actorPersonId,
            Result = result,
            ReasonCode = reasonCode,
            MetadataJson = JsonSerializer.Serialize(metadata, JsonOptions),
            OccurredAt = DateTimeOffset.UtcNow
        };

    private static string NormalizeProduct(string? productKey)
    {
        var normalized = string.IsNullOrWhiteSpace(productKey)
            ? "unknown"
            : productKey.Trim().ToLowerInvariant();
        return normalized is "compliance-core" or "compliance_core" ? "compliancecore" : normalized;
    }

    private static string ResolveDestinationProduct(string hint, string fileName)
    {
        if (ImplementedProducts.Contains(hint))
        {
            return hint;
        }

        var name = fileName.ToLowerInvariant();
        if (name.Contains("training") || name.Contains("cert")) return "trainarr";
        if (name.Contains("asset") || name.Contains("work-order") || name.Contains("maintenance")) return "maintainarr";
        if (name.Contains("trip") || name.Contains("route") || name.Contains("bol") || name.Contains("pod")) return "routarr";
        if (name.Contains("vendor") || name.Contains("supplier") || name.Contains("purchase")) return "supplyarr";
        if (name.Contains("inventory") || name.Contains("receiving") || name.Contains("stock")) return "loadarr";
        if (name.Contains("rule") || name.Contains("citation") || name.Contains("compliance")) return "compliancecore";
        if (name.Contains("quality") || name.Contains("capa") || name.Contains("nonconformance")) return "assurarr";
        if (name.Contains("report")) return "reportarr";
        if (name.Contains("person") || name.Contains("staff")) return "staffarr";
        return "recordarr";
    }

    private static string ResolveEntityType(string destinationProduct, string fileName)
    {
        var name = fileName.ToLowerInvariant();
        return destinationProduct switch
        {
            "staffarr" => name.Contains("location") ? "location" : "person",
            "trainarr" => name.Contains("cert") ? "certification_record" : "training_record",
            "maintainarr" => name.Contains("work") ? "work_order" : "asset",
            "routarr" => name.Contains("bol") || name.Contains("pod") ? "transport_evidence" : "trip",
            "supplyarr" => name.Contains("purchase") ? "purchase_context" : "vendor",
            "loadarr" => name.Contains("receiving") ? "receiving_session" : "inventory_record",
            "compliancecore" => "staged_rulepack",
            "assurarr" => "quality_case",
            "reportarr" => "report_definition",
            "recordarr" => "document_metadata",
            _ => "unknown"
        };
    }

    private static string BuildDeterministicPayload(ImportFile file, string destinationProduct, string entityType, decimal confidence) =>
        JsonSerializer.Serialize(new
        {
            destinationProduct,
            entityType,
            confidence,
            source = new
            {
                file.Id,
                file.FileName,
                file.ContentType,
                file.SizeBytes,
                file.Sha256,
                file.RecordArrRecordId,
                file.RecordArrFileId
            },
            proposedFields = new Dictionary<string, object?>
            {
                ["displayName"] = Path.GetFileNameWithoutExtension(file.FileName),
                ["sourceDocumentRef"] = file.RecordArrRecordId,
                ["sourceFileRef"] = file.RecordArrFileId
            }
        }, JsonOptions);

    private static string BuildCsvRowPayload(
        ImportFile file,
        CsvImportRow row,
        string destinationProduct,
        string entityType,
        decimal confidence)
    {
        var proposedFields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["rowNumber"] = row.RowNumber
        };

        foreach (var field in row.Fields)
        {
            proposedFields[field.Key] = field.Value;
        }

        if (destinationProduct.Equals("maintainarr", StringComparison.OrdinalIgnoreCase)
            && entityType.Contains("asset", StringComparison.OrdinalIgnoreCase))
        {
            AddMaintainArrAssetCsvProposedFields(proposedFields, row.Fields);
        }

        if (!proposedFields.ContainsKey("displayName"))
        {
            var displayName = ResolveCsvDisplayName(row.Fields);
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                proposedFields["displayName"] = displayName;
            }
        }

        return JsonSerializer.Serialize(new
        {
            destinationProduct,
            entityType,
            confidence,
            source = new
            {
                file.Id,
                file.FileName,
                file.ContentType,
                file.SizeBytes,
                file.Sha256,
                file.RecordArrRecordId,
                file.RecordArrFileId,
                row.RowNumber
            },
            sourceFields = row.Fields,
            proposedFields
        }, JsonOptions);
    }

    private static void AddMaintainArrAssetCsvProposedFields(
        IDictionary<string, object?> proposedFields,
        IReadOnlyDictionary<string, string> fields)
    {
        var assetTag = FindCsvFieldValue(
            fields,
            "assetTag",
            "assetNumber",
            "unitNumber",
            "unit",
            "fleetAsset",
            "fleetAssetNumber",
            "fleetAssetId",
            "equipmentId",
            "equipmentNumber",
            "vinSerial",
            "vin",
            "serialNumber");
        AddCanonicalField(proposedFields, "assetTag", assetTag);
        AddCanonicalField(proposedFields, "unitNumber", assetTag);
        AddCanonicalField(proposedFields, "assetNumber", assetTag);

        var displayName = ResolveMaintainArrAssetDisplayName(fields);
        AddCanonicalField(proposedFields, "displayName", displayName);
        AddCanonicalField(proposedFields, "name", displayName);

        var assetClass = FindCsvFieldValue(fields, "assetClass", "assetClassKey", "class", "category");
        AddCanonicalField(proposedFields, "assetClass", assetClass);
        AddCanonicalField(proposedFields, "assetClassName", assetClass);

        var assetType = FindCsvFieldValue(
            fields,
            "assetType",
            "assetTypeKey",
            "subType",
            "sub-class",
            "subClass",
            "type",
            "category");
        AddCanonicalField(proposedFields, "assetType", assetType);
        AddCanonicalField(proposedFields, "assetTypeName", assetType);

        AddCanonicalField(proposedFields, "description", FindCsvFieldValue(fields, "description", "assetDescription"));
        AddCanonicalField(proposedFields, "lifecycleStatus", FindCsvFieldValue(fields, "lifecycleStatus", "status"));
        AddCanonicalField(proposedFields, "siteRef", FindCsvFieldValue(fields, "siteRef", "location", "maintDivLoc", "expenseDivLoc"));
        AddCanonicalField(proposedFields, "siteName", FindCsvFieldValue(fields, "siteName", "locationName", "organization"));
        AddCanonicalField(proposedFields, "vin", FindCsvFieldValue(fields, "vin", "vinSerial", "vinSerialNumber", "VIN/Serial #"));
        AddCanonicalField(proposedFields, "serialNumber", FindCsvFieldValue(fields, "serialNumber", "vinSerial", "vinSerialNumber", "VIN/Serial #"));
        AddCanonicalField(proposedFields, "licensePlate", FindCsvFieldValue(fields, "licensePlate", "licenseNumber", "license #"));
        AddCanonicalField(proposedFields, "modelYear", FindCsvFieldValue(fields, "modelYear"));
        AddCanonicalField(proposedFields, "manufacturer", FindCsvFieldValue(fields, "manufacturer", "bodyManufacturer", "chassisManufacturer"));
        AddCanonicalField(proposedFields, "model", FindCsvFieldValue(fields, "model", "bodyModel", "chassisModel", "engineModel"));
        AddCanonicalField(proposedFields, "fuelType", FindCsvFieldValue(fields, "fuelType", "primaryFuel", "engineFuelType"));
        AddCanonicalField(proposedFields, "meterUnit", FindCsvFieldValue(fields, "meterUnit"));
        AddCanonicalField(proposedFields, "inServiceDate", FindCsvFieldValue(fields, "inServiceDate"));
    }

    private static string? ResolveMaintainArrAssetDisplayName(IReadOnlyDictionary<string, string> fields)
    {
        var description = FindCsvFieldValue(fields, "displayName", "name", "assetName", "description");
        if (!string.IsNullOrWhiteSpace(description))
        {
            return description;
        }

        var manufacturer = FindCsvFieldValue(fields, "bodyManufacturer", "chassisManufacturer", "manufacturer", "make");
        var model = FindCsvFieldValue(fields, "bodyModel", "chassisModel", "model", "engineModel");
        var composed = string.Join(" ", new[] { manufacturer, model }.Where(value => !string.IsNullOrWhiteSpace(value)));
        if (!string.IsNullOrWhiteSpace(composed))
        {
            return composed;
        }

        return FindCsvFieldValue(fields, "fleetAsset", "assetTag", "assetNumber", "unitNumber", "vinSerial", "serialNumber");
    }

    private static void AddCanonicalField(IDictionary<string, object?> proposedFields, string fieldKey, string? value)
    {
        if (proposedFields.ContainsKey(fieldKey) || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        proposedFields[fieldKey] = value.Trim();
    }

    private static string? FindCsvFieldValue(IReadOnlyDictionary<string, string> fields, params string[] aliases)
    {
        var normalizedAliases = aliases
            .Select(NormalizeColumnKey)
            .Where(alias => alias.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (normalizedAliases.Count == 0)
        {
            return null;
        }

        foreach (var field in fields)
        {
            if (normalizedAliases.Contains(NormalizeColumnKey(field.Key))
                && !string.IsNullOrWhiteSpace(field.Value))
            {
                return field.Value.Trim();
            }
        }

        return null;
    }

    private static IReadOnlyList<CsvImportRow> ParseCsvImportRows(byte[] bytes, string fileName, string contentType)
    {
        if (bytes.Length == 0)
        {
            return [];
        }

        var text = Encoding.UTF8.GetString(bytes);
        if (text.Length > 0 && text[0] == '\uFEFF')
        {
            text = text[1..];
        }

        var delimiter = ResolveDelimitedFileDelimiter(text, fileName, contentType);
        if (delimiter is null)
        {
            return [];
        }

        var parsedRows = ParseDelimitedRows(text, delimiter.Value);
        if (parsedRows.Count < 2)
        {
            return [];
        }

        var headers = BuildCsvHeaders(parsedRows[0]);
        var rows = new List<CsvImportRow>(parsedRows.Count - 1);
        for (var index = 1; index < parsedRows.Count; index++)
        {
            var values = parsedRows[index];
            if (values.All(string.IsNullOrWhiteSpace))
            {
                continue;
            }

            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var column = 0; column < Math.Max(headers.Count, values.Count); column++)
            {
                var value = column < values.Count ? values[column].Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var header = column < headers.Count ? headers[column] : $"column_{column + 1}";
                fields[header] = value;
            }

            if (fields.Count > 0)
            {
                rows.Add(new CsvImportRow(index + 1, fields));
            }
        }

        return rows;
    }

    private static char? ResolveDelimitedFileDelimiter(string text, string fileName, string contentType)
    {
        if (fileName.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".tab", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("tab-separated-values", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("tsv", StringComparison.OrdinalIgnoreCase))
        {
            return '\t';
        }

        if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("csv", StringComparison.OrdinalIgnoreCase))
        {
            return ',';
        }

        if (!fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
            && !contentType.Contains("text/plain", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var headerLine = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return null;
        }

        var candidates = new[] { ',', '\t', ';', '|' };
        var best = candidates
            .Select(delimiter => new
            {
                delimiter,
                count = CountDelimiterOutsideQuotes(headerLine, delimiter)
            })
            .OrderByDescending(x => x.count)
            .First();

        return best.count > 0 ? best.delimiter : null;
    }

    private static int CountDelimiterOutsideQuotes(string line, char delimiter)
    {
        var count = 0;
        var inQuotes = false;
        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    index++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (!inQuotes && character == delimiter)
            {
                count++;
            }
        }

        return count;
    }

    private static IReadOnlyList<string> BuildCsvHeaders(IReadOnlyList<string> rawHeaders)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var headers = new List<string>(rawHeaders.Count);
        for (var index = 0; index < rawHeaders.Count; index++)
        {
            var header = rawHeaders[index].Trim();
            if (string.IsNullOrWhiteSpace(header))
            {
                header = $"column_{index + 1}";
            }

            if (!counts.TryAdd(header, 1))
            {
                counts[header]++;
                header = $"{header}_{counts[header]}";
            }

            headers.Add(header);
        }

        return headers;
    }

    private static IReadOnlyList<IReadOnlyList<string>> ParseDelimitedRows(string text, char delimiter)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => ParseDelimitedRow(line, delimiter))
            .ToList();
    }

    private static IReadOnlyList<string> ParseDelimitedRow(string line, char delimiter)
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

            if (character == delimiter)
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

    private static string? ResolveCsvDisplayName(IReadOnlyDictionary<string, string> fields)
    {
        var preferredKeys = new[]
        {
            "displayname",
            "name",
            "assetname",
            "assettag",
            "assetid",
            "assetnumber",
            "unit",
            "unitnumber",
            "equipmentid",
            "vin",
            "serialnumber"
        };
        foreach (var preferredKey in preferredKeys)
        {
            var match = fields.FirstOrDefault(field => NormalizeColumnKey(field.Key) == preferredKey);
            if (!string.IsNullOrWhiteSpace(match.Value))
            {
                return match.Value;
            }
        }

        return fields.Values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string NormalizeColumnKey(string value) =>
        new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    private static string ExtractAiProposedPayload(JsonElement root, ImportFile file, string destinationProduct, string entityType, decimal confidence)
    {
        if (root.TryGetProperty("proposedRecords", out var records)
            && records.ValueKind == JsonValueKind.Array
            && records.GetArrayLength() > 0)
        {
            return records[0].GetRawText();
        }

        return BuildDeterministicPayload(file, destinationProduct, entityType, confidence);
    }

    private static string ReadString(JsonElement root, string propertyName, string fallback) =>
        root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? NormalizeProduct(value.GetString())
            : fallback;

    private static decimal ReadDecimal(JsonElement root, string propertyName, decimal fallback)
    {
        if (!root.TryGetProperty(propertyName, out var value))
        {
            return fallback;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var parsed) => Math.Clamp(parsed, 0m, 100m),
            _ => fallback
        };
    }

    private static IReadOnlyList<string> DeserializeStringArray(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<string>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static IReadOnlyList<SmartImportManualFieldMapping> NormalizeManualFieldMappings(
        IReadOnlyList<SmartImportManualFieldMapping>? fieldMappings)
    {
        if (fieldMappings is null || fieldMappings.Count == 0)
        {
            throw new StlApiException(
                "smart_import.mapping_override_required",
                "At least one manual mapping override is required.",
                400);
        }

        var mappings = new List<SmartImportManualFieldMapping>();
        var seenTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in fieldMappings)
        {
            var sourceField = mapping.SourceField?.Trim() ?? string.Empty;
            var targetField = mapping.TargetField?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sourceField) || string.IsNullOrWhiteSpace(targetField))
            {
                continue;
            }

            if (sourceField.Length > 256 || targetField.Length > 128)
            {
                throw new StlApiException(
                    "smart_import.mapping_override_too_long",
                    "Manual mapping source and target field names are too long.",
                    400);
            }

            if (!targetField.All(character => char.IsLetterOrDigit(character) || character is '_' or '-'))
            {
                throw new StlApiException(
                    "smart_import.invalid_mapping_target",
                    "Manual mapping target fields may only contain letters, numbers, underscores, or hyphens.",
                    400);
            }

            if (!seenTargets.Add(targetField))
            {
                continue;
            }

            mappings.Add(new SmartImportManualFieldMapping(sourceField, targetField));
        }

        if (mappings.Count == 0)
        {
            throw new StlApiException(
                "smart_import.mapping_override_required",
                "At least one manual mapping override is required.",
                400);
        }

        return mappings;
    }

    private static bool TryApplyManualFieldMappings(
        ImportProposedRecord proposed,
        IReadOnlyList<SmartImportManualFieldMapping> mappings,
        DateTimeOffset now)
    {
        if (JsonNode.Parse(proposed.ProposedPayloadJson) is not JsonObject root)
        {
            return false;
        }

        if (root["proposedFields"] is not JsonObject proposedFields)
        {
            proposedFields = new JsonObject();
            root["proposedFields"] = proposedFields;
        }

        var sourceFields = root["sourceFields"] as JsonObject ?? proposedFields;
        var appliedMappings = new List<object>();
        foreach (var mapping in mappings)
        {
            if (!TryReadSourceFieldValue(sourceFields, mapping.SourceField, out var sourceValue))
            {
                continue;
            }

            proposedFields[mapping.TargetField] = JsonValue.Create(sourceValue);
            appliedMappings.Add(new
            {
                mapping.SourceField,
                mapping.TargetField
            });
        }

        if (appliedMappings.Count == 0)
        {
            return false;
        }

        root["manualMappingOverride"] = JsonSerializer.SerializeToNode(new
        {
            appliedAt = now,
            mappings = appliedMappings
        }, JsonOptions);

        proposed.ProposedPayloadJson = root.ToJsonString(JsonOptions);
        proposed.ReviewStatus = "review_required";
        proposed.RequiresReview = true;
        proposed.ReviewReasonsJson = JsonSerializer.Serialize(
            DeserializeStringArray(proposed.ReviewReasonsJson)
                .Append("manual_mapping_override")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            JsonOptions);
        proposed.UpdatedAt = now;
        return true;
    }

    private static bool TryReadSourceFieldValue(JsonObject sourceFields, string sourceField, out string value)
    {
        if (sourceFields.TryGetPropertyValue(sourceField, out var exact)
            && TryReadScalarSourceValue(exact, out value))
        {
            return true;
        }

        var normalizedSourceField = NormalizeColumnKey(sourceField);
        foreach (var field in sourceFields)
        {
            if (NormalizeColumnKey(field.Key) == normalizedSourceField
                && TryReadScalarSourceValue(field.Value, out value))
            {
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static bool TryReadScalarSourceValue(JsonNode? node, out string value)
    {
        value = string.Empty;
        if (node is null)
        {
            return false;
        }

        if (node is JsonValue jsonValue)
        {
            if (jsonValue.TryGetValue<string>(out var stringValue))
            {
                value = stringValue.Trim();
            }
            else if (jsonValue.TryGetValue<int>(out var intValue))
            {
                value = intValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (jsonValue.TryGetValue<long>(out var longValue))
            {
                value = longValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (jsonValue.TryGetValue<decimal>(out var decimalValue))
            {
                value = decimalValue.ToString(CultureInfo.InvariantCulture);
            }
            else if (jsonValue.TryGetValue<bool>(out var boolValue))
            {
                value = boolValue ? "true" : "false";
            }
        }

        return !string.IsNullOrWhiteSpace(value);
    }

    private static string NormalizeDecision(string decision) =>
        decision.Trim().ToLowerInvariant() switch
        {
            "approved" or "approve" => "approved",
            "rejected" or "reject" => "rejected",
            "needs_changes" or "needs-changes" => "needs_changes",
            _ => throw new StlApiException("smart_import.invalid_review_decision", "Review decision must be approved, rejected, or needs_changes.", 400)
        };

    private static void RequireImportAccess(ClaimsPrincipal principal, Guid tenantId, string destinationProduct)
    {
        if (!principal.IsPlatformAdmin() && principal.GetTenantId() != tenantId)
        {
            throw new StlApiException("auth.tenant_forbidden", "Access to the requested tenant is forbidden.", 403);
        }

        if (destinationProduct != "unknown"
            && !principal.IsPlatformAdmin()
            && !principal.HasProductEntitlement(destinationProduct)
            && !principal.HasProductEntitlement("nexarr"))
        {
            throw new StlApiException("auth.forbidden", "Destination product entitlement is required for this import.", 403);
        }
    }

    private const string SmartImportSchema = """
    {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "destinationProduct": { "type": "string" },
        "entityType": { "type": "string" },
        "confidence": { "type": "number" },
        "notes": { "type": "string" },
        "proposedRecords": {
          "type": "array",
          "items": {
            "type": "object",
            "additionalProperties": true
          }
        }
      },
      "required": ["destinationProduct", "entityType", "confidence", "notes", "proposedRecords"]
    }
    """;

    private sealed record CsvImportRow(int RowNumber, IReadOnlyDictionary<string, string> Fields);
}
