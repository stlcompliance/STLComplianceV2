# LedgArr Tenant Settings

LedgArr tenant settings are the tenant-scoped administrative surface for accounting behavior inside STL Compliance.

They control:

- general ledger behavior
- financial legal entity requirements
- chart of accounts defaults
- financial dimensions
- source-product packet posting rules
- AP and AR controls
- inventory and fixed-asset accounting
- tax, banking, intercompany, approvals, close, integrations, reporting, and evidence behavior

## Ownership boundary note

LedgArr legal entities are accounting and business entities.

They are not Compliance Core governing bodies.

Do not model FMCSA, OSHA, EPA, MSHA, or other regulators as LedgArr legal entities.

## Product boundary rules

- LedgArr owns financial legal entities, books, ledgers, accounting controls, and finance packet posting behavior.
- Compliance Core owns governing bodies, regulations, citations, and rule meaning.
- CustomArr owns customers.
- SupplyArr owns vendors and vendor master identity.
- StaffArr owns people, roles, permissions, departments, sites, and internal location hierarchy.
- MaintainArr owns assets and maintenance execution.
- LoadArr owns warehouse and inventory execution.
- RoutArr owns transportation execution.
- OrdArr owns orders and invoice-ready operational packets.
- RecordArr owns stored financial evidence and retained documents.

LedgArr settings may store stable references and display snapshots to records from other products, but LedgArr must not become the source of truth for those records.

## Routes and API surface

The canonical settings workspace is `/ledgarr/settings`.

Section-level API routes live under `/api/v1/ledgarr/settings`:

- `GET /api/v1/ledgarr/settings`
- `GET /api/v1/ledgarr/settings/{sectionKey}`
- `PUT /api/v1/ledgarr/settings/{sectionKey}`
- `POST /api/v1/ledgarr/settings/{sectionKey}/validate`
- `POST /api/v1/ledgarr/settings/{sectionKey}/reset`
- `GET /api/v1/ledgarr/settings/{sectionKey}/audit`
- `GET /api/v1/ledgarr/settings/options`
- `GET /api/v1/ledgarr/settings/posting-source-options`

## Permissions

Minimum LedgArr settings permissions:

- `ledgarr.settings.view`
- `ledgarr.settings.manage`
- `ledgarr.legalEntities.view`
- `ledgarr.legalEntities.manage`
- `ledgarr.chartOfAccounts.view`
- `ledgarr.chartOfAccounts.manage`
- `ledgarr.postingRules.view`
- `ledgarr.postingRules.manage`
- `ledgarr.periodClose.view`
- `ledgarr.periodClose.manage`
- `ledgarr.integrations.view`
- `ledgarr.integrations.manage`

Viewing the settings area requires `ledgarr.settings.view`.

Editing general settings requires `ledgarr.settings.manage`.

Specialized sections require both the general settings permission and the section-specific permission where applicable.

## Audit and high-impact changes

Every successful update and reset writes:

- tenant id
- section key
- actor person id
- timestamp
- before and after payloads
- diff payload
- optional reason
- correlation id when present

High-impact changes require a change reason before save.

## Packet validation integration

LedgArr finance-packet intake uses tenant settings to enforce:

- source-product posting enablement
- legal-entity requirements
- required dimension blocking
- external ERP mirror and disabled-mode holds
- manual adjustment evidence requirements

When a packet does not satisfy settings requirements, LedgArr places it in a validation-failed state instead of silently posting it.
