# Unified Documentation Merge Changelog — 2026-06-23

## What changed

- Merged the former version-split platform and workflow documentation into one canonical, version-neutral tree.
- Removed the product-entitlement access model from current platform, product, API, route, help, and user guidance.
- Defined all ordinary products as available to every active tenant member.
- Restricted only the Compliance Core administrative studio to server-validated platform administrators.
- Preserved Compliance Core runtime evaluation for all tenants through authorized product workflows and service APIs.
- Converted ReferenceDataCore from a user-facing catch-all product into the narrow Platform Reference Data service and platform-admin utility.
- Added audit-regression constitutions for durability, tenant scope, endpoint authorization, actor attribution, browser sessions, fixture/no-op boundaries, concurrency/idempotency, uploads, CI, theming, navigation, and truthful errors.
- Added unified page-archetype constitutions for shells, lists, details, create/edit, drawers, workflows, page states, cross-product references, reports/printing, settings, and admin surfaces.
- Added product-specific production-safety and high-value capability documents across the suite.
- Updated the route map, event catalog, implementation sequence, ownership/access model, and shared UI rules.

## Removed concepts

Current implementations and docs must not introduce:

- tenant product subscriptions or product grants
- per-user product launch grants
- product-switcher filtering by product access grant
- product-access grant endpoints, events, or administration pages
- production workflows backed only by process-local mutable stores
- success UI before durable server confirmation
- tenant IDs supplied as trusted request data
- page-local hard-coded light/dark colors where semantic tokens exist
- a catch-all shared-data product that absorbs domain ownership

## Upgrade action

Drop this `docs/` directory over the repository documentation tree, remove superseded version folders and renamed legacy access guides, then run the documentation link and terminology checks described in `DOCUMENTATION_VALIDATION.md`.
