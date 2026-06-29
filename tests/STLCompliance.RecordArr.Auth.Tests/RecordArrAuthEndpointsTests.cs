using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecordArr.Api.Data;
using RecordArr.Api.Services;
using RecordArr.Api.Endpoints;
using STLCompliance.Shared.Auth;

namespace STLCompliance.RecordArr.Auth.Tests;

public sealed class RecordArrAuthEndpointsTests : IAsyncLifetime
{
    private static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DemoUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DemoPersonId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private WebApplicationFactory<global::RecordArr.Api.Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        const string signingKey = "test-signing-key-at-least-32-chars-long";
        var dbName = $"RecordArrAuth-{Guid.NewGuid():N}";

        _factory = new WebApplicationFactory<global::RecordArr.Api.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", string.Empty);
            builder.UseSetting("DATABASE_URL", string.Empty);
            builder.UseSetting("Auth:SigningKey", signingKey);
            builder.ConfigureServices(services =>
            {
                RemoveDbContext<RecordArrDbContext>(services);
                services.AddDbContext<RecordArrDbContext>(options => options.UseInMemoryDatabase(dbName));
            });
        });

        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task Session_bootstrap_allows_users_after_non_recordarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"]);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/session", token));
        response.EnsureSuccessStatusCode();

        var session = await ReadJsonObjectAsync(response);
        Assert.Equal("recordarr", session["productKey"]!.GetValue<string>());
        Assert.Contains(
            session["launchableProductKeys"]!.AsArray(),
            item => string.Equals(item?.GetValue<string>(), "recordarr", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            session["launchableProductKeys"]!.AsArray(),
            item => string.Equals(item?.GetValue<string>(), "ledgarr", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Seed_record_rejects_platform_admin_from_other_tenant()
    {
        var token = CreateAccessToken(["nexarr"], tenantRoleKey: "tenant_member", isPlatformAdmin: true);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/workspace/records/rec-bol-001", token));

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Created_record_allows_same_tenant_owner_after_non_recordarr_launch_context()
    {
        var token = CreateAccessToken(["nexarr"]);
        using var createRequest = Authorized(HttpMethod.Post, "/api/v1/workspace/records", token);
        createRequest.Content = JsonContent.Create(new WorkspaceEndpoints.CreateRecordRequest(
            "Driver packet",
            "Seeded by auth regression test.",
            "document",
            "operations",
            "packet",
            "driver_packet",
            "internal",
            "routarr",
            "trip",
            "trip-100",
            "Trip 100",
            DemoPersonId.ToString(),
            "packet.txt",
            "text/plain",
            Convert.ToBase64String("hello recordarr"u8.ToArray())));

        var createResponse = await _client.SendAsync(createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await ReadJsonObjectAsync(createResponse);
        var recordId = created["recordId"]!.GetValue<string>();

        var getResponse = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/workspace/records/{recordId}", token));
        getResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public void Created_record_file_and_context_metadata_survive_store_recreation()
    {
        var dbName = $"recordarr-persistence-{Guid.NewGuid():N}";
        var principal = CreatePrincipal();
        string recordId;
        string fileId;
        string metadataId;
        string linkId;
        string commentId;
        string uploadSessionId;
        string captureRequestId;
        string scanProcessingId;
        string ocrResultId;
        string extractionResultId;
        string generatedPdfFileId;
        string evidenceMappingId;
        string packageId;
        string packageManifestId;
        string packageManifestChecksum;
        string packagePdfFileId;
        string packageZipFileId;
        string retentionStatusId;
        string legalHoldId;
        string disposalReviewId;
        string controlledDocumentId;
        string controlledVersionId;
        string controlledVersionFileId;
        string documentReviewId;
        string documentDistributionId;
        string documentAcknowledgementId;
        string accessPolicyId;
        string accessGrantId;
        string externalShareId;
        string accessLogId;
        string fileIntegrityCheckId;
        string fileMalwareScanId;
        string storageReconciliationId;
        string signatureRecordId;
        string signatureFileId;
        string photoEvidenceId;
        string redactionId;
        string redactedRecordId;

        using (var db = CreateDb(dbName))
        {
            var store = new RecordArrStore(db);
            var record = store.CreateRecord(
                DemoTenantId.ToString(),
                "Durable driver packet",
                "Persists metadata across store recreation.",
                "document",
                "operations",
                "packet",
                "driver_packet",
                "internal",
                "routarr",
                "trip",
                "trip-persist-100",
                "Trip Persist 100",
                DemoPersonId.ToString(),
                DemoPersonId.ToString(),
                "packet.txt",
                "text/plain",
                "recordarr",
                "tenant/packet.txt",
                19);
            recordId = record.RecordId;

            var file = store.CreateFile(
                recordId,
                "packet-v2.txt",
                "text/plain",
                DemoPersonId.ToString(),
                "recordarr",
                "tenant/packet-v2.txt",
                22);
            fileId = file.FileId;

            var integrityCheck = store.CreateFileIntegrityCheck(
                DemoTenantId.ToString(),
                fileId,
                DemoPersonId.ToString(),
                file.ChecksumSha256,
                "metadata_checksum");
            fileIntegrityCheckId = integrityCheck.IntegrityCheckId;

            var malwareScan = store.CreateFileMalwareScan(
                DemoTenantId.ToString(),
                fileId,
                DemoPersonId.ToString(),
                "clean",
                "clamav",
                "1.2.3",
                "sig-2026-06-28");
            fileMalwareScanId = malwareScan.MalwareScanId;

            var storageReconciliation = store.RunStorageReconciliation(
                DemoTenantId.ToString(),
                DemoPersonId.ToString(),
                "durable-restore-test",
                recordId,
                [],
                []);
            storageReconciliationId = storageReconciliation.ReconciliationId;

            var accessPolicy = store.CreateAccessPolicy(
                DemoTenantId.ToString(),
                recordId,
                "restricted",
                "active",
                ["recordarr.records.read"],
                [],
                ["recordarr.files.download"],
                ["recordarr.external_shares.create"],
                [],
                [],
                DemoPersonId.ToString());
            accessPolicyId = accessPolicy.AccessPolicyId;

            var accessGrant = store.CreateAccessGrant(
                DemoTenantId.ToString(),
                recordId,
                "person",
                DemoPersonId.ToString(),
                "download",
                DemoPersonId.ToString(),
                DateTimeOffset.UtcNow.AddDays(10));
            accessGrantId = accessGrant.AccessGrantId;

            var externalShare = store.CreateExternalShare(
                DemoTenantId.ToString(),
                recordId,
                "Route Auditor",
                "auditor@example.com",
                "auditor_access",
                ["view", "download"],
                DemoPersonId.ToString());
            externalShareId = externalShare.ExternalShareId;
            store.RecordExternalShareAccess(
                DemoTenantId.ToString(),
                externalShareId,
                DemoPersonId.ToString(),
                "view",
                "203.0.113.10",
                "RecordArrAuthTests/1.0");

            accessLogId = store.AddAccessLog(
                recordId,
                "view",
                "allowed",
                DemoPersonId.ToString(),
                null,
                externalShareId,
                "203.0.113.10",
                "RecordArrAuthTests/1.0",
                "restart-persistence").AccessLogId;

            var signature = store.CreateSignatureRecord(
                DemoTenantId.ToString(),
                recordId,
                "proof_of_delivery",
                DemoPersonId.ToString(),
                null,
                "Route lead",
                "Signed route packet.",
                DemoPersonId.ToString(),
                "routarr",
                "routarr:trip:trip-persist-100",
                "38.6270,-90.1994",
                "field-device:demo");
            signatureRecordId = signature.SignatureRecordId;
            signatureFileId = signature.SignatureFileRef;

            var photoEvidence = store.CreatePhotoEvidence(
                DemoTenantId.ToString(),
                recordId,
                "delivery",
                DemoPersonId.ToString(),
                "routarr",
                "routarr:trip:trip-persist-100",
                "38.6270,-90.1994",
                "field-device:demo",
                "Trailer seal and packet captured.");
            photoEvidenceId = photoEvidence.PhotoEvidenceId;

            redactedRecordId = "rec-persist-redacted-100";
            var redaction = store.CreateRedaction(
                DemoTenantId.ToString(),
                recordId,
                redactedRecordId,
                "privacy",
                DemoPersonId.ToString(),
                ["mask:signature", "mask:phone"]);
            redactionId = redaction.RedactionId;

            var metadata = store.CreateRecordMetadata(
                recordId,
                "driver_packet_type",
                "inspection_handoff",
                "string",
                "source_product",
                1.0m,
                DemoPersonId.ToString());
            metadataId = metadata.MetadataId;

            var link = store.CreateRecordLink(
                recordId,
                null,
                "routarr:trip:trip-persist-100",
                "source",
                DemoPersonId.ToString());
            linkId = link.RecordLinkId;

            var comment = store.CreateRecordComment(
                recordId,
                "Initial reviewer context.",
                "internal",
                DemoPersonId.ToString());
            commentId = comment.CommentId;

            store.UpdateRecordComment(
                commentId,
                "Edited reviewer context.",
                "internal",
                DemoPersonId.ToString());

            var uploadSession = store.CreateUploadSession(
                DemoTenantId.ToString(),
                "routarr",
                "trip",
                "trip-persist-100",
                "pod",
                requiresDocumentScan: true,
                requiresOcr: true,
                requiresManualReview: true);
            uploadSessionId = uploadSession.UploadSessionId;

            var captureRequest = store.CreateCaptureRequest(
                DemoTenantId.ToString(),
                "routarr",
                "routarr:trip:trip-persist-100",
                "document_scan",
                "Capture driver packet proof",
                "Capture the signed inspection handoff packet.",
                required: true,
                uploadSession.UploadSessionId,
                "compliancecore:evidence:driver-packet");
            captureRequestId = captureRequest.CaptureRequestId;

            store.CompleteUploadSession(DemoTenantId.ToString(), uploadSession.UploadSessionId, recordId);

            var scan = store.CreateScanProcessing(recordId, "driver-packet-scan.jpg", "inspection_form");
            scanProcessingId = scan.ScanProcessingId;
            ocrResultId = scan.OcrResultId!;
            extractionResultId = scan.ExtractionResultId!;
            generatedPdfFileId = scan.GeneratedPdfFileRef!;

            store.ApplyManualCorrection(scanProcessingId, "12,12,540,20,540,720,12,720", DemoPersonId.ToString());
            store.ReviewExtractionResult(extractionResultId, DemoPersonId.ToString(), "completed", null);

            var evidenceMapping = store.CreateEvidenceMapping(
                recordId,
                "routarr",
                "trip",
                "trip-persist-100",
                "compliancecore:req:driver-packet",
                "driver_packet",
                "product_asserted",
                0.88m);
            evidenceMappingId = evidenceMapping.EvidenceMappingId;
            store.UpdateEvidenceMapping(DemoTenantId.ToString(), evidenceMappingId, "confirmed", DemoPersonId.ToString(), "Driver packet verified.", null);

            var package = store.CreatePackage(
                DemoTenantId.ToString(),
                "Driver packet audit package",
                "audit",
                "routarr",
                "routarr:trip:trip-persist-100",
                recordId,
                DemoPersonId.ToString());
            packageId = package.PackageId;
            packageManifestChecksum = package.ManifestChecksum!;
            packagePdfFileId = package.GeneratedPdfRecordRef!;
            packageZipFileId = package.GeneratedZipFileRef!;
            packageManifestId = store.GetManifest(DemoTenantId.ToString(), packageId)!.ManifestId;

            store.LockPackage(DemoTenantId.ToString(), packageId);
            store.ArchivePackage(DemoTenantId.ToString(), packageId);

            var retentionStatus = store.RecalculateRetentionStatuses(DemoTenantId.ToString())
                .Single(status => status.RecordId == recordId);
            retentionStatusId = retentionStatus.RetentionStatusId;

            var legalHold = store.CreateLegalHold(
                DemoTenantId.ToString(),
                "Driver packet evidence hold",
                "Preserve driver packet while route audit is open.",
                "audit",
                "routarr",
                "trip",
                "trip-persist-100",
                DemoPersonId.ToString(),
                ["source_object:routarr:trip:trip-persist-100"],
                [recordId]);
            legalHoldId = legalHold.LegalHoldId;
            store.ActivateLegalHold(DemoTenantId.ToString(), legalHoldId);

            var disposalReview = store.CreateDisposalReview(
                DemoTenantId.ToString(),
                recordId,
                retentionStatusId,
                "archive",
                DemoPersonId.ToString());
            disposalReviewId = disposalReview.DisposalReviewId;
            store.CompleteDisposalReview(
                DemoTenantId.ToString(),
                disposalReviewId,
                "approved",
                DemoPersonId.ToString(),
                "Archive approved when legal hold clears.");

            var blockedRetention = store.GetRetentionStatus(DemoTenantId.ToString(), recordId);
            Assert.Equal("blocked_by_legal_hold", blockedRetention?.Status);
            store.ReleaseLegalHold(DemoTenantId.ToString(), legalHoldId, DemoPersonId.ToString(), "Route audit complete.");

            var controlledDocument = store.CreateControlledDocument(
                DemoTenantId.ToString(),
                "Driver packet controlled procedure",
                "Controlled procedure for driver packet evidence handling.",
                "procedure",
                "operations",
                "driver_packet",
                DemoPersonId.ToString(),
                "org-operations",
                "site-north-yard",
                acknowledgementRequired: true);
            controlledDocumentId = controlledDocument.ControlledDocumentId;

            var controlledVersion = store.CreateDocumentVersion(
                DemoTenantId.ToString(),
                controlledDocumentId,
                "driver-packet-procedure.pdf",
                DemoPersonId.ToString(),
                "Initial controlled version.");
            controlledVersionId = controlledVersion.VersionId;
            controlledVersionFileId = controlledVersion.FileRef!;

            var review = store.RequestDocumentReview(
                DemoTenantId.ToString(),
                controlledDocumentId,
                controlledVersionId,
                "approval",
                DemoPersonId.ToString(),
                DemoPersonId.ToString(),
                DateTimeOffset.UtcNow.AddDays(5));
            documentReviewId = review.DocumentReviewId;

            store.CompleteDocumentReview(
                DemoTenantId.ToString(),
                documentReviewId,
                "approved",
                "Approved for route audit.",
                "Ready.");
            store.PromoteDocumentVersion(
                DemoTenantId.ToString(),
                controlledDocumentId,
                controlledVersionId,
                DemoPersonId.ToString(),
                DateTimeOffset.UtcNow);

            var distribution = store.CreateDocumentDistribution(
                DemoTenantId.ToString(),
                controlledDocumentId,
                controlledVersionId,
                "person",
                DemoPersonId.ToString());
            documentDistributionId = distribution.DistributionId;

            var acknowledgement = store.CreateDocumentAcknowledgement(
                DemoTenantId.ToString(),
                controlledDocumentId,
                controlledVersionId,
                DemoPersonId.ToString(),
                "I acknowledge this controlled procedure.",
                DateTimeOffset.UtcNow.AddDays(3));
            documentAcknowledgementId = acknowledgement.AcknowledgementId;
            store.CompleteDocumentAcknowledgement(DemoTenantId.ToString(), documentAcknowledgementId, null);
        }

        using (var db = CreateDb(dbName))
        {
            var recreatedStore = new RecordArrStore(db);
            var persisted = recreatedStore.GetRecord(principal, recordId);
            Assert.NotNull(persisted);
            Assert.Equal("Durable driver packet", persisted!.Title);
            Assert.Equal(4, persisted.VersionNumber);
            Assert.Equal(controlledVersionFileId, persisted.CurrentFileRef);

            var files = recreatedStore.GetFiles(principal, recordId);
            Assert.Contains(files, file => file.FileId == fileId && file.StorageKey == "tenant/packet-v2.txt");

            var accessPolicies = recreatedStore.GetAccessPolicies(DemoTenantId.ToString());
            Assert.Contains(accessPolicies, item =>
                item.AccessPolicyId == accessPolicyId &&
                item.RecordId == recordId &&
                item.PolicyType == "restricted" &&
                item.ReadRules.Contains("recordarr.records.read") &&
                item.DownloadRules.Contains("recordarr.files.download"));
            Assert.DoesNotContain(
                recreatedStore.GetAccessPolicies(Guid.NewGuid().ToString()),
                item => item.AccessPolicyId == accessPolicyId);

            var accessGrants = recreatedStore.GetAccessGrants(DemoTenantId.ToString());
            Assert.Contains(accessGrants, item =>
                item.AccessGrantId == accessGrantId &&
                item.RecordId == recordId &&
                item.GranteeType == "person" &&
                item.GranteeRef == DemoPersonId.ToString() &&
                item.Permission == "download" &&
                item.Status == "active");
            Assert.DoesNotContain(
                recreatedStore.GetAccessGrants(Guid.NewGuid().ToString()),
                item => item.AccessGrantId == accessGrantId);

            var externalShares = recreatedStore.GetExternalShares(DemoTenantId.ToString());
            Assert.Contains(externalShares, item =>
                item.ExternalShareId == externalShareId &&
                item.RecordId == recordId &&
                item.SharePurpose == "auditor_access" &&
                item.Status == "active" &&
                item.AccessCount == 1 &&
                item.LastAccessedAt is not null);
            Assert.DoesNotContain(
                recreatedStore.GetExternalShares(Guid.NewGuid().ToString()),
                item => item.ExternalShareId == externalShareId);

            var accessLogs = recreatedStore.GetAccessLogs(DemoTenantId.ToString(), recordId);
            Assert.Contains(accessLogs, item =>
                item.AccessLogId == accessLogId &&
                item.RecordId == recordId &&
                item.Action == "view" &&
                item.ExternalShareId == externalShareId &&
                item.ReasonCode == "restart-persistence");
            Assert.DoesNotContain(
                recreatedStore.GetAccessLogs(Guid.NewGuid().ToString(), recordId),
                item => item.AccessLogId == accessLogId);

            var auditEvents = recreatedStore.GetAuditEvents(DemoTenantId.ToString(), recordId);
            Assert.Contains(auditEvents, item =>
                item.RecordId == recordId &&
                item.Action == "view" &&
                item.Outcome == "allowed" &&
                item.CorrelationId == accessLogId &&
                !string.IsNullOrWhiteSpace(item.EventHash));
            Assert.Contains(auditEvents, item =>
                item.Action == "file.integrity_check" &&
                item.RecordId == recordId &&
                item.ReasonCode == "passed" &&
                !string.IsNullOrWhiteSpace(item.EventHash));
            Assert.Contains(auditEvents, item =>
                item.Action == "file.malware_scan" &&
                item.RecordId == recordId &&
                item.ReasonCode == "clean" &&
                !string.IsNullOrWhiteSpace(item.EventHash));
            Assert.DoesNotContain(
                recreatedStore.GetAuditEvents(Guid.NewGuid().ToString(), recordId),
                item => item.CorrelationId == accessLogId);

            Assert.Contains(accessLogs, item =>
                item.Action == "file.integrity_check" &&
                item.RecordId == recordId &&
                item.ReasonCode == "passed");

            var integrityChecks = recreatedStore.GetFileIntegrityChecks(DemoTenantId.ToString(), fileId);
            Assert.Contains(integrityChecks, item =>
                item.IntegrityCheckId == fileIntegrityCheckId &&
                item.FileId == fileId &&
                item.RecordId == recordId &&
                item.Status == "passed" &&
                item.CheckMethod == "metadata_checksum");
            Assert.DoesNotContain(
                recreatedStore.GetFileIntegrityChecks(Guid.NewGuid().ToString(), fileId),
                item => item.IntegrityCheckId == fileIntegrityCheckId);

            var malwareScans = recreatedStore.GetFileMalwareScans(DemoTenantId.ToString(), fileId);
            Assert.Contains(malwareScans, item =>
                item.MalwareScanId == fileMalwareScanId &&
                item.FileId == fileId &&
                item.RecordId == recordId &&
                item.Status == "clean" &&
                item.QuarantineStatus == "released" &&
                item.ScannerName == "clamav");
            Assert.DoesNotContain(
                recreatedStore.GetFileMalwareScans(Guid.NewGuid().ToString(), fileId),
                item => item.MalwareScanId == fileMalwareScanId);

            var storageReconciliations = recreatedStore.GetStorageReconciliations(DemoTenantId.ToString());
            Assert.Contains(storageReconciliations, item =>
                item.ReconciliationId == storageReconciliationId &&
                item.Scope == "durable-restore-test" &&
                item.TotalFiles > 0 &&
                item.CheckedFiles > 0);
            Assert.DoesNotContain(
                recreatedStore.GetStorageReconciliations(Guid.NewGuid().ToString()),
                item => item.ReconciliationId == storageReconciliationId);

            var signatures = recreatedStore.GetSignatureRecords(DemoTenantId.ToString(), recordId);
            Assert.Contains(signatures, item =>
                item.SignatureRecordId == signatureRecordId &&
                item.SignatureFileRef == signatureFileId &&
                item.SignaturePurpose == "proof_of_delivery" &&
                item.SourceObjectRef == "routarr:trip:trip-persist-100");
            Assert.DoesNotContain(
                recreatedStore.GetSignatureRecords(Guid.NewGuid().ToString(), recordId),
                item => item.SignatureRecordId == signatureRecordId);

            var photos = recreatedStore.GetPhotoEvidence(DemoTenantId.ToString(), recordId);
            Assert.Contains(photos, item =>
                item.PhotoEvidenceId == photoEvidenceId &&
                item.PhotoPurpose == "delivery" &&
                item.Notes == "Trailer seal and packet captured.");
            Assert.DoesNotContain(
                recreatedStore.GetPhotoEvidence(Guid.NewGuid().ToString(), recordId),
                item => item.PhotoEvidenceId == photoEvidenceId);

            var redactions = recreatedStore.GetRedactions(DemoTenantId.ToString());
            Assert.Contains(redactions, item =>
                item.RedactionId == redactionId &&
                item.SourceRecordId == recordId &&
                item.RedactedRecordId == redactedRecordId &&
                item.RedactionReason == "privacy" &&
                item.Status == "completed" &&
                item.RedactionRules.Contains("mask:signature"));
            Assert.DoesNotContain(
                recreatedStore.GetRedactions(Guid.NewGuid().ToString()),
                item => item.RedactionId == redactionId);

            var redactedRecord = recreatedStore.GetRecord(principal, redactedRecordId);
            Assert.NotNull(redactedRecord);
            Assert.Contains("redacted", redactedRecord!.Tags);

            var metadata = recreatedStore.GetRecordMetadata(recordId);
            Assert.Contains(metadata, item => item.MetadataId == metadataId && item.Key == "driver_packet_type");

            var links = recreatedStore.GetRecordLinks(recordId);
            Assert.Contains(links, item => item.RecordLinkId == linkId && item.SourceObjectRef == "routarr:trip:trip-persist-100");

            var comments = recreatedStore.GetRecordComments(recordId);
            Assert.Contains(comments, item =>
                item.CommentId == commentId &&
                item.Body == "Edited reviewer context." &&
                item.EditedByPersonId == DemoPersonId.ToString());

            var uploadSessions = recreatedStore.GetUploadSessions(DemoTenantId.ToString());
            Assert.Contains(uploadSessions, item =>
                item.UploadSessionId == uploadSessionId &&
                item.TenantId == DemoTenantId.ToString() &&
                item.Status == "completed" &&
                item.UploadedRecordRefs.Contains(recordId));

            var hiddenUploadSessions = recreatedStore.GetUploadSessions(Guid.NewGuid().ToString());
            Assert.DoesNotContain(hiddenUploadSessions, item => item.UploadSessionId == uploadSessionId);

            var captureRequests = recreatedStore.GetCaptureRequests(DemoTenantId.ToString());
            Assert.Contains(captureRequests, item =>
                item.CaptureRequestId == captureRequestId &&
                item.UploadSessionRef == uploadSessionId &&
                item.Status == "completed" &&
                item.CompletedAt is not null);

            var scans = recreatedStore.GetScanProcessing();
            Assert.Contains(scans, item =>
                item.ScanProcessingId == scanProcessingId &&
                item.Status == "manually_corrected" &&
                item.ManualEdgeCoordinates == "12,12,540,20,540,720,12,720" &&
                item.OcrResultId == ocrResultId &&
                item.ExtractionResultId == extractionResultId);

            var generatedFiles = recreatedStore.GetFiles(principal, recordId);
            Assert.Contains(generatedFiles, item => item.FileId == generatedPdfFileId && item.StorageProvider == "generated");
            Assert.Contains(generatedFiles, item => item.FileId == controlledVersionFileId && item.StorageProvider == "generated");

            var ocrResult = recreatedStore.GetOcrResult(ocrResultId);
            Assert.NotNull(ocrResult);
            Assert.Equal(recordId, ocrResult!.RecordId);

            var extractionResult = recreatedStore.GetExtractionResult(extractionResultId);
            Assert.NotNull(extractionResult);
            Assert.Equal("completed", extractionResult!.Status);
            Assert.Equal(DemoPersonId.ToString(), extractionResult.ReviewedByPersonId);
            Assert.All(extractionResult.ExtractedFields, field => Assert.Equal("accepted", field.ReviewStatus));

            var evidenceMappings = recreatedStore.GetEvidenceMappings(DemoTenantId.ToString());
            Assert.Contains(evidenceMappings, item =>
                item.EvidenceMappingId == evidenceMappingId &&
                item.Status == "confirmed" &&
                item.ConfirmedByPersonId == DemoPersonId.ToString() &&
                item.Notes == "Driver packet verified.");

            var hiddenEvidenceMappings = recreatedStore.GetEvidenceMappings(Guid.NewGuid().ToString());
            Assert.DoesNotContain(hiddenEvidenceMappings, item => item.EvidenceMappingId == evidenceMappingId);

            var coverage = recreatedStore.GetEvidenceCoverage(DemoTenantId.ToString());
            Assert.Contains(coverage, item =>
                item.ComplianceCoreRequirementRef == "compliancecore:req:driver-packet" &&
                item.Status == "satisfied" &&
                item.RecordRefs.Contains(recordId));

            var packages = recreatedStore.GetPackages(DemoTenantId.ToString());
            Assert.Contains(packages, item =>
                item.PackageId == packageId &&
                item.Status == "archived" &&
                item.ManifestChecksum == packageManifestChecksum &&
                item.GeneratedPdfRecordRef == packagePdfFileId &&
                item.GeneratedZipFileRef == packageZipFileId &&
                item.RecordRefs.Contains(recordId));

            var hiddenPackages = recreatedStore.GetPackages(Guid.NewGuid().ToString());
            Assert.DoesNotContain(hiddenPackages, item => item.PackageId == packageId);

            var packageManifest = recreatedStore.GetManifest(DemoTenantId.ToString(), packageId);
            Assert.NotNull(packageManifest);
            Assert.Equal(packageManifestId, packageManifest!.ManifestId);
            Assert.Equal(packageManifestChecksum, packageManifest.Checksum);
            Assert.Contains(packageManifest.RecordEntries, item => item.RecordRef == recordId);
            Assert.Contains(packageManifest.RequirementEntries, item => item.ComplianceRequirementRef == "compliancecore:req:driver-packet");
            Assert.Null(recreatedStore.GetManifest(Guid.NewGuid().ToString(), packageId));

            var packageFiles = recreatedStore.GetFiles(principal, recordId);
            Assert.Contains(packageFiles, item => item.FileId == packagePdfFileId && item.StorageProvider == "generated");
            Assert.Contains(packageFiles, item => item.FileId == packageZipFileId && item.StorageProvider == "generated");

            var retentionStatus = recreatedStore.GetRetentionStatus(DemoTenantId.ToString(), recordId);
            Assert.NotNull(retentionStatus);
            Assert.Equal(retentionStatusId, retentionStatus!.RetentionStatusId);
            Assert.Equal("active", retentionStatus.Status);
            Assert.Equal(disposalReviewId, retentionStatus.DisposalReviewRef);
            Assert.Null(recreatedStore.GetRetentionStatus(Guid.NewGuid().ToString(), recordId));

            var legalHolds = recreatedStore.GetLegalHolds(DemoTenantId.ToString());
            Assert.Contains(legalHolds, item =>
                item.LegalHoldId == legalHoldId &&
                item.Status == "released" &&
                item.RecordRefs.Contains(recordId) &&
                item.ReleasedByPersonId == DemoPersonId.ToString());

            var hiddenLegalHolds = recreatedStore.GetLegalHolds(Guid.NewGuid().ToString());
            Assert.DoesNotContain(hiddenLegalHolds, item => item.LegalHoldId == legalHoldId);

            var disposalReviews = recreatedStore.GetDisposalReviews(DemoTenantId.ToString());
            Assert.Contains(disposalReviews, item =>
                item.DisposalReviewId == disposalReviewId &&
                item.RecordId == recordId &&
                item.RetentionStatusRef == retentionStatusId &&
                item.Status == "approved" &&
                item.DecisionReason == "Archive approved when legal hold clears.");

            var hiddenDisposalReviews = recreatedStore.GetDisposalReviews(Guid.NewGuid().ToString());
            Assert.DoesNotContain(hiddenDisposalReviews, item => item.DisposalReviewId == disposalReviewId);

            var controlledDocuments = recreatedStore.GetControlledDocuments(DemoTenantId.ToString());
            Assert.Contains(controlledDocuments, item =>
                item.ControlledDocumentId == controlledDocumentId &&
                item.Status == "effective" &&
                item.CurrentVersionId == controlledVersionId &&
                item.AcknowledgementRequired);
            Assert.DoesNotContain(
                recreatedStore.GetControlledDocuments(Guid.NewGuid().ToString()),
                item => item.ControlledDocumentId == controlledDocumentId);

            var controlledVersions = recreatedStore.GetDocumentVersions(DemoTenantId.ToString(), controlledDocumentId);
            Assert.Contains(controlledVersions, item =>
                item.VersionId == controlledVersionId &&
                item.Status == "effective" &&
                item.FileRef is not null &&
                item.VersionNumber == 1);
            Assert.Empty(recreatedStore.GetDocumentVersions(Guid.NewGuid().ToString(), controlledDocumentId));

            var documentReviews = recreatedStore.GetDocumentReviews(DemoTenantId.ToString(), controlledDocumentId);
            Assert.Contains(documentReviews, item =>
                item.DocumentReviewId == documentReviewId &&
                item.Status == "approved" &&
                item.DecisionReason == "Approved for route audit.");
            Assert.Empty(recreatedStore.GetDocumentReviews(Guid.NewGuid().ToString(), controlledDocumentId));

            var documentDistributions = recreatedStore.GetDocumentDistributions(DemoTenantId.ToString(), controlledDocumentId);
            Assert.Contains(documentDistributions, item =>
                item.DistributionId == documentDistributionId &&
                item.Status == "acknowledged" &&
                item.AcknowledgementRef == documentAcknowledgementId);
            Assert.Empty(recreatedStore.GetDocumentDistributions(Guid.NewGuid().ToString(), controlledDocumentId));

            var documentAcknowledgements = recreatedStore.GetDocumentAcknowledgements(DemoTenantId.ToString(), controlledDocumentId);
            Assert.Contains(documentAcknowledgements, item =>
                item.AcknowledgementId == documentAcknowledgementId &&
                item.Status == "acknowledged" &&
                item.SignatureRecordRef is null &&
                item.AcknowledgedAt is not null);
            Assert.Empty(recreatedStore.GetDocumentAcknowledgements(Guid.NewGuid().ToString(), controlledDocumentId));
        }
    }

    private string CreateAccessToken(IReadOnlyList<string> launchableProductKeys, string tenantRoleKey = "tenant_member", bool isPlatformAdmin = false)
    {
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<RecordArrTokenService>();
        var (accessToken, _) = tokenService.CreateAccessToken(
            DemoUserId,
            DemoPersonId,
            "recordarr.user@demo.stl",
            "RecordArr User",
            DemoTenantId,
            Guid.NewGuid(),
            tenantRoleKey,
            launchableProductKeys,
            isPlatformAdmin);
        return accessToken;
    }

    private static RecordArrDbContext CreateDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new RecordArrDbContext(options);
    }

    private static ClaimsPrincipal CreatePrincipal()
    {
        var claims = new List<Claim>
        {
            new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, DemoUserId.ToString()),
            new(StlClaimTypes.TenantId, DemoTenantId.ToString()),
            new(StlClaimTypes.SessionId, Guid.NewGuid().ToString()),
            new(StlClaimTypes.TenantRoleKey, "tenant_member"),
            new(StlClaimTypes.PlatformAdmin, "false"),
            new(StlClaimTypes.PersonId, DemoPersonId.ToString()),
            new(StlClaimTypes.LaunchableProductKeys, "recordarr"),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>) || d.ServiceType == typeof(TContext))
            .ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string token)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task<JsonObject> ReadJsonObjectAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?.AsObject()
            ?? throw new InvalidOperationException("Expected a JSON object response.");
    }
}
