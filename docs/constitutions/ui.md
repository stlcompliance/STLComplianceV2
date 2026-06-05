# STL Compliance UI / Shared Shell / Color Scheme Constitution

## 1. Prime Directive

STL Compliance is one suite, not a pile of separate apps.

Every product may have its own domain, database, permissions, workflows, and navigation depth, but the user experience must feel like one coherent operating system.

The shared shell exists to make NexArr, StaffArr, TrainArr, MaintainArr, RoutArr, LoadArr, SupplyArr, Compliance Core, and future Arr products feel unified.

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

The shared shell must treat NexArr as the platform identity and entitlement authority.

The shell may display product availability, tenant context, launch links, and user state, but it must not recreate product authorization rules.

NexArr answers:

- Who is this person?
- Which tenant are they acting in?
- Which products can this tenant/person access?
- Which product launch links are allowed?

Products answer:

- What can this person do inside this product?
- Which product records can they see?
- Which workflow actions are permitted?

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

The product switcher must show only entitled products by default.

It may show unavailable products only when there is a clear reason, such as:

- User can request access
- Tenant can upgrade
- Admin can configure entitlement
- Product is coming soon

Unavailable products must never look accidentally broken.

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

# 8. Color Scheme

## 8.1 Base Theme

The default STL Compliance theme should be dark, operational, and high-contrast.

Primary base palette:

- App background: deep navy / near-black
- Main surface: dark slate
- Elevated surface: blue-gray slate
- Border: muted slate
- Text: near-white
- Muted text: cool gray
- Disabled text: low-contrast slate
- Accent: bright cyan/blue by default

The system should support light mode eventually, but dark mode is the primary visual identity.

## 8.2 Suggested Core Tokens

Use design tokens, not hardcoded colors.

```css
:root {
  --color-bg-app: #0b1120;
  --color-bg-shell: #0f172a;
  --color-bg-surface: #111827;
  --color-bg-surface-elevated: #1e293b;
  --color-bg-surface-muted: #162033;

  --color-border-subtle: #243044;
  --color-border-strong: #334155;

  --color-text-primary: #f8fafc;
  --color-text-secondary: #cbd5e1;
  --color-text-muted: #94a3b8;
  --color-text-disabled: #64748b;

  --color-accent: #38bdf8;
  --color-accent-hover: #0ea5e9;
  --color-accent-soft: rgba(56, 189, 248, 0.14);
  --color-accent-border: rgba(56, 189, 248, 0.42);

  --color-success: #22c55e;
  --color-warning: #f59e0b;
  --color-danger: #ef4444;
  --color-info: #38bdf8;

  --color-focus-ring: rgba(56, 189, 248, 0.55);
}