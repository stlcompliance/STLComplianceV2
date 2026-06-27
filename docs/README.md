# STL Compliance Documentation

This documentation package is now organized around a roadmapped rollout layer while preserving the complete product feature and workflow universe.

## Start with rollout

- [roadmap/README.md](roadmap/README.md) — staged rollout control layer.
- [roadmap/rollout-stages.md](roadmap/rollout-stages.md) — R0 through R12 release definitions.
- [roadmap/release-gates-and-acceptance.md](roadmap/release-gates-and-acceptance.md) — required proof before declaring release completion.
- [roadmap/vertical-slice-backlog.md](roadmap/vertical-slice-backlog.md) — cross-product slices that prove the suite.
- [roadmap/no-feature-loss-inventory.md](roadmap/no-feature-loss-inventory.md) — inventory summary and CSV links.

## Authority rule

Constitutions remain binding. Product feature and workflow catalogs remain the complete retained product universe. The roadmap determines sequence and release proof; it does not delete features or relax discipline.

---

## Original package overview

This directory is the canonical, version-neutral documentation set for the STL Compliance / Adaptive Risk Reduction suite.

There is no separate V1 or V2 documentation layer. Product ownership, platform rules, cross-product execution, page behavior, security controls, workflow packs, operations, and user guidance are maintained together. When documents conflict, use the following order of authority:

1. `constitutions/ownership.md`
2. Platform constitutions under `constitutions/`
3. Page constitutions under `constitutions/pages/`
4. Product scope and boundary documents under `products/`
5. Product model, workflow, API, and UI documents
6. Cross-product workflow packs under `workflows/`
7. Implementation notes and user guidance
8. Historical audit snapshots

## Start here

- `DOCS_MERGE_CHANGELOG.md` — what was merged, removed, and added.
- `constitutions/README.md` — binding constitution index and adoption order.
- `constitutions/pages/README.md` — unified page-archetype contract.
- `platform/audit-remediation-acceptance-matrix.md` — audit findings and required acceptance evidence.
- `platform/unified-ui-page-coverage-matrix.md` — expected page coverage by product.
- `DOCUMENTATION_VALIDATION.md` — link, manifest, terminology, and structure checks.
- `VALIDATION_REPORT_2026-06-23.md` — completed package validation and intentional historical exceptions.

## Access model

All active tenant members can launch every ordinary STL Compliance product. Product availability is not sold, granted, revoked, or varied by tenant or user. The Compliance Core administrative UI is the sole product UI reserved for platform administrators. Compliance Core evaluation, questionnaire, normalization, applicability, evidence-requirement, and ruling operations remain available to all tenants and users through the products and workflows that consume them.

Opening a product is not authorization to perform every action. StaffArr authority context and product-owned permission checks govern records and actions inside each product. NexArr owns identity, tenant membership, sessions, launch context, service identity, and platform-admin status; it does not own any variable per-product access model because ordinary product availability is fixed.

## Primary directories

- `constitutions/` — binding ownership, security, data, workflow, UI, release, and page rules.
- `products/` — product-specific end-state models, workflows, APIs, UI surfaces, and production-safety requirements.
- `platform/` — cross-suite route maps, event catalogs, integration contracts, audit standards, and implementation sequencing.
- `workflows/` — executable cross-product workflow packs.
- `platform-reference-data/` — platform service guidance for curated shared reference datasets; this is not a catch-all user-facing product.
- `user/` and `how-to/` — end-user and operator guidance.
- `deployment/` and `operations/` — deployment and runbook material.
- `audit/` — historical implementation snapshots; audit observations do not override constitutions.

## Required change workflow

Every code or documentation change must identify its owner, applicable constitutions, permission boundary, tenant boundary, durable data behavior, cross-product contract effects, page archetype, light/dark behavior, failure states, and regression proof. Use `constitutions/constitution-compliance-checklist.md` as the required review checklist.

