# Worker 149 — Companion product switcher (M11)

## Scope

Entitlement-aware product switcher in `companion-frontend` using `@stl/shared-ui` `ProductSwitcher`, `productCatalog`, and `productLaunchUrls`. Switching products calls NexArr launch context + handoff APIs and redirects to the target product `/launch` URL.

## Shared UI

- `ProductSwitcher` accepts optional `onSelectProduct`, `isPending`, and `errorMessage` for handoff-driven navigation (buttons instead of direct anchors).
- `ProductAppShell` / `ProductWorkspaceFrame` pass through launch handler props.

## Companion

- `GET /api/launch/context` and `POST /api/launch/handoff` via companion session token.
- `useCompanionProductLaunch` builds callback URLs from `buildProductLaunchUrlMap(import.meta.env)`.
- `ProductWorkspaceLayout` wires entitlements from `GET /api/companion/me` into the top bar switcher.

## Tests

- `ProductSwitcher.test.tsx` — handoff callback mode
- `productLaunch.test.ts`, `ProductWorkspaceLayout.test.tsx`
- `companion-product-switcher.spec.ts` (`E2E_LIVE`)
- `StlE2ePlaywrightSpecCatalog.CompanionProductSwitcherSpec`

## Boundaries

NexArr remains launch authority; companion UI never grants entitlements client-side beyond displaying `me.entitlements` and server launch denial codes.
