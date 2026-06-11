# Common Notifications

A notification is not the work record. A task points to work owned by a product.

## Common notification and task types
- Training assignment or signoff needed in TrainArr.
- Work order assignment or maintenance blocker in MaintainArr.
- Receiving, putaway, hold, or exception task in LoadArr.
- Dispatch, trip, proof, or blocker update in RoutArr.
- Incident review or readiness blocker in StaffArr.
- Import review, missing evidence, or finding review in Compliance Core.
- Report run, export, or alert in ReportArr.
- Document approval, acknowledgement, retention, or legal hold update in RecordArr.

## What to check
- Source product: tells you where the task came from.
- Status: tells you whether it is ready, blocked, completed, or needs review.
- Priority or severity: tells you urgency.
- Due time: tells you when action is needed.
- Blocked reason: tells you what must be fixed first.
- Action route: use it to open the owning product.

## Field Companion
Field Companion can aggregate tasks from several products, but the owning product still controls the final workflow. Use **Acknowledge** only to confirm you saw the task. Use **Open in [Product]** when the work must be completed in the owning product.
