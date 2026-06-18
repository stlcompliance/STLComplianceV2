# RoutArr - Tenant Settings Model

RoutArr tenant settings are the first-class configuration surface for transportation operations. They control dispatch behavior, demand intake, trip planning, tendering, status transitions, visibility, document handling, closeout, and integration behavior inside RoutArr.

RoutArr owns these settings because they govern transportation execution. Settings may reference other products, but they must not make RoutArr the source of truth for people, assets, locations, customers, carriers, documents, compliance meaning, or finance execution.

## Settings aggregate

```text
RoutArrTenantSettings
- tenantId
- version
- createdAt / createdByPersonId
- updatedAt / updatedByPersonId
- values
- listItems
- overrides
```

Settings are stored as typed rows, not raw JSON. Scalar values use boolean, integer, decimal, text, enum, time, and duration columns. Multi-select values use child list-item rows.

## Setting groups

```text
- general
- dispatchBoard
- demand
- planning
- tendering
- numbering
- assignment
- hosAvailability
- stopsAppointments
- dockYardHandoffs
- trackingVisibility
- exceptions
- detentionAccessorials
- rating
- documents
- portal
- notifications
- statusModel
- integrations
- overridesApprovals
- closeout
```

## Ownership boundaries

RoutArr settings may contain cross-product reference snapshots only when the source product is explicit:

```text
StaffArr
- person, site, terminal, dock, and internal location references

MaintainArr
- vehicle, trailer, asset, and readiness integration behavior

TrainArr
- driver and route qualification dependency behavior

Compliance Core
- HOS and compliance interpretation dependency behavior

LoadArr
- appointment, dock, yard, and receiving handoff behavior

SupplyArr
- carrier reference and tender integration behavior

CustomArr
- customer reference and portal visibility behavior

RecordArr
- document packet handoff behavior
```

RoutArr settings must not store internal database IDs or free-text canonical references. UI surfaces should show display snapshots and stable references only after a picker or provider supplies the source product, entity type, stable ID, display label, status, and snapshot timestamp.

## Effective settings

Effective settings start with platform defaults, then tenant values, then matching scoped overrides.

```text
tenant default
site
terminal
customer
carrier
lane
route type / service type
demand
trip
emergency override
```

Scoped overrides must carry a reason, actor, version, affected scope snapshot, and audit entry. Emergency overrides are explicit and always auditable.

## API surface

```text
GET    /api/v1/tenant-settings/effective
POST   /api/v1/tenant-settings/preview
GET    /api/v1/tenant-settings/editable
GET    /api/v1/tenant-settings/options
POST   /api/v1/tenant-settings/validate
PUT    /api/v1/tenant-settings/groups/{settingGroup}
POST   /api/v1/tenant-settings/groups/{settingGroup}/reset
GET    /api/v1/tenant-settings/audit
POST   /api/v1/tenant-settings/overrides
PUT    /api/v1/tenant-settings/overrides/{overrideKey}
DELETE /api/v1/tenant-settings/overrides/{overrideKey}
```

The effective endpoint is available to entitled RoutArr users. Editable, validation, reset, audit, preview, integration, and override operations require admin-gated RoutArr settings permissions.

## Validation rules

Settings validation must reject unsafe combinations before save. Examples include:

```text
- auto tender requires routing guide behavior
- require rate before tender requires rating or rating integration
- geofence auto-complete requires visibility and a geofence radius
- exact customer sharing requires customer visibility
- auto close cannot run while dispatcher review is required
- dock appointment gating requires a LoadArr handoff or RoutArr dock request path
- unqualified-driver allowances require explicit override reason audit
- RecordArr document packet handoff requires RecordArr integration
- MaintainArr readiness blocking requires MaintainArr integration
- HOS blocking/review requires Compliance Core integration
```

## Audit and events

Every update, reset, override create/update/delete, and validation failure is auditable. Audit entries record changed keys, actor person reference, previous and new version, summary, reason, and affected scope when present.

RoutArr emits tenant-scoped outbox events with no secrets:

```text
routarr.tenant_settings.updated
routarr.tenant_settings.reset
routarr.tenant_setting_override.created
routarr.tenant_setting_override.updated
routarr.tenant_setting_override.deleted
routarr.tenant_settings.validation_failed
```

## UI contract

The canonical settings UI is the RoutArr settings section at `/routarr/settings`. It should render the 21 groups as operational settings sections, save one group at a time with `expectedVersion`, show validation results before save, expose audit history, and preview effective settings against saved scoped override snapshots.

The UI must not expose raw internal IDs, raw JSON editing, or hidden bypass controls.
