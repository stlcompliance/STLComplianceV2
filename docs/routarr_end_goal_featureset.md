# RoutArr End Goal and Granular Feature Set

## 1. Product End Goal

**RoutArr** is the transportation execution product in the STL Compliance / Arr ecosystem. Its end goal is to become the system of record for dispatch, route planning, trip execution, driver/equipment assignment, stop workflows, transportation exceptions, proof collection, driver-facing work, and transportation operational reporting.

RoutArr should feel like a modern dispatch command center built for real-world fleet operations: simple enough for a dispatcher to run today’s routes, deep enough for a compliance/safety team to prove what happened, and integrated enough that it does not become another disconnected transportation silo.

RoutArr does **not** replace NexArr, StaffArr, MaintainArr, TrainArr, SupplyArr, or Compliance Core. It owns the transportation execution domain and consumes platform data from the correct owning products.

---

## 2. Plain-English Mission

RoutArr answers:

- What needs moved?
- Who is driving?
- What equipment is assigned?
- What route or trip is planned?
- What stops are required?
- What is the current trip status?
- What went wrong?
- Was proof collected?
- Was the driver qualified and available?
- Was the equipment available and fit for dispatch?
- What compliance records are created by transportation activity?

RoutArr should make dispatchers, drivers, supervisors, and compliance teams faster without hiding the operational truth.

---

## 3. Ownership Boundaries

### RoutArr Owns

RoutArr owns transportation execution records and workflows, including:

- Routes
- Trips
- Dispatch boards
- Driver assignments
- Vehicle/equipment assignments for trips
- Stop plans
- Stop execution
- Pickup and delivery workflows
- Proof of pickup
- Proof of delivery
- Transportation exceptions
- Dispatch notes
- Route status
- Trip status
- Load movement status
- Driver route work queues
- Dispatch communication history
- Transportation-specific operational reporting
- Transportation events emitted to other products
- Transportation workflow gates and product enforcement of Compliance Core outcomes; not legal/rule-pack ownership

### RoutArr Does Not Own

RoutArr should not own platform identity, people master data, personnel records, maintenance records, training/certification records, or platform-level compliance rule governance.

Those belong to:

| Product | Owns |
|---|---|
| **NexArr** | Platform identity, login, tenants, product entitlement, service tokens, product access |
| **StaffArr** | People, employment/person status, org structure, sites, departments, teams, positions, person permissions, person history |
| **TrainArr** | Training workflows, evaluations, completions, issued certifications/qualifications |
| **MaintainArr** | Assets, equipment records, inspections, defects, PM, work orders, maintenance readiness |
| **SupplyArr** | Vendors, customers, external parties, purchasing/supply records, external business entities |
| **Compliance Core** | Compliance rule normalization, rule packs, legal/regulatory mappings, platform compliance evaluation patterns |

---

## 4. Ecosystem Role

RoutArr is the **dispatch execution system**.

It should sit between real-world operations and the rest of the platform:

```text
NexArr
  validates identity, tenant, entitlement, product access

StaffArr
  provides people, drivers, permissions, org structure, active/inactive status

TrainArr
  provides certifications, qualifications, training-derived authorization

MaintainArr
  provides equipment/assets, readiness, defects, inspections, maintenance status

Compliance Core
  provides rule packs, rule definitions, legal mappings, compliance evaluation patterns

RoutArr
  plans, dispatches, executes, tracks, proves, reports, and emits transportation events
```

---

## 5. Core User Roles

RoutArr should support role-based experiences for:

### Driver

The driver sees only the work they need to perform.

Driver-facing features include:

- Today’s assigned trips
- Upcoming trips
- Route details
- Stop-by-stop workflow
- Navigation handoff
- Pickup confirmation
- Delivery confirmation
- Photos/documents/signatures
- Exception reporting
- Delay reporting
- Equipment issue reporting
- Accident/incident reporting
- Messaging with dispatch
- Completion history
- Personal route performance summary

### Dispatcher

The dispatcher manages live transportation execution.

Dispatcher features include:

- Dispatch board
- Route creation
- Trip creation
- Driver assignment
- Equipment assignment
- Stop sequencing
- Drag-and-drop scheduling
- Live route status
- Exception triage
- Driver communication
- Reassignment workflows
- Late route monitoring
- Missed stop monitoring
- Proof review
- Daily dispatch closeout

### Transportation Supervisor

The supervisor oversees operational performance and exceptions.

Supervisor features include:

- Route health dashboard
- Driver workload view
- Exception review
- Incident escalation
- Driver utilization
- Equipment utilization
- Compliance risk flags
- Missed SLA reporting
- Corrective action handoff
- Daily/weekly performance review

### Safety / Compliance User

The safety/compliance user reviews transportation records for auditability.

Compliance features include:

- HOS/short-haul tracking support where applicable
- Driver qualification checks through StaffArr/TrainArr
- Equipment readiness checks through MaintainArr
- Incident and exception review
- Proof archive
- Route audit trail
- Driver dispatch history
- Equipment dispatch history
- Compliance Core rule evaluation results
- Exportable audit packets

### Platform Admin

Platform admins manage cross-product access from NexArr, not from RoutArr directly.

RoutArr may display platform-derived information but must not become the owner of platform administration.

---

## 6. Primary Product Areas

A complete RoutArr should include the following product areas:

1. Dispatch Command Center
2. Route Planning
3. Trip Execution
4. Stop Management
5. Driver Portal
6. Equipment Assignment
7. Load / Movement Records
8. Proof Workflows
9. Exception Management
10. Compliance Checks
11. Short-Haul / Time Tracking Support
12. Incident Reporting
13. Driver and Equipment Availability
14. Customer / Location Handling
15. Communication and Notes
16. Reporting and Analytics
17. Audit Trail
18. Cross-Product Events
19. Admin / Configuration
20. API and Integration Layer

---

# 7. Granular Feature Set

## 7.1 Dispatch Command Center

### End Goal

Dispatchers should be able to run transportation operations from one primary board.

### Features

- Daily dispatch board
- Weekly dispatch board
- Route calendar
- Driver availability panel
- Equipment availability panel
- Unassigned work queue
- Assigned trip list
- Active trip map/list
- Late trip highlighting
- At-risk trip highlighting
- Completed trip list
- Canceled trip list
- Exception queue
- Drag-and-drop assignment
- Filter by site
- Filter by department
- Filter by customer
- Filter by route type
- Filter by driver
- Filter by vehicle/equipment
- Filter by status
- Filter by priority
- Filter by date range
- Search by route/trip/load/customer/location
- Dispatch notes panel
- Live status refresh
- Manual status override with reason
- Bulk assignment actions
- Bulk unassignment actions
- Bulk status updates
- Dispatch closeout workflow
- Daily operational summary

### Completion Criteria

RoutArr is not complete until a dispatcher can open one screen and understand:

- What is unassigned
- What is assigned
- What is active
- What is late
- What is blocked
- What needs human attention
- What has been completed

---

## 7.2 Route Planning

### End Goal

RoutArr should support creating reusable route plans and one-off operational routes.

### Features

- Create route template
- Create one-time route
- Route name
- Route code/number
- Route type
- Route priority
- Route effective dates
- Route active/inactive status
- Home site/base location
- Planned start time
- Planned end time
- Planned duration
- Planned mileage
- Route notes
- Default driver
- Default equipment
- Default stop sequence
- Recurring route schedule
- Route cloning
- Route version history
- Route archive
- Route cancellation
- Route import
- Route export
- Route optimization support
- Manual stop ordering
- Time-window-aware stop ordering
- Distance-aware stop ordering
- Service-time-aware planning
- Route conflict warnings
- Duplicate route detection

### Route Types

Suggested route types:

- Local delivery
- Regional delivery
- Linehaul
- Shuttle
- Milk run
- Pickup route
- Return route
- Transfer route
- Yard move
- Dedicated customer route
- On-demand route
- Emergency route
- Compliance-sensitive route

---

## 7.3 Trip Execution

### End Goal

A trip is the executable instance of planned transportation work.

### Features

- Create trip from route template
- Create ad hoc trip
- Trip number
- Trip status
- Trip priority
- Assigned driver
- Assigned vehicle
- Assigned trailer
- Assigned secondary equipment
- Planned departure time
- Actual departure time
- Planned arrival time
- Actual arrival time
- Planned completion time
- Actual completion time
- Planned mileage
- Actual mileage
- Planned duration
- Actual duration
- Dispatch release workflow
- Driver acceptance workflow
- Start trip workflow
- Pause trip workflow
- Resume trip workflow
- Complete trip workflow
- Cancel trip workflow
- Failed trip workflow
- Return-to-base workflow
- Reassignment workflow
- Split trip workflow
- Merge trip workflow
- Trip notes
- Trip attachments
- Trip audit log
- Trip event timeline

### Trip Statuses

Recommended canonical statuses:

- Draft
- Planned
- Ready for Dispatch
- Dispatched
- Accepted by Driver
- En Route to First Stop
- In Progress
- Delayed
- Blocked
- Exception
- Returning
- Completed
- Canceled
- Failed
- Closed

---

## 7.4 Stop Management

### End Goal

RoutArr should manage the stop-level reality of transportation work.

### Features

- Create stop
- Edit stop
- Delete/cancel stop
- Reorder stops
- Stop type
- Stop sequence number
- Planned arrival
- Actual arrival
- Planned departure
- Actual departure
- Service time estimate
- Actual service time
- Customer/location reference
- Address
- Dock/location notes
- Contact person
- Contact phone
- Contact email
- Required proof type
- Pickup workflow
- Delivery workflow
- Transfer workflow
- Inspection/check workflow
- Wait time tracking
- Detention time tracking
- Refusal workflow
- Partial delivery workflow
- Damaged goods workflow
- Missed stop workflow
- Stop exception workflow
- Stop completion notes
- Stop photos
- Stop documents
- Stop signature
- Stop geofence check
- Stop timestamp history

### Stop Types

Suggested stop types:

- Pickup
- Delivery
- Pickup and delivery
- Transfer
- Fuel
- Inspection
- Scale
- Wash
- Yard
- Break
- Customer check-in
- Return to base
- Maintenance stop
- Emergency stop

---

## 7.5 Driver Portal

### End Goal

Drivers should have a simple, mobile-first workflow that keeps dispatch records accurate without turning the driver into a data-entry clerk.

### Features

- Driver dashboard
- Today’s assigned trips
- Upcoming assigned trips
- Trip details
- Stop-by-stop task list
- Accept assignment
- Reject/decline assignment with reason, if permitted
- Start trip
- Arrive at stop
- Complete stop
- Capture signature
- Upload proof photo
- Upload document
- Report delay
- Report exception
- Report equipment issue
- Report accident/incident
- Message dispatch
- View dispatch notes
- Complete trip
- View completed trip history
- Offline-tolerant form entry
- Resync pending entries
- Driver notifications
- Driver acknowledgement prompts
- Simple “/me” experience backed by StaffArr identity

### Driver Portal Principles

- Minimal clicks
- Large buttons
- Mobile-first layout
- Clear current action
- Clear next stop
- Clear completion state
- No platform-admin controls
- No access to unrelated drivers’ work unless explicitly permitted

---

## 7.6 Equipment Assignment

### End Goal

RoutArr should assign equipment to trips while respecting asset readiness from MaintainArr.

### Features

- Assign vehicle
- Assign trailer
- Assign secondary trailer
- Assign dolly/converter gear
- Assign forklift/MHE if needed
- Assign special equipment
- View equipment availability
- View equipment readiness
- View open defects from MaintainArr
- View inspection status from MaintainArr
- View PM due/overdue status from MaintainArr
- Block dispatch when equipment is not dispatchable, if rules require
- Warn dispatch when equipment has open non-blocking issues
- Equipment assignment conflict detection
- Double-booking detection
- Equipment swap workflow
- Equipment release workflow
- Equipment utilization tracking
- Equipment trip history
- Equipment-specific notes

### Equipment Readiness States

RoutArr should consume or mirror readiness states from MaintainArr, such as:

- Available
- Assigned
- Out of service
- Maintenance hold
- Inspection due
- Defect reported
- Defect blocking
- PM due soon
- PM overdue
- Retired/inactive

RoutArr should not become the system of record for maintenance state.

---

## 7.7 Driver Assignment and Availability

### End Goal

RoutArr should assign qualified, active, available people to driving work by consuming StaffArr and TrainArr data.

### Features

- Driver assignment
- Driver availability view
- Active/inactive person check from StaffArr
- Site/department/team check from StaffArr
- Position/role check from StaffArr
- Driver permission check from StaffArr
- Certification/qualification check from TrainArr
- Expired certification warning
- Missing certification blocking
- Medical card expiration warning, if applicable
- License class warning, if applicable
- Endorsement warning, if applicable
- Training requirement warning
- Driver schedule conflict detection
- Driver workload view
- Driver reassignment
- Driver swap workflow
- Helper/passenger assignment
- Team-driver support
- Supervisor override workflow with reason
- Driver assignment audit trail

### Driver Qualification Principle

RoutArr may evaluate whether a person is eligible for a trip, but the underlying person, permission, and certification records must remain owned by StaffArr and TrainArr.

---

## 7.8 Load / Movement Records

### End Goal

RoutArr should track what is being moved without becoming a full ERP, WMS, or TMS unless intentionally expanded.

### Features

- Load/movement record
- Load number
- Reference number
- Customer reference
- Bill of lading reference
- Purchase order reference
- Shipment reference
- Commodity/category
- Load description
- Weight
- Pieces/pallets
- Temperature requirement
- Hazmat indicator
- Special handling notes
- Pickup location
- Delivery location
- Required pickup window
- Required delivery window
- Load status
- Attach load to trip
- Attach load to stop
- Split load across stops
- Multiple loads per trip
- Load proof records
- Load exception records
- Load cancellation
- Load completion
- Load history

### Load Statuses

Recommended load statuses:

- Draft
- Ready
- Planned
- Assigned
- Picked Up
- In Transit
- Partially Delivered
- Delivered
- Refused
- Damaged
- Returned
- Canceled
- Closed

---

## 7.9 Proof Workflows

### End Goal

RoutArr should prove what happened at each stop and preserve that evidence.

### Features

- Proof of pickup
- Proof of delivery
- Signature capture
- Photo capture
- Document upload
- Barcode/QR scan support
- Seal number entry
- Temperature reading entry
- Weight/ticket number entry
- Timestamp capture
- Geolocation capture, where enabled
- Manual proof entry
- Proof review queue
- Proof rejection workflow
- Proof correction workflow
- Missing proof warning
- Required proof enforcement
- Proof packet export
- Proof archive by trip/load/stop/customer
- Proof tamper/audit metadata

### Proof Types

Suggested proof types:

- Signature
- Photo
- Document
- Barcode scan
- QR scan
- Seal number
- Temperature reading
- Weight ticket
- Checklist
- Driver attestation
- Dispatcher confirmation
- Customer confirmation

---

## 7.10 Exception Management

### End Goal

RoutArr should make operational problems visible, actionable, and reportable.

### Features

- Create exception
- Driver-reported exception
- Dispatcher-created exception
- System-generated exception
- Exception type
- Exception severity
- Exception status
- Exception owner
- Exception due date
- Exception notes
- Exception attachments
- Exception escalation
- Exception resolution
- Corrective action handoff
- Related trip
- Related stop
- Related load
- Related driver
- Related equipment
- Related customer/location
- Exception audit trail
- Exception reporting

### Exception Types

Suggested exception types:

- Late departure
- Late arrival
- Missed pickup
- Missed delivery
- Refused delivery
- Damaged product
- Short product
- Over product
- Wrong product
- Customer unavailable
- Address issue
- Access issue
- Dock delay
- Weather delay
- Traffic delay
- Accident
- Breakdown
- Equipment defect
- Driver unavailable
- Paperwork issue
- Missing proof
- Compliance hold
- Safety hold
- Customer complaint
- Other

### Exception Severity

Suggested severities:

- Informational
- Low
- Medium
- High
- Critical

---

## 7.11 Compliance Checks

### End Goal

RoutArr should support transportation compliance checks without becoming the platform-wide compliance rule owner.

### Features

- Pre-dispatch compliance check
- Driver eligibility check
- Equipment readiness check
- Route compliance check
- Load compliance check
- Document completeness check
- Proof completeness check
- Hours/time availability check
- Short-haul status support
- Exception-based compliance flags
- Compliance Core rule result display
- Compliance override workflow
- Supervisor override reason
- Compliance event emission
- Audit packet generation

### Example Checks

RoutArr should be able to ask and display answers like:

- Is this person active?
- Is this person allowed to drive?
- Is this person assigned to a role that permits dispatch?
- Does this person have required training/certification?
- Is this vehicle available?
- Is this vehicle out of service?
- Does this vehicle have blocking defects?
- Is a required inspection overdue?
- Is the trip missing required proof?
- Is the route assigned to a driver who has a schedule conflict?
- Is the load marked hazmat and missing required authorization?
- Does the route require a compliance-sensitive workflow?

### Important Boundary

Compliance Core owns normalized rule definitions and legal mappings. RoutArr owns the transportation facts and workflow outcomes that those rules evaluate.

---

## 7.12 Short-Haul and Time Tracking Support

### End Goal

RoutArr should support operational time tracking for short-haul-style workflows where appropriate, without pretending to be an ELD unless intentionally certified and designed for that purpose.

### Features

- Driver on-duty time entry
- Driver off-duty time entry
- Dispatch release timestamp
- Trip start timestamp
- Trip end timestamp
- Stop arrival/departure timestamps
- Break tracking
- Workday summary
- Timecard-style export
- Short-haul eligibility flag
- Short-haul exception flag
- Radius/operating area support
- Return-to-base confirmation
- Daily driver route/time summary
- Supervisor review
- Audit trail of edits
- Edit reason required
- Manual correction workflow
- Compliance Core rule hooks
- ELD integration placeholder
- External timekeeping integration placeholder

### Non-Goal Unless Explicitly Built

RoutArr should not market itself as an ELD unless the product is intentionally designed, tested, certified, and operated as one.

---

## 7.13 Incident Reporting

### End Goal

RoutArr should allow transportation incidents to be reported and routed to the correct owning products.

### Features

- Driver incident report
- Dispatcher incident report
- Supervisor incident report
- Accident report
- Near miss report
- Injury report
- Property damage report
- Cargo damage report
- Customer complaint
- Equipment abuse report
- Safety concern
- Harassment/HR concern handoff to StaffArr
- Training-related incident handoff to TrainArr
- Equipment-related incident handoff to MaintainArr
- Compliance-related incident handoff to Compliance Core
- Incident severity
- Incident attachments
- Incident timeline
- Incident review status
- Corrective action linkage
- Incident event emission
- Audit record

### Incident Ownership Principle

RoutArr may originate transportation incidents, but the originating transportation incident remains a RoutArr record when it is tied to a trip, stop, load, route, dispatcher action, driver workflow, proof issue, or transportation exception. RoutArr should route linked cases or events to the owning product when the incident affects another domain:

| Incident impact | Owning product / record |
|---|---|
| Equipment defect, breakdown, inspection issue, or repair need | MaintainArr |
| Driver behavior, harassment, attendance, conduct, personnel issue | StaffArr |
| Retraining, reevaluation, qualification suspension, or remediation | TrainArr |
| Rule violation, compliance waiver, formal compliance determination | Compliance Core |
| Route/trip/stop/load execution issue | RoutArr |

The routed product should receive references back to the RoutArr incident instead of RoutArr giving up its transportation history.

---

## 7.14 Customer, Vendor, and Location Handling

### End Goal

RoutArr should support route execution against external parties and locations without prematurely owning every external-party master record if SupplyArr is intended to own that later.

### Features

- Customer reference
- Vendor reference
- External party reference
- Pickup location
- Delivery location
- Yard location
- Site location
- Contact records
- Dock notes
- Delivery instructions
- Location access notes
- Geofence metadata
- Location active/inactive status
- Location-specific proof requirements
- Location-specific appointment windows
- Location-specific equipment restrictions
- Location-specific safety instructions
- External-party sync hooks with SupplyArr
- Local reference/mirror tables where needed

### Boundary

SupplyArr owns external-party master records: customer/vendor identity, contacts, business relationship, documents, approval status, and external-party compliance. StaffArr owns internal operating sites/places. RoutArr owns trip and stop execution snapshots: the stop address used, instructions at time of dispatch, proof requirements for that stop, appointment windows, geofence result, and route execution history.

RoutArr can maintain operational location details required for dispatch, but long-term vendor/customer master data should belong to SupplyArr. RoutArr should store local references or immutable snapshots when dispatch history must remain accurate even if the SupplyArr master record later changes.

---

## 7.15 Communication and Notes

### End Goal

RoutArr should preserve dispatch communication context around trips and exceptions.

### Features

- Trip notes
- Stop notes
- Load notes
- Driver notes
- Dispatcher notes
- Internal-only notes
- Driver-visible notes
- Customer-visible note flag, if needed
- Message thread per trip
- Message thread per exception
- Driver-dispatch messaging
- Broadcast message to assigned drivers
- Acknowledgement-required messages
- Message read status
- Message attachment
- Note templates
- Communication audit trail

---

## 7.16 Notifications and Alerts

### End Goal

RoutArr should alert the right people before problems become invisible failures.

### Features

- New assignment notification
- Assignment changed notification
- Trip dispatched notification
- Trip accepted notification
- Driver late to start alert
- Stop late alert
- Route delayed alert
- Exception created alert
- Exception escalated alert
- Proof missing alert
- Equipment blocked alert
- Driver qualification issue alert
- Compliance hold alert
- Route canceled notification
- Reassignment notification
- Daily dispatch summary
- End-of-day incomplete route alert
- Notification preferences
- In-app notifications
- Email notification support
- SMS/push placeholder if desired later

---

## 7.17 Reporting and Analytics

### End Goal

RoutArr should show operational performance, compliance exposure, and execution quality.

### Reports

- Daily dispatch report
- Route performance report
- Driver performance report
- Equipment utilization report
- Stop performance report
- On-time pickup report
- On-time delivery report
- Exception report
- Missed stop report
- Proof compliance report
- Late route report
- Canceled trip report
- Driver workload report
- Equipment assignment report
- Customer/location performance report
- Detention/wait time report
- Short-haul/time summary report
- Compliance hold report
- Override report
- Audit trail report

### Dashboard Metrics

- Trips planned today
- Trips dispatched today
- Trips in progress
- Trips completed
- Trips late
- Trips blocked
- Open exceptions
- Critical exceptions
- Missing proof count
- Driver availability
- Equipment availability
- On-time pickup percentage
- On-time delivery percentage
- Average stop service time
- Average delay time
- Dispatch completion percentage
- Compliance holds by type

---

## 7.18 Audit Trail

### End Goal

Every meaningful transportation action should be explainable later.

### Features

- Entity-level audit trail
- Trip timeline
- Stop timeline
- Assignment history
- Status change history
- Proof capture history
- Exception history
- Override history
- Driver action history
- Dispatcher action history
- System-generated event history
- Before/after values
- Actor personId
- Actor display name mirror
- Timestamp
- Source system
- Reason/comment where required
- Exportable audit trail

### Audited Events

At minimum, audit:

- Route created/updated/canceled
- Trip created/updated/canceled
- Driver assigned/unassigned
- Equipment assigned/unassigned
- Trip dispatched
- Trip accepted
- Trip started
- Stop arrived
- Stop completed
- Proof uploaded
- Proof rejected/corrected
- Exception created/escalated/resolved
- Compliance hold applied/released
- Override performed
- Trip completed
- Trip closed

---

## 7.19 Cross-Product Events

### End Goal

RoutArr should participate in the Arr ecosystem through APIs and events, not direct cross-database foreign keys.

### Event Examples

RoutArr should emit events such as:

- `routarr.route.created`
- `routarr.route.updated`
- `routarr.trip.created`
- `routarr.trip.dispatched`
- `routarr.trip.accepted`
- `routarr.trip.started`
- `routarr.trip.completed`
- `routarr.trip.canceled`
- `routarr.stop.arrived`
- `routarr.stop.completed`
- `routarr.proof.created`
- `routarr.exception.created`
- `routarr.exception.resolved`
- `routarr.incident.created`
- `routarr.driver.assignment.changed`
- `routarr.equipment.assignment.changed`
- `routarr.compliance.hold.created`
- `routarr.compliance.hold.released`

### Events RoutArr May Consume

RoutArr may consume events such as:

- `staffarr.person.created`
- `staffarr.person.updated`
- `staffarr.person.deactivated`
- `staffarr.permission.changed`
- `staffarr.org.changed`
- `trainarr.certification.issued`
- `trainarr.certification.expired`
- `trainarr.training.required`
- `maintainarr.asset.created`
- `maintainarr.asset.updated`
- `maintainarr.asset.out_of_service`
- `maintainarr.asset.returned_to_service`
- `maintainarr.defect.created`
- `maintainarr.defect.closed`
- `compliancecore.rulepack.updated`
- `compliancecore.evaluation.changed`

---

## 7.20 API Surface

### End Goal

RoutArr should expose a clean API that supports its UI, integrations, and cross-product communication.

### Suggested API Areas

```text
/api/v1/health
/api/v1/me
/api/v1/dashboard
/api/v1/routes
/api/v1/route-templates
/api/v1/trips
/api/v1/stops
/api/v1/loads
/api/v1/dispatch-board
/api/v1/driver-work
/api/v1/equipment-assignments
/api/v1/driver-assignments
/api/v1/proofs
/api/v1/exceptions
/api/v1/incidents
/api/v1/compliance-checks
/api/v1/availability
/api/v1/reports
/api/v1/audit
/api/v1/events
/api/v1/config
```

### API Requirements

- Tenant-scoped endpoints
- NexArr service-token validation
- PersonId-based actor identity
- No direct cross-product database foreign keys
- Local mirror/reference tables for external product data
- Idempotent event handling
- Pagination
- Filtering
- Sorting
- Search
- Consistent error format
- OpenAPI documentation
- Request correlation IDs
- Audit logging
- Role/permission enforcement
- Product entitlement enforcement through NexArr

---

## 7.21 Data Model Areas

### Core Entities

Suggested RoutArr-owned entities:

- RouteTemplate
- RouteTemplateStop
- Route
- Trip
- TripStop
- TripLoad
- LoadMovement
- DriverAssignment
- EquipmentAssignment
- StopProof
- ProofAttachment
- TripException
- TripIncident
- DispatchNote
- DispatchMessage
- ComplianceCheckResult
- ComplianceHold
- TimeEntry
- DriverWorkDay
- RouteAuditEvent
- TripAuditEvent
- LocalPersonRef
- LocalAssetRef
- LocalLocationRef
- LocalExternalPartyRef
- IntegrationEventInbox
- IntegrationEventOutbox

### Local Reference Tables

RoutArr should keep local references/mirrors for external product records where needed.

Examples:

- `staff_person_ref`
- `staff_org_unit_ref`
- `staff_site_ref`
- `trainarr_certification_ref`
- `maintainarr_asset_ref`
- `maintainarr_asset_readiness_ref`
- `supplyarr_external_party_ref`
- `compliance_rule_ref`

These references should not pretend to be the source of truth.

---

## 7.22 Admin and Configuration

### End Goal

RoutArr admins should configure transportation behavior without gaining platform-admin power.

### Features

- Route type configuration
- Stop type configuration
- Exception type configuration
- Proof requirement configuration
- Dispatch status configuration
- Driver assignment rules
- Equipment assignment rules
- Default route settings
- Default trip settings
- Default proof settings
- Time tracking settings
- Short-haul support settings
- Compliance check settings
- Notification settings
- Report settings
- Integration settings
- Location defaults
- Customer/location proof defaults
- Override reason configuration
- Audit retention settings
- Import/export configuration

### Admin Boundary

RoutArr admin configuration must not allow users to bypass:

- NexArr authentication
- NexArr entitlement
- StaffArr person ownership
- TrainArr certification ownership
- MaintainArr asset readiness ownership
- Compliance Core rule ownership

---

# 8. UI Completion Checklist

## 8.1 Required Main Navigation

A mature RoutArr UI should include:

- Dashboard
- Dispatch Board
- Routes
- Trips
- Stops
- Loads
- Driver Work
- Equipment
- Exceptions
- Incidents
- Proof Review
- Compliance
- Reports
- Audit
- Configuration

## 8.2 Dashboard Widgets

- Today’s dispatch status
- Active trips
- Late trips
- Open exceptions
- Missing proof
- Driver availability
- Equipment availability
- Compliance holds
- On-time performance
- Recently completed trips
- Attention-needed queue

## 8.3 Dispatch Board UI

- Kanban/list/calendar hybrid
- Unassigned work lane
- Planned lane
- Dispatched lane
- In-progress lane
- Delayed/exception lane
- Completed lane
- Drag assignment
- Driver filter
- Equipment filter
- Site filter
- Date filter
- Exception side panel
- Trip detail drawer

## 8.4 Driver UI

- Mobile-first
- Today view
- Current trip card
- Next stop card
- Start/arrive/complete buttons
- Proof capture
- Exception button
- Equipment issue button
- Contact dispatch button
- Offline pending changes indicator

---

# 9. Security and Authorization

## End Goal

RoutArr should enforce product-level authorization while relying on NexArr for platform access.

### Requirements

RoutArr owns RoutArr permission codes and server-side enforcement. StaffArr owns which people are assigned those permission codes and scopes; NexArr owns login, tenant membership, entitlement, and product launch.

- NexArr login required
- Product entitlement required
- Tenant membership required
- Service-token validation for product-to-product calls
- PersonId-based actor context
- StaffArr-backed permission checks
- Route-level permission checks
- Dispatch permission checks
- Driver self-scope restrictions
- Supervisor team/department scope
- Site-level scoping
- Tenant isolation
- No frontend-only authorization
- Server-side enforcement for every sensitive action
- Audit all overrides
- Audit all permission-sensitive actions

### Example Permissions

- `routarr.dashboard.view`
- `routarr.dispatch.view`
- `routarr.dispatch.manage`
- `routarr.route.create`
- `routarr.route.update`
- `routarr.route.cancel`
- `routarr.trip.create`
- `routarr.trip.dispatch`
- `routarr.trip.reassign`
- `routarr.trip.cancel`
- `routarr.driver_work.view_self`
- `routarr.driver_work.execute_self`
- `routarr.proof.review`
- `routarr.exception.create`
- `routarr.exception.resolve`
- `routarr.incident.create`
- `routarr.compliance.view`
- `routarr.compliance.override`
- `routarr.reports.view`
- `routarr.audit.view`
- `routarr.config.manage`

---

# 10. Compliance and Legal Posture

## End Goal

RoutArr should support compliance workflows honestly and defensibly.

### Requirements

- Clear distinction between operational tracking and legally certified systems
- No false ELD claims unless intentionally built/certified as such
- Audit trail for manual time edits
- Driver qualification checks from authoritative systems
- Equipment readiness checks from authoritative systems
- Compliance Core integration for legal/rule mappings
- Exportable supporting records
- Configurable rule packs
- Override reason requirements
- Human review queues
- Immutable evidence metadata where practical
- Clear disclaimer in admin configuration where workflows are operational support, not legal advice

---

# 11. MVP Scope

## MVP Goal

The RoutArr MVP should prove the core dispatch loop:

```text
Create route/trip
Assign driver
Assign equipment
Dispatch trip
Driver executes stops
Collect proof
Report exception
Complete trip
Review audit/report
```

## MVP Features

- NexArr-authenticated access
- Tenant-scoped API
- StaffArr person reference support
- MaintainArr asset reference support
- Dashboard
- Dispatch board
- Route CRUD
- Trip CRUD
- Stop CRUD
- Driver assignment
- Equipment assignment
- Driver work view
- Start/complete trip
- Arrive/complete stop
- Basic proof upload
- Basic exception reporting
- Basic audit log
- Basic reports
- Configuration for route/stop/exception types
- OpenAPI documentation
- Event outbox/inbox foundation

## MVP Non-Goals

- Full route optimization
- Full ELD replacement
- Full TMS billing
- Full customer portal
- Full vendor/customer master data ownership
- Advanced AI dispatching
- Complex telematics integration
- Deep WMS/ERP integration

---

# 12. V1 Completion Scope

## V1 Goal

V1 should make RoutArr a functional transportation execution product for real dispatch operations.

## V1 Features

- Full dispatch board
- Route templates
- Recurring routes
- Trip execution
- Driver mobile portal
- Proof workflows
- Exception management
- Driver/equipment availability
- Compliance check display
- Compliance holds
- Supervisor overrides
- Audit timeline
- Core reporting
- Cross-product references
- Integration events
- Notification system
- Configuration UI
- Role/permission enforcement

---

# 13. V2 / Advanced Scope

## V2 Goal

V2 should make RoutArr intelligent, scalable, and deeply integrated.

## Advanced Features

- Route optimization
- Predictive delay warnings
- Telematics integration
- GPS breadcrumbs
- Geofence automation
- Customer portal
- Appointment scheduling
- Dock scheduling
- Advanced detention tracking
- Load tendering
- Carrier/third-party support
- Advanced driver scheduling
- Advanced HOS/ELD integrations
- AI-assisted dispatch recommendations
- Automated reassignment suggestions
- Voice-guided driver workflows
- Barcode/QR proof workflows
- Offline-first mobile app
- Advanced audit packet builder
- Cross-product compliance investigation views

---

# 14. Definition of Complete

RoutArr can be considered “complete” when it can reliably support this end-to-end operational reality:

1. A dispatcher creates or imports transportation work.
2. RoutArr turns that work into trips, stops, loads, and assignments.
3. RoutArr verifies that the driver is active, available, and qualified through StaffArr/TrainArr.
4. RoutArr verifies that equipment is available and dispatchable through MaintainArr.
5. RoutArr applies transportation compliance checks through Compliance Core where applicable.
6. The dispatcher releases the trip.
7. The driver receives and executes the trip from a mobile-first interface.
8. Each stop records timestamps, status, proof, exceptions, and notes.
9. Exceptions are routed to the correct owner.
10. Incidents are emitted to StaffArr, TrainArr, MaintainArr, or Compliance Core when appropriate.
11. The trip can be completed, closed, reviewed, and exported.
12. Supervisors can see performance, delays, exceptions, proof gaps, and compliance holds.
13. Auditors can reconstruct what happened from records, timestamps, actors, evidence, and override reasons.
14. RoutArr remains inside its ownership boundary and does not undermine the platform architecture.

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

## Audit-Informed Feature Additions: Dispatch Gates and External References

### Driver and Equipment Reference Contract

RoutArr owns dispatch assignment records, but not the master records for people or assets.

Features:

- Driver references use NexArr/StaffArr `personId`.
- Driver display data is read from StaffArr or local StaffArr reference snapshots.
- Driver active/inactive and assignment readiness come from StaffArr/TrainArr checks.
- Vehicle/equipment references use MaintainArr asset references.
- Equipment readiness and dispatchability come from MaintainArr readiness APIs.
- Product snapshots preserve what RoutArr knew at dispatch time without becoming source-of-truth master records.

Completion criteria:

- RoutArr can prove who and what was assigned to a trip while StaffArr and MaintainArr remain authoritative for people and assets.

### Dispatch Release Gate

RoutArr should not release transportation work until required product checks pass or are explicitly overridden with authority.

Gate inputs:

- Driver active status from StaffArr.
- Driver qualification/training status from TrainArr.
- Driver permission/readiness from StaffArr.
- Equipment readiness from MaintainArr.
- Route/trip compliance outcomes from Compliance Core.
- Customer/location/vendor restrictions from SupplyArr where applicable.

Features:

- Pre-dispatch validation panel.
- Allow/warn/block/review result display.
- Missing/stale data handling.
- Override request with reason and approver where permitted.
- Immutable dispatch-release snapshot.
- Audit trail showing which checks were evaluated.

Completion criteria:

- A dispatcher can tell why a route is releasable, blocked, risky, or waiting for review before dispatch.

### Transportation Event Publishing

RoutArr should emit events for other products without letting them own dispatch.

Events:

- Trip created/released/started/completed/cancelled.
- Driver assigned/unassigned.
- Equipment assigned/unassigned.
- Stop completed/failed/missed.
- Proof captured/rejected.
- Driver-reported defect.
- Transportation incident.
- Route exception.
- Compliance hold/override.

Completion criteria:

- MaintainArr, StaffArr, TrainArr, SupplyArr, and Compliance Core can react to transportation events through contracts, not direct database access.


# 15. Product Success Standard

RoutArr succeeds when dispatch no longer lives in scattered texts, whiteboards, spreadsheets, memory, and disconnected systems.

It should become the operational truth for transportation execution:

- Dispatchers know what is happening.
- Drivers know what to do next.
- Supervisors know what needs attention.
- Compliance users can prove what happened.
- Other products receive the transportation events they need.
- The platform stays clean because each product owns the right thing.

RoutArr’s job is not simply to draw routes on a map.

RoutArr’s job is to turn transportation work into controlled, provable, compliant execution.
