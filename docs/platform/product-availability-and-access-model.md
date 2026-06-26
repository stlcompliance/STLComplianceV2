# Product Availability and Access Model

## Canonical rule

Product availability is nonvariable. Every active tenant member may launch every ordinary STL Compliance product. There are no tenant product subscriptions, product entitlements, per-user product grants, or product-launch role bundles.

The only UI exception is Compliance Core:

- The Compliance Core administrative studio is visible and accessible only to validated NexArr platform administrators.
- Compliance Core runtime operations are platform capabilities available to every tenant and user through product APIs, questionnaires, evaluations, evidence checks, normalized facts, readiness results, and other product-owned workflows.
- A non-platform-admin must never need direct Compliance Core UI access to receive Compliance Core behavior.

## Three separate questions

1. **May the person sign in and act for this tenant?** NexArr validates account and tenant membership.
2. **May the person perform this product action on this record?** The owning product evaluates StaffArr authority context, product permission, record scope, workflow state, and blockers.
3. **May the person open the Compliance Core administrative studio?** NexArr validates platform-admin status server-side.

Do not collapse these questions into a retired product-entitlement check.

## Product switcher

The product switcher lists all active ordinary products. It may show operational state such as available, degraded, maintenance, or temporarily unavailable, but may not hide a product because a tenant or user was not granted it.

When a user has no useful permissions inside a product, the product still opens to a clear permission-limited landing state that explains what is available and who can assign the required permissions or scope. The UI must not say the product is missing, unlicensed, not entitled, or unavailable to the tenant.

Compliance Core appears only for platform administrators. The public STL Compliance Site is not part of the tenant workspace product launcher.

## Authorization after launch

A successful launch establishes identity, tenant context, session context, and product destination. It does not grant domain authority. Every owning product must enforce its own permissions at the API boundary.

Platform-admin status does not silently bypass tenant-domain permissions. Support or break-glass actions require an explicit audited workflow and may not reuse ordinary product routes without scope and reason.

## Service access

Service-to-service calls use NexArr-issued service identity, tenant context, and narrow scopes. Services do not call a retired entitlement endpoint. The destination validates the service client, tenant, scope, requested action, and owner-specific rules.

## Prohibited legacy concepts

The following are removed from the canonical model:

- `ProductEntitlement`
- tenant product grants or revocations
- per-user product launch grants
- launch outcomes that treat ordinary products as tenant-granted or missing
- entitlement-based product-switcher filtering
- entitlement events and entitlement administration pages
- product APIs that introspect tenant product entitlement

Historical database tables or API fields using those names must be migrated or ignored, not re-described as current behavior.
