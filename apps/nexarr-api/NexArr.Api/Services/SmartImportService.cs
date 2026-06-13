using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
                ? $"Source file was retained in RecordArr and {csvRows.Count} CSV rows were staged for Smart Import review."
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
        if (plan.Status != "approved")
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

        foreach (var step in steps.Where(x => x.Status is "pending" or "failed"))
        {
            step.Status = "failed";
            step.ErrorCode = "destination_commit_adapter_not_implemented";
            step.ErrorMessage = "Final Smart Import writes must be implemented by the owning product adapter before this step can commit.";
            step.Retryable = false;
            step.CompletedAt = DateTimeOffset.UtcNow;
        }

        plan.Status = "failed";
        batch.Status = SmartImportStatuses.Failed;
        batch.ErrorCode = "destination_commit_adapter_not_implemented";
        batch.ErrorMessage = "No domain-specific destination commit adapter completed a write.";
        db.ImportAuditEvents.Add(ImportAudit(batch.Id, tenantId, actorPersonId, "smart_import.commit_blocked", "failed", batch.ErrorCode, new
        {
            plan.Id,
            reason = batch.ErrorMessage
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
            Notes = $"{rows.Count} CSV rows were parsed into row-level import candidates for human review.",
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
            proposedFields
        }, JsonOptions);
    }

    private static IReadOnlyList<CsvImportRow> ParseCsvImportRows(byte[] bytes, string fileName, string contentType)
    {
        if (!IsCsvFile(fileName, contentType) || bytes.Length == 0)
        {
            return [];
        }

        var csv = Encoding.UTF8.GetString(bytes);
        if (csv.Length > 0 && csv[0] == '\uFEFF')
        {
            csv = csv[1..];
        }

        var parsedRows = ParseCsvRows(csv);
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

    private static bool IsCsvFile(string fileName, string contentType) =>
        fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
        || contentType.Contains("csv", StringComparison.OrdinalIgnoreCase);

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
