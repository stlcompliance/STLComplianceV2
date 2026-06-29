using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RecordArr.Api.Models;
using STLCompliance.Shared.Auth;
using RecordArr.Api.Services;

namespace RecordArr.Api.Data;

public sealed class RecordArrStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RecordArrDbContext db;
    private readonly object _gate = new();
    private readonly List<RecordArrRecordResponse> _records;
    private readonly List<RecordArrUploadSessionResponse> _uploadSessions;
    private readonly List<RecordArrCaptureRequestResponse> _captureRequests;
    private readonly List<RecordArrFileResponse> _files;
    private readonly List<RecordArrFileIntegrityCheckResponse> _fileIntegrityChecks;
    private readonly List<RecordArrFileMalwareScanResponse> _fileMalwareScans;
    private readonly List<RecordArrStorageReconciliationResponse> _storageReconciliations;
    private readonly List<RecordArrObjectStoreObjectResponse> _objectStoreObjects;
    private readonly List<RecordArrObjectStoreFixityObservationResponse> _objectStoreFixityObservations;
    private readonly List<RecordArrDisasterRecoveryRunResponse> _disasterRecoveryRuns;
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
    private readonly List<RecordArrDestructionCertificateResponse> _destructionCertificates;
    private readonly List<RecordArrRetentionSchedulerRunResponse> _retentionSchedulerRuns;
    private readonly List<RecordArrRetentionSchedulerLeaseResponse> _retentionSchedulerLeases;
    private readonly List<RecordArrRetentionSchedulerOutboxMessageResponse> _retentionSchedulerOutboxMessages;
    private readonly List<RecordArrLegalHoldResponse> _legalHolds;
    private readonly Dictionary<string, string> _legalHoldTenantIds;
    private readonly List<RecordArrControlledDocumentResponse> _controlledDocuments;
    private readonly List<RecordArrControlledDocumentVersionResponse> _documentVersions;
    private readonly List<RecordArrDocumentReviewResponse> _documentReviews;
    private readonly List<RecordArrDocumentDistributionResponse> _documentDistributions;
    private readonly List<RecordArrDocumentAcknowledgementResponse> _documentAcknowledgements;
    private readonly List<RecordArrAccessPolicyResponse> _accessPolicies;
    private readonly List<RecordArrAccessGrantResponse> _accessGrants;
    private readonly List<RecordArrExternalShareResponse> _externalShares;
    private readonly List<RecordArrRedactionResponse> _redactions;
    private readonly List<RecordArrRedactionProviderJobResponse> _redactionProviderJobs;
    private readonly List<RecordArrSignatureRecordResponse> _signatureRecords;
    private readonly List<RecordArrSignatureTrustServiceJobResponse> _signatureTrustServiceJobs;
    private readonly List<RecordArrPhotoEvidenceResponse> _photoEvidence;
    private readonly List<RecordArrAccessLogResponse> _accessLogs;
    private readonly List<RecordArrAccessHistorySealResponse> _accessHistorySeals;
    private readonly List<RecordArrAuditEventResponse> _auditEvents;
    private readonly List<RecordArrAuditSealResponse> _auditSeals;

    public RecordArrStore(RecordArrDbContext db)
    {
        this.db = db;
        _records = [];
        _uploadSessions = [];
        _captureRequests = [];
        _files = [];
        _fileIntegrityChecks = [];
        _fileMalwareScans = [];
        _storageReconciliations = [];
        _objectStoreObjects = [];
        _objectStoreFixityObservations = [];
        _disasterRecoveryRuns = [];
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
        _destructionCertificates = [];
        _retentionSchedulerRuns = [];
        _retentionSchedulerLeases = [];
        _retentionSchedulerOutboxMessages = [];
        _legalHolds = [];
        _legalHoldTenantIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _controlledDocuments = [];
        _documentVersions = [];
        _documentReviews = [];
        _documentDistributions = [];
        _documentAcknowledgements = [];
        _accessPolicies = [];
        _accessGrants = [];
        _externalShares = [];
        _redactions = [];
        _redactionProviderJobs = [];
        _signatureRecords = [];
        _signatureTrustServiceJobs = [];
        _photoEvidence = [];
        _accessLogs = [];
        _accessHistorySeals = [];
        _auditEvents = [];
        _auditSeals = [];

        SeedCanonicalFixtures();
        LoadDurableRecords();
    }

    private void LoadDurableRecords()
    {
        var records = db.RecordArrRecords
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrRecordResponse>(row.PayloadJson, JsonOptions))
            .Where(record => record is not null)
            .Select(record => record!)
            .ToArray();

        var files = db.RecordArrFiles
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrFileResponse>(row.PayloadJson, JsonOptions))
            .Where(file => file is not null)
            .Select(file => file!)
            .ToArray();
        var fileIntegrityChecks = db.RecordArrFileIntegrityChecks
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrFileIntegrityCheckResponse>(row.PayloadJson, JsonOptions))
            .Where(check => check is not null)
            .Select(check => check!)
            .ToArray();
        var fileMalwareScans = db.RecordArrFileMalwareScans
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrFileMalwareScanResponse>(row.PayloadJson, JsonOptions))
            .Where(scan => scan is not null)
            .Select(scan => scan!)
            .ToArray();
        var storageReconciliations = db.RecordArrStorageReconciliations
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrStorageReconciliationResponse>(row.PayloadJson, JsonOptions))
            .Where(reconciliation => reconciliation is not null)
            .Select(reconciliation => reconciliation!)
            .ToArray();
        var objectStoreObjects = db.RecordArrObjectStoreObjects
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrObjectStoreObjectResponse>(row.PayloadJson, JsonOptions))
            .Where(item => item is not null)
            .Select(item => item!)
            .ToArray();
        var objectStoreFixityObservations = db.RecordArrObjectStoreFixityObservations
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrObjectStoreFixityObservationResponse>(row.PayloadJson, JsonOptions))
            .Where(item => item is not null)
            .Select(item => item!)
            .ToArray();
        var disasterRecoveryRuns = db.RecordArrDisasterRecoveryRuns
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrDisasterRecoveryRunResponse>(row.PayloadJson, JsonOptions))
            .Where(run => run is not null)
            .Select(run => run!)
            .ToArray();
        var metadataRows = db.RecordArrRecordMetadata
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrRecordMetadataResponse>(row.PayloadJson, JsonOptions))
            .Where(metadata => metadata is not null)
            .Select(metadata => metadata!)
            .ToArray();
        var links = db.RecordArrRecordLinks
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrRecordLinkResponse>(row.PayloadJson, JsonOptions))
            .Where(link => link is not null)
            .Select(link => link!)
            .ToArray();
        var comments = db.RecordArrRecordComments
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrRecordCommentResponse>(row.PayloadJson, JsonOptions))
            .Where(comment => comment is not null)
            .Select(comment => comment!)
            .ToArray();
        var uploadSessions = db.RecordArrUploadSessions
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrUploadSessionResponse>(row.PayloadJson, JsonOptions))
            .Where(session => session is not null)
            .Select(session => session!)
            .ToArray();
        var captureRequests = db.RecordArrCaptureRequests
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrCaptureRequestResponse>(row.PayloadJson, JsonOptions))
            .Where(request => request is not null)
            .Select(request => request!)
            .ToArray();
        var scans = db.RecordArrScanProcessing
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrScanProcessingResponse>(row.PayloadJson, JsonOptions))
            .Where(scan => scan is not null)
            .Select(scan => scan!)
            .ToArray();
        var ocrResults = db.RecordArrOcrResults
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrOcrResultResponse>(row.PayloadJson, JsonOptions))
            .Where(result => result is not null)
            .Select(result => result!)
            .ToArray();
        var extractionResults = db.RecordArrExtractionResults
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrExtractionResultResponse>(row.PayloadJson, JsonOptions))
            .Where(result => result is not null)
            .Select(result => result!)
            .ToArray();
        var evidenceMappings = db.RecordArrEvidenceMappings
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrEvidenceMappingResponse>(row.PayloadJson, JsonOptions))
            .Where(mapping => mapping is not null)
            .Select(mapping => mapping!)
            .ToArray();
        var packages = db.RecordArrPackages
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrPackageResponse>(row.PayloadJson, JsonOptions))
            .Where(package => package is not null)
            .Select(package => package!)
            .ToArray();
        var manifests = db.RecordArrPackageManifests
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrPackageManifestResponse>(row.PayloadJson, JsonOptions))
            .Where(manifest => manifest is not null)
            .Select(manifest => manifest!)
            .ToArray();
        var retentionStatuses = db.RecordArrRetentionStatuses
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrRetentionStatusResponse>(row.PayloadJson, JsonOptions))
            .Where(status => status is not null)
            .Select(status => status!)
            .ToArray();
        var disposalReviews = db.RecordArrDisposalReviews
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrDisposalReviewResponse>(row.PayloadJson, JsonOptions))
            .Where(review => review is not null)
            .Select(review => review!)
            .ToArray();
        var destructionCertificates = db.RecordArrDestructionCertificates
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrDestructionCertificateResponse>(row.PayloadJson, JsonOptions))
            .Where(certificate => certificate is not null)
            .Select(certificate => certificate!)
            .ToArray();
        var retentionSchedulerRuns = db.RecordArrRetentionSchedulerRuns
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrRetentionSchedulerRunResponse>(row.PayloadJson, JsonOptions))
            .Where(run => run is not null)
            .Select(run => run!)
            .ToArray();
        var retentionSchedulerLeases = db.RecordArrRetentionSchedulerLeases
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrRetentionSchedulerLeaseResponse>(row.PayloadJson, JsonOptions))
            .Where(lease => lease is not null)
            .Select(lease => lease!)
            .ToArray();
        var retentionSchedulerOutboxMessages = db.RecordArrRetentionSchedulerOutboxMessages
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrRetentionSchedulerOutboxMessageResponse>(row.PayloadJson, JsonOptions))
            .Where(message => message is not null)
            .Select(message => NormalizeRetentionSchedulerOutboxMessage(message!))
            .ToArray();
        var legalHoldRows = db.RecordArrLegalHolds
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => new
            {
                TenantId = row.TenantId.ToString("D"),
                Hold = JsonSerializer.Deserialize<RecordArrLegalHoldResponse>(row.PayloadJson, JsonOptions)
            })
            .Where(row => row.Hold is not null)
            .Select(row => new { row.TenantId, Hold = row.Hold! })
            .ToArray();
        var controlledDocuments = db.RecordArrControlledDocuments
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrControlledDocumentResponse>(row.PayloadJson, JsonOptions))
            .Where(document => document is not null)
            .Select(document => document!)
            .ToArray();
        var documentVersions = db.RecordArrControlledDocumentVersions
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrControlledDocumentVersionResponse>(row.PayloadJson, JsonOptions))
            .Where(version => version is not null)
            .Select(version => version!)
            .ToArray();
        var documentReviews = db.RecordArrDocumentReviews
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrDocumentReviewResponse>(row.PayloadJson, JsonOptions))
            .Where(review => review is not null)
            .Select(review => review!)
            .ToArray();
        var documentDistributions = db.RecordArrDocumentDistributions
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrDocumentDistributionResponse>(row.PayloadJson, JsonOptions))
            .Where(distribution => distribution is not null)
            .Select(distribution => distribution!)
            .ToArray();
        var documentAcknowledgements = db.RecordArrDocumentAcknowledgements
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrDocumentAcknowledgementResponse>(row.PayloadJson, JsonOptions))
            .Where(acknowledgement => acknowledgement is not null)
            .Select(acknowledgement => acknowledgement!)
            .ToArray();
        var accessPolicies = db.RecordArrAccessPolicies
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrAccessPolicyResponse>(row.PayloadJson, JsonOptions))
            .Where(policy => policy is not null)
            .Select(policy => policy!)
            .ToArray();
        var accessGrants = db.RecordArrAccessGrants
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrAccessGrantResponse>(row.PayloadJson, JsonOptions))
            .Where(grant => grant is not null)
            .Select(grant => grant!)
            .ToArray();
        var externalShares = db.RecordArrExternalShares
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrExternalShareResponse>(row.PayloadJson, JsonOptions))
            .Where(share => share is not null)
            .Select(share => share!)
            .ToArray();
        var redactions = db.RecordArrRedactions
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrRedactionResponse>(row.PayloadJson, JsonOptions))
            .Where(redaction => redaction is not null)
            .Select(redaction => redaction!)
            .ToArray();
        var redactionProviderJobs = db.RecordArrRedactionProviderJobs
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrRedactionProviderJobResponse>(row.PayloadJson, JsonOptions))
            .Where(job => job is not null)
            .Select(job => job!)
            .ToArray();
        var signatureRecords = db.RecordArrSignatureRecords
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrSignatureRecordResponse>(row.PayloadJson, JsonOptions))
            .Where(signature => signature is not null)
            .Select(signature => signature!)
            .ToArray();
        var signatureTrustServiceJobs = db.RecordArrSignatureTrustServiceJobs
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrSignatureTrustServiceJobResponse>(row.PayloadJson, JsonOptions))
            .Where(job => job is not null)
            .Select(job => job!)
            .ToArray();
        var photoEvidence = db.RecordArrPhotoEvidence
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrPhotoEvidenceResponse>(row.PayloadJson, JsonOptions))
            .Where(photo => photo is not null)
            .Select(photo => photo!)
            .ToArray();
        var accessLogs = db.RecordArrAccessLogs
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrAccessLogResponse>(row.PayloadJson, JsonOptions))
            .Where(log => log is not null)
            .Select(log => log!)
            .ToArray();
        var accessHistorySeals = db.RecordArrAccessHistorySeals
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrAccessHistorySealResponse>(row.PayloadJson, JsonOptions))
            .Where(seal => seal is not null)
            .Select(seal => seal!)
            .ToArray();
        var auditEvents = db.RecordArrAuditEvents
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrAuditEventResponse>(row.PayloadJson, JsonOptions))
            .Where(auditEvent => auditEvent is not null)
            .Select(auditEvent => auditEvent!)
            .ToArray();
        var auditSeals = db.RecordArrAuditSeals
            .AsNoTracking()
            .AsEnumerable()
            .Select(row => JsonSerializer.Deserialize<RecordArrAuditSealResponse>(row.PayloadJson, JsonOptions))
            .Where(seal => seal is not null)
            .Select(seal => seal!)
            .ToArray();

        foreach (var record in records)
        {
            var index = _records.FindIndex(existing => string.Equals(existing.RecordId, record.RecordId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _records[index] = record;
            }
            else
            {
                _records.Add(record);
            }
        }

        foreach (var file in files)
        {
            var index = _files.FindIndex(existing => string.Equals(existing.FileId, file.FileId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _files[index] = file;
            }
            else
            {
                _files.Add(file);
            }
        }

        foreach (var check in fileIntegrityChecks)
        {
            var index = _fileIntegrityChecks.FindIndex(existing => string.Equals(existing.IntegrityCheckId, check.IntegrityCheckId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _fileIntegrityChecks[index] = check;
            }
            else
            {
                _fileIntegrityChecks.Add(check);
            }
        }

        foreach (var scan in fileMalwareScans)
        {
            var index = _fileMalwareScans.FindIndex(existing => string.Equals(existing.MalwareScanId, scan.MalwareScanId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _fileMalwareScans[index] = scan;
            }
            else
            {
                _fileMalwareScans.Add(scan);
            }
        }

        foreach (var reconciliation in storageReconciliations)
        {
            var index = _storageReconciliations.FindIndex(existing => string.Equals(existing.ReconciliationId, reconciliation.ReconciliationId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _storageReconciliations[index] = reconciliation;
            }
            else
            {
                _storageReconciliations.Add(reconciliation);
            }
        }

        foreach (var item in objectStoreObjects)
        {
            var index = _objectStoreObjects.FindIndex(existing => string.Equals(existing.ObjectStoreObjectId, item.ObjectStoreObjectId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _objectStoreObjects[index] = item;
            }
            else
            {
                _objectStoreObjects.Add(item);
            }
        }

        foreach (var item in objectStoreFixityObservations)
        {
            var index = _objectStoreFixityObservations.FindIndex(existing => string.Equals(existing.FixityObservationId, item.FixityObservationId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _objectStoreFixityObservations[index] = item;
            }
            else
            {
                _objectStoreFixityObservations.Add(item);
            }
        }

        foreach (var run in disasterRecoveryRuns)
        {
            var index = _disasterRecoveryRuns.FindIndex(existing => string.Equals(existing.DisasterRecoveryRunId, run.DisasterRecoveryRunId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _disasterRecoveryRuns[index] = run;
            }
            else
            {
                _disasterRecoveryRuns.Add(run);
            }
        }

        foreach (var file in files)
        {
            EnsureObjectStoreFileIndexed(file);
        }

        foreach (var metadata in metadataRows)
        {
            var index = _recordMetadata.FindIndex(existing => string.Equals(existing.MetadataId, metadata.MetadataId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _recordMetadata[index] = metadata;
            }
            else
            {
                _recordMetadata.Add(metadata);
            }
        }

        foreach (var link in links)
        {
            var index = _recordLinks.FindIndex(existing => string.Equals(existing.RecordLinkId, link.RecordLinkId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _recordLinks[index] = link;
            }
            else
            {
                _recordLinks.Add(link);
            }
        }

        foreach (var comment in comments)
        {
            var index = _recordComments.FindIndex(existing => string.Equals(existing.CommentId, comment.CommentId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _recordComments[index] = comment;
            }
            else
            {
                _recordComments.Add(comment);
            }
        }

        foreach (var session in uploadSessions)
        {
            var index = _uploadSessions.FindIndex(existing => string.Equals(existing.UploadSessionId, session.UploadSessionId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _uploadSessions[index] = session;
            }
            else
            {
                _uploadSessions.Add(session);
            }
        }

        foreach (var request in captureRequests)
        {
            var index = _captureRequests.FindIndex(existing => string.Equals(existing.CaptureRequestId, request.CaptureRequestId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _captureRequests[index] = request;
            }
            else
            {
                _captureRequests.Add(request);
            }
        }

        foreach (var scan in scans)
        {
            var index = _scans.FindIndex(existing => string.Equals(existing.ScanProcessingId, scan.ScanProcessingId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _scans[index] = scan;
            }
            else
            {
                _scans.Add(scan);
            }
        }

        foreach (var result in ocrResults)
        {
            var index = _ocrResults.FindIndex(existing => string.Equals(existing.OcrResultId, result.OcrResultId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _ocrResults[index] = result;
            }
            else
            {
                _ocrResults.Add(result);
            }
        }

        foreach (var result in extractionResults)
        {
            var index = _extractionResults.FindIndex(existing => string.Equals(existing.ExtractionResultId, result.ExtractionResultId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _extractionResults[index] = result;
            }
            else
            {
                _extractionResults.Add(result);
            }
        }

        foreach (var mapping in evidenceMappings)
        {
            var index = _evidenceMappings.FindIndex(existing => string.Equals(existing.EvidenceMappingId, mapping.EvidenceMappingId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _evidenceMappings[index] = mapping;
            }
            else
            {
                _evidenceMappings.Add(mapping);
            }
        }

        foreach (var package in packages)
        {
            var index = _packages.FindIndex(existing => string.Equals(existing.PackageId, package.PackageId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _packages[index] = package;
            }
            else
            {
                _packages.Add(package);
            }
        }

        foreach (var manifest in manifests)
        {
            var index = _manifests.FindIndex(existing => string.Equals(existing.ManifestId, manifest.ManifestId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _manifests[index] = manifest;
            }
            else
            {
                _manifests.Add(manifest);
            }
        }

        foreach (var status in retentionStatuses)
        {
            var index = _retentionStatuses.FindIndex(existing => string.Equals(existing.RetentionStatusId, status.RetentionStatusId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _retentionStatuses[index] = status;
            }
            else
            {
                _retentionStatuses.Add(status);
            }
        }

        foreach (var review in disposalReviews)
        {
            var index = _disposalReviews.FindIndex(existing => string.Equals(existing.DisposalReviewId, review.DisposalReviewId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _disposalReviews[index] = review;
            }
            else
            {
                _disposalReviews.Add(review);
            }
        }

        foreach (var certificate in destructionCertificates)
        {
            var index = _destructionCertificates.FindIndex(existing => string.Equals(existing.DestructionCertificateId, certificate.DestructionCertificateId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _destructionCertificates[index] = certificate;
            }
            else
            {
                _destructionCertificates.Add(certificate);
            }
        }

        foreach (var run in retentionSchedulerRuns)
        {
            var index = _retentionSchedulerRuns.FindIndex(existing => string.Equals(existing.SchedulerRunId, run.SchedulerRunId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _retentionSchedulerRuns[index] = run;
            }
            else
            {
                _retentionSchedulerRuns.Add(run);
            }
        }

        foreach (var lease in retentionSchedulerLeases)
        {
            var index = _retentionSchedulerLeases.FindIndex(existing => string.Equals(existing.LeaseId, lease.LeaseId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _retentionSchedulerLeases[index] = lease;
            }
            else
            {
                _retentionSchedulerLeases.Add(lease);
            }
        }

        foreach (var message in retentionSchedulerOutboxMessages)
        {
            var index = _retentionSchedulerOutboxMessages.FindIndex(existing => string.Equals(existing.OutboxMessageId, message.OutboxMessageId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _retentionSchedulerOutboxMessages[index] = message;
            }
            else
            {
                _retentionSchedulerOutboxMessages.Add(message);
            }
        }

        foreach (var row in legalHoldRows)
        {
            var hold = row.Hold;
            var index = _legalHolds.FindIndex(existing => string.Equals(existing.LegalHoldId, hold.LegalHoldId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _legalHolds[index] = hold;
            }
            else
            {
                _legalHolds.Add(hold);
            }

            _legalHoldTenantIds[hold.LegalHoldId] = row.TenantId;
        }

        foreach (var document in controlledDocuments)
        {
            var index = _controlledDocuments.FindIndex(existing => string.Equals(existing.ControlledDocumentId, document.ControlledDocumentId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _controlledDocuments[index] = document;
            }
            else
            {
                _controlledDocuments.Add(document);
            }
        }

        foreach (var version in documentVersions)
        {
            var index = _documentVersions.FindIndex(existing => string.Equals(existing.VersionId, version.VersionId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _documentVersions[index] = version;
            }
            else
            {
                _documentVersions.Add(version);
            }
        }

        foreach (var review in documentReviews)
        {
            var index = _documentReviews.FindIndex(existing => string.Equals(existing.DocumentReviewId, review.DocumentReviewId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _documentReviews[index] = review;
            }
            else
            {
                _documentReviews.Add(review);
            }
        }

        foreach (var distribution in documentDistributions)
        {
            var index = _documentDistributions.FindIndex(existing => string.Equals(existing.DistributionId, distribution.DistributionId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _documentDistributions[index] = distribution;
            }
            else
            {
                _documentDistributions.Add(distribution);
            }
        }

        foreach (var acknowledgement in documentAcknowledgements)
        {
            var index = _documentAcknowledgements.FindIndex(existing => string.Equals(existing.AcknowledgementId, acknowledgement.AcknowledgementId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _documentAcknowledgements[index] = acknowledgement;
            }
            else
            {
                _documentAcknowledgements.Add(acknowledgement);
            }
        }

        foreach (var policy in accessPolicies)
        {
            var index = _accessPolicies.FindIndex(existing => string.Equals(existing.AccessPolicyId, policy.AccessPolicyId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _accessPolicies[index] = policy;
            }
            else
            {
                _accessPolicies.Add(policy);
            }
        }

        foreach (var grant in accessGrants)
        {
            var index = _accessGrants.FindIndex(existing => string.Equals(existing.AccessGrantId, grant.AccessGrantId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _accessGrants[index] = grant;
            }
            else
            {
                _accessGrants.Add(grant);
            }
        }

        foreach (var share in externalShares)
        {
            var index = _externalShares.FindIndex(existing => string.Equals(existing.ExternalShareId, share.ExternalShareId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _externalShares[index] = share;
            }
            else
            {
                _externalShares.Add(share);
            }
        }

        foreach (var redaction in redactions)
        {
            var index = _redactions.FindIndex(existing => string.Equals(existing.RedactionId, redaction.RedactionId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _redactions[index] = redaction;
            }
            else
            {
                _redactions.Add(redaction);
            }
        }

        foreach (var job in redactionProviderJobs)
        {
            var index = _redactionProviderJobs.FindIndex(existing => string.Equals(existing.ProviderJobId, job.ProviderJobId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _redactionProviderJobs[index] = job;
            }
            else
            {
                _redactionProviderJobs.Add(job);
            }
        }

        foreach (var signature in signatureRecords)
        {
            var index = _signatureRecords.FindIndex(existing => string.Equals(existing.SignatureRecordId, signature.SignatureRecordId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _signatureRecords[index] = signature;
            }
            else
            {
                _signatureRecords.Add(signature);
            }
        }

        foreach (var job in signatureTrustServiceJobs)
        {
            var index = _signatureTrustServiceJobs.FindIndex(existing => string.Equals(existing.TrustServiceJobId, job.TrustServiceJobId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _signatureTrustServiceJobs[index] = job;
            }
            else
            {
                _signatureTrustServiceJobs.Add(job);
            }
        }

        foreach (var photo in photoEvidence)
        {
            var index = _photoEvidence.FindIndex(existing => string.Equals(existing.PhotoEvidenceId, photo.PhotoEvidenceId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _photoEvidence[index] = photo;
            }
            else
            {
                _photoEvidence.Add(photo);
            }
        }

        foreach (var log in accessLogs)
        {
            var index = _accessLogs.FindIndex(existing => string.Equals(existing.AccessLogId, log.AccessLogId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _accessLogs[index] = log;
            }
            else
            {
                _accessLogs.Add(log);
            }
        }

        foreach (var seal in accessHistorySeals)
        {
            var index = _accessHistorySeals.FindIndex(existing => string.Equals(existing.AccessHistorySealId, seal.AccessHistorySealId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _accessHistorySeals[index] = seal;
            }
            else
            {
                _accessHistorySeals.Add(seal);
            }
        }

        foreach (var auditEvent in auditEvents)
        {
            var index = _auditEvents.FindIndex(existing => string.Equals(existing.AuditEventId, auditEvent.AuditEventId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _auditEvents[index] = auditEvent;
            }
            else
            {
                _auditEvents.Add(auditEvent);
            }
        }

        foreach (var seal in auditSeals)
        {
            var index = _auditSeals.FindIndex(existing => string.Equals(existing.AuditSealId, seal.AuditSealId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _auditSeals[index] = seal;
            }
            else
            {
                _auditSeals.Add(seal);
            }
        }
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
        var seedAccessLog = new RecordArrAccessLogResponse(
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
            "seed-fixture");
        seedAccessLog = seedAccessLog with
        {
            AccessLogHash = ComputeAccessLogHash(tenantId, seedAccessLog, previousHash: null)
        };
        _accessLogs.Add(seedAccessLog);

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
            AddAccessLog(record.RecordId, "upload", "allowed", uploadedByPersonId, null, null, null, null, "api-upload");
            PersistFile(file);
            PersistRecord(record);
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
            AddAccessLog(record.RecordId, "print.archive", "allowed", uploadedByPersonId.Trim(), null, null, null, null, "official-print-archive");
            PersistFile(file);
            PersistRecord(record);
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
                AddAccessLog(file.RecordId, "view", "allowed", null, null, null, null, null, "file_lookup");
                return file;
            }

            AddAccessLog(file.RecordId, "view", "denied", null, null, null, null, null, "access_policy_denied");

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
                AddAccessLog(file.RecordId, "download", "denied", null, null, null, null, null, "access_policy_denied");
                throw new InvalidOperationException($"File {fileId} is not available for download.");
            }

            if (file.DeletedAt.HasValue)
            {
                throw new InvalidOperationException($"File {fileId} is not available for download.");
            }

            if (!IsFileSafeForDelivery(file))
            {
                AddAccessLog(file.RecordId, "download", "denied", null, null, null, null, null, $"malware-scan-{file.VirusScanStatus}");
                throw new InvalidOperationException($"File {fileId} is not available for download because its malware scan status is {file.VirusScanStatus}.");
            }

            AddAccessLog(file.RecordId, "download", "allowed", null, null, null, null, null, "file_download");

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
            EnsureRecordNotUnderActiveLegalHold(recordId, "file.upload", uploadedByPersonId);
            var file = CreateFileObject(
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
            PersistFile(file);

            var updatedRecord = _records.FirstOrDefault(record => string.Equals(record.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            if (updatedRecord is not null)
            {
                PersistRecord(updatedRecord);
            }

            return file;
        }
    }

    public IReadOnlyList<RecordArrFileMalwareScanResponse> GetFileMalwareScans(
        string tenantId,
        string? fileId = null,
        string? recordId = null)
    {
        lock (_gate)
        {
            var scans = _fileMalwareScans.Where(scan => string.Equals(scan.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(fileId))
            {
                scans = scans.Where(scan => string.Equals(scan.FileId, fileId, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(recordId))
            {
                scans = scans.Where(scan => string.Equals(scan.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            }

            return scans.OrderByDescending(scan => scan.ScannedAt).ToArray();
        }
    }

    public IReadOnlyList<RecordArrFileResponse> GetPendingMalwareScanFiles(string tenantId)
    {
        lock (_gate)
        {
            return _files
                .Where(file =>
                    string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                    file.DeletedAt is null &&
                    string.Equals(file.VirusScanStatus, "pending", StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => file.UploadedAt)
                .ThenBy(file => file.FileId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public RecordArrFileMalwareScanResponse CreateFileMalwareScan(
        string tenantId,
        string fileId,
        string scannedByPersonId,
        string status,
        string? scannerName = null,
        string? scannerVersion = null,
        string? signatureVersion = null,
        string? threatName = null,
        string? failureReason = null)
    {
        lock (_gate)
        {
            var fileIndex = _files.FindIndex(file =>
                string.Equals(file.FileId, fileId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));
            if (fileIndex < 0)
            {
                throw new InvalidOperationException($"File {fileId} not found.");
            }

            var file = _files[fileIndex];
            if (!RecordBelongsToTenant(file.RecordId, tenantId))
            {
                throw new InvalidOperationException($"File {fileId} not found.");
            }

            var normalizedStatus = NormalizeMalwareScanStatus(status, threatName);
            var quarantineStatus = normalizedStatus switch
            {
                "infected" => "quarantined",
                "failed" => "quarantined",
                "dead_lettered" => "dead_lettered",
                "pending" => "pending",
                _ => "released"
            };
            var normalizedFailureReason = normalizedStatus switch
            {
                "infected" => string.IsNullOrWhiteSpace(threatName) ? "malware_detected" : $"malware_detected:{threatName.Trim()}",
                "failed" => string.IsNullOrWhiteSpace(failureReason) ? "scanner_failed" : failureReason.Trim(),
                "dead_lettered" => string.IsNullOrWhiteSpace(failureReason) ? "malware_scan_dead_lettered" : failureReason.Trim(),
                _ => failureReason?.Trim()
            };

            var scan = new RecordArrFileMalwareScanResponse(
                $"mscan-{Guid.NewGuid():N}"[..14],
                tenantId,
                file.FileId,
                file.RecordId,
                file.StorageProvider,
                file.StorageKey,
                normalizedStatus,
                string.IsNullOrWhiteSpace(scannerName) ? "tenant_malware_scanner" : scannerName.Trim(),
                string.IsNullOrWhiteSpace(scannerVersion) ? null : scannerVersion.Trim(),
                string.IsNullOrWhiteSpace(signatureVersion) ? null : signatureVersion.Trim(),
                string.IsNullOrWhiteSpace(threatName) ? null : threatName.Trim(),
                quarantineStatus,
                DateTimeOffset.UtcNow,
                scannedByPersonId,
                normalizedFailureReason);

            _fileMalwareScans.Add(scan);
            PersistFileMalwareScan(scan);

            _files[fileIndex] = file with
            {
                VirusScanStatus = normalizedStatus,
                ProcessingStatus = normalizedStatus switch
                {
                    "infected" or "failed" => "quarantined",
                    "dead_lettered" => "dead_lettered",
                    "pending" => "scan_pending",
                    _ => "completed"
                }
            };
            PersistFile(_files[fileIndex]);

            AddAccessLog(file.RecordId, "file.malware_scan", "allowed", scannedByPersonId, null, null, null, null, normalizedStatus);

            return scan;
        }
    }

    public RecordArrMalwareScanRunResponse RunFileMalwareScanProvider(
        string tenantId,
        string requestedByPersonId,
        string? scannerName = null,
        string? scannerVersion = null,
        string? signatureVersion = null,
        IReadOnlyCollection<string>? infectedFileIds = null,
        IReadOnlyCollection<string>? failedFileIds = null,
        IReadOnlyCollection<string>? skippedFileIds = null)
    {
        lock (_gate)
        {
            var startedAt = DateTimeOffset.UtcNow;
            var normalizedScannerName = string.IsNullOrWhiteSpace(scannerName) ? "tenant_malware_scanner" : scannerName.Trim();
            var normalizedScannerVersion = string.IsNullOrWhiteSpace(scannerVersion) ? null : scannerVersion.Trim();
            var normalizedSignatureVersion = string.IsNullOrWhiteSpace(signatureVersion) ? null : signatureVersion.Trim();
            var infectedIds = new HashSet<string>(infectedFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var failedIds = new HashSet<string>(failedFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var skippedIds = new HashSet<string>(skippedFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var pendingFiles = _files
                .Where(file =>
                    string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                    file.DeletedAt is null &&
                    string.Equals(file.VirusScanStatus, "pending", StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => file.UploadedAt)
                .ThenBy(file => file.FileId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var scanResults = new List<RecordArrFileMalwareScanResponse>();
            var releasedFileRefs = new List<string>();
            var quarantinedFileRefs = new List<string>();
            var failedFileRefs = new List<string>();

            foreach (var file in pendingFiles)
            {
                var status = "clean";
                string? threatName = null;
                string? failureReason = null;

                if (infectedIds.Contains(file.FileId))
                {
                    status = "infected";
                    threatName = "provider_detected_threat";
                }
                else if (failedIds.Contains(file.FileId))
                {
                    status = "failed";
                    failureReason = "provider_scan_failed";
                }
                else if (skippedIds.Contains(file.FileId))
                {
                    status = "skipped";
                    failureReason = "provider_scan_skipped";
                }

                var scan = CreateFileMalwareScan(
                    tenantId,
                    file.FileId,
                    requestedByPersonId,
                    status,
                    normalizedScannerName,
                    normalizedScannerVersion,
                    normalizedSignatureVersion,
                    threatName,
                    failureReason);
                scanResults.Add(scan);

                if (string.Equals(scan.QuarantineStatus, "quarantined", StringComparison.OrdinalIgnoreCase))
                {
                    quarantinedFileRefs.Add(file.FileId);
                    if (string.Equals(scan.Status, "failed", StringComparison.OrdinalIgnoreCase))
                    {
                        failedFileRefs.Add(file.FileId);
                    }
                }
                else
                {
                    releasedFileRefs.Add(file.FileId);
                }
            }

            var completedAt = DateTimeOffset.UtcNow;
            var run = new RecordArrMalwareScanRunResponse(
                $"msrun-{Guid.NewGuid():N}"[..14],
                tenantId,
                normalizedScannerName,
                normalizedScannerVersion,
                normalizedSignatureVersion,
                startedAt,
                completedAt,
                requestedByPersonId,
                pendingFiles.Length,
                scanResults.Count,
                releasedFileRefs.Count,
                quarantinedFileRefs.Count,
                failedFileRefs.Count,
                releasedFileRefs.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                quarantinedFileRefs.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                failedFileRefs.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                scanResults.ToArray());

            return run;
        }
    }

    public RecordArrMalwareScanRunResponse RunFileMalwareScanProviderVerdicts(
        string tenantId,
        string requestedByPersonId,
        string? scannerName,
        string? scannerVersion,
        string? signatureVersion,
        IReadOnlyCollection<RecordArrMalwareScanProviderVerdict> verdicts)
    {
        lock (_gate)
        {
            var startedAt = DateTimeOffset.UtcNow;
            var normalizedScannerName = string.IsNullOrWhiteSpace(scannerName) ? "external_malware_scanner" : scannerName.Trim();
            var normalizedScannerVersion = string.IsNullOrWhiteSpace(scannerVersion) ? null : scannerVersion.Trim();
            var normalizedSignatureVersion = string.IsNullOrWhiteSpace(signatureVersion) ? null : signatureVersion.Trim();
            var pendingFiles = GetPendingMalwareScanFiles(tenantId);
            var pendingFileIds = pendingFiles
                .Select(file => file.FileId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var verdictsByFileId = verdicts
                .Where(verdict => !string.IsNullOrWhiteSpace(verdict.FileId))
                .GroupBy(verdict => verdict.FileId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);
            var scanResults = new List<RecordArrFileMalwareScanResponse>();
            var releasedFileRefs = new List<string>();
            var quarantinedFileRefs = new List<string>();
            var failedFileRefs = new List<string>();

            foreach (var verdict in verdictsByFileId.Values.OrderBy(verdict => verdict.FileId, StringComparer.OrdinalIgnoreCase))
            {
                if (!pendingFileIds.Contains(verdict.FileId))
                {
                    continue;
                }

                var normalizedStatus = NormalizeRecordArrEnum(
                    verdict.Status,
                    nameof(verdict.Status),
                    "clean",
                    "infected",
                    "failed");
                var scan = CreateFileMalwareScan(
                    tenantId,
                    verdict.FileId,
                    requestedByPersonId,
                    normalizedStatus,
                    normalizedScannerName,
                    normalizedScannerVersion,
                    normalizedSignatureVersion,
                    verdict.ThreatName,
                    verdict.FailureReason);
                scanResults.Add(scan);

                if (string.Equals(scan.QuarantineStatus, "quarantined", StringComparison.OrdinalIgnoreCase))
                {
                    quarantinedFileRefs.Add(verdict.FileId);
                    if (string.Equals(scan.Status, "failed", StringComparison.OrdinalIgnoreCase))
                    {
                        failedFileRefs.Add(verdict.FileId);
                    }
                }
                else
                {
                    releasedFileRefs.Add(verdict.FileId);
                }
            }

            var completedAt = DateTimeOffset.UtcNow;
            return new RecordArrMalwareScanRunResponse(
                $"msrun-{Guid.NewGuid():N}"[..14],
                tenantId,
                normalizedScannerName,
                normalizedScannerVersion,
                normalizedSignatureVersion,
                startedAt,
                completedAt,
                requestedByPersonId,
                pendingFiles.Count,
                scanResults.Count,
                releasedFileRefs.Count,
                quarantinedFileRefs.Count,
                failedFileRefs.Count,
                releasedFileRefs.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                quarantinedFileRefs.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                failedFileRefs.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                scanResults.ToArray());
        }
    }

    public RecordArrMalwareScanDeadLetterRunResponse DeadLetterFailedMalwareScans(
        string tenantId,
        string requestedByPersonId,
        int maxFiles = 100)
    {
        lock (_gate)
        {
            var startedAt = DateTimeOffset.UtcNow;
            var eligibleFiles = _files
                .Where(file =>
                    string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                    file.DeletedAt is null &&
                    string.Equals(file.VirusScanStatus, "failed", StringComparison.OrdinalIgnoreCase) &&
                    !_fileMalwareScans.Any(scan =>
                        string.Equals(scan.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(scan.FileId, file.FileId, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(scan.Status, "dead_lettered", StringComparison.OrdinalIgnoreCase)))
                .OrderBy(file => file.UploadedAt)
                .ThenBy(file => file.FileId, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Clamp(maxFiles, 1, 500))
                .ToArray();
            var deadLetterResults = new List<RecordArrFileMalwareScanResponse>();
            var deadLetteredFileRefs = new List<string>();

            foreach (var file in eligibleFiles)
            {
                var latestFailure = _fileMalwareScans
                    .Where(scan =>
                        string.Equals(scan.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(scan.FileId, file.FileId, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(scan.Status, "failed", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(scan => scan.ScannedAt)
                    .FirstOrDefault();
                var scan = CreateFileMalwareScan(
                    tenantId,
                    file.FileId,
                    requestedByPersonId,
                    "dead_lettered",
                    latestFailure?.ScannerName ?? "tenant_malware_scanner",
                    latestFailure?.ScannerVersion,
                    latestFailure?.SignatureVersion,
                    failureReason: latestFailure is null
                        ? "malware_scan_dead_lettered"
                        : $"malware_scan_dead_lettered:{latestFailure.FailureReason ?? latestFailure.MalwareScanId}");
                deadLetterResults.Add(scan);
                deadLetteredFileRefs.Add(file.FileId);
                AddAccessLog(file.RecordId, "file.malware_scan.dead_lettered", "denied", requestedByPersonId, null, null, null, null, scan.FailureReason ?? "malware_scan_dead_lettered");
            }

            var completedAt = DateTimeOffset.UtcNow;
            return new RecordArrMalwareScanDeadLetterRunResponse(
                $"msdlq-{Guid.NewGuid():N}"[..15],
                tenantId,
                startedAt,
                completedAt,
                requestedByPersonId,
                eligibleFiles.Length,
                deadLetterResults.Count,
                deadLetteredFileRefs.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                deadLetterResults.ToArray());
        }
    }

    public IReadOnlyList<RecordArrFileIntegrityCheckResponse> GetFileIntegrityChecks(
        string tenantId,
        string? fileId = null,
        string? recordId = null)
    {
        lock (_gate)
        {
            var query = _fileIntegrityChecks
                .Where(check => string.Equals(check.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(fileId))
            {
                query = query.Where(check => string.Equals(check.FileId, fileId, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(recordId))
            {
                query = query.Where(check => string.Equals(check.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderByDescending(check => check.CheckedAt)
                .ToArray();
        }
    }

    public RecordArrFileIntegrityCheckResponse CreateFileIntegrityCheck(
        string tenantId,
        string fileId,
        string checkedByPersonId,
        string? observedChecksumSha256 = null,
        string? checkMethod = null,
        string? observationSource = null,
        string? reconciliationRef = null)
    {
        lock (_gate)
        {
            var file = FindFile(fileId)
                ?? throw new InvalidOperationException($"File {fileId} not found.");

            if (!string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"File {fileId} not found.");
            }

            var now = DateTimeOffset.UtcNow;
            var normalizedExpectedChecksum = NormalizeChecksum(file.ChecksumSha256);
            var normalizedObservedChecksum = NormalizeChecksum(observedChecksumSha256) ?? normalizedExpectedChecksum;
            var status = file.DeletedAt.HasValue
                ? "unavailable"
                : string.Equals(normalizedExpectedChecksum, normalizedObservedChecksum, StringComparison.OrdinalIgnoreCase)
                    ? "passed"
                    : "failed";
            var failureReason = status switch
            {
                "unavailable" => "file_deleted_or_purged",
                "failed" => "checksum_mismatch",
                _ => null
            };

            var check = new RecordArrFileIntegrityCheckResponse(
                $"fix-{Guid.NewGuid():N}"[..12],
                tenantId,
                file.FileId,
                file.RecordId,
                file.StorageProvider,
                file.StorageKey,
                normalizedExpectedChecksum,
                normalizedObservedChecksum,
                status,
                NormalizeRecordArrEnum(checkMethod ?? "metadata_checksum", "checkMethod", "metadata_checksum", "object_hash", "restore_verify", "disaster_recovery_restore", "disaster_recovery_backup", "migration_verify", "manual_verify"),
                now,
                checkedByPersonId,
                failureReason);

            _fileIntegrityChecks.Add(check);
            PersistFileIntegrityCheck(check);
            RecordObjectStoreFixityObservation(file, check.Status, check.CheckedAt, checkedByPersonId, NormalizeRecordArrEnum(observationSource ?? "integrity_check", "observationSource", "file_created", "integrity_check", "storage_reconciliation", "storage_remediation", "disaster_recovery_restore", "disaster_recovery_backup", "migration_backfill"), check.ObservedChecksumSha256, check.IntegrityCheckId, reconciliationRef, check.FailureReason);
            AddAccessLog(file.RecordId, "file.integrity_check", "allowed", checkedByPersonId, null, null, null, null, status);
            return check;
        }
    }

    public IReadOnlyList<RecordArrStorageReconciliationResponse> GetStorageReconciliations(
        string tenantId,
        string? status = null)
    {
        lock (_gate)
        {
            var query = _storageReconciliations
                .Where(reconciliation => string.Equals(reconciliation.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(reconciliation => string.Equals(reconciliation.Status, status.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderByDescending(reconciliation => reconciliation.CompletedAt)
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrObjectStoreObjectResponse> GetObjectStoreObjects(
        string tenantId,
        string? fileId = null,
        string? recordId = null,
        string? status = null)
    {
        lock (_gate)
        {
            var query = _objectStoreObjects
                .Where(item => string.Equals(item.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(fileId))
            {
                query = query.Where(item => string.Equals(item.FileId, fileId.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(recordId))
            {
                query = query.Where(item => string.Equals(item.RecordId, recordId.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(item => string.Equals(item.Status, status.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderByDescending(item => item.LastObservedAt)
                .ThenBy(item => item.FileId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrObjectStoreFixityObservationResponse> GetObjectStoreFixityObservations(
        string tenantId,
        string? fileId = null,
        string? recordId = null,
        string? reconciliationId = null,
        string? status = null)
    {
        lock (_gate)
        {
            var query = _objectStoreFixityObservations
                .Where(item => string.Equals(item.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(fileId))
            {
                query = query.Where(item => string.Equals(item.FileId, fileId.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(recordId))
            {
                query = query.Where(item => string.Equals(item.RecordId, recordId.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(reconciliationId))
            {
                query = query.Where(item => string.Equals(item.ReconciliationRef, reconciliationId.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(item => string.Equals(item.Status, status.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderByDescending(item => item.ObservedAt)
                .ThenBy(item => item.FileId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public RecordArrObjectStoreLifecycleVerificationResponse VerifyObjectStoreLifecycle(
        string tenantId,
        string fileId,
        string verifiedByPersonId,
        string? providerName,
        string? policyRef,
        string? retentionMode,
        DateTimeOffset retainUntil,
        string? encryptionKeyRef,
        string? evidenceRef)
    {
        lock (_gate)
        {
            var file = _files.FirstOrDefault(candidate =>
                string.Equals(candidate.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.FileId, fileId, StringComparison.OrdinalIgnoreCase));
            if (file is null)
            {
                throw new InvalidOperationException($"File {fileId} not found.");
            }

            var normalizedProviderName = NormalizeRequiredEvidenceValue(providerName, nameof(providerName));
            var normalizedPolicyRef = NormalizeRequiredEvidenceValue(policyRef, nameof(policyRef));
            var normalizedRetentionMode = NormalizeRecordArrEnum(
                retentionMode ?? string.Empty,
                nameof(retentionMode),
                "governance",
                "compliance",
                "legal_hold",
                "locked",
                "provider_retained");
            var normalizedEncryptionKeyRef = NormalizeRequiredEvidenceValue(encryptionKeyRef, nameof(encryptionKeyRef));
            var normalizedEvidenceRef = NormalizeRequiredEvidenceValue(evidenceRef, nameof(evidenceRef));

            EnsureObjectStoreFileIndexed(file);

            var requiredRetainUntil = GetRequiredObjectRetainUntil(file.RecordId);
            var failureReason = requiredRetainUntil is not null && retainUntil < requiredRetainUntil.Value
                ? "retention_policy_not_satisfied"
                : null;
            var status = failureReason is null ? "verified" : "failed";
            var evidenceHash = ComputeObjectStoreLifecycleEvidenceHash(
                file,
                normalizedProviderName,
                normalizedPolicyRef,
                normalizedRetentionMode,
                retainUntil,
                normalizedEncryptionKeyRef,
                normalizedEvidenceRef,
                requiredRetainUntil);

            RecordObjectStoreFixityObservation(
                file,
                failureReason is null ? "passed" : "failed",
                DateTimeOffset.UtcNow,
                verifiedByPersonId,
                "object_lifecycle_verification",
                file.ChecksumSha256,
                null,
                normalizedEvidenceRef,
                failureReason,
                status,
                normalizedProviderName,
                normalizedPolicyRef,
                normalizedRetentionMode,
                retainUntil,
                normalizedEncryptionKeyRef,
                normalizedEvidenceRef,
                evidenceHash);

            var objectIndex = _objectStoreObjects.FindIndex(item =>
                string.Equals(item.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(item.FileId, file.FileId, StringComparison.OrdinalIgnoreCase));
            if (objectIndex < 0)
            {
                throw new InvalidOperationException($"Object-store object for file {fileId} was not indexed.");
            }

            var updated = _objectStoreObjects[objectIndex] with
            {
                LifecycleStatus = status,
                LifecycleProviderName = normalizedProviderName,
                LifecyclePolicyRef = normalizedPolicyRef,
                LifecycleRetentionMode = normalizedRetentionMode,
                LifecycleRetainUntil = retainUntil,
                LifecycleEncryptionKeyRef = normalizedEncryptionKeyRef,
                LifecycleEvidenceRef = normalizedEvidenceRef,
                LifecycleEvidenceHash = evidenceHash,
                LifecycleFailureReason = failureReason
            };
            _objectStoreObjects[objectIndex] = updated;
            PersistObjectStoreObject(updated);

            AddAccessLog(
                file.RecordId,
                "object_store.lifecycle_verified",
                failureReason is null ? "allowed" : "denied",
                verifiedByPersonId,
                null,
                null,
                null,
                null,
                failureReason ?? normalizedPolicyRef);

            return new RecordArrObjectStoreLifecycleVerificationResponse(
                updated.ObjectStoreObjectId,
                tenantId,
                file.FileId,
                file.RecordId,
                status,
                normalizedProviderName,
                normalizedPolicyRef,
                normalizedRetentionMode,
                retainUntil,
                normalizedEncryptionKeyRef,
                normalizedEvidenceRef,
                evidenceHash,
                failureReason,
                updated);
        }
    }

    public IReadOnlyList<RecordArrDisasterRecoveryRunResponse> GetDisasterRecoveryRuns(
        string tenantId,
        string? status = null)
    {
        lock (_gate)
        {
            var query = _disasterRecoveryRuns
                .Where(run => string.Equals(run.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(run => string.Equals(run.Status, status.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            return query
                .OrderByDescending(run => run.CompletedAt)
                .ThenBy(run => run.DisasterRecoveryRunId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public RecordArrDisasterRecoveryRunResponse RunDisasterRecoveryRestore(
        string tenantId,
        string requestedByPersonId,
        string? recoveryPointId,
        DateTimeOffset recoveryPointCreatedAt,
        int rpoTargetMinutes,
        int rtoTargetMinutes,
        IReadOnlyCollection<string>? recordIds = null,
        IReadOnlyCollection<string>? missingFileIds = null,
        IReadOnlyCollection<string>? corruptFileIds = null)
    {
        lock (_gate)
        {
            var startedAt = DateTimeOffset.UtcNow;
            var runId = $"dr-{Guid.NewGuid():N}"[..14];
            var normalizedRecoveryPointId = string.IsNullOrWhiteSpace(recoveryPointId) ? string.Empty : recoveryPointId.Trim();
            var requestedRecordIds = new HashSet<string>(recordIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var missingIds = new HashSet<string>(missingFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var corruptIds = new HashSet<string>(corruptFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var records = _records
                .Where(record => RecordBelongsToTenant(record.RecordId, tenantId))
                .Where(record => requestedRecordIds.Count == 0 || requestedRecordIds.Contains(record.RecordId))
                .OrderBy(record => record.UploadedAt)
                .ThenBy(record => record.RecordId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var foundRecordIds = records.Select(record => record.RecordId).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missingRecordIds = requestedRecordIds
                .Where(recordId => !foundRecordIds.Contains(recordId))
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var scope = requestedRecordIds.Count == 0
                ? "tenant"
                : $"records:{string.Join(",", requestedRecordIds.Order(StringComparer.OrdinalIgnoreCase))}";
            var completedAt = DateTimeOffset.UtcNow;
            var recoveryPointAgeMinutes = Math.Max(0, (int)Math.Ceiling((completedAt - recoveryPointCreatedAt).TotalMinutes));
            var durationSeconds = Math.Max(0, (int)Math.Ceiling((completedAt - startedAt).TotalSeconds));
            var rpoMet = rpoTargetMinutes > 0 && recoveryPointAgeMinutes <= rpoTargetMinutes;
            var rtoMet = rtoTargetMinutes > 0 && durationSeconds <= rtoTargetMinutes * 60;
            var blockedRecords = new List<string>(missingRecordIds);
            var restoredRecords = new List<string>();
            var verifiedFiles = new List<string>();
            var failedFiles = new List<string>();
            string? failureReason = null;

            if (string.IsNullOrWhiteSpace(normalizedRecoveryPointId))
            {
                failureReason = "missing_recovery_point";
            }
            else if (rpoTargetMinutes <= 0)
            {
                failureReason = "invalid_rpo_target";
            }
            else if (rtoTargetMinutes <= 0)
            {
                failureReason = "invalid_rto_target";
            }
            else if (!rpoMet)
            {
                failureReason = "rpo_missed";
            }
            else if (missingRecordIds.Length > 0)
            {
                failureReason = "record_not_found_or_cross_tenant";
            }

            if (failureReason is null)
            {
                foreach (var record in records)
                {
                    var recordFiles = _files
                        .Where(file => string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(file.RecordId, record.RecordId, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(file => file.UploadedAt)
                        .ThenBy(file => file.FileId, StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    var recordHasFailure = false;
                    foreach (var file in recordFiles)
                    {
                        if (missingIds.Contains(file.FileId))
                        {
                            failedFiles.Add(file.FileId);
                            recordHasFailure = true;
                            RecordObjectStoreFixityObservation(file, "missing", DateTimeOffset.UtcNow, requestedByPersonId, "disaster_recovery_restore", null, null, runId, "recovery_point_object_missing");
                            continue;
                        }

                        var observedChecksum = corruptIds.Contains(file.FileId)
                            ? $"sha256-dr-mismatch-{file.FileId}"
                            : file.ChecksumSha256;
                        var check = CreateFileIntegrityCheck(tenantId, file.FileId, requestedByPersonId, observedChecksum, "disaster_recovery_restore", "disaster_recovery_restore", runId);
                        if (string.Equals(check.Status, "passed", StringComparison.OrdinalIgnoreCase))
                        {
                            verifiedFiles.Add(file.FileId);
                        }
                        else
                        {
                            failedFiles.Add(file.FileId);
                            recordHasFailure = true;
                        }
                    }

                    if (recordHasFailure)
                    {
                        blockedRecords.Add(record.RecordId);
                    }
                    else
                    {
                        restoredRecords.Add(record.RecordId);
                    }
                }
            }

            completedAt = DateTimeOffset.UtcNow;
            durationSeconds = Math.Max(0, (int)Math.Ceiling((completedAt - startedAt).TotalSeconds));
            rtoMet = rtoTargetMinutes > 0 && durationSeconds <= rtoTargetMinutes * 60;
            if (failureReason is null && !rtoMet)
            {
                failureReason = "rto_missed";
            }

            var distinctBlockedRecords = blockedRecords.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
            var distinctRestoredRecords = restoredRecords.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
            var distinctVerifiedFiles = verifiedFiles.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
            var distinctFailedFiles = failedFiles.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
            var status = failureReason switch
            {
                "rpo_missed" => "rpo_missed",
                "rto_missed" => "rto_missed",
                null when distinctFailedFiles.Length == 0 && distinctBlockedRecords.Length == 0 => "passed",
                null => "failed",
                _ => "failed"
            };
            var evidenceSummary = status == "passed"
                ? $"Verified {distinctVerifiedFiles.Length} file(s) across {distinctRestoredRecords.Length} record(s) from recovery point {normalizedRecoveryPointId}."
                : string.Join(
                    "; ",
                    new[]
                    {
                        !rpoMet ? $"RPO missed: recovery point age {recoveryPointAgeMinutes} minute(s), target {rpoTargetMinutes} minute(s)" : null,
                        !rtoMet ? $"RTO missed: duration {durationSeconds} second(s), target {rtoTargetMinutes * 60} second(s)" : null,
                        distinctBlockedRecords.Length > 0 ? $"{distinctBlockedRecords.Length} blocked record(s)" : null,
                        distinctFailedFiles.Length > 0 ? $"{distinctFailedFiles.Length} failed file(s)" : null,
                        string.IsNullOrWhiteSpace(normalizedRecoveryPointId) ? "Missing recovery point id" : null
                    }.Where(part => part is not null));

            var run = new RecordArrDisasterRecoveryRunResponse(
                runId,
                tenantId,
                "restore_verification",
                scope,
                normalizedRecoveryPointId,
                recoveryPointCreatedAt,
                startedAt,
                completedAt,
                requestedByPersonId,
                status,
                rpoTargetMinutes,
                rtoTargetMinutes,
                recoveryPointAgeMinutes,
                durationSeconds,
                rpoMet,
                rtoMet,
                records.Length + missingRecordIds.Length,
                distinctRestoredRecords.Length,
                distinctBlockedRecords.Length,
                _files.Count(file => string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                    (requestedRecordIds.Count == 0 || requestedRecordIds.Contains(file.RecordId))),
                distinctVerifiedFiles.Length,
                distinctFailedFiles.Length,
                distinctRestoredRecords,
                distinctBlockedRecords,
                distinctVerifiedFiles,
                distinctFailedFiles,
                string.IsNullOrWhiteSpace(evidenceSummary) ? null : evidenceSummary,
                failureReason);

            _disasterRecoveryRuns.Add(run);
            PersistDisasterRecoveryRun(run);

            foreach (var recordId in distinctRestoredRecords.Concat(distinctBlockedRecords).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                AddAccessLog(recordId, "disaster_recovery.restore", status == "passed" ? "allowed" : "denied", requestedByPersonId, null, null, null, null, run.DisasterRecoveryRunId);
            }

            return run;
        }
    }

    public RecordArrDisasterRecoveryRunResponse RunDisasterRecoveryBackupVerification(
        string tenantId,
        string requestedByPersonId,
        string? backupProviderName,
        string? backupJobRef,
        string? backupManifestHash,
        string? recoveryPointId,
        DateTimeOffset recoveryPointCreatedAt,
        int rpoTargetMinutes,
        IReadOnlyCollection<string>? recordIds = null,
        IReadOnlyCollection<string>? missingFileIds = null,
        IReadOnlyCollection<string>? corruptFileIds = null)
    {
        lock (_gate)
        {
            var startedAt = DateTimeOffset.UtcNow;
            var runId = $"drb-{Guid.NewGuid():N}"[..15];
            var normalizedProviderName = string.IsNullOrWhiteSpace(backupProviderName) ? string.Empty : backupProviderName.Trim();
            var normalizedBackupJobRef = string.IsNullOrWhiteSpace(backupJobRef) ? string.Empty : backupJobRef.Trim();
            var normalizedManifestHash = string.IsNullOrWhiteSpace(backupManifestHash) ? string.Empty : backupManifestHash.Trim();
            var normalizedRecoveryPointId = string.IsNullOrWhiteSpace(recoveryPointId) ? string.Empty : recoveryPointId.Trim();
            var requestedRecordIds = new HashSet<string>(recordIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var missingIds = new HashSet<string>(missingFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var corruptIds = new HashSet<string>(corruptFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var records = _records
                .Where(record => RecordBelongsToTenant(record.RecordId, tenantId))
                .Where(record => requestedRecordIds.Count == 0 || requestedRecordIds.Contains(record.RecordId))
                .OrderBy(record => record.UploadedAt)
                .ThenBy(record => record.RecordId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var foundRecordIds = records.Select(record => record.RecordId).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missingRecordIds = requestedRecordIds
                .Where(recordId => !foundRecordIds.Contains(recordId))
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var scope = requestedRecordIds.Count == 0
                ? "tenant_backup"
                : $"backup_records:{string.Join(",", requestedRecordIds.Order(StringComparer.OrdinalIgnoreCase))}";
            var completedAt = DateTimeOffset.UtcNow;
            var recoveryPointAgeMinutes = Math.Max(0, (int)Math.Ceiling((completedAt - recoveryPointCreatedAt).TotalMinutes));
            var rpoMet = rpoTargetMinutes > 0 && recoveryPointAgeMinutes <= rpoTargetMinutes;
            var blockedRecords = new List<string>(missingRecordIds);
            var verifiedRecords = new List<string>();
            var verifiedFiles = new List<string>();
            var failedFiles = new List<string>();
            string? failureReason = null;

            if (string.IsNullOrWhiteSpace(normalizedProviderName))
            {
                failureReason = "missing_backup_provider";
            }
            else if (string.IsNullOrWhiteSpace(normalizedBackupJobRef))
            {
                failureReason = "missing_backup_job_ref";
            }
            else if (string.IsNullOrWhiteSpace(normalizedManifestHash))
            {
                failureReason = "missing_backup_manifest_hash";
            }
            else if (string.IsNullOrWhiteSpace(normalizedRecoveryPointId))
            {
                failureReason = "missing_recovery_point";
            }
            else if (rpoTargetMinutes <= 0)
            {
                failureReason = "invalid_rpo_target";
            }
            else if (!rpoMet)
            {
                failureReason = "rpo_missed";
            }
            else if (missingRecordIds.Length > 0)
            {
                failureReason = "record_not_found_or_cross_tenant";
            }

            if (failureReason is null)
            {
                foreach (var record in records)
                {
                    var recordFiles = _files
                        .Where(file => string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(file.RecordId, record.RecordId, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(file => file.UploadedAt)
                        .ThenBy(file => file.FileId, StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    var recordHasFailure = false;
                    foreach (var file in recordFiles)
                    {
                        if (missingIds.Contains(file.FileId))
                        {
                            failedFiles.Add(file.FileId);
                            recordHasFailure = true;
                            RecordObjectStoreFixityObservation(file, "missing", DateTimeOffset.UtcNow, requestedByPersonId, "disaster_recovery_backup", null, null, runId, "backup_manifest_object_missing");
                            continue;
                        }

                        var observedChecksum = corruptIds.Contains(file.FileId)
                            ? $"sha256-backup-mismatch-{file.FileId}"
                            : file.ChecksumSha256;
                        var check = CreateFileIntegrityCheck(tenantId, file.FileId, requestedByPersonId, observedChecksum, "disaster_recovery_backup", "disaster_recovery_backup", runId);
                        if (string.Equals(check.Status, "passed", StringComparison.OrdinalIgnoreCase))
                        {
                            verifiedFiles.Add(file.FileId);
                        }
                        else
                        {
                            failedFiles.Add(file.FileId);
                            recordHasFailure = true;
                        }
                    }

                    if (recordHasFailure)
                    {
                        blockedRecords.Add(record.RecordId);
                    }
                    else
                    {
                        verifiedRecords.Add(record.RecordId);
                    }
                }
            }

            completedAt = DateTimeOffset.UtcNow;
            var durationSeconds = Math.Max(0, (int)Math.Ceiling((completedAt - startedAt).TotalSeconds));
            var distinctBlockedRecords = blockedRecords.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
            var distinctVerifiedRecords = verifiedRecords.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
            var distinctVerifiedFiles = verifiedFiles.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
            var distinctFailedFiles = failedFiles.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.OrdinalIgnoreCase).ToArray();
            var status = failureReason switch
            {
                "rpo_missed" => "rpo_missed",
                null when distinctFailedFiles.Length == 0 && distinctBlockedRecords.Length == 0 => "passed",
                null => "failed",
                _ => "failed"
            };
            var evidenceSummary = status == "passed"
                ? $"Verified backup coverage for {distinctVerifiedFiles.Length} file(s) across {distinctVerifiedRecords.Length} record(s) from provider {normalizedProviderName} job {normalizedBackupJobRef}."
                : string.Join(
                    "; ",
                    new[]
                    {
                        string.IsNullOrWhiteSpace(normalizedProviderName) ? "Missing backup provider" : null,
                        string.IsNullOrWhiteSpace(normalizedBackupJobRef) ? "Missing backup job reference" : null,
                        string.IsNullOrWhiteSpace(normalizedManifestHash) ? "Missing backup manifest hash" : null,
                        string.IsNullOrWhiteSpace(normalizedRecoveryPointId) ? "Missing recovery point id" : null,
                        !rpoMet ? $"RPO missed: recovery point age {recoveryPointAgeMinutes} minute(s), target {rpoTargetMinutes} minute(s)" : null,
                        distinctBlockedRecords.Length > 0 ? $"{distinctBlockedRecords.Length} blocked record(s)" : null,
                        distinctFailedFiles.Length > 0 ? $"{distinctFailedFiles.Length} failed file(s)" : null
                    }.Where(part => part is not null));

            var run = new RecordArrDisasterRecoveryRunResponse(
                runId,
                tenantId,
                "backup_verification",
                scope,
                normalizedRecoveryPointId,
                recoveryPointCreatedAt,
                startedAt,
                completedAt,
                requestedByPersonId,
                status,
                rpoTargetMinutes,
                0,
                recoveryPointAgeMinutes,
                durationSeconds,
                rpoMet,
                true,
                records.Length + missingRecordIds.Length,
                distinctVerifiedRecords.Length,
                distinctBlockedRecords.Length,
                _files.Count(file => string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                    (requestedRecordIds.Count == 0 || requestedRecordIds.Contains(file.RecordId))),
                distinctVerifiedFiles.Length,
                distinctFailedFiles.Length,
                distinctVerifiedRecords,
                distinctBlockedRecords,
                distinctVerifiedFiles,
                distinctFailedFiles,
                string.IsNullOrWhiteSpace(evidenceSummary) ? null : evidenceSummary,
                failureReason,
                string.IsNullOrWhiteSpace(normalizedProviderName) ? null : normalizedProviderName,
                string.IsNullOrWhiteSpace(normalizedBackupJobRef) ? null : normalizedBackupJobRef,
                string.IsNullOrWhiteSpace(normalizedManifestHash) ? null : normalizedManifestHash);

            _disasterRecoveryRuns.Add(run);
            PersistDisasterRecoveryRun(run);

            foreach (var recordId in distinctVerifiedRecords.Concat(distinctBlockedRecords).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                AddAccessLog(recordId, "disaster_recovery.backup_verified", status == "passed" ? "allowed" : "denied", requestedByPersonId, null, null, null, null, run.DisasterRecoveryRunId);
            }

            return run;
        }
    }

    public IReadOnlyList<RecordArrFileResponse> GetStorageReconciliationCandidateFiles(
        string tenantId,
        string? recordId = null)
    {
        lock (_gate)
        {
            var normalizedRecordId = string.IsNullOrWhiteSpace(recordId) ? null : recordId.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedRecordId) && !RecordBelongsToTenant(normalizedRecordId, tenantId))
            {
                throw new InvalidOperationException($"Record {normalizedRecordId} not found.");
            }

            return _files
                .Where(file => string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .Where(file => string.IsNullOrWhiteSpace(normalizedRecordId) || string.Equals(file.RecordId, normalizedRecordId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => file.UploadedAt)
                .ThenBy(file => file.FileId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public RecordArrStorageReconciliationResponse RunStorageReconciliation(
        string tenantId,
        string requestedByPersonId,
        string? scope = null,
        string? recordId = null,
        IReadOnlyCollection<string>? missingFileIds = null,
        IReadOnlyCollection<string>? corruptFileIds = null,
        IReadOnlyCollection<string>? checkedFileIds = null)
    {
        lock (_gate)
        {
            var normalizedRecordId = string.IsNullOrWhiteSpace(recordId) ? null : recordId.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedRecordId) && !RecordBelongsToTenant(normalizedRecordId, tenantId))
            {
                throw new InvalidOperationException($"Record {normalizedRecordId} not found.");
            }

            var normalizedScope = string.IsNullOrWhiteSpace(scope)
                ? string.IsNullOrWhiteSpace(normalizedRecordId) ? "tenant" : $"record:{normalizedRecordId}"
                : scope.Trim();
            var missingIds = new HashSet<string>(missingFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var corruptIds = new HashSet<string>(corruptFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var checkedIds = checkedFileIds is null
                ? null
                : new HashSet<string>(checkedFileIds, StringComparer.OrdinalIgnoreCase);
            var reconciliationId = $"recon-{Guid.NewGuid():N}"[..16];
            var startedAt = DateTimeOffset.UtcNow;
            var files = _files
                .Where(file => string.Equals(file.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .Where(file => string.IsNullOrWhiteSpace(normalizedRecordId) || string.Equals(file.RecordId, normalizedRecordId, StringComparison.OrdinalIgnoreCase))
                .Where(file => checkedIds is null || checkedIds.Contains(file.FileId))
                .OrderBy(file => file.UploadedAt)
                .ThenBy(file => file.FileId, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var checkedFiles = 0;
            var passedFiles = 0;
            var missingFiles = 0;
            var corruptFiles = 0;
            var quarantinedFiles = 0;
            var pendingScanFiles = 0;
            var deletedFiles = 0;
            var issueFileRefs = new List<string>();

            foreach (var file in files)
            {
                var latestMalwareScan = _fileMalwareScans
                    .Where(scan => string.Equals(scan.FileId, file.FileId, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(scan => scan.ScannedAt)
                    .FirstOrDefault();

                if (file.DeletedAt.HasValue)
                {
                    deletedFiles++;
                    issueFileRefs.Add(file.FileId);
                    var deletedCheck = CreateFileIntegrityCheck(tenantId, file.FileId, requestedByPersonId, file.ChecksumSha256, "restore_verify", "storage_reconciliation", reconciliationId);
                    checkedFiles++;
                    if (string.Equals(deletedCheck.Status, "passed", StringComparison.OrdinalIgnoreCase))
                    {
                        passedFiles++;
                    }

                    continue;
                }

                if (missingIds.Contains(file.FileId))
                {
                    missingFiles++;
                    issueFileRefs.Add(file.FileId);
                    RecordObjectStoreFixityObservation(file, "missing", startedAt, requestedByPersonId, "storage_reconciliation", null, null, reconciliationId, "object_missing_from_inventory");
                    continue;
                }

                var malwareStatus = file.VirusScanStatus;
                if (string.Equals(malwareStatus, "pending", StringComparison.OrdinalIgnoreCase) ||
                    latestMalwareScan is { QuarantineStatus: "pending" })
                {
                    pendingScanFiles++;
                    issueFileRefs.Add(file.FileId);
                }

                if (string.Equals(malwareStatus, "infected", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(malwareStatus, "failed", StringComparison.OrdinalIgnoreCase) ||
                    latestMalwareScan is { QuarantineStatus: "quarantined" })
                {
                    quarantinedFiles++;
                    issueFileRefs.Add(file.FileId);
                }

                var observedChecksum = corruptIds.Contains(file.FileId)
                    ? $"sha256-reconciliation-mismatch-{file.FileId}"
                    : file.ChecksumSha256;
                var check = CreateFileIntegrityCheck(tenantId, file.FileId, requestedByPersonId, observedChecksum, "restore_verify", "storage_reconciliation", reconciliationId);
                checkedFiles++;
                if (string.Equals(check.Status, "passed", StringComparison.OrdinalIgnoreCase))
                {
                    passedFiles++;
                }
                else
                {
                    corruptFiles++;
                    issueFileRefs.Add(file.FileId);
                }
            }

            var distinctIssueFileRefs = issueFileRefs
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var status = distinctIssueFileRefs.Length == 0 ? "passed" : "issues_found";
            var issueSummary = status == "passed"
                ? null
                : string.Join(
                    "; ",
                    new[]
                    {
                        missingFiles > 0 ? $"{missingFiles} missing object(s)" : null,
                        corruptFiles > 0 ? $"{corruptFiles} corrupt object(s)" : null,
                        quarantinedFiles > 0 ? $"{quarantinedFiles} quarantined file(s)" : null,
                        pendingScanFiles > 0 ? $"{pendingScanFiles} pending malware scan(s)" : null,
                        deletedFiles > 0 ? $"{deletedFiles} deleted file(s)" : null
                    }.Where(part => part is not null));
            var reconciliation = new RecordArrStorageReconciliationResponse(
                reconciliationId,
                tenantId,
                normalizedScope,
                status,
                startedAt,
                DateTimeOffset.UtcNow,
                requestedByPersonId,
                files.Length,
                checkedFiles,
                passedFiles,
                missingFiles,
                corruptFiles,
                quarantinedFiles,
                pendingScanFiles,
                deletedFiles,
                distinctIssueFileRefs,
                issueSummary,
                status == "passed" ? "none_required" : "open");

            _storageReconciliations.Add(reconciliation);
            PersistStorageReconciliation(reconciliation);

            var auditRecordId = files.FirstOrDefault()?.RecordId ?? normalizedRecordId;
            if (!string.IsNullOrWhiteSpace(auditRecordId))
            {
                AddAccessLog(auditRecordId, "storage.reconciliation", "allowed", requestedByPersonId, null, null, null, null, status);
            }

            return reconciliation;
        }
    }

    public RecordArrStorageReconciliationRemediationResponse RemediateStorageReconciliation(
        string tenantId,
        string reconciliationId,
        string remediatedByPersonId,
        IReadOnlyCollection<string>? restoredFileIds = null,
        IReadOnlyCollection<string>? acceptedMissingFileIds = null,
        IReadOnlyCollection<string>? recheckedCorruptFileIds = null,
        IReadOnlyCollection<string>? releasedQuarantinedFileIds = null,
        IReadOnlyCollection<string>? scannedPendingFileIds = null)
    {
        lock (_gate)
        {
            var index = _storageReconciliations.FindIndex(reconciliation =>
                string.Equals(reconciliation.ReconciliationId, reconciliationId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(reconciliation.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Storage reconciliation {reconciliationId} not found.");
            }

            var current = _storageReconciliations[index];
            var remainingIssues = current.IssueFileRefs.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var resolved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var restoredIds = new HashSet<string>(restoredFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var acceptedMissingIds = new HashSet<string>(acceptedMissingFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var recheckedCorruptIds = new HashSet<string>(recheckedCorruptFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var releasedQuarantinedIds = new HashSet<string>(releasedQuarantinedFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var scannedPendingIds = new HashSet<string>(scannedPendingFileIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            foreach (var fileId in current.IssueFileRefs)
            {
                var file = _files.FirstOrDefault(candidate =>
                    string.Equals(candidate.FileId, fileId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(candidate.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));

                if (acceptedMissingIds.Contains(fileId))
                {
                    resolved.Add(fileId);
                    if (file is not null)
                    {
                        RecordObjectStoreFixityObservation(file, "accepted_missing", DateTimeOffset.UtcNow, remediatedByPersonId, "storage_remediation", null, null, current.ReconciliationId, "missing_object_accepted_by_operator");
                        AddAccessLog(file.RecordId, "storage.reconciliation.missing_accepted", "allowed", remediatedByPersonId, null, null, null, null, reconciliationId);
                    }

                    continue;
                }

                if (file is null)
                {
                    continue;
                }

                if (restoredIds.Contains(fileId) || recheckedCorruptIds.Contains(fileId))
                {
                    var check = CreateFileIntegrityCheck(tenantId, fileId, remediatedByPersonId, file.ChecksumSha256, "restore_verify", "storage_remediation", current.ReconciliationId);
                    if (string.Equals(check.Status, "passed", StringComparison.OrdinalIgnoreCase))
                    {
                        resolved.Add(fileId);
                        AddAccessLog(file.RecordId, "storage.reconciliation.restored", "allowed", remediatedByPersonId, null, null, null, null, reconciliationId);
                    }
                }

                if (releasedQuarantinedIds.Contains(fileId) || scannedPendingIds.Contains(fileId))
                {
                    var scan = CreateFileMalwareScan(
                        tenantId,
                        fileId,
                        remediatedByPersonId,
                        "clean",
                        "tenant_malware_scanner",
                        null,
                        null,
                        null,
                        releasedQuarantinedIds.Contains(fileId) ? "reconciled_quarantine_release" : "reconciled_pending_scan");
                    if (string.Equals(scan.QuarantineStatus, "released", StringComparison.OrdinalIgnoreCase))
                    {
                        resolved.Add(fileId);
                        RecordObjectStoreFixityObservation(file, "passed", scan.ScannedAt, remediatedByPersonId, "storage_remediation", file.ChecksumSha256, null, current.ReconciliationId, releasedQuarantinedIds.Contains(fileId) ? "quarantine_released_after_scan" : "pending_scan_released");
                        AddAccessLog(file.RecordId, "storage.reconciliation.scan_released", "allowed", remediatedByPersonId, null, null, null, null, reconciliationId);
                    }
                }
            }

            remainingIssues.ExceptWith(resolved);
            var remainingIssueRefs = remainingIssues
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var remediationStatus = remainingIssueRefs.Length == 0
                ? "completed"
                : resolved.Count == 0
                    ? current.RemediationStatus
                    : "partial";
            var updated = current with
            {
                Status = remainingIssueRefs.Length == 0 ? "passed" : current.Status,
                IssueFileRefs = remainingIssueRefs,
                IssueSummary = remainingIssueRefs.Length == 0
                    ? null
                    : $"{remainingIssueRefs.Length} unresolved storage issue(s) remain after remediation.",
                RemediationStatus = remediationStatus
            };
            _storageReconciliations[index] = updated;
            PersistStorageReconciliation(updated);

            return new RecordArrStorageReconciliationRemediationResponse(
                updated.ReconciliationId,
                tenantId,
                remediationStatus,
                DateTimeOffset.UtcNow,
                remediatedByPersonId,
                resolved.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                remainingIssueRefs,
                updated);
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

    public IReadOnlyList<RecordArrSignatureRecordResponse> GetSignatureRecords(string tenantId, string? recordId = null)
    {
        lock (_gate)
        {
            var query = _signatureRecords.Where(signature => RecordBelongsToTenant(signature.RecordId, tenantId));
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
        string? deviceSnapshot = null,
        string? providerName = null,
        string? providerEnvelopeRef = null,
        string? certificateFingerprintSha256 = null)
        => CreateSignatureRecord(
            ResolveRecordTenantId(recordId),
            recordId,
            signaturePurpose,
            signerPersonId,
            signerExternalName,
            signerTitle,
            attestationText,
            capturedByPersonId,
            sourceProduct,
            sourceObjectRef,
            geoCoordinates,
            deviceSnapshot,
            providerName,
            providerEnvelopeRef,
            certificateFingerprintSha256);

    public RecordArrSignatureRecordResponse CreateSignatureRecord(
        string tenantId,
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
        string? deviceSnapshot = null,
        string? providerName = null,
        string? providerEnvelopeRef = null,
        string? certificateFingerprintSha256 = null)
    {
        lock (_gate)
        {
            if (!RecordBelongsToTenant(recordId, tenantId))
            {
                throw new InvalidOperationException($"Record {recordId} not found.");
            }

            EnsureRecordNotUnderActiveLegalHold(recordId, "signature.captured", capturedByPersonId);
            var normalizedSignaturePurpose = NormalizeRecordArrEnum(
                signaturePurpose,
                nameof(signaturePurpose),
                "proof_of_delivery",
                "proof_of_pickup",
                "training_acknowledgement",
                "trainer_signoff",
                "evaluator_signoff",
                "work_order_closeout",
                "inspection_attestation",
                "quality_release",
                "customer_acceptance",
                "policy_acknowledgement",
                "other");
            var normalizedProviderName = string.IsNullOrWhiteSpace(providerName) ? null : providerName.Trim();
            var normalizedProviderEnvelopeRef = string.IsNullOrWhiteSpace(providerEnvelopeRef) ? null : providerEnvelopeRef.Trim();
            var normalizedCertificateFingerprint = NormalizeOptionalChecksum(certificateFingerprintSha256);
            if (!string.IsNullOrWhiteSpace(normalizedProviderName) &&
                (string.IsNullOrWhiteSpace(normalizedProviderEnvelopeRef) || string.IsNullOrWhiteSpace(normalizedCertificateFingerprint)))
            {
                throw new InvalidOperationException("Provider-backed signatures require a provider envelope reference and certificate fingerprint.");
            }

            if (string.IsNullOrWhiteSpace(normalizedProviderName) &&
                (!string.IsNullOrWhiteSpace(normalizedProviderEnvelopeRef) || !string.IsNullOrWhiteSpace(normalizedCertificateFingerprint)))
            {
                throw new InvalidOperationException("Provider signature evidence cannot be supplied without a provider name.");
            }

            var file = CreateFileObject(
                tenantId,
                recordId,
                $"signature-{normalizedSignaturePurpose}.png",
                "image/png",
                capturedByPersonId,
                sizeBytes: 128_000,
                imageWidth: 1600,
                imageHeight: 900,
                attachToRecord: true,
                setAsCurrentFile: false);
            var signedAt = DateTimeOffset.UtcNow;
            var verificationStatus = string.IsNullOrWhiteSpace(normalizedProviderName)
                ? "local_capture_only"
                : "provider_verified";
            var signatureEvidenceHash = ComputeSignatureEvidenceHash(
                tenantId,
                recordId,
                normalizedSignaturePurpose,
                signerPersonId,
                signerExternalName,
                signerTitle,
                attestationText,
                file.FileId,
                file.ChecksumSha256,
                signedAt,
                capturedByPersonId,
                sourceProduct,
                sourceObjectRef,
                normalizedProviderName,
                normalizedProviderEnvelopeRef,
                normalizedCertificateFingerprint);

            var signature = new RecordArrSignatureRecordResponse(
                $"sig-{Guid.NewGuid():N}"[..12],
                file.TenantId,
                recordId,
                normalizedSignaturePurpose,
                signerPersonId,
                signerExternalName,
                signerTitle,
                attestationText,
                file.FileId,
                signedAt,
                capturedByPersonId,
                sourceProduct,
                sourceObjectRef,
                geoCoordinates,
                deviceSnapshot,
                verificationStatus,
                normalizedProviderName,
                normalizedProviderEnvelopeRef,
                normalizedCertificateFingerprint,
                signatureEvidenceHash,
                signedAt,
                verificationStatus == "local_capture_only" ? "provider_not_configured" : null);
            _signatureRecords.Add(signature);
            PersistFile(file);
            var updatedRecord = _records.FirstOrDefault(record => string.Equals(record.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            if (updatedRecord is not null)
            {
                PersistRecord(updatedRecord);
            }

            PersistSignatureRecord(tenantId, signature);
            AddAccessLog(recordId, "signature.captured", "allowed", capturedByPersonId, null, null, null, null, "signature-captured");
            return signature;
        }
    }

    public RecordArrSignatureProviderReconciliationResponse ReconcileSignatureProviderStatus(
        string tenantId,
        string signatureRecordId,
        string reconciledByPersonId,
        string? providerName,
        string? providerEnvelopeRef,
        string? providerCallbackStatus,
        string? providerCallbackRef,
        string? certificateFingerprintSha256,
        string? trustTimestampAuthorityRef,
        string? longTermValidationStatus)
    {
        lock (_gate)
        {
            var index = _signatureRecords.FindIndex(signature =>
                string.Equals(signature.SignatureRecordId, signatureRecordId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(signature.RecordId, tenantId));
            if (index < 0)
            {
                throw new InvalidOperationException($"Signature record {signatureRecordId} not found.");
            }

            var current = _signatureRecords[index];
            if (string.IsNullOrWhiteSpace(current.ProviderName) ||
                string.IsNullOrWhiteSpace(current.ProviderEnvelopeRef))
            {
                throw new InvalidOperationException($"Signature record {signatureRecordId} does not have provider envelope evidence.");
            }

            var normalizedProviderName = NormalizeRequiredEvidenceValue(providerName, nameof(providerName));
            var normalizedProviderEnvelopeRef = NormalizeRequiredEvidenceValue(providerEnvelopeRef, nameof(providerEnvelopeRef));
            var normalizedCallbackStatus = NormalizeRecordArrEnum(
                providerCallbackStatus ?? string.Empty,
                nameof(providerCallbackStatus),
                "completed",
                "declined",
                "failed",
                "expired",
                "voided");
            var normalizedCallbackRef = NormalizeRequiredEvidenceValue(providerCallbackRef, nameof(providerCallbackRef));
            var normalizedCertificateFingerprint = NormalizeOptionalChecksum(certificateFingerprintSha256);
            var normalizedTrustTimestampAuthorityRef = string.IsNullOrWhiteSpace(trustTimestampAuthorityRef)
                ? null
                : trustTimestampAuthorityRef.Trim();
            var normalizedLongTermValidationStatus = string.IsNullOrWhiteSpace(longTermValidationStatus)
                ? null
                : NormalizeRecordArrEnum(
                    longTermValidationStatus,
                    nameof(longTermValidationStatus),
                    "valid",
                    "pending",
                    "failed",
                    "not_configured");

            if (!string.Equals(current.ProviderName, normalizedProviderName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(current.ProviderEnvelopeRef, normalizedProviderEnvelopeRef, StringComparison.OrdinalIgnoreCase))
            {
                AddAccessLog(
                    current.RecordId,
                    "signature.provider_reconciled",
                    "denied",
                    reconciledByPersonId,
                    null,
                    null,
                    null,
                    null,
                    "provider_envelope_mismatch");
                throw new InvalidOperationException($"Signature record {signatureRecordId} provider envelope evidence does not match.");
            }

            if (!string.IsNullOrWhiteSpace(normalizedCertificateFingerprint) &&
                !string.Equals(current.CertificateFingerprintSha256, normalizedCertificateFingerprint, StringComparison.OrdinalIgnoreCase))
            {
                AddAccessLog(
                    current.RecordId,
                    "signature.provider_reconciled",
                    "denied",
                    reconciledByPersonId,
                    null,
                    null,
                    null,
                    null,
                    "certificate_fingerprint_mismatch");
                throw new InvalidOperationException($"Signature record {signatureRecordId} certificate fingerprint evidence does not match.");
            }

            var receivedAt = DateTimeOffset.UtcNow;
            var providerCallbackEvidenceHash = ComputeSignatureProviderCallbackEvidenceHash(
                current,
                normalizedProviderName,
                normalizedProviderEnvelopeRef,
                normalizedCallbackStatus,
                normalizedCallbackRef,
                receivedAt,
                normalizedCertificateFingerprint ?? current.CertificateFingerprintSha256,
                normalizedTrustTimestampAuthorityRef,
                normalizedLongTermValidationStatus);

            var verificationStatus = normalizedCallbackStatus == "completed"
                ? "provider_verified"
                : "provider_rejected";
            var verificationFailureReason = normalizedCallbackStatus == "completed"
                ? null
                : $"provider_{normalizedCallbackStatus}";

            var updated = current with
            {
                VerificationStatus = verificationStatus,
                VerificationFailureReason = verificationFailureReason,
                ProviderCallbackStatus = normalizedCallbackStatus,
                ProviderCallbackRef = normalizedCallbackRef,
                ProviderCallbackReceivedAt = receivedAt,
                ProviderCallbackEvidenceHash = providerCallbackEvidenceHash,
                TrustTimestampAuthorityRef = normalizedTrustTimestampAuthorityRef,
                LongTermValidationStatus = normalizedLongTermValidationStatus
            };
            _signatureRecords[index] = updated;
            PersistSignatureRecord(tenantId, updated);
            AddAccessLog(
                current.RecordId,
                "signature.provider_reconciled",
                verificationStatus == "provider_verified" ? "allowed" : "denied",
                reconciledByPersonId,
                null,
                null,
                null,
                null,
                normalizedCallbackStatus);

            return new RecordArrSignatureProviderReconciliationResponse(
                updated.SignatureRecordId,
                tenantId,
                updated.RecordId,
                normalizedProviderName,
                normalizedProviderEnvelopeRef,
                normalizedCallbackStatus,
                normalizedCallbackRef,
                receivedAt,
                providerCallbackEvidenceHash,
                verificationStatus,
                verificationFailureReason,
                normalizedTrustTimestampAuthorityRef,
                normalizedLongTermValidationStatus,
                updated);
        }
    }

    public IReadOnlyList<RecordArrSignatureTrustServiceJobResponse> GetSignatureTrustServiceJobs(string tenantId)
    {
        lock (_gate)
        {
            return _signatureTrustServiceJobs
                .Where(job => string.Equals(job.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(job => job.RequestedAt)
                .ThenBy(job => job.TrustServiceJobId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public RecordArrSignatureTrustServiceJobResponse SubmitSignatureTrustServiceJob(
        string tenantId,
        string signatureRecordId,
        string requestedByPersonId,
        string? providerName,
        string? providerEnvelopeRef)
    {
        lock (_gate)
        {
            var signature = _signatureRecords.FirstOrDefault(item =>
                string.Equals(item.SignatureRecordId, signatureRecordId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(item.RecordId, tenantId));
            if (signature is null)
            {
                throw new InvalidOperationException($"Signature record {signatureRecordId} not found.");
            }

            if (string.IsNullOrWhiteSpace(signature.ProviderName) ||
                string.IsNullOrWhiteSpace(signature.ProviderEnvelopeRef) ||
                string.IsNullOrWhiteSpace(signature.CertificateFingerprintSha256) ||
                string.IsNullOrWhiteSpace(signature.SignatureEvidenceHash))
            {
                AddAccessLog(signature.RecordId, "signature.trust_service_job_submitted", "denied", requestedByPersonId, null, null, null, null, "provider_signature_evidence_missing");
                throw new InvalidOperationException($"Signature record {signatureRecordId} does not have provider signature evidence for trust-service submission.");
            }

            var normalizedProviderName = NormalizeRequiredEvidenceValue(providerName, nameof(providerName));
            var normalizedProviderEnvelopeRef = NormalizeRequiredEvidenceValue(providerEnvelopeRef, nameof(providerEnvelopeRef));
            if (!string.Equals(signature.ProviderName, normalizedProviderName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(signature.ProviderEnvelopeRef, normalizedProviderEnvelopeRef, StringComparison.OrdinalIgnoreCase))
            {
                AddAccessLog(signature.RecordId, "signature.trust_service_job_submitted", "denied", requestedByPersonId, null, null, null, null, "provider_envelope_mismatch");
                throw new InvalidOperationException($"Signature record {signatureRecordId} provider envelope evidence does not match.");
            }

            var existing = _signatureTrustServiceJobs.FirstOrDefault(job =>
                string.Equals(job.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(job.ProviderName, normalizedProviderName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(job.ProviderEnvelopeRef, normalizedProviderEnvelopeRef, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                return existing;
            }

            var requestedAt = DateTimeOffset.UtcNow;
            var submissionHash = ComputeSignatureTrustServiceJobSubmissionHash(
                signature,
                normalizedProviderName,
                normalizedProviderEnvelopeRef,
                requestedAt);
            var job = new RecordArrSignatureTrustServiceJobResponse(
                $"stj-{Guid.NewGuid():N}"[..12],
                tenantId,
                signature.SignatureRecordId,
                signature.RecordId,
                normalizedProviderName,
                normalizedProviderEnvelopeRef,
                "submitted",
                requestedByPersonId,
                requestedAt,
                signature.CertificateFingerprintSha256!,
                signature.SignatureEvidenceHash,
                submissionHash,
                LastSubmittedAt: requestedAt,
                SignatureRecord: signature);

            _signatureTrustServiceJobs.Add(job);
            PersistSignatureTrustServiceJob(job);
            AddAccessLog(signature.RecordId, "signature.trust_service_job_submitted", "allowed", requestedByPersonId, null, null, null, null, normalizedProviderEnvelopeRef);
            return job;
        }
    }

    public RecordArrSignatureTrustServiceJobResponse ProcessSignatureTrustServiceManifest(
        string tenantId,
        string processedByPersonId,
        string? providerName,
        string? providerEnvelopeRef,
        string? providerCallbackStatus,
        string? providerCallbackRef,
        string? certificateFingerprintSha256,
        string? trustTimestampAuthorityRef,
        string? longTermValidationStatus)
    {
        lock (_gate)
        {
            var normalizedProviderName = NormalizeRequiredEvidenceValue(providerName, nameof(providerName));
            var normalizedProviderEnvelopeRef = NormalizeRequiredEvidenceValue(providerEnvelopeRef, nameof(providerEnvelopeRef));
            var normalizedCallbackStatus = NormalizeRecordArrEnum(
                providerCallbackStatus ?? string.Empty,
                nameof(providerCallbackStatus),
                "completed",
                "declined",
                "failed",
                "expired",
                "voided");
            var normalizedCallbackRef = NormalizeRequiredEvidenceValue(providerCallbackRef, nameof(providerCallbackRef));
            var normalizedCertificateFingerprint = NormalizeRequiredEvidenceValue(certificateFingerprintSha256, nameof(certificateFingerprintSha256)).ToLowerInvariant();
            var index = _signatureTrustServiceJobs.FindIndex(job =>
                string.Equals(job.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(job.ProviderName, normalizedProviderName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(job.ProviderEnvelopeRef, normalizedProviderEnvelopeRef, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Signature trust-service job {normalizedProviderEnvelopeRef} not found.");
            }

            var job = _signatureTrustServiceJobs[index];
            var receivedAt = DateTimeOffset.UtcNow;
            if (!string.Equals(job.CertificateFingerprintSha256, normalizedCertificateFingerprint, StringComparison.OrdinalIgnoreCase))
            {
                var failedJob = job with
                {
                    Status = "failed",
                    ProviderCallbackStatus = normalizedCallbackStatus,
                    ProviderCallbackRef = normalizedCallbackRef,
                    ProviderCallbackReceivedAt = receivedAt,
                    FailureReason = "certificate_fingerprint_mismatch"
                };
                _signatureTrustServiceJobs[index] = failedJob;
                PersistSignatureTrustServiceJob(failedJob);
                AddAccessLog(job.RecordId, "signature.trust_service_job_reconciled", "denied", processedByPersonId, null, null, null, null, "certificate_fingerprint_mismatch");
                return failedJob;
            }

            var reconciliation = ReconcileSignatureProviderStatus(
                tenantId,
                job.SignatureRecordId,
                processedByPersonId,
                normalizedProviderName,
                normalizedProviderEnvelopeRef,
                normalizedCallbackStatus,
                normalizedCallbackRef,
                normalizedCertificateFingerprint,
                trustTimestampAuthorityRef,
                longTermValidationStatus);
            var status = normalizedCallbackStatus == "completed" ? "completed" : "failed";
            var failureReason = normalizedCallbackStatus == "completed" ? null : $"provider_{normalizedCallbackStatus}";
            var updatedJob = job with
            {
                Status = status,
                ProviderCallbackStatus = normalizedCallbackStatus,
                ProviderCallbackRef = normalizedCallbackRef,
                ProviderCallbackReceivedAt = reconciliation.ProviderCallbackReceivedAt,
                ProviderCallbackEvidenceHash = reconciliation.ProviderCallbackEvidenceHash,
                TrustTimestampAuthorityRef = reconciliation.TrustTimestampAuthorityRef,
                LongTermValidationStatus = reconciliation.LongTermValidationStatus,
                FailureReason = failureReason,
                SignatureRecord = reconciliation.SignatureRecord
            };
            _signatureTrustServiceJobs[index] = updatedJob;
            PersistSignatureTrustServiceJob(updatedJob);
            AddAccessLog(
                job.RecordId,
                "signature.trust_service_job_reconciled",
                status == "completed" ? "allowed" : "denied",
                processedByPersonId,
                null,
                null,
                null,
                null,
                normalizedCallbackStatus);
            return updatedJob;
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

    public IReadOnlyList<RecordArrPhotoEvidenceResponse> GetPhotoEvidence(string tenantId, string? recordId = null)
    {
        lock (_gate)
        {
            var query = _photoEvidence.Where(photo => RecordBelongsToTenant(photo.RecordId, tenantId));
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
        => CreatePhotoEvidence(
            ResolveRecordTenantId(recordId),
            recordId,
            photoPurpose,
            capturedByPersonId,
            sourceProduct,
            sourceObjectRef,
            geoCoordinates,
            deviceSnapshot,
            notes);

    public RecordArrPhotoEvidenceResponse CreatePhotoEvidence(
        string tenantId,
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
            if (!RecordBelongsToTenant(recordId, tenantId))
            {
                throw new InvalidOperationException($"Record {recordId} not found.");
            }

            EnsureRecordNotUnderActiveLegalHold(recordId, "photo_evidence.captured", capturedByPersonId);
            var normalizedPhotoPurpose = NormalizeRecordArrEnum(
                photoPurpose,
                nameof(photoPurpose),
                "defect",
                "damage",
                "completion",
                "before",
                "after",
                "receipt",
                "delivery",
                "quality",
                "incident",
                "audit",
                "training",
                "other");
            var file = CreateFileObject(
                tenantId,
                recordId,
                $"photo-{normalizedPhotoPurpose}.jpg",
                "image/jpeg",
                capturedByPersonId,
                sizeBytes: 256_000,
                imageWidth: 1920,
                imageHeight: 1080,
                attachToRecord: true,
                setAsCurrentFile: false);

            var photo = new RecordArrPhotoEvidenceResponse(
                $"pho-{Guid.NewGuid():N}"[..12],
                tenantId,
                recordId,
                normalizedPhotoPurpose,
                sourceProduct,
                sourceObjectRef,
                DateTimeOffset.UtcNow,
                capturedByPersonId,
                geoCoordinates,
                deviceSnapshot,
                notes);
            _photoEvidence.Add(photo);
            PersistFile(file);
            var updatedRecord = _records.FirstOrDefault(record => string.Equals(record.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            if (updatedRecord is not null)
            {
                PersistRecord(updatedRecord);
            }

            PersistPhotoEvidence(tenantId, photo);
            AddAccessLog(recordId, "photo_evidence.captured", "allowed", capturedByPersonId, null, null, null, null, "photo-evidence-captured");
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
            EnsureRecordNotUnderActiveLegalHold(recordId, "record.status.update", null);
            var updated = _records[index] with
            {
                Status = normalizedStatus,
                Classification = classification is null ? _records[index].Classification : NormalizeClassification(classification),
                EffectiveAt = effectiveAt ?? _records[index].EffectiveAt,
                ExpiresAt = expiresAt ?? _records[index].ExpiresAt
            };
            _records[index] = updated;
            PersistRecord(updated);
            return ProjectRecord(updated);
        }
    }

    public RecordArrRecordResponse ArchiveRecord(string recordId, string actorPersonId)
    {
        lock (_gate)
        {
            var record = RequireRecord(recordId);
            EnsureRecordCanBeDisposed(recordId, "archive", actorPersonId);
            return UpdateRecordLifecycle(record, "archived", actorPersonId, "archive");
        }
    }

    public RecordArrRecordResponse PurgeRecord(string recordId, string actorPersonId)
    {
        lock (_gate)
        {
            var record = RequireRecord(recordId);
            EnsureRecordCanBeDisposed(recordId, "purge", actorPersonId);
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
            EnsureRecordNotUnderActiveLegalHold(record.RecordId, "metadata.create", createdByPersonId);
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
            PersistRecordMetadata(ResolveRecordTenantId(record.RecordId), metadata);
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
            EnsureRecordNotUnderActiveLegalHold(record.RecordId, "link.create", createdByPersonId);
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
            PersistRecordLink(ResolveRecordTenantId(record.RecordId), link);
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
            EnsureRecordNotUnderActiveLegalHold(record.RecordId, "comment.create", createdByPersonId);
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
            PersistRecordComment(ResolveRecordTenantId(record.RecordId), comment);
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
            EnsureRecordNotUnderActiveLegalHold(current.RecordId, "comment.update", editedByPersonId);
            var updated = current with
            {
                Body = body.Trim(),
                Visibility = NormalizeRecordCommentVisibility(visibility),
                EditedAt = DateTimeOffset.UtcNow,
                EditedByPersonId = editedByPersonId
            };
            _recordComments[index] = updated;
            PersistRecordComment(ResolveRecordTenantId(updated.RecordId), updated);
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

    private static string NormalizeChecksum(string? checksum)
    {
        if (string.IsNullOrWhiteSpace(checksum))
        {
            throw new InvalidOperationException("'checksumSha256' is required.");
        }

        return checksum.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptionalChecksum(string? checksum)
        => string.IsNullOrWhiteSpace(checksum) ? null : checksum.Trim().ToLowerInvariant();

    private static string NormalizeRequiredEvidenceValue(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required for provider evidence verification.");
        }

        return value.Trim();
    }

    private static IReadOnlyList<string> NormalizeRequiredEvidenceRefs(IReadOnlyList<string>? values, string fieldName)
    {
        var normalized = NormalizeOptionalEvidenceRefs(values);
        if (normalized.Count == 0)
        {
            throw new InvalidOperationException($"{fieldName} must include at least one evidence reference.");
        }

        return normalized;
    }

    private static IReadOnlyList<string> NormalizeOptionalEvidenceRefs(IReadOnlyList<string>? values)
        => values?
               .Select(value => value?.Trim())
               .Where(value => !string.IsNullOrWhiteSpace(value))
               .Select(value => value!)
               .Distinct(StringComparer.OrdinalIgnoreCase)
               .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
               .ToArray() ??
           Array.Empty<string>();

    private DateTimeOffset? GetRequiredObjectRetainUntil(string recordId)
    {
        var retentionStatus = _retentionStatuses
            .Where(status => string.Equals(status.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(status => status.RetentionExpiresAt ?? DateTimeOffset.MinValue)
            .FirstOrDefault();
        if (retentionStatus?.RetentionExpiresAt is not null)
        {
            return retentionStatus.RetentionExpiresAt;
        }

        return _records.FirstOrDefault(record =>
            string.Equals(record.RecordId, recordId, StringComparison.OrdinalIgnoreCase))?.ExpiresAt;
    }

    private void AppendRecordLinkAuditTrail(RecordArrRecordResponse record, string action, string actorPersonId, string details)
    {
        AddAccessLog(record.RecordId, action, "allowed", actorPersonId, null, null, null, null, details);
    }

    public RecordArrUploadSessionResponse CreateUploadSession(string tenantId, string sourceProduct, string sourceObjectType, string sourceObjectId, string uploadPurpose, bool requiresDocumentScan, bool requiresOcr, bool requiresManualReview)
    {
        lock (_gate)
        {
            if (!Guid.TryParse(tenantId, out _))
            {
                throw new InvalidOperationException("Upload session tenantId is required.");
            }

            var session = new RecordArrUploadSessionResponse(
                $"upl-{Guid.NewGuid():N}"[..12],
                tenantId.Trim(),
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
            PersistUploadSession(session);
            return session;
        }
    }

    public RecordArrUploadSessionResponse? GetUploadSession(string tenantId, string uploadSessionId)
    {
        lock (_gate)
        {
            return _uploadSessions.FirstOrDefault(session =>
                string.Equals(session.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(session.UploadSessionId, uploadSessionId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyList<RecordArrUploadSessionResponse> GetUploadSessions(string tenantId)
    {
        lock (_gate)
        {
            return _uploadSessions
                .Where(session => string.Equals(session.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(session => session.CreatedAt)
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrCaptureRequestResponse> GetCaptureRequests(string tenantId)
    {
        lock (_gate)
        {
            return _captureRequests
                .Where(request => string.Equals(request.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(request => request.CreatedAt)
                .ToArray();
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
            PersistCaptureRequest(request);
            return request;
        }
    }

    public RecordArrCaptureRequestResponse CompleteCaptureRequest(string tenantId, string captureRequestId) => UpdateCaptureRequestStatus(tenantId, captureRequestId, "completed");

    public RecordArrCaptureRequestResponse SkipCaptureRequest(string tenantId, string captureRequestId) => UpdateCaptureRequestStatus(tenantId, captureRequestId, "skipped");

    public RecordArrCaptureRequestResponse CancelCaptureRequest(string tenantId, string captureRequestId) => UpdateCaptureRequestStatus(tenantId, captureRequestId, "canceled");

    public RecordArrCaptureRequestResponse ExpireCaptureRequest(string tenantId, string captureRequestId) => UpdateCaptureRequestStatus(tenantId, captureRequestId, "expired");

    public RecordArrUploadSessionResponse CompleteUploadSession(string tenantId, string uploadSessionId, string recordId)
    {
        lock (_gate)
        {
            var index = _uploadSessions.FindIndex(session =>
                string.Equals(session.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(session.UploadSessionId, uploadSessionId, StringComparison.OrdinalIgnoreCase));
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
            PersistUploadSession(updated);
            MarkCaptureRequestsCompleted(tenantId, uploadSessionId, recordId);
            return updated;
        }
    }

    public RecordArrUploadSessionResponse RevokeUploadSession(string tenantId, string uploadSessionId, string reason)
    {
        lock (_gate)
        {
            var index = _uploadSessions.FindIndex(session =>
                string.Equals(session.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(session.UploadSessionId, uploadSessionId, StringComparison.OrdinalIgnoreCase));
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
            AddAccessLog(current.UploadedRecordRefs.FirstOrDefault() ?? current.SourceObjectId, "share", "denied", null, null, null, null, null, reason);
            PersistUploadSession(updated);
            return updated;
        }
    }

    private RecordArrCaptureRequestResponse UpdateCaptureRequestStatus(string tenantId, string captureRequestId, string status)
    {
        lock (_gate)
        {
            var index = _captureRequests.FindIndex(request =>
                string.Equals(request.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(request.CaptureRequestId, captureRequestId, StringComparison.OrdinalIgnoreCase));
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
            PersistCaptureRequest(updated);
            return updated;
        }
    }

    private void MarkCaptureRequestsCompleted(string tenantId, string uploadSessionId, string recordId)
    {
        for (var i = 0; i < _captureRequests.Count; i++)
        {
            var current = _captureRequests[i];
            if (!string.Equals(current.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(current.UploadSessionRef, uploadSessionId, StringComparison.OrdinalIgnoreCase))
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
            PersistCaptureRequest(_captureRequests[i]);
            AddAccessLog(recordId, "capture_request", "allowed", null, null, null, null, null, $"Capture request {current.CaptureType} completed from upload session {uploadSessionId}.");
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
                scanId,
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
            PersistFile(originalFile);
            PersistFile(generatedPdfFile);
            var updatedRecord = _records.FirstOrDefault(record => string.Equals(record.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
            if (updatedRecord is not null)
            {
                PersistRecord(updatedRecord);
            }
            PersistOcrResult(ResolveRecordTenantId(recordId), ocrResult);
            PersistExtractionResult(ResolveRecordTenantId(recordId), extractionResult);
            PersistScanProcessing(ResolveRecordTenantId(recordId), scan);
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
            PersistScanProcessing(ResolveRecordTenantId(updated.RecordId), updated);
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
            PersistExtractionResult(ResolveRecordTenantId(updated.RecordId), updated);
            return updated;
        }
    }

    public IReadOnlyList<RecordArrEvidenceMappingResponse> GetEvidenceMappings(string tenantId)
    {
        lock (_gate)
        {
            return _evidenceMappings
                .Where(mapping => string.Equals(ResolveRecordTenantId(mapping.RecordId), tenantId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrEvidenceCoverageResponse> GetEvidenceCoverage(string tenantId)
    {
        lock (_gate)
        {
            _evidenceCoverage.Clear();
            _evidenceCoverage.AddRange(BuildEvidenceCoverage(tenantId));
            return _evidenceCoverage.ToArray();
        }
    }

    public RecordArrEvidenceMappingResponse CreateEvidenceMapping(string recordId, string sourceProduct, string sourceObjectType, string sourceObjectId, string complianceRequirementRef, string evidenceTypeKey, string mappingSource, decimal confidenceScore)
    {
        lock (_gate)
        {
            RequireRecord(recordId);
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
            _evidenceCoverage.AddRange(BuildEvidenceCoverage(ResolveRecordTenantId(recordId)));
            PersistEvidenceMapping(ResolveRecordTenantId(recordId), mapping);
            return mapping;
        }
    }

    public RecordArrEvidenceMappingResponse UpdateEvidenceMapping(string tenantId, string mappingId, string status, string? personId, string? notes, string? reason)
    {
        lock (_gate)
        {
            var index = _evidenceMappings.FindIndex(mapping =>
                string.Equals(ResolveRecordTenantId(mapping.RecordId), tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(mapping.EvidenceMappingId, mappingId, StringComparison.OrdinalIgnoreCase));
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
            _evidenceCoverage.AddRange(BuildEvidenceCoverage(tenantId));
            PersistEvidenceMapping(tenantId, updated);
            return updated;
        }
    }

    public IReadOnlyList<RecordArrPackageResponse> GetPackages(string tenantId)
    {
        lock (_gate)
        {
            return _packages
                .Where(package => string.Equals(ResolvePackageTenantId(package), tenantId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }

    public RecordArrPackageResponse? GetPackage(string tenantId, string packageId)
    {
        lock (_gate)
        {
            return _packages.FirstOrDefault(pkg =>
                string.Equals(pkg.PackageId, packageId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ResolvePackageTenantId(pkg), tenantId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public RecordArrPackageResponse CreatePackage(string tenantId, string title, string packageType, string sourceProduct, string sourceObjectRef, string recordRef, string requestedByPersonId)
    {
        lock (_gate)
        {
            RequireRecord(recordRef);
            var recordTenantId = ResolveRecordTenantId(recordRef);
            if (!string.Equals(recordTenantId, tenantId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Record {recordRef} was not found for the current tenant.");
            }
            EnsureRecordNotUnderActiveLegalHold(recordRef, "package.created", requestedByPersonId);

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
                recordTenantId,
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
                recordTenantId,
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
            PersistFile(generatedPdfFile);
            PersistFile(generatedZipFile);
            var recordIndex = _records.FindIndex(record => string.Equals(record.RecordId, recordRef, StringComparison.OrdinalIgnoreCase));
            if (recordIndex >= 0)
            {
                PersistRecord(_records[recordIndex]);
            }

            PersistPackage(recordTenantId, package);
            PersistPackageManifest(recordTenantId, manifest);
            return package;
        }
    }

    public RecordArrPackageResponse LockPackage(string tenantId, string packageId)
    {
        lock (_gate)
        {
            var index = _packages.FindIndex(pkg =>
                string.Equals(pkg.PackageId, packageId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ResolvePackageTenantId(pkg), tenantId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Package {packageId} not found.");
            }

            var current = _packages[index];
            EnsureRecordRefsNotUnderActiveLegalHold(current.RecordRefs, "package.locked", "system");
            var updated = current with
            {
                Status = "locked",
                LockedAt = DateTimeOffset.UtcNow
            };
            _packages[index] = updated;
            AddAccessLog(current.RecordRefs.FirstOrDefault() ?? current.SourceObjectRefs.FirstOrDefault() ?? packageId, "package.locked", "allowed", "system", null, null, null, null, "package-locked");
            PersistPackage(tenantId, updated);
            return updated;
        }
    }

    public RecordArrPackageResponse ArchivePackage(string tenantId, string packageId)
    {
        lock (_gate)
        {
            var index = _packages.FindIndex(pkg =>
                string.Equals(pkg.PackageId, packageId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ResolvePackageTenantId(pkg), tenantId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Package {packageId} not found.");
            }

            var current = _packages[index];
            if (string.Equals(current.Status, "archived", StringComparison.OrdinalIgnoreCase))
            {
                return current;
            }
            EnsureRecordRefsNotUnderActiveLegalHold(current.RecordRefs, "package.archived", "system");

            var updated = current with
            {
                Status = "archived",
                ArchivedAt = DateTimeOffset.UtcNow
            };
            _packages[index] = updated;
            AddAccessLog(current.RecordRefs.FirstOrDefault() ?? current.SourceObjectRefs.FirstOrDefault() ?? packageId, "package.archived", "allowed", "system", null, null, null, null, "package-archived");
            PersistPackage(tenantId, updated);
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

    public RecordArrRetentionStatusResponse? GetRetentionStatus(string tenantId, string recordId)
    {
        lock (_gate)
        {
            if (!RecordBelongsToTenant(recordId, tenantId))
            {
                return null;
            }

            return _retentionStatuses.FirstOrDefault(status => string.Equals(status.RecordId, recordId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyList<RecordArrRetentionStatusResponse> RecalculateRetentionStatuses(string tenantId)
    {
        lock (_gate)
        {
            CreateMissingRetentionStatuses(tenantId);
            RefreshRetentionStatusesForActiveLegalHolds(tenantId);
            return _retentionStatuses
                .Where(status => RecordBelongsToTenant(status.RecordId, tenantId))
                .ToArray();
        }
    }

    public RecordArrRetentionSchedulerRunResponse RunRetentionDispositionScheduler(string tenantId, string requestedByPersonId)
        => RunRetentionDispositionScheduler(tenantId, requestedByPersonId, executionPolicy: null);

    public RecordArrRetentionSchedulerRunResponse RunRetentionDispositionScheduler(string tenantId, string requestedByPersonId, string? executionPolicy)
    {
        lock (_gate)
        {
            const string schedulerKey = "retention-disposition";
            var normalizedExecutionPolicy = NormalizeRetentionDispositionExecutionPolicy(executionPolicy);
            var now = DateTimeOffset.UtcNow;
            var activeLease = _retentionSchedulerLeases.FirstOrDefault(lease =>
                string.Equals(lease.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(lease.SchedulerKey, schedulerKey, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(lease.Status, "acquired", StringComparison.OrdinalIgnoreCase) &&
                lease.ExpiresAt > now);
            if (activeLease is not null)
            {
                throw new InvalidOperationException("Retention disposition scheduler is already running for this tenant.");
            }

            var schedulerRunId = $"rsrun-{Guid.NewGuid():N}"[..18];
            var leaseId = $"rlease-{Guid.NewGuid():N}"[..19];
            var lease = new RecordArrRetentionSchedulerLeaseResponse(
                leaseId,
                tenantId,
                schedulerKey,
                "acquired",
                now,
                now.AddMinutes(15),
                null,
                requestedByPersonId,
                schedulerRunId);
            _retentionSchedulerLeases.Add(lease);
            PersistRetentionSchedulerLease(tenantId, lease);

            var statuses = RecalculateRetentionStatuses(tenantId);
            var eligibleStatuses = statuses
                .Where(status => status.Status is "eligible_for_archive" or "eligible_for_purge")
                .OrderBy(status => status.RetentionExpiresAt ?? DateTimeOffset.MaxValue)
                .ThenBy(status => status.RecordId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var createdReviewRefs = new List<string>();
            var outboxMessageRefs = new List<string>();
            var blockedRecordRefs = statuses
                .Where(status => string.Equals(status.Status, "blocked_by_legal_hold", StringComparison.OrdinalIgnoreCase))
                .Select(status => status.RecordId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(recordId => recordId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var skippedExistingReviewCount = 0;

            if (!string.Equals(normalizedExecutionPolicy, "create_pending_reviews_only", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(normalizedExecutionPolicy, "execute_approved_reviews", StringComparison.OrdinalIgnoreCase))
                {
                    var execution = ExecuteApprovedDisposalReviewsForScheduler(
                        tenantId,
                        requestedByPersonId,
                        schedulerRunId);

                    var executeReleasedLease = lease with
                    {
                        Status = "released",
                        ReleasedAt = DateTimeOffset.UtcNow
                    };
                    var executeLeaseIndex = _retentionSchedulerLeases.FindIndex(item => string.Equals(item.LeaseId, lease.LeaseId, StringComparison.OrdinalIgnoreCase));
                    if (executeLeaseIndex >= 0)
                    {
                        _retentionSchedulerLeases[executeLeaseIndex] = executeReleasedLease;
                    }
                    PersistRetentionSchedulerLease(tenantId, executeReleasedLease);

                    var executeRun = new RecordArrRetentionSchedulerRunResponse(
                        schedulerRunId,
                        leaseId,
                        tenantId,
                        now,
                        requestedByPersonId,
                        "completed",
                        normalizedExecutionPolicy,
                        statuses.Count,
                        eligibleStatuses.Length,
                        0,
                        execution.SkippedReviewCount,
                        blockedRecordRefs.Concat(execution.BlockedRecordRefs)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count(),
                        execution.ExecutedReviewRefs.Count,
                        0,
                        Array.Empty<string>(),
                        blockedRecordRefs.Concat(execution.BlockedRecordRefs)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(recordId => recordId, StringComparer.OrdinalIgnoreCase)
                            .ToArray(),
                        Array.Empty<string>(),
                        null);
                    _retentionSchedulerRuns.Add(executeRun);
                    PersistRetentionSchedulerRun(tenantId, executeRun);
                    return executeRun;
                }

                var failureReason = $"Retention disposition execution policy '{normalizedExecutionPolicy}' is not supported. The scheduler only creates pending human review records; automatic archive or purge execution must be completed through approved disposal reviews.";
                foreach (var status in eligibleStatuses)
                {
                    AddAccessLog(status.RecordId, "retention.scheduler.execution_policy_unsupported", "denied", requestedByPersonId, null, null, null, null, normalizedExecutionPolicy);
                }

                var failedReleasedLease = lease with
                {
                    Status = "released",
                    ReleasedAt = DateTimeOffset.UtcNow
                };
                var failedLeaseIndex = _retentionSchedulerLeases.FindIndex(item => string.Equals(item.LeaseId, lease.LeaseId, StringComparison.OrdinalIgnoreCase));
                if (failedLeaseIndex >= 0)
                {
                    _retentionSchedulerLeases[failedLeaseIndex] = failedReleasedLease;
                }
                PersistRetentionSchedulerLease(tenantId, failedReleasedLease);

                var failedRun = new RecordArrRetentionSchedulerRunResponse(
                    schedulerRunId,
                    leaseId,
                    tenantId,
                    now,
                    requestedByPersonId,
                    "failed",
                    normalizedExecutionPolicy,
                    statuses.Count,
                    eligibleStatuses.Length,
                    0,
                    0,
                    blockedRecordRefs.Length,
                    0,
                    0,
                    Array.Empty<string>(),
                    blockedRecordRefs,
                    Array.Empty<string>(),
                    failureReason);
                _retentionSchedulerRuns.Add(failedRun);
                PersistRetentionSchedulerRun(tenantId, failedRun);
                return failedRun;
            }

            foreach (var status in eligibleStatuses)
            {
                if (HasOpenDisposalReview(tenantId, status))
                {
                    skippedExistingReviewCount++;
                    continue;
                }

                var proposedAction = string.Equals(status.Status, "eligible_for_purge", StringComparison.OrdinalIgnoreCase)
                    ? "purge"
                    : "archive";
                var review = CreateDisposalReview(
                    tenantId,
                    status.RecordId,
                    status.RetentionStatusId,
                    proposedAction,
                    requestedByPersonId);
                createdReviewRefs.Add(review.DisposalReviewId);
                var outboxMessage = CreateRetentionSchedulerOutboxMessage(
                    tenantId,
                    schedulerRunId,
                    status,
                    review,
                    requestedByPersonId,
                    proposedAction);
                outboxMessageRefs.Add(outboxMessage.OutboxMessageId);
                AddAccessLog(status.RecordId, "retention.scheduler.review_created", "allowed", requestedByPersonId, null, null, null, null, proposedAction);
            }

            var releasedLease = lease with
            {
                Status = "released",
                ReleasedAt = DateTimeOffset.UtcNow
            };
            var leaseIndex = _retentionSchedulerLeases.FindIndex(item => string.Equals(item.LeaseId, lease.LeaseId, StringComparison.OrdinalIgnoreCase));
            if (leaseIndex >= 0)
            {
                _retentionSchedulerLeases[leaseIndex] = releasedLease;
            }
            PersistRetentionSchedulerLease(tenantId, releasedLease);

            var run = new RecordArrRetentionSchedulerRunResponse(
                schedulerRunId,
                leaseId,
                tenantId,
                now,
                requestedByPersonId,
                "completed",
                normalizedExecutionPolicy,
                statuses.Count,
                eligibleStatuses.Length,
                createdReviewRefs.Count,
                skippedExistingReviewCount,
                blockedRecordRefs.Length,
                0,
                outboxMessageRefs.Count,
                createdReviewRefs.ToArray(),
                blockedRecordRefs,
                outboxMessageRefs.ToArray(),
                null);
            _retentionSchedulerRuns.Add(run);
            PersistRetentionSchedulerRun(tenantId, run);
            return run;
        }
    }

    private RetentionApprovedReviewExecutionResult ExecuteApprovedDisposalReviewsForScheduler(
        string tenantId,
        string executedByPersonId,
        string schedulerRunId)
    {
        var approvedReviewIndexes = _disposalReviews
            .Select((review, index) => new { Review = review, Index = index })
            .Where(item =>
                string.Equals(item.Review.Status, "approved", StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(item.Review.RecordId, tenantId))
            .OrderBy(item => item.Review.ReviewedAt ?? item.Review.RequestedAt)
            .ThenBy(item => item.Review.DisposalReviewId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var executedReviewRefs = new List<string>();
        var blockedRecordRefs = new List<string>();
        var skippedReviewCount = 0;

        foreach (var item in approvedReviewIndexes)
        {
            var review = _disposalReviews[item.Index];
            var retentionIndex = _retentionStatuses.FindIndex(status =>
                string.Equals(status.RetentionStatusId, review.RetentionStatusRef, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(status.RecordId, review.RecordId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(status.RecordId, tenantId));
            if (retentionIndex < 0)
            {
                skippedReviewCount++;
                AddAccessLog(review.RecordId, "retention.scheduler.approved_review_missing_retention_status", "denied", executedByPersonId, null, null, null, null, schedulerRunId);
                continue;
            }

            if (RecordHasActiveLegalHold(review.RecordId, "retention.scheduler.approved_review_blocked_by_legal_hold", executedByPersonId))
            {
                _retentionStatuses[retentionIndex] = _retentionStatuses[retentionIndex] with
                {
                    Status = "blocked_by_legal_hold"
                };
                PersistRetentionStatus(tenantId, _retentionStatuses[retentionIndex]);
                blockedRecordRefs.Add(review.RecordId);
                skippedReviewCount++;
                continue;
            }

            if (!IsApprovedDisposalReviewStillEligible(_retentionStatuses[retentionIndex], review))
            {
                skippedReviewCount++;
                AddAccessLog(review.RecordId, "retention.scheduler.approved_review_not_eligible", "denied", executedByPersonId, null, null, null, null, _retentionStatuses[retentionIndex].Status);
                continue;
            }

            ApplyDisposalReviewOutcome(tenantId, review);
            var currentReviewIndex = _disposalReviews.FindIndex(candidate =>
                string.Equals(candidate.DisposalReviewId, review.DisposalReviewId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(candidate.RecordId, tenantId));
            if (currentReviewIndex >= 0)
            {
                var completedReview = _disposalReviews[currentReviewIndex] with
                {
                    Status = "completed",
                    CompletedAt = DateTimeOffset.UtcNow,
                    DecisionReason = string.IsNullOrWhiteSpace(_disposalReviews[currentReviewIndex].DecisionReason)
                        ? "Executed by retention disposition scheduler after approval."
                        : _disposalReviews[currentReviewIndex].DecisionReason
                };
                _disposalReviews[currentReviewIndex] = completedReview;
                PersistDisposalReview(tenantId, completedReview);
            }

            AddAccessLog(review.RecordId, "retention.scheduler.approved_review_executed", "allowed", executedByPersonId, null, null, null, null, schedulerRunId);
            executedReviewRefs.Add(review.DisposalReviewId);
        }

        return new RetentionApprovedReviewExecutionResult(
            executedReviewRefs.ToArray(),
            blockedRecordRefs
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(recordId => recordId, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            skippedReviewCount);
    }

    private static bool IsApprovedDisposalReviewStillEligible(
        RecordArrRetentionStatusResponse retentionStatus,
        RecordArrDisposalReviewResponse review)
    {
        var proposedAction = review.ProposedAction.Trim().ToLowerInvariant();
        return proposedAction switch
        {
            "archive" => string.Equals(retentionStatus.Status, "eligible_for_archive", StringComparison.OrdinalIgnoreCase),
            "purge" => string.Equals(retentionStatus.Status, "eligible_for_purge", StringComparison.OrdinalIgnoreCase),
            "retain" or "anonymize" => true,
            _ => false
        };
    }

    private sealed record RetentionApprovedReviewExecutionResult(
        IReadOnlyList<string> ExecutedReviewRefs,
        IReadOnlyList<string> BlockedRecordRefs,
        int SkippedReviewCount);

    private static string NormalizeRetentionDispositionExecutionPolicy(string? executionPolicy)
        => string.IsNullOrWhiteSpace(executionPolicy)
            ? "create_pending_reviews_only"
            : executionPolicy.Trim().ToLowerInvariant();

    public IReadOnlyList<RecordArrRetentionSchedulerRunResponse> GetRetentionSchedulerRuns(string tenantId)
    {
        lock (_gate)
        {
            return _retentionSchedulerRuns
                .Where(run => string.Equals(run.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(run => run.RanAt)
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrRetentionSchedulerLeaseResponse> GetRetentionSchedulerLeases(string tenantId)
    {
        lock (_gate)
        {
            return _retentionSchedulerLeases
                .Where(lease => string.Equals(lease.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(lease => lease.AcquiredAt)
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrRetentionSchedulerOutboxMessageResponse> GetRetentionSchedulerOutboxMessages(string tenantId)
    {
        lock (_gate)
        {
            return _retentionSchedulerOutboxMessages
                .Where(message => string.Equals(message.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(message => message.CreatedAt)
                .ToArray();
        }
    }

    private static RecordArrRetentionSchedulerOutboxMessageResponse NormalizeRetentionSchedulerOutboxMessage(RecordArrRetentionSchedulerOutboxMessageResponse message)
        => message with
        {
            DeliveryChannel = string.IsNullOrWhiteSpace(message.DeliveryChannel) ? "in_app" : NormalizeRetentionNotificationChannel(message.DeliveryChannel),
            RecipientRef = string.IsNullOrWhiteSpace(message.RecipientRef) ? "role:records" : message.RecipientRef.Trim(),
            DueAt = message.DueAt ?? message.CreatedAt.AddDays(3),
            EscalateAfter = message.EscalateAfter ?? message.CreatedAt.AddDays(1),
            EscalationLevel = Math.Max(0, message.EscalationLevel),
            ExternalProviderRef = string.IsNullOrWhiteSpace(message.ExternalProviderRef) ? null : message.ExternalProviderRef.Trim()
        };

    private static string NormalizeRetentionNotificationChannel(string? deliveryChannel)
        => NormalizeRecordArrEnum(
            string.IsNullOrWhiteSpace(deliveryChannel) ? "in_app" : deliveryChannel,
            nameof(deliveryChannel),
            "in_app",
            "email",
            "sms",
            "push",
            "webhook",
            "external_system");

    private RecordArrRetentionSchedulerOutboxMessageResponse CreateRetentionSchedulerOutboxMessage(
        string tenantId,
        string schedulerRunId,
        RecordArrRetentionStatusResponse status,
        RecordArrDisposalReviewResponse review,
        string requestedByPersonId,
        string proposedAction)
    {
        var deduplicationKey = string.Join(
            ":",
            "recordarr",
            tenantId,
            "retention-disposition-review",
            review.DisposalReviewId);
        var existing = _retentionSchedulerOutboxMessages.FirstOrDefault(message =>
            string.Equals(message.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(message.DeduplicationKey, deduplicationKey, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            return existing;
        }

        var message = new RecordArrRetentionSchedulerOutboxMessageResponse(
            $"rout-{Guid.NewGuid():N}"[..17],
            tenantId,
            schedulerRunId,
            "recordarr.retention.disposal_review.created",
            "pending",
            status.RecordId,
            review.DisposalReviewId,
            DateTimeOffset.UtcNow,
            requestedByPersonId,
            deduplicationKey,
            "Disposition review required",
            $"Record {status.RecordId} is eligible for {proposedAction} review.",
            "medium",
            $"/recordarr/records/{status.RecordId}/retention",
            null,
            0,
            null,
            null,
            null,
            "in_app",
            "role:records",
            DateTimeOffset.UtcNow.AddDays(3),
            DateTimeOffset.UtcNow.AddDays(1),
            0,
            null,
            null,
            null);
        _retentionSchedulerOutboxMessages.Add(message);
        PersistRetentionSchedulerOutboxMessage(tenantId, message);
        return message;
    }

    public RecordArrRetentionSchedulerOutboxDeliveryRunResponse ProcessRetentionSchedulerOutbox(string tenantId, string requestedByPersonId, int maxMessages = 100)
        => ProcessRetentionSchedulerOutbox(tenantId, requestedByPersonId, deliveryChannel: null, externalProviderRef: null, maxMessages);

    public RecordArrRetentionSchedulerOutboxDeliveryRunResponse ProcessRetentionSchedulerOutbox(
        string tenantId,
        string requestedByPersonId,
        string? deliveryChannel,
        string? externalProviderRef,
        int maxMessages = 100)
    {
        lock (_gate)
        {
            var normalizedDeliveryChannel = NormalizeRetentionNotificationChannel(deliveryChannel);
            var normalizedExternalProviderRef = string.IsNullOrWhiteSpace(externalProviderRef) ? null : externalProviderRef.Trim();
            var pendingMessages = _retentionSchedulerOutboxMessages
                .Where(message =>
                    string.Equals(message.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(message.Status, "pending", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(message.Status, "failed", StringComparison.OrdinalIgnoreCase)))
                .OrderBy(message => message.CreatedAt)
                .Take(Math.Clamp(maxMessages, 1, 500))
                .ToArray();
            var deliveredMessageRefs = new List<string>();
            var failedMessageRefs = new List<string>();
            var processedAt = DateTimeOffset.UtcNow;

            foreach (var message in pendingMessages)
            {
                var index = _retentionSchedulerOutboxMessages.FindIndex(item =>
                    string.Equals(item.OutboxMessageId, message.OutboxMessageId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));
                if (index < 0)
                {
                    continue;
                }

                var effectiveChannel = normalizedDeliveryChannel ?? NormalizeRetentionNotificationChannel(message.DeliveryChannel);
                var effectiveProviderRef = normalizedExternalProviderRef ?? (string.IsNullOrWhiteSpace(message.ExternalProviderRef) ? null : message.ExternalProviderRef.Trim());
                var hasCanonicalTaskRoute = !string.IsNullOrWhiteSpace(message.ActionRoute) &&
                    !string.IsNullOrWhiteSpace(message.DisposalReviewRef);
                var canDeliver = hasCanonicalTaskRoute &&
                    (string.Equals(effectiveChannel, "in_app", StringComparison.OrdinalIgnoreCase) ||
                     !string.IsNullOrWhiteSpace(effectiveProviderRef));
                var errorMessage = hasCanonicalTaskRoute
                    ? $"External delivery channel '{effectiveChannel}' requires a configured provider reference."
                    : "Notification is missing a canonical action route or disposal review reference.";
                var updated = message with
                {
                    Status = canDeliver ? "delivered" : "failed",
                    DeliveryAttemptCount = message.DeliveryAttemptCount + 1,
                    LastAttemptAt = processedAt,
                    DeliveredAt = canDeliver ? processedAt : null,
                    DeliveredByPersonId = canDeliver ? requestedByPersonId : null,
                    ErrorMessage = canDeliver ? null : errorMessage,
                    DeliveryChannel = effectiveChannel,
                    ExternalProviderRef = effectiveProviderRef
                };
                _retentionSchedulerOutboxMessages[index] = updated;
                PersistRetentionSchedulerOutboxMessage(tenantId, updated);

                if (canDeliver)
                {
                    deliveredMessageRefs.Add(updated.OutboxMessageId);
                    AddAccessLog(
                        updated.TargetRecordId,
                        "retention.scheduler.outbox_delivered",
                        "allowed",
                        requestedByPersonId,
                        null,
                        null,
                        null,
                        null,
                        updated.OutboxMessageId);
                }
                else
                {
                    failedMessageRefs.Add(updated.OutboxMessageId);
                    AddAccessLog(
                        updated.TargetRecordId,
                        "retention.scheduler.outbox_delivery_failed",
                        "denied",
                        requestedByPersonId,
                        null,
                        null,
                        null,
                        null,
                        updated.ErrorMessage);
                }
            }

            return new RecordArrRetentionSchedulerOutboxDeliveryRunResponse(
                tenantId,
                processedAt,
                requestedByPersonId,
                pendingMessages.Length,
                deliveredMessageRefs.Count,
                failedMessageRefs.Count,
                deliveredMessageRefs.ToArray(),
                failedMessageRefs.ToArray());
        }
    }

    public RecordArrRetentionSchedulerOutboxEscalationRunResponse EscalateRetentionSchedulerOutbox(
        string tenantId,
        string requestedByPersonId,
        string escalationRecipientRef,
        int maxMessages = 100)
    {
        lock (_gate)
        {
            if (string.IsNullOrWhiteSpace(escalationRecipientRef))
            {
                throw new InvalidOperationException("Escalation recipient is required.");
            }

            var escalatedAt = DateTimeOffset.UtcNow;
            var eligibleMessages = _retentionSchedulerOutboxMessages
                .Where(message =>
                    string.Equals(message.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(message.Status, "pending", StringComparison.OrdinalIgnoreCase) && message.EscalateAfter.HasValue && message.EscalateAfter <= escalatedAt ||
                     string.Equals(message.Status, "failed", StringComparison.OrdinalIgnoreCase)))
                .OrderBy(message => message.EscalateAfter ?? message.LastAttemptAt ?? message.CreatedAt)
                .Take(Math.Clamp(maxMessages, 1, 500))
                .ToArray();
            var escalatedMessageRefs = new List<string>();
            var recipientRef = escalationRecipientRef.Trim();

            foreach (var message in eligibleMessages)
            {
                var index = _retentionSchedulerOutboxMessages.FindIndex(item =>
                    string.Equals(item.OutboxMessageId, message.OutboxMessageId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));
                if (index < 0)
                {
                    continue;
                }

                var updated = message with
                {
                    Status = "escalated",
                    EscalationLevel = message.EscalationLevel + 1,
                    EscalatedToRecipientRef = recipientRef,
                    EscalatedAt = escalatedAt,
                    RecipientRef = recipientRef,
                    ErrorMessage = null
                };
                _retentionSchedulerOutboxMessages[index] = updated;
                PersistRetentionSchedulerOutboxMessage(tenantId, updated);
                escalatedMessageRefs.Add(updated.OutboxMessageId);
                AddAccessLog(
                    updated.TargetRecordId,
                    "retention.scheduler.outbox_escalated",
                    "allowed",
                    requestedByPersonId,
                    null,
                    null,
                    null,
                    null,
                    recipientRef);
            }

            return new RecordArrRetentionSchedulerOutboxEscalationRunResponse(
                tenantId,
                escalatedAt,
                requestedByPersonId,
                recipientRef,
                eligibleMessages.Length,
                escalatedMessageRefs.Count,
                escalatedMessageRefs.ToArray());
        }
    }

    private bool HasOpenDisposalReview(string tenantId, RecordArrRetentionStatusResponse status)
    {
        if (!string.IsNullOrWhiteSpace(status.DisposalReviewRef))
        {
            var referencedReview = _disposalReviews.FirstOrDefault(review =>
                string.Equals(review.DisposalReviewId, status.DisposalReviewRef, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(review.RecordId, tenantId));
            if (referencedReview is null ||
                !string.Equals(referencedReview.Status, "rejected", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(referencedReview.Status, "canceled", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return _disposalReviews.Any(review =>
            string.Equals(review.RetentionStatusRef, status.RetentionStatusId, StringComparison.OrdinalIgnoreCase) &&
            RecordBelongsToTenant(review.RecordId, tenantId) &&
            !string.Equals(review.Status, "rejected", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(review.Status, "canceled", StringComparison.OrdinalIgnoreCase));
    }

    public RecordArrLegalHoldResponse CreateLegalHold(string tenantId, string title, string description, string holdType, string sourceProduct, string sourceObjectType, string sourceObjectId, string createdByPersonId, IEnumerable<string> scopeRules, IEnumerable<string> recordRefs)
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
            var normalizedRecordRefs = ResolveLegalHoldRecordRefs(tenantId, normalizedScopeRules, recordRefs).ToArray();
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
            _legalHoldTenantIds[hold.LegalHoldId] = tenantId;
            PersistLegalHold(tenantId, hold);
            return hold;
        }
    }

    public RecordArrLegalHoldResponse ActivateLegalHold(string tenantId, string holdId)
    {
        lock (_gate)
        {
            var index = _legalHolds.FindIndex(hold =>
                string.Equals(hold.LegalHoldId, holdId, StringComparison.OrdinalIgnoreCase) &&
                LegalHoldBelongsToTenant(hold, tenantId));
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
            _legalHoldTenantIds[updated.LegalHoldId] = tenantId;
            PersistLegalHold(tenantId, updated);
            RefreshRetentionStatusesForActiveLegalHolds(tenantId);
            return updated;
        }
    }

    public RecordArrLegalHoldResponse ReleaseLegalHold(string tenantId, string holdId, string releasedByPersonId, string releaseReason)
    {
        lock (_gate)
        {
            var index = _legalHolds.FindIndex(hold =>
                string.Equals(hold.LegalHoldId, holdId, StringComparison.OrdinalIgnoreCase) &&
                LegalHoldBelongsToTenant(hold, tenantId));
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
            _legalHoldTenantIds[updated.LegalHoldId] = tenantId;
            PersistLegalHold(tenantId, updated);
            RefreshRetentionStatusesForActiveLegalHolds(tenantId);
            return updated;
        }
    }

    private void CreateMissingRetentionStatuses(string tenantId)
    {
        foreach (var record in _records.Where(record => string.Equals(ResolveRecordTenantId(record.RecordId), tenantId, StringComparison.OrdinalIgnoreCase)))
        {
            if (_retentionStatuses.Any(status => string.Equals(status.RecordId, record.RecordId, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var policy = SelectRetentionPolicyForRecord(record);
            if (policy is null)
            {
                continue;
            }

            var startAt = record.EffectiveAt ?? record.UploadedAt;
            var expiresAt = CalculateRetentionExpiresAt(startAt, policy);
            var status = new RecordArrRetentionStatusResponse(
                $"rstat-{Guid.NewGuid():N}"[..13],
                record.RecordId,
                policy.RetentionPolicyId,
                "active",
                startAt,
                expiresAt,
                expiresAt?.AddDays(-30),
                null,
                null,
                null);
            _retentionStatuses.Add(status);
            PersistRetentionStatus(tenantId, status);
        }
    }

    private RecordArrRetentionPolicyResponse? SelectRetentionPolicyForRecord(RecordArrRecordResponse record)
    {
        return _retentionPolicies.FirstOrDefault(policy =>
            MatchesRetentionApplicability(policy.RecordTypeApplicability, record.RecordType) &&
            MatchesRetentionApplicability(policy.DocumentTypeApplicability, record.DocumentType) &&
            MatchesRetentionApplicability(policy.SourceProductApplicability, record.SourceProduct)) ??
            _retentionPolicies.FirstOrDefault();
    }

    private static bool MatchesRetentionApplicability(string applicability, string value)
        => string.IsNullOrWhiteSpace(applicability) ||
           string.Equals(applicability, "*", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(applicability, "all", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(applicability, value, StringComparison.OrdinalIgnoreCase);

    private static DateTimeOffset? CalculateRetentionExpiresAt(DateTimeOffset startAt, RecordArrRetentionPolicyResponse policy)
    {
        if (policy.RetentionUnit.Equals("indefinite", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return policy.RetentionUnit.Trim().ToLowerInvariant() switch
        {
            "day" or "days" => startAt.AddDays(policy.RetainFor),
            "month" or "months" => startAt.AddMonths(policy.RetainFor),
            "year" or "years" => startAt.AddYears(policy.RetainFor),
            _ => startAt.AddYears(policy.RetainFor)
        };
    }

    private void RefreshRetentionStatusesForActiveLegalHolds(string tenantId)
    {
        var activeHoldRecordRefs = _legalHolds
            .Where(hold => string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase) && LegalHoldBelongsToTenant(hold, tenantId))
            .SelectMany(hold => hold.RecordRefs)
            .Select(recordRef => recordRef.Trim())
            .Where(recordRef => !string.IsNullOrWhiteSpace(recordRef))
            .Concat(_legalHolds
                .Where(hold => string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase) && LegalHoldBelongsToTenant(hold, tenantId))
                .SelectMany(hold => hold.ScopeRules.Select(scopeRule => ParseLegalHoldScopeRule(scopeRule)))
                .SelectMany(scopeRule => _records.Where(record =>
                    string.Equals(ResolveRecordTenantId(record.RecordId), tenantId, StringComparison.OrdinalIgnoreCase) &&
                    IsRecordMatchedByLegalHoldScopeRule(record, scopeRule)))
                .Select(record => record.RecordId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < _retentionStatuses.Count; i++)
        {
            var current = _retentionStatuses[i];
            if (!RecordBelongsToTenant(current.RecordId, tenantId))
            {
                continue;
            }

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
            PersistRetentionStatus(tenantId, _retentionStatuses[i]);
            if (string.Equals(nextStatus, "blocked_by_legal_hold", StringComparison.OrdinalIgnoreCase))
            {
                AddAccessLog(current.RecordId, "retention_status.blocked_by_legal_hold", "denied", "system", null, null, null, null, "blocked_by_legal_hold");
            }
            else if (string.Equals(current.Status, "blocked_by_legal_hold", StringComparison.OrdinalIgnoreCase))
            {
                AddAccessLog(current.RecordId, "retention_status.restored_after_legal_hold", "allowed", "system", null, null, null, null, nextStatus);
            }
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

    private IReadOnlyList<string> ResolveLegalHoldRecordRefs(string tenantId, IEnumerable<string> scopeRules, IEnumerable<string> recordRefs)
    {
        var refs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var recordRef in recordRefs)
        {
            if (!string.IsNullOrWhiteSpace(recordRef))
            {
                var normalized = recordRef.Trim();
                if (!RecordBelongsToTenant(normalized, tenantId))
                {
                    throw new InvalidOperationException($"Record {normalized} not found.");
                }

                refs.Add(normalized);
            }
        }

        foreach (var scopeRule in scopeRules.Select(ParseLegalHoldScopeRule))
        {
            foreach (var record in _records.Where(record =>
                string.Equals(ResolveRecordTenantId(record.RecordId), tenantId, StringComparison.OrdinalIgnoreCase) &&
                IsRecordMatchedByLegalHoldScopeRule(record, scopeRule)))
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

    private void PersistRecord(RecordArrRecordResponse record)
    {
        if (!Guid.TryParse(ResolveRecordTenantId(record.RecordId), out var tenantId))
        {
            throw new InvalidOperationException($"Record {record.RecordId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(record, JsonOptions);
        var entity = db.RecordArrRecords.FirstOrDefault(row =>
            row.TenantId == tenantId && row.RecordId == record.RecordId);
        if (entity is null)
        {
            db.RecordArrRecords.Add(new RecordArrRecordEntity
            {
                TenantId = tenantId,
                RecordId = record.RecordId,
                RecordNumber = record.RecordNumber,
                Title = record.Title,
                Status = record.Status,
                Classification = record.Classification,
                SourceProduct = record.SourceProduct,
                SourceObjectType = record.SourceObjectType,
                SourceObjectId = record.SourceObjectId,
                SourceObjectDisplayName = record.SourceObjectDisplayName,
                OwnerPersonId = record.OwnerPersonId,
                UploadedByPersonId = record.UploadedByPersonId ?? string.Empty,
                UploadedAt = record.UploadedAt,
                UpdatedAt = DateTimeOffset.UtcNow,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordNumber = record.RecordNumber;
            entity.Title = record.Title;
            entity.Status = record.Status;
            entity.Classification = record.Classification;
            entity.SourceProduct = record.SourceProduct;
            entity.SourceObjectType = record.SourceObjectType;
            entity.SourceObjectId = record.SourceObjectId;
            entity.SourceObjectDisplayName = record.SourceObjectDisplayName;
            entity.OwnerPersonId = record.OwnerPersonId;
            entity.UploadedByPersonId = record.UploadedByPersonId ?? string.Empty;
            entity.UploadedAt = record.UploadedAt;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistFile(RecordArrFileResponse file)
    {
        if (!Guid.TryParse(file.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"File {file.FileId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(file, JsonOptions);
        var entity = db.RecordArrFiles.FirstOrDefault(row =>
            row.TenantId == tenantId && row.FileId == file.FileId);
        if (entity is null)
        {
            db.RecordArrFiles.Add(new RecordArrFileEntity
            {
                TenantId = tenantId,
                FileId = file.FileId,
                RecordId = file.RecordId,
                FileNumber = file.FileNumber,
                StorageProvider = file.StorageProvider,
                StorageKey = file.StorageKey,
                OriginalFilename = file.OriginalFilename,
                MimeType = file.MimeType,
                SizeBytes = file.SizeBytes,
                ChecksumSha256 = file.ChecksumSha256,
                MalwareScanStatus = file.VirusScanStatus,
                ProcessingStatus = file.ProcessingStatus,
                UploadedAt = file.UploadedAt,
                DeletedAt = file.DeletedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = file.RecordId;
            entity.FileNumber = file.FileNumber;
            entity.StorageProvider = file.StorageProvider;
            entity.StorageKey = file.StorageKey;
            entity.OriginalFilename = file.OriginalFilename;
            entity.MimeType = file.MimeType;
            entity.SizeBytes = file.SizeBytes;
            entity.ChecksumSha256 = file.ChecksumSha256;
            entity.MalwareScanStatus = file.VirusScanStatus;
            entity.ProcessingStatus = file.ProcessingStatus;
            entity.UploadedAt = file.UploadedAt;
            entity.DeletedAt = file.DeletedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
        EnsureObjectStoreFileIndexed(file);
    }

    private void PersistFileIntegrityCheck(RecordArrFileIntegrityCheckResponse check)
    {
        if (!Guid.TryParse(check.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"File integrity check {check.IntegrityCheckId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(check, JsonOptions);
        var entity = db.RecordArrFileIntegrityChecks.FirstOrDefault(row =>
            row.TenantId == tenantId && row.IntegrityCheckId == check.IntegrityCheckId);
        if (entity is null)
        {
            db.RecordArrFileIntegrityChecks.Add(new RecordArrFileIntegrityCheckEntity
            {
                TenantId = tenantId,
                IntegrityCheckId = check.IntegrityCheckId,
                FileId = check.FileId,
                RecordId = check.RecordId,
                StorageProvider = check.StorageProvider,
                StorageKey = check.StorageKey,
                ExpectedChecksumSha256 = check.ExpectedChecksumSha256,
                ObservedChecksumSha256 = check.ObservedChecksumSha256,
                Status = check.Status,
                CheckMethod = check.CheckMethod,
                CheckedAt = check.CheckedAt,
                CheckedByPersonId = check.CheckedByPersonId,
                FailureReason = check.FailureReason,
                PayloadJson = payload
            });
        }
        else
        {
            entity.FileId = check.FileId;
            entity.RecordId = check.RecordId;
            entity.StorageProvider = check.StorageProvider;
            entity.StorageKey = check.StorageKey;
            entity.ExpectedChecksumSha256 = check.ExpectedChecksumSha256;
            entity.ObservedChecksumSha256 = check.ObservedChecksumSha256;
            entity.Status = check.Status;
            entity.CheckMethod = check.CheckMethod;
            entity.CheckedAt = check.CheckedAt;
            entity.CheckedByPersonId = check.CheckedByPersonId;
            entity.FailureReason = check.FailureReason;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistFileMalwareScan(RecordArrFileMalwareScanResponse scan)
    {
        if (!Guid.TryParse(scan.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"File malware scan {scan.MalwareScanId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(scan, JsonOptions);
        var entity = db.RecordArrFileMalwareScans.FirstOrDefault(row =>
            row.TenantId == tenantId && row.MalwareScanId == scan.MalwareScanId);
        if (entity is null)
        {
            db.RecordArrFileMalwareScans.Add(new RecordArrFileMalwareScanEntity
            {
                TenantId = tenantId,
                MalwareScanId = scan.MalwareScanId,
                FileId = scan.FileId,
                RecordId = scan.RecordId,
                StorageProvider = scan.StorageProvider,
                StorageKey = scan.StorageKey,
                Status = scan.Status,
                ScannerName = scan.ScannerName,
                ScannerVersion = scan.ScannerVersion,
                SignatureVersion = scan.SignatureVersion,
                ThreatName = scan.ThreatName,
                QuarantineStatus = scan.QuarantineStatus,
                ScannedAt = scan.ScannedAt,
                ScannedByPersonId = scan.ScannedByPersonId,
                FailureReason = scan.FailureReason,
                PayloadJson = payload
            });
        }
        else
        {
            entity.FileId = scan.FileId;
            entity.RecordId = scan.RecordId;
            entity.StorageProvider = scan.StorageProvider;
            entity.StorageKey = scan.StorageKey;
            entity.Status = scan.Status;
            entity.ScannerName = scan.ScannerName;
            entity.ScannerVersion = scan.ScannerVersion;
            entity.SignatureVersion = scan.SignatureVersion;
            entity.ThreatName = scan.ThreatName;
            entity.QuarantineStatus = scan.QuarantineStatus;
            entity.ScannedAt = scan.ScannedAt;
            entity.ScannedByPersonId = scan.ScannedByPersonId;
            entity.FailureReason = scan.FailureReason;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistStorageReconciliation(RecordArrStorageReconciliationResponse reconciliation)
    {
        if (!Guid.TryParse(reconciliation.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Storage reconciliation {reconciliation.ReconciliationId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(reconciliation, JsonOptions);
        var entity = db.RecordArrStorageReconciliations.FirstOrDefault(row =>
            row.TenantId == tenantId && row.ReconciliationId == reconciliation.ReconciliationId);
        if (entity is null)
        {
            db.RecordArrStorageReconciliations.Add(new RecordArrStorageReconciliationEntity
            {
                TenantId = tenantId,
                ReconciliationId = reconciliation.ReconciliationId,
                Scope = reconciliation.Scope,
                Status = reconciliation.Status,
                StartedAt = reconciliation.StartedAt,
                CompletedAt = reconciliation.CompletedAt,
                RequestedByPersonId = reconciliation.RequestedByPersonId,
                TotalFiles = reconciliation.TotalFiles,
                CheckedFiles = reconciliation.CheckedFiles,
                PassedFiles = reconciliation.PassedFiles,
                MissingFiles = reconciliation.MissingFiles,
                CorruptFiles = reconciliation.CorruptFiles,
                QuarantinedFiles = reconciliation.QuarantinedFiles,
                PendingScanFiles = reconciliation.PendingScanFiles,
                DeletedFiles = reconciliation.DeletedFiles,
                IssueSummary = reconciliation.IssueSummary,
                RemediationStatus = reconciliation.RemediationStatus,
                PayloadJson = payload
            });
        }
        else
        {
            entity.Scope = reconciliation.Scope;
            entity.Status = reconciliation.Status;
            entity.StartedAt = reconciliation.StartedAt;
            entity.CompletedAt = reconciliation.CompletedAt;
            entity.RequestedByPersonId = reconciliation.RequestedByPersonId;
            entity.TotalFiles = reconciliation.TotalFiles;
            entity.CheckedFiles = reconciliation.CheckedFiles;
            entity.PassedFiles = reconciliation.PassedFiles;
            entity.MissingFiles = reconciliation.MissingFiles;
            entity.CorruptFiles = reconciliation.CorruptFiles;
            entity.QuarantinedFiles = reconciliation.QuarantinedFiles;
            entity.PendingScanFiles = reconciliation.PendingScanFiles;
            entity.DeletedFiles = reconciliation.DeletedFiles;
            entity.IssueSummary = reconciliation.IssueSummary;
            entity.RemediationStatus = reconciliation.RemediationStatus;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void EnsureObjectStoreFileIndexed(RecordArrFileResponse file)
    {
        var existingObject = _objectStoreObjects.FirstOrDefault(item =>
            string.Equals(item.TenantId, file.TenantId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(item.FileId, file.FileId, StringComparison.OrdinalIgnoreCase));
        var hasObservation = _objectStoreFixityObservations.Any(item =>
            string.Equals(item.TenantId, file.TenantId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(item.FileId, file.FileId, StringComparison.OrdinalIgnoreCase));

        if (existingObject is not null && hasObservation)
        {
            return;
        }

        RecordObjectStoreFixityObservation(
            file,
            "indexed",
            file.UploadedAt,
            file.UploadedByPersonId,
            existingObject is null ? "file_created" : "migration_backfill",
            file.ChecksumSha256,
            null,
            null,
            null);
    }

    private void RecordObjectStoreFixityObservation(
        RecordArrFileResponse file,
        string status,
        DateTimeOffset observedAt,
        string observedByPersonId,
        string observationSource,
        string? observedChecksumSha256,
        string? integrityCheckRef,
        string? reconciliationRef,
        string? failureReason,
        string? lifecycleStatus = null,
        string? lifecycleProviderName = null,
        string? lifecyclePolicyRef = null,
        string? lifecycleRetentionMode = null,
        DateTimeOffset? lifecycleRetainUntil = null,
        string? lifecycleEncryptionKeyRef = null,
        string? lifecycleEvidenceRef = null,
        string? lifecycleEvidenceHash = null)
    {
        var normalizedStatus = NormalizeRecordArrEnum(
            status,
            "objectStoreStatus",
            "indexed",
            "passed",
            "failed",
            "missing",
            "unavailable",
            "accepted_missing");
        var normalizedSource = NormalizeRecordArrEnum(
            observationSource,
            "observationSource",
            "file_created",
            "integrity_check",
            "storage_reconciliation",
            "storage_remediation",
            "disaster_recovery_restore",
            "disaster_recovery_backup",
            "object_lifecycle_verification",
            "migration_backfill");
        var normalizedExpectedChecksum = NormalizeChecksum(file.ChecksumSha256) ?? file.ChecksumSha256;
        var normalizedObservedChecksum = string.IsNullOrWhiteSpace(observedChecksumSha256)
            ? null
            : NormalizeChecksum(observedChecksumSha256);
        var normalizedFailureReason = string.IsNullOrWhiteSpace(failureReason) ? null : failureReason.Trim();

        var observation = new RecordArrObjectStoreFixityObservationResponse(
            $"fixobs-{Guid.NewGuid():N}"[..16],
            file.TenantId,
            file.FileId,
            file.RecordId,
            file.StorageProvider,
            file.StorageKey,
            file.SizeBytes,
            normalizedExpectedChecksum,
            normalizedObservedChecksum,
            normalizedStatus,
            normalizedSource,
            string.IsNullOrWhiteSpace(integrityCheckRef) ? null : integrityCheckRef.Trim(),
            string.IsNullOrWhiteSpace(reconciliationRef) ? null : reconciliationRef.Trim(),
            observedAt,
            observedByPersonId,
            normalizedFailureReason,
            lifecycleStatus,
            lifecycleProviderName,
            lifecyclePolicyRef,
            lifecycleRetentionMode,
            lifecycleRetainUntil,
            lifecycleEncryptionKeyRef,
            lifecycleEvidenceRef,
            lifecycleEvidenceHash);

        _objectStoreFixityObservations.Add(observation);
        PersistObjectStoreFixityObservation(observation);

        var objectIndex = _objectStoreObjects.FindIndex(item =>
            string.Equals(item.TenantId, file.TenantId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(item.FileId, file.FileId, StringComparison.OrdinalIgnoreCase));
        var objectStoreObject = new RecordArrObjectStoreObjectResponse(
            objectIndex >= 0 ? _objectStoreObjects[objectIndex].ObjectStoreObjectId : $"obj-{Guid.NewGuid():N}"[..12],
            file.TenantId,
            file.FileId,
            file.RecordId,
            file.StorageProvider,
            file.StorageKey,
            file.SizeBytes,
            normalizedExpectedChecksum,
            normalizedObservedChecksum,
            normalizedStatus,
            normalizedSource,
            observation.IntegrityCheckRef,
            observation.ReconciliationRef,
            observedAt,
            observedByPersonId,
            normalizedFailureReason,
            lifecycleStatus ?? (objectIndex >= 0 ? _objectStoreObjects[objectIndex].LifecycleStatus : null),
            lifecycleProviderName ?? (objectIndex >= 0 ? _objectStoreObjects[objectIndex].LifecycleProviderName : null),
            lifecyclePolicyRef ?? (objectIndex >= 0 ? _objectStoreObjects[objectIndex].LifecyclePolicyRef : null),
            lifecycleRetentionMode ?? (objectIndex >= 0 ? _objectStoreObjects[objectIndex].LifecycleRetentionMode : null),
            lifecycleRetainUntil ?? (objectIndex >= 0 ? _objectStoreObjects[objectIndex].LifecycleRetainUntil : null),
            lifecycleEncryptionKeyRef ?? (objectIndex >= 0 ? _objectStoreObjects[objectIndex].LifecycleEncryptionKeyRef : null),
            lifecycleEvidenceRef ?? (objectIndex >= 0 ? _objectStoreObjects[objectIndex].LifecycleEvidenceRef : null),
            lifecycleEvidenceHash ?? (objectIndex >= 0 ? _objectStoreObjects[objectIndex].LifecycleEvidenceHash : null),
            objectIndex >= 0 ? _objectStoreObjects[objectIndex].LifecycleFailureReason : null);

        if (objectIndex >= 0)
        {
            _objectStoreObjects[objectIndex] = objectStoreObject;
        }
        else
        {
            _objectStoreObjects.Add(objectStoreObject);
        }

        PersistObjectStoreObject(objectStoreObject);
    }

    private void PersistObjectStoreObject(RecordArrObjectStoreObjectResponse item)
    {
        if (!Guid.TryParse(item.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Object-store object {item.ObjectStoreObjectId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(item, JsonOptions);
        var entity = db.RecordArrObjectStoreObjects.FirstOrDefault(row =>
            row.TenantId == tenantId && row.ObjectStoreObjectId == item.ObjectStoreObjectId);
        if (entity is null)
        {
            db.RecordArrObjectStoreObjects.Add(new RecordArrObjectStoreObjectEntity
            {
                TenantId = tenantId,
                ObjectStoreObjectId = item.ObjectStoreObjectId,
                FileId = item.FileId,
                RecordId = item.RecordId,
                StorageProvider = item.StorageProvider,
                StorageKey = item.StorageKey,
                SizeBytes = item.SizeBytes,
                ExpectedChecksumSha256 = item.ExpectedChecksumSha256,
                LastObservedChecksumSha256 = item.LastObservedChecksumSha256,
                Status = item.Status,
                LastObservationSource = item.LastObservationSource,
                LastIntegrityCheckRef = item.LastIntegrityCheckRef,
                LastReconciliationRef = item.LastReconciliationRef,
                LastObservedAt = item.LastObservedAt,
                LastObservedByPersonId = item.LastObservedByPersonId,
                FailureReason = item.FailureReason,
                PayloadJson = payload
            });
        }
        else
        {
            entity.FileId = item.FileId;
            entity.RecordId = item.RecordId;
            entity.StorageProvider = item.StorageProvider;
            entity.StorageKey = item.StorageKey;
            entity.SizeBytes = item.SizeBytes;
            entity.ExpectedChecksumSha256 = item.ExpectedChecksumSha256;
            entity.LastObservedChecksumSha256 = item.LastObservedChecksumSha256;
            entity.Status = item.Status;
            entity.LastObservationSource = item.LastObservationSource;
            entity.LastIntegrityCheckRef = item.LastIntegrityCheckRef;
            entity.LastReconciliationRef = item.LastReconciliationRef;
            entity.LastObservedAt = item.LastObservedAt;
            entity.LastObservedByPersonId = item.LastObservedByPersonId;
            entity.FailureReason = item.FailureReason;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistObjectStoreFixityObservation(RecordArrObjectStoreFixityObservationResponse observation)
    {
        if (!Guid.TryParse(observation.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Object-store fixity observation {observation.FixityObservationId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(observation, JsonOptions);
        var entity = db.RecordArrObjectStoreFixityObservations.FirstOrDefault(row =>
            row.TenantId == tenantId && row.FixityObservationId == observation.FixityObservationId);
        if (entity is null)
        {
            db.RecordArrObjectStoreFixityObservations.Add(new RecordArrObjectStoreFixityObservationEntity
            {
                TenantId = tenantId,
                FixityObservationId = observation.FixityObservationId,
                FileId = observation.FileId,
                RecordId = observation.RecordId,
                StorageProvider = observation.StorageProvider,
                StorageKey = observation.StorageKey,
                SizeBytes = observation.SizeBytes,
                ExpectedChecksumSha256 = observation.ExpectedChecksumSha256,
                ObservedChecksumSha256 = observation.ObservedChecksumSha256,
                Status = observation.Status,
                ObservationSource = observation.ObservationSource,
                IntegrityCheckRef = observation.IntegrityCheckRef,
                ReconciliationRef = observation.ReconciliationRef,
                ObservedAt = observation.ObservedAt,
                ObservedByPersonId = observation.ObservedByPersonId,
                FailureReason = observation.FailureReason,
                PayloadJson = payload
            });
        }
        else
        {
            entity.FileId = observation.FileId;
            entity.RecordId = observation.RecordId;
            entity.StorageProvider = observation.StorageProvider;
            entity.StorageKey = observation.StorageKey;
            entity.SizeBytes = observation.SizeBytes;
            entity.ExpectedChecksumSha256 = observation.ExpectedChecksumSha256;
            entity.ObservedChecksumSha256 = observation.ObservedChecksumSha256;
            entity.Status = observation.Status;
            entity.ObservationSource = observation.ObservationSource;
            entity.IntegrityCheckRef = observation.IntegrityCheckRef;
            entity.ReconciliationRef = observation.ReconciliationRef;
            entity.ObservedAt = observation.ObservedAt;
            entity.ObservedByPersonId = observation.ObservedByPersonId;
            entity.FailureReason = observation.FailureReason;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistRecordMetadata(string tenantIdValue, RecordArrRecordMetadataResponse metadata)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Record metadata {metadata.MetadataId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(metadata, JsonOptions);
        var entity = db.RecordArrRecordMetadata.FirstOrDefault(row =>
            row.TenantId == tenantId && row.MetadataId == metadata.MetadataId);
        if (entity is null)
        {
            db.RecordArrRecordMetadata.Add(new RecordArrRecordMetadataEntity
            {
                TenantId = tenantId,
                MetadataId = metadata.MetadataId,
                RecordId = metadata.RecordId,
                Key = metadata.Key,
                ValueType = metadata.ValueType,
                Source = metadata.Source,
                ConfidenceScore = metadata.ConfidenceScore,
                Verified = metadata.Verified,
                CreatedAt = DateTimeOffset.UtcNow,
                VerifiedAt = metadata.VerifiedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = metadata.RecordId;
            entity.Key = metadata.Key;
            entity.ValueType = metadata.ValueType;
            entity.Source = metadata.Source;
            entity.ConfidenceScore = metadata.ConfidenceScore;
            entity.Verified = metadata.Verified;
            entity.VerifiedAt = metadata.VerifiedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistRecordLink(string tenantIdValue, RecordArrRecordLinkResponse link)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Record link {link.RecordLinkId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(link, JsonOptions);
        var entity = db.RecordArrRecordLinks.FirstOrDefault(row =>
            row.TenantId == tenantId && row.RecordLinkId == link.RecordLinkId);
        if (entity is null)
        {
            db.RecordArrRecordLinks.Add(new RecordArrRecordLinkEntity
            {
                TenantId = tenantId,
                RecordLinkId = link.RecordLinkId,
                RecordId = link.RecordId,
                LinkedRecordId = link.LinkedRecordId,
                SourceObjectRef = link.SourceObjectRef,
                LinkType = link.LinkType,
                CreatedAt = link.CreatedAt,
                CreatedByPersonId = link.CreatedByPersonId,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = link.RecordId;
            entity.LinkedRecordId = link.LinkedRecordId;
            entity.SourceObjectRef = link.SourceObjectRef;
            entity.LinkType = link.LinkType;
            entity.CreatedAt = link.CreatedAt;
            entity.CreatedByPersonId = link.CreatedByPersonId;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistRecordComment(string tenantIdValue, RecordArrRecordCommentResponse comment)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Record comment {comment.CommentId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(comment, JsonOptions);
        var entity = db.RecordArrRecordComments.FirstOrDefault(row =>
            row.TenantId == tenantId && row.CommentId == comment.CommentId);
        if (entity is null)
        {
            db.RecordArrRecordComments.Add(new RecordArrRecordCommentEntity
            {
                TenantId = tenantId,
                CommentId = comment.CommentId,
                RecordId = comment.RecordId,
                Visibility = comment.Visibility,
                CreatedAt = comment.CreatedAt,
                CreatedByPersonId = comment.CreatedByPersonId,
                EditedAt = comment.EditedAt,
                EditedByPersonId = comment.EditedByPersonId,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = comment.RecordId;
            entity.Visibility = comment.Visibility;
            entity.CreatedAt = comment.CreatedAt;
            entity.CreatedByPersonId = comment.CreatedByPersonId;
            entity.EditedAt = comment.EditedAt;
            entity.EditedByPersonId = comment.EditedByPersonId;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistUploadSession(RecordArrUploadSessionResponse session)
    {
        if (!Guid.TryParse(session.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Upload session {session.UploadSessionId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(session, JsonOptions);
        var entity = db.RecordArrUploadSessions.FirstOrDefault(row =>
            row.TenantId == tenantId && row.UploadSessionId == session.UploadSessionId);
        if (entity is null)
        {
            db.RecordArrUploadSessions.Add(new RecordArrUploadSessionEntity
            {
                TenantId = tenantId,
                UploadSessionId = session.UploadSessionId,
                UploadSessionNumber = session.UploadSessionNumber,
                SessionType = session.SessionType,
                SourceProduct = session.SourceProduct,
                SourceObjectType = session.SourceObjectType,
                SourceObjectId = session.SourceObjectId,
                UploadPurpose = session.UploadPurpose,
                Status = session.Status,
                CreatedAt = session.CreatedAt,
                ExpiresAt = session.ExpiresAt,
                CompletedAt = session.CompletedAt,
                RevokedAt = session.RevokedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.UploadSessionNumber = session.UploadSessionNumber;
            entity.SessionType = session.SessionType;
            entity.SourceProduct = session.SourceProduct;
            entity.SourceObjectType = session.SourceObjectType;
            entity.SourceObjectId = session.SourceObjectId;
            entity.UploadPurpose = session.UploadPurpose;
            entity.Status = session.Status;
            entity.CreatedAt = session.CreatedAt;
            entity.ExpiresAt = session.ExpiresAt;
            entity.CompletedAt = session.CompletedAt;
            entity.RevokedAt = session.RevokedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistCaptureRequest(RecordArrCaptureRequestResponse request)
    {
        if (!Guid.TryParse(request.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Capture request {request.CaptureRequestId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(request, JsonOptions);
        var entity = db.RecordArrCaptureRequests.FirstOrDefault(row =>
            row.TenantId == tenantId && row.CaptureRequestId == request.CaptureRequestId);
        if (entity is null)
        {
            db.RecordArrCaptureRequests.Add(new RecordArrCaptureRequestEntity
            {
                TenantId = tenantId,
                CaptureRequestId = request.CaptureRequestId,
                SourceProduct = request.SourceProduct,
                SourceObjectRef = request.SourceObjectRef,
                CaptureType = request.CaptureType,
                Title = request.Title,
                Required = request.Required,
                Status = request.Status,
                UploadSessionRef = request.UploadSessionRef,
                EvidenceRequirementRef = request.EvidenceRequirementRef,
                CreatedAt = request.CreatedAt,
                CompletedAt = request.CompletedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.SourceProduct = request.SourceProduct;
            entity.SourceObjectRef = request.SourceObjectRef;
            entity.CaptureType = request.CaptureType;
            entity.Title = request.Title;
            entity.Required = request.Required;
            entity.Status = request.Status;
            entity.UploadSessionRef = request.UploadSessionRef;
            entity.EvidenceRequirementRef = request.EvidenceRequirementRef;
            entity.CreatedAt = request.CreatedAt;
            entity.CompletedAt = request.CompletedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistScanProcessing(string tenantIdValue, RecordArrScanProcessingResponse scan)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Scan {scan.ScanProcessingId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(scan, JsonOptions);
        var entity = db.RecordArrScanProcessing.FirstOrDefault(row =>
            row.TenantId == tenantId && row.ScanProcessingId == scan.ScanProcessingId);
        if (entity is null)
        {
            db.RecordArrScanProcessing.Add(new RecordArrScanProcessingEntity
            {
                TenantId = tenantId,
                ScanProcessingId = scan.ScanProcessingId,
                RecordId = scan.RecordId,
                OriginalFileName = scan.OriginalFileName,
                Status = scan.Status,
                ScanPurpose = scan.ScanPurpose,
                OriginalFileRef = scan.OriginalFileRef,
                GeneratedPdfFileRef = scan.GeneratedPdfFileRef,
                OcrResultId = scan.OcrResultId,
                ExtractionResultId = scan.ExtractionResultId,
                ConfidenceScore = scan.ConfidenceScore,
                ProcessedAt = scan.ProcessedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = scan.RecordId;
            entity.OriginalFileName = scan.OriginalFileName;
            entity.Status = scan.Status;
            entity.ScanPurpose = scan.ScanPurpose;
            entity.OriginalFileRef = scan.OriginalFileRef;
            entity.GeneratedPdfFileRef = scan.GeneratedPdfFileRef;
            entity.OcrResultId = scan.OcrResultId;
            entity.ExtractionResultId = scan.ExtractionResultId;
            entity.ConfidenceScore = scan.ConfidenceScore;
            entity.ProcessedAt = scan.ProcessedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistOcrResult(string tenantIdValue, RecordArrOcrResultResponse result)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"OCR result {result.OcrResultId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(result, JsonOptions);
        var entity = db.RecordArrOcrResults.FirstOrDefault(row =>
            row.TenantId == tenantId && row.OcrResultId == result.OcrResultId);
        if (entity is null)
        {
            db.RecordArrOcrResults.Add(new RecordArrOcrResultEntity
            {
                TenantId = tenantId,
                OcrResultId = result.OcrResultId,
                RecordId = result.RecordId,
                FileId = result.FileId,
                Engine = result.Engine,
                Status = result.Status,
                Language = result.Language,
                ConfidenceScore = result.ConfidenceScore,
                ExtractedAt = result.ExtractedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = result.RecordId;
            entity.FileId = result.FileId;
            entity.Engine = result.Engine;
            entity.Status = result.Status;
            entity.Language = result.Language;
            entity.ConfidenceScore = result.ConfidenceScore;
            entity.ExtractedAt = result.ExtractedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistExtractionResult(string tenantIdValue, RecordArrExtractionResultResponse result)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Extraction result {result.ExtractionResultId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(result, JsonOptions);
        var entity = db.RecordArrExtractionResults.FirstOrDefault(row =>
            row.TenantId == tenantId && row.ExtractionResultId == result.ExtractionResultId);
        if (entity is null)
        {
            db.RecordArrExtractionResults.Add(new RecordArrExtractionResultEntity
            {
                TenantId = tenantId,
                ExtractionResultId = result.ExtractionResultId,
                RecordId = result.RecordId,
                ExtractionType = result.ExtractionType,
                Status = result.Status,
                ConfidenceScore = result.ConfidenceScore,
                ExtractedAt = result.ExtractedAt,
                ReviewedByPersonId = result.ReviewedByPersonId,
                ReviewedAt = result.ReviewedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = result.RecordId;
            entity.ExtractionType = result.ExtractionType;
            entity.Status = result.Status;
            entity.ConfidenceScore = result.ConfidenceScore;
            entity.ExtractedAt = result.ExtractedAt;
            entity.ReviewedByPersonId = result.ReviewedByPersonId;
            entity.ReviewedAt = result.ReviewedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistEvidenceMapping(string tenantIdValue, RecordArrEvidenceMappingResponse mapping)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Evidence mapping {mapping.EvidenceMappingId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(mapping, JsonOptions);
        var entity = db.RecordArrEvidenceMappings.FirstOrDefault(row =>
            row.TenantId == tenantId && row.EvidenceMappingId == mapping.EvidenceMappingId);
        if (entity is null)
        {
            db.RecordArrEvidenceMappings.Add(new RecordArrEvidenceMappingEntity
            {
                TenantId = tenantId,
                EvidenceMappingId = mapping.EvidenceMappingId,
                RecordId = mapping.RecordId,
                SourceProduct = mapping.SourceProduct,
                SourceObjectType = mapping.SourceObjectType,
                SourceObjectId = mapping.SourceObjectId,
                ComplianceRequirementRef = mapping.ComplianceRequirementRef,
                EvidenceTypeKey = mapping.EvidenceTypeKey,
                Status = mapping.Status,
                MappingSource = mapping.MappingSource,
                ConfidenceScore = mapping.ConfidenceScore,
                ConfirmedByPersonId = mapping.ConfirmedByPersonId,
                ConfirmedAt = mapping.ConfirmedAt,
                RejectedByPersonId = mapping.RejectedByPersonId,
                RejectedAt = mapping.RejectedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = mapping.RecordId;
            entity.SourceProduct = mapping.SourceProduct;
            entity.SourceObjectType = mapping.SourceObjectType;
            entity.SourceObjectId = mapping.SourceObjectId;
            entity.ComplianceRequirementRef = mapping.ComplianceRequirementRef;
            entity.EvidenceTypeKey = mapping.EvidenceTypeKey;
            entity.Status = mapping.Status;
            entity.MappingSource = mapping.MappingSource;
            entity.ConfidenceScore = mapping.ConfidenceScore;
            entity.ConfirmedByPersonId = mapping.ConfirmedByPersonId;
            entity.ConfirmedAt = mapping.ConfirmedAt;
            entity.RejectedByPersonId = mapping.RejectedByPersonId;
            entity.RejectedAt = mapping.RejectedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistPackage(string tenantIdValue, RecordArrPackageResponse package)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Package {package.PackageId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(package, JsonOptions);
        var entity = db.RecordArrPackages.FirstOrDefault(row =>
            row.TenantId == tenantId && row.PackageId == package.PackageId);
        if (entity is null)
        {
            db.RecordArrPackages.Add(new RecordArrPackageEntity
            {
                TenantId = tenantId,
                PackageId = package.PackageId,
                PackageNumber = package.PackageNumber,
                Title = package.Title,
                PackageType = package.PackageType,
                Status = package.Status,
                SourceProduct = package.SourceProduct,
                ManifestChecksum = package.ManifestChecksum,
                GeneratedPdfRecordRef = package.GeneratedPdfRecordRef,
                GeneratedZipFileRef = package.GeneratedZipFileRef,
                CreatedAt = package.CreatedAt,
                CompletedAt = package.CompletedAt,
                LockedAt = package.LockedAt,
                ArchivedAt = package.ArchivedAt,
                ExpiresAt = package.ExpiresAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.PackageNumber = package.PackageNumber;
            entity.Title = package.Title;
            entity.PackageType = package.PackageType;
            entity.Status = package.Status;
            entity.SourceProduct = package.SourceProduct;
            entity.ManifestChecksum = package.ManifestChecksum;
            entity.GeneratedPdfRecordRef = package.GeneratedPdfRecordRef;
            entity.GeneratedZipFileRef = package.GeneratedZipFileRef;
            entity.CreatedAt = package.CreatedAt;
            entity.CompletedAt = package.CompletedAt;
            entity.LockedAt = package.LockedAt;
            entity.ArchivedAt = package.ArchivedAt;
            entity.ExpiresAt = package.ExpiresAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistPackageManifest(string tenantIdValue, RecordArrPackageManifestResponse manifest)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Package manifest {manifest.ManifestId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(manifest, JsonOptions);
        var entity = db.RecordArrPackageManifests.FirstOrDefault(row =>
            row.TenantId == tenantId && row.ManifestId == manifest.ManifestId);
        if (entity is null)
        {
            db.RecordArrPackageManifests.Add(new RecordArrPackageManifestEntity
            {
                TenantId = tenantId,
                ManifestId = manifest.ManifestId,
                PackageId = manifest.PackageId,
                ManifestVersion = manifest.ManifestVersion,
                GeneratedAt = manifest.GeneratedAt,
                Checksum = manifest.Checksum,
                GeneratedByPersonId = manifest.GeneratedByPersonId,
                PayloadJson = payload
            });
        }
        else
        {
            entity.PackageId = manifest.PackageId;
            entity.ManifestVersion = manifest.ManifestVersion;
            entity.GeneratedAt = manifest.GeneratedAt;
            entity.Checksum = manifest.Checksum;
            entity.GeneratedByPersonId = manifest.GeneratedByPersonId;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistRetentionStatus(string tenantIdValue, RecordArrRetentionStatusResponse status)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Retention status {status.RetentionStatusId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(status, JsonOptions);
        var entity = db.RecordArrRetentionStatuses.FirstOrDefault(row =>
            row.TenantId == tenantId && row.RetentionStatusId == status.RetentionStatusId);
        if (entity is null)
        {
            db.RecordArrRetentionStatuses.Add(new RecordArrRetentionStatusEntity
            {
                TenantId = tenantId,
                RetentionStatusId = status.RetentionStatusId,
                RecordId = status.RecordId,
                RetentionPolicyRef = status.RetentionPolicyRef,
                Status = status.Status,
                RetentionStartAt = status.RetentionStartAt,
                RetentionExpiresAt = status.RetentionExpiresAt,
                NextReviewAt = status.NextReviewAt,
                LastReviewedAt = status.LastReviewedAt,
                ReviewedByPersonId = status.ReviewedByPersonId,
                DisposalReviewRef = status.DisposalReviewRef,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = status.RecordId;
            entity.RetentionPolicyRef = status.RetentionPolicyRef;
            entity.Status = status.Status;
            entity.RetentionStartAt = status.RetentionStartAt;
            entity.RetentionExpiresAt = status.RetentionExpiresAt;
            entity.NextReviewAt = status.NextReviewAt;
            entity.LastReviewedAt = status.LastReviewedAt;
            entity.ReviewedByPersonId = status.ReviewedByPersonId;
            entity.DisposalReviewRef = status.DisposalReviewRef;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistDisposalReview(string tenantIdValue, RecordArrDisposalReviewResponse review)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Disposal review {review.DisposalReviewId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(review, JsonOptions);
        var entity = db.RecordArrDisposalReviews.FirstOrDefault(row =>
            row.TenantId == tenantId && row.DisposalReviewId == review.DisposalReviewId);
        if (entity is null)
        {
            db.RecordArrDisposalReviews.Add(new RecordArrDisposalReviewEntity
            {
                TenantId = tenantId,
                DisposalReviewId = review.DisposalReviewId,
                RecordId = review.RecordId,
                RetentionStatusRef = review.RetentionStatusRef,
                ProposedAction = review.ProposedAction,
                Status = review.Status,
                RequestedAt = review.RequestedAt,
                RequestedByPersonId = review.RequestedByPersonId,
                ReviewedByPersonId = review.ReviewedByPersonId,
                ReviewedAt = review.ReviewedAt,
                CompletedAt = review.CompletedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = review.RecordId;
            entity.RetentionStatusRef = review.RetentionStatusRef;
            entity.ProposedAction = review.ProposedAction;
            entity.Status = review.Status;
            entity.RequestedAt = review.RequestedAt;
            entity.RequestedByPersonId = review.RequestedByPersonId;
            entity.ReviewedByPersonId = review.ReviewedByPersonId;
            entity.ReviewedAt = review.ReviewedAt;
            entity.CompletedAt = review.CompletedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistDestructionCertificate(string tenantIdValue, RecordArrDestructionCertificateResponse certificate)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Destruction certificate {certificate.DestructionCertificateId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(certificate, JsonOptions);
        var entity = db.RecordArrDestructionCertificates.FirstOrDefault(row =>
            row.TenantId == tenantId && row.DestructionCertificateId == certificate.DestructionCertificateId);
        if (entity is null)
        {
            db.RecordArrDestructionCertificates.Add(new RecordArrDestructionCertificateEntity
            {
                TenantId = tenantId,
                DestructionCertificateId = certificate.DestructionCertificateId,
                CertificateNumber = certificate.CertificateNumber,
                RecordId = certificate.RecordId,
                RetentionStatusRef = certificate.RetentionStatusRef,
                DisposalReviewRef = certificate.DisposalReviewRef,
                DispositionAction = certificate.DispositionAction,
                Status = certificate.Status,
                RequestedAt = certificate.RequestedAt,
                ExecutedAt = certificate.ExecutedAt,
                ExecutedByPersonId = certificate.ExecutedByPersonId,
                CertificateHash = certificate.CertificateHash,
                FailureReason = certificate.FailureReason,
                PayloadJson = payload
            });
        }
        else
        {
            entity.CertificateNumber = certificate.CertificateNumber;
            entity.RecordId = certificate.RecordId;
            entity.RetentionStatusRef = certificate.RetentionStatusRef;
            entity.DisposalReviewRef = certificate.DisposalReviewRef;
            entity.DispositionAction = certificate.DispositionAction;
            entity.Status = certificate.Status;
            entity.RequestedAt = certificate.RequestedAt;
            entity.ExecutedAt = certificate.ExecutedAt;
            entity.ExecutedByPersonId = certificate.ExecutedByPersonId;
            entity.CertificateHash = certificate.CertificateHash;
            entity.FailureReason = certificate.FailureReason;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistRetentionSchedulerRun(string tenantIdValue, RecordArrRetentionSchedulerRunResponse run)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Retention scheduler run {run.SchedulerRunId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(run, JsonOptions);
        var entity = db.RecordArrRetentionSchedulerRuns.FirstOrDefault(row =>
            row.TenantId == tenantId && row.SchedulerRunId == run.SchedulerRunId);
        if (entity is null)
        {
            db.RecordArrRetentionSchedulerRuns.Add(new RecordArrRetentionSchedulerRunEntity
            {
                TenantId = tenantId,
                SchedulerRunId = run.SchedulerRunId,
                LeaseId = run.LeaseId,
                RanAt = run.RanAt,
                RequestedByPersonId = run.RequestedByPersonId,
                Status = run.Status,
                ExecutionPolicy = run.ExecutionPolicy,
                EvaluatedRecordCount = run.EvaluatedRecordCount,
                EligibleRecordCount = run.EligibleRecordCount,
                CreatedReviewCount = run.CreatedReviewCount,
                SkippedExistingReviewCount = run.SkippedExistingReviewCount,
                BlockedByLegalHoldCount = run.BlockedByLegalHoldCount,
                AutomaticExecutionCount = run.AutomaticExecutionCount,
                NotificationMessageCount = run.NotificationMessageCount,
                FailureReason = run.FailureReason,
                PayloadJson = payload
            });
        }
        else
        {
            entity.LeaseId = run.LeaseId;
            entity.RanAt = run.RanAt;
            entity.RequestedByPersonId = run.RequestedByPersonId;
            entity.Status = run.Status;
            entity.ExecutionPolicy = run.ExecutionPolicy;
            entity.EvaluatedRecordCount = run.EvaluatedRecordCount;
            entity.EligibleRecordCount = run.EligibleRecordCount;
            entity.CreatedReviewCount = run.CreatedReviewCount;
            entity.SkippedExistingReviewCount = run.SkippedExistingReviewCount;
            entity.BlockedByLegalHoldCount = run.BlockedByLegalHoldCount;
            entity.AutomaticExecutionCount = run.AutomaticExecutionCount;
            entity.NotificationMessageCount = run.NotificationMessageCount;
            entity.FailureReason = run.FailureReason;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistDisasterRecoveryRun(RecordArrDisasterRecoveryRunResponse run)
    {
        if (!Guid.TryParse(run.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Disaster recovery run {run.DisasterRecoveryRunId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(run, JsonOptions);
        var entity = db.RecordArrDisasterRecoveryRuns.FirstOrDefault(row =>
            row.TenantId == tenantId && row.DisasterRecoveryRunId == run.DisasterRecoveryRunId);
        if (entity is null)
        {
            db.RecordArrDisasterRecoveryRuns.Add(new RecordArrDisasterRecoveryRunEntity
            {
                TenantId = tenantId,
                DisasterRecoveryRunId = run.DisasterRecoveryRunId,
                Scope = run.Scope,
                RecoveryPointId = run.RecoveryPointId,
                RecoveryPointCreatedAt = run.RecoveryPointCreatedAt,
                StartedAt = run.StartedAt,
                CompletedAt = run.CompletedAt,
                RequestedByPersonId = run.RequestedByPersonId,
                Status = run.Status,
                RpoTargetMinutes = run.RpoTargetMinutes,
                RtoTargetMinutes = run.RtoTargetMinutes,
                RecoveryPointAgeMinutes = run.RecoveryPointAgeMinutes,
                DurationSeconds = run.DurationSeconds,
                RpoMet = run.RpoMet,
                RtoMet = run.RtoMet,
                TotalRecordCount = run.TotalRecordCount,
                RestoredRecordCount = run.RestoredRecordCount,
                BlockedRecordCount = run.BlockedRecordCount,
                TotalFileCount = run.TotalFileCount,
                VerifiedFileCount = run.VerifiedFileCount,
                FailedFileCount = run.FailedFileCount,
                EvidenceSummary = run.EvidenceSummary,
                FailureReason = run.FailureReason,
                PayloadJson = payload
            });
        }
        else
        {
            entity.Scope = run.Scope;
            entity.RecoveryPointId = run.RecoveryPointId;
            entity.RecoveryPointCreatedAt = run.RecoveryPointCreatedAt;
            entity.StartedAt = run.StartedAt;
            entity.CompletedAt = run.CompletedAt;
            entity.RequestedByPersonId = run.RequestedByPersonId;
            entity.Status = run.Status;
            entity.RpoTargetMinutes = run.RpoTargetMinutes;
            entity.RtoTargetMinutes = run.RtoTargetMinutes;
            entity.RecoveryPointAgeMinutes = run.RecoveryPointAgeMinutes;
            entity.DurationSeconds = run.DurationSeconds;
            entity.RpoMet = run.RpoMet;
            entity.RtoMet = run.RtoMet;
            entity.TotalRecordCount = run.TotalRecordCount;
            entity.RestoredRecordCount = run.RestoredRecordCount;
            entity.BlockedRecordCount = run.BlockedRecordCount;
            entity.TotalFileCount = run.TotalFileCount;
            entity.VerifiedFileCount = run.VerifiedFileCount;
            entity.FailedFileCount = run.FailedFileCount;
            entity.EvidenceSummary = run.EvidenceSummary;
            entity.FailureReason = run.FailureReason;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistRetentionSchedulerLease(string tenantIdValue, RecordArrRetentionSchedulerLeaseResponse lease)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Retention scheduler lease {lease.LeaseId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(lease, JsonOptions);
        var entity = db.RecordArrRetentionSchedulerLeases.FirstOrDefault(row =>
            row.TenantId == tenantId && row.LeaseId == lease.LeaseId);
        if (entity is null)
        {
            db.RecordArrRetentionSchedulerLeases.Add(new RecordArrRetentionSchedulerLeaseEntity
            {
                TenantId = tenantId,
                LeaseId = lease.LeaseId,
                SchedulerKey = lease.SchedulerKey,
                Status = lease.Status,
                AcquiredAt = lease.AcquiredAt,
                ExpiresAt = lease.ExpiresAt,
                ReleasedAt = lease.ReleasedAt,
                AcquiredByPersonId = lease.AcquiredByPersonId,
                SchedulerRunId = lease.SchedulerRunId,
                PayloadJson = payload
            });
        }
        else
        {
            entity.SchedulerKey = lease.SchedulerKey;
            entity.Status = lease.Status;
            entity.AcquiredAt = lease.AcquiredAt;
            entity.ExpiresAt = lease.ExpiresAt;
            entity.ReleasedAt = lease.ReleasedAt;
            entity.AcquiredByPersonId = lease.AcquiredByPersonId;
            entity.SchedulerRunId = lease.SchedulerRunId;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistRetentionSchedulerOutboxMessage(string tenantIdValue, RecordArrRetentionSchedulerOutboxMessageResponse message)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Retention scheduler outbox message {message.OutboxMessageId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(message, JsonOptions);
        var entity = db.RecordArrRetentionSchedulerOutboxMessages.FirstOrDefault(row =>
            row.TenantId == tenantId && row.OutboxMessageId == message.OutboxMessageId);
        if (entity is null)
        {
            db.RecordArrRetentionSchedulerOutboxMessages.Add(new RecordArrRetentionSchedulerOutboxMessageEntity
            {
                TenantId = tenantId,
                OutboxMessageId = message.OutboxMessageId,
                SchedulerRunId = message.SchedulerRunId,
                MessageType = message.MessageType,
                Status = message.Status,
                TargetRecordId = message.TargetRecordId,
                DisposalReviewRef = message.DisposalReviewRef,
                CreatedAt = message.CreatedAt,
                CreatedByPersonId = message.CreatedByPersonId,
                DeduplicationKey = message.DeduplicationKey,
                ErrorMessage = message.ErrorMessage,
                DeliveryAttemptCount = message.DeliveryAttemptCount,
                LastAttemptAt = message.LastAttemptAt,
                DeliveredAt = message.DeliveredAt,
                DeliveredByPersonId = message.DeliveredByPersonId,
                DeliveryChannel = message.DeliveryChannel,
                RecipientRef = message.RecipientRef,
                DueAt = message.DueAt,
                EscalateAfter = message.EscalateAfter,
                EscalationLevel = message.EscalationLevel,
                EscalatedToRecipientRef = message.EscalatedToRecipientRef,
                EscalatedAt = message.EscalatedAt,
                ExternalProviderRef = message.ExternalProviderRef,
                PayloadJson = payload
            });
        }
        else
        {
            entity.SchedulerRunId = message.SchedulerRunId;
            entity.MessageType = message.MessageType;
            entity.Status = message.Status;
            entity.TargetRecordId = message.TargetRecordId;
            entity.DisposalReviewRef = message.DisposalReviewRef;
            entity.CreatedAt = message.CreatedAt;
            entity.CreatedByPersonId = message.CreatedByPersonId;
            entity.DeduplicationKey = message.DeduplicationKey;
            entity.ErrorMessage = message.ErrorMessage;
            entity.DeliveryAttemptCount = message.DeliveryAttemptCount;
            entity.LastAttemptAt = message.LastAttemptAt;
            entity.DeliveredAt = message.DeliveredAt;
            entity.DeliveredByPersonId = message.DeliveredByPersonId;
            entity.DeliveryChannel = message.DeliveryChannel;
            entity.RecipientRef = message.RecipientRef;
            entity.DueAt = message.DueAt;
            entity.EscalateAfter = message.EscalateAfter;
            entity.EscalationLevel = message.EscalationLevel;
            entity.EscalatedToRecipientRef = message.EscalatedToRecipientRef;
            entity.EscalatedAt = message.EscalatedAt;
            entity.ExternalProviderRef = message.ExternalProviderRef;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistLegalHold(string tenantIdValue, RecordArrLegalHoldResponse hold)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Legal hold {hold.LegalHoldId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(hold, JsonOptions);
        var entity = db.RecordArrLegalHolds.FirstOrDefault(row =>
            row.TenantId == tenantId && row.LegalHoldId == hold.LegalHoldId);
        if (entity is null)
        {
            db.RecordArrLegalHolds.Add(new RecordArrLegalHoldEntity
            {
                TenantId = tenantId,
                LegalHoldId = hold.LegalHoldId,
                HoldNumber = hold.HoldNumber,
                Title = hold.Title,
                Status = hold.Status,
                HoldType = hold.HoldType,
                SourceProduct = hold.SourceProduct,
                SourceObjectType = hold.SourceObjectType,
                SourceObjectId = hold.SourceObjectId,
                CreatedAt = hold.CreatedAt,
                CreatedByPersonId = hold.CreatedByPersonId,
                ActivatedAt = hold.ActivatedAt,
                ReleasedAt = hold.ReleasedAt,
                ReleasedByPersonId = hold.ReleasedByPersonId,
                PayloadJson = payload
            });
        }
        else
        {
            entity.HoldNumber = hold.HoldNumber;
            entity.Title = hold.Title;
            entity.Status = hold.Status;
            entity.HoldType = hold.HoldType;
            entity.SourceProduct = hold.SourceProduct;
            entity.SourceObjectType = hold.SourceObjectType;
            entity.SourceObjectId = hold.SourceObjectId;
            entity.CreatedAt = hold.CreatedAt;
            entity.CreatedByPersonId = hold.CreatedByPersonId;
            entity.ActivatedAt = hold.ActivatedAt;
            entity.ReleasedAt = hold.ReleasedAt;
            entity.ReleasedByPersonId = hold.ReleasedByPersonId;
            entity.PayloadJson = payload;
        }

        _legalHoldTenantIds[hold.LegalHoldId] = tenantIdValue;
        db.SaveChanges();
    }

    private void PersistControlledDocument(string tenantIdValue, RecordArrControlledDocumentResponse document)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Controlled document {document.ControlledDocumentId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(document, JsonOptions);
        var entity = db.RecordArrControlledDocuments.FirstOrDefault(row =>
            row.TenantId == tenantId && row.ControlledDocumentId == document.ControlledDocumentId);
        if (entity is null)
        {
            db.RecordArrControlledDocuments.Add(new RecordArrControlledDocumentEntity
            {
                TenantId = tenantId,
                ControlledDocumentId = document.ControlledDocumentId,
                DocumentNumber = document.DocumentNumber,
                RecordId = document.RecordId,
                Title = document.Title,
                DocumentClass = document.DocumentClass,
                DocumentType = document.DocumentType,
                DocumentSubtype = document.DocumentSubtype,
                ControlledDocumentType = document.ControlledDocumentType,
                Status = document.Status,
                OwnerPersonId = document.OwnerPersonId,
                DepartmentOrgUnitId = document.DepartmentOrgUnitId,
                StaffarrSiteId = document.StaffarrSiteId,
                CurrentVersionId = document.CurrentVersionId,
                NextReviewAt = document.NextReviewAt,
                EffectiveAt = document.EffectiveAt,
                ExpiresAt = document.ExpiresAt,
                AcknowledgementRequired = document.AcknowledgementRequired,
                PayloadJson = payload
            });
        }
        else
        {
            entity.DocumentNumber = document.DocumentNumber;
            entity.RecordId = document.RecordId;
            entity.Title = document.Title;
            entity.DocumentClass = document.DocumentClass;
            entity.DocumentType = document.DocumentType;
            entity.DocumentSubtype = document.DocumentSubtype;
            entity.ControlledDocumentType = document.ControlledDocumentType;
            entity.Status = document.Status;
            entity.OwnerPersonId = document.OwnerPersonId;
            entity.DepartmentOrgUnitId = document.DepartmentOrgUnitId;
            entity.StaffarrSiteId = document.StaffarrSiteId;
            entity.CurrentVersionId = document.CurrentVersionId;
            entity.NextReviewAt = document.NextReviewAt;
            entity.EffectiveAt = document.EffectiveAt;
            entity.ExpiresAt = document.ExpiresAt;
            entity.AcknowledgementRequired = document.AcknowledgementRequired;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistDocumentVersion(string tenantIdValue, RecordArrControlledDocumentVersionResponse version)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Controlled document version {version.VersionId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(version, JsonOptions);
        var entity = db.RecordArrControlledDocumentVersions.FirstOrDefault(row =>
            row.TenantId == tenantId && row.VersionId == version.VersionId);
        if (entity is null)
        {
            db.RecordArrControlledDocumentVersions.Add(new RecordArrControlledDocumentVersionEntity
            {
                TenantId = tenantId,
                VersionId = version.VersionId,
                ControlledDocumentId = version.ControlledDocumentId,
                VersionNumber = version.VersionNumber,
                VersionLabel = version.VersionLabel,
                Status = version.Status,
                FileName = version.FileName,
                CreatedAt = version.CreatedAt,
                CreatedByPersonId = version.CreatedByPersonId,
                SubmittedForReviewAt = version.SubmittedForReviewAt,
                ApprovedAt = version.ApprovedAt,
                ApprovedByPersonId = version.ApprovedByPersonId,
                EffectiveAt = version.EffectiveAt,
                SupersededAt = version.SupersededAt,
                PreviousVersionRef = version.PreviousVersionRef,
                NextVersionRef = version.NextVersionRef,
                FileRef = version.FileRef,
                PayloadJson = payload
            });
        }
        else
        {
            entity.ControlledDocumentId = version.ControlledDocumentId;
            entity.VersionNumber = version.VersionNumber;
            entity.VersionLabel = version.VersionLabel;
            entity.Status = version.Status;
            entity.FileName = version.FileName;
            entity.CreatedAt = version.CreatedAt;
            entity.CreatedByPersonId = version.CreatedByPersonId;
            entity.SubmittedForReviewAt = version.SubmittedForReviewAt;
            entity.ApprovedAt = version.ApprovedAt;
            entity.ApprovedByPersonId = version.ApprovedByPersonId;
            entity.EffectiveAt = version.EffectiveAt;
            entity.SupersededAt = version.SupersededAt;
            entity.PreviousVersionRef = version.PreviousVersionRef;
            entity.NextVersionRef = version.NextVersionRef;
            entity.FileRef = version.FileRef;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistDocumentReview(string tenantIdValue, RecordArrDocumentReviewResponse review)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Document review {review.DocumentReviewId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(review, JsonOptions);
        var entity = db.RecordArrDocumentReviews.FirstOrDefault(row =>
            row.TenantId == tenantId && row.DocumentReviewId == review.DocumentReviewId);
        if (entity is null)
        {
            db.RecordArrDocumentReviews.Add(new RecordArrDocumentReviewEntity
            {
                TenantId = tenantId,
                DocumentReviewId = review.DocumentReviewId,
                ControlledDocumentId = review.ControlledDocumentId,
                VersionId = review.VersionId,
                ReviewType = review.ReviewType,
                Status = review.Status,
                RequestedByPersonId = review.RequestedByPersonId,
                ReviewerPersonId = review.ReviewerPersonId,
                RequestedAt = review.RequestedAt,
                DueAt = review.DueAt,
                ReviewedAt = review.ReviewedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.ControlledDocumentId = review.ControlledDocumentId;
            entity.VersionId = review.VersionId;
            entity.ReviewType = review.ReviewType;
            entity.Status = review.Status;
            entity.RequestedByPersonId = review.RequestedByPersonId;
            entity.ReviewerPersonId = review.ReviewerPersonId;
            entity.RequestedAt = review.RequestedAt;
            entity.DueAt = review.DueAt;
            entity.ReviewedAt = review.ReviewedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistDocumentDistribution(string tenantIdValue, RecordArrDocumentDistributionResponse distribution)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Document distribution {distribution.DistributionId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(distribution, JsonOptions);
        var entity = db.RecordArrDocumentDistributions.FirstOrDefault(row =>
            row.TenantId == tenantId && row.DistributionId == distribution.DistributionId);
        if (entity is null)
        {
            db.RecordArrDocumentDistributions.Add(new RecordArrDocumentDistributionEntity
            {
                TenantId = tenantId,
                DistributionId = distribution.DistributionId,
                ControlledDocumentId = distribution.ControlledDocumentId,
                VersionId = distribution.VersionId,
                DistributionType = distribution.DistributionType,
                TargetRef = distribution.TargetRef,
                Status = distribution.Status,
                DistributedAt = distribution.DistributedAt,
                AcknowledgedAt = distribution.AcknowledgedAt,
                AcknowledgementRef = distribution.AcknowledgementRef,
                PayloadJson = payload
            });
        }
        else
        {
            entity.ControlledDocumentId = distribution.ControlledDocumentId;
            entity.VersionId = distribution.VersionId;
            entity.DistributionType = distribution.DistributionType;
            entity.TargetRef = distribution.TargetRef;
            entity.Status = distribution.Status;
            entity.DistributedAt = distribution.DistributedAt;
            entity.AcknowledgedAt = distribution.AcknowledgedAt;
            entity.AcknowledgementRef = distribution.AcknowledgementRef;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistDocumentAcknowledgement(string tenantIdValue, RecordArrDocumentAcknowledgementResponse acknowledgement)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Document acknowledgement {acknowledgement.AcknowledgementId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(acknowledgement, JsonOptions);
        var entity = db.RecordArrDocumentAcknowledgements.FirstOrDefault(row =>
            row.TenantId == tenantId && row.AcknowledgementId == acknowledgement.AcknowledgementId);
        if (entity is null)
        {
            db.RecordArrDocumentAcknowledgements.Add(new RecordArrDocumentAcknowledgementEntity
            {
                TenantId = tenantId,
                AcknowledgementId = acknowledgement.AcknowledgementId,
                ControlledDocumentId = acknowledgement.ControlledDocumentId,
                VersionId = acknowledgement.VersionId,
                PersonId = acknowledgement.PersonId,
                Status = acknowledgement.Status,
                AcknowledgedAt = acknowledgement.AcknowledgedAt,
                SignatureRecordRef = acknowledgement.SignatureRecordRef,
                DueAt = acknowledgement.DueAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.ControlledDocumentId = acknowledgement.ControlledDocumentId;
            entity.VersionId = acknowledgement.VersionId;
            entity.PersonId = acknowledgement.PersonId;
            entity.Status = acknowledgement.Status;
            entity.AcknowledgedAt = acknowledgement.AcknowledgedAt;
            entity.SignatureRecordRef = acknowledgement.SignatureRecordRef;
            entity.DueAt = acknowledgement.DueAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistAccessPolicy(string tenantIdValue, RecordArrAccessPolicyResponse policy)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Access policy {policy.AccessPolicyId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(policy, JsonOptions);
        var entity = db.RecordArrAccessPolicies.FirstOrDefault(row =>
            row.TenantId == tenantId && row.AccessPolicyId == policy.AccessPolicyId);
        if (entity is null)
        {
            db.RecordArrAccessPolicies.Add(new RecordArrAccessPolicyEntity
            {
                TenantId = tenantId,
                AccessPolicyId = policy.AccessPolicyId,
                RecordId = policy.RecordId,
                PolicyType = policy.PolicyType,
                Status = policy.Status,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = policy.RecordId;
            entity.PolicyType = policy.PolicyType;
            entity.Status = policy.Status;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistAccessGrant(string tenantIdValue, RecordArrAccessGrantResponse grant)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Access grant {grant.AccessGrantId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(grant, JsonOptions);
        var entity = db.RecordArrAccessGrants.FirstOrDefault(row =>
            row.TenantId == tenantId && row.AccessGrantId == grant.AccessGrantId);
        if (entity is null)
        {
            db.RecordArrAccessGrants.Add(new RecordArrAccessGrantEntity
            {
                TenantId = tenantId,
                AccessGrantId = grant.AccessGrantId,
                RecordId = grant.RecordId,
                GranteeType = grant.GranteeType,
                GranteeRef = grant.GranteeRef,
                Permission = grant.Permission,
                Status = grant.Status,
                GrantedByPersonId = grant.GrantedByPersonId,
                GrantedAt = grant.GrantedAt,
                ExpiresAt = grant.ExpiresAt,
                RevokedAt = grant.RevokedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = grant.RecordId;
            entity.GranteeType = grant.GranteeType;
            entity.GranteeRef = grant.GranteeRef;
            entity.Permission = grant.Permission;
            entity.Status = grant.Status;
            entity.GrantedByPersonId = grant.GrantedByPersonId;
            entity.GrantedAt = grant.GrantedAt;
            entity.ExpiresAt = grant.ExpiresAt;
            entity.RevokedAt = grant.RevokedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistExternalShare(string tenantIdValue, RecordArrExternalShareResponse share)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"External share {share.ExternalShareId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(share, JsonOptions);
        var entity = db.RecordArrExternalShares.FirstOrDefault(row =>
            row.TenantId == tenantId && row.ExternalShareId == share.ExternalShareId);
        if (entity is null)
        {
            db.RecordArrExternalShares.Add(new RecordArrExternalShareEntity
            {
                TenantId = tenantId,
                ExternalShareId = share.ExternalShareId,
                ShareNumber = share.ShareNumber,
                RecordId = share.RecordId,
                SharePurpose = share.SharePurpose,
                Status = share.Status,
                RecipientEmail = share.RecipientEmail,
                CreatedAt = share.CreatedAt,
                CreatedByPersonId = share.CreatedByPersonId,
                ExpiresAt = share.ExpiresAt,
                RevokedAt = share.RevokedAt,
                LastAccessedAt = share.LastAccessedAt,
                AccessCount = share.AccessCount,
                PayloadJson = payload
            });
        }
        else
        {
            entity.ShareNumber = share.ShareNumber;
            entity.RecordId = share.RecordId;
            entity.SharePurpose = share.SharePurpose;
            entity.Status = share.Status;
            entity.RecipientEmail = share.RecipientEmail;
            entity.CreatedAt = share.CreatedAt;
            entity.CreatedByPersonId = share.CreatedByPersonId;
            entity.ExpiresAt = share.ExpiresAt;
            entity.RevokedAt = share.RevokedAt;
            entity.LastAccessedAt = share.LastAccessedAt;
            entity.AccessCount = share.AccessCount;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistRedaction(string tenantIdValue, RecordArrRedactionResponse redaction)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Redaction {redaction.RedactionId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(redaction, JsonOptions);
        var entity = db.RecordArrRedactions.FirstOrDefault(row =>
            row.TenantId == tenantId && row.RedactionId == redaction.RedactionId);
        if (entity is null)
        {
            db.RecordArrRedactions.Add(new RecordArrRedactionEntity
            {
                TenantId = tenantId,
                RedactionId = redaction.RedactionId,
                SourceRecordId = redaction.SourceRecordId,
                RedactedRecordId = redaction.RedactedRecordId,
                RedactionReason = redaction.RedactionReason,
                Status = redaction.Status,
                RedactedByPersonId = redaction.RedactedByPersonId,
                RedactedAt = redaction.RedactedAt,
                PayloadJson = payload
            });
        }
        else
        {
            entity.SourceRecordId = redaction.SourceRecordId;
            entity.RedactedRecordId = redaction.RedactedRecordId;
            entity.RedactionReason = redaction.RedactionReason;
            entity.Status = redaction.Status;
            entity.RedactedByPersonId = redaction.RedactedByPersonId;
            entity.RedactedAt = redaction.RedactedAt;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistRedactionProviderJob(RecordArrRedactionProviderJobResponse job)
    {
        if (!Guid.TryParse(job.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Redaction provider job {job.ProviderJobId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(job, JsonOptions);
        var entity = db.RecordArrRedactionProviderJobs.FirstOrDefault(row =>
            row.TenantId == tenantId && row.ProviderJobId == job.ProviderJobId);
        if (entity is null)
        {
            db.RecordArrRedactionProviderJobs.Add(new RecordArrRedactionProviderJobEntity
            {
                TenantId = tenantId,
                ProviderJobId = job.ProviderJobId,
                RedactionId = job.RedactionId,
                SourceRecordId = job.SourceRecordId,
                RedactedRecordId = job.RedactedRecordId,
                ProviderName = job.ProviderName,
                ProviderJobRef = job.ProviderJobRef,
                Status = job.Status,
                RequestedByPersonId = job.RequestedByPersonId,
                RequestedAt = job.RequestedAt,
                RedactionPackageHash = job.RedactionPackageHash,
                SubmissionEvidenceHash = job.SubmissionEvidenceHash,
                LastSubmittedAt = job.LastSubmittedAt,
                ProviderCallbackStatus = job.ProviderCallbackStatus,
                ProviderCallbackRef = job.ProviderCallbackRef,
                ProviderCallbackReceivedAt = job.ProviderCallbackReceivedAt,
                ProviderEvidenceHash = job.ProviderEvidenceHash,
                FailureReason = job.FailureReason,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RedactionId = job.RedactionId;
            entity.SourceRecordId = job.SourceRecordId;
            entity.RedactedRecordId = job.RedactedRecordId;
            entity.ProviderName = job.ProviderName;
            entity.ProviderJobRef = job.ProviderJobRef;
            entity.Status = job.Status;
            entity.RequestedByPersonId = job.RequestedByPersonId;
            entity.RequestedAt = job.RequestedAt;
            entity.RedactionPackageHash = job.RedactionPackageHash;
            entity.SubmissionEvidenceHash = job.SubmissionEvidenceHash;
            entity.LastSubmittedAt = job.LastSubmittedAt;
            entity.ProviderCallbackStatus = job.ProviderCallbackStatus;
            entity.ProviderCallbackRef = job.ProviderCallbackRef;
            entity.ProviderCallbackReceivedAt = job.ProviderCallbackReceivedAt;
            entity.ProviderEvidenceHash = job.ProviderEvidenceHash;
            entity.FailureReason = job.FailureReason;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistSignatureRecord(string tenantIdValue, RecordArrSignatureRecordResponse signature)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Signature record {signature.SignatureRecordId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(signature, JsonOptions);
        var entity = db.RecordArrSignatureRecords.FirstOrDefault(row =>
            row.TenantId == tenantId && row.SignatureRecordId == signature.SignatureRecordId);
        if (entity is null)
        {
            db.RecordArrSignatureRecords.Add(new RecordArrSignatureRecordEntity
            {
                TenantId = tenantId,
                SignatureRecordId = signature.SignatureRecordId,
                RecordId = signature.RecordId,
                SignaturePurpose = signature.SignaturePurpose,
                SignerPersonId = signature.SignerPersonId,
                SignerExternalName = signature.SignerExternalName,
                SignerTitle = signature.SignerTitle,
                SignatureFileRef = signature.SignatureFileRef,
                SignedAt = signature.SignedAt,
                CapturedByPersonId = signature.CapturedByPersonId,
                SourceProduct = signature.SourceProduct,
                SourceObjectRef = signature.SourceObjectRef,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = signature.RecordId;
            entity.SignaturePurpose = signature.SignaturePurpose;
            entity.SignerPersonId = signature.SignerPersonId;
            entity.SignerExternalName = signature.SignerExternalName;
            entity.SignerTitle = signature.SignerTitle;
            entity.SignatureFileRef = signature.SignatureFileRef;
            entity.SignedAt = signature.SignedAt;
            entity.CapturedByPersonId = signature.CapturedByPersonId;
            entity.SourceProduct = signature.SourceProduct;
            entity.SourceObjectRef = signature.SourceObjectRef;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistSignatureTrustServiceJob(RecordArrSignatureTrustServiceJobResponse job)
    {
        if (!Guid.TryParse(job.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Signature trust-service job {job.TrustServiceJobId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(job, JsonOptions);
        var entity = db.RecordArrSignatureTrustServiceJobs.FirstOrDefault(row =>
            row.TenantId == tenantId && row.TrustServiceJobId == job.TrustServiceJobId);
        if (entity is null)
        {
            db.RecordArrSignatureTrustServiceJobs.Add(new RecordArrSignatureTrustServiceJobEntity
            {
                TenantId = tenantId,
                TrustServiceJobId = job.TrustServiceJobId,
                SignatureRecordId = job.SignatureRecordId,
                RecordId = job.RecordId,
                ProviderName = job.ProviderName,
                ProviderEnvelopeRef = job.ProviderEnvelopeRef,
                Status = job.Status,
                RequestedByPersonId = job.RequestedByPersonId,
                RequestedAt = job.RequestedAt,
                CertificateFingerprintSha256 = job.CertificateFingerprintSha256,
                SignatureEvidenceHash = job.SignatureEvidenceHash,
                SubmissionEvidenceHash = job.SubmissionEvidenceHash,
                LastSubmittedAt = job.LastSubmittedAt,
                ProviderCallbackStatus = job.ProviderCallbackStatus,
                ProviderCallbackRef = job.ProviderCallbackRef,
                ProviderCallbackReceivedAt = job.ProviderCallbackReceivedAt,
                ProviderCallbackEvidenceHash = job.ProviderCallbackEvidenceHash,
                TrustTimestampAuthorityRef = job.TrustTimestampAuthorityRef,
                LongTermValidationStatus = job.LongTermValidationStatus,
                FailureReason = job.FailureReason,
                PayloadJson = payload
            });
        }
        else
        {
            entity.SignatureRecordId = job.SignatureRecordId;
            entity.RecordId = job.RecordId;
            entity.ProviderName = job.ProviderName;
            entity.ProviderEnvelopeRef = job.ProviderEnvelopeRef;
            entity.Status = job.Status;
            entity.RequestedByPersonId = job.RequestedByPersonId;
            entity.RequestedAt = job.RequestedAt;
            entity.CertificateFingerprintSha256 = job.CertificateFingerprintSha256;
            entity.SignatureEvidenceHash = job.SignatureEvidenceHash;
            entity.SubmissionEvidenceHash = job.SubmissionEvidenceHash;
            entity.LastSubmittedAt = job.LastSubmittedAt;
            entity.ProviderCallbackStatus = job.ProviderCallbackStatus;
            entity.ProviderCallbackRef = job.ProviderCallbackRef;
            entity.ProviderCallbackReceivedAt = job.ProviderCallbackReceivedAt;
            entity.ProviderCallbackEvidenceHash = job.ProviderCallbackEvidenceHash;
            entity.TrustTimestampAuthorityRef = job.TrustTimestampAuthorityRef;
            entity.LongTermValidationStatus = job.LongTermValidationStatus;
            entity.FailureReason = job.FailureReason;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistPhotoEvidence(string tenantIdValue, RecordArrPhotoEvidenceResponse photo)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            throw new InvalidOperationException($"Photo evidence {photo.PhotoEvidenceId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(photo, JsonOptions);
        var entity = db.RecordArrPhotoEvidence.FirstOrDefault(row =>
            row.TenantId == tenantId && row.PhotoEvidenceId == photo.PhotoEvidenceId);
        if (entity is null)
        {
            db.RecordArrPhotoEvidence.Add(new RecordArrPhotoEvidenceEntity
            {
                TenantId = tenantId,
                PhotoEvidenceId = photo.PhotoEvidenceId,
                RecordId = photo.RecordId,
                PhotoPurpose = photo.PhotoPurpose,
                SourceProduct = photo.SourceProduct,
                SourceObjectRef = photo.SourceObjectRef,
                CapturedAt = photo.CapturedAt,
                CapturedByPersonId = photo.CapturedByPersonId,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = photo.RecordId;
            entity.PhotoPurpose = photo.PhotoPurpose;
            entity.SourceProduct = photo.SourceProduct;
            entity.SourceObjectRef = photo.SourceObjectRef;
            entity.CapturedAt = photo.CapturedAt;
            entity.CapturedByPersonId = photo.CapturedByPersonId;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistAccessLog(string tenantIdValue, RecordArrAccessLogResponse log)
    {
        if (!Guid.TryParse(tenantIdValue, out var tenantId))
        {
            return;
        }

        var payload = JsonSerializer.Serialize(log, JsonOptions);
        var entity = db.RecordArrAccessLogs.FirstOrDefault(row =>
            row.TenantId == tenantId && row.AccessLogId == log.AccessLogId);
        if (entity is null)
        {
            db.RecordArrAccessLogs.Add(new RecordArrAccessLogEntity
            {
                TenantId = tenantId,
                AccessLogId = log.AccessLogId,
                RecordId = log.RecordId,
                Action = log.Action,
                Result = log.Result,
                ActorPersonId = log.ActorPersonId,
                ActorServiceClientId = log.ActorServiceClientId,
                ExternalShareId = log.ExternalShareId,
                OccurredAt = log.OccurredAt,
                ReasonCode = log.ReasonCode,
                PreviousAccessLogHash = log.PreviousAccessLogHash,
                AccessLogHash = log.AccessLogHash,
                PayloadJson = payload
            });
            db.SaveChanges();
        }
    }

    private void PersistAuditEvent(RecordArrAuditEventResponse auditEvent)
    {
        if (!Guid.TryParse(auditEvent.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Audit event {auditEvent.AuditEventId} has an invalid tenant id.");
        }

        var exists = db.RecordArrAuditEvents.Any(row =>
            row.TenantId == tenantId && row.AuditEventId == auditEvent.AuditEventId);
        if (exists)
        {
            return;
        }

        db.RecordArrAuditEvents.Add(new RecordArrAuditEventEntity
        {
            TenantId = tenantId,
            AuditEventId = auditEvent.AuditEventId,
            RecordId = auditEvent.RecordId,
            Action = auditEvent.Action,
            Outcome = auditEvent.Outcome,
            ActorType = auditEvent.ActorType,
            ActorPersonId = auditEvent.ActorPersonId,
            ActorServiceClientId = auditEvent.ActorServiceClientId,
            ExternalShareId = auditEvent.ExternalShareId,
            OccurredAt = auditEvent.OccurredAt,
            ReasonCode = auditEvent.ReasonCode,
            CorrelationId = auditEvent.CorrelationId,
            PreviousEventHash = auditEvent.PreviousEventHash,
            EventHash = auditEvent.EventHash,
            PayloadJson = JsonSerializer.Serialize(auditEvent, JsonOptions)
        });
        db.SaveChanges();
    }

    private void PersistAccessHistorySeal(RecordArrAccessHistorySealResponse seal)
    {
        if (!Guid.TryParse(seal.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Access history seal {seal.AccessHistorySealId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(seal, JsonOptions);
        var entity = db.RecordArrAccessHistorySeals.FirstOrDefault(row =>
            row.TenantId == tenantId && row.AccessHistorySealId == seal.AccessHistorySealId);
        if (entity is null)
        {
            db.RecordArrAccessHistorySeals.Add(new RecordArrAccessHistorySealEntity
            {
                TenantId = tenantId,
                AccessHistorySealId = seal.AccessHistorySealId,
                RecordId = seal.RecordId,
                Scope = seal.Scope,
                SealedAccessLogCount = seal.SealedAccessLogCount,
                FirstAccessLogId = seal.FirstAccessLogId,
                SealedThroughAccessLogId = seal.SealedThroughAccessLogId,
                SealedThroughAccessLogHash = seal.SealedThroughAccessLogHash,
                SealHash = seal.SealHash,
                Status = seal.Status,
                SealedByPersonId = seal.SealedByPersonId,
                SealedAt = seal.SealedAt,
                VerifiedAt = seal.VerifiedAt,
                IssueSummary = seal.IssueSummary,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = seal.RecordId;
            entity.Scope = seal.Scope;
            entity.SealedAccessLogCount = seal.SealedAccessLogCount;
            entity.FirstAccessLogId = seal.FirstAccessLogId;
            entity.SealedThroughAccessLogId = seal.SealedThroughAccessLogId;
            entity.SealedThroughAccessLogHash = seal.SealedThroughAccessLogHash;
            entity.SealHash = seal.SealHash;
            entity.Status = seal.Status;
            entity.SealedByPersonId = seal.SealedByPersonId;
            entity.SealedAt = seal.SealedAt;
            entity.VerifiedAt = seal.VerifiedAt;
            entity.IssueSummary = seal.IssueSummary;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

    private void PersistAuditSeal(RecordArrAuditSealResponse seal)
    {
        if (!Guid.TryParse(seal.TenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Audit seal {seal.AuditSealId} has an invalid tenant id.");
        }

        var payload = JsonSerializer.Serialize(seal, JsonOptions);
        var entity = db.RecordArrAuditSeals.FirstOrDefault(row =>
            row.TenantId == tenantId && row.AuditSealId == seal.AuditSealId);
        if (entity is null)
        {
            db.RecordArrAuditSeals.Add(new RecordArrAuditSealEntity
            {
                TenantId = tenantId,
                AuditSealId = seal.AuditSealId,
                RecordId = seal.RecordId,
                Scope = seal.Scope,
                SealedEventCount = seal.SealedEventCount,
                FirstAuditEventId = seal.FirstAuditEventId,
                SealedThroughAuditEventId = seal.SealedThroughAuditEventId,
                SealedThroughEventHash = seal.SealedThroughEventHash,
                SealHash = seal.SealHash,
                Status = seal.Status,
                SealedByPersonId = seal.SealedByPersonId,
                SealedAt = seal.SealedAt,
                VerifiedAt = seal.VerifiedAt,
                IssueSummary = seal.IssueSummary,
                PayloadJson = payload
            });
        }
        else
        {
            entity.RecordId = seal.RecordId;
            entity.Scope = seal.Scope;
            entity.SealedEventCount = seal.SealedEventCount;
            entity.FirstAuditEventId = seal.FirstAuditEventId;
            entity.SealedThroughAuditEventId = seal.SealedThroughAuditEventId;
            entity.SealedThroughEventHash = seal.SealedThroughEventHash;
            entity.SealHash = seal.SealHash;
            entity.Status = seal.Status;
            entity.SealedByPersonId = seal.SealedByPersonId;
            entity.SealedAt = seal.SealedAt;
            entity.VerifiedAt = seal.VerifiedAt;
            entity.IssueSummary = seal.IssueSummary;
            entity.PayloadJson = payload;
        }

        db.SaveChanges();
    }

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
        AddAccessLog(record.RecordId, status, "allowed", actorPersonId, null, null, null, null, reasonCode);
        PersistRecord(updated);
        return ProjectRecord(updated);
    }

    private string ResolveRecordTenantId(string recordId)
    {
        var fileTenantId = _files.FirstOrDefault(file =>
            string.Equals(file.RecordId, recordId, StringComparison.OrdinalIgnoreCase))?.TenantId;

        return string.IsNullOrWhiteSpace(fileTenantId) ? "unassigned" : fileTenantId;
    }

    private bool RecordBelongsToTenant(string recordId, string tenantId)
        => string.Equals(ResolveRecordTenantId(recordId), tenantId, StringComparison.OrdinalIgnoreCase);

    private bool LegalHoldBelongsToTenant(RecordArrLegalHoldResponse hold, string tenantId)
    {
        if (_legalHoldTenantIds.TryGetValue(hold.LegalHoldId, out var knownTenantId))
        {
            return string.Equals(knownTenantId, tenantId, StringComparison.OrdinalIgnoreCase);
        }

        var persistedTenantId = db.RecordArrLegalHolds
            .AsNoTracking()
            .Where(row => row.LegalHoldId == hold.LegalHoldId)
            .Select(row => row.TenantId.ToString())
            .FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(persistedTenantId))
        {
            _legalHoldTenantIds[hold.LegalHoldId] = persistedTenantId;
            return string.Equals(persistedTenantId, tenantId, StringComparison.OrdinalIgnoreCase);
        }

        return hold.RecordRefs.Any(recordRef => RecordBelongsToTenant(recordRef, tenantId));
    }

    private string ResolveControlledDocumentTenantId(string controlledDocumentId)
    {
        var recordId = _controlledDocuments.FirstOrDefault(document =>
            string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase))?.RecordId;
        return string.IsNullOrWhiteSpace(recordId) ? "unassigned" : ResolveRecordTenantId(recordId);
    }

    private bool ControlledDocumentBelongsToTenant(string controlledDocumentId, string tenantId)
        => string.Equals(ResolveControlledDocumentTenantId(controlledDocumentId), tenantId, StringComparison.OrdinalIgnoreCase);

    private string ResolveExternalShareTenantId(string shareId)
    {
        var recordId = _externalShares.FirstOrDefault(share =>
            string.Equals(share.ExternalShareId, shareId, StringComparison.OrdinalIgnoreCase))?.RecordId;
        return string.IsNullOrWhiteSpace(recordId) ? "unassigned" : ResolveRecordTenantId(recordId);
    }

    private string ResolveAccessGrantTenantId(string accessGrantId)
    {
        var recordId = _accessGrants.FirstOrDefault(grant =>
            string.Equals(grant.AccessGrantId, accessGrantId, StringComparison.OrdinalIgnoreCase))?.RecordId;
        return string.IsNullOrWhiteSpace(recordId) ? "unassigned" : ResolveRecordTenantId(recordId);
    }

    private string ResolvePackageTenantId(RecordArrPackageResponse package)
    {
        foreach (var recordRef in package.RecordRefs)
        {
            var tenantId = ResolveRecordTenantId(recordRef);
            if (!string.Equals(tenantId, "unassigned", StringComparison.OrdinalIgnoreCase))
            {
                return tenantId;
            }
        }

        if (!string.IsNullOrWhiteSpace(package.GeneratedZipFileRef))
        {
            var zipTenantId = _files.FirstOrDefault(file =>
                string.Equals(file.FileId, package.GeneratedZipFileRef, StringComparison.OrdinalIgnoreCase))?.TenantId;
            if (!string.IsNullOrWhiteSpace(zipTenantId))
            {
                return zipTenantId;
            }
        }

        if (!string.IsNullOrWhiteSpace(package.GeneratedPdfRecordRef))
        {
            var pdfTenantId = _files.FirstOrDefault(file =>
                string.Equals(file.FileId, package.GeneratedPdfRecordRef, StringComparison.OrdinalIgnoreCase))?.TenantId;
            if (!string.IsNullOrWhiteSpace(pdfTenantId))
            {
                return pdfTenantId;
            }
        }

        return "unassigned";
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
            "pending",
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

    private static bool IsFileSafeForDelivery(RecordArrFileResponse file)
        => string.Equals(file.VirusScanStatus, "clean", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(file.VirusScanStatus, "skipped", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeMalwareScanStatus(string status, string? threatName)
    {
        if (!string.IsNullOrWhiteSpace(threatName))
        {
            return "infected";
        }

        var normalized = string.IsNullOrWhiteSpace(status) ? "failed" : status.Trim().ToLowerInvariant();
        return normalized switch
        {
            "clean" => "clean",
            "infected" => "infected",
            "failed" => "failed",
            "dead_lettered" => "dead_lettered",
            "skipped" => "skipped",
            "pending" => "pending",
            _ => throw new InvalidOperationException($"Unsupported malware scan status '{status}'.")
        };
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
            PersistFile(_files[i]);
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
        var tenantId = ResolveRecordTenantId(current.RecordId);
        if (!string.Equals(tenantId, "unassigned", StringComparison.OrdinalIgnoreCase))
        {
            PersistControlledDocument(tenantId, _controlledDocuments[index]);
        }
    }

    private static RecordArrAuditTrailEntryResponse CreateControlledDocumentAuditTrailEntry(string action, string actorPersonId, string details)
        => new($"aud-{Guid.NewGuid():N}"[..12], action, actorPersonId, DateTimeOffset.UtcNow, details);

    private void EnsureRecordCanBeDisposed(string recordId, string action, string actorPersonId)
        => EnsureRecordNotUnderActiveLegalHold(recordId, action, actorPersonId);

    private void EnsureRecordNotUnderActiveLegalHold(string recordId, string action, string? actorPersonId)
    {
        if (RecordHasActiveLegalHold(recordId, action, actorPersonId))
        {
            var activeHold = FindActiveLegalHoldForRecord(recordId);
            throw new InvalidOperationException($"Record {recordId} is blocked by legal hold {activeHold?.HoldNumber ?? "active hold"}.");
        }
    }

    private bool RecordHasActiveLegalHold(string recordId, string action, string? actorPersonId, string? externalShareId = null)
    {
        var activeHold = FindActiveLegalHoldForRecord(recordId);
        if (activeHold is null)
        {
            return false;
        }

        AddAccessLog(recordId, action, "denied", actorPersonId, null, externalShareId, null, null, "blocked_by_legal_hold");
        return true;
    }

    private void EnsureRecordRefsNotUnderActiveLegalHold(IEnumerable<string> recordIds, string action, string? actorPersonId)
    {
        foreach (var recordId in recordIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            EnsureRecordNotUnderActiveLegalHold(recordId, action, actorPersonId);
        }
    }

    private RecordArrLegalHoldResponse? FindActiveLegalHoldForRecord(string recordId)
    {
        var tenantId = ResolveRecordTenantId(recordId);
        var record = _records.FirstOrDefault(candidate => string.Equals(candidate.RecordId, recordId, StringComparison.OrdinalIgnoreCase));

        return _legalHolds.FirstOrDefault(hold =>
            string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase) &&
            LegalHoldBelongsToTenant(hold, tenantId) &&
            (hold.RecordRefs.Any(reference => string.Equals(reference, recordId, StringComparison.OrdinalIgnoreCase)) ||
             record is not null && hold.ScopeRules.Any(scopeRule => IsRecordMatchedByLegalHoldScopeRule(record, ParseLegalHoldScopeRule(scopeRule)))));
    }

    public IReadOnlyList<RecordArrLegalHoldResponse> GetLegalHolds(string tenantId)
    {
        lock (_gate)
        {
            return _legalHolds
                .Where(hold => LegalHoldBelongsToTenant(hold, tenantId))
                .OrderByDescending(hold => hold.CreatedAt)
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrControlledDocumentResponse> GetControlledDocuments(string tenantId)
    {
        lock (_gate)
        {
            return _controlledDocuments
                .Where(document => RecordBelongsToTenant(document.RecordId, tenantId))
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrControlledDocumentVersionResponse> GetDocumentVersions(string tenantId, string? controlledDocumentId = null)
    {
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(controlledDocumentId) && !ControlledDocumentBelongsToTenant(controlledDocumentId, tenantId))
            {
                return [];
            }

            return string.IsNullOrWhiteSpace(controlledDocumentId)
                ? _documentVersions
                    .Where(version => ControlledDocumentBelongsToTenant(version.ControlledDocumentId, tenantId))
                    .ToArray()
                : _documentVersions
                    .Where(version => string.Equals(version.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(version => version.VersionNumber)
                    .ToArray();
        }
    }

    public IReadOnlyList<RecordArrDocumentReviewResponse> GetDocumentReviews(string tenantId, string? controlledDocumentId = null)
    {
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(controlledDocumentId) && !ControlledDocumentBelongsToTenant(controlledDocumentId, tenantId))
            {
                return [];
            }

            return string.IsNullOrWhiteSpace(controlledDocumentId)
                ? _documentReviews
                    .Where(review => ControlledDocumentBelongsToTenant(review.ControlledDocumentId, tenantId))
                    .ToArray()
                : _documentReviews
                    .Where(review => string.Equals(review.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
        }
    }

    public IReadOnlyList<RecordArrDocumentDistributionResponse> GetDocumentDistributions(string tenantId, string? controlledDocumentId = null)
    {
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(controlledDocumentId) && !ControlledDocumentBelongsToTenant(controlledDocumentId, tenantId))
            {
                return [];
            }

            return string.IsNullOrWhiteSpace(controlledDocumentId)
                ? _documentDistributions
                    .Where(distribution => ControlledDocumentBelongsToTenant(distribution.ControlledDocumentId, tenantId))
                    .ToArray()
                : _documentDistributions
                    .Where(distribution => string.Equals(distribution.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
        }
    }

    public IReadOnlyList<RecordArrDocumentAcknowledgementResponse> GetDocumentAcknowledgements(string tenantId, string? controlledDocumentId = null)
    {
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(controlledDocumentId) && !ControlledDocumentBelongsToTenant(controlledDocumentId, tenantId))
            {
                return [];
            }

            return string.IsNullOrWhiteSpace(controlledDocumentId)
                ? _documentAcknowledgements
                    .Where(acknowledgement => ControlledDocumentBelongsToTenant(acknowledgement.ControlledDocumentId, tenantId))
                    .ToArray()
                : _documentAcknowledgements
                    .Where(acknowledgement => string.Equals(acknowledgement.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
        }
    }

    public IReadOnlyList<RecordArrReminderResponse> GetReminders(ClaimsPrincipal principal)
    {
        lock (_gate)
        {
            RefreshControlledDocumentWorkflows(principal.GetTenantId().ToString());

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

    public RecordArrDocumentDistributionResponse CreateDocumentDistribution(string tenantId, string controlledDocumentId, string versionId, string distributionType, string targetRef)
    {
        lock (_gate)
        {
            if (!ControlledDocumentBelongsToTenant(controlledDocumentId, tenantId))
            {
                throw new InvalidOperationException($"Controlled document {controlledDocumentId} not found.");
            }
            var controlledDocument = _controlledDocuments.First(document => string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));
            EnsureRecordNotUnderActiveLegalHold(controlledDocument.RecordId, "document_distribution.created", "system");

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
            PersistDocumentDistribution(tenantId, distribution);
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "distributed",
                    "system",
                    $"Distributed version {versionId} to {distributionType}:{targetRef}."));
            return distribution;
        }
    }

    public RecordArrDocumentDistributionResponse RevokeDocumentDistribution(string tenantId, string distributionId, string revokedByPersonId, string? revokeReason)
    {
        lock (_gate)
        {
            return UpdateDocumentDistributionStatus(tenantId, distributionId, "revoked", revokedByPersonId, revokeReason ?? $"Revoked by {revokedByPersonId}");
        }
    }

    public RecordArrDocumentDistributionResponse ExpireDocumentDistribution(string tenantId, string distributionId, string expiredByPersonId, string? expireReason)
    {
        lock (_gate)
        {
            return UpdateDocumentDistributionStatus(tenantId, distributionId, "expired", expiredByPersonId, expireReason ?? $"Expired by {expiredByPersonId}");
        }
    }

    public RecordArrDocumentAcknowledgementResponse CreateDocumentAcknowledgement(string tenantId, string controlledDocumentId, string versionId, string personId, string? attestationText, DateTimeOffset? dueAt)
    {
        lock (_gate)
        {
            if (!ControlledDocumentBelongsToTenant(controlledDocumentId, tenantId))
            {
                throw new InvalidOperationException($"Controlled document {controlledDocumentId} not found.");
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("Acknowledgement personId is required.");
            }
            if (dueAt.HasValue && dueAt <= DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException("Acknowledgement dueAt must be in the future when provided.");
            }
            var controlledDocument = _controlledDocuments.First(document => string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));
            EnsureRecordNotUnderActiveLegalHold(controlledDocument.RecordId, "document_acknowledgement.created", personId);
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
            PersistDocumentAcknowledgement(tenantId, acknowledgement);
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "acknowledgement_requested",
                    personId,
                    $"Acknowledgement requested for version {versionId}."));
            return acknowledgement;
        }
    }

    public RecordArrDocumentAcknowledgementResponse CompleteDocumentAcknowledgement(string tenantId, string acknowledgementId, string? signatureRecordRef)
    {
        lock (_gate)
        {
            var index = _documentAcknowledgements.FindIndex(acknowledgement =>
                string.Equals(acknowledgement.AcknowledgementId, acknowledgementId, StringComparison.OrdinalIgnoreCase) &&
                ControlledDocumentBelongsToTenant(acknowledgement.ControlledDocumentId, tenantId));
            if (index < 0)
            {
                throw new InvalidOperationException($"Document acknowledgement {acknowledgementId} not found.");
            }

            var current = _documentAcknowledgements[index];
            var controlledDocument = _controlledDocuments.First(document => string.Equals(document.ControlledDocumentId, current.ControlledDocumentId, StringComparison.OrdinalIgnoreCase));
            EnsureRecordNotUnderActiveLegalHold(controlledDocument.RecordId, "document_acknowledgement.completed", current.PersonId);
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
            PersistDocumentAcknowledgement(tenantId, updated);
            MarkRelatedDocumentDistributionsAcknowledged(tenantId, updated);
            AppendControlledDocumentAuditTrail(
                current.ControlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "acknowledged",
                    "system",
                    $"Acknowledgement {acknowledgementId} completed."));
            return updated;
        }
    }

    private void MarkRelatedDocumentDistributionsAcknowledged(string tenantId, RecordArrDocumentAcknowledgementResponse acknowledgement)
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
            PersistDocumentDistribution(tenantId, _documentDistributions[i]);
            AppendControlledDocumentAuditTrail(
                acknowledgement.ControlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "distribution_acknowledged",
                    acknowledgement.PersonId,
                    $"Distribution {distribution.DistributionId} acknowledged."));
        }
    }

    private RecordArrDocumentDistributionResponse UpdateDocumentDistributionStatus(string tenantId, string distributionId, string status, string actorPersonId, string reason)
    {
        var index = _documentDistributions.FindIndex(distribution =>
            string.Equals(distribution.DistributionId, distributionId, StringComparison.OrdinalIgnoreCase) &&
            ControlledDocumentBelongsToTenant(distribution.ControlledDocumentId, tenantId));
        if (index < 0)
        {
            throw new InvalidOperationException($"Document distribution {distributionId} not found.");
        }

        var current = _documentDistributions[index];
        var controlledDocument = _controlledDocuments.First(document => string.Equals(document.ControlledDocumentId, current.ControlledDocumentId, StringComparison.OrdinalIgnoreCase));
        EnsureRecordNotUnderActiveLegalHold(controlledDocument.RecordId, $"document_distribution.{status}", actorPersonId);

        var normalizedStatus = NormalizeRecordArrEnum(
            status,
            nameof(status),
            "pending",
            "distributed",
            "acknowledged",
            "expired",
            "revoked");
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
        PersistDocumentDistribution(tenantId, updated);
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

    public IReadOnlyList<RecordArrAccessPolicyResponse> GetAccessPolicies(string tenantId)
    {
        lock (_gate)
        {
            return _accessPolicies
                .Where(policy => RecordBelongsToTenant(policy.RecordId, tenantId))
                .ToArray();
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
        => CreateAccessPolicy(
            ResolveRecordTenantId(recordId),
            recordId,
            policyType,
            status,
            readRules,
            writeRules,
            downloadRules,
            shareRules,
            exportRules,
            purgeRules,
            createdByPersonId);

    public RecordArrAccessPolicyResponse CreateAccessPolicy(
        string tenantId,
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
            if (!RecordBelongsToTenant(recordId, tenantId))
            {
                throw new InvalidOperationException($"Record {recordId} not found.");
            }
            EnsureRecordNotUnderActiveLegalHold(recordId, "access_policy.created", createdByPersonId);

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
            PersistAccessPolicy(tenantId, policy);
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
        => UpdateAccessPolicy(
            ResolveRecordTenantId(recordId),
            accessPolicyId,
            recordId,
            policyType,
            status,
            readRules,
            writeRules,
            downloadRules,
            shareRules,
            exportRules,
            purgeRules,
            updatedByPersonId);

    public RecordArrAccessPolicyResponse UpdateAccessPolicy(
        string tenantId,
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
            if (!RecordBelongsToTenant(recordId, tenantId))
            {
                throw new InvalidOperationException($"Record {recordId} not found.");
            }

            var index = _accessPolicies.FindIndex(policy =>
                string.Equals(policy.AccessPolicyId, accessPolicyId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(policy.RecordId, tenantId));
            if (index < 0)
            {
                throw new InvalidOperationException($"Access policy {accessPolicyId} not found.");
            }
            EnsureRecordNotUnderActiveLegalHold(recordId, "access_policy.updated", updatedByPersonId);

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
            PersistAccessPolicy(tenantId, updated);
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

    public IReadOnlyList<RecordArrAccessGrantResponse> GetAccessGrants(string tenantId)
    {
        lock (_gate)
        {
            return _accessGrants
                .Where(grant => RecordBelongsToTenant(grant.RecordId, tenantId))
                .ToArray();
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

                if (RecordHasActiveLegalHold(current.RecordId, "access_grant.expired", current.GrantedByPersonId))
                {
                    continue;
                }

                _accessGrants[i] = current with
                {
                    Status = "expired",
                    RevokedAt = current.ExpiresAt,
                    RevokeReason = $"Expired at {current.ExpiresAt:O}"
                };
                PersistAccessGrant(ResolveRecordTenantId(current.RecordId), _accessGrants[i]);
                AddAccessLog(current.RecordId, "access_grant.expired", "allowed", current.GrantedByPersonId, null, null, null, null, "grant-expired");
            }

            return _accessGrants.ToArray();
        }
    }

    public IReadOnlyList<RecordArrAccessGrantResponse> RefreshAccessGrants(string tenantId)
    {
        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            for (var i = 0; i < _accessGrants.Count; i++)
            {
                var current = _accessGrants[i];
                if (!RecordBelongsToTenant(current.RecordId, tenantId) ||
                    !string.Equals(current.Status, "active", StringComparison.OrdinalIgnoreCase) ||
                    !current.ExpiresAt.HasValue ||
                    current.ExpiresAt > now)
                {
                    continue;
                }

                if (RecordHasActiveLegalHold(current.RecordId, "access_grant.expired", current.GrantedByPersonId))
                {
                    continue;
                }

                _accessGrants[i] = current with
                {
                    Status = "expired",
                    RevokedAt = current.ExpiresAt,
                    RevokeReason = $"Expired at {current.ExpiresAt:O}"
                };
                PersistAccessGrant(tenantId, _accessGrants[i]);
                AddAccessLog(current.RecordId, "access_grant.expired", "allowed", current.GrantedByPersonId, null, null, null, null, "grant-expired");
            }

            return _accessGrants
                .Where(grant => RecordBelongsToTenant(grant.RecordId, tenantId))
                .ToArray();
        }
    }

    public RecordArrAccessGrantResponse CreateAccessGrant(string recordId, string granteeType, string granteeRef, string permission, string grantedByPersonId, DateTimeOffset? expiresAt)
        => CreateAccessGrant(ResolveRecordTenantId(recordId), recordId, granteeType, granteeRef, permission, grantedByPersonId, expiresAt);

    public RecordArrAccessGrantResponse CreateAccessGrant(string tenantId, string recordId, string granteeType, string granteeRef, string permission, string grantedByPersonId, DateTimeOffset? expiresAt)
    {
        lock (_gate)
        {
            if (!RecordBelongsToTenant(recordId, tenantId))
            {
                throw new InvalidOperationException($"Record {recordId} not found.");
            }
            EnsureRecordNotUnderActiveLegalHold(recordId, "access_grant.created", grantedByPersonId);

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
            PersistAccessGrant(tenantId, grant);
            AddAccessLog(recordId, "access_grant.created", "allowed", grantedByPersonId, null, null, null, null, $"{normalizedGranteeType}:{normalizedPermission}");
            return grant;
        }
    }

    public RecordArrAccessGrantResponse RevokeAccessGrant(string accessGrantId, string revokedByPersonId, string? revokeReason)
        => RevokeAccessGrant(ResolveAccessGrantTenantId(accessGrantId), accessGrantId, revokedByPersonId, revokeReason);

    public RecordArrAccessGrantResponse RevokeAccessGrant(string tenantId, string accessGrantId, string revokedByPersonId, string? revokeReason)
    {
        lock (_gate)
        {
            var index = _accessGrants.FindIndex(grant =>
                string.Equals(grant.AccessGrantId, accessGrantId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(grant.RecordId, tenantId));
            if (index < 0)
            {
                throw new InvalidOperationException($"Access grant {accessGrantId} not found.");
            }

            var current = _accessGrants[index];
            EnsureRecordNotUnderActiveLegalHold(current.RecordId, "access_grant.revoked", revokedByPersonId);
            var updated = current with
            {
                Status = "revoked",
                RevokedAt = DateTimeOffset.UtcNow,
                RevokeReason = revokeReason ?? $"Revoked by {revokedByPersonId}"
            };
            _accessGrants[index] = updated;
            PersistAccessGrant(tenantId, updated);
            AddAccessLog(current.RecordId, "access_grant.revoked", "allowed", revokedByPersonId, null, null, null, null, "grant-revoked");
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

    public IReadOnlyList<RecordArrExternalShareResponse> GetExternalShares(string tenantId)
    {
        lock (_gate)
        {
            return _externalShares
                .Where(share => RecordBelongsToTenant(share.RecordId, tenantId))
                .OrderByDescending(share => share.CreatedAt)
                .ToArray();
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

                if (RecordHasActiveLegalHold(current.RecordId, "external_share.expired", current.CreatedByPersonId, current.ExternalShareId))
                {
                    continue;
                }

                _externalShares[i] = current with
                {
                    Status = "expired"
                };
                PersistExternalShare(ResolveRecordTenantId(current.RecordId), _externalShares[i]);
                AddAccessLog(current.RecordId, "external_share.expired", "allowed", current.CreatedByPersonId, null, current.ExternalShareId, null, null, "share-expired");
            }

            return _externalShares.OrderByDescending(share => share.CreatedAt).ToArray();
        }
    }

    public IReadOnlyList<RecordArrExternalShareResponse> RefreshExternalShares(string tenantId)
    {
        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;
            for (var i = 0; i < _externalShares.Count; i++)
            {
                var current = _externalShares[i];
                if (!RecordBelongsToTenant(current.RecordId, tenantId) ||
                    string.Equals(current.Status, "revoked", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(current.Status, "expired", StringComparison.OrdinalIgnoreCase) ||
                    !current.ExpiresAt.HasValue ||
                    current.ExpiresAt > now)
                {
                    continue;
                }

                if (RecordHasActiveLegalHold(current.RecordId, "external_share.expired", current.CreatedByPersonId, current.ExternalShareId))
                {
                    continue;
                }

                _externalShares[i] = current with
                {
                    Status = "expired"
                };
                PersistExternalShare(tenantId, _externalShares[i]);
                AddAccessLog(current.RecordId, "external_share.expired", "allowed", current.CreatedByPersonId, null, current.ExternalShareId, null, null, "share-expired");
            }

            return _externalShares
                .Where(share => RecordBelongsToTenant(share.RecordId, tenantId))
                .OrderByDescending(share => share.CreatedAt)
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrRedactionResponse> GetRedactions()
    {
        lock (_gate)
        {
            return _redactions.OrderByDescending(redaction => redaction.RedactedAt).ToArray();
        }
    }

    public IReadOnlyList<RecordArrRedactionResponse> GetRedactions(string tenantId)
    {
        lock (_gate)
        {
            return _redactions
                .Where(redaction =>
                    RecordBelongsToTenant(redaction.SourceRecordId, tenantId) ||
                    RecordBelongsToTenant(redaction.RedactedRecordId, tenantId))
                .OrderByDescending(redaction => redaction.RedactedAt)
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrDisposalReviewResponse> GetDisposalReviews(string tenantId)
    {
        lock (_gate)
        {
            return _disposalReviews
                .Where(review => RecordBelongsToTenant(review.RecordId, tenantId))
                .ToArray();
        }
    }

    public IReadOnlyList<RecordArrDestructionCertificateResponse> GetDestructionCertificates(string tenantId, string? recordId = null)
    {
        lock (_gate)
        {
            return _destructionCertificates
                .Where(certificate => RecordBelongsToTenant(certificate.RecordId, tenantId))
                .Where(certificate => string.IsNullOrWhiteSpace(recordId) || string.Equals(certificate.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(certificate => certificate.ExecutedAt)
                .ToArray();
        }
    }

    public RecordArrDisposalReviewResponse CompleteDisposalReview(string tenantId, string disposalReviewId, string status, string? reviewedByPersonId, string? decisionReason)
    {
        lock (_gate)
        {
            var index = _disposalReviews.FindIndex(review =>
                string.Equals(review.DisposalReviewId, disposalReviewId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(review.RecordId, tenantId));
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
            PersistDisposalReview(tenantId, updated);
            AddAccessLog(updated.RecordId, "disposal_review.completed", "allowed", reviewedByPersonId, null, null, null, null, normalizedStatus);
            return updated;
        }
    }

    private void ApplyDisposalReviewOutcome(string tenantId, RecordArrDisposalReviewResponse review)
    {
        var retentionIndex = _retentionStatuses.FindIndex(status =>
            string.Equals(status.RetentionStatusId, review.RetentionStatusRef, StringComparison.OrdinalIgnoreCase) &&
            RecordBelongsToTenant(status.RecordId, tenantId));
        if (retentionIndex < 0)
        {
            return;
        }

        var record = _records.FirstOrDefault(candidate => string.Equals(candidate.RecordId, review.RecordId, StringComparison.OrdinalIgnoreCase));
        var activeHold = _legalHolds.FirstOrDefault(hold =>
            string.Equals(hold.Status, "active", StringComparison.OrdinalIgnoreCase) &&
            LegalHoldBelongsToTenant(hold, tenantId) &&
            (hold.RecordRefs.Any(recordRef => string.Equals(recordRef, review.RecordId, StringComparison.OrdinalIgnoreCase)) ||
             record is not null && hold.ScopeRules.Any(scopeRule => IsRecordMatchedByLegalHoldScopeRule(record, ParseLegalHoldScopeRule(scopeRule)))));

        if (activeHold is not null)
        {
            _retentionStatuses[retentionIndex] = _retentionStatuses[retentionIndex] with
            {
                Status = "blocked_by_legal_hold"
            };
            PersistRetentionStatus(tenantId, _retentionStatuses[retentionIndex]);
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
        PersistRetentionStatus(tenantId, _retentionStatuses[retentionIndex]);

        if (review.ProposedAction.Trim().Equals("archive", StringComparison.OrdinalIgnoreCase))
        {
            ArchiveRecord(review.RecordId, actorPersonId);
        }
        else if (review.ProposedAction.Trim().Equals("purge", StringComparison.OrdinalIgnoreCase))
        {
            PurgeRecord(review.RecordId, actorPersonId);
            CreateDestructionCertificateForReview(tenantId, review, _retentionStatuses[retentionIndex], actorPersonId);
        }
    }

    private RecordArrDestructionCertificateResponse CreateDestructionCertificateForReview(
        string tenantId,
        RecordArrDisposalReviewResponse review,
        RecordArrRetentionStatusResponse retentionStatus,
        string executedByPersonId)
    {
        var existing = _destructionCertificates.FirstOrDefault(certificate =>
            string.Equals(certificate.DisposalReviewRef, review.DisposalReviewId, StringComparison.OrdinalIgnoreCase) &&
            RecordBelongsToTenant(certificate.RecordId, tenantId));
        if (existing is not null)
        {
            return existing;
        }

        var deletedFiles = _files
            .Where(file =>
                string.Equals(file.RecordId, review.RecordId, StringComparison.OrdinalIgnoreCase) &&
                file.DeletedAt.HasValue)
            .OrderBy(file => file.FileId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var deletedFileRefs = deletedFiles
            .Select(file => file.FileId)
            .ToArray();
        var tombstoneRefs = deletedFiles
            .Select(file => $"{file.FileId}:{file.StorageProvider}:{file.StorageKey}:{file.ChecksumSha256}")
            .ToArray();
        var executedAt = DateTimeOffset.UtcNow;
        var certificateId = $"dcert-{Guid.NewGuid():N}"[..14];
        var certificate = new RecordArrDestructionCertificateResponse(
            certificateId,
            $"DCERT-{executedAt:yyMMdd-HHmmss}-{certificateId[^6..]}",
            review.RecordId,
            retentionStatus.RetentionStatusId,
            review.DisposalReviewId,
            "purge",
            "completed",
            review.RequestedAt,
            executedAt,
            executedByPersonId,
            deletedFileRefs,
            tombstoneRefs,
            string.Empty,
            null);
        certificate = certificate with
        {
            CertificateHash = ComputeDestructionCertificateHash(tenantId, certificate)
        };

        _destructionCertificates.Add(certificate);
        PersistDestructionCertificate(tenantId, certificate);
        AddAccessLog(review.RecordId, "disposition.certificate.created", "allowed", executedByPersonId, null, null, null, null, certificate.DestructionCertificateId);
        return certificate;
    }

    private static string ComputeDestructionCertificateHash(string tenantId, RecordArrDestructionCertificateResponse certificate)
        => ComputeChecksum(string.Join(
            "|",
            tenantId,
            certificate.DestructionCertificateId,
            certificate.CertificateNumber,
            certificate.RecordId,
            certificate.RetentionStatusRef,
            certificate.DisposalReviewRef,
            certificate.DispositionAction,
            certificate.Status,
            certificate.RequestedAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            certificate.ExecutedAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            certificate.ExecutedByPersonId,
            string.Join(",", certificate.DeletedFileRefs.OrderBy(value => value, StringComparer.OrdinalIgnoreCase)),
            string.Join(",", certificate.TombstoneRefs.OrderBy(value => value, StringComparer.OrdinalIgnoreCase)),
            certificate.FailureReason ?? string.Empty));

    public RecordArrControlledDocumentResponse? GetControlledDocument(string tenantId, string controlledDocumentId)
    {
        lock (_gate)
        {
            return _controlledDocuments.FirstOrDefault(document =>
                string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(document.RecordId, tenantId));
        }
    }

    public RecordArrControlledDocumentResponse CreateControlledDocument(
        string tenantId,
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
            var backingRecord = _records.FirstOrDefault(record => RecordBelongsToTenant(record.RecordId, tenantId))
                ?? throw new InvalidOperationException("Controlled document creation requires an existing tenant record.");
            EnsureRecordNotUnderActiveLegalHold(backingRecord.RecordId, "controlled_document.created", ownerPersonId);
            var document = new RecordArrControlledDocumentResponse(
                $"doc-{Guid.NewGuid():N}"[..12],
                $"DOC-{DateTimeOffset.UtcNow:yyMMdd-HHmmss}",
                backingRecord.RecordId,
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
            PersistControlledDocument(tenantId, document);
            return document;
        }
    }

    public RecordArrControlledDocumentVersionResponse CreateDocumentVersion(string tenantId, string controlledDocumentId, string fileName, string createdByPersonId, string? changeSummary)
    {
        lock (_gate)
        {
            if (!ControlledDocumentBelongsToTenant(controlledDocumentId, tenantId))
            {
                throw new InvalidOperationException($"Controlled document {controlledDocumentId} not found.");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new InvalidOperationException("Document version fileName is required.");
            }

            if (string.IsNullOrWhiteSpace(createdByPersonId))
            {
                throw new InvalidOperationException("Document version createdByPersonId is required.");
            }

            var controlledDocument = _controlledDocuments.First(document => string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase));
            EnsureRecordNotUnderActiveLegalHold(controlledDocument.RecordId, "controlled_document.version_created", createdByPersonId);
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
                _documentVersions.Count(version => string.Equals(version.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase)) + 1,
                $"v{_documentVersions.Count(version => string.Equals(version.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase)) + 1}",
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
            PersistFile(file);
            PersistRecord(_records.First(record => string.Equals(record.RecordId, controlledDocument.RecordId, StringComparison.OrdinalIgnoreCase)));
            PersistDocumentVersion(tenantId, version);
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

    public RecordArrControlledDocumentVersionResponse PromoteDocumentVersion(string tenantId, string controlledDocumentId, string versionId, string approvedByPersonId, DateTimeOffset? effectiveAt)
    {
        lock (_gate)
        {
            if (!ControlledDocumentBelongsToTenant(controlledDocumentId, tenantId))
            {
                throw new InvalidOperationException($"Controlled document {controlledDocumentId} not found.");
            }

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
            var document = _controlledDocuments[documentIndex];
            EnsureRecordNotUnderActiveLegalHold(document.RecordId, "controlled_document.version_promoted", approvedByPersonId);
            var promoted = _documentVersions[versionIndex] with
            {
                Status = "effective",
                ApprovedAt = now,
                ApprovedByPersonId = approvedByPersonId,
                EffectiveAt = effectiveAt ?? now
            };
            _documentVersions[versionIndex] = promoted;
            PersistDocumentVersion(tenantId, promoted);

            var previousVersionId = document.CurrentVersionId;
            var updatedDocument = document with
            {
                Status = "effective",
                CurrentVersionId = versionId,
                EffectiveAt = promoted.EffectiveAt,
                NextReviewAt = promoted.EffectiveAt?.AddDays(document.ReviewIntervalDays)
            };
            _controlledDocuments[documentIndex] = updatedDocument;
            PersistControlledDocument(tenantId, updatedDocument);

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
                    PersistDocumentVersion(tenantId, _documentVersions[previousIndex]);
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

    public IReadOnlyList<RecordArrControlledDocumentResponse> RefreshControlledDocumentWorkflows(string tenantId)
    {
        lock (_gate)
        {
            var now = DateTimeOffset.UtcNow;

            for (var i = 0; i < _controlledDocuments.Count; i++)
            {
                var current = _controlledDocuments[i];
                if (!RecordBelongsToTenant(current.RecordId, tenantId))
                {
                    continue;
                }

                if (!string.Equals(current.Status, "effective", StringComparison.OrdinalIgnoreCase) ||
                    !current.NextReviewAt.HasValue ||
                    current.NextReviewAt > now)
                {
                    continue;
                }

                if (RecordHasActiveLegalHold(current.RecordId, "controlled_document.periodic_review_due", "system"))
                {
                    continue;
                }

                var updated = current with { Status = "review" };
                _controlledDocuments[i] = updated;
                PersistControlledDocument(tenantId, updated);
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
                if (!ControlledDocumentBelongsToTenant(current.ControlledDocumentId, tenantId))
                {
                    continue;
                }

                if (!string.Equals(current.Status, "pending", StringComparison.OrdinalIgnoreCase) ||
                    !current.DueAt.HasValue ||
                    current.DueAt > now)
                {
                    continue;
                }

                var controlledDocument = _controlledDocuments.FirstOrDefault(document =>
                    string.Equals(document.ControlledDocumentId, current.ControlledDocumentId, StringComparison.OrdinalIgnoreCase));
                if (controlledDocument is not null &&
                    RecordHasActiveLegalHold(controlledDocument.RecordId, "document_acknowledgement.overdue", "system"))
                {
                    continue;
                }

                var updated = current with { Status = "overdue" };
                _documentAcknowledgements[i] = updated;
                PersistDocumentAcknowledgement(tenantId, updated);
                AppendControlledDocumentAuditTrail(
                    current.ControlledDocumentId,
                    CreateControlledDocumentAuditTrailEntry(
                        "acknowledgement_overdue",
                        "system",
                        $"Acknowledgement {current.AcknowledgementId} became overdue at {current.DueAt:O}."));
            }

            return _controlledDocuments
                .Where(document => RecordBelongsToTenant(document.RecordId, tenantId))
                .OrderBy(document => document.DocumentNumber)
                .ToArray();
        }
    }

    public RecordArrControlledDocumentResponse UpdateControlledDocumentStatus(string tenantId, string controlledDocumentId, string status, string updatedByPersonId)
    {
        lock (_gate)
        {
            var index = _controlledDocuments.FindIndex(document =>
                string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(document.RecordId, tenantId));
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
            EnsureRecordNotUnderActiveLegalHold(_controlledDocuments[index].RecordId, "controlled_document.status_updated", updatedByPersonId);
            var updated = _controlledDocuments[index] with
            {
                Status = normalizedStatus,
                NextReviewAt = normalizedStatus is "archived" or "obsolete" ? null : _controlledDocuments[index].NextReviewAt
            };
            _controlledDocuments[index] = updated;
            if (normalizedStatus is "archived" or "obsolete")
            {
                ArchiveControlledDocumentVersions(tenantId, controlledDocumentId);
            }
            PersistControlledDocument(tenantId, updated);
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    normalizedStatus,
                    updatedByPersonId,
                    $"Controlled document marked as {normalizedStatus}."));
            return updated;
        }
    }

    private void ArchiveControlledDocumentVersions(string tenantId, string controlledDocumentId)
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
            PersistDocumentVersion(tenantId, _documentVersions[i]);
        }
    }

    public RecordArrControlledDocumentResponse SupersedeControlledDocument(string tenantId, string controlledDocumentId, string supersededByDocumentRef, string supersededByPersonId)
    {
        lock (_gate)
        {
            var sourceIndex = _controlledDocuments.FindIndex(document =>
                string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(document.RecordId, tenantId));
            if (sourceIndex < 0)
            {
                throw new InvalidOperationException($"Controlled document {controlledDocumentId} not found.");
            }

            var replacementIndex = _controlledDocuments.FindIndex(document =>
                string.Equals(document.ControlledDocumentId, supersededByDocumentRef, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(document.RecordId, tenantId));
            if (replacementIndex < 0)
            {
                throw new InvalidOperationException($"Replacement controlled document {supersededByDocumentRef} not found.");
            }

            var source = _controlledDocuments[sourceIndex];
            var replacement = _controlledDocuments[replacementIndex];
            EnsureRecordNotUnderActiveLegalHold(source.RecordId, "controlled_document.superseded", supersededByPersonId);
            EnsureRecordNotUnderActiveLegalHold(replacement.RecordId, "controlled_document.supersedes", supersededByPersonId);
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
            PersistControlledDocument(tenantId, updatedSource);
            PersistControlledDocument(tenantId, updatedReplacement);
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

    public RecordArrDocumentReviewResponse RequestDocumentReview(string tenantId, string controlledDocumentId, string versionId, string reviewType, string requestedByPersonId, string reviewerPersonId, DateTimeOffset? dueAt)
    {
        lock (_gate)
        {
            var documentIndex = _controlledDocuments.FindIndex(document =>
                string.Equals(document.ControlledDocumentId, controlledDocumentId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(document.RecordId, tenantId));
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
            EnsureRecordNotUnderActiveLegalHold(_controlledDocuments[documentIndex].RecordId, "document_review.requested", requestedByPersonId);
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
            PersistDocumentReview(tenantId, review);
            UpdateDocumentVersionStatus(tenantId, controlledDocumentId, versionId, "review");
            _controlledDocuments[documentIndex] = _controlledDocuments[documentIndex] with { Status = "review" };
            PersistControlledDocument(tenantId, _controlledDocuments[documentIndex]);
            AppendControlledDocumentAuditTrail(
                controlledDocumentId,
                CreateControlledDocumentAuditTrailEntry(
                    "submitted_for_review",
                    requestedByPersonId,
                    $"Requested {reviewType} review for version {versionId}."));
            return review;
        }
    }

    public RecordArrDocumentReviewResponse CompleteDocumentReview(string tenantId, string reviewId, string status, string? decisionReason, string? comments)
    {
        lock (_gate)
        {
            var index = _documentReviews.FindIndex(review =>
                string.Equals(review.DocumentReviewId, reviewId, StringComparison.OrdinalIgnoreCase) &&
                ControlledDocumentBelongsToTenant(review.ControlledDocumentId, tenantId));
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
            var controlledDocument = _controlledDocuments.First(document => string.Equals(document.ControlledDocumentId, current.ControlledDocumentId, StringComparison.OrdinalIgnoreCase));
            EnsureRecordNotUnderActiveLegalHold(controlledDocument.RecordId, "document_review.completed", current.ReviewerPersonId);
            var updated = current with
            {
                Status = normalizedStatus,
                ReviewedAt = DateTimeOffset.UtcNow,
                DecisionReason = decisionReason,
                Comments = comments
            };
            _documentReviews[index] = updated;
            PersistDocumentReview(tenantId, updated);

            var documentIndex = _controlledDocuments.FindIndex(document => string.Equals(document.ControlledDocumentId, current.ControlledDocumentId, StringComparison.OrdinalIgnoreCase));
            if (documentIndex >= 0)
            {
                var document = _controlledDocuments[documentIndex];
                var completedAt = updated.ReviewedAt ?? DateTimeOffset.UtcNow;
                if (normalizedStatus == "approved")
                {
                    UpdateDocumentVersionStatus(tenantId, current.ControlledDocumentId, current.VersionId, "approved");
                    _controlledDocuments[documentIndex] = document with
                    {
                        Status = "approved"
                    };
                    PersistControlledDocument(tenantId, _controlledDocuments[documentIndex]);
                }
                else if (normalizedStatus is "rejected" or "changes_requested")
                {
                    UpdateDocumentVersionStatus(tenantId, current.ControlledDocumentId, current.VersionId, normalizedStatus == "rejected" ? "rejected" : "review");
                    _controlledDocuments[documentIndex] = document with { Status = "review" };
                    PersistControlledDocument(tenantId, _controlledDocuments[documentIndex]);
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

    private void UpdateDocumentVersionStatus(string tenantId, string controlledDocumentId, string versionId, string status)
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
        PersistDocumentVersion(tenantId, _documentVersions[versionIndex]);
    }

    public RecordArrExternalShareResponse CreateExternalShare(string recordId, string recipientName, string recipientEmail, string sharePurpose, IEnumerable<string> allowedActions, string createdByPersonId)
        => CreateExternalShare(ResolveRecordTenantId(recordId), recordId, recipientName, recipientEmail, sharePurpose, allowedActions, createdByPersonId);

    public RecordArrExternalShareResponse CreateExternalShare(string tenantId, string recordId, string recipientName, string recipientEmail, string sharePurpose, IEnumerable<string> allowedActions, string createdByPersonId)
    {
        lock (_gate)
        {
            if (!RecordBelongsToTenant(recordId, tenantId))
            {
                throw new InvalidOperationException($"Record {recordId} not found.");
            }
            EnsureRecordNotUnderActiveLegalHold(recordId, "external_share.created", createdByPersonId);

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
            PersistExternalShare(tenantId, share);
            return share;
        }
    }

    public RecordArrExternalShareResponse RevokeExternalShare(string shareId, string revokedByPersonId)
        => RevokeExternalShare(ResolveExternalShareTenantId(shareId), shareId, revokedByPersonId);

    public RecordArrExternalShareResponse RevokeExternalShare(string tenantId, string shareId, string revokedByPersonId)
    {
        lock (_gate)
        {
            var index = _externalShares.FindIndex(share =>
                string.Equals(share.ExternalShareId, shareId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(share.RecordId, tenantId));
            if (index < 0)
            {
                throw new InvalidOperationException($"External share {shareId} not found.");
            }

            var current = _externalShares[index];
            EnsureRecordNotUnderActiveLegalHold(current.RecordId, "external_share.revoked", revokedByPersonId);
            var updated = current with
            {
                Status = "revoked",
                RevokedAt = DateTimeOffset.UtcNow,
                RevokedByPersonId = revokedByPersonId
            };
            _externalShares[index] = updated;
            AddAccessLog(current.RecordId, "external_share.revoked", "allowed", revokedByPersonId, null, current.ExternalShareId, null, null, "external-share-revoked");
            PersistExternalShare(tenantId, updated);
            return updated;
        }
    }

    public RecordArrExternalShareResponse ExpireExternalShare(string shareId, string expiredByPersonId)
        => ExpireExternalShare(ResolveExternalShareTenantId(shareId), shareId, expiredByPersonId);

    public RecordArrExternalShareResponse ExpireExternalShare(string tenantId, string shareId, string expiredByPersonId)
    {
        lock (_gate)
        {
            var index = _externalShares.FindIndex(share =>
                string.Equals(share.ExternalShareId, shareId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(share.RecordId, tenantId));
            if (index < 0)
            {
                throw new InvalidOperationException($"External share {shareId} not found.");
            }

            var current = _externalShares[index];
            EnsureRecordNotUnderActiveLegalHold(current.RecordId, "external_share.expired", expiredByPersonId);
            var updated = current with
            {
                Status = "expired",
                RevokedAt = DateTimeOffset.UtcNow,
                RevokedByPersonId = expiredByPersonId
            };
            _externalShares[index] = updated;
            AddAccessLog(current.RecordId, "external_share.expired", "allowed", expiredByPersonId, null, current.ExternalShareId, null, null, "external-share-expired");
            PersistExternalShare(tenantId, updated);
            return updated;
        }
    }

    public RecordArrExternalShareResponse RecordExternalShareAccess(string shareId, string accessedByPersonId, string accessAction, string? sourceIp, string? userAgent)
        => RecordExternalShareAccess(ResolveExternalShareTenantId(shareId), shareId, accessedByPersonId, accessAction, sourceIp, userAgent);

    public RecordArrExternalShareResponse RecordExternalShareAccess(string tenantId, string shareId, string accessedByPersonId, string accessAction, string? sourceIp, string? userAgent)
    {
        lock (_gate)
        {
            var index = _externalShares.FindIndex(share =>
                string.Equals(share.ExternalShareId, shareId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(share.RecordId, tenantId));
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
                if (RecordHasActiveLegalHold(current.RecordId, "external_share.expired", current.CreatedByPersonId, current.ExternalShareId))
                {
                    AddAccessLog(current.RecordId, "external_share.accessed", "denied", accessedByPersonId, null, current.ExternalShareId, sourceIp, userAgent, "blocked_by_legal_hold");
                    throw new InvalidOperationException($"External share {shareId} is blocked by legal hold.");
                }

                var expired = current with
                {
                    Status = "expired"
                };
                _externalShares[index] = expired;
                PersistExternalShare(tenantId, expired);
                AddAccessLog(current.RecordId, "external_share.expired", "allowed", current.CreatedByPersonId, null, current.ExternalShareId, sourceIp, userAgent, "share-expired");
                AddAccessLog(current.RecordId, "external_share.accessed", "denied", accessedByPersonId, null, current.ExternalShareId, sourceIp, userAgent, "external-share-expired");
                throw new InvalidOperationException($"External share {shareId} has expired.");
            }

            var normalizedAccessAction = NormalizeRecordArrEnum(
                accessAction,
                nameof(accessAction),
                "view",
                "download",
                "upload",
                "sign");

            if (!current.AllowedActions.Contains(normalizedAccessAction, StringComparer.OrdinalIgnoreCase))
            {
                AddAccessLog(current.RecordId, "external_share.accessed", "denied", accessedByPersonId, null, current.ExternalShareId, sourceIp, userAgent, "external-share-action-not-allowed");
                throw new InvalidOperationException($"External share {shareId} does not allow {normalizedAccessAction}.");
            }

            var accessPolicy = _accessPolicies.FirstOrDefault(policy =>
                string.Equals(policy.RecordId, current.RecordId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(policy.Status, "active", StringComparison.OrdinalIgnoreCase));

            if (accessPolicy is not null && !IsExternalShareActionAllowedByPolicy(accessPolicy, normalizedAccessAction))
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
            AddAccessLog(current.RecordId, "external_share.accessed", "allowed", accessedByPersonId, null, current.ExternalShareId, sourceIp, userAgent, normalizedAccessAction);
            PersistExternalShare(tenantId, updated);
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

    private static string ComputeSignatureEvidenceHash(
        string tenantId,
        string recordId,
        string signaturePurpose,
        string? signerPersonId,
        string? signerExternalName,
        string? signerTitle,
        string attestationText,
        string signatureFileRef,
        string signatureFileChecksumSha256,
        DateTimeOffset signedAt,
        string capturedByPersonId,
        string sourceProduct,
        string sourceObjectRef,
        string? providerName,
        string? providerEnvelopeRef,
        string? certificateFingerprintSha256)
        => ComputeChecksum(string.Join(
            "|",
            tenantId,
            recordId,
            signaturePurpose,
            signerPersonId ?? string.Empty,
            signerExternalName ?? string.Empty,
            signerTitle ?? string.Empty,
            attestationText,
            signatureFileRef,
            signatureFileChecksumSha256,
            signedAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            capturedByPersonId,
            sourceProduct,
            sourceObjectRef,
            providerName ?? string.Empty,
            providerEnvelopeRef ?? string.Empty,
            certificateFingerprintSha256 ?? string.Empty));

    private static string ComputeSignatureProviderCallbackEvidenceHash(
        RecordArrSignatureRecordResponse signature,
        string providerName,
        string providerEnvelopeRef,
        string providerCallbackStatus,
        string providerCallbackRef,
        DateTimeOffset providerCallbackReceivedAt,
        string? certificateFingerprintSha256,
        string? trustTimestampAuthorityRef,
        string? longTermValidationStatus)
        => ComputeChecksum(string.Join(
            "|",
            signature.TenantId,
            signature.RecordId,
            signature.SignatureRecordId,
            signature.SignatureEvidenceHash,
            providerName,
            providerEnvelopeRef,
            providerCallbackStatus,
            providerCallbackRef,
            providerCallbackReceivedAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            certificateFingerprintSha256 ?? string.Empty,
            trustTimestampAuthorityRef ?? string.Empty,
            longTermValidationStatus ?? string.Empty));

    private static string ComputeSignatureTrustServiceJobSubmissionHash(
        RecordArrSignatureRecordResponse signature,
        string providerName,
        string providerEnvelopeRef,
        DateTimeOffset requestedAt)
        => ComputeChecksum(string.Join(
            "|",
            signature.TenantId,
            signature.RecordId,
            signature.SignatureRecordId,
            signature.SignatureEvidenceHash,
            signature.CertificateFingerprintSha256 ?? string.Empty,
            providerName,
            providerEnvelopeRef,
            requestedAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture)));

    private static string ComputeRedactionPackageHash(
        string tenantId,
        string sourceRecordId,
        string redactedRecordId,
        string redactionReason,
        string redactedByPersonId,
        DateTimeOffset redactedAt,
        IReadOnlyList<string> redactionRules,
        string? sourceFileRef,
        string? redactedFileRef)
        => ComputeChecksum(string.Join(
            "|",
            tenantId,
            sourceRecordId,
            redactedRecordId,
            redactionReason,
            redactedByPersonId,
            redactedAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            string.Join(",", redactionRules.Select(rule => rule.Trim()).Where(rule => rule.Length > 0).OrderBy(rule => rule, StringComparer.OrdinalIgnoreCase)),
            sourceFileRef ?? string.Empty,
            redactedFileRef ?? string.Empty));

    private static string ComputeRedactionProviderEvidenceHash(
        RecordArrRedactionResponse redaction,
        string providerName,
        string providerJobRef,
        string providerCallbackStatus,
        string providerCallbackRef,
        DateTimeOffset providerCallbackReceivedAt,
        string providerPackageHash)
        => ComputeChecksum(string.Join(
            "|",
            redaction.SourceRecordId,
            redaction.RedactedRecordId,
            redaction.RedactionId,
            redaction.RedactionPackageHash,
            providerName,
            providerJobRef,
            providerCallbackStatus,
            providerCallbackRef,
            providerCallbackReceivedAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            providerPackageHash));

    private static string ComputeRedactionProviderJobSubmissionHash(
        RecordArrRedactionResponse redaction,
        string providerName,
        string providerJobRef,
        DateTimeOffset requestedAt)
        => ComputeChecksum(string.Join(
            "|",
            redaction.SourceRecordId,
            redaction.RedactedRecordId,
            redaction.RedactionId,
            redaction.RedactionPackageHash,
            providerName,
            providerJobRef,
            requestedAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            string.Join(",", redaction.RedactionRules)));

    private static string ComputeRedactionOverlayReviewHash(
        RecordArrRedactionResponse redaction,
        string overlayReviewStatus,
        string reviewedByPersonId,
        DateTimeOffset reviewedAt,
        IReadOnlyList<string> overlayEvidenceRefs,
        IReadOnlyList<string> overlayIssueRefs)
        => ComputeChecksum(string.Join(
            "|",
            redaction.SourceRecordId,
            redaction.RedactedRecordId,
            redaction.RedactionId,
            redaction.RedactionPackageHash,
            overlayReviewStatus,
            reviewedByPersonId,
            reviewedAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            string.Join(",", overlayEvidenceRefs),
            string.Join(",", overlayIssueRefs)));

    private static string ComputeObjectStoreLifecycleEvidenceHash(
        RecordArrFileResponse file,
        string providerName,
        string policyRef,
        string retentionMode,
        DateTimeOffset retainUntil,
        string encryptionKeyRef,
        string evidenceRef,
        DateTimeOffset? requiredRetainUntil)
        => ComputeChecksum(string.Join(
            "|",
            file.TenantId,
            file.RecordId,
            file.FileId,
            file.StorageProvider,
            file.StorageKey,
            file.ChecksumSha256,
            providerName,
            policyRef,
            retentionMode,
            retainUntil.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            encryptionKeyRef,
            evidenceRef,
            requiredRetainUntil?.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty));

    private static string ComputeChecksum(string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private List<RecordArrEvidenceCoverageResponse> BuildEvidenceCoverage(string tenantId)
    {
        var coverage = new List<RecordArrEvidenceCoverageResponse>();
        var tenantMappings = _evidenceMappings
            .Where(mapping => string.Equals(ResolveRecordTenantId(mapping.RecordId), tenantId, StringComparison.OrdinalIgnoreCase));
        foreach (var group in tenantMappings.GroupBy(mapping =>
                     $"{mapping.SourceProduct}|{mapping.SourceObjectType}|{mapping.SourceObjectId}|{mapping.ComplianceRequirementRef}",
                     StringComparer.OrdinalIgnoreCase))
        {
            var recordRefs = group.Select(mapping => mapping.RecordId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var coverageTenantId = ResolveRecordTenantId(recordRefs.FirstOrDefault() ?? string.Empty);
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
                coverageTenantId,
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
        => CreateRedaction(ResolveRecordTenantId(sourceRecordId), sourceRecordId, redactedRecordId, redactionReason, redactedByPersonId, redactionRules);

    public RecordArrRedactionResponse CreateRedaction(string tenantId, string sourceRecordId, string redactedRecordId, string redactionReason, string redactedByPersonId, IEnumerable<string> redactionRules)
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
            if (!RecordBelongsToTenant(sourceRecordId, tenantId))
            {
                throw new InvalidOperationException($"Record {sourceRecordId} not found.");
            }

            var sourceRecord = RequireRecord(sourceRecordId);
            EnsureRecordNotUnderActiveLegalHold(sourceRecord.RecordId, "redaction.completed", redactedByPersonId);
            var normalizedRedactionReason = NormalizeRecordArrEnum(
                redactionReason,
                nameof(redactionReason),
                "privacy",
                "legal",
                "customer",
                "supplier",
                "internal",
                "security");
            if (redactionRules is null)
            {
                throw new InvalidOperationException("At least one redaction rule is required.");
            }

            var normalizedRedactionRules = redactionRules
                .Select(rule => rule?.Trim())
                .Where(rule => !string.IsNullOrWhiteSpace(rule))
                .Select(rule => rule!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(rule => rule, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (normalizedRedactionRules.Length == 0)
            {
                throw new InvalidOperationException("At least one redaction rule is required.");
            }

            var redactedRecord = CreateRedactedRecordCopy(tenantId, sourceRecord, redactedRecordId, normalizedRedactionReason, redactedByPersonId);
            var redactedAt = DateTimeOffset.UtcNow;
            var redactionPackageHash = ComputeRedactionPackageHash(
                tenantId,
                sourceRecord.RecordId,
                redactedRecordId,
                normalizedRedactionReason,
                redactedByPersonId,
                redactedAt,
                normalizedRedactionRules,
                sourceRecord.CurrentFileRef,
                redactedRecord.CurrentFileRef);
            var redaction = new RecordArrRedactionResponse(
                $"red-{Guid.NewGuid():N}"[..12],
                sourceRecord.RecordId,
                redactedRecordId,
                normalizedRedactionReason,
                "completed",
                redactedByPersonId,
                redactedAt,
                normalizedRedactionRules,
                ReviewStatus: "approved",
                ReviewedByPersonId: redactedByPersonId,
                ReviewedAt: redactedAt,
                ApprovalReason: "initial_redaction_review",
                RedactionPackageHash: redactionPackageHash,
                LockedAt: redactedAt);
            _redactions.Add(redaction);
            PersistRecord(redactedRecord);
            PersistRedaction(tenantId, redaction);
            AddAccessLog(sourceRecord.RecordId, "redaction.completed", "allowed", redactedByPersonId, null, null, null, null, normalizedRedactionReason);
            return redaction;
        }
    }

    public RecordArrRedactionProviderReconciliationResponse ReconcileRedactionProviderStatus(
        string tenantId,
        string redactionId,
        string reviewedByPersonId,
        string? providerName,
        string? providerJobRef,
        string? providerCallbackStatus,
        string? providerCallbackRef,
        string? providerPackageHash)
    {
        lock (_gate)
        {
            var index = _redactions.FindIndex(redaction =>
                string.Equals(redaction.RedactionId, redactionId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(redaction.SourceRecordId, tenantId));
            if (index < 0)
            {
                throw new InvalidOperationException($"Redaction {redactionId} not found.");
            }

            var current = _redactions[index];
            var normalizedProviderName = NormalizeRequiredEvidenceValue(providerName, nameof(providerName));
            var normalizedProviderJobRef = NormalizeRequiredEvidenceValue(providerJobRef, nameof(providerJobRef));
            var normalizedCallbackStatus = NormalizeRecordArrEnum(
                providerCallbackStatus ?? string.Empty,
                nameof(providerCallbackStatus),
                "completed",
                "rejected",
                "failed",
                "needs_review");
            var normalizedCallbackRef = NormalizeRequiredEvidenceValue(providerCallbackRef, nameof(providerCallbackRef));
            var normalizedProviderPackageHash = NormalizeRequiredEvidenceValue(providerPackageHash, nameof(providerPackageHash)).ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(current.RedactionPackageHash))
            {
                AddAccessLog(
                    current.SourceRecordId,
                    "redaction.provider_reconciled",
                    "denied",
                    reviewedByPersonId,
                    null,
                    null,
                    null,
                    null,
                    "redaction_package_hash_missing");
                throw new InvalidOperationException($"Redaction {redactionId} does not have locked package hash evidence.");
            }

            if (!string.Equals(current.RedactionPackageHash, normalizedProviderPackageHash, StringComparison.OrdinalIgnoreCase))
            {
                AddAccessLog(
                    current.SourceRecordId,
                    "redaction.provider_reconciled",
                    "denied",
                    reviewedByPersonId,
                    null,
                    null,
                    null,
                    null,
                    "provider_package_hash_mismatch");
                throw new InvalidOperationException($"Redaction {redactionId} provider package hash evidence does not match.");
            }

            var receivedAt = DateTimeOffset.UtcNow;
            var providerEvidenceHash = ComputeRedactionProviderEvidenceHash(
                current,
                normalizedProviderName,
                normalizedProviderJobRef,
                normalizedCallbackStatus,
                normalizedCallbackRef,
                receivedAt,
                normalizedProviderPackageHash);
            var providerReviewStatus = normalizedCallbackStatus == "completed"
                ? "provider_verified"
                : "provider_rejected";
            var providerFailureReason = normalizedCallbackStatus == "completed"
                ? null
                : $"provider_{normalizedCallbackStatus}";
            var updated = current with
            {
                ReviewedByPersonId = reviewedByPersonId,
                ReviewedAt = receivedAt,
                ApprovalReason = normalizedCallbackStatus == "completed" ? "provider_redaction_verified" : current.ApprovalReason,
                VerificationFailureReason = providerFailureReason,
                ProviderReviewStatus = providerReviewStatus,
                ProviderName = normalizedProviderName,
                ProviderJobRef = normalizedProviderJobRef,
                ProviderCallbackStatus = normalizedCallbackStatus,
                ProviderCallbackRef = normalizedCallbackRef,
                ProviderCallbackReceivedAt = receivedAt,
                ProviderPackageHash = normalizedProviderPackageHash,
                ProviderEvidenceHash = providerEvidenceHash,
                ProviderFailureReason = providerFailureReason
            };
            _redactions[index] = updated;
            PersistRedaction(tenantId, updated);
            AddAccessLog(
                current.SourceRecordId,
                "redaction.provider_reconciled",
                providerReviewStatus == "provider_verified" ? "allowed" : "denied",
                reviewedByPersonId,
                null,
                null,
                null,
                null,
                normalizedCallbackStatus);

            return new RecordArrRedactionProviderReconciliationResponse(
                updated.RedactionId,
                tenantId,
                updated.SourceRecordId,
                updated.RedactedRecordId,
                normalizedProviderName,
                normalizedProviderJobRef,
                normalizedCallbackStatus,
                normalizedCallbackRef,
                receivedAt,
                normalizedProviderPackageHash,
                providerEvidenceHash,
                providerReviewStatus,
                providerFailureReason,
                updated);
        }
    }

    public IReadOnlyList<RecordArrRedactionProviderJobResponse> GetRedactionProviderJobs(string tenantId)
    {
        lock (_gate)
        {
            return _redactionProviderJobs
                .Where(job => string.Equals(job.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(job => job.RequestedAt)
                .ThenBy(job => job.ProviderJobId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public RecordArrRedactionProviderJobResponse SubmitRedactionProviderJob(
        string tenantId,
        string redactionId,
        string requestedByPersonId,
        string? providerName,
        string? providerJobRef)
    {
        lock (_gate)
        {
            var redaction = _redactions.FirstOrDefault(item =>
                string.Equals(item.RedactionId, redactionId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(item.SourceRecordId, tenantId));
            if (redaction is null)
            {
                throw new InvalidOperationException($"Redaction {redactionId} not found.");
            }

            if (string.IsNullOrWhiteSpace(redaction.RedactionPackageHash))
            {
                AddAccessLog(redaction.SourceRecordId, "redaction.provider_job_submitted", "denied", requestedByPersonId, null, null, null, null, "redaction_package_hash_missing");
                throw new InvalidOperationException($"Redaction {redactionId} does not have locked package hash evidence.");
            }

            var normalizedProviderName = NormalizeRequiredEvidenceValue(providerName, nameof(providerName));
            var normalizedProviderJobRef = NormalizeRequiredEvidenceValue(providerJobRef, nameof(providerJobRef));
            var existing = _redactionProviderJobs.FirstOrDefault(job =>
                string.Equals(job.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(job.ProviderName, normalizedProviderName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(job.ProviderJobRef, normalizedProviderJobRef, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                return existing;
            }

            var requestedAt = DateTimeOffset.UtcNow;
            var submissionHash = ComputeRedactionProviderJobSubmissionHash(
                redaction,
                normalizedProviderName,
                normalizedProviderJobRef,
                requestedAt);
            var job = new RecordArrRedactionProviderJobResponse(
                $"rpj-{Guid.NewGuid():N}"[..12],
                tenantId,
                redaction.RedactionId,
                redaction.SourceRecordId,
                redaction.RedactedRecordId,
                normalizedProviderName,
                normalizedProviderJobRef,
                "submitted",
                requestedByPersonId,
                requestedAt,
                redaction.RedactionPackageHash,
                redaction.RedactionRules,
                submissionHash,
                LastSubmittedAt: requestedAt,
                Redaction: redaction);

            _redactionProviderJobs.Add(job);
            PersistRedactionProviderJob(job);
            AddAccessLog(redaction.SourceRecordId, "redaction.provider_job_submitted", "allowed", requestedByPersonId, null, null, null, null, normalizedProviderJobRef);
            return job;
        }
    }

    public RecordArrRedactionProviderJobResponse ProcessRedactionProviderJobManifest(
        string tenantId,
        string processedByPersonId,
        string? providerName,
        string? providerJobRef,
        string? providerCallbackStatus,
        string? providerCallbackRef,
        string? providerPackageHash)
    {
        lock (_gate)
        {
            var normalizedProviderName = NormalizeRequiredEvidenceValue(providerName, nameof(providerName));
            var normalizedProviderJobRef = NormalizeRequiredEvidenceValue(providerJobRef, nameof(providerJobRef));
            var normalizedCallbackStatus = NormalizeRecordArrEnum(
                providerCallbackStatus ?? string.Empty,
                nameof(providerCallbackStatus),
                "completed",
                "rejected",
                "failed",
                "needs_review");
            var normalizedCallbackRef = NormalizeRequiredEvidenceValue(providerCallbackRef, nameof(providerCallbackRef));
            var normalizedProviderPackageHash = NormalizeRequiredEvidenceValue(providerPackageHash, nameof(providerPackageHash)).ToLowerInvariant();
            var index = _redactionProviderJobs.FindIndex(job =>
                string.Equals(job.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(job.ProviderName, normalizedProviderName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(job.ProviderJobRef, normalizedProviderJobRef, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Redaction provider job {normalizedProviderJobRef} not found.");
            }

            var job = _redactionProviderJobs[index];
            var receivedAt = DateTimeOffset.UtcNow;
            if (!string.Equals(job.RedactionPackageHash, normalizedProviderPackageHash, StringComparison.OrdinalIgnoreCase))
            {
                var failedJob = job with
                {
                    Status = "failed",
                    ProviderCallbackStatus = normalizedCallbackStatus,
                    ProviderCallbackRef = normalizedCallbackRef,
                    ProviderCallbackReceivedAt = receivedAt,
                    FailureReason = "provider_package_hash_mismatch"
                };
                _redactionProviderJobs[index] = failedJob;
                PersistRedactionProviderJob(failedJob);
                AddAccessLog(job.SourceRecordId, "redaction.provider_job_reconciled", "denied", processedByPersonId, null, null, null, null, "provider_package_hash_mismatch");
                return failedJob;
            }

            var reconciliation = ReconcileRedactionProviderStatus(
                tenantId,
                job.RedactionId,
                processedByPersonId,
                normalizedProviderName,
                normalizedProviderJobRef,
                normalizedCallbackStatus,
                normalizedCallbackRef,
                normalizedProviderPackageHash);
            var status = normalizedCallbackStatus == "completed" ? "completed" : "failed";
            var failureReason = normalizedCallbackStatus == "completed" ? null : $"provider_{normalizedCallbackStatus}";
            var updatedJob = job with
            {
                Status = status,
                ProviderCallbackStatus = normalizedCallbackStatus,
                ProviderCallbackRef = normalizedCallbackRef,
                ProviderCallbackReceivedAt = reconciliation.ProviderCallbackReceivedAt,
                ProviderEvidenceHash = reconciliation.ProviderEvidenceHash,
                FailureReason = failureReason,
                Redaction = reconciliation.Redaction
            };
            _redactionProviderJobs[index] = updatedJob;
            PersistRedactionProviderJob(updatedJob);
            AddAccessLog(
                job.SourceRecordId,
                "redaction.provider_job_reconciled",
                status == "completed" ? "allowed" : "denied",
                processedByPersonId,
                null,
                null,
                null,
                null,
                normalizedCallbackStatus);
            return updatedJob;
        }
    }

    public RecordArrRedactionOverlayReviewResponse ReviewRedactionOverlay(
        string tenantId,
        string redactionId,
        string reviewedByPersonId,
        string? overlayReviewStatus,
        IReadOnlyList<string>? overlayEvidenceRefs,
        IReadOnlyList<string>? overlayIssueRefs)
    {
        lock (_gate)
        {
            var index = _redactions.FindIndex(redaction =>
                string.Equals(redaction.RedactionId, redactionId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(redaction.SourceRecordId, tenantId));
            if (index < 0)
            {
                throw new InvalidOperationException($"Redaction {redactionId} not found.");
            }

            var current = _redactions[index];
            if (string.IsNullOrWhiteSpace(current.RedactionPackageHash))
            {
                AddAccessLog(
                    current.SourceRecordId,
                    "redaction.overlay_reviewed",
                    "denied",
                    reviewedByPersonId,
                    null,
                    null,
                    null,
                    null,
                    "redaction_package_hash_missing");
                throw new InvalidOperationException($"Redaction {redactionId} does not have locked package hash evidence.");
            }

            var normalizedReviewStatus = NormalizeRecordArrEnum(
                overlayReviewStatus ?? string.Empty,
                nameof(overlayReviewStatus),
                "approved",
                "changes_requested",
                "rejected");
            var normalizedEvidenceRefs = NormalizeRequiredEvidenceRefs(overlayEvidenceRefs, nameof(overlayEvidenceRefs));
            var normalizedIssueRefs = NormalizeOptionalEvidenceRefs(overlayIssueRefs);
            var reviewedAt = DateTimeOffset.UtcNow;
            var overlayReviewHash = ComputeRedactionOverlayReviewHash(
                current,
                normalizedReviewStatus,
                reviewedByPersonId,
                reviewedAt,
                normalizedEvidenceRefs,
                normalizedIssueRefs);
            var overlayFailureReason = normalizedReviewStatus == "approved"
                ? null
                : $"overlay_{normalizedReviewStatus}";
            var updated = current with
            {
                ReviewStatus = normalizedReviewStatus,
                ReviewedByPersonId = reviewedByPersonId,
                ReviewedAt = reviewedAt,
                ApprovalReason = normalizedReviewStatus == "approved" ? "visual_overlay_review_approved" : current.ApprovalReason,
                VerificationFailureReason = overlayFailureReason,
                OverlayReviewStatus = normalizedReviewStatus,
                OverlayReviewedByPersonId = reviewedByPersonId,
                OverlayReviewedAt = reviewedAt,
                OverlayEvidenceRefs = normalizedEvidenceRefs,
                OverlayIssueRefs = normalizedIssueRefs,
                OverlayReviewHash = overlayReviewHash,
                OverlayFailureReason = overlayFailureReason
            };
            _redactions[index] = updated;
            PersistRedaction(tenantId, updated);
            AddAccessLog(
                current.SourceRecordId,
                "redaction.overlay_reviewed",
                normalizedReviewStatus == "approved" ? "allowed" : "denied",
                reviewedByPersonId,
                null,
                null,
                null,
                null,
                normalizedReviewStatus);

            return new RecordArrRedactionOverlayReviewResponse(
                updated.RedactionId,
                tenantId,
                updated.SourceRecordId,
                updated.RedactedRecordId,
                normalizedReviewStatus,
                reviewedByPersonId,
                reviewedAt,
                normalizedEvidenceRefs,
                normalizedIssueRefs,
                overlayReviewHash,
                overlayFailureReason,
                updated);
        }
    }

    private RecordArrRecordResponse CreateRedactedRecordCopy(
        string tenantId,
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
        PersistFile(redactedFile);

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
            var policy = sourcePolicy with
            {
                AccessPolicyId = $"acc-{Guid.NewGuid():N}"[..12],
                RecordId = redactedRecordId
            };
            _accessPolicies.Add(policy);
            PersistAccessPolicy(tenantId, policy);
        }

        foreach (var sourceGrant in _accessGrants.Where(grant => string.Equals(grant.RecordId, sourceRecord.RecordId, StringComparison.OrdinalIgnoreCase)).ToArray())
        {
            var grant = sourceGrant with
            {
                AccessGrantId = $"agr-{Guid.NewGuid():N}"[..12],
                RecordId = redactedRecordId
            };
            _accessGrants.Add(grant);
            PersistAccessGrant(tenantId, grant);
        }

        var link = new RecordArrRecordLinkResponse(
            $"rlk-{Guid.NewGuid():N}"[..12],
            redactedRecordId,
            sourceRecord.RecordId,
            null,
            "redacted_from",
            now,
            redactedByPersonId);
        _recordLinks.Add(link);
        PersistRecordLink(tenantId, link);

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

    public IReadOnlyList<RecordArrAccessLogResponse> GetAccessLogs(string tenantId, string? recordId = null)
    {
        lock (_gate)
        {
            var logs = string.IsNullOrWhiteSpace(recordId)
                ? _accessLogs.Where(log => RecordBelongsToTenant(log.RecordId, tenantId))
                : _accessLogs.Where(log =>
                    string.Equals(log.RecordId, recordId, StringComparison.OrdinalIgnoreCase) &&
                    RecordBelongsToTenant(log.RecordId, tenantId));
            return logs.OrderByDescending(log => log.OccurredAt).ToArray();
        }
    }

    public RecordArrAccessHistoryIntegrityReportResponse VerifyAccessHistoryIntegrity(string tenantId, string? recordId = null)
    {
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(recordId) && !RecordBelongsToTenant(recordId, tenantId))
            {
                throw new InvalidOperationException($"Record {recordId} not found.");
            }

            var accessLogs = _accessLogs
                .Where(log => RecordBelongsToTenant(log.RecordId, tenantId))
                .Where(log => string.IsNullOrWhiteSpace(recordId) || string.Equals(log.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(log => log.OccurredAt)
                .ThenBy(log => log.AccessLogId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var brokenAccessLogIds = new List<string>();
            string? previousHash = null;
            var enforceChainContinuity = string.IsNullOrWhiteSpace(recordId);

            foreach (var accessLog in accessLogs)
            {
                var expectedHash = ComputeAccessLogHash(tenantId, accessLog, accessLog.PreviousAccessLogHash);
                if (string.IsNullOrWhiteSpace(accessLog.AccessLogHash) ||
                    !string.Equals(accessLog.AccessLogHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    brokenAccessLogIds.Add(accessLog.AccessLogId);
                }

                if (enforceChainContinuity && !string.Equals(accessLog.PreviousAccessLogHash, previousHash, StringComparison.OrdinalIgnoreCase))
                {
                    brokenAccessLogIds.Add(accessLog.AccessLogId);
                }

                previousHash = accessLog.AccessLogHash;
            }

            var distinctBrokenAccessLogIds = brokenAccessLogIds
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return new RecordArrAccessHistoryIntegrityReportResponse(
                tenantId,
                string.IsNullOrWhiteSpace(recordId) ? null : recordId.Trim(),
                distinctBrokenAccessLogIds.Length == 0 ? "verified" : "broken",
                accessLogs.Length,
                accessLogs.FirstOrDefault()?.AccessLogId,
                accessLogs.LastOrDefault()?.AccessLogId,
                distinctBrokenAccessLogIds,
                distinctBrokenAccessLogIds.Length == 0 ? null : $"{distinctBrokenAccessLogIds.Length} access log(s) failed hash-chain verification.",
                DateTimeOffset.UtcNow);
        }
    }

    public IReadOnlyList<RecordArrAccessHistorySealResponse> GetAccessHistorySeals(string tenantId, string? recordId = null)
    {
        lock (_gate)
        {
            return _accessHistorySeals
                .Where(seal => string.Equals(seal.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .Where(seal => string.IsNullOrWhiteSpace(recordId) || string.Equals(seal.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(seal => seal.SealedAt)
                .ToArray();
        }
    }

    public RecordArrAccessHistorySealResponse SealAccessHistory(string tenantId, string? recordId, string sealedByPersonId)
    {
        lock (_gate)
        {
            var normalizedRecordId = string.IsNullOrWhiteSpace(recordId) ? null : recordId.Trim();
            if (normalizedRecordId is not null && !RecordBelongsToTenant(normalizedRecordId, tenantId))
            {
                throw new InvalidOperationException($"Record {normalizedRecordId} not found.");
            }

            var accessLogs = GetOrderedAccessLogsForSeal(tenantId, normalizedRecordId, sealedThroughAccessLogId: null);
            var seal = CreateAccessHistorySealSnapshot(
                $"ahseal-{Guid.NewGuid():N}"[..18],
                tenantId,
                normalizedRecordId,
                accessLogs,
                "sealed",
                sealedByPersonId,
                DateTimeOffset.UtcNow,
                verifiedAt: null,
                issueSummary: null);

            _accessHistorySeals.Add(seal);
            PersistAccessHistorySeal(seal);

            var auditRecordId = normalizedRecordId ?? accessLogs.LastOrDefault()?.RecordId;
            if (!string.IsNullOrWhiteSpace(auditRecordId))
            {
                AddAccessLog(auditRecordId, "access_history.sealed", "allowed", sealedByPersonId, null, null, null, null, seal.Scope);
            }

            return seal;
        }
    }

    public RecordArrAccessHistorySealResponse VerifyAccessHistorySeal(string tenantId, string accessHistorySealId)
    {
        lock (_gate)
        {
            var index = _accessHistorySeals.FindIndex(seal =>
                string.Equals(seal.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(seal.AccessHistorySealId, accessHistorySealId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Access history seal {accessHistorySealId} not found.");
            }

            var current = _accessHistorySeals[index];
            var accessLogs = GetOrderedAccessLogsForSeal(tenantId, current.RecordId, current.SealedThroughAccessLogId);
            var expectedHash = ComputeAccessHistorySealHash(current.TenantId, current.RecordId, accessLogs);
            var issueSummary = string.Equals(expectedHash, current.SealHash, StringComparison.OrdinalIgnoreCase) &&
                               accessLogs.Length == current.SealedAccessLogCount &&
                               string.Equals(accessLogs.LastOrDefault()?.AccessLogId, current.SealedThroughAccessLogId, StringComparison.OrdinalIgnoreCase) &&
                               string.Equals(accessLogs.LastOrDefault()?.AccessLogHash, current.SealedThroughAccessLogHash, StringComparison.OrdinalIgnoreCase)
                ? null
                : "Access history seal no longer matches the sealed access-log range.";

            var verified = current with
            {
                Status = issueSummary is null ? "verified" : "broken",
                VerifiedAt = DateTimeOffset.UtcNow,
                IssueSummary = issueSummary
            };
            _accessHistorySeals[index] = verified;
            PersistAccessHistorySeal(verified);
            return verified;
        }
    }

    public IReadOnlyList<RecordArrAuditEventResponse> GetAuditEvents(string tenantId, string? recordId = null)
    {
        lock (_gate)
        {
            var events = string.IsNullOrWhiteSpace(recordId)
                ? _auditEvents.Where(auditEvent => string.Equals(auditEvent.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                : _auditEvents.Where(auditEvent =>
                    string.Equals(auditEvent.RecordId, recordId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(auditEvent.TenantId, tenantId, StringComparison.OrdinalIgnoreCase));

            return events.OrderByDescending(auditEvent => auditEvent.OccurredAt).ToArray();
        }
    }

    public RecordArrAuditIntegrityReportResponse VerifyAuditIntegrity(string tenantId, string? recordId = null)
    {
        lock (_gate)
        {
            if (!string.IsNullOrWhiteSpace(recordId) && !RecordBelongsToTenant(recordId, tenantId))
            {
                throw new InvalidOperationException($"Record {recordId} not found.");
            }

            var events = _auditEvents
                .Where(auditEvent => string.Equals(auditEvent.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .Where(auditEvent => string.IsNullOrWhiteSpace(recordId) || string.Equals(auditEvent.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(auditEvent => auditEvent.OccurredAt)
                .ThenBy(auditEvent => auditEvent.AuditEventId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var brokenEventIds = new List<string>();
            string? previousHash = null;
            var enforceChainContinuity = string.IsNullOrWhiteSpace(recordId);

            foreach (var auditEvent in events)
            {
                var expectedHash = ComputeAuditEventHash(auditEvent, auditEvent.PreviousEventHash);
                if (!string.Equals(auditEvent.EventHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    brokenEventIds.Add(auditEvent.AuditEventId);
                }

                if (enforceChainContinuity && !string.Equals(auditEvent.PreviousEventHash, previousHash, StringComparison.OrdinalIgnoreCase))
                {
                    brokenEventIds.Add(auditEvent.AuditEventId);
                }

                previousHash = auditEvent.EventHash;
            }

            var distinctBrokenEventIds = brokenEventIds
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return new RecordArrAuditIntegrityReportResponse(
                tenantId,
                string.IsNullOrWhiteSpace(recordId) ? null : recordId.Trim(),
                distinctBrokenEventIds.Length == 0 ? "verified" : "broken",
                events.Length,
                events.FirstOrDefault()?.AuditEventId,
                events.LastOrDefault()?.AuditEventId,
                distinctBrokenEventIds,
                distinctBrokenEventIds.Length == 0 ? null : $"{distinctBrokenEventIds.Length} audit event(s) failed hash-chain verification.",
                DateTimeOffset.UtcNow);
        }
    }

    public IReadOnlyList<RecordArrAuditSealResponse> GetAuditSeals(string tenantId, string? recordId = null)
    {
        lock (_gate)
        {
            return _auditSeals
                .Where(seal => string.Equals(seal.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .Where(seal => string.IsNullOrWhiteSpace(recordId) || string.Equals(seal.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(seal => seal.SealedAt)
                .ToArray();
        }
    }

    public RecordArrAuditSealResponse SealAuditEvents(string tenantId, string? recordId, string sealedByPersonId)
    {
        lock (_gate)
        {
            var normalizedRecordId = string.IsNullOrWhiteSpace(recordId) ? null : recordId.Trim();
            if (normalizedRecordId is not null && !RecordBelongsToTenant(normalizedRecordId, tenantId))
            {
                throw new InvalidOperationException($"Record {normalizedRecordId} not found.");
            }

            var auditSealId = $"aseal-{Guid.NewGuid():N}"[..16];
            var events = GetOrderedAuditEventsForSeal(tenantId, normalizedRecordId, sealedThroughAuditEventId: null);
            var auditRecordId = normalizedRecordId ?? events.LastOrDefault()?.RecordId;
            if (!string.IsNullOrWhiteSpace(auditRecordId))
            {
                AddAccessLog(auditRecordId, "audit.sealed", "allowed", sealedByPersonId, null, null, null, null, auditSealId);
                events = GetOrderedAuditEventsForSeal(tenantId, normalizedRecordId, sealedThroughAuditEventId: null);
            }

            var seal = CreateAuditSealSnapshot(
                auditSealId,
                tenantId,
                normalizedRecordId,
                events,
                "sealed",
                sealedByPersonId,
                DateTimeOffset.UtcNow,
                verifiedAt: null,
                issueSummary: null);

            _auditSeals.Add(seal);
            PersistAuditSeal(seal);

            return seal;
        }
    }

    public RecordArrAuditSealResponse VerifyAuditSeal(string tenantId, string auditSealId)
    {
        lock (_gate)
        {
            var index = _auditSeals.FindIndex(seal =>
                string.Equals(seal.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(seal.AuditSealId, auditSealId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Audit seal {auditSealId} not found.");
            }

            var current = _auditSeals[index];
            var verified = VerifyAuditSealSnapshot(current, DateTimeOffset.UtcNow);
            _auditSeals[index] = verified;
            PersistAuditSeal(verified);
            return verified;
        }
    }

    public RecordArrAuditSealResponse AnchorAuditSeal(
        string tenantId,
        string auditSealId,
        string anchoredByPersonId,
        string? anchorProviderName,
        string? anchorReference,
        DateTimeOffset anchoredAt,
        string? anchoredSealHash)
    {
        lock (_gate)
        {
            var index = _auditSeals.FindIndex(seal =>
                string.Equals(seal.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(seal.AuditSealId, auditSealId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                throw new InvalidOperationException($"Audit seal {auditSealId} not found.");
            }

            var normalizedProvider = string.IsNullOrWhiteSpace(anchorProviderName) ? string.Empty : anchorProviderName.Trim();
            var normalizedReference = string.IsNullOrWhiteSpace(anchorReference) ? string.Empty : anchorReference.Trim();
            var normalizedAnchoredSealHash = NormalizeChecksum(anchoredSealHash) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedProvider))
            {
                throw new InvalidOperationException("Audit seal anchoring requires an anchor provider name.");
            }

            if (string.IsNullOrWhiteSpace(normalizedReference))
            {
                throw new InvalidOperationException("Audit seal anchoring requires an external anchor reference.");
            }

            if (string.IsNullOrWhiteSpace(normalizedAnchoredSealHash))
            {
                throw new InvalidOperationException("Audit seal anchoring requires the externally anchored seal hash.");
            }

            var verified = VerifyAuditSealSnapshot(_auditSeals[index], DateTimeOffset.UtcNow);
            string anchorStatus;
            string? anchorFailureReason = null;
            string? anchorEvidenceHash = null;

            if (!string.Equals(verified.Status, "verified", StringComparison.OrdinalIgnoreCase))
            {
                anchorStatus = "failed";
                anchorFailureReason = "seal_verification_failed";
            }
            else if (!string.Equals(normalizedAnchoredSealHash, NormalizeChecksum(verified.SealHash), StringComparison.OrdinalIgnoreCase))
            {
                anchorStatus = "failed";
                anchorFailureReason = "anchor_hash_mismatch";
            }
            else
            {
                anchorStatus = "anchored";
                anchorEvidenceHash = ComputeAuditAnchorEvidenceHash(
                    verified,
                    normalizedProvider,
                    normalizedReference,
                    anchoredAt,
                    normalizedAnchoredSealHash);
            }

            var anchored = verified with
            {
                AnchorStatus = anchorStatus,
                AnchorProviderName = normalizedProvider,
                AnchorReference = normalizedReference,
                AnchoredAt = anchoredAt,
                AnchoredSealHash = normalizedAnchoredSealHash,
                AnchorEvidenceHash = anchorEvidenceHash,
                AnchorFailureReason = anchorFailureReason
            };

            _auditSeals[index] = anchored;
            PersistAuditSeal(anchored);

            var auditRecordId = anchored.RecordId ??
                GetOrderedAuditEventsForSeal(tenantId, anchored.RecordId, anchored.SealedThroughAuditEventId)
                    .LastOrDefault()
                    ?.RecordId;
            if (!string.IsNullOrWhiteSpace(auditRecordId))
            {
                AddAccessLog(
                    auditRecordId,
                    "audit.seal_anchored",
                    anchorStatus == "anchored" ? "allowed" : "denied",
                    anchoredByPersonId,
                    null,
                    null,
                    null,
                    null,
                    anchored.AuditSealId);
            }

            return anchored;
        }
    }

    public RecordArrAuditGovernanceReportResponse VerifyAuditGovernance(string tenantId, string? recordId = null)
    {
        lock (_gate)
        {
            var normalizedRecordId = string.IsNullOrWhiteSpace(recordId) ? null : recordId.Trim();
            if (normalizedRecordId is not null && !RecordBelongsToTenant(normalizedRecordId, tenantId))
            {
                throw new InvalidOperationException($"Record {normalizedRecordId} not found.");
            }

            var verifiedAt = DateTimeOffset.UtcNow;
            var events = _auditEvents
                .Where(auditEvent => string.Equals(auditEvent.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .Where(auditEvent => normalizedRecordId is null || string.Equals(auditEvent.RecordId, normalizedRecordId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(auditEvent => auditEvent.OccurredAt)
                .ThenBy(auditEvent => auditEvent.AuditEventId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var integrity = VerifyAuditIntegrity(tenantId, normalizedRecordId);
            var relevantSealIndexes = _auditSeals
                .Select((seal, index) => new { Seal = seal, Index = index })
                .Where(item => string.Equals(item.Seal.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
                .Where(item =>
                    normalizedRecordId is null
                        ? string.IsNullOrWhiteSpace(item.Seal.RecordId)
                        : string.IsNullOrWhiteSpace(item.Seal.RecordId) || string.Equals(item.Seal.RecordId, normalizedRecordId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var verifiedSeals = new List<RecordArrAuditSealResponse>();
            var brokenSealIds = new List<string>();

            foreach (var item in relevantSealIndexes)
            {
                var verifiedSeal = VerifyAuditSealSnapshot(item.Seal, verifiedAt);
                _auditSeals[item.Index] = verifiedSeal;
                PersistAuditSeal(verifiedSeal);

                if (string.Equals(verifiedSeal.Status, "verified", StringComparison.OrdinalIgnoreCase))
                {
                    verifiedSeals.Add(verifiedSeal);
                }
                else
                {
                    brokenSealIds.Add(verifiedSeal.AuditSealId);
                }
            }

            var coveredAuditEventIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string? latestSealThroughAuditEventId = null;
            DateTimeOffset? latestSealThroughOccurredAt = null;
            foreach (var seal in verifiedSeals)
            {
                var sealedEvents = GetOrderedAuditEventsForSeal(tenantId, seal.RecordId, seal.SealedThroughAuditEventId);
                foreach (var auditEvent in sealedEvents)
                {
                    if (normalizedRecordId is not null &&
                        !string.Equals(auditEvent.RecordId, normalizedRecordId, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    coveredAuditEventIds.Add(auditEvent.AuditEventId);
                    if (latestSealThroughOccurredAt is null || auditEvent.OccurredAt > latestSealThroughOccurredAt)
                    {
                        latestSealThroughOccurredAt = auditEvent.OccurredAt;
                        latestSealThroughAuditEventId = auditEvent.AuditEventId;
                    }
                }
            }

            var unsealedAuditEventIds = events
                .Where(auditEvent => !coveredAuditEventIds.Contains(auditEvent.AuditEventId))
                .Select(auditEvent => auditEvent.AuditEventId)
                .ToArray();
            var status = integrity.Status == "broken" || brokenSealIds.Count > 0
                ? "broken"
                : unsealedAuditEventIds.Length > 0
                    ? "unsealed"
                    : "verified";
            var issueParts = new List<string>();
            if (integrity.Status == "broken" && !string.IsNullOrWhiteSpace(integrity.IssueSummary))
            {
                issueParts.Add(integrity.IssueSummary);
            }

            if (brokenSealIds.Count > 0)
            {
                issueParts.Add($"{brokenSealIds.Count} audit seal(s) failed verification.");
            }

            if (unsealedAuditEventIds.Length > 0)
            {
                issueParts.Add($"{unsealedAuditEventIds.Length} audit event(s) are not covered by a verified seal.");
            }

            return new RecordArrAuditGovernanceReportResponse(
                tenantId,
                normalizedRecordId,
                status,
                events.Length,
                relevantSealIndexes.Length,
                verifiedSeals.Count,
                unsealedAuditEventIds.Length,
                events.FirstOrDefault()?.AuditEventId,
                events.LastOrDefault()?.AuditEventId,
                latestSealThroughAuditEventId,
                integrity.BrokenAuditEventIds,
                brokenSealIds.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                unsealedAuditEventIds,
                issueParts.Count == 0 ? null : string.Join(" ", issueParts),
                verifiedAt);
        }
    }

    public RecordArrAccessLogResponse AddAccessLog(string recordId, string action, string result, string? actorPersonId, string? actorServiceClientId, string? externalShareId, string? sourceIp, string? userAgent, string? reasonCode)
    {
        lock (_gate)
        {
            var tenantId = ResolveRecordTenantId(recordId);
            var previousHash = _accessLogs
                .Where(log => RecordBelongsToTenant(log.RecordId, tenantId))
                .OrderByDescending(log => log.OccurredAt)
                .ThenByDescending(log => log.AccessLogId, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault()
                ?.AccessLogHash;
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
                reasonCode,
                previousHash,
                string.Empty);
            log = log with { AccessLogHash = ComputeAccessLogHash(tenantId, log, previousHash) };
            _accessLogs.Add(log);
            PersistAccessLog(tenantId, log);
            AppendAuditEvent(tenantId, log);
            return log;
        }
    }

    private static string ComputeAccessLogHash(string tenantId, RecordArrAccessLogResponse accessLog, string? previousHash)
        => ComputeChecksum(string.Join(
            "|",
            tenantId,
            accessLog.AccessLogId,
            accessLog.RecordId,
            accessLog.Action,
            accessLog.Result,
            accessLog.ActorPersonId ?? string.Empty,
            accessLog.ActorServiceClientId ?? string.Empty,
            accessLog.ExternalShareId ?? string.Empty,
            accessLog.OccurredAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            accessLog.SourceIp ?? string.Empty,
            accessLog.UserAgent ?? string.Empty,
            accessLog.ReasonCode ?? string.Empty,
            previousHash ?? string.Empty));

    private RecordArrAccessLogResponse[] GetOrderedAccessLogsForSeal(string tenantId, string? recordId, string? sealedThroughAccessLogId)
    {
        var logs = _accessLogs
            .Where(log => RecordBelongsToTenant(log.RecordId, tenantId))
            .Where(log => string.IsNullOrWhiteSpace(recordId) || string.Equals(log.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(log => log.OccurredAt)
            .ThenBy(log => log.AccessLogId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (string.IsNullOrWhiteSpace(sealedThroughAccessLogId))
        {
            return logs;
        }

        var throughIndex = Array.FindIndex(logs, log =>
            string.Equals(log.AccessLogId, sealedThroughAccessLogId, StringComparison.OrdinalIgnoreCase));
        if (throughIndex < 0)
        {
            return [];
        }

        return logs.Take(throughIndex + 1).ToArray();
    }

    private static RecordArrAccessHistorySealResponse CreateAccessHistorySealSnapshot(
        string accessHistorySealId,
        string tenantId,
        string? recordId,
        IReadOnlyList<RecordArrAccessLogResponse> accessLogs,
        string status,
        string sealedByPersonId,
        DateTimeOffset sealedAt,
        DateTimeOffset? verifiedAt,
        string? issueSummary)
        => new(
            accessHistorySealId,
            tenantId,
            recordId,
            string.IsNullOrWhiteSpace(recordId) ? "tenant" : "record",
            accessLogs.Count,
            accessLogs.FirstOrDefault()?.AccessLogId,
            accessLogs.LastOrDefault()?.AccessLogId,
            accessLogs.LastOrDefault()?.AccessLogHash,
            ComputeAccessHistorySealHash(tenantId, recordId, accessLogs),
            status,
            sealedByPersonId,
            sealedAt,
            verifiedAt,
            issueSummary);

    private static string ComputeAccessHistorySealHash(string tenantId, string? recordId, IReadOnlyList<RecordArrAccessLogResponse> accessLogs)
        => ComputeChecksum(string.Join(
            "|",
            tenantId,
            recordId ?? string.Empty,
            accessLogs.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            accessLogs.FirstOrDefault()?.AccessLogId ?? string.Empty,
            accessLogs.LastOrDefault()?.AccessLogId ?? string.Empty,
            accessLogs.LastOrDefault()?.AccessLogHash ?? string.Empty,
            string.Join(
                ";",
                accessLogs.Select(accessLog => string.Join(
                    ":",
                    accessLog.AccessLogId,
                    accessLog.PreviousAccessLogHash ?? string.Empty,
                    accessLog.AccessLogHash)))));

    private void AppendAuditEvent(string tenantId, RecordArrAccessLogResponse log)
    {
        if (!Guid.TryParse(tenantId, out _))
        {
            return;
        }

        var previousHash = _auditEvents
            .Where(auditEvent => string.Equals(auditEvent.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(auditEvent => auditEvent.OccurredAt)
            .ThenByDescending(auditEvent => auditEvent.AuditEventId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault()
            ?.EventHash;
        var actorType = !string.IsNullOrWhiteSpace(log.ActorServiceClientId)
            ? "service_client"
            : !string.IsNullOrWhiteSpace(log.ActorPersonId)
                ? "person"
                : "system";
        var auditEventId = $"aud-{Guid.NewGuid():N}"[..12];
        var auditEvent = new RecordArrAuditEventResponse(
            auditEventId,
            tenantId,
            log.RecordId,
            log.Action,
            log.Result,
            actorType,
            log.ActorPersonId,
            log.ActorServiceClientId,
            log.ExternalShareId,
            log.OccurredAt,
            log.ReasonCode,
            log.AccessLogId,
            previousHash,
            string.Empty);
        auditEvent = auditEvent with { EventHash = ComputeAuditEventHash(auditEvent, previousHash) };

        _auditEvents.Add(auditEvent);
        PersistAuditEvent(auditEvent);
    }

    private static string ComputeAuditEventHash(RecordArrAuditEventResponse auditEvent, string? previousHash)
        => ComputeChecksum(string.Join(
            "|",
            auditEvent.TenantId,
            auditEvent.AuditEventId,
            auditEvent.RecordId,
            auditEvent.Action,
            auditEvent.Outcome,
            auditEvent.ActorType,
            auditEvent.ActorPersonId ?? string.Empty,
            auditEvent.ActorServiceClientId ?? string.Empty,
            auditEvent.ExternalShareId ?? string.Empty,
            auditEvent.OccurredAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            auditEvent.ReasonCode ?? string.Empty,
            auditEvent.CorrelationId ?? string.Empty,
            previousHash ?? string.Empty));

    private RecordArrAuditEventResponse[] GetOrderedAuditEventsForSeal(string tenantId, string? recordId, string? sealedThroughAuditEventId)
    {
        var events = _auditEvents
            .Where(auditEvent => string.Equals(auditEvent.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
            .Where(auditEvent => string.IsNullOrWhiteSpace(recordId) || string.Equals(auditEvent.RecordId, recordId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(auditEvent => auditEvent.OccurredAt)
            .ThenBy(auditEvent => auditEvent.AuditEventId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (string.IsNullOrWhiteSpace(sealedThroughAuditEventId))
        {
            return events;
        }

        var throughIndex = Array.FindIndex(events, auditEvent =>
            string.Equals(auditEvent.AuditEventId, sealedThroughAuditEventId, StringComparison.OrdinalIgnoreCase));
        if (throughIndex < 0)
        {
            return [];
        }

        return events.Take(throughIndex + 1).ToArray();
    }

    private static RecordArrAuditSealResponse CreateAuditSealSnapshot(
        string auditSealId,
        string tenantId,
        string? recordId,
        IReadOnlyList<RecordArrAuditEventResponse> events,
        string status,
        string sealedByPersonId,
        DateTimeOffset sealedAt,
        DateTimeOffset? verifiedAt,
        string? issueSummary)
        => new(
            auditSealId,
            tenantId,
            recordId,
            string.IsNullOrWhiteSpace(recordId) ? "tenant" : "record",
            events.Count,
            events.FirstOrDefault()?.AuditEventId,
            events.LastOrDefault()?.AuditEventId,
            events.LastOrDefault()?.EventHash,
            ComputeAuditSealHash(tenantId, recordId, events),
            status,
            sealedByPersonId,
            sealedAt,
            verifiedAt,
            issueSummary);

    private RecordArrAuditSealResponse VerifyAuditSealSnapshot(RecordArrAuditSealResponse current, DateTimeOffset verifiedAt)
    {
        var events = GetOrderedAuditEventsForSeal(current.TenantId, current.RecordId, current.SealedThroughAuditEventId);
        var expectedHash = ComputeAuditSealHash(current.TenantId, current.RecordId, events);
        var issueSummary = string.Equals(expectedHash, current.SealHash, StringComparison.OrdinalIgnoreCase) &&
                           events.Length == current.SealedEventCount &&
                           string.Equals(events.LastOrDefault()?.AuditEventId, current.SealedThroughAuditEventId, StringComparison.OrdinalIgnoreCase) &&
                           string.Equals(events.LastOrDefault()?.EventHash, current.SealedThroughEventHash, StringComparison.OrdinalIgnoreCase)
            ? null
            : "Audit seal no longer matches the sealed audit-event range.";

        return current with
        {
            Status = issueSummary is null ? "verified" : "broken",
            VerifiedAt = verifiedAt,
            IssueSummary = issueSummary,
            AnchorStatus = issueSummary is not null && string.Equals(current.AnchorStatus, "anchored", StringComparison.OrdinalIgnoreCase)
                ? "broken"
                : current.AnchorStatus,
            AnchorFailureReason = issueSummary is not null && string.Equals(current.AnchorStatus, "anchored", StringComparison.OrdinalIgnoreCase)
                ? "sealed_range_no_longer_matches_anchor"
                : current.AnchorFailureReason
        };
    }

    private static string ComputeAuditSealHash(string tenantId, string? recordId, IReadOnlyList<RecordArrAuditEventResponse> events)
        => ComputeChecksum(string.Join(
            "|",
            tenantId,
            recordId ?? string.Empty,
            events.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            events.FirstOrDefault()?.AuditEventId ?? string.Empty,
            events.LastOrDefault()?.AuditEventId ?? string.Empty,
            events.LastOrDefault()?.EventHash ?? string.Empty,
            string.Join(
                ";",
                events.Select(auditEvent => string.Join(
                    ":",
                    auditEvent.AuditEventId,
                    auditEvent.PreviousEventHash ?? string.Empty,
                    auditEvent.EventHash)))));

    private static string ComputeAuditAnchorEvidenceHash(
        RecordArrAuditSealResponse seal,
        string anchorProviderName,
        string anchorReference,
        DateTimeOffset anchoredAt,
        string anchoredSealHash)
        => ComputeChecksum(string.Join(
            "|",
            seal.TenantId,
            seal.AuditSealId,
            seal.RecordId ?? string.Empty,
            seal.SealedThroughAuditEventId ?? string.Empty,
            seal.SealHash,
            anchorProviderName,
            anchorReference,
            anchoredAt.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture),
            anchoredSealHash));

    public RecordArrPackageManifestResponse? GetManifest(string tenantId, string packageId)
    {
        lock (_gate)
        {
            var package = GetPackage(tenantId, packageId);
            if (package is null)
            {
                return null;
            }

            return _manifests.FirstOrDefault(manifest => string.Equals(manifest.PackageId, packageId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public RecordArrDisposalReviewResponse CreateDisposalReview(string tenantId, string recordId, string retentionStatusRef, string proposedAction, string requestedByPersonId)
    {
        lock (_gate)
        {
            if (!RecordBelongsToTenant(recordId, tenantId))
            {
                throw new InvalidOperationException($"Record {recordId} not found.");
            }

            var retentionIndex = _retentionStatuses.FindIndex(status =>
                string.Equals(status.RetentionStatusId, retentionStatusRef, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(status.RecordId, recordId, StringComparison.OrdinalIgnoreCase) &&
                RecordBelongsToTenant(status.RecordId, tenantId));
            if (retentionIndex < 0)
            {
                throw new InvalidOperationException($"Retention status {retentionStatusRef} not found.");
            }

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
            PersistDisposalReview(tenantId, review);
            _retentionStatuses[retentionIndex] = _retentionStatuses[retentionIndex] with
            {
                DisposalReviewRef = review.DisposalReviewId
            };
            PersistRetentionStatus(tenantId, _retentionStatuses[retentionIndex]);
            return review;
        }
    }
}
