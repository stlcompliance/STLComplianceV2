# Compliance Core — GRC Full Feature Set

[Workflow catalog](./WORKFLOWS.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md) · [Rollout](./ROLLOUT.md)

## Document control

| Field | Value |
| --- | --- |
| Product | Compliance Core (GRC) |
| Category | Governance, Risk, and Compliance |
| Repository maturity | Durable |
| Audit basis | Static repository inspection on 2026-06-24 |
| Cataloged capabilities | 79 |
| Cataloged workflows | 17 |

## Product charter

Own the normalized regulatory knowledge, applicability, fact, requirement, evidence, evaluation, risk, finding, exception, and explainability models that convert legal and policy obligations into auditable operational decisions. Compliance Core is the suite rules and GRC engine; it does not take ownership of operational records from other products.

> **Implementation reality — Durable:** The repository contains an unusually broad persistent compliance domain, extensive APIs, and a dedicated workspace. Administrative authoring and review remain restricted, while runtime evaluations and workflow gates are intended to serve every tenant and product.

## Source-of-truth boundary

### Compliance Core owns

- Controlled vocabulary, aliases, compliance keys, material keys, governing bodies, jurisdictions, and regulatory programs.
- Regulatory sources, citations, rule packs, rule requirements, fact requirements, evidence requirements, mappings, assertions, and published versions.
- Tenant/product/subject compliance facts and source provenance, including confidence, effective dates, conflict state, and review status.
- Applicability questionnaires, theoretical situation evaluation, rule evaluation, explainability traces, findings, gates, risk signals, and recommended responses.
- Compliance-owned exceptions, exemptions, waivers, interpretations, compensating controls, approval state, and expiration/reassessment.
- Regulatory change monitoring, impact analysis, control-effectiveness/readiness analysis, compliance audit packages, and GRC administration evidence.
- SDS/HazCom reference and evaluation models where regulatory interpretation—not inventory custody—is the core concern.

### Compliance Core does not own

- People, sites, assets, work orders, training records, shipments, inventory, suppliers, customers, orders, finance transactions, quality records, or documents owned by other products.
- Tenant operational actions; it advises, gates, opens findings/tasks, and records responses while the owning product commits domain changes.
- Legal advice or an assurance that a tenant is compliant merely because configured rules pass.
- Fixed-suite access. All tenant products may call Compliance Core runtime services under permission and service policy.
- Governing bodies as LedgArr legal entities; regulatory authorities and business legal entities remain distinct concepts.

## Suite access and authorization

- All normal tenant users may launch every product application in the suite; product actions and records remain permission-scoped server-side.
- Fixed-suite access is nonvariable. Deployment health, feature readiness, legal restrictions, external-provider setup, and permissions may still constrain a capability.
- Platform administration remains a NexArr-only responsibility and is not a tenant product surface.
- Compliance Core administrative/authoring UI is restricted, while Compliance Core runtime evaluations, fact requirements, evidence needs, and workflow gates serve every tenant and product.

## Primary users

- Platform compliance administrator
- Regulatory content author
- Compliance/risk owner
- Control owner
- Internal auditor
- Operational process owner
- Evidence owner
- Executive reviewer
- External auditor/regulator/customer reviewer
- Integration/product service

## Required integrations

- All STL products
- NexArr
- StaffArr
- RecordArr
- ReportArr
- Authoritative regulatory/content sources
- External assurance/audit portals
- Notification and identity providers

## Product principles

- Compliance Core administrative UI is restricted to appropriately authorized platform/compliance roles, but its operational runtime serves every tenant and product.
- Every decision is versioned, cited, effective-time aware, explainable, and explicit about unknown, conflict, confidence, and missing evidence.
- Compliance Core never replaces a source product record and never commits another product’s operational transaction.
- Configured or passing rules are not marketed as legal advice or a universal certification of compliance.
- Rule/content publication requires tests, semantic impact review, approval separation, and rollback capability.

## Repository implementation snapshot

| Static indicator | Count |
| --- | --- |
| Persistent DbSet declarations | 81 |
| Discovered server classes | 541 |
| Discovered HTTP route declarations | 334 |
| Frontend source files | 139 |
| Frontend page files | 27 |
| Documentation headings | 131 |

### Evidence used for the current-state classification

- ComplianceCore.Api/Data/ComplianceCoreDbContext.cs declares 81 persistent sets spanning vocabulary, rulepacks, facts, mappings, evidence, evaluations, gates, findings, changes, risks, scenarios, SDS, and audit delivery.
- ComplianceCore.Api/Endpoints contains more than 300 statically discovered HTTP routes for authoring, evaluation, evidence, findings, gates, impact, scenarios, imports, exports, and audit packages.
- ComplianceCore.Web includes dedicated dashboard, rulepack, citation, evaluation, evidence, mapping, findings, gate, change-impact, source-management, reporting, and administrative workspace surfaces.
- The repository includes granular Compliance Core constitution/end-goal documents and import artifacts for vocabulary, rulepacks, evidence, exceptions, materials, mappings, fact requirements, and SDS references.

> Counts are static-discovery indicators, not proof that every route or screen is complete, reachable, secure, migrated, or production-ready.

## Feature catalog

### A. Currently implemented or meaningfully represented

These capabilities have repository evidence. Their state follows the product-level maturity and may still require hardening or completion.

| Feature ID | Capability | Class | State | Required behavior / evidence |
| --- | --- | --- | --- | --- |
| CC-CUR-001 | Controlled vocabulary and canonical key registry | CURRENT | Durable | Vocabulary types/terms, aliases, compliance keys, and material keys provide normalized machine-readable concepts across regulations and products. |
| CC-CUR-002 | Regulatory authority and jurisdiction model | CURRENT | Durable | Governing bodies, jurisdictions, programs, source references, and citations support traceable legal context. |
| CC-CUR-003 | Rulepack authoring and version lifecycle | CURRENT | Durable | Rule packs, requirements, expressions, dependencies, mappings, tests, publication, status, and historical versions are persistently represented. |
| CC-CUR-004 | Fact definitions and source provenance | CURRENT | Durable | Facts carry source, product/subject context, confidence, effective time, review/conflict state, and evaluation relevance. |
| CC-CUR-005 | Requirement and evidence modeling | CURRENT | Durable | Rule requirements, fact requirements, evidence requirements, references, and mapping workflows connect obligations to proof. |
| CC-CUR-006 | Source ingestion and staged import | CURRENT | Durable | Import batches, staged records, validation, mapping, review, diff, and publication concepts support controlled regulatory content ingestion. |
| CC-CUR-007 | Applicability and rule evaluation | CURRENT | Durable | Evaluation endpoints and models resolve facts against rules and retain explainability-oriented outputs. |
| CC-CUR-008 | Product fact mirrors and synchronization | CURRENT | Durable | Product facts and source state can be mirrored through APIs/events without cross-database foreign keys. |
| CC-CUR-009 | Workflow gates and product responses | CURRENT | Durable | Products can request gate decisions, receive missing-fact/evidence reasons, and report operational responses. |
| CC-CUR-010 | Findings, remediation, and closure evidence | CURRENT | Durable | Finding records, status, severity, owner/context, response, and verification concepts support corrective compliance work. |
| CC-CUR-011 | Exceptions, exemptions, waivers, and interpretations | CURRENT | Durable | Time-bounded departures and legal exceptions can be scoped, approved, supported by evidence, and reevaluated. |
| CC-CUR-012 | Regulatory change and impact analysis | CURRENT | Durable | Change-source, impact, affected rule/control/product, review, notification, and response models are present. |
| CC-CUR-013 | Risk and readiness analytics | CURRENT | Durable | Risk signals, control effectiveness, missing evidence, exposure, and forecast/readiness concepts are represented. |
| CC-CUR-014 | Questionnaires and theoretical situation evaluation | CURRENT | Durable | Plain-language fact collection and hypothetical scenario evaluation are modeled without altering production facts. |
| CC-CUR-015 | SDS and hazard communication support | CURRENT | Durable | SDS references, material/hazard keys, mappings, and regulatory context support HazCom-oriented evaluations. |
| CC-CUR-016 | Audit delivery and export packages | CURRENT | Durable | Manifest/export/job/orchestration endpoints support repeatable audit and evidence delivery. |
| CC-CUR-017 | Comprehensive administrative workspace | CURRENT | Durable | The web application exposes authoring, evaluation, evidence mapping, findings, change, reporting, and administration surfaces. |

### B. Common category baseline

These are expected for a credible Governance, Risk, and Compliance product. Where they overlap current behavior, the target definition is the acceptance standard rather than a duplicate feature.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| CC-COM-001 | Regulatory and framework library | COMMON | Target | Maintain obligations from laws, regulations, standards, contracts, internal policy, and customer requirements with source/version/jurisdiction/effective dates. |
| CC-COM-002 | Control library and cross-framework mapping | COMMON | Target | Define controls once and map them to multiple requirements, risks, policies, evidence sources, owners, tests, and products. |
| CC-COM-003 | Enterprise and operational risk register | COMMON | Target | Record risks, causes, impacts, likelihood, inherent/residual ratings, controls, treatment, acceptance, review, and escalation. |
| CC-COM-004 | Compliance applicability assessment | COMMON | Target | Determine which laws, rulepacks, controls, reports, and evidence apply by tenant, legal entity, operation, site, person, asset, material, product, customer, or event. |
| CC-COM-005 | Policy and attestation governance | COMMON | Target | Link policy obligations to RecordArr documents, owners, approvals, distribution, attestations, exceptions, and training. |
| CC-COM-006 | Assessment and control testing | COMMON | Target | Plan tests, select samples, execute procedures, capture evidence, record exceptions, rate design/operating effectiveness, and issue findings. |
| CC-COM-007 | Issue and remediation management | COMMON | Target | Create findings, assign plans/tasks, set due dates, accept risk, collect evidence, verify effectiveness, reopen, escalate, and close. |
| CC-COM-008 | Audit management | COMMON | Target | Plan scope, objectives, criteria, schedule, requests, workpapers, interviews, sampling, findings, review, reports, responses, and follow-up. |
| CC-COM-009 | Third-party risk governance | COMMON | Target | Assess suppliers/partners, collect evidence, evaluate services/data/locations/subcontractors, track remediation, monitor changes, and trigger SupplyArr controls. |
| CC-COM-010 | Compliance obligations calendar | COMMON | Target | Track filings, renewals, inspections, attestations, reports, permits, evidence refresh, control tests, and accountable owners. |
| CC-COM-011 | Evidence request and collection | COMMON | Target | Request reusable evidence from owning products, validate freshness/scope/integrity, prevent duplicate uploads, and preserve lineage. |
| CC-COM-012 | Regulatory change management | COMMON | Target | Monitor authoritative sources, triage changes, compare versions, assess impact, approve interpretation, update controls/rules, communicate, and verify adoption. |
| CC-COM-013 | Exception and risk acceptance | COMMON | Target | Use scoped, justified, approved, compensating, time-limited, reviewable exceptions with expiration and automatic reevaluation. |
| CC-COM-014 | Dashboards and regulatory reporting | COMMON | Target | Show obligations, coverage, findings, overdue work, risk, evidence health, control effectiveness, trend, and auditable point-in-time reports. |
| CC-COM-015 | Role segregation and privileged administration | COMMON | Target | Restrict authoring, approval, publication, override, and audit administration with separation of duties and complete logs. |
| CC-COM-016 | Business continuity and resilience governance | COMMON | Target | Map critical processes/dependencies, recovery objectives, tests, failures, corrective actions, and evidence where included in tenant scope. |

### C. Commonly requested but widely underserved

These address recurring user friction, fragmented add-ons, poor transparency, inaccessible setup, or weak SMB usability.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| CC-UND-001 | Plain-language applicability for small organizations | UNDERSERVED | Target | Guide non-specialists through operational questions, explain assumptions, and recommend likely obligations without requiring a consultant to model every fact. |
| CC-UND-002 | Requirement-to-workflow traceability | UNDERSERVED | Target | Show exactly which rule, fact, control, evidence, and source-product action caused a gate or task; never expose only an opaque risk score. |
| CC-UND-003 | Evidence reuse without spreadsheet chasing | UNDERSERVED | Target | Reuse a valid source record across multiple requirements and frameworks while preserving scope, freshness, lineage, and revocation. |
| CC-UND-004 | Operational compliance in the flow of work | UNDERSERVED | Target | Ask the minimum relevant questions and enforce explainable gates in asset, person, training, maintenance, warehouse, transport, order, quality, document, and finance workflows. |
| CC-UND-005 | Affordable local and industry rulepacks | UNDERSERVED | Target | Support federal, state, local, customer, insurer, and industry obligations without a large-enterprise content subscription or custom project. |
| CC-UND-006 | Conflict-aware fact management | UNDERSERVED | Target | Surface contradictory, stale, inferred, or low-confidence facts, show which decisions they affect, and route a human review instead of silently choosing one. |
| CC-UND-007 | Auditor-ready evidence room without duplicate systems | UNDERSERVED | Target | Offer scoped, expiring, read-only auditor access to approved packages and source evidence without buying a separate audit portal. |
| CC-UND-008 | Regulation change impact in operational terms | UNDERSERVED | Target | Translate changed language into affected sites, people, assets, materials, workflows, documents, training, controls, and due dates. |
| CC-UND-009 | Transparent risk acceptance for SMBs | UNDERSERVED | Target | Provide disciplined approvals, compensating controls, expirations, and reminders without enterprise committee tooling or hidden configuration. |
| CC-UND-010 | Compliance setup from existing data | UNDERSERVED | Target | Infer a draft profile from StaffArr, MaintainArr, SupplyArr, LoadArr, RoutArr, LedgArr, RecordArr, and tenant answers, then clearly identify unknowns. |
| CC-UND-011 | No compliance-by-checkbox theater | UNDERSERVED | Target | Require attributable evidence and source facts, distinguish configured/pass/verified states, and make uncertainty visible. |
| CC-UND-012 | Customer and supplier assurance exchange | UNDERSERVED | Target | Share a governed subset of controls, certificates, findings, and evidence through portals without exposing internal GRC data or email attachments. |
| CC-UND-013 | Scenario testing before operational change | UNDERSERVED | Target | Evaluate a planned facility, material, asset, route, service, or workforce change before creating live records. |

### D. Advanced capabilities to democratize

These are commonly found only in enterprise tiers, specialist products, or expensive implementations. STL should make them practical without weakening governance.

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| CC-DEM-001 | Continuous control monitoring | DEMOCRATIZE | Target | Evaluate product events and evidence freshness against controls continuously, with suppression, confidence, owner routing, and explainable alerts. |
| CC-DEM-002 | Cross-framework harmonization | DEMOCRATIZE | Target | Map common controls and evidence across OSHA, FMCSA, EPA, MSHA, ISO, SOC, NIST, customer, insurer, and internal frameworks with version impact. |
| CC-DEM-003 | Regulatory intelligence with cited AI assistance | DEMOCRATIZE | Target | Propose source classification, requirement extraction, mappings, summaries, and impacts while preserving citations, reviewer approval, and source versions. |
| CC-DEM-004 | Policy-as-code and workflow gates | DEMOCRATIZE | Target | Expose versioned, testable rules and decision APIs that products can call before consequential actions, with simulation and rollback. |
| CC-DEM-005 | Quantitative and scenario-based risk analysis | DEMOCRATIZE | Target | Model ranges, dependencies, scenarios, uncertainty, loss/exposure assumptions, and mitigations without replacing accountable judgment. |
| CC-DEM-006 | Control design and operating-effectiveness analytics | DEMOCRATIZE | Target | Combine test results, incidents, evidence, exceptions, workflow deviations, and trend to distinguish control presence from real effectiveness. |
| CC-DEM-007 | Automated evidence connectors | DEMOCRATIZE | Target | Collect signed or attributable evidence from product APIs/events and approved external systems with freshness, scope, reconciliation, and revocation. |
| CC-DEM-008 | Regulatory knowledge graph and impact graph | DEMOCRATIZE | Target | Navigate source → citation → requirement → fact → control → evidence → product record → finding → action with dependency and change propagation. |
| CC-DEM-009 | Readiness forecasting | DEMOCRATIZE | Target | Estimate likely future noncompliance based on expiring evidence, overdue actions, staffing/training, maintenance, inventory, document, supplier, and regulatory-change signals. |
| CC-DEM-010 | Advanced audit sampling and analytics | DEMOCRATIZE | Target | Risk-based sampling, reproducible populations, exceptions, continuous tests, anomaly flags, workpapers, review, and cited audit narratives. |
| CC-DEM-011 | Federated assurance exchange | DEMOCRATIZE | Target | Publish signed assurance packages or machine-readable claims to customers, suppliers, auditors, and regulators with selective disclosure and expiry. |
| CC-DEM-012 | Compliance digital twin | DEMOCRATIZE | Target | Evaluate theoretical operations and planned changes against versioned rules, facts, controls, constraints, and evidence before execution. |
| CC-DEM-013 | Control-owner workbench and certification | DEMOCRATIZE | Target | Provide enterprise-grade quarterly/periodic control certification, evidence roll-forward, challenge, delegation, and executive sign-off without enterprise licensing. |

### E. Suite-wide foundation required in Compliance Core

| Feature ID | Capability | Class | State | Required behavior |
| --- | --- | --- | --- | --- |
| CC-FND-001 | Tenant-scoped authorization | FOUNDATION | Target | Every read and write is tenant-scoped server-side; product UI visibility never substitutes for authorization. |
| CC-FND-002 | StaffArr-backed action permissions | FOUNDATION | Target | Use product action keys, role scopes, delegated authority, and explicit denial reasons; ownership does not prevent permissioned delegated workflows. |
| CC-FND-003 | Unified suite shell | FOUNDATION | Target | Consistent navigation, page anatomy, terminology, responsive behavior, keyboard access, and light/dark contrast across products. |
| CC-FND-004 | Record lifecycle and history | FOUNDATION | Target | Human-readable statuses, timeline events, actor/time/source attribution, undo or correction paths, and immutable audit evidence. |
| CC-FND-005 | Quick create for missing references | FOUNDATION · UNDERSERVED | Target | Create the minimum valid cross-product reference without abandoning the current task; preserve context and allow later backfill. |
| CC-FND-006 | Saved views and personal preferences | COMMON | Target | Per-user filters, columns, density, sort, board/list preference, default landing view, and product-scoped preferences. |
| CC-FND-007 | Bulk operations with preview | COMMON | Target | Selection-aware actions, dry-run/impact preview, partial failure handling, downloadable results, and permission checks per record. |
| CC-FND-008 | Import with mapping and review | COMMON · UNDERSERVED | Target | Product-specific import separate from Smart Import, with templates, mappings, validation, dedupe, dry run, commit plan, and rollback/correction evidence. |
| CC-FND-009 | Export and portability | COMMON · DEMOCRATIZE | Target | CSV/PDF/JSON or package export appropriate to the domain, without trapping customer data behind enterprise tiers. |
| CC-FND-010 | Notifications, tasks, and inbox | COMMON | Target | Actionable notifications with ownership, due dates, escalation, snooze/delegation, deep links, and duplicate suppression. |
| CC-FND-011 | API, webhooks, and event outbox | COMMON · DEMOCRATIZE | Target | Stable versioned APIs, idempotency, service authentication, signed webhooks/events, retries, dead-letter handling, and correlation IDs. |
| CC-FND-012 | RecordArr evidence references | FOUNDATION | Target | Files and evidence are stored once in RecordArr and referenced by owning records; products do not create shadow document vaults. |
| CC-FND-013 | ReportArr projections and metrics | FOUNDATION | Target | Operational products emit facts/events; ReportArr builds read models and reports without becoming the source of operational truth. |
| CC-FND-014 | Compliance Core applicability and gates | FOUNDATION | Target | Products request explainable requirements, missing facts, evidence needs, and gate decisions rather than embedding independent legal logic. |
| CC-FND-015 | Field Companion execution surface | FOUNDATION · UNDERSERVED | Target | Mobile-first task, scan, capture, acknowledgement, and offline execution against the owning product API. |
| CC-FND-016 | Professional print and report view | COMMON | Target | Print output is a purpose-built report document without the app shell, not a screenshot of the current page. |
| CC-FND-017 | Accessible error and degraded states | FOUNDATION | Target | Explain what failed, what remains saved, whether retry is safe, what the user can do, and which administrator or integration owns the issue. |
| CC-FND-018 | Time, locale, and units | FOUNDATION | Target | Store canonical timestamps and units; display tenant/user locale, timezone, currency, and measurement preferences consistently. |
| CC-FND-019 | Retention, privacy, and legal hold awareness | FOUNDATION | Target | Product deletion/archive actions respect RecordArr retention, legal holds, privacy policy, and downstream references. |
| CC-FND-020 | AI-assisted proposals, never silent commits | UNDERSERVED | Target | AI may classify, summarize, suggest, or draft; consequential changes remain reviewable, permissioned, attributable, and reversible. |

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
<summary>Persistent entity sets (81)</summary>

| Entity type | DbSet/property | Source |
| --- | --- | --- |
| VocabularyType | VocabularyTypes | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| VocabularyTerm | VocabularyTerms | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| VocabularyAlias | VocabularyAliases | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ComplianceKey | ComplianceKeys | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| MaterialKey | MaterialKeys | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ComplianceCoreAuditEvent | AuditEvents | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| GoverningBody | GoverningBodies | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| Jurisdiction | Jurisdictions | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RegulatoryProgram | RegulatoryPrograms | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RulePack | RulePacks | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RegulatoryCitation | RegulatoryCitations | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| FactDefinition | FactDefinitions | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| FactRequirement | FactRequirements | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| EvidenceReference | EvidenceReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| FactAssertion | FactAssertions | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| AuditTrace | AuditTraces | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RegulatoryMapping | RegulatoryMappings | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RuleEvaluationRun | RuleEvaluationRuns | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RuleTestCase | RuleTestCases | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ScheduledRuleEvaluationRun | ScheduledRuleEvaluationRuns | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| FactSource | FactSources | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ProductFactMirror | ProductFactMirrors | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ComplianceFinding | ComplianceFindings | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| WorkflowGateDefinition | WorkflowGateDefinitions | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| WorkflowGateCheckResult | WorkflowGateCheckResults | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ProductGateResponse | ProductGateResponses | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| SdsReference | SdsReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| HazComReference | HazComReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| AuditPackageGenerationJob | AuditPackageGenerationJobs | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| SourceIngestionBatch | SourceIngestionBatches | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| SourceIngestionJob | SourceIngestionJobs | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RuleChangeEvent | RuleChangeEvents | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RulePackMonitorSnapshot | RulePackMonitorSnapshots | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RuleChangeScanRun | RuleChangeScanRuns | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RiskScoreRun | RiskScoreRuns | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| RiskScore | RiskScores | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| MissingEvidenceWarningRun | MissingEvidenceWarningRuns | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| MissingEvidenceWarning | MissingEvidenceWarnings | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ControlEffectivenessRun | ControlEffectivenessRuns | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ControlEffectivenessRecord | ControlEffectivenessRecords | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ReadinessForecastRun | ReadinessForecastRuns | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ReadinessForecast | ReadinessForecasts | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| TenantM12AnalyticsWorkerSettings | TenantM12AnalyticsWorkerSettings | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| M12AnalyticsBatchRun | M12AnalyticsBatchRuns | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| TenantFactSourceSyncWorkerSettings | TenantFactSourceSyncWorkerSettings | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| FactSourceSyncStatus | FactSourceSyncStatuses | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ComplianceWaiver | ComplianceWaivers | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ComplianceExceptionExemption | ComplianceExceptionExemptions | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportSession | ImportSessions | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportSessionSourceFile | ImportSessionSourceFiles | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedRulePack | ImportStagedRulePacks | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedRuleRequirement | ImportStagedRuleRequirements | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedFactRequirement | ImportStagedFactRequirements | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedRegulatoryMapping | ImportStagedRegulatoryMappings | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedControlledVocabulary | ImportStagedControlledVocabulary | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedVocabularyAlias | ImportStagedVocabularyAliases | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedComplianceKey | ImportStagedComplianceKeys | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedMaterialKey | ImportStagedMaterialKeys | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedSdsReference | ImportStagedSdsReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedEvidenceReference | ImportStagedEvidenceReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedExceptionExemption | ImportStagedExceptionExemptions | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedMappingCandidate | ImportStagedMappingCandidates | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ImportStagedMappingDecision | ImportStagedMappingDecisions | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ComplianceEvidenceOptionGroup | ComplianceEvidenceOptionGroups | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ComplianceEvidenceOption | ComplianceEvidenceOptions | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| ExternalObjectReference | ExternalObjectReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| DocumentReference | DocumentReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| MaterialReference | MaterialReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| PartReference | PartReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| SystemReference | SystemReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| AssetReference | AssetReferences | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| QuestionnaireRun | QuestionnaireRuns | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| QuestionnaireAnswer | QuestionnaireAnswers | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| TheoreticalSituation | TheoreticalSituations | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| TheoreticalSituationContext | TheoreticalSituationContexts | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| TheoreticalApplicabilityResult | TheoreticalApplicabilityResults | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| TheoreticalSituationFact | TheoreticalSituationFacts | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| TheoreticalSituationIncident | TheoreticalSituationIncidents | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| TheoreticalSituationEvaluation | TheoreticalSituationEvaluations | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| TheoreticalSituationEvaluationDetail | TheoreticalSituationEvaluationDetails | ComplianceCore.Api/Data/ComplianceCoreDbContext.cs |
| TEntity | set | ComplianceCore.Api/Services/StagedImportService.cs |

</details>

<details>
<summary>Frontend page files (27)</summary>

| Page |
| --- |
| src/lib/createWorkspacePage.tsx |
| src/pages/LaunchPage.tsx |
| src/workspace/ComplianceCoreWorkspacePage.tsx |
| src/pages/admin/AdminPage.tsx |
| src/pages/change-impact/ChangeImpactPage.tsx |
| src/pages/citations/CitationsPage.tsx |
| src/pages/dashboard/DashboardPage.tsx |
| src/pages/evaluation/EvaluationPage.tsx |
| src/pages/evidence-mapping/EvidenceMappingPage.tsx |
| src/pages/evidence-requirements/EvidenceRequirementsPage.tsx |
| src/pages/evidence-types/EvidenceTypesPage.tsx |
| src/pages/exception-exemptions/ExceptionExemptionsPage.tsx |
| src/pages/fact-sources/FactSourcesPage.tsx |
| src/pages/findings/FindingsPage.tsx |
| src/pages/governing-bodies/GoverningBodiesPage.tsx |
| src/pages/imports/ImportsPage.tsx |
| src/pages/jurisdictions/JurisdictionsPage.tsx |
| src/pages/mappings/MappingsPage.tsx |
| src/pages/operator/OperatorPage.tsx |
| src/pages/questionnaires/QuestionnairesPage.tsx |
| src/pages/registry/RegistryPage.tsx |
| src/pages/reports/ReportsPage.tsx |
| src/pages/requirements/RequirementDetailPage.tsx |
| src/pages/retention-rules/RetentionRulesPage.tsx |
| src/pages/rulepack-diff/RulePackDiffPage.tsx |
| src/pages/rulepacks/RulePackDetailPage.tsx |
| src/pages/theoretical-situation/TheoreticalSituationPage.tsx |

</details>

<details>
<summary>Endpoint source families (49)</summary>

| Endpoint source file | Discovered route declarations |
| --- | --- |
| ComplianceCoreReportEndpoints.cs | 39 |
| StagedImportEndpoints.cs | 37 |
| TheoreticalSituationEndpoints.cs | 29 |
| RuleEvaluationEndpoints.cs | 19 |
| CitationFactEndpoints.cs | 15 |
| ComplianceWaiverEndpoints.cs | 14 |
| AuditPackageEndpoints.cs | 13 |
| VocabularyEndpoints.cs | 12 |
| CsvImportExportEndpoints.cs | 11 |
| RulePackEndpoints.cs | 10 |
| AuthEndpoints.cs | 8 |
| InternalFactEndpoints.cs | 8 |
| RuleCatalogEndpoints.cs | 8 |
| AuditRequirementEndpoints.cs | 7 |
| ExceptionExemptionEndpoints.cs | 7 |
| RegulatoryRegistryEndpoints.cs | 6 |
| RuleTestCaseEndpoints.cs | 6 |
| SourceIngestionEndpoints.cs | 6 |
| WorkflowGateEndpoints.cs | 5 |
| HazComEndpoints.cs | 4 |
| ProductGateResponseEndpoints.cs | 4 |
| RegulatoryMappingEndpoints.cs | 4 |
| SdsEndpoints.cs | 4 |
| AuditDeliveryOrchestrationEndpoints.cs | 3 |
| ControlEffectivenessEndpoints.cs | 3 |
| FactSourceEndpoints.cs | 3 |
| FindingEndpoints.cs | 3 |
| MissingEvidenceWarningEndpoints.cs | 3 |
| ProductGateEndpoints.cs | 3 |
| QuestionnaireEndpoints.cs | 3 |
| ReadinessForecastEndpoints.cs | 3 |
| RiskScoringEndpoints.cs | 3 |
| RuleChangeMonitoringEndpoints.cs | 3 |
| RuleVersionEndpoints.cs | 3 |
| CalculatorEndpoints.cs | 2 |
| ComplianceKeyEndpoints.cs | 2 |
| FactSourceSyncWorkerSettingsEndpoints.cs | 2 |
| InternalAuditPackageGenerationEndpoints.cs | 2 |
| InternalFactSourceSyncEndpoints.cs | 2 |
| InternalM12AnalyticsBatchEndpoints.cs | 2 |
| InternalRuleChangeMonitoringEndpoints.cs | 2 |
| InternalScheduledEvaluationEndpoints.cs | 2 |
| M12AnalyticsWorkerSettingsEndpoints.cs | 2 |
| MaterialKeyEndpoints.cs | 2 |
| FactSourceSyncHealthEndpoints.cs | 1 |
| InternalComplianceWaiverEndpoints.cs | 1 |
| OperatorDashboardEndpoints.cs | 1 |
| ProductFactIntegrationEndpoints.cs | 1 |
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

Stabilize content governance, explainability, effective-time behavior, rule testing/publication, operational gates, evidence reuse, and universal runtime availability.

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
