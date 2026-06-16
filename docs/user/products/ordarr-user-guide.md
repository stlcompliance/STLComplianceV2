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
- Orders
- Requests
- Intake
- Triage
- Handoffs
- Exceptions
- Completion packets
- Invoice-ready packets
- Bill-ready packets
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
- create an order or service request
- triage request readiness
- launch execution handoffs to products such as RoutArr, LoadArr, MaintainArr, Field Companion, or RecordArr
- monitor execution state without becoming the execution source of truth
- close order work after required completion evidence is available
- prepare invoice-ready and bill-ready packets for external finance systems

## Permissions Usually Needed
- ordarr.orders.read
- ordarr.orders.create
- ordarr.orders.manage
- ordarr.handoffs.manage
- ordarr.exceptions.resolve
- ordarr.packets.prepare
- ordarr.packets.export

## Related Products
- CustomArr owns customer account, contact, location, onboarding, and customer eligibility truth.
- Execution products own the tasks, trips, work orders, inspections, receiving sessions, and other execution records they perform.
- RecordArr owns retained files, evidence links, packages, retention, and file audit trail.
- ReportArr reports from OrdArr and execution read models without correcting source data.
- External finance systems own invoicing, accounts payable, payment, tax, and general ledger.

## Common Troubleshooting
- [Product not visible](../troubleshooting/product-not-visible.md)
- [Missing permission](../troubleshooting/missing-permission.md)
- If execution status looks stale, check the execution product handoff and read model before editing the order.
- If billing data is incomplete, review the completion packet and invoice-ready or bill-ready packet before sending anything to finance.
