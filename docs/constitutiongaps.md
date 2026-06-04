# Constitution Alignment Gaps (2026-06-03)

This document tracks gaps between current implementation and the published constitutions:

- [`docs/constitutions/ownership.md`](F:/STLComplianceV2/docs/constitutions/ownership.md)
- [`docs/constitutions/ui.md`](F:/STLComplianceV2/docs/constitutions/ui.md)

## 1) Missing Owned Products in the Current Product Set (P1)

**Why this is a gap**: The ownership constitution defines dedicated record/report/order/customer/assurance owners, but the suite registry currently documents only operational products and companion/compliance surfaces.

- Constitution expectations:
  - RecordArr, ReportArr, OrdArr, CustomArr, and AssurArr sections are defined as explicit owners at:
    - [record/report ownership](F:/STLComplianceV2/docs/constitutions/ownership.md:619)
    - [AssurArr ownership](F:/STLComplianceV2/docs/constitutions/ownership.md:663)
    - [CustomArr ownership](F:/STLComplianceV2/docs/constitutions/ownership.md:526)
    - [OrdArr ownership](F:/STLComplianceV2/docs/constitutions/ownership.md:572)
    - [report behavior rule](F:/STLComplianceV2/docs/constitutions/ownership.md:751)
- Current platform surface is “seven product APIs” (suite + six products + companion + marketing site):
  - [Final implementation report](F:/STLComplianceV2/FINAL_IMPLEMENTATION_REPORT.md:14)
- App and front-end registries also only expose current Arr products (`staffarr`, `trainarr`, `maintainarr`, `routarr`, `supplyarr`, `loadarr`, plus `compliancecore`/`companion`):
  - [Product switcher map keys](F:/STLComplianceV2/apps/suite-frontend/src/components/ProductSwitcher.tsx:16)
  - [Marketing product keys](F:/STLComplianceV2/apps/stlcompliancesite/src/content/products.ts:117)

**Gap impact**
- Cross-cutting ownership rules cannot be enforced when owners are collapsed into neighboring products.
- Nonconformance/case management, customer/request orchestration, document retention ownership, and consolidated reporting remain underspecified in runtime policy.

**Suggested remediation**
- Materialize these Arr products in roadmap order (or explicitly mark them as non-normative placeholders in constitution if intentionally deferred).
- Add ownership boundaries and event contracts to de-silo current product boundaries before further feature expansion.

## 2) Document/Record Handling Is Distributed Across Operational APIs (P1)

**Why this is a gap**: Constitution requires products to attach documents through RecordArr, with retention/audit handled there.

- Rule text says products should attach records through RecordArr and that RecordArr preserves storage, metadata, and retention/audit history:
  - [RecordArr owning rules](F:/STLComplianceV2/docs/constitutions/ownership.md:656)
- Current implementation has document endpoints inside non-RecordArr APIs:
  - [StaffArr personnel documents routes](F:/STLComplianceV2/apps/staffarr-api/StaffArr.Api/Endpoints/PersonnelDocumentEndpoints.cs:11)
  - [StaffArr document storage path](F:/STLComplianceV2/apps/staffarr-api/StaffArr.Api/Options/DocumentStorageOptions.cs:7)
  - [SupplyArr vendor documents routes](F:/STLComplianceV2/apps/supplyarr-api/SupplyArr.Api/Endpoints/VendorDocumentEndpoints.cs:7)
  - [SupplyArr document storage path](F:/STLComplianceV2/apps/supplyarr-api/SupplyArr.Api/Options/DocumentStorageOptions.cs:7)
  - [MaintainArr work-order evidence routes](F:/STLComplianceV2/apps/maintainarr-api/MaintainArr.Api/Endpoints/WorkOrderLaborEvidenceEndpoints.cs:11)
- Some of these paths persist files in product-local stores (`data/personnel-documents`, `data/supplyarr-documents`), which contradicts the constitution’s central-record model.

**Gap impact**
- Retention policy, legal hold behavior, and audit trail continuity are fragmented.
- Evidence provenance for cross-product workflows becomes product-local by default.

**Suggested remediation**
- Introduce `recordarr-api` as the document/storage authority.
- Convert existing upload/list/download endpoints to either delegate to RecordArr or emit signed delegated references only.

## 3) LoadArr Is Not Fully WMS-Aligned in Persistence (P1)

**Why this is a gap**: Ownership constitution positions LoadArr as the warehouse execution truth (`stock movement`, `inventory balances`, etc.).

- Constitutional behavior for LoadArr includes owning stock movement truth:
  - [LoadArr ownership summary in implementation plan](F:/STLComplianceV2/docs/loadarr_implementation.md:13)
  - [LoadArr owning rules](F:/STLComplianceV2/docs/constitutions/ownership.md:436)
- Current API host registration only maps auth/launch/workspace/inventory endpoints; no broader endpoint surface is guaranteed here:
  - [LoadArr Program endpoint map](F:/STLComplianceV2/apps/loadarr-api/LoadArr.Api/Program.cs:7)
- `LoadArrDbContext` currently does not define product entities:
  - [LoadArr DbContext](F:/STLComplianceV2/apps/loadarr-api/LoadArr.Api/Data/LoadArrDbContext.cs:1)
- Workspace response is generated from hard-coded inline sample data (`CreateWorkspaceSummary`) instead of source-of-truth persistence:
  - [Hard-coded summary method](F:/STLComplianceV2/apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs:1300)
  - [Sample location rows](F:/STLComplianceV2/apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs:1306)
  - [Sample inventory rows tied to hard-coded location IDs](F:/STLComplianceV2/apps/loadarr-api/LoadArr.Api/Endpoints/LoadArrWorkspaceEndpoints.cs:1352)

**Gap impact**
- Inventory truth and movement history can become inconsistent and unobservable if source-backed entities are absent.
- Handoffs into RoutArr/MaintainArr still operate without a durable LoadArr ledger contract.

**Suggested remediation**
- Add migrations/entities for movements, reservations, lots, holds, and adjustments under LoadArr.
- Keep workspace summary as presentation projection only, sourced from those entities.

## 4) UI Shell/Product Coherence Is Ahead of Product Model (P2)

**Why this is a gap**: UI constitution asks for a coherent suite and future-proofed shell for all products, including future Arrs.

- Shell-level intent:
  - [UI constitution prime directive](F:/STLComplianceV2/docs/constitutions/ui.md:3)
- Product switcher/product descriptions only describe implemented products:
  - [Product switcher static descriptions](F:/STLComplianceV2/apps/suite-frontend/src/components/ProductSwitcher.tsx:16)
  - [Marketing product registry keys](F:/STLComplianceV2/apps/stlcompliancesite/src/content/products.ts:117)

**Gap impact**
- New constitutional products are likely to be missed by product launch navigation, entitlement copy, and launch UX unless explicit planning is added.

**Suggested remediation**
- Add shell affordances for constitutional products with “not yet available” states rather than omitting product concepts entirely.
- Add placeholder routing/launch states so entitlement, handoff, and governance logic can be added without redesigning shell behavior later.

## 5) Reporting & Compliance Aggregation Is Underrepresented as a Separate System (P2)

**Why this is a gap**: Constitution defines ReportArr as a separate reporting and dashboard owner.

- Rule text:
  - [ReportArr reports; does not mutate](F:/STLComplianceV2/docs/constitutions/ownership.md:715)
- Current suite registry and implementation currently route reporting duties into existing products rather than a dedicated ReportArr surface:
  - [Marketing product registry keys](F:/STLComplianceV2/apps/stlcompliancesite/src/content/products.ts:117)
  - [Suite switcher keys](F:/STLComplianceV2/apps/suite-frontend/src/components/ProductSwitcher.tsx:16)

**Gap impact**
- Cross-suite reporting and analytics risks staying coupled to operational services and can drift from constitutional boundary.

**Suggested remediation**
- Define a ReportArr product scope with explicit reporting API contracts, dashboards, and dashboard ownership boundaries.

## Validation Notes

- This document only reflects implementation state currently present in the checked-out workspace; no source edits were performed yet.
- Gaps were prioritized by constitution-level blast radius (highest severity where ownership and data trust boundaries are most affected).
