using RecordArr.Api.Models;

namespace RecordArr.Api.Data;

public sealed class RecordArrStore
{
    private readonly object _gate = new();
    private readonly List<RecordArrRecordResponse> _records;
    private readonly List<RecordArrUploadSessionResponse> _uploadSessions;
    private readonly List<RecordArrScanProcessingResponse> _scans;
    private readonly List<RecordArrOcrResultResponse> _ocrResults;
    private readonly List<RecordArrExtractionResultResponse> _extractionResults;
    private readonly List<RecordArrEvidenceMappingResponse> _evidenceMappings;
    private readonly List<RecordArrPackageResponse> _packages;
    private readonly List<RecordArrPackageManifestResponse> _manifests;
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
    private readonly List<RecordArrAccessLogResponse> _accessLogs;

    public RecordArrStore()
    {
        var now = DateTimeOffset.UtcNow;

        _records =
        [
            new RecordArrRecordResponse(
                "rec-bol-001",
                "REC-260604-001",
                "Inbound BOL for delivery load",
                "Captured from RoutArr proof-of-delivery handoff.",
                "document",
                "bol",
                "active",
                "internal",
                "routarr",
                "trip",
                "trip-7781",
                "RT-7781",
                "person-route-lead",
                "person-route-lead",
                now.AddDays(-3),
                now.AddDays(27),
                now.AddDays(365),
                "bol-7781.pdf",
                "application/pdf",
                2,
                ["route", "evidence", "delivery"]),
            new RecordArrRecordResponse(
                "rec-sop-001",
                "REC-260604-002",
                "Warehouse safe handling procedure",
                "Controlled SOP for receiving and staging hazardous stock.",
                "document",
                "procedure",
                "review",
                "internal",
                "recordarr",
                "template",
                "sop-hazmat-01",
                "Hazmat Receiving Procedure",
                "person-doc-controller",
                "person-doc-controller",
                now.AddDays(-14),
                now.AddDays(1),
                now.AddDays(730),
                "hazmat-receiving-v3.pdf",
                "application/pdf",
                3,
                ["controlled", "procedure", "review"]),
        ];

        _uploadSessions =
        [
            new RecordArrUploadSessionResponse(
                "upl-001",
                "UPL-260604-001",
                "authenticated",
                "routarr",
                "trip",
                "trip-7781",
                "proof_of_delivery",
                "completed",
                true,
                true,
                true,
                now.AddHours(-4),
                now.AddHours(20),
                now.AddMinutes(-75),
                null,
                ["application/pdf", "image/jpeg"],
                5,
                25_000_000,
                ["rec-bol-001"]),
        ];

        _scans =
        [
            new RecordArrScanProcessingResponse(
                "scan-001",
                "rec-bol-001",
                "bol-7781.jpg",
                "completed",
                "bol",
                "edge:manual",
                "rec-bol-001",
                "ocr-001",
                "ext-001",
                0.94m,
                now.AddHours(-2),
                null),
        ];

        _ocrResults =
        [
            new RecordArrOcrResultResponse(
                "ocr-001",
                "rec-bol-001",
                "file-bol-001",
                "azure_document_intelligence",
                "completed",
                "en",
                0.93m,
                "Bill of lading number RT-7781 with delivery signature and pickup confirmation.",
                now.AddHours(-2),
                null),
        ];

        _extractionResults =
        [
            new RecordArrExtractionResultResponse(
                "ext-001",
                "rec-bol-001",
                "bol",
                "manual_review_required",
                [
                    new RecordArrExtractedFieldResponse("fld-001", "ext-001", "bol_number", "BOL Number", "RT-7781", "string", 0.98m, "unreviewed", null, null, null),
                    new RecordArrExtractedFieldResponse("fld-002", "ext-001", "delivery_signature", "Delivery Signature", "Avery Auditor", "string", 0.77m, "unreviewed", null, null, null),
                ],
                0.88m,
                now.AddHours(-2),
                null,
                null,
                "Fields with lower confidence need a quick human review."),
        ];

        _evidenceMappings =
        [
            new RecordArrEvidenceMappingResponse(
                "map-001",
                "rec-bol-001",
                "routarr",
                "trip",
                "trip-7781",
                "evidence_requirement.trip.pod",
                "proof_of_delivery",
                "confirmed",
                "user_confirmed",
                0.96m,
                "person-auditor",
                now.AddHours(-1),
                null,
                null,
                null,
                "Mapped by dispatch evidence review."),
        ];

        _packages =
        [
            new RecordArrPackageResponse(
                "pkg-001",
                "PKG-260604-001",
                "RoutArr closeout packet",
                "delivery",
                "complete",
                "routarr",
                ["trip-7781"],
                ["rec-bol-001"],
                "manifest-001",
                "rec-bol-001",
                null,
                now.AddHours(-1),
                now.AddHours(-1),
                null,
                null,
                now.AddDays(30)),
        ];

        _manifests =
        [
            new RecordArrPackageManifestResponse(
                "manifest-001",
                "pkg-001",
                1,
                now.AddHours(-1),
                [
                    new RecordArrPackageManifestEntryResponse("mrec-001", "record", "Inbound BOL for delivery load", "routarr", "trip-7781", "rec-bol-001", null, "active", "sha256-record-bol"),
                ],
                [
                    new RecordArrPackageManifestEntryResponse("mobj-001", "source_object", "RoutArr trip RT-7781", "routarr", "trip-7781", null, null, "closed", "sha256-source-trip"),
                ],
                [
                    new RecordArrPackageManifestEntryResponse("mreq-001", "requirement", "Proof of delivery", null, null, null, "evidence_requirement.trip.pod", "satisfied", "sha256-requirement-pod"),
                ],
                "sha256-manifest-001",
                "person-evidence-manager"),
        ];

        _retentionPolicies =
        [
            new RecordArrRetentionPolicyResponse(
                "ret-001",
                "default-delivery",
                "Delivery evidence retention",
                "Keeps delivery evidence for operational audit windows.",
                "delivery_record",
                "pod",
                "routarr",
                365,
                "days",
                "closure_at",
                "archive",
                true,
                "active",
                now.AddDays(-20),
                now.AddDays(-2)),
        ];

        _retentionStatuses =
        [
            new RecordArrRetentionStatusResponse(
                "rstat-001",
                "rec-bol-001",
                "ret-001",
                "active",
                now.AddDays(-3),
                now.AddDays(362),
                now.AddDays(330),
                null,
                null,
                null),
        ];

        _disposalReviews = [];

        _legalHolds =
        [
            new RecordArrLegalHoldResponse(
                "hold-001",
                "HOLD-260604-001",
                "Open audit hold",
                "Holds controlled documents while audit evidence is reviewed.",
                "active",
                "audit",
                ["record_type:document", "document_type:procedure"],
                ["rec-sop-001"],
                "compliancecore",
                "audit",
                "audit-901",
                now.AddDays(-1),
                "person-audit-admin",
                now.AddHours(-20),
                null,
                null,
                null),
        ];

        _controlledDocuments =
        [
            new RecordArrControlledDocumentResponse(
                "doc-001",
                "DOC-260604-001",
                "rec-sop-001",
                "Warehouse safe handling procedure",
                "Controlled SOP for receiving and staging hazardous stock.",
                "procedure",
                "review",
                "person-doc-controller",
                "org-receiving",
                "site-north-yard",
                "ver-002",
                180,
                now.AddDays(1),
                now.AddDays(-14),
                now.AddDays(730),
                null,
                null,
                true,
                ["rec-sop-001", "rec-bol-001"],
                [
                    new RecordArrAuditTrailEntryResponse(
                        "aud-001",
                        "created",
                        "person-doc-controller",
                        now.AddDays(-14),
                        "Controlled document created."),
                    new RecordArrAuditTrailEntryResponse(
                        "aud-002",
                        "version_created",
                        "person-doc-controller",
                        now.AddDays(-14),
                        "Initial version was created."),
                    new RecordArrAuditTrailEntryResponse(
                        "aud-003",
                        "submitted_for_review",
                        "person-doc-controller",
                        now.AddDays(-2),
                        "Review workflow started for the current version.")
                ]),
        ];

        _documentVersions =
        [
            new RecordArrControlledDocumentVersionResponse(
                "ver-001",
                "doc-001",
                1,
                "v1",
                "superseded",
                "hazmat-receiving-v1.pdf",
                now.AddDays(-14),
                "person-doc-controller",
                now.AddDays(-12),
                now.AddDays(-10),
                "person-doc-controller",
                now.AddDays(-14),
                now.AddDays(-7),
                "Initial release",
                null,
                "ver-002"),
            new RecordArrControlledDocumentVersionResponse(
                "ver-002",
                "doc-001",
                2,
                "v2",
                "review",
                "hazmat-receiving-v3.pdf",
                now.AddDays(-14),
                "person-doc-controller",
                now.AddDays(-2),
                null,
                null,
                now.AddDays(-14),
                null,
                "Added evidence capture and review steps",
                "ver-001",
                null),
        ];

        _documentReviews =
        [
            new RecordArrDocumentReviewResponse(
                "drev-001",
                "doc-001",
                "ver-002",
                "periodic_review",
                "in_review",
                "person-doc-controller",
                "person-quality-reviewer",
                now.AddDays(-2),
                now.AddDays(5),
                null,
                null,
                null),
        ];

        _documentDistributions =
        [
            new RecordArrDocumentDistributionResponse(
                "dist-001",
                "doc-001",
                "ver-002",
                "role",
                "quality-reviewer",
                "distributed",
                now.AddDays(-1),
                null,
                null),
        ];

        _documentAcknowledgements =
        [
            new RecordArrDocumentAcknowledgementResponse(
                "ack-001",
                "doc-001",
                "ver-002",
                "person-quality-reviewer",
                "pending",
                null,
                null,
                "I have read and will follow the controlled procedure.",
                now.AddDays(2)),
        ];

        _accessPolicies =
        [
            new RecordArrAccessPolicyResponse(
                "acc-001",
                "rec-bol-001",
                "product_scoped",
                "active",
                ["recordarr.records.read", "recordarr.files.download"],
                ["recordarr.records.update"],
                ["recordarr.files.download"],
                ["recordarr.external_shares.create"],
                ["recordarr.packages.export"],
                ["recordarr.records.purge"]),
        ];

        _accessGrants =
        [
            new RecordArrAccessGrantResponse(
                "grant-001",
                "rec-bol-001",
                "role",
                "evidence-manager",
                "read",
                "active",
                "person-doc-controller",
                now.AddHours(-6),
                now.AddDays(60),
                null,
                null),
        ];

        _externalShares =
        [
            new RecordArrExternalShareResponse(
                "share-001",
                "SHARE-260604-001",
                "rec-bol-001",
                "auditor_access",
                "active",
                "Avery Auditor",
                "avery.auditor@example.com",
                ["view", "download"],
                now.AddHours(-3),
                "person-doc-controller",
                now.AddDays(2),
                null,
                null,
                now.AddHours(-1),
                2),
        ];

        _redactions =
        [
            new RecordArrRedactionResponse(
                "red-001",
                "rec-bol-001",
                "rec-bol-001-redacted",
                "privacy",
                "completed",
                "person-doc-controller",
                now.AddHours(-2),
                ["mask:signature", "mask:phone"]),
        ];

        _accessLogs =
        [
            new RecordArrAccessLogResponse(
                "alog-001",
                "rec-bol-001",
                "view",
                "allowed",
                "person-quality-reviewer",
                null,
                null,
                now.AddMinutes(-75),
                "127.0.0.1",
                "Mozilla/5.0",
                "review"),
        ];
    }

    public RecordArrSessionResponse BuildSession(string userId, string personId, string tenantId, string tenantRoleKey, bool isPlatformAdmin, IEnumerable<string> entitlements) =>
        new(userId, personId, tenantId, $"session-{userId}", tenantRoleKey, isPlatformAdmin, "recordarr", true, entitlements.ToArray());

    public RecordArrDashboardResponse GetDashboard()
    {
        lock (_gate)
        {
            return new RecordArrDashboardResponse(
                DateTimeOffset.UtcNow,
                _records.Count,
                _records.Count(record => record.Status is "active" or "effective" or "approved"),
                _records.Count(record => record.Status is "review" or "processing"),
                _uploadSessions.Count,
                _packages.Count,
                _controlledDocuments.Count,
                _legalHolds.Count,
                _records.OrderByDescending(record => record.UploadedAt).Take(5).ToArray(),
                _packages.Where(pkg => pkg.Status is not "archived").Take(5).ToArray(),
                _controlledDocuments.Take(5).ToArray(),
                _legalHolds.Take(5).ToArray());
        }
    }

    public IReadOnlyList<RecordArrRecordResponse> GetRecords(string? search = null)
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

            return records.OrderByDescending(record => record.UploadedAt).ToArray();
        }
    }

    public RecordArrRecordResponse? GetRecord(string recordId)
    {
        lock (_gate)
        {
            return _records.FirstOrDefault(record => string.Equals(record.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public RecordArrRecordResponse CreateRecord(string title, string description, string recordType, string documentType, string sourceProduct, string sourceObjectType, string sourceObjectId, string sourceObjectDisplayName, string ownerPersonId, string uploadedByPersonId, string currentFileName, string currentMimeType)
    {
        lock (_gate)
        {
            var record = new RecordArrRecordResponse(
                $"rec-{Guid.NewGuid():N}"[..12],
                $"REC-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                title,
                description,
                recordType,
                documentType,
                "processing",
                "internal",
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
                [sourceProduct, recordType, documentType]);
            _records.Add(record);
            _accessLogs.Add(new RecordArrAccessLogResponse($"alog-{Guid.NewGuid():N}"[..12], record.RecordId, "upload", "allowed", uploadedByPersonId, null, null, DateTimeOffset.UtcNow, null, null, "api-upload"));
            return record;
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

            var updated = _records[index] with
            {
                Status = status,
                Classification = classification ?? _records[index].Classification,
                EffectiveAt = effectiveAt ?? _records[index].EffectiveAt,
                ExpiresAt = expiresAt ?? _records[index].ExpiresAt
            };
            _records[index] = updated;
            return updated;
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

    public RecordArrScanProcessingResponse CreateScanProcessing(string recordId, string originalFileName, string scanPurpose)
    {
        lock (_gate)
        {
            var scan = new RecordArrScanProcessingResponse(
                $"scan-{Guid.NewGuid():N}"[..12],
                recordId,
                originalFileName,
                "uploaded",
                scanPurpose,
                null,
                null,
                null,
                null,
                0.42m,
                null,
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

    public RecordArrScanProcessingResponse ApplyManualCorrection(string scanProcessingId, string edgeCoordinates)
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
                Status = "completed",
                EdgeCoordinates = edgeCoordinates,
                ProcessedAt = DateTimeOffset.UtcNow,
                ConfidenceScore = 0.91m
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

    public RecordArrEvidenceMappingResponse CreateEvidenceMapping(string recordId, string sourceProduct, string sourceObjectType, string sourceObjectId, string complianceRequirementRef, string evidenceTypeKey, string mappingSource, decimal confidenceScore)
    {
        lock (_gate)
        {
            var mapping = new RecordArrEvidenceMappingResponse(
                $"map-{Guid.NewGuid():N}"[..12],
                recordId,
                sourceProduct,
                sourceObjectType,
                sourceObjectId,
                complianceRequirementRef,
                evidenceTypeKey,
                "suggested",
                mappingSource,
                confidenceScore,
                null,
                null,
                null,
                null,
                null,
                null);
            _evidenceMappings.Add(mapping);
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

            var current = _evidenceMappings[index];
            var updated = current with
            {
                Status = status,
                ConfirmedByPersonId = status == "confirmed" ? personId : current.ConfirmedByPersonId,
                ConfirmedAt = status == "confirmed" ? DateTimeOffset.UtcNow : current.ConfirmedAt,
                RejectedByPersonId = status == "rejected" ? personId : current.RejectedByPersonId,
                RejectedAt = status == "rejected" ? DateTimeOffset.UtcNow : current.RejectedAt,
                RejectionReason = status == "rejected" ? reason : current.RejectionReason,
                Notes = notes ?? current.Notes
            };
            _evidenceMappings[index] = updated;
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

    public RecordArrPackageResponse CreatePackage(string title, string packageType, string sourceProduct, string sourceObjectRef, string recordRef)
    {
        lock (_gate)
        {
            var package = new RecordArrPackageResponse(
                $"pkg-{Guid.NewGuid():N}"[..12],
                $"PKG-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                title,
                packageType,
                "assembling",
                sourceProduct,
                [sourceObjectRef],
                [recordRef],
                null,
                null,
                null,
                DateTimeOffset.UtcNow,
                null,
                null,
                null,
                null);
            _packages.Add(package);
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
            var hold = new RecordArrLegalHoldResponse(
                $"hold-{Guid.NewGuid():N}"[..12],
                $"HOLD-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                title,
                description,
                "draft",
                holdType,
                scopeRules.ToArray(),
                recordRefs.ToArray(),
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

    private RecordArrRecordResponse RequireRecord(string recordId)
    {
        var record = _records.FirstOrDefault(candidate => string.Equals(candidate.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
        return record ?? throw new InvalidOperationException($"Record {recordId} not found.");
    }

    private RecordArrRecordResponse UpdateRecordLifecycle(RecordArrRecordResponse record, string status, string actorPersonId, string reasonCode)
    {
        var index = _records.FindIndex(candidate => string.Equals(candidate.RecordId, record.RecordId, StringComparison.OrdinalIgnoreCase));
        var updated = record with
        {
            Status = status,
            ExpiresAt = status is "purged" ? null : record.ExpiresAt
        };

        _records[index] = updated;
        _accessLogs.Add(new RecordArrAccessLogResponse($"alog-{Guid.NewGuid():N}"[..12], record.RecordId, status, "allowed", actorPersonId, null, null, DateTimeOffset.UtcNow, null, null, reasonCode));
        return updated;
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

    public RecordArrDocumentDistributionResponse CreateDocumentDistribution(string controlledDocumentId, string versionId, string distributionType, string targetRef)
    {
        lock (_gate)
        {
            var distribution = new RecordArrDocumentDistributionResponse(
                $"dist-{Guid.NewGuid():N}"[..12],
                controlledDocumentId,
                versionId,
                distributionType,
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

        var current = _documentDistributions[index];
        var updated = current with
        {
            Status = status
        };
        _documentDistributions[index] = updated;
        AppendControlledDocumentAuditTrail(
            current.ControlledDocumentId,
            CreateControlledDocumentAuditTrailEntry(
                status,
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
            var now = DateTimeOffset.UtcNow;
            var policy = new RecordArrAccessPolicyResponse(
                $"acc-{Guid.NewGuid():N}"[..12],
                recordId,
                policyType,
                status,
                readRules.ToArray(),
                writeRules.ToArray(),
                downloadRules.ToArray(),
                shareRules.ToArray(),
                exportRules.ToArray(),
                purgeRules.ToArray());
            _accessPolicies.Add(policy);
            AddAccessLog(recordId, "access_policy.created", "allowed", createdByPersonId, null, null, null, null, $"{policyType}:{status}");
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

            var updated = new RecordArrAccessPolicyResponse(
                accessPolicyId,
                recordId,
                policyType,
                status,
                readRules.ToArray(),
                writeRules.ToArray(),
                downloadRules.ToArray(),
                shareRules.ToArray(),
                exportRules.ToArray(),
                purgeRules.ToArray());
            _accessPolicies[index] = updated;
            AddAccessLog(recordId, "access_policy.updated", "allowed", updatedByPersonId, null, null, null, null, $"{policyType}:{status}");
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

    public RecordArrAccessGrantResponse CreateAccessGrant(string recordId, string granteeType, string granteeRef, string permission, string grantedByPersonId, DateTimeOffset? expiresAt)
    {
        lock (_gate)
        {
            var grant = new RecordArrAccessGrantResponse(
                $"grant-{Guid.NewGuid():N}"[..12],
                recordId,
                granteeType,
                granteeRef,
                permission,
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

            var current = _disposalReviews[index];
            var updated = current with
            {
                Status = status,
                ReviewedByPersonId = reviewedByPersonId ?? current.ReviewedByPersonId,
                ReviewedAt = DateTimeOffset.UtcNow,
                DecisionReason = decisionReason ?? current.DecisionReason,
                CompletedAt = status is "approved" or "rejected" or "completed" ? DateTimeOffset.UtcNow : current.CompletedAt
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

    public RecordArrControlledDocumentResponse CreateControlledDocument(string title, string description, string controlledDocumentType, string ownerPersonId, string departmentOrgUnitId, string staffarrSiteId, bool acknowledgementRequired)
    {
        lock (_gate)
        {
            var document = new RecordArrControlledDocumentResponse(
                $"doc-{Guid.NewGuid():N}"[..12],
                $"DOC-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                _records[0].RecordId,
                title,
                description,
                controlledDocumentType,
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
                null);
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

            var updated = _controlledDocuments[index] with
            {
                Status = status,
                NextReviewAt = status is "archived" or "obsolete" ? null : _controlledDocuments[index].NextReviewAt
            };
            _controlledDocuments[index] = updated;
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    status,
                    updatedByPersonId,
                    $"Controlled document marked as {status}."));
            return updated;
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

            var review = new RecordArrDocumentReviewResponse(
                $"drev-{Guid.NewGuid():N}"[..12],
                controlledDocumentId,
                versionId,
                reviewType,
                "pending",
                requestedByPersonId,
                reviewerPersonId,
                DateTimeOffset.UtcNow,
                dueAt,
                null,
                null,
                null);
            _documentReviews.Add(review);
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

            var current = _documentReviews[index];
            var updated = current with
            {
                Status = status,
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
                if (status.Trim().Equals("approved", StringComparison.OrdinalIgnoreCase) ||
                    status.Trim().Equals("completed", StringComparison.OrdinalIgnoreCase))
                {
                    _controlledDocuments[documentIndex] = document with
                    {
                        Status = "effective",
                        EffectiveAt = document.EffectiveAt ?? completedAt,
                        NextReviewAt = document.ReviewIntervalDays > 0 ? completedAt.AddDays(document.ReviewIntervalDays) : document.NextReviewAt
                    };
                }
                else if (status.Trim().Equals("rejected", StringComparison.OrdinalIgnoreCase) ||
                         status.Trim().Equals("changes_requested", StringComparison.OrdinalIgnoreCase))
                {
                    _controlledDocuments[documentIndex] = document with { Status = "review" };
                }
            }

            var auditAction = status.Trim().ToLowerInvariant() switch
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

    public RecordArrExternalShareResponse CreateExternalShare(string recordId, string recipientName, string recipientEmail, string sharePurpose, IEnumerable<string> allowedActions, string createdByPersonId)
    {
        lock (_gate)
        {
            var share = new RecordArrExternalShareResponse(
                $"share-{Guid.NewGuid():N}"[..12],
                $"SHARE-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                recordId,
                sharePurpose,
                "created",
                recipientName,
                recipientEmail,
                allowedActions.ToArray(),
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

    public RecordArrRedactionResponse CreateRedaction(string sourceRecordId, string redactedRecordId, string redactionReason, string redactedByPersonId, IEnumerable<string> redactionRules)
    {
        lock (_gate)
        {
            var redaction = new RecordArrRedactionResponse(
                $"red-{Guid.NewGuid():N}"[..12],
                sourceRecordId,
                redactedRecordId,
                redactionReason,
                "completed",
                redactedByPersonId,
                DateTimeOffset.UtcNow,
                redactionRules.ToArray());
            _redactions.Add(redaction);
            return redaction;
        }
    }

    public IReadOnlyList<RecordArrAccessLogResponse> GetAccessLogs()
    {
        lock (_gate)
        {
            return _accessLogs.OrderByDescending(log => log.OccurredAt).ToArray();
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
            var review = new RecordArrDisposalReviewResponse(
                $"disp-{Guid.NewGuid():N}"[..12],
                recordId,
                retentionStatusRef,
                proposedAction,
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
