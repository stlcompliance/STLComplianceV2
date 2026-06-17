# How to handle a customer complaint quality case

## Audience
Customer quality managers, account owners, quality reviewers, and operations managers

## Product
AssurArr

## Support Status
Supported by current UI/API

## Purpose
Track the quality workflow for a customer complaint while keeping customer master data and customer relationship context in CustomArr.

## Before You Start
- AssurArr owns the complaint quality case, quality investigation, holds, CAPA links, and quality closure.
- CustomArr owns the customer account, contacts, locations, and relationship history.
- RecordArr owns customer response records and supporting files.

## Steps
1. Open AssurArr.
2. Open Complaints.
3. Select Create complaint quality case.
4. Enter title, complaint type, severity, description, customer reference, and received date.
5. Add affected order, shipment, item, asset, location, contact snapshot, nonconformance, hold, CAPA, and record references when available.
6. Save the complaint case.
7. Triage the case and open a nonconformance or hold if the issue affects work, inventory, assets, shipments, or orders.
8. Attach customer response records and evidence through RecordArr-backed capture.
9. Coordinate customer-facing communication in CustomArr.
10. Close the complaint case when required investigation, response, corrective action, and evidence are complete.

## What Happens Next
AssurArr stores quality workflow history. CustomArr keeps the customer relationship context, and affected products handle their own operational corrections.

## Troubleshooting
- If the customer reference is wrong, correct it in CustomArr.
- If the issue becomes systemic, create or link a CAPA.
- If the complaint blocks an order or shipment, use an AssurArr hold and let the owning execution product respond to the hold.

## Related How-To Documents
- [How to create a customer](../customarr/create-a-customer.md)
- [How to create and close a CAPA](create-and-close-a-capa.md)

