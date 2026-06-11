# SupplyArr User Guide

## What This Product Is For
SupplyArr is for vendor and supplier records, supplier contacts, supplier documents, item and part master data, price snapshots, lead-time snapshots, purchase requests, RFQs, operational purchasing approvals, purchase intent, procurement status, and vendor mappings.

## Who Uses It
- procurement managers
- buyers
- clerks
- vendor users through vendor portal links
- warehouse managers who need procurement context

## Main Pages
- Parties
- Catalog
- Purchasing
- Procurement
- Approvals
- Exceptions
- Vendor orders
- Create vendor order
- Pricing
- Planning
- Readiness
- Settings
- Vendor portal

## Main Records
- party
- vendor
- supplier
- part
- purchase request
- purchase order or vendor order
- RFQ
- quote
- backorder
- supplier incident

## Common Workflows
- create vendors
- create parts
- draft purchase requests
- submit purchase requests for approval
- approve or reject purchase requests
- create vendor orders
- let vendors submit quotes

## Permissions Usually Needed
- supplyarr_admin
- supplyarr_manager
- supplyarr_buyer
- supplyarr_clerk
- supplyarr.parties.manage
- supplyarr.parts.manage
- supplyarr.purchaseRequests.create
- supplyarr.purchaseRequests.approve

## Related Products
- LoadArr owns physical receiving, inventory, and stock ledger.
- StaffArr owns approval authority context.
- RecordArr stores supplier and procurement documents when retained.

## Common Troubleshooting
- [Product not visible](../troubleshooting/product-not-visible.md)
- [Missing permission](../troubleshooting/missing-permission.md)
- If a page is visible but an action is disabled, check the record status and your role or permission assignment.
- Remember: SupplyArr does not own inventory balances, stock ledger, warehouse movement, receiving execution, payment execution, accounts payable, tax, banking, or the general ledger.
