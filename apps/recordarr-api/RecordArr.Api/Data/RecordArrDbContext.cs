using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Data;

namespace RecordArr.Api.Data;

public sealed class RecordArrDbContext(DbContextOptions<RecordArrDbContext> options) : PlatformDbContext(options)
{
    public DbSet<RecordArrRecordEntity> RecordArrRecords => Set<RecordArrRecordEntity>();
    public DbSet<RecordArrFileEntity> RecordArrFiles => Set<RecordArrFileEntity>();
    public DbSet<RecordArrFileIntegrityCheckEntity> RecordArrFileIntegrityChecks => Set<RecordArrFileIntegrityCheckEntity>();
    public DbSet<RecordArrFileMalwareScanEntity> RecordArrFileMalwareScans => Set<RecordArrFileMalwareScanEntity>();
    public DbSet<RecordArrStorageReconciliationEntity> RecordArrStorageReconciliations => Set<RecordArrStorageReconciliationEntity>();
    public DbSet<RecordArrObjectStoreObjectEntity> RecordArrObjectStoreObjects => Set<RecordArrObjectStoreObjectEntity>();
    public DbSet<RecordArrObjectStoreFixityObservationEntity> RecordArrObjectStoreFixityObservations => Set<RecordArrObjectStoreFixityObservationEntity>();
    public DbSet<RecordArrDisasterRecoveryRunEntity> RecordArrDisasterRecoveryRuns => Set<RecordArrDisasterRecoveryRunEntity>();
    public DbSet<RecordArrRecordMetadataEntity> RecordArrRecordMetadata => Set<RecordArrRecordMetadataEntity>();
    public DbSet<RecordArrRecordLinkEntity> RecordArrRecordLinks => Set<RecordArrRecordLinkEntity>();
    public DbSet<RecordArrRecordCommentEntity> RecordArrRecordComments => Set<RecordArrRecordCommentEntity>();
    public DbSet<RecordArrUploadSessionEntity> RecordArrUploadSessions => Set<RecordArrUploadSessionEntity>();
    public DbSet<RecordArrCaptureRequestEntity> RecordArrCaptureRequests => Set<RecordArrCaptureRequestEntity>();
    public DbSet<RecordArrScanProcessingEntity> RecordArrScanProcessing => Set<RecordArrScanProcessingEntity>();
    public DbSet<RecordArrOcrResultEntity> RecordArrOcrResults => Set<RecordArrOcrResultEntity>();
    public DbSet<RecordArrExtractionResultEntity> RecordArrExtractionResults => Set<RecordArrExtractionResultEntity>();
    public DbSet<RecordArrEvidenceMappingEntity> RecordArrEvidenceMappings => Set<RecordArrEvidenceMappingEntity>();
    public DbSet<RecordArrPackageEntity> RecordArrPackages => Set<RecordArrPackageEntity>();
    public DbSet<RecordArrPackageManifestEntity> RecordArrPackageManifests => Set<RecordArrPackageManifestEntity>();
    public DbSet<RecordArrRetentionStatusEntity> RecordArrRetentionStatuses => Set<RecordArrRetentionStatusEntity>();
    public DbSet<RecordArrDisposalReviewEntity> RecordArrDisposalReviews => Set<RecordArrDisposalReviewEntity>();
    public DbSet<RecordArrDestructionCertificateEntity> RecordArrDestructionCertificates => Set<RecordArrDestructionCertificateEntity>();
    public DbSet<RecordArrRetentionSchedulerRunEntity> RecordArrRetentionSchedulerRuns => Set<RecordArrRetentionSchedulerRunEntity>();
    public DbSet<RecordArrRetentionSchedulerLeaseEntity> RecordArrRetentionSchedulerLeases => Set<RecordArrRetentionSchedulerLeaseEntity>();
    public DbSet<RecordArrRetentionSchedulerOutboxMessageEntity> RecordArrRetentionSchedulerOutboxMessages => Set<RecordArrRetentionSchedulerOutboxMessageEntity>();
    public DbSet<RecordArrLegalHoldEntity> RecordArrLegalHolds => Set<RecordArrLegalHoldEntity>();
    public DbSet<RecordArrControlledDocumentEntity> RecordArrControlledDocuments => Set<RecordArrControlledDocumentEntity>();
    public DbSet<RecordArrControlledDocumentVersionEntity> RecordArrControlledDocumentVersions => Set<RecordArrControlledDocumentVersionEntity>();
    public DbSet<RecordArrDocumentReviewEntity> RecordArrDocumentReviews => Set<RecordArrDocumentReviewEntity>();
    public DbSet<RecordArrDocumentDistributionEntity> RecordArrDocumentDistributions => Set<RecordArrDocumentDistributionEntity>();
    public DbSet<RecordArrDocumentAcknowledgementEntity> RecordArrDocumentAcknowledgements => Set<RecordArrDocumentAcknowledgementEntity>();
    public DbSet<RecordArrAccessPolicyEntity> RecordArrAccessPolicies => Set<RecordArrAccessPolicyEntity>();
    public DbSet<RecordArrAccessGrantEntity> RecordArrAccessGrants => Set<RecordArrAccessGrantEntity>();
    public DbSet<RecordArrExternalShareEntity> RecordArrExternalShares => Set<RecordArrExternalShareEntity>();
    public DbSet<RecordArrRedactionEntity> RecordArrRedactions => Set<RecordArrRedactionEntity>();
    public DbSet<RecordArrRedactionProviderJobEntity> RecordArrRedactionProviderJobs => Set<RecordArrRedactionProviderJobEntity>();
    public DbSet<RecordArrSignatureRecordEntity> RecordArrSignatureRecords => Set<RecordArrSignatureRecordEntity>();
    public DbSet<RecordArrSignatureTrustServiceJobEntity> RecordArrSignatureTrustServiceJobs => Set<RecordArrSignatureTrustServiceJobEntity>();
    public DbSet<RecordArrPhotoEvidenceEntity> RecordArrPhotoEvidence => Set<RecordArrPhotoEvidenceEntity>();
    public DbSet<RecordArrAccessLogEntity> RecordArrAccessLogs => Set<RecordArrAccessLogEntity>();
    public DbSet<RecordArrAccessHistorySealEntity> RecordArrAccessHistorySeals => Set<RecordArrAccessHistorySealEntity>();
    public DbSet<RecordArrAuditEventEntity> RecordArrAuditEvents => Set<RecordArrAuditEventEntity>();
    public DbSet<RecordArrAuditSealEntity> RecordArrAuditSeals => Set<RecordArrAuditSealEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RecordArrRecordEntity>(entity =>
        {
            entity.ToTable("recordarr_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Classification).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectType).HasMaxLength(96).IsRequired();
            entity.Property(x => x.SourceObjectId).HasMaxLength(160).IsRequired();
            entity.Property(x => x.SourceObjectDisplayName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.OwnerPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.UploadedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RecordId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceObjectType, x.SourceObjectId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.UploadedAt });
        });

        modelBuilder.Entity<RecordArrFileEntity>(entity =>
        {
            entity.ToTable("recordarr_files");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FileId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageProvider).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.OriginalFilename).HasMaxLength(256).IsRequired();
            entity.Property(x => x.MimeType).HasMaxLength(160).IsRequired();
            entity.Property(x => x.ChecksumSha256).HasMaxLength(128).IsRequired();
            entity.Property(x => x.MalwareScanStatus).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProcessingStatus).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.FileId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.UploadedAt });
            entity.HasIndex(x => new { x.TenantId, x.ChecksumSha256 });
        });

        modelBuilder.Entity<RecordArrFileIntegrityCheckEntity>(entity =>
        {
            entity.ToTable("recordarr_file_integrity_checks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IntegrityCheckId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageProvider).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ExpectedChecksumSha256).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ObservedChecksumSha256).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CheckMethod).HasMaxLength(96).IsRequired();
            entity.Property(x => x.CheckedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(256);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.IntegrityCheckId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.FileId, x.CheckedAt });
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.CheckedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CheckedAt });
        });

        modelBuilder.Entity<RecordArrFileMalwareScanEntity>(entity =>
        {
            entity.ToTable("recordarr_file_malware_scans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MalwareScanId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageProvider).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScannerName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ScannerVersion).HasMaxLength(64);
            entity.Property(x => x.SignatureVersion).HasMaxLength(128);
            entity.Property(x => x.ThreatName).HasMaxLength(256);
            entity.Property(x => x.QuarantineStatus).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScannedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(256);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.MalwareScanId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.FileId, x.ScannedAt });
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.ScannedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ScannedAt });
            entity.HasIndex(x => new { x.TenantId, x.QuarantineStatus, x.ScannedAt });
        });

        modelBuilder.Entity<RecordArrStorageReconciliationEntity>(entity =>
        {
            entity.ToTable("recordarr_storage_reconciliations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReconciliationId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Scope).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RequestedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IssueSummary).HasMaxLength(512);
            entity.Property(x => x.RemediationStatus).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ReconciliationId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CompletedAt });
            entity.HasIndex(x => new { x.TenantId, x.RemediationStatus, x.CompletedAt });
        });

        modelBuilder.Entity<RecordArrObjectStoreObjectEntity>(entity =>
        {
            entity.ToTable("recordarr_object_store_objects");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ObjectStoreObjectId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageProvider).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ExpectedChecksumSha256).HasMaxLength(128).IsRequired();
            entity.Property(x => x.LastObservedChecksumSha256).HasMaxLength(128);
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LastObservationSource).HasMaxLength(96).IsRequired();
            entity.Property(x => x.LastIntegrityCheckRef).HasMaxLength(64);
            entity.Property(x => x.LastReconciliationRef).HasMaxLength(64);
            entity.Property(x => x.LastObservedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(256);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ObjectStoreObjectId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.FileId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.LastObservedAt });
            entity.HasIndex(x => new { x.TenantId, x.StorageProvider, x.StorageKey });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.LastObservedAt });
        });

        modelBuilder.Entity<RecordArrObjectStoreFixityObservationEntity>(entity =>
        {
            entity.ToTable("recordarr_object_store_fixity_observations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FixityObservationId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageProvider).HasMaxLength(64).IsRequired();
            entity.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
            entity.Property(x => x.ExpectedChecksumSha256).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ObservedChecksumSha256).HasMaxLength(128);
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ObservationSource).HasMaxLength(96).IsRequired();
            entity.Property(x => x.IntegrityCheckRef).HasMaxLength(64);
            entity.Property(x => x.ReconciliationRef).HasMaxLength(64);
            entity.Property(x => x.ObservedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(256);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.FixityObservationId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.FileId, x.ObservedAt });
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.ObservedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ObservedAt });
            entity.HasIndex(x => new { x.TenantId, x.ReconciliationRef });
        });

        modelBuilder.Entity<RecordArrDisasterRecoveryRunEntity>(entity =>
        {
            entity.ToTable("recordarr_disaster_recovery_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisasterRecoveryRunId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Scope).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RecoveryPointId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RequestedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.EvidenceSummary).HasMaxLength(512);
            entity.Property(x => x.FailureReason).HasMaxLength(512);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DisasterRecoveryRunId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CompletedAt });
            entity.HasIndex(x => new { x.TenantId, x.RecoveryPointId, x.CompletedAt });
        });

        modelBuilder.Entity<RecordArrRecordMetadataEntity>(entity =>
        {
            entity.ToTable("recordarr_record_metadata");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MetadataId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ValueType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.MetadataId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.Key });
        });

        modelBuilder.Entity<RecordArrRecordLinkEntity>(entity =>
        {
            entity.ToTable("recordarr_record_links");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RecordLinkId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LinkedRecordId).HasMaxLength(64);
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256);
            entity.Property(x => x.LinkType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RecordLinkId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.LinkType });
            entity.HasIndex(x => new { x.TenantId, x.SourceObjectRef });
        });

        modelBuilder.Entity<RecordArrRecordCommentEntity>(entity =>
        {
            entity.ToTable("recordarr_record_comments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CommentId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Visibility).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.EditedByPersonId).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CommentId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.CreatedAt });
        });

        modelBuilder.Entity<RecordArrUploadSessionEntity>(entity =>
        {
            entity.ToTable("recordarr_upload_sessions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UploadSessionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.UploadSessionNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SessionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectType).HasMaxLength(96).IsRequired();
            entity.Property(x => x.SourceObjectId).HasMaxLength(160).IsRequired();
            entity.Property(x => x.UploadPurpose).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.UploadSessionId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceObjectType, x.SourceObjectId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
        });

        modelBuilder.Entity<RecordArrCaptureRequestEntity>(entity =>
        {
            entity.ToTable("recordarr_capture_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CaptureRequestId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CaptureType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.UploadSessionRef).HasMaxLength(64);
            entity.Property(x => x.EvidenceRequirementRef).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.CaptureRequestId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.UploadSessionRef });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
        });

        modelBuilder.Entity<RecordArrScanProcessingEntity>(entity =>
        {
            entity.ToTable("recordarr_scan_processing");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScanProcessingId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ScanPurpose).HasMaxLength(96).IsRequired();
            entity.Property(x => x.OriginalFileRef).HasMaxLength(64);
            entity.Property(x => x.GeneratedPdfFileRef).HasMaxLength(64);
            entity.Property(x => x.OcrResultId).HasMaxLength(64);
            entity.Property(x => x.ExtractionResultId).HasMaxLength(64);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ScanProcessingId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.ProcessedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ProcessedAt });
        });

        modelBuilder.Entity<RecordArrOcrResultEntity>(entity =>
        {
            entity.ToTable("recordarr_ocr_results");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OcrResultId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Engine).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Language).HasMaxLength(32).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.OcrResultId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.ExtractedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ExtractedAt });
        });

        modelBuilder.Entity<RecordArrExtractionResultEntity>(entity =>
        {
            entity.ToTable("recordarr_extraction_results");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExtractionResultId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExtractionType).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReviewedByPersonId).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ExtractionResultId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.ExtractedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ExtractedAt });
        });

        modelBuilder.Entity<RecordArrEvidenceMappingEntity>(entity =>
        {
            entity.ToTable("recordarr_evidence_mappings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EvidenceMappingId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectType).HasMaxLength(96).IsRequired();
            entity.Property(x => x.SourceObjectId).HasMaxLength(160).IsRequired();
            entity.Property(x => x.ComplianceRequirementRef).HasMaxLength(160).IsRequired();
            entity.Property(x => x.EvidenceTypeKey).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.MappingSource).HasMaxLength(96).IsRequired();
            entity.Property(x => x.ConfirmedByPersonId).HasMaxLength(128);
            entity.Property(x => x.RejectedByPersonId).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.EvidenceMappingId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceObjectType, x.SourceObjectId, x.ComplianceRequirementRef });
        });

        modelBuilder.Entity<RecordArrPackageEntity>(entity =>
        {
            entity.ToTable("recordarr_packages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PackageId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PackageNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PackageType).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ManifestChecksum).HasMaxLength(128);
            entity.Property(x => x.GeneratedPdfRecordRef).HasMaxLength(64);
            entity.Property(x => x.GeneratedZipFileRef).HasMaxLength(64);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PackageId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.CreatedAt });
        });

        modelBuilder.Entity<RecordArrPackageManifestEntity>(entity =>
        {
            entity.ToTable("recordarr_package_manifests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ManifestId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PackageId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Checksum).HasMaxLength(128).IsRequired();
            entity.Property(x => x.GeneratedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ManifestId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.PackageId, x.ManifestVersion });
        });

        modelBuilder.Entity<RecordArrRetentionStatusEntity>(entity =>
        {
            entity.ToTable("recordarr_retention_statuses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RetentionStatusId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RetentionPolicyRef).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReviewedByPersonId).HasMaxLength(128);
            entity.Property(x => x.DisposalReviewRef).HasMaxLength(64);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RetentionStatusId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.NextReviewAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.RetentionExpiresAt });
        });

        modelBuilder.Entity<RecordArrDisposalReviewEntity>(entity =>
        {
            entity.ToTable("recordarr_disposal_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisposalReviewId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RetentionStatusRef).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProposedAction).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RequestedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ReviewedByPersonId).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DisposalReviewId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.RetentionStatusRef });
        });

        modelBuilder.Entity<RecordArrDestructionCertificateEntity>(entity =>
        {
            entity.ToTable("recordarr_destruction_certificates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DestructionCertificateId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CertificateNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RetentionStatusRef).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DisposalReviewRef).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DispositionAction).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExecutedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CertificateHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(256);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DestructionCertificateId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.CertificateNumber }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.ExecutedAt });
            entity.HasIndex(x => new { x.TenantId, x.DisposalReviewRef });
        });

        modelBuilder.Entity<RecordArrLegalHoldEntity>(entity =>
        {
            entity.ToTable("recordarr_legal_holds");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LegalHoldId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.HoldNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.HoldType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectType).HasMaxLength(96).IsRequired();
            entity.Property(x => x.SourceObjectId).HasMaxLength(160).IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ReleasedByPersonId).HasMaxLength(128);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.LegalHoldId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceObjectType, x.SourceObjectId });
        });

        modelBuilder.Entity<RecordArrControlledDocumentEntity>(entity =>
        {
            entity.ToTable("recordarr_controlled_documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ControlledDocumentId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DocumentNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DocumentClass).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DocumentType).HasMaxLength(96).IsRequired();
            entity.Property(x => x.DocumentSubtype).HasMaxLength(96).IsRequired();
            entity.Property(x => x.ControlledDocumentType).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.OwnerPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DepartmentOrgUnitId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.StaffarrSiteId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CurrentVersionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ControlledDocumentId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.NextReviewAt });
        });

        modelBuilder.Entity<RecordArrControlledDocumentVersionEntity>(entity =>
        {
            entity.ToTable("recordarr_controlled_document_versions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.VersionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ControlledDocumentId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.VersionLabel).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ApprovedByPersonId).HasMaxLength(128);
            entity.Property(x => x.PreviousVersionRef).HasMaxLength(64);
            entity.Property(x => x.NextVersionRef).HasMaxLength(64);
            entity.Property(x => x.FileRef).HasMaxLength(64);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.VersionId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ControlledDocumentId, x.VersionNumber });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
        });

        modelBuilder.Entity<RecordArrDocumentReviewEntity>(entity =>
        {
            entity.ToTable("recordarr_document_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DocumentReviewId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ControlledDocumentId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.VersionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ReviewType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RequestedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ReviewerPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DocumentReviewId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ControlledDocumentId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.ReviewerPersonId, x.DueAt });
        });

        modelBuilder.Entity<RecordArrDocumentDistributionEntity>(entity =>
        {
            entity.ToTable("recordarr_document_distributions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DistributionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ControlledDocumentId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.VersionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DistributionType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetRef).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AcknowledgementRef).HasMaxLength(64);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.DistributionId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ControlledDocumentId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.TargetRef, x.Status });
        });

        modelBuilder.Entity<RecordArrDocumentAcknowledgementEntity>(entity =>
        {
            entity.ToTable("recordarr_document_acknowledgements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AcknowledgementId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ControlledDocumentId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.VersionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SignatureRecordRef).HasMaxLength(64);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AcknowledgementId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ControlledDocumentId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.PersonId, x.DueAt });
        });

        modelBuilder.Entity<RecordArrAccessPolicyEntity>(entity =>
        {
            entity.ToTable("recordarr_access_policies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AccessPolicyId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PolicyType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AccessPolicyId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.Status });
        });

        modelBuilder.Entity<RecordArrAccessGrantEntity>(entity =>
        {
            entity.ToTable("recordarr_access_grants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AccessGrantId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.GranteeType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.GranteeRef).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Permission).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.GrantedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AccessGrantId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.GranteeType, x.GranteeRef, x.Permission });
        });

        modelBuilder.Entity<RecordArrExternalShareEntity>(entity =>
        {
            entity.ToTable("recordarr_external_shares");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExternalShareId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ShareNumber).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SharePurpose).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecipientEmail).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ExternalShareId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.ExpiresAt });
        });

        modelBuilder.Entity<RecordArrRedactionEntity>(entity =>
        {
            entity.ToTable("recordarr_redactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RedactionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceRecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RedactedRecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RedactionReason).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RedactedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.RedactionId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SourceRecordId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.RedactedRecordId });
        });

        modelBuilder.Entity<RecordArrSignatureRecordEntity>(entity =>
        {
            entity.ToTable("recordarr_signature_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SignatureRecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SignaturePurpose).HasMaxLength(96).IsRequired();
            entity.Property(x => x.SignerPersonId).HasMaxLength(128);
            entity.Property(x => x.SignerExternalName).HasMaxLength(256);
            entity.Property(x => x.SignerTitle).HasMaxLength(160);
            entity.Property(x => x.SignatureFileRef).HasMaxLength(64).IsRequired();
            entity.Property(x => x.CapturedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SignatureRecordId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.SignedAt });
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceObjectRef });
        });

        modelBuilder.Entity<RecordArrPhotoEvidenceEntity>(entity =>
        {
            entity.ToTable("recordarr_photo_evidence");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PhotoEvidenceId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PhotoPurpose).HasMaxLength(96).IsRequired();
            entity.Property(x => x.SourceProduct).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceObjectRef).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CapturedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.PhotoEvidenceId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.CapturedAt });
            entity.HasIndex(x => new { x.TenantId, x.SourceProduct, x.SourceObjectRef });
        });

        modelBuilder.Entity<RecordArrAccessLogEntity>(entity =>
        {
            entity.ToTable("recordarr_access_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AccessLogId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Result).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ActorPersonId).HasMaxLength(128);
            entity.Property(x => x.ActorServiceClientId).HasMaxLength(128);
            entity.Property(x => x.ExternalShareId).HasMaxLength(64);
            entity.Property(x => x.ReasonCode).HasMaxLength(256);
            entity.Property(x => x.PreviousAccessLogHash).HasMaxLength(128);
            entity.Property(x => x.AccessLogHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AccessLogId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.ExternalShareId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.AccessLogHash });
        });

        modelBuilder.Entity<RecordArrAccessHistorySealEntity>(entity =>
        {
            entity.ToTable("recordarr_access_history_seals");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AccessHistorySealId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64);
            entity.Property(x => x.Scope).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FirstAccessLogId).HasMaxLength(64);
            entity.Property(x => x.SealedThroughAccessLogId).HasMaxLength(64);
            entity.Property(x => x.SealedThroughAccessLogHash).HasMaxLength(128);
            entity.Property(x => x.SealHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SealedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IssueSummary).HasMaxLength(512);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AccessHistorySealId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.SealedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.SealedAt });
        });

        modelBuilder.Entity<RecordArrAuditEventEntity>(entity =>
        {
            entity.ToTable("recordarr_audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AuditEventId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Action).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Outcome).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ActorType).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ActorPersonId).HasMaxLength(128);
            entity.Property(x => x.ActorServiceClientId).HasMaxLength(128);
            entity.Property(x => x.ExternalShareId).HasMaxLength(64);
            entity.Property(x => x.ReasonCode).HasMaxLength(256);
            entity.Property(x => x.CorrelationId).HasMaxLength(128);
            entity.Property(x => x.PreviousEventHash).HasMaxLength(128);
            entity.Property(x => x.EventHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AuditEventId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.Action, x.OccurredAt });
            entity.HasIndex(x => new { x.TenantId, x.EventHash }).IsUnique();
        });

        modelBuilder.Entity<RecordArrAuditSealEntity>(entity =>
        {
            entity.ToTable("recordarr_audit_seals");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AuditSealId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64);
            entity.Property(x => x.Scope).HasMaxLength(64).IsRequired();
            entity.Property(x => x.FirstAuditEventId).HasMaxLength(64);
            entity.Property(x => x.SealedThroughAuditEventId).HasMaxLength(64);
            entity.Property(x => x.SealedThroughEventHash).HasMaxLength(128);
            entity.Property(x => x.SealHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SealedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.IssueSummary).HasMaxLength(512);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.AuditSealId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RecordId, x.SealedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.SealedAt });
        });

        modelBuilder.Entity<RecordArrRetentionSchedulerRunEntity>(entity =>
        {
            entity.ToTable("recordarr_retention_scheduler_runs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SchedulerRunId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.LeaseId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ExecutionPolicy).HasMaxLength(96).IsRequired();
            entity.Property(x => x.RequestedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(512);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.SchedulerRunId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.RanAt });
        });

        modelBuilder.Entity<RecordArrRetentionSchedulerLeaseEntity>(entity =>
        {
            entity.ToTable("recordarr_retention_scheduler_leases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LeaseId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SchedulerKey).HasMaxLength(96).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.AcquiredByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SchedulerRunId).HasMaxLength(64);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.LeaseId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SchedulerKey, x.Status, x.ExpiresAt });
        });

        modelBuilder.Entity<RecordArrRetentionSchedulerOutboxMessageEntity>(entity =>
        {
            entity.ToTable("recordarr_retention_scheduler_outbox_messages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OutboxMessageId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SchedulerRunId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.MessageType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.TargetRecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.DisposalReviewRef).HasMaxLength(64);
            entity.Property(x => x.CreatedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.DeduplicationKey).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DeliveredByPersonId).HasMaxLength(128);
            entity.Property(x => x.DeliveryChannel).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecipientRef).HasMaxLength(160).IsRequired();
            entity.Property(x => x.EscalatedToRecipientRef).HasMaxLength(160);
            entity.Property(x => x.ExternalProviderRef).HasMaxLength(160);
            entity.Property(x => x.ErrorMessage).HasMaxLength(512);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.OutboxMessageId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.DeduplicationKey }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.LastAttemptAt });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.EscalateAfter });
        });

        modelBuilder.Entity<RecordArrRedactionProviderJobEntity>(entity =>
        {
            entity.ToTable("recordarr_redaction_provider_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProviderJobId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RedactionId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SourceRecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RedactedRecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProviderName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProviderJobRef).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RequestedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.RedactionPackageHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SubmissionEvidenceHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProviderCallbackStatus).HasMaxLength(64);
            entity.Property(x => x.ProviderCallbackRef).HasMaxLength(160);
            entity.Property(x => x.ProviderEvidenceHash).HasMaxLength(128);
            entity.Property(x => x.FailureReason).HasMaxLength(512);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.ProviderJobId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ProviderName, x.ProviderJobRef }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.RedactionId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.RequestedAt });
        });

        modelBuilder.Entity<RecordArrSignatureTrustServiceJobEntity>(entity =>
        {
            entity.ToTable("recordarr_signature_trust_service_jobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TrustServiceJobId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.SignatureRecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RecordId).HasMaxLength(64).IsRequired();
            entity.Property(x => x.ProviderName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProviderEnvelopeRef).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(64).IsRequired();
            entity.Property(x => x.RequestedByPersonId).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CertificateFingerprintSha256).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SignatureEvidenceHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.SubmissionEvidenceHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ProviderCallbackStatus).HasMaxLength(64);
            entity.Property(x => x.ProviderCallbackRef).HasMaxLength(160);
            entity.Property(x => x.ProviderCallbackEvidenceHash).HasMaxLength(128);
            entity.Property(x => x.TrustTimestampAuthorityRef).HasMaxLength(256);
            entity.Property(x => x.LongTermValidationStatus).HasMaxLength(64);
            entity.Property(x => x.FailureReason).HasMaxLength(512);
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.TenantId);
            entity.HasIndex(x => new { x.TenantId, x.TrustServiceJobId }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.ProviderName, x.ProviderEnvelopeRef }).IsUnique();
            entity.HasIndex(x => new { x.TenantId, x.SignatureRecordId, x.Status });
            entity.HasIndex(x => new { x.TenantId, x.Status, x.RequestedAt });
        });
    }
}

public sealed class RecordArrRecordEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string RecordId { get; set; } = string.Empty;
    public string RecordNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public string SourceProduct { get; set; } = string.Empty;
    public string SourceObjectType { get; set; } = string.Empty;
    public string SourceObjectId { get; set; } = string.Empty;
    public string SourceObjectDisplayName { get; set; } = string.Empty;
    public string OwnerPersonId { get; set; } = string.Empty;
    public string UploadedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrFileEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string FileId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string FileNumber { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string OriginalFilename { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string ChecksumSha256 { get; set; } = string.Empty;
    public string MalwareScanStatus { get; set; } = string.Empty;
    public string ProcessingStatus { get; set; } = string.Empty;
    public DateTimeOffset UploadedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrFileIntegrityCheckEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string IntegrityCheckId { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string ExpectedChecksumSha256 { get; set; } = string.Empty;
    public string ObservedChecksumSha256 { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CheckMethod { get; set; } = string.Empty;
    public DateTimeOffset CheckedAt { get; set; }
    public string CheckedByPersonId { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrFileMalwareScanEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string MalwareScanId { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ScannerName { get; set; } = string.Empty;
    public string? ScannerVersion { get; set; }
    public string? SignatureVersion { get; set; }
    public string? ThreatName { get; set; }
    public string QuarantineStatus { get; set; } = string.Empty;
    public DateTimeOffset ScannedAt { get; set; }
    public string ScannedByPersonId { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrStorageReconciliationEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ReconciliationId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public string RequestedByPersonId { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public int CheckedFiles { get; set; }
    public int PassedFiles { get; set; }
    public int MissingFiles { get; set; }
    public int CorruptFiles { get; set; }
    public int QuarantinedFiles { get; set; }
    public int PendingScanFiles { get; set; }
    public int DeletedFiles { get; set; }
    public string? IssueSummary { get; set; }
    public string RemediationStatus { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrObjectStoreObjectEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ObjectStoreObjectId { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string ExpectedChecksumSha256 { get; set; } = string.Empty;
    public string? LastObservedChecksumSha256 { get; set; }
    public string Status { get; set; } = string.Empty;
    public string LastObservationSource { get; set; } = string.Empty;
    public string? LastIntegrityCheckRef { get; set; }
    public string? LastReconciliationRef { get; set; }
    public DateTimeOffset LastObservedAt { get; set; }
    public string LastObservedByPersonId { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrObjectStoreFixityObservationEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string FixityObservationId { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string StorageProvider { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string ExpectedChecksumSha256 { get; set; } = string.Empty;
    public string? ObservedChecksumSha256 { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ObservationSource { get; set; } = string.Empty;
    public string? IntegrityCheckRef { get; set; }
    public string? ReconciliationRef { get; set; }
    public DateTimeOffset ObservedAt { get; set; }
    public string ObservedByPersonId { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrDisasterRecoveryRunEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string DisasterRecoveryRunId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string RecoveryPointId { get; set; } = string.Empty;
    public DateTimeOffset RecoveryPointCreatedAt { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public string RequestedByPersonId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RpoTargetMinutes { get; set; }
    public int RtoTargetMinutes { get; set; }
    public int RecoveryPointAgeMinutes { get; set; }
    public int DurationSeconds { get; set; }
    public bool RpoMet { get; set; }
    public bool RtoMet { get; set; }
    public int TotalRecordCount { get; set; }
    public int RestoredRecordCount { get; set; }
    public int BlockedRecordCount { get; set; }
    public int TotalFileCount { get; set; }
    public int VerifiedFileCount { get; set; }
    public int FailedFileCount { get; set; }
    public string? EvidenceSummary { get; set; }
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrRecordMetadataEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string MetadataId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public bool Verified { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrRecordLinkEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string RecordLinkId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string? LinkedRecordId { get; set; }
    public string? SourceObjectRef { get; set; }
    public string LinkType { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedByPersonId { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrRecordCommentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string CommentId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset? EditedAt { get; set; }
    public string? EditedByPersonId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrUploadSessionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string UploadSessionId { get; set; } = string.Empty;
    public string UploadSessionNumber { get; set; } = string.Empty;
    public string SessionType { get; set; } = string.Empty;
    public string SourceProduct { get; set; } = string.Empty;
    public string SourceObjectType { get; set; } = string.Empty;
    public string SourceObjectId { get; set; } = string.Empty;
    public string UploadPurpose { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrCaptureRequestEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string CaptureRequestId { get; set; } = string.Empty;
    public string SourceProduct { get; set; } = string.Empty;
    public string SourceObjectRef { get; set; } = string.Empty;
    public string CaptureType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? UploadSessionRef { get; set; }
    public string? EvidenceRequirementRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrScanProcessingEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ScanProcessingId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ScanPurpose { get; set; } = string.Empty;
    public string? OriginalFileRef { get; set; }
    public string? GeneratedPdfFileRef { get; set; }
    public string? OcrResultId { get; set; }
    public string? ExtractionResultId { get; set; }
    public decimal ConfidenceScore { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrOcrResultEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string OcrResultId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string FileId { get; set; } = string.Empty;
    public string Engine { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public DateTimeOffset ExtractedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrExtractionResultEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ExtractionResultId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string ExtractionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public DateTimeOffset ExtractedAt { get; set; }
    public string? ReviewedByPersonId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrEvidenceMappingEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string EvidenceMappingId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string SourceProduct { get; set; } = string.Empty;
    public string SourceObjectType { get; set; } = string.Empty;
    public string SourceObjectId { get; set; } = string.Empty;
    public string ComplianceRequirementRef { get; set; } = string.Empty;
    public string EvidenceTypeKey { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string MappingSource { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string? ConfirmedByPersonId { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public string? RejectedByPersonId { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrPackageEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string PackageId { get; set; } = string.Empty;
    public string PackageNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SourceProduct { get; set; } = string.Empty;
    public string? ManifestChecksum { get; set; }
    public string? GeneratedPdfRecordRef { get; set; }
    public string? GeneratedZipFileRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? LockedAt { get; set; }
    public DateTimeOffset? ArchivedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrPackageManifestEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ManifestId { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public int ManifestVersion { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public string GeneratedByPersonId { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrRetentionStatusEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string RetentionStatusId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string RetentionPolicyRef { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset RetentionStartAt { get; set; }
    public DateTimeOffset? RetentionExpiresAt { get; set; }
    public DateTimeOffset? NextReviewAt { get; set; }
    public DateTimeOffset? LastReviewedAt { get; set; }
    public string? ReviewedByPersonId { get; set; }
    public string? DisposalReviewRef { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrDisposalReviewEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string DisposalReviewId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string RetentionStatusRef { get; set; } = string.Empty;
    public string ProposedAction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public string RequestedByPersonId { get; set; } = string.Empty;
    public string? ReviewedByPersonId { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrDestructionCertificateEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string DestructionCertificateId { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string RetentionStatusRef { get; set; } = string.Empty;
    public string DisposalReviewRef { get; set; } = string.Empty;
    public string DispositionAction { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
    public string ExecutedByPersonId { get; set; } = string.Empty;
    public string CertificateHash { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrRetentionSchedulerRunEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string SchedulerRunId { get; set; } = string.Empty;
    public string LeaseId { get; set; } = string.Empty;
    public DateTimeOffset RanAt { get; set; }
    public string RequestedByPersonId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ExecutionPolicy { get; set; } = string.Empty;
    public int EvaluatedRecordCount { get; set; }
    public int EligibleRecordCount { get; set; }
    public int CreatedReviewCount { get; set; }
    public int SkippedExistingReviewCount { get; set; }
    public int BlockedByLegalHoldCount { get; set; }
    public int AutomaticExecutionCount { get; set; }
    public int NotificationMessageCount { get; set; }
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrRetentionSchedulerLeaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string LeaseId { get; set; } = string.Empty;
    public string SchedulerKey { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset AcquiredAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
    public string AcquiredByPersonId { get; set; } = string.Empty;
    public string? SchedulerRunId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrRetentionSchedulerOutboxMessageEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string OutboxMessageId { get; set; } = string.Empty;
    public string SchedulerRunId { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TargetRecordId { get; set; } = string.Empty;
    public string? DisposalReviewRef { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedByPersonId { get; set; } = string.Empty;
    public string DeduplicationKey { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int DeliveryAttemptCount { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
    public string? DeliveredByPersonId { get; set; }
    public string DeliveryChannel { get; set; } = string.Empty;
    public string RecipientRef { get; set; } = string.Empty;
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? EscalateAfter { get; set; }
    public int EscalationLevel { get; set; }
    public string? EscalatedToRecipientRef { get; set; }
    public DateTimeOffset? EscalatedAt { get; set; }
    public string? ExternalProviderRef { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrLegalHoldEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string LegalHoldId { get; set; } = string.Empty;
    public string HoldNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string HoldType { get; set; } = string.Empty;
    public string SourceProduct { get; set; } = string.Empty;
    public string SourceObjectType { get; set; } = string.Empty;
    public string SourceObjectId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
    public string? ReleasedByPersonId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrControlledDocumentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ControlledDocumentId { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DocumentClass { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentSubtype { get; set; } = string.Empty;
    public string ControlledDocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string OwnerPersonId { get; set; } = string.Empty;
    public string DepartmentOrgUnitId { get; set; } = string.Empty;
    public string StaffarrSiteId { get; set; } = string.Empty;
    public string CurrentVersionId { get; set; } = string.Empty;
    public DateTimeOffset? NextReviewAt { get; set; }
    public DateTimeOffset? EffectiveAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool AcknowledgementRequired { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrControlledDocumentVersionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string VersionId { get; set; } = string.Empty;
    public string ControlledDocumentId { get; set; } = string.Empty;
    public int VersionNumber { get; set; }
    public string VersionLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset? SubmittedForReviewAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? ApprovedByPersonId { get; set; }
    public DateTimeOffset? EffectiveAt { get; set; }
    public DateTimeOffset? SupersededAt { get; set; }
    public string? PreviousVersionRef { get; set; }
    public string? NextVersionRef { get; set; }
    public string? FileRef { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrDocumentReviewEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string DocumentReviewId { get; set; } = string.Empty;
    public string ControlledDocumentId { get; set; } = string.Empty;
    public string VersionId { get; set; } = string.Empty;
    public string ReviewType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RequestedByPersonId { get; set; } = string.Empty;
    public string ReviewerPersonId { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrDocumentDistributionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string DistributionId { get; set; } = string.Empty;
    public string ControlledDocumentId { get; set; } = string.Empty;
    public string VersionId { get; set; } = string.Empty;
    public string DistributionType { get; set; } = string.Empty;
    public string TargetRef { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? DistributedAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public string? AcknowledgementRef { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrDocumentAcknowledgementEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string AcknowledgementId { get; set; } = string.Empty;
    public string ControlledDocumentId { get; set; } = string.Empty;
    public string VersionId { get; set; } = string.Empty;
    public string PersonId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public string? SignatureRecordRef { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrAccessPolicyEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string AccessPolicyId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrAccessGrantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string AccessGrantId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string GranteeType { get; set; } = string.Empty;
    public string GranteeRef { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string GrantedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset GrantedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrExternalShareEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ExternalShareId { get; set; } = string.Empty;
    public string ShareNumber { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string SharePurpose { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public DateTimeOffset? LastAccessedAt { get; set; }
    public int AccessCount { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrRedactionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string RedactionId { get; set; } = string.Empty;
    public string SourceRecordId { get; set; } = string.Empty;
    public string RedactedRecordId { get; set; } = string.Empty;
    public string RedactionReason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RedactedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset RedactedAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrRedactionProviderJobEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string ProviderJobId { get; set; } = string.Empty;
    public string RedactionId { get; set; } = string.Empty;
    public string SourceRecordId { get; set; } = string.Empty;
    public string RedactedRecordId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderJobRef { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RequestedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public string RedactionPackageHash { get; set; } = string.Empty;
    public string SubmissionEvidenceHash { get; set; } = string.Empty;
    public DateTimeOffset? LastSubmittedAt { get; set; }
    public string? ProviderCallbackStatus { get; set; }
    public string? ProviderCallbackRef { get; set; }
    public DateTimeOffset? ProviderCallbackReceivedAt { get; set; }
    public string? ProviderEvidenceHash { get; set; }
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrSignatureRecordEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string SignatureRecordId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string SignaturePurpose { get; set; } = string.Empty;
    public string? SignerPersonId { get; set; }
    public string? SignerExternalName { get; set; }
    public string? SignerTitle { get; set; }
    public string SignatureFileRef { get; set; } = string.Empty;
    public DateTimeOffset SignedAt { get; set; }
    public string CapturedByPersonId { get; set; } = string.Empty;
    public string SourceProduct { get; set; } = string.Empty;
    public string SourceObjectRef { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrSignatureTrustServiceJobEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string TrustServiceJobId { get; set; } = string.Empty;
    public string SignatureRecordId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderEnvelopeRef { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RequestedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset RequestedAt { get; set; }
    public string CertificateFingerprintSha256 { get; set; } = string.Empty;
    public string SignatureEvidenceHash { get; set; } = string.Empty;
    public string SubmissionEvidenceHash { get; set; } = string.Empty;
    public DateTimeOffset? LastSubmittedAt { get; set; }
    public string? ProviderCallbackStatus { get; set; }
    public string? ProviderCallbackRef { get; set; }
    public DateTimeOffset? ProviderCallbackReceivedAt { get; set; }
    public string? ProviderCallbackEvidenceHash { get; set; }
    public string? TrustTimestampAuthorityRef { get; set; }
    public string? LongTermValidationStatus { get; set; }
    public string? FailureReason { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrPhotoEvidenceEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string PhotoEvidenceId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string PhotoPurpose { get; set; } = string.Empty;
    public string SourceProduct { get; set; } = string.Empty;
    public string SourceObjectRef { get; set; } = string.Empty;
    public DateTimeOffset CapturedAt { get; set; }
    public string CapturedByPersonId { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrAccessLogEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string AccessLogId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string? ActorPersonId { get; set; }
    public string? ActorServiceClientId { get; set; }
    public string? ExternalShareId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? ReasonCode { get; set; }
    public string? PreviousAccessLogHash { get; set; }
    public string AccessLogHash { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrAccessHistorySealEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string AccessHistorySealId { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public int SealedAccessLogCount { get; set; }
    public string? FirstAccessLogId { get; set; }
    public string? SealedThroughAccessLogId { get; set; }
    public string? SealedThroughAccessLogHash { get; set; }
    public string SealHash { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SealedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset SealedAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public string? IssueSummary { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrAuditEventEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string AuditEventId { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string ActorType { get; set; } = string.Empty;
    public string? ActorPersonId { get; set; }
    public string? ActorServiceClientId { get; set; }
    public string? ExternalShareId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? ReasonCode { get; set; }
    public string? CorrelationId { get; set; }
    public string? PreviousEventHash { get; set; }
    public string EventHash { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}

public sealed class RecordArrAuditSealEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string AuditSealId { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public string Scope { get; set; } = string.Empty;
    public int SealedEventCount { get; set; }
    public string? FirstAuditEventId { get; set; }
    public string? SealedThroughAuditEventId { get; set; }
    public string? SealedThroughEventHash { get; set; }
    public string SealHash { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SealedByPersonId { get; set; } = string.Empty;
    public DateTimeOffset SealedAt { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public string? IssueSummary { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}
