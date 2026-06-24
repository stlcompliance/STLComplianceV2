# Audit Remediation Acceptance Matrix

## Purpose

This matrix converts the June 23, 2026 audit findings into release gates. Closing a ticket or rendering a page is not acceptance. Each item requires implementation evidence, automated regression proof, and documentation alignment.

| Finding | Required remediation | Required proof | Governing docs |
|---|---|---|---|
| SEC-001 AssurArr anonymous/default tenant | deny-by-default auth; route permissions; validated tenant/actor; durable tenant-owned data | anonymous 401, forbidden 403, two-tenant collision tests, route inventory completeness | endpoint authorization, tenancy, actor attribution, AssurArr safety addendum |
| SEC-002 RecordArr global/fail-open store | durable tenant-scoped records/files/policies; default-deny access; claim-derived actor | restart/multi-replica, cross-tenant, no-policy deny, legal-hold/purge transaction tests | persistence, uploads/evidence, RecordArr safety addendum |
| SEC-003 OrdArr global orders/no tenant | durable tenant-owned aggregates and idempotency; product permissions | cross-tenant list/detail/write denial, duplicate retry, concurrency and restart tests | persistence, state machine/idempotency, OrdArr safety addendum |
| SEC-004 ReportArr global/non-durable state | durable definitions, schedules, runs, lineage, recipients and worker cursors | tenant isolation, lease/recovery, restart, permission and output-lineage tests | reporting provenance, persistence, ReportArr safety addendum |
| FUNC-001 LoadArr fixtures/no-op/local success | transactional inventory ledger and durable workflow state; remove client fallback success | ledger invariants, duplicate scan, hold, conflict, restart and failed-write UI tests | fixture/no-op boundary, truthful errors, LoadArr safety addendum |
| REL-001/REL-002 broken CI | repair jobs/scripts/dependency installs; include all apps and packages | clean-checkout CI, migration gate, browser smoke, no missing-script pass | CI regression and contract/release constitutions |
| SEC-005 browser token/CSP | HttpOnly same-origin session/BFF or equivalent; CSRF; CSP on SPA HTML; no JS-readable refresh token | browser security tests, header snapshot, token-storage static check, logout/revocation tests | browser session/SPA hardening |
| SEC-006 Field Companion local sensitive data | minimize/encrypt/protect offline payloads; device/session binding; clear/revoke controls | storage inspection, lost-device/revocation, tamper/idempotency and privacy tests | mobile/offline, Field Companion security addendum |
| SEC-007 upload buffering/scanning | streaming/direct upload, hard limits, signature checks, quarantine/scan | oversized/malformed/malware simulation, safe download and quarantine-state tests | upload/file/evidence safety |
| SEC-008 actor spoofing | claim/service delegation-derived actor; append-only attribution | request-body spoof attempts fail; delegated actor chain preserved | actor identity/audit attribution |
| SEC-009 refresh race | atomic rotation, replay-family revocation, concurrency controls | parallel refresh race and replay tests | browser session/SPA hardening, NexArr security addendum |
| SEC-010 MFA plaintext | protected/encrypted secret storage and rotation/recovery controls | database inspection and key-rotation/recovery tests | NexArr security addendum |
| SEC-011 userId/personId conflation | explicit NexArr account ID to StaffArr person mapping | linked/unlinked/multi-membership identity tests | ownership, Field Companion boundary, StaffArr account workflow |
| UI-001 theme violations | semantic tokens/shared components; remove hard-coded light/dark assumptions | static theme audit plus light/dark visual smoke on every page archetype | UI, theme token and page constitutions |
| TEST-001 no meaningful tests | minimum route/page/domain test coverage; no `passWithNoTests` | per-product test inventory and failing zero-test gate | CI regression quality gates |

## Product release state

A blocked product becomes release-eligible only when all common platform gates and every product-specific row pass. Warning banners, environment labels, or documentation statements do not compensate for unsafe implementation.

## Evidence packet

Each remediation packet must contain:

- finding ID and affected routes/components
- code and migration references
- endpoint authorization-map delta
- tenant and actor data-flow explanation
- automated test names and results
- light/dark and designed-state screenshots for UI changes
- restart/multi-replica evidence where durable state is involved
- updated owner/product/page/workflow docs
- reviewer and approval record
