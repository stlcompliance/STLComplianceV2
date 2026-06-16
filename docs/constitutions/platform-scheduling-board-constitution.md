# STL Compliance Platform Scheduling Board Constitution

## 1. Purpose

This constitution defines the shared Scheduling Board and Planning Board UX contract.

The board is a shared UI and interaction pattern for product-owned schedules. It is not a scheduling product.

## 2. Prime Directive

The board displays product-owned demand and scheduled execution. Every write goes through the owning product adapter and owning product API.

## 3. Required Views

The shared board must support:

- unscheduled drawer or backlog
- scheduled timeline, calendar, or board view
- resource lanes
- product filter
- site or location filter
- date range filter
- priority filter
- conflict drawer
- read-only details drawer
- product-owned edit launch link
- mobile read-only or simplified planning view

## 4. Scheduling Actions

The board may offer:

- drag/drop schedule
- drag/drop reschedule
- resize scheduled window when the product supports it
- unschedule
- cancel
- complete when the product supports it
- bulk schedule only when the owning product endpoint supports it
- keyboard-accessible scheduling actions

Unavailable actions must be hidden or disabled based on product-provided allowed actions and permission flags.

## 5. Display DTO

The board display item should support:

- product key
- item type
- item ID
- title and subtitle
- current status
- schedule status
- priority
- requested window
- promised window
- scheduled window
- customer reference when visible
- order reference when visible
- site and location references
- resource needs
- assigned resources
- blockers
- warnings
- source references
- owning product URL
- allowed actions
- permission flags
- stale or projection status

The DTO is display-only. It must not become a universal canonical schedule table.

## 6. Conflict Drawer

Conflicts must be grouped by:

- resource
- compliance
- qualification
- asset readiness
- location
- document or evidence
- order state
- missing facts
- permissions

The drawer must show plain-language reasons and product-owned available actions. It must not show raw JSON.

## 7. Product-Owned Adapters

Each adapter must declare:

- product key
- product display name
- supported item types
- whether schedule, reschedule, unschedule, cancel, complete, resize, and bulk schedule are supported
- endpoint paths or functions
- source/freshness behavior

Adapters must not call unrelated product write endpoints.

## 8. Accessibility And Mobile

Drag/drop must not be the only way to schedule. The board must provide keyboard-accessible schedule, reschedule, and unschedule actions when those actions are available.

Mobile may use a simplified read-only or action-sheet flow where dense drag/drop is not practical.

## 9. Minimum Acceptable Implementation

A shared scheduling board is minimally acceptable when it has:

1. Unscheduled and scheduled sections.
2. Resource lanes or resource summaries.
3. Product/source ownership visible.
4. Requested, promised, and scheduled windows displayed distinctly.
5. Owning product adapter writes.
6. Validation and conflict display.
7. Permission-aware actions.
8. Product detail links.
9. Loading, empty, error, and stale states.
10. No raw JSON.
