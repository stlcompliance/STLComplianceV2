# W189 — SupplyArr M8 supplier onboarding

## Scope

Tenant-scoped supplier/vendor onboarding workflow with compliance document checklist gating, review transitions, audit logging, and integration outbox events — owned by SupplyArr.

## Persistence

| Table | Purpose |
|-------|---------|
| `supplyarr_party_supplier_onboarding` | Per-party onboarding state (one row per external party) |
| `supplyarr_tenant_supplier_onboarding_settings` | Tenant required document type keys (JSON) |
| `supplyarr_party_compliance_documents` | W184 — document register/review (reused for checklist) |

Migration: `SupplyArrSupplierOnboarding`.

### Onboarding status model

- `draft` — editable; documents can be registered
- `pending_review` — submitted; awaiting approver
- `approved` — party `approval_status` synced to `approved`
- `rejected` — party `approval_status` synced to `restricted`; can resubmit from draft/rejected
- `suspended` — only from `approved`; party restricted

Default required document types: `w9`, `insurance_certificate`, `supplier_agreement`.

## API

### Supplier onboarding (`/api/supplier-onboarding`, JWT)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/document-requirements` | `RequireSupplierOnboardingRead` |
| PUT | `/document-requirements` | `RequireSupplierOnboardingManage` |
| GET | `/pending` | `RequireSupplierOnboardingReview` |
| POST | `/start` | `RequireSupplierOnboardingManage` |
| GET | `/parties/{partyId}` | `RequireSupplierOnboardingRead` |
| POST | `/parties/{partyId}/submit` | `RequireSupplierOnboardingManage` |
| POST | `/parties/{partyId}/approve` | `RequireSupplierOnboardingReview` |
| POST | `/parties/{partyId}/reject` | `RequireSupplierOnboardingReview` |
| POST | `/parties/{partyId}/suspend` | `RequireSupplierOnboardingReview` |

### Party compliance documents (`/api/parties/{partyId}/compliance-documents`)

| Method | Route | Auth |
|--------|-------|------|
| GET | `/` | read |
| POST | `/` | manage |
| POST | `/{documentId}/approve` | review |
| POST | `/{documentId}/reject` | review |

Auth maps: read = parties read; manage = parties manage; review = purchase request approve (manager/admin).

## Outbox (W187)

Events on transitions: `supplier_onboarding.submitted`, `supplier_onboarding.approved`, `supplier_onboarding.rejected`, `supplier_onboarding.suspended`.

## UI

- `SupplierOnboardingPanel` on Parties workspace — party picker, document checklist, register/approve docs, submit, pending queue, approve/reject.

## Tests

- `SupplyArrSupplierOnboardingTests` — E2E with docs + outbox; submit blocked without docs
- `SupplierOnboardingPanel.test.tsx`

## Next slice

Per backlog M8: **emergency purchase workflow** (or cross-milestone **M10** RoutArr demand intake mirror).
