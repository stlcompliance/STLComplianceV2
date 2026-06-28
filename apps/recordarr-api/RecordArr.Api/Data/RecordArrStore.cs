using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RecordArr.Api.Models;
using STLCompliance.Shared.Auth;
using RecordArr.Api.Services;

namespace RecordArr.Api.Data;

public sealed class RecordArrStore
{
    private readonly object _gate = new();
    private readonly List<RecordArrRecordResponse> _records;
    private readonly List<RecordArrUploadSessionResponse> _uploadSessions;
    private readonly List<RecordArrCaptureRequestResponse> _captureRequests;
    private readonly List<RecordArrFileResponse> _files;
    private readonly List<RecordArrScanProcessingResponse> _scans;
    private readonly List<RecordArrOcrResultResponse> _ocrResults;
    private readonly List<RecordArrExtractionResultResponse> _extractionResults;
    private readonly List<RecordArrEvidenceMappingResponse> _evidenceMappings;
    private readonly List<RecordArrEvidenceCoverageResponse> _evidenceCoverage;
    private readonly List<RecordArrPackageResponse> _packages;
    private readonly List<RecordArrPackageManifestResponse> _manifests;
    private readonly List<RecordArrRecordMetadataResponse> _recordMetadata;
    private readonly List<RecordArrRecordLinkResponse> _recordLinks;
    private readonly List<RecordArrRecordCommentResponse> _recordComments;
    private readonly List<RecordArrRetentionPolicyResponse> _retentionPolicies;
    private readonly List<RecordArrRetentionStatusResponse> _retentionStatuses;
    private readonly List<RecordArrDisposalReviewResponse> _disposalReviews;
    private readonly List<RecordArrLegalHoldResponse> _legalHolds;
    private readonly List<RecordArrControlledDocumentResponse> _controlledDocuments;
    private readonly List<RecordArrControlledDocumentVersionResponse> _documentVersions;
    private readonly List<RecordArrDocumentReviewResponse> _documentReviews;
    private readonly List<RecordArrDocumentDistributionResponse> _documentDistributions;
    private readonly List<RecordArrDocumentAcknowledgementResponse> _documentAcknowledgements;
    private readonly List<RecordArrAccessPolicyResponse> _accessPolicies;
    private readonly List<RecordArrAccessGrantResponse> _accessGrants;
    private readonly List<RecordArrExternalShareResponse> _externalShares;
    private readonly List<RecordArrRedactionResponse> _redactions;
    private readonly List<RecordArrSignatureRecordResponse> _signatureRecords;
    private readonly List<RecordArrPhotoEvidenceResponse> _photoEvidence;
    private readonly List<RecordArrAccessLogResponse> _accessLogs;

    public RecordArrStore()
    {
        _records = [];
        _uploadSessions = [];
        _captureRequests = [];
        _files = [];
        _scans = [];
        _ocrResults = [];
        _extractionResults = [];
        _evidenceMappings = [];
        _evidenceCoverage = [];
        _packages = [];
        _manifests = [];
        _recordMetadata = [];
        _recordLinks = [];
        _recordComments = [];
        _retentionPolicies = [];
        _retentionStatuses = [];
        _disposalReviews = [];
        _legalHolds = [];
        _controlledDocuments = [];
        _documentVersions = [];
        _documentReviews = [];
        _documentDistributions = [];
        _documentAcknowledgements = [];
        _accessPolicies = [];
        _accessGrants = [];
        _externalShares = [];
        _redactions = [];
        _signatureRecords = [];
        _photoEvidence = [];
        _accessLogs = [];

        SeedCanonicalFixtures();
    }

    private void SeedCanonicalFixtures()
    {
        const string tenantId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        const string recordId = "rec-bol-001";
        const string controlledDocumentId = "doc-001";
        const string versionId = "ver-002";
        const string retentionPolicyId = "rpol-001";
        const string retentionStatusId = "rstat-001";
        var now = DateTimeOffset.UtcNow;

        var seedFile = CreateFileObject(
            tenantId,
            recordId,
            "bol.pdf",
            "application/pdf",
            "person-route-lead",
            storageProvider: "recordarr",
            storageKey: "recordarr/seed/rec-bol-001/bol.pdf",
            sizeBytes: 42_000,
            pageCount: 2,
            attachToRecord: false,
            setAsCurrentFile: false);

        var seedRecord = new RecordArrRecordResponse(
            recordId,
            "REC-BOL-001",
            "Bill of lading",
            "Seed bill of lading record for integration and reminder tests.",
            "document",
            "shipping",
            "bol",
            "bill_of_lading",
            "active",
            "internal",
            "routarr",
            "trip",
            "trip-7781",
            "TR-7781",
            "person-route-lead",
            "person-route-lead",
            now.AddDays(-30),
            now.AddDays(-28),
            now.AddYears(1),
            seedFile.OriginalFilename,
            seedFile.MimeType,
            1,
            ["routarr", "document", "shipping", "bol"],
            seedFile.FileId,
            [seedFile.FileId],
            seedFile.FileId,
            ["routarr:trip:trip-7781"],
            [],
            [seedFile.FileId],
            [],
            [],
            [],
            [],
            retentionPolicyId,
            retentionStatusId,
            [],
            null,
            ["routarr", "document", "shipping", "bol"],
            [
                new RecordArrAuditTrailEntryResponse(
                    "aud-001",
                    "created",
                    "person-route-lead",
                    now.AddDays(-30),
                    "Seed canonical record created.")
            ]);

        _records.Add(seedRecord);
        _accessPolicies.Add(new RecordArrAccessPolicyResponse(
            "acc-seed-001",
            recordId,
            "default",
            "active",
            ["allow_all"],
            [],
            ["allow_all"],
            [],
            [],
            []));
        _recordLinks.Add(new RecordArrRecordLinkResponse(
            "rlk-001",
            recordId,
            null,
            "routarr:trip:trip-7781",
            "source",
            now.AddDays(-30),
            "person-route-lead"));
        _accessLogs.Add(new RecordArrAccessLogResponse(
            "alog-001",
            recordId,
            "seed",
            "allowed",
            "person-route-lead",
            null,
            null,
            now.AddDays(-30),
            null,
            null,
            "seed-fixture"));

        _retentionPolicies.Add(new RecordArrRetentionPolicyResponse(
            retentionPolicyId,
            "shipping-bol",
            "Shipping bill of lading retention",
            "Seed retention policy for bill of lading records.",
            "document",
            "bol",
            "routarr",
            365,
            "days",
            "created_at",
            "archive",
            false,
            "active",
            now.AddDays(-90),
            now.AddDays(-1)));
        _retentionStatuses.Add(new RecordArrRetentionStatusResponse(
            retentionStatusId,
            recordId,
            retentionPolicyId,
            "active",
            now.AddDays(-180),
            now.AddDays(90),
            now.AddDays(7),
            now.AddDays(-1),
            "person-record-admin",
            null));

        _controlledDocuments.Add(new RecordArrControlledDocumentResponse(
            controlledDocumentId,
            "DOC-001",
            recordId,
            "Bill of lading control",
            "Seed controlled document used by reminder and workflow tests.",
            "procedure",
            "operations",
            "bol",
            "procedure",
            "effective",
            "person-doc-controller",
            "org-receiving",
            "site-north-yard",
            versionId,
            180,
            now.AddDays(7),
            now.AddDays(-30),
            null,
            null,
            null,
            true,
            [recordId],
            [
                new RecordArrAuditTrailEntryResponse(
                    "aud-002",
                    "created",
                    "person-doc-controller",
                    now.AddDays(-30),
                    "Seed controlled document created.")
            ]));
        _documentVersions.Add(new RecordArrControlledDocumentVersionResponse(
            versionId,
            controlledDocumentId,
            1,
            "v1",
            "effective",
            "bol.pdf",
            now.AddDays(-30),
            "person-doc-controller",
            now.AddDays(-29),
            now.AddDays(-28),
            "person-doc-controller",
            now.AddDays(-28),
            null,
            "Initial release",
            null,
            null,
            null));
        _documentAcknowledgements.Add(new RecordArrDocumentAcknowledgementResponse(
            "dack-001",
            controlledDocumentId,
            versionId,
            "person-doc-controller",
            "pending",
            null,
            null,
            "Acknowledgement required for the seeded controlled document.",
            now.AddDays(7)));
    }

    public RecordArrSessionResponse BuildSession(string userId, string personId, string tenantId, string tenantRoleKey, bool isPlatformAdmin) =>
        new(userId, personId, tenantId, $"session-{userId}", tenantRoleKey, isPlatformAdmin, "recordarr", RecordArrSuiteLaunchCatalog.OrdinaryProductKeys);

    public RecordArrDashboardResponse GetDashboard(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            var accessibleRecords = _records.Where(record => CanReadRecord(principal, record)).ToArray();
            return new RecordArrDashboardResponse(
                DateTimeOffset.UtcNow,
                accessibleRecords.Length,
                accessibleRecords.Count(record => record.Status is "active" or "effective" or "approved"),
                accessibleRecords.Count(record => record.Status is "review" or "processing"),
                _uploadSessions.Count,
                _packages.Count,
                _controlledDocuments.Count,
                _legalHolds.Count,
                accessibleRecords.OrderByDescending(record => record.UploadedAt).Take(5).Select(ProjectRecord).ToArray(),
                _packages.Where(pkg => pkg.Status is not "archived").Take(5).ToArray(),
                _controlledDocuments.Take(5).ToArray(),
                _legalHolds.Take(5).ToArray());
        }
    }

    public IReadOnlyList<RecordArrRecordResponse> GetRecords(ClaimsPrincipal principal, string? search = null)
    {
        lock (_gate)
        {
            var query = string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim();

            var records = _records.AsEnumerable();
            if (query is not null)
            {
                records = records.Where(record =>
                    record.RecordNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    record.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    record.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    record.SourceProduct.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    record.SourceObjectId.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    record.SourceObjectDisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    record.CurrentFileName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    record.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase)));
            }

            return records
                .Where(record => CanReadRecord(principal, record))
                .OrderByDescending(record => record.UploadedAt)
                .Select(ProjectRecord)
                .ToArray();
        }
    }

    public RecordArrRecordResponse? GetRecord(ClaimsPrincipal principal, string recordId)
    {
        lock (_gate)
        {
            var record = _records.FirstOrDefault(candidate => string.Equals(candidate.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            return record is null || !CanReadRecord(principal, record) ? null : ProjectRecord(record);
        }
    }

    public RecordArrRecordResponse CreateRecord(
        string tenantId,
        string title,
        string description,
        string recordType,
        string documentClass,
        string documentType,
        string documentSubtype,
        string classification,
        string sourceProduct,
        string sourceObjectType,
        string sourceObjectId,
        string sourceObjectDisplayName,
        string ownerPersonId,
        string uploadedByPersonId,
        string currentFileName,
        string currentMimeType,
        string? currentStorageProvider = null,
        string? currentStorageKey = null,
        long? currentSizeBytes = null)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new InvalidOperationException("Record creation requires a tenant id.");
            }
            if (string.IsNullOrWhiteSpace(sourceProduct) ||
                string.IsNullOrWhiteSpace(sourceObjectType) ||
                string.IsNullOrWhiteSpace(sourceObjectId) ||
                string.IsNullOrWhiteSpace(sourceObjectDisplayName))
            {
                throw new InvalidOperationException("Record creation requires a primary target reference.");
            }
            if (string.IsNullOrWhiteSpace(ownerPersonId) || string.IsNullOrWhiteSpace(uploadedByPersonId))
            {
                throw new InvalidOperationException("Record creation requires an owner and uploader.");
            }

            var normalizedRecordType = NormalizeRecordArrEnum(
                recordType,
                nameof(recordType),
                "document",
                "photo",
                "signature",
                "video",
                "audio",
                "form_submission",
                "generated_pdf",
                "certificate",
                "inspection_record",
                "training_record",
                "maintenance_record",
                "receiving_record",
                "delivery_record",
                "quality_record",
                "audit_evidence",
                "evidence_package",
                "report_output",
                "other");
            var normalizedDocumentClass = NormalizeDocumentClassKey(documentClass, nameof(documentClass));
            var normalizedDocumentType = NormalizeRequiredDocumentField(documentType, nameof(documentType));
            var normalizedDocumentSubtype = NormalizeRequiredDocumentField(documentSubtype, nameof(documentSubtype));
            var normalizedClassification = NormalizeClassification(classification);
            var recordId = $"rec-{Guid.NewGuid():N}"[..12];
            var file = CreateFileObject(
                tenantId.Trim(),
                recordId,
                currentFileName,
                currentMimeType,
                uploadedByPersonId,
                currentStorageProvider,
                currentStorageKey,
                currentSizeBytes,
                attachToRecord: false,
                setAsCurrentFile: false);
            var record = new RecordArrRecordResponse(
                recordId,
                $"REC-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                title,
                description,
                normalizedRecordType,
                normalizedDocumentClass,
                normalizedDocumentType,
                normalizedDocumentSubtype,
                "processing",
                normalizedClassification,
                sourceProduct,
                sourceObjectType,
                sourceObjectId,
                sourceObjectDisplayName,
                ownerPersonId,
                uploadedByPersonId,
                DateTimeOffset.UtcNow,
                null,
                null,
                currentFileName,
                currentMimeType,
                1,
                [sourceProduct, normalizedRecordType, normalizedDocumentClass, normalizedDocumentType, normalizedDocumentSubtype],
                file.FileId,
                [file.FileId],
                file.FileId,
                [$"{sourceProduct}:{sourceObjectType}:{sourceObjectId}"],
                [],
                [file.FileId],
                [],
                [],
                [],
                [],
                null,
                null,
                [],
                null,
                [sourceProduct, normalizedRecordType, normalizedDocumentClass, normalizedDocumentType, normalizedDocumentSubtype],
                [],
                null,
                null);
            _records.Add(record);
            _recordLinks.Add(new RecordArrRecordLinkResponse(
                $"rlk-{Guid.NewGuid():N}"[..12],
                record.RecordId,
                null,
                $"{sourceProduct}:{sourceObjectType}:{sourceObjectId}",
                "source",
                DateTimeOffset.UtcNow,
                uploadedByPersonId));
            _accessLogs.Add(new RecordArrAccessLogResponse($"alog-{Guid.NewGuid():N}"[..12], record.RecordId, "upload", "allowed", uploadedByPersonId, null, null, DateTimeOffset.UtcNow, null, null, "api-upload"));
            return ProjectRecord(record);
        }
    }

    public RecordArrRecordResponse CreateGeneratedPdfRecord(
        string tenantId,
        string sourceProduct,
        string sourceEntityType,
        string sourceEntityId,
        string sourceDisplayName,
        string title,
        string description,
        string documentClass,
        string documentType,
        string documentSubtype,
        string classification,
        string ownerPersonId,
        string uploadedByPersonId,
        string fileName,
        string storageProvider,
        string storageKey,
        long sizeBytes,
        string checksumSha256)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(sourceProduct) ||
                string.IsNullOrWhiteSpace(sourceEntityType) ||
                string.IsNullOrWhiteSpace(sourceEntityId))
            {
                throw new InvalidOperationException("Generated PDF archive requires a source reference.");
            }

            if (string.IsNullOrWhiteSpace(ownerPersonId) ||
                string.IsNullOrWhiteSpace(uploadedByPersonId))
            {
                throw new InvalidOperationException("Generated PDF archive requires an owner and uploader.");
            }

            if (string.IsNullOrWhiteSpace(storageProvider) ||
                string.IsNullOrWhiteSpace(storageKey) ||
                string.IsNullOrWhiteSpace(checksumSha256))
            {
                throw new InvalidOperationException("Generated PDF archive requires storage and checksum metadata.");
            }

            var normalizedDocumentClass = NormalizeDocumentClassKey(documentClass, nameof(documentClass));
            var normalizedDocumentType = NormalizeRequiredDocumentField(documentType, nameof(documentType));
            var normalizedDocumentSubtype = NormalizeRequiredDocumentField(documentSubtype, nameof(documentSubtype));
            var normalizedClassification = NormalizeClassification(classification);
            var normalizedDisplayName = string.IsNullOrWhiteSpace(sourceDisplayName)
                ? $"{sourceProduct}:{sourceEntityType}:{sourceEntityId}"
                : sourceDisplayName.Trim();
            var recordId = $"rec-{Guid.NewGuid():N}"[..12];
            var now = DateTimeOffset.UtcNow;
            var file = CreateFileObject(
                tenantId.Trim(),
                recordId,
                fileName,
                "application/pdf",
                uploadedByPersonId,
                storageProvider,
                storageKey,
                sizeBytes,
                pageCount: 1,
                attachToRecord: false,
                setAsCurrentFile: false,
                checksumSha256: checksumSha256);
            var tags = new[]
            {
                sourceProduct.Trim(),
                "generated_pdf",
                normalizedDocumentClass,
                normalizedDocumentType,
                normalizedDocumentSubtype,
            };
            var record = new RecordArrRecordResponse(
                recordId,
                $"REC-{now:yyMMdd-HHmmss}",
                title,
                description,
                "generated_pdf",
                normalizedDocumentClass,
                normalizedDocumentType,
                normalizedDocumentSubtype,
                "approved",
                normalizedClassification,
                sourceProduct.Trim(),
                sourceEntityType.Trim(),
                sourceEntityId.Trim(),
                normalizedDisplayName,
                ownerPersonId.Trim(),
                uploadedByPersonId.Trim(),
                now,
                now,
                null,
                fileName,
                "application/pdf",
                1,
                tags,
                file.FileId,
                [file.FileId],
                file.FileId,
                [$"{sourceProduct.Trim()}:{sourceEntityType.Trim()}:{sourceEntityId.Trim()}"],
                [],
                [file.FileId],
                [],
                [],
                [],
                [],
                null,
                null,
                [],
                null,
                tags,
                [
                    new RecordArrAuditTrailEntryResponse(
                        $"aud-{Guid.NewGuid():N}"[..12],
                        "official_pdf_archived",
                        uploadedByPersonId.Trim(),
                        now,
                        description)
                ],
                null,
                null);
            _records.Add(record);
            _recordLinks.Add(new RecordArrRecordLinkResponse(
                $"rlk-{Guid.NewGuid():N}"[..12],
                record.RecordId,
                null,
                $"{sourceProduct.Trim()}:{sourceEntityType.Trim()}:{sourceEntityId.Trim()}",
                "generated_from",
                now,
                uploadedByPersonId.Trim()));
            _accessLogs.Add(new RecordArrAccessLogResponse(
                $"alog-{Guid.NewGuid():N}"[..12],
                record.RecordId,
                "print.archive",
                "allowed",
                uploadedByPersonId.Trim(),
                null,
                null,
                now,
                null,
                null,
                "official-print-archive"));
            return ProjectRecord(record);
        }
    }

    public IReadOnlyList<RecordArrFileResponse> GetFiles(ClaimsPrincipal principal, string? recordId = null)
    {
        lock (_gate)
        {
            var query = _files.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(recordId))
            {
                query = query.Where(file => string.Equals(file.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            }

            return query
                .Where(file => CanReadRecord(principal, RequireRecord(file.RecordId)))
                .OrderByDescending(file => file.UploadedAt)
                .ToArray();
        }
    }

    public RecordArrFileResponse? GetFile(ClaimsPrincipal principal, string fileId)
    {
        lock (_gate)
        {
            var file = FindFile(fileId);
            if (file is null)
            {
                return null;
            }

            var record = RequireRecord(file.RecordId);
            var canReadRecord = CanReadRecord(principal, record);
            var canInspectDeletedTombstone =
                file.DeletedAt.HasValue &&
                string.Equals(record.OwnerPersonId, principal.GetPersonId().ToString(), StringComparison.OrdinalIgnoreCase);

            if (canReadRecord || canInspectDeletedTombstone)
            {
                _accessLogs.Add(new RecordArrAccessLogResponse($"alog-{Guid.NewGuid():N}"[..12], file.RecordId, "view", "allowed", null, null, null, DateTimeOffset.UtcNow, null, null, "file_lookup"));
                return file;
            }

            _accessLogs.Add(new RecordArrAccessLogResponse($"alog-{Guid.NewGuid():N}"[..12], file.RecordId, "view", "denied", null, null, null, DateTimeOffset.UtcNow, null, null, "access_policy_denied"));

            return null;
        }
    }

    public string DownloadFile(ClaimsPrincipal principal, string fileId)
    {
        lock (_gate)
        {
            var file = FindFile(fileId);
            if (file is null)
            {
                throw new InvalidOperationException($"File {fileId} not found.");
            }

            if (!CanDownloadRecord(principal, RequireRecord(file.RecordId)))
            {
                _accessLogs.Add(new RecordArrAccessLogResponse($"alog-{Guid.NewGuid():N}"[..12], file.RecordId, "download", "denied", null, null, null, DateTimeOffset.UtcNow, null, null, "access_policy_denied"));
                throw new InvalidOperationException($"File {fileId} is not available for download.");
            }

            if (file.DeletedAt.HasValue)
            {
                throw new InvalidOperationException($"File {fileId} is not available for download.");
            }

            _accessLogs.Add(new RecordArrAccessLogResponse($"alog-{Guid.NewGuid():N}"[..12], file.RecordId, "download", "allowed", null, null, null, DateTimeOffset.UtcNow, null, null, "file_download"));

            return string.Join(
                Environment.NewLine,
                [
                    "RecordArr file download",
                    $"File: {file.FileNumber}",
                    $"Original filename: {file.OriginalFilename}",
                    $"Mime type: {file.MimeType}",
                    $"Record: {file.RecordId}",
                    $"Uploaded at: {file.UploadedAt:O}",
                    $"Storage key: {file.StorageKey}",
                    $"Checksum: {file.ChecksumSha256}",
                ]);
        }
    }

    public RecordArrFileResponse CreateFile(
        string recordId,
        string originalFilename,
        string mimeType,
        string uploadedByPersonId,
        string? storageProvider = null,
        string? storageKey = null,
        long? sizeBytes = null,
        int? pageCount = null,
        int? imageWidth = null,
        int? imageHeight = null,
        int? durationSeconds = null)
    {
        lock (_gate)
        {
            RequireRecord(recordId);
            return CreateFileObject(
                ResolveRecordTenantId(recordId),
                recordId,
                originalFilename,
                mimeType,
                uploadedByPersonId,
                storageProvider,
                storageKey,
                sizeBytes,
                pageCount,
                imageWidth,
                imageHeight,
                durationSeconds,
                attachToRecord: true,
                setAsCurrentFile: true);
        }
    }

    public IReadOnlyList<RecordArrSignatureRecordResponse> GetSignatureRecords(string? recordId = null)
    {
        lock (_gate)
        {
            var query = _signatureRecords.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(recordId))
            {
                query = query.Where(signature => string.Equals(signature.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderByDescending(signature => signature.SignedAt).ToArray();
        }
    }

    public RecordArrSignatureRecordResponse CreateSignatureRecord(
        string recordId,
        string signaturePurpose,
        string? signerPersonId,
        string? signerExternalName,
        string? signerTitle,
        string attestationText,
        string capturedByPersonId,
        string sourceProduct,
        string sourceObjectRef,
        string? geoCoordinates = null,
        string? deviceSnapshot = null)
    {
        lock (_gate)
        {
            RequireRecord(recordId);
            var file = CreateFileObject(
                ResolveRecordTenantId(recordId),
                recordId,
                $"signature-{signaturePurpose}.png",
                "image/png",
                capturedByPersonId,
                sizeBytes: 128_000,
                imageWidth: 1600,
                imageHeight: 900,
                attachToRecord: true,
                setAsCurrentFile: false);

            var signature = new RecordArrSignatureRecordResponse(
                $"sig-{Guid.NewGuid():N}"[..12],
                file.TenantId,
                recordId,
                signaturePurpose,
                signerPersonId,
                signerExternalName,
                signerTitle,
                attestationText,
                file.FileId,
                DateTimeOffset.UtcNow,
                capturedByPersonId,
                sourceProduct,
                sourceObjectRef,
                geoCoordinates,
                deviceSnapshot);
            _signatureRecords.Add(signature);
            return signature;
        }
    }

    public IReadOnlyList<RecordArrPhotoEvidenceResponse> GetPhotoEvidence(string? recordId = null)
    {
        lock (_gate)
        {
            var query = _photoEvidence.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(recordId))
            {
                query = query.Where(photo => string.Equals(photo.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            }

            return query.OrderByDescending(photo => photo.CapturedAt).ToArray();
        }
    }

    public RecordArrPhotoEvidenceResponse CreatePhotoEvidence(
        string recordId,
        string photoPurpose,
        string capturedByPersonId,
        string sourceProduct,
        string sourceObjectRef,
        string? geoCoordinates = null,
        string? deviceSnapshot = null,
        string? notes = null)
    {
        lock (_gate)
        {
            RequireRecord(recordId);
            CreateFileObject(
                ResolveRecordTenantId(recordId),
                recordId,
                $"photo-{photoPurpose}.jpg",
                "image/jpeg",
                capturedByPersonId,
                sizeBytes: 256_000,
                imageWidth: 1920,
                imageHeight: 1080,
                attachToRecord: true,
                setAsCurrentFile: false);

            var photo = new RecordArrPhotoEvidenceResponse(
                $"pho-{Guid.NewGuid():N}"[..12],
                ResolveRecordTenantId(recordId),
                recordId,
                photoPurpose,
                sourceProduct,
                sourceObjectRef,
                DateTimeOffset.UtcNow,
                capturedByPersonId,
                geoCoordinates,
                deviceSnapshot,
                notes);
            _photoEvidence.Add(photo);
            return photo;
        }
    }

    public RecordArrRecordResponse UpdateRecordStatus(string recordId, string status, string? classification = null, DateTimeOffset? effectiveAt = null, DateTimeOffset? expiresAt = null)
    {
        lock (_gate)
        {
            var index = _records.FindIndex(record => string.Equals(record.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Record {recordId} not found.");
            }

            var normalizedStatus = NormalizeRecordStatus(status);
            var updated = _records[index] with
            {
                Status = normalizedStatus,
                Classification = classification is null ? _records[index].Classification : NormalizeClassification(classification),
                EffectiveAt = effectiveAt ?? _records[index].EffectiveAt,
                ExpiresAt = expiresAt ?? _records[index].ExpiresAt
            };
            _records[index] = updated;
            return ProjectRecord(updated);
        }
    }

    public RecordArrRecordResponse ArchiveRecord(string recordId, string actorPersonId)
    {
        lock (_gate)
        {
            var record = RequireRecord(recordId);
            EnsureRecordCanBeDisposed(recordId);
            return UpdateRecordLifecycle(record, "archived", actorPersonId, "archive");
        }
    }

    public RecordArrRecordResponse PurgeRecord(string recordId, string actorPersonId)
    {
        lock (_gate)
        {
            var record = RequireRecord(recordId);
            EnsureRecordCanBeDisposed(recordId);
            return UpdateRecordLifecycle(record, "purged", actorPersonId, "purge");
        }
    }

    public IReadOnlyList<RecordArrRecordMetadataResponse> GetRecordMetadata(string recordId)
    {
        lock (_gate)
        {
            return _recordMetadata
                .Where(metadata => string.Equals(metadata.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(metadata => metadata.MetadataId)
                .ToArray();
        }
    }

    public RecordArrRecordMetadataResponse CreateRecordMetadata(
        string recordId,
        string key,
        string value,
        string valueType,
        string source,
        decimal confidenceScore,
        string createdByPersonId)
    {
        lock (_gate)
        {
            var record = RequireRecord(recordId);
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("Record metadata key is required.");
            }
            if (confidenceScore < 0m || confidenceScore > 1m)
            {
                throw new InvalidOperationException("Record metadata confidence must be between 0 and 1.");
            }
            var normalizedValueType = NormalizeRecordMetadataValueType(valueType);
            var normalizedSource = NormalizeRecordMetadataSource(source);

            var metadata = new RecordArrRecordMetadataResponse(
                $"meta-{Guid.NewGuid():N}"[..12],
                record.RecordId,
                key.Trim(),
                value,
                normalizedValueType,
                normalizedSource,
                confidenceScore,
                false,
                null,
                null);
            _recordMetadata.Add(metadata);
            AppendRecordLinkAuditTrail(record, "metadata_added", createdByPersonId, $"Added metadata {key.Trim()}.");
            return metadata;
        }
    }

    public IReadOnlyList<RecordArrRecordLinkResponse> GetRecordLinks(string recordId)
    {
        lock (_gate)
        {
            return _recordLinks
                .Where(link => string.Equals(link.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(link => link.CreatedAt)
                .ToArray();
        }
    }

    public RecordArrRecordLinkResponse CreateRecordLink(
        string recordId,
        string? linkedRecordId,
        string? sourceObjectRef,
        string linkType,
        string createdByPersonId)
    {
        lock (_gate)
        {
            var record = RequireRecord(recordId);
            if (string.IsNullOrWhiteSpace(linkedRecordId) && string.IsNullOrWhiteSpace(sourceObjectRef))
            {
                throw new InvalidOperationException("A record link requires either a linked record or a source object reference.");
            }

            if (!string.IsNullOrWhiteSpace(linkedRecordId) &&
                !_records.Any(candidate => string.Equals(candidate.RecordId, linkedRecordId.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Linked record {linkedRecordId} not found.");
            }

            var normalizedLinkType = NormalizeRecordLinkType(linkType);
            var link = new RecordArrRecordLinkResponse(
                $"rlk-{Guid.NewGuid():N}"[..12],
                record.RecordId,
                string.IsNullOrWhiteSpace(linkedRecordId) ? null : linkedRecordId.Trim(),
                string.IsNullOrWhiteSpace(sourceObjectRef) ? null : sourceObjectRef.Trim(),
                normalizedLinkType,
                DateTimeOffset.UtcNow,
                createdByPersonId);
            _recordLinks.Add(link);
            AppendRecordLinkAuditTrail(record, "link_added", createdByPersonId, $"Added {normalizedLinkType} link.");
            return link;
        }
    }

    public IReadOnlyList<RecordArrRecordCommentResponse> GetRecordComments(string recordId)
    {
        lock (_gate)
        {
            return _recordComments
                .Where(comment => string.Equals(comment.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(comment => comment.CreatedAt)
                .ToArray();
        }
    }

    public RecordArrRecordCommentResponse CreateRecordComment(string recordId, string body, string visibility, string createdByPersonId)
    {
        lock (_gate)
        {
            var record = RequireRecord(recordId);
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new InvalidOperationException("Record comment body is required.");
            }

            var comment = new RecordArrRecordCommentResponse(
                $"com-{Guid.NewGuid():N}"[..12],
                record.RecordId,
                body.Trim(),
                NormalizeRecordCommentVisibility(visibility),
                DateTimeOffset.UtcNow,
                createdByPersonId,
                null,
                null);
            _recordComments.Add(comment);
            return comment;
        }
    }

    public RecordArrRecordCommentResponse UpdateRecordComment(string commentId, string body, string visibility, string editedByPersonId)
    {
        lock (_gate)
        {
            var index = _recordComments.FindIndex(comment => string.Equals(comment.CommentId, commentId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Record comment {commentId} not found.");
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new InvalidOperationException("Record comment body is required.");
            }

            var current = _recordComments[index];
            var updated = current with
            {
                Body = body.Trim(),
                Visibility = NormalizeRecordCommentVisibility(visibility),
                EditedAt = DateTimeOffset.UtcNow,
                EditedByPersonId = editedByPersonId
            };
            _recordComments[index] = updated;
            return updated;
        }
    }

    private static string NormalizeClassification(string classification)
    {
        if (string.IsNullOrWhiteSpace(classification))
        {
            return "internal";
        }

        return classification.Trim().ToLowerInvariant() switch
        {
            "public" => "public",
            "internal" => "internal",
            "confidential" => "confidential",
            "restricted" => "restricted",
            "legal_hold" => "legal_hold",
            _ => throw new InvalidOperationException($"Unsupported record classification '{classification}'.")
        };
    }

    private static string NormalizeRequiredDocumentField(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Record creation requires {fieldName}.");
        }

        return value.Trim().ToLowerInvariant();
    }

    private static string NormalizeDocumentClassKey(string documentClass, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(documentClass))
        {
            throw new InvalidOperationException($"Record creation requires {fieldName}.");
        }

        var normalized = documentClass.Trim().ToLowerInvariant();
        if (normalized.All(character => char.IsLower(character) || char.IsDigit(character) || character == '_'))
        {
            return normalized;
        }

        throw new InvalidOperationException($"Unsupported {fieldName} '{documentClass}'. Document class keys must use letters, numbers, and underscores.");
    }

    private static string NormalizeRecordStatus(string status)
    {
        return NormalizeRecordArrEnum(
            status,
            nameof(status),
            "draft",
            "processing",
            "active",
            "review",
            "approved",
            "rejected",
            "superseded",
            "expired",
            "archived",
            "purged");
    }

    private static string NormalizeRecordMetadataValueType(string valueType)
    {
        if (string.IsNullOrWhiteSpace(valueType))
        {
            return "string";
        }

        return valueType.Trim().ToLowerInvariant() switch
        {
            "string" => "string",
            "number" => "number",
            "boolean" => "boolean",
            "date" => "date",
            "datetime" => "datetime",
            "enum" => "enum",
            "object_ref" => "object_ref",
            "json" => "json",
            _ => throw new InvalidOperationException($"Unsupported record metadata value type '{valueType}'.")
        };
    }

    private static string NormalizeRecordMetadataSource(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return "user";
        }

        return source.Trim().ToLowerInvariant() switch
        {
            "user" => "user",
            "source_product" => "source_product",
            "ocr" => "ocr",
            "extraction" => "extraction",
            "system" => "system",
            "import" => "import",
            _ => throw new InvalidOperationException($"Unsupported record metadata source '{source}'.")
        };
    }

    private static string NormalizeRecordLinkType(string linkType)
    {
        if (string.IsNullOrWhiteSpace(linkType))
        {
            return "related_to";
        }

        return linkType.Trim().ToLowerInvariant() switch
        {
            "source" => "source",
            "evidence_for" => "evidence_for",
            "supersedes" => "supersedes",
            "duplicate_of" => "duplicate_of",
            "attachment_to" => "attachment_to",
            "package_member" => "package_member",
            "generated_from" => "generated_from",
            "redacted_from" => "redacted_from",
            "related_to" => "related_to",
            _ => throw new InvalidOperationException($"Unsupported record link type '{linkType}'.")
        };
    }

    private static string NormalizeRecordCommentVisibility(string visibility)
    {
        if (string.IsNullOrWhiteSpace(visibility))
        {
            return "internal";
        }

        return visibility.Trim().ToLowerInvariant() switch
        {
            "internal" => "internal",
            "auditor_visible" => "auditor_visible",
            "product_visible" => "product_visible",
            "customer_visible" => "customer_visible",
            "supplier_visible" => "supplier_visible",
            _ => throw new InvalidOperationException($"Unsupported record comment visibility '{visibility}'.")
        };
    }

    private static string NormalizeRecordArrEnum(string value, string parameterName, params string[] allowedValues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"'{parameterName}' is required.");
        }

        var normalized = value.Trim().ToLowerInvariant();
        if (allowedValues.Any(allowed => string.Equals(allowed, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return normalized;
        }

        throw new InvalidOperationException($"Unsupported {parameterName} '{value}'. Allowed values: {string.Join(", ", allowedValues)}.");
    }

    private void AppendRecordLinkAuditTrail(RecordArrRecordResponse record, string action, string actorPersonId, string details)
    {
        AddAccessLog(record.RecordId, action, "allowed", actorPersonId, null, null, null, null, details);
    }

    public RecordArrUploadSessionResponse CreateUploadSession(string sourceProduct, string sourceObjectType, string sourceObjectId, string uploadPurpose, bool requiresDocumentScan, bool requiresOcr, bool requiresManualReview)
    {
        lock (_gate)
        {
            var session = new RecordArrUploadSessionResponse(
                $"upl-{Guid.NewGuid():N}"[..12],
                $"UPL-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                "authenticated",
                sourceProduct,
                sourceObjectType,
                sourceObjectId,
                uploadPurpose,
                "active",
                requiresDocumentScan,
                requiresOcr,
                requiresManualReview,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(24),
                null,
                null,
                ["application/pdf", "image/jpeg", "image/png"],
                10,
                25_000_000,
                []);
            _uploadSessions.Add(session);
            return session;
        }
    }

    public RecordArrUploadSessionResponse? GetUploadSession(string uploadSessionId)
    {
        lock (_gate)
        {
            return _uploadSessions.FirstOrDefault(session => string.Equals(session.UploadSessionId, uploadSessionId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyList<RecordArrUploadSessionResponse> GetUploadSessions()
    {
        lock (_gate)
        {
            return _uploadSessions.OrderByDescending(session => session.CreatedAt).ToArray();
        }
    }

    public IReadOnlyList<RecordArrCaptureRequestResponse> GetCaptureRequests()
    {
        lock (_gate)
        {
            return _captureRequests.OrderByDescending(request => request.CreatedAt).ToArray();
        }
    }

    public RecordArrCaptureRequestResponse CreateCaptureRequest(
        string tenantId,
        string sourceProduct,
        string sourceObjectRef,
        string captureType,
        string title,
        string instructions,
        bool required,
        string? uploadSessionRef,
        string? evidenceRequirementRef)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new InvalidOperationException("Capture request tenantId is required.");
            }

            if (string.IsNullOrWhiteSpace(sourceProduct))
            {
                throw new InvalidOperationException("Capture request sourceProduct is required.");
            }

            if (string.IsNullOrWhiteSpace(sourceObjectRef))
            {
                throw new InvalidOperationException("Capture request sourceObjectRef is required.");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new InvalidOperationException("Capture request title is required.");
            }

            if (string.IsNullOrWhiteSpace(instructions))
            {
                throw new InvalidOperationException("Capture request instructions are required.");
            }

            var normalizedCaptureType = NormalizeRecordArrEnum(
                captureType,
                nameof(captureType),
                "photo",
                "document_scan",
                "signature",
                "video",
                "audio",
                "file_upload",
                "generated_pdf");

            var request = new RecordArrCaptureRequestResponse(
                $"cap-{Guid.NewGuid():N}"[..12],
                tenantId.Trim(),
                sourceProduct.Trim(),
                sourceObjectRef.Trim(),
                normalizedCaptureType,
                title.Trim(),
                instructions.Trim(),
                required,
                "open",
                string.IsNullOrWhiteSpace(uploadSessionRef) ? null : uploadSessionRef.Trim(),
                string.IsNullOrWhiteSpace(evidenceRequirementRef) ? null : evidenceRequirementRef.Trim(),
                DateTimeOffset.UtcNow,
                null);
            _captureRequests.Add(request);
            return request;
        }
    }

    public RecordArrCaptureRequestResponse CompleteCaptureRequest(string captureRequestId) => UpdateCaptureRequestStatus(captureRequestId, "completed");

    public RecordArrCaptureRequestResponse SkipCaptureRequest(string captureRequestId) => UpdateCaptureRequestStatus(captureRequestId, "skipped");

    public RecordArrCaptureRequestResponse CancelCaptureRequest(string captureRequestId) => UpdateCaptureRequestStatus(captureRequestId, "canceled");

    public RecordArrCaptureRequestResponse ExpireCaptureRequest(string captureRequestId) => UpdateCaptureRequestStatus(captureRequestId, "expired");

    public RecordArrUploadSessionResponse CompleteUploadSession(string uploadSessionId, string recordId)
    {
        lock (_gate)
        {
            var index = _uploadSessions.FindIndex(session => string.Equals(session.UploadSessionId, uploadSessionId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Upload session {uploadSessionId} not found.");
            }

            var current = _uploadSessions[index];
            var updated = current with
            {
                Status = "completed",
                CompletedAt = DateTimeOffset.UtcNow,
                UploadedRecordRefs = current.UploadedRecordRefs.Concat([recordId]).ToArray()
            };
            _uploadSessions[index] = updated;
            MarkCaptureRequestsCompleted(uploadSessionId, recordId);
            return updated;
        }
    }

    public RecordArrUploadSessionResponse RevokeUploadSession(string uploadSessionId, string reason)
    {
        lock (_gate)
        {
            var index = _uploadSessions.FindIndex(session => string.Equals(session.UploadSessionId, uploadSessionId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Upload session {uploadSessionId} not found.");
            }

            var current = _uploadSessions[index];
            var updated = current with
            {
                Status = "revoked",
                RevokedAt = DateTimeOffset.UtcNow
            };
            _uploadSessions[index] = updated;
            _accessLogs.Add(new RecordArrAccessLogResponse($"alog-{Guid.NewGuid():N}"[..12], current.UploadedRecordRefs.FirstOrDefault() ?? current.SourceObjectId, "share", "denied", null, null, null, DateTimeOffset.UtcNow, null, null, reason));
            return updated;
        }
    }

    private RecordArrCaptureRequestResponse UpdateCaptureRequestStatus(string captureRequestId, string status)
    {
        lock (_gate)
        {
            var index = _captureRequests.FindIndex(request => string.Equals(request.CaptureRequestId, captureRequestId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Capture request {captureRequestId} not found.");
            }

            var current = _captureRequests[index];
            if (string.Equals(current.Status, status, StringComparison.OrdinalIgnoreCase))
            {
                return current;
            }

            if (current.Status is "completed" or "skipped" or "expired" or "canceled")
            {
                throw new InvalidOperationException($"Capture request {captureRequestId} is already {current.Status}.");
            }

            if (status is not ("completed" or "skipped" or "expired" or "canceled"))
            {
                throw new InvalidOperationException($"Unsupported capture request status '{status}'.");
            }

            var updated = current with
            {
                Status = status,
                CompletedAt = status == "completed" ? DateTimeOffset.UtcNow : current.CompletedAt
            };
            _captureRequests[index] = updated;
            return updated;
        }
    }

    private void MarkCaptureRequestsCompleted(string uploadSessionId, string recordId)
    {
        for (var i = 0; i < _captureRequests.Count; i++)
        {
            var current = _captureRequests[i];
            if (!string.Equals(current.UploadSessionRef, uploadSessionId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (current.Status is "completed" or "skipped" or "expired" or "canceled")
            {
                continue;
            }

            _captureRequests[i] = current with
            {
                Status = "completed",
                CompletedAt = DateTimeOffset.UtcNow
            };
            _accessLogs.Add(new RecordArrAccessLogResponse($"alog-{Guid.NewGuid():N}"[..12], recordId, "capture_request", "allowed", null, null, null, DateTimeOffset.UtcNow, null, null, $"Capture request {current.CaptureType} completed from upload session {uploadSessionId}."));
        }
    }

    public RecordArrScanProcessingResponse CreateScanProcessing(string recordId, string originalFileName, string scanPurpose)
    {
        lock (_gate)
        {
            RequireRecord(recordId);
            var now = DateTimeOffset.UtcNow;
            var scanId = $"scan-{Guid.NewGuid():N}"[..12];
            var edgeDetectionId = $"edge-{Guid.NewGuid():N}"[..12];
            var enhancementSettingsId = $"enh-{Guid.NewGuid():N}"[..12];
            var originalFile = CreateFileObject(
                ResolveRecordTenantId(recordId),
                recordId,
                originalFileName,
                "image/jpeg",
                "system",
                sizeBytes: 384_000,
                imageWidth: 1920,
                imageHeight: 1080,
                attachToRecord: true,
                setAsCurrentFile: false);
            var generatedPdfFile = CreateFileObject(
                ResolveRecordTenantId(recordId),
                recordId,
                $"{Path.GetFileNameWithoutExtension(originalFileName)}.pdf",
                "application/pdf",
                "system",
                storageProvider: "generated",
                storageKey: $"recordarr/renditions/{recordId}/{originalFile.FileId}-pdf",
                sizeBytes: 128_000,
                pageCount: 1,
                attachToRecord: true,
                setAsCurrentFile: true);
            var ocrResultId = $"ocr-{Guid.NewGuid():N}"[..12];
            var ocrPageResultId = $"ocrpage-{Guid.NewGuid():N}"[..12];
            var ocrResult = new RecordArrOcrResultResponse(
                ocrResultId,
                recordId,
                generatedPdfFile.FileId,
                "azure_document_intelligence",
                "completed",
                "en",
                0.93m,
                $"OCR text extracted from {originalFileName}.",
                [
                    new RecordArrOcrPageResultResponse(
                        ocrPageResultId,
                        ocrResultId,
                        1,
                        $"OCR text extracted from {originalFileName}.",
                        0.93m,
                        1920,
                        1080,
                        [$"OCR text extracted from {originalFileName}."]),
                ],
                [$"OCR text extracted from {originalFileName}."],
                now,
                null);
            _ocrResults.Add(ocrResult);
            var extractionResultId = $"ext-{Guid.NewGuid():N}"[..12];
            var extractionResult = new RecordArrExtractionResultResponse(
                extractionResultId,
                recordId,
                scanPurpose,
                "manual_review_required",
                [
                    new RecordArrExtractedFieldResponse(
                        $"fld-{Guid.NewGuid():N}"[..12],
                        extractionResultId,
                        "detected_value",
                        "Detected Value",
                        originalFileName,
                        "string",
                        0.81m,
                        1,
                        "20,20,480,60",
                        "unreviewed",
                        null,
                        null,
                        null),
                ],
                0.84m,
                now,
                null,
                null,
                "Auto extraction queued for review.");
            _extractionResults.Add(extractionResult);
            var scan = new RecordArrScanProcessingResponse(
                $"scan-{Guid.NewGuid():N}"[..12],
                recordId,
                originalFileName,
                "completed",
                scanPurpose,
                "edge:detected",
                null,
                null,
                null,
                originalFile.FileId,
                generatedPdfFile.FileId,
                recordId,
                ocrResult.OcrResultId,
                extractionResult.ExtractionResultId,
                new RecordArrEdgeDetectionResultResponse(
                    edgeDetectionId,
                    scanId,
                    "detected",
                    0.95m,
                    0,
                    "10,10,540,20,540,720,10,720",
                    now,
                    false),
                new RecordArrImageEnhancementSettingsResponse(
                    enhancementSettingsId,
                    scanId,
                    true,
                    true,
                    true,
                    false,
                    true,
                    false,
                    true,
                    false,
                    "pdf"),
                0.94m,
                now,
                null);
            _scans.Add(scan);
            return scan;
        }
    }

    public RecordArrScanProcessingResponse? GetScanProcessing(string scanProcessingId)
    {
        lock (_gate)
        {
            return _scans.FirstOrDefault(scan => string.Equals(scan.ScanProcessingId, scanProcessingId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyList<RecordArrScanProcessingResponse> GetScanProcessing()
    {
        lock (_gate)
        {
            return _scans.OrderByDescending(scan => scan.ProcessedAt ?? DateTimeOffset.MinValue).ToArray();
        }
    }

    public RecordArrScanProcessingResponse ApplyManualCorrection(string scanProcessingId, string edgeCoordinates, string correctedByPersonId)
    {
        lock (_gate)
        {
            var index = _scans.FindIndex(scan => string.Equals(scan.ScanProcessingId, scanProcessingId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Scan {scanProcessingId} not found.");
            }

            var current = _scans[index];
            var updated = current with
            {
                Status = "manually_corrected",
                EdgeCoordinates = edgeCoordinates,
                ManualEdgeCoordinates = edgeCoordinates,
                CorrectedByPersonId = correctedByPersonId,
                CorrectedAt = DateTimeOffset.UtcNow,
                ProcessedAt = DateTimeOffset.UtcNow,
                ConfidenceScore = 0.91m,
                EdgeDetectionResult = current.EdgeDetectionResult is null
                    ? null
                    : current.EdgeDetectionResult with
                    {
                        Status = "detected",
                        ConfidenceScore = 0.98m,
                        Corners = edgeCoordinates,
                        DetectedAt = DateTimeOffset.UtcNow,
                        RequiresManualCorrection = false
                    }
            };
            _scans[index] = updated;
            return updated;
        }
    }

    public RecordArrOcrResultResponse? GetOcrResult(string ocrResultId)
    {
        lock (_gate)
        {
            return _ocrResults.FirstOrDefault(result => string.Equals(result.OcrResultId, ocrResultId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public RecordArrExtractionResultResponse? GetExtractionResult(string extractionResultId)
    {
        lock (_gate)
        {
            return _extractionResults.FirstOrDefault(result => string.Equals(result.ExtractionResultId, extractionResultId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public RecordArrExtractionResultResponse ReviewExtractionResult(string extractionResultId, string reviewedByPersonId, string status, string? failureReason)
    {
        lock (_gate)
        {
            var index = _extractionResults.FindIndex(result => string.Equals(result.ExtractionResultId, extractionResultId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Extraction result {extractionResultId} not found.");
            }

            var current = _extractionResults[index];
            var reviewedFields = current.ExtractedFields.Select(field =>
                field with
                {
                    ReviewStatus = status == "completed" ? "accepted" : field.ReviewStatus,
                    CorrectedByPersonId = status == "completed" ? reviewedByPersonId : field.CorrectedByPersonId,
                    CorrectedAt = status == "completed" ? DateTimeOffset.UtcNow : field.CorrectedAt,
                }).ToArray();

            var updated = current with
            {
                Status = status,
                ReviewedByPersonId = reviewedByPersonId,
                ReviewedAt = DateTimeOffset.UtcNow,
                FailureReason = failureReason,
                ExtractedFields = reviewedFields,
                ConfidenceScore = status == "completed" ? Math.Max(current.ConfidenceScore, 0.92m) : current.ConfidenceScore
            };
            _extractionResults[index] = updated;
            return updated;
        }
    }

    public IReadOnlyList<RecordArrEvidenceMappingResponse> GetEvidenceMappings()
    {
        lock (_gate)
        {
            return _evidenceMappings.ToArray();
        }
    }

    public IReadOnlyList<RecordArrEvidenceCoverageResponse> GetEvidenceCoverage()
    {
        lock (_gate)
        {
            _evidenceCoverage.Clear();
            _evidenceCoverage.AddRange(BuildEvidenceCoverage());
            return _evidenceCoverage.ToArray();
        }
    }

    public RecordArrEvidenceMappingResponse CreateEvidenceMapping(string recordId, string sourceProduct, string sourceObjectType, string sourceObjectId, string complianceRequirementRef, string evidenceTypeKey, string mappingSource, decimal confidenceScore)
    {
        lock (_gate)
        {
            var normalizedMappingSource = NormalizeRecordArrEnum(
                mappingSource,
                nameof(mappingSource),
                "compliancecore_suggestion",
                "user_confirmed",
                "product_asserted",
                "import",
                "system");
            var mapping = new RecordArrEvidenceMappingResponse(
                $"map-{Guid.NewGuid():N}"[..12],
                recordId,
                sourceProduct,
                sourceObjectType,
                sourceObjectId,
                complianceRequirementRef,
                evidenceTypeKey,
                "suggested",
                normalizedMappingSource,
                confidenceScore,
                null,
                null,
                null,
                null,
                null,
                null);
            _evidenceMappings.Add(mapping);
            _evidenceCoverage.Clear();
            _evidenceCoverage.AddRange(BuildEvidenceCoverage());
            return mapping;
        }
    }

    public RecordArrEvidenceMappingResponse UpdateEvidenceMapping(string mappingId, string status, string? personId, string? notes, string? reason)
    {
        lock (_gate)
        {
            var index = _evidenceMappings.FindIndex(mapping => string.Equals(mapping.EvidenceMappingId, mappingId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Evidence mapping {mappingId} not found.");
            }

            var normalizedStatus = NormalizeRecordArrEnum(
                status,
                nameof(status),
                "suggested",
                "confirmed",
                "rejected",
                "superseded",
                "expired");
            var current = _evidenceMappings[index];
            var updated = current with
            {
                Status = normalizedStatus,
                ConfirmedByPersonId = normalizedStatus == "confirmed" ? personId : current.ConfirmedByPersonId,
                ConfirmedAt = normalizedStatus == "confirmed" ? DateTimeOffset.UtcNow : current.ConfirmedAt,
                RejectedByPersonId = normalizedStatus == "rejected" ? personId : current.RejectedByPersonId,
                RejectedAt = normalizedStatus == "rejected" ? DateTimeOffset.UtcNow : current.RejectedAt,
                RejectionReason = normalizedStatus == "rejected" ? reason : current.RejectionReason,
                Notes = notes ?? current.Notes
            };
            _evidenceMappings[index] = updated;
            _evidenceCoverage.Clear();
            _evidenceCoverage.AddRange(BuildEvidenceCoverage());
            return updated;
        }
    }

    public IReadOnlyList<RecordArrPackageResponse> GetPackages()
    {
        lock (_gate)
        {
            return _packages.ToArray();
        }
    }

    public RecordArrPackageResponse? GetPackage(string packageId)
    {
        lock (_gate)
        {
            return _packages.FirstOrDefault(pkg => string.Equals(pkg.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public RecordArrPackageResponse CreatePackage(string title, string packageType, string sourceProduct, string sourceObjectRef, string recordRef, string requestedByPersonId)
    {
        lock (_gate)
        {
            RequireRecord(recordRef);
            var normalizedPackageType = NormalizeRecordArrEnum(
                packageType,
                nameof(packageType),
                "audit",
                "work_order_closeout",
                "training_completion",
                "receiving",
                "delivery",
                "quality",
                "capa",
                "customer",
                "supplier",
                "compliance",
                "incident",
                "person_audit",
                "report_output",
                "custom");
            var now = DateTimeOffset.UtcNow;
            var packageRecordRefs = BuildPackageRecordRefs(recordRef, sourceObjectRef);
            var sourceObjectRefs = BuildPackageSourceObjectRefs(sourceObjectRef, recordRef, packageRecordRefs);
            var recordEntries = BuildPackageRecordEntries(packageRecordRefs);
            var sourceObjectEntries = BuildPackageSourceObjectEntries(sourceProduct, sourceObjectRefs);
            var requirementEntries = BuildPackageRequirementEntries(sourceProduct, sourceObjectRef, packageRecordRefs);
            var manifestChecksum = ComputePackageManifestChecksum(recordEntries, sourceObjectEntries, requirementEntries);
            var packageId = $"pkg-{Guid.NewGuid():N}"[..12];
            var generatedPdfFile = CreateFileObject(
                ResolveRecordTenantId(recordRef),
                recordRef,
                $"{title}.pdf",
                "application/pdf",
                requestedByPersonId,
                storageProvider: "generated",
                storageKey: $"recordarr/packages/{packageId}/manifest.pdf",
                sizeBytes: Math.Max(32_768, title.Length * 2048L),
                pageCount: Math.Max(1, recordEntries.Count + sourceObjectEntries.Count + requirementEntries.Count),
                attachToRecord: true,
                setAsCurrentFile: false);
            var generatedZipFile = CreateFileObject(
                ResolveRecordTenantId(recordRef),
                recordRef,
                $"{title}.zip",
                "application/zip",
                requestedByPersonId,
                storageProvider: "generated",
                storageKey: $"recordarr/packages/{packageId}/package.zip",
                sizeBytes: Math.Max(65_536, title.Length * 4096L),
                attachToRecord: true,
                setAsCurrentFile: false);
            var manifest = new RecordArrPackageManifestResponse(
                $"manifest-{Guid.NewGuid():N}"[..12],
                packageId,
                1,
                now,
                recordEntries,
                sourceObjectEntries,
                requirementEntries,
                manifestChecksum,
                requestedByPersonId);

            var package = new RecordArrPackageResponse(
                packageId,
                $"PKG-{now:yyMMdd-HHmmss}",
                title,
                normalizedPackageType,
                "complete",
                sourceProduct,
                sourceObjectRefs,
                packageRecordRefs,
                manifestChecksum,
                generatedPdfFile.FileId,
                generatedZipFile.FileId,
                now,
                now,
                null,
                null,
                null);
            _manifests.Add(manifest);
            _packages.Add(package);
            AddAccessLog(recordRef, "package.created", "allowed", requestedByPersonId, null, null, null, null, "package-created");
            return package;
        }
    }

    public RecordArrPackageResponse LockPackage(string packageId)
    {
        lock (_gate)
        {
            var index = _packages.FindIndex(pkg => string.Equals(pkg.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Package {packageId} not found.");
            }

            var current = _packages[index];
            var updated = current with
            {
                Status = "locked",
                LockedAt = DateTimeOffset.UtcNow
            };
            _packages[index] = updated;
            AddAccessLog(current.RecordRefs.FirstOrDefault() ?? current.SourceObjectRefs.FirstOrDefault() ?? packageId, "package.locked", "allowed", "system", null, null, null, null, "package-locked");
            return updated;
        }
    }

    public RecordArrPackageResponse ArchivePackage(string packageId)
    {
        lock (_gate)
        {
            var index = _packages.FindIndex(pkg => string.Equals(pkg.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Package {packageId} not found.");
            }

            var current = _packages[index];
            if (string.Equals(current.Status, "archived", StringComparison.OrdinalIgnoreCase))
            {
                return current;
            }

            var updated = current with
            {
                Status = "archived",
                ArchivedAt = DateTimeOffset.UtcNow
            };
            _packages[index] = updated;
            AddAccessLog(current.RecordRefs.FirstOrDefault() ?? current.SourceObjectRefs.FirstOrDefault() ?? packageId, "package.archived", "allowed", "system", null, null, null, null, "package-archived");
            return updated;
        }
    }

    public IReadOnlyList<RecordArrRetentionPolicyResponse> GetRetentionPolicies()
    {
        lock (_gate)
        {
            return _retentionPolicies.ToArray();
        }
    }

    public RecordArrRetentionStatusResponse? GetRetentionStatus(string recordId)
    {
        lock (_gate)
        {
            return _retentionStatuses.FirstOrDefault(status => string.Equals(status.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyList<RecordArrRetentionStatusResponse> RecalculateRetentionStatuses()
    {
        lock (_gate)
        {
            RefreshRetentionStatusesForActiveLegalHolds();
            return _retentionStatuses.ToArray();
        }
    }

    public RecordArrLegalHoldResponse CreateLegalHold(string title, string description, string holdType, string sourceProduct, string sourceObjectType, string sourceObjectId, string createdByPersonId, IEnumerable<string> scopeRules, IEnumerable<string> recordRefs)
    {
        lock (_gate)
        {
            var normalizedHoldType = NormalizeRecordArrEnum(
                holdType,
                nameof(holdType),
                "legal",
                "regulatory",
                "audit",
                "investigation",
                "customer_dispute",
                "supplier_dispute",
                "internal_review");
            var normalizedScopeRules = scopeRules.Select(ParseLegalHoldScopeRule).ToArray();
            var normalizedRecordRefs = ResolveLegalHoldRecordRefs(normalizedScopeRules, recordRefs).ToArray();
            var hold = new RecordArrLegalHoldResponse(
                $"hold-{Guid.NewGuid():N}"[..12],
                $"HOLD-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                title,
                description,
                "draft",
                normalizedHoldType,
                normalizedScopeRules,
                normalizedRecordRefs,
                sourceProduct,
                sourceObjectType,
                sourceObjectId,
                DateTimeOffset.UtcNow,
                createdByPersonId,
                null,
                null,
                null,
                null);
            _legalHolds.Add(hold);
            return hold;
        }
    }

    public RecordArrLegalHoldResponse ActivateLegalHold(string holdId)
    {
        lock (_gate)
        {
            var index = _legalHolds.FindIndex(hold => string.Equals(hold.LegalHoldId, holdId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Legal hold {holdId} not found.");
            }

            var current = _legalHolds[index];
            var updated = current with
            {
                Status = "active",
                ActivatedAt = DateTimeOffset.UtcNow
            };
            _legalHolds[index] = updated;
            RefreshRetentionStatusesForActiveLegalHolds();
            return updated;
        }
    }

    public RecordArrLegalHoldResponse ReleaseLegalHold(string holdId, string releasedByPersonId, string releaseReason)
    {
        lock (_gate)
        {
            var index = _legalHolds.FindIndex(hold => string.Equals(hold.LegalHoldId, holdId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Legal hold {holdId} not found.");
            }

            var current = _legalHolds[index];
            var updated = current with
            {
                Status = "released",
                ReleasedAt = DateTimeOffset.UtcNow,
                ReleasedByPersonId = releasedByPersonId,
                ReleaseReason = releaseReason
            };
            _legalHolds[index] = updated;
            RefreshRetentionStatusesForActiveLegalHolds();
            return updated;
        }
    }

    private void RefreshRetentionStatusesForActiveLegalHolds()
    {
        var activeHoldRecordRefs = _legalHolds
            .Where(hold => string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase))
            .SelectMany(hold => hold.RecordRefs)
            .Select(recordRef => recordRef.Trim())
            .Where(recordRef => !string.IsNullOrWhiteSpace(recordRef))
            .Concat(_legalHolds
                .Where(hold => string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase))
                .SelectMany(hold => hold.ScopeRules.Select(scopeRule => ParseLegalHoldScopeRule(scopeRule)))
                .SelectMany(scopeRule => _records.Where(record => IsRecordMatchedByLegalHoldScopeRule(record, scopeRule)))
                .Select(record => record.RecordId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < _retentionStatuses.Count; i++)
        {
            var current = _retentionStatuses[i];
            var policy = _retentionPolicies.FirstOrDefault(candidate =>
                string.Equals(candidate.RetentionPolicyId, current.RetentionPolicyRef, StringComparison.OrdinalIgnoreCase));

            var nextStatus = activeHoldRecordRefs.Contains(current.RecordId)
                ? "blocked_by_legal_hold"
                : GetRetentionLifecycleStatus(current, policy);

            if (string.Equals(current.Status, nextStatus, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _retentionStatuses[i] = current with
            {
                Status = nextStatus
            };
        }
    }

    private static string GetRetentionLifecycleStatus(RecordArrRetentionStatusResponse retentionStatus, RecordArrRetentionPolicyResponse? policy)
    {
        if (policy is null)
        {
            return retentionStatus.Status == "blocked_by_legal_hold" ? "active" : retentionStatus.Status;
        }

        if (string.Equals(policy.RetentionUnit, "indefinite", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(retentionStatus.Status, "indefinite", StringComparison.OrdinalIgnoreCase))
        {
            return "indefinite";
        }

        var now = DateTimeOffset.UtcNow;
        if (retentionStatus.RetentionExpiresAt is not null && retentionStatus.RetentionExpiresAt <= now)
        {
            return policy.DisposalAction.Trim().ToLowerInvariant() switch
            {
                "archive" => "eligible_for_archive",
                "purge" => "eligible_for_purge",
                "anonymize" => "eligible_for_purge",
                _ => "eligible_for_archive"
            };
        }

        if (retentionStatus.NextReviewAt is not null && retentionStatus.NextReviewAt <= now)
        {
            return "due_for_review";
        }

        if (retentionStatus.Status is "blocked_by_legal_hold")
        {
            return "active";
        }

        return "active";
    }

    private static string ParseLegalHoldScopeRule(string scopeRule)
    {
        if (string.IsNullOrWhiteSpace(scopeRule))
        {
            throw new InvalidOperationException("Legal hold scope rules cannot be empty.");
        }

        var parts = scopeRule.Split([':', '='], 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"Unsupported legal hold scope rule '{scopeRule}'. Expected '<scopeType>:<value>'.");
        }

        var scopeType = NormalizeRecordArrEnum(
            parts[0],
            "scopeType",
            "record",
            "record_type",
            "document_type",
            "source_product",
            "source_object",
            "person",
            "asset",
            "customer",
            "supplier",
            "date_range",
            "search_query");
        return $"{scopeType}:{parts[1]}";
    }

    private IReadOnlyList<string> ResolveLegalHoldRecordRefs(IEnumerable<string> scopeRules, IEnumerable<string> recordRefs)
    {
        var refs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var recordRef in recordRefs)
        {
            if (!string.IsNullOrWhiteSpace(recordRef))
            {
                refs.Add(recordRef.Trim());
            }
        }

        foreach (var scopeRule in scopeRules.Select(ParseLegalHoldScopeRule))
        {
            foreach (var record in _records.Where(record => IsRecordMatchedByLegalHoldScopeRule(record, scopeRule)))
            {
                refs.Add(record.RecordId);
            }
        }

        return refs.ToArray();
    }

    private static bool IsRecordMatchedByLegalHoldScopeRule(RecordArrRecordResponse record, string scopeRule)
    {
        var parts = scopeRule.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        var scopeType = parts[0];
        var value = parts[1];
        return scopeType switch
        {
            "record" => string.Equals(record.RecordId, value, StringComparison.OrdinalIgnoreCase),
            "record_type" => string.Equals(record.RecordType, value, StringComparison.OrdinalIgnoreCase),
            "document_type" => string.Equals(record.DocumentType, value, StringComparison.OrdinalIgnoreCase),
            "source_product" => string.Equals(record.SourceProduct, value, StringComparison.OrdinalIgnoreCase),
            "source_object" => string.Equals($"{record.SourceProduct}:{record.SourceObjectType}:{record.SourceObjectId}", value, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(record.SourceObjectId, value, StringComparison.OrdinalIgnoreCase),
            "person" => string.Equals(record.OwnerPersonId, value, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(record.UploadedByPersonId, value, StringComparison.OrdinalIgnoreCase),
            "date_range" => IsRecordWithinDateRange(record, value),
            "search_query" => MatchesSearchQuery(record, value),
            "asset" or "customer" or "supplier" => string.Equals(record.SourceObjectType, scopeType, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static bool IsRecordWithinDateRange(RecordArrRecordResponse record, string value)
    {
        var parts = value.Split("..", 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2 ||
            !DateTimeOffset.TryParse(parts[0], out var start) ||
            !DateTimeOffset.TryParse(parts[1], out var end))
        {
            return false;
        }

        return record.UploadedAt >= start && record.UploadedAt <= end;
    }

    private static bool MatchesSearchQuery(RecordArrRecordResponse record, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return record.Title.Contains(value, StringComparison.OrdinalIgnoreCase) ||
               record.Description.Contains(value, StringComparison.OrdinalIgnoreCase) ||
               record.Tags.Any(tag => tag.Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private RecordArrRecordResponse RequireRecord(string recordId)
    {
        var record = _records.FirstOrDefault(candidate => string.Equals(candidate.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
        return record ?? throw new InvalidOperationException($"Record {recordId} not found.");
    }

    private RecordArrRecordResponse ProjectRecord(RecordArrRecordResponse record)
    {
        var recordId = record.RecordId;
        var sourceObjectRef = $"{record.SourceProduct}:{record.SourceObjectType}:{record.SourceObjectId}";
        var sourceObjectRefs = _recordLinks
            .Where(link =>
                string.Equals(link.RecordId, recordId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(link.LinkedRecordId, recordId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(link.SourceObjectRef, sourceObjectRef, StringComparison.OrdinalIgnoreCase))
            .Select(link => link.SourceObjectRef)
            .Concat(new[] { sourceObjectRef })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var metadataRefs = _recordMetadata
            .Where(metadata => string.Equals(metadata.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
            .Select(metadata => metadata.MetadataId)
            .ToArray();

        var versionRefs = record.FileRefs.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var ocrResultRefs = _ocrResults
            .Where(result => string.Equals(result.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
            .Select(result => result.OcrResultId)
            .ToArray();
        var extractionResultRefs = _extractionResults
            .Where(result => string.Equals(result.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
            .Select(result => result.ExtractionResultId)
            .ToArray();
        var evidenceMappingRefs = _evidenceMappings
            .Where(mapping => string.Equals(mapping.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
            .Select(mapping => mapping.EvidenceMappingId)
            .ToArray();
        var packageRefs = _packages
            .Where(package => package.RecordRefs.Contains(recordId, StringComparer.OrdinalIgnoreCase))
            .Select(package => package.PackageId)
            .ToArray();
        var retentionStatus = _retentionStatuses.FirstOrDefault(status => string.Equals(status.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
        var legalHoldRefs = _legalHolds
            .Where(hold => hold.RecordRefs.Contains(recordId, StringComparer.OrdinalIgnoreCase))
            .Select(hold => hold.LegalHoldId)
            .ToArray();
        var accessPolicyRef = _accessPolicies
            .FirstOrDefault(policy => string.Equals(policy.RecordId, recordId, StringComparison.OrdinalIgnoreCase))?.AccessPolicyId;
        var complianceRefs = _evidenceMappings
            .Where(mapping => string.Equals(mapping.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
            .Select(mapping => mapping.ComplianceRequirementRef)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var auditTrail = _accessLogs
            .Where(log => string.Equals(log.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(log => log.OccurredAt)
            .Select(log => new RecordArrAuditTrailEntryResponse(
                log.AccessLogId,
                log.Action,
                log.ActorPersonId ?? log.ActorServiceClientId ?? "system",
                log.OccurredAt,
                log.ReasonCode ?? log.Result))
            .ToArray();

        return record with
        {
            CurrentVersionRef = record.CurrentFileRef,
            SourceObjectRefs = sourceObjectRefs,
            MetadataRefs = metadataRefs,
            VersionRefs = versionRefs,
            OcrResultRefs = ocrResultRefs,
            ExtractionResultRefs = extractionResultRefs,
            EvidenceMappingRefs = evidenceMappingRefs,
            PackageRefs = packageRefs,
            RetentionPolicyRef = retentionStatus?.RetentionPolicyRef,
            RetentionStatusRef = retentionStatus?.RetentionStatusId,
            LegalHoldRefs = legalHoldRefs,
            AccessPolicyRef = accessPolicyRef,
            ComplianceRefs = complianceRefs,
            AuditTrail = auditTrail,
            RecordRef = BuildRecordRef(record, retentionStatus)
        };
    }

    private static RecordArrRecordRefResponse BuildRecordRef(RecordArrRecordResponse record, RecordArrRetentionStatusResponse? retentionStatus)
        => new(
            record.RecordId,
            record.RecordNumber,
            record.Title,
            record.RecordType,
            record.DocumentClass,
            record.DocumentType,
            record.DocumentSubtype,
            record.Status,
            record.Classification,
            record.VersionNumber,
            record.ExpiresAt,
            retentionStatus?.Status,
            DateTimeOffset.UtcNow);

    private RecordArrFileResponse? FindFile(string fileId)
        => _files.FirstOrDefault(candidate => string.Equals(candidate.FileId, fileId, StringComparison.OrdinalIgnoreCase));

    private bool CanReadRecord(ClaimsPrincipal principal, RecordArrRecordResponse record)
        => CanAccessRecord(principal, record, "recordarr.records.read");

    private bool CanDownloadRecord(ClaimsPrincipal principal, RecordArrRecordResponse record)
        => CanAccessRecord(principal, record, "recordarr.files.download");

    private bool CanAccessRecord(ClaimsPrincipal principal, RecordArrRecordResponse record, string permission)
    {
        if (!string.Equals(ResolveRecordTenantId(record.RecordId), principal.GetTenantId().ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (record.Status is "purged")
        {
            return false;
        }

        var personId = principal.GetPersonId().ToString();
        if (string.Equals(record.OwnerPersonId, personId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var activePolicy = _accessPolicies
            .FirstOrDefault(policy => string.Equals(policy.RecordId, record.RecordId, StringComparison.OrdinalIgnoreCase) &&
                                      string.Equals(policy.Status, "active", StringComparison.OrdinalIgnoreCase));

        if (activePolicy is null)
        {
            return false;
        }

        if (IsPolicyPermissionAllowed(activePolicy, permission))
        {
            return true;
        }

        return _accessGrants.Any(grant =>
            string.Equals(grant.RecordId, record.RecordId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(grant.Status, "active", StringComparison.OrdinalIgnoreCase) &&
            (!grant.ExpiresAt.HasValue || grant.ExpiresAt > DateTimeOffset.UtcNow) &&
            IsGrantMatchedByPrincipal(principal, grant) &&
            PermissionMatches(grant.Permission, permission));
    }

    private static bool IsPolicyPermissionAllowed(RecordArrAccessPolicyResponse policy, string permission)
    {
        var rules = permission switch
        {
            "recordarr.records.read" => policy.ReadRules,
            "recordarr.files.download" => policy.DownloadRules,
            "recordarr.records.update" => policy.WriteRules,
            "recordarr.external_shares.create" => policy.ShareRules,
            "recordarr.packages.export" => policy.ExportRules,
            "recordarr.records.purge" => policy.PurgeRules,
            _ => Array.Empty<string>()
        };

        return rules.Any(rule =>
            string.Equals(rule, "allow_all", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(rule, permission, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(rule, "recordarr.records.read", StringComparison.OrdinalIgnoreCase) && permission is "recordarr.records.read" or "recordarr.files.download" ||
            string.Equals(rule, "recordarr.files.download", StringComparison.OrdinalIgnoreCase) && permission is "recordarr.records.read" or "recordarr.files.download");
    }

    private static bool IsGrantMatchedByPrincipal(ClaimsPrincipal principal, RecordArrAccessGrantResponse grant)
    {
        return grant.GranteeType switch
        {
            "person" => string.Equals(principal.GetPersonId().ToString(), grant.GranteeRef, StringComparison.OrdinalIgnoreCase),
            "role" => string.Equals(principal.GetTenantRoleKey(), grant.GranteeRef, StringComparison.OrdinalIgnoreCase),
            "product" => principal.IsServicePrincipal() &&
                         string.Equals(
                             principal.GetSourceProductKey(),
                             ProductKeyAliases.Normalize(grant.GranteeRef),
                             StringComparison.OrdinalIgnoreCase),
            "service_client" => string.Equals(principal.GetUserId().ToString(), grant.GranteeRef, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static bool PermissionMatches(string grantPermission, string permission)
        => string.Equals(grantPermission, permission, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(grantPermission, "read", StringComparison.OrdinalIgnoreCase) && permission is "recordarr.records.read" or "recordarr.files.download" ||
           string.Equals(grantPermission, "download", StringComparison.OrdinalIgnoreCase) && permission is "recordarr.files.download";

    private RecordArrRecordResponse UpdateRecordLifecycle(RecordArrRecordResponse record, string status, string actorPersonId, string reasonCode)
    {
        var index = _records.FindIndex(candidate => string.Equals(candidate.RecordId, record.RecordId, StringComparison.OrdinalIgnoreCase));
        if (status is "purged")
        {
            MarkRecordFilesDeleted(record.RecordId, reasonCode);
        }

        var updated = record with
        {
            Status = status,
            ExpiresAt = status is "purged" ? null : record.ExpiresAt,
            ArchivedAt = status is "archived" ? DateTimeOffset.UtcNow : record.ArchivedAt,
            PurgedAt = status is "purged" ? DateTimeOffset.UtcNow : record.PurgedAt
        };

        _records[index] = updated;
        _accessLogs.Add(new RecordArrAccessLogResponse($"alog-{Guid.NewGuid():N}"[..12], record.RecordId, status, "allowed", actorPersonId, null, null, DateTimeOffset.UtcNow, null, null, reasonCode));
        return ProjectRecord(updated);
    }

    private string ResolveRecordTenantId(string recordId)
    {
        var fileTenantId = _files.FirstOrDefault(file =>
            string.Equals(file.RecordId, recordId, StringComparison.OrdinalIgnoreCase))?.TenantId;

        return string.IsNullOrWhiteSpace(fileTenantId) ? "unassigned" : fileTenantId;
    }

    private RecordArrFileResponse CreateFileObject(
        string tenantId,
        string recordId,
        string originalFilename,
        string mimeType,
        string uploadedByPersonId,
        string? storageProvider = null,
        string? storageKey = null,
        long? sizeBytes = null,
        int? pageCount = null,
        int? imageWidth = null,
        int? imageHeight = null,
        int? durationSeconds = null,
        bool attachToRecord = true,
        bool setAsCurrentFile = true,
        string? checksumSha256 = null)
    {
        var now = DateTimeOffset.UtcNow;
        var fileId = $"file-{Guid.NewGuid():N}"[..12];
        var normalizedFilename = originalFilename.Trim().Replace(' ', '_');
        var extension = Path.GetExtension(normalizedFilename).TrimStart('.').ToLowerInvariant();
        var file = new RecordArrFileResponse(
            fileId,
            tenantId,
            recordId,
            $"FILE-{now:yyMMdd-HHmmss}-{_files.Count + 1:000}",
            storageProvider ?? "local",
            storageKey ?? $"recordarr/files/{fileId}",
            originalFilename,
            normalizedFilename,
            string.IsNullOrWhiteSpace(extension) ? "bin" : extension,
            mimeType,
            sizeBytes ?? Math.Max(4_096, originalFilename.Length * 1_024L),
            string.IsNullOrWhiteSpace(checksumSha256) ? $"sha256-{fileId}" : checksumSha256.Trim(),
            pageCount,
            imageWidth,
            imageHeight,
            durationSeconds,
            now,
            uploadedByPersonId,
            "clean",
            "completed",
            "encrypted",
            null,
            null,
            BuildRenditions(fileId, recordId, mimeType, sizeBytes, pageCount, now));

        _files.Add(file);

        if (!attachToRecord)
        {
            return file;
        }

        var recordIndex = _records.FindIndex(candidate => string.Equals(candidate.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
        if (recordIndex >= 0)
        {
            var record = _records[recordIndex];
            var fileRefs = record.FileRefs.Append(fileId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            _records[recordIndex] = setAsCurrentFile
                ? record with
                {
                    CurrentFileName = originalFilename,
                    CurrentMimeType = mimeType,
                    CurrentFileRef = fileId,
                    FileRefs = fileRefs,
                    VersionNumber = record.VersionNumber + 1
                }
                : record with
                {
                    FileRefs = fileRefs
                };
        }

        return file;
    }

    private static IReadOnlyList<RecordArrFileRenditionResponse> BuildRenditions(
        string fileId,
        string recordId,
        string mimeType,
        long? sizeBytes,
        int? pageCount,
        DateTimeOffset now)
    {
        if (!mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(mimeType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        return
        [
            new RecordArrFileRenditionResponse(
                $"rend-{Guid.NewGuid():N}"[..12],
                fileId,
                recordId,
                string.Equals(mimeType, "application/pdf", StringComparison.OrdinalIgnoreCase) ? "preview" : "thumbnail",
                $"recordarr/renditions/{fileId}/preview",
                mimeType,
                sizeBytes is null ? 16_384 : Math.Max(8_192, sizeBytes.Value / 4),
                pageCount,
                "generated",
                now)
        ];
    }

    private void MarkRecordFilesDeleted(string recordId, string deleteReason)
    {
        for (var i = 0; i < _files.Count; i++)
        {
            var file = _files[i];
            if (!string.Equals(file.RecordId, recordId, StringComparison.OrdinalIgnoreCase) || file.DeletedAt.HasValue)
            {
                continue;
            }

            _files[i] = file with
            {
                DeletedAt = DateTimeOffset.UtcNow,
                DeleteReason = deleteReason
            };
        }
    }

    private void AppendControlledDocumentAuditTrail(string controlledDocumentId, RecordArrAuditTrailEntryResponse entry)
    {
        var index = _controlledDocuments.FindIndex(document => string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return;
        }

        var current = _controlledDocuments[index];
        _controlledDocuments[index] = current with { AuditTrail = current.AuditTrail.Append(entry).ToArray() };
    }

    private static RecordArrAuditTrailEntryResponse CreateControlledDocumentAuditTrailEntry(string action, string actorPersonId, string details)
        => new($"aud-{Guid.NewGuid():N}"[..12], action, actorPersonId, DateTimeOffset.UtcNow, details);

    private void EnsureRecordCanBeDisposed(string recordId)
    {
        var activeHold = _legalHolds.FirstOrDefault(hold =>
            string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase) &&
            hold.RecordRefs.Any(reference => string.Equals(reference, recordId, StringComparison.OrdinalIgnoreCase)));

        if (activeHold is not null)
        {
            throw new InvalidOperationException($"Record {recordId} is blocked by legal hold {activeHold.HoldNumber}.");
        }
    }

    public IReadOnlyList<RecordArrLegalHoldResponse> GetLegalHolds()
    {
        lock (_gate)
        {
            return _legalHolds.OrderByDescending(hold => hold.CreatedAt).ToArray();
        }
    }

    public IReadOnlyList<RecordArrControlledDocumentResponse> GetControlledDocuments()
    {
        lock (_gate)
        {
            return _controlledDocuments.ToArray();
        }
    }

    public IReadOnlyList<RecordArrControlledDocumentVersionResponse> GetDocumentVersions(string? controlledDocumentId = null)
    {
        lock (_gate)
        {
            return string.IsNullOrWhiteSpace(controlledDocumentId)
                ? _documentVersions.ToArray()
                : _documentVersions
                    .Where(version => string.Equals(version.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(version => version.VersionNumber)
                    .ToArray();
        }
    }

    public IReadOnlyList<RecordArrDocumentReviewResponse> GetDocumentReviews(string? controlledDocumentId = null)
    {
        lock (_gate)
        {
            return string.IsNullOrWhiteSpace(controlledDocumentId)
                ? _documentReviews.ToArray()
                : _documentReviews
                    .Where(review => string.Equals(review.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
        }
    }

    public IReadOnlyList<RecordArrDocumentDistributionResponse> GetDocumentDistributions(string? controlledDocumentId = null)
    {
        lock (_gate)
        {
            return string.IsNullOrWhiteSpace(controlledDocumentId)
                ? _documentDistributions.ToArray()
                : _documentDistributions
                    .Where(distribution => string.Equals(distribution.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
        }
    }

    public IReadOnlyList<RecordArrDocumentAcknowledgementResponse> GetDocumentAcknowledgements(string? controlledDocumentId = null)
    {
        lock (_gate)
        {
            return string.IsNullOrWhiteSpace(controlledDocumentId)
                ? _documentAcknowledgements.ToArray()
                : _documentAcknowledgements
                    .Where(acknowledgement => string.Equals(acknowledgement.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
        }
    }

    public IReadOnlyList<RecordArrReminderResponse> GetReminders(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            RefreshControlledDocumentWorkflows();

            var now = DateTimeOffset.UtcNow;
            var reminderWindow = now.AddDays(14);
            var reminders = new List<RecordArrReminderResponse>();

            foreach (var document in _controlledDocuments
                         .Where(document => string.Equals(document.Status, "effective", StringComparison.OrdinalIgnoreCase) ||
                                            string.Equals(document.Status, "review", StringComparison.OrdinalIgnoreCase))
                         .Where(document => document.NextReviewAt.HasValue && document.NextReviewAt <= reminderWindow)
                         .OrderBy(document => document.NextReviewAt))
            {
                if (GetRecord(principal, document.RecordId) is null)
                {
                    continue;
                }

                reminders.Add(new RecordArrReminderResponse(
                    $"rem-{Guid.NewGuid():N}"[..12],
                    "controlled_document_review",
                    document.NextReviewAt <= now ? "due_for_review" : "due_for_review",
                    $"{document.DocumentNumber} review due",
                    $"{document.Title} is scheduled for periodic review.",
                    document.RecordId,
                    document.ControlledDocumentId,
                    document.CurrentVersionId,
                    document.OwnerPersonId,
                    document.NextReviewAt,
                    now,
                    $"controlled-document:{document.ControlledDocumentId}"));
            }

            foreach (var acknowledgement in _documentAcknowledgements
                         .Where(acknowledgement => (string.Equals(acknowledgement.Status, "pending", StringComparison.OrdinalIgnoreCase) ||
                                                    string.Equals(acknowledgement.Status, "overdue", StringComparison.OrdinalIgnoreCase)) &&
                                                   acknowledgement.DueAt.HasValue &&
                                                   acknowledgement.DueAt <= reminderWindow)
                         .OrderBy(acknowledgement => acknowledgement.DueAt))
            {
                var document = _controlledDocuments.FirstOrDefault(candidate =>
                    string.Equals(candidate.ControlledDocumentId, acknowledgement.ControlledDocumentId, StringComparison.OrdinalIgnoreCase));
                if (document is null || GetRecord(principal, document.RecordId) is null)
                {
                    continue;
                }

                reminders.Add(new RecordArrReminderResponse(
                    $"rem-{Guid.NewGuid():N}"[..12],
                    "document_acknowledgement",
                    acknowledgement.DueAt <= now ? "overdue" : "due_for_review",
                    $"{document.DocumentNumber} acknowledgement due",
                    $"{acknowledgement.PersonId} still needs to acknowledge {document.Title}.",
                    document.RecordId,
                    document.ControlledDocumentId,
                    acknowledgement.VersionId,
                    acknowledgement.PersonId,
                    acknowledgement.DueAt,
                    now,
                    $"document-acknowledgement:{acknowledgement.AcknowledgementId}"));
            }

            foreach (var retentionStatus in _retentionStatuses
                         .Where(status => string.Equals(status.Status, "active", StringComparison.OrdinalIgnoreCase) &&
                                          status.NextReviewAt.HasValue &&
                                          status.NextReviewAt <= reminderWindow)
                         .OrderBy(status => status.NextReviewAt))
            {
                if (GetRecord(principal, retentionStatus.RecordId) is null)
                {
                    continue;
                }

                reminders.Add(new RecordArrReminderResponse(
                    $"rem-{Guid.NewGuid():N}"[..12],
                    "retention_review",
                    retentionStatus.NextReviewAt <= now ? "due_for_review" : "due_for_review",
                    $"{retentionStatus.RecordId} retention review due",
                    "The retention schedule for this record is due for review.",
                    retentionStatus.RecordId,
                    null,
                    null,
                    null,
                    retentionStatus.NextReviewAt,
                    now,
                    $"retention-status:{retentionStatus.RetentionStatusId}"));
            }

            foreach (var record in _records
                         .Where(record => record.ExpiresAt.HasValue &&
                                          record.ExpiresAt <= reminderWindow &&
                                          !string.Equals(record.Status, "purged", StringComparison.OrdinalIgnoreCase))
                         .OrderBy(record => record.ExpiresAt))
            {
                if (GetRecord(principal, record.RecordId) is null)
                {
                    continue;
                }

                reminders.Add(new RecordArrReminderResponse(
                    $"rem-{Guid.NewGuid():N}"[..12],
                    "record_expiration",
                    record.ExpiresAt <= now ? "overdue" : "due_for_action",
                    $"{record.RecordNumber} expires soon",
                    $"{record.Title} expires on {record.ExpiresAt:O}.",
                    record.RecordId,
                    null,
                    null,
                    record.OwnerPersonId,
                    record.ExpiresAt,
                    now,
                    $"record-expiration:{record.RecordId}"));
            }

            return reminders
                .OrderBy(reminder => reminder.DueAt ?? DateTimeOffset.MaxValue)
                .ThenBy(reminder => reminder.ReminderType, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public RecordArrDocumentDistributionResponse CreateDocumentDistribution(string controlledDocumentId, string versionId, string distributionType, string targetRef)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(targetRef))
            {
                throw new InvalidOperationException("Document distribution targetRef is required.");
            }
            var normalizedDistributionType = NormalizeRecordArrEnum(
                distributionType,
                nameof(distributionType),
                "person",
                "role",
                "department",
                "site",
                "team",
                "product",
                "external_link");
            var distribution = new RecordArrDocumentDistributionResponse(
                $"dist-{Guid.NewGuid():N}"[..12],
                controlledDocumentId,
                versionId,
                normalizedDistributionType,
                targetRef,
                "distributed",
                DateTimeOffset.UtcNow,
                null,
                null);
            _documentDistributions.Add(distribution);
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "distributed",
                    "system",
                    $"Distributed version {versionId} to {distributionType}:{targetRef}."));
            return distribution;
        }
    }

    public RecordArrDocumentDistributionResponse RevokeDocumentDistribution(string distributionId, string revokedByPersonId, string? revokeReason)
    {
        lock (_gate)
        {
            return UpdateDocumentDistributionStatus(distributionId, "revoked", revokedByPersonId, revokeReason ?? $"Revoked by {revokedByPersonId}");
        }
    }

    public RecordArrDocumentDistributionResponse ExpireDocumentDistribution(string distributionId, string expiredByPersonId, string? expireReason)
    {
        lock (_gate)
        {
            return UpdateDocumentDistributionStatus(distributionId, "expired", expiredByPersonId, expireReason ?? $"Expired by {expiredByPersonId}");
        }
    }

    public RecordArrDocumentAcknowledgementResponse CreateDocumentAcknowledgement(string controlledDocumentId, string versionId, string personId, string? attestationText, DateTimeOffset? dueAt)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("Acknowledgement personId is required.");
            }
            if (dueAt.HasValue && dueAt <= DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException("Acknowledgement dueAt must be in the future when provided.");
            }
            var acknowledgement = new RecordArrDocumentAcknowledgementResponse(
                $"dack-{Guid.NewGuid():N}"[..12],
                controlledDocumentId,
                versionId,
                personId,
                "pending",
                null,
                null,
                attestationText,
                dueAt);
            _documentAcknowledgements.Add(acknowledgement);
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "acknowledgement_requested",
                    personId,
                    $"Acknowledgement requested for version {versionId}."));
            return acknowledgement;
        }
    }

    public RecordArrDocumentAcknowledgementResponse CompleteDocumentAcknowledgement(string acknowledgementId, string? signatureRecordRef)
    {
        lock (_gate)
        {
            var index = _documentAcknowledgements.FindIndex(acknowledgement => string.Equals(acknowledgement.AcknowledgementId, acknowledgementId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Document acknowledgement {acknowledgementId} not found.");
            }

            var current = _documentAcknowledgements[index];
            if (string.Equals(current.Status, "waived", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Document acknowledgement {acknowledgementId} is waived and cannot be completed.");
            }
            if (!string.IsNullOrWhiteSpace(signatureRecordRef) && string.IsNullOrWhiteSpace(signatureRecordRef.Trim()))
            {
                throw new InvalidOperationException("Document acknowledgement signatureRecordRef cannot be blank.");
            }
            var updated = current with
            {
                Status = "acknowledged",
                AcknowledgedAt = DateTimeOffset.UtcNow,
                SignatureRecordRef = signatureRecordRef ?? current.SignatureRecordRef
            };
            _documentAcknowledgements[index] = updated;
            MarkRelatedDocumentDistributionsAcknowledged(updated);
            AppendControlledDocumentAuditTrail(
                current.ControlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "acknowledged",
                    "system",
                    $"Acknowledgement {acknowledgementId} completed."));
            return updated;
        }
    }

    private void MarkRelatedDocumentDistributionsAcknowledged(RecordArrDocumentAcknowledgementResponse acknowledgement)
    {
        var now = acknowledgement.AcknowledgedAt ?? DateTimeOffset.UtcNow;
        for (var i = 0; i < _documentDistributions.Count; i++)
        {
            var distribution = _documentDistributions[i];
            if (!string.Equals(distribution.ControlledDocumentId, acknowledgement.ControlledDocumentId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.Equals(distribution.VersionId, acknowledgement.VersionId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.Equals(distribution.TargetRef, acknowledgement.PersonId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _documentDistributions[i] = distribution with
            {
                Status = "acknowledged",
                AcknowledgedAt = now,
                AcknowledgementRef = acknowledgement.AcknowledgementId
            };
            AppendControlledDocumentAuditTrail(
                acknowledgement.ControlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "distribution_acknowledged",
                    acknowledgement.PersonId,
                    $"Distribution {distribution.DistributionId} acknowledged."));
        }
    }

    private RecordArrDocumentDistributionResponse UpdateDocumentDistributionStatus(string distributionId, string status, string actorPersonId, string reason)
    {
        var index = _documentDistributions.FindIndex(distribution => string.Equals(distribution.DistributionId, distributionId, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            throw new InvalidOperationException($"Document distribution {distributionId} not found.");
        }

        var normalizedStatus = NormalizeRecordArrEnum(
            status,
            nameof(status),
            "pending",
            "distributed",
            "acknowledged",
            "expired",
            "revoked");
        var current = _documentDistributions[index];
        if (string.Equals(current.Status, "revoked", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(current.Status, "expired", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Document distribution {distributionId} is already {current.Status}.");
        }
        if (string.Equals(normalizedStatus, "acknowledged", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(current.Status, "distributed", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(current.Status, "acknowledged", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Document distribution {distributionId} cannot be acknowledged from {current.Status}.");
        }
        var updated = current with
        {
            Status = normalizedStatus
        };
        _documentDistributions[index] = updated;
        AppendControlledDocumentAuditTrail(
            current.ControlledDocumentId,
            CreateControlledDocumentAuditTrailEntry(
                normalizedStatus,
                actorPersonId,
                $"Distribution {distributionId} {reason}."));
        return updated;
    }

    public IReadOnlyList<RecordArrAccessPolicyResponse> GetAccessPolicies()
    {
        lock (_gate)
        {
            return _accessPolicies.ToArray();
        }
    }

    public RecordArrAccessPolicyResponse CreateAccessPolicy(
        string recordId,
        string policyType,
        string status,
        IEnumerable<string> readRules,
        IEnumerable<string> writeRules,
        IEnumerable<string> downloadRules,
        IEnumerable<string> shareRules,
        IEnumerable<string> exportRules,
        IEnumerable<string> purgeRules,
        string createdByPersonId)
    {
        lock (_gate)
        {
            var normalizedPolicyType = NormalizeRecordArrEnum(
                policyType,
                nameof(policyType),
                "default",
                "restricted",
                "legal_hold",
                "product_scoped",
                "public_link",
                "external_share");
            var normalizedStatus = NormalizeRecordArrEnum(
                status,
                nameof(status),
                "active",
                "inactive",
                "superseded");
            var policy = new RecordArrAccessPolicyResponse(
                $"acc-{Guid.NewGuid():N}"[..12],
                recordId,
                normalizedPolicyType,
                normalizedStatus,
                readRules.ToArray(),
                writeRules.ToArray(),
                downloadRules.ToArray(),
                shareRules.ToArray(),
                exportRules.ToArray(),
                purgeRules.ToArray());
            _accessPolicies.Add(policy);
            AddAccessLog(recordId, "access_policy.created", "allowed", createdByPersonId, null, null, null, null, $"{normalizedPolicyType}:{normalizedStatus}");
            return policy;
        }
    }

    public RecordArrAccessPolicyResponse UpdateAccessPolicy(
        string accessPolicyId,
        string recordId,
        string policyType,
        string status,
        IEnumerable<string> readRules,
        IEnumerable<string> writeRules,
        IEnumerable<string> downloadRules,
        IEnumerable<string> shareRules,
        IEnumerable<string> exportRules,
        IEnumerable<string> purgeRules,
        string updatedByPersonId)
    {
        lock (_gate)
        {
            var index = _accessPolicies.FindIndex(policy => string.Equals(policy.AccessPolicyId, accessPolicyId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Access policy {accessPolicyId} not found.");
            }

            var normalizedPolicyType = NormalizeRecordArrEnum(
                policyType,
                nameof(policyType),
                "default",
                "restricted",
                "legal_hold",
                "product_scoped",
                "public_link",
                "external_share");
            var normalizedStatus = NormalizeRecordArrEnum(
                status,
                nameof(status),
                "active",
                "inactive",
                "superseded");
            var updated = new RecordArrAccessPolicyResponse(
                accessPolicyId,
                recordId,
                normalizedPolicyType,
                normalizedStatus,
                readRules.ToArray(),
                writeRules.ToArray(),
                downloadRules.ToArray(),
                shareRules.ToArray(),
                exportRules.ToArray(),
                purgeRules.ToArray());
            _accessPolicies[index] = updated;
            AddAccessLog(recordId, "access_policy.updated", "allowed", updatedByPersonId, null, null, null, null, $"{normalizedPolicyType}:{normalizedStatus}");
            return updated;
        }
    }

    public IReadOnlyList<RecordArrAccessGrantResponse> GetAccessGrants()
    {
        lock (_gate)
        {
            return _accessGrants.ToArray();
        }
    }

    public IReadOnlyList<RecordArrAccessGrantResponse> RefreshAccessGrants()
    {
        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            for (var i = 0; i < _accessGrants.Count; i++)
            {
                var current = _accessGrants[i];
                if (!string.Equals(current.Status, "active", StringComparison.OrdinalIgnoreCase) ||
                    !current.ExpiresAt.HasValue ||
                    current.ExpiresAt > now)
                {
                    continue;
                }

                _accessGrants[i] = current with
                {
                    Status = "expired",
                    RevokedAt = current.ExpiresAt,
                    RevokeReason = $"Expired at {current.ExpiresAt:O}"
                };
                AddAccessLog(current.RecordId, "access_grant.expired", "allowed", current.GrantedByPersonId, null, null, null, null, "grant-expired");
            }

            return _accessGrants.ToArray();
        }
    }

    public RecordArrAccessGrantResponse CreateAccessGrant(string recordId, string granteeType, string granteeRef, string permission, string grantedByPersonId, DateTimeOffset? expiresAt)
    {
        lock (_gate)
        {
            var normalizedGranteeType = NormalizeRecordArrEnum(
                granteeType,
                nameof(granteeType),
                "person",
                "role",
                "product",
                "service_client",
                "external_link");
            var normalizedPermission = NormalizeRecordArrEnum(
                permission,
                nameof(permission),
                "read",
                "download",
                "upload_new_version",
                "approve",
                "classify",
                "map_evidence",
                "export",
                "share",
                "archive",
                "purge");
            var grant = new RecordArrAccessGrantResponse(
                $"grant-{Guid.NewGuid():N}"[..12],
                recordId,
                normalizedGranteeType,
                granteeRef,
                normalizedPermission,
                "active",
                grantedByPersonId,
                DateTimeOffset.UtcNow,
                expiresAt,
                null,
                null);
            _accessGrants.Add(grant);
            return grant;
        }
    }

    public RecordArrAccessGrantResponse RevokeAccessGrant(string accessGrantId, string revokedByPersonId, string? revokeReason)
    {
        lock (_gate)
        {
            var index = _accessGrants.FindIndex(grant => string.Equals(grant.AccessGrantId, accessGrantId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Access grant {accessGrantId} not found.");
            }

            var current = _accessGrants[index];
            var updated = current with
            {
                Status = "revoked",
                RevokedAt = DateTimeOffset.UtcNow,
                RevokeReason = revokeReason ?? $"Revoked by {revokedByPersonId}"
            };
            _accessGrants[index] = updated;
            return updated;
        }
    }

    public IReadOnlyList<RecordArrExternalShareResponse> GetExternalShares()
    {
        lock (_gate)
        {
            return _externalShares.OrderByDescending(share => share.CreatedAt).ToArray();
        }
    }

    public IReadOnlyList<RecordArrExternalShareResponse> RefreshExternalShares()
    {
        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            for (var i = 0; i < _externalShares.Count; i++)
            {
                var current = _externalShares[i];
                if (string.Equals(current.Status, "revoked", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(current.Status, "expired", StringComparison.OrdinalIgnoreCase) ||
                    !current.ExpiresAt.HasValue ||
                    current.ExpiresAt > now)
                {
                    continue;
                }

                _externalShares[i] = current with
                {
                    Status = "expired"
                };
                AddAccessLog(current.RecordId, "external_share.expired", "allowed", current.CreatedByPersonId, null, current.ExternalShareId, null, null, "share-expired");
            }

            return _externalShares.OrderByDescending(share => share.CreatedAt).ToArray();
        }
    }

    public IReadOnlyList<RecordArrRedactionResponse> GetRedactions()
    {
        lock (_gate)
        {
            return _redactions.OrderByDescending(redaction => redaction.RedactedAt).ToArray();
        }
    }

    public IReadOnlyList<RecordArrDisposalReviewResponse> GetDisposalReviews()
    {
        lock (_gate)
        {
            return _disposalReviews.ToArray();
        }
    }

    public RecordArrDisposalReviewResponse CompleteDisposalReview(string disposalReviewId, string status, string? reviewedByPersonId, string? decisionReason)
    {
        lock (_gate)
        {
            var index = _disposalReviews.FindIndex(review => string.Equals(review.DisposalReviewId, disposalReviewId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Disposal review {disposalReviewId} not found.");
            }

            var normalizedStatus = NormalizeRecordArrEnum(
                status,
                nameof(status),
                "pending",
                "approved",
                "rejected",
                "completed",
                "canceled");
            var current = _disposalReviews[index];
            var updated = current with
            {
                Status = normalizedStatus,
                ReviewedByPersonId = reviewedByPersonId ?? current.ReviewedByPersonId,
                ReviewedAt = DateTimeOffset.UtcNow,
                DecisionReason = decisionReason ?? current.DecisionReason,
                CompletedAt = normalizedStatus is "approved" or "rejected" or "completed" ? DateTimeOffset.UtcNow : current.CompletedAt
            };
            _disposalReviews[index] = updated;
            ApplyDisposalReviewOutcome(updated);
            return updated;
        }
    }

    private void ApplyDisposalReviewOutcome(RecordArrDisposalReviewResponse review)
    {
        var retentionIndex = _retentionStatuses.FindIndex(status => string.Equals(status.RetentionStatusId, review.RetentionStatusRef, StringComparison.OrdinalIgnoreCase));
        if (retentionIndex < 0)
        {
            return;
        }

        var activeHold = _legalHolds.FirstOrDefault(hold =>
            string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase) &&
            hold.RecordRefs.Any(recordRef => string.Equals(recordRef, review.RecordId, StringComparison.OrdinalIgnoreCase)));

        if (activeHold is not null)
        {
            _retentionStatuses[retentionIndex] = _retentionStatuses[retentionIndex] with
            {
                Status = "blocked_by_legal_hold"
            };
            return;
        }

        if (!string.Equals(review.Status, "approved", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(review.Status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var actorPersonId = review.ReviewedByPersonId ?? review.RequestedByPersonId;
        var targetRetentionStatus = review.ProposedAction.Trim().ToLowerInvariant() switch
        {
            "archive" => "archived",
            "purge" => "purged",
            "retain" => "indefinite",
            "anonymize" => "indefinite",
            _ => _retentionStatuses[retentionIndex].Status
        };

        _retentionStatuses[retentionIndex] = _retentionStatuses[retentionIndex] with
        {
            Status = targetRetentionStatus
        };

        if (review.ProposedAction.Trim().Equals("archive", StringComparison.OrdinalIgnoreCase))
        {
            ArchiveRecord(review.RecordId, actorPersonId);
        }
        else if (review.ProposedAction.Trim().Equals("purge", StringComparison.OrdinalIgnoreCase))
        {
            PurgeRecord(review.RecordId, actorPersonId);
        }
    }

    public RecordArrControlledDocumentResponse? GetControlledDocument(string controlledDocumentId)
    {
        lock (_gate)
        {
            return _controlledDocuments.FirstOrDefault(document => string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public RecordArrControlledDocumentResponse CreateControlledDocument(
        string title,
        string description,
        string documentClass,
        string documentType,
        string documentSubtype,
        string ownerPersonId,
        string departmentOrgUnitId,
        string staffarrSiteId,
        bool acknowledgementRequired)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(ownerPersonId) || string.IsNullOrWhiteSpace(departmentOrgUnitId) || string.IsNullOrWhiteSpace(staffarrSiteId))
            {
                throw new InvalidOperationException("Controlled document creation requires an owner, department, and site.");
            }

            var normalizedDocumentClass = NormalizeDocumentClassKey(documentClass, nameof(documentClass));
            var normalizedDocumentType = NormalizeRequiredDocumentField(documentType, nameof(documentType));
            var normalizedDocumentSubtype = NormalizeRequiredDocumentField(documentSubtype, nameof(documentSubtype));
            var document = new RecordArrControlledDocumentResponse(
                $"doc-{Guid.NewGuid():N}"[..12],
                $"DOC-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                _records[0].RecordId,
                title,
                description,
                normalizedDocumentClass,
                normalizedDocumentType,
                normalizedDocumentSubtype,
                normalizedDocumentSubtype,
                "draft",
                ownerPersonId,
                departmentOrgUnitId,
                staffarrSiteId,
                $"ver-{Guid.NewGuid():N}"[..8],
                180,
                DateTimeOffset.UtcNow.AddDays(180),
                null,
                null,
                null,
                null,
                acknowledgementRequired,
                Array.Empty<string>(),
                [
                    new RecordArrAuditTrailEntryResponse(
                        $"aud-{Guid.NewGuid():N}"[..12],
                        "created",
                        ownerPersonId,
                        DateTimeOffset.UtcNow,
                        "Controlled document created.")
                ]);
            _controlledDocuments.Add(document);
            return document;
        }
    }

    public RecordArrControlledDocumentVersionResponse CreateDocumentVersion(string controlledDocumentId, string fileName, string createdByPersonId, string? changeSummary)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("Document version fileName is required.");
            }

            if (string.IsNullOrWhiteSpace(createdByPersonId))
            {
                throw new InvalidOperationException("Document version createdByPersonId is required.");
            }

            var controlledDocument = _controlledDocuments.First(document => string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));
            var file = CreateFileObject(
                ResolveRecordTenantId(controlledDocument.RecordId),
                controlledDocument.RecordId,
                fileName,
                "application/pdf",
                createdByPersonId,
                storageProvider: "generated",
                storageKey: $"recordarr/controlled-documents/{controlledDocumentId}/{fileName}",
                sizeBytes: 256_000,
                pageCount: 1,
                attachToRecord: true,
                setAsCurrentFile: true);
            var version = new RecordArrControlledDocumentVersionResponse(
                $"ver-{Guid.NewGuid():N}"[..8],
                controlledDocumentId,
                _documentVersions.Count + 1,
                $"v{_documentVersions.Count + 1}",
                "draft",
                fileName,
                DateTimeOffset.UtcNow,
                createdByPersonId,
                null,
                null,
                null,
                null,
                null,
                changeSummary,
                _documentVersions.LastOrDefault()?.VersionId,
                null,
                file.FileId);
            _documentVersions.Add(version);
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                new RecordArrAuditTrailEntryResponse(
                    $"aud-{Guid.NewGuid():N}"[..12],
                    "version_created",
                    createdByPersonId,
                    DateTimeOffset.UtcNow,
                    $"Created version {version.VersionLabel}."));
            return version;
        }
    }

    public RecordArrControlledDocumentVersionResponse PromoteDocumentVersion(string controlledDocumentId, string versionId, string approvedByPersonId, DateTimeOffset? effectiveAt)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(approvedByPersonId))
            {
                throw new InvalidOperationException("Document version approvedByPersonId is required.");
            }

            var versionIndex = _documentVersions.FindIndex(version =>
                string.Equals(version.VersionId, versionId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(version.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));

            if (versionIndex < 0)
            {
                throw new InvalidOperationException($"Document version {versionId} not found.");
            }

            var documentIndex = _controlledDocuments.FindIndex(document => string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));
            if (documentIndex < 0)
            {
                throw new InvalidOperationException($"Controlled document {controlledDocumentId} not found.");
            }

            var now = DateTimeOffset.UtcNow;
            var promoted = _documentVersions[versionIndex] with
            {
                Status = "effective",
                ApprovedAt = now,
                ApprovedByPersonId = approvedByPersonId,
                EffectiveAt = effectiveAt ?? now
            };
            _documentVersions[versionIndex] = promoted;

            var document = _controlledDocuments[documentIndex];
            var previousVersionId = document.CurrentVersionId;
            var updatedDocument = document with
            {
                Status = "effective",
                CurrentVersionId = versionId,
                EffectiveAt = promoted.EffectiveAt,
                NextReviewAt = promoted.EffectiveAt?.AddDays(document.ReviewIntervalDays)
            };
            _controlledDocuments[documentIndex] = updatedDocument;

            if (!string.IsNullOrWhiteSpace(previousVersionId) && !string.Equals(previousVersionId, versionId, StringComparison.OrdinalIgnoreCase))
            {
                var previousIndex = _documentVersions.FindIndex(version => string.Equals(version.VersionId, previousVersionId, StringComparison.OrdinalIgnoreCase));
                if (previousIndex >= 0)
                {
                    _documentVersions[previousIndex] = _documentVersions[previousIndex] with
                    {
                        Status = "superseded",
                        SupersededAt = now,
                        NextVersionRef = versionId
                    };
                }
            }

            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                new RecordArrAuditTrailEntryResponse(
                    $"aud-{Guid.NewGuid():N}"[..12],
                    "version_promoted",
                    approvedByPersonId,
                    now,
                    $"Promoted version {promoted.VersionLabel} to effective."));

            return promoted;
        }
    }

    public IReadOnlyList<RecordArrControlledDocumentResponse> RefreshControlledDocumentWorkflows()
    {
        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;

            for (var i = 0; i < _controlledDocuments.Count; i++)
            {
                var current = _controlledDocuments[i];
                if (!string.Equals(current.Status, "effective", StringComparison.OrdinalIgnoreCase) ||
                    !current.NextReviewAt.HasValue ||
                    current.NextReviewAt > now)
                {
                    continue;
                }

                var updated = current with { Status = "review" };
                _controlledDocuments[i] = updated;
                AppendControlledDocumentAuditTrail(
                    current.ControlledDocumentId,
                    CreateControlledDocumentAuditTrailEntry(
                        "periodic_review_due",
                        "system",
                        $"Periodic review became due at {current.NextReviewAt:O}."));
            }

            for (var i = 0; i < _documentAcknowledgements.Count; i++)
            {
                var current = _documentAcknowledgements[i];
                if (!string.Equals(current.Status, "pending", StringComparison.OrdinalIgnoreCase) ||
                    !current.DueAt.HasValue ||
                    current.DueAt > now)
                {
                    continue;
                }

                var updated = current with { Status = "overdue" };
                _documentAcknowledgements[i] = updated;
                AppendControlledDocumentAuditTrail(
                    current.ControlledDocumentId,
                    CreateControlledDocumentAuditTrailEntry(
                        "acknowledgement_overdue",
                        "system",
                        $"Acknowledgement {current.AcknowledgementId} became overdue at {current.DueAt:O}."));
            }

            return _controlledDocuments.OrderBy(document => document.DocumentNumber).ToArray();
        }
    }

    public RecordArrControlledDocumentResponse UpdateControlledDocumentStatus(string controlledDocumentId, string status, string updatedByPersonId)
    {
        lock (_gate)
        {
            var index = _controlledDocuments.FindIndex(document => string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Controlled document {controlledDocumentId} not found.");
            }

            var normalizedStatus = NormalizeRecordArrEnum(
                status,
                nameof(status),
                "draft",
                "review",
                "approved",
                "effective",
                "superseded",
                "obsolete",
                "archived");
            var updated = _controlledDocuments[index] with
            {
                Status = normalizedStatus,
                NextReviewAt = normalizedStatus is "archived" or "obsolete" ? null : _controlledDocuments[index].NextReviewAt
            };
            _controlledDocuments[index] = updated;
            if (normalizedStatus is "archived" or "obsolete")
            {
                ArchiveControlledDocumentVersions(controlledDocumentId);
            }
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    normalizedStatus,
                    updatedByPersonId,
                    $"Controlled document marked as {normalizedStatus}."));
            return updated;
        }
    }

    private void ArchiveControlledDocumentVersions(string controlledDocumentId)
    {
        for (var i = 0; i < _documentVersions.Count; i++)
        {
            var current = _documentVersions[i];
            if (!string.Equals(current.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.Equals(current.Status, "superseded", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(current.Status, "rejected", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _documentVersions[i] = current with
            {
                Status = "archived"
            };
        }
    }

    public RecordArrControlledDocumentResponse SupersedeControlledDocument(string controlledDocumentId, string supersededByDocumentRef, string supersededByPersonId)
    {
        lock (_gate)
        {
            var sourceIndex = _controlledDocuments.FindIndex(document => string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));
            if (sourceIndex < 0)
            {
                throw new InvalidOperationException($"Controlled document {controlledDocumentId} not found.");
            }

            var replacementIndex = _controlledDocuments.FindIndex(document => string.Equals(document.ControlledDocumentId, supersededByDocumentRef, StringComparison.OrdinalIgnoreCase));
            if (replacementIndex < 0)
            {
                throw new InvalidOperationException($"Replacement controlled document {supersededByDocumentRef} not found.");
            }

            var source = _controlledDocuments[sourceIndex];
            var replacement = _controlledDocuments[replacementIndex];
            var now = DateTimeOffset.UtcNow;

            var updatedSource = source with
            {
                Status = "superseded",
                SupersededByDocumentRef = replacement.ControlledDocumentId,
                NextReviewAt = null
            };
            var updatedReplacement = replacement with
            {
                SupersedesDocumentRef = source.ControlledDocumentId,
                EffectiveAt = replacement.EffectiveAt ?? now
            };

            _controlledDocuments[sourceIndex] = updatedSource;
            _controlledDocuments[replacementIndex] = updatedReplacement;
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "superseded",
                    supersededByPersonId,
                    $"Superseded by {replacement.ControlledDocumentId}."));
            AppendControlledDocumentAuditTrail(
                replacement.ControlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "supersedes",
                    supersededByPersonId,
                    $"Supersedes {source.ControlledDocumentId}."));
            return updatedSource;
        }
    }

    public RecordArrDocumentReviewResponse RequestDocumentReview(string controlledDocumentId, string versionId, string reviewType, string requestedByPersonId, string reviewerPersonId, DateTimeOffset? dueAt)
    {
        lock (_gate)
        {
            var documentIndex = _controlledDocuments.FindIndex(document => string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));
            if (documentIndex < 0)
            {
                throw new InvalidOperationException($"Controlled document {controlledDocumentId} not found.");
            }

            var normalizedReviewType = NormalizeRecordArrEnum(
                reviewType,
                nameof(reviewType),
                "approval",
                "periodic_review",
                "change_review",
                "compliance_review",
                "quality_review",
                "legal_review");
            var review = new RecordArrDocumentReviewResponse(
                $"drev-{Guid.NewGuid():N}"[..12],
                controlledDocumentId,
                versionId,
                normalizedReviewType,
                "pending",
                requestedByPersonId,
                reviewerPersonId,
                DateTimeOffset.UtcNow,
                dueAt,
                null,
                null,
                null);
            _documentReviews.Add(review);
            UpdateDocumentVersionStatus(controlledDocumentId, versionId, "review");
            _controlledDocuments[documentIndex] = _controlledDocuments[documentIndex] with { Status = "review" };
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "submitted_for_review",
                    requestedByPersonId,
                    $"Requested {reviewType} review for version {versionId}."));
            return review;
        }
    }

    public RecordArrDocumentReviewResponse CompleteDocumentReview(string reviewId, string status, string? decisionReason, string? comments)
    {
        lock (_gate)
        {
            var index = _documentReviews.FindIndex(review => string.Equals(review.DocumentReviewId, reviewId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Document review {reviewId} not found.");
            }

            var normalizedStatus = NormalizeRecordArrEnum(
                status,
                nameof(status),
                "pending",
                "in_review",
                "approved",
                "rejected",
                "changes_requested",
                "canceled");
            var current = _documentReviews[index];
            var updated = current with
            {
                Status = normalizedStatus,
                ReviewedAt = DateTimeOffset.UtcNow,
                DecisionReason = decisionReason,
                Comments = comments
            };
            _documentReviews[index] = updated;

            var documentIndex = _controlledDocuments.FindIndex(document => string.Equals(document.ControlledDocumentId, current.ControlledDocumentId, StringComparison.OrdinalIgnoreCase));
            if (documentIndex >= 0)
            {
                var document = _controlledDocuments[documentIndex];
                var completedAt = updated.ReviewedAt ?? DateTimeOffset.UtcNow;
                if (normalizedStatus == "approved")
                {
                    UpdateDocumentVersionStatus(current.ControlledDocumentId, current.VersionId, "approved");
                    _controlledDocuments[documentIndex] = document with
                    {
                        Status = "approved"
                    };
                }
                else if (normalizedStatus is "rejected" or "changes_requested")
                {
                    UpdateDocumentVersionStatus(current.ControlledDocumentId, current.VersionId, normalizedStatus == "rejected" ? "rejected" : "review");
                    _controlledDocuments[documentIndex] = document with { Status = "review" };
                }
            }

            var auditAction = normalizedStatus switch
            {
                "approved" => "review_approved",
                "rejected" => "review_rejected",
                "changes_requested" => "review_changes_requested",
                _ => "review_completed"
            };
            AppendControlledDocumentAuditTrail(
                current.ControlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    auditAction,
                    current.ReviewerPersonId,
                    $"Review {reviewId} completed with status {status}."));
            return updated;
        }
    }

    private void UpdateDocumentVersionStatus(string controlledDocumentId, string versionId, string status)
    {
        var versionIndex = _documentVersions.FindIndex(version =>
            string.Equals(version.VersionId, versionId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(version.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));

        if (versionIndex < 0)
        {
            return;
        }

        var normalizedStatus = NormalizeRecordArrEnum(
            status,
            nameof(status),
            "draft",
            "review",
            "approved",
            "effective",
            "superseded",
            "rejected",
            "archived");

        _documentVersions[versionIndex] = _documentVersions[versionIndex] with
        {
            Status = normalizedStatus,
            SubmittedForReviewAt = normalizedStatus == "review" && _documentVersions[versionIndex].SubmittedForReviewAt is null
                ? DateTimeOffset.UtcNow
                : _documentVersions[versionIndex].SubmittedForReviewAt,
            ApprovedAt = normalizedStatus == "approved" ? DateTimeOffset.UtcNow : _documentVersions[versionIndex].ApprovedAt,
            SupersededAt = normalizedStatus == "superseded" ? DateTimeOffset.UtcNow : _documentVersions[versionIndex].SupersededAt
        };
    }

    public RecordArrExternalShareResponse CreateExternalShare(string recordId, string recipientName, string recipientEmail, string sharePurpose, IEnumerable<string> allowedActions, string createdByPersonId)
    {
        lock (_gate)
        {
            var normalizedSharePurpose = NormalizeRecordArrEnum(
                sharePurpose,
                nameof(sharePurpose),
                "customer_view",
                "supplier_response",
                "auditor_access",
                "legal_review",
                "public_download",
                "temporary_upload");
            var normalizedAllowedActions = allowedActions.Select(action => NormalizeRecordArrEnum(
                action,
                nameof(allowedActions),
                "view",
                "download",
                "upload",
                "sign")).ToArray();
            var share = new RecordArrExternalShareResponse(
                $"share-{Guid.NewGuid():N}"[..12],
                $"SHARE-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                recordId,
                normalizedSharePurpose,
                "created",
                recipientName,
                recipientEmail,
                normalizedAllowedActions,
                DateTimeOffset.UtcNow,
                createdByPersonId,
                DateTimeOffset.UtcNow.AddDays(2),
                null,
                null,
                DateTimeOffset.UtcNow,
                0);
            _externalShares.Add(share);
            AddAccessLog(recordId, "external_share.created", "allowed", createdByPersonId, null, share.ExternalShareId, null, null, "external-share-created");
            return share;
        }
    }

    public RecordArrExternalShareResponse RevokeExternalShare(string shareId, string revokedByPersonId)
    {
        lock (_gate)
        {
            var index = _externalShares.FindIndex(share => string.Equals(share.ExternalShareId, shareId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"External share {shareId} not found.");
            }

            var current = _externalShares[index];
            var updated = current with
            {
                Status = "revoked",
                RevokedAt = DateTimeOffset.UtcNow,
                RevokedByPersonId = revokedByPersonId
            };
            _externalShares[index] = updated;
            AddAccessLog(current.RecordId, "external_share.revoked", "allowed", revokedByPersonId, null, current.ExternalShareId, null, null, "external-share-revoked");
            return updated;
        }
    }

    public RecordArrExternalShareResponse ExpireExternalShare(string shareId, string expiredByPersonId)
    {
        lock (_gate)
        {
            var index = _externalShares.FindIndex(share => string.Equals(share.ExternalShareId, shareId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"External share {shareId} not found.");
            }

            var current = _externalShares[index];
            var updated = current with
            {
                Status = "expired",
                RevokedAt = DateTimeOffset.UtcNow,
                RevokedByPersonId = expiredByPersonId
            };
            _externalShares[index] = updated;
            AddAccessLog(current.RecordId, "external_share.expired", "allowed", expiredByPersonId, null, current.ExternalShareId, null, null, "external-share-expired");
            return updated;
        }
    }

    public RecordArrExternalShareResponse RecordExternalShareAccess(string shareId, string accessedByPersonId, string accessAction, string? sourceIp, string? userAgent)
    {
        lock (_gate)
        {
            var index = _externalShares.FindIndex(share => string.Equals(share.ExternalShareId, shareId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"External share {shareId} not found.");
            }

            var current = _externalShares[index];
            if (current.Status is "revoked" or "expired")
            {
                AddAccessLog(current.RecordId, "external_share.accessed", "denied", accessedByPersonId, null, current.ExternalShareId, sourceIp, userAgent, "external-share-status");
                throw new InvalidOperationException($"External share {shareId} is not active.");
            }

            if (current.ExpiresAt.HasValue && current.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                var expired = current with
                {
                    Status = "expired"
                };
                _externalShares[index] = expired;
                AddAccessLog(current.RecordId, "external_share.expired", "allowed", current.CreatedByPersonId, null, current.ExternalShareId, sourceIp, userAgent, "share-expired");
                AddAccessLog(current.RecordId, "external_share.accessed", "denied", accessedByPersonId, null, current.ExternalShareId, sourceIp, userAgent, "external-share-expired");
                throw new InvalidOperationException($"External share {shareId} has expired.");
            }

            var accessPolicy = _accessPolicies.FirstOrDefault(policy =>
                string.Equals(policy.RecordId, current.RecordId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(policy.Status, "active", StringComparison.OrdinalIgnoreCase));

            if (accessPolicy is not null && !IsExternalShareActionAllowedByPolicy(accessPolicy, accessAction))
            {
                AddAccessLog(current.RecordId, "external_share.accessed", "denied", accessedByPersonId, null, current.ExternalShareId, sourceIp, userAgent, "access_policy_denied");
                throw new InvalidOperationException($"External share {shareId} is denied by access policy.");
            }

            var nextStatus = current.Status == "created" ? "active" : current.Status;

            var now = DateTimeOffset.UtcNow;
            var updated = current with
            {
                Status = nextStatus,
                LastAccessedAt = now,
                AccessCount = current.AccessCount + 1
            };
            _externalShares[index] = updated;
            AddAccessLog(current.RecordId, "external_share.accessed", "allowed", accessedByPersonId, null, current.ExternalShareId, sourceIp, userAgent, accessAction);
            return updated;
        }
    }

    private static bool IsExternalShareActionAllowedByPolicy(RecordArrAccessPolicyResponse policy, string accessAction)
    {
        if (string.IsNullOrWhiteSpace(accessAction))
        {
            return false;
        }

        return accessAction.Trim().ToLowerInvariant() switch
        {
            "view" => policy.ReadRules.Count > 0,
            "download" => policy.DownloadRules.Count > 0,
            "upload" => policy.WriteRules.Count > 0,
            "sign" => policy.ShareRules.Count > 0 || policy.WriteRules.Count > 0,
            _ => policy.ReadRules.Count > 0 || policy.WriteRules.Count > 0 || policy.DownloadRules.Count > 0 || policy.ShareRules.Count > 0 || policy.ExportRules.Count > 0 || policy.PurgeRules.Count > 0
        };
    }

    private IReadOnlyList<string> BuildPackageRecordRefs(string recordRef, string? sourceObjectRef)
    {
        var refs = new List<string> { recordRef };
        var linkedRefs = _recordLinks
            .Where(link =>
                string.Equals(link.RecordId, recordRef, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(link.LinkedRecordId, recordRef, StringComparison.OrdinalIgnoreCase))
            .SelectMany(link => new[] { link.RecordId, link.LinkedRecordId })
            .Where(record => !string.IsNullOrWhiteSpace(record));

        refs.AddRange(linkedRefs!);
        return refs.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private IReadOnlyList<string> BuildPackageSourceObjectRefs(string sourceObjectRef, string recordRef, IReadOnlyList<string> packageRecordRefs)
    {
        var refs = new List<string>();
        if (!string.IsNullOrWhiteSpace(sourceObjectRef))
        {
            refs.Add(sourceObjectRef.Trim());
        }

        refs.AddRange(_recordLinks
            .Where(link =>
                packageRecordRefs.Contains(link.RecordId, StringComparer.OrdinalIgnoreCase) ||
                (link.LinkedRecordId is not null && packageRecordRefs.Contains(link.LinkedRecordId, StringComparer.OrdinalIgnoreCase)))
            .Where(link => !string.IsNullOrWhiteSpace(link.SourceObjectRef))
            .Select(link => link.SourceObjectRef!.Trim()));

        refs.AddRange(_recordLinks
            .Where(link => string.Equals(link.RecordId, recordRef, StringComparison.OrdinalIgnoreCase))
            .Where(link => !string.IsNullOrWhiteSpace(link.SourceObjectRef))
            .Select(link => link.SourceObjectRef!.Trim()));

        return refs.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private IReadOnlyList<RecordArrPackageManifestEntryResponse> BuildPackageRecordEntries(IReadOnlyList<string> recordRefs)
    {
        return recordRefs
            .Select(recordRef =>
            {
                var record = RequireRecord(recordRef);
                return new RecordArrPackageManifestEntryResponse(
                    $"mrec-{Guid.NewGuid():N}"[..12],
                    "record",
                    record.Title,
                    record.SourceProduct,
                    $"{record.SourceProduct}:{record.SourceObjectType}:{record.SourceObjectId}",
                    record.RecordId,
                    null,
                    record.Status,
                    ComputeChecksum($"{record.RecordId}|{record.Status}|{record.CurrentFileRef}|{record.VersionNumber}"));
            })
            .ToArray();
    }

    private IReadOnlyList<RecordArrPackageManifestEntryResponse> BuildPackageSourceObjectEntries(string sourceProduct, IReadOnlyList<string> sourceObjectRefs)
    {
        return sourceObjectRefs
            .Select(sourceObjectRef =>
            {
                var record = _records.FirstOrDefault(candidate => string.Equals($"{candidate.SourceProduct}:{candidate.SourceObjectType}:{candidate.SourceObjectId}", sourceObjectRef, StringComparison.OrdinalIgnoreCase));
                var displayName = record?.SourceObjectDisplayName ?? sourceObjectRef;
                var statusSnapshot = record?.Status ?? "active";
                return new RecordArrPackageManifestEntryResponse(
                    $"mobj-{Guid.NewGuid():N}"[..12],
                    "source_object",
                    displayName,
                    sourceProduct,
                    sourceObjectRef,
                    record?.RecordId,
                    null,
                    statusSnapshot,
                    ComputeChecksum($"{sourceObjectRef}|{displayName}|{statusSnapshot}"));
            })
            .ToArray();
    }

    private IReadOnlyList<RecordArrPackageManifestEntryResponse> BuildPackageRequirementEntries(string sourceProduct, string sourceObjectRef, IReadOnlyList<string> recordRefs)
    {
        var requirements = _evidenceMappings
            .Where(mapping =>
                string.Equals(mapping.SourceProduct, sourceProduct, StringComparison.OrdinalIgnoreCase) &&
                (string.Equals($"{mapping.SourceProduct}:{mapping.SourceObjectType}:{mapping.SourceObjectId}", sourceObjectRef, StringComparison.OrdinalIgnoreCase) ||
                 recordRefs.Contains(mapping.RecordId, StringComparer.OrdinalIgnoreCase)))
            .GroupBy(mapping => mapping.ComplianceRequirementRef, StringComparer.OrdinalIgnoreCase);

        var entries = new List<RecordArrPackageManifestEntryResponse>();
        foreach (var group in requirements)
        {
            var representative = group.First();
            var statusSnapshot = group.Any(mapping => string.Equals(mapping.Status, "confirmed", StringComparison.OrdinalIgnoreCase))
                ? "satisfied"
                : group.Any(mapping => string.Equals(mapping.Status, "rejected", StringComparison.OrdinalIgnoreCase))
                    ? "invalid"
                    : "warning";
            entries.Add(new RecordArrPackageManifestEntryResponse(
                $"mreq-{Guid.NewGuid():N}"[..12],
                "requirement",
                representative.ComplianceRequirementRef,
                representative.SourceProduct,
                $"{representative.SourceProduct}:{representative.SourceObjectType}:{representative.SourceObjectId}",
                representative.RecordId,
                representative.ComplianceRequirementRef,
                statusSnapshot,
                ComputeChecksum($"{representative.ComplianceRequirementRef}|{statusSnapshot}|{string.Join(",", group.Select(mapping => mapping.RecordId).Distinct(StringComparer.OrdinalIgnoreCase))}")));
        }

        return entries.ToArray();
    }

    private static string ComputePackageManifestChecksum(
        IReadOnlyList<RecordArrPackageManifestEntryResponse> recordEntries,
        IReadOnlyList<RecordArrPackageManifestEntryResponse> sourceObjectEntries,
        IReadOnlyList<RecordArrPackageManifestEntryResponse> requirementEntries)
    {
        var payload = string.Join(
            "|",
            recordEntries.Concat(sourceObjectEntries).Concat(requirementEntries).Select(entry => entry.Checksum));
        return ComputeChecksum(payload);
    }

    private static string ComputeChecksum(string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private List<RecordArrEvidenceCoverageResponse> BuildEvidenceCoverage()
    {
        var coverage = new List<RecordArrEvidenceCoverageResponse>();
        foreach (var group in _evidenceMappings.GroupBy(mapping =>
                     $"{mapping.SourceProduct}|{mapping.SourceObjectType}|{mapping.SourceObjectId}|{mapping.ComplianceRequirementRef}",
                     StringComparer.OrdinalIgnoreCase))
        {
            var recordRefs = group.Select(mapping => mapping.RecordId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var tenantId = ResolveRecordTenantId(recordRefs.FirstOrDefault() ?? string.Empty);
            var statuses = group.Select(mapping => mapping.Status.Trim().ToLowerInvariant()).ToArray();
            var status = statuses.Contains("confirmed")
                ? "satisfied"
                : statuses.Contains("rejected")
                    ? "invalid"
                    : statuses.Contains("expired")
                        ? "expired"
                        : statuses.Contains("suggested")
                            ? "warning"
                            : "unknown";
            var missingEvidenceTypes = status == "warning"
                ? group.Select(mapping => mapping.EvidenceTypeKey).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
                : Array.Empty<string>();
            var invalidRecordRefs = status is "invalid" or "expired"
                ? recordRefs
                : Array.Empty<string>();
            var sourceObjectRef = $"{group.First().SourceProduct}:{group.First().SourceObjectType}:{group.First().SourceObjectId}";
            var seed = $"{group.First().SourceProduct}-{group.First().SourceObjectType}-{group.First().SourceObjectId}-{group.First().ComplianceRequirementRef}".ToLowerInvariant();
            coverage.Add(new RecordArrEvidenceCoverageResponse(
                $"cov-{seed}"[..Math.Min(48, $"cov-{seed}".Length)],
                tenantId,
                group.First().SourceProduct,
                sourceObjectRef,
                group.First().ComplianceRequirementRef,
                status,
                recordRefs,
                missingEvidenceTypes,
                invalidRecordRefs,
                DateTimeOffset.UtcNow,
                $"eval-{seed}"[..Math.Min(48, $"eval-{seed}".Length)]));
        }

        return coverage;
    }

    public RecordArrRedactionResponse CreateRedaction(string sourceRecordId, string redactedRecordId, string redactionReason, string redactedByPersonId, IEnumerable<string> redactionRules)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(sourceRecordId))
            {
                throw new InvalidOperationException("Source record id is required.");
            }
            if (string.IsNullOrWhiteSpace(redactedRecordId))
            {
                throw new InvalidOperationException("Redacted record id is required.");
            }
            if (_records.Any(record => string.Equals(record.RecordId, redactedRecordId, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Redacted record {redactedRecordId} already exists.");
            }

            var sourceRecord = RequireRecord(sourceRecordId);
            var normalizedRedactionReason = NormalizeRecordArrEnum(
                redactionReason,
                nameof(redactionReason),
                "privacy",
                "legal",
                "customer",
                "supplier",
                "internal",
                "security");

            CreateRedactedRecordCopy(sourceRecord, redactedRecordId, normalizedRedactionReason, redactedByPersonId);
            var redaction = new RecordArrRedactionResponse(
                $"red-{Guid.NewGuid():N}"[..12],
                sourceRecord.RecordId,
                redactedRecordId,
                normalizedRedactionReason,
                "completed",
                redactedByPersonId,
                DateTimeOffset.UtcNow,
                redactionRules.ToArray());
            _redactions.Add(redaction);
            return redaction;
        }
    }

    private RecordArrRecordResponse CreateRedactedRecordCopy(
        RecordArrRecordResponse sourceRecord,
        string redactedRecordId,
        string redactionReason,
        string redactedByPersonId)
    {
        var sourceFile = FindFile(sourceRecord.CurrentFileRef);
        var redactedFileName = BuildRedactedFileName(sourceRecord.CurrentFileName);
        var redactedMimeType = sourceFile?.MimeType ?? sourceRecord.CurrentMimeType;
        var redactedFile = CreateFileObject(
            sourceFile?.TenantId ?? ResolveRecordTenantId(sourceRecord.RecordId),
            redactedRecordId,
            redactedFileName,
            redactedMimeType,
            redactedByPersonId,
            storageProvider: "generated",
            storageKey: $"recordarr/redactions/{redactedRecordId}/{redactedFileName}",
            sizeBytes: sourceFile?.SizeBytes,
            pageCount: sourceFile?.PageCount,
            imageWidth: sourceFile?.ImageWidth,
            imageHeight: sourceFile?.ImageHeight,
            durationSeconds: sourceFile?.DurationSeconds,
            attachToRecord: false,
            setAsCurrentFile: false);

        var now = DateTimeOffset.UtcNow;
        var redactedRecord = new RecordArrRecordResponse(
            redactedRecordId,
            $"REC-{now:yyMMdd-HHmmss}",
            $"{sourceRecord.Title} (Redacted)",
            $"{sourceRecord.Description} Redacted copy created for {redactionReason}.",
            sourceRecord.RecordType,
            sourceRecord.DocumentClass,
            sourceRecord.DocumentType,
            sourceRecord.DocumentSubtype,
            "active",
            sourceRecord.Classification,
            sourceRecord.SourceProduct,
            sourceRecord.SourceObjectType,
            sourceRecord.SourceObjectId,
            sourceRecord.SourceObjectDisplayName,
            sourceRecord.OwnerPersonId,
            redactedByPersonId,
            now,
            now,
            sourceRecord.ExpiresAt,
            redactedFile.OriginalFilename,
            redactedFile.MimeType,
            1,
            [..sourceRecord.Tags, "redacted"],
            redactedFile.FileId,
            [redactedFile.FileId],
            redactedFile.FileId,
            [$"{sourceRecord.SourceProduct}:{sourceRecord.SourceObjectType}:{sourceRecord.SourceObjectId}", sourceRecord.RecordId],
            sourceRecord.MetadataRefs,
            [redactedFile.FileId],
            [],
            [],
            [],
            [],
            null,
            null,
            [],
            null,
            [],
            [],
            null,
            null);

        _records.Add(redactedRecord);
        foreach (var sourcePolicy in _accessPolicies.Where(policy => string.Equals(policy.RecordId, sourceRecord.RecordId, StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            _accessPolicies.Add(sourcePolicy with
            {
                AccessPolicyId = $"acc-{Guid.NewGuid():N}"[..12],
                RecordId = redactedRecordId
            });
        }

        foreach (var sourceGrant in _accessGrants.Where(grant => string.Equals(grant.RecordId, sourceRecord.RecordId, StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            _accessGrants.Add(sourceGrant with
            {
                AccessGrantId = $"agr-{Guid.NewGuid():N}"[..12],
                RecordId = redactedRecordId
            });
        }

        _recordLinks.Add(new RecordArrRecordLinkResponse(
            $"rlk-{Guid.NewGuid():N}"[..12],
            redactedRecordId,
            sourceRecord.RecordId,
            null,
            "redacted_from",
            now,
            redactedByPersonId));

        return ProjectRecord(redactedRecord);
    }

    private static string BuildRedactedFileName(string sourceFileName)
    {
        var trimmed = string.IsNullOrWhiteSpace(sourceFileName) ? "redacted-record.pdf" : sourceFileName.Trim();
        var extension = Path.GetExtension(trimmed);
        var baseName = Path.GetFileNameWithoutExtension(trimmed);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "redacted-record";
        }

        var suffix = string.IsNullOrWhiteSpace(extension)
            ? ".pdf"
            : extension;

        return $"{baseName}-redacted{suffix}";
    }

    public IReadOnlyList<RecordArrAccessLogResponse> GetAccessLogs(string? recordId = null)
    {
        lock (_gate)
        {
            var logs = string.IsNullOrWhiteSpace(recordId)
                ? _accessLogs
                : _accessLogs.Where(log => string.Equals(log.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            return logs.OrderByDescending(log => log.OccurredAt).ToArray();
        }
    }

    public RecordArrAccessLogResponse AddAccessLog(string recordId, string action, string result, string? actorPersonId, string? actorServiceClientId, string? externalShareId, string? sourceIp, string? userAgent, string? reasonCode)
    {
        lock (_gate)
        {
            var log = new RecordArrAccessLogResponse(
                $"alog-{Guid.NewGuid():N}"[..12],
                recordId,
                action,
                result,
                actorPersonId,
                actorServiceClientId,
                externalShareId,
                DateTimeOffset.UtcNow,
                sourceIp,
                userAgent,
                reasonCode);
            _accessLogs.Add(log);
            return log;
        }
    }

    public RecordArrPackageManifestResponse? GetManifest(string packageId)
    {
        lock (_gate)
        {
            return _manifests.FirstOrDefault(manifest => string.Equals(manifest.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public RecordArrDisposalReviewResponse CreateDisposalReview(string recordId, string retentionStatusRef, string proposedAction, string requestedByPersonId)
    {
        lock (_gate)
        {
            var normalizedProposedAction = NormalizeRecordArrEnum(
                proposedAction,
                nameof(proposedAction),
                "archive",
                "purge",
                "anonymize",
                "retain");
            var review = new RecordArrDisposalReviewResponse(
                $"disp-{Guid.NewGuid():N}"[..12],
                recordId,
                retentionStatusRef,
                normalizedProposedAction,
                "pending",
                DateTimeOffset.UtcNow,
                requestedByPersonId,
                null,
                null,
                null,
                null);
            _disposalReviews.Add(review);
            return review;
        }
    }
}
