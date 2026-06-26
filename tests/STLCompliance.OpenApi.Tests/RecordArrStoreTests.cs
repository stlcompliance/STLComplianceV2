using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RecordArr.Api.Data;
using STLCompliance.Shared.Auth;

namespace STLCompliance.OpenApi.Tests;

public sealed class RecordArrStoreTests
{
    private const string DefaultTenantId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

    [Fact]
    public void CreateFile_attaches_file_to_record_and_updates_current_file_ref()
    {
        var store = new RecordArrStore();
        var principal = CreatePrincipal();

        var file = store.CreateFile(
            "rec-bol-001",
            "new-bol.pdf",
            "application/pdf",
            "person-route-lead");

        var record = store.GetRecord(principal, "rec-bol-001");

        Assert.NotNull(record);
        Assert.Equal(file.FileId, record!.CurrentFileRef);
        Assert.Contains(file.FileId, record.FileRefs);
        Assert.Equal(record.CurrentFileRef, record.CurrentVersionRef);
        Assert.Contains("routarr:trip:trip-7781", record.SourceObjectRefs);
        Assert.Contains(file.FileId, record.VersionRefs);
        Assert.NotEmpty(record.AuditTrail);
        Assert.Equal("new-bol.pdf", record.CurrentFileName);
        Assert.Equal("application/pdf", record.CurrentMimeType);
    }

    [Fact]
    public void CreateRecord_can_attach_a_single_initial_file_without_placeholder_duplication()
    {
        var store = new RecordArrStore();
        var principal = CreatePrincipal(isPlatformAdmin: true);
        var storageKey = "recordarr/smart-import/tenant/batch/hash/source.pdf";

        var record = store.CreateRecord(
            DefaultTenantId,
            "Smart Import source: source.pdf",
            "Source file retained for import review.",
            "document",
            "other",
            "import_source",
            "uploaded",
            "internal",
            "nexarr",
            "smart_import_batch",
            "batch-001",
            "source.pdf",
            "person-importer",
            "person-importer",
            "source.pdf",
            "application/pdf",
            "recordarr",
            storageKey,
            4096);

        var files = store.GetFiles(principal, record.RecordId);
        var file = Assert.Single(files);

        Assert.Equal(record.CurrentFileRef, file.FileId);
        Assert.Equal(storageKey, file.StorageKey);
        Assert.Equal("recordarr", file.StorageProvider);
        Assert.Equal(4096, file.SizeBytes);
        Assert.Equal(1, record.VersionNumber);
        Assert.Single(record.FileRefs);
        Assert.Single(record.VersionRefs);
    }

    [Fact]
    public void CreateScanProcessing_creates_original_and_generated_files()
    {
        var store = new RecordArrStore();
        var principal = CreatePrincipal();

        var scan = store.CreateScanProcessing("rec-bol-001", "captured-bol.jpg", "bol");

        Assert.NotNull(scan.OriginalFileRef);
        Assert.NotNull(scan.GeneratedPdfFileRef);
        Assert.Equal("completed", scan.Status);
        Assert.NotNull(scan.OcrResultId);
        Assert.NotNull(scan.ExtractionResultId);
        Assert.NotNull(scan.EdgeDetectionResult);
        Assert.NotNull(scan.EnhancementSettings);
        Assert.Equal("detected", scan.EdgeDetectionResult!.Status);
        Assert.Equal("pdf", scan.EnhancementSettings!.OutputFormat);
        Assert.Null(scan.ManualEdgeCoordinates);
        Assert.Null(scan.CorrectedAt);
        Assert.Null(scan.CorrectedByPersonId);

        var ocr = store.GetOcrResult(scan.OcrResultId!);
        Assert.NotNull(ocr);
        Assert.NotEmpty(ocr!.PageResults);
        Assert.NotEmpty(ocr.BlockResults);

        var extraction = store.GetExtractionResult(scan.ExtractionResultId!);
        Assert.NotNull(extraction);
        Assert.All(extraction!.ExtractedFields, field => Assert.NotNull(field.BoundingBox));

        var files = store.GetFiles(principal, "rec-bol-001");
        Assert.Contains(files, file => file.FileId == scan.OriginalFileRef);
        Assert.Contains(files, file => file.FileId == scan.GeneratedPdfFileRef);
    }

    [Fact]
    public void Manual_correction_records_audit_fields()
    {
        var store = new RecordArrStore();

        var scan = store.CreateScanProcessing("rec-bol-001", "captured-bol.jpg", "bol");
        var corrected = store.ApplyManualCorrection(scan.ScanProcessingId, "12,12,532,24,532,718,12,718", "person-route-lead");

        Assert.Equal("manually_corrected", corrected.Status);
        Assert.Equal("12,12,532,24,532,718,12,718", corrected.ManualEdgeCoordinates);
        Assert.Equal("person-route-lead", corrected.CorrectedByPersonId);
        Assert.NotNull(corrected.CorrectedAt);
    }

    [Fact]
    public void Capture_requests_can_be_created_and_completed_from_upload_sessions()
    {
        var store = new RecordArrStore();

        var request = store.CreateCaptureRequest(
            DefaultTenantId,
            "routarr",
            "routarr:trip:trip-9000",
            "photo",
            "Dock photo",
            "Capture the load dock before departure.",
            true,
            "upl-900",
            "evidence_requirement.trip.pod");

        var completed = store.CompleteCaptureRequest(request.CaptureRequestId);
        var linked = store.GetCaptureRequests().First(entry => entry.CaptureRequestId == request.CaptureRequestId);

        Assert.Equal("completed", completed.Status);
        Assert.NotNull(completed.CompletedAt);
        Assert.Equal("photo", completed.CaptureType);
        Assert.Equal("completed", linked.Status);
    }

    [Fact]
    public void CreateSignatureAndPhotoEvidence_create_file_backed_evidence_records()
    {
        var store = new RecordArrStore();
        var principal = CreatePrincipal();

        var signature = store.CreateSignatureRecord(
            "rec-bol-001",
            "proof_of_delivery",
            "person-route-lead",
            "Avery Auditor",
            "Driver",
            "Signed on delivery.",
            "person-route-lead",
            "routarr",
            "trip-7781");

        var photo = store.CreatePhotoEvidence(
            "rec-bol-001",
            "delivery",
            "person-route-lead",
            "routarr",
            "trip-7781",
            notes: "Dock photo.");

        Assert.Equal("proof_of_delivery", signature.SignaturePurpose);
        Assert.False(string.IsNullOrWhiteSpace(signature.SignatureFileRef));
        Assert.Equal("delivery", photo.PhotoPurpose);
        Assert.Contains(store.GetFiles(principal, "rec-bol-001"), file => file.FileId == signature.SignatureFileRef);
    }

    [Fact]
    public void PurgeRecord_marks_file_objects_as_deleted()
    {
        var store = new RecordArrStore();
        var principal = CreatePrincipal(isPlatformAdmin: true);

        var file = store.CreateFile(
            "rec-bol-001",
            "purge-me.pdf",
            "application/pdf",
            "person-route-lead");

        store.PurgeRecord("rec-bol-001", "person-record-admin");

        var purgedFile = store.GetFile(principal, file.FileId);
        Assert.NotNull(purgedFile);
        Assert.NotNull(purgedFile!.DeletedAt);
        Assert.Equal("purge", purgedFile.DeleteReason);
    }

    [Fact]
    public void Access_policy_filters_records_for_authenticated_principal()
    {
        var store = new RecordArrStore();
        var principal = CreatePrincipal(personId: "person-doc-controller", tenantRoleKey: "evidence-manager");

        var records = store.GetRecords(principal);

        Assert.NotEmpty(records);
        Assert.Contains(records, record => record.RecordId == "rec-bol-001");
    }

    [Fact]
    public void Invalid_access_policy_values_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateAccessPolicy(
                "rec-bol-001",
                "unknown-policy",
                "active",
                [],
                [],
                [],
                [],
                [],
                [],
                "person-doc-controller"));
    }

    [Fact]
    public void Invalid_external_share_and_redaction_values_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateExternalShare(
                "rec-bol-001",
                "Recipient",
                "recipient@example.com",
                "not-a-purpose",
                ["view"],
                "person-doc-controller"));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateRedaction(
                "rec-bol-001",
                "rec-redacted-001",
                "not-a-reason",
                "person-doc-controller",
                []));
    }

    [Fact]
    public void Redaction_creates_a_redacted_copy_and_link()
    {
        var store = new RecordArrStore();
        var principal = CreatePrincipal(personId: "person-doc-controller");

        var redaction = store.CreateRedaction(
            "rec-bol-001",
            "rec-bol-001-redacted",
            "privacy",
            "person-doc-controller",
            ["mask:signature", "mask:phone"]);

        var redactedRecord = store.GetRecord(principal, "rec-bol-001-redacted");
        var sourceLinks = store.GetRecordLinks("rec-bol-001-redacted");

        Assert.NotNull(redactedRecord);
        Assert.Equal("completed", redaction.Status);
        Assert.Equal("rec-bol-001", redaction.SourceRecordId);
        Assert.Equal("rec-bol-001-redacted", redaction.RedactedRecordId);
        Assert.Equal("active", redactedRecord!.Status);
        Assert.Equal("rec-bol-001-redacted", redactedRecord.RecordId);
        Assert.EndsWith("-redacted.pdf", redactedRecord.CurrentFileName, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("redacted", redactedRecord.Tags);
        Assert.Contains(sourceLinks, link => link.LinkType == "redacted_from" && link.LinkedRecordId == "rec-bol-001");
    }

    [Fact]
    public void Invalid_access_grant_values_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateAccessGrant(
                "rec-bol-001",
                "not-a-grantee",
                "role:auditor",
                "read",
                "person-doc-controller",
                null));
    }

    [Fact]
    public void Product_access_grant_matches_service_source_product_without_user_launch_context()
    {
        var store = new RecordArrStore();
        var record = store.CreateRecord(
            DefaultTenantId,
            "Service-access record",
            "Validates product access grants for product services.",
            "document",
            "other",
            "operations",
            "standard",
            "internal",
            "routarr",
            "trip",
            "trip-service-001",
            "RT-SVC-001",
            "person-record-owner",
            "person-record-owner",
            "service-access.pdf",
            "application/pdf");

        store.CreateAccessPolicy(
            record.RecordId,
            "product_scoped",
            "active",
            [],
            [],
            [],
            [],
            [],
            [],
            "person-record-admin");
        store.CreateAccessGrant(
            record.RecordId,
            "product",
            "routarr",
            "read",
            "person-record-admin",
            null);

        var recordView = store.GetRecord(CreateServicePrincipal("routarr"), record.RecordId);

        Assert.NotNull(recordView);
        Assert.Equal(record.RecordId, recordView!.RecordId);
    }

    [Fact]
    public void Product_access_grant_does_not_match_user_launch_access()
    {
        var store = new RecordArrStore();
        var record = store.CreateRecord(
            DefaultTenantId,
            "User-launch record",
            "Validates that product grants do not behave like launch entitlements.",
            "document",
            "other",
            "operations",
            "standard",
            "internal",
            "routarr",
            "trip",
            "trip-user-001",
            "RT-USR-001",
            "person-record-owner",
            "person-record-owner",
            "user-launch.pdf",
            "application/pdf");

        store.CreateAccessPolicy(
            record.RecordId,
            "product_scoped",
            "active",
            [],
            [],
            [],
            [],
            [],
            [],
            "person-record-admin");
        store.CreateAccessGrant(
            record.RecordId,
            "product",
            "routarr",
            "read",
            "person-record-admin",
            null);

        var recordView = store.GetRecord(
            CreatePrincipal(
                personId: "person-ordinary-user",
                tenantRoleKey: "operations",
                isPlatformAdmin: false,
                "routarr"),
            record.RecordId);

        Assert.Null(recordView);
    }

    [Fact]
    public void Invalid_record_type_and_document_type_values_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateRecord(
                DefaultTenantId,
                "Bad record",
                "Invalid record type.",
                "not-a-record-type",
                "bol",
                "shipping",
                "standard",
                "internal",
                "routarr",
                "trip",
                "trip-7781",
                "RT-7781",
                "person-route-lead",
                "person-route-lead",
                "bad.pdf",
                "application/pdf"));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateRecord(
                DefaultTenantId,
                "Bad record",
                "Invalid document type.",
                "document",
                "not-a-document-type",
                "shipping",
                "standard",
                "internal",
                "routarr",
                "trip",
                "trip-7781",
                "RT-7781",
                "person-route-lead",
                "person-route-lead",
                "bad.pdf",
                "application/pdf"));
    }

    [Fact]
    public void Invalid_capture_request_values_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateCaptureRequest(
                DefaultTenantId,
                "routarr",
                "routarr:trip:trip-7781",
                "not-a-capture-type",
                "Bad request",
                "Invalid capture type.",
                true,
                null,
                null));
    }

    [Fact]
    public void Archive_and_purge_record_stamp_lifecycle_timestamps()
    {
        var store = new RecordArrStore();

        var record = store.CreateRecord(
            DefaultTenantId,
            "Lifecycle record",
            "Testing archive and purge timestamps.",
            "document",
            "procedure",
            "operations",
            "standard",
            "internal",
            "routarr",
            "trip",
            "trip-7781",
            "RT-7781",
            "person-route-lead",
            "person-route-lead",
            "lifecycle.pdf",
            "application/pdf");

        var archived = store.ArchiveRecord(record.RecordId, "person-record-admin");
        var purged = store.PurgeRecord(record.RecordId, "person-record-admin");

        Assert.Equal("archived", archived.Status);
        Assert.NotNull(archived.ArchivedAt);
        Assert.Equal("purged", purged.Status);
        Assert.NotNull(purged.PurgedAt);
    }

    [Fact]
    public void CreatePackage_generates_a_manifest_and_marks_the_package_complete()
    {
        var store = new RecordArrStore();

        var package = store.CreatePackage(
            "Trip closeout packet",
            "delivery",
            "routarr",
            "routarr:trip:trip-7781",
            "rec-bol-001",
            "person-evidence-manager");

        var manifest = store.GetManifest(package.PackageId);

        Assert.Equal("complete", package.Status);
        Assert.NotNull(package.ManifestChecksum);
        Assert.NotNull(package.GeneratedPdfRecordRef);
        Assert.NotNull(package.GeneratedZipFileRef);
        Assert.NotNull(manifest);
        Assert.Equal(package.ManifestChecksum, manifest!.Checksum);
        Assert.NotEmpty(manifest.RecordEntries);
        Assert.NotEmpty(manifest.SourceObjectEntries);
    }

    [Fact]
    public void Reminders_include_due_controlled_document_work_items()
    {
        var store = new RecordArrStore();
        var principal = CreatePrincipal(personId: "person-doc-controller");

        var reminders = store.GetReminders(principal);

        Assert.Contains(reminders, reminder =>
            reminder.ReminderType == "controlled_document_review" &&
            reminder.ControlledDocumentId == "doc-001" &&
            reminder.Status == "due_for_review");

        Assert.Contains(reminders, reminder =>
            reminder.ReminderType == "document_acknowledgement" &&
            reminder.ControlledDocumentId == "doc-001" &&
            reminder.Status == "due_for_review");
    }

    [Fact]
    public void Reminders_include_expiring_records()
    {
        var store = new RecordArrStore();
        var principal = CreatePrincipal(isPlatformAdmin: true);

        var expiring = store.CreateRecord(
            DefaultTenantId,
            "Expiring record",
            "Test record expiry reminder.",
            "document",
            "procedure",
            "operations",
            "standard",
            "internal",
            "routarr",
            "trip",
            "trip-9001",
            "RT-9001",
            "person-route-lead",
            "person-route-lead",
            "expiring.pdf",
            "application/pdf");

        store.UpdateRecordStatus(expiring.RecordId, "active", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7));

        var reminders = store.GetReminders(principal);

        Assert.Contains(reminders, reminder =>
            reminder.ReminderType == "record_expiration" &&
            reminder.RecordId == expiring.RecordId &&
            reminder.Status == "due_for_action");
    }

    [Fact]
    public void Access_logs_can_be_filtered_by_record_id()
    {
        var store = new RecordArrStore();

        var filtered = store.GetAccessLogs("rec-bol-001");

        Assert.NotEmpty(filtered);
        Assert.All(filtered, log => Assert.Equal("rec-bol-001", log.RecordId));
    }

    [Fact]
    public void Legal_hold_scope_rules_block_matching_records_until_release()
    {
        var store = new RecordArrStore();

        var hold = store.CreateLegalHold(
            "RoutArr audit hold",
            "Hold delivery evidence while audit proceeds.",
            "audit",
            "routarr",
            "trip",
            "trip-7781",
            "person-record-admin",
            ["source_product:routarr"],
            []);

        store.ActivateLegalHold(hold.LegalHoldId);

        var blockedStatus = store.GetRetentionStatus("rec-bol-001");
        Assert.Equal("blocked_by_legal_hold", blockedStatus?.Status);

        store.ReleaseLegalHold(hold.LegalHoldId, "person-record-admin", "Audit complete.");

        var restoredStatus = store.GetRetentionStatus("rec-bol-001");
        Assert.Equal("active", restoredStatus?.Status);
    }

    [Fact]
    public void Invalid_legal_hold_values_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateLegalHold(
                "Bad hold",
                "Invalid hold type.",
                "not-a-hold-type",
                "routarr",
                "trip",
                "trip-7781",
                "person-record-admin",
                ["source_product:routarr"],
                []));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateLegalHold(
                "Bad hold",
                "Invalid scope rule.",
                "audit",
                "routarr",
                "trip",
                "trip-7781",
                "person-record-admin",
                ["bad-scope-rule"],
                []));
    }

    [Fact]
    public void Invalid_controlled_document_workflow_values_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentDistribution(
                "doc-001",
                "ver-002",
                "not-a-distribution-type",
                "person-doc-controller"));

        Assert.Throws<InvalidOperationException>(() =>
            store.RequestDocumentReview(
                "doc-001",
                "ver-002",
                "not-a-review-type",
                "person-doc-controller",
                "person-reviewer",
                null));

        Assert.Throws<InvalidOperationException>(() =>
            store.UpdateControlledDocumentStatus(
                "doc-001",
                "not-a-document-status",
                "person-doc-controller"));
    }

    [Fact]
    public void Invalid_disposal_review_values_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDisposalReview(
                "rec-bol-001",
                "rstat-001",
                "not-a-proposed-action",
                "person-record-admin"));

        var review = store.CreateDisposalReview(
            "rec-bol-001",
            "rstat-001",
            "retain",
            "person-record-admin");

        Assert.Throws<InvalidOperationException>(() =>
            store.CompleteDisposalReview(
                review.DisposalReviewId,
                "not-a-review-status",
                "person-record-admin",
                "Nope"));
    }

    [Fact]
    public void Document_distribution_and_acknowledgement_invalid_inputs_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentDistribution(
                "doc-001",
                "ver-002",
                "person",
                " "));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentAcknowledgement(
                "doc-001",
                "ver-002",
                " ",
                "Attest.",
                DateTimeOffset.UtcNow.AddDays(1)));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentAcknowledgement(
                "doc-001",
                "ver-002",
                "person-doc-controller",
                "Attest.",
                DateTimeOffset.UtcNow.AddMinutes(-5)));
    }

    [Fact]
    public void Controlled_document_version_inputs_are_rejected_when_blank()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentVersion(
                "doc-001",
                " ",
                "person-doc-controller",
                "Change summary"));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentVersion(
                "doc-001",
                "version.pdf",
                " ",
                "Change summary"));

        Assert.Throws<InvalidOperationException>(() =>
            store.PromoteDocumentVersion(
                "doc-001",
                "ver-002",
                " ",
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Controlled_document_review_flow_updates_version_statuses()
    {
        var store = new RecordArrStore();

        var document = store.CreateControlledDocument(
            "Review flow doc",
            "Checks version lifecycle transitions.",
            "procedure",
            "operations",
            "review_flow",
            "person-doc-controller",
            "org-receiving",
            "site-north-yard",
            true);

        var version = store.CreateDocumentVersion(
            document.ControlledDocumentId,
            "review-flow.pdf",
            "person-doc-controller",
            "Initial draft.");

        var review = store.RequestDocumentReview(
            document.ControlledDocumentId,
            version.VersionId,
            "approval",
            "person-doc-controller",
            "person-reviewer",
            DateTimeOffset.UtcNow.AddDays(3));

        var duringReviewVersion = store.GetDocumentVersions(document.ControlledDocumentId).Single(item => item.VersionId == version.VersionId);
        Assert.Equal("review", duringReviewVersion.Status);

        store.CompleteDocumentReview(review.DocumentReviewId, "approved", "Looks good.", "Approved.");

        var approvedVersion = store.GetDocumentVersions(document.ControlledDocumentId).Single(item => item.VersionId == version.VersionId);
        Assert.Equal("approved", approvedVersion.Status);

        var promoted = store.PromoteDocumentVersion(document.ControlledDocumentId, version.VersionId, "person-doc-controller", DateTimeOffset.UtcNow);
        Assert.Equal("effective", promoted.Status);

        var effectiveVersion = store.GetDocumentVersions(document.ControlledDocumentId).Single(item => item.VersionId == version.VersionId);
        Assert.Equal("effective", effectiveVersion.Status);
    }

    [Fact]
    public void Controlled_document_archiving_archives_versions()
    {
        var store = new RecordArrStore();

        var document = store.CreateControlledDocument(
            "Archive flow doc",
            "Checks archived document version lifecycle.",
            "procedure",
            "operations",
            "archive_flow",
            "person-doc-controller",
            "org-receiving",
            "site-north-yard",
            true);

        var version = store.CreateDocumentVersion(
            document.ControlledDocumentId,
            "archive-flow.pdf",
            "person-doc-controller",
            "Initial draft.");

        store.UpdateControlledDocumentStatus(document.ControlledDocumentId, "archived", "person-doc-controller");

        var archivedVersion = store.GetDocumentVersions(document.ControlledDocumentId).Single(item => item.VersionId == version.VersionId);
        Assert.Equal("archived", archivedVersion.Status);
    }

    [Fact]
    public void Revoked_document_distributions_cannot_be_mutated_again()
    {
        var store = new RecordArrStore();

        var distribution = store.CreateDocumentDistribution(
            "doc-001",
            "ver-002",
            "person",
            "person-doc-controller");

        store.RevokeDocumentDistribution(distribution.DistributionId, "person-doc-controller", "No longer needed.");

        Assert.Throws<InvalidOperationException>(() =>
            store.ExpireDocumentDistribution(
                distribution.DistributionId,
                "person-doc-controller",
                "Still no longer needed."));
    }

    [Fact]
    public void Invalid_package_and_controlled_document_types_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreatePackage(
                "Bad package",
                "not-a-package-type",
                "routarr",
                "routarr:trip:trip-7781",
                "rec-bol-001",
                "person-evidence-manager"));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateControlledDocument(
                "Bad controlled document",
                "Invalid type.",
                "not-a-controlled-document-type",
                "operations",
                "invalid_type",
                "person-doc-controller",
                "org-receiving",
                "site-north-yard",
                true));
    }

    [Fact]
    public void Invalid_evidence_mapping_values_are_rejected()
    {
        var store = new RecordArrStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateEvidenceMapping(
                "rec-bol-001",
                "routarr",
                "trip",
                "trip-7781",
                "evidence_requirement.trip.pod",
                "proof_of_delivery",
                "not-a-mapping-source",
                0.9m));

        var mapping = store.CreateEvidenceMapping(
            "rec-bol-001",
            "routarr",
            "trip",
            "trip-7781",
            "evidence_requirement.trip.pod",
            "proof_of_delivery",
            "user_confirmed",
            0.9m);

        Assert.Throws<InvalidOperationException>(() =>
            store.UpdateEvidenceMapping(
                mapping.EvidenceMappingId,
                "not-a-mapping-status",
                "person-evidence-manager",
                null,
                null));
    }

    [Fact]
    public void Invalid_record_status_values_are_rejected()
    {
        var store = new RecordArrStore();

        var record = store.CreateRecord(
            DefaultTenantId,
            "Status test",
            "Checks record status validation.",
            "document",
            "other",
            "compliance",
            "standard",
            "internal",
            "recordarr",
            "template",
            "status-test",
            "Status Test",
            "person-doc-controller",
            "person-doc-controller",
            "status-test.pdf",
            "application/pdf");

        Assert.Throws<InvalidOperationException>(() =>
            store.UpdateRecordStatus(record.RecordId, "not-a-record-status"));
    }

    private static ClaimsPrincipal CreatePrincipal(
        string? personId = null,
        string? tenantRoleKey = null,
        bool isPlatformAdmin = false,
        params string[] entitlements)
    {
        var userId = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(StlClaimTypes.TenantId, DefaultTenantId),
            new(StlClaimTypes.SessionId, Guid.NewGuid().ToString()),
            new(StlClaimTypes.TenantRoleKey, tenantRoleKey ?? "evidence-manager"),
            new(StlClaimTypes.PlatformAdmin, isPlatformAdmin.ToString().ToLowerInvariant()),
            new(StlClaimTypes.LaunchableProductKeys, string.Join(',', entitlements.Length == 0 ? ["recordarr"] : entitlements)),
        };

        if (!string.IsNullOrWhiteSpace(personId))
        {
            claims.Add(new Claim(StlClaimTypes.PersonId, personId));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static ClaimsPrincipal CreateServicePrincipal(string sourceProductKey)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(StlClaimTypes.TenantId, DefaultTenantId),
            new(StlClaimTypes.SessionId, Guid.NewGuid().ToString()),
            new(StlClaimTypes.PlatformAdmin, "false"),
            new(StlServiceTokenClaimTypes.TokenType, StlServiceTokenClaimTypes.ServiceTokenTypeValue),
            new(StlServiceTokenClaimTypes.SourceProduct, sourceProductKey)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestServiceAuth"));
    }
}

