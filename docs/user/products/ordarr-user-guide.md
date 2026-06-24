# OrdArr User Guide

## What This Product Is For
OrdArr is for order and request orchestration, intake, lifecycle status, customer-facing order context, product handoffs, completion packets, invoice-ready packets, and bill-ready packets.

OrdArr owns the parent business object for requested work. It does not own customer truth, execution truth, retained files, or accounting.

## Who Uses It
- order coordinators
- customer service users
- dispatch and operations coordinators
- billing preparation users
- managers tracking cross-product fulfillment

## Main Pages
- Dashboard
- Orders
- Handoffs
- Completion
- Reports
- Settings

## Main Records
- order request
- order line
- order participant
- order handoff
- order exception
- completion packet
- invoice-ready packet
- bill-ready packet

## Common Workflows
- create an order from the workspace
- review the order timeline, holds, and downstream handoffs
- approve or hold an order before downstream release
- monitor execution state without becoming the execution source of truth
- capture basic return or RMA records
- review completion and finance packet readiness

## Permissions Usually Needed
- ordarr.order_requests.read
- ordarr.order_requests.create
- ordarr.order_requests.update
- ordarr.order_requests.submit
- ordarr.order_requests.approve
- ordarr.order_requests.cancel
- ordarr.order_handoffs.manage
- ordarr.completion_packets.review
- ordarr.financial_packets.prepare

## Related Products
- CustomArr owns customer account, contact, location, onboarding, and customer eligibility truth.
- Execution products own the tasks, trips, work orders, inspections, receiving sessions, and other execution records they perform.
- RecordArr owns retained files, evidence links, packages, retention, and file audit trail.
- ReportArr reports from OrdArr and execution read models without correcting source data.
- External finance systems own invoicing, accounts payable, payment, tax, and general ledger.

## Common Troubleshooting
- [Product or feature not visible](../troubleshooting/product-or-feature-not-visible.md)
- [Missing permission](../troubleshooting/missing-permission.md)
- If execution status looks stale, check the execution product handoff and read model before editing the order.
- If billing data is incomplete, review the completion packet and invoice-ready or bill-ready packet before sending anything to finance.
- If an order is on hold, check the hold reason and release permission before approving it.
