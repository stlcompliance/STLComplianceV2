# STL Compliance V2 Expansion Documents Index

## Purpose

This add-on package extends the existing full document set with V2 expansion documents that deepen execution without adding unnecessary product sprawl.

V1 defines product ownership, source-of-truth boundaries, granular models, and user-facing how-to coverage.

V2 should define how the suite behaves when real work crosses product boundaries:

- shared reference data ownership
- cross-product workflow packs
- readiness and blocker behavior
- external portal access
- permission/action matrices
- AI-assisted intake and review
- event catalogs and consumer matrices
- integration contracts and review queues
- audit packet standards
- implementation sequencing
- product UI route maps

## Added product package

- `products/referencedatacore/README_manifest.md`
- `products/referencedatacore/referencedatacore_00_scope_and_boundaries.md`
- `products/referencedatacore/referencedatacore_01_public_identifier_model.md`
- `products/referencedatacore/referencedatacore_02_uom_package_normalization_model.md`
- `products/referencedatacore/referencedatacore_03_manufacturer_brand_taxonomy_model.md`
- `products/referencedatacore/referencedatacore_04_crosswalk_alias_resolution_model.md`
- `products/referencedatacore/referencedatacore_05_workflows_status_events_apis.md`

## Added product extension document

- `products/compliancecore/compliancecore_06_questionnaire_engine_model.md`

## Added platform constitutions

- `constitutions/platform-cross-product-workflow-pack-constitution.md`
- `constitutions/platform-operational-readiness-blocker-constitution.md`
- `constitutions/platform-external-portal-access-constitution.md`
- `constitutions/platform-ai-assisted-intake-review-constitution.md`
- `constitutions/platform-permission-action-matrix-constitution.md`
- `constitutions/README_v2_addendum.md`

## Added V2 platform planning documents

- `platform/v2/README.md`
- `platform/v2/implementation-sequencing.md`
- `platform/v2/product-ui-route-map.md`
- `platform/v2/event-catalog-and-consumer-matrix.md`
- `platform/v2/integration-contracts-and-review-queues.md`
- `platform/v2/audit-packet-standards.md`

## Added cross-product workflow packs

- `workflows/v2/README.md`
- `workflows/v2/order-to-fulfillment.md`
- `workflows/v2/procure-to-receive-to-putaway.md`
- `workflows/v2/defect-to-work-order-to-parts-to-return-to-service.md`
- `workflows/v2/incident-to-retraining.md`
- `workflows/v2/quality-hold-release.md`
- `workflows/v2/vendor-order-completion-and-dispatch.md`

## Adoption order

1. Adopt the ReferenceDataCore product package so the already-listed `referencedatacore` product key has canonical product documents.
2. Adopt the cross-product workflow pack constitution.
3. Add the V2 workflow packs.
4. Adopt the readiness/blocker constitution.
5. Adopt the permission/action matrix constitution.
6. Adopt external portal access rules before exposing customer, vendor, carrier, or supplier interactions.
7. Adopt the AI-assisted intake and review constitution before implementing upload classification or record proposal features.
8. Use the event catalog, integration contracts, audit packet standards, and UI route map as implementation guides.
9. Extend existing product README manifests if the repository requires every product folder to list all local markdown files.

## Non-goals

This package does not create:

- AccountingArr
- PayrollArr
- HRISArr
- CRMArr
- SupportArr
- WorkflowArr
- ChatArr

Those remain external integrations, platform patterns, or future decisions unless STL explicitly chooses to build a replacement product.

## Prime V2 directive

V2 expands execution, not product count.

A V2 implementation should make the existing suite act coordinated across product boundaries while preserving one owner per business truth.
