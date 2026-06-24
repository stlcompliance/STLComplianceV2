# Settings and Preferences Page Constitution

## Separation

- User preferences apply to the signed-in person.
- Product preferences are visible only in the current product scope.
- Light/dark mode is a cross-product user preference.
- Tenant/product settings affect shared behavior and require permission.
- Platform settings are NexArr/platform-admin concerns.

Do not mix these scopes in one form.

## Layout

Settings use a full page with grouped, scannable sections. The topbar user menu links to Preferences; it is not the full settings editor. Avoid giant forms and unexplained internal keys.

## Values

Show effective value, source/default/inheritance, scope, validation, and consequences. Advanced technical values are gated and secondary.

## Save behavior

Use consistent save/cancel, dirty-state protection, concurrency/conflict handling, audit history, and per-section errors. Dangerous changes require shared confirmation with reason and effect.

## Theme

Every settings control, preview, modal, diff, and validation state works in light/dark. Raw JSON is not the default diff; show human-readable field changes with an optional advanced technical disclosure.

## Unified UI Regression Gate

A page is not complete because it renders. It is complete only when it uses the canonical app shell and approved shared page, action, form, table, drawer, feedback, and navigation primitives applicable to its archetype. Local clones that merely resemble shared components are regressions.

All styling must resolve through central semantic design tokens or shared component variants. Raw application colors, light-only surfaces, dark-only overrides, and component-local theme systems are prohibited. Background, text, border, icon, focus, hover, selected, disabled, destructive, warning, success, and overlay contrast must remain readable in both light and dark modes.

The page must avoid walls of text, overpopulated forms, excessive table columns, unexplained internal labels, and raw IDs outside explicitly technical administration surfaces. It must provide designed loading, empty, forbidden, not-found, validation, conflict, error, stale, partial, and degraded behavior whenever those states can occur.

Regression proof must include route-level behavior, keyboard and focus handling, representative desktop and compact-width rendering, light/dark visual coverage, permission denial, tenant isolation where data is present, and truthful server-confirmed success.
