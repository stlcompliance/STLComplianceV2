# Dashboard Page Constitution

## 1. Purpose

Dashboard pages are operational command surfaces.

A dashboard exists to show the current operating picture for a product, expose risk, surface blockers, and guide the user toward the next useful action. It is not a marketing page, generic analytics page, settings page, or dumping ground for every metric the product can calculate.

Every dashboard must answer, at a glance:

1. What is happening right now?
2. What is healthy?
3. What is at risk?
4. What needs attention?
5. What can the user do next?

## 2. Scope

This constitution governs the main content area of dashboard pages only.

It applies to product dashboard routes such as:

- `/app/staffarr`
- `/app/routarr`
- `/app/loadarr`
- `/app/supplyarr`
- `/app/maintainarr`
- `/app/trainarr`
- `/app/reportarr`
- `/app/recordarr`
- `/app/compliancecore`

This constitution does not define sidebar, topbar, global navigation, authentication shell, tenant picker, product launcher, or platform chrome rules. Those belong to the shared shell and NexArr/platform UI constitutions.

## 3. Authority

Dashboard implementations must follow this constitution unless a more specific product constitution explicitly overrides it.

Product ownership constitutions remain authoritative for data ownership. A dashboard may display cross-product readiness signals, but it must not assume ownership of another product’s records, workflows, or canonical identifiers.

Operational dashboards are not report artifacts by default. They are exempt from the reporting constitution's report-specific rules unless the dashboard is explicitly implemented as a report-backed surface, scheduled dashboard export, or other ReportArr-owned deliverable.

For example:

- RoutArr may show vehicle readiness from MaintainArr, but RoutArr does not own vehicle inspection clearance.
- LoadArr may show dock handoff state from RoutArr or StaffArr locations, but LoadArr does not own dispatch execution.
- StaffArr may show training readiness from TrainArr, but StaffArr does not own training program completion.
- SupplyArr may show receiving or shipment state from LoadArr or RoutArr, but SupplyArr does not own transportation execution.

Dashboards may summarize external signals. Ownership remains with the source product.

## 4. Dashboard Design Principle

A dashboard must be built around operational usefulness, not visual symmetry alone.

The page should prioritize high-signal information over decorative density. Every card, chart, table, and callout must have a reason to exist.

A dashboard section is valid only if it supports at least one of these outcomes:

- Detect a problem
- Confirm readiness
- Track execution
- Reveal workload
- Explain risk
- Trigger a next action
- Provide situational awareness
- Link to a deeper workflow

## 5. Main Content Structure

Each dashboard main content area should use the following structure unless the product has a justified domain-specific reason to differ:

1. Page header
2. Primary KPI strip
3. Primary operational view
4. Attention or risk panel
5. Execution table or work queue
6. Readiness, blockers, or compliance panel
7. Secondary cards for upcoming work, handoffs, activity, alerts, or quick actions
8. Dashboard scope note where cross-product signals are shown

The layout may vary by product, but the information hierarchy must remain clear.

The most important operational picture belongs above the fold. The user should not have to scroll to discover whether the product is healthy or at risk.

## 6. Page Header

Each dashboard must have a clear page title.

The header should include:

- Product or domain-specific title
- One-sentence operational summary
- Date, date range, or live-status context when relevant
- Primary dashboard action when appropriate
- Filters only when they materially change the displayed operating picture

Examples of valid dashboard titles:

- `Dispatch dashboard`
- `StaffArr Dashboard`
- `Load dashboard`
- `Supply dashboard`
- `Maintenance readiness`
- `Training readiness`

Avoid vague titles such as:

- `Overview`
- `Home`
- `Analytics`
- `Summary`

unless paired with product-specific context.

## 7. KPI Strip

The first metric row must contain the highest-signal operating indicators for the product.

KPI cards must be limited to metrics that are:

- Immediately meaningful
- Timely
- Actionable or status-bearing
- Connected to product ownership
- Useful for triage

KPI cards should not be used for vanity statistics.

Each KPI card should include:

- Metric label
- Current value
- Status, delta, threshold, or contextual note
- Optional icon
- Optional severity indicator
- Optional drill-in link or click behavior

A KPI without context is incomplete unless the value is self-explanatory.

Bad:

- `128`
- `94%`
- `$285,430`

Better:

- `Total Employees: 128, up 5 since last week`
- `Coverage: 94%, up 3% vs last week`
- `Total Spend: $285,430, up 8.3% vs prior period`

## 8. KPI Selection by Product

Each product dashboard should select KPIs based on its own domain.

### StaffArr

StaffArr dashboards should prioritize:

- Total people
- Ready for duty
- Restricted
- Not ready
- Open incidents
- Certifications or credentials expiring
- Coverage by team, site, department, or role
- Pending personnel actions

StaffArr must not treat training completion as locally owned when TrainArr owns the canonical training state.

### RoutArr

RoutArr dashboards should prioritize:

- Active routes
- On-time window
- HOS remaining
- Equipment utilization
- Exceptions open
- Load handoffs
- Route delays
- Dispatch release blockers

RoutArr owns dispatch, route planning, trip execution, assignment, stop management, ETA, and transportation exceptions.

### LoadArr

LoadArr dashboards should prioritize:

- Active loads
- In-transit loads
- Delivered loads
- Pending loads
- Receiving status
- Putaway or staging workload
- Inventory movement risk
- Dock or handoff readiness where relevant

LoadArr owns WMS behavior, receiving workflow, putaway, reservations, picks, stock ledger, inventory balances, and warehouse execution signals.

### SupplyArr

SupplyArr dashboards should prioritize:

- Total orders
- Total spend
- Active shipments or order fulfillment state
- Supplier risk
- Purchasing blockers
- Vendor/customer document gaps
- Procurement exceptions
- Lead time or price variance

SupplyArr owns vendors, customers, external parties, purchasing expectations, procurement records, pricing snapshots, and supplier-facing documents.

### MaintainArr

MaintainArr dashboards should prioritize:

- Asset readiness
- Open defects
- PM due or overdue
- Inspection completion
- Work orders open
- Critical assets down
- Parts demand linked to maintenance work
- Maintenance blockers

MaintainArr owns assets, components, PMs, inspections, repairs, defects, work orders, and asset readiness.

### TrainArr

TrainArr dashboards should prioritize:

- Training assignments
- Certifications issued
- Expiring qualifications
- Overdue training
- Remediation required
- Evaluation bottlenecks
- Program completion
- Trainer signoff workload

TrainArr owns training definitions, training assignments, evaluations, remediation, signoffs, certificates, and qualifications.

### Compliance Core

Compliance Core dashboards should prioritize:

- Rulepack health
- Evidence gaps
- Requirement coverage
- Control failures
- Expiring evidence
- Import validation issues
- Mapping confidence review queues
- Compliance situations requiring attention

Compliance Core owns regulatory normalization, rulepacks, requirement mappings, evidence interpretation, compliance evaluation, and controlled compliance catalogs.

## 9. Primary Operational View

Each dashboard should include one dominant operational view.

This section should be visually larger than supporting cards and should communicate the product’s core operating state.

Examples:

- RoutArr: route map, route board, dispatch queue, or ETA risk board
- StaffArr: readiness overview, schedule coverage, team readiness, or people status
- LoadArr: loads overview, receiving board, warehouse flow, or shipment status
- SupplyArr: spend overview, order status, supplier performance, or procurement exceptions
- MaintainArr: asset readiness, work order board, PM calendar, or defect risk board
- TrainArr: training assignment funnel, qualification coverage, expiring credentials, or evaluation queue
- Compliance Core: compliance coverage, evidence gaps, rulepack evaluation status, or import review queue

The primary operational view should be more than a decorative chart. It must support a practical decision.

## 10. Charts

Charts may be used when they reveal a trend, distribution, workload shape, forecast, or comparison.

Charts must include:

- Clear title
- Time period or scope
- Axis or legend labels where applicable
- Values readable enough to interpret
- Drill-in behavior when useful
- Empty state when no data exists

Charts must not hide the actual operational meaning.

A chart should not be used when a table, queue, or status card would communicate the information more directly.

Valid chart uses:

- Readiness trend over 30 days
- Scheduled hours vs coverage
- Load status distribution
- Spend over time
- Open defects by severity
- Training assignment completion status

Invalid chart uses:

- Decorative line with no decision value
- Donut chart where the categories are unclear
- Dense analytics with no operational takeaway
- Fake trend data that cannot be traced to a source

## 11. Tables and Boards

Dashboards may include compact tables or boards when the user needs to inspect live operational records.

Dashboard tables should show only the fields needed for triage.

Common table fields include:

- Identifier
- Owner or assignee
- Status
- Date or ETA
- Severity or risk
- Next action
- Source product when cross-product
- Drill-in action

Dashboard tables must not become full management screens. Full CRUD, advanced search, bulk edit, and deep filtering belong on dedicated list/detail pages.

A dashboard table should usually show between 5 and 10 records. Larger datasets require a dedicated page link.

## 12. Attention Panels

Every operational dashboard should include an attention, risk, blocker, or alert section when the product has any time-sensitive state.

Attention panels should prioritize:

1. Critical safety, compliance, readiness, or release blockers
2. Items that will soon become blockers
3. Exceptions requiring human review
4. Missing handoffs or stale events
5. Informational alerts

Each attention item should include:

- Short human-readable title
- Why it matters
- Severity
- Age or due time when relevant
- Source product when cross-product
- Direct action or drill-in path

Attention panels must not bury critical items below low-value notifications.

## 13. Severity Rules

Dashboard severity language must be consistent.

Recommended severity levels:

- `Critical`
- `High`
- `Medium`
- `Low`
- `Info`
- `Blocked`
- `Review`
- `Watch`
- `Healthy`

Severity should be derived from product rules, thresholds, or source-system state. It must not be hardcoded by frontend color alone.

Color may reinforce severity, but text must carry the meaning.

## 14. Cross-Product Signals

Dashboards may display cross-product signals when those signals affect operational decisions.

Cross-product dashboard cards must clearly distinguish between:

- Data owned by the current product
- Readiness signals from another product
- External references
- Handoff states
- Mirrored snapshots
- Cached labels

Cross-product signals must be read-only unless the current product owns the action being taken.

Examples:

- RoutArr may show `Vehicle inspection clearance` from MaintainArr as a dispatch blocker.
- RoutArr may show `Driver qualification coverage` from StaffArr or TrainArr as a release condition.
- LoadArr may show `Dock appointment not confirmed` from RoutArr as an inbound receiving risk.
- SupplyArr may show `Receiving pending` from LoadArr as purchase order fulfillment context.
- StaffArr may show `Training assignments` from TrainArr as readiness context.

The dashboard must not create, mutate, or silently reconcile another product’s canonical records.

When action is required in another product, the dashboard should provide a handoff link, not duplicate the workflow.

## 15. Dashboard Scope Note

When a dashboard includes cross-product data, the page should include a compact scope note.

The note should explain what the dashboard owns and what is shown as reference.

Example:

`Dashboard scope: RoutArr owns dispatch, route planning, trip execution, assignment, stop management, ETA, and exceptions. External readiness signals are surfaced as references only.`

Scope notes should be concise and placed near the bottom of the main content area unless the product requires stronger warning context.

## 16. Actions

Dashboard actions must be limited to common, high-value actions.

Primary actions may include:

- Create route
- Create shift
- New load
- Create order
- Create work order
- Create assignment
- Upload evidence
- Report incident
- Run report

Secondary actions may include:

- View all
- View report
- Open queue
- Review blockers
- Drill into detail
- Export when appropriate

Dashboards should not expose every possible action. The dashboard should guide action, not replace the product.

## 17. Quick Actions

Quick action panels are allowed when a product has common workflows that users frequently start from the dashboard.

Quick actions must:

- Respect permissions
- Use clear labels
- Route to canonical create/review flows
- Not bypass required guided workflows
- Not create records without confirmation when required

Quick actions should not become an alternate navigation menu.

## 18. Date and Time Context

Dashboards must make their time basis clear.

Depending on the product, this may be:

- Today
- This week
- Last 7 days
- Last 30 days
- Current shift
- Current route window
- Current dispatch day
- Current payroll week
- Current compliance period
- Selected terminal/site/date range

Relative deltas must specify the comparison period.

Examples:

- `up 8% vs last week`
- `up 15% vs last month`
- `9 vs last 7 days`
- `next 30 days`

Avoid vague deltas such as:

- `up 8%`
- `improved`
- `worse`
- `trending`

## 19. Freshness and Live State

Dashboards must show freshness when data is live, near-live, delayed, cached, or manually refreshed.

Freshness indicators may include:

- Last updated timestamp
- Live badge
- Sync status
- Data delay notice
- Manual refresh control
- Source unavailable warning

Do not imply live data if the dashboard uses cached snapshots.

When a source product or integration is unavailable, show degraded state clearly.

## 20. Empty States

Every dashboard section must define an empty state.

Empty states should explain:

- What is missing
- Whether that is good, bad, or neutral
- What the user can do next

Examples:

- `No open incidents. Your team has no active reported incidents.`
- `No routes scheduled for this date. Create a route or adjust the date filter.`
- `No expiring certifications in the next 30 days.`
- `No active blockers. Dispatch release checks are currently clear.`

Empty states must not look like broken cards.

## 21. Loading States

Dashboard sections must support independent loading states.

A slow chart must not block the whole dashboard if KPI cards or tables can load separately.

Loading states should preserve layout stability and avoid large page jumps.

Use skeletons, placeholders, or section-level spinners where appropriate.

## 22. Error States

Dashboard errors must be specific to the failed section when possible.

An error state should include:

- What failed
- Whether other dashboard data is still valid
- Retry action when appropriate
- Source product or integration if relevant
- Permission explanation when applicable

Avoid generic full-page failures unless the entire dashboard cannot be rendered.

## 23. Permission-Aware Rendering

Dashboards must respect user permissions.

If a user lacks permission to view a metric, record, or action:

- Do not leak sensitive values
- Do not show clickable actions they cannot use
- Prefer hiding sections over showing forbidden data
- Use permission-aware empty states where helpful

Examples:

- A user without incident access should not see incident details.
- A user without payroll or scheduling authority should not see restricted labor cost details.
- A user without compliance access should not see sensitive audit findings.

## 24. Data Provenance

Dashboard data must be traceable to a source.

Each metric should be backed by:

- Product-owned query
- Product-owned read model
- Event-derived projection
- Source-product API
- Controlled mirror table
- Explicit integration feed

Frontend-only fabricated metrics are not allowed.

Mock data is allowed only in isolated mockup, fixture, storybook, or development preview contexts and must not ship as production dashboard logic.

## 25. Read Models

Dashboards should prefer purpose-built read models or summary endpoints over assembling complex operational summaries in the frontend.

Recommended backend patterns:

- `/api/v1/dashboard/summary`
- `/api/v1/dashboard/kpis`
- `/api/v1/dashboard/activity`
- `/api/v1/dashboard/blockers`
- `/api/v1/dashboard/readiness`
- `/api/v1/dashboard/attention`
- `/api/v1/dashboard/execution-board`

Dashboard endpoints should return view-ready data, including labels, status, severity, ordering, drill-in references, and freshness metadata.

The frontend should not duplicate business rules that belong to the backend or product domain.

## 26. Ordering Rules

Dashboard content must be ordered by operational priority.

Recommended ordering:

1. Critical blockers
2. Time-sensitive work
3. Active execution
4. Readiness and compliance
5. Trends
6. Recent activity
7. Informational announcements

Within attention lists, sort by severity first, then due time or age.

Within operational tables, use the product’s natural urgency order.

Examples:

- Routes: exception status, ETA risk, departure time
- Work orders: safety critical, asset down, due date
- Training: overdue, expiring soon, required before duty
- Loads: blocked, appointment risk, receiving state
- Orders: delayed, missing approval, supplier risk

## 27. Density

Dashboard pages may be information-rich, but they must remain scannable.

Rules:

- Use section headers consistently
- Use compact supporting text
- Avoid paragraphs inside cards unless needed
- Use badges for status
- Use tables for repeatable records
- Use charts only where visual comparison helps
- Avoid more than one dominant chart per dashboard row
- Avoid stacking too many equal-weight cards

A dashboard with many cards but no clear priority is not acceptable.

## 28. Visual Style

Dashboard main content should follow the platform visual language.

Recommended visual traits:

- Dark background
- Elevated surface cards
- Subtle borders
- Rounded panels
- Clear typography hierarchy
- Blue primary action treatment
- Teal/green for healthy or synced state
- Amber/orange for watch, pending, or warning
- Red for critical risk
- Purple may be used for training, schedule, or special domain indicators
- Muted text for supporting context

Color must support meaning. It must not be the only way to understand state.

## 29. Card Rules

Dashboard cards must have a clear job.

A valid card includes at least one of:

- Metric
- Status
- Trend
- Queue
- Risk item
- Action
- Summary of owned work
- Cross-product signal relevant to the product

Cards should not exist solely to fill grid space.

Every card should have:

- Title or label
- Primary content
- Supporting context
- Optional action
- Defined loading, empty, and error states

## 30. Responsive Behavior

Dashboard main content must support desktop and mobile as first-class experiences.

Desktop dashboards may use multi-column grids.

Mobile dashboards must:

- Preserve information hierarchy
- Stack sections in priority order
- Keep KPI cards readable
- Avoid horizontal overflow
- Convert wide tables into compact cards or scrollable regions
- Keep primary actions reachable
- Keep attention items above low-priority analytics

Mobile must not be treated as an afterthought.

## 31. Accessibility

Dashboard pages must be accessible.

Requirements:

- Text contrast must be sufficient
- Icons must not be the only status indicator
- Charts must have text summaries or accessible labels
- Buttons and links must be keyboard reachable
- Focus states must be visible
- Time-sensitive warnings must be text-readable
- Tables must use meaningful headers
- Badges must include readable labels

Animations should be subtle and must not be required to understand the page.

## 32. Drill-In Behavior

Dashboard items should route to canonical product pages.

Examples:

- Clicking a route opens the RoutArr route detail page.
- Clicking an incident opens the StaffArr incident detail page.
- Clicking a training assignment opens the TrainArr assignment detail page.
- Clicking a work order opens the MaintainArr work order detail page.
- Clicking a supplier opens the SupplyArr supplier detail page.
- Clicking a load opens the LoadArr load detail page.

Dashboard drill-ins must not create duplicate detail pages.

## 33. Creation Behavior

Dashboards may provide create buttons, but create workflows must follow the product’s canonical creation rules.

If the product requires a guided workflow, the dashboard action must open that guided workflow.

Dashboard create actions must not bypass:

- Required fields
- Ownership validation
- Controlled catalogs
- Permission checks
- Tenant checks
- Cross-product reference validation
- Approval gates
- Required review steps

## 34. Activity Feeds

Recent activity panels are allowed when they help users understand what changed.

Activity feed items should include:

- Actor or source
- Action
- Target record
- Timestamp
- Optional severity or status
- Drill-in link

Activity feeds should not replace audit logs. Audit logs belong in dedicated record or admin views.

## 35. Announcements

Announcements may appear on dashboards when they affect operations.

Valid announcements include:

- Upcoming rule changes
- System maintenance
- Required retraining
- Deadline reminders
- Policy changes
- Product-specific operational notices

Announcements must not bury urgent blockers.

## 36. Handoffs

Dashboards may show handoffs when work moves between products.

A handoff item should include:

- Source product
- Target product
- Record summary
- Current state
- Required next action
- Time sensitivity
- Sync or acknowledgement status

Handoff panels must not mutate the source product’s canonical records unless the current product owns that handoff action.

## 37. Readiness and Blocker Panels

Readiness panels should show whether work can proceed.

They are especially important for dispatch, staffing, maintenance, training, loading, and compliance workflows.

Readiness panels should summarize checks such as:

- Person qualification
- Equipment readiness
- Required documents
- Inspection clearance
- Open defects
- Appointment alignment
- Required approvals
- Compliance evidence
- Training completion
- Site or location validity

A readiness panel should make it obvious whether the user can proceed, should review, or is blocked.

## 38. No Raw JSON

Dashboard pages must never show raw JSON to ordinary users.

Structured diagnostic data may be available only in admin/debug views when explicitly permitted.

Dashboard UI should translate system state into human-readable labels, summaries, badges, and actions.

## 39. No Freestyle Business Rules in Frontend

The frontend may format and present dashboard data, but it must not invent business logic.

Rules for readiness, risk, compliance, qualification, dispatch release, PM status, or work authorization must come from product-owned services, read models, rule engines, or source-product APIs.

Frontend code may map already-computed statuses to presentation styles.

## 40. Tenant Isolation

Dashboard data must always be tenant-scoped.

No dashboard query, cache, event stream, read model, or cross-product signal may leak data across tenants.

Cross-product dashboard data must use tenant-safe service-token flows, product APIs, or approved mirror/reference tables.

## 41. Performance

Dashboard pages should load quickly enough to support operational use.

Requirements:

- Prioritize above-the-fold content
- Use section-level loading where possible
- Avoid frontend-heavy aggregation over large datasets
- Cache safe summaries where appropriate
- Use pagination or limits for dashboard tables
- Avoid expensive chart queries on every render
- Use refresh intervals intentionally

Live dashboards should balance freshness with system load.

## 42. Refresh Behavior

Dashboards may support automatic refresh where live operations require it.

Auto-refresh must not interrupt user interaction.

Refresh behavior should be explicit for operational pages where timing matters.

Recommended patterns:

- Manual refresh button
- Last updated timestamp
- Live badge
- Quiet background refresh
- Stale data warning
- Section-level refresh for expensive panels

## 43. Product-Specific Identity

Each dashboard should feel like the product it represents while remaining part of the STL Compliance suite.

Product dashboards may vary in:

- Primary operational view
- KPI selection
- Domain labels
- Card mix
- Chart types
- Queue types
- Severity emphasis
- Primary action

They should not vary in:

- Basic readability
- Main content hierarchy
- Permission behavior
- Tenant safety
- Loading/error/empty handling
- Cross-product ownership rules
- Design token usage

## 44. Dashboard Anti-Patterns

The following are not allowed:

- Showing every available metric with no priority
- Using dashboards as a replacement for list/detail pages
- Displaying cross-product data without ownership clarity
- Creating records in another product from the dashboard without a handoff
- Showing fake production metrics
- Hardcoding readiness rules in frontend components
- Hiding critical blockers below decorative charts
- Using color without text labels
- Showing raw JSON
- Rendering broken empty cards
- Allowing one failed widget to crash the whole dashboard
- Treating mobile as a compressed desktop screenshot
- Making dashboard tables full CRUD screens
- Mixing unrelated product workflows into the dashboard

## 45. Minimum Acceptable Dashboard

A product dashboard is minimally acceptable when it includes:

1. Clear page title
2. Product-specific operational summary
3. KPI strip with meaningful context
4. Primary operational view
5. Attention/risk/blocker section when applicable
6. At least one drill-in path to canonical records
7. Loading, empty, and error states
8. Permission-aware rendering
9. Tenant-scoped data
10. Product ownership boundaries respected

A dashboard that is visually attractive but operationally vague is not complete.

## 46. Preferred Dashboard Composition

A strong dashboard should include:

1. Header with title, summary, date/scope, and primary action
2. Four to six KPI cards
3. One large operational visualization or board
4. One attention-required panel
5. One execution table or queue
6. One readiness/blocker section
7. Supporting cards for upcoming work, handoffs, alerts, or activity
8. Compact scope note when cross-product signals appear

This structure may be adapted, but the dashboard must retain clear operational priority.

## 47. Implementation Rule

Dashboard implementations should be built from reusable dashboard primitives, not one-off visual code.

Recommended primitives:

- DashboardPage
- DashboardHeader
- DashboardKpiCard
- DashboardSection
- DashboardChartCard
- DashboardTableCard
- DashboardAttentionPanel
- DashboardReadinessPanel
- DashboardActivityFeed
- DashboardScopeNote
- DashboardEmptyState
- DashboardErrorState
- DashboardSkeleton

Reusable primitives must remain presentation-focused. Product-specific business rules belong in product services, queries, read models, or API responses.

## 48. Final Rule

A dashboard is successful when a qualified user can open it and understand the current state of the product without hunting.

The dashboard should make the next operational decision obvious.
