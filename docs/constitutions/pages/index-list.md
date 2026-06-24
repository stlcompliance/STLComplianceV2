# Index, List, Directory, Board, and Queue Page Constitution

## Audit drivers

The audit found overloaded tables/navigation, fixture-backed lists, stale/hard-coded references, and inconsistent empty/error states.

## Standard layout

1. page header with one primary action
2. one-sentence context only when needed
3. search and high-value filters
4. active-filter chips and saved views where justified
5. list/table/cards/board/queue
6. pagination or virtualized continuation
7. optional right preview drawer
8. designed page states

## Data truth

Rows are tenant- and permission-scoped, owner-backed, and durable. Fixture/demo rows may not appear in production. Stale, snapshot, partial, or degraded data is labeled.

## Density

Default columns contain only information needed to identify, prioritize, and choose an action. Secondary data belongs in a drawer/detail page or optional columns. Avoid walls of columns, nested text blocks, and action-icon clutter.

## Interaction

- Clicking the primary identity opens the canonical detail page or preview drawer consistently.
- Row menus contain secondary actions; the most common safe action may be inline.
- Bulk actions appear only when safely supported server-side.
- Filters and sort are shareable/canonical where operationally useful.
- Cross-product values use friendly owner-backed links, never free text or raw IDs.

## States

Implement loading/skeleton, initial empty, no results, forbidden, stale, degraded dependency, partial data, error, and retry. Empty state explains what belongs here and offers only a valid next action.

## Responsive behavior

Tables may become prioritized cards or controlled horizontal regions, but key identity/status/action remain visible. Mobile must not simply compress every desktop column.

## Definition of done

Prove tenant isolation, permission filtering, real persistence, filtering/sorting/pagination, row navigation, empty/no-results/degraded/error states, keyboard use, light/dark contrast, and realistic data volume.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
