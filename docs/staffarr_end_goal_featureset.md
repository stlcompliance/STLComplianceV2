# StaffArr End Goal and Granular Feature Set

## 1. Product End Goal

**StaffArr is the people operations and workforce readiness product for the STL Compliance / Arr ecosystem.**

Its end goal is to be the single operational source of truth for people, organizational structure, staffing readiness, person-to-permission assignments, employment/personnel history, personnel incidents, workforce-related routing, and workforce compliance visibility across all products.

StaffArr should answer the operational question:

> **Who is this person, where do they belong, what are they allowed to do, who manages them, what is their current status, and what history supports that answer?**

StaffArr does **not** replace NexArr as the platform identity/authentication gate, and it does **not** replace TrainArr as the training workflow and certification issuance engine. Instead, StaffArr sits between platform identity and product execution by giving every product a consistent, auditable people layer.

---

## 2. Platform Role

### 2.1 StaffArr Owns

StaffArr should own:

- People / personnel records
- Employment and workforce status
- Organizational structure
- Sites / places / operating locations
- Departments
- Positions / job roles
- Teams / crews / reporting groups
- Manager-subordinate hierarchy
- Person-to-permission assignment ledger, permission templates, assignment scopes, approval history, and workforce authorization visibility
- Manual authorization overrides
- Staffing readiness views
- Assignment history
- Personnel history / audit package
- Personnel incident intake and workforce-related incident routing, without taking ownership of every product-originated domain incident
- Active / inactive workforce state
- Person-facing `/me` experience
- Supervisor-facing subordinate management
- Workforce compliance visibility
- Training/certification visibility as consumed from TrainArr

### 2.2 StaffArr Depends On

StaffArr depends on **NexArr** for:

- Tenant existence
- Platform person identity
- Login capability
- Authentication
- Product entitlement
- Platform-level service authorization
- Product launch / handoff context

StaffArr depends on **TrainArr** for:

- Certification definitions
- Training program definitions
- Training workflow completion
- Evaluations
- Signoffs
- Issued certifications / qualifications
- Retraining determinations
- Training-related remediation workflows

StaffArr may be consumed by **MaintainArr**, **RoutArr**, **SupplyArr**, **TrainArr**, and **Compliance Core** for:

- Person lookup
- Site lookup
- Department lookup
- Position lookup
- Team lookup
- Permission checks
- Assignment validation
- Active/inactive status
- Manager/subordinate relationships
- Incident reporting
- Personnel compliance context

---

## 3. Ownership Boundaries

### 3.1 NexArr vs StaffArr

| Concern | Owning Product |
|---|---|
| Tenant exists | NexArr |
| Tenant has access to StaffArr | NexArr |
| Platform identity | NexArr |
| Login account / credentials | NexArr |
| Whether a person can log in | NexArr |
| Platform person ID | NexArr |
| Person workforce profile | StaffArr |
| Employment status | StaffArr |
| Site / department / position assignment | StaffArr |
| Org hierarchy | StaffArr |
| Person-to-permission assignment ledger, templates, scopes, and history | StaffArr |

StaffArr should reference NexArr `personId`; it should not create a separate competing identity source of truth.

### 3.2 StaffArr vs TrainArr

| Concern | Owning Product |
|---|---|
| Certification definition | TrainArr |
| Certification category | TrainArr |
| Governing body | TrainArr |
| Citation mapping/snapshot attached to a training program or certification | TrainArr, by reference to Compliance Core |
| Training program workflow | TrainArr |
| Training step completion | TrainArr |
| Trainer / evaluator signoff | TrainArr |
| Issued certification after training | TrainArr |
| Person certification visibility | StaffArr consumes from TrainArr |
| Manual override assignment to person | StaffArr |
| Override reason / approver / expiration | StaffArr |
| Workforce readiness calculation | StaffArr, using TrainArr records |
| Retraining recommendation | TrainArr |
| Workforce action assignment | StaffArr |

StaffArr may display certifications and qualifications, but TrainArr remains the source of truth for training-derived certification issuance. Compliance Core remains the source of truth for the normalized citation registry, rule packs, jurisdiction metadata, and legal/policy interpretation; TrainArr maps training programs and completed certificates to Compliance Core citation references and preserves point-in-time citation snapshots for defensibility.


### 3.3 StaffArr vs Product Apps

| Concern | Owning Product |
|---|---|
| Asset maintenance work | MaintainArr |
| Driver dispatch / routes | RoutArr |
| Vendors / external parties | SupplyArr or future external-party product |
| Training workflow | TrainArr |
| Platform entitlement | NexArr |
| People, org structure, workforce permission assignment ledger | StaffArr |
| Product permission definitions and server-side enforcement | Owning product |
| Product operational records and workflow decisions | Owning product |

Other products should not maintain their own isolated employee/personnel tables except as local reference/mirror tables needed for performance, audit snapshots, or offline/self-hosted operation.

StaffArr does not decide what a product permission means in the product domain. For example, StaffArr may record that a person has `maintainarr.work_order.close`, but MaintainArr owns the permission definition, the closeout authorization rules, and the backend enforcement that determines whether the action is allowed in context.


---

## 4. Core Product Philosophy

StaffArr should feel like a **workforce command center**, not a generic HR app.

It should be built around practical operational questions:

- Who works here?
- Is this person active?
- What site are they assigned to?
- What department are they in?
- What position do they hold?
- Who do they report to?
- Who reports to them?
- What products can they access?
- What are they allowed to do inside each product?
- Are they qualified for the work they are assigned?
- Are they missing required training or certification?
- Has an incident affected their ability to perform certain work?
- What changed, when did it change, and who approved it?

---

## 5. Major Modules

## 5.1 Dashboard

### End Goal

The StaffArr dashboard should provide a high-confidence snapshot of workforce readiness and people operations across the tenant.

### Features

- Total active people
- Total inactive people
- Pending onboarding count
- People missing required assignments
- People missing required certifications
- Expiring certifications count
- Open personnel incidents
- Incidents requiring supervisor action
- Open retraining recommendations
- Pending manual override reviews
- Site-level staffing readiness
- Department-level readiness
- Product-access readiness summary
- Recent personnel changes
- Recent permission changes
- Recent incident activity
- Recent training/certification changes from TrainArr

### Dashboard Cards

- Workforce Overview
- Readiness Warnings
- Certification Risk
- Permission Risk
- Incident Queue
- Onboarding Queue
- Supervisor Tasks
- Recent Changes

### Filters

- Tenant
- Site
- Department
- Position
- Team
- Manager
- Product
- Active/inactive status
- Readiness state
- Date range

---

## 5.2 People Directory

### End Goal

The People Directory is the primary operational directory for all humans known to the tenant.

### Features

- Search people by name, employee number, email, phone, site, department, position, team, manager, status, or product access
- View active and inactive people
- Filter by site
- Filter by department
- Filter by position
- Filter by team
- Filter by manager
- Filter by product access
- Filter by missing certification
- Filter by expired certification
- Filter by incident status
- Filter by onboarding state
- Bulk select people
- Bulk assign site
- Bulk assign department
- Bulk assign position
- Bulk assign team
- Bulk activate/deactivate where permitted
- Bulk export people list
- Bulk permission assignment where permitted
- Bulk training/certification requirement review

### Directory Columns

- Name
- Person ID
- Employee number
- Status
- Site
- Department
- Position
- Team
- Manager
- Product access
- Readiness status
- Certification status
- Incident status
- Last updated

---

## 5.3 Person Profile

### End Goal

The Person Profile should be the complete operational record for one person across the tenant.

### Profile Sections

- Overview
- Contact information
- Employment information
- Site / department / position / team assignment
- Manager and reporting chain
- Direct reports
- Product access
- Permissions
- Certifications and qualifications
- Training requirements
- Manual overrides
- Incidents
- Personnel notes
- Documents
- Assignment history
- Permission history
- Certification history
- Incident history
- Audit timeline

### Overview Fields

- Full name
- Preferred name
- Person ID from NexArr
- Employee number
- Work email
- Work phone
- Active/inactive status
- Hire date
- Termination date, if applicable
- Primary site
- Primary department
- Primary position
- Primary team
- Manager
- Readiness state
- Compliance warnings
- Product access summary

### Employment Fields

- Employment type
- Full-time / part-time / contractor / temporary
- Job title
- Position assignment
- Department assignment
- Site assignment
- Start date
- End date
- Supervisor
- Work status
- Rehire eligibility flag, if applicable
- Notes

### Person Timeline

The timeline should show every meaningful change, including:

- Person created
- Person activated
- Person deactivated
- Site changed
- Department changed
- Position changed
- Team changed
- Manager changed
- Permission granted
- Permission revoked
- Certification granted by TrainArr
- Certification expired
- Manual override granted
- Manual override revoked
- Incident created
- Incident closed
- Retraining recommended
- Retraining completed
- Documents added
- Notes added

---

## 5.4 Person Creation and Onboarding

### End Goal

StaffArr should provide a guided person creation and onboarding flow that creates the workforce record cleanly and coordinates with NexArr and TrainArr as needed.

### Creation Flow

1. Basic identity
2. Contact details
3. Employment details
4. Site assignment
5. Department assignment
6. Position assignment
7. Team assignment
8. Manager assignment
9. Product access intent
10. Initial permission templates
11. Required training/certification review
12. Documents
13. Review and create

### NexArr Integration

During creation, StaffArr should be able to:

- Search for an existing NexArr person
- Link to an existing `personId`
- Request creation of a new NexArr person
- Specify whether the person should have login capability
- Request a login invite if allowed
- Avoid storing raw passwords
- Display whether the person has a platform login account

### TrainArr Integration

After person creation, StaffArr should be able to:

- Ask TrainArr what certifications/training are required for the person's assignments
- Display missing requirements
- Create training assignment requests where appropriate
- Show whether the person is production-ready
- Track onboarding completion status using TrainArr outputs

---

## 5.5 `/me` Self-Service Portal

### End Goal

The `/me` experience should allow a worker to understand their own status, requirements, assignments, and available actions without giving them broad administrative access.

### Features

- View personal profile summary
- View assigned site, department, position, and team
- View manager
- View direct reports, if supervisor
- View active certifications
- View expiring certifications
- View missing requirements
- View assigned training from TrainArr
- View product access
- View permissions summary in plain English
- Submit personnel update request
- Submit incident / concern report
- Submit document upload, if allowed
- View own incident/report history, where appropriate
- View onboarding checklist
- View readiness status

### Self-Service Actions

- Update phone number request
- Update emergency/contact info request, if modeled
- Upload document
- Acknowledge policy
- Report issue
- Report incident
- Request access
- Request correction
- View assigned tasks

---

## 5.6 Supervisor / Manager View

### End Goal

Managers need a focused workspace for the people they are responsible for.

### Features

- View direct reports
- View indirect reports, if allowed
- View team readiness
- View missing certifications by subordinate
- View expiring certifications by subordinate
- View open incidents involving subordinates
- View onboarding progress
- View pending personnel actions
- Assign team-level notes
- Request access changes
- Request training assignment
- Review subordinate self-service requests
- Escalate incidents
- Approve or deny selected personnel requests
- Export subordinate roster

### Manager Dashboard

- My team headcount
- People not ready for assigned work
- Certifications expiring soon
- Open incidents
- Training pending
- Permission requests pending
- Onboarding status

---

## 5.7 Organization Structure

### End Goal

StaffArr should own the tenant's practical operating structure.

### Structures

- Sites / places / operating locations
- Departments
- Positions
- Teams
- Reporting relationships
- Optional regions/divisions/business units if needed later

### Structure Principles

- Sites represent operating locations or places where work is performed.
- Departments represent functional groups.
- Positions represent work roles or job functions.
- Teams represent practical reporting or crew structures.
- Manager hierarchy represents accountability and approval flow.

---

## 5.8 Sites / Places

### End Goal

Sites are the operating places that products can reference consistently.

### Features

- Create site
- Edit site
- Deactivate site
- Parent/child site structure, if needed
- Site code
- Site name
- Address
- Time zone
- Operational status
- Assigned departments
- Assigned teams
- Assigned people
- Product availability by site
- Site-level readiness dashboard
- Site-level incident dashboard
- Site-level permission defaults

### Site Relationships

- A person may have a primary site.
- A person may have secondary site access.
- A department may exist at one or many sites.
- A team may be assigned to a site.
- Products may use site references without owning the site table.

---

## 5.9 Departments

### End Goal

Departments organize people by function.

### Features

- Create department
- Edit department
- Deactivate department
- Department code
- Department name
- Parent department
- Assigned site or global department flag
- Department manager
- Assigned positions
- Assigned teams
- Assigned people
- Department-level readiness dashboard
- Department-level permission templates
- Department-level training requirement review

Examples:

- Maintenance
- Transportation
- Warehouse
- Production
- Safety
- Compliance
- Human Resources
- Administration

---

## 5.10 Positions

### End Goal

Positions define the operational role a person fills and drive permissions, training requirements, and readiness checks.

### Features

- Create position
- Edit position
- Deactivate position
- Position code
- Position title
- Department relationship
- Site relationship, if site-specific
- Position description
- Default permission templates
- Required certifications from TrainArr
- Required training from TrainArr
- Product access recommendations
- Supervisor eligibility flag
- Safety-sensitive flag
- Regulated role flag
- Driver role flag
- Technician role flag
- Inspector role flag
- Trainer/evaluator role flag
- Position readiness rules

Examples:

- Fleet Technician
- Lead Mechanic
- Maintenance Planner
- Transportation Manager
- Driver
- Dispatcher
- Warehouse Associate
- Safety Coordinator
- Compliance Manager
- Trainer
- Evaluator

---

## 5.11 Teams and Crews

### End Goal

Teams represent practical working groups and reporting groups.

### Features

- Create team
- Edit team
- Deactivate team
- Team name
- Team code
- Team type
- Assigned site
- Assigned department
- Team lead
- Members
- Shift relationship, if modeled
- Team readiness view
- Team incident view
- Team permission templates

### Team Types

- Maintenance crew
- Driver group
- Warehouse team
- Compliance team
- Training cohort
- Temporary project team
- Emergency response team

---

## 5.12 Manager-Subordinate Hierarchy

### End Goal

StaffArr should provide a clear hierarchy view for accountability, approval routing, and people management.

### Features

- Assign manager
- Change manager
- Show direct reports
- Show indirect reports
- Show reporting tree
- Show dotted-line relationships, if supported
- Prevent circular reporting chains
- Effective-dated manager assignments
- Manager change history
- Approval routing based on hierarchy
- Incident escalation based on hierarchy
- Training approval based on hierarchy
- Permission request approval based on hierarchy

### UI Views

- Org chart view
- Tree view
- List view
- Manager profile view
- My team view
- Site hierarchy view
- Department hierarchy view

---

## 5.13 Permissions and Access Assignments

### End Goal

StaffArr should own the person-to-permission assignment ledger while NexArr owns authentication and product entitlement, and each product owns its permission definitions and server-side enforcement.

StaffArr answers:

> **Inside this tenant and product, what is this person allowed to do?**

### Permission Concepts

- Product access visibility
- Product role
- Product permission
- Permission template
- Assignment scope
- Site-scoped permission
- Department-scoped permission
- Team-scoped permission
- Global tenant permission
- Manual override
- Expiring permission assignment
- Emergency access assignment

### Features

- View product access summary
- Assign product role
- Remove product role
- Assign granular permission
- Remove granular permission
- Assign permission template
- Create permission template
- Edit permission template
- Deactivate permission template
- Scope permission by product
- Scope permission by site
- Scope permission by department
- Scope permission by team
- Scope permission by person
- Expiration date for permission
- Approval requirement for sensitive permission
- Permission history
- Permission conflict warnings
- Permission export
- Permission review campaign

### Product Permission Boundary

Product permission examples in StaffArr are assignment targets, not StaffArr-owned domain rules. Each product publishes or documents the permission keys it understands. StaffArr assigns those keys to people, scopes them by site/department/team/person when appropriate, records approvals and history, and exposes assignment checks. The product backend must still enforce every sensitive action using its own domain rules.

### Product Examples

MaintainArr permissions may include:

- View assets
- Create work orders
- Close work orders
- Approve repairs
- Perform inspections
- Manage PM schedules
- Manage parts requests
- View maintenance reports

RoutArr permissions may include:

- View routes
- Dispatch routes
- Assign drivers
- Assign vehicles
- Close trips
- Manage driver exceptions
- View transportation reports

TrainArr permissions may include:

- View training
- Create training programs
- Assign training
- Sign as trainer
- Sign as evaluator
- Issue manual remediation request
- View training reports

StaffArr permissions may include:

- View people
- Create people
- Edit people
- Deactivate people
- Manage sites
- Manage departments
- Manage positions
- Manage teams
- Manage permissions
- View incidents
- Manage incidents
- Run audits

---

## 5.14 Permission Templates

### End Goal

Permission templates reduce repetitive setup and keep roles consistent.

### Features

- Create template
- Clone template
- Edit template
- Retire template
- Assign template to position
- Assign template to department
- Assign template to team
- Assign template to person
- Preview permissions before applying
- Compare template against person's current permissions
- Detect drift from template
- Reapply template
- Remove template-derived permissions
- Require approval for high-risk template

### Template Examples

- Fleet Technician
- Lead Mechanic
- Maintenance Supervisor
- Driver
- Dispatcher
- Warehouse Associate
- Safety Manager
- Compliance Admin
- Site Manager

---

## 5.15 Certifications and Qualifications Visibility

### End Goal

StaffArr should display and act on workforce qualification data without stealing TrainArr ownership.

### Features

- View certification status by person
- View certifications by site
- View certifications by department
- View certifications by position
- View certifications by team
- View missing certifications
- View expired certifications
- View expiring soon certifications
- View suspended qualifications
- View revoked qualifications
- View training-derived certifications from TrainArr
- View manual overrides from StaffArr
- Show source of authority
- Show expiration
- Show evidence link, if available
- Show governing body
- Show legal citation reference, sourced from TrainArr’s program/certification mapping and backed by Compliance Core
- Show related product permissions
- Show readiness impact

### Readiness States

- Ready
- Ready with warning
- Not ready
- Expired
- Missing requirement
- Suspended
- Pending training
- Pending evaluation
- Pending signoff
- Manual override active
- Manual override expired

---

## 5.16 Manual Overrides

### End Goal

StaffArr should allow controlled, auditable manual authorization overrides without undermining TrainArr or Compliance Core.

A StaffArr manual override is a personnel authorization exception. It is not a TrainArr-issued training completion and it is not a Compliance Core waiver against a legal/policy rule. If the exception requires a compliance waiver, StaffArr should reference the Compliance Core waiver instead of pretending the workforce override alone clears the rule.

### Features

- Create override
- Select person
- Select certification/qualification/authorization being overridden
- Attach reason
- Attach approver
- Set effective date
- Set expiration date
- Attach document/evidence
- Define scope
- Limit to site/department/product/work type
- Require second approval for high-risk override
- Notify Compliance Core, if applicable
- Notify TrainArr, if training-related
- Show override in readiness calculations
- Revoke override
- Expire override automatically
- Audit all override changes

### Override Rules

- Overrides should be visible, not hidden.
- Overrides should be time-limited by default.
- Overrides should have a reason.
- Overrides should have an approver.
- Overrides should not create TrainArr-issued certifications.
- Overrides should not impersonate training completion.
- Overrides should be treated as exceptions, not normal records.

---

## 5.17 Incidents

### End Goal

StaffArr should own personnel incident cases, workforce incident intake, and personnel-related incident routing.

Incidents may originate in StaffArr or be reported by other products, but the originating product keeps ownership of its domain incident record. StaffArr receives or creates the personnel case when a person, workforce status, conduct issue, qualification concern, corrective action, or personnel history impact is involved.

### Incident Ownership Split

| Incident kind | Owning record |
|---|---|
| Driver behavior, harassment, attendance, conduct, personnel issue | StaffArr |
| Route, trip, stop, load, dispatch, or transportation exception | RoutArr |
| Equipment defect, breakdown, repair issue, maintenance abuse finding | MaintainArr |
| Supplier, vendor, purchasing, receiving, inventory, or stockout issue | SupplyArr |
| Training failure, remediation assignment, qualification suspension/revocation | TrainArr |
| Rule violation, compliance evaluation, waiver, or formal compliance determination | Compliance Core |


### Incident Sources

- StaffArr self-report
- StaffArr supervisor report
- MaintainArr equipment/repair incident
- RoutArr transportation/driver incident
- TrainArr training/evaluation incident
- SupplyArr vendor/customer interaction incident
- Compliance Core rule violation
- API/service event

### Incident Types

- Safety incident
- Conduct incident
- Training concern
- Certification concern
- Equipment abuse concern
- Driver behavior concern
- Policy violation
- Attendance/workforce issue
- Qualification issue
- Near miss
- Injury report
- Harassment concern
- Supervisor escalation
- Other

### Features

- Create incident
- Receive incident from product event
- Attach involved people
- Attach reporting person
- Attach affected site
- Attach department
- Attach product source
- Attach asset/trip/work order reference from source product
- Classify severity
- Classify category
- Assign owner
- Assign due date
- Track status
- Add notes
- Attach documents
- Add corrective action
- Add retraining recommendation
- Route training-related incident to TrainArr
- Route compliance-related incident to Compliance Core
- Close incident
- Reopen incident
- Audit incident timeline

### Incident Statuses

- Draft
- Submitted
- Triage
- Needs review
- Assigned
- In progress
- Waiting on training review
- Waiting on compliance review
- Corrective action pending
- Closed
- Reopened
- Voided

---

## 5.18 Incident Routing to TrainArr

### End Goal

When an incident may affect certification, qualification, training, or authorization, StaffArr should notify TrainArr for evaluation.

### Routing Triggers

- Incident type is training-related
- Incident type is certification-related
- Supervisor marks retraining required
- Compliance Core rule requires retraining review
- Repeated incidents exceed threshold
- Safety-sensitive incident occurred
- Driver/technician qualification is questioned
- Evaluation failure was reported

### TrainArr Response Expected

TrainArr may return:

- No training action required
- Retraining recommended
- Retraining required
- Evaluation required
- Certification suspended
- Certification revoked
- Certification remains valid
- New training assignment created
- Remediation completed

### StaffArr Responsibilities

- Display TrainArr decision
- Update readiness state
- Notify manager
- Add timeline entry
- Keep incident linked to training decision
- Preserve audit trail

---

## 5.19 Personnel History and Audit Package

### End Goal

StaffArr should generate a complete person history/audit package for compliance, management review, litigation defense, internal investigation, or operational review.

StaffArr owns the personnel audit package. Product-owned raw evidence remains in the product that created it; StaffArr should include product references, snapshots, or attached copies only where needed for the person history.

### Audit Package Contents

- Person identity summary
- Employment status history
- Site assignment history
- Department assignment history
- Position assignment history
- Team assignment history
- Manager history
- Product access history
- Permission assignment history
- Certification/qualification history from TrainArr
- Manual override history
- Training incident history
- Personnel incident history
- Documents
- Notes, if permitted
- Approval history
- Relevant product references
- Timeline of major events

### Export Formats

- PDF
- Markdown
- CSV for tabular sections
- JSON for system export

### Audit Requirements

- Include generated timestamp
- Include generated by user/person
- Include tenant
- Include date range
- Include source systems
- Include legal/compliance disclaimer if needed
- Preserve immutable event references

---

## 5.20 Documents

### End Goal

StaffArr should store or reference workforce-related documents while allowing document ownership rules to remain clean.

### Features

- Upload document
- Link external document
- Attach document to person
- Attach document to incident
- Attach document to override
- Attach document to onboarding step
- Attach document to personnel note
- Document type classification
- Expiration date
- Review date
- Access control
- Version history
- Document audit log

### Document Types

- Employment document
- Policy acknowledgement
- License copy
- Medical card copy
- Certification evidence
- Training evidence
- Incident evidence
- Corrective action document
- Authorization approval
- Other

---

## 5.21 Notes

### End Goal

StaffArr should support operational notes without becoming an uncontrolled dumping ground.

### Features

- Add note to person
- Add note to incident
- Add note to override
- Add note to onboarding record
- Mark note type
- Mark note sensitivity
- Limit note visibility
- Audit note creation
- Audit note edits
- Soft-delete/void note with reason

### Note Types

- General
- Supervisor note
- Compliance note
- Safety note
- Training note
- Permission note
- Incident note
- Onboarding note

---

## 5.22 Onboarding

### End Goal

StaffArr should coordinate onboarding from person creation through readiness for productive work.

### Features

- Onboarding templates by position
- Onboarding templates by department
- Onboarding templates by site
- Checklist steps
- Required documents
- Required acknowledgements
- Required product access
- Required training from TrainArr
- Required certifications from TrainArr
- Manager assignment
- Equipment assignment reference, if sourced from MaintainArr
- Badge/access request, if modeled
- Completion percentage
- Blockers
- Due dates
- Overdue warnings
- Onboarding completion approval

### Onboarding States

- Not started
- In progress
- Waiting on documents
- Waiting on training
- Waiting on certification
- Waiting on manager approval
- Ready for work
- Completed
- Cancelled

---

## 5.23 Offboarding

### End Goal

StaffArr should coordinate removal from active workforce operations while preserving history.

### Features

- Start offboarding
- Set separation date
- Set separation reason, if allowed
- Remove product permissions
- Notify NexArr to disable login, if appropriate
- Remove from active teams
- End position assignment
- End department assignment
- End site assignment
- Close open personnel tasks
- Reassign direct reports
- Reassign incident ownership
- Preserve historical records
- Generate offboarding checklist
- Mark inactive

### Offboarding Checklist Examples

- Disable login
- Remove product access
- Reassign manager responsibilities
- Reassign open incidents
- Recover documents/equipment, if modeled
- Close onboarding/training assignments
- Preserve audit record

---

## 5.24 Workforce Readiness Engine

### End Goal

StaffArr should calculate whether a person is ready for specific work based on assignments, permissions, status, certifications, incidents, and overrides.

### Inputs

- Active/inactive status
- Position
- Department
- Site
- Team
- Product access
- Product permissions
- Required certifications from TrainArr
- Certification status from TrainArr
- Manual overrides
- Open incidents
- Compliance Core rules
- Product-specific requirements

### Outputs

- Ready
- Not ready
- Warning
- Missing requirement
- Expired requirement
- Blocked by incident
- Blocked by inactive status
- Blocked by missing product access
- Blocked by missing permission
- Override required
- Override active

### Readiness Views

- Person readiness
- Position readiness
- Team readiness
- Department readiness
- Site readiness
- Product readiness
- Work-type readiness

---

## 5.25 Product Integration Directory

### End Goal

StaffArr should expose clean APIs/events so other products can depend on workforce records without duplicating ownership.

### Consuming Products

- MaintainArr
- RoutArr
- TrainArr
- SupplyArr
- Compliance Core
- Future Arr products

### StaffArr Should Provide

- Person lookup
- Person active status
- Person assignment summary
- Site lookup
- Department lookup
- Position lookup
- Team lookup
- Manager lookup
- Permission check
- Bulk permission check
- Product role summary
- Certification/readiness summary
- Incident submission endpoint
- Event subscription feed

### Products Should Send to StaffArr

- Incident created
- Personnel-related exception
- Product permission request
- Product access usage, if needed
- Person-related operational status change
- Relevant audit event

---

## 5.26 Event System

### End Goal

StaffArr should communicate major workforce changes through events instead of direct cross-database coupling.

### Events StaffArr Emits

- `person.created`
- `person.updated`
- `person.activated`
- `person.deactivated`
- `person.assignment.changed`
- `person.manager.changed`
- `site.created`
- `site.updated`
- `department.created`
- `department.updated`
- `position.created`
- `position.updated`
- `team.created`
- `team.updated`
- `permission.assigned`
- `permission.revoked`
- `permission.template.applied`
- `override.created`
- `override.revoked`
- `incident.created`
- `incident.updated`
- `incident.closed`
- `readiness.changed`

### Events StaffArr Consumes

From TrainArr:

- `certification.issued`
- `certification.expired`
- `certification.revoked`
- `training.assigned`
- `training.completed`
- `training.failed`
- `retraining.required`
- `evaluation.completed`

From MaintainArr:

- `maintenance.incident.created`
- `workorder.person.involved`
- `inspection.person.involved`
- `asset.abuse.flagged`

From RoutArr:

- `driver.incident.created`
- `route.exception.created`
- `driver.assignment.requested`
- `trip.person.involved`

From Compliance Core:

- `rule.violation.detected`
- `compliance.action.required`
- `compliance.review.completed`

---

## 5.27 API Surface

### End Goal

StaffArr should expose a stable `/api/v1` surface for people, org structure, permissions, incidents, and readiness.

### Core API Groups

- `/api/v1/people`
- `/api/v1/me`
- `/api/v1/sites`
- `/api/v1/departments`
- `/api/v1/positions`
- `/api/v1/teams`
- `/api/v1/hierarchy`
- `/api/v1/permissions`
- `/api/v1/permission-templates`
- `/api/v1/certifications`
- `/api/v1/readiness`
- `/api/v1/incidents`
- `/api/v1/overrides`
- `/api/v1/onboarding`
- `/api/v1/offboarding`
- `/api/v1/documents`
- `/api/v1/audit`
- `/api/v1/integrations`
- `/api/v1/events`
- `/api/v1/reports`

### API Requirements

- Tenant-scoped access
- Service-token support
- User-token support
- Product-to-product authorization
- Idempotent integration endpoints where appropriate
- Pagination
- Filtering
- Sorting
- Search
- Audit metadata
- Validation errors in consistent format
- No direct cross-product database foreign keys
- Local reference IDs for external product references

---

## 5.28 Suggested Data Model

### Core Tables

- `people`
- `person_profiles`
- `person_contact_methods`
- `person_employment_records`
- `person_assignments`
- `person_status_history`
- `sites`
- `departments`
- `positions`
- `teams`
- `team_memberships`
- `manager_assignments`
- `permission_definitions`
- `permission_templates`
- `permission_template_items`
- `person_permission_assignments`
- `person_product_access`
- `manual_overrides`
- `incidents`
- `incident_people`
- `incident_references`
- `incident_actions`
- `onboarding_records`
- `onboarding_steps`
- `offboarding_records`
- `documents`
- `person_documents`
- `notes`
- `audit_events`
- `external_references`
- `trainarr_certification_refs`
- `readiness_snapshots`

### Important Reference Fields

- `tenantId`
- `personId`
- `nexarrPersonId`
- `siteId`
- `departmentId`
- `positionId`
- `teamId`
- `managerPersonId`
- `sourceProduct`
- `sourceRecordType`
- `sourceRecordId`
- `effectiveFrom`
- `effectiveTo`
- `createdByPersonId`
- `updatedByPersonId`

---

## 5.29 Reporting

### End Goal

StaffArr should provide practical workforce and compliance reporting.

### Reports

- Active workforce roster
- Inactive workforce roster
- Site roster
- Department roster
- Position roster
- Team roster
- Manager/subordinate report
- Product access report
- Permission assignment report
- Permission drift report
- Certification readiness report
- Missing certification report
- Expiring certification report
- Manual override report
- Incident report
- Open incident report
- Training-related incident report
- Onboarding status report
- Offboarding status report
- Personnel change report
- Audit package report

### Export Formats

- CSV
- PDF
- Markdown
- JSON

---

## 5.30 Search

### End Goal

StaffArr search should make people and workforce records easy to find.

### Search Targets

- People
- Employee numbers
- Sites
- Departments
- Positions
- Teams
- Managers
- Permissions
- Certifications
- Incidents
- Documents
- Notes, if allowed

### Search Features

- Global search
- Scoped search
- Typeahead people picker
- Advanced filters
- Saved filters
- Recent searches
- Search result permissions trimming

---

## 5.31 Notifications and Tasks

### End Goal

StaffArr should create action visibility around workforce problems.

### Notification Types

- Certification expiring
- Certification expired
- Missing requirement
- Permission request pending
- Override expiring
- Override expired
- Incident assigned
- Incident overdue
- Onboarding task due
- Offboarding task due
- Manager approval needed
- TrainArr remediation required
- Compliance Core action required

### Task Types

- Review person
- Approve permission
- Review override
- Assign manager
- Complete onboarding step
- Complete offboarding step
- Review incident
- Attach document
- Request training
- Review readiness blocker

---

## 5.32 UI / Navigation

### End Goal

StaffArr UI should be intuitive, operational, and clearly separated from other product domains.

### Primary Navigation

- Dashboard
- People
- My Team
- `/me`
- Sites
- Departments
- Positions
- Teams
- Permissions
- Certifications
- Incidents
- Onboarding
- Offboarding
- Reports
- Settings
- Audit

### Important UI Principles

- StaffArr should not look like MaintainArr with people pasted into it.
- Avoid cross-product bleed in the main navigation.
- Product-specific details should appear as references, not owned pages.
- A person profile may show MaintainArr/RoutArr/TrainArr references, but actions should deep-link to the owning product when appropriate.
- Permission and readiness explanations should be plain English.
- Hierarchy should be easy to understand visually.
- `/me` should be simplified and worker-friendly.
- Manager view should focus on action, not administration clutter.

---

## 5.33 Settings

### End Goal

StaffArr settings should configure people operations without undermining NexArr, TrainArr, or other product ownership.

### Settings Areas

- Person fields
- Employee number rules
- Site settings
- Department settings
- Position settings
- Team settings
- Permission templates
- Approval workflows
- Incident categories
- Incident severity levels
- Onboarding templates
- Offboarding templates
- Readiness rules
- Notification rules
- Integration settings
- Audit retention
- Document settings

---

## 5.34 Compliance Core Integration

### End Goal

Compliance Core should use StaffArr data to evaluate people-related regulatory and policy rules, but StaffArr should remain the owner of people operations.

### StaffArr Provides to Compliance Core

- Person status
- Role/position
- Site/department/team
- Permission assignments
- Training/certification visibility
- Incident history references
- Manual override references

### Compliance Core Provides to StaffArr

- Rule evaluation results
- Compliance warnings
- Required actions
- Rule violation events
- Human-readable rule explanations
- Citation references

### Example Flow

If a person is assigned as a regulated driver, Compliance Core may evaluate whether required credentials exist. StaffArr displays the result and blocks or warns on readiness, while TrainArr remains the source for training/certification issuance and NexArr remains the source for authentication.

---

## 5.35 Security and Access Control

### End Goal

StaffArr must protect sensitive workforce records and avoid privilege escalation.

### Requirements

- Tenant isolation
- Product entitlement check through NexArr
- User authorization through StaffArr permissions
- Service authorization for product-to-product calls
- No frontend-only enforcement for critical rules
- Server-side permission checks
- Audit all permission changes
- Audit all manual overrides
- Audit all incident changes
- Sensitive note/document access controls
- Manager-scoped access for subordinate records
- Platform admin access only where appropriate
- No backdoor product-level admin paths
- No bypass of NexArr login gate
- No local auth competing with NexArr

---

## 5.36 Self-Hosted / Data Plane Considerations

### End Goal

StaffArr should be compatible with the broader control-plane/data-plane architecture where NexArr remains the lean platform/control plane and product data may optionally live in customer-managed environments.

### Requirements

- Treat customer-hosted product data as untrusted input
- Validate all external references
- Use signed service calls
- Avoid direct cross-product database foreign keys
- Maintain local reference/mirror tables as needed
- Preserve tenant isolation
- Support event replay/idempotency where possible
- Support audit exports
- Make integrations resilient to temporary product outages

---

## 6. Key Workflows

## 6.1 Hire / Create Person Workflow

1. Admin opens StaffArr person creation flow.
2. StaffArr searches NexArr for existing person.
3. Admin links or creates NexArr `personId`.
4. Admin sets whether person should have login capability.
5. Admin enters employment details.
6. Admin assigns site, department, position, team, and manager.
7. StaffArr applies default permission templates.
8. StaffArr asks TrainArr for required training/certifications.
9. StaffArr creates onboarding checklist.
10. StaffArr displays readiness blockers.
11. Manager and/or admin completes onboarding actions.
12. TrainArr issues certifications after training completion.
13. StaffArr updates readiness.
14. Person becomes ready for assigned work.

---

## 6.2 Manager Reviews Team Readiness

1. Manager opens My Team.
2. StaffArr lists direct reports.
3. StaffArr highlights missing certifications, incidents, and permission issues.
4. Manager opens a person.
5. Manager sees readiness blockers.
6. Manager requests training, permission correction, or incident review.
7. StaffArr routes actions to owning product when needed.
8. Readiness updates when blockers are resolved.

---

## 6.3 Product Requests Person Authorization

1. MaintainArr, RoutArr, or another product asks StaffArr whether a person can perform an action.
2. StaffArr validates tenant, person, active status, assignment, permissions, and readiness.
3. StaffArr may check cached or live TrainArr certification state.
4. StaffArr returns allowed/denied/warning with reasons.
5. Product enforces the result server-side.
6. StaffArr records authorization event if required.

---

## 6.4 Incident From Another Product

1. RoutArr records a driver incident or MaintainArr records a technician incident.
2. Source product emits incident event or calls StaffArr incident endpoint.
3. StaffArr creates personnel incident.
4. StaffArr links source product record.
5. StaffArr assigns owner based on manager/site/department rules.
6. StaffArr determines whether TrainArr review is needed.
7. TrainArr evaluates retraining/certification impact.
8. StaffArr updates person readiness and incident status.
9. Incident is resolved and preserved in person history.

---

## 6.5 Manual Override Workflow

1. Admin identifies missing/expired requirement or special authorization case.
2. Admin opens person profile.
3. Admin creates manual override.
4. StaffArr requires reason, scope, approver, effective date, and expiration date.
5. StaffArr logs override.
6. StaffArr updates readiness with visible override state.
7. StaffArr notifies Compliance Core or TrainArr if relevant.
8. Override expires or is revoked.
9. StaffArr preserves override history.

---

## 6.6 Offboarding Workflow

1. Admin or manager starts offboarding.
2. StaffArr sets planned separation date.
3. StaffArr lists access, permissions, direct reports, open incidents, and open tasks.
4. Admin reassigns responsibilities.
5. StaffArr removes or schedules removal of permissions.
6. StaffArr notifies NexArr to disable login if appropriate.
7. StaffArr marks person inactive.
8. StaffArr preserves complete history.

---

## 7. Completion Criteria

StaffArr can be considered **functionally complete** when it can do the following end-to-end:

### 7.1 People and Org Completion

- Create and manage people linked to NexArr `personId`
- Track active/inactive status
- Assign site, department, position, team, and manager
- Display hierarchy accurately
- Show `/me` profile
- Show manager subordinate view
- Preserve assignment history

### 7.2 Permissions Completion

- Define permissions
- Define permission templates
- Assign permissions to people
- Scope permissions by product/site/department/team
- Expose permission checks to products
- Audit all permission changes
- Prevent frontend-only privilege escalation

### 7.3 TrainArr Integration Completion

- Display TrainArr certification definitions/statuses
- Display required training/certification blockers
- Receive certification/training events
- Route training-related incidents to TrainArr
- Show retraining/remediation result
- Maintain StaffArr-owned manual overrides separately

### 7.4 Incident Completion

- Create StaffArr incidents
- Receive incidents from other products
- Link involved people
- Assign ownership
- Track status/actions
- Route training-related incidents to TrainArr
- Update readiness based on incidents
- Preserve incident history

### 7.5 Readiness Completion

- Calculate person readiness
- Calculate team readiness
- Calculate site readiness
- Explain blockers in plain English
- Include status, permissions, certifications, incidents, and overrides
- Expose readiness API to products

### 7.6 Audit Completion

- Maintain person timeline
- Maintain permission history
- Maintain assignment history
- Maintain incident history
- Maintain override history
- Generate person audit package
- Export key reports

### 7.7 Integration Completion

- No direct cross-product database foreign keys
- API/event integration with NexArr
- API/event integration with TrainArr
- API/event integration with MaintainArr/RoutArr as incident and authorization consumers
- Service-token protected product-to-product calls
- Local reference/mirror records where needed

---

## 8. Non-Goals

StaffArr should **not** own:

- Platform login credentials
- Tenant entitlement
- Product subscription state
- Training program workflow
- Certification issuance from completed training
- Maintenance work orders
- Assets
- Routes
- Dispatch
- Vendors/customers
- Legal rule authoring
- Compliance rule engine ownership

StaffArr may reference or display those things where useful, but the owning product remains responsible for the authoritative record.

---

## 9. V1 Build Priorities

A practical V1 should focus on:

1. NexArr-linked people records
2. Sites, departments, positions, teams
3. Manager hierarchy
4. People directory
5. Person profile
6. `/me` profile
7. Manager/subordinate view
8. Permission templates and assignments
9. TrainArr certification visibility
10. Manual overrides
11. Incident intake
12. Incident routing to TrainArr
13. Readiness calculation
14. Audit timeline
15. Product-facing person/permission/readiness APIs

---


---

## Audit-Informed Feature Additions: Platform Access, Ownership, and Verification

These additions are part of the product feature set. They are not optional implementation notes.

### NexArr Launch and Product Session Contract

Protected product experiences must use the platform launch pattern:

1. User starts in NexArr.
2. NexArr validates login, tenant status, product status, entitlement, callback allowlist, and launch state.
3. NexArr redirects to the product callback path: `/auth/nexarr/callback`.
4. The product backend redeems the handoff code server-side.
5. The product creates a local product session containing at minimum `personId`, `tenantId`, `productCode`, entitlement snapshot, and session expiry.
6. The product then applies its own server-side domain authorization rules.

### Required Access Features

- `/auth/nexarr/callback` route in the product frontend and backend.
- Server-side handoff redemption.
- Expired, reused, missing, wrong-product, and invalid-callback handoff rejection.
- Friendly launch failure, entitlement denied, invalid callback, product unavailable, and tenant selection states.
- Product session hydration endpoint.
- Product logout or session clear behavior that does not create a competing login system.
- Quick-switch menu that reads NexArr catalog data and sends users back through NexArr `/launch/{productCode}`.
- Tenant context display sourced from the validated product session.
- Current user display sourced from `personId` and product/session data.
- No product-generated trusted launch URLs.
- No product-side entitlement guessing.
- No product-owned platform login.

### Authority and Safety Rules

- Frontend hiding is not authorization.
- No production feature may rely on localStorage admin switches, mock users, hardcoded role strings, fake permission strings, or frontend-only entitlement checks.
- Development-only identity or permission shortcuts must be guarded by `VITE_APP_ENV=development` and must not ship as production fallbacks.
- Product APIs must validate tenant, session, entitlement, product permission, and record ownership server-side.
- Cross-product records must use APIs, events, service tokens, local mirrors, snapshots, or external references. No direct cross-product database foreign keys.
- Product switchers and shared shells are visual/structural only; they do not centralize product-specific authorization.

### Feature Verification Standard

A feature is complete only when there is concrete implementation evidence:

- Backend route/service/model/schema where applicable.
- Frontend route/page/component/API client where applicable.
- Persistence where the feature implies stored data.
- Authorization where the feature implies protected access.
- Cross-product contract where the feature depends on another product.
- Tests or smoke checks where practical.

TODO text, mock-only state, placeholder UI, documentation-only claims, sample data, or frontend-only screens do not count as completed features.

---

## Audit-Informed Feature Additions: Workforce Authority and Product Consumption

### Product Permission Catalog Visibility

StaffArr owns the assignment ledger, templates, scopes, approvals, and history. Each product still owns its permission definitions and server-side enforcement.

Features:

- Product permission catalog import/sync from each product.
- Permission key display by product, scope type, sensitivity, and description.
- Assignment workflows that reference product-owned permission keys.
- Scoped assignment support by tenant/site/department/team/person/product where applicable.
- Approval and expiration rules for sensitive permissions.
- Assignment history and revocation history.
- Product-facing permission-check API.
- Clear “StaffArr assigned” vs “Product enforced” explanation.

Completion criteria:

- StaffArr can show and assign product permissions without redefining what those permissions mean inside the product.

### Workforce Readiness API

StaffArr should expose readiness to products as a stable contract.

Features:

- Person readiness endpoint by `personId`.
- Team/site/department readiness summaries.
- Readiness reason codes.
- Active/inactive workforce state.
- Permission status.
- Manual authorization overrides.
- TrainArr certification/qualification status snapshot.
- Incident/restriction blockers.
- Manager/supervisor context.
- Staleness/confidence metadata.
- Audit snapshot support.

Completion criteria:

- MaintainArr, RoutArr, TrainArr, SupplyArr, and Compliance Core can ask whether a person is workforce-ready without each product recreating personnel logic.

### Product-Owned Incident Routing

StaffArr should intake workforce-related incidents without taking ownership of every domain incident.

Features:

- Receive product-originated incident reference.
- Link involved people by `personId`.
- Classify workforce impact.
- Route retraining needs to TrainArr.
- Route maintenance/equipment facts back to MaintainArr.
- Route dispatch facts back to RoutArr.
- Track StaffArr-owned personnel actions separately from product-owned incident resolution.

Completion criteria:

- A transportation, maintenance, training, or supply incident can affect workforce readiness without StaffArr swallowing the entire domain workflow.


## 10. Final Vision Statement

StaffArr is complete when every Arr product can confidently ask it:

> **Who is this person, are they active, where do they belong, what are they allowed to do, are they ready for this work, and what history proves it?**

At that point, StaffArr becomes the operational people layer of the STL Compliance platform: not just a directory, not just HR, and not just permissions, but the workforce readiness backbone that lets MaintainArr, RoutArr, TrainArr, SupplyArr, Compliance Core, and future products operate from the same trusted personnel reality.
