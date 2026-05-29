# TrainArr End Goal and Granular Feature Set

## Executive Summary

TrainArr is the qualification engine for the Arr ecosystem.

Its end goal is to convert training requirements, evaluations, signoffs, remediation, and evidence into trusted, auditable authorization. It should answer a simple operational question:

> Is this person trained, evaluated, current, and allowed to perform this work right now?

TrainArr should not merely store training records. It should actively manage the full lifecycle of training programs, required qualifications, retraining triggers, completion evidence, evaluator signoffs, regulatory references, and cross-product authorization signals.

TrainArr exists so that StaffArr, MaintainArr, RoutArr, SupplyArr, Compliance Core, and future products can rely on a single source for training-derived qualifications without each product inventing its own training logic.

---

## Product Positioning

### TrainArr’s Role

TrainArr owns:

- Training programs
- Training requirements
- Training steps
- Evaluations
- Signoffs
- Completion records
- Evidence
- Training-derived certificates and qualifications
- Recertification schedules
- Remediation workflows
- Training compliance status
- Training audit packages
- Training workflow rules, completion rules, renewal rules, and qualification issuance logic
- Qualification publishing to StaffArr
- Training-related authorization signals for other products

TrainArr should be treated as the system that turns learning, observation, evaluation, and proof into usable operational eligibility.

### Plain-English Description

TrainArr makes sure people are properly trained before the business depends on them.

It helps define what someone must learn, prove, sign, be evaluated on, and renew. When all required parts are complete, TrainArr issues or updates the resulting qualification so other products can safely use it.

---

## Core End Goal

TrainArr is complete when a tenant can:

1. Define a training program from scratch.
2. Attach requirements to roles, positions, assets, equipment classes, task types, sites, departments, regulatory categories, or product workflows.
3. Guide a trainee through each required step.
4. Capture evidence, acknowledgements, tests, practical evaluations, and trainer/evaluator signoffs.
5. Automatically determine whether the person has completed the requirement.
6. Issue a certificate, qualification, or authorization.
7. Publish that result to StaffArr against the correct `personId`.
8. Let MaintainArr, RoutArr, SupplyArr, and future products check whether a person is qualified before work is assigned.
9. Detect expired, missing, incomplete, suspended, or incident-triggered training needs.
10. Produce a clean audit package showing why the person was or was not qualified at a specific point in time.

---

## Ownership Boundaries

### NexArr Ownership

NexArr owns platform access.

TrainArr depends on NexArr for:

- Tenant validation
- Product entitlement checks
- Platform identity
- Service-to-service authentication
- Product access licensing
- Platform-level user login state

TrainArr must not become an alternate login or platform entitlement system.

### StaffArr Ownership

StaffArr owns people and organizational structure.

TrainArr depends on StaffArr for:

- `personId`
- Person identity display data
- Employment/activity status
- Site membership
- Department membership
- Team membership
- Position/role assignment
- Supervisor/manager relationships
- Platform-wide personnel history presentation
- Manual qualification overrides where appropriate

TrainArr should not own canonical people, sites, departments, teams, positions, or employment status.

### TrainArr Ownership

TrainArr owns training workflow logic and training-derived outcomes.

Training workflow logic means step completion rules, assessment rules, evaluator signoff rules, renewal rules, recertification rules, remediation rules, and qualification issuance logic. It does not mean ownership of Compliance Core's legal/policy rule packs or cross-product compliance evaluation engine.

TrainArr owns:

- Training definitions
- Training program versions
- Training paths
- Required steps
- Course content references
- Tests and assessments
- Practical evaluations
- Trainer/evaluator signoffs
- Trainee acknowledgements
- Completion evidence
- Training records
- Training completion state
- Recertification logic
- Retraining logic
- Training-derived certificates
- Training-derived qualifications
- Qualification issuance after completion
- Training-related audit history

### Compliance Core Ownership

Compliance Core owns normalized rule packs and compliance interpretation support.

TrainArr depends on Compliance Core for:

- Rule categories
- Regulatory citation references
- Jurisdictional rule packs
- Requirement mapping support
- Law/regulation metadata
- Tenant-applicable compliance rule context

TrainArr should be able to display and attach citation references from Compliance Core, but Compliance Core should own the canonical citation registry, normalized rule-pack definitions, jurisdiction metadata, and cross-product compliance interpretation. TrainArr may preserve point-in-time citation snapshots on program versions and completed records for audit defensibility, but those snapshots do not make TrainArr the citation source of truth.

### MaintainArr Ownership

MaintainArr owns maintenance execution.

TrainArr integrates with MaintainArr for:

- Asset-related training requirements
- Technician task qualification checks
- Inspection qualification checks
- Repair authorization checks
- Incident-triggered retraining
- Equipment-class training requirements
- Maintenance workflow gating

MaintainArr should ask TrainArr whether a person is qualified. MaintainArr should not duplicate TrainArr’s training records.

### RoutArr Ownership

RoutArr owns routing and transportation execution.

TrainArr integrates with RoutArr for:

- Driver qualification checks
- Vehicle/equipment operation training
- Route-specific training requirements
- Hazmat/special-load training requirements
- Cross-state or jurisdictional training flags
- Incident-triggered retraining
- Dispatch authorization gating

RoutArr should ask TrainArr whether a person is qualified. RoutArr should not own training completions.

### SupplyArr Ownership

SupplyArr owns supply/vendor/customer/parts execution.

TrainArr integrates with SupplyArr for:

- Material handling training
- Forklift/powered industrial truck training checks
- Parts handling requirements
- Hazardous material handling requirements
- Purchasing or receiving authorization requirements
- Vendor-facing training or documentation requirements where applicable

---

## Target User Groups

TrainArr should support these users:

### Trainees

People assigned training requirements.

They need to:

- See assigned training
- Understand why it is required
- Complete training steps
- Upload evidence
- Take tests
- Acknowledge policies
- Request evaluation
- See due dates and expiration dates
- See what they are currently qualified to do

### Trainers

People who teach or guide training.

They need to:

- Manage trainee progress
- Record instruction
- Upload or attach evidence
- Approve practice completions
- Request evaluations
- Comment on deficiencies
- Assign remediation

### Evaluators

People authorized to evaluate competency.

They need to:

- Perform practical evaluations
- Use structured evaluation forms
- Pass/fail individual skills
- Require remediation
- Sign off final competency
- Attach evidence
- Record conditions of evaluation

### Supervisors and Managers

People responsible for workforce readiness.

They need to:

- See team training status
- See missing qualifications
- See upcoming expirations
- Assign or request training
- Prevent unqualified work assignment
- Review incident-triggered retraining
- Export audit evidence

### Compliance/Admin Users

People responsible for program structure and legal defensibility.

They need to:

- Build training programs
- Map requirements to roles/tasks/equipment
- Attach citations
- Version programs
- Audit changes
- Manage trainer/evaluator authorization
- Generate compliance packages
- Review rule-pack impact

### Platform Admins

People responsible for platform configuration.

They need to:

- Configure product integration
- Manage tenant-level TrainArr settings
- Review service health
- Validate cross-product sync
- Manage rule-pack availability
- Monitor authorization dependencies

---

## Core Domain Model

### Training Program

A structured set of requirements intended to produce a completion, certificate, qualification, or authorization.

Examples:

- Powered Industrial Truck Operator
- DOT Driver Qualification Refresher
- Brake Inspection Authorization
- Forklift Practical Evaluation
- Lockout/Tagout Awareness
- Hazmat Handling
- Preventive Maintenance Inspector
- New Hire Safety Orientation
- Incident Remediation Program

Required fields:

- Program ID
- Tenant ID
- Name
- Description
- Category
- Status
- Version
- Owner
- Effective date
- Retirement date
- Renewal interval
- Default expiration behavior
- Applicable roles/tasks/equipment
- Required steps
- Completion rule
- Resulting certificate/qualification
- Citation references
- Audit metadata

Statuses:

- Draft
- In review
- Active
- Retired
- Archived

### Program Version

Every published training program should be versioned.

TrainArr must preserve:

- Version number
- Effective date
- Retired date
- Author
- Approver
- Change summary
- Step definitions at that point in time
- Completion logic at that point in time
- Citations at that point in time
- Historical completion validity

A person who completed version 2 should not have their record rewritten when version 3 is published.

### Training Step

A required unit of progress inside a training program.

Step types should include:

- Read and acknowledge
- Watch media
- Attend instructor-led session
- Complete checklist
- Upload evidence
- Pass quiz
- Pass written test
- Pass practical evaluation
- Trainer signoff
- Evaluator signoff
- Supervisor approval
- External certificate upload
- Policy acknowledgement
- Field observation
- Remediation task
- Conditional branch
- Cross-product verification
- Manual admin approval

Step fields:

- Step ID
- Program version ID
- Name
- Description
- Step type
- Required/optional flag
- Sort order
- Dependencies
- Passing criteria
- Required evidence
- Required signer role
- Expiration behavior
- Remediation behavior
- Instructions
- Audit metadata

### Guided Program Builder

TrainArr should replace any short-form training creation screen with a guided, step-by-step workflow.

Builder stages:

1. Basic information
2. Program category
3. Applicability
4. Regulatory/citation context
5. Training path structure
6. Step creation
7. Evaluation requirements
8. Evidence requirements
9. Completion rules
10. Resulting qualification/certificate
11. Renewal and retraining rules
12. Cross-product authorization behavior
13. Review and publish

The builder should feel like a professional workflow, not a plain CRUD form.

### Training Assignment

A link between a person and a training requirement.

Assignment fields:

- Assignment ID
- Tenant ID
- Person ID
- Program ID
- Program version ID
- Assigned by
- Assigned reason
- Assignment source
- Due date
- Priority
- Status
- Started date
- Completed date
- Expiration date
- Current step
- Completion percentage
- Related product
- Related entity reference
- Audit metadata

Assignment sources:

- Manual assignment
- Position requirement
- Role requirement
- Site requirement
- Department requirement
- Equipment assignment
- Asset class requirement
- Route requirement
- Dispatch requirement
- Maintenance task requirement
- Incident-triggered retraining
- Compliance Core rule
- Recertification
- External import

Statuses:

- Assigned
- Not started
- In progress
- Waiting for trainee
- Waiting for trainer
- Waiting for evaluator
- Waiting for supervisor
- Remediation required
- Completed
- Expired
- Suspended
- Waived
- Cancelled

### Training Attempt

A concrete attempt to complete assigned training.

TrainArr should preserve attempts instead of overwriting outcomes.

Attempt fields:

- Attempt ID
- Assignment ID
- Started at
- Completed at
- Outcome
- Score
- Evaluator
- Trainer
- Evidence
- Comments
- Failure reasons
- Remediation requirement
- Audit metadata

Outcomes:

- Passed
- Failed
- Incomplete
- Cancelled
- Remediation required
- Awaiting signoff

### Evidence

Proof attached to a training step or completion.

Evidence types:

- File upload
- Image
- Video
- PDF
- External certificate
- Signature
- Typed acknowledgement
- Quiz result
- Evaluation checklist
- Observation note
- External system reference
- Cross-product event reference

Evidence fields:

- Evidence ID
- Tenant ID
- Person ID
- Assignment ID
- Step ID
- File/reference type
- Storage reference
- Uploaded by
- Uploaded at
- Verified by
- Verified at
- Tamper-evidence hash
- Retention class
- Audit metadata

### Signoff

A formal approval action by a trainee, trainer, evaluator, supervisor, or admin.

Signoff fields:

- Signoff ID
- Assignment ID
- Step ID
- Signer person ID
- Signer role
- Signature method
- Statement text
- Signed at
- Pass/fail outcome
- Comments
- Device/session metadata
- Audit metadata

Signoff types:

- Trainee acknowledgement
- Trainer signoff
- Evaluator signoff
- Supervisor approval
- Admin override
- External verifier approval

### Certificate

A formal training result issued by TrainArr.

Certificate fields:

- Certificate ID
- Tenant ID
- Person ID
- Program ID
- Program version ID
- Certificate type
- Certificate number
- Issued date
- Effective date
- Expiration date
- Issued by
- Source assignment
- Status
- Evidence package
- StaffArr publish status
- Audit metadata

Statuses:

- Active
- Expiring soon
- Expired
- Suspended
- Revoked
- Superseded

### Qualification

A usable authorization result derived from training.

A certificate may be a document. A qualification should be the operational signal other products use.

Examples:

- Can operate forklift
- Can perform annual inspection
- Can inspect brake systems
- Can dispatch hazmat load
- Can train PIT operators
- Can evaluate PIT operators
- Can perform PM inspection
- Can close safety-critical work order
- Can approve trainee completion

Qualification fields:

- Qualification ID
- Tenant ID
- Person ID
- Qualification type
- Scope
- Source program
- Source certificate
- Effective date
- Expiration date
- Status
- Restrictions
- Product visibility
- StaffArr publish state
- Audit metadata

Qualification scopes:

- Global tenant
- Site
- Department
- Team
- Position
- Equipment class
- Asset type
- Specific asset
- Task type
- Route type
- Material category
- Jurisdiction

---

## Granular Feature Set

## 1. Dashboard and Command Center

### 1.1 Tenant Training Dashboard

TrainArr should provide a high-level dashboard showing:

- Active training assignments
- Overdue assignments
- Expiring qualifications
- Failed evaluations
- Remediation backlog
- Upcoming recertifications
- Unqualified assignment risks
- Programs needing review
- Rule-pack-driven requirement changes
- Cross-product qualification blocks
- Audit readiness score

### 1.2 Personal Training Dashboard

For each user:

- My assigned training
- My in-progress steps
- My completed programs
- My certificates
- My qualifications
- My upcoming expirations
- My failed/remediation items
- My required evaluations
- My signoff requests
- My evidence uploads

### 1.3 Manager Dashboard

For supervisors/managers:

- Team compliance status
- Missing required training
- Expiring team qualifications
- Training bottlenecks
- People blocked from work
- People assigned to work they are not qualified for
- Pending manager approvals
- Open remediation
- Drilldown by site, department, team, position, or person

### 1.4 Trainer/Evaluator Dashboard

For trainers/evaluators:

- Assigned trainees
- Pending evaluations
- Pending signoffs
- Failed steps requiring follow-up
- Upcoming instructor-led sessions
- Practical evaluations to schedule
- Trainer workload
- Evaluator authorization status

### 1.5 Compliance Dashboard

For compliance/admin users:

- Program coverage
- Rule coverage
- Citation coverage
- Training gaps by role/task/equipment
- Version review status
- Expired program versions
- Program change impact
- Audit package generation
- Cross-product risk heatmap

---

## 2. Guided Training Program Creation

### 2.1 Wizard-Based Program Creation

Replace short-form program creation with a guided workflow.

The wizard should support:

- Save as draft
- Resume later
- Step validation
- Contextual help
- Plain-English summaries
- Preview before publish
- Required field enforcement
- Version-aware publishing
- Role-based access
- Template-based creation

### 2.2 Program Type Selection

Program types:

- Awareness training
- Skill training
- Equipment operation training
- Safety training
- Compliance training
- Practical evaluation
- Certification program
- Refresher training
- Remediation training
- Onboarding program
- Role qualification
- Task authorization
- External certificate tracking

### 2.3 Applicability Builder

Admins should define when a program applies.

Applicability dimensions:

- Site
- Department
- Team
- Position
- Role
- Person
- Equipment class
- Asset type
- Specific asset
- Work category
- Task type
- Route type
- Dispatch type
- Material type
- Incident category
- Jurisdiction
- Compliance rule
- Product event

Example rules:

- If position is warehouse associate, require PIT awareness.
- If assigned to forklift operation, require PIT operator qualification.
- If assigned to annual inspection work, require inspection authorization.
- If route crosses state lines and vehicle/load meets configured rule criteria, require applicable driver qualifications.
- If incident category is preventable forklift contact, require remediation training.

### 2.4 Step Builder

The step builder should allow admins to create detailed training paths.

Step configuration:

- Name
- Description
- Instructions
- Required/optional
- Dependency rules
- Required evidence
- Required signer
- Passing score
- Retry limits
- Remediation behavior
- Expiration behavior
- Conditional visibility
- Estimated duration
- Completion method

### 2.5 Conditional Branching

Training paths should support conditional steps.

Examples:

- If trainee fails quiz, assign remediation.
- If trainee has external certificate, allow evidence upload and verifier review.
- If trainee operates a specific equipment class, require practical evaluation for that class.
- If incident severity is high, require supervisor approval before requalification.
- If recertification is late, require full retraining instead of refresher.

### 2.6 Completion Rule Builder

Admins should define how completion is calculated.

Supported logic:

- All steps required
- Any step from a group
- Minimum score
- Required signoff
- Required evidence verification
- Required evaluator pass
- Expiration-aware completion
- Conditional completion
- Manual approval required
- External certificate accepted
- Rule-pack requirement satisfied

### 2.7 Result Builder

Admins should define what the completed program grants.

Results:

- Certificate
- Qualification
- Authorization
- Refresher completion
- Remediation completion
- Evidence-only record
- StaffArr personnel history entry
- Cross-product authorization signal

### 2.8 Publish Review

Before publishing, TrainArr should show:

- Program summary
- Applicability summary
- Step list
- Completion rules
- Resulting qualifications
- Citation references
- Renewal rules
- Integration impact
- People affected
- Potential conflicts
- Required approvals

---

## 3. Training Library

### 3.1 Program Catalog

Users should browse available training programs by:

- Category
- Status
- Site
- Department
- Position
- Equipment class
- Product area
- Compliance topic
- Required/optional
- Internal/external
- Active/retired

### 3.2 Templates

TrainArr should support reusable templates.

Template categories:

- New hire onboarding
- Safety orientation
- Equipment operator
- Driver qualification
- Maintenance technician
- Inspector authorization
- Forklift/PIT training
- Lockout/tagout
- Incident remediation
- Annual refresher
- External certificate capture

### 3.3 Program Duplication

Admins should be able to:

- Duplicate a program
- Fork a program for another site
- Create a new version
- Import from template
- Convert draft to active program
- Retire old versions

### 3.4 Content References

TrainArr should support training content references without needing to own every content system.

Content types:

- Uploaded PDF
- Uploaded video
- External URL
- Internal document reference
- Policy document
- Compliance Core citation
- MaintainArr asset procedure
- StaffArr policy
- SupplyArr vendor document
- Embedded text lesson
- Quiz bank

---

## 4. Assignment Engine

### 4.1 Manual Assignment

Authorized users can assign training to:

- Individual person
- Team
- Department
- Site
- Position
- Role
- Dynamic group
- People matching rule

### 4.2 Automatic Assignment

TrainArr should automatically assign training based on:

- New hire onboarding
- Position changes
- Site transfers
- Department changes
- Equipment assignment
- Work assignment
- Route assignment
- Incident report
- Qualification expiration
- Compliance rule change
- Program version change
- Manager request
- Cross-product event

### 4.3 Assignment Reason Tracking

Every assignment should explain why it exists.

Assignment reason examples:

- Required by position
- Required by site
- Required by equipment assignment
- Required before dispatch
- Required before maintenance task
- Required due to incident
- Required by recertification
- Required by Compliance Core rule
- Manually assigned by supervisor

### 4.4 Due Date Logic

Due dates should support:

- Fixed date
- Relative to assignment
- Relative to hire date
- Relative to position start
- Relative to equipment assignment
- Relative to incident date
- Relative to certificate expiration
- Grace period
- Escalation threshold

### 4.5 Assignment Priorities

Priorities:

- Low
- Normal
- High
- Urgent
- Work-blocking
- Compliance-critical

### 4.6 Bulk Assignment

Admins should be able to:

- Assign to many people
- Filter before assignment
- Preview affected people
- Exclude already-qualified people
- Assign by CSV/import
- Schedule future assignment
- Cancel pending bulk assignment

---

## 5. Learner Experience

### 5.1 My Training Page

The trainee should see:

- Required training
- Optional training
- Due dates
- Status
- Next action
- Progress percentage
- Training reason
- Related role/task/equipment
- Expiration impact
- Contact person

### 5.2 Guided Training Player

The training player should guide the trainee through:

- Instructions
- Reading material
- Video content
- Acknowledgements
- Evidence uploads
- Quizzes
- Written tests
- Practical evaluation requests
- Signatures
- Completion summary

### 5.3 Progress Tracking

Progress should show:

- Completed steps
- Remaining steps
- Blocked steps
- Failed steps
- Pending signoffs
- Pending evaluations
- Pending evidence verification

### 5.4 Evidence Upload

Trainees should be able to:

- Upload files
- Attach photos
- Add notes
- Submit external certificates
- Reupload rejected evidence
- View verification status

### 5.5 Acknowledgements

TrainArr should capture:

- Policy acknowledgement
- Training material acknowledgement
- Safety responsibility acknowledgement
- Refusal acknowledgement
- Digital signature
- Timestamp
- Statement version

### 5.6 Mobile-Friendly Completion

The trainee experience should work well on mobile devices for:

- Reading assigned training
- Uploading photos
- Signing acknowledgements
- Viewing certificates
- Requesting evaluations
- Completing simple checklists

---

## 6. Testing and Assessment

### 6.1 Quiz Builder

Admins should create quizzes with:

- Multiple choice
- True/false
- Multi-select
- Short answer
- Numeric answer
- Scenario question
- Random question pools
- Required passing score
- Retry limits
- Feedback messages

### 6.2 Question Bank

TrainArr should support reusable question banks.

Fields:

- Question text
- Question type
- Answer choices
- Correct answer
- Explanation
- Difficulty
- Category
- Related citation
- Related program
- Active/inactive status

### 6.3 Test Attempts

TrainArr should record:

- Attempt count
- Start time
- End time
- Answers
- Score
- Pass/fail
- Reviewer
- Retake eligibility
- Remediation trigger

### 6.4 Anti-Gaming Controls

Optional controls:

- Randomized questions
- Randomized answer order
- Time limits
- Lockout after failed attempts
- Supervisor reset
- Question pool rotation
- Required remediation after repeated failure

### 6.5 Practical Evaluation Forms

TrainArr should support structured practical evaluations.

Evaluation fields:

- Skill/task name
- Pass/fail criteria
- Observation notes
- Safety-critical failure flag
- Required comments on failure
- Evaluator signature
- Trainee acknowledgement
- Retest requirement

---

## 7. Trainer and Evaluator Workflows

### 7.1 Trainer Assignment

Programs should define who can train:

- Specific persons
- StaffArr role
- StaffArr position
- Existing qualification
- Site-based trainer pool
- Department-based trainer pool
- External trainer

### 7.2 Evaluator Authorization

Programs should define who can evaluate:

- Specific persons
- StaffArr role
- StaffArr permission
- Existing evaluator qualification
- Site-based evaluator pool
- Third-party evaluator

### 7.3 Trainer Console

Trainer console should include:

- Assigned trainees
- Pending instruction
- Training sessions
- Trainee progress
- Evidence review
- Comments
- Remediation notes
- Signoff actions

### 7.4 Evaluator Console

Evaluator console should include:

- Pending evaluations
- Evaluation forms
- Trainee history
- Prior failures
- Required evidence
- Pass/fail decision
- Restrictions
- Final signoff

### 7.5 Instructor-Led Sessions

TrainArr should support sessions with:

- Session title
- Program
- Instructor
- Location
- Date/time
- Capacity
- Attendee roster
- Attendance capture
- Session materials
- Completion outcome
- No-show tracking

### 7.6 Evaluator Conflict Controls

Optional controls:

- Prevent self-evaluation
- Prevent subordinate-only evaluation if configured
- Require second evaluator for high-risk qualification
- Require supervisor approval for override
- Require reason for pass after previous failure

---

## 8. Remediation and Retraining

### 8.1 Remediation Assignment

Remediation should be triggered by:

- Failed quiz
- Failed written test
- Failed practical evaluation
- Incident
- Supervisor concern
- Compliance rule
- Expired qualification
- Unsafe behavior
- Repeated near miss
- Manual assignment

### 8.2 Remediation Programs

Remediation programs can be:

- Full retraining
- Partial retraining
- Targeted skill step
- Coaching acknowledgement
- Practical reevaluation
- Supervisor review
- Incident-specific learning path

### 8.3 Incident-Triggered Retraining

TrainArr should accept training-related incident events from StaffArr or other products.

Examples:

- Forklift contact incident
- Preventable vehicle incident
- Improper lockout/tagout
- Failed inspection procedure
- Unsafe maintenance repair
- Policy violation
- Customer/vendor safety issue
- Driver behavior event

### 8.4 Requalification After Incident

TrainArr should support:

- Immediate suspension of qualification
- Temporary restriction
- Required remediation
- Required reevaluation
- Supervisor approval
- Compliance review
- Automatic reactivation after completion
- Permanent revocation

### 8.5 Remediation History

A person’s training record should show:

- Original failure/incident
- Assigned remediation
- Completion result
- Evaluator notes
- Qualification impact
- Final outcome

---

## 9. Certificates and Qualifications

### 9.1 Certificate Issuance

TrainArr should issue certificates when completion rules are satisfied.

Certificate options:

- Internal certificate
- External certificate record
- Refresher certificate
- Temporary certificate
- Conditional certificate
- Restricted certificate

### 9.2 Qualification Issuance

TrainArr should issue operational qualifications based on program results.

Qualification examples:

- Forklift operator
- Forklift evaluator
- Brake inspector
- Annual inspector
- PM technician
- Driver qualified
- Hazmat handling authorized
- Safety trainer
- Incident reviewer

### 9.3 Restrictions

Qualifications may include restrictions.

Examples:

- Site-limited
- Equipment-class-limited
- Asset-specific
- Supervisor-required
- Daylight-only
- Temporary
- Training-only
- Cannot train others
- Cannot evaluate others
- Requires annual renewal

### 9.4 StaffArr Publishing

After TrainArr issues a qualification, it should publish the result to StaffArr.

Publish behavior:

- Send person ID
- Send qualification type
- Send status
- Send effective date
- Send expiration date
- Send source program
- Send source certificate
- Send restrictions
- Send revocation/suspension updates
- Send audit reference

### 9.5 Qualification Status

Statuses:

- Active
- Pending
- Expiring soon
- Expired
- Suspended
- Revoked
- Restricted
- Superseded
- Waived by referenced Compliance Core waiver or StaffArr manual override
- Manually overridden in StaffArr

### 9.6 Manual Overrides

Manual overrides should be carefully controlled.

A manual override is not a training completion and should not mutate the historical TrainArr record. StaffArr owns personnel authorization overrides. Compliance Core owns compliance waivers. TrainArr should display both when relevant to qualification visibility, but it should keep training-derived status separate from override/waiver status.

Recommended model:

- StaffArr owns manual override assignment because it owns personnel administration.
- TrainArr records training-derived status.
- TrainArr should display StaffArr override state when relevant.
- Overrides should require reason, approver, expiration, and audit history.
- Overrides should not erase TrainArr’s actual training completion status.

---

## 10. Recertification and Expiration Management

### 10.1 Expiration Rules

Programs should define:

- No expiration
- Fixed expiration date
- Rolling expiration interval
- Calendar-year expiration
- End-of-month expiration
- Jurisdiction-specific expiration
- Rule-pack-driven expiration

### 10.2 Renewal Workflows

Renewal options:

- Full retraining
- Refresher course
- Short quiz
- Practical reevaluation
- Evidence upload
- Supervisor acknowledgement
- Automatic extension if criteria met

### 10.3 Expiration Notifications

TrainArr should notify:

- Trainee
- Supervisor
- Trainer
- Evaluator
- Compliance/admin users

Notification windows:

- 90 days
- 60 days
- 30 days
- 14 days
- 7 days
- Due today
- Expired

### 10.4 Expiration Effects

When a qualification expires:

- Mark certificate expired
- Mark qualification expired
- Publish status to StaffArr
- Notify related products
- Block authorization checks where configured
- Create renewal assignment where configured

### 10.5 Grace Periods

Programs may define:

- No grace period
- Soft grace period
- Supervisor-approved grace period
- Compliance-approved grace period
- Work-blocking expiration
- Restricted-work grace period

---

## 11. Cross-Product Authorization

### 11.1 Authorization Check API

Other products should be able to ask TrainArr:

- Is this person qualified for this action?
- Is this person qualified for this equipment?
- Is this person qualified for this task?
- Is this person qualified at this site?
- Is this person qualified for this route/load?
- Is this person’s qualification current?
- Is this person restricted?
- What training is missing?

### 11.2 Authorization Response

The API should return:

- Allowed/blocked/warn
- Person ID
- Qualification status
- Required qualifications
- Missing qualifications
- Expired qualifications
- Suspended qualifications
- Restrictions
- Evidence reference
- Suggested remediation
- Human-readable reason
- Machine-readable reason code

### 11.3 MaintainArr Examples

MaintainArr should check TrainArr before:

- Assigning safety-critical work order
- Assigning inspection
- Closing regulated inspection
- Assigning asset-specific repair
- Allowing technician to perform brake inspection
- Allowing technician to approve PM completion

### 11.4 RoutArr Examples

RoutArr should check TrainArr before:

- Dispatching driver
- Assigning vehicle/equipment
- Assigning route type
- Assigning special load
- Assigning hazmat-related work
- Releasing trip after incident-related suspension

### 11.5 SupplyArr Examples

SupplyArr should check TrainArr before:

- Assigning material handling task
- Assigning forklift/pallet jack operation
- Handling regulated materials
- Approving certain receiving/inspection tasks
- Performing vendor/customer safety-related tasks

### 11.6 StaffArr Examples

StaffArr should consume TrainArr data for:

- Person profile qualifications
- Personnel history
- Manager views
- Manual overrides
- Active/inactive enforcement
- Qualification history display
- Audit package assembly

---

## 12. Compliance Core Integration

### 12.1 Citation Attachment

Training programs should attach citation references from Compliance Core.

Citation display should include:

- Citation title
- Jurisdiction
- Regulation/source label
- Requirement summary
- Applicability notes
- Effective date
- Version/reference ID
- Plain-English explanation

### 12.2 Rule-Pack Requirements

Compliance Core may identify that training is required based on tenant facts.

TrainArr should support:

- Receiving required-training signals
- Mapping rule requirement to program
- Showing rule reason
- Assigning affected people
- Tracking compliance status
- Reporting coverage gaps

### 12.3 Rule Change Impact

When a rule changes, TrainArr should show:

- Affected programs
- Affected qualifications
- Affected people
- Training gap
- Required update
- Deadline
- Recommended action

### 12.4 Legal Defensibility

TrainArr should preserve:

- Program version at time of completion
- Citation references at time of completion
- Person assignment reason
- Step evidence
- Signoffs
- Evaluator identity
- Completion logic
- Expiration logic
- Changes after completion

---

## 13. Audit and Reporting

### 13.1 Person Training History

For each person:

- Assigned programs
- Completed programs
- Failed attempts
- Remediation
- Certificates
- Qualifications
- Expirations
- Suspensions
- Revocations
- Evidence
- Signoffs
- Overrides
- Related incidents

### 13.2 Audit Package

TrainArr should generate training-scope audit packages. StaffArr owns the broader personnel audit package, and Compliance Core owns cross-product compliance audit packages that evaluate rules across products.

TrainArr should generate an audit package for:

- Person
- Program
- Site
- Department
- Role
- Equipment class
- Date range
- Incident
- Qualification type

Audit package contents:

- Summary
- Person details from StaffArr
- Training assignments
- Completion records
- Program version
- Step evidence
- Signoffs
- Evaluator records
- Certificates
- Qualification status
- Citations
- Expiration history
- Override history
- Cross-product references

### 13.3 Point-in-Time Qualification Report

TrainArr should answer:

> Was this person qualified for this action on this date?

The report should include:

- Person ID
- Action/task
- Relevant qualification
- Status on date
- Source certificate
- Program version
- Expiration state
- Restrictions
- Evidence
- Signoffs
- Audit trail

### 13.4 Training Matrix

TrainArr should provide a matrix by:

- Person
- Position
- Department
- Site
- Qualification
- Program
- Status
- Due date
- Expiration date

Matrix statuses:

- Complete
- In progress
- Missing
- Overdue
- Expiring soon
- Expired
- Suspended
- Not applicable
- Waived

### 13.5 Gap Analysis

Gap reports:

- Missing training by person
- Missing training by role
- Missing training by site
- Missing training by equipment class
- Missing training by compliance category
- Missing evaluator coverage
- Program without citation
- Qualification without renewal rule
- Assigned work without qualification

### 13.6 Export Formats

Exports:

- PDF
- CSV
- Excel-compatible CSV
- JSON
- Audit bundle ZIP
- StaffArr personnel package reference

---

## 14. Notifications and Escalations

### 14.1 Notification Events

Notify for:

- New assignment
- Due soon
- Overdue
- Step completed
- Evidence rejected
- Signoff requested
- Evaluation requested
- Evaluation failed
- Remediation assigned
- Certificate issued
- Qualification published
- Expiration warning
- Qualification expired
- Qualification suspended
- Rule change impact

### 14.2 Escalation Rules

Escalate to:

- Trainee
- Supervisor
- Manager
- Trainer
- Evaluator
- Compliance admin
- Platform admin

Escalation triggers:

- Overdue training
- Repeated failed attempts
- Missing evaluator action
- Expiring qualification
- Work-blocking qualification gap
- Incident-triggered remediation not completed
- Rule-driven deadline approaching

### 14.3 Notification Channels

Potential channels:

- In-app notification
- Email
- StaffArr notification surface
- Product-specific warning banner
- API event to relevant product

---

## 15. Search and Filtering

### 15.1 Global Search

Search across:

- Programs
- Assignments
- People
- Certificates
- Qualifications
- Evidence
- Citations
- Incidents
- Evaluations

### 15.2 Filterable Fields

Common filters:

- Person
- Site
- Department
- Team
- Position
- Program
- Qualification
- Status
- Due date
- Expiration date
- Trainer
- Evaluator
- Assignment reason
- Product source
- Compliance category
- Citation

### 15.3 Saved Views

Users should save views such as:

- My overdue trainees
- Expiring forklift qualifications
- Unqualified drivers
- Failed practical evaluations
- Programs missing citations
- Site training matrix
- Incident remediation backlog

---

## 16. API Feature Set

### 16.1 Core API Resources

Recommended API areas:

- `/api/v1/programs`
- `/api/v1/program-versions`
- `/api/v1/program-steps`
- `/api/v1/assignments`
- `/api/v1/attempts`
- `/api/v1/evidence`
- `/api/v1/signoffs`
- `/api/v1/evaluations`
- `/api/v1/certificates`
- `/api/v1/qualifications`
- `/api/v1/authorization-checks`
- `/api/v1/requirements`
- `/api/v1/remediation`
- `/api/v1/recertification`
- `/api/v1/audit-packages`
- `/api/v1/reports`
- `/api/v1/integrations`
- `/api/v1/events`

### 16.2 Program APIs

Operations:

- Create draft program
- Update draft
- Add steps
- Configure applicability
- Attach citations
- Configure completion rules
- Preview publish impact
- Publish version
- Retire version
- Duplicate program
- Archive program

### 16.3 Assignment APIs

Operations:

- Assign training
- Bulk assign
- Cancel assignment
- Start assignment
- Complete step
- Submit evidence
- Request signoff
- Request evaluation
- Mark remediation required
- Complete assignment
- Recalculate assignment status

### 16.4 Certificate/Qualification APIs

Operations:

- Issue certificate
- Revoke certificate
- Suspend qualification
- Reinstate qualification
- Publish to StaffArr
- Recalculate expiration
- Get person qualifications
- Get qualification history
- Check authorization

### 16.5 Reporting APIs

Operations:

- Get training matrix
- Get gap report
- Get overdue report
- Get expiring report
- Generate audit package
- Get point-in-time qualification
- Export report

### 16.6 Integration APIs

Operations:

- Receive StaffArr person/org updates
- Receive incident events
- Receive product task/dispatch checks
- Receive Compliance Core rule impacts
- Publish qualification changes
- Publish assignment changes
- Publish remediation changes
- Health check service connectivity

---

## 17. Event Model

### 17.1 Events TrainArr Should Emit

Examples:

- `training.program.created`
- `training.program.published`
- `training.program.retired`
- `training.assignment.created`
- `training.assignment.started`
- `training.assignment.completed`
- `training.step.completed`
- `training.evidence.submitted`
- `training.evidence.rejected`
- `training.signoff.requested`
- `training.signoff.completed`
- `training.evaluation.requested`
- `training.evaluation.passed`
- `training.evaluation.failed`
- `training.remediation.required`
- `training.certificate.issued`
- `training.certificate.expired`
- `training.qualification.issued`
- `training.qualification.suspended`
- `training.qualification.revoked`
- `training.qualification.published_to_staffarr`

### 17.2 Events TrainArr Should Consume

Examples:

- `staffarr.person.created`
- `staffarr.person.updated`
- `staffarr.person.deactivated`
- `staffarr.position.assigned`
- `staffarr.site.assigned`
- `staffarr.team.changed`
- `staffarr.manual_qualification_override.created`
- `maintainarr.incident.created`
- `maintainarr.work_assignment.requested`
- `routarr.dispatch_assignment.requested`
- `routarr.incident.created`
- `supplyarr.material_handling_assignment.requested`
- `compliancecore.rule_pack.updated`
- `compliancecore.training_requirement.created`

### 17.3 Event Requirements

Events should include:

- Event ID
- Tenant ID
- Source product
- Event type
- Correlation ID
- Occurred timestamp
- Subject reference
- Payload version
- Signature or service authentication context

---

## 18. Security and Permissions

### 18.1 Role-Based Access

TrainArr should support permission checks for:

- View own training
- Complete own training
- Upload evidence
- Create programs
- Edit draft programs
- Publish programs
- Assign training
- Bulk assign training
- Review evidence
- Sign as trainer
- Sign as evaluator
- Approve completion
- Issue certificate
- Suspend qualification
- Revoke qualification
- Generate audit package
- Manage integrations
- Manage tenant settings

### 18.2 Trainer/Evaluator Eligibility

A user should only train or evaluate when:

- StaffArr says they are active
- They have the required permission
- They hold the required qualification
- They are within scope
- They are not blocked by conflict rules
- Their own qualification is current

### 18.3 Tenant Isolation

TrainArr must enforce tenant isolation across:

- Programs
- Assignments
- Evidence
- Certificates
- Qualifications
- Reports
- Events
- API access

### 18.4 Service-to-Service Security

Cross-product APIs should use:

- NexArr-issued service credentials
- Service client ID
- Service secret
- Tenant-scoped authorization
- Product entitlement validation
- Event signature validation
- Least-privilege access

### 18.5 Evidence Security

Evidence should support:

- Access control
- Retention policy
- Tamper-evidence hash
- Download audit
- Deletion restrictions
- Legal hold flag
- Redaction support where appropriate

---

## 19. UI and Navigation

### 19.1 Primary Navigation

Recommended TrainArr navigation:

- Dashboard
- My Training
- Programs
- Assignments
- Evaluations
- Certificates
- Qualifications
- Matrix
- Remediation
- Reports
- Audit
- Rule Coverage
- Settings

### 19.2 Program Detail Page

Sections:

- Overview
- Versions
- Applicability
- Steps
- Completion Rules
- Results
- Citations
- Assignments
- Reports
- Audit Log

### 19.3 Person Training Profile

Sections:

- Summary
- Assigned Training
- Completed Training
- Certificates
- Qualifications
- Expirations
- Remediation
- Evidence
- Signoffs
- Incidents
- Audit History

### 19.4 Assignment Detail Page

Sections:

- Assignment overview
- Why required
- Step progress
- Evidence
- Signoffs
- Evaluations
- Remediation
- Certificate result
- Qualification result
- Related product references
- Audit log

### 19.5 Training Matrix UI

Features:

- Sticky person/program columns
- Status icons
- Filters
- Grouping
- Export
- Drilldown
- Bulk actions
- Expiration highlighting
- Missing requirement explanation

### 19.6 Authorization Warning UI

When another product detects missing training, TrainArr should support returning UI-friendly explanations:

- Block reason
- Missing qualification
- Required training program
- Person currently assigned or not assigned
- Next action
- Supervisor action
- Estimated path to qualification

---

## 20. Data Quality and Validation

### 20.1 Required Validation

Validate:

- Program has at least one completion path
- Active program has a result or explicit evidence-only mode
- Published program cannot have invalid steps
- Required signoff has eligible signer definition
- Required evaluation has eligible evaluator definition
- Expiring certificate has renewal behavior
- Qualification has scope
- Applicability rule references valid StaffArr/product references
- Citation references are valid if required
- Program publish does not create impossible completion state

### 20.2 Duplicate Prevention

Prevent or warn on:

- Duplicate active program with same qualification
- Duplicate assignment
- Duplicate certificate number
- Conflicting qualification scopes
- Contradictory requirement rules
- Multiple active versions unintentionally applying to same group

### 20.3 Data Repair Tools

Admins should be able to:

- Recalculate assignment status
- Recalculate qualification status
- Retry StaffArr publish
- Retry event processing
- Resolve broken references
- Merge duplicate external certificates
- Reopen assignment if allowed
- Correct evidence metadata with audit trail

---

## 21. Import and Migration

### 21.1 Import Sources

TrainArr should support importing:

- Existing training matrix CSV
- Existing certificates
- External training records
- Personnel training history
- External LMS completion exports
- Scanned certificate files
- Manual historical completions

### 21.2 Import Mapping

Import workflow should map:

- Person to StaffArr `personId`
- Program name to TrainArr program
- Certificate type
- Issue date
- Expiration date
- Evidence file
- Issuer
- Qualification scope
- Historical status

### 21.3 Import Validation

Import should detect:

- Unknown person
- Duplicate record
- Expired certificate
- Missing evidence
- Invalid dates
- Unknown program
- Conflicting qualification
- Missing scope

### 21.4 Historical Records

TrainArr should support historical records that:

- Do not require full modern step evidence
- Are clearly marked as imported
- Preserve source file/reference
- Can still feed qualification status if accepted
- Are included in audit packages with source context

---

## 22. Tenant Settings

### 22.1 General Settings

Settings:

- Default expiration warning windows
- Default assignment due days
- Default evidence retention
- Default trainer/evaluator rules
- Default certificate numbering
- Default timezone
- Default audit package format

### 22.2 Qualification Settings

Settings:

- Work-blocking behavior
- Grace period behavior
- StaffArr publish behavior
- Cross-product warning behavior
- Override display behavior
- Expiration recalculation schedule

### 22.3 Integration Settings

Settings:

- StaffArr connection
- Compliance Core connection
- MaintainArr connection
- RoutArr connection
- SupplyArr connection
- Event publishing
- Event consumption
- Retry behavior
- Service credentials

### 22.4 Notification Settings

Settings:

- Email enabled
- In-app enabled
- Escalation windows
- Supervisor notification rules
- Compliance notification rules
- Digest frequency

---

## 23. Background Jobs

TrainArr should include background workers for:

- Expiration scanning
- Recertification assignment
- Qualification status recalculation
- StaffArr publish retries
- Event processing
- Notification dispatch
- Rule-pack impact processing
- Audit package generation
- Evidence retention processing
- Orphan reference detection
- Training assignment recalculation after StaffArr org changes

---

## 24. Observability and Admin Operations

### 24.1 System Health

Show:

- API status
- Database status
- Background job status
- Event queue status
- StaffArr connection
- NexArr connection
- Compliance Core connection
- Product integration status
- Failed event count
- Failed publish count

### 24.2 Audit Logs

Audit:

- Program changes
- Program publishing
- Assignment changes
- Evidence actions
- Signoffs
- Evaluations
- Certificate issuance
- Qualification changes
- Suspensions/revocations
- Manual corrections
- Admin settings changes
- Integration failures

### 24.3 Admin Recovery

Admins should be able to:

- Retry failed event
- Retry StaffArr publish
- Rebuild read models
- Recalculate assignments
- Recalculate qualifications
- View dead-letter queue
- Export diagnostic bundle
- Validate integration credentials

---

## 25. Suggested Database Areas

TrainArr should likely include tables/entities for:

- TrainingProgram
- TrainingProgramVersion
- TrainingProgramStep
- TrainingProgramApplicability
- TrainingProgramCitation
- TrainingCompletionRule
- TrainingResultDefinition
- TrainingAssignment
- TrainingAssignmentStepState
- TrainingAttempt
- TrainingEvidence
- TrainingSignoff
- TrainingEvaluation
- TrainingEvaluationForm
- TrainingEvaluationItem
- TrainingCertificate
- TrainingQualification
- TrainingQualificationScope
- TrainingRemediation
- TrainingNotification
- TrainingImportBatch
- TrainingImportRow
- TrainingAuditLog
- TrainingEventOutbox
- TrainingEventInbox
- IntegrationConnection
- BackgroundJobStatus

Because each product has its own PostgreSQL database, TrainArr should not use foreign keys directly into StaffArr, NexArr, MaintainArr, RoutArr, SupplyArr, or Compliance Core databases.

Use local references such as:

- `tenantId`
- `personId`
- `staffArrPersonRef`
- `siteRef`
- `departmentRef`
- `positionRef`
- `productRef`
- `externalEntityType`
- `externalEntityId`
- `complianceRuleRef`
- `citationRef`

---

## 26. Completion Criteria

TrainArr can be considered functionally complete when the following are true.

### 26.1 Program Lifecycle Complete

- Admins can create detailed programs.
- Programs are versioned.
- Steps support multiple completion types.
- Programs can be published, retired, duplicated, and audited.
- Old completions remain tied to the correct historical version.

### 26.2 Assignment Lifecycle Complete

- Training can be manually and automatically assigned.
- Assignments explain why they exist.
- Trainees can complete steps.
- Trainers/evaluators can sign off.
- Failed attempts can trigger remediation.
- Completion produces certificate/qualification results.

### 26.3 Qualification Lifecycle Complete

- TrainArr issues training-derived qualifications.
- Qualifications have scope, status, restrictions, and expiration.
- Qualifications publish to StaffArr.
- Suspensions/revocations/expirations also publish to StaffArr.
- Other products can check authorization through TrainArr.

### 26.4 Cross-Product Compatibility Complete

- NexArr validates tenant/product access.
- StaffArr remains source of truth for people/org.
- Compliance Core remains source of truth for rule/citation context.
- MaintainArr can request work-order qualification checks.
- RoutArr can request dispatch qualification checks.
- SupplyArr can request material-handling qualification checks.
- Products do not duplicate TrainArr training records.

### 26.5 Audit Defensibility Complete

- Person training history is complete.
- Evidence is preserved.
- Signoffs are preserved.
- Program versions are preserved.
- Point-in-time qualification can be proven.
- Audit packages can be generated without manual hunting.

### 26.6 Operational Readiness Complete

- Expiration jobs run.
- Recertification jobs run.
- Notifications work.
- Event retries work.
- Failed integrations are visible.
- Admin recovery tools exist.
- Reports and exports are usable.

---

## 27. V1 Priorities

A practical v1 should focus on:

1. Program builder with versioning.
2. Training steps replacing the current simple “training steps” model.
3. Assignment engine.
4. Trainee completion flow.
5. Evidence upload.
6. Trainer/evaluator signoff.
7. Certificate/qualification issuance.
8. StaffArr `personId` integration.
9. StaffArr qualification publishing.
10. Basic authorization check API.
11. Compliance citation attachment.
12. Expiration and renewal.
13. Training matrix.
14. Person audit package.
15. Cross-product event foundation.

---

## 28. Future Advanced Features

Advanced later-stage features:

- AI-assisted program drafting
- AI-assisted citation mapping
- AI-assisted evidence review
- Voice-guided training/evaluation
- Offline mobile evaluations
- QR-based equipment training launch
- Skill simulation support
- Training effectiveness analytics
- Predictive retraining recommendations
- Multi-language training content
- External LMS connectors
- Vendor/customer training portals
- Digital wallet certificates
- Smart badge/ID qualification checks
- Risk-weighted authorization scoring
- Training cost tracking
- Training labor tracking
- Workforce readiness forecasting

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

## Audit-Informed Feature Additions: Qualification Gates and Publication Reliability

### Qualification Check API

TrainArr should expose a stable authorization signal to products that need to know whether a person is qualified right now.

Features:

- Check qualification by `personId`, work/action key, product context, task/equipment/activity context, site, and effective time.
- Return qualified, not qualified, expired, suspended, missing, incomplete, remediation required, waived, or review-needed outcomes.
- Include program version, completion record, certificate/qualification ID, expiration, evidence summary, and trace ID.
- Include missing or stale dependency facts.
- Support batch checks for assignment boards.
- Support audit snapshots at decision time.

Completion criteria:

- MaintainArr, RoutArr, SupplyArr, and StaffArr can gate work assignment or execution without duplicating training logic.

### Qualification Publication to StaffArr

TrainArr should publish completed, updated, expired, suspended, or revoked qualifications to StaffArr reliably.

Features:

- Outbox event for qualification issued/updated/expired/suspended/revoked.
- Retry and dead-letter handling.
- Idempotency keys.
- StaffArr acknowledgement tracking.
- Reconciliation job for missed publications.
- Point-in-time program/citation/evidence snapshot.
- Person linkage through NexArr/StaffArr `personId`.

Completion criteria:

- StaffArr readiness changes when TrainArr qualification state changes, and failures are visible and recoverable.

### Immutable Training Versioning

Published training programs and completed training records must be immutable enough for audits.

Features:

- Draft/edit/publish lifecycle.
- Versioned program definitions.
- Versioned steps, evidence requirements, completion rules, and citation snapshots.
- Completion records tied to the program version completed.
- Superseded version handling.
- Renewal/retraining rules for changes that require new training.
- Audit export showing what the trainee completed at the time.

Completion criteria:

- Publishing version 3 of a program does not rewrite what a person completed under version 2.


## 29. Final Product Definition

TrainArr is not just a training checklist.

TrainArr is the system that proves a person was taught, tested, evaluated, signed off, and authorized before the business allowed them to perform the work.

A complete TrainArr should let STL Compliance say:

> We know who this person is from StaffArr.  
> We know the platform access is valid through NexArr.  
> We know which rules apply through Compliance Core.  
> We know what work is being attempted through MaintainArr, RoutArr, SupplyArr, or another product.  
> And we know through TrainArr whether this person is qualified to do it.

That is the end goal.
