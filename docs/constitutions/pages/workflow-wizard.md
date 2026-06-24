# Workflow Wizard Page Constitution

## Purpose

Use a wizard only when order, branching, review, or durable checkpoints materially reduce error. Do not turn every long form into ceremonial steps.

## Structure

- clear workflow title and subject
- progress and current step
- one task-focused step at a time
- concise explanation and validation
- safe back/forward behavior
- owner-backed references and quick create
- final review of records, effects, approvals, and evidence
- durable completion result and next actions

## State ownership

The server owns workflow state and completion. Client drafts may preserve work but cannot create final status. Each retryable transition uses idempotency and concurrency rules.

## Cross-product effects

Before commit, show which owner records will be created/updated and which handoffs/tasks will follow. Do not imply atomic cross-database completion when downstream work is pending; show accepted/pending/failed states truthfully.

## Minimal create and backfill

Collect the minimum needed to proceed. Optional/backfill information moves to later sections or a follow-up task rather than overcrowding the current step.

## Recovery

Users can resume safely when the workflow supports it. Conflicts, expired sessions, unavailable owners, rejected evidence, and validation failures preserve completed work and explain the next step.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
