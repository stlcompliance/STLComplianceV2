# RecordArr — Production Safety, File Security, and Navigation

## Audit mandate

Replace the global singleton fixture store with tenant-scoped durable metadata and controlled object storage. Access-policy absence defaults to deny. Actor identity comes from the principal.

## Durable model

Persist record identity, file/version metadata, upload/quarantine/scan state, OCR/extraction jobs, metadata assertions, links/comments, evidence mappings, packages, controlled-document versions, approvals, access grants, external shares, retention schedules, legal holds, access logs, and purge events.

## Audit governance

RecordArr audit events are tenant-scoped and hash-chained. Audit seals checkpoint a tenant or record audit range and now include the `audit.sealed` event in the sealed range so a fresh seal can truthfully cover the ledger through the sealing action.

Operators must verify audit health through the tenant-scoped workspace or integration routes:

- `GET /audit-integrity`
- `GET /audit-governance`
- `POST /audit-seals`
- `POST /audit-seals/{auditSealId}/verify`
- `POST /audit-seals/{auditSealId}/anchor`

`audit-integrity` verifies event hashes and chain continuity. `audit-governance` re-verifies applicable audit seals, persists broken seal status when tampering is detected, reports verified seal coverage, and flags audit events that are not covered by any verified seal. A tenant or record is not audit-governed while the governance report is `unsealed` or `broken`.

`audit-seals/{auditSealId}/anchor` records explicit provider anchor evidence for a verified seal. The route requires provider name, anchor reference, anchor timestamp, and anchored seal hash; missing provider evidence fails truthfully, mismatched hashes persist a failed anchor state, and later sealed-range tampering breaks the persisted anchor state.

The disabled-by-default `AuditAnchorWorker` can poll explicit external anchor manifest files only for configured tenant IDs. It must be configured with:

- `AuditAnchorWorker:Enabled`
- `AuditAnchorWorker:TenantIds`
- `AuditAnchorWorker:ManifestPath`
- `AuditAnchorWorker:RequestedByPersonId`
- `AuditAnchorWorker:PollIntervalSeconds`

Manifest rows must include the tenant, known unanchored audit seal ID, anchor provider, anchor reference, anchor timestamp, and anchored seal hash:

```json
{
  "manifests": [
    {
      "tenantId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "auditSealId": "aseal-abc123",
      "anchorProviderName": "RecordArr TSA",
      "anchorReference": "tsa-anchor-123",
      "anchoredAt": "2026-06-29T04:15:00Z",
      "anchoredSealHash": "sha256-seal-hash"
    }
  ]
}
```

Missing manifest paths, missing rows, cross-tenant rows, unknown seals, already anchored seals, and missing provider evidence must not create anchor success. Hash mismatches are processed through the same durable anchor path and persist failed anchor evidence rather than provider success.

This is RecordArr-owned tamper-evident audit governance with explicit external anchor evidence capture and a disabled manifest worker for provider-supplied anchors. External WORM storage, notarization provider scheduling, timestamp-authority lifecycle management, and provider-backed immutable audit retention beyond explicit anchor manifests remain future provider/operational controls and must not be represented as complete.

## Upload pipeline

Use streamed or direct-object upload with limits, signature validation, hash, quarantine, scanning, safe delivery, and status UI. OCR/extraction is reviewable proposal data with page provenance.

## Malware scanner worker runbook

New files remain unavailable until malware scan evidence releases them. The hosted malware scanner worker is disabled by default and may be enabled only for explicit tenants through the `MalwareScanWorker` configuration section:

- `Enabled`
- `TenantIds`
- `RequestedByPersonId`
- `ScannerName`
- `ScannerVersion`
- `SignatureVersion`
- `VerdictManifestPath`
- `PollIntervalSeconds`

The worker consumes an external verdict manifest and applies only verdicts that match pending files for the current tenant. Missing manifests, missing verdicts, unknown statuses, or cross-tenant rows must leave files pending or denied rather than creating a clean result.

Expected verdict manifest shape:

```json
{
  "verdicts": [
    {
      "tenantId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "fileId": "file-example",
      "status": "clean",
      "threatName": null,
      "failureReason": null
    }
  ]
}
```

Allowed verdict statuses are the statuses accepted by the RecordArr malware-scan provider path, including clean, skipped, infected, and failed. Operators must verify each enabled tenant has an external scanner producing the manifest atomically, a configured scanner/signature version, and access/audit evidence for released, quarantined, failed, or dead-lettered files. No tenant may treat pending files as downloadable evidence while the worker has no verdict.

## Object-store reconciliation worker runbook

The hosted object-store reconciliation worker is disabled by default and may be enabled only for explicit tenants through the `ObjectStoreReconciliationWorker` configuration section:

- `Enabled`
- `TenantIds`
- `RequestedByPersonId`
- `InventoryManifestPath`
- `PollIntervalSeconds`

The worker consumes an external object-store inventory manifest and records reconciliation only when provider evidence explicitly identifies verified, missing, or corrupt known RecordArr files. It ignores cross-tenant rows and unknown file IDs, and it applies remediation only when the manifest explicitly names restored, accepted-missing, rechecked-corrupt, released-quarantined, or scanned-pending file IDs. Missing manifests or evidence-free manifests must create no clean reconciliation.

Expected inventory manifest shape:

```json
{
  "inventories": [
    {
      "tenantId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "scope": "external_object_store_inventory",
      "recordId": null,
      "verifiedFileIds": ["file-clean"],
      "missingFileIds": ["file-missing"],
      "corruptFileIds": ["file-corrupt"],
      "restoredFileIds": [],
      "acceptedMissingFileIds": [],
      "recheckedCorruptFileIds": [],
      "releasedQuarantinedFileIds": [],
      "scannedPendingFileIds": []
    }
  ]
}
```

Operators must verify each reconciliation through the persisted reconciliation row, fixity/access evidence, issue refs, and remediation status. Tenants with missing/corrupt evidence, pending malware scans, or unresolved remediation remain blocked for production evidence reliance until the owning records administrator resolves or explicitly accepts the issue.

RecordArr now maintains tenant-scoped durable object-store object index rows and fixity observation history for file creation/backfill, explicit integrity checks, storage reconciliation findings, and remediation outcomes. Missing objects record a missing observation without an observed checksum; corrupt objects record failed checksum evidence; restored, rechecked, accepted-missing, released-quarantined, and scanned-pending remediation outcomes update the latest object index and retain the historical observation linked to the reconciliation run.

Operators can inspect and verify object-store lifecycle evidence through:

- `GET /object-store-objects`
- `GET /object-store-fixity-observations`
- `POST /files/{fileId}/object-store-lifecycle-verifications`

Lifecycle verification requires explicit provider name, policy reference, retention mode, retain-until timestamp, encryption key reference, and provider evidence reference. Missing provider evidence fails truthfully. Retain-until dates that do not satisfy the known RecordArr retention/expiration requirement persist a failed lifecycle state instead of claiming immutable storage. Passing lifecycle verification stores provider policy/encryption/evidence hash details on the object index, writes a fixity observation with `object_lifecycle_verification`, and writes access-log evidence.

Lifecycle verification proves that a provider attestation was supplied and checked against RecordArr retention needs. It is not a full object-storage control plane. Provider-side bucket policy creation, WORM/legal-hold enforcement, lifecycle transitions, replication, key rotation, and provider callback reconciliation remain operational/provider controls until explicitly implemented.

## Disaster recovery restore verification

RecordArr disaster-recovery restore runs are tenant-scoped durable evidence records. A run records the recovery point, RPO/RTO targets, recovery-point age, duration, restored/blocked record refs, verified/failed file refs, evidence summary, and truthful failure reason. Restore runs verify file fixity through the same object-store observation trail used by integrity checks and storage reconciliation; they do not claim that a provider backup restored content unless the requested recovery point and file checks pass.

Operators must run restore verification through the tenant-scoped workspace or integration routes:

- `POST /disaster-recovery-runs`
- `GET /disaster-recovery-runs`
- `POST /disaster-recovery-backup-verifications`

Stale recovery points, missing recovery point IDs, cross-tenant or missing record refs, missing file objects, corrupt file checksums, and RTO misses create failed or degraded restore-run evidence instead of success. Tenants with failed restore runs remain blocked for production evidence reliance until records administrators resolve the object-store/provider issue and rerun the verification successfully.

Backup verification runs are also tenant-scoped durable disaster-recovery evidence records. They require explicit backup provider name, backup job reference, backup manifest hash, recovery point ID, and RPO target before RecordArr can claim backup coverage. Missing provider/job/manifest/recovery-point evidence fails truthfully. Passing runs write `backup_verification` evidence with provider/job/manifest details, verified/failed file refs, fixity observations, and access-log evidence through the same workspace and integration route family.

The disabled-by-default `BackupVerificationWorker` can poll explicit provider backup manifest files only for configured tenant IDs. It must be configured with:

- `BackupVerificationWorker:Enabled`
- `BackupVerificationWorker:TenantIds`
- `BackupVerificationWorker:ManifestPath`
- `BackupVerificationWorker:RequestedByPersonId`
- `BackupVerificationWorker:DefaultRpoTargetMinutes`
- `BackupVerificationWorker:PollIntervalSeconds`

Manifest rows must include the tenant, provider name, backup job reference, manifest hash, recovery point, recovery-point creation time, RPO target, and at least one known tenant record ID:

```json
{
  "manifests": [
    {
      "tenantId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "backupProviderName": "backup-vault",
      "backupJobRef": "job-123",
      "backupManifestHash": "sha256-manifest",
      "recoveryPointId": "rp-2026-06-29T04:15Z",
      "recoveryPointCreatedAt": "2026-06-29T04:15:00Z",
      "rpoTargetMinutes": 60,
      "recordIds": ["rec-bol-001"],
      "missingFileIds": [],
      "corruptFileIds": []
    }
  ]
}
```

Missing manifest paths, missing rows, cross-tenant rows, missing provider/job/manifest/recovery-point evidence, unknown record IDs, and unknown file IDs must not create passing backup verification evidence. The worker must never synthesize provider backup coverage; it only runs the same durable backup-verification path when the manifest scopes a known tenant record and supplies explicit provider evidence.

Backup verification proves that a provider-supplied manifest covers known RecordArr records at a recovery point. It is not a full managed backup orchestrator. Provider scheduling, backup job execution, immutable backup retention, storage-tier lifecycle policy enforcement, and external provider reconciliation beyond explicit backup manifests and the configured manifest worker remain operational/provider controls until explicitly implemented.

## Signature and redaction evidence

Signature records are tenant-scoped durable protected-evidence records. Local signature capture is persisted with `VerificationStatus` of `local_capture_only`, a locked signature evidence hash, and `provider_not_configured` as the truthful verification failure reason. A signature may claim `provider_verified` only when the integration request supplies a provider name, provider envelope reference, and certificate fingerprint. Provider envelope evidence is stored with the signature payload and covered by the signature evidence hash; missing provider evidence fails the request instead of silently creating a trusted provider signature.

Provider signature callback reconciliation is explicit and tenant-scoped through:

- `POST /signatures/{signatureRecordId}/provider-reconciliations`

Reconciliation is allowed only for signatures that already carry provider envelope evidence. The callback must repeat the matching provider name and envelope reference, provide a callback status and callback reference, and may include a matching certificate fingerprint, trust timestamp authority reference, and long-term validation status. `completed` callbacks persist provider callback evidence and keep the signature `provider_verified`; `declined`, `failed`, `expired`, and `voided` callbacks persist truthful `provider_rejected` state and denial access-log evidence. Local-only signatures and provider/envelope mismatches fail rather than upgrading trust.

Durable signature trust-service job orchestration is explicit and tenant-scoped through:

- `GET /signature-trust-service-jobs`
- `POST /signatures/{signatureRecordId}/trust-service-jobs`
- `POST /signature-trust-service-jobs/provider-manifests`

Submitting a signature trust-service job requires provider name, provider envelope reference, and an existing provider-backed signature with stored certificate fingerprint and locked signature evidence hash. The persisted job records signature/record refs, provider envelope evidence, certificate fingerprint, actor/time, and submission evidence hash. Repeated submissions for the same provider envelope return the existing job rather than duplicating external work. Provider manifest processing reconciles the job only when the manifest echoes the matching provider name, envelope ref, callback ref/status, and certificate fingerprint. Certificate mismatches persist failed job evidence and denied access-log evidence without updating the signature as provider verified.

The hosted signature trust-service worker is disabled by default and may be enabled only for explicit tenants through the `SignatureTrustServiceWorker` configuration section: `Enabled`, `TenantIds`, `RequestedByPersonId`, `ManifestPath`, and `PollIntervalSeconds`. The worker reads submitted durable trust-service jobs for each configured tenant, consumes only explicit provider manifest rows, and calls the same tenant-scoped manifest reconciliation path used by the workspace and integration APIs.

Signature trust-service manifest files must use this shape:

```json
{
  "manifests": [
    {
      "tenantId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "providerName": "DocuSign",
      "providerEnvelopeRef": "env-123",
      "providerCallbackStatus": "completed",
      "providerCallbackRef": "callback-123",
      "certificateFingerprintSha256": "fingerprint",
      "trustTimestampAuthorityRef": "tsa-123",
      "longTermValidationStatus": "valid"
    }
  ]
}
```

Missing manifest paths, missing rows, cross-tenant rows, unknown provider envelopes, incomplete provider evidence, and certificate mismatches must leave jobs submitted or fail through the existing durable manifest path. The worker must never synthesize provider callbacks, timestamps, or long-term-validation status.

Redactions are tenant-scoped durable protected-evidence records. A redaction request must include at least one explicit redaction rule. The generated redacted copy stores copied access controls, a source-record link, review actor/time, approval reason, locked timestamp, and a deterministic redaction package hash covering the source record, redacted record, reason, actor, rules, and file refs. Empty redaction rules fail truthfully and create no completed redaction evidence.

Provider redaction callback reconciliation is explicit and tenant-scoped through:

- `POST /redactions/{redactionId}/provider-reconciliations`

Reconciliation is allowed only when the provider callback supplies provider name, provider job reference, callback status, callback reference, and the exact locked redaction package hash created by RecordArr. Package hash mismatches fail and write denied access-log evidence. `completed` callbacks persist provider review evidence and mark the redaction `provider_verified`; `rejected`, `failed`, and `needs_review` callbacks persist truthful `provider_rejected` state without claiming provider approval.

Durable redaction provider job orchestration is explicit and tenant-scoped through:

- `GET /redaction-provider-jobs`
- `POST /redactions/{redactionId}/provider-jobs`
- `POST /redaction-provider-jobs/provider-manifests`

Submitting a redaction provider job requires provider name, provider job reference, and an existing generated redaction with locked RecordArr package-hash evidence. The persisted job records source/redacted record refs, redaction rules, package hash, provider job ref, actor/time, and submission evidence hash. Repeated submissions for the same provider job ref return the existing job rather than duplicating external work. Provider manifest processing reconciles the job only when the manifest echoes the matching provider name, job ref, callback ref/status, and locked package hash. Package-hash mismatches persist failed job evidence and denied access-log evidence without updating the redaction as provider verified.

The hosted redaction provider worker is disabled by default and may be enabled only for explicit tenants through the `RedactionProviderWorker` configuration section: `Enabled`, `TenantIds`, `RequestedByPersonId`, `ManifestPath`, and `PollIntervalSeconds`. The worker reads submitted durable redaction provider jobs for each configured tenant, consumes only explicit provider manifest rows, and calls the same tenant-scoped manifest reconciliation path used by the workspace and integration APIs.

Redaction provider manifest files must use this shape:

```json
{
  "manifests": [
    {
      "tenantId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "providerName": "redact-provider",
      "providerJobRef": "job-123",
      "providerCallbackStatus": "completed",
      "providerCallbackRef": "callback-123",
      "redactionPackageHash": "locked-package-hash"
    }
  ]
}
```

Missing manifest paths, missing rows, cross-tenant rows, unknown provider jobs, incomplete provider evidence, and package-hash mismatches must leave jobs submitted or fail through the existing durable manifest path. The worker must never synthesize provider review, callback, approval, or delivery success.

Rendered redaction overlay review is explicit and tenant-scoped through:

- `POST /redactions/{redactionId}/overlay-reviews`

Overlay review is allowed only for generated redactions with a locked RecordArr redaction package hash. The request must include a review status of `approved`, `changes_requested`, or `rejected` and at least one rendered overlay evidence reference. Evidence and issue references are normalized and deduplicated before the review hash is calculated. `approved` reviews persist overlay evidence refs, review actor/time, review hash, and approval reason evidence. `changes_requested` and `rejected` reviews persist truthful failure state, issue refs when supplied, and denial access-log evidence without claiming redaction approval.

The disabled-by-default `RedactionOverlayReviewWorker` can poll explicit rendered-overlay review manifest files only for configured tenant IDs. It must be configured with:

- `RedactionOverlayReviewWorker:Enabled`
- `RedactionOverlayReviewWorker:TenantIds`
- `RedactionOverlayReviewWorker:ManifestPath`
- `RedactionOverlayReviewWorker:RequestedByPersonId`
- `RedactionOverlayReviewWorker:PollIntervalSeconds`

Manifest rows must include the tenant, redaction ID, locked redaction package hash, review status, and at least one rendered overlay evidence ref:

```json
{
  "manifests": [
    {
      "tenantId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "redactionId": "red-abc123",
      "redactionPackageHash": "locked-package-hash",
      "overlayReviewStatus": "approved",
      "overlayEvidenceRefs": ["rendered-page-1-overlay"],
      "overlayIssueRefs": []
    }
  ]
}
```

Missing manifest paths, missing rows, cross-tenant rows, already-reviewed redactions, stale package hashes, and missing rendered-overlay evidence must leave redactions unreviewed. The worker must never synthesize rendered evidence or claim overlay approval without a matching locked package hash and explicit rendered overlay refs.

These controls provide durable RecordArr-owned signature/redaction evidence plus explicit signature trust-service job submission/manifest reconciliation, a disabled-by-default trust-service manifest worker, direct signature callback reconciliation, redaction provider job submission/manifest reconciliation, a disabled-by-default redaction provider manifest worker, direct redaction provider callback reconciliation, rendered redaction overlay review evidence, and a disabled-by-default rendered-overlay review manifest worker. External trust-service webhook ingestion, provider scheduling, provider-managed timestamp/long-term-validation automation beyond explicit trust-service jobs/manifests and the configured manifest worker, external redaction-provider webhook ingestion/provider scheduling/delivery orchestration beyond explicit provider jobs/manifests and the configured manifest worker, rendered-overlay generation/provider scheduling beyond explicit overlay manifests and the configured manifest worker, and broader managed rendered-overlay automation remain future provider-orchestration work and must not be represented as complete.

## Retention and access

Archive, supersession, legal hold, retention, external sharing, and purge are transactional and permissioned. Purge is impossible while any effective hold or prohibition applies. Every access/download is tenant checked and logged.
Record access grants are record-scoped sharing controls. A grant that targets a product is for authenticated product-service workflows, not for suite launch availability or ordinary user launch rights.

## Retention scheduler runbook

The retention disposition scheduler is tenant-specific and must be operated through the tenant-scoped workspace or integration routes:

- `POST /retention-disposition-runs`
- `POST /retention-disposition-outbox/process`
- `POST /retention-disposition-outbox/escalate`

Supported `ExecutionPolicy` values are:

- `create_pending_reviews_only` — recalculate retention statuses, acquire/release a durable tenant scheduler lease, create pending archive/purge disposal reviews for eligible records, skip legal-held records, and create in-app outbox messages for human review.
- `execute_approved_reviews` — execute only already-approved disposal reviews after rechecking tenant scope, active legal holds, and current retention eligibility. Executed reviews are marked completed. Held, missing, or ineligible reviews remain unexecuted and write denied access evidence.

Operators must run `create_pending_reviews_only` before any automatic execution window, resolve review/outbox failures, and confirm that required human approvals exist. `execute_approved_reviews` must be scheduled per tenant, never globally, and must be rerunnable without duplicate side effects. Each run must be verified through the persisted scheduler run, released lease, access-log entries, destruction certificates where purge occurred, retention statuses, and remaining approved reviews. Unsupported policies are expected to fail truthfully, release the lease, and create no archive/purge side effects.

Any tenant that has active legal holds, pending external delivery failures, missing object-store evidence, failed malware scans, or unresolved storage reconciliation issues must stay in pending-review mode until the blocking evidence is resolved or explicitly accepted by the owning records administrator.

## Navigation

Use direct destinations where a group has one child. Core groups: Records, Capture, Controlled Documents, Packages, Retention & Holds, Access & Sharing, Administration. Document navigation supports class → type → subtype filters plus search, saved views, and source/owner context.

## Pages

Primary record detail includes Overview, Files/Versions, Metadata, Evidence, Related Records, Access, Retention/Holds, and History. Raw JSON is advanced-only. StaffArr sites and all foreign references use live owner pickers.
