# Admin and Permission Surface Page Constitution

## Purpose

Admin pages may expose technical concepts that ordinary pages must hide, but they remain task-oriented and human-readable.

## Rules

- Human labels are primary; permission keys, scopes, IDs, and source products are secondary.
- Explain effect, scope, inherited/explicit state, risk, and affected users before change.
- Role names do not replace server-side permission checks.
- Platform-admin concepts stay in NexArr/Compliance Core studio and do not leak into ordinary tenant workflows.
- Compliance Core studio routes are server-side platform-admin-only.
- Product permission administration belongs in StaffArr/product authority workflows, not product-availability grants.

## Changes

Sensitive grants/removals require permission, structured reason, confirmation, concurrency protection, and immutable audit. Preview effective permissions and scope before save where feasible.

## Raw technical data

Raw payloads and IDs may be available through an explicit advanced/audit panel when necessary. They are never the primary explanation.

## Definition of done

Test inherited/explicit permissions, scope, denial, platform-admin separation, audit, stale edit, light/dark, keyboard/focus, and user-friendly labels.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
