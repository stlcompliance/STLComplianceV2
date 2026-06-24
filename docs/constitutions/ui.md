# STL Compliance UI / Shared Shell / Color Scheme Constitution

## 1. Prime Directive

STL Compliance is one suite, not a pile of separate apps.

Every product may have its own domain, database, permissions, workflows, and navigation depth, but the user experience must feel like one coherent operating system.

The shared shell exists to make NexArr, StaffArr, TrainArr, MaintainArr, RoutArr, CustomArr, OrdArr, LedgArr, LoadArr, SupplyArr, Compliance Core, Field Companion, RecordArr, ReportArr, and future Arr products feel unified.

Desktop and mobile are both first-class surfaces. Mobile is not a Field Companion-only afterthought. Desktop is not the only “real” app.

---

# 2. Shell Ownership

## 2.1 Shared Shell Owns

The shared shell owns:

- Global layout
- Topbar
- App/product switcher
- Current tenant context
- Current user context
- Global search entry point
- Notifications entry point
- Help/support entry point
- User/account menu
- Product launch/handoff
- Responsive shell behavior
- Global route framing
- Global loading/error/empty states
- Theme and accent application
- Permission-aware visibility for products and shell-level actions

## 2.2 Products Own

Each product owns:

- Product-specific pages
- Product-specific navigation sections
- Domain workflows
- Product dashboards
- Product-specific permissions
- Product-level command actions
- Product-level empty states
- Product-specific table/list/card layouts
- Product-specific mobile workflow design

## 2.3 NexArr Relationship

NexArr is the secure front door.

The shared shell treats NexArr as the platform identity, tenant-membership, session, launch-context, service-identity, and platform-admin authority. Product availability is nonvariable: every active tenant member can launch every ordinary product.

The shell displays the full ordinary product catalog, tenant context, operational status, launch links, and user state. Compliance Core studio is the only product UI hidden from non-platform-admins. The shell must not recreate product-domain authorization rules.

NexArr answers:

- Which platform account is signed in?
- Which tenant membership is active?
- What session and launch context is valid?
- Is the actor a platform administrator for Compliance Core studio access?
- Is the destination active, degraded, in maintenance, or temporarily unavailable?

Products answer:

- What can this person do inside this product?
- Which product records can they see?
- Which workflow actions are permitted?
- Which record, site, location, party, or state restrictions apply?

---

# 3. Layout Philosophy

The UI should feel:

- Operational
- Dense but readable
- Calm under pressure
- Fast to scan
- Hard to misuse
- Friendly enough for non-technical users
- Serious enough for compliance, maintenance, logistics, training, and audits

The shell must avoid looking like a consumer toy, but it also must not become cold enterprise clutter.

The correct feeling is:

“Modern operations console with human-friendly workflows.”

---

# 4. Desktop Is First Class

Desktop layouts must support:

- High-density tables
- Multi-column detail views
- Persistent navigation
- Side-by-side context
- Bulk review
- Audit workflows
- Dispatch boards
- Maintenance planning
- Compliance rule review
- Training program building
- Inventory and receiving workflows
- Long-form configuration screens

Desktop must not be treated as a stretched mobile layout.

## 4.1 Desktop Shell Structure

Standard desktop shell:

- Left suite/product sidebar
- Topbar
- Main content area
- Optional right-side contextual panel
- Optional command bar
- Optional bottom status/toast region

Desktop should use the available width intelligently.

Large screens should support:

- Master/detail views
- Split panels
- Sticky workflow summaries
- Persistent filters
- Contextual side panels
- Table column density controls

## 4.2 Desktop Navigation

Desktop navigation should favor persistent orientation.

Users should always know:

- Which tenant they are in
- Which product they are in
- Which module they are in
- Which record or workflow they are viewing
- What action is primary
- What action is dangerous
- What action is blocked

## 4.3 Desktop Sidebar

The desktop sidebar should support:

- Suite-level product switcher
- Current product navigation
- Collapsed icon mode
- Expanded label mode
- Active route state
- Permission-hidden items
- Disabled-but-explained gated items where helpful
- Product identity header
- Tenant context indicator

Sidebar should not become a dumping ground.

Every sidebar item must represent a real navigation destination or workflow entry.

---

# 5. Mobile Is First Class

Mobile must support real work.

Mobile is not only for viewing.

Mobile should support:

- Inspections
- Training signoffs
- Incident reporting
- Receiving actions
- Dispatch updates
- Driver/operator workflows
- Work order updates
- Asset lookup
- Person lookup where permitted
- Evidence/photo/document capture
- QR-code workflows
- Approvals
- Notifications
- Basic dashboards
- Field-safe forms

## 5.1 Mobile Shell Structure

Standard mobile shell:

- Top app bar
- Current product/context indicator
- Bottom navigation or task rail
- Slide-out product/menu drawer
- Full-screen workflow pages
- Sticky primary action zone
- Touch-friendly command buttons
- Mobile-safe modals converted to sheets
- Offline/poor-connection indicators where applicable

Mobile must not depend on hover.

Mobile must not hide critical actions behind tiny icons.

Mobile must not require horizontal scrolling for normal use.

## 5.2 Mobile Navigation

Mobile navigation should prioritize tasks over deep hierarchy.

Use:

- Bottom navigation for high-frequency destinations
- Drawer for full product navigation
- Action sheets for contextual actions
- Stepper flows for complex forms
- Sticky CTAs for active workflows
- Search-first navigation where helpful

Mobile should not blindly copy the desktop sidebar.

## 5.3 Mobile Tables

Tables must become usable mobile views.

Rules:

- Tables collapse into cards on mobile
- Highest-value fields appear first
- Status, due state, owner, and next action are always visible when relevant
- Secondary fields collapse behind “details”
- Bulk actions must be available only when touch-safe
- Filters become sheets
- Sort controls become compact menus
- Column pickers become mobile field selectors

## 5.4 Mobile Forms

Mobile forms must be broken into logical sections.

Rules:

- One major decision per section
- Required fields first
- Optional fields later
- Long forms use stepper/wizard behavior
- Save draft where workflows are long
- Large tap targets
- Clear validation messages
- Camera/upload workflows are native-feeling
- Signature/signoff workflows are mobile-safe

---

# 6. Responsive Breakpoints

Use intentional breakpoints, not random CSS patches.

## 6.1 Breakpoint Classes

- Compact mobile: 320px–479px
- Mobile: 480px–767px
- Tablet: 768px–1023px
- Small desktop: 1024px–1279px
- Desktop: 1280px–1535px
- Wide desktop: 1536px+

## 6.2 Behavior by Breakpoint

### Compact Mobile

- No persistent sidebar
- Full-screen workflows
- Bottom nav or drawer
- Cards instead of tables
- Minimal chrome
- Primary action sticky at bottom

### Mobile

- Drawer navigation
- Bottom task navigation
- Cards/lists
- Stepper forms
- Sheets instead of modals

### Tablet

- Optional compact rail
- Two-column layouts allowed carefully
- Master/detail allowed when readable
- Tables allowed only when columns are limited

### Desktop

- Persistent sidebar
- Full tables
- Multi-column detail pages
- Right context panel allowed
- Advanced filters visible

### Wide Desktop

- Split views encouraged
- Right-side panels encouraged
- Dense operational dashboards allowed
- Avoid overly wide text blocks

---

# 7. Shared Shell Regions

## 7.1 Topbar

The topbar must include:

- Current tenant indicator
- Current product indicator
- Global search entry
- Notifications
- User menu
- Optional environment/status indicator

The topbar should not be overloaded with product-specific controls.

Product-specific actions belong in page headers, command bars, or contextual panels.

## 7.2 Product Switcher

The product switcher must show every active ordinary product to every active tenant member. It must not filter products by tenant package, subscription, role, or per-user launch grant.

Compliance Core studio appears only for validated platform administrators. Its absence for ordinary users does not disable Compliance Core runtime behavior inside other products.

A product may show operational state such as available, degraded, maintenance, or temporarily unavailable. That state must be explained clearly and must never use licensing, upgrade, request-access, or missing-product language.

A user who lacks product-local permissions may still open the product and receive a clear permission-limited landing state. Actions and records remain protected by the product API.

## 7.3 Breadcrumbs

Breadcrumbs should be used on deeper desktop pages.

Mobile should use compact back navigation instead of long breadcrumb trails.

Breadcrumbs should follow:

Suite / Product / Module / Record

Example:

STL Compliance / MaintainArr / Assets / Truck 104

## 7.4 Page Header

Every major page should have a consistent header:

- Title
- Short description where helpful
- Status badge if record-specific
- Primary action
- Secondary actions
- More menu for rare actions

Page headers must collapse gracefully on mobile.

## 7.5 Command Bar

Use command bars for operational pages with repeated actions.

Examples:

- Work order board
- Dispatch board
- Training assignment queue
- Inventory receiving queue
- Compliance rule review queue
- Incident review queue

Command bars may include:

- Filters
- Search within page
- Saved views
- Bulk actions
- Export
- Refresh
- Density toggle

---

# 8. Theme, Color, and Shared Component Enforcement

## 8.1 Equal light and dark application states

Light and dark modes are equal supported states across the complete application. Neither mode may depend on emergency overrides, unreadable inherited colors, or product-local patches. Every shell region, page, form, table, drawer, modal, menu, tooltip, toast, chart, loading state, empty state, error state, disabled state, and print preview must be readable and usable in both modes.

The system may have a preferred initial mode, but no product may be designed as dark-only or light-only. User selection is a cross-product preference.

## 8.2 Semantic tokens

Use central semantic design tokens rather than hard-coded palette values in product code. Tokens must cover at least:

- app, shell, surface, elevated, inset, overlay, and print backgrounds
- primary, secondary, muted, inverse, disabled, and link text
- subtle, default, strong, focus, selected, and destructive borders
- primary, secondary, quiet, destructive, and link actions
- hover, active, selected, focus, disabled, loading, and drag states
- success, warning, danger, info, neutral, blocked, stale, and pending statuses
- chart/data-visualization series designed for both themes

Raw hex/rgb/hsl values and palette-specific utility classes are permitted only in approved central token, brand, or domain-visualization files with an explicit audit annotation.

## 8.3 Shared components first

Products must use the shared shell, page header, action bar, filter bar, table/list/board, forms, reference picker, quick create, badges, drawers, dialogs, toasts, page states, print runtime, and scheduling primitives before creating local equivalents.

Product-local styling is reserved for genuinely domain-specific visualization. Basic cards, panels, headings, forms, tables, buttons, status badges, dialogs, and page states are not domain-specific.

## 8.4 Contrast and status

Text, controls, focus rings, selected states, dividers, and disabled states require appropriate contrast in both themes. Status must never rely on color alone; pair color with text, icon, shape, or pattern.

## 8.5 No walls of content

Unified UI does not mean cramming every field and route onto one screen. Pages must remain scannable:

- sidebars contain durable destinations only
- tables show decision-useful default columns
- long forms are grouped and collect required basics first
- detail pages are read-first with predictable tabs
- explanatory text is brief and contextual
- advanced and technical information is disclosed intentionally

## 8.6 Page constitutions

Every route must declare and comply with a page archetype under `constitutions/pages/`. The same list, detail, create/edit, drawer, dashboard, wizard, report, settings, and admin patterns apply across products.

## 8.7 Audit and release enforcement

The repository theme audit is a mandatory CI gate. Shared component fixtures and product visual smoke tests render light/dark and normal/hover/focus/selected/disabled/loading/empty/error/degraded states. A page is not accepted based on a single screenshot or one theme.

## 8.8 Audit-aligned requirements

This constitution directly addresses NAV-001 through NAV-005, UX-001 through UX-006, and UI-001 through UI-004 from the June 23, 2026 audit. Product-local design systems, hard-coded colors, browser-native dialogs, raw JSON/ID-first presentation, fake success, and inconsistent page states are regression blockers.
