# TrainArr — LMS Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Document control

| Field | Value |
| --- | --- |
| Product | TrainArr (LMS) |
| Category | Learning Management System |
| Repository maturity | Durable |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 73 |
| Cataloged workflows | 15 |

## Product charter

TrainArr owns learning definitions, programs, versions, assignments, execution progress, evaluations, practical signoffs, remediation, qualifications, certificates, and training evidence. It converts role, asset, location, incident, and compliance requirements into demonstrable readiness. StaffArr owns the person and workforce assignment; Compliance Core owns regulatory meaning; RecordArr owns files.

> **Implementation reality — Durable:** TrainArr has persistent program/version, definition/step/branch/completion-rule, assignment/progress, evaluation, signoff, evidence, qualification, certificate-publication, remediation, renewal, applicability, notification, retention, impact-analysis, integration, history, audit, and worker-run models. The strongest remaining opportunities are content interoperability, richer learner experience, skill visibility, offline execution, and enterprise-grade automation made accessible to smaller teams.

## Source-of-truth boundary

### TrainArr owns

- Training definitions, steps, completion rules, branches, content references, programs, versions, and publication lifecycle.
- Assignments, enrollment, learner progress, labor/material demand associated with training execution, due dates, status, and completion.
- Assessments/evaluations, revisions, practical observations, instructor/evaluator signoffs, and evidence links.
- Qualifications, qualification checks/issues, certificate issuance/publication, renewal, suspension, remediation, and recertification.
- Training matrices, applicability profiles, requirements, rulepack impact, assignment escalation/reminders, and training history.
- TrainArr tenant settings, notification policy, evidence retention, orphan-reference checks, integrations, and publication to StaffArr.

### TrainArr does not own

- Person identity, employment, manager, organization, or location; StaffArr owns those records.
- Legal interpretation or applicability rules; Compliance Core supplies rule/fact/evidence context.
- Personnel incident truth; StaffArr owns the incident while TrainArr owns remediation/training outcomes.
- File binaries and controlled-document lifecycle; RecordArr owns them.
- Operational permission to perform a task; the owning product enforces action permission and may use TrainArr qualification results.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Learner
- Manager
- Training administrator
- Instructor
- Evaluator/assessor
- Program owner
- Subject-matter expert
- Compliance reviewer
- Auditor
- External learner

## Required integrations

- StaffArr
- Compliance Core
- RecordArr
- ReportArr
- Field Companion
- NexArr notifications
- Calendar/conferencing providers
- External content/credential providers
- Operational products

## Product principles

- Training completion, qualification, and operational permission are distinct truths.
- Every issued qualification must be traceable to versioned requirements, evidence, and authorized evaluators.
- Content may be referenced from RecordArr; TrainArr owns learning structure and completion evidence, not file storage.
- Applicability and due dates must be explainable to learners and managers.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 56 |
| Discovered server classes | 492 |
| Discovered HTTP route declarations | 211 |
| Frontend source files | 136 |
| Frontend page files | 19 |
| Documentation headings | 93 |

### Evidence used for the current-state classification

- Persistent TrainingDefinitions, steps, completion rules, step branches, programs, program definitions/content references, versions, and version definitions.
- Persistent assignments, step progress, labor entries, material demand/status, evaluations/revisions, signoffs, evidence, citation attachments, and rulepack requirements.
- Persistent qualification issues/checks, certificate publications, StaffArr incident remediations, training matrices, applicability profiles, and requirements.
- Persistent reminder/escalation/recertification/qualification recalculation/rulepack impact/evidence retention/orphan-reference settings and runs.
- Persistent notification dispatch, StaffArr publication delivery, integration/event processing, domain events, person training history, audit, and audit packages.
- trainarr-frontend routes for learner catalog/player, assignments, manual entry, instructor/evaluator queues, evidence, remediation, citations/rulepacks, matrix, certificates, qualifications, reports, and settings.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| TR-CUR-001 | Training definition and step builder | CURRENT | Durable | Definitions contain ordered steps, completion rules, branches, and reusable content/requirement context. |
| TR-CUR-002 | Program composition and versioning | CURRENT | Durable | Programs group definitions and content references into versioned, publishable structures. |
| TR-CUR-003 | Assignment and learner progress | CURRENT | Durable | Assignments and per-step progress support individual execution, due dates, and status. |
| TR-CUR-004 | Conditional completion rules and branching | CURRENT | Durable | Step branches and completion rules support more than a linear checklist. |
| TR-CUR-005 | Learner course/player routes | CURRENT | Durable | Learners have catalog, my-training, assignment, and course-player experiences. |
| TR-CUR-006 | Manual completion and administration queues | CURRENT | Durable | Manual assignment/progress entry and administrative/instructor/evaluator queues are represented. |
| TR-CUR-007 | Evaluations and revisions | CURRENT | Durable | Structured evaluations and revision history support knowledge/practical assessment correction. |
| TR-CUR-008 | Practical signoffs | CURRENT | Durable | Authorized evaluators can attest observed capability or step completion. |
| TR-CUR-009 | Training evidence | CURRENT | Durable | Evidence records, citation attachments, and RecordArr-style references support proof of completion. |
| TR-CUR-010 | Qualification checks and issues | CURRENT | Durable | Issue/check models support pass, warning, blocked, stale, missing, or disputed readiness outcomes. |
| TR-CUR-011 | Certificates and StaffArr publication | CURRENT | Durable | Certificate publications and delivery records expose qualification outcomes to workforce views. |
| TR-CUR-012 | Incident remediation | CURRENT | Durable | StaffArr incident handoffs can create remediation and retraining assignments. |
| TR-CUR-013 | Training matrix and applicability profiles | CURRENT | Durable | Role/person/context requirements can be represented and recalculated. |
| TR-CUR-014 | Renewal and recertification workers | CURRENT | Durable | Tenant settings and runs support recertification assignments, reminders, and escalation. |
| TR-CUR-015 | Rulepack impact analysis | CURRENT | Durable | Changes in Compliance Core requirements can be detected and routed for training impact review. |
| TR-CUR-016 | Evidence retention and orphan-reference workers | CURRENT | Durable | Automated checks identify stale evidence or broken source references. |
| TR-CUR-017 | Training history, audit, notifications, and integrations | CURRENT | Durable | Person history entries, domain events, dispatch records, integration settings, and audit packages are durable. |

### B. Common category baseline

These are expected for a credible Learning Management System product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| TR-COM-001 | Course and content authoring | COMMON | Target | Rich text, documents, video, audio, links, embedded activities, question banks, reusable modules, and preview. |
| TR-COM-002 | Standards-based content interoperability | COMMON | Target | SCORM 1.2/2004, xAPI, cmi5, and LTI import/launch with version, completion, score, and statement handling. |
| TR-COM-003 | Learning paths and prerequisites | COMMON | Target | Ordered or conditional paths, equivalencies, exemptions, credit transfer, and prerequisite enforcement. |
| TR-COM-004 | Assessments and question banks | COMMON | Target | Multiple item types, randomization, attempts, passing rules, feedback, accommodations, and item analytics. |
| TR-COM-005 | Instructor-led and virtual sessions | COMMON | Target | Sessions, capacity, waitlists, attendance, rosters, calendar, conferencing links, and instructor materials. |
| TR-COM-006 | Practical skills evaluation | COMMON | Target | Observation checklists, assessor qualifications, evidence, signatures, remediation, and re-evaluation. |
| TR-COM-007 | Certificates and expiration | COMMON | Target | Branded certificates, verification, validity windows, renewal windows, suspension/revocation, and printable/portable proof. |
| TR-COM-008 | Automatic enrollment | COMMON | Target | Assign from StaffArr role, location, assignment, manager, hire/transfer, risk, asset/work type, or compliance applicability. |
| TR-COM-009 | Compliance reminders and escalation | COMMON | Target | Due/overdue/expiring notifications, manager escalation, grace periods, and policy-driven consequences. |
| TR-COM-010 | Learner and manager dashboards | COMMON | Target | Current assignments, progress, due dates, blockers, team readiness, skills gaps, and recommended actions. |
| TR-COM-011 | Content and program version control | COMMON | Target | Publish, supersede, migrate learners, freeze historical evidence, and compare versions. |
| TR-COM-012 | Surveys and feedback | COMMON | Target | Course evaluation, confidence, instructor feedback, and delayed effectiveness checks. |
| TR-COM-013 | Training reports and audit packages | COMMON | Target | Completion, overdue, qualification, certificate, attendance, assessment, evidence, and history by person/role/location/period. |
| TR-COM-014 | Accessibility and localization | COMMON | Target | Captions, transcripts, keyboard/screen-reader support, language variants, timezone-safe sessions, and accessible assessment alternatives. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| TR-UND-001 | Capability, not completion, visibility | UNDERSERVED | Target | Separate attendance/course completion from observed skill, current qualification, confidence, practice, and work performance evidence. |
| TR-UND-002 | Offline-first learning and practical signoff | UNDERSERVED | Target | Download permitted content, execute checklists/assessments, capture evidence, and resolve sync conflicts in low-connectivity environments. |
| TR-UND-003 | Training embedded in operational work | UNDERSERVED | Target | Launch the exact micro-instruction, checklist, or qualification step from a work order, route, warehouse task, quality action, or incident. |
| TR-UND-004 | Fast role-based assignment with explanations | UNDERSERVED | Target | Show why each person is assigned, which rule/role/location triggered it, and how to resolve an incorrect assignment. |
| TR-UND-005 | Portable worker credential wallet | UNDERSERVED | Target | Give workers verifiable, privacy-controlled copies of certificates, skills, expirations, and evidence where policy permits. |
| TR-UND-006 | Small-team instructor workflow | UNDERSERVED | Target | One person can schedule, teach, assess, sign off, and close a session without navigating separate enterprise modules while controls remain intact. |
| TR-UND-007 | Evidence-backed equivalency and prior learning | UNDERSERVED | Target | Review external certificates, experience, military/apprenticeship records, and demonstrations for credit with explicit confidence and expiry. |
| TR-UND-008 | Contextual remediation | UNDERSERVED | Target | Generate focused retraining from failed questions, observed steps, incidents, or expired subskills instead of repeating an entire course. |
| TR-UND-009 | Spaced reinforcement and knowledge checks | UNDERSERVED | Target | Schedule brief follow-ups after training to measure retention and trigger coaching before full retraining is needed. |
| TR-UND-010 | Manager coaching and peer practice | UNDERSERVED | Target | Structured coaching plans, mentor assignments, peer observation, and discussion tied to measurable skills. |
| TR-UND-011 | Transparent learner workload | UNDERSERVED | Target | Show estimated duration, due-date rationale, prerequisites, scheduling conflicts, and why a requirement applies. |
| TR-UND-012 | Content reuse without cloning | UNDERSERVED | Target | Reference controlled procedures or media once and safely reuse them across programs while preserving version-specific evidence. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| TR-DEM-001 | AI-assisted course and assessment drafting | DEMOCRATIZE | Target | Draft objectives, outlines, scenarios, questions, translations, summaries, and remediation from approved source material with citations and human review. |
| TR-DEM-002 | Skills ontology and workforce skill graph | DEMOCRATIZE | Target | Map content, assessments, observations, qualifications, jobs, and operational tasks to a governed skill model accessible to smaller employers. |
| TR-DEM-003 | Adaptive learning paths | DEMOCRATIZE | Target | Use demonstrated mastery and failed subskills to adjust practice while preserving auditable completion requirements. |
| TR-DEM-004 | Simulation and scenario authoring | DEMOCRATIZE | Target | Branching scenarios, role plays, equipment/process simulations, and scored decision paths without custom development. |
| TR-DEM-005 | Proctoring and identity assurance options | DEMOCRATIZE | Target | Risk-based identity checks, environment attestation, live/recorded review, privacy safeguards, and accommodations rather than mandatory surveillance. |
| TR-DEM-006 | External/customer/partner academies | DEMOCRATIZE | Target | Branded portals, self-registration/invitation, commerce or contract access, tenant separation, and portable certificates. |
| TR-DEM-007 | Training effectiveness analytics | DEMOCRATIZE | Target | Link training cohorts to incidents, defects, quality, productivity, rework, and readiness while controlling confounding and privacy. |
| TR-DEM-008 | Competency-based qualification engine | DEMOCRATIZE | Target | Combine knowledge, observation, experience, recency, supervisor approval, and evidence into explainable readiness. |
| TR-DEM-009 | Automated regulation-to-training impact | DEMOCRATIZE | Target | When Compliance Core changes a rulepack, identify affected programs, content, people, evidence, and deadlines before publishing changes. |
| TR-DEM-010 | Multilingual content transformation | DEMOCRATIZE | Target | Create reviewed language variants, captions, voiceover, terminology glossaries, and equivalency tracking without outsourcing every update. |

### E. Suite-wide foundation required in TrainArr

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| TR-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| TR-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| TR-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| TR-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| TR-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| TR-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| TR-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| TR-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| TR-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| TR-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| TR-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| TR-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| TR-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| TR-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| TR-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| TR-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| TR-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| TR-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| TR-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| TR-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Persistent entity sets (56)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| CertificationPublication | CertificationPublications | TrainArr.Api/Data/TrainArrDbContext.cs |
| StaffarrIncidentRemediation | StaffarrIncidentRemediations | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingDefinition | TrainingDefinitions | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingDefinitionStep | TrainingDefinitionSteps | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingDefinitionCompletionRule | TrainingDefinitionCompletionRules | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingDefinitionStepBranch | TrainingDefinitionStepBranches | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingAssignmentStepProgress | TrainingAssignmentStepProgress | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingAssignment | TrainingAssignments | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingAssignmentLaborEntry | TrainingAssignmentLaborEntries | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingAssignmentMaterialDemandLine | TrainingAssignmentMaterialDemandLines | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingAssignmentMaterialDemandStatusEvent | TrainingAssignmentMaterialDemandStatusEvents | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingProgram | TrainingPrograms | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingProgramDefinition | TrainingProgramDefinitions | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingProgramContentReference | TrainingProgramContentReferences | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingProgramVersion | TrainingProgramVersions | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingProgramVersionDefinition | TrainingProgramVersionDefinitions | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingMatrixEntry | TrainingMatrixEntries | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingApplicabilityProfile | TrainingApplicabilityProfiles | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingRequirement | TrainingRequirements | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingEvidence | TrainingEvidence | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingEvaluation | TrainingEvaluations | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingEvaluationRevision | TrainingEvaluationRevisions | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingSignoff | TrainingSignoffs | TrainArr.Api/Data/TrainArrDbContext.cs |
| QualificationIssue | QualificationIssues | TrainArr.Api/Data/TrainArrDbContext.cs |
| QualificationCheckRecord | QualificationCheckRecords | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingCitationAttachment | TrainingCitationAttachments | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingRulePackRequirement | TrainingRulePackRequirements | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainArrTenantSettings | TrainArrTenantSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantTrainingNotificationSettings | TenantTrainingNotificationSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantAssignmentDueReminderSettings | TenantAssignmentDueReminderSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| AssignmentDueReminderRun | AssignmentDueReminderRuns | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantAssignmentEscalationSettings | TenantAssignmentEscalationSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| AssignmentEscalationEvent | AssignmentEscalationEvents | TrainArr.Api/Data/TrainArrDbContext.cs |
| AssignmentEscalationRun | AssignmentEscalationRuns | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantRecertificationSettings | TenantRecertificationSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantQualificationRecalculationSettings | TenantQualificationRecalculationSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| QualificationRecalculationState | QualificationRecalculationStates | TrainArr.Api/Data/TrainArrDbContext.cs |
| QualificationRecalculationRun | QualificationRecalculationRuns | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantRulePackImpactSettings | TenantRulePackImpactSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| RulePackImpactState | RulePackImpactStates | TrainArr.Api/Data/TrainArrDbContext.cs |
| RulePackImpactRun | RulePackImpactRuns | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantEvidenceRetentionSettings | TenantEvidenceRetentionSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| EvidenceRetentionRun | EvidenceRetentionRuns | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantOrphanReferenceSettings | TenantOrphanReferenceSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| OrphanReferenceFinding | OrphanReferenceFindings | TrainArr.Api/Data/TrainArrDbContext.cs |
| OrphanReferenceRun | OrphanReferenceRuns | TrainArr.Api/Data/TrainArrDbContext.cs |
| RecertificationAssignmentRun | RecertificationAssignmentRuns | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingNotificationDispatch | TrainingNotificationDispatches | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantIntegrationSettings | TenantIntegrationSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantStaffarrPublicationSettings | TenantStaffarrPublicationSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| StaffarrPublicationDelivery | StaffarrPublicationDeliveries | TrainArr.Api/Data/TrainArrDbContext.cs |
| TenantEventProcessingSettings | TenantEventProcessingSettings | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainingDomainEvent | TrainingDomainEvents | TrainArr.Api/Data/TrainArrDbContext.cs |
| PersonTrainingHistoryEntry | PersonTrainingHistoryEntries | TrainArr.Api/Data/TrainArrDbContext.cs |
| TrainArrAuditEvent | AuditEvents | TrainArr.Api/Data/TrainArrDbContext.cs |
| AuditPackageGenerationJob | AuditPackageGenerationJobs | TrainArr.Api/Data/TrainArrDbContext.cs |

</details>

<details>
<summary>Frontend page files (19)</summary>

| Page |
| --- |
| src/lib/createWorkspacePage.tsx |
| src/pages/AssignmentWorkspacePage.tsx |
| src/pages/LaunchPage.tsx |
| src/workspace/TrainArrWorkspacePage.tsx |
| src/pages/assignments/AssignmentsPage.tsx |
| src/pages/catalog/CatalogPage.tsx |
| src/pages/certificates/CertificatesPage.tsx |
| src/pages/citations/CitationsPage.tsx |
| src/pages/dashboard/DashboardPage.tsx |
| src/pages/evaluator/EvaluatorPage.tsx |
| src/pages/instructor/InstructorPage.tsx |
| src/pages/matrix/MatrixPage.tsx |
| src/pages/my-training/MyTrainingPage.tsx |
| src/pages/programs/ProgramsPage.tsx |
| src/pages/qualifications/QualificationsPage.tsx |
| src/pages/remediation/RemediationPage.tsx |
| src/pages/reports/ReportsPage.tsx |
| src/pages/rule-packs/RulePacksPage.tsx |
| src/pages/settings/SettingsPage.tsx |

</details>

<details>
<summary>Endpoint source families (55)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| TrainArrReportEndpoints.cs | 19 |
| IntegrationEndpoints.cs | 10 |
| TrainingDefinitionStepEndpoints.cs | 10 |
| QualificationIssueEndpoints.cs | 9 |
| AuthEndpoints.cs | 8 |
| TrainingProgramEndpoints.cs | 8 |
| AuditPackageEndpoints.cs | 6 |
| TrainingEvaluationEndpoints.cs | 6 |
| TrainingRequirementEndpoints.cs | 6 |
| AssignmentEscalationSettingsEndpoints.cs | 5 |
| TrainingDefinitionCompletionRuleEndpoints.cs | 5 |
| AssignmentDueReminderSettingsEndpoints.cs | 4 |
| OrphanReferenceSettingsEndpoints.cs | 4 |
| QualificationRecalculationSettingsEndpoints.cs | 4 |
| RulePackImpactSettingsEndpoints.cs | 4 |
| TenantSettingsEndpoints.cs | 4 |
| TrainingApplicabilityProfileEndpoints.cs | 4 |
| TrainingAssignmentEndpoints.cs | 4 |
| TrainingAssignmentMaterialDemandEndpoints.cs | 4 |
| TrainingDefinitionStepBranchEndpoints.cs | 4 |
| TrainingEvidenceEndpoints.cs | 4 |
| TrainingMatrixEndpoints.cs | 4 |
| TrainingSignoffEndpoints.cs | 4 |
| EventProcessingSettingsEndpoints.cs | 3 |
| EvidenceRetentionSettingsEndpoints.cs | 3 |
| IntegrationSettingsEndpoints.cs | 3 |
| NotificationSettingsEndpoints.cs | 3 |
| QualificationCheckEndpoints.cs | 3 |
| RecertificationSettingsEndpoints.cs | 3 |
| StaffarrPublicationSettingsEndpoints.cs | 3 |
| TrainingAssignmentLaborEndpoints.cs | 3 |
| TrainingCitationEndpoints.cs | 3 |
| TrainingProgramVersionEndpoints.cs | 3 |
| TrainingRulePackRequirementEndpoints.cs | 3 |
| EventAndAuditEndpoints.cs | 2 |
| IncidentRemediationEndpoints.cs | 2 |
| InternalAssignmentDueReminderEndpoints.cs | 2 |
| InternalAssignmentEscalationEndpoints.cs | 2 |
| InternalAuditPackageGenerationEndpoints.cs | 2 |
| InternalEvidenceRetentionEndpoints.cs | 2 |
| InternalOrphanReferenceEndpoints.cs | 2 |
| InternalQualificationExpirationEndpoints.cs | 2 |
| InternalQualificationRecalculationEndpoints.cs | 2 |
| InternalRecertificationAssignmentEndpoints.cs | 2 |
| InternalRulePackImpactEndpoints.cs | 2 |
| InternalStaffarrPublicationRetryEndpoints.cs | 2 |
| InternalTrainingEventProcessingEndpoints.cs | 2 |
| InternalTrainingNotificationEndpoints.cs | 2 |
| PersonTrainingHistoryEndpoints.cs | 2 |
| RulePackImpactEndpoints.cs | 2 |
| TrainingDefinitionEndpoints.cs | 2 |
| CertificationPublicationEndpoints.cs | 1 |
| FieldInboxEndpoints.cs | 1 |
| PersonalTrainingDashboardEndpoints.cs | 1 |
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

Finish end-to-end delivery, assessment, remediation, credential, equivalency, and external-provider interoperability around the strong durable core.

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
