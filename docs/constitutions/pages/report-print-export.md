# Report, Print, and Export Page Constitution

## Audit drivers

The suite requires professional report output rather than printing the current app page without its shell. ReportArr audit findings also require durable definitions, schedules, outputs, and lineage.

## Print presentation

Print/preview removes the app shell and renders a professional, mostly text report with title, tenant/context, reporting period, filters, generated timestamp, source/freshness, page numbers, readable tables, sensible page breaks, and signature/approval blocks where required.

Light/dark preference does not create a dark printed document unless explicitly requested.

## Report truth

Every metric/table declares source product(s), as-of/freshness, filters, permission scope, and definition/version. ReportArr presents and analyzes; it does not mutate source truth.

## Runs and schedules

Definitions are versioned separately from runs. Runs, schedules, recipients, status, retries, and outputs are durable and tenant-scoped. Generated evidence-grade output is stored through RecordArr with hash and lineage.

## Export security

Exports enforce the same row/column/record permissions as the page, log actor and criteria, and avoid leaking hidden columns or foreign tenant data.

## States

Show queued, running, complete, partial, failed, canceled, expired, and unavailable-source states. A downloaded file is never offered before the run completes and access is revalidated.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
