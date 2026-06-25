# ReportArr — BI Workflow Catalog

[Feature catalog](./FEATURESET.md) · [Suite status legend](../../00_STATUS_LEGEND.md) · [Cross-product workflows](../../02_CROSS_PRODUCT_WORKFLOWS.md)

## Workflow contract

This document defines the end-to-end business state machines for ReportArr. A workflow is not complete because a page or endpoint exists: the trigger, authority, state transitions, validation, handoffs, evidence, exceptions, retries, conflict behavior, notifications, reporting, and final source-of-truth commit must all work together.

## Ownership rule

ReportArr is the suite reporting and BI plane. It consumes product events/APIs and approved external sources to build governed read models, datasets, metrics, dashboards, operational reports, schedules, alerts, and audit/management packages. It never becomes the source of operational truth and does not write directly into product databases. Every displayed number must have a definition, source, refresh time, lineage, and access policy.

- Operational records, approvals, actions, or corrections; users drill into and act through the owning product.
- Compliance evaluation; Compliance Core supplies findings/requirements while ReportArr visualizes them.
- File/document storage; RecordArr owns report exports/packages when retained as records.
- Authentication/role assignment; NexArr/StaffArr supply identity and permissions while ReportArr enforces dataset/report access.
- Unreviewed AI-generated metric definitions or hidden calculations.

## Workflow index

| Workflow ID | Workflow | Class | State | Trigger |
| --- | --- | --- | --- | --- |
| RP-WF-001 | Register source connector and establish ingestion | CURRENT · COMMON | Scaffold | BI/integration administrator creates a source connector. |
| RP-WF-002 | Ingest events, replay, quarantine, and reconcile | COMMON · FOUNDATION | Target | Source event arrives or backfill/replay job runs. |
| RP-WF-003 | Define and certify dataset/read model | CURRENT · COMMON | Scaffold | Analyst/model owner creates or revises a dataset/read model. |
| RP-WF-004 | Define governed metric or KPI | CURRENT · COMMON | Scaffold | Business owner or analyst proposes a metric/KPI. |
| RP-WF-005 | Build and publish dashboard | CURRENT · COMMON | Scaffold | Authorized creator starts a dashboard from certified datasets or approved exploratory models. |
| RP-WF-006 | Build paginated/operational report | CURRENT · COMMON | Scaffold | Report author creates a report definition. |
| RP-WF-007 | Run, export, and securely deliver report | CURRENT · COMMON | Scaffold | User runs report or a schedule/event triggers it. |
| RP-WF-008 | Schedule and subscription management | CURRENT · COMMON | Scaffold | Authorized user creates/edits a schedule or subscription. |
| RP-WF-009 | Alert detection, acknowledgement, and response | CURRENT · UNDERSERVED | Scaffold | Metric threshold, anomaly, SLA, data quality, or event rule evaluates true. |
| RP-WF-010 | Drill through to source and take action | UNDERSERVED | Target | Viewer selects a data point, row, exception, or KPI contributor. |
| RP-WF-011 | Refresh, rebuild, schema-change impact, and rollback | CURRENT · FOUNDATION | Scaffold | Scheduled refresh runs, source schema changes, model version publishes, or data defect is detected. |
| RP-WF-012 | Cross-product audit/management package | CURRENT · DEMOCRATIZE | Scaffold | Authorized owner defines or runs a package for a period/scope. |
| RP-WF-013 | Natural-language question to governed answer | UNDERSERVED · DEMOCRATIZE | Target | Authorized user asks a question in ReportArr or another product. |
| RP-WF-014 | Data-quality incident and correction | COMMON · UNDERSERVED | Target | Quality test, user report, reconciliation, or source owner identifies incorrect/stale/missing BI data. |
| RP-WF-015 | BI access review and external embedding | COMMON · DEMOCRATIZE | Target | Dashboard/report/dataset is shared/embedded or scheduled access review begins. |

## Universal workflow requirements

- **Authority:** resolve user/service identity, tenant, action permission, organizational/record scope, delegation, and separation of duties on the server.
- **State:** use explicit human-readable states and legal transitions; never infer final completion solely from a screen closing or an external request being sent.
- **Idempotency:** retries, double-clicks, event replay, import retry, webhooks, and offline sync cannot create duplicate effects.
- **Concurrency:** stale edits receive a conflict with current context and permitted resolution; never silently last-write-wins consequential data.
- **Evidence:** retain actor, source, version, time, reason, input/output, approvals, external calls, attachments by RecordArr reference, and correlation/causation.
- **Handoffs:** the receiving product accepts/rejects explicitly and emits an outcome; the sender does not mark downstream work complete merely because it dispatched a request.
- **Degradation:** state what is saved, what failed, whether retry is safe, and the manual or alternate path. Safety/compliance/financial hard gates never silently fail open.
- **Notifications:** notify only actionable audiences, deduplicate, respect preference/urgency/quiet-hour policy, escalate, and deep-link through a fresh permission check.
- **Mobile/offline:** only server-declared offline-safe actions queue; final authorization, concurrency, references, and hard gates are revalidated by the owning product.
- **Reporting:** emit events/facts to ReportArr with source/effective time and data-quality state; ReportArr never substitutes for the operational record.

## RP-WF-001 — Register source connector and establish ingestion

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Connect an approved source with clear ownership, security, schema, and replay behavior. |
| Trigger | BI/integration administrator creates a source connector. |

### Actors

- BI administrator
- Source product owner
- NexArr
- ReportArr

### State path

`draft → testing → dry_run → active → degraded → disabled`

### Required sequence

1. Choose product event/API/database-approved export or external provider connector and define business purpose.
2. Configure NexArr-managed credentials/service identity and least-privilege scope.
3. Discover/declare schema, keys, tenant mapping, event time, deletion/correction semantics, and rate limits.
4. Test connectivity and sample data without persisting secrets in ReportArr UI/logs.
5. Define cursor/checkpoint, backfill window, quarantine, retry, and reconciliation policy.
6. Run dry ingestion and validate counts, tenant isolation, duplicates, and data types.
7. Approve/activate connector and monitor health/freshness.
8. Version schema/mappings and handle safe deprecation.

### Exception and recovery paths

- Authentication failure, schema drift, cross-tenant data, no stable key, source rate limit, duplicate events, deleted records, or unsupported sensitive field.
- Direct cross-product database access is prohibited.

### Cross-product and external handoffs

- NexArr → ReportArr: credential/connection.
- Source → ReportArr: API/events.
- ReportArr → source owner: reconciliation/health.

### Evidence and audit record

- Purpose/owner/scope.
- Schema/mapping/version.
- Test/dry-run results.
- Activation/health changes.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Time to connect.
- Ingestion success.
- Freshness.
- Schema incidents.
- Tenant isolation tests.

## RP-WF-002 — Ingest events, replay, quarantine, and reconcile

| Field | Definition |
| --- | --- |
| Classification | COMMON · FOUNDATION |
| Implementation state | Target |
| Purpose | Build reliable analytical facts from product events without losing or duplicating data. |
| Trigger | Source event arrives or backfill/replay job runs. |

### Actors

- ReportArr ingestion service
- Source product
- BI operator

### State path

`received → validated → processed → quarantined → retry → reconciled → replayed`

### Required sequence

1. Validate event envelope, tenant, signature/service identity, schema version, event/aggregate IDs, sequence, and time.
2. Deduplicate idempotently and store raw normalized event/ref according to policy.
3. Transform into staging facts and update cursor/checkpoint.
4. Quarantine invalid/poison events with safe error detail.
5. Apply ordered/upsert/correction/delete semantics to read model.
6. Acknowledge processing and retry transient failures.
7. Reconcile counts/versions against source APIs or control totals.
8. Replay/rebuild from a known checkpoint with audit.

### Exception and recovery paths

- Out-of-order/duplicate event, unknown schema, missing tenant, source correction, clock skew, poison payload, partial rebuild, or source retention gap.
- Rebuild must not expose incomplete model as current.

### Cross-product and external handoffs

- Source products ↔ ReportArr.
- NexArr: service identity.
- ReportArr → alert/task for operator.

### Evidence and audit record

- Envelope/raw ref.
- Validation/transform version.
- Cursor/model changes.
- Quarantine/replay/reconciliation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Event latency.
- Duplicate suppression.
- Quarantine aging.
- Reconciliation variance.
- Replay success.

## RP-WF-003 — Define and certify dataset/read model

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Create a governed analytical model with known grain, lineage, security, and quality. |
| Trigger | Analyst/model owner creates or revises a dataset/read model. |

### Actors

- Data modeler
- Business owner
- Security/privacy reviewer
- ReportArr

### State path

`draft → building → validation → review → certified → deprecated → retired`

### Required sequence

1. Define business purpose, grain, facts, dimensions, time fields, keys, joins, history, and source lineage.
2. Add fields/measures with descriptions, types, formats, units/currency, calculations, null behavior, and sensitivity.
3. Configure tenant, row, column, export, and external-sharing policies.
4. Add freshness, completeness, reconciliation, and anomaly tests.
5. Build sample model and compare control totals/source records.
6. Review with business/security owners and resolve ambiguities.
7. Certify/publish version with owner/SLA and deprecation policy.
8. Monitor quality, usage, dependencies, and changes.

### Exception and recovery paths

- Ambiguous grain, many-to-many duplication, missing history, sensitive field leakage, metric disagreement, failed reconciliation, or source unavailable.
- Exploratory dataset remains uncertified and clearly labeled.

### Cross-product and external handoffs

- ReportArr ↔ source owner/StaffArr/NexArr for security context.
- RecordArr: definition/approval package if retained.

### Evidence and audit record

- Definition/version.
- Lineage/security.
- Tests/reconciliation.
- Certification/owners.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Certification time.
- Test pass.
- Metric disputes.
- Security findings.
- Usage/adoption.

## RP-WF-004 — Define governed metric or KPI

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Create one trusted definition with targets, ownership, and drill path. |
| Trigger | Business owner or analyst proposes a metric/KPI. |

### Actors

- Metric owner
- Analyst
- Approver
- ReportArr

### State path

`proposed → testing → review → certified → changed → deprecated`

### Required sequence

1. Define business question, name, description, owner, audience, grain, formula, dimensions, time logic, units, inclusions/exclusions, and source dataset.
2. Specify target/thresholds, direction, frequency, late data, and acceptable quality.
3. Build test cases and reconcile historical examples with source owners.
4. Configure row/column security and drill-to-source path.
5. Review and certify version.
6. Publish to catalog/dashboards/reports/API.
7. Track changes, usage, anomalies, and owner review date.
8. Supersede rather than silently change formula.

### Exception and recovery paths

- Competing definitions, denominator ambiguity, late corrections, unit/currency mismatch, privacy-small group, no owner, or source model uncertified.
- Calculated forecast/estimate is labeled separately from actual.

### Cross-product and external handoffs

- ReportArr ↔ product/business owners.
- Compliance Core/LedgArr etc. provide source definitions where applicable.

### Evidence and audit record

- Definition/formula/version.
- Tests/reconciliation.
- Approval/security.
- Change/deprecation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Metric reuse.
- Definition disputes.
- Test failures.
- Stale owner review.
- Dashboard inconsistency.

## RP-WF-005 — Build and publish dashboard

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Create an interactive, accessible view that preserves security and source context. |
| Trigger | Authorized creator starts a dashboard from certified datasets or approved exploratory models. |

### Actors

- Dashboard author
- Reviewer
- Viewer
- ReportArr

### State path

`draft → review → published → changed → deprecated → retired`

### Required sequence

1. Define audience, decisions, questions, refresh need, and permitted datasets.
2. Add pages/widgets with appropriate chart/table/KPI types, descriptions, units, and as-of time.
3. Configure filters, cross-filter, drilldown/drillthrough, details, action links, and empty/error states.
4. Test row/column security as representative roles/users.
5. Test performance, accessibility, mobile/responsive layout, print/export, and light/dark contrast.
6. Review with business owner and remove clutter/vanity metrics.
7. Publish version, set sharing/subscription policy, and announce changes.
8. Monitor usage, slow queries, stale widgets, and retire unused assets.

### Exception and recovery paths

- Query too slow, dataset stale, security leakage, misleading visualization, inaccessible colors/labels, mobile overflow, or export exposes hidden data.
- External portal dashboard has separate scoped policy.

### Cross-product and external handoffs

- ReportArr → owning products: drill/action links.
- NexArr/StaffArr: viewer context.
- RecordArr: retained snapshots where needed.

### Evidence and audit record

- Dashboard/version.
- Security/performance/accessibility tests.
- Review/publish.
- Usage/change history.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Load time.
- Usage/return rate.
- Security test pass.
- Export success.
- Retired clutter.

## RP-WF-006 — Build paginated/operational report

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Create a professional print-ready report rather than printing an application page. |
| Trigger | Report author creates a report definition. |

### Actors

- Report author
- Business owner
- ReportArr

### State path

`draft → testing → review → published → superseded → retired`

### Required sequence

1. Define purpose, audience, dataset/version, parameters, default filters, as-of semantics, and output formats.
2. Design sections, tables, groups, subtotals, charts, notes, signatures, headers/footers, page numbers, and legal/confidential markings.
3. Set page size/orientation/margins and sensible page breaks/repeating headers.
4. Configure row/column security and export restrictions.
5. Test empty/large/edge cases, localization/timezone/currency/units, accessibility, and deterministic totals.
6. Review sample PDF/Excel/CSV against source control totals.
7. Publish version with owner and schedule eligibility.
8. Retain run metadata and deprecate superseded versions.

### Exception and recovery paths

- Report truncation, orphan rows, total mismatch, page overflow, sensitive field, locale error, too-large export, or source refresh incomplete.
- Interactive dashboard screenshot is not an acceptable print report.

### Cross-product and external handoffs

- ReportArr ↔ source dataset/RecordArr for retained exports.
- NexArr/StaffArr: access.

### Evidence and audit record

- Definition/version.
- Layout/security tests.
- Control totals.
- Publication/deprecation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Render success.
- Total reconciliation.
- Pagination defects.
- Export size/time.
- User correction requests.

## RP-WF-007 — Run, export, and securely deliver report

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Execute a report reproducibly and deliver only authorized output. |
| Trigger | User runs report or a schedule/event triggers it. |

### Actors

- Viewer/requester
- ReportArr
- Recipient

### State path

`queued → running → rendering → complete → delivered → failed → expired → canceled`

### Required sequence

1. Validate user/service identity, report access, parameters, row/column policy, export permission, and data freshness.
2. Snapshot report/model/version, filters, as-of time, user timezone/locale, and source refresh IDs.
3. Execute asynchronously for large output and show progress/cancel.
4. Render requested format and scan/validate result.
5. Store retained export in RecordArr or temporary protected storage with expiry.
6. Deliver through secure link, portal, email attachment only if policy permits, or integration destination.
7. Record delivery/open/download/failure and retry policy.
8. Allow rerun/supplement while preserving prior run reproducibility.

### Exception and recovery paths

- No data, stale/incomplete model, query timeout, recipient unauthorized, email bounce, file too large, export policy block, or scheduled user disabled.
- Recipient access is rechecked at download time.

### Cross-product and external handoffs

- ReportArr ↔ RecordArr/NexArr/notification/integration destination.
- ReportArr → source owner on data failure.

### Evidence and audit record

- Run version/parameters/security.
- Refresh IDs/as-of.
- Output hash/location.
- Delivery/access/failures.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Run success.
- Render time.
- Delivery failure.
- Unauthorized export prevented.
- Reproducibility.

## RP-WF-008 — Schedule and subscription management

| Field | Definition |
| --- | --- |
| Classification | CURRENT · COMMON |
| Implementation state | Scaffold |
| Purpose | Deliver recurring information at the right time without stale data or permission drift. |
| Trigger | Authorized user creates/edits a schedule or subscription. |

### Actors

- Schedule owner
- Recipients
- ReportArr

### State path

`draft → active → paused → failing → expired → retired`

### Required sequence

1. Select report/dashboard/KPI version, parameters/filters, recipients, output, frequency/event condition, timezone, and delivery channel.
2. Validate each recipient scope, consent/purpose, external/internal status, and export policy.
3. Configure freshness prerequisite, empty/no-change suppression, quiet hours, retry, and expiry/review.
4. Preview sample output as each security context where needed.
5. Activate schedule and create next run.
6. Before each run, revalidate owner/recipients/access/model version and source freshness.
7. Record deliveries/failures and notify owner of chronic issues.
8. Pause/expire/retire when owner leaves, report changes, or recipient no longer qualifies.

### Exception and recovery paths

- DST/timezone shift, recipient loses access, owner inactive, report superseded, source stale, external email changed, or too many duplicate subscriptions.
- Distribution lists require membership snapshot and review.

### Cross-product and external handoffs

- ReportArr ↔ NexArr/StaffArr/notification provider.
- RecordArr: retained output.

### Evidence and audit record

- Schedule/version.
- Recipient/access snapshots.
- Run/delivery history.
- Changes/expiry.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- On-time delivery.
- Failure rate.
- Stale-data suppression.
- Orphan schedule.
- Duplicate subscription.

## RP-WF-009 — Alert detection, acknowledgement, and response

| Field | Definition |
| --- | --- |
| Classification | CURRENT · UNDERSERVED |
| Implementation state | Scaffold |
| Purpose | Turn meaningful metric exceptions into owned, actionable work without alert fatigue. |
| Trigger | Metric threshold, anomaly, SLA, data quality, or event rule evaluates true. |

### Actors

- Alert owner
- Responder
- ReportArr
- Owning product

### State path

`detected → notified → acknowledged → in_response → snoozed → resolved → closed`

### Required sequence

1. Evaluate rule with metric/model version, window, baseline, hysteresis, suppression, maintenance window, and data-quality gate.
2. Deduplicate/correlate related signals and calculate severity/business impact with transparent rationale.
3. Create alert with owner, affected records/dimensions, evidence, and recommended next actions.
4. Notify through inbox/channel and link to source dashboard/product record.
5. Responder acknowledges, assigns/delegates, comments, snoozes with reason, or launches owning-product action.
6. Monitor condition and escalation until resolved/accepted.
7. Close with cause/outcome and prevent immediate re-alert unless criteria recur.
8. Review alert precision and tune through versioned change.

### Exception and recovery paths

- Source stale, false positive, no owner, mass-event storm, threshold oscillation, user unavailable, or condition resolves before response.
- ReportArr does not directly change source records.

### Cross-product and external handoffs

- ReportArr → NexArr inbox/notification.
- ReportArr → owning product action/deep link.
- Owning product → ReportArr: resolution event.

### Evidence and audit record

- Rule/input/version.
- Detection/rationale.
- Notifications/response.
- Outcome/tuning.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Precision/actionability.
- Time to acknowledge/resolution.
- Duplicate suppression.
- Unowned alerts.
- Recurrence.

## RP-WF-010 — Drill through to source and take action

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED |
| Implementation state | Target |
| Purpose | Move from insight to the exact permitted operational record without treating BI as the system of action. |
| Trigger | Viewer selects a data point, row, exception, or KPI contributor. |

### Actors

- Viewer
- ReportArr
- Owning product

### State path

`requested → resolved → opened → action_requested → completed → denied`

### Required sequence

1. Resolve selected metric dimensions and lineage to candidate source product/record/version/time.
2. Apply viewer row/column and product action permissions.
3. Show contributing records with source status/freshness and explain aggregation.
4. Open owning product detail/drawer in correct tenant context or a read-only snapshot when source unavailable.
5. Offer only permitted actions through product API/deep link.
6. Pass correlation/filter context without sensitive data in URL.
7. Record navigation/action request and receive outcome.
8. Refresh read model after source event rather than optimistic local write.

### Exception and recovery paths

- Source record archived/deleted/merged, viewer lacks product permission, aggregate cannot be uniquely attributed, source unavailable, or read model stale.
- External portal may see summary but not internal source.

### Cross-product and external handoffs

- ReportArr ↔ owning product/NexArr/StaffArr.
- Owning product → ReportArr: events.

### Evidence and audit record

- Lineage resolution.
- Permission decision.
- Deep link/action request.
- Outcome/refresh.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Drill success.
- Denied due to scope.
- Stale source rate.
- Action completion.
- User time to resolution.

## RP-WF-011 — Refresh, rebuild, schema-change impact, and rollback

| Field | Definition |
| --- | --- |
| Classification | CURRENT · FOUNDATION |
| Implementation state | Scaffold |
| Purpose | Maintain analytical models safely when sources or definitions change. |
| Trigger | Scheduled refresh runs, source schema changes, model version publishes, or data defect is detected. |

### Actors

- BI operator
- Data model owner
- Source owner
- ReportArr

### State path

`planned → building → testing → approval → published → failed → rolled_back`

### Required sequence

1. Determine incremental/full refresh scope, dependencies, source freshness, and current published version.
2. Run ingestion/model build in isolated version/partition.
3. Execute schema, data quality, reconciliation, security, performance, and golden-report tests.
4. Calculate downstream dataset/metric/report/dashboard/alert/schedule impact.
5. Require owner approval for breaking changes or material metric differences.
6. Atomically publish new model and invalidate caches.
7. Rollback to prior version on failure and quarantine bad source data.
8. Communicate changes and retain run/test lineage.

### Exception and recovery paths

- Source unavailable, schema drift, key semantics changed, partial backfill, control total mismatch, security test fail, query regression, or rollback data incompatible.
- Late-arriving data may update history without changing model version.

### Cross-product and external handoffs

- ReportArr ↔ source owner/NexArr/RecordArr.
- ReportArr → all dependent asset owners.

### Evidence and audit record

- Refresh/build inputs.
- Test results/impact.
- Approval/publish.
- Rollback/communications.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Refresh success.
- Data freshness.
- Test failures.
- Rollback rate.
- Change-related incidents.

## RP-WF-012 — Cross-product audit/management package

| Field | Definition |
| --- | --- |
| Classification | CURRENT · DEMOCRATIZE |
| Implementation state | Scaffold |
| Purpose | Create a repeatable board, audit, management, or customer package with governed metrics and evidence. |
| Trigger | Authorized owner defines or runs a package for a period/scope. |

### Actors

- Package owner
- Reviewers
- External recipient
- ReportArr
- RecordArr

### State path

`planned → running → review → approved → finalized → shared → supplemented`

### Required sequence

1. Define purpose, audience, scope, period/as-of, entities/sites/products, metrics/reports, narratives, evidence, and approvals.
2. Run required certified reports/metrics against a consistent snapshot.
3. Generate variance/trend commentary proposals with citations to metrics/source events.
4. Collect RecordArr evidence packages and product attestations where required.
5. Review numbers, narrative, redactions, confidentiality, and actions.
6. Approve and render professional package with index, definitions, as-of time, source/refresh notes, and appendices.
7. Store/finalize in RecordArr and share securely.
8. Track questions, supplemental versions, and follow-up actions.

### Exception and recovery paths

- Metric model changed mid-package, source refresh incomplete, evidence missing, narrative unsupported, sensitive detail, or recipient scope differs.
- Original approved package remains immutable.

### Cross-product and external handoffs

- ReportArr ↔ RecordArr/all source products/NexArr.
- Action links → owning products.

### Evidence and audit record

- Package definition/version.
- Report runs/snapshot.
- Narrative citations/review.
- Approval/final artifact/access.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Package cycle.
- Data/evidence gaps.
- Review changes.
- Recipient questions.
- Repeatability.

## RP-WF-013 — Natural-language question to governed answer

| Field | Definition |
| --- | --- |
| Classification | UNDERSERVED · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Answer business questions using certified models and explicit definitions, filters, and citations. |
| Trigger | Authorized user asks a question in ReportArr or another product. |

### Actors

- User
- AI assistant
- ReportArr

### State path

`asked → interpreting → querying → answered → clarification → refused → failed`

### Required sequence

1. Authenticate user/tenant and determine allowed datasets/fields/actions.
2. Interpret question into candidate metric/dimensions/time/filter and show clarifying assumptions when material.
3. Select certified definitions and reject unsupported/unowned metrics.
4. Generate query plan under row/column security and resource limits.
5. Execute and validate result quality/freshness.
6. Present answer with formula/definition, filters, as-of time, source lineage, uncertainty, and drill links.
7. Allow user to refine/save as view/report proposal.
8. Audit prompt, selected assets, query, result refs, and feedback without exposing hidden data.

### Exception and recovery paths

- Ambiguous term, no certified metric, insufficient permission, sparse/privacy-sensitive group, stale source, query too expensive, or answer conflicts with known control total.
- AI never invents a number or commits source changes.

### Cross-product and external handoffs

- AI/ReportArr ↔ certified semantic layer.
- ReportArr → source products for drill only.
- NexArr: AI audit.

### Evidence and audit record

- Question/user scope.
- Interpretation/definitions.
- Query/security/result refs.
- Answer/citations/feedback.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Answer success.
- Clarification rate.
- Definition citation usage.
- Security blocks.
- User correction.

## RP-WF-014 — Data-quality incident and correction

| Field | Definition |
| --- | --- |
| Classification | COMMON · UNDERSERVED |
| Implementation state | Target |
| Purpose | Detect, communicate, repair, and document analytical data defects without hiding prior reports. |
| Trigger | Quality test, user report, reconciliation, or source owner identifies incorrect/stale/missing BI data. |

### Actors

- BI operator
- Model owner
- Source owner
- Affected report owners

### State path

`reported → triage → contained → repair → rebuild → validated → communicated → closed`

### Required sequence

1. Open incident with affected datasets/fields/metrics/reports/periods/users and severity.
2. Confirm defect vs expected late data/model definition and identify source/transformation/root cause.
3. Mark affected assets with visible warning or pause schedules/alerts as appropriate.
4. Correct source through owning product or transformation/model version—not manual dashboard edits.
5. Rebuild/reprocess affected range and rerun tests/reconciliation.
6. Identify prior reports/exports/packages materially affected and notify owners/recipients.
7. Publish correction note/supplement without deleting prior approved output.
8. Close with cause, prevention, and quality-rule update.

### Exception and recovery paths

- Source cannot be corrected, issue spans long history, privacy/security breach, multiple models disagree, or external recipients acted on incorrect data.
- Financial/audit reports may require formal restatement.

### Cross-product and external handoffs

- ReportArr ↔ source product/RecordArr/NexArr notifications.
- Owning products handle source correction.

### Evidence and audit record

- Incident/scope.
- Cause/source/transformation.
- Containment/repair/tests.
- Affected outputs/communications.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Detection time.
- Time to contain/correct.
- Affected reports/users.
- Recurrence.
- Quality-rule coverage.

## RP-WF-015 — BI access review and external embedding

| Field | Definition |
| --- | --- |
| Classification | COMMON · DEMOCRATIZE |
| Implementation state | Target |
| Purpose | Safely expose analytics to internal and external audiences and regularly prove access remains appropriate. |
| Trigger | Dashboard/report/dataset is shared/embedded or scheduled access review begins. |

### Actors

- BI administrator
- Data owner
- Access reviewer
- Portal owner
- ReportArr

### State path

`requested → testing → active → review → modified → revoked → expired`

### Required sequence

1. Define audience, purpose, tenant/customer/supplier scope, role, row/column policy, export, cache, and expiry.
2. Test with representative identities and attempt cross-scope access.
3. Issue embed configuration/token through server-side NexArr context; never trust client filters.
4. Log views/queries/exports and monitor unusual access.
5. Review users/groups/portal grants, usage, owner, sensitive fields, and stale shares on schedule.
6. Revoke/modify/recertify with downstream cache/token invalidation.
7. Notify owners of unused/orphaned assets and external access.
8. Retain review and test evidence.

### Exception and recovery paths

- Owner left, customer contact changed, token replay, cached data after revoke, export bypass, group too broad, or row-security rule changed.
- Public unauthenticated embedding is restricted to explicitly public non-tenant data.

### Cross-product and external handoffs

- ReportArr ↔ NexArr/StaffArr/CustomArr/SupplyArr portals.
- RecordArr: review evidence.

### Evidence and audit record

- Purpose/policy/tests.
- Embed/share issuance.
- Usage/anomalies.
- Review/revocation.

### Field Companion / offline behavior

Field Companion may execute permissioned steps, capture evidence, and queue offline-safe actions; the owning product validates and commits on sync.

### Measures and acceptance signals

- Security test pass.
- Stale access removed.
- External usage.
- Unauthorized attempts.
- Revocation latency.



## Workflow definition of done

A workflow is releasable only when its happy path, every listed exception, dependency outage, duplicate/retry, stale/concurrent update, permission denial, cancellation, correction/void, archival/retention, import/API/event path, notification, audit trail, reporting projection, mobile behavior, and professional printable output have automated and human-usable acceptance coverage.
