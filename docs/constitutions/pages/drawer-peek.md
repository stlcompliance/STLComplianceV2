# Drawer, Peek, and Quick-Create Page Constitution

## Purpose

Drawers preserve context while users preview records, complete bounded edits, or create a missing reference.

## Rules

- A drawer does not replace the full detail page for a primary record.
- Header, title, status, close, actions, width behavior, focus trap, return focus, and dirty-state protection use shared primitives.
- Errors remain visible and input is preserved.
- Nested drawers are avoided; use an in-drawer step or promote to a full page.
- Drawers work in light/dark and responsive sheet mode.

## Preview drawer

Show identity, status, key decision information, highest-value related facts, and a clear link to full details. Do not reproduce every detail tab.

## Quick create

Quick create is used when the user needs a missing reference to finish the current task. It:

- calls the owning product
- collects only the minimum valid fields
- respects permission and tenant boundaries
- creates no local shadow record
- returns the new owner-backed reference
- selects it automatically in the originating form
- permits later backfill through the owner’s full detail/edit page

## Save truth

Close or success occurs only after durable owner confirmation. Pending/offline create is explicitly labeled and may not be treated as a selectable final reference until the owner accepts it.

## Definition of done

Test focus, escape/close, dirty confirmation, API failure preservation, permission denial, quick-create return selection, mobile sheet behavior, light/dark, and deep-link escape to full details.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
