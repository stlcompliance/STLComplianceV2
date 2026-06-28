# Field Companion Rollout Plan

| Field | Value |
| --- | --- |
| Product key | `fieldcompanion` |
| Category | MAM / mobile companion |
| Entry release | R9 — Field Companion mobile execution |
| Completion release | R9 — Field Companion mobile execution |
| Expansion release | R12 — Expansion, portals, advanced integrations, AI, and category depth |
| Role | Mobile assigned work, secure capture/upload, offline queueing, sync, scanning, and product action surfaces. |
| Roadmap slice | Mobile execution after owning APIs are durable |
| Must not violate | Never become a mobile source of truth; replay all actions through owning APIs. |
| Feature rows retained | 71 |
| Workflow rows retained | 17 |

## Release mapping

| Stage | Stage name | Features | Workflows |
| --- | --- | --- | --- |
| R9 | Field Companion mobile execution | 33 | 13 |
| R12 | Expansion, portals, advanced integrations, AI, and category depth | 38 | 4 |

## Implementation interpretation

- Current/represented capabilities are hardened in R9 unless they are only supporting another release gate.
- Common category baseline remains retained for R9.
- Advanced, widely requested, or democratized capabilities remain retained for R12 unless pulled forward by a vertical slice.
- Do not implement this product by copying another product's source truth.
- Do not call this product complete until its release gates pass for data, authorization, tenant scope, UI, evidence, recovery, and reportability.

## Source docs

- [Feature catalog](../../products/fieldcompanion/FEATURESET.md)
- [Workflow catalog](../../products/fieldcompanion/WORKFLOWS.md)
- [Product manifest](../../products/fieldcompanion/README_manifest.md)
- [Complete feature rollout CSV](../reference/feature-rollout-map.csv)
- [Complete workflow rollout CSV](../reference/workflow-rollout-map.csv)

## R0 Trust Gate pass

Status: Clear for the current mobile launch/profile/offline-state slice; broader mobile execution remains limited by owning-product readiness.

Completed in this pass:

- Changed NexArr-backed Field Companion handoff and `/api/v1/mobile/me` responses to use a fixed ordinary-suite launch catalog for Field Companion sessions.
- Ensured Compliance Core is not exposed in Field Companion ordinary launch lists, while `fieldProductKeys` remains the explicit list of mobile-capable product work surfaces.
- Preserved server-side authentication, active tenant membership, requested person identity, and owning-product field inbox validation.
- Verified frontend session storage, API client compatibility, workspace loading, offline queue preservation, and local submission state behavior.

Files touched:

- `apps/nexarr-api/NexArr.Api/Services/FieldCompanionAuthService.cs`
- `apps/nexarr-api/NexArr.Api/Services/FieldCompanionSuiteLaunchCatalog.cs`
- `tests/STLCompliance.NexArr.Auth.Tests/NexArrFieldCompanionFieldInboxTests.cs`

Tests run:

- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~NexArrFieldCompanionFieldInboxTests" --logger "console;verbosity=minimal"` — passed 7 tests. Existing NuGet pruning and xUnit analyzer warnings remain.
- `npm test -- client.test.ts sessionStorage.test.ts useFieldCompanionWorkspace.test.tsx useOfflineQueue.test.tsx OfflineQueuePanel.test.tsx submissionState.test.ts` in `apps/fieldcompanion-frontend` — passed 7 files / 21 tests.

Remaining R0 blockers:

- Field Companion has no dedicated durable backend source of truth in this repo by design. Durable offline intents, submissions, notifications, and sync outcomes must continue to be owned by NexArr or the owning product APIs, and any product-specific mobile action is only production-trust-clear when that owning product validates tenant, permission, idempotency, concurrency, evidence, and failure states.
- Several Field Companion test helpers still seed legacy launch-destination compatibility rows for older NexArr fixtures. They do not drive the current fixed-suite launch behavior, but should be retired when the remaining NexArr launch-destination compatibility tests are cleaned up.
- No R1/R9 feature expansion was started in this R0 pass.

## R1 Foundation spine pass

Status: Not applicable. Field Companion has no R1 feature or workflow rows in the roadmap rollout maps, and its entry release remains R9.

R1 scope audited:

- `docs/roadmap/reference/feature-rollout-map.csv` contains no Field Companion rows for `R1`.
- `docs/roadmap/reference/workflow-rollout-map.csv` contains no Field Companion rows for `R1`.
- Field Companion's product FEATURESET and WORKFLOWS remain retained full scope, including the no-mobile-source-of-truth boundary, but they do not authorize starting the R9 mobile execution stage during the R1 suite stage.

Files touched:

- `docs/roadmap/products/fieldcompanion.md`

Tests run:

- Not run. This was a roadmap applicability/documentation pass only; no Field Companion code, UI, API, data-flow, or test files changed.

Remaining blockers: None for R1. Field Companion must wait for the suite to reach R9 and for owning product APIs to validate mobile actions, idempotency, concurrency, evidence, and failure states.

R1 stage result: Field Companion is clear for the R1 suite gate as not applicable.

## R9 Field Companion mobile execution pass

Status: Clear for R9. Field Companion remains a mobile execution surface backed by NexArr and owning-product APIs; no Field Companion-owned operational source of truth was introduced.

R9 scope audited:

- R9 roadmap rows: 33 feature rows and 13 workflow rows mapped to Field Companion.
- Product boundaries in `docs/products/fieldcompanion/FEATURESET.md`, `docs/products/fieldcompanion/WORKFLOWS.md`, and `docs/constitutions/platform-mobile-offline-capture-sync-constitution.md`.
- Frontend mobile execution surfaces for launch/profile, home, scan, offline queue, shared-device protection, and product workspace layout.
- NexArr-backed Field Companion server tests for mobile session, field inbox, scan resolution, field submissions, evidence, offline sync, clock, notifications, tenant scope, and permission failures.

Completed in this pass:

- Removed ordinary-user exposure of raw person, user, tenant, task, reason-code, and idempotency details from the Field Companion profile, scan result, offline conflict, and shared-device queue surfaces.
- Replaced tenant slug display in Field Companion home/profile/shared-device copy with tenant display-name context.
- Kept scan resolution and offline queue recovery actionable through product labels, task titles, human-readable statuses, and plain recovery guidance.
- Preserved the no-mobile-source-of-truth boundary: queued work remains pending until NexArr/owning-product sync validates and accepts it; domain actions continue to route through owning product APIs.
- Preserved current offline work on errors and shared devices by keeping review/sync/discard paths explicit rather than silently clearing queued work.

Files touched:

- `apps/fieldcompanion-frontend/src/pages/ProfilePage.tsx`
- `apps/fieldcompanion-frontend/src/pages/ProfilePage.test.tsx`
- `apps/fieldcompanion-frontend/src/pages/HomePage.tsx`
- `apps/fieldcompanion-frontend/src/pages/HomePage.test.tsx`
- `apps/fieldcompanion-frontend/src/components/FieldScanPanel.tsx`
- `apps/fieldcompanion-frontend/src/components/FieldScanPanel.test.tsx`
- `apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.tsx`
- `apps/fieldcompanion-frontend/src/components/OfflineQueuePanel.test.tsx`
- `apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.tsx`
- `apps/fieldcompanion-frontend/src/components/SharedDeviceProtectionOverlay.test.tsx`
- `apps/fieldcompanion-frontend/src/layouts/ProductWorkspaceLayout.tsx`
- `docs/roadmap/products/fieldcompanion.md`

Tests run:

- `npm test -- ProfilePage.test.tsx FieldScanPanel.test.tsx OfflineQueuePanel.test.tsx` in `apps/fieldcompanion-frontend` — passed 3 files / 8 tests.
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~FieldCompanion" --logger "console;verbosity=minimal"` — passed 65 tests. Existing NuGet pruning warnings remain.
- `npm test -- ProfilePage.test.tsx HomePage.test.tsx FieldScanPanel.test.tsx OfflineQueuePanel.test.tsx SharedDeviceProtectionOverlay.test.tsx ProductWorkspaceLayout.test.tsx` in `apps/fieldcompanion-frontend` — passed 6 files / 15 tests.
- `npm test` in `apps/fieldcompanion-frontend` — passed 54 files / 152 tests.
- `npm run test:theme` in `apps/fieldcompanion-frontend` — no violations.

Remaining blockers:

- No R9 blockers remain for the represented Field Companion slice.
- Field Companion still has no dedicated durable backend by design; durable mobile action state remains in NexArr or the owning products.
- R12 remains the home for one-time external capture links, advanced MAM/MDM integration, voice/glove workflows, computer vision, advanced offline policy, geofencing/proximity, AR guidance, and broader mobile observability.

R9 stage result: Field Companion is clear for the R9 suite gate.

## R12 Expansion, portals, advanced integrations, AI, and category depth pass

Status: Clear for the current R12 pass with advanced mobile/MAM, external capture, and owner-backed action-contract work deferred below. No next-stage work was started.

R12 scope audited:

- R12 roadmap rows: 38 feature rows and 4 workflow rows mapped to Field Companion.
- R12 workflows: `FC-WF-006` mobile synchronization conflict resolution, `FC-WF-012` one-time external capture link, `FC-WF-013` app-protection policy and selective wipe, and `FC-WF-017` emergency/degraded mobile operation.
- R12 feature areas covering privacy-safe location/time proof, external capture, human-readable conflicts, user-controlled storage/network behavior, micro-surfaces, accessible evidence collection, shared-device safety, MAM/MDM integration, selective wipe, per-app VPN/certificates, mobile threat defense, computer vision, remote expert assistance, credential wallet, advanced offline policy, proximity/AR guidance, mobile observability, tenant-scoped authorization, StaffArr-backed permissions, RecordArr evidence references, ReportArr projections, Compliance Core gates, and AI-assisted proposals.
- Current frontend slices for device diagnostics, degraded-operation support summaries, clock submissions, push subscription registration, profile/readiness, offline queue conflicts, and shared-device protection.

Completed in this pass:

- Added a Field Companion device-privacy helper that converts browser/device diagnostics into coarse labels such as browser family, device class, and language group.
- Replaced full browser user-agent/platform display in device readiness and support summaries with coarse diagnostics.
- Replaced raw user-agent metadata in clock punch submissions with a coarse device source label.
- Replaced raw user-agent metadata in push subscription registration with the same coarse device source label.
- Preserved the no-mobile-source-of-truth boundary: Field Companion still queues and presents mobile work while NexArr or the owning product validates, syncs, commits, rejects, and audits domain outcomes.
- Did not build external no-login capture links, native MAM/MDM controls, computer vision, remote expert, credential wallet, advanced offline policy engine, geofence/proximity, AR guidance, or AI-assisted proposal flows.

Files touched:

- `apps/fieldcompanion-frontend/src/lib/devicePrivacy.ts`
- `apps/fieldcompanion-frontend/src/lib/deviceCapabilities.ts`
- `apps/fieldcompanion-frontend/src/lib/deviceCapabilities.test.ts`
- `apps/fieldcompanion-frontend/src/lib/pushNotifications.ts`
- `apps/fieldcompanion-frontend/src/lib/pushNotifications.test.ts`
- `apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.tsx`
- `apps/fieldcompanion-frontend/src/components/DeviceCapabilityPanel.test.tsx`
- `apps/fieldcompanion-frontend/src/pages/ClockPage.tsx`
- `apps/fieldcompanion-frontend/src/pages/ClockPage.test.tsx`
- `docs/roadmap/products/fieldcompanion.md`

Tests run:

- `npm test -- deviceCapabilities.test.ts pushNotifications.test.ts DeviceCapabilityPanel.test.tsx degradedOperation.test.ts DegradedOperationPanel.test.tsx ClockPage.test.tsx ProfilePage.test.tsx OfflineQueuePanel.test.tsx SharedDeviceProtectionOverlay.test.tsx` in `apps/fieldcompanion-frontend` - passed 9 files / 19 tests.
- `npm run test:theme` in `apps/fieldcompanion-frontend` - no violations.
- `dotnet test tests/STLCompliance.NexArr.Auth.Tests/STLCompliance.NexArr.Auth.Tests.csproj --filter "FullyQualifiedName~FieldCompanion" --logger "console;verbosity=minimal"` - passed 65 tests. Existing NuGet pruning warnings remain.

Deferred R12 blockers:

- `FC-WF-006` remains partial: the current offline queue explains conflicts and preserves retry/discard paths, but full field-level merge/reapply policies, supervisor review, and owning-product conflict schemas remain deferred to product action contracts.
- `FC-WF-012` one-time external capture links remain deferred until NexArr token issuance, public scoped capture surfaces, RecordArr evidence intake, owning-product commit contracts, revocation, replay protection, and receipt/audit flows are implemented.
- `FC-WF-013` remains partial: current device cleanup and shared-device protection are present, but external MAM/MDM provider integration, conditional launch, per-app VPN/certificates, managed open-in controls, mobile threat defense, verified selective wipe, and residual-cache verification are deferred.
- `FC-WF-017` remains partial: degraded-operation guidance is present, but tenant-approved emergency procedures, product-specific fallback policies, diagnostic packages without secrets, operator notifications, and reconciliation workflows are deferred.
- Advanced R12 capabilities remain retained but not started in this pass: voice/glove workflows, computer-vision-assisted capture, remote expert assistance, digital credential wallet, advanced offline policy engine, geofenced/proximity-aware actions, AR work guidance, fleet compatibility observability, privacy/legal-hold awareness, and AI-assisted proposals with human review.

R12 stage result: Field Companion is clear for the R12 suite gate with the deferred mobile/MAM, external-capture, owner-contract, and advanced-device blockers above.
