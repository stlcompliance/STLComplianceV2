# Cross-Product Reference Page Constitution

## Audit drivers

UX-004 found hard-coded StaffArr/SupplyArr options that looked integrated but were not live owner data.

## Prime directive

Users select business records; they do not type foreign IDs or recreate another product’s truth.

## Reference picker

A shared picker resolves against the owning product and returns stable owner ID, display name/code, status, relevant context, and optional snapshot fields. Search is tenant- and permission-scoped.

## Display

Show friendly names and meaningful codes as primary. Raw IDs are technical metadata only. Links open the owner record through the canonical route/handoff behavior.

## Snapshot and availability

Historical snapshots are allowed for audit/display but are labeled and never treated as editable foreign truth. If the owner is unavailable, show degraded state and any clearly dated snapshot; do not replace it with hard-coded options.

## Quick create

When safe and permissioned, offer owner-backed quick create. The new record is created by the owner, returned, and selected without abandoning current work.

## Merge/archive behavior

Owner merge, archive, deletion, or supersession must resolve predictably. Consumers retain historical references and show the current canonical target where appropriate.

## Definition of done

Test live search, permission scope, owner outage, stale snapshot, quick create, merged/archived reference, keyboard use, light/dark, and no free-text/ID fallback.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
