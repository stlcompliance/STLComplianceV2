# RecordArr Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `recordarr` |
| Category | DMS / evidence vault |
| Entry release | R1 — Foundation spine |
| Completion release | R3 — MaintainArr flagship operational slice |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Document metadata, files, versions, record packets, retention, evidence references, and audit packages. |
| Roadmap slice | Foundation evidence layer |
| Must not violate | Replace any in-memory/file-prototype truth before products rely on evidence persistence. |
| Feature rows retained | 69 |
| Workflow rows retained | 15 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R1 | Foundation spine | 13 | 12 |
| R3 | MaintainArr flagship operational slice | 21 | 2 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 35 | 1 |

## Implementation interpretation

- Current/represented capabilities are hardened in R1 unless they are only supporting another release gate.
- Common category baseline remains retained for R3.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## R0 Trust Gate pass

Status: Product pass completed with deferred R0 blockers. RecordArr is not production-trust-clear until the deferred persistence and tenant-scope blockers below are resolved.

Completed fixes:

- Removed the stale RecordArr session `hasRecordArrAccess` success flag from API contracts, frontend session typing, and current tests.
- Stopped treating legacy launchable-product claims as RecordArr availability truth. RecordArr handoff/session bootstrap now uses a fixed ordinary-suite launch catalog and does not include Compliance Core in normal tenant launch context.
- Kept ordinary tenant users able to reach RecordArr after NexArr validates identity and tenant context; product actions remain server-side permission and record-scope concerns.

Deferred R0 blockers:

- Durable DMS truth remains deferred. `RecordArrStore` is still a process-local singleton with in-memory operational records, files, evidence mappings, packages, retention state, legal holds, access policies, shares, redactions, signatures, photo evidence, and access logs. This violates the RecordArr roadmap requirement to replace prototype truth before other products rely on evidence persistence.
- Several workspace/integration collection surfaces are still shaped around global in-memory collections that are not tenant-owning persistence models. Some responses have tenant fields, but other objects, such as upload sessions, packages, mappings, and retention policies, cannot be made fully tenant-auditable without the durable domain migration.

Deferred justification:

- The deferred items require a RecordArr data model, migrations, object-storage metadata, retention/audit persistence, and endpoint-by-endpoint tenant/action enforcement. A superficial filter over the prototype lists would risk hiding unsafe architecture while still losing work on restart.
- The pass therefore fixed the bounded session/access-truth issue and records the durable evidence-store migration as the blocking R0 work for RecordArr.

Files touched:

- `apps/recordarr-api/RecordArr.Api/Data/RecordArrStore.cs`
- `apps/recordarr-api/RecordArr.Api/Endpoints/AuthEndpoints.cs`
- `apps/recordarr-api/RecordArr.Api/Models/RecordArrContracts.cs`
- `apps/recordarr-api/RecordArr.Api/Services/HandoffAuthService.cs`
- `apps/recordarr-api/RecordArr.Api/Services/RecordArrSuiteLaunchCatalog.cs`
- `apps/recordarr-frontend/src/api/client.ts`
- `apps/recordarr-frontend/src/App.test.tsx`
- `tests/STLCompliance.RecordArr.Auth.Tests/RecordArrAuthEndpointsTests.cs`

Tests run:

- `dotnet test tests/STLCompliance.RecordArr.Auth.Tests/STLCompliance.RecordArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` - passed 3 tests.
- `npm test -- App.test.tsx` from `apps/recordarr-frontend` - passed 1 file / 10 tests.

Remaining blockers: The deferred durable-store and tenant-scope blockers above remain active R0 blockers. RecordArr may be revisited during the R0 suite stage before any R1 work begins if the rollout owner decides deferred blockers must be resolved rather than carried.

R0 stage result: RecordArr product pass is complete for stage-gated rollout bookkeeping, but RecordArr is not clear for production trust until the deferred blockers are closed.

## R1 Foundation spine pass

Status: Product pass completed with deferred R1 blockers. RecordArr remains the R1 foundation evidence-layer product, but it is not clear for production reliance until the durable DMS truth blockers below are resolved.

R1 roadmap scope audited:

- Feature rows `RE-CUR-001` through `RE-CUR-013` cover record/file prototypes, upload sessions, scan/OCR/extraction, metadata/link/comment scaffolds, controlled-document/version concepts, distribution acknowledgement, evidence mapping, packages/manifests, retention/disposal/legal holds, access policies/shares, redaction/signature, photo evidence, and comprehensive DMS navigation.
- Workflow rows `RE-WF-001`, `RE-WF-002`, and `RE-WF-004` through `RE-WF-013` cover the expected foundation record, controlled-document, evidence-package, external-share, redaction/signature, retention, disposition, and legal-hold operating paths.
- All audited R1 rows are explicitly marked current partial/scaffold in the rollout map. This pass did not pull R3 or R12 scope forward.

Deferred R1 blockers:

- The R0 durable-store blocker directly blocks R1 completion for production trust. `RecordArrStore` is still registered as a singleton and keeps records, files, upload sessions, OCR/extraction output, evidence mappings, packages, retention, legal holds, access policies, shares, redactions, signatures, photo evidence, and access logs in process-local lists.
- The R1 evidence foundation cannot be treated as a suite-wide authoritative DMS/evidence vault until durable metadata persistence, approved object-storage metadata, immutable audit, version/hash integrity, retention/legal-hold enforcement, and endpoint-by-endpoint tenant/action checks are implemented.
- Cross-product references in the seed and package/evidence surfaces remain useful for workflow context, but they are not production-grade crosstalk until the RecordArr-owned durable model and tenant-auditable access layer exist.

Deferred justification:

- A narrow R1 patch would either hide the unsafe singleton architecture behind filters or expand into the full durable DMS migration. The roadmap's product-specific rule is to replace prototype truth before other products rely on evidence persistence, so the truthful R1 result is to keep the blocker explicit.
- The durable migration should be handled as a focused RecordArr product-stage effort before MaintainArr, TrainArr, SupplyArr, LoadArr, RoutArr, ReportArr, or Field Companion rely on RecordArr for authoritative evidence persistence.

Files touched:

- `docs/roadmap/products/recordarr.md`

Tests run:

- `dotnet test tests/STLCompliance.RecordArr.Auth.Tests/STLCompliance.RecordArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` - passed 3 tests.
- `npm test -- App.test.tsx` from `apps/recordarr-frontend` - passed 1 file / 10 tests.

Remaining blockers: The R0 durable-store and tenant-scope blockers remain active and are carried as R1 blockers. RecordArr's R1 pass is complete for stage-gated rollout bookkeeping, but RecordArr is not clear for production reliance as the suite evidence vault until those blockers are closed.

R1 stage result: RecordArr product pass is complete with deferred blockers. Continue the R1 suite only with the explicit understanding that downstream products must not depend on RecordArr as authoritative evidence persistence until the durable DMS migration is finished.

## R3 MaintainArr flagship operational slice pass

Status: Product pass completed with deferred R3 blockers. RecordArr is not clear as the production-authoritative DMS/evidence vault for the MaintainArr flagship slice until the durable storage migration closes.

R3 roadmap scope audited:

- Feature rows `RE-COM-001` through `RE-COM-014` cover durable content and metadata storage, document versioning, metadata/classification, full-text search, capture/imaging, OCR/extraction, controlled documents, records retention, legal hold, access/sharing, redaction/privacy, e-signature, evidence packages/audit export, and APIs/connectors.
- Feature rows `RE-FND-006` through `RE-FND-011` and `RE-FND-016` cover saved views, bulk operations, import review, export/portability, notifications, APIs/webhooks/outbox, and professional report output.
- Workflow rows `RE-WF-003` and `RE-WF-014` cover intake routing and controlled-document periodic review.
- These rows are the RecordArr-owned DMS/evidence-vault baseline required by R3; they are not optional MaintainArr embellishments.

Deferred R3 blockers:

- `RecordArrDbContext` has no operational DMS entities, and `RecordArrStore` remains the registered singleton source for records, files, upload sessions, capture requests, scan/OCR/extraction results, evidence mappings, packages/manifests, metadata, links, comments, retention, disposal, legal holds, controlled documents, reviews, distributions, acknowledgements, access policies, grants, external shares, redactions, signatures, photo evidence, and access logs.
- Because the store is process-local memory, R3 requirements for durable metadata DB, object-storage metadata, checksums, immutable versions, retention/legal-hold enforcement, access-history integrity, audit package reproducibility, API idempotency, and restart-safe user work are not satisfied.
- The RecordArr API exposes broad in-memory workspace and integration endpoints. Some reads are tenant-filtered through the current user, but the R3 baseline requires endpoint-by-endpoint durable tenant scope, server-side action checks, and auditable persistence rather than filters over prototype collections.
- MaintainArr may attach references or create package/evidence context only as non-authoritative placeholders until RecordArr owns durable retained evidence. MaintainArr must not treat RecordArr prototype IDs, files, manifests, or packages as production truth.

Deferred justification:

- Closing these blockers requires a full RecordArr domain persistence design: migrations, storage metadata, file/object lifecycle, checksums, immutable version/audit records, retention/legal-hold enforcement, access policy evaluation, tenant/action enforcement, import/intake transaction handling, and migration of existing endpoint behavior.
- A narrow patch in the R3 RecordArr pass would either mask the unsafe prototype store or remove represented features, both of which would violate the roadmap and product constitution. The truthful stage-gated result is to carry the durable DMS migration as an explicit blocker before downstream products rely on RecordArr for production evidence.

Files touched:

- `docs/roadmap/products/recordarr.md`

Tests run:

- `dotnet test tests/STLCompliance.RecordArr.Auth.Tests/STLCompliance.RecordArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` - passed 3 tests.
- `npm test -- App.test.tsx` from `apps/recordarr-frontend` - passed 1 file / 10 tests.

Remaining blockers: The R0/R1 durable-store and tenant-scope blockers remain active and now directly block R3 production evidence reliance. RecordArr's R3 pass is complete for stage-gated rollout bookkeeping with explicit deferral, but RecordArr is not clear for production-authoritative retained evidence.

R3 stage result: RecordArr product pass is complete with deferred blockers. Continue the R3 suite only with the explicit understanding that MaintainArr must not depend on RecordArr as authoritative evidence persistence until the durable DMS migration is finished.

## R12 Expansion pass

Status: Product pass completed with deferred R12 blockers. RecordArr is not clear for production advanced DMS, data-room, AI, offline-capture, or disaster-recovery reliance until the durable DMS migration closes.

R12 scope audited:

- RecordArr has 35 R12 feature rows and 1 R12 workflow row (`RE-WF-015`, disaster recovery and integrity verification) in the roadmap rollout maps.
- The R12 rows cover metadata-first DMS usability, scan/OCR approval depth, one-file-many-reference behavior, source-cited semantic search/Q&A, universal intake routing, transparent OCR review, point-of-work controlled procedures, portable archive/exit, human-readable retention, secure customer/supplier/auditor data rooms, offline encrypted field capture, duplicate detection, intelligent document processing, records governance, knowledge graph retrieval, digital signatures/trust services, eDiscovery, automated change impact, provenance, external collaboration rooms, advanced forms capture, long-term preservation, and shared foundation behaviors.
- The current represented RecordArr slice remains an in-memory prototype/scaffold for those document and evidence workflows. This pass did not expand R12 capabilities or hide the carried durable-store blockers.

Deferred R12 blockers:

- `RecordArrStore` remains the process-local singleton source for records, files, upload sessions, capture/OCR/extraction output, evidence mappings, packages, retention/disposal/legal holds, controlled documents, access/shares/redactions/signatures, photo evidence, and access logs.
- `RecordArrDbContext` still has no operational DMS entities. There is no production durable metadata model, object-store metadata/index, immutable audit ledger, fixity/restore history, retention/legal-hold enforcement model, malware/quarantine processing state, or disaster-recovery reconciliation workflow.
- R12 data rooms, external collaboration rooms, semantic retrieval/Q&A, intelligent document processing, eDiscovery, trust-service signatures, long-term preservation, offline encrypted capture, and disaster recovery would all be misleading if represented as production-ready on top of the current in-memory store.
- Existing external share, redaction, legal hold, package, and access-log surfaces may remain useful for workflow context and UI direction, but they are not production-authoritative and must not be used as suite evidence truth until the durable RecordArr migration is complete.

Deferred justification:

- The required fix is not a narrow R12 patch. It is the same product-level durable DMS migration already carried from R0/R1/R3: migrations, object-storage metadata, checksums, immutable versions, audit, retention/hold enforcement, endpoint-by-endpoint tenant/action checks, disaster recovery/fixity, and safe migration of existing scaffold behavior.
- A bounded patch in R12 would either mask unsafe architecture or remove represented features, both of which violate the roadmap. The correct product-stage result is an explicit deferral with tests proving the current shell and auth smoke still behave as expected.

Files touched:

- `docs/roadmap/products/recordarr.md`

Tests run:

- `dotnet test tests/STLCompliance.RecordArr.Auth.Tests/STLCompliance.RecordArr.Auth.Tests.csproj --logger "console;verbosity=minimal"` - passed, 3 tests.
- `npm test -- App.test.tsx` from `apps/recordarr-frontend` - passed, 1 file / 10 tests.
- `npm run test:theme` from `apps/recordarr-frontend` - passed with no theme audit violations.

Remaining blockers: The R0/R1/R3 durable-store, tenant-scope, and production-evidence blockers remain active and also block R12 advanced DMS reliance. RecordArr's R12 pass is complete for stage-gated rollout bookkeeping, but RecordArr is not clear for production reliance as the suite evidence vault or advanced DMS surface.

R12 product result: RecordArr product pass is complete with deferred blockers. Continue R12 with MaintainArr, and continue to call out that downstream products must not rely on RecordArr prototype data as production-authoritative evidence until the durable migration is finished.

## Source docs

- [Feature catalog](../../products/recordarr/FEATURESET.md)
- [Workflow catalog](../../products/recordarr/WORKFLOWS.md)
- [Product manifest](../../products/recordarr/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)
