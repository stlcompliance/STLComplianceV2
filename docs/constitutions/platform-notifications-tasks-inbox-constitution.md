# STL Compliance Notifications, Tasks, and Inbox Constitution

## 1. Purpose

This constitution defines how STL Compliance surfaces work, alerts, reminders, blockers, approvals, and messages without creating a dedicated NotificationArr too early or confusing notifications with source-of-truth work records.

## 2. Scope

This constitution applies to:

- In-app notifications
- Mobile push notifications
- Email/SMS delivery where implemented
- Task inboxes
- Field Companion task surfaces
- Approval requests
- Reminders
- Escalations
- Notification grouping/deduplication
- User preferences
- System-generated alerts

## 3. Prime directive

A notification is not the work.

A task points to work.

The product that owns the required action owns the task/work record.

Delivery channels do not own the business truth.

## 4. Notification vs task

### Notification

A notification informs a user that something happened, changed, is due, is blocked, or needs attention.

### Task

A task is an actionable item tied to a product-owned workflow or record.

Examples:

- TrainArr owns a training signoff task.
- MaintainArr owns a work order assignment task.
- LoadArr owns a receiving task.
- AssurArr owns a CAPA review task.
- RecordArr owns a document approval/read-and-acknowledge task.
- StaffArr owns personnel/incident review tasks.
- RoutArr owns dispatch/trip update tasks.

### Inbox

An inbox aggregates tasks and notifications for the user.

An inbox may be shell-level or Field Companion-level, but it must not become the owner of product work.

## 5. Required notification fields

A notification should include:

- Notification ID
- Tenant ID
- Source product
- Source record type
- Source record ID
- Recipient: person/team/role/queue
- Title
- Plain-language message
- Severity
- Reason/category
- Created time
- Due time or urgency when relevant
- Action route
- Read/acknowledged state
- Delivery channels attempted
- Correlation ID

## 6. Required task fields

A task should include:

- Task ID
- Owning product
- Tenant ID
- Related record type
- Related record ID
- Assigned person/team/role/queue
- Required action
- Status
- Priority/severity
- Due time
- Blocking effect where relevant
- Source/reason
- Canonical action route

The task owner is the product that owns the required action.

## 7. Severity

Recommended severity levels:

- `critical`
- `high`
- `medium`
- `low`
- `info`

Recommended special states:

- `blocked`
- `approval_required`
- `review_required`
- `due_soon`
- `overdue`

Severity must be text-readable, not color-only.

## 8. Delivery channels

Delivery channels may include:

- In-app notification
- Field Companion inbox
- Push notification
- Email
- SMS
- Webhook
- External system handoff

Channels are delivery methods, not source-of-truth systems.

A failed delivery must not erase the underlying task or business state.

## 9. Preferences

Notification preferences may control delivery channel and frequency.

Preferences must not suppress required safety, compliance, legal, security, or urgent operational notifications unless an explicit policy allows it.

Preferences should be scoped by:

- User/person
- Tenant
- Product
- Category
- Severity
- Channel

## 10. Deduplication and grouping

The platform should prevent noisy duplicates.

Group notifications when:

- Many records need the same review
- A repeated event occurs for the same record
- A batch import produces many similar warnings
- A dashboard/queue is a better surface than individual alerts

Critical blockers should not be hidden inside low-priority groups.

## 11. Escalation

Escalation rules must be explicit.

An escalation should define:

- Trigger
- Delay/threshold
- Original owner/assignee
- Escalation recipient
- Message
- Blocking effect
- Source product
- Audit/activity behavior

Examples:

- Overdue training signoff escalates to manager.
- Open incident review escalates to safety lead.
- Unresolved receiving exception escalates to AssurArr or LoadArr supervisor.
- CAPA due date breach escalates to assurance owner.

## 12. Field Companion

Field Companion may aggregate mobile tasks across products.

Field Companion owns the mobile task surface, not the underlying records.

A Field Companion task action must write back to the owning product API.

Offline mobile tasks must show sync state and must not pretend pending local actions are confirmed.

## 13. Approval notifications

Approval notifications must identify:

- What is being approved
- Owning product
- Requested by
- Why approval is needed
- Due time
- Consequence of approval/rejection
- Canonical route to approve/reject

Approval must happen through the owning product's authorized workflow, not only by clicking a notification.

## 14. Read and acknowledge

Read/acknowledge records may be operational notifications or RecordArr-controlled records depending on context.

If the acknowledgment is a controlled document, SOP, policy, or retained evidence, RecordArr owns the stored record/acknowledgment artifact.

## 15. Templates

Templates may exist for consistent notification language.

Templates must not encode hidden business rules that belong to products or Compliance Core.

Template content should include placeholders that resolve safely and do not leak restricted data.

## 16. Anti-patterns

The following are not allowed:

- Treating notifications as the source-of-truth work record
- Creating tasks with no owning product
- Delivery failure deleting business tasks
- Suppressing critical compliance/safety notifications by ordinary preference
- Spamming one notification per repeated event when grouping is appropriate
- Notifications with no action route or source record
- Cross-product task actions that bypass the owning product
- Raw event payloads in notifications

## 17. Minimum acceptable implementation

A notification/task feature is minimally acceptable when it has:

1. Source product
2. Owning product for the task/action
3. Tenant-safe recipient resolution
4. Plain-language title/message
5. Severity/category
6. Action route to canonical workflow
7. Delivery/read state
8. Dedup/group behavior where needed
9. Audit/activity for material alerts or approvals
10. No ownership confusion
