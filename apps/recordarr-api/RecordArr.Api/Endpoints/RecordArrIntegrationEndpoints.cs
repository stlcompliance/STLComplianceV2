using RecordArr.Api.Data;
using RecordArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace RecordArr.Api.Endpoints;

public static class RecordArrIntegrationEndpoints
{
    private const long MaxInlineFileUploadBytes = 25L * 1024 * 1024;
    private const int MaxInlineFileUploadBase64Chars = (int)(((MaxInlineFileUploadBytes + 2) / 3) * 4);

    public static void MapRecordArrIntegrationEndpoints(this WebApplication app)
    {
        MapRoutes(app.MapGroup("/api/integrations"), "/api/integrations");
        MapRoutes(app.MapGroup("/api/v1/integrations"), "/api/v1/integrations");
    }

    private static void MapRoutes(RouteGroupBuilder group, string routePrefix)
    {
        group.WithTags("Integrations").RequireAuthorization();

        group.MapGet("/records", (HttpContext context, string? search, RecordArrStore store) => Results.Ok(store.GetRecords(context.User, search)))
            .WithName($"ListRecordArrIntegrationRecords{routePrefix}");

        group.MapGet("/reminders", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetReminders(context.User)))
            .WithName($"ListRecordArrIntegrationReminders{routePrefix}");

        group.MapGet("/records/{recordId}", (HttpContext context, string recordId, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            return record is null ? Results.NotFound() : Results.Ok(record);
        }).WithName($"GetRecordArrIntegrationRecord{routePrefix}");

        group.MapGet("/records/{recordId}/metadata", (HttpContext context, string recordId, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            return record is null ? Results.NotFound() : Results.Ok(store.GetRecordMetadata(recordId));
        }).WithName($"ListRecordArrIntegrationRecordMetadata{routePrefix}");

        group.MapPost("/records/{recordId}/metadata", (HttpContext context, string recordId, WorkspaceEndpoints.CreateRecordMetadataRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            var metadata = store.CreateRecordMetadata(recordId, request.Key, request.Value, request.ValueType, request.Source, request.ConfidenceScore, GetActorPersonId(context));
            return Results.Created($"{routePrefix}/records/{recordId}/metadata/{metadata.MetadataId}", metadata);
        }).WithName($"CreateRecordArrIntegrationRecordMetadata{routePrefix}");

        group.MapGet("/records/{recordId}/links", (HttpContext context, string recordId, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            return record is null ? Results.NotFound() : Results.Ok(store.GetRecordLinks(recordId));
        }).WithName($"ListRecordArrIntegrationRecordLinks{routePrefix}");

        group.MapPost("/records/{recordId}/links", (HttpContext context, string recordId, WorkspaceEndpoints.CreateRecordLinkRequest request, RecordArrStore store) =>
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
            return Results.Created($"{routePrefix}/records/{recordId}/links/{link.RecordLinkId}", link);
        }).WithName($"CreateRecordArrIntegrationRecordLink{routePrefix}");

        group.MapGet("/records/{recordId}/comments", (HttpContext context, string recordId, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            return record is null ? Results.NotFound() : Results.Ok(store.GetRecordComments(recordId));
        }).WithName($"ListRecordArrIntegrationRecordComments{routePrefix}");

        group.MapPost("/records/{recordId}/comments", (HttpContext context, string recordId, WorkspaceEndpoints.CreateRecordCommentRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            var comment = store.CreateRecordComment(recordId, request.Body, request.Visibility, GetActorPersonId(context));
            return Results.Created($"{routePrefix}/records/{recordId}/comments/{comment.CommentId}", comment);
        }).WithName($"CreateRecordArrIntegrationRecordComment{routePrefix}");

        group.MapPatch("/records/{recordId}/comments/{commentId}", (HttpContext context, string recordId, string commentId, WorkspaceEndpoints.UpdateRecordCommentRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            var comment = store.UpdateRecordComment(commentId, request.Body, request.Visibility, GetActorPersonId(context));
            return Results.Ok(comment);
        }).WithName($"UpdateRecordArrIntegrationRecordComment{routePrefix}");

        group.MapPost("/records", (HttpContext context, WorkspaceEndpoints.CreateRecordRequest request, RecordArrStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.SourceProduct) ||
                string.IsNullOrWhiteSpace(request.SourceObjectType) ||
                string.IsNullOrWhiteSpace(request.SourceObjectId) ||
                string.IsNullOrWhiteSpace(request.SourceObjectDisplayName))
            {
                return Results.BadRequest(new { code = "missing_primary_target", message = "Record creation requires a primary target reference." });
            }
            if (string.IsNullOrWhiteSpace(request.DocumentClass) || string.IsNullOrWhiteSpace(request.DocumentType) || string.IsNullOrWhiteSpace(request.DocumentSubtype))
            {
                return Results.BadRequest(new { code = "missing_document_classification", message = "Record creation requires document class, type, and subtype." });
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
                request.CurrentMimeType);
            return Results.Created($"{routePrefix}/records/{record.RecordId}", record);
        }).WithName($"CreateRecordArrIntegrationRecord{routePrefix}");

        group.MapPatch("/records/{recordId}", (HttpContext context, string recordId, WorkspaceEndpoints.UpdateRecordRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, recordId);
            if (record is null)
            {
                return Results.NotFound();
            }

            var updated = store.UpdateRecordStatus(recordId, request.Status, request.Classification, request.EffectiveAt, request.ExpiresAt);
            return Results.Ok(updated);
        }).WithName($"UpdateRecordArrIntegrationRecord{routePrefix}");

        group.MapPost("/records/{recordId}/archive", (HttpContext context, string recordId, WorkspaceEndpoints.DisposeRecordRequest request, RecordArrStore store) =>
        {
            var updated = store.ArchiveRecord(recordId, GetActorPersonId(context));
            return Results.Ok(updated);
        }).WithName($"ArchiveRecordArrIntegrationRecord{routePrefix}");

        group.MapPost("/records/{recordId}/purge", (HttpContext context, string recordId, WorkspaceEndpoints.DisposeRecordRequest request, RecordArrStore store) =>
        {
            var updated = store.PurgeRecord(recordId, GetActorPersonId(context));
            return Results.Ok(updated);
        }).WithName($"PurgeRecordArrIntegrationRecord{routePrefix}");

        group.MapPost("/files", (HttpContext context, WorkspaceEndpoints.CreateFileRequest request, RecordArrStore store) =>
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
            return Results.Created($"{routePrefix}/files/{file.FileId}", file);
        }).WithName($"CreateRecordArrIntegrationFile{routePrefix}");

        group.MapPost("/smart-import/source-files", async (
            SmartImportRetainSourceRequest request,
            HttpContext context,
            RecordArrStore store,
            RecordArrDocumentStorageService storage,
            StlServiceTokenValidator tokenValidator) =>
        {
            ValidateSmartImportRetainSourceCaller(context, tokenValidator, request.TenantId);

            if (request.ContentBase64.Length > MaxInlineFileUploadBase64Chars)
            {
                return Results.BadRequest("ContentBase64 exceeds the maximum inline size.");
            }

            byte[] contentBytes;
            try
            {
                contentBytes = Convert.FromBase64String(request.ContentBase64);
            }
            catch (FormatException)
            {
                return Results.BadRequest("ContentBase64 must be valid base64.");
            }

            if (contentBytes.LongLength != request.SizeBytes)
            {
                return Results.BadRequest("SizeBytes must match the decoded ContentBase64 payload length.");
            }

            await using var contentStream = new MemoryStream(contentBytes, writable: false);
            var storageKey = await storage.SaveAsync(request.TenantId, request.ImportBatchId, request.Sha256, request.FileName, contentStream);
            var record = store.CreateRecord(
                request.TenantId.ToString("D"),
                $"Smart Import source: {request.FileName}",
                "Source file retained for STL Smart Import review and audit.",
                "document",
                ResolveDocumentClass(request.FileName, request.ContentType),
                ResolveDocumentType(request.FileName, request.ContentType),
                ResolveDocumentSubtype(request.FileName, request.ContentType),
                "internal",
                "nexarr",
                "smart_import_batch",
                request.ImportBatchId.ToString("D"),
                request.FileName,
                GetActorPersonId(context),
                GetActorPersonId(context),
                request.FileName,
                request.ContentType,
                "recordarr",
                storageKey,
                request.SizeBytes);

            store.CreateRecordMetadata(record.RecordId, "sha256", request.Sha256, "string", "import", 1.0m, GetActorPersonId(context));
            store.CreateRecordMetadata(record.RecordId, "destination_product_hint", request.DestinationProductHint, "string", "import", 1.0m, GetActorPersonId(context));

            return Results.Created($"{routePrefix}/files/{record.CurrentFileRef}", new SmartImportRetainSourceResponse(
                record.RecordId,
                record.CurrentFileRef,
                storageKey,
                "retained"));
        }).WithName($"RetainRecordArrSmartImportSource{routePrefix}");

        group.MapGet("/files", (HttpContext context, string? recordId, RecordArrStore store) => Results.Ok(store.GetFiles(context.User, recordId)))
            .WithName($"ListRecordArrIntegrationFiles{routePrefix}");

        group.MapGet("/files/{fileId}", (HttpContext context, string fileId, RecordArrStore store) =>
        {
            var file = store.GetFile(context.User, fileId);
            return file is null ? Results.NotFound() : Results.Ok(file);
        }).WithName($"GetRecordArrIntegrationFile{routePrefix}");

        group.MapGet("/files/{fileId}/download", (HttpContext context, string fileId, RecordArrStore store) =>
        {
            try
            {
                return Results.Text(store.DownloadFile(context.User, fileId), "text/plain");
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        }).WithName($"DownloadRecordArrIntegrationFile{routePrefix}");

        group.MapGet("/capture-requests", (RecordArrStore store) => Results.Ok(store.GetCaptureRequests()))
            .WithName($"ListRecordArrIntegrationCaptureRequests{routePrefix}");

        group.MapPost("/capture-requests", (HttpContext context, WorkspaceEndpoints.CreateCaptureRequestRequest request, RecordArrStore store) =>
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
            return Results.Created($"{routePrefix}/capture-requests/{captureRequest.CaptureRequestId}", captureRequest);
        }).WithName($"CreateRecordArrIntegrationCaptureRequest{routePrefix}");

        group.MapPost("/capture-requests/{captureRequestId}/complete", (string captureRequestId, RecordArrStore store) =>
        {
            var captureRequest = store.CompleteCaptureRequest(captureRequestId);
            return Results.Ok(captureRequest);
        }).WithName($"CompleteRecordArrIntegrationCaptureRequest{routePrefix}");

        group.MapPost("/capture-requests/{captureRequestId}/skip", (string captureRequestId, RecordArrStore store) =>
        {
            var captureRequest = store.SkipCaptureRequest(captureRequestId);
            return Results.Ok(captureRequest);
        }).WithName($"SkipRecordArrIntegrationCaptureRequest{routePrefix}");

        group.MapPost("/capture-requests/{captureRequestId}/cancel", (string captureRequestId, RecordArrStore store) =>
        {
            var captureRequest = store.CancelCaptureRequest(captureRequestId);
            return Results.Ok(captureRequest);
        }).WithName($"CancelRecordArrIntegrationCaptureRequest{routePrefix}");

        group.MapPost("/capture-requests/{captureRequestId}/expire", (string captureRequestId, RecordArrStore store) =>
        {
            var captureRequest = store.ExpireCaptureRequest(captureRequestId);
            return Results.Ok(captureRequest);
        }).WithName($"ExpireRecordArrIntegrationCaptureRequest{routePrefix}");

        group.MapGet("/upload-sessions/{uploadSessionId}", (string uploadSessionId, RecordArrStore store) =>
        {
            var session = store.GetUploadSession(uploadSessionId);
            return session is null ? Results.NotFound() : Results.Ok(session);
        }).WithName($"GetRecordArrIntegrationUploadSession{routePrefix}");

        group.MapGet("/upload-sessions", (RecordArrStore store) => Results.Ok(store.GetUploadSessions()))
            .WithName($"ListRecordArrIntegrationUploadSessions{routePrefix}");

        group.MapPost("/upload-sessions/{uploadSessionId}/complete", (string uploadSessionId, WorkspaceEndpoints.CompleteUploadSessionRequest request, RecordArrStore store) =>
        {
            var session = store.CompleteUploadSession(uploadSessionId, request.RecordId);
            return Results.Ok(session);
        }).WithName($"CompleteRecordArrIntegrationUploadSession{routePrefix}");

        group.MapPost("/upload-sessions/{uploadSessionId}/revoke", (string uploadSessionId, WorkspaceEndpoints.RevokeUploadSessionRequest request, RecordArrStore store) =>
        {
            var session = store.RevokeUploadSession(uploadSessionId, request.Reason);
            return Results.Ok(session);
        }).WithName($"RevokeRecordArrIntegrationUploadSession{routePrefix}");

        group.MapPost("/document-scans", (WorkspaceEndpoints.CreateDocumentScanRequest request, RecordArrStore store) =>
        {
            var scan = store.CreateScanProcessing(request.RecordId, request.OriginalFileName, request.ScanPurpose);
            return Results.Created($"{routePrefix}/document-scans/{scan.ScanProcessingId}", scan);
        }).WithName($"CreateRecordArrIntegrationDocumentScan{routePrefix}");

        group.MapGet("/document-scans", (HttpContext context, RecordArrStore store) => Results.Ok(store.GetScanProcessing().Where(scan => store.GetRecord(context.User, scan.RecordId) is not null)))
            .WithName($"ListRecordArrIntegrationDocumentScans{routePrefix}");

        group.MapGet("/document-scans/{scanProcessingId}", (HttpContext context, string scanProcessingId, RecordArrStore store) =>
        {
            var scan = store.GetScanProcessing(scanProcessingId);
            return scan is null || store.GetRecord(context.User, scan.RecordId) is null ? Results.NotFound() : Results.Ok(scan);
        }).WithName($"GetRecordArrIntegrationDocumentScan{routePrefix}");

        group.MapPost("/document-scans/{scanProcessingId}/manual-correction", (HttpContext context, string scanProcessingId, WorkspaceEndpoints.ManualCorrectionRequest request, RecordArrStore store) =>
        {
            var scan = store.GetScanProcessing(scanProcessingId);
            if (scan is null || store.GetRecord(context.User, scan.RecordId) is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(store.ApplyManualCorrection(scanProcessingId, request.EdgeCoordinates, GetActorPersonId(context)));
        }).WithName($"ApplyRecordArrIntegrationManualCorrection{routePrefix}");

        group.MapPost("/signatures", (HttpContext context, WorkspaceEndpoints.CreateSignatureRecordRequest request, RecordArrStore store) =>
        {
            var signature = store.CreateSignatureRecord(
                request.RecordId,
                request.SignaturePurpose,
                request.SignerPersonId,
                request.SignerExternalName,
                request.SignerTitle,
                request.AttestationText,
                GetActorPersonId(context),
                request.SourceProduct,
                request.SourceObjectRef,
                request.GeoCoordinates,
                request.DeviceSnapshot);
            return Results.Created($"{routePrefix}/signatures/{signature.SignatureRecordId}", signature);
        }).WithName($"CreateRecordArrIntegrationSignature{routePrefix}");

        group.MapPost("/photo-evidence", (HttpContext context, WorkspaceEndpoints.CreatePhotoEvidenceRequest request, RecordArrStore store) =>
        {
            var photo = store.CreatePhotoEvidence(
                request.RecordId,
                request.PhotoPurpose,
                GetActorPersonId(context),
                request.SourceProduct,
                request.SourceObjectRef,
                request.GeoCoordinates,
                request.DeviceSnapshot,
                request.Notes);
            return Results.Created($"{routePrefix}/photo-evidence/{photo.PhotoEvidenceId}", photo);
        }).WithName($"CreateRecordArrIntegrationPhotoEvidence{routePrefix}");

        group.MapGet("/ocr-results/{ocrResultId}", (string ocrResultId, RecordArrStore store) =>
        {
            var result = store.GetOcrResult(ocrResultId);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName($"GetRecordArrIntegrationOcrResult{routePrefix}");

        group.MapGet("/extraction-results/{extractionResultId}", (string extractionResultId, RecordArrStore store) =>
        {
            var result = store.GetExtractionResult(extractionResultId);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName($"GetRecordArrIntegrationExtractionResult{routePrefix}");

        group.MapPost("/extraction-results/{extractionResultId}/review", (HttpContext context, string extractionResultId, WorkspaceEndpoints.ReviewExtractionResultRequest request, RecordArrStore store) =>
        {
            var extractionResult = store.GetExtractionResult(extractionResultId);
            if (extractionResult is null || store.GetRecord(context.User, extractionResult.RecordId) is null)
            {
                return Results.NotFound();
            }

            var result = store.ReviewExtractionResult(extractionResultId, GetActorPersonId(context), request.Status, request.FailureReason);
            return Results.Ok(result);
        }).WithName($"ReviewRecordArrIntegrationExtractionResult{routePrefix}");

        group.MapGet("/evidence-mappings", (RecordArrStore store) => Results.Ok(store.GetEvidenceMappings()))
            .WithName($"ListRecordArrIntegrationEvidenceMappings{routePrefix}");

        group.MapPost("/evidence-mappings", (HttpContext context, WorkspaceEndpoints.CreateEvidenceMappingRequest request, RecordArrStore store) =>
        {
            var record = store.GetRecord(context.User, request.RecordId);
            if (record is null)
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
            return Results.Created($"{routePrefix}/evidence-mappings/{mapping.EvidenceMappingId}", mapping);
        }).WithName($"CreateRecordArrIntegrationEvidenceMapping{routePrefix}");

        group.MapPost("/evidence-mappings/{mappingId}/confirm", (HttpContext context, string mappingId, WorkspaceEndpoints.ConfirmEvidenceMappingRequest request, RecordArrStore store) =>
        {
            var mapping = store.UpdateEvidenceMapping(mappingId, "confirmed", GetActorPersonId(context), request.Notes, null);
            return Results.Ok(mapping);
        }).WithName($"ConfirmRecordArrIntegrationEvidenceMapping{routePrefix}");

        group.MapPost("/evidence-mappings/{mappingId}/reject", (HttpContext context, string mappingId, WorkspaceEndpoints.RejectEvidenceMappingRequest request, RecordArrStore store) =>
        {
            var mapping = store.UpdateEvidenceMapping(mappingId, "rejected", GetActorPersonId(context), request.Notes, request.RejectionReason);
            return Results.Ok(mapping);
        }).WithName($"RejectRecordArrIntegrationEvidenceMapping{routePrefix}");

        group.MapGet("/evidence-coverage", (RecordArrStore store) => Results.Ok(store.GetEvidenceCoverage()))
            .WithName($"ListRecordArrIntegrationEvidenceCoverage{routePrefix}");

        group.MapGet("/record-packages", (RecordArrStore store) => Results.Ok(store.GetPackages()))
            .WithName($"ListRecordArrIntegrationPackages{routePrefix}");

        group.MapGet("/record-packages/{packageId}", (string packageId, RecordArrStore store) =>
        {
            var package = store.GetPackage(packageId);
            return package is null ? Results.NotFound() : Results.Ok(package);
        }).WithName($"GetRecordArrIntegrationPackage{routePrefix}");

        group.MapPost("/record-packages", (HttpContext context, WorkspaceEndpoints.CreatePackageRequest request, RecordArrStore store) =>
        {
            var package = store.CreatePackage(request.Title, request.PackageType, request.SourceProduct, request.SourceObjectRef, request.RecordRef, GetActorPersonId(context));
            return Results.Created($"{routePrefix}/record-packages/{package.PackageId}", package);
        }).WithName($"CreateRecordArrIntegrationPackage{routePrefix}");

        group.MapPost("/record-packages/{packageId}/lock", (string packageId, RecordArrStore store) =>
        {
            var package = store.LockPackage(packageId);
            return Results.Ok(package);
        }).WithName($"LockRecordArrIntegrationPackage{routePrefix}");

        group.MapPost("/record-packages/{packageId}/archive", (string packageId, RecordArrStore store) =>
        {
            var package = store.ArchivePackage(packageId);
            return Results.Ok(package);
        }).WithName($"ArchiveRecordArrIntegrationPackage{routePrefix}");

        group.MapGet("/record-packages/{packageId}/download", (string packageId, RecordArrStore store) =>
        {
            var package = store.GetPackage(packageId);
            return package is null
                ? Results.NotFound()
                : Results.Ok(new { package.PackageId, package.PackageNumber, package.Title, package.Status, package.RecordRefs, package.SourceObjectRefs, package.ManifestChecksum, package.GeneratedPdfRecordRef, package.GeneratedZipFileRef });
        }).WithName($"DownloadRecordArrIntegrationPackage{routePrefix}");

        group.MapGet("/retention-policies", (RecordArrStore store) => Results.Ok(store.GetRetentionPolicies()))
            .WithName($"ListRecordArrIntegrationRetentionPolicies{routePrefix}");

        group.MapGet("/records/{recordId}/retention-status", (string recordId, RecordArrStore store) =>
        {
            var status = store.GetRetentionStatus(recordId);
            return status is null ? Results.NotFound() : Results.Ok(status);
        }).WithName($"GetRecordArrIntegrationRetentionStatus{routePrefix}");

        group.MapPost("/retention-statuses/recalculate", (RecordArrStore store) =>
        {
            return Results.Ok(store.RecalculateRetentionStatuses());
        }).WithName($"RecalculateRecordArrIntegrationRetentionStatuses{routePrefix}");

        group.MapPost("/legal-holds", (HttpContext context, WorkspaceEndpoints.CreateLegalHoldRequest request, RecordArrStore store) =>
        {
            var hold = store.CreateLegalHold(
                request.Title,
                request.Description,
                request.HoldType,
                request.SourceProduct,
                request.SourceObjectType,
                request.SourceObjectId,
                GetActorPersonId(context),
                request.ScopeRules,
                request.RecordRefs);
            return Results.Created($"{routePrefix}/legal-holds/{hold.LegalHoldId}", hold);
        }).WithName($"CreateRecordArrIntegrationLegalHold{routePrefix}");

        group.MapGet("/legal-holds", (RecordArrStore store) => Results.Ok(store.GetLegalHolds()))
            .WithName($"ListRecordArrIntegrationLegalHolds{routePrefix}");

        group.MapPost("/legal-holds/{legalHoldId}/release", (HttpContext context, string legalHoldId, WorkspaceEndpoints.ReleaseLegalHoldRequest request, RecordArrStore store) =>
        {
            var hold = store.ReleaseLegalHold(legalHoldId, GetActorPersonId(context), request.ReleaseReason);
            return Results.Ok(hold);
        }).WithName($"ReleaseRecordArrIntegrationLegalHold{routePrefix}");

        group.MapGet("/controlled-documents", (RecordArrStore store) => Results.Ok(store.GetControlledDocuments()))
            .WithName($"ListRecordArrIntegrationControlledDocuments{routePrefix}");

        group.MapGet("/controlled-documents/{controlledDocumentId}", (string controlledDocumentId, RecordArrStore store) =>
        {
            var document = store.GetControlledDocument(controlledDocumentId);
            return document is null ? Results.NotFound() : Results.Ok(document);
        }).WithName($"GetRecordArrIntegrationControlledDocument{routePrefix}");

        group.MapPost("/controlled-documents", (CreateControlledDocumentRequest request, RecordArrStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.DocumentClass) || string.IsNullOrWhiteSpace(request.DocumentType) || string.IsNullOrWhiteSpace(request.DocumentSubtype))
            {
                return Results.BadRequest(new { code = "missing_document_classification", message = "Controlled document creation requires document class, type, and subtype." });
            }
            if (string.IsNullOrWhiteSpace(request.OwnerPersonId) || string.IsNullOrWhiteSpace(request.DepartmentOrgUnitId) || string.IsNullOrWhiteSpace(request.StaffarrSiteId))
            {
                return Results.BadRequest(new { code = "missing_controlled_document_ownership", message = "Controlled document creation requires an owner, department, and site." });
            }

            var document = store.CreateControlledDocument(
                request.Title,
                request.Description,
                request.DocumentClass,
                request.DocumentType,
                request.DocumentSubtype,
                request.OwnerPersonId,
                request.DepartmentOrgUnitId,
                request.StaffarrSiteId,
                request.AcknowledgementRequired);
            return Results.Created($"{routePrefix}/controlled-documents/{document.ControlledDocumentId}", document);
        }).WithName($"CreateRecordArrIntegrationControlledDocument{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/versions", (HttpContext context, string controlledDocumentId, CreateControlledDocumentVersionRequest request, RecordArrStore store) =>
        {
            var version = store.CreateDocumentVersion(controlledDocumentId, request.FileName, GetActorPersonId(context), request.ChangeSummary);
            return Results.Created($"{routePrefix}/controlled-documents/{controlledDocumentId}/versions/{version.VersionId}", version);
        }).WithName($"CreateRecordArrIntegrationControlledDocumentVersion{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/reviews", (HttpContext context, string controlledDocumentId, CreateDocumentReviewRequest request, RecordArrStore store) =>
        {
            var review = store.RequestDocumentReview(controlledDocumentId, request.VersionId, request.ReviewType, GetActorPersonId(context), request.ReviewerPersonId, request.DueAt);
            return Results.Created($"{routePrefix}/controlled-documents/{controlledDocumentId}/reviews/{review.DocumentReviewId}", review);
        }).WithName($"CreateRecordArrIntegrationDocumentReview{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/reviews/{reviewId}/complete", (string controlledDocumentId, string reviewId, CompleteDocumentReviewRequest request, RecordArrStore store) =>
        {
            var review = store.CompleteDocumentReview(reviewId, request.Status, request.DecisionReason, request.Comments);
            return Results.Ok(review);
        }).WithName($"CompleteRecordArrIntegrationDocumentReview{routePrefix}");

        group.MapGet("/controlled-documents/{controlledDocumentId}/versions", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentVersions(controlledDocumentId)))
            .WithName($"ListRecordArrIntegrationControlledDocumentVersions{routePrefix}");

        group.MapPost("/controlled-documents/refresh-workflows", (RecordArrStore store) =>
            Results.Ok(store.RefreshControlledDocumentWorkflows()))
            .WithName($"RefreshRecordArrIntegrationControlledDocumentWorkflows{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/versions/{versionId}/promote", (HttpContext context, string controlledDocumentId, string versionId, WorkspaceEndpoints.PromoteControlledDocumentVersionRequest request, RecordArrStore store) =>
        {
            var version = store.PromoteDocumentVersion(controlledDocumentId, versionId, GetActorPersonId(context), request.EffectiveAt);
            return Results.Ok(version);
        }).WithName($"PromoteRecordArrIntegrationControlledDocumentVersion{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/archive", (HttpContext context, string controlledDocumentId, WorkspaceEndpoints.UpdateControlledDocumentStatusRequest request, RecordArrStore store) =>
        {
            var document = store.UpdateControlledDocumentStatus(controlledDocumentId, "archived", GetActorPersonId(context));
            return Results.Ok(document);
        }).WithName($"ArchiveRecordArrIntegrationControlledDocument{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/obsolete", (HttpContext context, string controlledDocumentId, WorkspaceEndpoints.UpdateControlledDocumentStatusRequest request, RecordArrStore store) =>
        {
            var document = store.UpdateControlledDocumentStatus(controlledDocumentId, "obsolete", GetActorPersonId(context));
            return Results.Ok(document);
        }).WithName($"ObsoleteRecordArrIntegrationControlledDocument{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/supersede", (HttpContext context, string controlledDocumentId, WorkspaceEndpoints.SupersedeControlledDocumentRequest request, RecordArrStore store) =>
        {
            var document = store.SupersedeControlledDocument(controlledDocumentId, request.SupersededByDocumentRef, GetActorPersonId(context));
            return Results.Ok(document);
        }).WithName($"SupersedeRecordArrIntegrationControlledDocument{routePrefix}");

        group.MapGet("/controlled-documents/{controlledDocumentId}/reviews", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentReviews(controlledDocumentId)))
            .WithName($"ListRecordArrIntegrationControlledDocumentReviews{routePrefix}");

        group.MapGet("/controlled-documents/{controlledDocumentId}/distributions", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentDistributions(controlledDocumentId)))
            .WithName($"ListRecordArrIntegrationControlledDocumentDistributions{routePrefix}");

        group.MapGet("/controlled-documents/{controlledDocumentId}/acknowledgements", (string controlledDocumentId, RecordArrStore store) =>
            Results.Ok(store.GetDocumentAcknowledgements(controlledDocumentId)))
            .WithName($"ListRecordArrIntegrationControlledDocumentAcknowledgements{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions", (string controlledDocumentId, WorkspaceEndpoints.CreateDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.CreateDocumentDistribution(controlledDocumentId, request.VersionId, request.DistributionType, request.TargetRef);
            return Results.Created($"{routePrefix}/controlled-documents/{controlledDocumentId}/distributions/{distribution.DistributionId}", distribution);
        }).WithName($"CreateRecordArrIntegrationControlledDocumentDistribution{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions/{distributionId}/revoke", (HttpContext context, string controlledDocumentId, string distributionId, WorkspaceEndpoints.RevokeDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.RevokeDocumentDistribution(distributionId, GetActorPersonId(context), request.RevokeReason);
            return Results.Ok(distribution);
        }).WithName($"RevokeRecordArrIntegrationControlledDocumentDistribution{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/distributions/{distributionId}/expire", (HttpContext context, string controlledDocumentId, string distributionId, WorkspaceEndpoints.ExpireDocumentDistributionRequest request, RecordArrStore store) =>
        {
            var distribution = store.ExpireDocumentDistribution(distributionId, GetActorPersonId(context), request.ExpireReason);
            return Results.Ok(distribution);
        }).WithName($"ExpireRecordArrIntegrationControlledDocumentDistribution{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/acknowledgements", (HttpContext context, string controlledDocumentId, WorkspaceEndpoints.CreateDocumentAcknowledgementRequest request, RecordArrStore store) =>
        {
            var acknowledgement = store.CreateDocumentAcknowledgement(controlledDocumentId, request.VersionId, GetActorPersonId(context), request.AttestationText, request.DueAt);
            return Results.Created($"{routePrefix}/controlled-documents/{controlledDocumentId}/acknowledgements/{acknowledgement.AcknowledgementId}", acknowledgement);
        }).WithName($"CreateRecordArrIntegrationControlledDocumentAcknowledgement{routePrefix}");

        group.MapPost("/controlled-documents/{controlledDocumentId}/acknowledgements/{acknowledgementId}/complete", (string controlledDocumentId, string acknowledgementId, WorkspaceEndpoints.CompleteDocumentAcknowledgementRequest request, RecordArrStore store) =>
        {
            var acknowledgement = store.CompleteDocumentAcknowledgement(acknowledgementId, request.SignatureRecordRef);
            return Results.Ok(acknowledgement);
        }).WithName($"CompleteRecordArrIntegrationControlledDocumentAcknowledgement{routePrefix}");

        group.MapPost("/external-shares", (HttpContext context, CreateExternalShareRequest request, RecordArrStore store) =>
        {
            var share = store.CreateExternalShare(request.RecordId, request.RecipientName, request.RecipientEmail, request.SharePurpose, request.AllowedActions, GetActorPersonId(context));
            return Results.Created($"{routePrefix}/external-shares/{share.ExternalShareId}", share);
        }).WithName($"CreateRecordArrIntegrationExternalShare{routePrefix}");

        group.MapPost("/external-shares/{externalShareId}/revoke", (HttpContext context, string externalShareId, RevokeExternalShareRequest request, RecordArrStore store) =>
        {
            var share = store.RevokeExternalShare(externalShareId, GetActorPersonId(context));
            return Results.Ok(share);
        }).WithName($"RevokeRecordArrIntegrationExternalShare{routePrefix}");

        group.MapPost("/external-shares/{externalShareId}/access", (HttpContext context, string externalShareId, WorkspaceEndpoints.RecordExternalShareAccessRequest request, RecordArrStore store) =>
        {
            var share = store.RecordExternalShareAccess(externalShareId, GetActorPersonId(context), request.AccessAction, request.SourceIp, request.UserAgent);
            return Results.Ok(share);
        }).WithName($"AccessRecordArrIntegrationExternalShare{routePrefix}");

        group.MapPost("/external-shares/refresh-statuses", (RecordArrStore store) =>
            Results.Ok(store.RefreshExternalShares()))
            .WithName($"RefreshRecordArrIntegrationExternalShares{routePrefix}");

        group.MapPost("/external-shares/{externalShareId}/expire", (HttpContext context, string externalShareId, WorkspaceEndpoints.ExpireExternalShareRequest request, RecordArrStore store) =>
        {
            var share = store.ExpireExternalShare(externalShareId, GetActorPersonId(context));
            return Results.Ok(share);
        }).WithName($"ExpireRecordArrIntegrationExternalShare{routePrefix}");

        group.MapPost("/redactions", (HttpContext context, CreateRedactionRequest request, RecordArrStore store) =>
        {
            var redaction = store.CreateRedaction(request.SourceRecordId, request.RedactedRecordId, request.RedactionReason, GetActorPersonId(context), request.RedactionRules);
            return Results.Created($"{routePrefix}/redactions/{redaction.RedactionId}", redaction);
        }).WithName($"CreateRecordArrIntegrationRedaction{routePrefix}");

        group.MapGet("/access-policies", (RecordArrStore store) => Results.Ok(store.GetAccessPolicies()))
            .WithName($"ListRecordArrIntegrationAccessPolicies{routePrefix}");

        group.MapPost("/access-policies", (HttpContext context, WorkspaceEndpoints.CreateAccessPolicyRequest request, RecordArrStore store) =>
        {
            var policy = store.CreateAccessPolicy(
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
            return Results.Created($"{routePrefix}/access-policies/{policy.AccessPolicyId}", policy);
        }).WithName($"CreateRecordArrIntegrationAccessPolicy{routePrefix}");

        group.MapPost("/access-policies/{accessPolicyId}/update", (HttpContext context, string accessPolicyId, WorkspaceEndpoints.UpdateAccessPolicyRequest request, RecordArrStore store) =>
        {
            var policy = store.UpdateAccessPolicy(
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
        }).WithName($"UpdateRecordArrIntegrationAccessPolicy{routePrefix}");

        group.MapGet("/access-grants", (RecordArrStore store) => Results.Ok(store.GetAccessGrants()))
            .WithName($"ListRecordArrIntegrationAccessGrants{routePrefix}");

        group.MapPost("/access-grants/refresh-statuses", (RecordArrStore store) =>
            Results.Ok(store.RefreshAccessGrants()))
            .WithName($"RefreshRecordArrIntegrationAccessGrants{routePrefix}");

        group.MapPost("/access-grants", (HttpContext context, WorkspaceEndpoints.CreateAccessGrantRequest request, RecordArrStore store) =>
        {
            var grant = store.CreateAccessGrant(request.RecordId, request.GranteeType, request.GranteeRef, request.Permission, GetActorPersonId(context), request.ExpiresAt);
            return Results.Created($"{routePrefix}/access-grants/{grant.AccessGrantId}", grant);
        }).WithName($"CreateRecordArrIntegrationAccessGrant{routePrefix}");

        group.MapPost("/access-grants/{accessGrantId}/revoke", (HttpContext context, string accessGrantId, WorkspaceEndpoints.RevokeAccessGrantRequest request, RecordArrStore store) =>
        {
            var grant = store.RevokeAccessGrant(accessGrantId, GetActorPersonId(context), request.RevokeReason);
            return Results.Ok(grant);
        }).WithName($"RevokeRecordArrIntegrationAccessGrant{routePrefix}");

        group.MapGet("/external-shares", (RecordArrStore store) => Results.Ok(store.GetExternalShares()))
            .WithName($"ListRecordArrIntegrationExternalShares{routePrefix}");

        group.MapGet("/redactions", (RecordArrStore store) => Results.Ok(store.GetRedactions()))
            .WithName($"ListRecordArrIntegrationRedactions{routePrefix}");

        group.MapGet("/disposal-reviews", (RecordArrStore store) => Results.Ok(store.GetDisposalReviews()))
            .WithName($"ListRecordArrIntegrationDisposalReviews{routePrefix}");

        group.MapPost("/disposal-reviews", (HttpContext context, WorkspaceEndpoints.CreateDisposalReviewRequest request, RecordArrStore store) =>
        {
            var review = store.CreateDisposalReview(request.RecordId, request.RetentionStatusRef, request.ProposedAction, GetActorPersonId(context));
            return Results.Created($"{routePrefix}/disposal-reviews/{review.DisposalReviewId}", review);
        }).WithName($"CreateRecordArrIntegrationDisposalReview{routePrefix}");

        group.MapPost("/disposal-reviews/{disposalReviewId}/complete", (HttpContext context, string disposalReviewId, WorkspaceEndpoints.CompleteDisposalReviewRequest request, RecordArrStore store) =>
        {
            var review = store.CompleteDisposalReview(disposalReviewId, request.Status, GetActorPersonId(context), request.DecisionReason);
            return Results.Ok(review);
        }).WithName($"CompleteRecordArrIntegrationDisposalReview{routePrefix}");

        group.MapGet("/access-logs", (string? recordId, RecordArrStore store) => Results.Ok(store.GetAccessLogs(recordId)))
            .WithName($"ListRecordArrIntegrationAccessLogs{routePrefix}");
    }

    private static string GetActorPersonId(HttpContext context) =>
        context.User.GetPersonId().ToString("D");

    public sealed record CreateControlledDocumentRequest(
        string Title,
        string Description,
        string DocumentClass,
        string DocumentType,
        string DocumentSubtype,
        string OwnerPersonId,
        string DepartmentOrgUnitId,
        string StaffarrSiteId,
        bool AcknowledgementRequired);
    public sealed record CreateControlledDocumentVersionRequest(string FileName, string? ChangeSummary);
    public sealed record CreateDocumentReviewRequest(string VersionId, string ReviewType, string ReviewerPersonId, DateTimeOffset? DueAt);
    public sealed record CompleteDocumentReviewRequest(string Status, string? DecisionReason, string? Comments);
    public sealed record CreateExternalShareRequest(string RecordId, string RecipientName, string RecipientEmail, string SharePurpose, IReadOnlyList<string> AllowedActions);
    public sealed record RevokeExternalShareRequest();
    public sealed record CreateRedactionRequest(string SourceRecordId, string RedactedRecordId, string RedactionReason, string RedactedByPersonId, IReadOnlyList<string> RedactionRules);

    public sealed record SmartImportRetainSourceRequest(
        Guid TenantId,
        Guid ImportBatchId,
        string FileName,
        string ContentType,
        long SizeBytes,
        string Sha256,
        string ContentBase64,
        string DestinationProductHint);

    public sealed record SmartImportRetainSourceResponse(
        string RecordId,
        string FileId,
        string StorageKey,
        string Status);

    private static string ResolveDocumentType(string fileName, string contentType)
    {
        var name = fileName.ToLowerInvariant();
        if (name.Contains("certificate") || name.Contains("cert")) return "certificate";
        if (name.Contains("policy")) return "policy";
        if (name.Contains("procedure") || name.Contains("sop")) return "procedure";
        if (name.Contains("inspection")) return "inspection_form";
        if (name.Contains("training")) return "training_evidence";
        if (name.Contains("quality")) return "quality_evidence";
        if (contentType.Contains("image", StringComparison.OrdinalIgnoreCase)) return "photo_evidence";
        return "other";
    }

    private static string ResolveDocumentClass(string fileName, string contentType)
    {
        var name = fileName.ToLowerInvariant();
        if (name.Contains("policy") || name.Contains("procedure") || name.Contains("sop"))
        {
            return "governance";
        }

        if (name.Contains("certificate") || name.Contains("cert") || name.Contains("permit") || name.Contains("license"))
        {
            return "compliance";
        }

        if (contentType.Contains("image", StringComparison.OrdinalIgnoreCase))
        {
            return "evidence";
        }

        return "document";
    }

    private static string ResolveDocumentSubtype(string fileName, string contentType)
    {
        var name = fileName.ToLowerInvariant();
        if (name.Contains("certificate") || name.Contains("cert")) return "certificate";
        if (name.Contains("insurance")) return "insurance";
        if (name.Contains("registration")) return "registration";
        if (name.Contains("cab_card") || name.Contains("cab-card") || name.Contains("cab card")) return "cab_card";
        if (name.Contains("policy")) return "policy";
        if (name.Contains("procedure") || name.Contains("sop")) return "procedure";
        if (contentType.Contains("image", StringComparison.OrdinalIgnoreCase)) return "photo";
        return "source_file";
    }

    private static void ValidateSmartImportRetainSourceCaller(
        HttpContext context,
        StlServiceTokenValidator tokenValidator,
        Guid tenantId)
    {
        var bearer = ServiceTokenBearerParser.ParseAuthorizationHeader(context.Request.Headers.Authorization);
        var serviceToken = tokenValidator.TryValidate(bearer);
        if (serviceToken is null)
        {
            return;
        }

        tokenValidator.ValidateOrThrow(bearer, new ServiceTokenRequirements
        {
            ExpectedSourceProduct = "nexarr",
            RequiredTargetProduct = "recordarr",
            TenantId = tenantId,
            RequiredActionScope = "platform.smart_import.retain"
        });
    }
}
