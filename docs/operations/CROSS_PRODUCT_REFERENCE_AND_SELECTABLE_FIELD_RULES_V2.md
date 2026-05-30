# STL Compliance / Arr Suite
# Cross-Product Reference Governance + Selectable Field Rules (V2)

Generated: 2026-05-30

## Core principle

Users work with friendly labels (pickers, dropdowns, typeaheads, scanners, and guided creation).

Users do not manually enter internal IDs, keys, statuses, rule identifiers, permission keys, law citation keys, product/entity references, or controlled vocabulary values.

If the UI provides search text entry, the committed value still must come from the owning product, Compliance Core, or a verified local mirror.

Example:

- User sees: `Driver Qualification File Required Documents`
- System stores: `docs.req.driverqualificationfile` or `docs.req.dqf`

## Absolute UI rule

If a field can change:

- who can do work
- what work is legal
- what asset is available
- what route can run
- what part can be purchased
- what training is required
- what law applies
- what permission is granted
- what incident type is recorded
- what document is required
- what compliance rule is evaluated
- what record is linked

then the value must be selected, generated, or system-populated.

It must not be free-typed.

## Internal key generation rule

System-generated keys are:

- predictable
- readable
- lowercase
- stable
- unique within tenant/product/domain
- not user-edited in normal workflows
- based on meaning (semantic), not random UUIDs when semantic keys are suitable

Recommended format:

`{domain}.{kind}.{slug}`

Examples:

| User-created title | Generated key |
|---|---|
| Driver Qualification File Required Documents | `docs.req.dqf` |
| Forklift Practical Evaluation | `train.eval.forklift.practical` |
| Annual Vehicle Inspection | `inspection.req.annualvehicle` |
| DOT Medical Card | `docs.req.dotmedicalcard` |
| Hazmat Endorsement Required | `cert.req.hazmatendorsement` |
| CDL Class A Required | `cert.req.cdl.classa` |
| Out of Service Defect | `defect.severity.outofservice` |
| Missing Signature | `evidence.issue.missingsignature` |
| Driver Qualification File | `docs.type.driverqualificationfile` |
| FMCSA Vehicle Maintenance Rule Pack | `rulepack.fmcsa.vehiclemaintenance` |

Collision behavior:

- First key: `docs.req.dqf`
- Next: `docs.req.dqf.2`
- Next: `docs.req.dqf.3`

## Semantic key prefixes

| Prefix | Meaning |
|---|---|
| `product.*` | Product registry key |
| `tenant.*` | Tenant classification |
| `person.*` | Person classification |
| `site.*` | Site/location classification |
| `dept.*` | Department classification |
| `pos.*` | Position classification |
| `team.*` | Team classification |
| `role.*` | Role classification |
| `perm.*` | Permission key |
| `docs.*` | Document/document requirement |
| `evidence.*` | Evidence requirement/type/issue |
| `rule.*` | Individual compliance rule |
| `rulepack.*` | Group of compliance rules |
| `law.*` | Law/regulatory citation |
| `gov.*` | Governing body |
| `train.*` | Training requirement/program/evaluation |
| `cert.*` | Certification/qualification |
| `incident.*` | Incident type/status/reason |
| `inspection.*` | Inspection requirement/template/result |
| `defect.*` | Defect type/severity/failure mode |
| `asset.*` | Asset type/category/status |
| `route.*` | Route/trip/dispatch status/type |
| `purchase.*` | Procurement status/type/reason |
| `vendor.*` | Vendor category/status |
| `customer.*` | Customer category/status |
| `material.*` | Material classification |
| `hazmat.*` | Hazard classification |
| `uom.*` | Unit of measure |
| `currency.*` | Currency key |
| `jurisdiction.*` | Regulatory jurisdiction |
| `status.*` | Shared status value |

## Universal reference rules

| Pattern | Treatment | Examples |
|---|---|---|
| `*Id` | Select/search/scan/system-populate | `personId`, `assetId`, `vendorId`, `routeId` |
| `*Key` | Select from registry or auto-generate | `ruleKey`, `materialKey`, `hazardClassKey` |
| `*StatusKey` | Select from status registry | `tripStatusKey`, `readinessStatusKey` |
| `*TypeKey` | Select from type registry | `incidentTypeKey`, `evaluationTypeKey` |
| `*CategoryKey` | Select from category registry | `assetCategoryKey`, `partCategoryKey` |
| `*SeverityKey` | Select from Compliance Core | `defectSeverityKey` |
| `*ReasonKey` | Select from reason registry | `overrideReasonKey`, `failureReasonKey` |
| `sourceProduct` | Select from product registry | `staffarr`, `maintainarr`, `routarr` |
| `sourceEntity` | Select from product entity registry | `work_order`, `trip`, `inspection` |
| `sourceId` | Select/search owning record | Canonical source record ID |

## Fields that may be free-typed

| Field type | Examples | Rule |
|---|---|---|
| Notes | `notes`, `vendorNotes`, `repairNotes` | Allowed |
| Descriptions | `description`, `defectDescription`, `exceptionDescription` | Allowed |
| Comments | `reviewComment`, `driverComment`, `evaluatorComment` | Allowed |
| Narratives | `incidentNarrative`, `correctiveActionNarrative` | Allowed |
| Instructions | `stopInstructions`, `trainingInstructions` | Allowed |
| Evidence text | `evidenceDescription`, `finding` | Allowed |
| External numbers | `invoiceNumber`, `quoteNumber`, `vendorReferenceNumber` | Allowed (externally owned) |
| Temporary display text | `customItemDescription` | Allowed only until canonical record exists |

## Canonical stored reference shape

```ts
type SelectableReference = {
  tenantId: string

  sourceProduct:
    | 'nexarr'
    | 'staffarr'
    | 'trainarr'
    | 'maintainarr'
    | 'routarr'
    | 'supplyarr'
    | 'compliancecore'

  sourceEntity: string
  sourceId: string

  labelSnapshot: string
  statusSnapshot?: string

  selectedAt: string
  selectedByPersonId: string

  lastVerifiedAt?: string
  lastSyncedAt?: string

  isAuthoritative: false
}
```

## Canonical generated key shape

```ts
type GeneratedSemanticKey = {
  tenantId?: string
  productKey: string
  domain: string
  kind: string
  title: string
  generatedKey: string
  slug: string

  createdAt: string
  createdByPersonId: string

  isSystemGenerated: true
  isUserEditable: false
  isDeprecated?: boolean
  replacedByKey?: string
}
```

## Cross-product flow reference requirements

| Flow | Source | Target(s) | Required references |
|---|---|---|---|
| Person created | StaffArr | NexArr, TrainArr | `tenantId`, `personId`, `siteId`, `departmentId`, `positionId`, `teamId` |
| Training assigned | TrainArr | StaffArr | `personId`, `trainingProgramId`, `certificationId`, `positionId`, `siteId` |
| Training completed | TrainArr | StaffArr | `trainingAssignmentId`, `personId`, `certificationId`, `qualificationId`, `trainerPersonId`, `signoffPersonId` |
| Incident reported | Any | StaffArr | `incidentId`, `sourceProduct`, `sourceEntity`, `sourceId`, `personId`, `siteId` |
| Vehicle dispatch | RoutArr | StaffArr, TrainArr, MaintainArr | `driverPersonId`, `vehicleAssetId`, `trailerAssetId`, `readinessStatusKey`, `requiredCertificationId`, `routeId`, `tripId` |
| Work order assignment | MaintainArr | StaffArr, TrainArr | `workOrderId`, `assetId`, `technicianPersonId`, `teamId`, `requiredCertificationId` |
| Parts request from work order | MaintainArr | SupplyArr | `workOrderId`, `assetId`, `partId`, `materialKey`, `purchaseRequestId` |
| Purchase approval | SupplyArr | StaffArr | `purchaseRequestId`, `purchaseOrderId`, `requestedByPersonId`, `approvedByPersonId`, `vendorId`, `partId` |
| Compliance evaluation | Any | Compliance Core | `ruleKey`, `rulePackId`, `lawCitationKey`, `materialKey`, `personId`, `assetId`, `certificationId`, `inspectionId` |
| Manual certification override | StaffArr | TrainArr, Compliance Core | `manualOverrideId`, `personId`, `certificationId`, `overridePersonId`, `overrideReasonKey` |

## Backend generation pseudocode

```ts
function generateSemanticKey(input) {
  const title = input.title
  const domain = input.domain
  const kind = input.kind
  const aliases = input.aliases

  const normalizedTitle = normalize(title)
  const slug = removeStopWords(normalizedTitle)
  const compactSlug = slug.replaceAll('-', '')

  const preferredAlias = chooseKnownAlias(title, aliases)

  let candidate
  if (preferredAlias) {
    candidate = `${domain}.${kind}.${preferredAlias}`
  } else {
    candidate = `${domain}.${kind}.${compactSlug}`
  }

  candidate = candidate.toLowerCase()
  if (!exists(candidate)) return candidate
  return makeUnique(candidate)
}
```

Known aliases:

- `Driver Qualification File` -> `dqf`
- `Safety Data Sheet` -> `sds`
- `Commercial Driver License` -> `cdl`
- `Department of Transportation` -> `dot`
- `Federal Motor Carrier Safety Administration` -> `fmcsa`
- `Occupational Safety and Health Administration` -> `osha`
- `Mine Safety and Health Administration` -> `msha`
- `Environmental Protection Agency` -> `epa`
- `Preventive Maintenance` -> `pm`
- `Personal Protective Equipment` -> `ppe`
- `Lockout Tagout` -> `loto`
- `Powered Industrial Truck` -> `pit`

## Enforcement checklist

For every cross-product field:

1. Use searchable select/combobox/typeahead/scanner/modal picker.
2. Show friendly label; store canonical ID/key.
3. Generate semantic keys automatically on controlled record creation.
4. Store display snapshot only for audit readability.
5. Revalidate references server-side before save.
6. Filter options by tenant, entitlement, permission, active status, and owner rules.
7. Never trust frontend labels.
8. Never require manual raw ID/key entry.
9. Never require users to invent rule keys, doc keys, status keys, category keys, or citation keys.
10. Block or warn when selected references are stale, missing, inactive, unauthorized, or blocked.

## One-line rule

Users create and select meaningful business objects by name.

The system generates, stores, validates, and links IDs/keys behind the scenes.
