# STL Compliance Accessibility, Time, Localization, and Human Factors Constitution

## 1. Purpose

This constitution defines human factors that must be consistent across STL Compliance: accessibility, time handling, localization readiness, units of measure, readable language, and field-safe interaction.

Regulated operational software must be hard to misuse, readable under pressure, and clear across locations and time zones.

## 2. Scope

This constitution applies to:

- UI accessibility
- Forms
- Tables/cards
- Status labels
- Date/time display
- Time zones
- Units of measure
- Localization readiness
- Plain-language validation
- High-risk confirmations
- Mobile/touch ergonomics
- Keyboard/focus behavior
- Charts and visual indicators

## 3. Prime directive

Critical operational meaning must never rely on color, icon, hover, relative time, abbreviation, or tribal knowledge alone.

## 4. Accessibility baseline

All primary workflows must support:

- Sufficient text contrast
- Keyboard navigation where applicable
- Visible focus states
- Readable status text
- Proper labels for inputs
- Meaningful table headers
- Accessible button/link labels
- Non-color status indicators
- Error messages tied to fields
- Reduced reliance on animation

## 5. Color and status

Color may reinforce meaning but must not be the only way to understand state.

Every status badge should include readable text.

Examples:

- `Blocked`
- `Ready`
- `Expired`
- `Needs review`
- `Pending sync`
- `Source unavailable`

## 6. Time storage

System timestamps should be stored in UTC unless a specific domain requires another representation.

The original source timezone/site timezone should be preserved when it affects business meaning.

## 7. Time display

UI must make time basis clear.

Show one or more of:

- User local time
- Site local time
- Route/terminal time
- UTC where needed for audit/admin
- Exact timestamp on hover/tap or secondary metadata

Do not display ambiguous dates for time-sensitive or audit-relevant records.

## 8. Relative time

Relative time is allowed only with enough context.

Good:

- `Due in 2 hours — Jun 10, 2026, 3:00 PM CDT`
- `Updated 8 minutes ago — source: LoadArr`

Bad:

- `Soon`
- `Recently`
- `Overdue` without due date where the due date matters

## 9. Date ranges and comparisons

Dashboards, reports, and metrics must state the comparison period.

Good:

- `up 8% vs last 7 days`
- `12 overdue as of Jun 10, 2026`
- `next 30 days`

Bad:

- `up 8%`
- `trending up`
- `better`

## 10. Units of measure

Units must be explicit.

Examples:

- Miles vs kilometers
- Hours vs days
- Pounds vs kilograms
- Gallons vs liters
- Each vs case vs pallet
- Fahrenheit vs Celsius

A numeric value without a unit is incomplete when unit matters.

## 11. Localization readiness

Even if English is the initial language, UI should avoid hardcoded date formats, number formats, currency formats, and unit assumptions where practical.

Text should be organized so future translation is possible.

Avoid embedding business logic in translated strings.

## 12. Plain-language validation

Validation messages should explain the problem and correction.

Good:

- `Select a StaffArr site before choosing a parts room.`
- `This driver cannot be assigned because required qualification Forklift Operator expires before the trip date.`

Bad:

- `Invalid field`
- `Error 400`
- `Rule failed`

## 13. High-risk confirmations

High-risk actions must use confirmation language that states the business effect.

Examples:

- `Dispatch trip`
- `Close work order`
- `Approve supplier`
- `Release inventory hold`
- `Publish rulepack`
- `Archive person record`
- `Send financial handoff`

Avoid vague buttons like `Done`, `OK`, or `Continue` for material state changes.

## 14. Mobile ergonomics

Mobile must use:

- Large tap targets
- Sticky primary actions where helpful
- Sheets instead of tiny modals
- Cards instead of dense tables
- No hover-only behavior
- Clear offline/sync state
- Camera/upload/signature flows designed for touch

## 15. Field-safe workflows

Field workflows should account for:

- Gloves
- Bad lighting
- Noise
- Interrupted work
- Weak connectivity
- Shared devices where policy allows
- Need to save draft/resume
- Large checklists
- Quick scan/search

## 16. Charts and data visualization

Charts must include readable labels or summaries.

Charts must not be the only way to understand a critical state.

A chart with operational effect should include drill-in or supporting table where practical.

## 17. Abbreviations and jargon

Use common industry abbreviations only where users are expected to understand them.

Where ambiguity exists, provide expanded labels or help text.

Examples:

- `PM` may mean preventive maintenance.
- `POD` may mean proof of delivery.
- `DQF` may mean driver qualification file.

Do not expose internal engineering jargon to ordinary users.

## 18. Anti-patterns

The following are not allowed:

- Color-only risk/status
- Hover-only critical information
- Tiny touch targets for field workflows
- Ambiguous dates in audit/compliance contexts
- Numbers without units where unit matters
- Generic error messages for user-correctable problems
- Vague labels for high-risk actions
- Raw enum/internal code shown as primary status
- Dense desktop table copied directly to mobile

## 19. Minimum acceptable implementation

A human-facing feature is minimally acceptable when it has:

1. Text-readable status
2. Accessible labels and focus behavior
3. Clear time basis
4. Explicit units where needed
5. Plain-language validation
6. Safe high-risk confirmation labels
7. Mobile-safe behavior when used on mobile
8. No critical meaning hidden behind color/hover alone
