using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RecordArr.Api.Data;
using RecordArr.Api.Models;
using RecordArr.Api.Options;
using RecordArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace STLCompliance.OpenApi.Tests;

public sealed class RecordArrStoreTests
{
    private const string DefaultTenantId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void CreateFile_attaches_file_to_record_and_updates_current_file_ref()
    {
        var store = CreateStore();
        var principal = CreatePrincipal(personId: "person-record-admin");

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
        var store = CreateStore();
        var ownerPersonId = Guid.NewGuid().ToString("D");
        var principal = CreatePrincipal(personId: ownerPersonId);
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
            ownerPersonId,
            ownerPersonId,
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
    public void File_integrity_checks_persist_pass_fail_and_tenant_scope()
    {
        var store = CreateStore();
        var principal = CreatePrincipal(personId: "person-record-admin");

        var file = store.CreateFile(
            "rec-bol-001",
            "integrity-bol.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/integrity-bol.pdf",
            8192);

        var passed = store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", file.ChecksumSha256, "metadata_checksum");
        var failed = store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", "sha256-mismatch", "object_hash");

        Assert.Equal("passed", passed.Status);
        Assert.Null(passed.FailureReason);
        Assert.Equal("failed", failed.Status);
        Assert.Equal("checksum_mismatch", failed.FailureReason);
        Assert.Equal("object_hash", failed.CheckMethod);

        var checks = store.GetFileIntegrityChecks(DefaultTenantId, file.FileId);
        Assert.Contains(checks, check => check.IntegrityCheckId == passed.IntegrityCheckId);
        Assert.Contains(checks, check => check.IntegrityCheckId == failed.IntegrityCheckId);
        Assert.DoesNotContain(
            store.GetFileIntegrityChecks(Guid.NewGuid().ToString(), file.FileId),
            check => check.IntegrityCheckId == passed.IntegrityCheckId || check.IntegrityCheckId == failed.IntegrityCheckId);
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, file.RecordId),
            log => log.Action == "file.integrity_check" && log.ReasonCode == "failed");
        var auditEvents = store.GetAuditEvents(DefaultTenantId, file.RecordId);
        var passedAudit = Assert.Single(auditEvents, auditEvent => auditEvent.Action == "file.integrity_check" && auditEvent.ReasonCode == "passed");
        var failedAudit = Assert.Single(auditEvents, auditEvent => auditEvent.Action == "file.integrity_check" && auditEvent.ReasonCode == "failed");
        Assert.Equal("person", passedAudit.ActorType);
        Assert.Equal("allowed", failedAudit.Outcome);
        Assert.Equal(passedAudit.EventHash, failedAudit.PreviousEventHash);
        Assert.False(string.IsNullOrWhiteSpace(failedAudit.CorrelationId));
        Assert.DoesNotContain(
            store.GetAuditEvents(Guid.NewGuid().ToString(), file.RecordId),
            auditEvent => auditEvent.AuditEventId == passedAudit.AuditEventId || auditEvent.AuditEventId == failedAudit.AuditEventId);
        Assert.NotNull(store.GetFile(principal, file.FileId));
    }

    [Fact]
    public void Object_store_index_and_fixity_history_persist_and_remain_tenant_scoped()
    {
        var dbName = $"recordarr-object-store-index-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string fileId;
        string recordId;
        string failedCheckId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var file = store.CreateFile(
                "rec-bol-001",
                "object-index.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/object-index.pdf",
                8192);
            fileId = file.FileId;
            recordId = file.RecordId;

            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", file.ChecksumSha256, "metadata_checksum");
            var failed = store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", "sha256-object-index-mismatch", "object_hash");
            failedCheckId = failed.IntegrityCheckId;

            var index = Assert.Single(store.GetObjectStoreObjects(DefaultTenantId, fileId));
            Assert.Equal("failed", index.Status);
            Assert.Equal(file.StorageKey, index.StorageKey);
            Assert.Equal(failedCheckId, index.LastIntegrityCheckRef);
            Assert.Equal("checksum_mismatch", index.FailureReason);

            var observations = store.GetObjectStoreFixityObservations(DefaultTenantId, fileId);
            Assert.Contains(observations, item => item.Status == "indexed" && item.ObservationSource == "file_created");
            Assert.Contains(observations, item => item.Status == "passed" && item.IntegrityCheckRef is not null);
            Assert.Contains(observations, item => item.Status == "failed" && item.IntegrityCheckRef == failedCheckId);
            Assert.DoesNotContain(store.GetObjectStoreObjects(Guid.NewGuid().ToString(), fileId), item => item.FileId == fileId);
            Assert.DoesNotContain(store.GetObjectStoreFixityObservations(Guid.NewGuid().ToString(), fileId), item => item.FileId == fileId);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var persistedIndex = Assert.Single(recreated.GetObjectStoreObjects(DefaultTenantId, fileId));
            Assert.Equal(recordId, persistedIndex.RecordId);
            Assert.Equal("failed", persistedIndex.Status);
            Assert.Equal(failedCheckId, persistedIndex.LastIntegrityCheckRef);

            var persistedHistory = recreated.GetObjectStoreFixityObservations(DefaultTenantId, fileId);
            Assert.Contains(persistedHistory, item => item.Status == "indexed");
            Assert.Contains(persistedHistory, item => item.Status == "failed" && item.IntegrityCheckRef == failedCheckId);
        }
    }

    [Fact]
    public void Object_store_lifecycle_verification_requires_provider_evidence_and_persists_policy_scope()
    {
        var dbName = $"recordarr-object-lifecycle-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string fileId;
        string recordId;
        string evidenceHash;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var file = store.CreateFile(
                "rec-bol-001",
                "object-lifecycle.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/object-lifecycle.pdf",
                8192);
            fileId = file.FileId;
            recordId = file.RecordId;

            Assert.Throws<InvalidOperationException>(() => store.VerifyObjectStoreLifecycle(
                DefaultTenantId,
                fileId,
                "person-record-admin",
                providerName: " ",
                policyRef: "worm-policy-001",
                retentionMode: "compliance",
                retainUntil: DateTimeOffset.UtcNow.AddYears(2),
                encryptionKeyRef: "kms-key-001",
                evidenceRef: "provider-attestation-001"));

            var failed = store.VerifyObjectStoreLifecycle(
                DefaultTenantId,
                fileId,
                "person-record-admin",
                providerName: "aws-s3-object-lock",
                policyRef: "worm-policy-001",
                retentionMode: "compliance",
                retainUntil: DateTimeOffset.UtcNow.AddDays(1),
                encryptionKeyRef: "kms-key-001",
                evidenceRef: "provider-attestation-too-short");

            Assert.Equal("failed", failed.Status);
            Assert.Equal("retention_policy_not_satisfied", failed.FailureReason);
            Assert.Equal("failed", failed.ObjectStoreObject.LifecycleStatus);
            Assert.Equal("retention_policy_not_satisfied", failed.ObjectStoreObject.LifecycleFailureReason);

            var verified = store.VerifyObjectStoreLifecycle(
                DefaultTenantId,
                fileId,
                "person-record-admin",
                providerName: "aws-s3-object-lock",
                policyRef: "worm-policy-001",
                retentionMode: "compliance",
                retainUntil: DateTimeOffset.UtcNow.AddYears(2),
                encryptionKeyRef: "kms-key-001",
                evidenceRef: "provider-attestation-002");

            Assert.Equal("verified", verified.Status);
            Assert.Null(verified.FailureReason);
            Assert.Equal("aws-s3-object-lock", verified.ProviderName);
            Assert.Equal("worm-policy-001", verified.PolicyRef);
            Assert.Equal("compliance", verified.RetentionMode);
            Assert.Equal("kms-key-001", verified.EncryptionKeyRef);
            Assert.Equal("provider-attestation-002", verified.EvidenceRef);
            Assert.False(string.IsNullOrWhiteSpace(verified.EvidenceHash));
            evidenceHash = verified.EvidenceHash;

            var objectIndex = Assert.Single(store.GetObjectStoreObjects(DefaultTenantId, fileId));
            Assert.Equal("verified", objectIndex.LifecycleStatus);
            Assert.Equal(evidenceHash, objectIndex.LifecycleEvidenceHash);
            Assert.Equal("provider-attestation-002", objectIndex.LifecycleEvidenceRef);
            Assert.DoesNotContain(store.GetObjectStoreObjects(Guid.NewGuid().ToString(), fileId), item => item.FileId == fileId);

            var observations = store.GetObjectStoreFixityObservations(DefaultTenantId, fileId);
            Assert.Contains(observations, item =>
                item.ObservationSource == "object_lifecycle_verification" &&
                item.LifecycleStatus == "verified" &&
                item.LifecycleEvidenceHash == evidenceHash);
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, recordId),
                log => log.Action == "object_store.lifecycle_verified" && log.Result == "allowed" && log.ReasonCode == "worm-policy-001");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, recordId),
                log => log.Action == "object_store.lifecycle_verified" && log.Result == "denied" && log.ReasonCode == "retention_policy_not_satisfied");
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var persisted = Assert.Single(recreated.GetObjectStoreObjects(DefaultTenantId, fileId));
            Assert.Equal(recordId, persisted.RecordId);
            Assert.Equal("verified", persisted.LifecycleStatus);
            Assert.Equal(evidenceHash, persisted.LifecycleEvidenceHash);
            Assert.Equal("aws-s3-object-lock", persisted.LifecycleProviderName);

            Assert.Contains(
                recreated.GetObjectStoreFixityObservations(DefaultTenantId, fileId),
                item => item.ObservationSource == "object_lifecycle_verification" && item.LifecycleEvidenceHash == evidenceHash);
        }
    }

    [Fact]
    public void Audit_integrity_verification_detects_tampered_durable_events()
    {
        var dbName = $"recordarr-audit-integrity-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string recordId;
        string tamperedAuditEventId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var file = store.CreateFile(
                "rec-bol-001",
                "audit-integrity.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/audit-integrity.pdf",
                8192);
            recordId = file.RecordId;

            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", file.ChecksumSha256, "metadata_checksum");
            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", "sha256-mismatch", "object_hash");

            var verified = store.VerifyAuditIntegrity(DefaultTenantId);
            Assert.Equal("verified", verified.Status);
            Assert.True(verified.CheckedEvents >= 2);
            Assert.Empty(verified.BrokenAuditEventIds);

            var scoped = store.VerifyAuditIntegrity(DefaultTenantId, recordId);
            Assert.Equal("verified", scoped.Status);
            Assert.True(scoped.CheckedEvents >= 2);
            Assert.DoesNotContain(
                store.GetAuditEvents(Guid.NewGuid().ToString(), recordId),
                auditEvent => auditEvent.RecordId == recordId);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var tampered = db.RecordArrAuditEvents.First(row =>
                row.TenantId == Guid.Parse(DefaultTenantId) &&
                row.Action == "file.integrity_check");
            tamperedAuditEventId = tampered.AuditEventId;
            var originalHash = tampered.EventHash;
            tampered.EventHash = "tampered";
            tampered.PayloadJson = tampered.PayloadJson.Replace(originalHash, "tampered", StringComparison.Ordinal);
            db.SaveChanges();
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var broken = recreated.VerifyAuditIntegrity(DefaultTenantId);

            Assert.Equal("broken", broken.Status);
            Assert.Contains(tamperedAuditEventId, broken.BrokenAuditEventIds);
            Assert.Contains("failed hash-chain verification", broken.IssueSummary);
            Assert.DoesNotContain(
                recreated.VerifyAuditIntegrity(Guid.NewGuid().ToString()).BrokenAuditEventIds,
                auditEventId => auditEventId == tamperedAuditEventId);
        }
    }

    [Fact]
    public void Access_history_integrity_verification_detects_tampered_durable_access_logs()
    {
        var dbName = $"recordarr-access-integrity-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string recordId;
        string tamperedAccessLogId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var file = store.CreateFile(
                "rec-bol-001",
                "access-integrity.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/access-integrity.pdf",
                8192);
            recordId = file.RecordId;

            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", file.ChecksumSha256, "metadata_checksum");
            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", "sha256-mismatch", "object_hash");

            var verified = store.VerifyAccessHistoryIntegrity(DefaultTenantId);
            Assert.Equal("verified", verified.Status);
            Assert.True(verified.CheckedAccessLogs >= 2);
            Assert.Empty(verified.BrokenAccessLogIds);

            var scoped = store.VerifyAccessHistoryIntegrity(DefaultTenantId, recordId);
            Assert.Equal("verified", scoped.Status);
            Assert.True(scoped.CheckedAccessLogs >= 2);

            var accessLogs = store.GetAccessLogs(DefaultTenantId, recordId);
            Assert.All(accessLogs, accessLog => Assert.False(string.IsNullOrWhiteSpace(accessLog.AccessLogHash)));
            Assert.DoesNotContain(
                store.GetAccessLogs(Guid.NewGuid().ToString(), recordId),
                accessLog => accessLog.RecordId == recordId);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var tampered = db.RecordArrAccessLogs.First(row =>
                row.TenantId == Guid.Parse(DefaultTenantId) &&
                row.Action == "file.integrity_check");
            tamperedAccessLogId = tampered.AccessLogId;
            var originalHash = tampered.AccessLogHash;
            tampered.AccessLogHash = "tampered-access-log";
            tampered.PayloadJson = tampered.PayloadJson.Replace(originalHash, "tampered-access-log", StringComparison.Ordinal);
            db.SaveChanges();
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var broken = recreated.VerifyAccessHistoryIntegrity(DefaultTenantId);

            Assert.Equal("broken", broken.Status);
            Assert.Contains(tamperedAccessLogId, broken.BrokenAccessLogIds);
            Assert.Contains("failed hash-chain verification", broken.IssueSummary);
            Assert.DoesNotContain(
                recreated.VerifyAccessHistoryIntegrity(Guid.NewGuid().ToString()).BrokenAccessLogIds,
                accessLogId => accessLogId == tamperedAccessLogId);
        }
    }

    [Fact]
    public void Access_history_seals_persist_and_detect_tampered_sealed_access_logs()
    {
        var dbName = $"recordarr-access-history-seal-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string accessHistorySealId;
        string sealedAccessLogId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var record = store.CreateRecord(
                DefaultTenantId,
                "Access history seal source",
                "Source file retained for access-history seal verification.",
                "document",
                "other",
                "access_history_seal",
                "uploaded",
                "internal",
                "recordarr",
                "access_history_seal_test",
                "seal-001",
                "access-history-seal.pdf",
                "person-route-lead",
                "person-route-lead",
                "access-history-seal.pdf",
                "application/pdf",
                "recordarr",
                "tenant/access-history-seal.pdf",
                8192);
            var file = store.CreateFile(
                record.RecordId,
                "access-history-seal-extra.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/access-history-seal-extra.pdf",
                8192);

            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", file.ChecksumSha256, "metadata_checksum");
            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", "sha256-mismatch", "object_hash");

            var seal = store.SealAccessHistory(DefaultTenantId, record.RecordId, "person-record-admin");
            accessHistorySealId = seal.AccessHistorySealId;
            sealedAccessLogId = Assert.IsType<string>(seal.SealedThroughAccessLogId);

            Assert.Equal("record", seal.Scope);
            Assert.Equal("sealed", seal.Status);
            Assert.True(seal.SealedAccessLogCount >= 2);
            Assert.False(string.IsNullOrWhiteSpace(seal.SealedThroughAccessLogHash));
            Assert.StartsWith("ahseal-", seal.AccessHistorySealId);

            var verified = store.VerifyAccessHistorySeal(DefaultTenantId, accessHistorySealId);
            Assert.Equal("verified", verified.Status);
            Assert.Null(verified.IssueSummary);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var verified = recreated.VerifyAccessHistorySeal(DefaultTenantId, accessHistorySealId);

            Assert.Equal("verified", verified.Status);
            Assert.Contains(recreated.GetAccessHistorySeals(DefaultTenantId), seal => seal.AccessHistorySealId == accessHistorySealId);
            Assert.DoesNotContain(
                recreated.GetAccessHistorySeals(Guid.NewGuid().ToString()),
                seal => seal.AccessHistorySealId == accessHistorySealId);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var tampered = db.RecordArrAccessLogs.First(row =>
                row.TenantId == Guid.Parse(DefaultTenantId) &&
                row.AccessLogId == sealedAccessLogId);
            var originalHash = tampered.AccessLogHash;
            tampered.AccessLogHash = "tampered-sealed-access-log";
            tampered.PayloadJson = tampered.PayloadJson.Replace(originalHash, "tampered-sealed-access-log", StringComparison.Ordinal);
            db.SaveChanges();
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var broken = recreated.VerifyAccessHistorySeal(DefaultTenantId, accessHistorySealId);

            Assert.Equal("broken", broken.Status);
            Assert.Contains("sealed access-log range", broken.IssueSummary);
        }
    }

    [Fact]
    public void Audit_seals_persist_and_detect_tampered_sealed_events()
    {
        var dbName = $"recordarr-audit-seal-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string auditSealId;
        string sealedAuditEventId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var file = store.CreateFile(
                "rec-bol-001",
                "audit-seal.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/audit-seal.pdf",
                8192);

            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", file.ChecksumSha256, "metadata_checksum");
            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", "sha256-mismatch", "object_hash");

            var seal = store.SealAuditEvents(DefaultTenantId, recordId: null, "person-record-admin");
            auditSealId = seal.AuditSealId;
            sealedAuditEventId = Assert.IsType<string>(seal.SealedThroughAuditEventId);

            Assert.Equal("tenant", seal.Scope);
            Assert.Equal("sealed", seal.Status);
            Assert.True(seal.SealedEventCount >= 2);
            Assert.False(string.IsNullOrWhiteSpace(seal.SealHash));

            var verified = store.VerifyAuditSeal(DefaultTenantId, auditSealId);
            Assert.Equal("verified", verified.Status);
            Assert.Null(verified.IssueSummary);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var verified = recreated.VerifyAuditSeal(DefaultTenantId, auditSealId);

            Assert.Equal("verified", verified.Status);
            Assert.Contains(recreated.GetAuditSeals(DefaultTenantId), seal => seal.AuditSealId == auditSealId);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var tampered = db.RecordArrAuditEvents.First(row =>
                row.TenantId == Guid.Parse(DefaultTenantId) &&
                row.AuditEventId == sealedAuditEventId);
            var originalHash = tampered.EventHash;
            tampered.EventHash = "tampered-sealed-event";
            tampered.PayloadJson = tampered.PayloadJson.Replace(originalHash, "tampered-sealed-event", StringComparison.Ordinal);
            db.SaveChanges();
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var broken = recreated.VerifyAuditSeal(DefaultTenantId, auditSealId);

            Assert.Equal("broken", broken.Status);
            Assert.Contains("no longer matches", broken.IssueSummary);
        }
    }

    [Fact]
    public void Audit_seal_anchors_require_provider_evidence_persist_and_break_on_tamper()
    {
        var dbName = $"recordarr-audit-anchor-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string auditSealId;
        string sealedAuditEventId;
        string sealHash;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var file = store.CreateFile(
                "rec-bol-001",
                "audit-anchor.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/audit-anchor.pdf",
                8192);
            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", file.ChecksumSha256, "metadata_checksum");

            var seal = store.SealAuditEvents(DefaultTenantId, recordId: null, "person-record-admin");
            auditSealId = seal.AuditSealId;
            sealedAuditEventId = Assert.IsType<string>(seal.SealedThroughAuditEventId);
            sealHash = seal.SealHash;

            Assert.Throws<InvalidOperationException>(() => store.AnchorAuditSeal(
                DefaultTenantId,
                auditSealId,
                "person-record-admin",
                anchorProviderName: null,
                anchorReference: "tsa-anchor-7781",
                DateTimeOffset.UtcNow,
                seal.SealHash));

            var mismatch = store.AnchorAuditSeal(
                DefaultTenantId,
                auditSealId,
                "person-record-admin",
                "RecordArr TSA",
                "tsa-anchor-7781",
                DateTimeOffset.UtcNow,
                "sha256-anchor-mismatch");

            Assert.Equal("failed", mismatch.AnchorStatus);
            Assert.Equal("anchor_hash_mismatch", mismatch.AnchorFailureReason);
            Assert.Null(mismatch.AnchorEvidenceHash);

            var anchored = store.AnchorAuditSeal(
                DefaultTenantId,
                auditSealId,
                "person-record-admin",
                "RecordArr TSA",
                "tsa-anchor-7782",
                DateTimeOffset.UtcNow,
                seal.SealHash);

            Assert.Equal("verified", anchored.Status);
            Assert.Equal("anchored", anchored.AnchorStatus);
            Assert.Equal("RecordArr TSA", anchored.AnchorProviderName);
            Assert.Equal("tsa-anchor-7782", anchored.AnchorReference);
            Assert.Equal(seal.SealHash, anchored.AnchoredSealHash);
            Assert.False(string.IsNullOrWhiteSpace(anchored.AnchorEvidenceHash));
            Assert.Null(anchored.AnchorFailureReason);
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "audit.seal_anchored" && log.ReasonCode == auditSealId);
            Assert.DoesNotContain(
                store.GetAuditSeals(Guid.NewGuid().ToString()),
                item => item.AuditSealId == auditSealId);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var persisted = Assert.Single(recreated.GetAuditSeals(DefaultTenantId), seal => seal.AuditSealId == auditSealId);

            Assert.Equal("anchored", persisted.AnchorStatus);
            Assert.Equal("RecordArr TSA", persisted.AnchorProviderName);
            Assert.Equal(sealHash, persisted.AnchoredSealHash);
            Assert.False(string.IsNullOrWhiteSpace(persisted.AnchorEvidenceHash));
        }

        using (var db = new RecordArrDbContext(options))
        {
            var tampered = db.RecordArrAuditEvents.First(row =>
                row.TenantId == Guid.Parse(DefaultTenantId) &&
                row.AuditEventId == sealedAuditEventId);
            var originalHash = tampered.EventHash;
            tampered.EventHash = "tampered-anchor-sealed-event";
            tampered.PayloadJson = tampered.PayloadJson.Replace(originalHash, "tampered-anchor-sealed-event", StringComparison.Ordinal);
            db.SaveChanges();
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var broken = recreated.VerifyAuditSeal(DefaultTenantId, auditSealId);

            Assert.Equal("broken", broken.Status);
            Assert.Equal("broken", broken.AnchorStatus);
            Assert.Equal("sealed_range_no_longer_matches_anchor", broken.AnchorFailureReason);
        }
    }

    [Fact]
    public async Task Audit_anchor_manifest_provider_returns_only_unanchored_tenant_seals()
    {
        var databaseName = $"recordarr-audit-anchor-provider-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        var otherTenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        string sealId;
        string sealHash;
        string alreadyAnchoredSealId;
        string alreadyAnchoredSealHash;
        string otherTenantSealId;
        string otherTenantSealHash;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var file = store.CreateFile(
                "rec-bol-001",
                "audit-anchor-provider.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/audit-anchor-provider.pdf",
                8192);
            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", file.ChecksumSha256, "metadata_checksum");
            var seal = store.SealAuditEvents(DefaultTenantId, recordId: null, "person-record-admin");
            var alreadyAnchoredSeal = store.SealAuditEvents(DefaultTenantId, recordId: null, "person-record-admin");

            store.AnchorAuditSeal(
                DefaultTenantId,
                alreadyAnchoredSeal.AuditSealId,
                "person-record-admin",
                "RecordArr TSA",
                "tsa-already-anchored",
                DateTimeOffset.UtcNow,
                alreadyAnchoredSeal.SealHash);

            var otherTenantRecord = store.CreateRecord(
                otherTenantId,
                "Other tenant audit anchor source",
                "Validates tenant filtering for audit anchor manifests.",
                "document",
                "operations",
                "audit",
                "seal",
                "internal",
                "recordarr",
                "audit",
                "other-tenant-anchor-source",
                "Other Tenant Anchor Source",
                "person-other-owner",
                "person-other-owner",
                "other-anchor-source.pdf",
                "application/pdf");
            store.CreateFile(
                otherTenantRecord.RecordId,
                "other-anchor-evidence.pdf",
                "application/pdf",
                "person-other-owner",
                "recordarr",
                "tenant/other-anchor-evidence.pdf",
                4096);
            var otherTenantSeal = store.SealAuditEvents(otherTenantId, recordId: null, "person-other-owner");

            sealId = seal.AuditSealId;
            sealHash = seal.SealHash;
            alreadyAnchoredSealId = alreadyAnchoredSeal.AuditSealId;
            alreadyAnchoredSealHash = alreadyAnchoredSeal.SealHash;
            otherTenantSealId = otherTenantSeal.AuditSealId;
            otherTenantSealHash = otherTenantSeal.SealHash;
        }

        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-audit-anchor-provider-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        manifests = new object[]
                        {
                            new
                            {
                                tenantId = otherTenantId,
                                auditSealId = otherTenantSealId,
                                anchorProviderName = "RecordArr TSA",
                                anchorReference = "tsa-other-tenant",
                                anchoredAt = DateTimeOffset.UtcNow,
                                anchoredSealHash = otherTenantSealHash
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                auditSealId = alreadyAnchoredSealId,
                                anchorProviderName = "RecordArr TSA",
                                anchorReference = "tsa-already-anchored",
                                anchoredAt = DateTimeOffset.UtcNow,
                                anchoredSealHash = alreadyAnchoredSealHash
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                auditSealId = "unknown-seal",
                                anchorProviderName = "RecordArr TSA",
                                anchorReference = "tsa-unknown",
                                anchoredAt = DateTimeOffset.UtcNow,
                                anchoredSealHash = sealHash
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                auditSealId = sealId,
                                anchorProviderName = " ",
                                anchorReference = "tsa-missing-provider",
                                anchoredAt = DateTimeOffset.UtcNow,
                                anchoredSealHash = sealHash
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                auditSealId = sealId,
                                anchorProviderName = "RecordArr TSA",
                                anchorReference = "tsa-valid",
                                anchoredAt = DateTimeOffset.UtcNow,
                                anchoredSealHash = sealHash
                            }
                        }
                    },
                    JsonOptions));

            using var verifyScope = provider.CreateScope();
            var store = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var options = new StaticOptionsMonitor<AuditAnchorWorkerOptions>(new AuditAnchorWorkerOptions
            {
                Enabled = true,
                TenantIds = [DefaultTenantId],
                ManifestPath = manifestPath
            });
            var manifestProvider = new ManifestRecordArrAuditAnchorManifestProvider(
                options,
                NullLogger<ManifestRecordArrAuditAnchorManifestProvider>.Instance);

            var manifests = await manifestProvider.GetManifestsAsync(
                DefaultTenantId,
                store.GetAuditSeals(DefaultTenantId),
                CancellationToken.None);

            var manifest = Assert.Single(manifests);
            Assert.Equal(sealId, manifest.AuditSealId);
            Assert.Equal("RecordArr TSA", manifest.AnchorProviderName);
            Assert.Equal("tsa-valid", manifest.AnchorReference);
            Assert.Equal(sealHash, manifest.AnchoredSealHash);
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }
    }

    [Fact]
    public async Task Audit_anchor_worker_requires_explicit_manifest_and_processes_only_matching_tenant_seals()
    {
        var databaseName = $"recordarr-audit-anchor-worker-{Guid.NewGuid():N}";
        await using var provider = CreateStoreProvider(databaseName);
        var otherTenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        string sealId;
        string sealHash;
        string otherTenantSealId;
        string otherTenantSealHash;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var file = store.CreateFile(
                "rec-bol-001",
                "audit-anchor-worker.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/audit-anchor-worker.pdf",
                8192);
            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", file.ChecksumSha256, "metadata_checksum");
            var seal = store.SealAuditEvents(DefaultTenantId, recordId: null, "person-record-admin");

            var otherTenantRecord = store.CreateRecord(
                otherTenantId,
                "Other tenant audit anchor worker source",
                "Validates tenant filtering for audit anchor worker.",
                "document",
                "operations",
                "audit",
                "seal",
                "internal",
                "recordarr",
                "audit",
                "other-tenant-anchor-worker-source",
                "Other Tenant Anchor Worker Source",
                "person-other-owner",
                "person-other-owner",
                "other-anchor-worker-source.pdf",
                "application/pdf");
            store.CreateFile(
                otherTenantRecord.RecordId,
                "other-anchor-worker-evidence.pdf",
                "application/pdf",
                "person-other-owner",
                "recordarr",
                "tenant/other-anchor-worker-evidence.pdf",
                4096);
            var otherTenantSeal = store.SealAuditEvents(otherTenantId, recordId: null, "person-other-owner");

            sealId = seal.AuditSealId;
            sealHash = seal.SealHash;
            otherTenantSealId = otherTenantSeal.AuditSealId;
            otherTenantSealHash = otherTenantSeal.SealHash;
        }

        var missingManifestOptions = new StaticOptionsMonitor<AuditAnchorWorkerOptions>(new AuditAnchorWorkerOptions
        {
            Enabled = true,
            TenantIds = [DefaultTenantId],
            RequestedByPersonId = "recordarr-audit-anchor-worker"
        });
        var missingWorker = new RecordArrAuditAnchorWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new ManifestRecordArrAuditAnchorManifestProvider(
                missingManifestOptions,
                NullLogger<ManifestRecordArrAuditAnchorManifestProvider>.Instance),
            missingManifestOptions,
            NullLogger<RecordArrAuditAnchorWorker>.Instance);

        await missingWorker.RunOnceAsync();

        using (var missingVerifyScope = provider.CreateScope())
        {
            var store = missingVerifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var seal = Assert.Single(store.GetAuditSeals(DefaultTenantId), item => item.AuditSealId == sealId);
            Assert.Null(seal.AnchorStatus);
        }

        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-audit-anchor-worker-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        manifests = new object[]
                        {
                            new
                            {
                                tenantId = otherTenantId,
                                auditSealId = otherTenantSealId,
                                anchorProviderName = "RecordArr TSA",
                                anchorReference = "tsa-other-tenant",
                                anchoredAt = DateTimeOffset.UtcNow,
                                anchoredSealHash = otherTenantSealHash
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                auditSealId = sealId,
                                anchorProviderName = "RecordArr TSA",
                                anchorReference = "tsa-worker",
                                anchoredAt = DateTimeOffset.UtcNow,
                                anchoredSealHash = sealHash
                            }
                        }
                    },
                    JsonOptions));

            var options = new AuditAnchorWorkerOptions
            {
                Enabled = true,
                TenantIds = [DefaultTenantId],
                ManifestPath = manifestPath,
                RequestedByPersonId = "recordarr-audit-anchor-worker"
            };
            var optionsMonitor = new StaticOptionsMonitor<AuditAnchorWorkerOptions>(options);
            var worker = new RecordArrAuditAnchorWorker(
                provider.GetRequiredService<IServiceScopeFactory>(),
                new ManifestRecordArrAuditAnchorManifestProvider(
                    optionsMonitor,
                    NullLogger<ManifestRecordArrAuditAnchorManifestProvider>.Instance),
                optionsMonitor,
                NullLogger<RecordArrAuditAnchorWorker>.Instance);

            await worker.RunOnceAsync();
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var anchored = Assert.Single(verifyStore.GetAuditSeals(DefaultTenantId), item => item.AuditSealId == sealId);
        var verifiedOtherTenantSeal = Assert.Single(verifyStore.GetAuditSeals(otherTenantId), item => item.AuditSealId == otherTenantSealId);

        Assert.Equal("anchored", anchored.AnchorStatus);
        Assert.Equal("RecordArr TSA", anchored.AnchorProviderName);
        Assert.Equal("tsa-worker", anchored.AnchorReference);
        Assert.Equal(sealHash, anchored.AnchoredSealHash);
        Assert.False(string.IsNullOrWhiteSpace(anchored.AnchorEvidenceHash));
        Assert.Null(anchored.AnchorFailureReason);
        Assert.Null(verifiedOtherTenantSeal.AnchorStatus);
        Assert.Contains(
            verifyStore.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
            log => log.Action == "audit.seal_anchored" &&
                   log.Result == "allowed" &&
                   log.ReasonCode == sealId);
    }

    [Fact]
    public void Audit_governance_reports_unsealed_coverage_and_broken_seals()
    {
        var dbName = $"recordarr-audit-governance-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string auditSealId;
        string sealedAuditEventId;
        string fileId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var file = store.CreateFile(
                "rec-bol-001",
                "audit-governance.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/audit-governance.pdf",
                8192);
            fileId = file.FileId;

            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", file.ChecksumSha256, "metadata_checksum");

            var unsealed = store.VerifyAuditGovernance(DefaultTenantId);
            Assert.Equal("unsealed", unsealed.Status);
            Assert.True(unsealed.UnsealedEventCount > 0);
            Assert.NotEmpty(unsealed.UnsealedAuditEventIds);
            Assert.Contains("not covered by a verified seal", unsealed.IssueSummary);

            var seal = store.SealAuditEvents(DefaultTenantId, recordId: null, "person-record-admin");
            auditSealId = seal.AuditSealId;
            sealedAuditEventId = Assert.IsType<string>(seal.SealedThroughAuditEventId);

            var verified = store.VerifyAuditGovernance(DefaultTenantId);
            Assert.Equal("verified", verified.Status);
            Assert.Equal(0, verified.UnsealedEventCount);
            Assert.Empty(verified.UnsealedAuditEventIds);
            Assert.Equal(1, verified.VerifiedSealCount);
            Assert.Equal(sealedAuditEventId, verified.LatestSealThroughAuditEventId);
            Assert.Null(verified.IssueSummary);
            Assert.DoesNotContain(
                store.VerifyAuditGovernance(Guid.NewGuid().ToString()).BrokenAuditSealIds,
                sealId => sealId == auditSealId);

            store.CreateFileIntegrityCheck(DefaultTenantId, file.FileId, "person-route-lead", "sha256-governance-mismatch", "object_hash");
            var newActivity = store.VerifyAuditGovernance(DefaultTenantId);
            Assert.Equal("unsealed", newActivity.Status);
            Assert.True(newActivity.UnsealedEventCount > 0);
            Assert.Equal(sealedAuditEventId, newActivity.LatestSealThroughAuditEventId);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var unsealedAfterRestart = recreated.VerifyAuditGovernance(DefaultTenantId);
            Assert.Equal("unsealed", unsealedAfterRestart.Status);
            Assert.Contains(auditSealId, recreated.GetAuditSeals(DefaultTenantId).Select(seal => seal.AuditSealId));
        }

        using (var db = new RecordArrDbContext(options))
        {
            var tampered = db.RecordArrAuditEvents.First(row =>
                row.TenantId == Guid.Parse(DefaultTenantId) &&
                row.AuditEventId == sealedAuditEventId);
            var originalHash = tampered.EventHash;
            tampered.EventHash = "tampered-governance-sealed-event";
            tampered.PayloadJson = tampered.PayloadJson.Replace(originalHash, "tampered-governance-sealed-event", StringComparison.Ordinal);
            db.SaveChanges();
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var broken = recreated.VerifyAuditGovernance(DefaultTenantId);

            Assert.Equal("broken", broken.Status);
            Assert.Contains(auditSealId, broken.BrokenAuditSealIds);
            Assert.Contains(sealedAuditEventId, broken.BrokenAuditEventIds);
            Assert.Contains("audit seal", broken.IssueSummary);

            var persistedSeal = Assert.Single(recreated.GetAuditSeals(DefaultTenantId), seal => seal.AuditSealId == auditSealId);
            Assert.Equal("broken", persistedSeal.Status);
            Assert.Equal(fileId, recreated.GetFile(CreatePrincipal(personId: "person-record-admin"), fileId)!.FileId);
        }
    }

    [Fact]
    public void File_malware_scans_gate_downloads_and_persist_quarantine_state()
    {
        var store = CreateStore();
        var principal = CreatePrincipal(personId: "person-record-admin");

        var file = store.CreateFile(
            "rec-bol-001",
            "malware-check.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/malware-check.pdf",
            8192);

        Assert.Equal("pending", file.VirusScanStatus);
        var pendingFailure = Assert.Throws<InvalidOperationException>(() => store.DownloadFile(principal, file.FileId));
        Assert.Contains("malware scan status is pending", pendingFailure.Message);

        var clean = store.CreateFileMalwareScan(
            DefaultTenantId,
            file.FileId,
            "person-route-lead",
            "clean",
            "clamav",
            "1.2.3",
            "sig-2026-06-28");

        Assert.Equal("clean", clean.Status);
        Assert.Equal("released", clean.QuarantineStatus);
        Assert.Contains("malware-check.pdf", store.DownloadFile(principal, file.FileId));

        var infected = store.CreateFileMalwareScan(
            DefaultTenantId,
            file.FileId,
            "person-route-lead",
            "infected",
            "clamav",
            "1.2.3",
            "sig-2026-06-28",
            "Eicar-Test-Signature");

        Assert.Equal("infected", infected.Status);
        Assert.Equal("quarantined", infected.QuarantineStatus);
        Assert.Equal("malware_detected:Eicar-Test-Signature", infected.FailureReason);
        var infectedFailure = Assert.Throws<InvalidOperationException>(() => store.DownloadFile(principal, file.FileId));
        Assert.Contains("malware scan status is infected", infectedFailure.Message);

        var scans = store.GetFileMalwareScans(DefaultTenantId, file.FileId);
        Assert.Contains(scans, scan => scan.MalwareScanId == clean.MalwareScanId);
        Assert.Contains(scans, scan => scan.MalwareScanId == infected.MalwareScanId);
        Assert.DoesNotContain(
            store.GetFileMalwareScans(Guid.NewGuid().ToString(), file.FileId),
            scan => scan.MalwareScanId == clean.MalwareScanId || scan.MalwareScanId == infected.MalwareScanId);
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, file.RecordId),
            log => log.Action == "file.malware_scan" && log.ReasonCode == "infected");
        Assert.Contains(
            store.GetAuditEvents(DefaultTenantId, file.RecordId),
            auditEvent => auditEvent.Action == "file.malware_scan" && auditEvent.ReasonCode == "infected");
    }

    [Fact]
    public void Malware_scan_provider_run_processes_pending_files_idempotently_and_preserves_quarantine()
    {
        var store = CreateStore();
        var principal = CreatePrincipal();
        var cleanFile = store.CreateFile(
            "rec-bol-001",
            "provider-clean.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/provider-clean.pdf",
            8192);
        var infectedFile = store.CreateFile(
            "rec-bol-001",
            "provider-infected.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/provider-infected.pdf",
            8192);
        var failedFile = store.CreateFile(
            "rec-bol-001",
            "provider-failed.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/provider-failed.pdf",
            8192);

        var run = store.RunFileMalwareScanProvider(
            DefaultTenantId,
            "person-route-lead",
            "clamav-worker",
            "1.2.4",
            "sig-2026-06-29",
            infectedFileIds: [infectedFile.FileId],
            failedFileIds: [failedFile.FileId]);

        Assert.Equal(4, run.PendingFileCount);
        Assert.Equal(4, run.ScannedFileCount);
        Assert.Equal(2, run.ReleasedFileCount);
        Assert.Equal(2, run.QuarantinedFileCount);
        Assert.Equal(1, run.FailedFileCount);
        Assert.Contains(cleanFile.FileId, run.ReleasedFileRefs);
        Assert.Contains(infectedFile.FileId, run.QuarantinedFileRefs);
        Assert.Contains(failedFile.FileId, run.FailedFileRefs);
        Assert.Contains("provider-clean.pdf", store.DownloadFile(principal, cleanFile.FileId));
        Assert.Throws<InvalidOperationException>(() => store.DownloadFile(principal, infectedFile.FileId));
        Assert.Throws<InvalidOperationException>(() => store.DownloadFile(principal, failedFile.FileId));

        var retry = store.RunFileMalwareScanProvider(
            DefaultTenantId,
            "person-route-lead",
            "clamav-worker",
            "1.2.4",
            "sig-2026-06-29",
            infectedFileIds: [infectedFile.FileId],
            failedFileIds: [failedFile.FileId]);

        Assert.Equal(0, retry.PendingFileCount);
        Assert.Equal(0, retry.ScannedFileCount);
        Assert.Equal(3, store.GetFileMalwareScans(DefaultTenantId).Count(scan =>
            scan.FileId == cleanFile.FileId ||
            scan.FileId == infectedFile.FileId ||
            scan.FileId == failedFile.FileId));
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
            log => log.Action == "file.malware_scan" && log.ReasonCode == "infected");
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
            log => log.Action == "file.malware_scan" && log.ReasonCode == "failed");
    }

    [Fact]
    public void Malware_scan_external_verdicts_preserve_missing_verdicts_and_reject_unknown_statuses()
    {
        var store = CreateStore();
        var principal = CreatePrincipal();
        var cleanFile = store.CreateFile(
            "rec-bol-001",
            "external-clean.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/external-clean.pdf",
            4096);
        var pendingFile = store.CreateFile(
            "rec-bol-001",
            "external-missing-verdict.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/external-missing-verdict.pdf",
            4096);

        var run = store.RunFileMalwareScanProviderVerdicts(
            DefaultTenantId,
            "recordarr-malware-worker",
            "clamav-sidecar",
            "1.3.0",
            "sig-2026-06-29",
            [
                new RecordArrMalwareScanProviderVerdict(cleanFile.FileId, "clean", null, null)
            ]);

        Assert.True(run.PendingFileCount >= 2);
        Assert.Equal(1, run.ScannedFileCount);
        Assert.Contains(cleanFile.FileId, run.ReleasedFileRefs);
        Assert.Equal("clean", store.GetFile(principal, cleanFile.FileId)?.VirusScanStatus);
        Assert.Equal("pending", store.GetFile(principal, pendingFile.FileId)?.VirusScanStatus);
        Assert.Contains("external-clean.pdf", store.DownloadFile(principal, cleanFile.FileId));
        var pendingFailure = Assert.Throws<InvalidOperationException>(() => store.DownloadFile(principal, pendingFile.FileId));
        Assert.Contains("malware scan status is pending", pendingFailure.Message);

        var invalidStatusFailure = Assert.Throws<InvalidOperationException>(() =>
            store.RunFileMalwareScanProviderVerdicts(
                DefaultTenantId,
                "recordarr-malware-worker",
                "clamav-sidecar",
                "1.3.0",
                "sig-2026-06-29",
                [
                    new RecordArrMalwareScanProviderVerdict(pendingFile.FileId, "unknown", null, null)
                ]));
        Assert.Contains("Unsupported Status 'unknown'", invalidStatusFailure.Message);
        Assert.Equal("pending", store.GetFile(principal, pendingFile.FileId)?.VirusScanStatus);
        Assert.DoesNotContain(
            store.GetFileMalwareScans(DefaultTenantId, pendingFile.FileId),
            scan => string.Equals(scan.Status, "unknown", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Malware_scan_manifest_provider_returns_only_pending_tenant_verdicts()
    {
        var store = CreateStore();
        var cleanFile = store.CreateFile(
            "rec-bol-001",
            "manifest-clean.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/manifest-clean.pdf",
            4096);
        var pendingFile = store.CreateFile(
            "rec-bol-001",
            "manifest-pending.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/manifest-pending.pdf",
            4096);
        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-malware-verdicts-{Guid.NewGuid():N}.json");

        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        verdicts = new object[]
                        {
                            new { tenantId = DefaultTenantId, fileId = cleanFile.FileId, status = "clean" },
                            new { tenantId = Guid.NewGuid().ToString(), fileId = pendingFile.FileId, status = "infected", threatName = "wrong-tenant" },
                            new { tenantId = DefaultTenantId, fileId = "file-not-pending", status = "clean" }
                        }
                    },
                    JsonOptions));

            var provider = new ManifestRecordArrMalwareScanVerdictProvider(
                new StaticOptionsMonitor<MalwareScanWorkerOptions>(new MalwareScanWorkerOptions
                {
                    Enabled = true,
                    VerdictManifestPath = manifestPath
                }),
                NullLogger<ManifestRecordArrMalwareScanVerdictProvider>.Instance);

            var verdicts = await provider.GetVerdictsAsync(
                DefaultTenantId,
                store.GetPendingMalwareScanFiles(DefaultTenantId),
                CancellationToken.None);

            var verdict = Assert.Single(verdicts);
            Assert.Equal(cleanFile.FileId, verdict.FileId);
            Assert.Equal("clean", verdict.Status);
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }
    }

    [Fact]
    public void Malware_scan_dead_lettering_is_durable_idempotent_and_recoverable()
    {
        var dbName = $"recordarr-malware-dead-letter-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var principal = CreatePrincipal();
        string fileId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var file = store.CreateFile(
                "rec-bol-001",
                "provider-dead-letter.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/provider-dead-letter.pdf",
                8192);
            fileId = file.FileId;
            store.RunFileMalwareScanProvider(
                DefaultTenantId,
                "person-route-lead",
                "clamav-worker",
                "1.2.4",
                "sig-2026-06-29",
                failedFileIds: [file.FileId]);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);

            var deadLetter = store.DeadLetterFailedMalwareScans(DefaultTenantId, "person-route-lead");

            Assert.Equal(1, deadLetter.EligibleFailedFileCount);
            Assert.Equal(1, deadLetter.DeadLetteredFileCount);
            Assert.Contains(fileId, deadLetter.DeadLetteredFileRefs);
            Assert.Contains(deadLetter.DeadLetterResults, scan =>
                scan.FileId == fileId &&
                scan.Status == "dead_lettered" &&
                scan.QuarantineStatus == "dead_lettered");
            Assert.Equal("dead_lettered", store.GetFile(principal, fileId)?.VirusScanStatus);
            Assert.Equal("dead_lettered", store.GetFile(principal, fileId)?.ProcessingStatus);
            var blockedDownload = Assert.Throws<InvalidOperationException>(() => store.DownloadFile(principal, fileId));
            Assert.Contains("malware scan status is dead_lettered", blockedDownload.Message);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);

            var retry = store.DeadLetterFailedMalwareScans(DefaultTenantId, "person-route-lead");

            Assert.Equal(0, retry.EligibleFailedFileCount);
            Assert.Equal(0, retry.DeadLetteredFileCount);
            Assert.Single(store.GetFileMalwareScans(DefaultTenantId, fileId), scan => scan.Status == "dead_lettered");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "file.malware_scan.dead_lettered" &&
                       log.Result == "denied" &&
                       log.ReasonCode!.StartsWith("malware_scan_dead_lettered", StringComparison.OrdinalIgnoreCase));

            store.CreateFileMalwareScan(DefaultTenantId, fileId, "person-route-lead", "clean", "clamav-worker", "1.2.5", "sig-2026-06-30");
            Assert.Equal("clean", store.GetFile(principal, fileId)?.VirusScanStatus);
            Assert.Contains("provider-dead-letter.pdf", store.DownloadFile(principal, fileId));
        }
    }

    [Fact]
    public void Clean_malware_scan_releases_a_quarantined_file_with_durable_evidence()
    {
        var store = CreateStore();
        var principal = CreatePrincipal();
        var file = store.CreateFile(
            "rec-bol-001",
            "false-positive.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/false-positive.pdf",
            8192);

        store.CreateFileMalwareScan(
            DefaultTenantId,
            file.FileId,
            "person-route-lead",
            "infected",
            "clamav",
            "1.2.4",
            "sig-2026-06-29",
            "FalsePositive.Test");
        Assert.Throws<InvalidOperationException>(() => store.DownloadFile(principal, file.FileId));

        var release = store.CreateFileMalwareScan(
            DefaultTenantId,
            file.FileId,
            "person-record-admin",
            "clean",
            "clamav",
            "1.2.4",
            "sig-2026-06-29");

        Assert.Equal("released", release.QuarantineStatus);
        Assert.Contains("false-positive.pdf", store.DownloadFile(principal, file.FileId));
        Assert.Equal("clean", store.GetFile(principal, file.FileId)?.VirusScanStatus);
        Assert.Equal("completed", store.GetFile(principal, file.FileId)?.ProcessingStatus);
    }

    [Fact]
    public void Storage_reconciliation_persists_fixity_counts_and_tenant_scope()
    {
        var store = CreateStore();

        var cleanFile = store.CreateFile(
            "rec-bol-001",
            "restore-clean.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/restore-clean.pdf",
            4096);
        store.CreateFileMalwareScan(DefaultTenantId, cleanFile.FileId, "person-route-lead", "clean");

        var corruptFile = store.CreateFile(
            "rec-bol-001",
            "restore-corrupt.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/restore-corrupt.pdf",
            4096);
        store.CreateFileMalwareScan(DefaultTenantId, corruptFile.FileId, "person-route-lead", "clean");

        var missingFile = store.CreateFile(
            "rec-bol-001",
            "restore-missing.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/restore-missing.pdf",
            4096);
        store.CreateFileMalwareScan(DefaultTenantId, missingFile.FileId, "person-route-lead", "clean");

        var reconciliation = store.RunStorageReconciliation(
            DefaultTenantId,
            "person-route-lead",
            "restore-test",
            "rec-bol-001",
            [missingFile.FileId],
            [corruptFile.FileId]);

        Assert.Equal("issues_found", reconciliation.Status);
        Assert.Equal("restore-test", reconciliation.Scope);
        Assert.Equal("open", reconciliation.RemediationStatus);
        Assert.True(reconciliation.TotalFiles >= 3);
        Assert.Equal(1, reconciliation.MissingFiles);
        Assert.Equal(1, reconciliation.CorruptFiles);
        Assert.Contains(missingFile.FileId, reconciliation.IssueFileRefs);
        Assert.Contains(corruptFile.FileId, reconciliation.IssueFileRefs);
        Assert.Contains("missing object", reconciliation.IssueSummary);
        Assert.Contains("corrupt object", reconciliation.IssueSummary);

        var persisted = store.GetStorageReconciliations(DefaultTenantId);
        Assert.Contains(persisted, item => item.ReconciliationId == reconciliation.ReconciliationId);
        Assert.DoesNotContain(
            store.GetStorageReconciliations(Guid.NewGuid().ToString()),
            item => item.ReconciliationId == reconciliation.ReconciliationId);
        Assert.Contains(
            store.GetStorageReconciliations(DefaultTenantId, "issues_found"),
            item => item.ReconciliationId == reconciliation.ReconciliationId);

        Assert.Contains(
            store.GetFileIntegrityChecks(DefaultTenantId, corruptFile.FileId),
            check => check.CheckMethod == "restore_verify" && check.Status == "failed");
        Assert.Contains(
            store.GetFileIntegrityChecks(DefaultTenantId, cleanFile.FileId),
            check => check.CheckMethod == "restore_verify" && check.Status == "passed");
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
            log => log.Action == "storage.reconciliation" && log.ReasonCode == "issues_found");
        Assert.Contains(
            store.GetAuditEvents(DefaultTenantId, "rec-bol-001"),
            auditEvent => auditEvent.Action == "storage.reconciliation" && auditEvent.ReasonCode == "issues_found");
        var history = store.GetObjectStoreFixityObservations(DefaultTenantId, recordId: "rec-bol-001", reconciliationId: reconciliation.ReconciliationId);
        Assert.Contains(history, item => item.FileId == missingFile.FileId && item.Status == "missing");
        Assert.Contains(history, item => item.FileId == corruptFile.FileId && item.Status == "failed");
        Assert.Contains(history, item => item.FileId == cleanFile.FileId && item.Status == "passed");
        Assert.Contains(
            store.GetObjectStoreObjects(DefaultTenantId, corruptFile.FileId),
            item => item.Status == "failed" && item.LastReconciliationRef == reconciliation.ReconciliationId);
    }

    [Fact]
    public void Storage_reconciliation_remediation_resolves_issue_refs_and_persists_evidence()
    {
        var store = CreateStore();
        var ownerPersonId = Guid.NewGuid().ToString("D");
        var principal = CreatePrincipal(personId: ownerPersonId);
        var record = store.CreateRecord(
            DefaultTenantId,
            "Storage remediation packet",
            "Isolated record for storage reconciliation remediation tests.",
            "evidence_package",
            "operations",
            "storage_reconciliation",
            "remediation",
            "internal",
            "recordarr",
            "storage_reconciliation",
            "storage-remediation-test",
            "Storage remediation packet",
            ownerPersonId,
            ownerPersonId,
            "storage-remediation-source.pdf",
            "application/pdf",
            "recordarr",
            "tenant/storage-remediation-source.pdf",
            4096);
        store.CreateFileMalwareScan(DefaultTenantId, record.CurrentFileRef, ownerPersonId, "clean");
        var corruptFile = store.CreateFile(
            record.RecordId,
            "remediate-corrupt.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/remediate-corrupt.pdf",
            4096);
        store.CreateFileMalwareScan(DefaultTenantId, corruptFile.FileId, "person-route-lead", "clean");
        var missingFile = store.CreateFile(
            record.RecordId,
            "remediate-missing.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/remediate-missing.pdf",
            4096);
        store.CreateFileMalwareScan(DefaultTenantId, missingFile.FileId, "person-route-lead", "clean");
        var quarantinedFile = store.CreateFile(
            record.RecordId,
            "remediate-quarantine.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/remediate-quarantine.pdf",
            4096);
        store.CreateFileMalwareScan(DefaultTenantId, quarantinedFile.FileId, "person-route-lead", "infected", threatName: "Eicar-Test-Signature");
        var pendingFile = store.CreateFile(
            record.RecordId,
            "remediate-pending.pdf",
            "application/pdf",
            "person-route-lead",
            "recordarr",
            "tenant/remediate-pending.pdf",
            4096);

        var reconciliation = store.RunStorageReconciliation(
            DefaultTenantId,
            "person-route-lead",
            "restore-test",
            record.RecordId,
            [missingFile.FileId],
            [corruptFile.FileId]);

        Assert.Contains(corruptFile.FileId, reconciliation.IssueFileRefs);
        Assert.Contains(missingFile.FileId, reconciliation.IssueFileRefs);
        Assert.Contains(quarantinedFile.FileId, reconciliation.IssueFileRefs);
        Assert.Contains(pendingFile.FileId, reconciliation.IssueFileRefs);

        var remediation = store.RemediateStorageReconciliation(
            DefaultTenantId,
            reconciliation.ReconciliationId,
            "person-record-admin",
            restoredFileIds: [missingFile.FileId],
            recheckedCorruptFileIds: [corruptFile.FileId],
            releasedQuarantinedFileIds: [quarantinedFile.FileId],
            scannedPendingFileIds: [pendingFile.FileId]);

        Assert.Equal("completed", remediation.RemediationStatus);
        Assert.Empty(remediation.RemainingIssueFileRefs);
        Assert.Contains(corruptFile.FileId, remediation.ResolvedFileRefs);
        Assert.Contains(missingFile.FileId, remediation.ResolvedFileRefs);
        Assert.Contains(quarantinedFile.FileId, remediation.ResolvedFileRefs);
        Assert.Contains(pendingFile.FileId, remediation.ResolvedFileRefs);
        Assert.Equal("passed", remediation.Reconciliation.Status);
        Assert.Equal("completed", store.GetStorageReconciliations(DefaultTenantId).Single(item => item.ReconciliationId == reconciliation.ReconciliationId).RemediationStatus);
        Assert.Contains("remediate-quarantine.pdf", store.DownloadFile(principal, quarantinedFile.FileId));
        Assert.Contains("remediate-pending.pdf", store.DownloadFile(principal, pendingFile.FileId));

        var retry = store.RemediateStorageReconciliation(
            DefaultTenantId,
            reconciliation.ReconciliationId,
            "person-record-admin",
            restoredFileIds: [missingFile.FileId],
            recheckedCorruptFileIds: [corruptFile.FileId],
            releasedQuarantinedFileIds: [quarantinedFile.FileId],
            scannedPendingFileIds: [pendingFile.FileId]);

        Assert.Equal("completed", retry.RemediationStatus);
        Assert.Empty(retry.RemainingIssueFileRefs);
        Assert.DoesNotContain(
            store.GetStorageReconciliations(Guid.NewGuid().ToString()),
            item => item.ReconciliationId == reconciliation.ReconciliationId);
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, record.RecordId),
            log => log.Action == "storage.reconciliation.restored" && log.ReasonCode == reconciliation.ReconciliationId);
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, record.RecordId),
            log => log.Action == "storage.reconciliation.scan_released" && log.ReasonCode == reconciliation.ReconciliationId);
        var history = store.GetObjectStoreFixityObservations(DefaultTenantId, recordId: record.RecordId, reconciliationId: reconciliation.ReconciliationId);
        Assert.Contains(history, item => item.FileId == missingFile.FileId && item.Status == "passed" && item.ObservationSource == "storage_remediation");
        Assert.Contains(history, item => item.FileId == corruptFile.FileId && item.Status == "passed" && item.ObservationSource == "storage_remediation");
        Assert.Contains(history, item => item.FileId == quarantinedFile.FileId && item.Status == "passed" && item.FailureReason == "quarantine_released_after_scan");
        Assert.Contains(history, item => item.FileId == pendingFile.FileId && item.Status == "passed" && item.FailureReason == "pending_scan_released");
        Assert.DoesNotContain(
            store.GetObjectStoreFixityObservations(Guid.NewGuid().ToString(), recordId: record.RecordId, reconciliationId: reconciliation.ReconciliationId),
            item => item.ReconciliationRef == reconciliation.ReconciliationId);
    }

    [Fact]
    public void Disaster_recovery_restore_runs_persist_fixity_evidence_and_tenant_scope()
    {
        var dbName = $"recordarr-dr-restore-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string runId;
        string recordId;
        string fileId;

        using (var context = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(context);
            var ownerPersonId = Guid.NewGuid().ToString("D");
            var record = store.CreateRecord(
                DefaultTenantId,
                "DR restore packet",
                "Isolated record for disaster recovery restore verification.",
                "evidence_package",
                "operations",
                "disaster_recovery",
                "restore",
                "internal",
                "recordarr",
                "disaster_recovery",
                "dr-restore-test",
                "DR restore packet",
                ownerPersonId,
                ownerPersonId,
                "dr-restore-source.pdf",
                "application/pdf",
                "recordarr",
                "tenant/dr-restore-source.pdf",
                4096);
            var file = store.CreateFile(
                record.RecordId,
                "dr-restored-evidence.pdf",
                "application/pdf",
                ownerPersonId,
                "recordarr",
                "tenant/dr-restored-evidence.pdf",
                2048);

            var run = store.RunDisasterRecoveryRestore(
                DefaultTenantId,
                ownerPersonId,
                "rp-2026-06-29T04:00Z",
                DateTimeOffset.UtcNow.AddMinutes(-5),
                60,
                30,
                [record.RecordId]);

            Assert.Equal("passed", run.Status);
            Assert.True(run.RpoMet);
            Assert.True(run.RtoMet);
            Assert.Contains(record.RecordId, run.RestoredRecordRefs);
            Assert.Contains(file.FileId, run.VerifiedFileRefs);
            Assert.DoesNotContain(file.FileId, run.FailedFileRefs);
            Assert.Contains("Verified", run.EvidenceSummary);
            Assert.Contains(
                store.GetObjectStoreFixityObservations(DefaultTenantId, file.FileId, reconciliationId: run.DisasterRecoveryRunId),
                item => item.ObservationSource == "disaster_recovery_restore" && item.Status == "passed");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, record.RecordId),
                log => log.Action == "disaster_recovery.restore" && log.ReasonCode == run.DisasterRecoveryRunId);

            runId = run.DisasterRecoveryRunId;
            recordId = record.RecordId;
            fileId = file.FileId;
        }

        using (var recreatedContext = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(recreatedContext);
            var persisted = Assert.Single(recreated.GetDisasterRecoveryRuns(DefaultTenantId), run => run.DisasterRecoveryRunId == runId);

            Assert.Equal("passed", persisted.Status);
            Assert.Contains(recordId, persisted.RestoredRecordRefs);
            Assert.Contains(fileId, persisted.VerifiedFileRefs);
            Assert.DoesNotContain(
                recreated.GetDisasterRecoveryRuns(Guid.NewGuid().ToString()),
                run => run.DisasterRecoveryRunId == runId);
            Assert.Contains(
                recreated.GetObjectStoreFixityObservations(DefaultTenantId, fileId, reconciliationId: runId),
                item => item.ObservationSource == "disaster_recovery_restore" && item.Status == "passed");
        }
    }

    [Fact]
    public void Disaster_recovery_restore_runs_fail_truthfully_for_stale_or_cross_tenant_recovery()
    {
        var store = CreateStore();
        var otherTenantId = Guid.NewGuid().ToString("D");
        var otherRecord = store.CreateRecord(
            otherTenantId,
            "Other tenant DR record",
            "This record must not be restored from the default tenant.",
            "evidence_package",
            "operations",
            "disaster_recovery",
            "restore",
            "internal",
            "recordarr",
            "disaster_recovery",
            "cross-tenant-dr-test",
            "Other tenant DR record",
            "person-other",
            "person-other",
            "other-dr-source.pdf",
            "application/pdf",
            "recordarr",
            "other-tenant/other-dr-source.pdf",
            1024);

        var staleRun = store.RunDisasterRecoveryRestore(
            DefaultTenantId,
            "person-record-admin",
            "rp-too-old",
            DateTimeOffset.UtcNow.AddHours(-6),
            60,
            30,
            ["rec-bol-001"]);

        Assert.Equal("rpo_missed", staleRun.Status);
        Assert.False(staleRun.RpoMet);
        Assert.Equal("rpo_missed", staleRun.FailureReason);
        Assert.Empty(staleRun.VerifiedFileRefs);
        Assert.Contains("RPO missed", staleRun.EvidenceSummary);

        var crossTenantRun = store.RunDisasterRecoveryRestore(
            DefaultTenantId,
            "person-record-admin",
            "rp-current",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            60,
            30,
            [otherRecord.RecordId]);

        Assert.Equal("failed", crossTenantRun.Status);
        Assert.Equal("record_not_found_or_cross_tenant", crossTenantRun.FailureReason);
        Assert.Contains(otherRecord.RecordId, crossTenantRun.BlockedRecordRefs);
        Assert.DoesNotContain(
            store.GetDisasterRecoveryRuns(otherTenantId),
            run => run.DisasterRecoveryRunId == crossTenantRun.DisasterRecoveryRunId);
    }

    [Fact]
    public void Disaster_recovery_backup_verification_requires_provider_evidence_and_persists_tenant_scope()
    {
        var dbName = $"recordarr-dr-backup-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string runId;
        string recordId;
        string fileId;

        using (var context = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(context);
            var ownerPersonId = Guid.NewGuid().ToString("D");
            var record = store.CreateRecord(
                DefaultTenantId,
                "DR backup packet",
                "Isolated record for disaster recovery backup verification.",
                "evidence_package",
                "operations",
                "disaster_recovery",
                "backup",
                "internal",
                "recordarr",
                "disaster_recovery",
                "dr-backup-test",
                "DR backup packet",
                ownerPersonId,
                ownerPersonId,
                "dr-backup-source.pdf",
                "application/pdf",
                "recordarr",
                "tenant/dr-backup-source.pdf",
                4096);
            var file = store.CreateFile(
                record.RecordId,
                "dr-backed-up-evidence.pdf",
                "application/pdf",
                ownerPersonId,
                "recordarr",
                "tenant/dr-backed-up-evidence.pdf",
                2048);

            var missingProvider = store.RunDisasterRecoveryBackupVerification(
                DefaultTenantId,
                ownerPersonId,
                backupProviderName: null,
                backupJobRef: "backup-job-7781",
                backupManifestHash: "sha256-backup-manifest",
                recoveryPointId: "rp-2026-06-29T04:15Z",
                recoveryPointCreatedAt: DateTimeOffset.UtcNow.AddMinutes(-5),
                rpoTargetMinutes: 60,
                recordIds: [record.RecordId]);

            Assert.Equal("backup_verification", missingProvider.RunType);
            Assert.Equal("failed", missingProvider.Status);
            Assert.Equal("missing_backup_provider", missingProvider.FailureReason);
            Assert.Empty(missingProvider.VerifiedFileRefs);

            var run = store.RunDisasterRecoveryBackupVerification(
                DefaultTenantId,
                ownerPersonId,
                "render-object-storage",
                "backup-job-7781",
                "sha256-backup-manifest",
                "rp-2026-06-29T04:15Z",
                DateTimeOffset.UtcNow.AddMinutes(-5),
                60,
                [record.RecordId]);

            Assert.Equal("backup_verification", run.RunType);
            Assert.Equal("passed", run.Status);
            Assert.Equal("render-object-storage", run.BackupProviderName);
            Assert.Equal("backup-job-7781", run.BackupJobRef);
            Assert.Equal("sha256-backup-manifest", run.BackupManifestHash);
            Assert.Contains(record.RecordId, run.RestoredRecordRefs);
            Assert.Contains(file.FileId, run.VerifiedFileRefs);
            Assert.Contains("Verified backup coverage", run.EvidenceSummary);
            Assert.Contains(
                store.GetObjectStoreFixityObservations(DefaultTenantId, file.FileId, reconciliationId: run.DisasterRecoveryRunId),
                item => item.ObservationSource == "disaster_recovery_backup" && item.Status == "passed");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, record.RecordId),
                log => log.Action == "disaster_recovery.backup_verified" && log.ReasonCode == run.DisasterRecoveryRunId);

            runId = run.DisasterRecoveryRunId;
            recordId = record.RecordId;
            fileId = file.FileId;
        }

        using (var recreatedContext = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(recreatedContext);
            var persisted = Assert.Single(recreated.GetDisasterRecoveryRuns(DefaultTenantId), run => run.DisasterRecoveryRunId == runId);

            Assert.Equal("backup_verification", persisted.RunType);
            Assert.Equal("passed", persisted.Status);
            Assert.Contains(recordId, persisted.RestoredRecordRefs);
            Assert.Contains(fileId, persisted.VerifiedFileRefs);
            Assert.DoesNotContain(
                recreated.GetDisasterRecoveryRuns(Guid.NewGuid().ToString()),
                run => run.DisasterRecoveryRunId == runId);
            Assert.Contains(
                recreated.GetObjectStoreFixityObservations(DefaultTenantId, fileId, reconciliationId: runId),
                item => item.ObservationSource == "disaster_recovery_backup" && item.Status == "passed");
        }
    }

    [Fact]
    public async Task Backup_verification_manifest_provider_returns_only_tenant_record_scoped_manifests()
    {
        var databaseName = $"recordarr-backup-provider-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        var otherTenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        string recordId;
        string fileId;
        string otherTenantRecordId;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var record = store.CreateRecord(
                DefaultTenantId,
                "Backup provider scope",
                "Validates provider backup manifest filtering.",
                "evidence_package",
                "operations",
                "disaster_recovery",
                "backup",
                "internal",
                "recordarr",
                "disaster_recovery",
                "backup-provider-test",
                "Backup Provider Test",
                "person-record-owner",
                "person-record-owner",
                "backup-provider.pdf",
                "application/pdf",
                "recordarr",
                "tenant/backup-provider.pdf",
                4096);
            var file = store.CreateFile(
                record.RecordId,
                "backup-provider-attachment.pdf",
                "application/pdf",
                "person-record-owner",
                "recordarr",
                "tenant/backup-provider-attachment.pdf",
                2048);
            var otherTenantRecord = store.CreateRecord(
                otherTenantId,
                "Other backup provider scope",
                "Must not leak into the default tenant backup worker.",
                "evidence_package",
                "operations",
                "disaster_recovery",
                "backup",
                "internal",
                "recordarr",
                "disaster_recovery",
                "backup-provider-other-tenant-test",
                "Other Backup Provider Test",
                "person-other-owner",
                "person-other-owner",
                "backup-provider-other.pdf",
                "application/pdf",
                "recordarr",
                "tenant/backup-provider-other.pdf",
                4096);

            recordId = record.RecordId;
            fileId = file.FileId;
            otherTenantRecordId = otherTenantRecord.RecordId;
        }

        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-backup-provider-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        manifests = new object[]
                        {
                            new
                            {
                                tenantId = otherTenantId,
                                backupProviderName = "backup-vault",
                                backupJobRef = "job-other-tenant",
                                backupManifestHash = "sha256-other-manifest",
                                recoveryPointId = "rp-other",
                                recoveryPointCreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                                rpoTargetMinutes = 60,
                                recordIds = new[] { otherTenantRecordId },
                                missingFileIds = Array.Empty<string>(),
                                corruptFileIds = Array.Empty<string>()
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                backupProviderName = "",
                                backupJobRef = "job-missing-provider",
                                backupManifestHash = "sha256-missing-provider",
                                recoveryPointId = "rp-missing-provider",
                                recoveryPointCreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                                rpoTargetMinutes = 60,
                                recordIds = new[] { recordId },
                                missingFileIds = Array.Empty<string>(),
                                corruptFileIds = Array.Empty<string>()
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                backupProviderName = "backup-vault",
                                backupJobRef = "job-unknown-record",
                                backupManifestHash = "sha256-unknown-record",
                                recoveryPointId = "rp-unknown-record",
                                recoveryPointCreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                                rpoTargetMinutes = 60,
                                recordIds = new[] { "unknown-record" },
                                missingFileIds = new[] { fileId },
                                corruptFileIds = Array.Empty<string>()
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                backupProviderName = "backup-vault",
                                backupJobRef = "job-valid-record",
                                backupManifestHash = "sha256-valid-record",
                                recoveryPointId = "rp-valid-record",
                                recoveryPointCreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                                rpoTargetMinutes = 60,
                                recordIds = new[] { recordId },
                                missingFileIds = new[] { "unknown-file" },
                                corruptFileIds = Array.Empty<string>()
                            }
                        }
                    },
                    JsonOptions));

            using var verifyScope = provider.CreateScope();
            var store = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var options = new StaticOptionsMonitor<BackupVerificationWorkerOptions>(new BackupVerificationWorkerOptions
            {
                Enabled = true,
                TenantIds = [DefaultTenantId],
                ManifestPath = manifestPath,
                DefaultRpoTargetMinutes = 60
            });
            var manifestProvider = new ManifestRecordArrBackupVerificationManifestProvider(
                options,
                NullLogger<ManifestRecordArrBackupVerificationManifestProvider>.Instance);

            var manifests = await manifestProvider.GetManifestsAsync(
                DefaultTenantId,
                store.GetStorageReconciliationCandidateFiles(DefaultTenantId),
                CancellationToken.None);

            var manifest = Assert.Single(manifests);
            Assert.Equal("backup-vault", manifest.BackupProviderName);
            Assert.Equal("job-valid-record", manifest.BackupJobRef);
            Assert.Equal("sha256-valid-record", manifest.BackupManifestHash);
            Assert.Equal("rp-valid-record", manifest.RecoveryPointId);
            Assert.Equal([recordId], manifest.RecordIds);
            Assert.Empty(manifest.MissingFileIds);
            Assert.Empty(manifest.CorruptFileIds);
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }
    }

    [Fact]
    public async Task Backup_verification_worker_requires_explicit_manifest_and_processes_only_matching_tenant_records()
    {
        var databaseName = $"recordarr-backup-worker-{Guid.NewGuid():N}";
        await using var provider = CreateStoreProvider(databaseName);
        var otherTenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        string recordId;
        string fileId;
        string otherTenantRecordId;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var record = store.CreateRecord(
                DefaultTenantId,
                "Backup worker scope",
                "Validates backup verification worker persistence.",
                "evidence_package",
                "operations",
                "disaster_recovery",
                "backup",
                "internal",
                "recordarr",
                "disaster_recovery",
                "backup-worker-test",
                "Backup Worker Test",
                "person-record-owner",
                "person-record-owner",
                "backup-worker.pdf",
                "application/pdf",
                "recordarr",
                "tenant/backup-worker.pdf",
                4096);
            var file = store.CreateFile(
                record.RecordId,
                "backup-worker-attachment.pdf",
                "application/pdf",
                "person-record-owner",
                "recordarr",
                "tenant/backup-worker-attachment.pdf",
                2048);
            var otherTenantRecord = store.CreateRecord(
                otherTenantId,
                "Other backup worker scope",
                "Must not leak into the default tenant backup worker.",
                "evidence_package",
                "operations",
                "disaster_recovery",
                "backup",
                "internal",
                "recordarr",
                "disaster_recovery",
                "backup-worker-other-tenant-test",
                "Other Backup Worker Test",
                "person-other-owner",
                "person-other-owner",
                "backup-worker-other.pdf",
                "application/pdf",
                "recordarr",
                "tenant/backup-worker-other.pdf",
                4096);

            recordId = record.RecordId;
            fileId = file.FileId;
            otherTenantRecordId = otherTenantRecord.RecordId;
        }

        var missingManifestOptions = new StaticOptionsMonitor<BackupVerificationWorkerOptions>(new BackupVerificationWorkerOptions
        {
            Enabled = true,
            TenantIds = [DefaultTenantId],
            RequestedByPersonId = "recordarr-backup-verification-worker"
        });
        var missingWorker = new RecordArrBackupVerificationWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new ManifestRecordArrBackupVerificationManifestProvider(
                missingManifestOptions,
                NullLogger<ManifestRecordArrBackupVerificationManifestProvider>.Instance),
            missingManifestOptions,
            NullLogger<RecordArrBackupVerificationWorker>.Instance);

        await missingWorker.RunOnceAsync();

        using (var missingVerifyScope = provider.CreateScope())
        {
            var store = missingVerifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            Assert.Empty(store.GetDisasterRecoveryRuns(DefaultTenantId));
        }

        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-backup-worker-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        manifests = new object[]
                        {
                            new
                            {
                                tenantId = otherTenantId,
                                backupProviderName = "backup-vault",
                                backupJobRef = "job-other-tenant",
                                backupManifestHash = "sha256-other-manifest",
                                recoveryPointId = "rp-other",
                                recoveryPointCreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                                rpoTargetMinutes = 60,
                                recordIds = new[] { otherTenantRecordId },
                                missingFileIds = Array.Empty<string>(),
                                corruptFileIds = Array.Empty<string>()
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                backupProviderName = "backup-vault",
                                backupJobRef = "job-worker",
                                backupManifestHash = "sha256-worker",
                                recoveryPointId = "rp-worker",
                                recoveryPointCreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                                rpoTargetMinutes = 60,
                                recordIds = new[] { recordId },
                                missingFileIds = Array.Empty<string>(),
                                corruptFileIds = Array.Empty<string>()
                            }
                        }
                    },
                    JsonOptions));

            var options = new BackupVerificationWorkerOptions
            {
                Enabled = true,
                TenantIds = [DefaultTenantId],
                ManifestPath = manifestPath,
                RequestedByPersonId = "recordarr-backup-verification-worker",
                DefaultRpoTargetMinutes = 60
            };
            var optionsMonitor = new StaticOptionsMonitor<BackupVerificationWorkerOptions>(options);
            var worker = new RecordArrBackupVerificationWorker(
                provider.GetRequiredService<IServiceScopeFactory>(),
                new ManifestRecordArrBackupVerificationManifestProvider(
                    optionsMonitor,
                    NullLogger<ManifestRecordArrBackupVerificationManifestProvider>.Instance),
                optionsMonitor,
                NullLogger<RecordArrBackupVerificationWorker>.Instance);

            await worker.RunOnceAsync();
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var run = Assert.Single(verifyStore.GetDisasterRecoveryRuns(DefaultTenantId));

        Assert.Equal("backup_verification", run.RunType);
        Assert.Equal("passed", run.Status);
        Assert.Equal("backup-vault", run.BackupProviderName);
        Assert.Equal("job-worker", run.BackupJobRef);
        Assert.Equal("sha256-worker", run.BackupManifestHash);
        Assert.Contains(recordId, run.RestoredRecordRefs);
        Assert.Contains(fileId, run.VerifiedFileRefs);
        Assert.Empty(verifyStore.GetDisasterRecoveryRuns(otherTenantId));
        Assert.Contains(
            verifyStore.GetAccessLogs(DefaultTenantId, recordId),
            log => log.Action == "disaster_recovery.backup_verified" &&
                   log.Result == "allowed" &&
                   log.ReasonCode == run.DisasterRecoveryRunId);
    }

    [Fact]
    public async Task Object_store_inventory_worker_requires_explicit_manifest_evidence()
    {
        var databaseName = $"recordarr-object-store-worker-empty-{Guid.NewGuid():N}";
        await using var provider = CreateStoreProvider(databaseName);

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var file = store.CreateFile(
                "rec-bol-001",
                "object-store-no-manifest.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/object-store-no-manifest.pdf",
                4096);
            store.CreateFileMalwareScan(DefaultTenantId, file.FileId, "person-route-lead", "clean");
        }

        var worker = new RecordArrObjectStoreReconciliationWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new ManifestRecordArrObjectStoreInventoryProvider(
                new StaticOptionsMonitor<ObjectStoreReconciliationWorkerOptions>(new ObjectStoreReconciliationWorkerOptions
                {
                    Enabled = true,
                    TenantIds = [DefaultTenantId],
                    InventoryManifestPath = Path.Combine(Path.GetTempPath(), $"missing-object-store-{Guid.NewGuid():N}.json")
                }),
                NullLogger<ManifestRecordArrObjectStoreInventoryProvider>.Instance),
            new StaticOptionsMonitor<ObjectStoreReconciliationWorkerOptions>(new ObjectStoreReconciliationWorkerOptions
            {
                Enabled = true,
                TenantIds = [DefaultTenantId],
                InventoryManifestPath = Path.Combine(Path.GetTempPath(), $"missing-object-store-{Guid.NewGuid():N}.json")
            }),
            NullLogger<RecordArrObjectStoreReconciliationWorker>.Instance);

        await worker.RunOnceAsync();

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        Assert.Empty(verifyStore.GetStorageReconciliations(DefaultTenantId));
    }

    [Fact]
    public async Task Object_store_inventory_worker_records_and_remediates_only_explicit_tenant_inventory()
    {
        var databaseName = $"recordarr-object-store-worker-{Guid.NewGuid():N}";
        await using var provider = CreateStoreProvider(databaseName);
        string cleanFileId;
        string missingFileId;
        string corruptFileId;
        string otherTenantFileId;
        const string otherTenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var cleanFile = store.CreateFile(
                "rec-bol-001",
                "object-store-clean.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/object-store-clean.pdf",
                4096);
            store.CreateFileMalwareScan(DefaultTenantId, cleanFile.FileId, "person-route-lead", "clean");
            cleanFileId = cleanFile.FileId;

            var missingFile = store.CreateFile(
                "rec-bol-001",
                "object-store-missing.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/object-store-missing.pdf",
                4096);
            store.CreateFileMalwareScan(DefaultTenantId, missingFile.FileId, "person-route-lead", "clean");
            missingFileId = missingFile.FileId;

            var corruptFile = store.CreateFile(
                "rec-bol-001",
                "object-store-corrupt.pdf",
                "application/pdf",
                "person-route-lead",
                "recordarr",
                "tenant/object-store-corrupt.pdf",
                4096);
            store.CreateFileMalwareScan(DefaultTenantId, corruptFile.FileId, "person-route-lead", "clean");
            corruptFileId = corruptFile.FileId;

            var otherTenantRecord = store.CreateRecord(
                otherTenantId,
                "Other tenant object store packet",
                "Proves manifest rows stay tenant-scoped.",
                "evidence_package",
                "operations",
                "storage_reconciliation",
                "remediation",
                "internal",
                "recordarr",
                "storage_reconciliation",
                "other-tenant-object-store-test",
                "Other tenant object store packet",
                "person-doc-controller",
                "person-doc-controller",
                "other-tenant-object-store.pdf",
                "application/pdf",
                "recordarr",
                "other-tenant/object-store.pdf",
                4096);
            store.CreateFileMalwareScan(otherTenantId, otherTenantRecord.CurrentFileRef, "person-doc-controller", "clean");
            otherTenantFileId = otherTenantRecord.CurrentFileRef;
        }

        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-object-store-{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(
            manifestPath,
            JsonSerializer.Serialize(
                new
                {
                    inventories = new object[]
                    {
                        new
                        {
                            tenantId = otherTenantId,
                            scope = "wrong-tenant-inventory",
                            verifiedFileIds = new[] { cleanFileId },
                            missingFileIds = new[] { missingFileId }
                        },
                        new
                        {
                            tenantId = DefaultTenantId,
                            scope = "external-object-store-restore-test",
                            verifiedFileIds = new[] { cleanFileId },
                            missingFileIds = new[] { missingFileId, otherTenantFileId },
                            corruptFileIds = new[] { corruptFileId },
                            restoredFileIds = new[] { missingFileId, otherTenantFileId },
                            recheckedCorruptFileIds = new[] { corruptFileId }
                        }
                    }
                },
                JsonOptions));

        var options = new ObjectStoreReconciliationWorkerOptions
        {
            Enabled = true,
            TenantIds = [DefaultTenantId],
            InventoryManifestPath = manifestPath,
            RequestedByPersonId = "recordarr-object-store-worker"
        };
        var optionsMonitor = new StaticOptionsMonitor<ObjectStoreReconciliationWorkerOptions>(options);
        var worker = new RecordArrObjectStoreReconciliationWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new ManifestRecordArrObjectStoreInventoryProvider(
                optionsMonitor,
                NullLogger<ManifestRecordArrObjectStoreInventoryProvider>.Instance),
            optionsMonitor,
            NullLogger<RecordArrObjectStoreReconciliationWorker>.Instance);

        await worker.RunOnceAsync();

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var reconciliation = Assert.Single(verifyStore.GetStorageReconciliations(DefaultTenantId));
        Assert.Equal("passed", reconciliation.Status);
        Assert.Equal("completed", reconciliation.RemediationStatus);
        Assert.Equal(3, reconciliation.TotalFiles);
        Assert.Equal(2, reconciliation.CheckedFiles);
        Assert.Empty(reconciliation.IssueFileRefs);
        Assert.DoesNotContain(otherTenantFileId, reconciliation.IssueFileRefs);
        Assert.Empty(verifyStore.GetStorageReconciliations(otherTenantId));
        Assert.Contains(
            verifyStore.GetFileIntegrityChecks(DefaultTenantId, corruptFileId),
            check => check.CheckMethod == "restore_verify" && check.Status == "passed");
        Assert.Contains(
            verifyStore.GetFileIntegrityChecks(DefaultTenantId, missingFileId),
            check => check.CheckMethod == "restore_verify" && check.Status == "passed");
        Assert.Contains(
            verifyStore.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
            log => log.Action == "storage.reconciliation.restored" && log.ReasonCode == reconciliation.ReconciliationId);
    }

    [Fact]
    public void CreateScanProcessing_creates_original_and_generated_files()
    {
        var store = CreateStore();
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
        var store = CreateStore();

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
        var store = CreateStore();

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

        var completed = store.CompleteCaptureRequest(DefaultTenantId, request.CaptureRequestId);
        var linked = store.GetCaptureRequests(DefaultTenantId).First(entry => entry.CaptureRequestId == request.CaptureRequestId);

        Assert.Equal("completed", completed.Status);
        Assert.NotNull(completed.CompletedAt);
        Assert.Equal("photo", completed.CaptureType);
        Assert.Equal("completed", linked.Status);
    }

    [Fact]
    public void CreateSignatureAndPhotoEvidence_create_file_backed_evidence_records()
    {
        var store = CreateStore();
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
    public void Signature_records_persist_provider_evidence_and_truthful_local_capture_status()
    {
        var databaseName = $"recordarr-signature-provider-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        string signatureId;
        string localSignatureId;
        const string certificateFingerprint = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var signature = store.CreateSignatureRecord(
                tenantId: DefaultTenantId,
                recordId: "rec-bol-001",
                signaturePurpose: "proof_of_delivery",
                signerPersonId: "person-route-lead",
                signerExternalName: "Avery Auditor",
                signerTitle: "Driver",
                attestationText: "Signed with external provider envelope.",
                capturedByPersonId: "person-route-lead",
                sourceProduct: "routarr",
                sourceObjectRef: "trip-7781",
                providerName: "DocuSign",
                providerEnvelopeRef: "env-7781",
                certificateFingerprintSha256: certificateFingerprint);
            var localSignature = store.CreateSignatureRecord(
                tenantId: DefaultTenantId,
                recordId: "rec-bol-001",
                signaturePurpose: "customer_acceptance",
                signerPersonId: null,
                signerExternalName: "Jordan Customer",
                signerTitle: "Receiver",
                attestationText: "Local capture only.",
                capturedByPersonId: "person-route-lead",
                sourceProduct: "routarr",
                sourceObjectRef: "trip-7781");

            signatureId = signature.SignatureRecordId;
            localSignatureId = localSignature.SignatureRecordId;

            Assert.Equal("provider_verified", signature.VerificationStatus);
            Assert.Equal("DocuSign", signature.ProviderName);
            Assert.Equal("env-7781", signature.ProviderEnvelopeRef);
            Assert.Equal(certificateFingerprint, signature.CertificateFingerprintSha256);
            Assert.False(string.IsNullOrWhiteSpace(signature.SignatureEvidenceHash));
            Assert.Equal(signature.SignedAt, signature.LockedAt);
            Assert.Null(signature.VerificationFailureReason);

            Assert.Equal("local_capture_only", localSignature.VerificationStatus);
            Assert.Equal("provider_not_configured", localSignature.VerificationFailureReason);
            Assert.False(string.IsNullOrWhiteSpace(localSignature.SignatureEvidenceHash));

            Assert.Throws<InvalidOperationException>(() =>
                store.CreateSignatureRecord(
                    tenantId: DefaultTenantId,
                    recordId: "rec-bol-001",
                    signaturePurpose: "proof_of_delivery",
                    signerPersonId: "person-route-lead",
                    signerExternalName: null,
                    signerTitle: null,
                    attestationText: "Missing provider envelope.",
                    capturedByPersonId: "person-route-lead",
                    sourceProduct: "routarr",
                    sourceObjectRef: "trip-7781",
                    providerName: "DocuSign",
                    certificateFingerprintSha256: certificateFingerprint));
            Assert.Throws<InvalidOperationException>(() =>
                store.CreateSignatureRecord(
                    tenantId: DefaultTenantId,
                    recordId: "rec-bol-001",
                    signaturePurpose: "proof_of_delivery",
                    signerPersonId: "person-route-lead",
                    signerExternalName: null,
                    signerTitle: null,
                    attestationText: "Provider evidence without provider.",
                    capturedByPersonId: "person-route-lead",
                    sourceProduct: "routarr",
                    sourceObjectRef: "trip-7781",
                    providerEnvelopeRef: "env-7782",
                    certificateFingerprintSha256: certificateFingerprint));
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var persisted = Assert.Single(verifyStore.GetSignatureRecords(DefaultTenantId, "rec-bol-001"), signature => signature.SignatureRecordId == signatureId);
        var localPersisted = Assert.Single(verifyStore.GetSignatureRecords(DefaultTenantId, "rec-bol-001"), signature => signature.SignatureRecordId == localSignatureId);

        Assert.Equal("provider_verified", persisted.VerificationStatus);
        Assert.Equal(certificateFingerprint, persisted.CertificateFingerprintSha256);
        Assert.False(string.IsNullOrWhiteSpace(persisted.SignatureEvidenceHash));
        Assert.Equal("local_capture_only", localPersisted.VerificationStatus);
        Assert.DoesNotContain(
            verifyStore.GetSignatureRecords("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "rec-bol-001"),
            signature => signature.SignatureRecordId == signatureId || signature.SignatureRecordId == localSignatureId);
    }

    [Fact]
    public void Signature_provider_reconciliation_requires_matching_provider_evidence_and_persists_callback_status()
    {
        var databaseName = $"recordarr-signature-provider-reconciliation-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        string completedSignatureId;
        string rejectedSignatureId;
        string localSignatureId;
        const string certificateFingerprint = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var completedSignature = store.CreateSignatureRecord(
                tenantId: DefaultTenantId,
                recordId: "rec-bol-001",
                signaturePurpose: "proof_of_delivery",
                signerPersonId: "person-route-lead",
                signerExternalName: "Avery Auditor",
                signerTitle: "Driver",
                attestationText: "Signed with external provider envelope.",
                capturedByPersonId: "person-route-lead",
                sourceProduct: "routarr",
                sourceObjectRef: "trip-7781",
                providerName: "DocuSign",
                providerEnvelopeRef: "env-7781",
                certificateFingerprintSha256: certificateFingerprint);
            var rejectedSignature = store.CreateSignatureRecord(
                tenantId: DefaultTenantId,
                recordId: "rec-bol-001",
                signaturePurpose: "customer_acceptance",
                signerPersonId: null,
                signerExternalName: "Jordan Customer",
                signerTitle: "Receiver",
                attestationText: "Customer signed through external provider.",
                capturedByPersonId: "person-route-lead",
                sourceProduct: "routarr",
                sourceObjectRef: "trip-7781",
                providerName: "DocuSign",
                providerEnvelopeRef: "env-7782",
                certificateFingerprintSha256: certificateFingerprint);
            var localSignature = store.CreateSignatureRecord(
                tenantId: DefaultTenantId,
                recordId: "rec-bol-001",
                signaturePurpose: "policy_acknowledgement",
                signerPersonId: "person-route-lead",
                signerExternalName: null,
                signerTitle: "Driver",
                attestationText: "Local capture only.",
                capturedByPersonId: "person-route-lead",
                sourceProduct: "routarr",
                sourceObjectRef: "trip-7781");

            completedSignatureId = completedSignature.SignatureRecordId;
            rejectedSignatureId = rejectedSignature.SignatureRecordId;
            localSignatureId = localSignature.SignatureRecordId;

            Assert.Throws<InvalidOperationException>(() =>
                store.ReconcileSignatureProviderStatus(
                    DefaultTenantId,
                    localSignatureId,
                    "person-route-lead",
                    "DocuSign",
                    "env-local",
                    "completed",
                    "callback-local",
                    null,
                    null,
                    null));

            Assert.Throws<InvalidOperationException>(() =>
                store.ReconcileSignatureProviderStatus(
                    DefaultTenantId,
                    completedSignatureId,
                    "person-route-lead",
                    "DocuSign",
                    "env-mismatch",
                    "completed",
                    "callback-mismatch",
                    certificateFingerprint,
                    null,
                    null));

            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "signature.provider_reconciled" &&
                       log.Result == "denied" &&
                       log.ReasonCode == "provider_envelope_mismatch");

            var completed = store.ReconcileSignatureProviderStatus(
                DefaultTenantId,
                completedSignatureId,
                "person-route-lead",
                "DocuSign",
                "env-7781",
                "completed",
                "callback-7781",
                certificateFingerprint,
                "tsa-001",
                "valid");

            Assert.Equal("provider_verified", completed.VerificationStatus);
            Assert.Null(completed.VerificationFailureReason);
            Assert.Equal("completed", completed.ProviderCallbackStatus);
            Assert.Equal("callback-7781", completed.ProviderCallbackRef);
            Assert.Equal("tsa-001", completed.TrustTimestampAuthorityRef);
            Assert.Equal("valid", completed.LongTermValidationStatus);
            Assert.False(string.IsNullOrWhiteSpace(completed.ProviderCallbackEvidenceHash));
            Assert.Equal(completed.ProviderCallbackEvidenceHash, completed.SignatureRecord.ProviderCallbackEvidenceHash);

            var rejected = store.ReconcileSignatureProviderStatus(
                DefaultTenantId,
                rejectedSignatureId,
                "person-route-lead",
                "DocuSign",
                "env-7782",
                "declined",
                "callback-7782",
                certificateFingerprint,
                null,
                "failed");

            Assert.Equal("provider_rejected", rejected.VerificationStatus);
            Assert.Equal("provider_declined", rejected.VerificationFailureReason);
            Assert.Equal("declined", rejected.ProviderCallbackStatus);
            Assert.Equal("failed", rejected.LongTermValidationStatus);

            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "signature.provider_reconciled" &&
                       log.Result == "allowed" &&
                       log.ReasonCode == "completed");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "signature.provider_reconciled" &&
                       log.Result == "denied" &&
                       log.ReasonCode == "declined");
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var persistedCompleted = Assert.Single(verifyStore.GetSignatureRecords(DefaultTenantId, "rec-bol-001"), signature => signature.SignatureRecordId == completedSignatureId);
        var persistedRejected = Assert.Single(verifyStore.GetSignatureRecords(DefaultTenantId, "rec-bol-001"), signature => signature.SignatureRecordId == rejectedSignatureId);

        Assert.Equal("provider_verified", persistedCompleted.VerificationStatus);
        Assert.Equal("completed", persistedCompleted.ProviderCallbackStatus);
        Assert.Equal("callback-7781", persistedCompleted.ProviderCallbackRef);
        Assert.NotNull(persistedCompleted.ProviderCallbackReceivedAt);
        Assert.False(string.IsNullOrWhiteSpace(persistedCompleted.ProviderCallbackEvidenceHash));
        Assert.Equal("tsa-001", persistedCompleted.TrustTimestampAuthorityRef);
        Assert.Equal("valid", persistedCompleted.LongTermValidationStatus);

        Assert.Equal("provider_rejected", persistedRejected.VerificationStatus);
        Assert.Equal("provider_declined", persistedRejected.VerificationFailureReason);
        Assert.Equal("declined", persistedRejected.ProviderCallbackStatus);
        Assert.DoesNotContain(
            verifyStore.GetSignatureRecords("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "rec-bol-001"),
            signature => signature.SignatureRecordId == completedSignatureId ||
                         signature.SignatureRecordId == rejectedSignatureId ||
                         signature.SignatureRecordId == localSignatureId);
    }

    [Fact]
    public void Signature_trust_service_jobs_are_durable_and_reconcile_only_matching_provider_manifests()
    {
        var databaseName = $"recordarr-signature-trust-service-jobs-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        string completedSignatureId;
        string failedSignatureId;
        string completedJobId;
        string failedJobId;
        const string certificateFingerprint = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var completedSignature = store.CreateSignatureRecord(
                tenantId: DefaultTenantId,
                recordId: "rec-bol-001",
                signaturePurpose: "proof_of_delivery",
                signerPersonId: "person-route-lead",
                signerExternalName: "Avery Auditor",
                signerTitle: "Driver",
                attestationText: "Signed with external provider envelope.",
                capturedByPersonId: "person-route-lead",
                sourceProduct: "routarr",
                sourceObjectRef: "trip-7781",
                providerName: "DocuSign",
                providerEnvelopeRef: "env-trust-7781",
                certificateFingerprintSha256: certificateFingerprint);
            var failedSignature = store.CreateSignatureRecord(
                tenantId: DefaultTenantId,
                recordId: "rec-bol-001",
                signaturePurpose: "customer_acceptance",
                signerPersonId: null,
                signerExternalName: "Jordan Customer",
                signerTitle: "Receiver",
                attestationText: "Customer signed through external provider.",
                capturedByPersonId: "person-route-lead",
                sourceProduct: "routarr",
                sourceObjectRef: "trip-7781",
                providerName: "DocuSign",
                providerEnvelopeRef: "env-trust-7782",
                certificateFingerprintSha256: certificateFingerprint);
            var localSignature = store.CreateSignatureRecord(
                tenantId: DefaultTenantId,
                recordId: "rec-bol-001",
                signaturePurpose: "policy_acknowledgement",
                signerPersonId: "person-route-lead",
                signerExternalName: null,
                signerTitle: "Driver",
                attestationText: "Local capture only.",
                capturedByPersonId: "person-route-lead",
                sourceProduct: "routarr",
                sourceObjectRef: "trip-7781");

            completedSignatureId = completedSignature.SignatureRecordId;
            failedSignatureId = failedSignature.SignatureRecordId;

            Assert.Throws<InvalidOperationException>(() =>
                store.SubmitSignatureTrustServiceJob(
                    DefaultTenantId,
                    localSignature.SignatureRecordId,
                    "person-route-lead",
                    "DocuSign",
                    "env-local"));

            var submitted = store.SubmitSignatureTrustServiceJob(
                DefaultTenantId,
                completedSignatureId,
                "person-route-lead",
                "DocuSign",
                "env-trust-7781");
            var idempotentSubmit = store.SubmitSignatureTrustServiceJob(
                DefaultTenantId,
                completedSignatureId,
                "person-route-lead",
                "DocuSign",
                "env-trust-7781");
            var failedSubmit = store.SubmitSignatureTrustServiceJob(
                DefaultTenantId,
                failedSignatureId,
                "person-route-lead",
                "DocuSign",
                "env-trust-7782");

            completedJobId = submitted.TrustServiceJobId;
            failedJobId = failedSubmit.TrustServiceJobId;

            Assert.Equal(completedJobId, idempotentSubmit.TrustServiceJobId);
            Assert.Equal("submitted", submitted.Status);
            Assert.Equal(certificateFingerprint, submitted.CertificateFingerprintSha256);
            Assert.Equal(completedSignature.SignatureEvidenceHash, submitted.SignatureEvidenceHash);
            Assert.False(string.IsNullOrWhiteSpace(submitted.SubmissionEvidenceHash));

            var mismatch = store.ProcessSignatureTrustServiceManifest(
                DefaultTenantId,
                "person-route-lead",
                "DocuSign",
                "env-trust-7782",
                "completed",
                "callback-trust-7782",
                "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd",
                "tsa-002",
                "valid");

            Assert.Equal("failed", mismatch.Status);
            Assert.Equal("certificate_fingerprint_mismatch", mismatch.FailureReason);
            Assert.Null(mismatch.ProviderCallbackEvidenceHash);

            var completed = store.ProcessSignatureTrustServiceManifest(
                DefaultTenantId,
                "person-route-lead",
                "DocuSign",
                "env-trust-7781",
                "completed",
                "callback-trust-7781",
                certificateFingerprint,
                "tsa-001",
                "valid");

            Assert.Equal("completed", completed.Status);
            Assert.Equal("completed", completed.ProviderCallbackStatus);
            Assert.False(string.IsNullOrWhiteSpace(completed.ProviderCallbackEvidenceHash));
            Assert.Equal("provider_verified", completed.SignatureRecord?.VerificationStatus);
            Assert.Equal("tsa-001", completed.TrustTimestampAuthorityRef);
            Assert.Equal("valid", completed.LongTermValidationStatus);

            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "signature.trust_service_job_submitted" &&
                       log.Result == "allowed" &&
                       log.ReasonCode == "env-trust-7781");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "signature.trust_service_job_reconciled" &&
                       log.Result == "denied" &&
                       log.ReasonCode == "certificate_fingerprint_mismatch");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "signature.trust_service_job_reconciled" &&
                       log.Result == "allowed" &&
                       log.ReasonCode == "completed");
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var persistedCompletedJob = Assert.Single(verifyStore.GetSignatureTrustServiceJobs(DefaultTenantId), job => job.TrustServiceJobId == completedJobId);
        var persistedFailedJob = Assert.Single(verifyStore.GetSignatureTrustServiceJobs(DefaultTenantId), job => job.TrustServiceJobId == failedJobId);
        var persistedCompletedSignature = Assert.Single(verifyStore.GetSignatureRecords(DefaultTenantId, "rec-bol-001"), signature => signature.SignatureRecordId == completedSignatureId);
        var persistedFailedSignature = Assert.Single(verifyStore.GetSignatureRecords(DefaultTenantId, "rec-bol-001"), signature => signature.SignatureRecordId == failedSignatureId);

        Assert.Equal("completed", persistedCompletedJob.Status);
        Assert.Equal("completed", persistedCompletedJob.ProviderCallbackStatus);
        Assert.False(string.IsNullOrWhiteSpace(persistedCompletedJob.ProviderCallbackEvidenceHash));
        Assert.Equal("provider_verified", persistedCompletedSignature.VerificationStatus);
        Assert.Equal("completed", persistedCompletedSignature.ProviderCallbackStatus);
        Assert.Equal("valid", persistedCompletedSignature.LongTermValidationStatus);

        Assert.Equal("failed", persistedFailedJob.Status);
        Assert.Equal("certificate_fingerprint_mismatch", persistedFailedJob.FailureReason);
        Assert.Null(persistedFailedJob.ProviderCallbackEvidenceHash);
        Assert.Null(persistedFailedSignature.ProviderCallbackStatus);
        Assert.DoesNotContain(
            verifyStore.GetSignatureTrustServiceJobs("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            job => job.TrustServiceJobId == completedJobId ||
                   job.TrustServiceJobId == failedJobId);
    }

    [Fact]
    public async Task Signature_trust_service_manifest_provider_returns_only_submitted_tenant_manifests()
    {
        var store = CreateStore();
        const string certificateFingerprint = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
        var signature = store.CreateSignatureRecord(
            tenantId: DefaultTenantId,
            recordId: "rec-bol-001",
            signaturePurpose: "proof_of_delivery",
            signerPersonId: "person-route-lead",
            signerExternalName: "Avery Auditor",
            signerTitle: "Driver",
            attestationText: "Signed with external provider envelope.",
            capturedByPersonId: "person-route-lead",
            sourceProduct: "routarr",
            sourceObjectRef: "trip-7781",
            providerName: "DocuSign",
            providerEnvelopeRef: "env-worker-provider",
            certificateFingerprintSha256: certificateFingerprint);
        store.SubmitSignatureTrustServiceJob(
            DefaultTenantId,
            signature.SignatureRecordId,
            "person-route-lead",
            "DocuSign",
            "env-worker-provider");
        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-signature-trust-provider-{Guid.NewGuid():N}.json");

        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        manifests = new object[]
                        {
                            new
                            {
                                tenantId = Guid.NewGuid().ToString(),
                                providerName = "DocuSign",
                                providerEnvelopeRef = "env-worker-provider",
                                providerCallbackStatus = "completed",
                                providerCallbackRef = "wrong-tenant-callback",
                                certificateFingerprintSha256 = certificateFingerprint
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                providerName = "DocuSign",
                                providerEnvelopeRef = "env-not-submitted",
                                providerCallbackStatus = "completed",
                                providerCallbackRef = "unknown-callback",
                                certificateFingerprintSha256 = certificateFingerprint
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                providerName = "DocuSign",
                                providerEnvelopeRef = "env-worker-provider",
                                providerCallbackStatus = "completed",
                                providerCallbackRef = "callback-worker-provider",
                                certificateFingerprintSha256 = certificateFingerprint,
                                trustTimestampAuthorityRef = "tsa-worker",
                                longTermValidationStatus = "valid"
                            }
                        }
                    },
                    JsonOptions));

            var provider = new ManifestRecordArrSignatureTrustServiceManifestProvider(
                new StaticOptionsMonitor<SignatureTrustServiceWorkerOptions>(new SignatureTrustServiceWorkerOptions
                {
                    Enabled = true,
                    ManifestPath = manifestPath
                }),
                NullLogger<ManifestRecordArrSignatureTrustServiceManifestProvider>.Instance);

            var manifests = await provider.GetManifestsAsync(
                DefaultTenantId,
                store.GetSignatureTrustServiceJobs(DefaultTenantId),
                CancellationToken.None);

            var manifest = Assert.Single(manifests);
            Assert.Equal("DocuSign", manifest.ProviderName);
            Assert.Equal("env-worker-provider", manifest.ProviderEnvelopeRef);
            Assert.Equal("callback-worker-provider", manifest.ProviderCallbackRef);
            Assert.Equal("tsa-worker", manifest.TrustTimestampAuthorityRef);
            Assert.Equal("valid", manifest.LongTermValidationStatus);
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }
    }

    [Fact]
    public async Task Signature_trust_service_worker_requires_explicit_manifest_and_processes_only_matching_tenant_jobs()
    {
        var databaseName = $"recordarr-signature-trust-worker-{Guid.NewGuid():N}";
        await using var provider = CreateStoreProvider(databaseName);
        const string certificateFingerprint = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
        string signatureId;
        string otherTenantRecordId;
        string otherTenantSignatureId;
        const string otherTenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var signature = store.CreateSignatureRecord(
                tenantId: DefaultTenantId,
                recordId: "rec-bol-001",
                signaturePurpose: "proof_of_delivery",
                signerPersonId: "person-route-lead",
                signerExternalName: "Avery Auditor",
                signerTitle: "Driver",
                attestationText: "Signed with external provider envelope.",
                capturedByPersonId: "person-route-lead",
                sourceProduct: "routarr",
                sourceObjectRef: "trip-7781",
                providerName: "DocuSign",
                providerEnvelopeRef: "env-worker-7781",
                certificateFingerprintSha256: certificateFingerprint);
            signatureId = signature.SignatureRecordId;
            store.SubmitSignatureTrustServiceJob(
                DefaultTenantId,
                signature.SignatureRecordId,
                "person-route-lead",
                "DocuSign",
                "env-worker-7781");

            var otherTenantRecord = store.CreateRecord(
                otherTenantId,
                "Other tenant signature packet",
                "Proves manifest rows stay tenant-scoped.",
                "evidence_package",
                "operations",
                "signature_trust_service",
                "active",
                "internal",
                "recordarr",
                "signature_trust_service",
                "other-tenant-signature-test",
                "Other tenant signature packet",
                "person-doc-controller",
                "person-doc-controller",
                "other-tenant-signature.pdf",
                "application/pdf",
                "recordarr",
                "other-tenant/signature.pdf",
                4096);
            otherTenantRecordId = otherTenantRecord.RecordId;
            var otherTenantSignature = store.CreateSignatureRecord(
                tenantId: otherTenantId,
                recordId: otherTenantRecord.RecordId,
                signaturePurpose: "proof_of_delivery",
                signerPersonId: "person-route-lead",
                signerExternalName: "Other Tenant Signer",
                signerTitle: "Driver",
                attestationText: "Signed with external provider envelope.",
                capturedByPersonId: "person-route-lead",
                sourceProduct: "routarr",
                sourceObjectRef: "trip-other-tenant",
                providerName: "DocuSign",
                providerEnvelopeRef: "env-worker-other-tenant",
                certificateFingerprintSha256: certificateFingerprint);
            otherTenantSignatureId = otherTenantSignature.SignatureRecordId;
            store.SubmitSignatureTrustServiceJob(
                otherTenantId,
                otherTenantSignature.SignatureRecordId,
                "person-route-lead",
                "DocuSign",
                "env-worker-other-tenant");
        }

        var missingManifestOptions = new SignatureTrustServiceWorkerOptions
        {
            Enabled = true,
            TenantIds = [DefaultTenantId],
            ManifestPath = Path.Combine(Path.GetTempPath(), $"missing-signature-trust-{Guid.NewGuid():N}.json")
        };
        var missingMonitor = new StaticOptionsMonitor<SignatureTrustServiceWorkerOptions>(missingManifestOptions);
        var missingWorker = new RecordArrSignatureTrustServiceWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new ManifestRecordArrSignatureTrustServiceManifestProvider(
                missingMonitor,
                NullLogger<ManifestRecordArrSignatureTrustServiceManifestProvider>.Instance),
            missingMonitor,
            NullLogger<RecordArrSignatureTrustServiceWorker>.Instance);

        await missingWorker.RunOnceAsync();

        using (var missingVerifyScope = provider.CreateScope())
        {
            var store = missingVerifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var job = Assert.Single(store.GetSignatureTrustServiceJobs(DefaultTenantId));
            Assert.Equal("submitted", job.Status);
            var signature = Assert.Single(store.GetSignatureRecords(DefaultTenantId, "rec-bol-001"), item => item.SignatureRecordId == signatureId);
            Assert.Null(signature.ProviderCallbackStatus);
        }

        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-signature-trust-worker-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        manifests = new object[]
                        {
                            new
                            {
                                tenantId = otherTenantId,
                                providerName = "DocuSign",
                                providerEnvelopeRef = "env-worker-7781",
                                providerCallbackStatus = "completed",
                                providerCallbackRef = "wrong-tenant-callback",
                                certificateFingerprintSha256 = certificateFingerprint,
                                trustTimestampAuthorityRef = "tsa-wrong-tenant",
                                longTermValidationStatus = "valid"
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                providerName = "DocuSign",
                                providerEnvelopeRef = "env-worker-7781",
                                providerCallbackStatus = "completed",
                                providerCallbackRef = "callback-worker-7781",
                                certificateFingerprintSha256 = certificateFingerprint,
                                trustTimestampAuthorityRef = "tsa-worker-7781",
                                longTermValidationStatus = "valid"
                            }
                        }
                    },
                    JsonOptions));

            var options = new SignatureTrustServiceWorkerOptions
            {
                Enabled = true,
                TenantIds = [DefaultTenantId],
                ManifestPath = manifestPath,
                RequestedByPersonId = "recordarr-signature-trust-worker"
            };
            var optionsMonitor = new StaticOptionsMonitor<SignatureTrustServiceWorkerOptions>(options);
            var worker = new RecordArrSignatureTrustServiceWorker(
                provider.GetRequiredService<IServiceScopeFactory>(),
                new ManifestRecordArrSignatureTrustServiceManifestProvider(
                    optionsMonitor,
                    NullLogger<ManifestRecordArrSignatureTrustServiceManifestProvider>.Instance),
                optionsMonitor,
                NullLogger<RecordArrSignatureTrustServiceWorker>.Instance);

            await worker.RunOnceAsync();
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var completedJob = Assert.Single(verifyStore.GetSignatureTrustServiceJobs(DefaultTenantId));
        var completedSignature = Assert.Single(verifyStore.GetSignatureRecords(DefaultTenantId, "rec-bol-001"), item => item.SignatureRecordId == signatureId);
        var otherTenantJob = Assert.Single(verifyStore.GetSignatureTrustServiceJobs(otherTenantId));
        var verifiedOtherTenantSignature = Assert.Single(verifyStore.GetSignatureRecords(otherTenantId, otherTenantRecordId), item => item.SignatureRecordId == otherTenantSignatureId);

        Assert.Equal("completed", completedJob.Status);
        Assert.Equal("callback-worker-7781", completedJob.ProviderCallbackRef);
        Assert.Equal("tsa-worker-7781", completedJob.TrustTimestampAuthorityRef);
        Assert.False(string.IsNullOrWhiteSpace(completedJob.ProviderCallbackEvidenceHash));
        Assert.Equal("provider_verified", completedSignature.VerificationStatus);
        Assert.Equal("callback-worker-7781", completedSignature.ProviderCallbackRef);
        Assert.Equal("valid", completedSignature.LongTermValidationStatus);

        Assert.Equal("submitted", otherTenantJob.Status);
        Assert.Null(verifiedOtherTenantSignature.ProviderCallbackStatus);
        Assert.Contains(
            verifyStore.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
            log => log.Action == "signature.trust_service_job_reconciled" &&
                   log.Result == "allowed" &&
                   log.ReasonCode == "completed");
    }

    [Fact]
    public void PurgeRecord_marks_file_objects_as_deleted()
    {
        var store = CreateStore();
        var ownerPersonId = Guid.NewGuid().ToString("D");
        var principal = CreatePrincipal(personId: ownerPersonId);

        var record = store.CreateRecord(
            DefaultTenantId,
            "Purge candidate",
            "Checks file tombstone behavior.",
            "document",
            "other",
            "import_source",
            "uploaded",
            "internal",
            "nexarr",
            "smart_import_batch",
            "batch-001",
            "source.pdf",
            ownerPersonId,
            ownerPersonId,
            "purge-me.pdf",
            "application/pdf");

        store.PurgeRecord(record.RecordId, ownerPersonId);

        var purgedFile = store.GetFile(principal, record.CurrentFileRef);
        Assert.NotNull(purgedFile);
        Assert.NotNull(purgedFile!.DeletedAt);
        Assert.Equal("purge", purgedFile.DeleteReason);
    }

    [Fact]
    public void PurgeRecord_under_active_legal_hold_preserves_files_and_logs_denial()
    {
        var store = CreateStore();
        var ownerPersonId = Guid.NewGuid().ToString("D");
        var principal = CreatePrincipal(personId: ownerPersonId);

        var record = store.CreateRecord(
            DefaultTenantId,
            "Held purge candidate",
            "Checks legal-hold purge prevention.",
            "document",
            "other",
            "import_source",
            "uploaded",
            "internal",
            "nexarr",
            "smart_import_batch",
            "batch-002",
            "held-source.pdf",
            ownerPersonId,
            ownerPersonId,
            "held-purge-me.pdf",
            "application/pdf");

        var hold = store.CreateLegalHold(
            DefaultTenantId,
            "Direct purge hold",
            "Preserve the record during review.",
            "audit",
            "nexarr",
            "smart_import_batch",
            "batch-002",
            "person-record-admin",
            [],
            [record.RecordId]);
        store.ActivateLegalHold(DefaultTenantId, hold.LegalHoldId);

        var error = Assert.Throws<InvalidOperationException>(() =>
            store.PurgeRecord(record.RecordId, ownerPersonId));

        Assert.Contains("blocked by legal hold", error.Message);

        var preservedRecord = store.GetRecord(principal, record.RecordId);
        Assert.NotNull(preservedRecord);
        Assert.Equal(record.Status, preservedRecord!.Status);
        Assert.Null(preservedRecord.PurgedAt);

        var preservedFile = store.GetFile(principal, record.CurrentFileRef);
        Assert.NotNull(preservedFile);
        Assert.Null(preservedFile!.DeletedAt);
        Assert.Null(preservedFile.DeleteReason);

        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, record.RecordId),
            log => log.Action == "purge" &&
                   log.Result == "denied" &&
                   log.ActorPersonId == ownerPersonId &&
                   log.ReasonCode == "blocked_by_legal_hold");
    }

    [Fact]
    public void Access_policy_filters_records_for_authenticated_principal()
    {
        var store = CreateStore();
        var principal = CreatePrincipal(personId: "person-doc-controller", tenantRoleKey: "evidence-manager");

        var records = store.GetRecords(principal);

        Assert.NotEmpty(records);
        Assert.Contains(records, record => record.RecordId == "rec-bol-001");
    }

    [Fact]
    public void Invalid_access_policy_values_are_rejected()
    {
        var store = CreateStore();

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
        var store = CreateStore();

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
    public void External_share_denies_actions_outside_share_scope_and_logs_denial()
    {
        var store = CreateStore();
        var share = store.CreateExternalShare(
            DefaultTenantId,
            "rec-bol-001",
            "Route auditor",
            "auditor@example.com",
            "auditor_access",
            ["view"],
            "person-doc-controller");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            store.RecordExternalShareAccess(
                DefaultTenantId,
                share.ExternalShareId,
                "person-doc-controller",
                "download",
                "203.0.113.20",
                "RecordArrStoreTests/1.0"));

        Assert.Contains("does not allow download", exception.Message);
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
            log =>
                log.ExternalShareId == share.ExternalShareId &&
                log.Result == "denied" &&
                log.ReasonCode == "external-share-action-not-allowed");
    }

    [Fact]
    public void Redaction_creates_a_redacted_copy_and_link()
    {
        var store = CreateStore();
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
    public void Redactions_persist_locked_package_hash_review_evidence_and_tenant_scope()
    {
        var databaseName = $"recordarr-redaction-package-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        string redactionId;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var redaction = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-package",
                "privacy",
                "person-doc-controller",
                [" mask:phone ", "mask:signature", "mask:phone"]);

            redactionId = redaction.RedactionId;

            Assert.Equal("completed", redaction.Status);
            Assert.Equal("approved", redaction.ReviewStatus);
            Assert.Equal("person-doc-controller", redaction.ReviewedByPersonId);
            Assert.Equal("initial_redaction_review", redaction.ApprovalReason);
            Assert.Equal(redaction.RedactedAt, redaction.ReviewedAt);
            Assert.Equal(redaction.RedactedAt, redaction.LockedAt);
            Assert.False(string.IsNullOrWhiteSpace(redaction.RedactionPackageHash));
            Assert.Equal(["mask:phone", "mask:signature"], redaction.RedactionRules);

            var emptyRulesError = Assert.Throws<InvalidOperationException>(() =>
                store.CreateRedaction(
                    DefaultTenantId,
                    "rec-bol-001",
                    "rec-bol-001-redacted-empty",
                    "privacy",
                    "person-doc-controller",
                    []));
            Assert.Contains("At least one redaction rule", emptyRulesError.Message);
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var persisted = Assert.Single(verifyStore.GetRedactions(DefaultTenantId), redaction => redaction.RedactionId == redactionId);

        Assert.Equal("approved", persisted.ReviewStatus);
        Assert.Equal("person-doc-controller", persisted.ReviewedByPersonId);
        Assert.False(string.IsNullOrWhiteSpace(persisted.RedactionPackageHash));
        Assert.DoesNotContain(
            verifyStore.GetRedactions("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            redaction => redaction.RedactionId == redactionId);
    }

    [Fact]
    public void Redaction_provider_reconciliation_requires_matching_package_hash_and_persists_provider_evidence()
    {
        var databaseName = $"recordarr-redaction-provider-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        string verifiedRedactionId;
        string rejectedRedactionId;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var verifiedRedaction = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-provider",
                "privacy",
                "person-doc-controller",
                ["mask:phone", "mask:signature"]);
            var rejectedRedaction = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-provider-rejected",
                "legal",
                "person-doc-controller",
                ["mask:customer_signature"]);

            verifiedRedactionId = verifiedRedaction.RedactionId;
            rejectedRedactionId = rejectedRedaction.RedactionId;

            Assert.Throws<InvalidOperationException>(() =>
                store.ReconcileRedactionProviderStatus(
                    DefaultTenantId,
                    verifiedRedactionId,
                    "person-doc-controller",
                    "redact-provider",
                    "job-7781",
                    "completed",
                    "callback-mismatch",
                    "mismatched-package-hash"));

            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "redaction.provider_reconciled" &&
                       log.Result == "denied" &&
                       log.ReasonCode == "provider_package_hash_mismatch");

            var verified = store.ReconcileRedactionProviderStatus(
                DefaultTenantId,
                verifiedRedactionId,
                "person-doc-controller",
                "redact-provider",
                "job-7781",
                "completed",
                "callback-7781",
                verifiedRedaction.RedactionPackageHash);

            Assert.Equal("provider_verified", verified.ProviderReviewStatus);
            Assert.Null(verified.ProviderFailureReason);
            Assert.Equal("completed", verified.ProviderCallbackStatus);
            Assert.Equal(verifiedRedaction.RedactionPackageHash, verified.ProviderPackageHash);
            Assert.False(string.IsNullOrWhiteSpace(verified.ProviderEvidenceHash));
            Assert.Equal(verified.ProviderEvidenceHash, verified.Redaction.ProviderEvidenceHash);
            Assert.Equal("provider_redaction_verified", verified.Redaction.ApprovalReason);

            var rejected = store.ReconcileRedactionProviderStatus(
                DefaultTenantId,
                rejectedRedactionId,
                "person-doc-controller",
                "redact-provider",
                "job-7782",
                "needs_review",
                "callback-7782",
                rejectedRedaction.RedactionPackageHash);

            Assert.Equal("provider_rejected", rejected.ProviderReviewStatus);
            Assert.Equal("provider_needs_review", rejected.ProviderFailureReason);
            Assert.Equal("needs_review", rejected.ProviderCallbackStatus);
            Assert.Equal("provider_needs_review", rejected.Redaction.VerificationFailureReason);

            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "redaction.provider_reconciled" &&
                       log.Result == "allowed" &&
                       log.ReasonCode == "completed");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "redaction.provider_reconciled" &&
                       log.Result == "denied" &&
                       log.ReasonCode == "needs_review");
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var persistedVerified = Assert.Single(verifyStore.GetRedactions(DefaultTenantId), redaction => redaction.RedactionId == verifiedRedactionId);
        var persistedRejected = Assert.Single(verifyStore.GetRedactions(DefaultTenantId), redaction => redaction.RedactionId == rejectedRedactionId);

        Assert.Equal("provider_verified", persistedVerified.ProviderReviewStatus);
        Assert.Equal("completed", persistedVerified.ProviderCallbackStatus);
        Assert.Equal("redact-provider", persistedVerified.ProviderName);
        Assert.Equal("job-7781", persistedVerified.ProviderJobRef);
        Assert.False(string.IsNullOrWhiteSpace(persistedVerified.ProviderEvidenceHash));
        Assert.NotNull(persistedVerified.ProviderCallbackReceivedAt);

        Assert.Equal("provider_rejected", persistedRejected.ProviderReviewStatus);
        Assert.Equal("provider_needs_review", persistedRejected.ProviderFailureReason);
        Assert.Equal("needs_review", persistedRejected.ProviderCallbackStatus);
        Assert.DoesNotContain(
            verifyStore.GetRedactions("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            redaction => redaction.RedactionId == verifiedRedactionId ||
                         redaction.RedactionId == rejectedRedactionId);
    }

    [Fact]
    public void Redaction_provider_jobs_are_durable_and_reconcile_only_matching_provider_manifests()
    {
        var databaseName = $"recordarr-redaction-provider-jobs-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        string completedRedactionId;
        string failedRedactionId;
        string completedJobId;
        string failedJobId;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var completedRedaction = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-provider-job",
                "privacy",
                "person-doc-controller",
                ["mask:phone", "mask:signature"]);
            var failedRedaction = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-provider-job-failed",
                "legal",
                "person-doc-controller",
                ["mask:customer_signature"]);

            completedRedactionId = completedRedaction.RedactionId;
            failedRedactionId = failedRedaction.RedactionId;

            var submitted = store.SubmitRedactionProviderJob(
                DefaultTenantId,
                completedRedactionId,
                "person-doc-controller",
                "redact-provider",
                "job-redact-7781");
            var idempotentSubmit = store.SubmitRedactionProviderJob(
                DefaultTenantId,
                completedRedactionId,
                "person-doc-controller",
                "redact-provider",
                "job-redact-7781");
            var failedSubmit = store.SubmitRedactionProviderJob(
                DefaultTenantId,
                failedRedactionId,
                "person-doc-controller",
                "redact-provider",
                "job-redact-7782");

            completedJobId = submitted.ProviderJobId;
            failedJobId = failedSubmit.ProviderJobId;

            Assert.Equal(completedJobId, idempotentSubmit.ProviderJobId);
            Assert.Equal("submitted", submitted.Status);
            Assert.Equal(completedRedaction.RedactionPackageHash, submitted.RedactionPackageHash);
            Assert.False(string.IsNullOrWhiteSpace(submitted.SubmissionEvidenceHash));
            Assert.Equal(["mask:phone", "mask:signature"], submitted.RedactionRules);

            var mismatch = store.ProcessRedactionProviderJobManifest(
                DefaultTenantId,
                "person-doc-controller",
                "redact-provider",
                "job-redact-7782",
                "completed",
                "callback-redact-7782",
                "wrong-package-hash");

            Assert.Equal("failed", mismatch.Status);
            Assert.Equal("provider_package_hash_mismatch", mismatch.FailureReason);
            Assert.Null(mismatch.ProviderEvidenceHash);

            var completed = store.ProcessRedactionProviderJobManifest(
                DefaultTenantId,
                "person-doc-controller",
                "redact-provider",
                "job-redact-7781",
                "completed",
                "callback-redact-7781",
                completedRedaction.RedactionPackageHash);

            Assert.Equal("completed", completed.Status);
            Assert.Equal("completed", completed.ProviderCallbackStatus);
            Assert.False(string.IsNullOrWhiteSpace(completed.ProviderEvidenceHash));
            Assert.Equal("provider_verified", completed.Redaction?.ProviderReviewStatus);

            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "redaction.provider_job_submitted" &&
                       log.Result == "allowed" &&
                       log.ReasonCode == "job-redact-7781");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "redaction.provider_job_reconciled" &&
                       log.Result == "denied" &&
                       log.ReasonCode == "provider_package_hash_mismatch");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "redaction.provider_job_reconciled" &&
                       log.Result == "allowed" &&
                       log.ReasonCode == "completed");
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var persistedCompletedJob = Assert.Single(verifyStore.GetRedactionProviderJobs(DefaultTenantId), job => job.ProviderJobId == completedJobId);
        var persistedFailedJob = Assert.Single(verifyStore.GetRedactionProviderJobs(DefaultTenantId), job => job.ProviderJobId == failedJobId);
        var persistedCompletedRedaction = Assert.Single(verifyStore.GetRedactions(DefaultTenantId), redaction => redaction.RedactionId == completedRedactionId);
        var persistedFailedRedaction = Assert.Single(verifyStore.GetRedactions(DefaultTenantId), redaction => redaction.RedactionId == failedRedactionId);

        Assert.Equal("completed", persistedCompletedJob.Status);
        Assert.Equal("completed", persistedCompletedJob.ProviderCallbackStatus);
        Assert.False(string.IsNullOrWhiteSpace(persistedCompletedJob.ProviderEvidenceHash));
        Assert.Equal("provider_verified", persistedCompletedRedaction.ProviderReviewStatus);
        Assert.Equal("completed", persistedCompletedRedaction.ProviderCallbackStatus);

        Assert.Equal("failed", persistedFailedJob.Status);
        Assert.Equal("provider_package_hash_mismatch", persistedFailedJob.FailureReason);
        Assert.Null(persistedFailedJob.ProviderEvidenceHash);
        Assert.Null(persistedFailedRedaction.ProviderReviewStatus);
        Assert.DoesNotContain(
            verifyStore.GetRedactionProviderJobs("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            job => job.ProviderJobId == completedJobId ||
                   job.ProviderJobId == failedJobId);
    }

    [Fact]
    public async Task Redaction_provider_manifest_provider_returns_only_submitted_tenant_manifests()
    {
        var databaseName = $"recordarr-redaction-provider-manifest-provider-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        RecordArrRedactionProviderJobResponse submittedJob;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var redaction = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-provider-worker-provider",
                "privacy",
                "person-doc-controller",
                ["mask:phone"]);
            submittedJob = store.SubmitRedactionProviderJob(
                DefaultTenantId,
                redaction.RedactionId,
                "person-doc-controller",
                "redact-provider",
                "job-redact-provider-worker");
        }

        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-redaction-provider-manifest-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        manifests = new object[]
                        {
                            new
                            {
                                tenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
                                providerName = "redact-provider",
                                providerJobRef = "job-redact-provider-worker",
                                providerCallbackStatus = "completed",
                                providerCallbackRef = "wrong-tenant-callback",
                                redactionPackageHash = submittedJob.RedactionPackageHash
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                providerName = "redact-provider",
                                providerJobRef = "unknown-redaction-job",
                                providerCallbackStatus = "completed",
                                providerCallbackRef = "unknown-callback",
                                redactionPackageHash = submittedJob.RedactionPackageHash
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                providerName = "redact-provider",
                                providerJobRef = "job-redact-provider-worker",
                                providerCallbackStatus = "completed",
                                providerCallbackRef = "callback-redact-provider-worker",
                                redactionPackageHash = submittedJob.RedactionPackageHash
                            }
                        }
                    },
                    JsonOptions));

            var options = new StaticOptionsMonitor<RedactionProviderWorkerOptions>(new RedactionProviderWorkerOptions
            {
                Enabled = true,
                TenantIds = [DefaultTenantId],
                ManifestPath = manifestPath
            });
            var manifestProvider = new ManifestRecordArrRedactionProviderManifestProvider(
                options,
                NullLogger<ManifestRecordArrRedactionProviderManifestProvider>.Instance);

            var manifests = await manifestProvider.GetManifestsAsync(DefaultTenantId, [submittedJob], CancellationToken.None);

            var manifest = Assert.Single(manifests);
            Assert.Equal("redact-provider", manifest.ProviderName);
            Assert.Equal("job-redact-provider-worker", manifest.ProviderJobRef);
            Assert.Equal("completed", manifest.ProviderCallbackStatus);
            Assert.Equal("callback-redact-provider-worker", manifest.ProviderCallbackRef);
            Assert.Equal(submittedJob.RedactionPackageHash, manifest.RedactionPackageHash);
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }
    }

    [Fact]
    public async Task Redaction_provider_worker_requires_explicit_manifest_and_processes_only_matching_tenant_jobs()
    {
        var databaseName = $"recordarr-redaction-provider-worker-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        const string otherTenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        string redactionId;
        string redactionPackageHash;
        string otherTenantRedactionId;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var redaction = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-provider-worker",
                "privacy",
                "person-doc-controller",
                ["mask:phone", "mask:signature"]);
            redactionId = redaction.RedactionId;
            redactionPackageHash = redaction.RedactionPackageHash!;
            store.SubmitRedactionProviderJob(
                DefaultTenantId,
                redaction.RedactionId,
                "person-doc-controller",
                "redact-provider",
                "job-redact-worker");

            var otherTenantRecord = store.CreateRecord(
                otherTenantId,
                "Other tenant redaction packet",
                "Proves redaction manifest rows stay tenant-scoped.",
                "evidence_package",
                "operations",
                "redaction_provider",
                "active",
                "internal",
                "recordarr",
                "redaction_provider",
                "other-tenant-redaction-test",
                "Other tenant redaction packet",
                "person-doc-controller",
                "person-doc-controller",
                "other-tenant-redaction.pdf",
                "application/pdf",
                "recordarr",
                "other-tenant/redaction.pdf",
                4096);
            var otherTenantRedaction = store.CreateRedaction(
                otherTenantId,
                otherTenantRecord.RecordId,
                "other-tenant-redacted-provider-worker",
                "privacy",
                "person-doc-controller",
                ["mask:phone"]);
            otherTenantRedactionId = otherTenantRedaction.RedactionId;
            store.SubmitRedactionProviderJob(
                otherTenantId,
                otherTenantRedaction.RedactionId,
                "person-doc-controller",
                "redact-provider",
                "job-redact-other-tenant");
        }

        var missingManifestOptions = new RedactionProviderWorkerOptions
        {
            Enabled = true,
            TenantIds = [DefaultTenantId],
            ManifestPath = Path.Combine(Path.GetTempPath(), $"missing-redaction-provider-{Guid.NewGuid():N}.json")
        };
        var missingMonitor = new StaticOptionsMonitor<RedactionProviderWorkerOptions>(missingManifestOptions);
        var missingWorker = new RecordArrRedactionProviderWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new ManifestRecordArrRedactionProviderManifestProvider(
                missingMonitor,
                NullLogger<ManifestRecordArrRedactionProviderManifestProvider>.Instance),
            missingMonitor,
            NullLogger<RecordArrRedactionProviderWorker>.Instance);

        await missingWorker.RunOnceAsync();

        using (var missingVerifyScope = provider.CreateScope())
        {
            var store = missingVerifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var job = Assert.Single(store.GetRedactionProviderJobs(DefaultTenantId));
            Assert.Equal("submitted", job.Status);
            var redaction = Assert.Single(store.GetRedactions(DefaultTenantId), item => item.RedactionId == redactionId);
            Assert.Null(redaction.ProviderCallbackStatus);
        }

        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-redaction-provider-worker-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        manifests = new object[]
                        {
                            new
                            {
                                tenantId = otherTenantId,
                                providerName = "redact-provider",
                                providerJobRef = "job-redact-worker",
                                providerCallbackStatus = "completed",
                                providerCallbackRef = "wrong-tenant-callback",
                                redactionPackageHash
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                providerName = "redact-provider",
                                providerJobRef = "job-redact-worker",
                                providerCallbackStatus = "completed",
                                providerCallbackRef = "callback-redact-worker",
                                redactionPackageHash
                            }
                        }
                    },
                    JsonOptions));

            var options = new RedactionProviderWorkerOptions
            {
                Enabled = true,
                TenantIds = [DefaultTenantId],
                ManifestPath = manifestPath,
                RequestedByPersonId = "recordarr-redaction-provider-worker"
            };
            var optionsMonitor = new StaticOptionsMonitor<RedactionProviderWorkerOptions>(options);
            var worker = new RecordArrRedactionProviderWorker(
                provider.GetRequiredService<IServiceScopeFactory>(),
                new ManifestRecordArrRedactionProviderManifestProvider(
                    optionsMonitor,
                    NullLogger<ManifestRecordArrRedactionProviderManifestProvider>.Instance),
                optionsMonitor,
                NullLogger<RecordArrRedactionProviderWorker>.Instance);

            await worker.RunOnceAsync();
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var completedJob = Assert.Single(verifyStore.GetRedactionProviderJobs(DefaultTenantId));
        var completedRedaction = Assert.Single(verifyStore.GetRedactions(DefaultTenantId), item => item.RedactionId == redactionId);
        var otherTenantJob = Assert.Single(verifyStore.GetRedactionProviderJobs(otherTenantId));
        var verifiedOtherTenantRedaction = Assert.Single(verifyStore.GetRedactions(otherTenantId), item => item.RedactionId == otherTenantRedactionId);

        Assert.Equal("completed", completedJob.Status);
        Assert.Equal("callback-redact-worker", completedJob.ProviderCallbackRef);
        Assert.False(string.IsNullOrWhiteSpace(completedJob.ProviderEvidenceHash));
        Assert.Equal("provider_verified", completedRedaction.ProviderReviewStatus);
        Assert.Equal("callback-redact-worker", completedRedaction.ProviderCallbackRef);

        Assert.Equal("submitted", otherTenantJob.Status);
        Assert.Null(verifiedOtherTenantRedaction.ProviderCallbackStatus);
        Assert.Contains(
            verifyStore.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
            log => log.Action == "redaction.provider_job_reconciled" &&
                   log.Result == "allowed" &&
                   log.ReasonCode == "completed");
    }

    [Fact]
    public void Redaction_overlay_review_requires_rendered_evidence_and_persists_review_state()
    {
        var databaseName = $"recordarr-redaction-overlay-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        string approvedRedactionId;
        string changesRequestedRedactionId;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var approvedRedaction = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-overlay",
                "privacy",
                "person-doc-controller",
                ["mask:phone", "mask:signature"]);
            var changesRequestedRedaction = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-overlay-changes",
                "security",
                "person-doc-controller",
                ["mask:driver_license"]);

            approvedRedactionId = approvedRedaction.RedactionId;
            changesRequestedRedactionId = changesRequestedRedaction.RedactionId;

            Assert.Throws<InvalidOperationException>(() =>
                store.ReviewRedactionOverlay(
                    DefaultTenantId,
                    approvedRedactionId,
                    "person-doc-controller",
                    "approved",
                    [],
                    []));

            var approved = store.ReviewRedactionOverlay(
                DefaultTenantId,
                approvedRedactionId,
                "person-doc-controller",
                "approved",
                ["rendered-page-1-overlay", " rendered-page-1-overlay ", "rendered-page-2-overlay"],
                []);

            Assert.Equal("approved", approved.OverlayReviewStatus);
            Assert.Equal(["rendered-page-1-overlay", "rendered-page-2-overlay"], approved.OverlayEvidenceRefs);
            Assert.Empty(approved.OverlayIssueRefs);
            Assert.False(string.IsNullOrWhiteSpace(approved.OverlayReviewHash));
            Assert.Null(approved.OverlayFailureReason);
            Assert.Equal("visual_overlay_review_approved", approved.Redaction.ApprovalReason);

            var changesRequested = store.ReviewRedactionOverlay(
                DefaultTenantId,
                changesRequestedRedactionId,
                "person-doc-controller",
                "changes_requested",
                ["rendered-page-3-overlay"],
                ["unmasked-footer"]);

            Assert.Equal("changes_requested", changesRequested.OverlayReviewStatus);
            Assert.Equal("overlay_changes_requested", changesRequested.OverlayFailureReason);
            Assert.Equal("overlay_changes_requested", changesRequested.Redaction.VerificationFailureReason);
            Assert.Equal(["unmasked-footer"], changesRequested.OverlayIssueRefs);

            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "redaction.overlay_reviewed" &&
                       log.Result == "allowed" &&
                       log.ReasonCode == "approved");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "redaction.overlay_reviewed" &&
                       log.Result == "denied" &&
                       log.ReasonCode == "changes_requested");
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var persistedApproved = Assert.Single(verifyStore.GetRedactions(DefaultTenantId), redaction => redaction.RedactionId == approvedRedactionId);
        var persistedChangesRequested = Assert.Single(verifyStore.GetRedactions(DefaultTenantId), redaction => redaction.RedactionId == changesRequestedRedactionId);

        Assert.Equal("approved", persistedApproved.OverlayReviewStatus);
        Assert.Equal(["rendered-page-1-overlay", "rendered-page-2-overlay"], persistedApproved.OverlayEvidenceRefs);
        Assert.False(string.IsNullOrWhiteSpace(persistedApproved.OverlayReviewHash));

        Assert.Equal("changes_requested", persistedChangesRequested.OverlayReviewStatus);
        Assert.Equal("overlay_changes_requested", persistedChangesRequested.OverlayFailureReason);
        Assert.Equal(["unmasked-footer"], persistedChangesRequested.OverlayIssueRefs);
        Assert.DoesNotContain(
            verifyStore.GetRedactions("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            redaction => redaction.RedactionId == approvedRedactionId ||
                         redaction.RedactionId == changesRequestedRedactionId);
    }

    [Fact]
    public async Task Redaction_overlay_manifest_provider_returns_only_reviewable_tenant_manifests()
    {
        var databaseName = $"recordarr-redaction-overlay-provider-{Guid.NewGuid():N}";
        using var provider = CreateStoreProvider(databaseName);
        var otherTenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        string reviewableRedactionId;
        string reviewablePackageHash;
        string alreadyReviewedRedactionId;
        string alreadyReviewedPackageHash;
        string otherTenantRedactionId;
        string otherTenantPackageHash;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var reviewable = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-overlay-provider",
                "privacy",
                "person-doc-controller",
                ["mask:phone"]);
            var alreadyReviewed = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-overlay-provider-reviewed",
                "security",
                "person-doc-controller",
                ["mask:driver_license"]);
            var otherTenantRecord = store.CreateRecord(
                otherTenantId,
                "Other tenant overlay source",
                "Validates tenant filtering for redaction overlay manifests.",
                "document",
                "operations",
                "safety",
                "incident",
                "internal",
                "recordarr",
                "record",
                "other-tenant-overlay-source",
                "Other Tenant Overlay Source",
                "person-other-owner",
                "person-other-owner",
                "other-overlay-source.pdf",
                "application/pdf");
            var otherTenantRedaction = store.CreateRedaction(
                otherTenantId,
                otherTenantRecord.RecordId,
                "other-tenant-redacted-overlay-provider",
                "privacy",
                "person-other-owner",
                ["mask:phone"]);

            store.ReviewRedactionOverlay(
                DefaultTenantId,
                alreadyReviewed.RedactionId,
                "person-doc-controller",
                "approved",
                ["already-rendered-overlay"],
                []);

            reviewableRedactionId = reviewable.RedactionId;
            reviewablePackageHash = reviewable.RedactionPackageHash;
            alreadyReviewedRedactionId = alreadyReviewed.RedactionId;
            alreadyReviewedPackageHash = alreadyReviewed.RedactionPackageHash;
            otherTenantRedactionId = otherTenantRedaction.RedactionId;
            otherTenantPackageHash = otherTenantRedaction.RedactionPackageHash;
        }

        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-redaction-overlay-provider-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        manifests = new object[]
                        {
                            new
                            {
                                tenantId = otherTenantId,
                                redactionId = otherTenantRedactionId,
                                redactionPackageHash = otherTenantPackageHash,
                                overlayReviewStatus = "approved",
                                overlayEvidenceRefs = new[] { "wrong-tenant-rendered-overlay" },
                                overlayIssueRefs = Array.Empty<string>()
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                redactionId = alreadyReviewedRedactionId,
                                redactionPackageHash = alreadyReviewedPackageHash,
                                overlayReviewStatus = "approved",
                                overlayEvidenceRefs = new[] { "already-reviewed-rendered-overlay" },
                                overlayIssueRefs = Array.Empty<string>()
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                redactionId = reviewableRedactionId,
                                redactionPackageHash = "stale-package-hash",
                                overlayReviewStatus = "approved",
                                overlayEvidenceRefs = new[] { "stale-rendered-overlay" },
                                overlayIssueRefs = Array.Empty<string>()
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                redactionId = reviewableRedactionId,
                                redactionPackageHash = reviewablePackageHash,
                                overlayReviewStatus = "changes_requested",
                                overlayEvidenceRefs = new[] { " rendered-page-1-overlay ", "rendered-page-1-overlay" },
                                overlayIssueRefs = new[] { "unmasked-footer" }
                            }
                        }
                    },
                    JsonOptions));

            using var verifyScope = provider.CreateScope();
            var store = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var options = new StaticOptionsMonitor<RedactionOverlayReviewWorkerOptions>(new RedactionOverlayReviewWorkerOptions
            {
                Enabled = true,
                TenantIds = [DefaultTenantId],
                ManifestPath = manifestPath
            });
            var manifestProvider = new ManifestRecordArrRedactionOverlayReviewManifestProvider(
                options,
                NullLogger<ManifestRecordArrRedactionOverlayReviewManifestProvider>.Instance);

            var manifests = await manifestProvider.GetManifestsAsync(
                DefaultTenantId,
                store.GetRedactions(DefaultTenantId),
                CancellationToken.None);

            var manifest = Assert.Single(manifests);
            Assert.Equal(reviewableRedactionId, manifest.RedactionId);
            Assert.Equal(reviewablePackageHash, manifest.RedactionPackageHash);
            Assert.Equal("changes_requested", manifest.OverlayReviewStatus);
            Assert.Equal(["rendered-page-1-overlay"], manifest.OverlayEvidenceRefs);
            Assert.Equal(["unmasked-footer"], manifest.OverlayIssueRefs);
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }
    }

    [Fact]
    public async Task Redaction_overlay_worker_requires_explicit_manifest_and_processes_only_matching_tenant_redactions()
    {
        var databaseName = $"recordarr-redaction-overlay-worker-{Guid.NewGuid():N}";
        await using var provider = CreateStoreProvider(databaseName);
        var otherTenantId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        string redactionId;
        string redactionPackageHash;
        string otherTenantRedactionId;
        string otherTenantPackageHash;

        using (var seedScope = provider.CreateScope())
        {
            var store = seedScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var redaction = store.CreateRedaction(
                DefaultTenantId,
                "rec-bol-001",
                "rec-bol-001-redacted-overlay-worker",
                "privacy",
                "person-doc-controller",
                ["mask:phone"]);
            var otherTenantRecord = store.CreateRecord(
                otherTenantId,
                "Other tenant overlay worker source",
                "Validates tenant filtering for redaction overlay worker.",
                "document",
                "operations",
                "safety",
                "incident",
                "internal",
                "recordarr",
                "record",
                "other-tenant-overlay-worker-source",
                "Other Tenant Overlay Worker Source",
                "person-other-owner",
                "person-other-owner",
                "other-overlay-worker-source.pdf",
                "application/pdf");
            var otherTenantRedaction = store.CreateRedaction(
                otherTenantId,
                otherTenantRecord.RecordId,
                "other-tenant-redacted-overlay-worker",
                "privacy",
                "person-other-owner",
                ["mask:phone"]);

            redactionId = redaction.RedactionId;
            redactionPackageHash = redaction.RedactionPackageHash;
            otherTenantRedactionId = otherTenantRedaction.RedactionId;
            otherTenantPackageHash = otherTenantRedaction.RedactionPackageHash;
        }

        var missingManifestOptions = new StaticOptionsMonitor<RedactionOverlayReviewWorkerOptions>(new RedactionOverlayReviewWorkerOptions
        {
            Enabled = true,
            TenantIds = [DefaultTenantId],
            RequestedByPersonId = "recordarr-redaction-overlay-worker"
        });
        var missingWorker = new RecordArrRedactionOverlayReviewWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            new ManifestRecordArrRedactionOverlayReviewManifestProvider(
                missingManifestOptions,
                NullLogger<ManifestRecordArrRedactionOverlayReviewManifestProvider>.Instance),
            missingManifestOptions,
            NullLogger<RecordArrRedactionOverlayReviewWorker>.Instance);

        await missingWorker.RunOnceAsync();

        using (var missingVerifyScope = provider.CreateScope())
        {
            var store = missingVerifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
            var redaction = Assert.Single(store.GetRedactions(DefaultTenantId), item => item.RedactionId == redactionId);
            Assert.Null(redaction.OverlayReviewStatus);
        }

        var manifestPath = Path.Combine(Path.GetTempPath(), $"recordarr-redaction-overlay-worker-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                manifestPath,
                JsonSerializer.Serialize(
                    new
                    {
                        manifests = new object[]
                        {
                            new
                            {
                                tenantId = otherTenantId,
                                redactionId = otherTenantRedactionId,
                                redactionPackageHash = otherTenantPackageHash,
                                overlayReviewStatus = "approved",
                                overlayEvidenceRefs = new[] { "wrong-tenant-rendered-overlay" },
                                overlayIssueRefs = Array.Empty<string>()
                            },
                            new
                            {
                                tenantId = DefaultTenantId,
                                redactionId,
                                redactionPackageHash,
                                overlayReviewStatus = "approved",
                                overlayEvidenceRefs = new[] { "rendered-overlay-page-1", "rendered-overlay-page-2" },
                                overlayIssueRefs = Array.Empty<string>()
                            }
                        }
                    },
                    JsonOptions));

            var options = new RedactionOverlayReviewWorkerOptions
            {
                Enabled = true,
                TenantIds = [DefaultTenantId],
                ManifestPath = manifestPath,
                RequestedByPersonId = "recordarr-redaction-overlay-worker"
            };
            var optionsMonitor = new StaticOptionsMonitor<RedactionOverlayReviewWorkerOptions>(options);
            var worker = new RecordArrRedactionOverlayReviewWorker(
                provider.GetRequiredService<IServiceScopeFactory>(),
                new ManifestRecordArrRedactionOverlayReviewManifestProvider(
                    optionsMonitor,
                    NullLogger<ManifestRecordArrRedactionOverlayReviewManifestProvider>.Instance),
                optionsMonitor,
                NullLogger<RecordArrRedactionOverlayReviewWorker>.Instance);

            await worker.RunOnceAsync();
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }

        using var verifyScope = provider.CreateScope();
        var verifyStore = verifyScope.ServiceProvider.GetRequiredService<RecordArrStore>();
        var reviewedRedaction = Assert.Single(verifyStore.GetRedactions(DefaultTenantId), item => item.RedactionId == redactionId);
        var verifiedOtherTenantRedaction = Assert.Single(verifyStore.GetRedactions(otherTenantId), item => item.RedactionId == otherTenantRedactionId);

        Assert.Equal("approved", reviewedRedaction.OverlayReviewStatus);
        Assert.Equal("recordarr-redaction-overlay-worker", reviewedRedaction.OverlayReviewedByPersonId);
        Assert.Equal(["rendered-overlay-page-1", "rendered-overlay-page-2"], reviewedRedaction.OverlayEvidenceRefs);
        Assert.False(string.IsNullOrWhiteSpace(reviewedRedaction.OverlayReviewHash));
        Assert.Null(reviewedRedaction.OverlayFailureReason);

        Assert.Null(verifiedOtherTenantRedaction.OverlayReviewStatus);
        Assert.Contains(
            verifyStore.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
            log => log.Action == "redaction.overlay_reviewed" &&
                   log.Result == "allowed" &&
                   log.ReasonCode == "approved");
    }

    [Fact]
    public void Invalid_access_grant_values_are_rejected()
    {
        var store = CreateStore();

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
        var store = CreateStore();
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
        var store = CreateStore();
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
        var store = CreateStore();

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
        var store = CreateStore();

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
        var store = CreateStore();

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
        var store = CreateStore();

        var package = store.CreatePackage(
            DefaultTenantId,
            "Trip closeout packet",
            "delivery",
            "routarr",
            "routarr:trip:trip-7781",
            "rec-bol-001",
            "person-evidence-manager");

        var manifest = store.GetManifest(DefaultTenantId, package.PackageId);

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
        var store = CreateStore();
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
        var store = CreateStore();
        var ownerPersonId = Guid.NewGuid().ToString("D");
        var principal = CreatePrincipal(personId: ownerPersonId);

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
            ownerPersonId,
            ownerPersonId,
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
        var store = CreateStore();

        var filtered = store.GetAccessLogs("rec-bol-001");

        Assert.NotEmpty(filtered);
        Assert.All(filtered, log => Assert.Equal("rec-bol-001", log.RecordId));
    }

    [Fact]
    public void Legal_hold_scope_rules_block_matching_records_until_release()
    {
        var store = CreateStore();
        const string tenantId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

        var hold = store.CreateLegalHold(
            tenantId,
            "RoutArr audit hold",
            "Hold delivery evidence while audit proceeds.",
            "audit",
            "routarr",
            "trip",
            "trip-7781",
            "person-record-admin",
            ["source_product:routarr"],
            []);

        store.ActivateLegalHold(tenantId, hold.LegalHoldId);

        var blockedStatus = store.GetRetentionStatus(tenantId, "rec-bol-001");
        Assert.Equal("blocked_by_legal_hold", blockedStatus?.Status);

        store.ReleaseLegalHold(tenantId, hold.LegalHoldId, "person-record-admin", "Audit complete.");

        var restoredStatus = store.GetRetentionStatus(tenantId, "rec-bol-001");
        Assert.Equal("active", restoredStatus?.Status);
    }

    [Fact]
    public void Legal_hold_retention_status_refresh_writes_durable_audit_evidence()
    {
        var dbName = $"recordarr-legal-hold-retention-refresh-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string holdId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var hold = store.CreateLegalHold(
                DefaultTenantId,
                "Retention refresh hold",
                "Prove retention status hold transitions are not silent.",
                "legal",
                "recordarr",
                "record",
                "rec-bol-001",
                "person-record-admin",
                [],
                ["rec-bol-001"]);
            store.ActivateLegalHold(DefaultTenantId, hold.LegalHoldId);
            holdId = hold.LegalHoldId;
        }

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);

            Assert.Equal("blocked_by_legal_hold", store.GetRetentionStatus(DefaultTenantId, "rec-bol-001")?.Status);
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "retention_status.blocked_by_legal_hold" &&
                       log.Result == "denied" &&
                       log.ReasonCode == "blocked_by_legal_hold");

            store.ReleaseLegalHold(DefaultTenantId, holdId, "person-record-admin", "Hold complete.");
        }

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);

            Assert.Equal("active", store.GetRetentionStatus(DefaultTenantId, "rec-bol-001")?.Status);
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, "rec-bol-001"),
                log => log.Action == "retention_status.restored_after_legal_hold" &&
                       log.Result == "allowed" &&
                       log.ReasonCode == "active");
        }
    }

    [Fact]
    public void Invalid_legal_hold_values_are_rejected()
    {
        var store = CreateStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateLegalHold(
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
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
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
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
        var store = CreateStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentDistribution(
                DefaultTenantId,
                "doc-001",
                "ver-002",
                "not-a-distribution-type",
                "person-doc-controller"));

        Assert.Throws<InvalidOperationException>(() =>
            store.RequestDocumentReview(
                DefaultTenantId,
                "doc-001",
                "ver-002",
                "not-a-review-type",
                "person-doc-controller",
                "person-reviewer",
                null));

        Assert.Throws<InvalidOperationException>(() =>
            store.UpdateControlledDocumentStatus(
                DefaultTenantId,
                "doc-001",
                "not-a-document-status",
                "person-doc-controller"));
    }

    [Fact]
    public void Invalid_disposal_review_values_are_rejected()
    {
        var store = CreateStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDisposalReview(
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                "rec-bol-001",
                "rstat-001",
                "not-a-proposed-action",
                "person-record-admin"));

        var review = store.CreateDisposalReview(
            "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            "rec-bol-001",
            "rstat-001",
            "retain",
            "person-record-admin");

        Assert.Throws<InvalidOperationException>(() =>
            store.CompleteDisposalReview(
                "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                review.DisposalReviewId,
                "not-a-review-status",
                "person-record-admin",
                "Nope"));
    }

    [Fact]
    public void Approved_archive_disposal_review_executes_through_scheduler_without_immediate_side_effects()
    {
        var dbName = $"recordarr-approved-archive-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string recordId;
        string reviewId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var record = store.CreateRecord(
                DefaultTenantId,
                "Expired archive retention record",
                "Record eligible for approved archive execution.",
                "document",
                "other",
                "retention_test",
                "expired",
                "internal",
                "recordarr",
                "record",
                "rec-retention-scheduler-archive-001",
                "Expired archive retention record",
                "person-record-admin",
                "person-record-admin",
                "archive-execution.pdf",
                "application/pdf",
                "recordarr",
                "tenant/archive-execution.pdf",
                8192);
            store.CreateFileMalwareScan(DefaultTenantId, record.CurrentFileRef, "person-record-admin", "clean");
            recordId = record.RecordId;
            store.UpdateRecordStatus(
                record.RecordId,
                "active",
                null,
                DateTimeOffset.UtcNow.AddDays(-400),
                null);
            var retentionStatus = store.RecalculateRetentionStatuses(DefaultTenantId)
                .Single(status => status.RecordId == record.RecordId);

            var review = store.CreateDisposalReview(
                DefaultTenantId,
                recordId,
                retentionStatus.RetentionStatusId,
                "archive",
                "person-record-admin");
            reviewId = review.DisposalReviewId;

            var approvedReview = store.CompleteDisposalReview(
                DefaultTenantId,
                review.DisposalReviewId,
                "approved",
                "person-record-admin",
                "Retention elapsed and no active legal hold.");
            Assert.Equal("approved", approvedReview.Status);
            Assert.Equal("eligible_for_archive", store.GetRetentionStatus(DefaultTenantId, record.RecordId)!.Status);

            var executionRun = store.RunRetentionDispositionScheduler(DefaultTenantId, "person-record-admin", "execute_approved_reviews");

            Assert.Equal("completed", executionRun.Status);
            Assert.Equal("execute_approved_reviews", executionRun.ExecutionPolicy);
            Assert.Equal(1, executionRun.AutomaticExecutionCount);
            Assert.Equal(0, executionRun.CreatedReviewCount);
            Assert.Equal(0, executionRun.NotificationMessageCount);
            Assert.Equal("completed", store.GetDisposalReviews(DefaultTenantId).Single(item => item.DisposalReviewId == review.DisposalReviewId).Status);
            Assert.Equal("archived", store.GetRetentionStatus(DefaultTenantId, recordId)!.Status);
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, recordId),
                log => log.Action == "archived" && log.Result == "allowed");
            Assert.Contains(
                store.GetAccessLogs(DefaultTenantId, recordId),
                log => log.Action == "retention.scheduler.approved_review_executed" && log.Result == "allowed");
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            Assert.Equal("archived", recreated.GetRetentionStatus(DefaultTenantId, recordId)!.Status);
            Assert.Equal("completed", recreated.GetDisposalReviews(DefaultTenantId).Single(review => review.DisposalReviewId == reviewId).Status);
            Assert.DoesNotContain(
                recreated.GetDisposalReviews(Guid.NewGuid().ToString()),
                other => other.DisposalReviewId == reviewId);
        }
    }

    [Fact]
    public void Retention_disposition_scheduler_creates_pending_reviews_idempotently()
    {
        var store = CreateStore();
        var record = store.CreateRecord(
            DefaultTenantId,
            "Expired retention record",
            "Record eligible for scheduled disposition review.",
            "document",
            "other",
            "retention_test",
            "expired",
            "internal",
            "recordarr",
            "record",
            "rec-retention-scheduler-001",
            "Expired retention record",
            "person-record-admin",
            "person-record-admin",
            "expired-retention.pdf",
            "application/pdf",
            "recordarr",
            "tenant/expired-retention.pdf",
            8192);

        store.UpdateRecordStatus(
            record.RecordId,
            "active",
            null,
            DateTimeOffset.UtcNow.AddDays(-400),
            null);

        var firstRun = store.RunRetentionDispositionScheduler(DefaultTenantId, "person-record-admin");

        Assert.False(string.IsNullOrWhiteSpace(firstRun.SchedulerRunId));
        Assert.False(string.IsNullOrWhiteSpace(firstRun.LeaseId));
        Assert.Equal("completed", firstRun.Status);
        Assert.Equal("create_pending_reviews_only", firstRun.ExecutionPolicy);
        Assert.Equal(1, firstRun.CreatedReviewCount);
        Assert.Equal(0, firstRun.AutomaticExecutionCount);
        Assert.Equal(1, firstRun.NotificationMessageCount);
        Assert.Contains(firstRun.CreatedDisposalReviewRefs, reviewRef => !string.IsNullOrWhiteSpace(reviewRef));
        Assert.Contains(firstRun.OutboxMessageRefs, messageRef => !string.IsNullOrWhiteSpace(messageRef));

        var review = Assert.Single(store.GetDisposalReviews(DefaultTenantId), item => item.RecordId == record.RecordId);
        Assert.Equal("archive", review.ProposedAction);
        Assert.Equal("pending", review.Status);
        var lease = Assert.Single(store.GetRetentionSchedulerLeases(DefaultTenantId), item => item.LeaseId == firstRun.LeaseId);
        Assert.Equal("released", lease.Status);
        Assert.Equal(firstRun.SchedulerRunId, lease.SchedulerRunId);
        var outboxMessage = Assert.Single(store.GetRetentionSchedulerOutboxMessages(DefaultTenantId), item => item.OutboxMessageId == firstRun.OutboxMessageRefs.Single());
        Assert.Equal("pending", outboxMessage.Status);
        Assert.Equal(record.RecordId, outboxMessage.TargetRecordId);
        Assert.Equal(review.DisposalReviewId, outboxMessage.DisposalReviewRef);
        Assert.Equal(0, outboxMessage.DeliveryAttemptCount);

        var deliveryRun = store.ProcessRetentionSchedulerOutbox(DefaultTenantId, "person-record-admin");

        Assert.Equal(1, deliveryRun.PendingBeforeCount);
        Assert.Equal(1, deliveryRun.DeliveredCount);
        Assert.Equal(0, deliveryRun.FailedCount);
        Assert.Contains(outboxMessage.OutboxMessageId, deliveryRun.DeliveredMessageRefs);
        var deliveredOutboxMessage = Assert.Single(store.GetRetentionSchedulerOutboxMessages(DefaultTenantId), item => item.OutboxMessageId == outboxMessage.OutboxMessageId);
        Assert.Equal("delivered", deliveredOutboxMessage.Status);
        Assert.Equal(1, deliveredOutboxMessage.DeliveryAttemptCount);
        Assert.NotNull(deliveredOutboxMessage.DeliveredAt);
        Assert.Equal("person-record-admin", deliveredOutboxMessage.DeliveredByPersonId);
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, record.RecordId),
            log => log.Action == "retention.scheduler.review_created" && log.Result == "allowed");
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, record.RecordId),
            log => log.Action == "retention.scheduler.outbox_delivered" && log.Result == "allowed");

        var secondRun = store.RunRetentionDispositionScheduler(DefaultTenantId, "person-record-admin");

        Assert.Equal(0, secondRun.CreatedReviewCount);
        Assert.Equal(0, secondRun.NotificationMessageCount);
        Assert.True(secondRun.SkippedExistingReviewCount >= 1);
        Assert.Single(store.GetDisposalReviews(DefaultTenantId), item => item.RecordId == record.RecordId);
        var secondDeliveryRun = store.ProcessRetentionSchedulerOutbox(DefaultTenantId, "person-record-admin");
        Assert.Equal(0, secondDeliveryRun.PendingBeforeCount);
        Assert.Equal(0, secondDeliveryRun.DeliveredCount);
    }

    [Fact]
    public void Retention_disposition_scheduler_rejects_unknown_execution_policy_without_side_effects()
    {
        var store = CreateStore();
        var record = store.CreateRecord(
            DefaultTenantId,
            "Expired automatic disposition record",
            "Record eligible for scheduled disposition but not automatic execution.",
            "document",
            "other",
            "retention_test",
            "expired",
            "internal",
            "recordarr",
            "record",
            "rec-retention-scheduler-auto-001",
            "Expired automatic disposition record",
            "person-record-admin",
            "person-record-admin",
            "expired-auto-retention.pdf",
            "application/pdf",
            "recordarr",
            "tenant/expired-auto-retention.pdf",
            8192);

        store.UpdateRecordStatus(
            record.RecordId,
            "active",
            null,
            DateTimeOffset.UtcNow.AddDays(-400),
            null);

        var failedRun = store.RunRetentionDispositionScheduler(DefaultTenantId, "person-record-admin", "execute_without_review");

        Assert.Equal("failed", failedRun.Status);
        Assert.Equal("execute_without_review", failedRun.ExecutionPolicy);
        Assert.Equal(1, failedRun.EligibleRecordCount);
        Assert.Equal(0, failedRun.CreatedReviewCount);
        Assert.Equal(0, failedRun.AutomaticExecutionCount);
        Assert.Equal(0, failedRun.NotificationMessageCount);
        Assert.Empty(failedRun.CreatedDisposalReviewRefs);
        Assert.Empty(failedRun.OutboxMessageRefs);
        Assert.Contains("only creates pending human review records", failedRun.FailureReason);
        Assert.DoesNotContain(store.GetDisposalReviews(DefaultTenantId), review => review.RecordId == record.RecordId);
        Assert.Empty(store.GetDestructionCertificates(DefaultTenantId, record.RecordId));
        Assert.DoesNotContain(store.GetRetentionSchedulerOutboxMessages(DefaultTenantId), message => message.TargetRecordId == record.RecordId);
        var releasedLease = Assert.Single(store.GetRetentionSchedulerLeases(DefaultTenantId), lease => lease.LeaseId == failedRun.LeaseId);
        Assert.Equal("released", releasedLease.Status);
        var persistedRun = Assert.Single(store.GetRetentionSchedulerRuns(DefaultTenantId), run => run.SchedulerRunId == failedRun.SchedulerRunId);
        Assert.Equal("failed", persistedRun.Status);
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, record.RecordId),
            log => log.Action == "retention.scheduler.execution_policy_unsupported" &&
                log.Result == "denied" &&
                log.ReasonCode == "execute_without_review");
        Assert.Equal("eligible_for_archive", store.GetRetentionStatus(DefaultTenantId, record.RecordId)!.Status);
    }

    [Fact]
    public void Retention_disposition_scheduler_executes_only_approved_reviews_and_preserves_held_records()
    {
        var store = CreateStore();
        var archiveRecord = store.CreateRecord(
            DefaultTenantId,
            "Approved archive retention record",
            "Record eligible for approved archive execution.",
            "document",
            "other",
            "retention_test",
            "expired",
            "internal",
            "recordarr",
            "record",
            "rec-retention-approved-archive-001",
            "Approved archive retention record",
            "person-record-admin",
            "person-record-admin",
            "approved-archive-retention.pdf",
            "application/pdf",
            "recordarr",
            "tenant/approved-archive-retention.pdf",
            8192);
        var heldRecord = store.CreateRecord(
            DefaultTenantId,
            "Held approved purge retention record",
            "Record eligible for approved purge execution until legal hold blocks it.",
            "document",
            "other",
            "retention_test",
            "expired",
            "internal",
            "recordarr",
            "record",
            "rec-retention-approved-held-001",
            "Held approved purge retention record",
            "person-record-admin",
            "person-record-admin",
            "approved-held-retention.pdf",
            "application/pdf",
            "recordarr",
            "tenant/approved-held-retention.pdf",
            8192);
        store.CreateFileMalwareScan(DefaultTenantId, archiveRecord.CurrentFileRef, "person-record-admin", "clean");
        store.CreateFileMalwareScan(DefaultTenantId, heldRecord.CurrentFileRef, "person-record-admin", "clean");
        store.UpdateRecordStatus(archiveRecord.RecordId, "active", null, DateTimeOffset.UtcNow.AddDays(-400), null);
        store.UpdateRecordStatus(heldRecord.RecordId, "active", null, DateTimeOffset.UtcNow.AddDays(-400), null);
        var statuses = store.RecalculateRetentionStatuses(DefaultTenantId);
        var archiveStatus = statuses.Single(status => status.RecordId == archiveRecord.RecordId);
        var heldStatus = statuses.Single(status => status.RecordId == heldRecord.RecordId);
        var archiveReview = store.CreateDisposalReview(
            DefaultTenantId,
            archiveRecord.RecordId,
            archiveStatus.RetentionStatusId,
            "archive",
            "person-record-admin");
        var heldReview = store.CreateDisposalReview(
            DefaultTenantId,
            heldRecord.RecordId,
            heldStatus.RetentionStatusId,
            "purge",
            "person-record-admin");
        store.CompleteDisposalReview(DefaultTenantId, archiveReview.DisposalReviewId, "approved", "person-record-admin", "Approved archive.");
        store.CompleteDisposalReview(DefaultTenantId, heldReview.DisposalReviewId, "approved", "person-record-admin", "Approved purge before hold.");
        var hold = store.CreateLegalHold(
            DefaultTenantId,
            "Approved review execution hold",
            "Preserve approved disposal review while hold is active.",
            "legal",
            "recordarr",
            "record",
            heldRecord.RecordId,
            "person-record-admin",
            [],
            [heldRecord.RecordId]);
        store.ActivateLegalHold(DefaultTenantId, hold.LegalHoldId);

        var executionRun = store.RunRetentionDispositionScheduler(DefaultTenantId, "person-record-admin", "execute_approved_reviews");

        Assert.Equal("completed", executionRun.Status);
        Assert.Equal("execute_approved_reviews", executionRun.ExecutionPolicy);
        Assert.Equal(1, executionRun.AutomaticExecutionCount);
        Assert.Equal(1, executionRun.BlockedByLegalHoldCount);
        Assert.Contains(heldRecord.RecordId, executionRun.BlockedRecordRefs);
        Assert.Equal("completed", store.GetDisposalReviews(DefaultTenantId).Single(review => review.DisposalReviewId == archiveReview.DisposalReviewId).Status);
        Assert.Equal("approved", store.GetDisposalReviews(DefaultTenantId).Single(review => review.DisposalReviewId == heldReview.DisposalReviewId).Status);
        Assert.Equal("archived", store.GetRetentionStatus(DefaultTenantId, archiveRecord.RecordId)!.Status);
        Assert.Equal("blocked_by_legal_hold", store.GetRetentionStatus(DefaultTenantId, heldRecord.RecordId)!.Status);
        Assert.Empty(store.GetDestructionCertificates(DefaultTenantId, heldRecord.RecordId));
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, archiveRecord.RecordId),
            log => log.Action == "retention.scheduler.approved_review_executed" && log.Result == "allowed");
        Assert.Contains(
            store.GetAccessLogs(DefaultTenantId, heldRecord.RecordId),
            log => log.Action == "retention.scheduler.approved_review_blocked_by_legal_hold" &&
                log.Result == "denied" &&
                log.ReasonCode == "blocked_by_legal_hold");

        var retry = store.RunRetentionDispositionScheduler(DefaultTenantId, "person-record-admin", "execute_approved_reviews");

        Assert.Equal(0, retry.AutomaticExecutionCount);
        Assert.Equal(1, retry.BlockedByLegalHoldCount);
        Assert.Single(store.GetDisposalReviews(DefaultTenantId), review => review.DisposalReviewId == archiveReview.DisposalReviewId);
        Assert.Empty(store.GetDestructionCertificates(DefaultTenantId, heldRecord.RecordId));
    }

    [Fact]
    public void Retention_disposition_scheduler_persists_run_lease_and_outbox_evidence()
    {
        var dbName = $"recordarr-retention-scheduler-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string schedulerRunId;
        string leaseId;
        string outboxMessageId;
        string recordId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var record = store.CreateRecord(
                DefaultTenantId,
                "Durable retention scheduler record",
                "Record eligible for durable scheduled disposition review.",
                "document",
                "other",
                "retention_test",
                "expired",
                "internal",
                "recordarr",
                "record",
                "rec-retention-scheduler-durable-001",
                "Durable retention scheduler record",
                "person-record-admin",
                "person-record-admin",
                "durable-retention.pdf",
                "application/pdf",
                "recordarr",
                "tenant/durable-retention.pdf",
                8192);
            recordId = record.RecordId;
            store.UpdateRecordStatus(
                record.RecordId,
                "active",
                null,
                DateTimeOffset.UtcNow.AddDays(-400),
                null);

            var run = store.RunRetentionDispositionScheduler(DefaultTenantId, "person-record-admin");
            schedulerRunId = run.SchedulerRunId;
            leaseId = run.LeaseId;
            outboxMessageId = Assert.Single(run.OutboxMessageRefs);

            var deliveryRun = store.ProcessRetentionSchedulerOutbox(DefaultTenantId, "person-record-admin");
            Assert.Contains(outboxMessageId, deliveryRun.DeliveredMessageRefs);
        }

        using (var db = new RecordArrDbContext(options))
        {
            var recreated = new RecordArrStore(db);
            var run = Assert.Single(recreated.GetRetentionSchedulerRuns(DefaultTenantId), item => item.SchedulerRunId == schedulerRunId);
            var lease = Assert.Single(recreated.GetRetentionSchedulerLeases(DefaultTenantId), item => item.LeaseId == leaseId);
            var message = Assert.Single(recreated.GetRetentionSchedulerOutboxMessages(DefaultTenantId), item => item.OutboxMessageId == outboxMessageId);

            Assert.Equal("completed", run.Status);
            Assert.Equal("create_pending_reviews_only", run.ExecutionPolicy);
            Assert.Equal(1, run.CreatedReviewCount);
            Assert.Equal(1, run.NotificationMessageCount);
            Assert.Equal("released", lease.Status);
            Assert.Equal(schedulerRunId, lease.SchedulerRunId);
            Assert.Equal("recordarr.retention.disposal_review.created", message.MessageType);
            Assert.Equal("delivered", message.Status);
            Assert.Equal(1, message.DeliveryAttemptCount);
            Assert.NotNull(message.LastAttemptAt);
            Assert.NotNull(message.DeliveredAt);
            Assert.Equal("person-record-admin", message.DeliveredByPersonId);
            Assert.Equal(recordId, message.TargetRecordId);
            Assert.False(string.IsNullOrWhiteSpace(message.DeduplicationKey));
            Assert.DoesNotContain(
                recreated.GetRetentionSchedulerRuns(Guid.NewGuid().ToString()),
                item => item.SchedulerRunId == schedulerRunId);
            Assert.DoesNotContain(
                recreated.GetRetentionSchedulerLeases(Guid.NewGuid().ToString()),
                item => item.LeaseId == leaseId);
            Assert.DoesNotContain(
                recreated.GetRetentionSchedulerOutboxMessages(Guid.NewGuid().ToString()),
                item => item.OutboxMessageId == outboxMessageId);
        }
    }

    [Fact]
    public void Retention_scheduler_outbox_tracks_external_delivery_retry_and_escalation()
    {
        var store = CreateStore();
        var firstRecord = store.CreateRecord(
            DefaultTenantId,
            "External notification retention record",
            "Record eligible for external notification delivery verification.",
            "document",
            "other",
            "retention_test",
            "expired",
            "internal",
            "recordarr",
            "record",
            "rec-retention-external-001",
            "External notification record",
            "person-record-admin",
            "person-record-admin",
            "retention-external-001.pdf",
            "application/pdf",
            "recordarr",
            "tenant/retention-external-001.pdf",
            8192);
        var secondRecord = store.CreateRecord(
            DefaultTenantId,
            "Escalated notification retention record",
            "Record eligible for escalation verification.",
            "document",
            "other",
            "retention_test",
            "expired",
            "internal",
            "recordarr",
            "record",
            "rec-retention-external-002",
            "Escalated notification record",
            "person-record-admin",
            "person-record-admin",
            "retention-external-002.pdf",
            "application/pdf",
            "recordarr",
            "tenant/retention-external-002.pdf",
            8192);
        store.UpdateRecordStatus(firstRecord.RecordId, "active", null, DateTimeOffset.UtcNow.AddDays(-400), null);
        store.UpdateRecordStatus(secondRecord.RecordId, "active", null, DateTimeOffset.UtcNow.AddDays(-400), null);

        var run = store.RunRetentionDispositionScheduler(DefaultTenantId, "person-record-admin");

        Assert.Equal(2, run.CreatedReviewCount);
        Assert.Equal(2, run.NotificationMessageCount);
        var initialMessages = store.GetRetentionSchedulerOutboxMessages(DefaultTenantId)
            .Where(message => run.OutboxMessageRefs.Contains(message.OutboxMessageId))
            .ToArray();
        Assert.All(initialMessages, message =>
        {
            Assert.Equal("pending", message.Status);
            Assert.Equal("in_app", message.DeliveryChannel);
            Assert.Equal("role:records", message.RecipientRef);
            Assert.NotNull(message.DueAt);
            Assert.NotNull(message.EscalateAfter);
        });

        var failedDelivery = store.ProcessRetentionSchedulerOutbox(
            DefaultTenantId,
            "person-record-admin",
            deliveryChannel: "email",
            externalProviderRef: null,
            maxMessages: 100);

        Assert.Equal(2, failedDelivery.PendingBeforeCount);
        Assert.Equal(0, failedDelivery.DeliveredCount);
        Assert.Equal(2, failedDelivery.FailedCount);
        var failedMessages = store.GetRetentionSchedulerOutboxMessages(DefaultTenantId)
            .Where(message => run.OutboxMessageRefs.Contains(message.OutboxMessageId))
            .OrderBy(message => message.CreatedAt)
            .ToArray();
        Assert.All(failedMessages, message =>
        {
            Assert.Equal("failed", message.Status);
            Assert.Equal("email", message.DeliveryChannel);
            Assert.Equal(1, message.DeliveryAttemptCount);
            Assert.Contains("requires a configured provider", message.ErrorMessage);
            Assert.Null(message.DeliveredAt);
        });

        var retryDelivery = store.ProcessRetentionSchedulerOutbox(
            DefaultTenantId,
            "person-record-admin",
            deliveryChannel: "email",
            externalProviderRef: "provider:retention-email",
            maxMessages: 1);

        Assert.Equal(1, retryDelivery.DeliveredCount);
        Assert.Equal(0, retryDelivery.FailedCount);
        var retriedMessageId = Assert.Single(retryDelivery.DeliveredMessageRefs);
        var retriedMessage = Assert.Single(store.GetRetentionSchedulerOutboxMessages(DefaultTenantId), message => message.OutboxMessageId == retriedMessageId);
        Assert.Equal("delivered", retriedMessage.Status);
        Assert.Equal(2, retriedMessage.DeliveryAttemptCount);
        Assert.Equal("provider:retention-email", retriedMessage.ExternalProviderRef);
        Assert.NotNull(retriedMessage.DeliveredAt);

        var escalationRun = store.EscalateRetentionSchedulerOutbox(
            DefaultTenantId,
            "person-record-admin",
            "role:records-supervisor");

        Assert.Equal(1, escalationRun.EligibleBeforeCount);
        Assert.Equal(1, escalationRun.EscalatedCount);
        var escalatedMessageId = Assert.Single(escalationRun.EscalatedMessageRefs);
        var escalatedMessage = Assert.Single(store.GetRetentionSchedulerOutboxMessages(DefaultTenantId), message => message.OutboxMessageId == escalatedMessageId);
        Assert.Equal("escalated", escalatedMessage.Status);
        Assert.Equal(1, escalatedMessage.EscalationLevel);
        Assert.Equal("role:records-supervisor", escalatedMessage.RecipientRef);
        Assert.Equal("role:records-supervisor", escalatedMessage.EscalatedToRecipientRef);
        Assert.NotNull(escalatedMessage.EscalatedAt);

        var accessLogs = store.GetAccessLogs(DefaultTenantId, null);
        Assert.Contains(accessLogs, log => log.Action == "retention.scheduler.outbox_delivery_failed" && log.Result == "denied");
        Assert.Contains(accessLogs, log => log.Action == "retention.scheduler.outbox_delivered" && log.Result == "allowed");
        Assert.Contains(accessLogs, log => log.Action == "retention.scheduler.outbox_escalated" && log.Result == "allowed");
    }

    [Fact]
    public void Retention_disposition_scheduler_skips_records_blocked_by_legal_hold()
    {
        var store = CreateStore();
        var record = store.CreateRecord(
            DefaultTenantId,
            "Held retention record",
            "Record eligible for scheduled disposition but under hold.",
            "document",
            "other",
            "retention_test",
            "held",
            "internal",
            "recordarr",
            "record",
            "rec-retention-held-001",
            "Held retention record",
            "person-record-admin",
            "person-record-admin",
            "held-retention.pdf",
            "application/pdf",
            "recordarr",
            "tenant/held-retention.pdf",
            8192);
        store.UpdateRecordStatus(
            record.RecordId,
            "active",
            null,
            DateTimeOffset.UtcNow.AddDays(-400),
            null);
        var hold = store.CreateLegalHold(
            DefaultTenantId,
            "Retention scheduler hold",
            "Preserve this record while scheduler runs.",
            "legal",
            "recordarr",
            "record",
            record.RecordId,
            "person-record-admin",
            [],
            [record.RecordId]);
        store.ActivateLegalHold(DefaultTenantId, hold.LegalHoldId);

        var run = store.RunRetentionDispositionScheduler(DefaultTenantId, "person-record-admin");

        Assert.Equal(0, run.CreatedReviewCount);
        Assert.Contains(record.RecordId, run.BlockedRecordRefs);
        Assert.DoesNotContain(store.GetDisposalReviews(DefaultTenantId), review => review.RecordId == record.RecordId);
        Assert.Equal("blocked_by_legal_hold", store.GetRetentionStatus(DefaultTenantId, record.RecordId)?.Status);
    }

    [Fact]
    public void Active_legal_hold_blocks_non_disposition_record_mutations_with_denial_evidence()
    {
        var store = CreateStore();
        var record = store.CreateRecord(
            DefaultTenantId,
            "Held evidence record",
            "Record that must stay preserved while under hold.",
            "document",
            "other",
            "legal",
            "hold",
            "internal",
            "recordarr",
            "record",
            "rec-held-mutation-001",
            "Held evidence record",
            "person-record-admin",
            "person-record-admin",
            "held-evidence.pdf",
            "application/pdf",
            "recordarr",
            "tenant/held-evidence.pdf",
            8192);
        var hold = store.CreateLegalHold(
            DefaultTenantId,
            "Mutation blocking hold",
            "Preserve the record and prevent mutation while legal hold is active.",
            "legal",
            "recordarr",
            "record",
            record.RecordId,
            "person-record-admin",
            [],
            [record.RecordId]);
        store.ActivateLegalHold(DefaultTenantId, hold.LegalHoldId);

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateFile(record.RecordId, "replacement.pdf", "application/pdf", "person-record-admin"));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateRecordMetadata(record.RecordId, "matter", "M-100", "text", "manual", 1m, "person-record-admin"));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateRecordLink(record.RecordId, null, "maintainarr:work_order:wo-held", "related", "person-record-admin"));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateRecordComment(record.RecordId, "Hold note", "internal", "person-record-admin"));
        Assert.Throws<InvalidOperationException>(() =>
            store.UpdateRecordStatus(record.RecordId, "review"));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateSignatureRecord(
                tenantId: DefaultTenantId,
                recordId: record.RecordId,
                signaturePurpose: "policy_acknowledgement",
                signerPersonId: "person-record-admin",
                signerExternalName: null,
                signerTitle: "Records admin",
                attestationText: "I acknowledge.",
                capturedByPersonId: "person-record-admin",
                sourceProduct: "recordarr",
                sourceObjectRef: $"record:{record.RecordId}"));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreatePhotoEvidence(
                tenantId: DefaultTenantId,
                recordId: record.RecordId,
                photoPurpose: "audit",
                capturedByPersonId: "person-record-admin",
                sourceProduct: "recordarr",
                sourceObjectRef: $"record:{record.RecordId}"));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateRedaction(
                DefaultTenantId,
                record.RecordId,
                "rec-redacted-held",
                "legal",
                "person-record-admin",
                ["mask:all"]));

        Assert.Empty(store.GetRecordMetadata(record.RecordId));
        Assert.DoesNotContain(store.GetRecordLinks(record.RecordId), link => link.LinkType == "related");
        Assert.Empty(store.GetRecordComments(record.RecordId));
        Assert.Empty(store.GetSignatureRecords(DefaultTenantId, record.RecordId));
        Assert.Empty(store.GetPhotoEvidence(DefaultTenantId, record.RecordId));
        Assert.DoesNotContain(store.GetRedactions(DefaultTenantId), redaction => redaction.SourceRecordId == record.RecordId);

        var deniedActions = store.GetAccessLogs(DefaultTenantId, record.RecordId)
            .Where(log => log.Result == "denied" && log.ReasonCode == "blocked_by_legal_hold")
            .Select(log => log.Action)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("file.upload", deniedActions);
        Assert.Contains("metadata.create", deniedActions);
        Assert.Contains("link.create", deniedActions);
        Assert.Contains("comment.create", deniedActions);
        Assert.Contains("record.status.update", deniedActions);
        Assert.Contains("signature.captured", deniedActions);
        Assert.Contains("photo_evidence.captured", deniedActions);
        Assert.Contains("redaction.completed", deniedActions);
    }

    [Fact]
    public void Active_legal_hold_blocks_package_access_and_share_mutations_with_denial_evidence()
    {
        var store = CreateStore();
        var package = store.CreatePackage(
            DefaultTenantId,
            "Pre-hold evidence package",
            "audit",
            "routarr",
            "routarr:trip:trip-7781",
            "rec-bol-001",
            "person-evidence-manager");
        var policy = store.CreateAccessPolicy(
            DefaultTenantId,
            "rec-bol-001",
            "restricted",
            "active",
            ["role:records"],
            ["person-record-admin"],
            ["role:records"],
            ["person-record-admin"],
            ["role:records"],
            ["person-record-admin"],
            "person-record-admin");
        var grant = store.CreateAccessGrant(
            DefaultTenantId,
            "rec-bol-001",
            "person",
            "person-auditor",
            "read",
            "person-record-admin",
            DateTimeOffset.UtcNow.AddDays(3));
        var share = store.CreateExternalShare(
            DefaultTenantId,
            "rec-bol-001",
            "Auditor",
            "auditor@example.test",
            "auditor_access",
            ["view", "download"],
            "person-record-admin");
        var hold = store.CreateLegalHold(
            DefaultTenantId,
            "Access and package hold",
            "Preserve package/access/share state while legal hold is active.",
            "legal",
            "recordarr",
            "record",
            "rec-bol-001",
            "person-record-admin",
            [],
            ["rec-bol-001"]);
        store.ActivateLegalHold(DefaultTenantId, hold.LegalHoldId);

        Assert.Throws<InvalidOperationException>(() =>
            store.CreatePackage(
                DefaultTenantId,
                "Blocked package",
                "audit",
                "routarr",
                "routarr:trip:trip-7781",
                "rec-bol-001",
                "person-evidence-manager"));
        Assert.Throws<InvalidOperationException>(() => store.LockPackage(DefaultTenantId, package.PackageId));
        Assert.Throws<InvalidOperationException>(() => store.ArchivePackage(DefaultTenantId, package.PackageId));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateAccessPolicy(
                DefaultTenantId,
                "rec-bol-001",
                "restricted",
                "active",
                ["role:records"],
                ["person-record-admin"],
                ["role:records"],
                ["person-record-admin"],
                ["role:records"],
                ["person-record-admin"],
                "person-record-admin"));
        Assert.Throws<InvalidOperationException>(() =>
            store.UpdateAccessPolicy(
                DefaultTenantId,
                policy.AccessPolicyId,
                "rec-bol-001",
                "restricted",
                "inactive",
                ["role:records"],
                ["person-record-admin"],
                ["role:records"],
                ["person-record-admin"],
                ["role:records"],
                ["person-record-admin"],
                "person-record-admin"));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateAccessGrant(
                DefaultTenantId,
                "rec-bol-001",
                "person",
                "person-second-auditor",
                "read",
                "person-record-admin",
                DateTimeOffset.UtcNow.AddDays(3)));
        Assert.Throws<InvalidOperationException>(() =>
            store.RevokeAccessGrant(DefaultTenantId, grant.AccessGrantId, "person-record-admin", "Hold review."));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateExternalShare(
                DefaultTenantId,
                "rec-bol-001",
                "Second Auditor",
                "second-auditor@example.test",
                "auditor_access",
                ["view"],
                "person-record-admin"));
        Assert.Throws<InvalidOperationException>(() =>
            store.RevokeExternalShare(DefaultTenantId, share.ExternalShareId, "person-record-admin"));
        Assert.Throws<InvalidOperationException>(() =>
            store.ExpireExternalShare(DefaultTenantId, share.ExternalShareId, "person-record-admin"));

        Assert.Equal("complete", store.GetPackage(DefaultTenantId, package.PackageId)?.Status);
        Assert.Equal("active", store.GetAccessPolicies(DefaultTenantId).Single(item => item.AccessPolicyId == policy.AccessPolicyId).Status);
        Assert.Equal("active", store.GetAccessGrants(DefaultTenantId).Single(item => item.AccessGrantId == grant.AccessGrantId).Status);
        Assert.Equal("created", store.GetExternalShares(DefaultTenantId).Single(item => item.ExternalShareId == share.ExternalShareId).Status);

        var deniedActions = store.GetAccessLogs(DefaultTenantId, "rec-bol-001")
            .Where(log => log.Result == "denied" && log.ReasonCode == "blocked_by_legal_hold")
            .Select(log => log.Action)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("package.created", deniedActions);
        Assert.Contains("package.locked", deniedActions);
        Assert.Contains("package.archived", deniedActions);
        Assert.Contains("access_policy.created", deniedActions);
        Assert.Contains("access_policy.updated", deniedActions);
        Assert.Contains("access_grant.created", deniedActions);
        Assert.Contains("access_grant.revoked", deniedActions);
        Assert.Contains("external_share.created", deniedActions);
        Assert.Contains("external_share.revoked", deniedActions);
        Assert.Contains("external_share.expired", deniedActions);
    }

    [Fact]
    public void Active_legal_hold_preserves_access_expiry_refresh_and_replay_state()
    {
        var dbName = $"recordarr-legal-hold-passive-refresh-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string grantId;
        string shareId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var grant = store.CreateAccessGrant(
                DefaultTenantId,
                "rec-bol-001",
                "person",
                "person-auditor",
                "read",
                "person-record-admin",
                DateTimeOffset.UtcNow.AddMinutes(-5));
            var share = store.CreateExternalShare(
                DefaultTenantId,
                "rec-bol-001",
                "Auditor",
                "auditor@example.test",
                "auditor_access",
                ["view"],
                "person-record-admin");
            var hold = store.CreateLegalHold(
                DefaultTenantId,
                "Passive access hold",
                "Preserve passive expiry and replay state while legal hold is active.",
                "legal",
                "recordarr",
                "record",
                "rec-bol-001",
                "person-record-admin",
                [],
                ["rec-bol-001"]);
            store.ActivateLegalHold(DefaultTenantId, hold.LegalHoldId);
            grantId = grant.AccessGrantId;
            shareId = share.ExternalShareId;
        }

        using (var db = new RecordArrDbContext(options))
        {
            var row = db.RecordArrExternalShares.Single(share => share.ExternalShareId == shareId);
            var share = JsonSerializer.Deserialize<RecordArrExternalShareResponse>(row.PayloadJson, JsonOptions)!;
            var expiredShare = share with { ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5) };
            row.ExpiresAt = expiredShare.ExpiresAt;
            row.PayloadJson = JsonSerializer.Serialize(expiredShare, JsonOptions);
            db.SaveChanges();
        }

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);

            store.RefreshAccessGrants(DefaultTenantId);
            store.RefreshExternalShares(DefaultTenantId);
            var error = Assert.Throws<InvalidOperationException>(() =>
                store.RecordExternalShareAccess(DefaultTenantId, shareId, "external-auditor", "view", "127.0.0.1", "test-agent"));

            Assert.Contains("blocked by legal hold", error.Message);
            Assert.Equal("active", store.GetAccessGrants(DefaultTenantId).Single(grant => grant.AccessGrantId == grantId).Status);
            Assert.Equal("created", store.GetExternalShares(DefaultTenantId).Single(share => share.ExternalShareId == shareId).Status);

            var deniedActions = store.GetAccessLogs(DefaultTenantId, "rec-bol-001")
                .Where(log => log.Result == "denied" && log.ReasonCode == "blocked_by_legal_hold")
                .Select(log => log.Action)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.Contains("access_grant.expired", deniedActions);
            Assert.Contains("external_share.expired", deniedActions);
            Assert.Contains("external_share.accessed", deniedActions);
        }
    }

    [Fact]
    public void Active_legal_hold_preserves_controlled_document_refresh_state()
    {
        var dbName = $"recordarr-legal-hold-controlled-refresh-{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        string controlledDocumentId;
        string acknowledgementId;

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);
            var document = store.CreateControlledDocument(
                DefaultTenantId,
                "Held refresh controlled document",
                "Document used to prove refresh does not mutate held records.",
                "procedure",
                "operations",
                "refresh-hold",
                "person-doc-controller",
                "org-receiving",
                "site-north-yard",
                true);
            store.UpdateControlledDocumentStatus(DefaultTenantId, document.ControlledDocumentId, "effective", "person-doc-controller");
            var acknowledgement = store.CreateDocumentAcknowledgement(
                DefaultTenantId,
                document.ControlledDocumentId,
                document.CurrentVersionId,
                "person-reviewer",
                "I acknowledge.",
                DateTimeOffset.UtcNow.AddDays(3));
            var hold = store.CreateLegalHold(
                DefaultTenantId,
                "Controlled refresh hold",
                "Preserve controlled document refresh state while legal hold is active.",
                "legal",
                "recordarr",
                "record",
                "rec-bol-001",
                "person-record-admin",
                [],
                ["rec-bol-001"]);
            store.ActivateLegalHold(DefaultTenantId, hold.LegalHoldId);
            controlledDocumentId = document.ControlledDocumentId;
            acknowledgementId = acknowledgement.AcknowledgementId;
        }

        using (var db = new RecordArrDbContext(options))
        {
            var documentRow = db.RecordArrControlledDocuments.Single(document => document.ControlledDocumentId == controlledDocumentId);
            var document = JsonSerializer.Deserialize<RecordArrControlledDocumentResponse>(documentRow.PayloadJson, JsonOptions)!;
            var dueDocument = document with { NextReviewAt = DateTimeOffset.UtcNow.AddMinutes(-5) };
            documentRow.NextReviewAt = dueDocument.NextReviewAt;
            documentRow.PayloadJson = JsonSerializer.Serialize(dueDocument, JsonOptions);

            var acknowledgementRow = db.RecordArrDocumentAcknowledgements.Single(acknowledgement => acknowledgement.AcknowledgementId == acknowledgementId);
            var acknowledgement = JsonSerializer.Deserialize<RecordArrDocumentAcknowledgementResponse>(acknowledgementRow.PayloadJson, JsonOptions)!;
            var dueAcknowledgement = acknowledgement with { DueAt = DateTimeOffset.UtcNow.AddMinutes(-5) };
            acknowledgementRow.DueAt = dueAcknowledgement.DueAt;
            acknowledgementRow.PayloadJson = JsonSerializer.Serialize(dueAcknowledgement, JsonOptions);

            db.SaveChanges();
        }

        using (var db = new RecordArrDbContext(options))
        {
            var store = new RecordArrStore(db);

            store.RefreshControlledDocumentWorkflows(DefaultTenantId);

            Assert.Equal("effective", store.GetControlledDocument(DefaultTenantId, controlledDocumentId)?.Status);
            Assert.Equal(
                "pending",
                store.GetDocumentAcknowledgements(DefaultTenantId, controlledDocumentId)
                    .Single(acknowledgement => acknowledgement.AcknowledgementId == acknowledgementId)
                    .Status);

            var deniedActions = store.GetAccessLogs(DefaultTenantId, "rec-bol-001")
                .Where(log => log.Result == "denied" && log.ReasonCode == "blocked_by_legal_hold")
                .Select(log => log.Action)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.Contains("controlled_document.periodic_review_due", deniedActions);
            Assert.Contains("document_acknowledgement.overdue", deniedActions);
        }
    }

    [Fact]
    public void Active_legal_hold_blocks_controlled_document_mutations_with_denial_evidence()
    {
        var store = CreateStore();
        var version = store.CreateDocumentVersion(
            DefaultTenantId,
            "doc-001",
            "pre-hold-controlled-document.pdf",
            "person-doc-controller",
            "Pre-hold draft.");
        var review = store.RequestDocumentReview(
            DefaultTenantId,
            "doc-001",
            version.VersionId,
            "approval",
            "person-doc-controller",
            "person-reviewer",
            DateTimeOffset.UtcNow.AddDays(5));
        var distribution = store.CreateDocumentDistribution(
            DefaultTenantId,
            "doc-001",
            version.VersionId,
            "person",
            "person-reviewer");
        var acknowledgement = store.CreateDocumentAcknowledgement(
            DefaultTenantId,
            "doc-001",
            version.VersionId,
            "person-reviewer",
            "I acknowledge.",
            DateTimeOffset.UtcNow.AddDays(5));
        var replacement = store.CreateControlledDocument(
            DefaultTenantId,
            "Replacement controlled document",
            "Pre-hold replacement used to test supersession blocking.",
            "procedure",
            "operations",
            "replacement",
            "person-doc-controller",
            "org-receiving",
            "site-north-yard",
            true);
        var hold = store.CreateLegalHold(
            DefaultTenantId,
            "Controlled document hold",
            "Preserve controlled document workflow state while legal hold is active.",
            "legal",
            "recordarr",
            "record",
            "rec-bol-001",
            "person-record-admin",
            [],
            ["rec-bol-001"]);
        store.ActivateLegalHold(DefaultTenantId, hold.LegalHoldId);

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateControlledDocument(
                DefaultTenantId,
                "Blocked controlled document",
                "Should not be created while backing record is held.",
                "procedure",
                "operations",
                "blocked",
                "person-doc-controller",
                "org-receiving",
                "site-north-yard",
                true));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentVersion(DefaultTenantId, "doc-001", "blocked-version.pdf", "person-doc-controller", "Blocked."));
        Assert.Throws<InvalidOperationException>(() =>
            store.PromoteDocumentVersion(DefaultTenantId, "doc-001", version.VersionId, "person-doc-controller", DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() =>
            store.UpdateControlledDocumentStatus(DefaultTenantId, "doc-001", "archived", "person-doc-controller"));
        Assert.Throws<InvalidOperationException>(() =>
            store.SupersedeControlledDocument(DefaultTenantId, "doc-001", replacement.ControlledDocumentId, "person-doc-controller"));
        Assert.Throws<InvalidOperationException>(() =>
            store.RequestDocumentReview(DefaultTenantId, "doc-001", version.VersionId, "approval", "person-doc-controller", "person-reviewer", DateTimeOffset.UtcNow.AddDays(7)));
        Assert.Throws<InvalidOperationException>(() =>
            store.CompleteDocumentReview(DefaultTenantId, review.DocumentReviewId, "approved", "Approved.", "Looks good."));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentDistribution(DefaultTenantId, "doc-001", version.VersionId, "person", "person-second-reviewer"));
        Assert.Throws<InvalidOperationException>(() =>
            store.RevokeDocumentDistribution(DefaultTenantId, distribution.DistributionId, "person-doc-controller", "Hold review."));
        Assert.Throws<InvalidOperationException>(() =>
            store.ExpireDocumentDistribution(DefaultTenantId, distribution.DistributionId, "person-doc-controller", "Hold review."));
        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentAcknowledgement(DefaultTenantId, "doc-001", version.VersionId, "person-second-reviewer", "I acknowledge.", DateTimeOffset.UtcNow.AddDays(5)));
        Assert.Throws<InvalidOperationException>(() =>
            store.CompleteDocumentAcknowledgement(DefaultTenantId, acknowledgement.AcknowledgementId, null));

        Assert.Equal("review", store.GetControlledDocument(DefaultTenantId, "doc-001")?.Status);
        Assert.Equal("pending", store.GetDocumentReviews(DefaultTenantId, "doc-001").Single(item => item.DocumentReviewId == review.DocumentReviewId).Status);
        Assert.Equal("distributed", store.GetDocumentDistributions(DefaultTenantId, "doc-001").Single(item => item.DistributionId == distribution.DistributionId).Status);
        Assert.Equal("pending", store.GetDocumentAcknowledgements(DefaultTenantId, "doc-001").Single(item => item.AcknowledgementId == acknowledgement.AcknowledgementId).Status);

        var deniedActions = store.GetAccessLogs(DefaultTenantId, "rec-bol-001")
            .Where(log => log.Result == "denied" && log.ReasonCode == "blocked_by_legal_hold")
            .Select(log => log.Action)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.Contains("controlled_document.created", deniedActions);
        Assert.Contains("controlled_document.version_created", deniedActions);
        Assert.Contains("controlled_document.version_promoted", deniedActions);
        Assert.Contains("controlled_document.status_updated", deniedActions);
        Assert.Contains("controlled_document.superseded", deniedActions);
        Assert.Contains("document_review.requested", deniedActions);
        Assert.Contains("document_review.completed", deniedActions);
        Assert.Contains("document_distribution.created", deniedActions);
        Assert.Contains("document_distribution.revoked", deniedActions);
        Assert.Contains("document_distribution.expired", deniedActions);
        Assert.Contains("document_acknowledgement.created", deniedActions);
        Assert.Contains("document_acknowledgement.completed", deniedActions);
    }

    [Fact]
    public void Document_distribution_and_acknowledgement_invalid_inputs_are_rejected()
    {
        var store = CreateStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentDistribution(
                DefaultTenantId,
                "doc-001",
                "ver-002",
                "person",
                " "));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentAcknowledgement(
                DefaultTenantId,
                "doc-001",
                "ver-002",
                " ",
                "Attest.",
                DateTimeOffset.UtcNow.AddDays(1)));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentAcknowledgement(
                DefaultTenantId,
                "doc-001",
                "ver-002",
                "person-doc-controller",
                "Attest.",
                DateTimeOffset.UtcNow.AddMinutes(-5)));
    }

    [Fact]
    public void Controlled_document_version_inputs_are_rejected_when_blank()
    {
        var store = CreateStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentVersion(
                DefaultTenantId,
                "doc-001",
                " ",
                "person-doc-controller",
                "Change summary"));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateDocumentVersion(
                DefaultTenantId,
                "doc-001",
                "version.pdf",
                " ",
                "Change summary"));

        Assert.Throws<InvalidOperationException>(() =>
            store.PromoteDocumentVersion(
                DefaultTenantId,
                "doc-001",
                "ver-002",
                " ",
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Controlled_document_review_flow_updates_version_statuses()
    {
        var store = CreateStore();

        var document = store.CreateControlledDocument(
            DefaultTenantId,
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
            DefaultTenantId,
            document.ControlledDocumentId,
            "review-flow.pdf",
            "person-doc-controller",
            "Initial draft.");

        var review = store.RequestDocumentReview(
            DefaultTenantId,
            document.ControlledDocumentId,
            version.VersionId,
            "approval",
            "person-doc-controller",
            "person-reviewer",
            DateTimeOffset.UtcNow.AddDays(3));

        var duringReviewVersion = store.GetDocumentVersions(DefaultTenantId, document.ControlledDocumentId).Single(item => item.VersionId == version.VersionId);
        Assert.Equal("review", duringReviewVersion.Status);

        store.CompleteDocumentReview(DefaultTenantId, review.DocumentReviewId, "approved", "Looks good.", "Approved.");

        var approvedVersion = store.GetDocumentVersions(DefaultTenantId, document.ControlledDocumentId).Single(item => item.VersionId == version.VersionId);
        Assert.Equal("approved", approvedVersion.Status);

        var promoted = store.PromoteDocumentVersion(DefaultTenantId, document.ControlledDocumentId, version.VersionId, "person-doc-controller", DateTimeOffset.UtcNow);
        Assert.Equal("effective", promoted.Status);

        var effectiveVersion = store.GetDocumentVersions(DefaultTenantId, document.ControlledDocumentId).Single(item => item.VersionId == version.VersionId);
        Assert.Equal("effective", effectiveVersion.Status);
    }

    [Fact]
    public void Controlled_document_archiving_archives_versions()
    {
        var store = CreateStore();

        var document = store.CreateControlledDocument(
            DefaultTenantId,
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
            DefaultTenantId,
            document.ControlledDocumentId,
            "archive-flow.pdf",
            "person-doc-controller",
            "Initial draft.");

        store.UpdateControlledDocumentStatus(DefaultTenantId, document.ControlledDocumentId, "archived", "person-doc-controller");

        var archivedVersion = store.GetDocumentVersions(DefaultTenantId, document.ControlledDocumentId).Single(item => item.VersionId == version.VersionId);
        Assert.Equal("archived", archivedVersion.Status);
    }

    [Fact]
    public void Revoked_document_distributions_cannot_be_mutated_again()
    {
        var store = CreateStore();

        var distribution = store.CreateDocumentDistribution(
            DefaultTenantId,
            "doc-001",
            "ver-002",
            "person",
            "person-doc-controller");

        store.RevokeDocumentDistribution(DefaultTenantId, distribution.DistributionId, "person-doc-controller", "No longer needed.");

        Assert.Throws<InvalidOperationException>(() =>
            store.ExpireDocumentDistribution(
                DefaultTenantId,
                distribution.DistributionId,
                "person-doc-controller",
                "Still no longer needed."));
    }

    [Fact]
    public void Invalid_package_and_controlled_document_types_are_rejected()
    {
        var store = CreateStore();

        Assert.Throws<InvalidOperationException>(() =>
            store.CreatePackage(
                DefaultTenantId,
                "Bad package",
                "not-a-package-type",
                "routarr",
                "routarr:trip:trip-7781",
                "rec-bol-001",
                "person-evidence-manager"));

        Assert.Throws<InvalidOperationException>(() =>
            store.CreateControlledDocument(
                DefaultTenantId,
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
        var store = CreateStore();

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
                DefaultTenantId,
                mapping.EvidenceMappingId,
                "not-a-mapping-status",
                "person-evidence-manager",
                null,
                null));
    }

    [Fact]
    public void Invalid_record_status_values_are_rejected()
    {
        var store = CreateStore();

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

    private static RecordArrStore CreateStore()
    {
        var options = new DbContextOptionsBuilder<RecordArrDbContext>()
            .UseInMemoryDatabase($"recordarr-openapi-tests-{Guid.NewGuid():N}")
            .Options;

        return new RecordArrStore(new RecordArrDbContext(options));
    }

    private static ServiceProvider CreateStoreProvider(string databaseName)
    {
        var services = new ServiceCollection();
        services.AddDbContext<RecordArrDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddScoped<RecordArrStore>();
        return services.BuildServiceProvider();
    }

    private sealed class StaticOptionsMonitor<TOptions>(TOptions value) : IOptionsMonitor<TOptions>
    {
        public TOptions CurrentValue => value;

        public TOptions Get(string? name) => value;

        public IDisposable? OnChange(Action<TOptions, string?> listener) => null;
    }
}


