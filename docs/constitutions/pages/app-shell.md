# App Shell Page Constitution

## Audit drivers

NAV-001 through NAV-005, UI-001 through UI-004, and UX-003 exposed navigation drift, hard-coded themes, duplicate design systems, and internal labels.

## Canonical structure

Every product uses the shared suite shell:

1. suite/product switcher
2. current tenant context
3. current product identity and grouped product navigation
4. topbar with search/notifications/help/user menu
5. breadcrumb and page header
6. main content
7. optional right contextual drawer
8. shared toast/status region

Product identity and domain navigation change; shell behavior, spacing, interaction, accessibility, and responsive collapse do not.

## Product availability

The switcher lists all active ordinary products for every active tenant member. It does not filter by product permission. Compliance Core is shown only to platform administrators. Product permissions govern actions after launch.

## Navigation rules

- Sidebars contain durable destinations, not every action.
- Groups remain scannable and workflow-oriented.
- Active route, breadcrumb, page title, tenant, and product are always clear.
- Mobile uses the same information architecture through drawer/bottom/task navigation.
- No critical action depends on hover.
- Route aliases redirect to canonical URLs and do not create duplicate active states.

## Visual rules

The shell uses semantic tokens and shared components. Topbar, sidebar, menus, hover/selected/focus/disabled states, dividers, overlays, drawers, and toasts must be readable in light and dark.

## Content rules

Ordinary users do not see raw role keys, `platform_admin`, GUIDs, environment labels, linkage explanations, or developer hints unless the information is necessary to complete an explicit admin task.

## Definition of done

Test tenant/product switching, permission-limited landing, Compliance Core visibility, keyboard navigation, mobile collapse, light/dark states, route/breadcrumb consistency, and degraded product destination behavior.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
