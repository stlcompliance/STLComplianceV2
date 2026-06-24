# Unified UI Regression Verification

## Automated layers

1. **Static token audit:** reject raw colors, light-only utilities, product-local replicas of shared components, unapproved inline style values, and inaccessible focus removal.
2. **Component contract tests:** shared shell, page header, filters, tables, forms, drawers, dialogs, toasts, tabs, badges, empty/error states, print layout, and reference picker behavior.
3. **Route smoke matrix:** every product home and primary page archetype renders under light/dark, permission-limited, empty, realistic, and degraded fixtures.
4. **Visual regression:** stable viewport captures for desktop, narrow desktop/tablet, and mobile where supported. Ignore only genuinely nondeterministic regions.
5. **Accessibility regression:** keyboard traversal, focus restoration, dialog trapping, accessible names, heading order, table semantics, live status, and contrast.
6. **Behavior regression:** server failure cannot yield success; quick-create preserves parent work; drawer close warns on dirty state; product switch retains tenant; print omits shell.

## Required suite scenarios

- Every ordinary product appears in the launcher for an active tenant member.
- Compliance Core studio appears only for a platform administrator.
- A user with no useful permission can still open an ordinary product and sees an honest permission-limited landing state.
- Cross-product owner outage produces a named degraded state, not blank or stale-unlabeled data.
- Shared shell, topbar, sidebar, menus, overlays, drawers, forms, tables, charts, and toasts pass light/dark checks.
- No normal page exposes GUIDs, machine permission keys, internal roles such as `platform_admin`, raw JSON, environment/debug labels, or unnecessary linkage explanations.
- A failed create/edit/transition leaves user input intact and does not show a success toast.
- Quick create returns the newly created owner record to the originating picker without closing the parent workflow.
- Print output is a professional report layout, not the application viewport without navigation.

## Product-local additions

Each product adds domain scenarios: WMS scans and conflicts, financial approvals and closed periods, maintenance voice/manual fallback, mobile offline conflict, document quarantine/OCR review, report lineage/scheduling, quality holds/releases, dispatch feasibility, training assessments/signoffs, and other high-risk actions.

## Failure policy

A failing visual or behavior regression must be fixed or approved as an intentional design-system change with updated baselines across all affected products. Product-local exceptions are not accepted merely because the page remains readable to its author.
