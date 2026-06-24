# Empty, Loading, Error, and Degraded Page State Constitution

## Audit drivers

CQ-005, FUNC-001, UX-005, and UI-004 showed false success, blank/ad hoc states, and inconsistent recovery.

## Required states

Every page and major independent section considers:

- loading
- initial empty
- filtered no-results
- no permission
- not found/deleted/archived
- validation failure
- conflict/stale edit
- dependency unavailable
- stale snapshot
- partial data
- offline/pending sync
- unexpected error

## Behavior

Loading does not erase stable prior data unnecessarily. Empty states explain the record/work expected and offer a valid action. No-results preserves filters. Permission states do not imply missing data. Degraded states name the unavailable source in user language and explain what remains usable.

A write failure states what was not saved and preserves recoverable input. A retry is shown only when idempotent/safe. Correlation details are expandable and secondary.

## Shared presentation

Use shared `PageState`, `SectionState`, `InlineError`, `PermissionDenied`, `DependencyUnavailable`, `NoResults`, and equivalent patterns. Products supply domain copy and actions, not new state layouts.

## Accessibility and theme

States use headings/landmarks/live regions appropriately, do not rely on color alone, retain keyboard focus, and remain readable in light/dark.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
