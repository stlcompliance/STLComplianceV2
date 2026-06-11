# STL Compliance Settings, Admin, Configuration, and Setup Constitution

## 1. Purpose

This constitution defines where setup and configuration live so platform admin, product admin, tenant setup, product settings, reference data, integrations, permissions, and workflow configuration do not become tangled.

## 2. Scope

This constitution applies to:

- Platform admin
- Product admin
- Tenant setup
- Product settings
- User preferences
- Integration settings
- Workflow configuration
- Reference-data setup
- Product dependency settings
- Dangerous configuration changes
- Setup pages and setup wizards

## 3. Prime directive

Configuration must be owned by the product or platform authority that owns the underlying decision.

Settings screens must not become secret CRUD backdoors around guided workflows, ownership rules, or permission gates.

## 4. Configuration scopes

Every setting must declare scope:

- `platform`
- `tenant`
- `product`
- `site`
- `department`
- `role`
- `team`
- `person`
- `integration`
- `record`

A setting without scope is incomplete.

## 5. NexArr setup ownership

NexArr owns setup for:

- Tenant identity
- Tenant membership
- Product entitlement
- Product launch/handoff
- Platform admin
- Service clients
- Service tokens
- Platform-level integration authorization where required
- Product dependency visibility
- Platform access audit

No product may create its own platform admin system.

## 6. StaffArr setup ownership

StaffArr owns setup for:

- People
- Workers
- Org units
- Internal sites
- Buildings
- Rooms
- Docks
- Yards
- Operational locations
- Departments
- Positions
- Teams
- Manager relationships
- Permission assignments
- Role assignments
- Delegation/temporary authority

Products consume StaffArr people/location/authority context.

## 7. Compliance Core setup ownership

Compliance Core owns setup for:

- Governing bodies
- Rulepacks
- Regulations/citations
- Compliance vocabulary
- Evidence requirements
- Applicability logic
- Exemptions/exceptions
- Rule-to-product mappings
- Import mapping review where regulatory meaning is involved

Products must not build competing regulatory catalogs.

## 8. RecordArr setup ownership

RecordArr owns setup for:

- Controlled document categories
- Record categories
- Templates
- Retention schedules
- Legal hold behavior
- Document approval behavior
- Read-and-acknowledge configuration
- Evidence storage behavior

Compliance Core owns evidence meaning; RecordArr owns document/retention mechanics.

## 9. Product admin ownership

Each product owns settings for its domain execution.

Examples:

- MaintainArr: PM settings, work order defaults, inspection workflow configuration
- RoutArr: dispatch settings, route/trip defaults, exception handling defaults
- LoadArr: receiving/putaway/pick/issue behavior, inventory adjustment reason codes
- SupplyArr: supplier approval workflow defaults, purchasing thresholds/context
- TrainArr: training program workflow defaults, evaluator rules, remediation behavior
- AssurArr: CAPA workflow settings, severity defaults, release verification rules
- ReportArr: report schedules, subscriptions, metric display settings
- CustomArr: customer onboarding/requirements settings
- OrdArr: request/order intake and orchestration settings

## 10. User preferences

User preferences may control experience, not business truth.

Examples:

- Theme when supported
- Notification delivery preferences
- Saved views
- Table density
- Default landing page

User preferences must not bypass required safety/compliance/approval notifications or permission checks.

## 11. Integration settings

Integration settings must declare:

- External system
- Owning product/platform area
- Tenant scope
- Credential authority
- Sync direction
- Mapping behavior
- Failure behavior
- Writeback permissions
- Last sync status

External credentials must be permission-protected and secret-managed.

## 12. Setup wizards

Setup wizards are encouraged when configuration has dependencies.

A setup wizard should show:

- Required steps
- Owner of each configuration area
- Dependencies
- Completion state
- Blocking issues
- Safe defaults
- Review before activation

## 13. Dangerous settings

Dangerous settings require stronger treatment.

Examples:

- External writeback enablement
- Rulepack publish/activation
- Retention schedule changes
- Service token creation
- Break-glass access
- Inventory adjustment permissions
- Financial handoff configuration
- Deleting/archive behavior

Dangerous settings should require:

- Explicit permission
- Confirmation
- Plain-language impact preview
- Audit event
- Possibly two-person review depending on risk

## 14. Defaults

Defaults must be explicit.

Do not hide business-changing defaults in code without a settings surface or documented rule.

Defaults should identify whether they are:

- Platform defaults
- Tenant defaults
- Product defaults
- Site defaults
- User preferences

## 15. Dependency visibility

Setup should show product dependencies.

Examples:

- RoutArr dispatch may depend on StaffArr people, TrainArr qualifications, MaintainArr asset readiness, and LoadArr load readiness.
- MaintainArr parts workflow may depend on SupplyArr items/vendors and LoadArr inventory.
- Compliance reporting may depend on Compliance Core, RecordArr, and source product events.

## 16. Configuration audit

Configuration changes must be audit-visible when they affect security, workflow, compliance, retention, reporting, integrations, or external writebacks.

Audit should include:

- Changed setting
- Old/new summary
- Actor
- Time
- Scope
- Reason when required

## 17. Anti-patterns

The following are not allowed:

- Product-level platform admin
- Settings that bypass ownership rules
- Hidden settings that change business effects with no UI/audit
- Integration credentials in frontend or plain config
- Setup pages as unvalidated CRUD bypasses
- User preferences that suppress mandatory safety/compliance actions
- Product-specific duplicate regulatory catalogs
- Configuration with no scope

## 18. Minimum acceptable implementation

A settings/admin feature is minimally acceptable when it has:

1. Clear owner
2. Clear scope
3. Permission gate
4. Dependency awareness
5. Impact preview for dangerous settings
6. Audit for material changes
7. No hidden bypass of canonical workflows
8. Safe default behavior
