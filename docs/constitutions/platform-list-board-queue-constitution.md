# STL Compliance List, Board, Queue, and Search Result Constitution

## 1. Purpose

This constitution defines the everyday operational surfaces users use to find, triage, sort, filter, search, and act on records across STL Compliance.

Dashboards summarize the operating picture. Detail pages explain one record. Create pages make new records. Lists, boards, queues, and search results help users manage sets of records.

## 2. Scope

This constitution applies to:

- List pages
- Management tables
- Boards
- Queues
- Review tables
- Kanban-style views
- Search result pages
- Saved views
- Bulk action surfaces
- Mobile list/card views

## 3. Prime directive

A list or queue must help users find the right record and take the next valid action without hiding ownership, status, or risk.

Lists must not become ungoverned CRUD dumps.

## 4. Surface definitions

### List

A list is a general record-finding and management surface.

Examples:

- Assets
- People
- Suppliers
- Training programs
- Work orders
- Documents
- Rules

### Board

A board is an operational state view, usually grouped by status, lane, time, assignee, site, or priority.

Examples:

- Work order board
- Dispatch board
- Receiving board
- Training evaluation board
- CAPA board

### Queue

A queue is a triage surface for work requiring attention.

Examples:

- Incident review queue
- Import review queue
- Evidence gap queue
- Supplier approval queue
- Inventory exception queue

### Search result

A search result helps users find records across one product or multiple products.

## 5. List page structure

A standard list page should include:

- Page header
- Description/scope where helpful
- Primary action when permitted
- Search/filter command bar
- Saved view selector where useful
- Table or card list
- Bulk action area when applicable
- Empty/loading/error states
- Pagination or cursor loading

## 6. Required row/card fields

Operational rows/cards should show the fields needed for triage.

Common fields:

- Identifier
- Human-readable title/name
- Status
- Lifecycle category
- Owner/assignee/team where relevant
- Site/location where relevant
- Date/due/ETA/effective/expiration where relevant
- Severity/risk/priority where relevant
- Source product when cross-product
- Next action when relevant
- Drill-in link

Do not show raw IDs as the primary label unless the ID is operationally meaningful.

## 7. Source and ownership

Cross-product rows must show source product when ownership matters.

Examples:

- A RoutArr dispatch queue may show MaintainArr vehicle readiness as a read-only source signal.
- A StaffArr readiness queue may show TrainArr qualification state as a source signal.
- A SupplyArr purchase list may show LoadArr receiving state as a handoff/reference.

The current product must not expose edit actions for another product's canonical record unless it is an approved handoff/API action.

## 8. Search

Search should support plain language where possible and stable identifiers where known.

Search results should include:

- Source product
- Entity type
- Stable ID
- Display label
- Status
- Context
- Last updated/freshness when cross-product
- Canonical detail route

Cross-product search must not leak records across tenants or permissions.

## 9. Filters

Filters should be meaningful and product-specific.

Common filters:

- Status
- Lifecycle category
- Site/location
- Owner/assignee/team
- Date range
- Due/overdue
- Severity/risk
- Source product
- Archive state
- Review state
- Handoff state

Filters should be backed by product APIs or read models, not frontend-only guesses over unbounded data.

## 10. Saved views

Saved views are useful for repeated work.

A saved view should preserve:

- Filters
- Sort
- Columns/fields
- Density
- Grouping/lane where applicable
- Scope
- Owner: user/team/product default

Saved views must respect permission changes.

## 11. Sorting

Default sort should reflect operational urgency.

Examples:

- Work orders: blocked/safety critical, asset down, due date
- Training: overdue, expiring soon, required before duty
- Trips: exception, ETA risk, departure time
- Receiving: appointment time, blocked, supplier risk
- CAPA: criticality, due date, recurrence

## 12. Boards

Boards should group work by meaningful states.

Board lanes must map to product-owned status/lifecycle rules.

Boards must support:

- Clear lane labels
- Record cards with key fields
- Drag/drop only when the transition is valid and permission-gated
- Blocked states
- Empty lane states
- Mobile-safe fallback

Drag/drop must not bypass required workflow validation.

## 13. Queues

Queues should prioritize work requiring action.

A queue item should show:

- What needs review/action
- Why it matters
- Severity/priority
- Age/due time
- Source product
- Required next action
- Owner/queue
- Drill-in

Queues must not bury critical blockers under informational items.

## 14. Bulk actions

Bulk actions must be permission-gated and review-before-submit.

Bulk actions should show:

- Selected count
- Affected records
- Records that will be skipped
- Validation errors
- Cross-product effects
- Confirmation for destructive/state-changing actions

Bulk actions must be idempotent and auditable when state-changing.

## 15. Mobile behavior

Tables collapse into cards on mobile.

Mobile cards must show:

- Highest-value fields first
- Status
- Due/ETA/expiration where relevant
- Owner/assignee where relevant
- Next action
- Source product when cross-product

Filters should become sheets. Sort should become compact menus. Bulk actions should appear only when touch-safe.

## 16. Empty/loading/error states

Every list, board, queue, and search surface must define:

- Loading state
- Empty state
- No results after filters
- Permission-limited state
- Source unavailable state
- Error/retry state
- Archived/deleted/superseded state when relevant

Empty states should say whether the absence of records is good, bad, or neutral.

## 17. No raw JSON

Rows, cards, filters, queue items, and search results must not show raw JSON, database rows, internal payloads, service-token claims, or rule blobs to ordinary users.

## 18. Performance

List APIs must be paginated or cursor-based.

The frontend must not fetch unbounded datasets to filter locally.

Use read models or summary endpoints for heavy operational boards.

## 19. Anti-patterns

The following are not allowed:

- Generic CRUD tables with no operational meaning
- Full record editing inside dashboard/list rows
- Drag/drop workflow transitions without validation
- Free-text references to canonical records
- Hiding ownership/source on cross-product rows
- Unbounded list queries
- Bulk actions without review
- Mobile tables that require horizontal scrolling for normal use
- Raw JSON in row details

## 20. Minimum acceptable implementation

A list/board/queue is minimally acceptable when it has:

1. Clear purpose and owning product
2. Tenant- and permission-safe data
3. Search/filter/sort appropriate to the workflow
4. Status/lifecycle visibility
5. Canonical drill-in route
6. Loading/empty/error states
7. Mobile-safe behavior
8. No ownership ambiguity
9. No raw JSON
