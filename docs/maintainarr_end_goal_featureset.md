# MaintainArr End Goal and Granular Feature Set

## MaintainArr end goal

**MaintainArr should be the maintenance execution system for the Arr ecosystem.** Its end-state job is to know every maintainable asset, its condition, its defects, its inspection history, its PM state, its repair history, its downtime, its cost, and whether it is safe, compliant, and ready to operate.

It should not just be “work orders with inspections.” It should become the system that turns maintenance facts into operational readiness.

In plain English:

> **MaintainArr owns assets, maintenance, inspections, defects, PM, work orders, maintenance evidence, asset readiness, and maintenance cost.**

It should answer:

- Can this asset be used?
- Why or why not?
- What is due?
- What is broken?
- Who touched it?
- What did they find?
- What was repaired?
- What did it cost?
- What evidence proves it?
- What risk remains?
- What needs to happen next?

---

## 1. Product ownership boundaries

### MaintainArr owns

#### Core ownership

- Assets
- Asset maintenance profile
- Asset condition
- Asset status/readiness
- Inspections
- Inspection templates/runs/results
- Defects
- Work orders
- PM programs/schedules/events
- Maintenance labor records
- Maintenance cost records
- Maintenance attachments/evidence
- Maintenance history
- Maintenance audit packages
- Asset downtime
- Asset meter history
- Maintenance-related compliance evidence

### MaintainArr references from other products

#### From NexArr

- Tenant validity
- Product entitlement
- Platform identity validation
- Service-to-service trust
- Platform person identity via `personId`

#### From StaffArr

- People
- Sites
- Departments
- Teams
- Positions
- Person active/inactive status
- Permission assignments
- Supervisor/manager relationships

#### From TrainArr

- Training completion
- Certifications
- Qualifications
- Authorization to perform certain maintenance/inspection tasks

#### From RoutArr

- Dispatch status
- Route/trip assignment
- Asset usage context
- Driver-reported defects
- DVIR-related operational events

#### From SupplyArr

- Vendors
- Dealers
- Parts catalog
- Purchase orders
- Inventory availability
- Vendor documents
- Pricing/lead time snapshots

#### From Compliance Core

- Rule evaluations
- Legal citations
- Compliance requirements
- Regulatory applicability
- Rule-pack versions
- Evidence requirements

---

## 2. Major end-state modules

### A. Asset registry

MaintainArr should have a full asset registry for anything that requires maintenance.

#### Asset types

- Semi tractors
- Straight trucks
- Box trucks
- Pickup trucks
- Vans
- Trailers
- Tankers
- Reefers
- Forklifts
- Powered pallet jacks
- Yard trucks
- Construction equipment
- MHE
- Shop equipment
- Facilities equipment
- Tools requiring inspection/calibration
- Non-powered assets
- Custom tenant-defined asset types

#### Asset identity fields

- MaintainArr `assetId`
- Tenant ID reference
- StaffArr site reference
- Asset number/unit number
- VIN/serial number
- License plate
- USDOT/regulated flag references
- GVWR/GCWR
- Make
- Model
- Year
- Trim/configuration
- Engine
- Fuel type
- Transmission
- Axle configuration
- Brake type
- Body type
- Ownership type
- Lease/rental status
- Warranty status
- In-service date
- Out-of-service date
- Disposal/sale date

#### Asset classification

- Asset domain
- Asset class
- Asset type
- Asset subtype
- Maintenance category
- Regulatory category
- Inspection category
- PM family
- Cost center reference
- Site ownership
- Department/team reference

#### Asset configuration

- Number of axles
- Drive axle configuration
- Tandem/single axle
- Duals/super singles
- Drum/disc brakes
- Sleeper/day cab
- CNG/diesel/gas/electric/hybrid
- Reefer equipped
- Liftgate equipped
- PTO equipped
- Hydraulic system equipped
- DEF equipped
- Air brake equipped
- ABS equipped
- ELD-required asset flag/reference
- DOT-regulated asset flag/reference

---

## 3. Asset lifecycle

### Asset onboarding

- Create asset manually
- Import assets from CSV
- Import from telematics
- Import from existing EAM/CMMS
- VIN decode support
- Duplicate detection
- Asset number conflict detection
- Required field validation by asset type
- Regulatory applicability hints
- Initial PM baseline setup
- Initial inspection requirement setup
- Initial meter reading capture
- Initial document upload

### Asset lifecycle states

- Pending setup
- Active
- Available
- Assigned
- In use
- Due soon
- PM due
- Inspection due
- Defect open
- Restricted use
- Out of service
- In shop
- Waiting on parts
- Waiting on approval
- Retired
- Sold/disposed

### Asset readiness

MaintainArr should calculate a clear asset readiness result:

- Ready
- Ready with warning
- Restricted
- Not ready
- Out of service
- Unknown / insufficient data

Readiness should consider:

- Open critical defects
- Open safety defects
- Overdue PM
- Overdue inspection
- Failed inspection
- Missing required documents
- Expired certifications/calibrations
- Compliance Core rule failures
- Active work order status
- Manual manager hold
- Telematics fault severity
- RoutArr dispatch needs

---

## 4. Meter and usage tracking

### Meter types

- Odometer
- Engine hours
- Idle hours
- PTO hours
- Cycle count
- Fuel usage
- Reefer hours
- Hydraulic hours
- Custom meter types

### Meter capture sources

- Manual entry
- Inspection answer
- Work order entry
- Telematics import
- RoutArr trip completion
- Fuel transaction
- Bulk import
- API event

### Meter intelligence

- Meter history
- Meter correction workflow
- Rollover handling
- Bad reading detection
- Impossible jump detection
- Missing reading alerts
- Estimated usage
- Telematics-vs-manual variance
- PM forecast based on usage velocity
- “Due soon” prediction
- Confidence score for PM calculations

---

## 5. Inspections

This should be one of MaintainArr’s strongest modules.

### Inspection template builder

- Asset-type-specific templates
- Configurable sections
- Configurable questions
- Conditional branching
- Required/optional questions
- Numeric answers
- Text answers
- Pass/fail answers
- OK/defect/N/A answers
- Photo-required answers
- Signature-required answers
- Measurement thresholds
- Legal citation references from Compliance Core
- Defect creation mapping
- Work order creation mapping
- PM trigger mapping
- Versioned templates
- Template publishing workflow
- Template cloning
- Template import/export
- Tenant-level templates
- Platform-level template packs
- Site-specific template overrides

### Dynamic inspection logic

Examples:

- Ask if tandem axle is equipped.
- If yes, include tandem axle questions.
- If no, use single-drive-axle logic.
- Ask if super singles are equipped.
- If yes, suppress dual tire questions.
- If no, include dual tire questions.
- Ask brake type once.
- Use drum/disc branching after that.
- Ask fuel type once.
- Use diesel/CNG/gas/electric-specific questions afterward.

### Inspection runner

- Mobile-first inspection experience
- Resume incomplete inspection
- Resume at most recently submitted question
- Save every answer immediately
- Offline-capable mode
- Question-by-question timestamps
- Localized timestamps
- Time spent per question
- Pause/break tracking
- Running total time
- Total time excluding breaks
- Total time including breaks
- Inspector identity
- Asset identity
- Template version
- Inspection location
- Photos/videos/documents
- Required signature capture
- Voice-guided mode
- TTS prompts
- STT answers
- Constrained vocabulary support
- Numeric voice normalization
- Review screen before submission
- Failed-answer summary
- Defect summary
- Generated work order summary

### Inspection history

- Completed inspections
- Failed inspections
- Cancelled inspections
- Abandoned inspections
- Superseded inspections
- Inspection details page
- Full Q&A list
- Answer timestamps
- Time spent per question
- Break time
- Attachments
- Inspector
- Asset
- Template version
- Related work order
- Related defects
- Related PM event
- Exportable PDF/HTML package
- Permission-gated detailed view

### Inspection analytics

- Average time by template
- Average time by question
- Questions commonly failed
- Questions commonly skipped
- Inspector variance
- Site variance
- Asset class variance
- Highlight rows under/over N seconds/minutes
- Suspiciously fast inspections
- Repeated N/A usage
- Repeat failures
- Failure trends by asset type

---

## 6. Defect management

### Defect creation sources

- Inspection failure
- Driver report from RoutArr
- Technician report
- Supervisor report
- Telematics fault
- PM finding
- Work order finding
- Incident from StaffArr
- Compliance Core rule failure
- Manual entry

### Defect fields

- Defect ID
- Asset ID
- Source
- Source record reference
- Reporter `personId`
- Severity
- Safety impact
- Regulatory impact
- Operational impact
- Location/component
- Description
- Photos/videos
- Date/time observed
- Site reference
- Status
- Required repair type
- Related inspection answer
- Related work order
- Related PM
- Related compliance rule
- OOS flag
- Deferred flag
- Approved deferral reason
- Repair verification requirement

### Defect lifecycle

- Reported
- Needs review
- Accepted
- Rejected
- Duplicate
- Deferred
- Planned
- Assigned to work order
- In repair
- Repaired
- Verified
- Closed
- Reopened

### Defect severity

- Cosmetic
- Minor
- Monitor
- Operational
- Safety
- Critical
- Regulatory
- Out of service

### Defect intelligence

- Repeat defects
- Chronic asset issues
- Component failure patterns
- Defects by site
- Defects by asset class
- Defects by technician
- Defects by inspection template
- Defects causing downtime
- Defects causing compliance failures
- Mean time from report to repair
- Mean time from repair to verification

---

## 7. Work orders

### Work order creation sources

- Manual creation
- Inspection failure
- PM due event
- Defect escalation
- Telematics fault
- RoutArr operational report
- Compliance Core rule failure
- Recurring work
- Campaign/recall
- Bulk asset action

### Work order types

- Corrective repair
- Preventive maintenance
- Inspection-generated repair
- DOT/regulated repair
- Emergency repair
- Road call
- Breakdown
- Campaign
- Recall
- Warranty
- Shop task
- Facility repair
- Calibration
- Install/modification
- Decommissioning

### Work order lifecycle

- Draft
- Requested
- Needs approval
- Approved
- Planned
- Scheduled
- Assigned
- In progress
- Waiting on asset
- Waiting on parts
- Waiting on vendor
- Waiting on approval
- Paused
- Completed
- Verified
- Closed
- Cancelled
- Reopened

### Work order structure

- Header
- Asset
- Site
- Priority
- Severity
- Requested by
- Assigned technician(s)
- Supervisor/manager
- Due date
- SLA target
- Related defects
- Related inspections
- Related PMs
- Related documents
- Labor lines
- Parts lines
- Vendor lines
- Notes
- Attachments
- Cost summary
- Downtime summary
- Completion verification

### Task/job lines

- Multiple jobs per WO
- Job category
- VMRS-adjacent system/component/assembly
- Complaint
- Cause
- Correction
- Required steps
- Technician notes
- Labor estimate
- Labor actual
- Parts required
- Completion status
- Verification status
- Photos required
- Signoff required

### Labor tracking

- Technician assignment
- Clock in/out
- Manual labor entry
- Labor category
- Billable/non-billable
- Regular/overtime
- Internal labor cost
- Vendor labor cost
- Time by job line
- Time by asset
- Time by defect
- Time by PM
- Approval workflow for edited labor

### Work order completion

- Required closing fields
- Meter reading required
- Defect resolution required
- Parts usage required/optional
- Labor required/optional
- Photos required when configured
- Signoff required when configured
- Supervisor verification
- Compliance evidence check
- Reopen flow
- Close-to-history flow

---

## 8. Preventive maintenance

### PM program builder

- PM templates
- Asset class PMs
- Asset-specific PMs
- Site-specific PMs
- Tenant-level PM standards
- Meter-based PM
- Time-based PM
- Hybrid meter/time PM
- Seasonal PM
- Regulatory PM
- Inspection-driven PM
- Usage-driven PM
- Condition-based PM
- Telematics-triggered PM

### PM scheduling

- Due every X miles
- Due every X hours
- Due every X days/weeks/months
- Due by whichever comes first
- Due by whichever comes last
- Grace period
- Warning window
- Seasonal date windows
- Site blackout periods
- Forecasted due date
- Auto-generate work order
- Auto-generate inspection
- Require manager approval before generation
- Merge multiple PMs into one WO
- Split PMs by shop/site/craft
- Suppress duplicate PMs

### PM evaluation engine

Background worker should:

- Recompute PM due states
- Detect due soon
- Detect overdue
- Emit `pm_events`
- Auto-create WOs where configured
- Auto-create inspection runs where configured
- Recalculate after meter updates
- Recalculate after WO close
- Recalculate after inspection close
- Recalculate after asset status change

### PM lifecycle

- Not due
- Due soon
- Due
- Overdue
- Generated
- In progress
- Completed
- Skipped
- Deferred
- Reset
- Suspended

### PM history

- Last completed date
- Last completed meter
- Next due date
- Next due meter
- Completion evidence
- WO link
- Inspection link
- Technician
- Cost
- Parts
- Time
- Compliance impact

---

## 9. Downtime and availability

### Downtime tracking

- Automatic downtime from OOS status
- Manual downtime events
- Downtime by work order
- Downtime by defect
- Downtime by PM
- Downtime by inspection failure
- Downtime by waiting reason
- Downtime by site
- Downtime by asset class

### Downtime reasons

- In repair
- Awaiting parts
- Awaiting technician
- Awaiting vendor
- Awaiting approval
- Failed inspection
- Regulatory hold
- Accident/incident hold
- Warranty hold
- Awaiting transport
- Unknown

### Availability metrics

- Asset availability percentage
- Fleet availability percentage
- Downtime hours
- Planned vs unplanned downtime
- Mean time to repair
- Mean time between failures
- Repeat downtime
- Chronic asset detection

---

## 10. Parts, materials, and SupplyArr integration

MaintainArr should track parts usage and maintenance demand, but SupplyArr should own the deeper supply/vendor/purchasing system.

### MaintainArr-owned parts behavior

- Parts used on work orders
- Parts requested for work orders
- Parts required by PM template
- Parts required by job line
- Part snapshots on work order close
- Part cost snapshot
- Quantity used
- Quantity wasted
- Core return flag
- Warranty part flag
- Failed part tracking
- Component replacement history

### SupplyArr integration

- Search parts catalog
- Check availability
- Request parts
- Reserve parts
- Create purchase request
- Link PO
- Link vendor quote
- Link invoice
- Receive part status updates
- Maintain local reference/mirror records
- Never direct cross-database FK

---

## 11. Telematics and diagnostics

### Telematics ingestion

- Odometer
- Engine hours
- GPS location reference
- Fault codes
- Check engine status
- ABS faults
- DEF/aftertreatment faults
- Battery voltage
- Fuel level
- Idle time
- Harsh events
- Temperature where relevant
- Reefer data where relevant

### Diagnostic behavior

- Fault code ingestion
- Fault severity mapping
- Auto-create defect from configured faults
- Auto-create work order from severe faults
- Fault-to-component mapping
- Fault history by asset
- Repeat fault detection
- False positive suppression
- Technician diagnostic notes
- Repair validation after fault clears

### Telematics providers

End-state should support a provider abstraction for:

- MyGeotab
- Samsara
- Motive
- Verizon Connect
- Fleetio import/export compatibility
- Generic webhook
- Generic CSV import
- Generic API connector

---

## 12. Documents and evidence

### Asset documents

- Registration
- Insurance
- Annual inspection
- Lease documents
- Warranty documents
- Recall documents
- Calibration certificates
- Repair invoices
- Photos
- Manuals
- Spec sheets

### Maintenance documents

- WO attachments
- Inspection photos
- Technician photos
- Vendor invoices
- Parts receipts
- DOT inspection evidence
- Before/after photos
- Signed repair verification
- Compliance Core evidence references

### Document features

- Upload
- Preview
- Download
- Expiration date
- Required document rules
- Missing document alerts
- Document type taxonomy
- Document-to-asset link
- Document-to-WO link
- Document-to-inspection link
- Document-to-defect link
- Permission-gated access
- Audit history

---

## 13. Compliance-aware maintenance

MaintainArr should not be the legal rule source. **Compliance Core owns rule packs, citation interpretation, evidence requirements, waivers, and legal/policy evaluation.** MaintainArr should provide the maintenance facts, preserve maintenance evidence, and execute maintenance-side enforcement based on product rules and Compliance Core outcomes.

### MaintainArr compliance responsibilities

- Store maintenance evidence
- Generate maintenance compliance events
- Ask Compliance Core for rule evaluation
- Display rule outcomes
- Block/allow maintenance actions based on rule result
- Mark assets not ready when maintenance compliance fails
- Keep rule evaluation snapshots
- Link citations to templates/questions/WOs/defects
- Export maintenance audit packages that reference Compliance Core evaluations and preserve product-owned maintenance evidence

### Examples

- Annual inspection expired → asset readiness becomes restricted/not ready.
- DOT inspection failed → defect created and OOS flag applied.
- Brake defect unresolved → asset cannot be marked ready.
- Forklift inspection failed → asset unavailable until repaired.
- Required PM overdue → warning/restriction depending on tenant/rule setting.
- Repair requires qualified technician → verify qualification from TrainArr/StaffArr before assignment or closeout.
- Asset GVWR/regulatory profile changes → re-evaluate required inspection/maintenance rules.

### Evidence Boundary

MaintainArr owns raw maintenance evidence such as inspection answers, repair photos, work order notes, vendor invoices attached to repairs, and asset documents. Compliance Core owns the rule evidence requirement and evaluation snapshot that references this evidence. StaffArr owns personnel history packages that may reference MaintainArr records when a person was involved.

---

## 14. Cross-product event model

MaintainArr should emit and consume platform events.

### Events MaintainArr emits

- `asset.created`
- `asset.updated`
- `asset.status_changed`
- `asset.readiness_changed`
- `inspection.started`
- `inspection.answer_submitted`
- `inspection.completed`
- `inspection.failed`
- `inspection.cancelled`
- `defect.created`
- `defect.accepted`
- `defect.deferred`
- `defect.repaired`
- `defect.closed`
- `work_order.created`
- `work_order.assigned`
- `work_order.started`
- `work_order.completed`
- `work_order.closed`
- `pm.due_soon`
- `pm.due`
- `pm.overdue`
- `pm.completed`
- `asset.out_of_service`
- `asset.returned_to_service`
- `maintenance.incident_created`
- `maintenance.compliance_evidence_created`

### Events MaintainArr consumes

#### From StaffArr

- `person.created`
- `person.updated`
- `person.deactivated`
- `site.created`
- `site.updated`
- `team.updated`
- `permission.assignment_changed`

#### From TrainArr

- `certification.issued`
- `certification.expired`
- `qualification.granted`
- `qualification.revoked`
- `training.remediation_required`

#### From RoutArr

- `trip.completed`
- `asset.dispatched`
- `asset.returned`
- `driver.defect_reported`
- `route.exception_created`

#### From SupplyArr

- `part.reserved`
- `part.unavailable`
- `purchase_order.created`
- `part.received`
- `vendor_repair.completed`

#### From Compliance Core

- `rule_pack.updated`
- `rule_evaluation.failed`
- `rule_evaluation.passed`
- `evidence_requirement.changed`

---

## 15. Permission gates

MaintainArr should have granular permissions, not just “admin/user.” StaffArr owns the person-to-permission assignment ledger and permission templates; MaintainArr owns the MaintainArr permission catalog, domain authorization rules, and server-side enforcement for maintenance actions.


### Asset permissions

- View assets
- Create assets
- Edit assets
- Delete/retire assets
- Change asset status
- View asset cost
- View asset documents
- Upload asset documents
- Export asset history

### Inspection permissions

- View inspection templates
- Create/edit templates
- Publish templates
- Start inspections
- Submit inspections
- Cancel inspections
- View inspection history
- View detailed inspection timing
- Override inspection failure
- Export inspection evidence

### Defect permissions

- Report defect
- Review defect
- Accept/reject defect
- Defer defect
- Mark defect repaired
- Verify defect repair
- Close defect
- Override OOS defect

### Work order permissions

- View WOs
- Create WOs
- Edit WOs
- Assign WOs
- Start WOs
- Complete WOs
- Close WOs
- Reopen WOs
- Approve WOs
- View WO costs
- Edit labor
- Edit parts
- Delete/cancel WOs

### PM permissions

- View PMs
- Create PM programs
- Edit PM programs
- Assign PMs to assets
- Defer PM
- Skip PM
- Force PM generation
- Reset PM baseline

### Admin permissions

- Manage MaintainArr settings
- Manage import/export
- Manage integration settings
- Manage automation rules
- View audit logs
- Manage compliance mappings
- Manage system jobs

---

## 16. Standard MaintainArr roles

### Technician

- View assigned WOs
- Start/complete assigned tasks
- Submit labor
- Add notes/photos
- Report defects
- Perform assigned inspections
- View limited asset details

### Lead Technician

- Technician permissions
- Assign work within team
- Review defects
- Verify repairs
- Reopen WOs
- View team workload

### Inspector

- Perform inspections
- View assigned inspection history
- Submit defects
- Add evidence
- Cannot override failures unless separately granted

### Maintenance Supervisor

- Manage WOs
- Assign technicians
- Review defects
- Approve deferrals
- Close WOs
- View downtime
- View costs if granted

### Maintenance Manager

- Full maintenance operational control
- PM management
- Asset readiness management
- Reporting
- Cost review
- Compliance evidence review

### Fleet Admin

- Asset setup
- PM setup
- Inspection template setup
- Integration setup
- Import/export
- Advanced reporting

### Auditor / Compliance Viewer

- Read-only evidence access
- Inspection history
- WO history
- Defect history
- Asset compliance package
- No operational edits

### Platform Admin

- Only for cross-platform/system-level access
- Should come from NexArr, not MaintainArr-owned login logic

---

## 17. UI end-state

### Main navigation

- Dashboard
- Assets
- Work Orders
- Inspections
- Defects
- PM
- Downtime
- Parts Requests
- Documents
- Reports
- Compliance
- Imports
- Settings

### Dashboard

- Fleet readiness
- Assets available
- Assets restricted
- Assets OOS
- PM due soon
- PM overdue
- Failed inspections
- Critical defects
- Open WOs
- Waiting on parts
- Downtime summary
- Top problem assets
- Cost month-to-date
- Compliance alerts
- Technician workload

### Asset detail page

Tabs:

- Overview
- Readiness
- Specs
- Meters
- Inspections
- Defects
- Work Orders
- PM
- Downtime
- Parts
- Documents
- Cost
- Timeline
- Audit

### Work order board

Views:

- Kanban
- Table
- Calendar
- Technician workload
- Asset timeline
- Priority queue
- Waiting on parts
- Overdue
- Ready to close

### Inspection UI

Views:

- Start inspection
- In-progress inspections
- Inspection history
- Failed inspections
- Cancelled inspections
- Template builder
- Template versions
- Inspection analytics

### PM UI

Views:

- PM dashboard
- Due soon
- Overdue
- PM calendar
- PM programs
- Asset PM assignments
- Forecast
- Auto-generation logs

### Defect UI

Views:

- Defect inbox
- Needs review
- Critical defects
- Deferred defects
- Repaired pending verification
- Repeat defects
- Defect analytics

---

## 18. Reporting

### Maintenance reports

- Open work orders
- Closed work orders
- WO aging
- WO backlog
- Labor by technician
- Labor by asset
- Labor by site
- Labor by job type
- Parts usage
- Vendor cost
- Maintenance cost by asset
- Maintenance cost by mile/hour
- PM compliance
- PM overdue
- PM forecast
- Inspection pass/fail
- Defect trends
- Downtime
- Asset availability
- Repeat repairs
- Top 10 cost assets
- Top 10 downtime assets
- Chronic defect assets
- Technician productivity

### Compliance reports

- Annual inspection status
- Failed inspection evidence
- Out-of-service history
- Repair verification
- PM compliance evidence
- Asset readiness history
- Required document status
- Audit package by asset
- Audit package by date range
- Audit package by site
- Rule evaluation history from Compliance Core

### Executive reports

- Fleet readiness
- Cost trend
- Downtime trend
- Compliance risk
- Maintenance backlog
- PM completion rate
- Asset replacement candidates
- Site comparison
- Department comparison

---

## 19. Automation

### Automation rules

- Auto-create WO from failed inspection
- Auto-create WO from critical defect
- Auto-create WO from PM due
- Auto-create defect from telematics fault
- Auto-mark asset OOS from critical failure
- Auto-return asset to restricted/ready after verification
- Auto-notify supervisor of overdue PM
- Auto-notify parts when WO needs parts
- Auto-notify RoutArr when asset becomes unavailable
- Auto-request Compliance Core evaluation after asset/profile change
- Auto-request TrainArr qualification check before assignment
- Auto-escalate stale WOs
- Auto-escalate unverified repairs
- Auto-close minor WOs after configured verification
- Auto-generate audit packet on demand/schedule

---

## 20. Import/export

### Imports

- Assets
- Meters
- Work orders
- PM schedules
- Inspection templates
- Inspection history
- Parts usage
- Defects
- Documents metadata
- Telematics readings
- Vendor repair history
- Legacy EAM exports

### Export formats

- CSV
- XLSX
- PDF
- HTML audit package
- JSON API export
- Evidence ZIP package

### Import features

- Mapping wizard
- Validation preview
- Duplicate detection
- Error report
- Partial import support
- Rollback support
- Import history
- Saved import mappings

---

## 21. API surface

### Core API groups

- `/api/v1/assets`
- `/api/v1/asset-types`
- `/api/v1/asset-classes`
- `/api/v1/meters`
- `/api/v1/inspections`
- `/api/v1/inspection-templates`
- `/api/v1/inspection-runs`
- `/api/v1/defects`
- `/api/v1/work-orders`
- `/api/v1/work-order-tasks`
- `/api/v1/labor`
- `/api/v1/parts-usage`
- `/api/v1/pm-programs`
- `/api/v1/pm-events`
- `/api/v1/downtime`
- `/api/v1/documents`
- `/api/v1/readiness`
- `/api/v1/reports`
- `/api/v1/imports`
- `/api/v1/exports`
- `/api/v1/integrations`
- `/api/v1/events`
- `/api/v1/audit`
- `/api/v1/settings`

### API expectations

- Tenant scoped
- Permission gated
- Service-token compatible
- Event outbox
- Idempotency keys for external event ingestion
- Pagination
- Filtering
- Sorting
- Bulk operations
- Audit logging
- OpenAPI documentation
- Consistent error envelope
- Consistent validation envelope
- No cross-product direct DB foreign keys

---

## 22. Audit logging

MaintainArr should audit:

- Asset creation/edit/status changes
- Meter edits/corrections
- Inspection start/submit/cancel
- Inspection answer edits
- Defect status changes
- Defect deferrals
- WO status changes
- Labor edits
- Parts usage edits
- PM schedule changes
- PM skips/deferrals
- Document uploads/deletions
- Permission-sensitive views
- Compliance overrides
- Rule evaluation snapshots
- Integration event failures
- Import/export actions

Audit entries should include:

- Actor `personId`
- Tenant
- Timestamp
- Entity type
- Entity ID
- Action
- Before value
- After value
- Reason/comment
- Source IP/device where appropriate
- Service/client ID for automated actions

---

## 23. Mobile and shop-floor usability

### Mobile

- Fast asset search
- QR/barcode scan
- Start inspection from asset
- Add defect from asset
- Add photo quickly
- View assigned WOs
- Complete WO task
- Submit labor
- Offline mode
- Resume interrupted work
- Large buttons
- Glove-friendly UI
- Low-signal resilience

### Shop display

- Work queue board
- Technician assignments
- Asset status board
- Waiting on parts board
- PM due board
- OOS board
- Supervisor action queue

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

## Audit-Informed Feature Additions: Maintenance Boundaries and Cross-Product Readiness

### MaintainArr Overreach Guardrails

MaintainArr should not become the suite hub just because maintenance work touches people, parts, dispatch, training, and compliance.

Required feature boundaries:

- People pickers reference NexArr/StaffArr `personId`; MaintainArr does not own canonical personnel records.
- Technician qualification checks call TrainArr and/or StaffArr readiness APIs; MaintainArr does not store authoritative training completions.
- Parts demand, purchase requests, vendor orders, receiving, inventory, and supplier records are SupplyArr-owned.
- MaintainArr may create a SupplyArr-backed purchase request from a work order, but SupplyArr owns approval, PO, receiving, vendor, and inventory truth.
- Dispatch availability is exposed to RoutArr; MaintainArr does not own route/trip execution.
- Compliance Core outcomes are consumed as maintenance gates; MaintainArr does not author rule packs.

### SupplyArr-Backed Parts and Procurement Request Features

MaintainArr should support maintenance-friendly parts workflows without owning procurement.

Features:

- Work-order parts demand line.
- Part availability read-through from SupplyArr.
- Part reservation request to SupplyArr.
- Purchase request creation from a work order through SupplyArr API or embedded owner-controlled surface.
- Purchase/request status display on the work order.
- Received-part status display from SupplyArr.
- Part cost snapshot copied to maintenance cost records when consumed.
- Clear deep-link to the SupplyArr source record.

Completion criteria:

- A shop manager can request needed parts without leaving the work-order flow for routine work, while SupplyArr remains the system of record for procurement.

### Asset Readiness Gate API

MaintainArr should expose asset readiness as a product-facing contract.

Features:

- `assetId` readiness endpoint.
- Readiness reason codes.
- Open critical/safety defect flags.
- PM due/overdue flags.
- Inspection due/failed flags.
- Active work-order and hold state.
- Compliance Core failure/reference IDs.
- Confidence/staleness metadata.
- RoutArr-safe dispatchability summary.
- Audit snapshot of readiness at decision time.

Completion criteria:

- RoutArr can determine whether equipment is dispatchable without duplicating MaintainArr maintenance logic.


## 24. “Complete” definition

MaintainArr is complete when it can support a real fleet/shop from end to end:

1. Asset is created.
2. Asset is classified.
3. Required inspections and PMs are assigned.
4. Asset usage/meter data flows in.
5. Inspection is performed on mobile.
6. Defects are captured with evidence.
7. Critical defects restrict asset readiness.
8. Work orders are generated.
9. Technicians perform repairs.
10. Labor, parts, notes, photos, and meter readings are recorded.
11. Repairs are verified.
12. PM schedules reset correctly.
13. Asset readiness recalculates.
14. RoutArr can see whether the asset is usable.
15. Compliance Core can evaluate maintenance evidence.
16. StaffArr/TrainArr can validate whether the worker was authorized.
17. Reports prove what happened.
18. Audit package can be generated without digging through scattered systems.

The end goal is:

> **MaintainArr should be able to prove, at any moment, that an asset is maintained, inspected, repaired, documented, and ready — or explain exactly why it is not.**
