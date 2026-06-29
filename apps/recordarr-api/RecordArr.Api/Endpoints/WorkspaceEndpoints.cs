using RecordArr.Api.Data;
using STLCompliance.Shared.Auth;
using RecordArr.Api.Services;

namespace RecordArr.Api.Endpoints;

public static class WorkspaceEndpoints
{
    private const long MaxInlineFileUploadBytes = 25L * 1024 * 1024;
    private const int MaxInlineFileUploadBase64Chars = (int)(((MaxInlineFileUploadBytes + 2) / 3) * 4);

    public static void MapRecordArrWorkspaceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/workspace").WithTags("Workspace").RequireAuthorization();

        group.MapGet("/summary", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetDashboard(context.User)))
            .WithName("GetRecordArrWorkspaceSummary");

        group.MapGet("/reminders", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetReminders(context.User)))
            .WithName("ListRecordArrReminders");

        group.MapGet("/records", (HttpContext context, string? search, RecordArrStore store) => Results.Ok(store.GetRecords(context.User, search)))
            .WithName("ListRecordArrRecords");

        group.MapGet("/records/{recordId}", (HttpContext context, string recordId, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            return record is null ? Results.NotFound() : Results.Ok(record);
        }).WithName("GetRecordArrRecord");

        group.MapGet("/records/{recordId}/metadata", (HttpContext context, string recordId, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            return record is null ? Results.NotFound() : Results.Ok(store.GetRecordMetadata(recordId));
        })
            .WithName("ListRecordArrRecordMetadata");

        group.MapPost("/records/{recordId}/metadata", (HttpContext context, string recordId, CreateRecordMetadataRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            var metadata = store.CreateRecordMetadata(recordId, request.Key, request.Value, request.ValueType, request.Source, request.ConfidenceScore, GetActorPersonId(context));
            return Results.Created($"/api/v1/workspace/records/{recordId}/metadata/{metadata.MetadataId}", metadata);
        }).WithName("CreateRecordArrRecordMetadata");

        group.MapGet("/records/{recordId}/links", (HttpContext context, string recordId, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            return record is null ? Results.NotFound() : Results.Ok(store.GetRecordLinks(recordId));
        }).WithName("ListRecordArrRecordLinks");

        group.MapPost("/records/{recordId}/links", (HttpContext context, string recordId, CreateRecordLinkRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(request.LinkedRecordId) && store.GetRecord(context.User, request.LinkedRecordId) is null)
            {
                return Results.NotFound();
            }

            var link = store.CreateRecordLink(recordId, request.LinkedRecordId, request.SourceObjectRef, request.LinkType, GetActorPersonId(context));
            return Results.Created($"/api/v1/workspace/records/{recordId}/links/{link.RecordLinkId}", link);
        }).WithName("CreateRecordArrRecordLink");

        group.MapGet("/records/{recordId}/comments", (HttpContext context, string recordId, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            return record is null ? Results.NotFound() : Results.Ok(store.GetRecordComments(recordId));
        }).WithName("ListRecordArrRecordComments");

        group.MapPost("/records/{recordId}/comments", (HttpContext context, string recordId, CreateRecordCommentRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            var comment = store.CreateRecordComment(recordId, request.Body, request.Visibility, GetActorPersonId(context));
            return Results.Created($"/api/v1/workspace/records/{recordId}/comments/{comment.CommentId}", comment);
        }).WithName("CreateRecordArrRecordComment");

        group.MapPatch("/records/{recordId}/comments/{commentId}", (HttpContext context, string recordId, string commentId, UpdateRecordCommentRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            var comment = store.UpdateRecordComment(commentId, request.Body, request.Visibility, GetActorPersonId(context));
            return Results.Ok(comment);
        }).WithName("UpdateRecordArrRecordComment");

        group.MapPost("/records", async (HttpContext context, CreateRecordRequest request, RecordArrStore store, RecordArrDocumentStorageService storage) =>
        {
            if (string.IsNullOrWhiteSpace(request.SourceProduct))
            {
                return Results.BadRequest(new { code = "missing_source_product", message = "Record creation requires a source product." });
            }
            if (string.IsNullOrWhiteSpace(request.SourceObjectType) || string.IsNullOrWhiteSpace(request.SourceObjectId) || string.IsNullOrWhiteSpace(request.SourceObjectDisplayName))
            {
                return Results.BadRequest(new { code = "missing_primary_target", message = "Record creation requires a primary target reference." });
            }
            if (string.IsNullOrWhiteSpace(request.DocumentClass) || string.IsNullOrWhiteSpace(request.DocumentType) || string.IsNullOrWhiteSpace(request.DocumentSubtype))
            {
                return Results.BadRequest(new { code = "missing_document_classification", message = "Record creation requires document class, type, and subtype." });
            }

            string? storageProvider = null;
            string? storageKey = null;
            long? sizeBytes = null;

            if (!string.IsNullOrWhiteSpace(request.FileContentBase64))
            {
                if (string.IsNullOrWhiteSpace(request.CurrentFileName))
                {
                    return Results.BadRequest(new { code = "missing_file_name", message = "CurrentFileName is required when uploading file content." });
                }

                if (request.FileContentBase64.Length > MaxInlineFileUploadBase64Chars)
                {
                    return Results.BadRequest(new { code = "file_too_large", message = $"FileContentBase64 exceeds the maximum inline size of {MaxInlineFileUploadBytes} bytes." });
                }

                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(request.FileContentBase64);
                }
                catch (FormatException)
                {
                    return Results.BadRequest(new { code = "invalid_file_content", message = "FileContentBase64 must be valid base64." });
                }

                if (fileBytes.LongLength > MaxInlineFileUploadBytes)
                {
                    return Results.BadRequest(new { code = "file_too_large", message = $"FileContentBase64 exceeds the maximum inline size of {MaxInlineFileUploadBytes} bytes." });
                }

                await using var fileStream = new MemoryStream(fileBytes, writable: false);
                storageKey = await storage.SaveAsync(
                    context.User.GetTenantId(),
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString("N"),
                    request.CurrentFileName,
                    fileStream);
                storageProvider = "recordarr";
                sizeBytes = fileBytes.LongLength;
            }

            var record = store.CreateRecord(
                context.User.GetTenantId().ToString(),
                request.Title,
                request.Description,
                request.RecordType,
                request.DocumentClass,
                request.DocumentType,
                request.DocumentSubtype,
                request.Classification,
                request.SourceProduct,
                request.SourceObjectType,
                request.SourceObjectId,
                request.SourceObjectDisplayName,
                request.OwnerPersonId,
                GetActorPersonId(context),
                request.CurrentFileName,
                request.CurrentMimeType,
                storageProvider,
                storageKey,
                sizeBytes);
            return Results.Created($"/api/v1/workspace/records/{record.RecordId}", record);
        }).WithName("CreateRecordArrRecord");

        group.MapPatch("/records/{recordId}", (string recordId, UpdateRecordRequest request, RecordArrStore store) =>
        {
            var updated = store.UpdateRecordStatus(recordId, request.Status, request.Classification, request.EffectiveAt, request.ExpiresAt);
            return Results.Ok(updated);
        }).WithName("UpdateRecordArrRecord");

        group.MapPost("/records/{recordId}/archive", (HttpContext context, string recordId, DisposeRecordRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            var updated = store.ArchiveRecord(recordId, GetActorPersonId(context));
            return Results.Ok(updated);
        }).WithName("ArchiveRecordArrRecord");

        group.MapPost("/records/{recordId}/purge", (HttpContext context, string recordId, DisposeRecordRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            var updated = store.PurgeRecord(recordId, GetActorPersonId(context));
            return Results.Ok(updated);
        }).WithName("PurgeRecordArrRecord");

        group.MapGet("/access-logs", (HttpContext context, string? recordId, RecordArrStore store) => Results.Ok(store.GetAccessLogs(context.User.GetTenantId().ToString(), recordId)))
            .WithName("ListRecordArrAccessLogs");

        group.MapGet("/access-integrity", (HttpContext context, string? recordId, RecordArrStore store) =>
            Results.Ok(store.VerifyAccessHistoryIntegrity(context.User.GetTenantId().ToString(), recordId)))
            .WithName("VerifyRecordArrAccessHistoryIntegrity");

        group.MapGet("/access-history-seals", (HttpContext context, string? recordId, RecordArrStore store) =>
            Results.Ok(store.GetAccessHistorySeals(context.User.GetTenantId().ToString(), recordId)))
            .WithName("ListRecordArrAccessHistorySeals");

        group.MapPost("/access-history-seals", (HttpContext context, CreateAccessHistorySealRequest request, RecordArrStore store) =>
        {
            var seal = store.SealAccessHistory(context.User.GetTenantId().ToString(), request.RecordId, GetActorPersonId(context));
            return Results.Created($"/api/v1/workspace/access-history-seals/{seal.AccessHistorySealId}", seal);
        }).WithName("CreateRecordArrAccessHistorySeal");

        group.MapPost("/access-history-seals/{accessHistorySealId}/verify", (HttpContext context, string accessHistorySealId, RecordArrStore store) =>
            Results.Ok(store.VerifyAccessHistorySeal(context.User.GetTenantId().ToString(), accessHistorySealId)))
            .WithName("VerifyRecordArrAccessHistorySeal");

        group.MapGet("/audit-events", (HttpContext context, string? recordId, RecordArrStore store) => Results.Ok(store.GetAuditEvents(context.User.GetTenantId().ToString(), recordId)))
            .WithName("ListRecordArrAuditEvents");

        group.MapGet("/audit-integrity", (HttpContext context, string? recordId, RecordArrStore store) =>
            Results.Ok(store.VerifyAuditIntegrity(context.User.GetTenantId().ToString(), recordId)))
            .WithName("VerifyRecordArrAuditIntegrity");

        group.MapGet("/audit-governance", (HttpContext context, string? recordId, RecordArrStore store) =>
            Results.Ok(store.VerifyAuditGovernance(context.User.GetTenantId().ToString(), recordId)))
            .WithName("VerifyRecordArrAuditGovernance");

        group.MapGet("/audit-seals", (HttpContext context, string? recordId, RecordArrStore store) =>
            Results.Ok(store.GetAuditSeals(context.User.GetTenantId().ToString(), recordId)))
            .WithName("ListRecordArrAuditSeals");

        group.MapPost("/audit-seals", (HttpContext context, CreateAuditSealRequest request, RecordArrStore store) =>
        {
            var seal = store.SealAuditEvents(context.User.GetTenantId().ToString(), request.RecordId, GetActorPersonId(context));
            return Results.Created($"/api/v1/workspace/audit-seals/{seal.AuditSealId}", seal);
        }).WithName("CreateRecordArrAuditSeal");

        group.MapPost("/audit-seals/{auditSealId}/verify", (HttpContext context, string auditSealId, RecordArrStore store) =>
            Results.Ok(store.VerifyAuditSeal(context.User.GetTenantId().ToString(), auditSealId)))
            .WithName("VerifyRecordArrAuditSeal");

        group.MapPost("/audit-seals/{auditSealId}/anchor", (HttpContext context, string auditSealId, AnchorAuditSealRequest request, RecordArrStore store) =>
            Results.Ok(store.AnchorAuditSeal(
                context.User.GetTenantId().ToString(),
                auditSealId,
                GetActorPersonId(context),
                request.AnchorProviderName,
                request.AnchorReference,
                request.AnchoredAt,
                request.AnchoredSealHash)))
            .WithName("AnchorRecordArrAuditSeal");

        group.MapGet("/capture-requests", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetCaptureRequests(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrCaptureRequests");

        group.MapPost("/capture-requests", (HttpContext context, CreateCaptureRequestRequest request, RecordArrStore store) =>
        {
            var captureRequest = store.CreateCaptureRequest(
                context.User.GetTenantId().ToString(),
                request.SourceProduct,
                request.SourceObjectRef,
                request.CaptureType,
                request.Title,
                request.Instructions,
                request.Required,
                request.UploadSessionRef,
                request.EvidenceRequirementRef);
            return Results.Created($"/api/v1/workspace/capture-requests/{captureRequest.CaptureRequestId}", captureRequest);
        }).WithName("CreateRecordArrCaptureRequest");

        group.MapPost("/capture-requests/{captureRequestId}/complete", (HttpContext context, string captureRequestId, RecordArrStore store) =>
        {
            var captureRequest = store.CompleteCaptureRequest(context.User.GetTenantId().ToString(), captureRequestId);
            return Results.Ok(captureRequest);
        }).WithName("CompleteRecordArrCaptureRequest");

        group.MapPost("/capture-requests/{captureRequestId}/skip", (HttpContext context, string captureRequestId, RecordArrStore store) =>
        {
            var captureRequest = store.SkipCaptureRequest(context.User.GetTenantId().ToString(), captureRequestId);
            return Results.Ok(captureRequest);
        }).WithName("SkipRecordArrCaptureRequest");

        group.MapPost("/capture-requests/{captureRequestId}/cancel", (HttpContext context, string captureRequestId, RecordArrStore store) =>
        {
            var captureRequest = store.CancelCaptureRequest(context.User.GetTenantId().ToString(), captureRequestId);
            return Results.Ok(captureRequest);
        }).WithName("CancelRecordArrCaptureRequest");

        group.MapPost("/capture-requests/{captureRequestId}/expire", (HttpContext context, string captureRequestId, RecordArrStore store) =>
        {
            var captureRequest = store.ExpireCaptureRequest(context.User.GetTenantId().ToString(), captureRequestId);
            return Results.Ok(captureRequest);
        }).WithName("ExpireRecordArrCaptureRequest");

        group.MapPost("/files", (HttpContext context, CreateFileRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, request.RecordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            var file = store.CreateFile(
                request.RecordId,
                request.OriginalFilename,
                request.MimeType,
                GetActorPersonId(context),
                request.StorageProvider,
                request.StorageKey,
                request.SizeBytes,
                request.PageCount,
                request.ImageWidth,
                request.ImageHeight,
                request.DurationSeconds);
            return Results.Created($"/api/v1/workspace/files/{file.FileId}", file);
        }).WithName("CreateRecordArrFile");

        group.MapGet("/file-malware-scans", (HttpContext context, string? fileId, string? recordId, RecordArrStore store) =>
            Results.Ok(store.GetFileMalwareScans(context.User.GetTenantId().ToString(), fileId, recordId)))
            .WithName("ListRecordArrFileMalwareScans");

        group.MapPost("/files/{fileId}/malware-scans", (HttpContext context, string fileId, CreateFileMalwareScanRequest request, RecordArrStore store) =>
        {
            var scan = store.CreateFileMalwareScan(
                context.User.GetTenantId().ToString(),
                fileId,
                GetActorPersonId(context),
                request.Status,
                request.ScannerName,
                request.ScannerVersion,
                request.SignatureVersion,
                request.ThreatName,
                request.FailureReason);
            return Results.Created($"/api/v1/workspace/file-malware-scans/{scan.MalwareScanId}", scan);
        }).WithName("CreateRecordArrFileMalwareScan");

        group.MapPost("/file-malware-scan-runs", (HttpContext context, RunFileMalwareScanProviderRequest request, RecordArrStore store) =>
            Results.Ok(store.RunFileMalwareScanProvider(
                context.User.GetTenantId().ToString(),
                GetActorPersonId(context),
                request.ScannerName,
                request.ScannerVersion,
                request.SignatureVersion,
                request.InfectedFileIds ?? Array.Empty<string>(),
                request.FailedFileIds ?? Array.Empty<string>(),
                request.SkippedFileIds ?? Array.Empty<string>())))
            .WithName("RunRecordArrFileMalwareScanProvider");

        group.MapPost("/file-malware-scan-dead-letters", (HttpContext context, DeadLetterFileMalwareScansRequest request, RecordArrStore store) =>
            Results.Ok(store.DeadLetterFailedMalwareScans(
                context.User.GetTenantId().ToString(),
                GetActorPersonId(context),
                request.MaxFiles ?? 100)))
            .WithName("DeadLetterRecordArrFileMalwareScans");

        group.MapGet("/file-integrity-checks", (HttpContext context, string? fileId, string? recordId, RecordArrStore store) =>
            Results.Ok(store.GetFileIntegrityChecks(context.User.GetTenantId().ToString(), fileId, recordId)))
            .WithName("ListRecordArrFileIntegrityChecks");

        group.MapPost("/files/{fileId}/integrity-checks", (HttpContext context, string fileId, CreateFileIntegrityCheckRequest request, RecordArrStore store) =>
        {
            var check = store.CreateFileIntegrityCheck(
                context.User.GetTenantId().ToString(),
                fileId,
                GetActorPersonId(context),
                request.ObservedChecksumSha256,
                request.CheckMethod);
            return Results.Created($"/api/v1/workspace/file-integrity-checks/{check.IntegrityCheckId}", check);
        }).WithName("CreateRecordArrFileIntegrityCheck");

        group.MapGet("/storage-reconciliations", (HttpContext context, string? status, RecordArrStore store) =>
            Results.Ok(store.GetStorageReconciliations(context.User.GetTenantId().ToString(), status)))
            .WithName("ListRecordArrStorageReconciliations");

        group.MapPost("/storage-reconciliations", (HttpContext context, RunStorageReconciliationRequest request, RecordArrStore store) =>
        {
            var reconciliation = store.RunStorageReconciliation(
                context.User.GetTenantId().ToString(),
                GetActorPersonId(context),
                request.Scope,
                request.RecordId,
                request.MissingFileIds,
                request.CorruptFileIds,
                request.CheckedFileIds);
            return Results.Created($"/api/v1/workspace/storage-reconciliations/{reconciliation.ReconciliationId}", reconciliation);
        }).WithName("RunRecordArrStorageReconciliation");

        group.MapPost("/storage-reconciliations/{reconciliationId}/remediation", (HttpContext context, string reconciliationId, RemediateStorageReconciliationRequest request, RecordArrStore store) =>
            Results.Ok(store.RemediateStorageReconciliation(
                context.User.GetTenantId().ToString(),
                reconciliationId,
                GetActorPersonId(context),
                request.RestoredFileIds ?? Array.Empty<string>(),
                request.AcceptedMissingFileIds ?? Array.Empty<string>(),
                request.RecheckedCorruptFileIds ?? Array.Empty<string>(),
                request.ReleasedQuarantinedFileIds ?? Array.Empty<string>(),
                request.ScannedPendingFileIds ?? Array.Empty<string>())))
            .WithName("RemediateRecordArrStorageReconciliation");

        group.MapGet("/object-store-objects", (HttpContext context, string? fileId, string? recordId, string? status, RecordArrStore store) =>
            Results.Ok(store.GetObjectStoreObjects(context.User.GetTenantId().ToString(), fileId, recordId, status)))
            .WithName("ListRecordArrObjectStoreObjects");

        group.MapGet("/object-store-fixity-observations", (HttpContext context, string? fileId, string? recordId, string? reconciliationId, string? status, RecordArrStore store) =>
            Results.Ok(store.GetObjectStoreFixityObservations(context.User.GetTenantId().ToString(), fileId, recordId, reconciliationId, status)))
            .WithName("ListRecordArrObjectStoreFixityObservations");

        group.MapPost("/files/{fileId}/object-store-lifecycle-verifications", (HttpContext context, string fileId, VerifyObjectStoreLifecycleRequest request, RecordArrStore store) =>
            Results.Ok(store.VerifyObjectStoreLifecycle(
                context.User.GetTenantId().ToString(),
                fileId,
                GetActorPersonId(context),
                request.ProviderName,
                request.PolicyRef,
                request.RetentionMode,
                request.RetainUntil,
                request.EncryptionKeyRef,
                request.EvidenceRef)))
            .WithName("VerifyRecordArrObjectStoreLifecycle");

        group.MapGet("/disaster-recovery-runs", (HttpContext context, string? status, RecordArrStore store) =>
            Results.Ok(store.GetDisasterRecoveryRuns(context.User.GetTenantId().ToString(), status)))
            .WithName("ListRecordArrDisasterRecoveryRuns");

        group.MapPost("/disaster-recovery-runs", (HttpContext context, RunDisasterRecoveryRestoreRequest request, RecordArrStore store) =>
        {
            var run = store.RunDisasterRecoveryRestore(
                context.User.GetTenantId().ToString(),
                GetActorPersonId(context),
                request.RecoveryPointId,
                request.RecoveryPointCreatedAt,
                request.RpoTargetMinutes,
                request.RtoTargetMinutes,
                request.RecordIds,
                request.MissingFileIds,
                request.CorruptFileIds);
            return Results.Created($"/api/v1/workspace/disaster-recovery-runs/{run.DisasterRecoveryRunId}", run);
        }).WithName("RunRecordArrDisasterRecoveryRestore");

        group.MapPost("/disaster-recovery-backup-verifications", (HttpContext context, RunDisasterRecoveryBackupVerificationRequest request, RecordArrStore store) =>
        {
            var run = store.RunDisasterRecoveryBackupVerification(
                context.User.GetTenantId().ToString(),
                GetActorPersonId(context),
                request.BackupProviderName,
                request.BackupJobRef,
                request.BackupManifestHash,
                request.RecoveryPointId,
                request.RecoveryPointCreatedAt,
                request.RpoTargetMinutes,
                request.RecordIds,
                request.MissingFileIds,
                request.CorruptFileIds);
            return Results.Created($"/api/v1/workspace/disaster-recovery-runs/{run.DisasterRecoveryRunId}", run);
        }).WithName("RunRecordArrDisasterRecoveryBackupVerification");

        group.MapPost("/upload-sessions", (HttpContext context, CreateUploadSessionRequest request, RecordArrStore store) =>
        {
            var session = store.CreateUploadSession(
                context.User.GetTenantId().ToString(),
                request.SourceProduct,
                request.SourceObjectType,
                request.SourceObjectId,
                request.UploadPurpose,
                request.RequiresDocumentScan,
                request.RequiresOcr,
                request.RequiresManualReview);
            return Results.Created($"/api/v1/workspace/upload-sessions/{session.UploadSessionId}", session);
        }).WithName("CreateRecordArrUploadSession");

        group.MapGet("/upload-sessions", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetUploadSessions(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrUploadSessions");

        group.MapGet("/upload-sessions/{uploadSessionId}", (HttpContext context, string uploadSessionId, RecordArrStore store) =>
        {
            var session = store.GetUploadSession(context.User.GetTenantId().ToString(), uploadSessionId);
            return session is null ? Results.NotFound() : Results.Ok(session);
        }).WithName("GetRecordArrUploadSession");

        group.MapPost("/upload-sessions/{uploadSessionId}/complete", (HttpContext context, string uploadSessionId, CompleteUploadSessionRequest request, RecordArrStore store) =>
        {
            var session = store.CompleteUploadSession(context.User.GetTenantId().ToString(), uploadSessionId, request.RecordId);
            return Results.Ok(session);
        }).WithName("CompleteRecordArrUploadSession");

        group.MapPost("/upload-sessions/{uploadSessionId}/revoke", (HttpContext context, string uploadSessionId, RevokeUploadSessionRequest request, RecordArrStore store) =>
        {
            var session = store.RevokeUploadSession(context.User.GetTenantId().ToString(), uploadSessionId, request.Reason);
            return Results.Ok(session);
        }).WithName("RevokeRecordArrUploadSession");

        group.MapPost("/document-scans", (CreateDocumentScanRequest request, RecordArrStore store) =>
        {
            var scan = store.CreateScanProcessing(request.RecordId, request.OriginalFileName, request.ScanPurpose);
            return Results.Created($"/api/v1/workspace/document-scans/{scan.ScanProcessingId}", scan);
        }).WithName("CreateRecordArrDocumentScan");

        group.MapGet("/document-scans", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetScanProcessing().Where(scan => store.GetRecord(context.User, scan.RecordId) is not null)))
            .WithName("ListRecordArrDocumentScans");

        group.MapGet("/document-scans/{scanProcessingId}", (HttpContext context, string scanProcessingId, RecordArrStore store) =>
        {
            var scan = store.GetScanProcessing(scanProcessingId);
            return scan is null || store.GetRecord(context.User, scan.RecordId) is null ? Results.NotFound() : Results.Ok(scan);
        }).WithName("GetRecordArrDocumentScan");

        group.MapPost("/document-scans/{scanProcessingId}/manual-correction", (HttpContext context, string scanProcessingId, ManualCorrectionRequest request, RecordArrStore store) =>
        {
            var existing = store.GetScanProcessing(scanProcessingId);
            if (existing is null || store.GetRecord(context.User, existing.RecordId) is null)
            {
                return Results.NotFound();
            }

            var scan = store.ApplyManualCorrection(scanProcessingId, request.EdgeCoordinates, GetActorPersonId(context));
            return Results.Ok(scan);
        }).WithName("ApplyRecordArrManualCorrection");

        group.MapGet("/ocr-results/{ocrResultId}", (HttpContext context, string ocrResultId, RecordArrStore store) =>
        {
            var result = store.GetOcrResult(ocrResultId);
            return result is null || store.GetRecord(context.User, result.RecordId) is null ? Results.NotFound() : Results.Ok(result);
        }).WithName("GetRecordArrOcrResult");

        group.MapGet("/extraction-results/{extractionResultId}", (HttpContext context, string extractionResultId, RecordArrStore store) =>
        {
            var result = store.GetExtractionResult(extractionResultId);
            return result is null || store.GetRecord(context.User, result.RecordId) is null ? Results.NotFound() : Results.Ok(result);
        }).WithName("GetRecordArrExtractionResult");

        group.MapPost("/extraction-results/{extractionResultId}/review", (HttpContext context, string extractionResultId, ReviewExtractionResultRequest request, RecordArrStore store) =>
        {
            var existing = store.GetExtractionResult(extractionResultId);
            if (existing is null || store.GetRecord(context.User, existing.RecordId) is null)
            {
                return Results.NotFound();
            }

            var result = store.ReviewExtractionResult(extractionResultId, GetActorPersonId(context), request.Status, request.FailureReason);
            return Results.Ok(result);
        }).WithName("ReviewRecordArrExtractionResult");

        group.MapGet("/evidence-mappings", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetEvidenceMappings(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrEvidenceMappings");

        group.MapPost("/evidence-mappings", (HttpContext context, CreateEvidenceMappingRequest request, RecordArrStore store) =>
        {
            if (store.GetRecord(context.User, request.RecordId) is null)
            {
                return Results.NotFound();
            }

            var mapping = store.CreateEvidenceMapping(
                request.RecordId,
                request.SourceProduct,
                request.SourceObjectType,
                request.SourceObjectId,
                request.ComplianceRequirementRef,
                request.EvidenceTypeKey,
                request.MappingSource,
                request.ConfidenceScore);
            return Results.Created($"/api/v1/workspace/evidence-mappings/{mapping.EvidenceMappingId}", mapping);
        }).WithName("CreateRecordArrEvidenceMapping");

        group.MapPost("/evidence-mappings/{mappingId}/confirm", (HttpContext context, string mappingId, ConfirmEvidenceMappingRequest request, RecordArrStore store) =>
        {
            var mapping = store.UpdateEvidenceMapping(context.User.GetTenantId().ToString(), mappingId, "confirmed", GetActorPersonId(context), request.Notes, null);
            return Results.Ok(mapping);
        }).WithName("ConfirmRecordArrEvidenceMapping");

        group.MapPost("/evidence-mappings/{mappingId}/reject", (HttpContext context, string mappingId, RejectEvidenceMappingRequest request, RecordArrStore store) =>
        {
            var mapping = store.UpdateEvidenceMapping(context.User.GetTenantId().ToString(), mappingId, "rejected", GetActorPersonId(context), request.Notes, request.RejectionReason);
            return Results.Ok(mapping);
        }).WithName("RejectRecordArrEvidenceMapping");

        group.MapGet("/evidence-coverage", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetEvidenceCoverage(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrEvidenceCoverage");

        group.MapPost("/record-packages", (HttpContext context, CreatePackageRequest request, RecordArrStore store) =>
        {
            var package = store.CreatePackage(context.User.GetTenantId().ToString(), request.Title, request.PackageType, request.SourceProduct, request.SourceObjectRef, request.RecordRef, GetActorPersonId(context));
            return Results.Created($"/api/v1/workspace/record-packages/{package.PackageId}", package);
        }).WithName("CreateRecordArrPackage");

        group.MapGet("/record-packages/{packageId}", (HttpContext context, string packageId, RecordArrStore store) =>
        {
            var package = store.GetPackage(context.User.GetTenantId().ToString(), packageId);
            return package is null ? Results.NotFound() : Results.Ok(package);
        }).WithName("GetRecordArrPackage");

        group.MapPost("/record-packages/{packageId}/lock", (HttpContext context, string packageId, RecordArrStore store) =>
        {
            var package = store.LockPackage(context.User.GetTenantId().ToString(), packageId);
            return Results.Ok(package);
        }).WithName("LockRecordArrPackage");

        group.MapPost("/record-packages/{packageId}/archive", (HttpContext context, string packageId, RecordArrStore store) =>
        {
            var package = store.ArchivePackage(context.User.GetTenantId().ToString(), packageId);
            return Results.Ok(package);
        }).WithName("ArchiveRecordArrPackage");

        group.MapGet("/record-packages/{packageId}/manifest", (HttpContext context, string packageId, RecordArrStore store) =>
        {
            var manifest = store.GetManifest(context.User.GetTenantId().ToString(), packageId);
            return manifest is null ? Results.NotFound() : Results.Ok(manifest);
        }).WithName("GetRecordArrPackageManifest");

        group.MapGet("/record-packages/{packageId}/download", (HttpContext context, string packageId, RecordArrStore store) =>
        {
            var tenantId = context.User.GetTenantId().ToString();
            var package = store.GetPackage(tenantId, packageId);
            if (package is null)
            {
                return Results.NotFound();
            }

            var manifest = store.GetManifest(tenantId, packageId);
            var lines = new List<string>
            {
                $"Package: {package.PackageNumber}",
                $"Title: {package.Title}",
                $"Type: {package.PackageType}",
                $"Status: {package.Status}",
                $"Source product: {package.SourceProduct}",
                $"Source objects: {string.Join(", ", package.SourceObjectRefs)}",
                $"Record refs: {string.Join(", ", package.RecordRefs)}",
                $"Manifest checksum: {package.ManifestChecksum ?? "n/a"}",
                $"Generated PDF ref: {package.GeneratedPdfRecordRef ?? "n/a"}",
                $"Generated ZIP ref: {package.GeneratedZipFileRef ?? "n/a"}",
                $"Created at: {package.CreatedAt:O}",
                $"Completed at: {package.CompletedAt:O}",
                $"Locked at: {package.LockedAt:O}",
                $"Archived at: {package.ArchivedAt:O}",
                $"Expires at: {package.ExpiresAt:O}",
            };

            if (manifest is not null)
            {
                lines.Add(string.Empty);
                lines.Add($"Manifest: {manifest.ManifestId}");
                lines.Add($"Manifest version: {manifest.ManifestVersion}");
                lines.Add($"Checksum: {manifest.Checksum}");
                lines.Add($"Generated at: {manifest.GeneratedAt:O}");
                lines.Add($"Generated by: {manifest.GeneratedByPersonId}");
                lines.Add(string.Empty);
                lines.Add("Records:");
                lines.AddRange(manifest.RecordEntries.Select(entry => $"- {entry.DisplayName} [{entry.EntryType}] :: {entry.StatusSnapshot ?? "n/a"}"));
                lines.Add("Source objects:");
                lines.AddRange(manifest.SourceObjectEntries.Select(entry => $"- {entry.DisplayName} [{entry.EntryType}] :: {entry.StatusSnapshot ?? "n/a"}"));
                lines.Add("Requirements:");
                lines.AddRange(manifest.RequirementEntries.Select(entry => $"- {entry.DisplayName} [{entry.EntryType}] :: {entry.StatusSnapshot ?? "n/a"}"));
            }

            return Results.Text(string.Join(Environment.NewLine, lines), "text/plain");
        }).WithName("DownloadRecordArrPackage");

        group.MapGet("/controlled-documents/{controlledDocumentId}/versions", (HttpContext context, string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentVersions(context.User.GetTenantId().ToString(), controlledDocumentId)))
            .WithName("ListRecordArrControlledDocumentVersions");

        group.MapPost("/controlled-documents/refresh-workflows", (HttpContext context, RecordArrStore store) =>
            Results.Ok(store.RefreshControlledDocumentWorkflows(context.User.GetTenantId().ToString())))
            .WithName("RefreshRecordArrControlledDocumentWorkflows");

        group.MapPost("/controlled-documents/{controlledDocumentId}/versions/{versionId}/promote", (HttpContext context, string controlledDocumentId, string versionId, PromoteControlledDocumentVersionRequest request, RecordArrStore store) =>
        {
            var version = store.PromoteDocumentVersion(context.User.GetTenantId().ToString(), controlledDocumentId, versionId, GetActorPersonId(context), request.EffectiveAt);
            return Results.Ok(version);
        }).WithName("PromoteRecordArrControlledDocumentVersion");

        group.MapPost("/controlled-documents/{controlledDocumentId}/archive", (HttpContext context, string controlledDocumentId, UpdateControlledDocumentStatusRequest request, RecordArrStore store) =>
        {
            var document = store.UpdateControlledDocumentStatus(context.User.GetTenantId().ToString(), controlledDocumentId, "archived", GetActorPersonId(context));
            return Results.Ok(document);
        }).WithName("ArchiveRecordArrControlledDocument");

        group.MapPost("/controlled-documents/{controlledDocumentId}/obsolete", (HttpContext context, string controlledDocumentId, UpdateControlledDocumentStatusRequest request, RecordArrStore store) =>
        {
            var document = store.UpdateControlledDocumentStatus(context.User.GetTenantId().ToString(), controlledDocumentId, "obsolete", GetActorPersonId(context));
            return Results.Ok(document);
        }).WithName("ObsoleteRecordArrControlledDocument");

        group.MapPost("/controlled-documents/{controlledDocumentId}/supersede", (HttpContext context, string controlledDocumentId, SupersedeControlledDocumentRequest request, RecordArrStore store) =>
        {
            var document = store.SupersedeControlledDocument(context.User.GetTenantId().ToString(), controlledDocumentId, request.SupersededByDocumentRef, GetActorPersonId(context));
            return Results.Ok(document);
        }).WithName("SupersedeRecordArrControlledDocument");

        group.MapGet("/controlled-documents/{controlledDocumentId}/reviews", (HttpContext context, string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentReviews(context.User.GetTenantId().ToString(), controlledDocumentId)))
            .WithName("ListRecordArrControlledDocumentReviews");

        group.MapGet("/controlled-documents/{controlledDocumentId}/distributions", (HttpContext context, string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentDistributions(context.User.GetTenantId().ToString(), controlledDocumentId)))
            .WithName("ListRecordArrControlledDocumentDistributions");

        group.MapGet("/controlled-documents/{controlledDocumentId}/acknowledgements", (HttpContext context, string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentAcknowledgements(context.User.GetTenantId().ToString(), controlledDocumentId)))
            .WithName("ListRecordArrControlledDocumentAcknowledgements");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions", (HttpContext context, string controlledDocumentId, CreateDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.CreateDocumentDistribution(context.User.GetTenantId().ToString(), controlledDocumentId, request.VersionId, request.DistributionType, request.TargetRef);
            return Results.Created($"/api/v1/workspace/controlled-documents/{controlledDocumentId}/distributions/{distribution.DistributionId}", distribution);
        }).WithName("CreateRecordArrControlledDocumentDistribution");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions/{distributionId}/revoke", (HttpContext context, string controlledDocumentId, string distributionId, RevokeDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.RevokeDocumentDistribution(context.User.GetTenantId().ToString(), distributionId, GetActorPersonId(context), request.RevokeReason);
            return Results.Ok(distribution);
        }).WithName("RevokeRecordArrControlledDocumentDistribution");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions/{distributionId}/expire", (HttpContext context, string controlledDocumentId, string distributionId, ExpireDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.ExpireDocumentDistribution(context.User.GetTenantId().ToString(), distributionId, GetActorPersonId(context), request.ExpireReason);
            return Results.Ok(distribution);
        }).WithName("ExpireRecordArrControlledDocumentDistribution");

        group.MapPost("/controlled-documents/{controlledDocumentId}/acknowledgements", (HttpContext context, string controlledDocumentId, CreateDocumentAcknowledgementRequest request, RecordArrStore store) =>
        {
            var acknowledgement = store.CreateDocumentAcknowledgement(context.User.GetTenantId().ToString(), controlledDocumentId, request.VersionId, GetActorPersonId(context), request.AttestationText, request.DueAt);
            return Results.Created($"/api/v1/workspace/controlled-documents/{controlledDocumentId}/acknowledgements/{acknowledgement.AcknowledgementId}", acknowledgement);
        }).WithName("CreateRecordArrControlledDocumentAcknowledgement");

        group.MapPost("/controlled-documents/{controlledDocumentId}/acknowledgements/{acknowledgementId}/complete", (HttpContext context, string controlledDocumentId, string acknowledgementId, CompleteDocumentAcknowledgementRequest request, RecordArrStore store) =>
        {
            var acknowledgement = store.CompleteDocumentAcknowledgement(context.User.GetTenantId().ToString(), acknowledgementId, request.SignatureRecordRef);
            return Results.Ok(acknowledgement);
        }).WithName("CompleteRecordArrControlledDocumentAcknowledgement");

        group.MapGet("/access-policies", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetAccessPolicies(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrAccessPolicies");

        group.MapPost("/access-policies", (HttpContext context, CreateAccessPolicyRequest request, RecordArrStore store) =>
        {
            var policy = store.CreateAccessPolicy(
                context.User.GetTenantId().ToString(),
                request.RecordId,
                request.PolicyType,
                request.Status,
                request.ReadRules,
                request.WriteRules,
                request.DownloadRules,
                request.ShareRules,
                request.ExportRules,
                request.PurgeRules,
                GetActorPersonId(context));
            return Results.Created($"/api/v1/workspace/access-policies/{policy.AccessPolicyId}", policy);
        }).WithName("CreateRecordArrAccessPolicy");

        group.MapPost("/access-policies/{accessPolicyId}/update", (HttpContext context, string accessPolicyId, UpdateAccessPolicyRequest request, RecordArrStore store) =>
        {
            var policy = store.UpdateAccessPolicy(
                context.User.GetTenantId().ToString(),
                accessPolicyId,
                request.RecordId,
                request.PolicyType,
                request.Status,
                request.ReadRules,
                request.WriteRules,
                request.DownloadRules,
                request.ShareRules,
                request.ExportRules,
                request.PurgeRules,
                GetActorPersonId(context));
            return Results.Ok(policy);
        }).WithName("UpdateRecordArrAccessPolicy");

        group.MapGet("/access-grants", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetAccessGrants(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrAccessGrants");

        group.MapPost("/access-grants/refresh-statuses", (HttpContext context, RecordArrStore store) =>
            Results.Ok(store.RefreshAccessGrants(context.User.GetTenantId().ToString())))
            .WithName("RefreshRecordArrAccessGrants");

        group.MapPost("/access-grants", (HttpContext context, CreateAccessGrantRequest request, RecordArrStore store) =>
        {
            var grant = store.CreateAccessGrant(context.User.GetTenantId().ToString(), request.RecordId, request.GranteeType, request.GranteeRef, request.Permission, GetActorPersonId(context), request.ExpiresAt);
            return Results.Created($"/api/v1/workspace/access-grants/{grant.AccessGrantId}", grant);
        }).WithName("CreateRecordArrAccessGrant");

        group.MapPost("/access-grants/{accessGrantId}/revoke", (HttpContext context, string accessGrantId, RevokeAccessGrantRequest request, RecordArrStore store) =>
        {
            var grant = store.RevokeAccessGrant(context.User.GetTenantId().ToString(), accessGrantId, GetActorPersonId(context), request.RevokeReason);
            return Results.Ok(grant);
        }).WithName("RevokeRecordArrAccessGrant");

        group.MapGet("/external-shares", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetExternalShares(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrExternalShares");

        group.MapPost("/external-shares/refresh-statuses", (HttpContext context, RecordArrStore store) =>
            Results.Ok(store.RefreshExternalShares(context.User.GetTenantId().ToString())))
            .WithName("RefreshRecordArrExternalShares");

        group.MapPost("/external-shares/{externalShareId}/access", (HttpContext context, string externalShareId, RecordExternalShareAccessRequest request, RecordArrStore store) =>
        {
            var share = store.RecordExternalShareAccess(context.User.GetTenantId().ToString(), externalShareId, GetActorPersonId(context), request.AccessAction, request.SourceIp, request.UserAgent);
            return Results.Ok(share);
        }).WithName("AccessRecordArrExternalShare");

        group.MapPost("/external-shares/{externalShareId}/expire", (HttpContext context, string externalShareId, ExpireExternalShareRequest request, RecordArrStore store) =>
        {
            var share = store.ExpireExternalShare(context.User.GetTenantId().ToString(), externalShareId, GetActorPersonId(context));
            return Results.Ok(share);
        }).WithName("ExpireRecordArrExternalShare");

        group.MapGet("/redactions", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetRedactions(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrRedactions");

        group.MapGet("/redaction-provider-jobs", (HttpContext context, RecordArrStore store) =>
            Results.Ok(store.GetRedactionProviderJobs(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrRedactionProviderJobs");

        group.MapPost("/redactions/{redactionId}/provider-jobs", (HttpContext context, string redactionId, SubmitRedactionProviderJobRequest request, RecordArrStore store) =>
            Results.Ok(store.SubmitRedactionProviderJob(
                context.User.GetTenantId().ToString(),
                redactionId,
                GetActorPersonId(context),
                request.ProviderName,
                request.ProviderJobRef)))
            .WithName("SubmitRecordArrRedactionProviderJob");

        group.MapPost("/redaction-provider-jobs/provider-manifests", (HttpContext context, ProcessRedactionProviderJobManifestRequest request, RecordArrStore store) =>
            Results.Ok(store.ProcessRedactionProviderJobManifest(
                context.User.GetTenantId().ToString(),
                GetActorPersonId(context),
                request.ProviderName,
                request.ProviderJobRef,
                request.ProviderCallbackStatus,
                request.ProviderCallbackRef,
                request.ProviderPackageHash)))
            .WithName("ProcessRecordArrRedactionProviderJobManifest");

        group.MapPost("/redactions/{redactionId}/provider-reconciliations", (HttpContext context, string redactionId, ReconcileRedactionProviderRequest request, RecordArrStore store) =>
            Results.Ok(store.ReconcileRedactionProviderStatus(
                context.User.GetTenantId().ToString(),
                redactionId,
                GetActorPersonId(context),
                request.ProviderName,
                request.ProviderJobRef,
                request.ProviderCallbackStatus,
                request.ProviderCallbackRef,
                request.ProviderPackageHash)))
            .WithName("ReconcileRecordArrRedactionProviderStatus");

        group.MapPost("/redactions/{redactionId}/overlay-reviews", (HttpContext context, string redactionId, ReviewRedactionOverlayRequest request, RecordArrStore store) =>
            Results.Ok(store.ReviewRedactionOverlay(
                context.User.GetTenantId().ToString(),
                redactionId,
                GetActorPersonId(context),
                request.OverlayReviewStatus,
                request.OverlayEvidenceRefs,
                request.OverlayIssueRefs)))
            .WithName("ReviewRecordArrRedactionOverlay");

        group.MapPost("/signatures/{signatureRecordId}/provider-reconciliations", (HttpContext context, string signatureRecordId, ReconcileSignatureProviderRequest request, RecordArrStore store) =>
            Results.Ok(store.ReconcileSignatureProviderStatus(
                context.User.GetTenantId().ToString(),
                signatureRecordId,
                GetActorPersonId(context),
                request.ProviderName,
                request.ProviderEnvelopeRef,
                request.ProviderCallbackStatus,
                request.ProviderCallbackRef,
                request.CertificateFingerprintSha256,
                request.TrustTimestampAuthorityRef,
                request.LongTermValidationStatus)))
            .WithName("ReconcileRecordArrSignatureProviderStatus");

        group.MapGet("/signature-trust-service-jobs", (HttpContext context, RecordArrStore store) =>
            Results.Ok(store.GetSignatureTrustServiceJobs(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrSignatureTrustServiceJobs");

        group.MapPost("/signatures/{signatureRecordId}/trust-service-jobs", (HttpContext context, string signatureRecordId, SubmitSignatureTrustServiceJobRequest request, RecordArrStore store) =>
            Results.Ok(store.SubmitSignatureTrustServiceJob(
                context.User.GetTenantId().ToString(),
                signatureRecordId,
                GetActorPersonId(context),
                request.ProviderName,
                request.ProviderEnvelopeRef)))
            .WithName("SubmitRecordArrSignatureTrustServiceJob");

        group.MapPost("/signature-trust-service-jobs/provider-manifests", (HttpContext context, ProcessSignatureTrustServiceManifestRequest request, RecordArrStore store) =>
            Results.Ok(store.ProcessSignatureTrustServiceManifest(
                context.User.GetTenantId().ToString(),
                GetActorPersonId(context),
                request.ProviderName,
                request.ProviderEnvelopeRef,
                request.ProviderCallbackStatus,
                request.ProviderCallbackRef,
                request.CertificateFingerprintSha256,
                request.TrustTimestampAuthorityRef,
                request.LongTermValidationStatus)))
            .WithName("ProcessRecordArrSignatureTrustServiceManifest");

        group.MapGet("/disposal-reviews", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetDisposalReviews(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrDisposalReviews");

        group.MapGet("/destruction-certificates", (HttpContext context, string? recordId, RecordArrStore store) =>
            Results.Ok(store.GetDestructionCertificates(context.User.GetTenantId().ToString(), recordId)))
            .WithName("ListRecordArrDestructionCertificates");

        group.MapPost("/disposal-reviews", (HttpContext context, CreateDisposalReviewRequest request, RecordArrStore store) =>
        {
            var review = store.CreateDisposalReview(context.User.GetTenantId().ToString(), request.RecordId, request.RetentionStatusRef, request.ProposedAction, GetActorPersonId(context));
            return Results.Created($"/api/v1/workspace/disposal-reviews/{review.DisposalReviewId}", review);
        }).WithName("CreateRecordArrDisposalReview");

        group.MapPost("/disposal-reviews/{disposalReviewId}/complete", (HttpContext context, string disposalReviewId, CompleteDisposalReviewRequest request, RecordArrStore store) =>
        {
            var review = store.CompleteDisposalReview(context.User.GetTenantId().ToString(), disposalReviewId, request.Status, GetActorPersonId(context), request.DecisionReason);
            return Results.Ok(review);
        }).WithName("CompleteRecordArrDisposalReview");

        group.MapGet("/retention-policies", (RecordArrStore store) => Results.Ok(store.GetRetentionPolicies()))
            .WithName("ListRecordArrRetentionPolicies");

        group.MapGet("/records/{recordId}/retention-status", (HttpContext context, string recordId, RecordArrStore store) =>
        {
            var status = store.GetRetentionStatus(context.User.GetTenantId().ToString(), recordId);
            return status is null ? Results.NotFound() : Results.Ok(status);
        }).WithName("GetRecordArrRetentionStatus");

        group.MapPost("/retention-statuses/recalculate", (HttpContext context, RecordArrStore store) =>
        {
            return Results.Ok(store.RecalculateRetentionStatuses(context.User.GetTenantId().ToString()));
        }).WithName("RecalculateRecordArrRetentionStatuses");

        group.MapPost("/retention-disposition-runs", (HttpContext context, RunRetentionDispositionSchedulerRequest? request, RecordArrStore store) =>
        {
            return Results.Ok(store.RunRetentionDispositionScheduler(context.User.GetTenantId().ToString(), GetActorPersonId(context), request?.ExecutionPolicy));
        }).WithName("RunRecordArrRetentionDispositionScheduler");

        group.MapGet("/retention-disposition-runs", (HttpContext context, RecordArrStore store) =>
            Results.Ok(store.GetRetentionSchedulerRuns(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrRetentionDispositionSchedulerRuns");

        group.MapGet("/retention-disposition-leases", (HttpContext context, RecordArrStore store) =>
            Results.Ok(store.GetRetentionSchedulerLeases(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrRetentionDispositionSchedulerLeases");

        group.MapGet("/retention-disposition-outbox", (HttpContext context, RecordArrStore store) =>
            Results.Ok(store.GetRetentionSchedulerOutboxMessages(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrRetentionDispositionSchedulerOutbox");

        group.MapPost("/retention-disposition-outbox/process", (HttpContext context, string? deliveryChannel, string? externalProviderRef, int? maxMessages, RecordArrStore store) =>
            Results.Ok(store.ProcessRetentionSchedulerOutbox(
                context.User.GetTenantId().ToString(),
                GetActorPersonId(context),
                deliveryChannel,
                externalProviderRef,
                maxMessages ?? 100)))
            .WithName("ProcessRecordArrRetentionDispositionSchedulerOutbox");

        group.MapPost("/retention-disposition-outbox/escalate", (HttpContext context, EscalateRetentionSchedulerOutboxRequest request, RecordArrStore store) =>
            Results.Ok(store.EscalateRetentionSchedulerOutbox(
                context.User.GetTenantId().ToString(),
                GetActorPersonId(context),
                request.EscalationRecipientRef,
                request.MaxMessages ?? 100)))
            .WithName("EscalateRecordArrRetentionDispositionSchedulerOutbox");

        group.MapPost("/legal-holds", (HttpContext context, CreateLegalHoldRequest request, RecordArrStore store) =>
        {
            var hold = store.CreateLegalHold(
                context.User.GetTenantId().ToString(),
                request.Title,
                request.Description,
                request.HoldType,
                request.SourceProduct,
                request.SourceObjectType,
                request.SourceObjectId,
                GetActorPersonId(context),
                request.ScopeRules,
                request.RecordRefs);
            return Results.Created($"/api/v1/workspace/legal-holds/{hold.LegalHoldId}", hold);
        }).WithName("CreateRecordArrLegalHold");

        group.MapGet("/legal-holds", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetLegalHolds(context.User.GetTenantId().ToString())))
            .WithName("ListRecordArrLegalHolds");

        group.MapPost("/legal-holds/{legalHoldId}/activate", (HttpContext context, string legalHoldId, RecordArrStore store) =>
        {
            var hold = store.ActivateLegalHold(context.User.GetTenantId().ToString(), legalHoldId);
            return Results.Ok(hold);
        }).WithName("ActivateRecordArrLegalHold");

        group.MapPost("/legal-holds/{legalHoldId}/release", (HttpContext context, string legalHoldId, ReleaseLegalHoldRequest request, RecordArrStore store) =>
        {
            var hold = store.ReleaseLegalHold(context.User.GetTenantId().ToString(), legalHoldId, GetActorPersonId(context), request.ReleaseReason);
            return Results.Ok(hold);
        }).WithName("ReleaseRecordArrLegalHold");
    }

    private static string GetActorPersonId(HttpContext context) =>
        context.User.GetPersonId().ToString("D");

    public sealed record CreateRecordRequest(
        string Title,
        string Description,
        string RecordType,
        string DocumentClass,
        string DocumentType,
        string DocumentSubtype,
        string Classification,
        string SourceProduct,
        string SourceObjectType,
        string SourceObjectId,
        string SourceObjectDisplayName,
        string OwnerPersonId,
        string CurrentFileName,
        string CurrentMimeType,
        string? FileContentBase64 = null);

    public sealed record UpdateRecordRequest(
        string Status,
        string? Classification,
        DateTimeOffset? EffectiveAt,
        DateTimeOffset? ExpiresAt);

    public sealed record DisposeRecordRequest(string ActorPersonId);

    public sealed record CreateFileRequest(
        string RecordId,
        string OriginalFilename,
        string MimeType,
        string? StorageProvider,
        string? StorageKey,
        long? SizeBytes,
        int? PageCount,
        int? ImageWidth,
        int? ImageHeight,
        int? DurationSeconds);

    public sealed record CreateFileIntegrityCheckRequest(string? ObservedChecksumSha256, string? CheckMethod);

    public sealed record CreateFileMalwareScanRequest(
        string Status,
        string? ScannerName,
        string? ScannerVersion,
        string? SignatureVersion,
        string? ThreatName,
        string? FailureReason);

    public sealed record RunFileMalwareScanProviderRequest(
        string? ScannerName,
        string? ScannerVersion,
        string? SignatureVersion,
        IReadOnlyList<string>? InfectedFileIds,
        IReadOnlyList<string>? FailedFileIds,
        IReadOnlyList<string>? SkippedFileIds);

    public sealed record DeadLetterFileMalwareScansRequest(int? MaxFiles);

    public sealed record RunStorageReconciliationRequest(
        string? Scope,
        string? RecordId,
        IReadOnlyList<string>? CheckedFileIds,
        IReadOnlyList<string>? MissingFileIds,
        IReadOnlyList<string>? CorruptFileIds);

    public sealed record RemediateStorageReconciliationRequest(
        IReadOnlyList<string>? RestoredFileIds,
        IReadOnlyList<string>? AcceptedMissingFileIds,
        IReadOnlyList<string>? RecheckedCorruptFileIds,
        IReadOnlyList<string>? ReleasedQuarantinedFileIds,
        IReadOnlyList<string>? ScannedPendingFileIds);

    public sealed record VerifyObjectStoreLifecycleRequest(
        string? ProviderName,
        string? PolicyRef,
        string? RetentionMode,
        DateTimeOffset RetainUntil,
        string? EncryptionKeyRef,
        string? EvidenceRef);

    public sealed record RunDisasterRecoveryRestoreRequest(
        string? RecoveryPointId,
        DateTimeOffset RecoveryPointCreatedAt,
        int RpoTargetMinutes,
        int RtoTargetMinutes,
        IReadOnlyList<string>? RecordIds,
        IReadOnlyList<string>? MissingFileIds,
        IReadOnlyList<string>? CorruptFileIds);

    public sealed record RunDisasterRecoveryBackupVerificationRequest(
        string? BackupProviderName,
        string? BackupJobRef,
        string? BackupManifestHash,
        string? RecoveryPointId,
        DateTimeOffset RecoveryPointCreatedAt,
        int RpoTargetMinutes,
        IReadOnlyList<string>? RecordIds,
        IReadOnlyList<string>? MissingFileIds,
        IReadOnlyList<string>? CorruptFileIds);

    public sealed record CreateAccessHistorySealRequest(string? RecordId);

    public sealed record CreateAuditSealRequest(string? RecordId);

    public sealed record AnchorAuditSealRequest(
        string? AnchorProviderName,
        string? AnchorReference,
        DateTimeOffset AnchoredAt,
        string? AnchoredSealHash);

    public sealed record CreateCaptureRequestRequest(
        string SourceProduct,
        string SourceObjectRef,
        string CaptureType,
        string Title,
        string Instructions,
        bool Required,
        string? UploadSessionRef,
        string? EvidenceRequirementRef);

    public sealed record CreateUploadSessionRequest(
        string SourceProduct,
        string SourceObjectType,
        string SourceObjectId,
        string UploadPurpose,
        bool RequiresDocumentScan,
        bool RequiresOcr,
        bool RequiresManualReview);

    public sealed record CompleteUploadSessionRequest(string RecordId);
    public sealed record RevokeUploadSessionRequest(string Reason);
    public sealed record CreateDocumentScanRequest(string RecordId, string OriginalFileName, string ScanPurpose);
    public sealed record CreateRecordMetadataRequest(string Key, string Value, string ValueType, string Source, decimal ConfidenceScore);
    public sealed record CreateRecordLinkRequest(string? LinkedRecordId, string? SourceObjectRef, string LinkType);
    public sealed record CreateRecordCommentRequest(string Body, string Visibility);
    public sealed record UpdateRecordCommentRequest(string Body, string Visibility);
    public sealed record ManualCorrectionRequest(string EdgeCoordinates);
    public sealed record ReviewExtractionResultRequest(string Status, string? FailureReason);
    public sealed record CreateDocumentDistributionRequest(string VersionId, string DistributionType, string TargetRef);
    public sealed record RevokeDocumentDistributionRequest(string? RevokeReason);
    public sealed record ExpireDocumentDistributionRequest(string ExpiredByPersonId, string? ExpireReason);
    public sealed record CreateDocumentAcknowledgementRequest(string VersionId, string PersonId, string? AttestationText, DateTimeOffset? DueAt);
    public sealed record CompleteDocumentAcknowledgementRequest(string? SignatureRecordRef);
    public sealed record CreateSignatureRecordRequest(
        string RecordId,
        string SignaturePurpose,
        string? SignerPersonId,
        string? SignerExternalName,
        string? SignerTitle,
        string AttestationText,
        string SourceProduct,
        string SourceObjectRef,
        string? GeoCoordinates,
        string? DeviceSnapshot,
        string? ProviderName = null,
        string? ProviderEnvelopeRef = null,
        string? CertificateFingerprintSha256 = null);

    public sealed record ReconcileSignatureProviderRequest(
        string? ProviderName,
        string? ProviderEnvelopeRef,
        string? ProviderCallbackStatus,
        string? ProviderCallbackRef,
        string? CertificateFingerprintSha256,
        string? TrustTimestampAuthorityRef,
        string? LongTermValidationStatus);

    public sealed record SubmitSignatureTrustServiceJobRequest(
        string? ProviderName,
        string? ProviderEnvelopeRef);

    public sealed record ProcessSignatureTrustServiceManifestRequest(
        string? ProviderName,
        string? ProviderEnvelopeRef,
        string? ProviderCallbackStatus,
        string? ProviderCallbackRef,
        string? CertificateFingerprintSha256,
        string? TrustTimestampAuthorityRef,
        string? LongTermValidationStatus);

    public sealed record ReconcileRedactionProviderRequest(
        string? ProviderName,
        string? ProviderJobRef,
        string? ProviderCallbackStatus,
        string? ProviderCallbackRef,
        string? ProviderPackageHash);

    public sealed record SubmitRedactionProviderJobRequest(
        string? ProviderName,
        string? ProviderJobRef);

    public sealed record ProcessRedactionProviderJobManifestRequest(
        string? ProviderName,
        string? ProviderJobRef,
        string? ProviderCallbackStatus,
        string? ProviderCallbackRef,
        string? ProviderPackageHash);

    public sealed record ReviewRedactionOverlayRequest(
        string? OverlayReviewStatus,
        IReadOnlyList<string>? OverlayEvidenceRefs,
        IReadOnlyList<string>? OverlayIssueRefs);

    public sealed record CreatePhotoEvidenceRequest(
        string RecordId,
        string PhotoPurpose,
        string SourceProduct,
        string SourceObjectRef,
        string? GeoCoordinates,
        string? DeviceSnapshot,
        string? Notes);
    public sealed record PromoteControlledDocumentVersionRequest(DateTimeOffset? EffectiveAt);
    public sealed record UpdateControlledDocumentStatusRequest();
    public sealed record SupersedeControlledDocumentRequest(string SupersededByDocumentRef, string SupersededByPersonId);
    public sealed record CreateAccessPolicyRequest(
        string RecordId,
        string PolicyType,
        string Status,
        IReadOnlyList<string> ReadRules,
        IReadOnlyList<string> WriteRules,
        IReadOnlyList<string> DownloadRules,
        IReadOnlyList<string> ShareRules,
        IReadOnlyList<string> ExportRules,
        IReadOnlyList<string> PurgeRules);
    public sealed record UpdateAccessPolicyRequest(
        string RecordId,
        string PolicyType,
        string Status,
        IReadOnlyList<string> ReadRules,
        IReadOnlyList<string> WriteRules,
        IReadOnlyList<string> DownloadRules,
        IReadOnlyList<string> ShareRules,
        IReadOnlyList<string> ExportRules,
        IReadOnlyList<string> PurgeRules);
    public sealed record CreateAccessGrantRequest(string RecordId, string GranteeType, string GranteeRef, string Permission, DateTimeOffset? ExpiresAt);
    public sealed record RevokeAccessGrantRequest(string? RevokeReason);
    public sealed record RecordExternalShareAccessRequest(string AccessAction, string? SourceIp, string? UserAgent);
    public sealed record ExpireExternalShareRequest(string ExpiredByPersonId);
    public sealed record CreateDisposalReviewRequest(string RecordId, string RetentionStatusRef, string ProposedAction);
    public sealed record CompleteDisposalReviewRequest(string Status, string? DecisionReason);
    public sealed record RunRetentionDispositionSchedulerRequest(string? ExecutionPolicy);
    public sealed record EscalateRetentionSchedulerOutboxRequest(string EscalationRecipientRef, int? MaxMessages);

    public sealed record CreateEvidenceMappingRequest(
        string RecordId,
        string SourceProduct,
        string SourceObjectType,
        string SourceObjectId,
        string ComplianceRequirementRef,
        string EvidenceTypeKey,
        string MappingSource,
        decimal ConfidenceScore);

    public sealed record ConfirmEvidenceMappingRequest(string ConfirmedByPersonId, string? Notes);
    public sealed record RejectEvidenceMappingRequest(string RejectedByPersonId, string RejectionReason, string? Notes);

    public sealed record CreatePackageRequest(
        string Title,
        string PackageType,
        string SourceProduct,
        string SourceObjectRef,
        string RecordRef);

    public sealed record CreateLegalHoldRequest(
        string Title,
        string Description,
        string HoldType,
        string SourceProduct,
        string SourceObjectType,
        string SourceObjectId,
        IReadOnlyList<string> ScopeRules,
        IReadOnlyList<string> RecordRefs);

    public sealed record ReleaseLegalHoldRequest(string ReleaseReason);
}
