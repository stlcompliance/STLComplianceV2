# STL Compliance Constitutions Index

These files are the enforceable platform and page rules for the unified STL Compliance suite. Product documents may add domain detail, but may not silently weaken ownership, tenancy, security, durability, truthful-state, or unified-UI requirements.

There is one canonical constitution set. There are no separate V1 and V2 layers.

## Authority order

1. `ownership.md`
2. security, tenancy, product-availability, persistence, and production-readiness constitutions
3. API, event, lifecycle, workflow, evidence, and reporting constitutions
4. UI, navigation, theme, accessibility, and page-archetype constitutions
5. product-specific documents and implementation notes

A deliberate ownership or platform-rule change must update every affected constitution, product manifest, contract, route map, event catalog, workflow pack, and test gate in the same change.

## Access model

- Every active tenant member may launch every ordinary STL Compliance product.
- Product entitlements, tenant subscriptions, and per-user launch grants are not part of the platform model.
- StaffArr authority context and product-owned permission checks govern actions inside products.
- The Compliance Core administrative studio is platform-admin-only.
- Compliance Core runtime evaluation is available to every tenant through authorized product workflows and service APIs.

See `platform-product-availability-compliancecore-access-constitution.md`.

## Audit-regression gate set

Adopt these first because they directly prevent the audited failures:

1. `platform-production-readiness-gate-constitution.md`
2. `platform-durable-persistence-tenant-scope-constitution.md`
3. `platform-endpoint-authorization-map-constitution.md`
4. `platform-fixture-demo-noop-boundary-constitution.md`
5. `platform-ci-regression-quality-gates-constitution.md`
6. `platform-browser-session-spa-hardening-constitution.md`
7. `platform-actor-identity-audit-attribution-constitution.md`
8. `platform-state-machine-idempotency-concurrency-constitution.md`
9. `platform-upload-file-evidence-safety-constitution.md`
10. `platform-theme-token-component-enforcement-constitution.md`
11. `platform-navigation-information-architecture-constitution.md`
12. `platform-user-trust-error-truthfulness-constitution.md`

## Core platform constitutions

- `platform-api-integration-constitution.md`
- `platform-events-handoffs-readmodels-constitution.md`
- `platform-event-envelope-constitution.md`
- `platform-security-tenancy-authority-constitution.md`
- `platform-permission-action-matrix-constitution.md`
- `platform-reference-snapshot-mirror-constitution.md`
- `platform-record-lifecycle-status-constitution.md`
- `platform-product-key-naming-constitution.md`
- `platform-audit-evidence-retention-constitution.md`
- `platform-reference-data-ingestion-constitution.md`
- `platform-list-board-queue-constitution.md`
- `platform-notifications-tasks-inbox-constitution.md`
- `platform-external-systems-integration-constitution.md`
- `platform-external-portal-access-constitution.md`
- `platform-reporting-metrics-provenance-constitution.md`
- `platform-mobile-offline-capture-sync-constitution.md`
- `platform-settings-admin-configuration-constitution.md`
- `platform-error-degraded-state-constitution.md`
- `platform-workflow-approval-assignment-escalation-constitution.md`
- `platform-scheduling-and-created-events-constitution.md`
- `platform-scheduling-board-constitution.md`
- `platform-operational-readiness-blocker-constitution.md`
- `platform-accessibility-time-localization-human-factors-constitution.md`
- `platform-contract-testing-release-constitution.md`
- `platform-cross-product-workflow-pack-constitution.md`
- `platform-ai-assisted-intake-review-constitution.md`
- `platform-product-availability-compliancecore-access-constitution.md`

## Unified UI and page constitutions

`ui.md` governs the shared visual and interaction system. `pages/README.md` indexes the required page archetypes:

- app shell
- dashboard/overview
- index/list/directory/board/queue
- record create/edit
- record detail
- drawer/peek/quick create
- workflow wizard
- empty/loading/error/degraded states
- cross-product references
- report/print/export
- settings/preferences
- admin/permission surfaces

A page is not complete because it renders. It is complete only after it proves tenant scope, permission behavior, durable server truth, designed failure/degraded states, light/dark readability, responsive behavior, accessibility, and use of approved shared components.

## Required change checklist

Every product, platform, and UI change must complete `constitution-compliance-checklist.md`. CI and review evidence are required; prose-only claims are not proof.
