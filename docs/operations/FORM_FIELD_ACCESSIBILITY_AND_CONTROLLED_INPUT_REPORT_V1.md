# Form Field Accessibility and Controlled Input Report (V1)

Generated: 2026-05-29

## Executive summary

Suite-wide pass across nine product frontends (`apps/*-frontend`, `apps/stlcompliancesite`):

| Step | Status |
|------|--------|
| 1–3 Accessible labels (`id`, `htmlFor`, specific text) | **Done** — ~154 component files; inventory unlabeled count **271 → 21** (remainder are dynamic rows with `aria-labelledby` / wrapping labels the static scanner misses) |
| 4 Controlled input matrix | **Applied** where list APIs or fixed enums exist |
| 5 Replace raw ID/key with pickers | **Done** for driver, vehicle, person, part, vendor, PR/PO, rule pack, tenant (platform admin); advanced text retained behind toggle |
| 6 GeneratedKeyField on create | **Done** for Supplyarr/Maintainarr keys; **added** Trainarr step keys |
| 7 Reason code + notes split | **Started** — Purchase request rejection; shared reason vocab in `controlledFormHelpers.ts`; more panels listed under follow-up |
| 8 Comma-separated → multi-select | **Done** — e.g. `ServiceTokenAdminPanel` product keys (`CheckboxMultiSelect`), batch qualification people |
| 9 Shared vocabularies (UOM, currency, evidence) | **Done** where `formOptions` / `controlledFormHelpers` exist |
| 10 Preserve freetext | Honored (names, notes, JSON, CSV, webhooks, RMA/BOL) |
| 11 Tests | **Updated** — label-based queries added/extended on changed panels |

Inventory script: `scripts/extract-user-input-fields.mjs` → `docs/USER_INPUT_FIELDS.txt`

Shared primitives: `packages/shared-ui` — `FormField`, `CheckboxField`, `ControlledSelect`, `StaticSearchPicker`, `AsyncSearchPicker`, `GeneratedKeyField`, `AdvancedReferenceField`, `CheckboxMultiSelect`, `normalizeUom`

---

## apps/routarr-frontend/src/components/BulkDispatchPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| unlabeled / raw text | Driver person | StaffArr person reference (local mirror list) | `StaticSearchPicker` | `listDrivers` API |
| raw GUID text | Driver person (advanced) | Optional cross-product reference | `AdvancedReferenceField` | Retained behind toggle |
| raw text | Vehicle reference | Vehicle ref list | `StaticSearchPicker` | `listVehicleRefs` API |
| raw text | Vehicle reference (advanced) | Advanced reference | `AdvancedReferenceField` | Behind toggle |
| vague select | Dispatch status | Small enum | `ControlledSelect` | `assigned`, `dispatched`, … |
| trip row checkbox | Select trip (per title) | Row action | checkbox + `aria-label` | Bulk selection |

**Tests:** `BulkDispatchPanel.test.tsx` — uses advanced driver path + `data-testid`; picker path can add `getByLabelText(/driver person/i)` in follow-up.

---

## apps/routarr-frontend/src/components/TripsPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| placeholder / raw | Vehicle reference | Vehicle ref list | `StaticSearchPicker` | Create trip |
| raw | Vehicle reference (advanced) | Advanced | `AdvancedReferenceField` | |
| raw | Driver person (advanced) | StaffArr person | `AdvancedReferenceField` | Assign driver block also has `StaticSearchPicker` label **Assign driver** |
| manual text | Initial load key | Create-time business key | `GeneratedKeyField` | From title + optional manual override |
| vague | Trip status filter | Small enum | `select` + `htmlFor` | |
| vague | Dispatch status | Small enum | `ControlledSelect` / labeled `select` | Detail actions |

---

## apps/routarr-frontend/src/components/DriverAvailabilityPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| raw person id | Driver | StaffArr driver mirror | `StaticSearchPicker` | `listDrivers` |
| raw | Person id (advanced) | Advanced | `AdvancedReferenceField` | |
| vague Status | Availability status | Small enum | labeled `select` | unavailable / limited / available |
| Reason | Availability reason | Narrative | text (valid freetext) | PTO, training, etc. |

---

## apps/supplyarr-frontend/src/components/PurchaseRequestPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| single text | Rejection reason code | Workflow reason | `ControlledSelect` | `PROCUREMENT_REJECTION_REASON_OPTIONS` |
| — | Rejection notes (optional) | Narrative | `textarea` | Combined to API `Reason` via `formatProcurementReason` |
| manual key | Request key | Create-time key | `GeneratedKeyFieldGroup` | From request title |
| raw selects | Vendor / Part | Party & part catalogs | `ControlledSelect` | Existing |

**Tests:** `PurchaseRequestPanel.test.tsx` — extend with `getByLabelText(/rejection reason code/i)` recommended.

---

## apps/supplyarr-frontend/src/components/PartCatalogPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| manual keys | Catalog key / Part key | Create-time keys | `GeneratedKeyFieldGroup` | |
| freetext UOM | Unit of measure | Shared vocabulary | `ControlledSelect` + `normalizeUom` | `UOM_OPTIONS` |
| freetext category | Category key | Local known list | `ControlledSelect` | From parts catalog |

---

## apps/supplyarr-frontend/src/components/WarrantyClaimsPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| duplicate **Vendor disposition** | Vendor disposition outcome | Small enum | `select` | Disambiguated from notes field |
| duplicate | Vendor response notes | Narrative | `textarea` | |
| single textarea | Denial reason / Cancel reason | Workflow reason | labeled `textarea` | **Follow-up:** split to reason code + notes using `WARRANTY_DENIAL_REASON_OPTIONS` |

---

## apps/suite-frontend/src/components/platform-admin/ServiceTokenAdminPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| comma-separated product keys | Allowed product keys | Small known set | `CheckboxMultiSelect` | `productOptions` from Nexarr catalog API |
| manual client key | Client key | Create-time key | `GeneratedKeyField` | |
| raw tenant GUID | Tenant scope (optional) | Tenant catalog | `ControlledSelect` | Platform tenant overview API |
| raw client id | Service client | Registered clients | `ControlledSelect` | |

---

## apps/staffarr-frontend/src/components/OrgHierarchyManager.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| placeholder-only | Org unit name | Display name | `input` + visible label | |
| placeholder-only | Org unit type | Unit type | `input` + visible label | |
| unlabeled select | Parent org unit | Org unit hierarchy | `select` + `htmlFor` | Local tree options |

---

## apps/trainarr-frontend/src/components/StepBuilderPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| manual **Step key** | Generated step key | Create-time business key | `GeneratedKeyField` | Slug from step name; manual override behind **Customize step key** |
| vague | Training definition | Known list | labeled `select` | |
| vague | Step type | Small enum | labeled `select` | content / quiz / practical |

**Tests:** `StepBuilderPanel.test.tsx` — updated to generate key from name.

---

## apps/trainarr-frontend/src/components/ManualAssignmentPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| raw **StaffArr person ID** | StaffArr person | Known people from assignments/issues | `StaticSearchPicker` | `personPickerOptions` workspace |
| raw | StaffArr person (advanced) | Advanced | `AdvancedReferenceField` | |
| select | Training definition | Known list | labeled `select` | |
| text | Compliance Core rule pack key | Rule pack list | `ControlledSelect` via `QualificationCheckPanel` | |

**Tests:** `ManualAssignmentPanel.test.tsx` — `getByLabelText(/StaffArr person/i)`.

---

## apps/trainarr-frontend/src/components/BatchQualificationCheckPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| textarea person IDs | StaffArr people | Multiple known keys | `CheckboxMultiSelect` | Replaces comma-separated paste for normal path |
| text | Qualification key | Business key | text | Qualification picker API **not yet** in V1 |
| select | Compliance Core rule pack key | Rule pack list | `ControlledSelect` | |

---

## apps/trainarr-frontend/src/components/AssignmentMaterialDemandPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| freetext UOM | Unit of measure | Shared vocabulary | `ControlledSelect` + `normalizeUom` | `MATERIAL_DEMAND_UOM_OPTIONS` |
| raw | SupplyArr part id (optional) | Part reference | text + advanced pattern | Mirror API pending full picker |

---

## apps/companion-frontend/src/components/FieldTaskInspectionPanel.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| unlabeled dynamic | Checklist answers | Grouped by prompt | `select` / `number` / `textarea` + `aria-labelledby` | Prompt element id per item |

---

## apps/stlcompliancesite/src/pages/DemoContactPage.tsx

| Field before | Applied label | Classification | New control | Notes |
|---|---|---|---|---|
| (already labeled) | Name, Work email, Organization, What would you like to see? | Marketing freetext | native inputs | No change required |

---

## Follow-up (documented gaps)

| Area | Gap | Recommended control |
|------|-----|-------------------|
| Supplyarr backorders/returns cancel | Single reason textarea | `PROCUREMENT_CANCEL_REASON_OPTIONS` + notes |
| Supplyarr emergency purchase | Emergency reason + override | `EMERGENCY_PURCHASE_REASON_OPTIONS` + notes |
| Warranty denial | Single textarea | `WARRANTY_DENIAL_REASON_OPTIONS` + notes |
| Trainarr qualification key | Free text | Async picker when qualification catalog endpoint exposed |
| Compliance Core M12 scope filters | Purchase request ID context | `AdvancedReferenceField` until cross-product PR picker |
| Platform audit export | Actor user ID GUID | Advanced only (no suite-wide user picker in V1) |
| Reason codes server-wide | Frontend constants in `controlledFormHelpers` | Align with Compliance Core vocabulary API when published |

---

## Test coverage summary (changed panels)

| File | Tests updated |
|------|----------------|
| `ManualAssignmentPanel.test.tsx` | `getByLabelText(/StaffArr person/i)` |
| `AuthorizationCheckOperationsPanel.test.tsx` | `getByLabelText(/StaffArr person/i)` |
| `StepBuilderPanel.test.tsx` | Generated key from name |
| `CreatePersonPanel.test.tsx` | Already label-based |
| `BatchRuleEvaluationPanel.test.tsx` | Pack/fact checkbox labels |
| `BulkDispatchPanel.test.tsx` | Advanced path (picker label tests optional) |
| `shared-ui/forms.components.test.tsx` | `GeneratedKeyField`, `CheckboxMultiSelect` |

Run per app: `npm run build` and `npm test` in each `apps/*-frontend`.

---

## Guardrails acknowledged

- Labels and pickers do **not** grant authority; APIs enforce tenant, entitlement, and permissions.
- No new UI dependencies; only `@stl/shared-ui` primitives.
- Bulk CSV, JSON rule authoring, and migration/advanced GUID entry preserved behind **Advanced** or dedicated panels.
