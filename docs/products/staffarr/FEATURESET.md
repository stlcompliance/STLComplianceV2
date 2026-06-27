# StaffArr — HRM Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Document control

| Field | Value |
| --- | --- |
| Product | StaffArr (HRM) |
| Category | Human Resource Management |
| Repository maturity | Durable |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 72 |
| Cataloged workflows | 15 |

## Product charter

StaffArr is the tenant system of record for people in the workforce context, organizational structure, internal locations, assignments, delegated authority, personnel incidents, readiness context, and HR processes. It is also the practical administration surface for product roles and permission assignments. NexArr remains the source of truth for login credentials and sessions, while StaffArr may expose permissioned NexArr-backed account provisioning and account-edit actions.

> **Implementation reality — Durable:** StaffArr has one of the broadest persistent domains in the repository: people, organization and location hierarchies, roles and permissions, incidents and readiness, recruiting, applications, timekeeping, performance, benefits, compensation, exports, history, audit, and background workers. Some experiences remain integration- or UI-completion targets, but the core system is durable.

## Source-of-truth boundary

### StaffArr owns

- People/personnel profile, employment relationship, worker status, identifiers appropriate to HR, manager chain, and workforce history.
- Organization units, sites, buildings, warehouses, docks, rooms, yards, staging/quarantine areas, parts rooms, service counters, trucks when modeled as locations, shelves/bins when addressable, and transitional internal locations.
- Positions, teams, departments, assignments, supervisory relationships, availability context, and internal authority assignments.
- Staff roles, product permission templates, role scopes, person-role assignments, effective permission projections, and permission audit.
- Personnel incidents, notes, restrictions, readiness overrides, offboarding tasks, worker update requests, and workforce history.
- Recruiting requisitions, applications, candidates, interview stages, offers, and conversion into a person/employee.
- Timekeeping policy, clock events, work sessions, timesheets, time entries, labor allocation, leave, attendance, availability, corrections, exceptions, and attestations.
- Performance cycles, goals, competency assessment, feedback, performance improvement plans, benefits enrollment, dependents/beneficiaries, compensation profiles and changes.

### StaffArr does not own

- Platform credentials, authentication factors, sessions, or external IdP mappings; NexArr owns those truths.
- Training definitions, assignments, evaluations, certificates, or qualification issuance; TrainArr owns them and publishes outcomes to StaffArr.
- Payroll calculation, tax filing, payments, or general-ledger posting; StaffArr prepares time/compensation evidence and LedgArr or an external payroll system owns financial execution.
- Operational asset, inventory, route, order, quality, document, or compliance records.
- Customer/vendor identities; CustomArr and SupplyArr own external commercial parties in their respective domains.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Employee/worker
- Manager/supervisor
- HR administrator
- Recruiter
- Timekeeper
- Benefits/compensation administrator
- Tenant permission administrator
- Safety/incident reviewer
- Auditor

## Required integrations

- NexArr
- TrainArr
- All operational products
- RecordArr
- ReportArr
- Compliance Core
- LedgArr or external payroll
- Field Companion
- External benefits/background/recruiting providers

## Product principles

- A person record does not require a login; login management may be delegated through NexArr-owned actions.
- Internal locations are canonical StaffArr records and are referenced by LoadArr, MaintainArr, RoutArr, and other products.
- Sensitive HR data is purpose-limited; managers receive operational readiness answers without unrestricted personnel access.
- All workforce changes are effective-dated and preserve before/after history rather than overwriting prior truth.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 72 |
| Discovered server classes | 515 |
| Discovered HTTP route declarations | 370 |
| Frontend source files | 139 |
| Frontend page files | 29 |
| Documentation headings | 99 |

### Evidence used for the current-state classification

- Persistent People, OrgUnits, InternalLocations, OrgUnitAssignments, role/permission/scope/person-role records, permission projections, and audit logs.
- Persistent personnel incidents, notes, attachments, offboarding, training blockers/acknowledgements, readiness rollups, and personnel history rollups/events.
- Persistent employment application templates/submissions, requisitions, candidates, interview stages, and offers.
- Persistent timekeeping profiles, pay policies/codes, clock events, work sessions, leave, attendance, availability, timesheets, entries, labor allocations, exceptions, corrections, attestations, and labor-evidence inbox.
- Persistent performance cycles/goals/competencies/feedback/PIPs, benefit records, compensation profiles/change requests, exports/schedules/delivery runs, notifications, audit packages, and worker runs.
- staffarr-frontend routes for My StaffArr, people, organization, locations, roles, permissions, readiness, incidents, hiring, timekeeping, performance, benefits/compensation, reports, audit, imports, settings, and applications.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| ST-CUR-001 | People directory and person records | CURRENT | Durable | Durable workforce records with active/inactive state, assignments, history, notes, documents, readiness, and linked account context. |
| ST-CUR-002 | Organization hierarchy | CURRENT | Durable | Org units, reporting relationships, assignments, hierarchy views, and organization-scoped operations. |
| ST-CUR-003 | Canonical internal location hierarchy | CURRENT | Durable | Sites through operational sublocations and addressable storage/work areas, referenced by other products rather than duplicated. |
| ST-CUR-004 | Cross-product role and permission administration | CURRENT | Durable | Role definitions, permission catalog cache, scopes, person-role assignment, effective projections, audit, and permission-oriented UI. |
| ST-CUR-005 | Certification and readiness mirrors | CURRENT | Durable | StaffArr-facing certification definitions, person certification views, readiness overrides/rollups, training blockers, and TrainArr publication routing. |
| ST-CUR-006 | Personnel incident management | CURRENT | Durable | Incidents, notes, attachments, supply demand context, status events, TrainArr routing, and person history integration. |
| ST-CUR-007 | Personnel notes and documents | CURRENT | Durable | Permissioned personnel notes and document references with history and audit context. |
| ST-CUR-008 | Offboarding records and checklists | CURRENT | Durable | Structured offboarding record/steps tied to person status and downstream account/access actions. |
| ST-CUR-009 | Personnel update requests | CURRENT | Durable | Worker- or manager-submitted changes with review rather than uncontrolled direct edits. |
| ST-CUR-010 | Recruiting and application intake | CURRENT | Durable | Application templates, public/internal submissions, requisitions, candidates, interview stages, and offers. |
| ST-CUR-011 | Timekeeping and attendance | CURRENT | Durable | Profiles, policies, pay codes, clock events, work sessions, leave, attendance, availability, timesheets, entries, exceptions, corrections, and attestations. |
| ST-CUR-012 | Labor allocation and evidence intake | CURRENT | Durable | Allocate worked time to product records/projects and reconcile evidence from operational products. |
| ST-CUR-013 | Performance management | CURRENT | Durable | Review cycles, goals, competency assessments, feedback, and performance improvement plans. |
| ST-CUR-014 | Benefits and compensation administration | CURRENT | Durable | Enrollments, dependents, beneficiaries, compensation profiles, and change requests. |
| ST-CUR-015 | People exports, schedules, and audit packages | CURRENT | Durable | Presets, scheduled exports, delivery runs/notifications, history rollups, and audit package generation. |
| ST-CUR-016 | Tenant settings and worker/background runs | CURRENT | Durable | Tenant configuration, notifications, publication settings, and recurring worker execution. |

### B. Common category baseline

These are expected for a credible Human Resource Management product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| ST-COM-001 | Employee and contingent-worker master record | COMMON | Target | Support employees, contractors, temporary workers, volunteers, applicants, former workers, and rehires with effective-dated relationships. |
| ST-COM-002 | Position and job architecture | COMMON | Target | Job families, positions, grades, employment types, FLSA/classification fields, headcount, vacancies, and effective dates. |
| ST-COM-003 | Onboarding plans | COMMON | Target | Role/location-specific tasks, forms, equipment/access requests, training, acknowledgements, due dates, and accountable owners. |
| ST-COM-004 | Employee self-service | COMMON | Target | Profile updates, emergency contacts, leave, timesheets, documents, goals, benefits, acknowledgements, and account support within permissions. |
| ST-COM-005 | Manager self-service | COMMON | Target | Team roster, approvals, readiness, attendance, goals, compensation requests, development, incidents, and delegated approvals. |
| ST-COM-006 | Applicant tracking | COMMON | Target | Requisition approval, posting, application, screening, interviews, scorecards, references, offer, background-check status, and conversion. |
| ST-COM-007 | Time and attendance | COMMON | Target | Clock, schedule/availability, meal/rest capture, overtime policy, leave balances, exceptions, correction, attestation, and export. |
| ST-COM-008 | Leave management | COMMON | Target | Request, balance, eligibility, approval, documentation, overlapping absence, protected-leave flags, and return-to-work steps. |
| ST-COM-009 | Performance and development | COMMON | Target | Goals, check-ins, competencies, feedback, reviews, calibration, development plans, and improvement plans. |
| ST-COM-010 | Compensation administration | COMMON | Target | Salary/hourly profiles, effective-dated changes, approval, budget context, total compensation, and pay-equity review. |
| ST-COM-011 | Benefits administration | COMMON | Target | Plans/elections, dependents, beneficiaries, eligibility, life events, evidence, enrollment windows, and carrier export. |
| ST-COM-012 | Policy and handbook acknowledgements | COMMON | Target | Publish policies through RecordArr, assign acknowledgements, remind/escalate, and preserve signed evidence. |
| ST-COM-013 | Workforce analytics | COMMON | Target | Headcount, turnover, tenure, vacancy, time-to-hire, absence, overtime, readiness, training, incidents, performance, and diversity metrics with privacy controls. |
| ST-COM-014 | Data import/export and payroll bridge | COMMON | Target | Repeatable employee/time/benefit/compensation exchange with validation, mapping, reconciliation, and audit. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| ST-UND-001 | Simple but complete small-team mode | UNDERSERVED | Target | Progressive disclosure and sensible defaults provide real HR rigor without forcing a small employer through enterprise-only setup and terminology. |
| ST-UND-002 | One person timeline across HR and operations | UNDERSERVED | Target | A permission-aware timeline connects assignments, training, incidents, time, maintenance/route/warehouse work, documents, and changes without copying source records. |
| ST-UND-003 | Quick login provisioning from the person page | UNDERSERVED | Target | Appropriately permissioned HR users can create, suspend, or update NexArr login information without leaving StaffArr or violating NexArr ownership. |
| ST-UND-004 | Skills and capability graph | UNDERSERVED | Target | Show demonstrated skills, qualifications, experience, interests, and gaps rather than equating course completion with capability. |
| ST-UND-005 | Transparent employee update workflow | UNDERSERVED | Target | Workers see requested changes, reviewer, status, effective date, rejection reason, and which downstream systems changed. |
| ST-UND-006 | Cross-location readiness board | UNDERSERVED | Target | Managers can find qualified, available people by location, shift, role, and task without exposing unrelated HR data. |
| ST-UND-007 | Fair scheduling and shift exchange | UNDERSERVED | Target | Availability, preferences, rest rules, qualifications, coverage, swaps, open shifts, and approval are visible and explainable. |
| ST-UND-008 | Low-burden incident-to-support flow | UNDERSERVED | Target | Capture safety/quality/personnel events once, route confidential components correctly, and connect retraining/restrictions without duplicating the incident. |
| ST-UND-009 | Worker-owned portable evidence | UNDERSERVED | Target | Provide exportable certifications, training evidence, employment documents, and acknowledgements where policy allows. |
| ST-UND-010 | Privacy-preserving workforce analytics | UNDERSERVED | Target | Minimum group sizes, field suppression, purpose-based access, and audit prevent analytics from becoming covert surveillance. |
| ST-UND-011 | Rehire and multi-relationship handling | UNDERSERVED | Target | Preserve one person history while supporting multiple employment periods, concurrent roles, contractor-to-employee conversion, and future-dated changes. |
| ST-UND-012 | Guided classification and compliance questions | UNDERSERVED | Target | Plain-language questionnaires gather required facts and ask Compliance Core for implications rather than embedding legal conclusions in HR forms. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| ST-DEM-001 | Enterprise-grade role mining and access review | DEMOCRATIZE | Target | Use actual job/assignment patterns to suggest role cleanup, identify excessive access, and run manager/system-owner certifications. |
| ST-DEM-002 | Workforce planning and scenario modeling | DEMOCRATIZE | Target | Model vacancies, transfers, overtime, training lead time, succession risk, seasonal demand, and budget without changing live records. |
| ST-DEM-003 | Compensation cycle and pay-equity analysis | DEMOCRATIZE | Target | Budget pools, manager worksheets, calibration, range penetration, compa-ratio, protected-class-safe analysis, approvals, and audit. |
| ST-DEM-004 | Advanced scheduling optimization | DEMOCRATIZE | Target | Generate explainable schedules using demand, skills, availability, preferences, rest limits, cost, and fairness constraints. |
| ST-DEM-005 | Case management with confidentiality walls | DEMOCRATIZE | Target | HR cases, investigations, accommodations, grievances, and sensitive notes with need-to-know access, legal hold, and separate evidence packages. |
| ST-DEM-006 | Succession and talent review | DEMOCRATIZE | Target | Critical roles, successors, readiness, development actions, retention risk, and calibration with strict access controls. |
| ST-DEM-007 | People analytics semantic layer | DEMOCRATIZE | Target | Reusable governed definitions for headcount, turnover, overtime, absence, diversity, and readiness available through ReportArr without consultant-built BI. |
| ST-DEM-008 | Global/effective-dated workforce administration | DEMOCRATIZE | Target | Country/location-specific fields, contracts, calendars, time rules, translations, currencies, and future-dated organizational changes. |
| ST-DEM-009 | External worker and agency portal | DEMOCRATIZE | Target | Scoped onboarding, documents, time confirmation, qualifications, assignments, and offboarding for contractors and staffing partners. |
| ST-DEM-010 | Automated evidence-ready HR audits | DEMOCRATIZE | Target | Generate a role-, location-, period-, or person-scoped package of policies, acknowledgements, changes, time attestations, access, and qualifications. |

### E. Suite-wide foundation required in StaffArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| ST-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| ST-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| ST-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| ST-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| ST-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. StaffArr now exposes governed site quick-create for downstream products, including MaintainArr PM owning-site selection and the StaffArr person-create placement and home-base location flow. |
| ST-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| ST-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| ST-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| ST-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| ST-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| ST-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| ST-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| ST-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| ST-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| ST-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| ST-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| ST-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| ST-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| ST-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| ST-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

## Cross-cutting nonfunctional requirements

| Area | Acceptance requirement |
| --- | --- |
| Security and tenancy | Every server operation validates tenant, identity/service principal, action permission, subject scope, and object tenant. Client-provided tenant, role, status, amount, or decision data is never trusted. |
| Auditability | Record actor/service, source, before/after or immutable event, reason, effective time, correlation/causation, version, approvals, overrides, and external calls. Audit logs are searchable but not user-editable. |
| Idempotency and concurrency | Commands support idempotency and optimistic concurrency or explicit conflict behavior. Retries, imports, events, and offline sync cannot create duplicate business effects. |
| Availability and degradation | Each dependency has timeout, retry/circuit behavior, health visibility, saved-state guarantees, and a user-readable degraded path. Safety/compliance/financial hard gates never silently fail open. |
| Privacy and data minimization | Collect only domain-required data, classify sensitive fields, restrict exports/logs/notifications, support retention and lawful correction/deletion, and avoid covert employee or device tracking. |
| Accessibility and responsive design | Meet keyboard, screen-reader, contrast, zoom/reflow, focus, error-identification, target-size, reduced-motion, and mobile requirements in both light and dark modes. |
| Performance | Use pagination/virtualization, asynchronous long jobs, bounded queries, indexes, backpressure, caching with invalidation, and measurable latency/error budgets. |
| Observability | Emit structured logs, metrics, traces, job/event status, dead-letter/quarantine state, dependency health, and correlation IDs without secrets or excessive personal data. |
| Configuration governance | Tenant configuration is versioned, validated, permissioned, explainable, testable, exportable, and recoverable. Product behavior is not hidden in hard-coded UI-only rules. |
| Integration contracts | APIs/events are versioned, documented, idempotent, tenant-scoped, effective-time aware, and backward-compatible within policy; no cross-product database foreign keys. |
| Data portability and professional output | Users can obtain useful structured exports and report-quality printable artifacts without the application shell or enterprise-only licensing. |
| AI safety and provenance | AI output is a proposal with source/context/confidence and human review. AI cannot reveal secrets, bypass permissions, invent records, or silently commit consequential changes. |

## Repository object inventory

<details>
<summary>Persistent entity sets (72)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| StaffPerson | People | StaffArr.Api/Data/StaffArrDbContext.cs |
| OrgUnit | OrgUnits | StaffArr.Api/Data/StaffArrDbContext.cs |
| InternalLocation | InternalLocations | StaffArr.Api/Data/StaffArrDbContext.cs |
| OrgUnitAssignment | OrgUnitAssignments | StaffArr.Api/Data/StaffArrDbContext.cs |
| PermissionTemplate | PermissionTemplates | StaffArr.Api/Data/StaffArrDbContext.cs |
| StaffRole | StaffRoles | StaffArr.Api/Data/StaffArrDbContext.cs |
| StaffRolePermission | StaffRolePermissions | StaffArr.Api/Data/StaffArrDbContext.cs |
| StaffRoleScope | StaffRoleScopes | StaffArr.Api/Data/StaffArrDbContext.cs |
| StaffPersonRole | StaffPersonRoles | StaffArr.Api/Data/StaffArrDbContext.cs |
| PermissionCatalogCacheEntry | PermissionCatalogCacheEntries | StaffArr.Api/Data/StaffArrDbContext.cs |
| PermissionAuditLogEntry | PermissionAuditLogEntries | StaffArr.Api/Data/StaffArrDbContext.cs |
| CertificationDefinition | CertificationDefinitions | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonCertification | PersonCertifications | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonReadinessOverride | PersonReadinessOverrides | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonnelIncident | PersonnelIncidents | StaffArr.Api/Data/StaffArrDbContext.cs |
| IncidentNote | IncidentNotes | StaffArr.Api/Data/StaffArrDbContext.cs |
| IncidentAttachment | IncidentAttachments | StaffArr.Api/Data/StaffArrDbContext.cs |
| IncidentSupplyDemandLine | IncidentSupplyDemandLines | StaffArr.Api/Data/StaffArrDbContext.cs |
| IncidentSupplyDemandStatusEvent | IncidentSupplyDemandStatusEvents | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonnelNote | PersonnelNotes | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonnelDocument | PersonnelDocuments | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonTrainingBlocker | PersonTrainingBlockers | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonTrainingAcknowledgement | PersonTrainingAcknowledgements | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonOffboardingRecord | PersonOffboardingRecords | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonOffboardingStep | PersonOffboardingSteps | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonnelUpdateRequest | PersonnelUpdateRequests | StaffArr.Api/Data/StaffArrDbContext.cs |
| IncidentTrainarrRouting | IncidentTrainarrRoutings | StaffArr.Api/Data/StaffArrDbContext.cs |
| ReadinessRollup | ReadinessRollups | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonPermissionProjection | PersonPermissionProjections | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonPermissionProjectionEntry | PersonPermissionProjectionEntries | StaffArr.Api/Data/StaffArrDbContext.cs |
| StaffArrAuditEvent | AuditEvents | StaffArr.Api/Data/StaffArrDbContext.cs |
| TenantPersonExportPreset | TenantPersonExportPresets | StaffArr.Api/Data/StaffArrDbContext.cs |
| TenantPersonExportSchedule | TenantPersonExportSchedules | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonExportDeliveryRun | PersonExportDeliveryRuns | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonExportDeliveryNotification | PersonExportDeliveryNotifications | StaffArr.Api/Data/StaffArrDbContext.cs |
| AuditPackageGenerationJob | AuditPackageGenerationJobs | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonnelHistoryRollup | PersonnelHistoryRollups | StaffArr.Api/Data/StaffArrDbContext.cs |
| PersonnelHistoryEvent | PersonnelHistoryEvents | StaffArr.Api/Data/StaffArrDbContext.cs |
| TenantStaffArrWorkerSettings | TenantStaffArrWorkerSettings | StaffArr.Api/Data/StaffArrDbContext.cs |
| StaffArrTenantSettings | TenantSettings | StaffArr.Api/Data/StaffArrDbContext.cs |
| EmploymentApplicationTemplate | EmploymentApplicationTemplates | StaffArr.Api/Data/StaffArrDbContext.cs |
| EmploymentApplicationSubmission | EmploymentApplicationSubmissions | StaffArr.Api/Data/StaffArrDbContext.cs |
| RecruitingRequisition | RecruitingRequisitions | StaffArr.Api/Data/StaffArrDbContext.cs |
| RecruitingCandidate | RecruitingCandidates | StaffArr.Api/Data/StaffArrDbContext.cs |
| RecruitingInterviewStage | RecruitingInterviewStages | StaffArr.Api/Data/StaffArrDbContext.cs |
| RecruitingOffer | RecruitingOffers | StaffArr.Api/Data/StaffArrDbContext.cs |
| StaffArrWorkerRun | StaffArrWorkerRuns | StaffArr.Api/Data/StaffArrDbContext.cs |
| TimekeepingProfile | TimekeepingProfiles | StaffArr.Api/Data/StaffArrDbContext.cs |
| PayPolicy | TimekeepingPayPolicies | StaffArr.Api/Data/StaffArrDbContext.cs |
| PayCode | TimekeepingPayCodes | StaffArr.Api/Data/StaffArrDbContext.cs |
| ClockEvent | TimekeepingClockEvents | StaffArr.Api/Data/StaffArrDbContext.cs |
| WorkSession | TimekeepingWorkSessions | StaffArr.Api/Data/StaffArrDbContext.cs |
| LeaveRequest | TimekeepingLeaveRequests | StaffArr.Api/Data/StaffArrDbContext.cs |
| AttendanceEvent | TimekeepingAttendanceEvents | StaffArr.Api/Data/StaffArrDbContext.cs |
| AvailabilityBlock | TimekeepingAvailabilityBlocks | StaffArr.Api/Data/StaffArrDbContext.cs |
| PerformanceReviewCycle | PerformanceReviewCycles | StaffArr.Api/Data/StaffArrDbContext.cs |
| PerformanceGoal | PerformanceGoals | StaffArr.Api/Data/StaffArrDbContext.cs |
| PerformanceCompetencyAssessment | PerformanceCompetencyAssessments | StaffArr.Api/Data/StaffArrDbContext.cs |
| PerformanceFeedbackEntry | PerformanceFeedbackEntries | StaffArr.Api/Data/StaffArrDbContext.cs |
| PerformanceImprovementPlan | PerformanceImprovementPlans | StaffArr.Api/Data/StaffArrDbContext.cs |
| BenefitEnrollment | BenefitEnrollments | StaffArr.Api/Data/StaffArrDbContext.cs |
| BenefitDependent | BenefitDependents | StaffArr.Api/Data/StaffArrDbContext.cs |
| BenefitBeneficiary | BenefitBeneficiaries | StaffArr.Api/Data/StaffArrDbContext.cs |
| CompensationProfile | CompensationProfiles | StaffArr.Api/Data/StaffArrDbContext.cs |
| CompensationChangeRequest | CompensationChangeRequests | StaffArr.Api/Data/StaffArrDbContext.cs |
| TimesheetPeriod | TimekeepingTimesheetPeriods | StaffArr.Api/Data/StaffArrDbContext.cs |
| TimeEntry | TimekeepingTimeEntries | StaffArr.Api/Data/StaffArrDbContext.cs |
| LaborAllocation | TimekeepingLaborAllocations | StaffArr.Api/Data/StaffArrDbContext.cs |
| TimeException | TimekeepingExceptions | StaffArr.Api/Data/StaffArrDbContext.cs |
| TimeCorrection | TimekeepingCorrections | StaffArr.Api/Data/StaffArrDbContext.cs |
| TimeAttestation | TimekeepingAttestations | StaffArr.Api/Data/StaffArrDbContext.cs |
| LaborEvidenceInboxItem | TimekeepingLaborEvidenceInbox | StaffArr.Api/Data/StaffArrDbContext.cs |

</details>

<details>
<summary>Frontend page files (29)</summary>

| Page |
| --- |
| src/lib/createWorkspacePage.tsx |
| src/pages/LaunchPage.tsx |
| src/workspace/StaffArrWorkspacePage.tsx |
| src/pages/admin/AdminPage.tsx |
| src/pages/audit-packages/AuditPackagesPage.tsx |
| src/pages/benefits-compensation/BenefitsCompensationPage.tsx |
| src/pages/certifications/CertificationsPage.tsx |
| src/pages/employment-applications/EmploymentApplicationsPage.tsx |
| src/pages/hrm/HrmPage.tsx |
| src/pages/imports/ImportsPage.tsx |
| src/pages/incidents/IncidentCreatePage.tsx |
| src/pages/incidents/IncidentsPage.tsx |
| src/pages/locations/LocationsPage.tsx |
| src/pages/me/MePage.tsx |
| src/pages/my-team/MyTeamPage.tsx |
| src/pages/org/OrgPage.tsx |
| src/pages/organization-structure/OrganizationStructurePage.tsx |
| src/pages/people/PeoplePage.tsx |
| src/pages/performance/PerformancePage.tsx |
| src/pages/permissions/PermissionsPage.tsx |
| src/pages/readiness/ReadinessPage.tsx |
| src/pages/recruiting/RecruitingPage.tsx |
| src/pages/reports/ReportsPage.tsx |
| src/pages/restrictions/RestrictionsPage.tsx |
| src/pages/roles/RolesPage.tsx |
| src/pages/settings/SettingsPage.tsx |
| src/pages/timekeeping/TimekeepingPage.tsx |
| src/pages/timekeeping/TimesheetDetailPage.tsx |
| src/pages/training-acknowledgements/TrainingAcknowledgementsPage.tsx |

</details>

<details>
<summary>Endpoint source families (45)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| IntegrationEndpoints.cs | 52 |
| TimekeepingEndpoints.cs | 48 |
| V1FeatureAliasEndpoints.cs | 34 |
| RecruitingEndpoints.cs | 18 |
| RoleManagementEndpoints.cs | 17 |
| BenefitsCompensationEndpoints.cs | 15 |
| PerformanceEndpoints.cs | 15 |
| MePortalEndpoints.cs | 10 |
| EmploymentApplicationEndpoints.cs | 9 |
| IncidentEndpoints.cs | 9 |
| PeopleEndpoints.cs | 9 |
| PeopleExportEndpoints.cs | 9 |
| ReadinessRollupEndpoints.cs | 9 |
| AuditPackageEndpoints.cs | 8 |
| AuthEndpoints.cs | 8 |
| OrgUnitEndpoints.cs | 8 |
| PersonAccountAccessEndpoints.cs | 7 |
| LocationEndpoints.cs | 6 |
| OffboardingEndpoints.cs | 5 |
| ReferenceIntegrationEndpoints.cs | 5 |
| EntityExportEndpoints.cs | 4 |
| FieldsetEndpoints.cs | 4 |
| ImportEndpoints.cs | 4 |
| ManagerHierarchyEndpoints.cs | 4 |
| OrgUnitAssignmentEndpoints.cs | 4 |
| PersonnelDocumentEndpoints.cs | 4 |
| PersonnelHistoryEndpoints.cs | 4 |
| ReadinessEndpoints.cs | 4 |
| StaffArrWorkerAdminEndpoints.cs | 4 |
| IncidentSupplyDemandEndpoints.cs | 3 |
| PersonnelNoteEndpoints.cs | 3 |
| PersonnelUpdateRequestEndpoints.cs | 3 |
| TenantSettingsEndpoints.cs | 3 |
| EventAndAuditEndpoints.cs | 2 |
| InternalAuditPackageGenerationEndpoints.cs | 2 |
| InternalCertificationExpirationEndpoints.cs | 2 |
| InternalPermissionProjectionEndpoints.cs | 2 |
| InternalPersonExportDeliveryEndpoints.cs | 2 |
| InternalPersonnelHistoryEndpoints.cs | 2 |
| InternalReadinessRollupEndpoints.cs | 2 |
| PersonLookupEndpoints.cs | 2 |
| TrainingAcknowledgementEndpoints.cs | 2 |
| CertificationEndpoints.cs | 1 |
| FieldInboxEndpoints.cs | 1 |
| SettingsEndpoints.cs | 1 |

</details>

## Implementation order

| Phase | Exit objective |
| --- | --- |
| 0 — Boundary and durability | Remove shadow ownership, in-memory/static production paths, legacy access conflicts, cross-DB assumptions, and unaudited writes. Establish tenant-safe persistence and event/API contracts. |
| 1 — Current-path hardening | Make every currently implemented workflow complete, permissioned, observable, recoverable, accessible, and consistent in light/dark/mobile/print states. |
| 2 — Common baseline | Deliver the category-standard capabilities in the `COMMON` catalog with migrations, APIs, workflows, UI, reporting, imports/exports, and tests. |
| 3 — Underserved differentiation | Prioritize high-frequency friction, SMB affordability, transparent limits, quick create, evidence reuse, offline/mobile execution, and owner-respecting integration. |
| 4 — Enterprise democratization | Add advanced analytics, automation, optimization, collaboration, governance, and ecosystem functions without commercial feature withholding or opaque AI. |

### Immediate product priority

Complete unified delegated identity/account management through NexArr-backed actions and keep person/location/organization boundaries canonical.

## Definition of done for every feature

- The owning domain, actor permissions, tenant boundary, state model, effective dates, concurrency, idempotency, and source references are explicit.
- Create, read, update/correct, archive/void/close, details, history, search/list, import/export, bulk action, notification, print/report, and API/event behavior exist where the domain permits them.
- The UI includes empty, loading, success, validation, permission-denied, conflict, dependency-down, partial-failure, and retry states in light/dark and responsive layouts.
- Quick create is available for missing permitted reference entities without abandoning the current operation.
- Cross-product reads and writes use authenticated APIs/events; no cross-product database foreign keys or UI-only write shortcuts are introduced.
- Audit, metrics, logs, traces, outbox/retry, data retention, accessibility, security, privacy, and automated tests meet the nonfunctional requirements above.
- AI, automation, optimization, and recommendation features expose inputs, assumptions, confidence, alternatives, and approval; they never silently commit consequential records.

## Related workflow specification

The operational state machines, triggers, actors, steps, exceptions, evidence, handoffs, mobile behavior, and measures are defined in [WORKFLOWS.md](./WORKFLOWS.md).
