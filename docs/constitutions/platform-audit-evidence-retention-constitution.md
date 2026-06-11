# STL Compliance Audit, Activity, Evidence, and Retention Constitution

## 1. Purpose

This constitution defines how STL Compliance records what happened, stores evidence, preserves document history, and applies retention without confusing activity feeds, audit logs, operational records, and stored files.

## 2. Scope

This constitution applies to:

- Activity feeds
- Audit logs
- Security audit
- Evidence files
- Document metadata
- Record packages
- Retention schedules
- Legal holds
- Versioning
- Access history
- Exported report artifacts
- Compliance evidence classification

## 3. Prime directive

Activity is for humans to understand recent change.

Audit is for accountability and traceability.

Evidence is for supporting a requirement, decision, or record.

Retention is for preserving or disposing records according to policy.

Do not collapse all four into generic notes.

## 4. Ownership

RecordArr owns:

- Stored files
- Document metadata
- File versions
- Controlled documents
- Record packages
- Retention schedules
- Legal holds
- Document approvals
- Evidence file storage
- Document access history

Compliance Core owns:

- Evidence requirements
- Evidence meaning
- Applicability
- Rule/evidence classification
- Compliance gap analysis

Products own:

- Operational records
- Domain activity
- Domain decisions
- Requests to attach/store evidence
- References to RecordArr documents

ReportArr owns:

- Report definitions
- Rendered report artifacts before storage
- Scheduled report generation
- Report subscriptions

RecordArr owns stored report artifacts once they are retained as records.

## 5. Activity feed

Activity feeds are user-facing summaries of recent change.

Activity items should include:

- Actor or source
- Plain-language action
- Target record
- Timestamp
- Source product
- Optional status/severity
- Drill-in link

Activity feeds must not expose raw event payloads, raw rule JSON, service-token claims, or database rows.

Activity feeds are not a complete immutable audit log unless explicitly designed and protected as one.

## 6. Audit log

Audit logs preserve accountability.

Audit records should include:

- Tenant ID
- Event/action ID
- Actor type
- Actor ID, using `personId` when human
- Service client when service-initiated
- Source product
- Target product when cross-product
- Target record type
- Target record ID
- Action
- Timestamp in UTC
- Correlation ID
- Outcome
- Before/after summary when appropriate
- Reason/justification when required
- IP/device/session context where appropriate and allowed

Audit logs should be tamper-evident or protected from ordinary user mutation.

## 7. Security audit

Security-sensitive events require stronger audit treatment.

Examples:

- Login
- Failed login
- Tenant switch
- Product launch
- Permission assignment
- Service token creation/rotation/revocation
- Break-glass activation
- Sensitive document access
- External credential changes
- Export/download of sensitive data
- Legal hold change

## 8. Evidence

Evidence is a document, file, record, signature, photo, inspection result, external status, or product event that supports a rule, requirement, decision, or audit package.

Evidence must have:

- Evidence ID or RecordArr document/record ID
- Source product
- Owning business record when applicable
- Evidence type
- Status
- Uploaded/captured/generated time
- Actor or service source
- Linked requirement when applicable
- Effective date when applicable
- Expiration date when applicable
- Review status when applicable

## 9. Evidence states

Recommended evidence states:

- `current`
- `expiring_soon`
- `expired`
- `pending_review`
- `approved`
- `rejected`
- `superseded`
- `missing`
- `not_applicable`
- `source_unavailable`

Evidence state must be readable in UI and reports.

## 10. RecordArr storage rule

Files that become records or evidence must be stored through RecordArr or a RecordArr-controlled storage path.

Products may upload, attach, capture, or request files, but RecordArr owns:

- Storage identity
- File version
- Metadata
- Access history
- Retention
- Legal hold
- Controlled document lifecycle where applicable

## 11. Compliance Core classification rule

Compliance Core determines what evidence is required and what evidence means for rules.

Products may collect evidence, but they do not invent regulatory meaning locally when Compliance Core owns that interpretation.

## 12. Versioning

Controlled documents and record packages must preserve versions.

A new version must not overwrite the historical version used in a prior decision.

Version metadata should include:

- Version number or ID
- Created by
- Created time
- Effective date
- Superseded date
- Approval state
- Replacement/superseding record

## 13. Retention

Retention controls how long records and evidence are preserved.

Retention schedules should define:

- Record category
- Owner
- Trigger date
- Retention duration
- Disposition action
- Legal/regulatory basis where applicable
- Exception handling

Retention must not delete records under legal hold.

## 14. Legal hold

Legal hold overrides ordinary cleanup and retention disposition.

Legal hold changes must be permission-gated and audit-visible.

A legal hold should record:

- Hold ID
- Scope
- Reason
- Applied by
- Applied time
- Released by
- Released time
- Affected records

## 15. Access history

RecordArr must preserve access history for controlled records and sensitive documents where required.

Access history should show:

- Who accessed
- When
- What action occurred: viewed, downloaded, uploaded, replaced, approved, rejected, shared
- Source product or route
- Tenant

## 16. Report exports

ReportArr may render exports.

If an export becomes a retained record, RecordArr owns the stored artifact.

The stored report should preserve:

- Report definition/version
- Filters
- Date/time generated
- Actor/service
- Source products
- Source freshness
- Snapshot/current distinction

## 17. User-facing audit display

Normal users should see readable audit/activity summaries.

Admin/debug views may expose technical payloads only with explicit permission.

Do not show raw JSON by default.

## 18. Anti-patterns

The following are not allowed:

- Evidence hidden only in notes
- Activity feed treated as complete immutable audit
- Stored files owned locally by every product
- Compliance evidence meaning invented in each frontend
- Legal hold bypass through retention cleanup
- Overwriting historical document versions
- Deleting evidence referenced by an audit package
- Raw event payloads shown to normal users
- Report exports stored outside RecordArr when retained as records

## 19. Minimum acceptable implementation

An evidence/audit feature is minimally acceptable when it has:

1. Clear owner of operational record
2. RecordArr file/document identity when a file is stored
3. Compliance Core evidence meaning when rules are involved
4. Activity feed for human context when useful
5. Audit log for accountability where required
6. Retention/legal-hold behavior when records are retained
7. Access history for sensitive/controlled documents
8. No raw payloads exposed to ordinary users
