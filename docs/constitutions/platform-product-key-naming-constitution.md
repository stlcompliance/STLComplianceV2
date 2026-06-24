# STL Compliance Product Key, Event, Permission, and Object Reference Constitution

## 1. Purpose

This constitution defines stable machine naming for STL Compliance products, events, permissions, API references, and human-readable object numbers.

The suite may display friendly product names, but integrations must use one canonical machine key for each product.

## 2. Scope

This constitution applies to:

- Product registry keys
- Service-token scopes
- API product references
- Event type prefixes
- Permission prefixes
- Cross-product references
- Object number prefixes
- Display names where they might be confused with machine keys

## 3. Prime directive

Display names are not machine keys.

Every product must have one canonical lowercase machine key. Events, permissions, service scopes, product references, and route metadata must use that key consistently.

## 4. Canonical product keys

The canonical machine keys are:

- `assurarr`
- `compliancecore`
- `customarr`
- `fieldcompanion`
- `ledgarr`
- `loadarr`
- `maintainarr`
- `nexarr`
- `ordarr`
- `recordarr`
- `reportarr`
- `routarr`
- `staffarr`
- `stlcompliancesite`
- `supplyarr`
- `trainarr`

Display names remain user-facing:

- AssurArr
- Compliance Core
- CustomArr
- Field Companion
- LedgArr
- LoadArr
- MaintainArr
- NexArr
- OrdArr
- RecordArr
- ReportArr
- RoutArr
- StaffArr
- STL Compliance Site
- SupplyArr
- TrainArr


## 4.1 Platform service namespaces

A platform capability that is not a user-facing product must not be added to the product registry merely to obtain an event prefix. The narrow Platform Reference Data service uses the namespace `platform.reference_data.*` for events/scopes and `/api/v1/reference-data/*` for APIs. It is not a launcher product key.

## 5. Event names

Cross-product event names must use:

```text
{productKey}.{domain_or_resource}.{past_tense_fact}
```

Examples:

- `fieldcompanion.mobile_task.created`
- `compliancecore.rulepack.activated`
- `ordarr.order.completed`
- `nexarr.product_launch.created`

Event names must not use product display names, spaces, underscores in product keys, or capitalized prefixes.

## 6. Permission names

Permission names must use:

```text
{productKey}.{domain}.{action}
```

Examples:

- `staffarr.people.read`
- `nexarr.platform_admin.manage`
- `fieldcompanion.mobile.use`
- `compliancecore.rulepacks.publish`

Product-local permissions may be assigned or displayed through StaffArr authority context, but the owning product remains responsible for evaluating permissions in its domain. NexArr remains final authority for platform admin, login, tenant membership, launch context, service-client, and service-token capabilities.

## 7. API and reference product keys

API payloads, reference envelopes, service-token scopes, route metadata, and product registries must use canonical product keys.

Good:

```json
{
  "productKey": "fieldcompanion",
  "sourceProduct": "compliancecore"
}
```

Bad:

```json
{
  "productKey": "FieldCompanion",
  "sourceProduct": "compliancecore"
}
```

## 8. Object prefixes

Short object prefixes are product-scoped unless a constitution explicitly declares them globally unique.

A globally meaningful object reference must include:

- Product key
- Object type
- Stable object ID
- Optional human-readable object number

Recommended shape:

```json
{
  "productKey": "recordarr",
  "objectType": "record_package",
  "objectId": "pkg_123",
  "objectNumber": "PKG-2026-00042"
}
```

Do not rely on a short object prefix such as `ACC`, `DOC`, `PKG`, or `LOG` to be unique across the suite.

## 9. Migration and compatibility

Old variants such as `FieldCompanion`, `field_companion`, and `compliance_core` may appear only in explicit migration notes or historical examples.

New docs, APIs, events, permissions, and service scopes must use the canonical product keys.

## 10. Anti-patterns

The following are not allowed:

- Mixing display names and machine keys in API payloads
- Using `FieldCompanion.` as an event or permission prefix
- Using `field_companion` or `compliance_core` as product keys
- Creating product-specific aliases without documenting them as legacy migration values
- Treating object number prefixes as globally unique by themselves

## 11. Minimum acceptable implementation

A product-facing contract is minimally acceptable when it:

1. Uses the canonical product key.
2. Uses lowercase product-key event prefixes.
3. Uses lowercase product-key permission prefixes.
4. Separates display name from machine key.
5. Includes product key and object type in cross-product references.
6. Treats short object prefixes as product-scoped.
