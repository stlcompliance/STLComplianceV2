# STL Compliance Theme Token and Shared Component Enforcement Constitution

## 1. Audit drivers

UI-001 found 71 hard-coded/light-only color violations. UI-003 found product-local CSS recreating a second design system.

## 2. Prime directive

Every product uses the shared semantic theme and shared interaction primitives. Light and dark are equal application states, not a default plus patches.

## 3. Token rule

Raw hex/rgb/hsl values and palette-specific utility colors are prohibited outside approved token/brand/visualization files. Components use semantic surface, text, border, action, focus, selection, and status tokens.

## 4. Shared component rule

Products must prefer shared shell, page header, action bar, filter bar, table/list, badge, form field, reference picker, quick create, drawer, dialog, toast, page state, print, and scheduling primitives. Product-local CSS may express domain visualization, not reinvent basic UI chrome.

## 5. State coverage

Every component and page must be readable in both themes for normal, hover, focus, active, selected, disabled, loading, empty, validation, error, warning, success, stale, degraded, and print states.

## 6. Contrast and status

Status may not rely on color alone. Text/icon/shape accompany status. Focus indicators remain visible on all surfaces. Disabled content remains legible without appearing actionable.

## 7. Enforcement

- theme audit is a mandatory CI gate
- shared component fixtures render both themes and all states
- product shell and critical workflows receive screenshot comparisons
- documented brand exceptions use semantic tokens and audit annotations

## 8. Definition of done

A page cannot be accepted based on one screenshot or one theme. Review evidence must include light/dark, responsive widths, modal/drawer/table/form states, and print when applicable.
