# STL Compliance Workflow, Approval, Assignment, and Escalation Constitution

## 1. Purpose

This constitution defines how work is assigned, approved, blocked, escalated, reassigned, handed off, and closed across STL Compliance without creating a generic WorkflowArr before it is needed.

## 2. Scope

This constitution applies to:

- Product workflows
- Assignments
- Queues
- Approvals
- Reviews
- Escalations
- Blockers
- Reassignment
- Cross-product handoffs
- Workflow history
- Overrides
- Closeout

## 3. Prime directive

Work ownership stays with the product that owns the business record.

A cross-product workflow creates a handoff, reference, task, or event. It does not duplicate the source record or transfer ownership unless explicitly designed.

## 4. Workflow ownership

Examples:

- MaintainArr owns maintenance workflow.
- RoutArr owns dispatch/trip workflow.
- LoadArr owns receiving/warehouse/inventory workflow.
- TrainArr owns training/evaluation/remediation workflow.
- AssurArr owns nonconformance/CAPA/assurance workflow.
- RecordArr owns document approval/read-and-acknowledge workflow.
- SupplyArr owns supplier/procurement-context workflow.
- OrdArr owns order/request orchestration workflow.
- Compliance Core owns rule/mapping/evidence evaluation workflow.
- StaffArr owns personnel/authority/location/personnel-history workflow.

## 5. Assignment targets

Assignments may target:

- Person
- Team
- Role
- Queue
- Site/location context
- Service/system owner

Human assignment must use `personId` when assigned to a person.

Team/role/authority context should come from StaffArr where applicable.

## 6. Assignment fields

An assignment should include:

- Assignment ID
- Owning product
- Related record
- Assigned to person/team/role/queue
- Assigned by
- Assigned time
- Due/needed by time
- Priority
- Required action
- Status
- Reassignment history

## 7. Approval gates

Approval gates must define:

- What requires approval
- Why approval is required
- Who may approve
- What data/evidence is required before approval
- Approval due time when applicable
- Approval effect
- Rejection effect
- Override policy if any

Approval must be a clear state transition, not a hidden save side effect.

## 8. Review vs approval

Review means evaluation or checking.

Approval means authority decision.

A record may need review before approval.

Examples:

- A supplier document is reviewed for completeness, then supplier is approved.
- A CAPA action is reviewed for effectiveness, then case is closed.
- An import mapping is reviewed, then approved into a catalog.

## 9. Blockers

A blocker prevents workflow progress.

Blockers must include:

- Source product/rule
- Reason
- Severity
- Required clearing action
- Owner/queue when known
- Whether override is allowed
- Audit requirement for override

## 10. Overrides

Overrides must be rare and explicit.

An override should record:

- Overridden rule/blocker
- Actor
- Authority/permission
- Reason
- Time
- Expiration if temporary
- Risk acknowledgement
- Downstream notifications/events

Overrides must not delete the original blocker history.

## 11. Escalation

Escalation moves attention, not ownership, unless explicitly designed.

Escalation rules should define:

- Trigger
- Threshold/delay
- Original assignee/owner
- Escalation recipient
- Message/reason
- Business effect
- Audit/activity behavior

Examples:

- Overdue training → manager notification
- Repeated incident → StaffArr personnel review and TrainArr retraining review
- Supplier defect → AssurArr case and SupplyArr supplier issue
- Inventory exception → LoadArr hold and AssurArr nonconformance

## 12. Reassignment

Reassignment must preserve history.

A reassignment should record:

- Previous assignee
- New assignee
- Actor
- Reason when required
- Time
- Impact on due date/status

## 13. Cross-product handoff

A handoff must identify:

- Source product/record
- Target product/action
- Current handoff state
- Required next action
- Due/priority
- Source summary
- Target acceptance/rejection/blocking state

The target product owns its own accepted work.

## 14. Closeout

Closeout must define what complete means.

Closeout may require:

- Work completion
- Review
- Approval
- Evidence
- Document package
- Compliance evaluation
- External handoff
- Customer/vendor signoff
- Report snapshot

Completed and closed may be separate lifecycle states.

## 15. Workflow history

Detail views must show workflow history where relevant.

History should include:

- State changes
- Assignments
- Reassignments
- Approvals/rejections
- Blockers
- Overrides
- Handoffs
- Escalations
- Completion/closeout

Use plain language. Do not show raw event payloads to ordinary users.

## 16. Workflow UI

Workflow UI should make clear:

- Current state
- Required next action
- Who owns the action
- What is blocked
- What happens if user proceeds
- What evidence/approval is missing
- What product owns cross-product actions

## 17. Anti-patterns

The following are not allowed:

- Generic workflow engine owning product records by accident
- Assignment by free-text name
- Approval as hidden save side effect
- Escalation that loses original owner/history
- Blocking with no clear clearing action
- Overrides without reason/audit
- Cross-product workflow that edits target records without approved API/handoff
- Closeout before required evidence/review is complete

## 18. Minimum acceptable implementation

A workflow feature is minimally acceptable when it has:

1. Owning product/record
2. Explicit states/transitions
3. Assignment target using `personId`, team, role, or queue
4. Approval/review rules where applicable
5. Blocker model
6. Escalation behavior
7. Reassignment history
8. Handoff model for cross-product work
9. Workflow history on detail pages
10. Permission/audit controls
