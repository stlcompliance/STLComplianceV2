# STL Compliance Product Availability and Compliance Core Access Constitution

## 1. Purpose

This constitution removes product entitlement as an access concept and defines the one exceptional UI boundary for Compliance Core.

## 2. Prime directive

Every active tenant member can launch every ordinary STL Compliance product. Product availability is a platform constant, not tenant configuration, billing state, role assignment, feature flag, or product grant.

The Compliance Core administrative UI is platform-admin-only. Compliance Core runtime operation is available platform-wide through the products and services that invoke it.

## 3. NexArr responsibilities

NexArr owns:

- identity and account status
- tenant identity and active tenant membership
- session and launch context
- product registry and destination metadata
- service identity and token scopes
- platform-admin status
- the server-side Compliance Core studio gate

NexArr does not own product entitlements, per-product tenant access, or per-user product launch grants.

## 4. Product responsibilities

Each product owns authorization for its domain. A product API evaluates:

- authenticated identity or service client
- active tenant context
- StaffArr authority projection where applicable
- product permission key
- record/site/location/customer/supplier scope
- workflow state and blocker rules
- required approval, reason, and evidence

A launch token is context, not authorization for domain actions.

## 5. Product switcher

The switcher must:

- show all active ordinary product destinations
- use one consistent order and display name catalog
- show degraded or maintenance state without pretending the product is unlicensed
- show Compliance Core only to validated platform administrators
- never display “no entitled products,” “request product access,” or similar language

## 6. Compliance Core runtime

All products may use Compliance Core runtime contracts when needed. Runtime authorization is based on validated tenant/user/service context and the calling product’s permitted workflow, not direct studio access.

Examples include:

- tenant and record questionnaires
- fact normalization
- applicability evaluation
- evidence-requirement evaluation
- readiness and blocker results
- rule citation resolution
- theoretical situation evaluation

Ordinary users see plain-language results in their current product. They do not need access to rule-authoring, catalog-maintenance, mapping, import, or activation screens.

## 7. Platform administrators

Only platform administrators may access the Compliance Core administrative studio. Server-side validation is mandatory on every studio route and administrative API. Hiding a navigation item is not sufficient.

Platform-admin status alone does not grant ordinary tenant-domain actions. Any support impersonation or break-glass action must be explicit, time-limited, reasoned, and audited.

## 8. Migration requirements

Remove or deprecate:

- entitlement entities and endpoints
- entitlement checks in launch and handoff
- entitlement event types
- entitlement management pages
- entitlement language in user help
- tests that expect products to be hidden by tenant/user grants

Replace them with membership, local permission, product operational-state, and Compliance Core studio-gate tests.

## 9. Required regression proof

Tests must prove:

1. Every active tenant member receives every ordinary product in the launcher.
2. Product actions remain permission-gated after launch.
3. Non-platform-admins receive 403 from every Compliance Core studio/admin route.
4. The same non-platform-admin can receive Compliance Core runtime results through an authorized product workflow.
5. Platform-admin status does not bypass tenant-domain permission checks.
