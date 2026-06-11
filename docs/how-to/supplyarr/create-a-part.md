# How to create a part

## Audience
Supply chain users, maintenance planners, and inventory coordinators

## Product
SupplyArr

## Support Status
Supported by current UI/API

## Purpose
Create the purchasable item or part context used by SupplyArr, MaintainArr, and LoadArr.

## Before You Start
- SupplyArr owns item, part, material, vendor-item, price, and lead-time context.
- LoadArr owns physical inventory balances and movement.
- MaintainArr owns maintenance asset and work-order demand.

## Steps
1. Open SupplyArr.
2. Open Catalog.
3. Use the create or add part action available in the catalog.
4. Enter part name, number, description, unit, category, and vendor relationships requested by the page.
5. Add preferred supplier, price snapshot, lead-time snapshot, and external IDs when available.
6. Save the part.
7. Confirm downstream products reference the SupplyArr part instead of creating local part masters.

## What Happens Next
The part becomes the commercial and procurement source reference. LoadArr can stock it, and MaintainArr can request it for work orders.

## Troubleshooting
- If the physical quantity is wrong, fix it in LoadArr rather than SupplyArr.
- If the part is needed on a work order, request it through the MaintainArr parts demand workflow.

